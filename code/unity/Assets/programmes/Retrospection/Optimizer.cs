using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moteur d'optimisation What-if qui simule la stratégie financière optimale "Fourmi"
/// et la compare à la trajectoire réelle du joueur sur les 12 derniers mois.
/// </summary>
public static class Optimizer
{
    [Serializable]
    public struct SimulationResult
    {
        public int indexMois;             // Index du mois dans le jeu
        public Mois moisCalendrier;       // Mois civil (Juillet, Août, etc.)
        public argent soldeCourant;       // Solde simulé du compte courant
        public argent soldeEpargne;       // Solde simulé du compte d'épargne (Livret A)
        public argent patrimoineTotal;    // Somme du courant et de l'épargne
    }

    /// <summary>
    /// Simule la stratégie optimale "Fourmi" sur les 12 derniers mois de jeu.
    /// La stratégie "Fourmi" consiste à maintenir le même niveau de vie (mêmes dépenses de consommation)
    /// que le joueur, mais à optimiser les placements en conservant un buffer fixe de 500 € sur le compte
    /// courant et en versant immédiatement tout excédent sur le Livret A.
    /// </summary>
    public static List<SimulationResult> SimulerFourmi(GameData gameData)
    {
        List<SimulationResult> resultats = new List<SimulationResult>();
        if (gameData == null || gameData.historiqueSnapshots == null || gameData.historiqueSnapshots.Count < 2)
        {
            Debug.LogWarning("[What-If] Historique insuffisant pour lancer la simulation.");
            return resultats;
        }

        int totalSnapshots = gameData.historiqueSnapshots.Count;
        // La simulation porte sur les 12 derniers mois maximum (13 snapshots couvrent 12 intervalles)
        int startIndex = Mathf.Max(0, totalSnapshots - 13);

        // Initialisation de la simulation à partir du premier snapshot de la période
        SnapshotEtatJeu snapshotInitial = gameData.historiqueSnapshots[startIndex];
        
        // Copie des soldes de départ de la période
        argent courantSimule = snapshotInitial.joueur.comptes.ContainsKey("courant") 
            ? snapshotInitial.joueur.comptes["courant"].GetSolde() 
            : new argent(100000); // 1000 € par défaut

        argent epargneSimule = snapshotInitial.joueur.comptes.ContainsKey("epargne") 
            ? snapshotInitial.joueur.comptes["epargne"].GetSolde() 
            : new argent(0); // 0 € si non ouvert

        float interetsAccumules = 0f;
        int moisEcoulesEpargne = 0;

        // On enregistre le point de départ dans les résultats de simulation
        resultats.Add(new SimulationResult
        {
            indexMois = snapshotInitial.indexMois,
            moisCalendrier = snapshotInitial.moisCalendrier,
            soldeCourant = courantSimule,
            soldeEpargne = epargneSimule,
            patrimoineTotal = courantSimule + epargneSimule
        });

        // Simulation mois par mois
        for (int i = startIndex + 1; i < totalSnapshots; i++)
        {
            SnapshotEtatJeu snapActuel = gameData.historiqueSnapshots[i];
            
            // 1. Versement du salaire réel perçu par le joueur ce mois-ci
            argent salaireCeMois = snapActuel.joueur.salaire;
            courantSimule += salaireCeMois;

            // 2. Calcul et accumulation mensuelle des intérêts du Livret A
            // Le taux appliqué est celui en vigueur dans l'économie réelle à ce mois (DonneesEnvironnement)
            float tauxAnnuel = snapActuel.env.tauxEpargne;
            interetsAccumules += epargneSimule.centimes * (tauxAnnuel / 12f);
            moisEcoulesEpargne++;

            // Versement annuel des intérêts en décembre (ou au bout de 12 mois)
            if (snapActuel.moisCalendrier == Mois.Decembre || moisEcoulesEpargne >= 12)
            {
                int interetsVerse = Mathf.RoundToInt(interetsAccumules);
                epargneSimule.centimes += interetsVerse;
                interetsAccumules = 0f;
                moisEcoulesEpargne = 0;
            }

            // 3. Déduction des dépenses réelles de consommation du joueur pour ce mois.
            // On calcule la somme de toutes les dépenses réelles du joueur (transactions négatives
            // du compte courant, hors transferts vers l'épargne). La stratégie Fourmi conserve
            // le même niveau de vie (mêmes dépenses) mais optimise le placement de l'excédent.
            long depensesConsommation = 0;
            if (snapActuel.joueur.comptes.ContainsKey("courant"))
            {
                var compteCourantReel = snapActuel.joueur.comptes["courant"];
                if (compteCourantReel.GetHistorique() != null && compteCourantReel.GetHistorique().GetHistorique() != null)
                {
                    foreach (var transac in compteCourantReel.GetHistorique().GetHistorique())
                    {
                        // Les transactions de type "courant vers epargne" sont des placements d'épargne,
                        // pas des dépenses de consommation (nourriture, plaisir, etc.). On les exclut.
                        if (transac.montant.centimes < 0 && transac.libelle != "courant vers epargne")
                        {
                            depensesConsommation += -transac.montant.centimes;
                        }
                    }
                }
            }
            courantSimule.centimes -= (int)depensesConsommation;

            // 4. Optimisation des flux : maintien d'un buffer courant de 500 € (50 000 centimes)
            int cibleBuffer = 50000;
            if (courantSimule.centimes > cibleBuffer)
            {
                // Transfert de l'excédent vers le compte d'épargne rémunéré
                int excedent = courantSimule.centimes - cibleBuffer;
                courantSimule.centimes -= excedent;
                epargneSimule.centimes += excedent;
            }
            else if (courantSimule.centimes < cibleBuffer && epargneSimule.centimes > 0)
            {
                // Retrait depuis l'épargne vers le courant pour restaurer le buffer de sécurité
                int manque = cibleBuffer - courantSimule.centimes;
                int montantRetire = Mathf.Min(manque, epargneSimule.centimes);
                epargneSimule.centimes -= montantRetire;
                courantSimule.centimes += montantRetire;
            }

            // Enregistrement de l'état simulé à la fin du mois
            resultats.Add(new SimulationResult
            {
                indexMois = snapActuel.indexMois,
                moisCalendrier = snapActuel.moisCalendrier,
                soldeCourant = courantSimule,
                soldeEpargne = epargneSimule,
                patrimoineTotal = courantSimule + epargneSimule
            });
        }

        return resultats;
    }
}
