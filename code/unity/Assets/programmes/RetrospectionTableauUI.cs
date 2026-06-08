using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère l'affichage du tableau comparatif dans la fenêtre de rétrospection.
/// Remplit le tableau avec les résultats du joueur par rapport à la stratégie Fourmi.
/// </summary>
public class RetrospectionTableauUI : MonoBehaviour
{
    [Header("Données de jeu")]
    public GameData gameData;

    [Header("Composant Tableau")]
    public TableauScroll tableauComparatif;

    private void Start()
    {
        if (tableauComparatif == null)
        {
            tableauComparatif = GetComponentInChildren<TableauScroll>(true);
        }

        if (gameData == null)
        {
            gameData = Resources.Load<GameData>("GameData");
        }

        ActualiserTableau();
    }

    /// <summary>
    /// Remplit le tableau comparatif avec les soldes de fin de période.
    /// </summary>
    public void ActualiserTableau()
    {
        if (gameData == null || tableauComparatif == null) return;

        // Récupération des historiques
        List<Optimizer.SimulationResult> reel = Optimizer.ObtenirHistoriqueReel(gameData);
        List<Optimizer.SimulationResult> simule = Optimizer.SimulerFourmi(gameData);

        if (reel.Count == 0 || simule.Count == 0) return;

        // Vider le tableau avant de le remplir
        tableauComparatif.Vider();

        // Extraction des résultats du dernier mois
        Optimizer.SimulationResult bilanReel = reel[reel.Count - 1];
        Optimizer.SimulationResult bilanSimule = simule[simule.Count - 1];

        // Calcul de la différence (manque à gagner)
        argent difference = bilanSimule.patrimoineTotal - bilanReel.patrimoineTotal;

        // En-tête des colonnes : Stratégie | Patrimoine Final | Écart
        tableauComparatif.Add("STRATÉGIE", "PATRIMOINE", "ÉCART / OPTIMAL");

        // Ligne 1 : Résultat optimal (Fourmi)
        tableauComparatif.Add("Optimal (Fourmi)", bilanSimule.patrimoineTotal.ToString(), "Idéal");

        // Ligne 2 : Résultat réel du joueur
        string ecartStr = difference.centimes > 0 ? "-" + difference.ToString() : "0.00 €";
        tableauComparatif.Add("Joueur (Réel)", bilanReel.patrimoineTotal.ToString(), ecartStr);

        Debug.Log("[What-If] Tableau comparatif mis à jour.");
    }
}
