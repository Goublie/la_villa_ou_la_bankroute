using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Affiche le dernier bilan compare entre le joueur et le scenario What If.
/// </summary>
public class RetrospectionTableauUI : MonoBehaviour
{
    [Header("Donnees de jeu")]
    public GameData gameData;

    [Header("Composant Tableau")]
    public TableauScroll tableauComparatif;

    private void Start()
    {
        if (tableauComparatif == null)
        {
            tableauComparatif =
                GetComponentInChildren<TableauScroll>(true);
        }

        if (gameData == null)
        {
            gameData = Resources.Load<GameData>("GameData");
        }

        ActualiserTableau();
    }

    public void ActualiserTableau()
    {
        if (gameData == null || tableauComparatif == null)
        {
            return;
        }

        List<Optimizer.SimulationResult> reel =
            Optimizer.ObtenirHistoriqueReel(gameData);
        List<Optimizer.SimulationResult> alternatif =
            Optimizer.ObtenirHistoriqueWhatIf(gameData);

        tableauComparatif.Vider();

        if (reel.Count == 0 || alternatif.Count == 0)
        {
            tableauComparatif.Add(
                "STRATEGIE",
                "PATRIMOINE",
                "ECART");
            tableauComparatif.Add(
                "Historique insuffisant",
                "-",
                "-");
            return;
        }

        Optimizer.SimulationResult bilanReel =
            reel[reel.Count - 1];
        Optimizer.SimulationResult bilanAlternatif =
            alternatif[alternatif.Count - 1];

        int ecart = DifferenceSaturee(
            bilanAlternatif.patrimoineTotal.centimes,
            bilanReel.patrimoineTotal.centimes);

        tableauComparatif.Add(
            "STRATEGIE",
            "PATRIMOINE",
            "ECART / WHAT IF");

        tableauComparatif.Add(
            "Strategie What If",
            bilanAlternatif.patrimoineTotal.ToString(),
            "Reference");

        tableauComparatif.Add(
            "Joueur (Reel)",
            bilanReel.patrimoineTotal.ToString(),
            FormaterEcartJoueur(ecart));

        Debug.Log(
            "[What-If] Tableau comparatif mis a jour.");
    }

    private static string FormaterEcartJoueur(int avantageWhatIf)
    {
        if (avantageWhatIf > 0)
        {
            return "-" + new argent(avantageWhatIf);
        }

        if (avantageWhatIf < 0)
        {
            int avantageJoueur =
                avantageWhatIf == int.MinValue
                    ? int.MaxValue
                    : -avantageWhatIf;
            return "+" + new argent(avantageJoueur);
        }

        return "0.00 EUR";
    }

    private static int DifferenceSaturee(
        int gauche,
        int droite)
    {
        long difference = (long)gauche - droite;

        if (difference > int.MaxValue)
        {
            return int.MaxValue;
        }

        return difference < int.MinValue
            ? int.MinValue
            : (int)difference;
    }
}