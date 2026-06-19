using System;

/// <summary>
/// Agregat des variables economiques externes au joueur.
/// </summary>
[Serializable]
public class DonneesEnvironnement
{
    /// <summary>
    /// Taux annuel courant du Livret A, ou 0.0175 pour 1,75 %.
    /// </summary>
    public float tauxEpargne = 0.0175f;

    /// <summary>
    /// Produit une copie independante pour un snapshot.
    /// </summary>
    public DonneesEnvironnement Copier()
    {
        return new DonneesEnvironnement
        {
            tauxEpargne = tauxEpargne
        };
    }
}
