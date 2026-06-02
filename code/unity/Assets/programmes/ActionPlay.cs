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

        //Donne un salaire au joueur
        gameData.comptes["courant"].AjoutHistorique("salaire", gameData.salaire);
        gameData.comptes["courant"].AjoutHistorique("plaisir",new argent(1));

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
