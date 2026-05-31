using System;
using UnityEngine;
using System.Collections.Generic;

public class CompteBanquaire
{
    protected Historique historique; //Contient toutes les entrés/sorties d'argent lie la source à son montant
    private argent totalEntree; // somme total d'argent positif
    private argent totalSortie; // somme total d'argent négatif
    protected argent solde; // solde du compte au début du mois
    private argent soldeFinMois; // Solde du compte après calcul

    public event Action OnSoldeModifie;

    public CompteBanquaire()
    {
        argent montantInitial = new argent(100000);
        historique = new Historique();
        totalEntree = montantInitial;
        totalSortie = new argent(0);
        solde = montantInitial;
    }

    public CompteBanquaire(Historique _historique)
    {
        historique = _historique;
        calculEntree();
        calculSortie();
        calculSolde();
    }

    //Calcul et renvoie la somme des entrées d'argent
    public argent calculEntree()
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
    public argent calculSortie()
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
    public argent calculSolde()
    {
        solde = totalEntree + totalSortie;
        OnSoldeModifie?.Invoke();
        return solde;
    }

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
        historique.Add(libelle,montant);
        if (montant.centimes >0)
        {
            totalEntree += montant;
        }
        else
        {
            totalSortie += montant;
        }
        calculSolde();
        OnSoldeModifie?.Invoke();
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
}