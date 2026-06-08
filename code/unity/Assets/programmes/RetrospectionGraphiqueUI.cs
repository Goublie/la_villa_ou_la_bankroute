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

        Debug.Log($"[What-If] Début du tracé du graphique. Nombre de snapshots : {gameData.historiqueSnapshots.Count}");

        // 3. Remplissage des données à partir de l'historique des snapshots
        for (int i = 0; i < gameData.historiqueSnapshots.Count; i++)
        {
            SnapshotEtatJeu snapshot = gameData.historiqueSnapshots[i];
            string labelMois = snapshot.moisCalendrier.ToString();
            
            // Ajout de l'étiquette sur l'axe X
            lineChart.AddXAxisData(labelMois);

            // Récupération des soldes
            long courantCents = 0;
            if (snapshot.joueur != null && snapshot.joueur.comptes != null && snapshot.joueur.comptes.ContainsKey("courant"))
            {
                courantCents = snapshot.joueur.comptes["courant"].GetSolde().centimes;
            }

            long epargneCents = 0;
            if (snapshot.joueur != null && snapshot.joueur.comptes != null && snapshot.joueur.comptes.ContainsKey("epargne"))
            {
                epargneCents = snapshot.joueur.comptes["epargne"].GetSolde().centimes;
            }

            // Somme du courant et de l'épargne en euros
            float totalEuros = (courantCents + epargneCents) / 100f;

            // Ajout de la donnée dans la série
            lineChart.AddData(0, totalEuros);

            Debug.Log($"[What-If] Index {i} - Mois : {labelMois} | Courant : {courantCents / 100f} € | Épargne : {epargneCents / 100f} € | Total : {totalEuros} €");
        }

        // 4. Force le rafraîchissement
        lineChart.SetAllDirty();
        Debug.Log("[What-If] Graphique mis à jour avec le patrimoine du joueur.");
    }
}
