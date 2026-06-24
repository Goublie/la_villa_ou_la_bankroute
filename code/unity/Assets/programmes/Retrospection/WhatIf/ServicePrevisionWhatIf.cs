using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Estimation mensuelle d'un actif construite uniquement avec les donnees
/// disponibles au mois observe.
/// </summary>
[Serializable]
public sealed class PrevisionActifWhatIf
{
    public string actifId;
    public int moisObservation;
    public int nombreObservations;
    public int nombreImpactsActifs;
    public float rendementMoyenMensuelPourcent;
    public float tendanceRecenteMensuellePourcent;
    public float effetEvenementsConnusPourcent;
    public float rendementMensuelEstimePourcent;
    public float volatiliteMensuellePourcent;
    public float risqueEstimePourcent;
    public float drawdownHistoriquePourcent;
    public float confiance01;
    public string explication;
}

/// <summary>
/// Produit des previsions deterministes sans jamais lire un prix posterieur au
/// mois d'observation.
/// </summary>
/// <remarks>
/// Les prix futurs du fichier historique ne sont pas utilises. Les seuls effets
/// d'evenements pris en compte sont ceux deja presents dans DonneesBourse et
/// actifs au mois observe.
/// </remarks>
public static class ServicePrevisionWhatIf
{
    private const int FenetreHistoriqueMois = 12;
    private const int FenetreRecenteMois = 3;
    private const float PoidsTendanceRecente = 0.65f;
    private const float PoidsMoyenneHistorique = 0.35f;
    private const float RendementMensuelMinimum = -25f;
    private const float RendementMensuelMaximum = 25f;

    /// <summary>
    /// Estime un actif du catalogue en construisant explicitement un historique
    /// tronque au mois observe.
    /// </summary>
    public static PrevisionActifWhatIf Estimer(
        DefinitionActifFinancier actif,
        DonneesBourse donneesConnues,
        int moisObservation)
    {
        if (actif == null || actif.Prix == null || actif.Prix.Count == 0)
        {
            return CreerPrevisionNeutre(
                actif != null ? actif.Id : string.Empty,
                Math.Max(0, moisObservation),
                "Actif ou historique indisponible.");
        }

        int dernierIndex = Limiter(
            Math.Max(0, moisObservation),
            0,
            actif.Prix.Count - 1);

        List<float> prixConnus = new List<float>(dernierIndex + 1);
        for (int index = 0; index <= dernierIndex; index++)
        {
            prixConnus.Add(
                MarcheBoursier.ObtenirPrix(
                    actif.Id,
                    index,
                    donneesConnues));
        }

        IReadOnlyList<ImpactEvenementMarche> impactsConnus =
            donneesConnues != null
                ? donneesConnues.impactsMarche
                : null;

        return EstimerDepuisPrixConnus(
            actif.Id,
            prixConnus,
            impactsConnus,
            dernierIndex);
    }

    /// <summary>
    /// Surcharge pure utilisee par les tests et par le futur moteur de recherche.
    /// Meme si la liste contient des valeurs futures, seuls les index inferieurs
    /// ou egaux au mois d'observation sont lus.
    /// </summary>
    public static PrevisionActifWhatIf EstimerDepuisPrixConnus(
        string actifId,
        IReadOnlyList<float> prix,
        IReadOnlyList<ImpactEvenementMarche> impactsConnus,
        int moisObservation)
    {
        if (prix == null || prix.Count == 0)
        {
            return CreerPrevisionNeutre(
                actifId,
                Math.Max(0, moisObservation),
                "Historique de prix vide.");
        }

        int indexActuel = Limiter(
            Math.Max(0, moisObservation),
            0,
            prix.Count - 1);

        List<float> rendements = CalculerRendements(
            prix,
            indexActuel,
            FenetreHistoriqueMois);
        List<float> rendementsRecents = CalculerRendements(
            prix,
            indexActuel,
            FenetreRecenteMois);

        float moyenneHistorique = CalculerMoyenne(rendements);
        float tendanceRecente = rendementsRecents.Count > 0
            ? CalculerMoyenne(rendementsRecents)
            : moyenneHistorique;
        float volatilite = CalculerEcartType(
            rendements,
            moyenneHistorique);
        float drawdown = CalculerDrawdownMaximum(
            prix,
            indexActuel,
            FenetreHistoriqueMois);

        int nombreImpactsActifs = 0;
        float effetEvenements = 0f;
        float multiplicateurVolatilite = 1f;

        if (impactsConnus != null)
        {
            foreach (ImpactEvenementMarche impact in impactsConnus)
            {
                if (impact == null ||
                    impact.actifId != actifId ||
                    !impact.EstActif(indexActuel))
                {
                    continue;
                }

                nombreImpactsActifs++;
                effetEvenements += impact.tendanceMensuellePourcent;
                multiplicateurVolatilite *= Limiter(
                    impact.coefficientVolatilite,
                    0.1f,
                    5f);
            }
        }

        float rendementEstime =
            (tendanceRecente * PoidsTendanceRecente) +
            (moyenneHistorique * PoidsMoyenneHistorique) +
            effetEvenements;
        rendementEstime = Limiter(
            rendementEstime,
            RendementMensuelMinimum,
            RendementMensuelMaximum);

        float risqueEstime = Limiter(
            volatilite * multiplicateurVolatilite,
            0f,
            100f);

        float couvertureHistorique = Limiter(
            rendements.Count / (float)FenetreHistoriqueMois,
            0f,
            1f);
        float penaliteRisque = 1f - (Math.Min(75f, risqueEstime) / 150f);
        float confiance = Limiter(
            couvertureHistorique * penaliteRisque,
            0f,
            1f);

        return new PrevisionActifWhatIf
        {
            actifId = actifId ?? string.Empty,
            moisObservation = indexActuel,
            nombreObservations = rendements.Count,
            nombreImpactsActifs = nombreImpactsActifs,
            rendementMoyenMensuelPourcent = moyenneHistorique,
            tendanceRecenteMensuellePourcent = tendanceRecente,
            effetEvenementsConnusPourcent = effetEvenements,
            rendementMensuelEstimePourcent = rendementEstime,
            volatiliteMensuellePourcent = volatilite,
            risqueEstimePourcent = risqueEstime,
            drawdownHistoriquePourcent = drawdown,
            confiance01 = confiance,
            explication = ConstruireExplication(
                tendanceRecente,
                moyenneHistorique,
                effetEvenements,
                risqueEstime,
                nombreImpactsActifs,
                rendements.Count)
        };
    }

