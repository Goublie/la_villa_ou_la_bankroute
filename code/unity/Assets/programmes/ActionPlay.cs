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

        //Active toutes les fonctions liées à cette action
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
