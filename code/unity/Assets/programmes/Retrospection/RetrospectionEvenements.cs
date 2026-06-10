using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Gère la génération de conseils financiers et l'affichage des événements
/// marquants de la période de rétrospective.
/// </summary>
public class RetrospectionEvenements : MonoBehaviour
{
    [Header("Données de jeu")]
    public GameData gameData;

    [Header("Composant Texte Conseils")]
    public TextMeshProUGUI texteConseils; // Le champ de texte pour afficher le bilan

    private void Start()
    {
        if (texteConseils == null)
        {
            texteConseils = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (gameData == null)
        {
            gameData = Resources.Load<GameData>("GameData");
        }

        GenererConseils();
    }

    /// <summary>
    /// Analyse la période écoulée et affiche des conseils et recommandations personnalisés.
    /// </summary>
    public void GenererConseils()
    {
        if (gameData == null || texteConseils == null) return;

        List<PointPatrimoine> reel = gameData.ObtenirHistoriquePatrimoineReel();
        List<Optimizer.SimulationResult> simule = Optimizer.Simuler(gameData);

        if (reel.Count == 0 || simule.Count == 0) return;

        PointPatrimoine bilanReel = reel[reel.Count - 1];
        Optimizer.SimulationResult bilanSimule = simule[simule.Count - 1];

        // Calcul des intérêts perçus réellement par le joueur
        long interetsReels = 0;
        if (gameData.joueur != null && gameData.joueur.comptes != null && gameData.joueur.comptes.ContainsKey("epargne"))
        {
            foreach (Transaction t in gameData.joueur.comptes["epargne"].GetHistorique().GetHistorique())
            {
                if (t.libelle == "interets")
                {
                    interetsReels += t.montant.centimes;
                }
            }
        }

        // Calcul du manque à gagner sur les intérêts
        long interetsSimules = 0;
        foreach (var r in simule)
        {
            // Dans la simulation Fourmi, l'écart sur le Livret A vient de l'optimisation des dépôts
            // On peut estimer le manque à gagner global sur le patrimoine final
        }

        argent manqueAGagner = bilanSimule.patrimoineTotal - bilanReel.patrimoineTotal;

        // Rédaction du bilan en français expliquant la logique économique
        string bilanText = "<color=#00e676><b>Bilan d'Optimisation What-If</b></color>\n\n";
        bilanText += $"• Votre patrimoine de fin de période : <b>{bilanReel.patrimoineTotal}</b>\n";
        bilanText += $"• Patrimoine atteignable (stratégie Fourmi) : <b>{bilanSimule.patrimoineTotal}</b>\n\n";

        if (manqueAGagner.centimes > 5000) // Manque à gagner supérieur à 50 €
        {
            bilanText += $"<color=#ff4d4d><b>Manque à gagner : {manqueAGagner}</b></color>\n\n";
            bilanText += "<b>Conseil Financier :</b>\n";
            bilanText += "Vous avez laissé dormir trop d'argent sur votre compte courant non rémunéré. ";
            bilanText += "La stratégie optimale démontre qu'en conservant un simple buffer de 500 € pour vos dépenses courantes ";
            bilanText += "et en plaçant systématiquement le reste sur votre Livret A, ";
            bilanText += $"vous auriez accumulé <b>{manqueAGagner}</b> de plus grâce aux intérêts composés et à la rigueur d'épargne.\n\n";
            bilanText += "<i>Pensez à automatiser vos virements vers l'épargne dès le début du mois !</i>";
        }
        else
        {
            bilanText += "<color=#00e676><b>Félicitations !</b></color>\n\n";
            bilanText += "Votre gestion financière est excellente et très proche de l'optimum théorique. ";
            bilanText += "Vous avez su limiter l'argent oisif sur votre compte courant et maximiser l'usage de votre Livret A.";
        }

        texteConseils.text = bilanText;
        Debug.Log("[What-If] Conseils personnalisés générés.");
    }
}
