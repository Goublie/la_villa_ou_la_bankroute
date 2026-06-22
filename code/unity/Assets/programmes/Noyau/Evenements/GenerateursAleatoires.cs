using System;

/// <summary>
/// Contrat minimal d'un generateur aleatoire reproductible.
/// </summary>
public interface IGenerateurAleatoire
{
    /// <summary>
    /// Etat serialisable apres le dernier tirage.
    /// </summary>
    uint Etat { get; }

    /// <summary>
    /// Retourne un entier dans [minimumInclus, maximumExclus[.
    /// </summary>
    int ProchainEntier(int minimumInclus, int maximumExclus);

    /// <summary>
    /// Retourne une valeur dans [0, 1[.
    /// </summary>
    double ProchaineValeur();
}

/// <summary>
/// Base XorShift32 dont l'etat tient dans un entier serialisable.
/// </summary>
public abstract class GenerateurAleatoireXorShift : IGenerateurAleatoire
{
    private uint etat;

    protected GenerateurAleatoireXorShift(uint graine)
    {
        etat = NormaliserGraine(graine);
    }

    public uint Etat => etat;

    public int ProchainEntier(int minimumInclus, int maximumExclus)
    {
        if (maximumExclus <= minimumInclus)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumExclus),
                "La borne maximale doit depasser la borne minimale.");
        }

        uint amplitude = (uint)(maximumExclus - minimumInclus);
        return minimumInclus + (int)(ProchainEntierNonSigne() % amplitude);
    }

    public double ProchaineValeur()
    {
        return ProchainEntierNonSigne() / ((double)uint.MaxValue + 1d);
    }

    private uint ProchainEntierNonSigne()
    {
        uint valeur = etat;
        valeur ^= valeur << 13;
        valeur ^= valeur >> 17;
        valeur ^= valeur << 5;
        etat = NormaliserGraine(valeur);
        return etat;
    }

    private static uint NormaliserGraine(uint graine)
    {
        return graine == 0u ? 0x6D2B79F5u : graine;
    }
}

/// <summary>
/// Generateur du jeu initialise par une graine non deterministe ou restauree.
/// </summary>
public sealed class GenerateurAleatoireJeu : GenerateurAleatoireXorShift
{
    public GenerateurAleatoireJeu(uint etatPersistant)
        : base(etatPersistant == 0u ? CreerGraine() : etatPersistant)
    {
    }

    private static uint CreerGraine()
    {
        unchecked
        {
            long ticks = DateTime.UtcNow.Ticks;
            return (uint)(ticks ^ (ticks >> 32) ^ Environment.TickCount);
        }
    }
}

/// <summary>
/// Generateur a graine explicite pour les tests et simulations reproductibles.
/// </summary>
public sealed class GenerateurAleatoireDeterministe :
    GenerateurAleatoireXorShift
{
    public GenerateurAleatoireDeterministe(int graine)
        : base(unchecked((uint)graine))
    {
    }
}
