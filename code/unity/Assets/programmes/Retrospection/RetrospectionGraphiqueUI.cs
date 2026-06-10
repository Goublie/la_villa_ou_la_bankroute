using System.Collections.Generic;
using UnityEngine;
using XCharts.Runtime;

/// <summary>
/// Gère le rendu graphique du patrimoine réel du joueur pour la rétrospection.
/// Se contente de sommer le solde du compte courant et du compte épargne pour chaque snapshot.
/// </summary>
public class RetrospectionGraphiqueUI : MonoBehaviour
{
    [Header("Données de jeu")]
    public GameData gameData;

    [Header("Composant XCharts")]
    public LineChart lineChart; // Le composant de graphique linéaire de XCharts

    private void Start()
    {
        // Si le composant n'est pas assigné dans l'inspecteur, on tente de le récupérer sur le GameObject
        if (lineChart == null)
        {
            lineChart = GetComponent<LineChart>();
        }

        if (gameData == null)
        {
            gameData = Resources.Load<GameData>("GameData");
        }

        ActualiserGraphique();
    }

    /// <summary>
    /// Génère le graphique et peuple la série avec le patrimoine réel du joueur.
    /// </summary>
    public void ActualiserGraphique()
    {
        if (gameData == null || lineChart == null)
        {
            Debug.LogError("[What-If] Impossible d'actualiser le graphique : GameData ou LineChart manquant.");
            return;
        }

        // 1. Nettoyage des anciennes données
        lineChart.ClearData();

        // 2. S'assurer qu'il y a exactement 2 séries (Joueur Réel et Optimal Fourmi)
        while (lineChart.series.Count < 2)
        {
            lineChart.AddSerie<Line>();
        }
        while (lineChart.series.Count > 2)
        {
            lineChart.series.RemoveAt(lineChart.series.Count - 1);
        }

        // Configuration de la série 0 : Joueur (Réel)
        var serieJoueur = lineChart.series[0];
        serieJoueur.serieName = "Joueur (Réel)";
        serieJoueur.show = true;

        // Configuration de la série 1 : Optimal
        var serieSimule = lineChart.series[1];
        serieSimule.serieName = "Optimal";
        serieSimule.show = true;

        // Configuration programmatique de l'axe Y pour afficher les euros
        var yAxis = lineChart.GetChartComponent<YAxis>();
        if (yAxis != null && yAxis.axisLabel != null)
        {
            yAxis.axisLabel.formatter = "{value} €";
        }

        Debug.Log("[What-If] Début du tracé du graphique.");

        // 3. Remplissage des données à partir de l'historique des snapshots et de la simulation
        List<PointPatrimoine> historiqueReel = gameData.ObtenirHistoriquePatrimoineReel();
        List<Optimizer.SimulationResult> historiqueSimule = Optimizer.Simuler(gameData);

        int maxPoints = Mathf.Min(historiqueReel.Count, historiqueSimule.Count);

        for (int i = 0; i < maxPoints; i++)
        {
            PointPatrimoine ptReel = historiqueReel[i];
            Optimizer.SimulationResult ptSimule = historiqueSimule[i];
            string labelMois = ptReel.moisCalendrier.ToString();
            
            // Ajout de l'étiquette sur l'axe X (une seule fois par index de mois)
            lineChart.AddXAxisData(labelMois);

            // Conversion des patrimoines en euros (double)
            double eurosReel = ptReel.patrimoineTotal.ToDouble();
            double eurosSimule = ptSimule.patrimoineTotal.ToDouble();

            // Ajout des données dans les séries respectives
            lineChart.AddData(0, eurosReel);
            lineChart.AddData(1, eurosSimule);

            Debug.Log($"[What-If] Index {i} - Mois : {labelMois} | Réel : {eurosReel} € | Optimisé : {eurosSimule} €");
        }

        // 4. Force le rafraîchissement
        lineChart.SetAllDirty();
        Debug.Log("[What-If] Graphique mis à jour avec le patrimoine du joueur.");
    }
}
