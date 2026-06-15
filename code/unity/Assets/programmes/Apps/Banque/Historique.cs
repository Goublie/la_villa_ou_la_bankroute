using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Conserve les operations d'un compte, de la plus recente a la plus ancienne.
/// </summary>
[Serializable]
public class Historique
{
    private List<Transaction> histo = new List<Transaction>();

    /// <summary>
    /// Ajoute une transaction en tete de l'historique.
    /// </summary>
    public void Add(Transaction transaction)
    {
        if (transaction == null)
        {
            return;
        }

        AssurerListe();
        histo.Insert(0, transaction);
    }

    /// <summary>
    /// Cree puis ajoute une transaction signee.
    /// </summary>
    public void Add(string libelle, argent somme)
    {
        Add(new Transaction(libelle, somme));
    }

    /// <summary>
    /// Retourne l'indice du premier libelle correspondant, ou -1.
    /// </summary>
    public int GetIndiceDeLibelle(string libelle)
    {
        AssurerListe();
        for (int i = 0; i < histo.Count; i++)
        {
            if (histo[i] != null && histo[i].libelle == libelle)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Modifie la premiere operation portant ce libelle ou en cree une.
    /// </summary>
    /// <remarks>
    /// Cette methode est conservee pour les depenses mensuelles pilotees par
    /// les sliders. Les nouvelles operations ponctuelles doivent de preference
    /// ajouter une transaction distincte.
    /// </remarks>
    public void ModifieOuAjoute(string libelle, argent nouveauMontant)
    {
        int indice = GetIndiceDeLibelle(libelle);
        if (indice < 0)
        {
            Add(libelle, nouveauMontant);
            return;
        }

        histo[indice].montant = nouveauMontant;
    }

    /// <summary>
    /// Efface les operations de la periode courante.
    /// </summary>
    public void Clear()
    {
        AssurerListe();
        histo.Clear();
    }

    /// <summary>
    /// Retourne la liste mutable historique pour compatibilite avec les UI.
    /// </summary>
    public List<Transaction> GetHistorique()
    {
        AssurerListe();
        return histo;
    }

    /// <summary>
    /// Retourne une copie de la liste des montants signes.
    /// </summary>
    public List<argent> GetMontants()
    {
        AssurerListe();
        List<argent> montants = new List<argent>();
        foreach (Transaction transaction in histo)
        {
            if (transaction != null)
            {
                montants.Add(transaction.montant);
            }
        }

        return montants;
    }

    /// <summary>
    /// Retourne le nombre d'operations.
    /// </summary>
    public int GetSize()
    {
        AssurerListe();
        return histo.Count;
    }

    /// <summary>
    /// Produit une copie profonde de toutes les transactions.
    /// </summary>
    public Historique Copier()
    {
        Historique copie = new Historique();
        AssurerListe();

        // Add insere en tete : la copie doit donc parcourir du plus ancien
        // au plus recent pour conserver exactement l'ordre d'origine.
        for (int i = histo.Count - 1; i >= 0; i--)
        {
            if (histo[i] != null)
            {
                copie.Add(histo[i].Copier());
            }
        }

        return copie;
    }

    private void AssurerListe()
    {
        if (histo == null)
        {
            histo = new List<Transaction>();
        }
    }
}
