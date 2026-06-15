using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Charge les series historiques et calcule les prix affectes par les
/// evenements de marche.
/// </summary>
/// <remarks>
/// Cette couche est la source de prix du domaine Bourse. Elle ne connait ni
/// boutons, ni textes, ni prefabs.
/// </remarks>
public static class MarcheBoursier
{
    [Serializable]
    private class PointMarche
    {
        public float Close;
    }

    [Serializable]
    private class PointLivret
    {
        public float Rendement_mensuel_pct;
    }

    private static readonly Dictionary<string, string> CheminsResources =
        new Dictionary<string, string>
        {
            { "cac40", "Bourse/cac40" },
            { "nvidia", "Bourse/nvidia" },
            { "alphabet", "Bourse/alphabet" },
            { "totalenergies", "Bourse/totalenergies" },
            { "bitcoin", "Bourse/bitcoin" }
        };

    private static readonly Dictionary<string, List<float>> Courbes =
        new Dictionary<string, List<float>>();

    /// <summary>
    /// Retourne la serie mensuelle d'un actif, exprimee en euros.
    /// </summary>
    public static List<float> ObtenirCourbe(string actifId)
    {
        if (string.IsNullOrEmpty(actifId))
        {
            return new List<float>();
        }

        if (!Courbes.TryGetValue(actifId, out List<float> courbe))
        {
            courbe = actifId == "livret_a"
                ? ChargerCourbeLivretA()
                : ChargerCourbeMarche(actifId);
            Courbes[actifId] = courbe;
        }

        return courbe;
    }

    /// <summary>
    /// Recalcule la valeur totale d'un portefeuille pour le mois indique.
    /// </summary>
    /// <remarks>
    /// Effet de bord : met a jour le cache patrimonial de
    /// <paramref name="portefeuille"/>.
    /// </remarks>
    public static void MettreAJourValorisation(
        DonneesBourse portefeuille,
        int mois)
    {
        if (portefeuille == null)
        {
            return;
        }

        long valeurTotale = 0;
        if (portefeuille.positions != null)
        {
            foreach (PositionBourse position in portefeuille.positions)
            {
                if (position == null || position.quantite <= 0f)
                {
                    continue;
                }

                int prixCentimes = ObtenirPrixCentimes(
                    position.actifId,
                    mois,
                    portefeuille);
                valeurTotale += (long)Math.Round(
                    position.quantite * prixCentimes);
            }
        }

        portefeuille.DefinirValeurMarche(
            LimiterEnEntier(valeurTotale),
            Mathf.Max(0, mois));
    }

    /// <summary>
    /// Retourne le prix en euros apres application des impacts actifs.
    /// </summary>
    public static float ObtenirPrix(
        string actifId,
        int mois,
        DonneesBourse donnees)
    {
        List<float> courbe = ObtenirCourbe(actifId);
        if (courbe.Count == 0)
        {
            return 0f;
        }

        int index = Mathf.Clamp(mois, 0, courbe.Count - 1);
        float prix = courbe[index];

        if (donnees == null || donnees.impactsMarche == null)
        {
            return prix;
        }

        foreach (ImpactEvenementMarche impact in donnees.impactsMarche)
        {
            if (impact == null ||
                impact.actifId != actifId ||
                !impact.EstActif(index))
            {
                continue;
            }

            int debut = Mathf.Clamp(impact.moisDebut, 0, courbe.Count - 1);
            float prixDebut = courbe[debut];
            float volatilite = Mathf.Max(0f, impact.coefficientVolatilite);
            float tendance = Mathf.Pow(
                Mathf.Max(0.01f, 1f + impact.tendanceMensuellePourcent / 100f),
                Mathf.Max(0, index - debut));
            float rapportMarche = prixDebut > 0f ? prix / prixDebut : 1f;
            float rapportVolatilite =
                1f + (rapportMarche - 1f) * volatilite;

            prix *=
                Mathf.Max(0f, impact.coefficientPrix) *
                (rapportMarche > 0f ? rapportVolatilite / rapportMarche : 1f) *
                tendance;
        }

        return Mathf.Max(0f, prix);
    }

    /// <summary>
    /// Ajoute ou remplace un impact provenant d'un evenement metier.
    /// </summary>
    /// <remarks>
    /// L'impact est copie afin qu'une actualite ne puisse pas modifier
    /// retroactivement l'etat enregistre dans le portefeuille.
    /// </remarks>
    public static void AppliquerImpactEvenement(
        DonneesBourse donnees,
        ImpactEvenementMarche impact)
    {
        if (donnees == null ||
            impact == null ||
            string.IsNullOrEmpty(impact.actifId))
        {
            return;
        }

        if (donnees.impactsMarche == null)
        {
            donnees.impactsMarche = new List<ImpactEvenementMarche>();
        }

        if (!string.IsNullOrEmpty(impact.evenementId))
        {
            donnees.impactsMarche.RemoveAll(
                existant => existant != null &&
                    existant.evenementId == impact.evenementId);
        }

        donnees.impactsMarche.Add(impact.Copier());
    }

    private static int ObtenirPrixCentimes(
        string actifId,
        int mois,
        DonneesBourse donnees)
    {
        return Mathf.RoundToInt(ObtenirPrix(actifId, mois, donnees) * 100f);
    }

    private static List<float> ChargerCourbeMarche(string actifId)
    {
        if (!CheminsResources.TryGetValue(actifId, out string cheminResources))
        {
            return new List<float>();
        }

        TextAsset fichier = Resources.Load<TextAsset>(cheminResources);
        if (fichier == null)
        {
            Debug.LogWarning("[Bourse] Données absentes pour " + actifId + ".");
            return new List<float>();
        }

        try
        {
            List<PointMarche> points =
                JsonConvert.DeserializeObject<List<PointMarche>>(fichier.text);
            List<float> prix = new List<float>();
            if (points != null)
            {
                foreach (PointMarche point in points)
                {
                    if (point != null && point.Close > 0f)
                    {
                        prix.Add(point.Close);
                    }
                }
            }

            return prix;
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "[Bourse] Lecture impossible pour " + actifId + " : " +
                exception.Message);
            return new List<float>();
        }
    }

    private static List<float> ChargerCourbeLivretA()
    {
        TextAsset fichier =
            Resources.Load<TextAsset>("livret_a_simulation_40ans_simplifie");
        if (fichier == null)
        {
            return new List<float>();
        }

        try
        {
            Dictionary<string, List<PointLivret>> donnees =
                JsonConvert.DeserializeObject<Dictionary<string, List<PointLivret>>>(
                    fichier.text);
            if (donnees == null ||
                !donnees.TryGetValue("livret_a_simulation", out List<PointLivret> points) ||
                points == null ||
                points.Count == 0)
            {
                return new List<float>();
            }

            List<float> prix = new List<float> { 100f };
            float valeur = 100f;
            const int decalageJuillet2026 = 6;

            for (int mois = 0; mois < 480; mois++)
            {
                int index = Mathf.Min(decalageJuillet2026 + mois, points.Count - 1);
                valeur *= 1f + (points[index].Rendement_mensuel_pct / 100f);
                prix.Add(valeur);
            }

            return prix;
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "[Bourse] Lecture impossible pour le Livret A : " +
                exception.Message);
            return new List<float>();
        }
    }

    private static int LimiterEnEntier(long valeur)
    {
        if (valeur > int.MaxValue)
        {
            return int.MaxValue;
        }

        return valeur < 0 ? 0 : (int)valeur;
    }
}
