using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Représente une photographie complète de l'état du jeu à un instant T (début ou fin de mois).
/// Cette structure utilise les classes globales DonneesJoueur et DonneesEnvironnement pour le stockage.
/// </summary>
[System.Serializable]
public class SnapshotEtatJeu
{
    public int indexMois;                    // Le nombre de mois passés depuis le début du jeu
    public Mois moisCalendrier;              // Le mois civil actuel (Juillet, Août, etc.)

    public DonneesJoueur joueur;             // Contient l'état financier et personnel propre au joueur (copie en profondeur)
    public DonneesEnvironnement env;        // Contient l'état de l'environnement macroéconomique (copie en profondeur)

    /// <summary>
    /// Constructeur créant une copie en profondeur (deep copy) pour garantir l'indépendance de l'historique.
    /// </summary>
    public SnapshotEtatJeu(GameData gameData)
    {
        this.indexMois = gameData.nombreMoisPasses;
        this.moisCalendrier = gameData.moisActuel;

        // 1. Clonage des données personnelles et d'allocation d'énergie
        this.joueur = new DonneesJoueur();
        if (gameData.joueur != null)
        {
            this.joueur.energie = gameData.joueur.energie;
            this.joueur.santeMentale = gameData.joueur.santeMentale;
            this.joueur.salaire = new argent(gameData.joueur.salaire.centimes);

            // 2. Clonage en profondeur du dictionnaire des comptes (avec historique complet des transactions)
            this.joueur.comptes = new Dictionary<string, CompteBanquaire>();
            if (gameData.joueur.comptes != null)
            {
                foreach (var kvp in gameData.joueur.comptes)
                {
                    CompteBanquaire compteOriginal = kvp.Value;
                    long soldeOriginal = compteOriginal.GetSolde().centimes;
                    
                    Historique histCopie = new Historique();
                    Historique histOriginal = compteOriginal.GetHistorique();
                    
                    if (histOriginal != null && histOriginal.GetHistorique() != null)
                    {
                        List<Transaction> transactions = histOriginal.GetHistorique();
                        
                        // Calcul de la somme des transactions enregistrées pour détecter d'éventuels soldes de départ non transactionnels
                        long sommeTransactionsOriginales = 0;
                        foreach (Transaction t in transactions)
                        {
                            if (t != null)
                            {
                                sommeTransactionsOriginales += t.montant.centimes;
                            }
                        }
                        
                        // Réconciliation du solde de départ (ex: les 1000 € d'initialisation par défaut sans transaction)
                        long decalageReconciliation = soldeOriginal - sommeTransactionsOriginales;
                        if (decalageReconciliation != 0)
                        {
                            // On injecte le solde initial de régularisation en premier pour qu'il soit le plus ancien
                            histCopie.Add(new Transaction("solde_initial", new argent(decalageReconciliation)));
                        }

                        // Copie en profondeur de toutes les transactions de la plus ancienne à la plus récente
                        for (int i = transactions.Count - 1; i >= 0; i--)
                        {
                            Transaction transacOrig = transactions[i];
                            if (transacOrig != null)
                            {
                                Transaction transacCopie = new Transaction(
                                    transacOrig.libelle, 
                                    new argent(transacOrig.montant.centimes)
                                );
                                histCopie.Add(transacCopie);
                            }
                        }
                    }
                    else
                    {
                        // Si pas de transactions, on initialise simplement le solde actuel comme point de départ
                        if (soldeOriginal != 0)
                        {
                            histCopie.Add(new Transaction("solde_initial", new argent(soldeOriginal)));
                        }
                    }

                    // Initialisation du compte avec son historique cloné
                    CompteBanquaire compteCopie = new CompteBanquaire(histCopie);
                    this.joueur.comptes.Add(kvp.Key, compteCopie);
                }
            }

            // 3. Clonage en profondeur de la liste des investissements
            this.joueur.investissements = new List<Investissement>();
            if (gameData.joueur.investissements != null)
            {
                foreach (var invest in gameData.joueur.investissements)
                {
                    // Copie le capital investi et le taux d'intérêt de la période
                    this.joueur.investissements.Add(new Investissement(
                        new argent(invest.sommeInvestie.centimes),
                        invest.taux,
                        12 // Durée de capitalisation standard
                    ));
                }
            }

            this.joueur.bourse = gameData.joueur.bourse != null
                ? gameData.joueur.bourse.Copier()
                : new DonneesBourse();
        }

        // 4. Clonage des données de l'environnement macroéconomique
        this.env = new DonneesEnvironnement();
        if (gameData.joueur != null && gameData.joueur.comptes != null && gameData.joueur.comptes.ContainsKey("epargne"))
        {
            Epargne epgn = (Epargne)gameData.joueur.comptes["epargne"];
            this.env.tauxEpargne = epgn.GetTaux();
        }
        else
        {
            // Taux réglementé initial de secours
            this.env.tauxEpargne = gameData.env != null ? gameData.env.tauxEpargne : 0.0175f;
        }
    }
}
