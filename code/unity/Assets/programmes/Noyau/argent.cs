using System;
using UnityEngine;

/// <summary>
/// Valeur monetaire du jeu stockee sous forme de centimes entiers.
/// </summary>
/// <remarks>
/// Les services metier doivent privilegier le constructeur en centimes.
/// Le constructeur en euros flottants est conserve pour les anciens scripts
/// Unity et arrondit une seule fois a la frontiere d'entree.
/// </remarks>
[Serializable]
public struct argent : IEquatable<argent>
{
    /// <summary>
    /// Montant signe en centimes.
    /// </summary>
    public int centimes;

    /// <summary>
    /// Cree un montant exact a partir de centimes.
    /// </summary>
    public argent(int centimes)
    {
        this.centimes = centimes;
    }

    /// <summary>
    /// Cree un montant depuis des euros et arrondit au centime le plus proche.
    /// </summary>
    public argent(float somme)
        : this(Mathf.RoundToInt(somme * 100f))
    {
    }

    /// <summary>
    /// Formate le montant avec deux decimales et le symbole euro.
    /// </summary>
    public override string ToString()
    {
        decimal montant = centimes / 100m;
        return montant.ToString("F2") + " \u20AC";
    }

    /// <summary>
    /// Formate la valeur en euros selon un format numerique .NET.
    /// </summary>
    public string ToString(string format)
    {
        decimal montant = centimes / 100m;
        return montant.ToString(format);
    }

    /// <inheritdoc />
    public bool Equals(argent autre)
    {
        return centimes == autre.centimes;
    }

    /// <inheritdoc />
    public override bool Equals(object objet)
    {
        return objet is argent autre && Equals(autre);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return centimes;
    }

    public static argent operator +(argent a, argent b) =>
        new argent(a.centimes + b.centimes);

    public static argent operator -(argent a, argent b) =>
        new argent(a.centimes - b.centimes);

    public static argent operator -(argent a) =>
        new argent(-a.centimes);

    /// <summary>
    /// Soustrait un nombre entier d'euros. API conservee pour les anciennes UI.
    /// </summary>
    public static argent operator -(argent a, int euros) =>
        new argent(a.centimes - euros * 100);

    /// <summary>
    /// Ajoute un nombre entier d'euros. API conservee pour les anciennes UI.
    /// </summary>
    public static argent operator +(argent a, int euros) =>
        new argent(a.centimes + euros * 100);

    public static argent operator *(argent a, float multiplicateur) =>
        new argent((int)(a.centimes * multiplicateur));

    public static argent operator *(float multiplicateur, argent a) =>
        a * multiplicateur;

    public static bool operator >(argent a, argent b) =>
        a.centimes > b.centimes;

    public static bool operator <(argent a, argent b) =>
        a.centimes < b.centimes;

    public static bool operator >=(argent a, argent b) =>
        a.centimes >= b.centimes;

    public static bool operator <=(argent a, argent b) =>
        a.centimes <= b.centimes;

    public static bool operator ==(argent a, argent b) =>
        a.Equals(b);

    public static bool operator !=(argent a, argent b) =>
        !a.Equals(b);
}
