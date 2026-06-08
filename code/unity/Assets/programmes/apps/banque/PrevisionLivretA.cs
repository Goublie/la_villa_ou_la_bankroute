using System;

// Classe représentant un point de prédiction mensuel pour le taux du Livret A.
// Elle modélise les données d'inflation et de taux d'intérêt issues du fichier JSON de simulation.
[System.Serializable]
public class PrevisionLivretA
{
    public int Mois;                        // L'index cumulé du mois de simulation
    public int Annee;                       // L'année civile (ex: 2026)
    public int MoisCalendrier;              // Le mois de l'année (1 à 12)
    public string Periode;                  // Période sous format texte (ex: "2026-07")
    public float Taux_annuel_pct;           // Le taux d'intérêt annuel en pourcentage (ex: 1.75 pour 1.75%)
    public float Rendement_mensuel_pct;      // Le rendement mensuel composé équivalent
    public float Inflation_simulee_pct;     // L'inflation annuelle simulée pour cette période
}
