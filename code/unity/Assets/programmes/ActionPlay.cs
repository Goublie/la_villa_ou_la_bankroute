using System;
using UnityEngine;

public class ActionPlay : MonoBehaviour
{
    public GameData gameData; // Référence vers ton ScriptableObject

    public static event Action moisPasse;

    public void Jouer()
    {
        incrementerMois();
        foreach (Investissement invest in gameData.investissements)
        {
            invest.ComposerBenefices();
        }

        // Vide l'historique de tous les comptes pour le nouveau mois
        foreach (var compte in gameData.comptes.Values)
        {
            compte.ViderHistorique();
        }

        //Donne un salaire au joueur
        gameData.comptes["courant"].AjoutHistorique("salaire", gameData.salaire);

        //Active les fontions d'affichage
        moisPasse?.Invoke();
    }
    
    private void incrementerMois()
    {
        if (gameData != null)
        {
            gameData.moisPasse++;
        }
    }
}
