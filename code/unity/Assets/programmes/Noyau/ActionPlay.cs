using System;
using UnityEngine;

/// Script responsable de la logique lors de l'appuis sur le bouton du passage de mois.
public class ActionPlay : MonoBehaviour
{
    
    public GameData gameData; //Stocke les données du jeu.

    public static event Action OnMoisPasse; // Permet de notifier tous les abonnés que le mois a changé, pour qu'ils agissent

    private void Awake()
    {
        //On réinitialise l'événement statique lors de l'initialisation de la scène
        //pour casser les anciens liens d'abonnement (notamment avec d'anciens objets Epargne)
        //et ainsi éviter des exceptions de type NullReferenceException après rechargement.
        OnMoisPasse = null;
    }

    private void Start()
    {
        // Si aucun snapshot n'est enregistré (lancement de la partie), on prend la photo de départ (Mois 0 / juillet 2026)
        if (gameData != null && gameData.historiqueSnapshots != null && gameData.historiqueSnapshots.Count == 0)
        {
            EnregistrerSnapshot();
            Debug.Log("[What-If] Photographie de l'état initial enregistrée.");
        }
    }

    // Fonction principale appelée par le bouton pour terminer le mois en cours et jouer le suivant.
    public void Jouer()
    {
        //Calcul et versement des bénéfices mensuels sur les investissements du joueur
        if (gameData.joueur.investissements != null)
        {
            foreach (Investissement invest in gameData.joueur.investissements)
            {
                invest.ComposerBenefices();
            }
        }

        // Prise de la photographie de fin de mois (juste avant le passage au mois suivant et le nettoyage des historiques)
        EnregistrerSnapshot();

        //Passage officiel au mois calendrier suivant et incrémentation du compteur de temps global
        IncrementerMois();
        if (gameData != null && gameData.joueur != null)
        {
            MarcheBoursier.MettreAJourValorisation(
                gameData.joueur.bourse,
                gameData.nombreMoisPasses);
        }

        //Remise à 0 des historiques du joueur        
        if (gameData.joueur.comptes != null)
        {
            foreach (var compte in gameData.joueur.comptes.Values)
            {
                // Vide l'historique des opérations du mois précédent pour démarrer le nouveau mois à zéro
                compte.ViderHistorique();
            }
            
            // Versement du revenu d'activité (salaire) sur le compte courant du joueur pour le nouveau mois
            if (gameData.joueur.comptes.ContainsKey("courant"))
            {
                gameData.joueur.comptes["courant"].AjoutHistorique("salaire", gameData.joueur.salaire);
            }
        }

        // Notification à tous les abonnés (mise à jour de l'affichage UI, réajustement des taux d'intérêt du Livret A)
        OnMoisPasse?.Invoke();
    }
    
    // Enregistre un instantané de l'état actuel de la partie
    private void EnregistrerSnapshot()
    {
        if (gameData != null && gameData.historiqueSnapshots != null)
        {
            SnapshotEtatJeu snapshot = new SnapshotEtatJeu(gameData);
            gameData.historiqueSnapshots.Add(snapshot);
        }
    }

    // Incrémente le mois calendrier. Si le joueur termine le mois de décembre, 
    // déclenche un bilan de fin d'année (introspection) et réinitialise à janvier.
    private void IncrementerMois()
    {
        if (gameData == null) return;

        // Incrémente le nombre total de mois passés dans le jeu
        gameData.nombreMoisPasses++;

        // Si on est en Décembre, on passe à Janvier et on change de scène
        if (gameData.moisActuel == Mois.Decembre)
        {
            gameData.moisActuel = Mois.Janvier;
            Debug.Log("Fin d'année ! Appel du ScenesManager...");
            if (ScenesManager.Instance != null)
            {
                ScenesManager.Instance.ChargerIntrospection();
            }
        }
        else
        {
            // Sinon on passe au mois suivant
            gameData.moisActuel = (Mois)((int)gameData.moisActuel + 1);
            Debug.Log("Nouveau mois : " + gameData.moisActuel);
        }
    }
}
