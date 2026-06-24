using System;

/// <summary>
/// Stocke les choix de niveau de vie du joueur pour les 5 catégories.
/// Chaque valeur est bornée entre 1 (minimum) et 5 (maximum).
/// </summary>
[Serializable]
public class DonneesNiveauVie
{
    public int logement     = 1;
    public int sport        = 1;
    public int transport    = 1;
    public int alimentation = 1;
    public int vieSociale   = 1;

    public DonneesNiveauVie() { }

    /// <summary>Copie profonde pour les snapshots What If.</summary>
    public DonneesNiveauVie Copier()
    {
        return new DonneesNiveauVie
        {
            logement     = this.logement,
            sport        = this.sport,
            transport    = this.transport,
            alimentation = this.alimentation,
            vieSociale   = this.vieSociale
        };
    }

    /// <summary>Borne une valeur entre 1 et 5.</summary>
    public static int Borner(int valeur) => Math.Clamp(valeur, 1, 5);
}
