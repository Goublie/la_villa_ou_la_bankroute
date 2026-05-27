using System;
using System.Collections.Generic;

public class CompteBanquaire
{
    private Dictionnary<string, argent> historique; //Contient toutes les entrés/sorties d'argent lie la source à son montant
    private argent totalEntree; // somme total d'argent positif
    private argent totalSortie; // somme total d'argent négatif
    private argent Solde; // solde du compte

    CompteBanquaire()
    {
        historique = new Dictionnary<string, argent>();
    }

    CompteBanquaire(Dictionnary<string, argent> _historique)
    {
        historique = _historique;
        calculEntree;
        calculSortie;
        calculSolde;
    }

    //Calcul et renvoie la somme des entrées d'argent
    public argent calculEntree()
    {
        totalEntree = 0;
        foreach(argent montant in historique.Values)
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
        totalSortie = 0;
        foreach(argent montant in historique.Values)
        {
            if(montant.centimes < 0)
            {
                totalSortie +=montant;
            }
        }
        return totalSortie;
    }

    public argent calculSolde()
    {
        Solde = totalEntree + totalSortie;
    }

    public void ajoutHistorique(string source, argent montant)
    {
        historique.add(source,montant);
        if (montant.centimes >0)
        {
            totalEntree += montant;
        }
        else
        {
            totalSortie += montant;
        }
        calculSolde();
    }

    ///////////////
    /// GETTERS ///
    ///////////////

    //Renvoie l'historique du compte
    public Dictionnary<string, argent> getHistorique()
    {
        return historique;
    }

    //Renvoie le solde du compte
    public argent getSolde()
    {
        return Solde;
    }
}