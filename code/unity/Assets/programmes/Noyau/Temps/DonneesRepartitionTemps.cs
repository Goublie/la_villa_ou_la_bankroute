using System;

/// <summary>
/// Identifie les applications dont le temps d'utilisation est arbitre chaque mois.
/// </summary>
public enum TypeApplicationTemps
{
    Aucun,
    Banque,
    Actualites,
    Salariat,
    Bourse,
    Entrepreneuriat
}

/// <summary>
/// Temps alloue et restant pour une application, exprime en minutes et secondes.
/// </summary>
[Serializable]
public class AllocationTempsApplication
{
    /// <summary>
    /// Budget choisi par le joueur pour le mois courant, en minutes entieres.
    /// </summary>
    public int minutesInitiales;

    /// <summary>
    /// Temps encore disponible pendant le mois courant, en secondes.
    /// </summary>
    public float secondesRestantes;

    /// <summary>
    /// Repare les valeurs negatives apres migration ou edition manuelle.
    /// </summary>
    public void InitialiserSiNecessaire()
    {
        minutesInitiales = Math.Max(0, minutesInitiales);
        secondesRestantes = Math.Max(0f, secondesRestantes);
    }

    /// <summary>
    /// Cree une copie profonde pour les snapshots et simulations What If.
    /// </summary>
    public AllocationTempsApplication Copier()
    {
        InitialiserSiNecessaire();
        return new AllocationTempsApplication
        {
            minutesInitiales = minutesInitiales,
            secondesRestantes = secondesRestantes
        };
    }
}

/// <summary>
/// Agregat persistant de la repartition mensuelle du temps joueur par application.
/// </summary>
/// <remarks>
/// Il ne remplace pas <see cref="ServicePassageMensuel"/> : il memorise seulement
/// le budget d'utilisation de chaque fenetre. Le passage de mois remet ce budget
/// a zero pour forcer une nouvelle repartition avant de rejouer le mois suivant.
/// </remarks>
[Serializable]
public class DonneesRepartitionTemps
{
    public const int BudgetMensuelMinutes = 30;

    public int budgetMensuelMinutes = BudgetMensuelMinutes;
    public AllocationTempsApplication banque =
        new AllocationTempsApplication();
    public AllocationTempsApplication actualites =
        new AllocationTempsApplication();
    public AllocationTempsApplication salariat =
        new AllocationTempsApplication();
    public AllocationTempsApplication bourse =
        new AllocationTempsApplication();
    public AllocationTempsApplication entrepreneuriat =
        new AllocationTempsApplication();

    /// <summary>
    /// Repare les sous-objets absents ou les valeurs hors bornes.
    /// </summary>
    public void InitialiserSiNecessaire()
    {
        budgetMensuelMinutes = Math.Max(1, budgetMensuelMinutes);
        banque = banque ?? new AllocationTempsApplication();
        actualites = actualites ?? new AllocationTempsApplication();
        salariat = salariat ?? new AllocationTempsApplication();
        bourse = bourse ?? new AllocationTempsApplication();
        entrepreneuriat =
            entrepreneuriat ?? new AllocationTempsApplication();

        banque.InitialiserSiNecessaire();
        actualites.InitialiserSiNecessaire();
        salariat.InitialiserSiNecessaire();
        bourse.InitialiserSiNecessaire();
        entrepreneuriat.InitialiserSiNecessaire();
    }

    /// <summary>
    /// Retourne l'allocation associee a une application.
    /// </summary>
    public AllocationTempsApplication Obtenir(TypeApplicationTemps type)
    {
        InitialiserSiNecessaire();
        switch (type)
        {
            case TypeApplicationTemps.Banque:
                return banque;
            case TypeApplicationTemps.Actualites:
                return actualites;
            case TypeApplicationTemps.Salariat:
                return salariat;
            case TypeApplicationTemps.Bourse:
                return bourse;
            case TypeApplicationTemps.Entrepreneuriat:
                return entrepreneuriat;
            default:
                return null;
        }
    }

    /// <summary>
    /// Calcule le total alloue, en minutes, toutes applications confondues.
    /// </summary>
    public int CalculerTotalMinutes()
    {
        InitialiserSiNecessaire();
        return banque.minutesInitiales +
            actualites.minutesInitiales +
            salariat.minutesInitiales +
            bourse.minutesInitiales +
            entrepreneuriat.minutesInitiales;
    }

    /// <summary>
    /// Indique si au moins une application dispose encore de temps ce mois-ci.
    /// </summary>
    public bool ATempsRestant()
    {
        InitialiserSiNecessaire();
        return banque.secondesRestantes > 0f ||
            actualites.secondesRestantes > 0f ||
            salariat.secondesRestantes > 0f ||
            bourse.secondesRestantes > 0f ||
            entrepreneuriat.secondesRestantes > 0f;
    }

    /// <summary>
    /// Remet toutes les allocations a zero pour ouvrir une nouvelle phase de choix.
    /// </summary>
    public void ReinitialiserAllocation()
    {
        InitialiserSiNecessaire();
        RemettreAZero(banque);
        RemettreAZero(actualites);
        RemettreAZero(salariat);
        RemettreAZero(bourse);
        RemettreAZero(entrepreneuriat);
    }

    /// <summary>
    /// Cree une copie profonde pour les snapshots et simulations What If.
    /// </summary>
    public DonneesRepartitionTemps Copier()
    {
        InitialiserSiNecessaire();
        return new DonneesRepartitionTemps
        {
            budgetMensuelMinutes = budgetMensuelMinutes,
            banque = banque.Copier(),
            actualites = actualites.Copier(),
            salariat = salariat.Copier(),
            bourse = bourse.Copier(),
            entrepreneuriat = entrepreneuriat.Copier()
        };
    }

    private static void RemettreAZero(AllocationTempsApplication allocation)
    {
        allocation.minutesInitiales = 0;
        allocation.secondesRestantes = 0f;
    }
}
