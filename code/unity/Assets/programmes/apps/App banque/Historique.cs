using System;
using System.Collections.Generic;

[Serializable]
public class Historique 
{
    public List<string> libelles;
    public List<argent> montants;

    private int size; // taille de libelles ET montants
    public Historique()
    {
        libelles = new List<string>();
        montants = new List<argent>();
        size = 0;
    }

    public void Add(string libelle, argent somme)
    {
        libelles.Add(libelle);
        montants.Add(somme);
        size++;
    }

    //fonction qui retourne le monant a partir du libelle

    ///////////////
    /// GETTERS ///
    ///////////////
    public int GetSize()
    {
        return size;
    }
    public List<string> GetLibelles()
    {
        return libelles;
    }

    public List<argent> GetMontants()
    {
        return montants;
    }
}