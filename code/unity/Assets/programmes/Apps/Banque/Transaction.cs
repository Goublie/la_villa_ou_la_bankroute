using System;
using Newtonsoft.Json; // SOLUTION : Ajout du namespace pour le JsonConstructor

/// <summary>
/// Represente une operation financiere signee dans l'historique d'un compte.
/// </summary>
[Serializable]
public class Transaction
{
    /// <summary>
    /// Libelle fonctionnel de l'operation.
    /// </summary>
    public string libelle;

    /// <summary>
    /// Montant signe exprime en centimes. Une sortie est negative.
    /// </summary>
    public argent montant;

    /// <summary>
    /// Index absolu du mois de jeu, ou -1 pour les anciennes operations.
    /// </summary>
    public int indexMois;

    /// <summary>
    /// Constructeur sans paramètre requis par Newtonsoft.Json pour la désérialisation.
    /// </summary>
    [JsonConstructor]
    public Transaction()
    {
        // Laisser vide : Newtonsoft va mapper directement "libelle", "montant" et "indexMois" depuis le JSON
    }

    /// <summary>
    /// Cree une transaction.
    /// </summary>
    public Transaction(string libelle, argent montant, int indexMois = -1)
    {
        this.libelle = string.IsNullOrWhiteSpace(libelle)
            ? "operation"
            : libelle.Trim();
        this.montant = montant;
        this.indexMois = indexMois;
    }

    /// <summary>
    /// Cree une transaction a partir d'un montant deja exprime en centimes.
    /// </summary>
    public Transaction(string libelle, int montantCentimes)
        : this(libelle, new argent(montantCentimes))
    {
    }

    /// <summary>
    /// Produit une copie independante utilisable dans un snapshot.
    /// </summary>
    public Transaction Copier()
    {
        return new Transaction(
            libelle,
            new argent(montant.centimes),
            indexMois);
    }
}