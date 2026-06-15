using System.Collections.Generic;

/// <summary>
/// Categories fonctionnelles utilisees pour presenter les actifs financiers.
/// </summary>
public enum CategorieActifFinancier
{
    Indices,
    Actions,
    Crypto,
    Energie,
    Defensif
}

/// <summary>
/// Definition immuable d'un actif disponible dans le catalogue Bourse.
/// </summary>
public sealed class DefinitionActifFinancier
{
    /// <summary>
    /// Cree une definition a partir de ses metadonnees et de sa courbe source.
    /// </summary>
    public DefinitionActifFinancier(
        string id,
        string nom,
        CategorieActifFinancier categorie,
        string niveauRisque,
        string description,
        List<float> prix)
    {
        Id = id;
        Nom = nom;
        Categorie = categorie;
        NiveauRisque = niveauRisque;
        Description = description;
        Prix = prix != null
            ? new List<float>(prix)
            : new List<float>();
    }

    /// <summary>
    /// Identifiant stable utilise par les positions et les evenements.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Nom affiche au joueur.
    /// </summary>
    public string Nom { get; }

    /// <summary>
    /// Categorie fonctionnelle utilisee pour filtrer le catalogue.
    /// </summary>
    public CategorieActifFinancier Categorie { get; }

    /// <summary>
    /// Niveau de risque descriptif affiche dans l'interface.
    /// </summary>
    public string NiveauRisque { get; }

    /// <summary>
    /// Description courte de l'actif.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Serie historique mensuelle en euros, indexee depuis le debut du jeu.
    /// </summary>
    public IReadOnlyList<float> Prix { get; }
}
