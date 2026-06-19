using System;

/// <summary>
/// Service metier de repartition et consommation du temps mensuel par application.
/// </summary>
public sealed class ServiceRepartitionTemps
{
    private readonly DonneesRepartitionTemps donnees;

    /// <summary>
    /// Cree un service lie a l'agregat de temps du joueur.
    /// </summary>
    public ServiceRepartitionTemps(DonneesRepartitionTemps donnees)
    {
        this.donnees = donnees ??
            throw new ArgumentNullException(nameof(donnees));
        this.donnees.InitialiserSiNecessaire();
    }

    /// <summary>
    /// Definit le budget mensuel en minutes et initialise les secondes restantes.
    /// </summary>
    /// <remarks>
    /// Effet de bord : remplace l'allocation precedente. La somme doit etre
    /// exactement egale au budget mensuel, afin que l'UI ne puisse pas valider
    /// un mois incomplet ou excedentaire.
    /// </remarks>
    public ResultatOperation DefinirAllocation(
        int minutesBanque,
        int minutesActualites,
        int minutesSalariat,
        int minutesBourse,
        int minutesEntrepreneuriat,
        int minutesImmobilier)
    {
        if (minutesBanque < 0 ||
            minutesActualites < 0 ||
            minutesSalariat < 0 ||
            minutesBourse < 0 ||
            minutesEntrepreneuriat < 0 ||
            minutesImmobilier < 0)
        {
            return ResultatOperation.Echec(
                "Le temps alloue ne peut pas etre negatif.",
                "temps_negatif");
        }

        int total = minutesBanque +
            minutesActualites +
            minutesSalariat +
            minutesBourse +
            minutesEntrepreneuriat +
            minutesImmobilier;
        if (total != donnees.budgetMensuelMinutes)
        {
            return ResultatOperation.Echec(
                "Le total doit etre egal au budget mensuel.",
                "budget_incomplet");
        }

        Appliquer(donnees.banque, minutesBanque);
        Appliquer(donnees.actualites, minutesActualites);
        Appliquer(donnees.salariat, minutesSalariat);
        Appliquer(donnees.bourse, minutesBourse);
        Appliquer(donnees.entrepreneuriat, minutesEntrepreneuriat);
        Appliquer(donnees.immobilier, minutesImmobilier);
        donnees.allocationValidee = true;
        return ResultatOperation.Reussite(
            "Temps alloue.",
            default,
            "temps_alloue");
    }

    /// <summary>
    /// Consomme du temps pour une application et borne le restant a zero.
    /// </summary>
    /// <returns>True si l'application dispose encore de temps apres consommation.</returns>
    public bool Consommer(TypeApplicationTemps type, float deltaSecondes)
    {
        if (deltaSecondes <= 0f)
        {
            return PeutOuvrir(type);
        }

        AllocationTempsApplication allocation = donnees.Obtenir(type);
        if (allocation == null)
        {
            return false;
        }

        allocation.secondesRestantes =
            Math.Max(0f, allocation.secondesRestantes - deltaSecondes);
        return allocation.secondesRestantes > 0f;
    }

    /// <summary>
    /// Retourne le temps restant d'une application en secondes.
    /// </summary>
    public float ObtenirSecondesRestantes(TypeApplicationTemps type)
    {
        AllocationTempsApplication allocation = donnees.Obtenir(type);
        return allocation == null ? 0f : allocation.secondesRestantes;
    }

    /// <summary>
    /// Indique si une fenetre peut etre ouverte selon son temps restant.
    /// </summary>
    public bool PeutOuvrir(TypeApplicationTemps type)
    {
        return EstAllocationValidee() &&
            ObtenirSecondesRestantes(type) > 0f;
    }

    /// <summary>
    /// Indique si la repartition mensuelle a ete validee.
    /// </summary>
    /// <remarks>
    /// Le passage au mois suivant se base sur cet etat, pas sur le temps
    /// restant, afin qu'un joueur ayant consomme ses trente minutes puisse
    /// quand meme terminer le mois.
    /// </remarks>
    public bool EstAllocationValidee()
    {
        donnees.InitialiserSiNecessaire();
        return donnees.allocationValidee;
    }

    /// <summary>
    /// Remet l'allocation a zero au debut d'une nouvelle phase mensuelle.
    /// </summary>
    public void ReinitialiserAllocation()
    {
        donnees.ReinitialiserAllocation();
    }

    private static void Appliquer(
        AllocationTempsApplication allocation,
        int minutes)
    {
        allocation.minutesInitiales = minutes;
        allocation.secondesRestantes = minutes * 60f;
    }
}
