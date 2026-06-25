using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

/// <summary>
/// Parametres centralises de la premiere version du moteur d'evenements.
/// </summary>
public static class ParametresEvenements
{
    public const int RumeursParMois = 2;
    public const float ProbabiliteMinimale = 0.05f;
    public const float ProbabiliteMaximale = 0.95f;
    public const float SeuilVariationExtreme = 0.30f;
}

/// <summary>
/// Resultat explicite du chargement et de l'audit d'un catalogue.
/// </summary>
public sealed class ResultatChargementCatalogue
{
    public CatalogueEvenements Catalogue { get; internal set; }
    public List<string> Erreurs { get; } = new List<string>();
    public List<string> Avertissements { get; } = new List<string>();
    public bool EstValide => Catalogue != null && Erreurs.Count == 0;

    /// <summary>
    /// Resume les erreurs afin qu'un appelant puisse les journaliser ou tester.
    /// </summary>
    public string ConstruireMessageErreurs()
    {
        return Erreurs.Count == 0
            ? string.Empty
            : string.Join(" | ", Erreurs);
    }
}

/// <summary>
/// Catalogue valide des definitions d'evenements et de leurs sources.
/// </summary>
public sealed class CatalogueEvenements
{
    private const string ImportanceFaible = "Faible";
    private const string ImportanceModeree = "Mod\u00E9r\u00E9e";
    private const string ImportanceForte = "Forte";
    private const string ImportanceCritique = "Critique";

