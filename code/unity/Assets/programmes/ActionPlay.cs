using System;
using UnityEngine;

public class ActionPlay : MonoBehaviour
{
    public GameData gameData; // Référence vers ton ScriptableObject
    public HUDManager hudManager; // Référence vers le HUDManager pour rafraîchir l'affichage

    public static event Action moisPasse;

    public void Jouer()
    {
        incrementerMois();
        foreach (Investissement invest in gameData.investissements)
        {
            invest.ComposerBenefices();
        }

        //Active toutes les fonctios liées à cette action
        moisPasse?.Invoke();
    }
    public void incrementerMois()
    {
        if (gameData != null)
        {
            gameData.moisPasse++;
        }
        if (hudManager != null)
        {
            hudManager.ActualiserAffichage();
        }
    }
}
