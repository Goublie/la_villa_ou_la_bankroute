using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Fournit les donnees reglementaires utilisees par le Livret A.
/// </summary>
public static class ServiceLivretA
{
    private const int DecalageJuillet2026 = 6;
    private const float TauxSecours = 0.0175f;

    private static List<PrevisionLivretA> previsions;
    private static bool chargementEffectue;

    /// <summary>
    /// Retourne le taux annuel applicable au mois de jeu indique.
    /// </summary>
    /// <param name="mois">Index absolu depuis juillet 2026.</param>
    /// <param name="tauxSecours">
    /// Taux utilise si la ressource JSON est absente ou invalide.
    /// </param>
    public static float ObtenirTauxAnnuel(
        int mois,
        float tauxSecours = TauxSecours)
    {
        ChargerPrevisions();
        if (previsions == null || previsions.Count == 0)
        {
            return Mathf.Max(0f, tauxSecours);
        }

        int index = Mathf.Clamp(
            DecalageJuillet2026 + Mathf.Max(0, mois),
            0,
            previsions.Count - 1);
        return Mathf.Max(0f, previsions[index].Taux_annuel_pct / 100f);
    }

    /// <summary>
    /// Convertit l'index absolu en mois civil, le jeu debutant en juillet.
    /// </summary>
    public static Mois ObtenirMoisCalendrier(int mois)
    {
        int index = ((int)Mois.Juillet + Mathf.Max(0, mois)) % 12;
        return (Mois)index;
    }

    private static void ChargerPrevisions()
    {
        if (chargementEffectue)
        {
            return;
        }

        chargementEffectue = true;
        TextAsset ressource = Resources.Load<TextAsset>(
            "livret_a_simulation_40ans_simplifie");
        if (ressource == null)
        {
            return;
        }

        try
        {
            Dictionary<string, List<PrevisionLivretA>> donnees =
                JsonConvert.DeserializeObject<
                    Dictionary<string, List<PrevisionLivretA>>>(
                    ressource.text);
            if (donnees != null)
            {
                donnees.TryGetValue(
                    "livret_a_simulation",
                    out previsions);
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "[Banque] Lecture des taux du Livret A impossible : " +
                exception.Message);
        }
    }
}
