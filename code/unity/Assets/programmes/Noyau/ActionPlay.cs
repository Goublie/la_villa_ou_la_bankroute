using System;
using UnityEngine;

/// <summary>
/// Orchestre la cloture du mois courant et l'ouverture du mois suivant.
/// </summary>
public class ActionPlay : MonoBehaviour
{
    public GameData gameData;

    /// <summary>
    /// Notification visuelle envoyee apres la fin de toutes les mutations.
    /// </summary>
    public static event Action OnMoisPasse;

    private void Awake()
    {
        // Les objets de la scene precedente ne doivent pas rester abonnes.
        // Les services metier ne dependent plus de cet evenement statique.
        OnMoisPasse = null;
    }

    private void Start()
    {
        if (gameData == null)
        {
            return;
        }

        gameData.joueur?.InitialiserSiNecessaire();
        if (gameData.joueur != null && gameData.joueur.bourse != null)
        {
            new ServiceBourse(gameData.joueur.bourse)
                .AppliquerEvolutionMensuelle(
                    gameData.nombreMoisPasses);
        }
        if (gameData.historiqueSnapshots != null &&
            gameData.historiqueSnapshots.Count == 0)
        {
            EnregistrerSnapshot();
        }
    }

    /// <summary>
    /// Termine le mois courant puis prepare le mois suivant.
    /// </summary>
    /// <remarks>
    /// Ordre volontaire : evolutions du mois, valorisation et snapshot de
    /// cloture, changement de calendrier, nettoyage des historiques, salaire,
    /// puis notification des UI. Cet ordre evite qu'un snapshot depende de
    /// l'ouverture d'une application.
    /// </remarks>
    public void Jouer()
    {
        if (gameData == null || gameData.joueur == null)
        {
            Debug.LogError("[Temps] GameData ou DonneesJoueur manquant.");
            return;
        }

        DonneesJoueur joueur = gameData.joueur;
        joueur.InitialiserSiNecessaire();

        AppliquerEvolutionsMensuelles(joueur);
        if (joueur.bourse != null)
        {
            new ServiceBourse(joueur.bourse)
                .AppliquerEvolutionMensuelle(
                    gameData.nombreMoisPasses);
        }
        EnregistrerSnapshot();

        IncrementerMois();
        OuvrirNouveauMois(joueur);
        OnMoisPasse?.Invoke();
    }

    private void AppliquerEvolutionsMensuelles(DonneesJoueur joueur)
    {
        if (joueur.investissements != null)
        {
            foreach (Investissement investissement in joueur.investissements)
            {
                investissement?.AppliquerEvolutionMensuelle(
                    gameData.nombreMoisPasses);
            }
        }

        if (joueur.comptes != null &&
            joueur.comptes.TryGetValue(
                ServiceBanque.LivretAId,
                out CompteBanquaire compte) &&
            compte is Epargne epargne)
        {
            epargne.AppliquerEvolutionMensuelle(
                gameData.nombreMoisPasses);
            if (gameData.env != null)
            {
                gameData.env.tauxEpargne = epargne.GetTaux();
            }
        }
    }

    private void OuvrirNouveauMois(DonneesJoueur joueur)
    {
        if (joueur.comptes == null)
        {
            return;
        }

        foreach (CompteBanquaire compte in joueur.comptes.Values)
        {
            compte?.ViderHistorique();
        }

        ServiceBanque banque = new ServiceBanque(joueur);
        banque.ObtenirCompteCourant().AjoutHistorique(
            "salaire",
            joueur.salaire);
    }

    private void EnregistrerSnapshot()
    {
        if (gameData == null || gameData.historiqueSnapshots == null)
        {
            return;
        }

        gameData.historiqueSnapshots.Add(
            new SnapshotEtatJeu(gameData));
    }

    private void IncrementerMois()
    {
        gameData.nombreMoisPasses++;
        if (gameData.moisActuel == Mois.Decembre)
        {
            gameData.moisActuel = Mois.Janvier;
            if (ScenesManager.Instance != null)
            {
                ScenesManager.Instance.ChargerIntrospection();
            }
        }
        else
        {
            gameData.moisActuel =
                (Mois)((int)gameData.moisActuel + 1);
        }
    }
}
