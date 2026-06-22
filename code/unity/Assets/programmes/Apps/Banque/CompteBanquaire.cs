using System;
using UnityEngine;

/// <summary>
/// Represente un compte monetaire possedant un solde et un historique signe.
/// </summary>
[Serializable]
public class CompteBanquaire : IPatrimoine
{
    protected Historique historique;
    private argent totalEntree;
    private argent totalSortie;
    protected argent solde;
    private argent soldeFinMois;

    /// <summary>
    /// Signale qu'une operation a modifie le solde.
    /// </summary>
    public event Action OnSoldeModifie;

    /// <summary>
    /// Cree un compte avec un montant initial exprime en centimes.
    /// </summary>
    public CompteBanquaire(argent montantInitial = default)
    {
        historique = new Historique();
        totalEntree = montantInitial.centimes >= 0
            ? montantInitial
            : new argent(0);
        totalSortie = montantInitial.centimes < 0
            ? montantInitial
            : new argent(0);
        solde = montantInitial;
        soldeFinMois = montantInitial;
    }

    /// <summary>
    /// Reconstruit un compte depuis un historique dont la somme porte le solde.
    /// </summary>
    public CompteBanquaire(Historique historique)
    {
        this.historique = historique ?? new Historique();
        RecalculerTotaux();
    }

    protected CompteBanquaire(
        Historique historique,
        argent totalEntree,
        argent totalSortie,
        argent solde,
        argent soldeFinMois)
    {
        this.historique = historique ?? new Historique();
        this.totalEntree = totalEntree;
        this.totalSortie = totalSortie;
        this.solde = solde;
        this.soldeFinMois = soldeFinMois;
    }

    /// <summary>
    /// Calcule la somme des entrees positives en centimes.
    /// </summary>
    public argent CalculEntree()
    {
        totalEntree = new argent(0);
        foreach (argent montant in historique.GetMontants())
        {
            if (montant.centimes >= 0)
            {
                totalEntree += montant;
            }
        }

        return totalEntree;
    }

    /// <summary>
    /// Calcule la somme des sorties negatives en centimes.
    /// </summary>
    public argent CalculSortie()
    {
        totalSortie = new argent(0);
        foreach (argent montant in historique.GetMontants())
        {
            if (montant.centimes < 0)
            {
                totalSortie += montant;
            }
        }

        return totalSortie;
    }

    /// <summary>
    /// Recalcule le solde depuis les totaux courants.
    /// </summary>
    public argent CalculSolde()
    {
        solde = totalEntree + totalSortie;
        soldeFinMois = solde;
        return solde;
    }

    /// <summary>
    /// Transfere une somme strictement positive vers un autre compte.
    /// </summary>
    /// <remarks>
    /// Cette methode modifie les deux historiques et les deux soldes. Elle est
    /// conservee comme API compatible ; les nouvelles commandes passent par
    /// <see cref="ServiceBanque"/> afin de recevoir un resultat detaille.
    /// </remarks>
    public bool Transferer(
        CompteBanquaire destination,
        string libelleSource,
        string libelleDestination,
        argent somme)
    {
        if (destination == null ||
            somme.centimes <= 0 ||
            solde < somme)
        {
            return false;
        }

        AjoutHistorique(libelleSource, -somme);
        destination.AjoutHistorique(libelleDestination, somme);
        return true;
    }

    /// <summary>
    /// Ajoute une operation signee et actualise le solde.
    /// </summary>
    /// <remarks>
    /// Effet de bord : declenche une fois <see cref="OnSoldeModifie"/>.
    /// </remarks>
    public virtual void AjoutHistorique(string libelle, argent montant)
    {
        historique.Add(libelle, montant);
        if (montant.centimes > 0)
        {
            totalEntree += montant;
        }
        else
        {
            totalSortie += montant;
        }

        CalculSolde();
        OnSoldeModifie?.Invoke();
    }

    /// <summary>
    /// Clot la periode et reporte le solde comme base du nouveau mois.
    /// </summary>
    public void ViderHistorique()
    {
        historique.Clear();
        totalEntree = solde.centimes >= 0 ? solde : new argent(0);
        totalSortie = solde.centimes < 0 ? solde : new argent(0);
        soldeFinMois = solde;
        OnSoldeModifie?.Invoke();
    }

    /// <summary>
    /// Retourne l'historique de la periode courante.
    /// </summary>
    public Historique GetHistorique()
    {
        return historique;
    }

    /// <summary>
    /// Retourne le solde courant en centimes.
    /// </summary>
    public argent GetSolde()
    {
        return solde;
    }

    /// <inheritdoc />
    public argent GetValeurPatrimoine()
    {
        return GetSolde();
    }

    /// <summary>
    /// Produit une copie profonde du compte et de son historique.
    /// </summary>
    public virtual CompteBanquaire Copier()
    {
        return new CompteBanquaire(
            historique.Copier(),
            new argent(totalEntree.centimes),
            new argent(totalSortie.centimes),
            new argent(solde.centimes),
            new argent(soldeFinMois.centimes));
    }

    protected argent GetTotalEntree()
    {
        return totalEntree;
    }

    protected argent GetTotalSortie()
    {
        return totalSortie;
    }

    protected argent GetSoldeFinMois()
    {
        return soldeFinMois;
    }

    private void RecalculerTotaux()
    {
        CalculEntree();
        CalculSortie();
        CalculSolde();
    }
}
