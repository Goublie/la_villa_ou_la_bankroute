using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Historique 
{
    private List<Transaction> histo;

    public Historique()
    {
        histo = new List<Transaction>();
    }

    //Ajoute une transaction à partir d'une transaction déjà créée
    public void Add(Transaction transaction)
    {
        histo.Insert(0,transaction);
    }

    //Ajoute une transaction à l'historique à partir d'un labelle et d'un montant
    public void Add(string libelle, argent somme)
    {
        Add(new Transaction(libelle,somme));
    }

    //Retourne le montant associé au libelle, 0 si le libelle n'est pas trouvé
    public int GetIndiceDeLibelle(string libelle)
    {
        for (int i=0; i<histo.Count; i++)
        {
            if (histo[i].libelle == libelle)
            {
                return i;
            }
        }
        Debug.Log("Aucun montant associé à ce libelle");
        return -1; //Valeur par défaut
    }

    //Modifie le montant associé au libelle, ou ajoute la dépenses si le libelle n'existe pas encore    
    public void ModifieOuAjoute(string libelle, argent nouveauMontant)
    {
        int indice = GetIndiceDeLibelle(libelle);
        
        if(indice == -1)
        {
            // La dépense n'existe pas encore, on la crée
            Add(libelle, nouveauMontant);
        }
        else
        {
            // Elle existe déjà, on la met à jour
            histo[indice].montant = nouveauMontant;
        }
    }

    //Vide l'historique
    public void Clear()
    {
        histo.Clear();
    }

    ///////////////
    /// GETTERS ///
    ///////////////
    public List<Transaction> GetHistorique()
    {
        return histo;
    }

    public List<argent> GetMontants()
    {
        List<argent> ListMontants = new List<argent>();
        foreach(Transaction t in histo)
        {
            ListMontants.Add(t.montant);
        }
        return ListMontants;
    }

    public int GetSize()
    {
        return histo.Count;
    }
}