    private static List<float> CalculerRendements(
        IReadOnlyList<float> prix,
        int indexActuel,
        int maximumMois)
    {
        List<float> rendements = new List<float>();
        if (prix == null || prix.Count < 2 || indexActuel < 1)
        {
            return rendements;
        }

        int premierIndex = Math.Max(
            1,
            indexActuel - Math.Max(1, maximumMois) + 1);

        for (int index = premierIndex; index <= indexActuel; index++)
        {
            float precedent = prix[index - 1];
            float actuel = prix[index];
            if (precedent <= 0f || actuel <= 0f)
            {
                continue;
            }

            rendements.Add(((actuel / precedent) - 1f) * 100f);
        }

        return rendements;
    }

    private static float CalculerMoyenne(IReadOnlyList<float> valeurs)
    {
        if (valeurs == null || valeurs.Count == 0)
        {
            return 0f;
        }

        double total = 0d;
        for (int index = 0; index < valeurs.Count; index++)
        {
            total += valeurs[index];
        }

        return (float)(total / valeurs.Count);
    }

    private static float CalculerEcartType(
        IReadOnlyList<float> valeurs,
        float moyenne)
    {
        if (valeurs == null || valeurs.Count <= 1)
        {
            return 0f;
        }

        double variance = 0d;
        for (int index = 0; index < valeurs.Count; index++)
        {
            double ecart = valeurs[index] - moyenne;
            variance += ecart * ecart;
        }

        return (float)Math.Sqrt(variance / valeurs.Count);
    }

    private static float CalculerDrawdownMaximum(
        IReadOnlyList<float> prix,
        int indexActuel,
        int maximumMois)
    {
        if (prix == null || prix.Count == 0)
        {
            return 0f;
        }

        int premierIndex = Math.Max(
            0,
            indexActuel - Math.Max(1, maximumMois) + 1);
        float sommet = 0f;
        float drawdownMaximum = 0f;

        for (int index = premierIndex; index <= indexActuel; index++)
        {
            float valeur = prix[index];
            if (valeur <= 0f)
            {
                continue;
            }

            sommet = Math.Max(sommet, valeur);
            if (sommet <= 0f)
            {
                continue;
            }

            float drawdown = ((sommet - valeur) / sommet) * 100f;
            drawdownMaximum = Math.Max(drawdownMaximum, drawdown);
        }

        return drawdownMaximum;
    }

    private static PrevisionActifWhatIf CreerPrevisionNeutre(
        string actifId,
        int moisObservation,
        string explication)
    {
        return new PrevisionActifWhatIf
        {
            actifId = actifId ?? string.Empty,
            moisObservation = Math.Max(0, moisObservation),
            explication = explication ?? string.Empty
        };
    }

    private static string ConstruireExplication(
        float tendanceRecente,
        float moyenneHistorique,
        float effetEvenements,
        float risque,
        int nombreImpacts,
        int nombreObservations)
    {
        CultureInfo culture = CultureInfo.InvariantCulture;
        return
            "Tendance recente " +
            tendanceRecente.ToString("0.00", culture) +
            " %, moyenne connue " +
            moyenneHistorique.ToString("0.00", culture) +
            " %, effet confirme " +
            effetEvenements.ToString("0.00", culture) +
            " %, risque " +
            risque.ToString("0.00", culture) +
            " %. Impacts actifs : " +
            nombreImpacts +
            ", observations : " +
            nombreObservations +
            ".";
    }

    private static int Limiter(int valeur, int minimum, int maximum)
    {
        return Math.Min(maximum, Math.Max(minimum, valeur));
    }

    private static float Limiter(float valeur, float minimum, float maximum)
    {
        return Math.Min(maximum, Math.Max(minimum, valeur));
    }
}