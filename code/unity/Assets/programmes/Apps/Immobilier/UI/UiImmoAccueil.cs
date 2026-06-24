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

        // On affiche les 12 derniers mois jusqu'au mois actuel inclus
        int moisActuel = gameData.nombreMoisPasses;
        int indexDernier = 6 + moisActuel; // Alignement Juillet 2026 (Mois 6 du jeu)
        int indexPremier = Mathf.Max(0, indexDernier - 11);

        for (int i = indexPremier; i <= indexDernier && i < historique.Count; i++)
        {
            var point = historique[i];
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
