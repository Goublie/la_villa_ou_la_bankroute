using System;
using UnityEngine;

public class ActionPlay : MonoBehaviour
{
    public GameData gameData; // Référence vers ton ScriptableObject

    public static event Action moisPasse;

    public void Jouer()
    {
        IncrementerMois();

        if (gameData.investissements != null)
        {
            foreach (Investissement invest in gameData.investissements)
            {
                invest.ComposerBenefices();
            }
        }

        // Vide l'historique de tous les comptes pour le nouveau mois
        if (gameData.comptes != null)
        {
            foreach (var compte in gameData.comptes.Values)
            {
                compte.ViderHistorique();
            }
            
            // Donne un salaire au joueur
            if (gameData.comptes.ContainsKey("courant"))
            {
                gameData.comptes["courant"].AjoutHistorique("salaire", gameData.salaire);
            }
        }

        // Active les fonctions d'affichage
        moisPasse?.Invoke();
    }
    
    private void IncrementerMois()
    {
        if (gameData == null) return;

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
