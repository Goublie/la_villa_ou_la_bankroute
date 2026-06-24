using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CreditsUI : MonoBehaviour
{
    [Header("Données")]
    [SerializeField] private GameData G;

    [Header("Composants")]
    [SerializeField] private TableauScrollSelectable tableauCredits;
    [SerializeField] private TMP_Text texteDetails;

    private Dictionary<LigneSelectable, DonneesPret> mapLignesPrets = new Dictionary<LigneSelectable, DonneesPret>();
    private DonneesPret pretSelectionne;

    private void Start()
    {
        if (texteDetails != null)
        {
            texteDetails.text = "Sélectionnez un crédit pour voir les détails.";
        }
        
        if (tableauCredits != null)
        {
            tableauCredits.OnSelectionChanged.AddListener(OnPretSelectionne);
        }
    }

    private void OnEnable()
    {
        RafraichirListe();
    }

    public void RafraichirListe()
    {
        if (tableauCredits != null)
        {
            foreach (var ligne in tableauCredits.tableau)
            {
                ligne.Vider();
            }
        }
        
        mapLignesPrets.Clear();
        pretSelectionne = null;
        
        if (texteDetails != null) texteDetails.text = "Sélectionnez un crédit pour voir les détails.";

        if (G == null || G.joueur == null || G.joueur.pretsImmobiliers == null) return;

        foreach (var pret in G.joueur.pretsImmobiliers)
        {
            if (pret.moisRestants <= 0) continue;

            // Colonnes: Montant Initial | Mensualité | Mois restants | Taux
            var ligne = tableauCredits.AjouterEtRetournerLigne(
                pret.montantEmprunte.ToString(),
                pret.mensualite.ToString(),
                $"{pret.moisRestants} mois",
                $"{pret.tauxAnnuel:F2} %"
            ) as LigneSelectable;

            if (ligne != null)
            {
                mapLignesPrets.Add(ligne, pret);
            }
        }
    }

    private void OnPretSelectionne(LigneSelectable ligne)
    {
        if (mapLignesPrets.TryGetValue(ligne, out DonneesPret pret))
        {
            pretSelectionne = pret;

            if (texteDetails != null)
            {
                texteDetails.text = $"Détails du prêt :\nCapital restant dû : {pret.capitalRestantDu.ToString()}\nTaux : {pret.tauxAnnuel:F2}%\nMensualité : {pret.mensualite.ToString()}";
            }
        }
    }

    public DonneesPret ObtenirPretSelectionne()
    {
        return pretSelectionne;
    }
}
