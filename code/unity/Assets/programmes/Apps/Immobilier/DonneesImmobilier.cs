using System;
using System.Collections.Generic;

public enum Ville
{
    Bordeaux,
    Lyon,
    Marseille,
    Nantes,
    Paris,
    Toulouse
}

public enum TypeBien
{
    Studio,
    AppartementT2,
    AppartementT4,
    ImmeubleRapport,
    LocalCommercial
}

/// <summary>
/// Représente un bien immobilier possédé par le joueur ou disponible sur le marché.
/// </summary>
[Serializable]
public class BienImmobilier : IPatrimoine
{
    public string idUnique;
    public Ville ville;
    public TypeBien type;
    public argent prixAchat;
    public argent valeurActuelle;
    public argent loyerMensuel;
    public bool estLoue;

    public BienImmobilier() 
    {
        idUnique = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// La valeur patrimoniale d'un bien est sa valeur actuelle sur le marché.
    /// </summary>
    public argent GetValeurPatrimoine()
    {
        return valeurActuelle;
    }
}