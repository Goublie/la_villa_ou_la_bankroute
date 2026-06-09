using System.Collections.Generic;
using UnityEngine;
using XCharts.Runtime; // Namespace requis pour manipuler XCharts

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

        // 2. S'assurer qu'il y a exactement 1 série pour le joueur
        while (lineChart.series.Count < 1)
        {
            lineChart.AddSerie<Line>();
        }
        while (lineChart.series.Count > 1)
        {
            lineChart.series.RemoveAt(lineChart.series.Count - 1);
        }

        // Configuration de la série unique
        var serieJoueur = lineChart.series[0];
        serieJoueur.serieName = "Joueur (Réel)";
        serieJoueur.show = true;

        Debug.Log($"[What-If] Début du tracé du graphique. Nombre de points : {gameData.historiqueSnapshots.Count}");

        // 3. Remplissage des données à partir de l'historique des snapshots
        List<PointPatrimoine> historiqueReel = gameData.ObtenirHistoriquePatrimoineReel();
        for (int i = 0; i < historiqueReel.Count; i++)
        {
            PointPatrimoine pt = historiqueReel[i];
            string labelMois = pt.moisCalendrier.ToString();
            
            // Ajout de l'étiquette sur l'axe X
            lineChart.AddXAxisData(labelMois);

            // Conversion du patrimoine total en euros
            float totalEuros = pt.patrimoineTotal.centimes / 100f;

            // Ajout de la donnée dans la série
            lineChart.AddData(0, totalEuros);

            Debug.Log($"[What-If] Index {i} - Mois : {labelMois} | Total : {totalEuros} €");
        }

        // 4. Force le rafraîchissement
        lineChart.SetAllDirty();
        Debug.Log("[What-If] Graphique mis à jour avec le patrimoine du joueur.");
    }
}
