using System.Collections.Generic;

/// <summary>
/// Catalogue des actifs accessibles dans l'application Bourse.
/// </summary>
/// <remarks>
/// Les noms, risques et sources de donnees sont centralises ici afin que
/// l'interface ne porte plus de configuration metier. L'actif defensif
/// "livret_a" est un indice de comparaison boursier historique et reste
/// distinct du compte Livret A gere par Banque.
/// </remarks>
public static class CatalogueActifs
{
    private static List<DefinitionActifFinancier> actifs;

    /// <summary>
    /// Retourne les definitions dont les donnees historiques sont disponibles.
    /// </summary>
    public static IReadOnlyList<DefinitionActifFinancier> ObtenirActifs()
    {
        if (actifs == null)
        {
            actifs = ConstruireCatalogue();
        }

        return actifs;
    }

    /// <summary>
    /// Recherche un actif par son identifiant stable.
    /// </summary>
    public static DefinitionActifFinancier Trouver(string actifId)
    {
        if (string.IsNullOrEmpty(actifId))
        {
            return null;
        }

        foreach (DefinitionActifFinancier actif in ObtenirActifs())
        {
            if (actif.Id == actifId)
            {
                return actif;
            }
        }

        return null;
    }

    private static List<DefinitionActifFinancier> ConstruireCatalogue()
    {
        List<DefinitionActifFinancier> resultat =
            new List<DefinitionActifFinancier>();

        Ajouter(
            resultat,
            "cac40",
            "CAC 40",
            CategorieActifFinancier.Indices,
            "Modere",
            "Indice des grandes capitalisations francaises.");
        Ajouter(
            resultat,
            "nvidia",
            "Nvidia",
            CategorieActifFinancier.Actions,
            "Eleve",
            "Action technologique a forte croissance et forte volatilite.");
        Ajouter(
            resultat,
            "alphabet",
            "Alphabet",
            CategorieActifFinancier.Actions,
            "Modere",
            "Groupe technologique diversifie.");
        Ajouter(
            resultat,
            "bitcoin",
            "Bitcoin",
            CategorieActifFinancier.Crypto,
            "Tres eleve",
            "Cryptoactif sensible au sentiment de marche.");
        Ajouter(
            resultat,
            "totalenergies",
            "TotalEnergies",
            CategorieActifFinancier.Energie,
            "Modere",
            "Action energetique sensible aux matieres premieres.");
        Ajouter(
            resultat,
            "livret_a",
            "Livret A",
            CategorieActifFinancier.Defensif,
            "Faible",
            "Indice defensif de comparaison fonde sur les taux du projet.");

        return resultat;
    }

    private static void Ajouter(
        List<DefinitionActifFinancier> resultat,
        string id,
        string nom,
        CategorieActifFinancier categorie,
        string niveauRisque,
        string description)
    {
        List<float> courbe = MarcheBoursier.ObtenirCourbe(id);
        if (courbe == null || courbe.Count == 0)
        {
            return;
        }

        resultat.Add(
            new DefinitionActifFinancier(
                id,
                nom,
                categorie,
                niveauRisque,
                description,
                courbe));
    }
}
