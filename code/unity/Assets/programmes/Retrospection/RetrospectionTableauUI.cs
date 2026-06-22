using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Affiche le dernier bilan comparé et le journal des ordres What If.
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

        AjouterBilanComparatif(reel, alternatif);
        AjouterJournalOrdres();

        Debug.Log(
            "[What-If] Tableau comparatif et journal des ordres mis à jour.");
    }

    private void AjouterBilanComparatif(
        List<Optimizer.SimulationResult> reel,
        List<Optimizer.SimulationResult> alternatif)
    {
        tableauComparatif.Add(
            "STRATÉGIE",
            "PATRIMOINE",
            "ÉCART / WHAT IF");

        if (reel.Count == 0 || alternatif.Count == 0)
        {
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
            "Stratégie What If",
            bilanAlternatif.patrimoineTotal.ToString(),
            "Référence");

        tableauComparatif.Add(
            "Joueur (Réel)",
            bilanReel.patrimoineTotal.ToString(),
            FormaterEcartJoueur(ecart));
    }

    private void AjouterJournalOrdres()
    {
        tableauComparatif.Add(
            "12 DERNIERS MOIS",
            "ORDRE WHAT IF",
            "DÉTAIL");

        List<LigneOperationRetrospectionWhatIf> lignes =
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireLignesOrdres(
                    gameData.whatIf,
                    gameData.nombreMoisPasses);

        if (lignes.Count == 0)
        {
            tableauComparatif.Add(
                "-",
                "Aucune opération",
                "Le modèle n'a encore exécuté aucun achat ou vente.");
            return;
        }

        foreach (
            LigneOperationRetrospectionWhatIf ligne
            in lignes)
        {
            tableauComparatif.Add(
                ligne.mois,
                ligne.operation,
                ligne.detail);
        }
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