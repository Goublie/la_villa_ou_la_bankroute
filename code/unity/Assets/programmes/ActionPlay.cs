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

    // Fonction principale appelée par le bouton pour terminer le mois en cours et jouer le suivant.
    public void Jouer()
    {
        //Calcul et versement des bénéfices mensuels sur les investissements du joueur
        if (gameData.investissements != null)
        {
            foreach (Investissement invest in gameData.investissements)
            {
                invest.ComposerBenefices();
            }
        }

        //Passage officiel au mois calendrier suivant et incrémentation du compteur de temps global
        IncrementerMois();

        //Remise à 0 des historiques du joueur        
        if (gameData.comptes != null)
        {
            foreach (var compte in gameData.comptes.Values)
            {
                // Vide l'historique des opérations du mois précédent pour démarrer le nouveau mois à zéro
                compte.ViderHistorique();
            }
            
            // Versement du revenu d'activité (salaire) sur le compte courant du joueur pour le nouveau mois
            if (gameData.comptes.ContainsKey("courant"))
            {
                gameData.comptes["courant"].AjoutHistorique("salaire", gameData.salaire);
            }
        }

        // Notification à tous les abonnés (mise à jour de l'affichage UI, réajustement des taux d'intérêt du Livret A)
        OnMoisPasse?.Invoke();
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
