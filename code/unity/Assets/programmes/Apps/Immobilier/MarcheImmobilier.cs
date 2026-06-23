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
    private static Dictionary<string, List<PointImmo>> _courbesVilles;

    /// <summary>
    /// Récupère le prix au m2 (s'auto-initialise au premier appel, comme la Bourse).
    /// </summary>
    public static float ObtenirPrixM2(string villeId, int nombreMoisPasses)
    {
        if (_courbesVilles == null)
        {
            ChargerDonnees();
        }

        string cle = villeId?.ToLower();
        if (_courbesVilles == null || !_courbesVilles.TryGetValue(cle, out List<PointImmo> points) || points == null)
        {
            return 0f;
        }

        // Alignement Juillet 2026 (Mois 0 du jeu)
        int moisTotalCumules = 6 + nombreMoisPasses; 
        int anneeCible = 2026 + (moisTotalCumules / 12);
        int moisCible = (moisTotalCumules % 12) + 1;

        PointImmo pointTrouve = points.Find(p => p.Annee == anneeCible && p.Mois == moisCible);
        if (pointTrouve != null)
        {
            return pointTrouve.Prix_m2;
        }

        if (points.Count > 0) return points[points.Count - 1].Prix_m2;
        return 0f;
    }

    public static List<PointImmo> ObtenirHistoriqueComplet(string villeId)
    {
        if (_courbesVilles == null) 
        {
            ChargerDonnees();
        }
        
        string cle = villeId?.ToLower();
        if (_courbesVilles != null && _courbesVilles.TryGetValue(cle, out List<PointImmo> points))
        {
            return points;
        }
        return new List<PointImmo>();
    }

    private static void ChargerDonnees()
    {
        TextAsset fichierJson = Resources.Load<TextAsset>(CheminResource);
        if (fichierJson == null)
        {
            Debug.LogError($"[MarcheImmobilier] Fichier introuvable dans Resources : {CheminResource}");
            _courbesVilles = new Dictionary<string, List<PointImmo>>();
            return;
        }

        try
        {
            _courbesVilles = JsonConvert.DeserializeObject<Dictionary<string, List<PointImmo>>>(fichierJson.text);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MarcheImmobilier] Erreur parsing : {ex.Message}");
            _courbesVilles = new Dictionary<string, List<PointImmo>>();
        }
    }
}