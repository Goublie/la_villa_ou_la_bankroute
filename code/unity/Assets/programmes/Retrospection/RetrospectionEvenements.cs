using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Affiche une explication du bilan et de la derniere decision What If.
/// </summary>
public class RetrospectionEvenements : MonoBehaviour
{
    [Header("Donnees de jeu")]
    public GameData gameData;

    [Header("Composant Texte Conseils")]
    public TextMeshProUGUI texteConseils;

    private void Start()
    {
        if (texteConseils == null)
        {
            texteConseils =
                GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (gameData == null)
        {
            gameData = Resources.Load<GameData>("GameData");
        }

        GenererConseils();
    }

    public void GenererConseils()
    {
        if (gameData == null || texteConseils == null)
        {
            return;
        }

        List<Optimizer.SimulationResult> reel =
            Optimizer.ObtenirHistoriqueReel(gameData);
        List<Optimizer.SimulationResult> alternatif =
            Optimizer.ObtenirHistoriqueWhatIf(gameData);

        if (reel.Count == 0 || alternatif.Count == 0)
        {
            texteConseils.text =
                "<color=#00e676><b>Bilan What If</b></color>\n\n" +
                "Le moteur est actif, mais aucun mois complet n'a encore " +
                "ete cloture. Revenez apres le prochain passage mensuel.";
            return;
        }

        Optimizer.SimulationResult bilanReel =
            reel[reel.Count - 1];
        Optimizer.SimulationResult bilanAlternatif =
            alternatif[alternatif.Count - 1];

        int ecart = DifferenceSaturee(
            bilanAlternatif.patrimoineTotal.centimes,
            bilanReel.patrimoineTotal.centimes);

        string texte =
            "<color=#00e676><b>Bilan d'optimisation What If</b></color>\n\n" +
            "• Votre patrimoine reel : <b>" +
            bilanReel.patrimoineTotal +
            "</b>\n" +
            "• Scenario alternatif : <b>" +
            bilanAlternatif.patrimoineTotal +
            "</b>\n\n";

        if (ecart > 5000)
        {
            texte +=
                "<color=#ff4d4d><b>Manque a gagner : " +
                new argent(ecart) +
                "</b></color>\n\n" +
                "Le moteur a trouve une repartition boursiere qui aurait " +
                "mieux valorise votre patrimoine, avec les memes revenus " +
                "et depenses externes.";
        }
        else if (ecart < -5000)
        {
            int avantageReel =
                ecart == int.MinValue
                    ? int.MaxValue
                    : -ecart;

            texte +=
                "<color=#00e676><b>Votre strategie reelle a fait mieux de " +
                new argent(avantageReel) +
                ".</b></color>\n\n" +
                "Le scenario What If n'est pas une verite parfaite : il " +
                "compare une strategie calculee avec les informations " +
                "connues a chaque mois.";
        }
        else
        {
            texte +=
                "<color=#00e676><b>Trajectoires tres proches.</b></color>\n\n" +
                "Vos choix reels sont restes proches de la meilleure " +
                "allocation identifiee par le moteur.";
        }

        DecisionWhatIf derniereDecision =
            ObtenirDerniereDecision(gameData.whatIf);

        if (derniereDecision != null)
        {
            texte +=
                "\n\n<b>Derniere decision du moteur :</b>\n" +
                ConstruireResumeDecision(derniereDecision);
        }

        texteConseils.text = texte;
        Debug.Log(
            "[What-If] Conseils personnalises generes.");
    }

    private static DecisionWhatIf ObtenirDerniereDecision(
        DonneesWhatIf donnees)
    {
        if (donnees?.decisions == null)
        {
            return null;
        }

        DecisionWhatIf meilleure = null;

        foreach (DecisionWhatIf decision in donnees.decisions)
        {
            if (decision != null &&
                (meilleure == null ||
                 decision.indexMois > meilleure.indexMois))
            {
                meilleure = decision;
            }
        }

        return meilleure?.Copier();
    }

    private static string ConstruireResumeDecision(
        DecisionWhatIf decision)
    {
        string resume = string.IsNullOrWhiteSpace(
            decision.explication)
            ? "Allocation alternative calculee."
            : decision.explication.Trim();

        List<string> allocations = new List<string>();

        if (decision.allocations != null)
        {
            foreach (
                AllocationActifWhatIf allocation
                in decision.allocations)
            {
                if (allocation == null ||
                    allocation.pourcentage <= 0 ||
                    string.IsNullOrWhiteSpace(
                        allocation.actifId))
                {
                    continue;
                }

                string nom =
                    allocation.actifId ==
                    MoteurRechercheFaisceauWhatIf.LiquiditesId
                        ? "Liquidites"
                        : allocation.actifId;

                allocations.Add(
                    nom +
                    " " +
                    allocation.pourcentage +
                    " %");
            }
        }

        allocations.Sort(System.StringComparer.Ordinal);

        if (allocations.Count > 0)
        {
            resume +=
                "\nAllocation : " +
                string.Join(", ", allocations) +
                ".";
        }

        resume +=
            "\nRendement mensuel attendu : " +
            decision.rendementAttenduPourcent.ToString("0.00") +
            " %, risque estime : " +
            decision.risqueEstime.ToString("0.00") +
            ".";

        return resume;
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