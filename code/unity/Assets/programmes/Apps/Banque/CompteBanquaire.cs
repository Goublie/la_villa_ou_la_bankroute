using System;
using UnityEngine;
using System.Collections.Generic;

public class CompteBanquaire : IPatrimoine
{
    protected Historique historique; //Contient toutes les entrés/sorties d'argent lie la source à son montant
    private argent totalEntree; // somme total d'argent positif
    private argent totalSortie; // somme total d'argent négatif
    protected argent solde; // solde du compte au début du mois
    private argent soldeFinMois; // Solde du compte après calcul

    public event Action OnSoldeModifie;

    public CompteBanquaire(argent montantInitial = default)
    {
        historique = new Historique();
        totalEntree = montantInitial;
        totalSortie = new argent(0);
        solde = montantInitial;
    }

    public CompteBanquaire(Historique _historique)
    {
        historique = _historique;
        CalculEntree();
        CalculSortie();
        CalculSolde();
    }

    //Calcul et renvoie la somme des entrées d'argent
    public argent CalculEntree()
    {
        totalEntree = new argent(0);
        foreach(argent montant in historique.GetMontants())
        {
            if(montant.centimes >= 0)
            {
                totalEntree +=montant;
            }
        }
        return totalEntree;
    }

    //Calcul et renvoie la somme des sorties d'argent
    public argent CalculSortie()
    {
        totalSortie = new argent(0);
        foreach(argent montant in historique.GetMontants())
        {
            if(montant.centimes < 0)
            {
                totalSortie +=montant;
            }
        }
        return totalSortie;
    }

    //Calcul et renvoie le solde du compte
    public argent CalculSolde()
    {
        solde = totalEntree + totalSortie;
        OnSoldeModifie?.Invoke();
        return solde;
    }

    //Transfere de l'argent depuis le CompteBanquaire actuel vers le CompteBanquaire destination, en y ajoutant un libelle pour chaque compte, renvoie true en cas de réussite, false sinon.
    public bool Transferer(CompteBanquaire destination,string libeleSource, string libeleDestination, argent somme )
    {
        if (somme.centimes <= 0)
        {
            Debug.Log("transfert d'une somme négative impossible");
            return false;
        }
        if(solde < somme)
        {
            Debug.Log("La somme du compte source est trop faible");
            return false;
        }
        AjoutHistorique(libeleSource, -somme);
        destination.AjoutHistorique(libeleDestination, somme);
        return true;
    }

    //Ajoute une transaction à l'istorique et recalcule le solde et le montant des sorties ou entrées
    public virtual void AjoutHistorique(string libelle, argent montant)
    {
        Debug.Log("Ajout dans l'historique");
        historique.Add(libelle,montant);
        if (montant.centimes >0)
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

    // Vide l'historique et réinitialise les compteurs d'entrée/sortie pour le nouveau mois
    public void ViderHistorique()
    {
        historique.Clear();
        totalEntree = solde; // Le solde actuel devient la base du nouveau mois
        totalSortie = new argent(0);
        CalculSolde();
    }

    ///////////////
    /// GETTERS ///
    ///////////////

    //Renvoie l'historique du compte
    public Historique GetHistorique()
    {
        return historique;
    }

    //Renvoie le solde du compte
    public argent GetSolde()
    {
        return solde;
    }

    // Implémentation de IPatrimoine
    public argent GetValeurPatrimoine()
    {
        return GetSolde();
    }
}