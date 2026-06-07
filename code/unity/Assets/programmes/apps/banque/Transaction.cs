using UnityEngine;

public class Transaction
{
    public string libelle;
    public argent montant;

    public Transaction(string _libelle, argent _montant)
    {
        if (_libelle == "")
        {
            Debug.Log("Libelle vide");
            return;
        }
        libelle = _libelle;
        montant = _montant;
    }

    public Transaction(string _libelle, int montant) : this(_libelle, new argent(montant))
    {}
}