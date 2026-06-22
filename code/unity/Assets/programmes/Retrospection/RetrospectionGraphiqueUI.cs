using System.Collections.Generic;
using UnityEngine;
using XCharts.Runtime;

/// <summary>
/// Affiche le patrimoine reel et la trajectoire du moteur What If.
/// </summary>
/// <remarks>
/// Le composant ne recalcule aucune regle financiere. Il adapte seulement les
/// points deja enregistres par le cycle mensuel.
/// </remarks>
public class RetrospectionGraphiqueUI : MonoBehaviour
{
    [Header("Donnees de jeu")]
    public GameData gameData;

    [Header("Composant XCharts")]
    public LineChart lineChart;

    private void Start()
    {
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
    /// Peuple le graphique avec les courbes reelle et alternative.
    /// </summary>
    public void ActualiserGraphique()
    {
        if (gameData == null || lineChart == null)
        {
            Debug.LogError(
                "[What-If] Impossible d'actualiser le graphique : " +
                "GameData ou LineChart manquant.");
            return;
        }

        List<Optimizer.SimulationResult> reel =
            Optimizer.ObtenirHistoriqueReel(gameData);
        List<Optimizer.SimulationResult> alternatif =
            Optimizer.ObtenirHistoriqueWhatIf(gameData);

        if (reel.Count == 0 || alternatif.Count == 0)
        {
            return;
        }

        lineChart.ClearData();
        AssurerDeuxSeries();

        lineChart.series[0].serieName = "Joueur (Reel)";
        lineChart.series[0].show = true;
        lineChart.series[1].serieName = "Strategie What If";
        lineChart.series[1].show = true;

        YAxis axeY = lineChart.GetChartComponent<YAxis>();
        if (axeY != null && axeY.axisLabel != null)
        {
            axeY.axisLabel.formatter = "{value} EUR";
        }

        int nombrePoints = Mathf.Min(
            reel.Count,
            alternatif.Count);

        for (int index = 0; index < nombrePoints; index++)
        {
            Optimizer.SimulationResult pointReel = reel[index];
            Optimizer.SimulationResult pointAlternatif =
                alternatif[index];

            lineChart.AddXAxisData(
                pointReel.moisCalendrier.ToString());
            lineChart.AddData(
                0,
                pointReel.patrimoineTotal.centimes / 100d);
            lineChart.AddData(
                1,
                pointAlternatif.patrimoineTotal.centimes / 100d);
        }

        lineChart.SetAllDirty();
        lineChart.RefreshChart();
    }

    private void AssurerDeuxSeries()
    {
        while (lineChart.series.Count < 2)
        {
            lineChart.AddSerie<Line>();
        }

        while (lineChart.series.Count > 2)
        {
            lineChart.series.RemoveAt(
                lineChart.series.Count - 1);
        }
    }
}