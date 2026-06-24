using System;
using UnityEngine;
using XCharts.Runtime;

/// <summary>
/// Gère l'onglet Accueil du module Immobilier.
/// Permet de sélectionner une ville et d'afficher son historique de prix au m² via XCharts.
/// </summary>
public class UiImmoAccueil : MonoBehaviour
{
    [Header("Données partagées")]
    [SerializeField] private GameData gameData;

    [Header("Composants UI")]
    [SerializeField] private LineChart lineChart;

    [Header("Navigation Panels")]
    [SerializeField] private GameObject panneauAccueil;
    [SerializeField] private GameObject panneauMarche;
    [SerializeField] private GameObject panneauMesBiens;

    private ActionPlay actionPlay;
    private Ville villeSelectionnee = Ville.Bordeaux;

    private void Awake()
    {
        ResoudreDependances();
    }

    private void OnEnable()
    {
        ResoudreDependances();
        ActionPlay.OnMoisPasse += ActualiserAffichage;
        ActualiserAffichage();
    }

    private void OnDisable()
    {
        ActionPlay.OnMoisPasse -= ActualiserAffichage;
    }

    private void ResoudreDependances()
    {
        if (actionPlay == null)
        {
            actionPlay = FindFirstObjectByType<ActionPlay>();
            if (actionPlay != null && gameData == null)
            {
                gameData = actionPlay.gameData;
            }
        }
    }

    /// <summary>
    /// Actualise l'onglet Accueil (le graphique).
    /// </summary>
    public void ActualiserAffichage()
    {
        if (gameData == null) return;
        DessinerGraphique(villeSelectionnee);
    }

    /// <summary>
    /// Change la ville sélectionnée et met à jour le graphique.
    /// </summary>
    public void ChangerVilleSelectionnee(Ville ville)
    {
        villeSelectionnee = ville;
        ActualiserAffichage();
        Debug.Log($"[UiImmoAccueil] Ville sélectionnée : {villeSelectionnee}");
    }

    /// <summary>
    /// Sélectionne une ville par son nom sous forme de chaîne (pratique pour l'Inspecteur Unity).
    /// </summary>
    public void SelectionnerVille(string nomVille)
    {
        if (Enum.TryParse(nomVille, true, out Ville ville))
        {
            ChangerVilleSelectionnee(ville);
        }
        else
        {
            Debug.LogWarning($"[UiImmoAccueil] Ville inconnue : {nomVille}");
        }
    }

    // Raccourcis pour l'inspecteur
    public void SelectionnerBordeaux() => ChangerVilleSelectionnee(Ville.Bordeaux);
    public void SelectionnerLyon() => ChangerVilleSelectionnee(Ville.Lyon);
    public void SelectionnerMarseille() => ChangerVilleSelectionnee(Ville.Marseille);
    public void SelectionnerNantes() => ChangerVilleSelectionnee(Ville.Nantes);
    public void SelectionnerParis() => ChangerVilleSelectionnee(Ville.Paris);
    public void SelectionnerToulouse() => ChangerVilleSelectionnee(Ville.Toulouse);

    /// <summary>
    /// Met à jour la courbe XCharts avec les données de prix au m² de la ville sélectionnée.
    /// </summary>
    private void DessinerGraphique(Ville ville)
    {
        if (lineChart == null) return;

        lineChart.RemoveData();

        // Récupérer l'historique complet des prix
        var historique = MarcheImmobilier.ObtenirHistoriqueComplet(ville.ToString().ToLower());
        if (historique == null || historique.Count == 0) return;

        // Mise à jour du titre du graphique avec le nom de la ville
        var titre = lineChart.GetChartComponent<XCharts.Runtime.Title>();
        if (titre != null) titre.text = ville.ToString();

        // Configuration de la série de données
        var serie = lineChart.GetSerie(0);
        if (serie == null)
        {
            lineChart.AddSerie<Line>(ville.ToString());
        }
        else
        {
            serie.serieName = ville.ToString();
        }

        // Calcul de la date actuelle en jeu : le jeu commence en Juillet 2026 (mois 0)
        int moisTotalCumules = 6 + gameData.nombreMoisPasses;
        int anneeActuelle = 2026 + (moisTotalCumules / 12);
        int moisActuelNum = (moisTotalCumules % 12) + 1;

        // On convertit la date actuelle en un entier comparable (AAAAMM)
        int dateLimite = anneeActuelle * 100 + moisActuelNum;

        // On filtre tous les points dont la date est <= la date actuelle du jeu
        // et on garde les 24 derniers pour avoir une courbe riche
        const int NB_POINTS_AFFICHES = 24;
        var pointsFiltres = new System.Collections.Generic.List<MarcheImmobilier.PointImmo>();
        foreach (var point in historique)
        {
            int datePoint = point.Annee * 100 + point.Mois;
            if (datePoint <= dateLimite)
                pointsFiltres.Add(point);
        }

        int debut = Mathf.Max(0, pointsFiltres.Count - NB_POINTS_AFFICHES);
        for (int i = debut; i < pointsFiltres.Count; i++)
        {
            var point = pointsFiltres[i];
            string labelX = $"{point.Mois:D2}/{point.Annee % 100}";
            lineChart.AddXAxisData(labelX);
            lineChart.AddData(0, point.Prix_m2);
        }

        lineChart.SetAllDirty();
        lineChart.RefreshChart();
    }

    // --- NAVIGATION ---
    public void AllerAuMarche()
    {
        if (panneauAccueil != null) panneauAccueil.SetActive(false);
        if (panneauMesBiens != null) panneauMesBiens.SetActive(false);
        if (panneauMarche != null) panneauMarche.SetActive(true);
    }

    public void AllerAMesBiens()
    {
        if (panneauAccueil != null) panneauAccueil.SetActive(false);
        if (panneauMarche != null) panneauMarche.SetActive(false);
        if (panneauMesBiens != null) panneauMesBiens.SetActive(true);
    }
}