    private static readonly Dictionary<string, string>
        CorrespondancesActifsBourse =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "cac40", "cac40" },
            { "nvidia", "nvidia" },
            { "google", "alphabet" },
            { "alphabet", "alphabet" },
            { "bitcoin", "bitcoin" },
            { "totalenergies", "totalenergies" }
        };

    private readonly List<DefinitionEvenement> evenements;
    private readonly List<SourceActualite> sources;
    private readonly Dictionary<string, DefinitionEvenement> parId;
    private readonly Dictionary<string, SourceActualite> sourcesParId;

    private CatalogueEvenements(
        List<DefinitionEvenement> evenements,
        List<SourceActualite> sources,
        List<string> evenementsSansImpact,
        List<string> ciblesImpactInconnues,
        List<string> variationsExtremes)
    {
        this.evenements = evenements;
        this.sources = sources;
        EvenementsSansImpact = evenementsSansImpact;
        CiblesImpactInconnues = ciblesImpactInconnues;
        VariationsExtremes = variationsExtremes;
        parId = new Dictionary<string, DefinitionEvenement>();
        sourcesParId = new Dictionary<string, SourceActualite>();

        foreach (DefinitionEvenement evenement in evenements)
        {
            parId[evenement.id] = evenement;
        }

        foreach (SourceActualite source in sources)
        {
            sourcesParId[source.id] = source;
        }
    }

    public IReadOnlyList<DefinitionEvenement> Evenements => evenements;
    public IReadOnlyList<SourceActualite> Sources => sources;
    public IReadOnlyList<string> EvenementsSansImpact { get; }
    public IReadOnlyList<string> CiblesImpactInconnues { get; }
    public IReadOnlyList<string> VariationsExtremes { get; }

    /// <summary>
    /// Charge les deux fichiers versionnes situes dans Resources/Actualites.
    /// </summary>
    public static ResultatChargementCatalogue ChargerDepuisResources()
    {
        TextAsset texteEvenements = Resources.Load<TextAsset>(
            "Actualites/evenements_jeu_clean");
        TextAsset texteSources = Resources.Load<TextAsset>(
            "Actualites/sources");
        if (texteEvenements == null || texteSources == null)
        {
            ResultatChargementCatalogue absent =
                new ResultatChargementCatalogue();
            absent.Erreurs.Add(
                "Les fichiers Resources/Actualites sont introuvables.");
            return absent;
        }

        return ChargerDepuisJson(texteEvenements.text, texteSources.text);
    }

    /// <summary>
    /// Parse et valide des tableaux JSON sans lever d'exception silencieuse.
    /// </summary>
    public static ResultatChargementCatalogue ChargerDepuisJson(
        string jsonEvenements,
        string jsonSources)
    {
        ResultatChargementCatalogue resultat =
            new ResultatChargementCatalogue();
        try
        {
            ListeEvenements enveloppeEvenements =
                JsonUtility.FromJson<ListeEvenements>(
                    "{\"elements\":" + jsonEvenements + "}");
            ListeSources enveloppeSources =
                JsonUtility.FromJson<ListeSources>(
                    "{\"elements\":" + jsonSources + "}");
            List<DefinitionEvenement> evenements =
                enveloppeEvenements?.elements;
            List<SourceActualite> sources = enveloppeSources?.elements;

            ValiderEtConstruire(evenements, sources, resultat);
        }
        catch (Exception exception)
        {
            resultat.Erreurs.Add(
                "JSON Actualites illisible : " + exception.Message);
        }

        return resultat;
    }

    /// <summary>
    /// Recherche une definition par identifiant stable.
    /// </summary>
    public DefinitionEvenement TrouverEvenement(string id)
    {
        return !string.IsNullOrWhiteSpace(id) &&
            parId.TryGetValue(id, out DefinitionEvenement evenement)
                ? evenement
                : null;
    }

    /// <summary>
    /// Recherche une source par identifiant stable.
    /// </summary>
    public SourceActualite TrouverSource(string id)
    {
        return !string.IsNullOrWhiteSpace(id) &&
            sourcesParId.TryGetValue(id, out SourceActualite source)
                ? source
                : null;
    }

    /// <summary>
    /// Filtre les sources avant tirage selon leurs domaines declares.
    /// </summary>
    public List<SourceActualite> ObtenirSourcesCompatibles(string categorie)
    {
        return sources.FindAll(source =>
            source != null && source.AccepteCategorie(categorie));
    }

    /// <summary>
    /// Traduit une cible JSON connue vers l'identifiant stable de la Bourse.
    /// </summary>
    public static bool EssayerObtenirActifBourse(
        string cibleImpact,
        out string actifId)
    {
        if (string.IsNullOrWhiteSpace(cibleImpact))
        {
            actifId = null;
            return false;
        }

        return CorrespondancesActifsBourse.TryGetValue(
            NormaliserCleActif(cibleImpact),
            out actifId);
    }

    /// <summary>
    /// Produit une cle comparable en ignorant casse, accents, espaces et
    /// ponctuation, sans inventer de correspondance semantique.
    /// </summary>
    private static string NormaliserCleActif(string valeur)
    {
        string decomposee = valeur.Trim().Normalize(
            NormalizationForm.FormD);
        StringBuilder cle = new StringBuilder(decomposee.Length);
        foreach (char caractere in decomposee)
        {
            UnicodeCategory categorie =
                CharUnicodeInfo.GetUnicodeCategory(caractere);
            if (categorie != UnicodeCategory.NonSpacingMark &&
                char.IsLetterOrDigit(caractere))
            {
                cle.Append(char.ToLowerInvariant(caractere));
            }
        }

        return cle.ToString();
    }

    private static void ValiderEtConstruire(
        List<DefinitionEvenement> evenements,
        List<SourceActualite> sources,
        ResultatChargementCatalogue resultat)
    {
        if (evenements == null || evenements.Count == 0)
        {
            resultat.Erreurs.Add("Le catalogue d'evenements est vide.");
        }

        if (sources == null || sources.Count == 0)
        {
            resultat.Erreurs.Add("Le catalogue de sources est vide.");
        }

        if (resultat.Erreurs.Count > 0)
        {
            return;
        }

        List<string> sansImpact = new List<string>();
        HashSet<string> ciblesInconnues =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        List<string> variationsExtremes = new List<string>();
        ValiderSources(sources, resultat);
        ValiderEvenements(
            evenements,
            sansImpact,
            ciblesInconnues,
            variationsExtremes,
            resultat);
        ValiderCouvertureCategories(evenements, sources, resultat);

        if (resultat.Erreurs.Count > 0)
        {
            return;
        }

        List<string> ciblesTriees = new List<string>(ciblesInconnues);
        ciblesTriees.Sort(StringComparer.OrdinalIgnoreCase);
        resultat.Catalogue = new CatalogueEvenements(
            evenements,
            sources,
            sansImpact,
            ciblesTriees,
            variationsExtremes);
        resultat.Avertissements.Add(
            sansImpact.Count + " evenement(s) sans impact brut.");
        resultat.Avertissements.Add(
            ciblesTriees.Count + " cible(s) d'impact non supportee(s).");
        resultat.Avertissements.Add(
            variationsExtremes.Count + " variation(s) d'au moins 30 %." );
    }

    private static void ValiderSources(
        List<SourceActualite> sources,
        ResultatChargementCatalogue resultat)
    {
        HashSet<string> ids = new HashSet<string>();
        foreach (SourceActualite source in sources)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.id))
            {
                resultat.Erreurs.Add("Une source n'a pas d'identifiant.");
                continue;
            }

            if (!ids.Add(source.id))
            {
                resultat.Erreurs.Add(
                    "Identifiant de source duplique : " + source.id);
            }

            if (string.IsNullOrWhiteSpace(source.nom))
            {
                resultat.Erreurs.Add("Source sans nom : " + source.id);
            }

            if (float.IsNaN(source.fiabilite) ||
                float.IsInfinity(source.fiabilite) ||
                source.fiabilite < 0f ||
                source.fiabilite > 1f)
            {
                resultat.Erreurs.Add(
                    "Fiabilite invalide pour la source " + source.id);
            }

            if (source.domaines == null || source.domaines.Count == 0)
            {
                resultat.Erreurs.Add(
                    "Aucun domaine pour la source " + source.id);
                continue;
            }

            foreach (string domaine in source.domaines)
            {
                if (!CategoriesEvenements.EstValide(domaine))
                {
                    resultat.Erreurs.Add(
                        "Domaine inconnu pour " + source.id + " : " + domaine);
                }
            }
        }
    }

    private static void ValiderEvenements(
        List<DefinitionEvenement> evenements,
        List<string> sansImpact,
        HashSet<string> ciblesInconnues,
        List<string> variationsExtremes,
        ResultatChargementCatalogue resultat)
    {
        HashSet<string> ids = new HashSet<string>();
        foreach (DefinitionEvenement evenement in evenements)
        {
            if (evenement == null || string.IsNullOrWhiteSpace(evenement.id))
            {
                resultat.Erreurs.Add("Un evenement n'a pas d'identifiant.");
                continue;
            }

            if (!ids.Add(evenement.id))
            {
                resultat.Erreurs.Add(
                    "Identifiant d'evenement duplique : " + evenement.id);
            }

            if (!CategoriesEvenements.EstValide(evenement.categorie))
            {
                resultat.Erreurs.Add(
                    "Categorie invalide pour " + evenement.id + " : " +
                    evenement.categorie);
            }

            if (!ImportanceValide(evenement.importance))
            {
                resultat.Erreurs.Add(
                    "Importance invalide pour " + evenement.id + " : " +
                    evenement.importance);
            }

            if (string.IsNullOrWhiteSpace(evenement.titre) ||
                string.IsNullOrWhiteSpace(evenement.message))
            {
                resultat.Erreurs.Add(
                    "Texte incomplet pour l'evenement " + evenement.id);
            }

            if (evenement.impacts == null || evenement.impacts.Count == 0)
            {
                sansImpact.Add(evenement.id);
                continue;
            }

            foreach (ImpactDefinitionEvenement impact in evenement.impacts)
            {
                if (impact == null || string.IsNullOrWhiteSpace(impact.actif))
                {
                    resultat.Erreurs.Add(
                        "Impact incomplet pour l'evenement " + evenement.id);
                    continue;
                }

                if (float.IsNaN(impact.variation) ||
                    float.IsInfinity(impact.variation))
                {
                    resultat.Erreurs.Add(
                        "Variation invalide pour l'evenement " + evenement.id);
                }

                bool cibleConnue = true;
                if (evenement.categorie == CategoriesEvenements.Boursiers)
                {
                    cibleConnue = EssayerObtenirActifBourse(
                        impact.actif,
                        out _);
                }
                else if (
                    evenement.categorie ==
                    CategoriesEvenements.Immobiliers)
                {
                    cibleConnue =
                        string.Equals(
                            impact.actif,
                            "Immobilier",
                            StringComparison.OrdinalIgnoreCase) &&
                        impact.dureeMois > 0 &&
                        !float.IsNaN(impact.variationLoyer) &&
                        !float.IsInfinity(impact.variationLoyer);
                }

                if (!cibleConnue)
                {
                    ciblesInconnues.Add(impact.actif);
                }

                if (Math.Abs(impact.variation) >=
                    ParametresEvenements.SeuilVariationExtreme)
                {
                    variationsExtremes.Add(
                        evenement.id + ":" + impact.actif + ":" +
                        impact.variation);
                }
            }
        }
    }

    private static void ValiderCouvertureCategories(
        List<DefinitionEvenement> evenements,
        List<SourceActualite> sources,
        ResultatChargementCatalogue resultat)
    {
        HashSet<string> categories = new HashSet<string>();
        foreach (DefinitionEvenement evenement in evenements)
        {
            if (evenement != null &&
                CategoriesEvenements.EstValide(evenement.categorie))
            {
                categories.Add(evenement.categorie);
            }
        }

        foreach (string categorie in categories)
        {
            bool compatible = sources.Exists(
                source => source != null && source.AccepteCategorie(categorie));
            if (!compatible)
            {
                resultat.Erreurs.Add(
                    "Aucune source compatible avec la categorie " + categorie);
            }
        }
    }

    private static bool ImportanceValide(string importance)
    {
        return importance == ImportanceFaible ||
            importance == ImportanceModeree ||
            importance == ImportanceForte ||
            importance == ImportanceCritique;
    }

    [Serializable]
    private class ListeEvenements
    {
        public List<DefinitionEvenement> elements;
    }

    [Serializable]
    private class ListeSources
    {
        public List<SourceActualite> elements;
    }
}
