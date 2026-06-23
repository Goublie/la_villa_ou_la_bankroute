using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class MarcheImmobilier
{
    [Serializable]
    public class PointImmo
    {
        public int Annee;
        public int Mois;
        public string Periode;
        public int Mois_simule;
        public float Prix_m2;
        public string Type;
    }

    private const string CheminResource = "Immobilier/immo_villes_historique_et_simulation_40ans";
    private static Dictionary<string, List<PointImmo>> _courbesVilles = new Dictionary<string, List<PointImmo>>();
    private static bool _estInitialise = false;

    /// <summary>
    /// Force le chargement et le parsing du fichier de simulation immobilière.
    /// </summary>
    public static void InitialiserSiNecessaire()
    {
        if (_estInitialise) return;

        TextAsset fichierJson = Resources.Load<TextAsset>(CheminResource);
        if (fichierJson == null)
        {
            Debug.LogError($"[MarcheImmobilier] Impossible de trouver le fichier de données dans les Resources à l'emplacement : {CheminResource}");
            return;
        }

        try
        {
            var donnees parsing = JsonConvert.DeserializeObject<Dictionary<string, List<PointImmo>>>(fichierJson.text);
            if (parsing != null)
            {
                _courbesVilles = parsing;
                _estInitialise = true;
                Debug.Log($"[MarcheImmobilier] Base de données immobilière chargée avec succès. {_courbesVilles.Count} villes configurées.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MarcheImmobilier] Échec du parsing JSON : {ex.Message}");
        }
    }

    /// <summary>
    /// Récupère le prix brut au m2 pour une ville et un mois de jeu donnés (alignement Juillet 2026).
    /// </summary>
    public static float ObtenirPrixM2(string villeId, int nombreMoisPasses)
    {
        InitialiserSiNecessaire();

        string cle = villeId?.ToLower();
        if (!_courbesVilles.TryGetValue(cle, out List<PointImmo> points) || points == null)
        {
            Debug.LogWarning($"[MarcheImmobilier] Ville inconnue ou non chargée : {villeId}");
            return 0f;
        }

        // Alignement temporel strict (Option 1) : Le mois 0 correspond à Juillet 2026 (index de départ interne)
        // Juillet = 7e mois de l'année.
        int moisTotalCumules = 6 + nombreMoisPasses; 
        int anneeCible = 2026 + (moisTotalCumules / 12);
        int moisCible = (moisTotalCumules % 12) + 1; // Format 1-12 utilisé dans le JSON

        // Recherche du point concordant dans la simulation
        PointImmo pointTrouve = points.Find(p => p.Annee == anneeCible && p.Mois == moisCible);
        if (pointTrouve != null)
        {
            return pointTrouve.Prix_m2;
        }

        // Sécurité anti-débordement : renvoie la dernière valeur connue de la simulation
        if (points.Count > 0)
        {
            return points[points.Count - 1].Prix_m2;
        }

        return 0f;
    }

    /// <summary>
    /// Permet à l'UI d'extraire l'intégralité des points (réels + futurs) pour tracer les graphiques d'historique.
    /// </summary>
    public static List<PointImmo> ObtenirHistoriqueComplet(string villeId)
    {
        InitialiserSiNecessaire();
        string cle = villeId?.ToLower();
        if (_courbesVilles.TryGetValue(cle, out List<PointImmo> points))
        {
            return points;
        }
        return new List<PointImmo>();
    }
}