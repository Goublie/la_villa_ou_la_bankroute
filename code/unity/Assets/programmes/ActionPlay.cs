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

        //Active les fontions d'affichage
        moisPasse?.Invoke();

        //Donne un salaire au joueur
        gameData.comptes["courant"].AjoutHistorique("salaire", gameData.salaire);
    }
    
    private void incrementerMois()
    {
        if (gameData != null)
        {
            gameData.moisPasse++;
        }
    }
}
