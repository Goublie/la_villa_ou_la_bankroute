using System;

/// <summary>
/// Encapsule l'ensemble des variables macroéconomiques et environnementales externes au joueur (marchés, taux, news).
/// </summary>
[System.Serializable]
public class DonneesEnvironnement
{
    // Le taux d'intérêt annuel réglementé du Livret A (modélisé selon les prédictions)
    public float tauxEpargne = 0.0175f;
}
