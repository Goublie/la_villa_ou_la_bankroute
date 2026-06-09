using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

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

                int prixCentimes = ObtenirPrixCentimes(position.actifId, mois);
                valeurTotale += (long)Math.Round(
                    position.quantite * prixCentimes);
            }
        }

        portefeuille.DefinirValeurMarche(
            LimiterEnEntier(valeurTotale),
            Mathf.Max(0, mois));
    }

    private static int ObtenirPrixCentimes(string actifId, int mois)
    {
        List<float> courbe = ObtenirCourbe(actifId);
        if (courbe.Count == 0)
        {
            return 0;
        }

        int index = Mathf.Clamp(mois, 0, courbe.Count - 1);
        return Mathf.RoundToInt(courbe[index] * 100f);
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
