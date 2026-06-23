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
    [SerializeField] private Button boutonAllerVersModif;
    
    [Header("Navigation")]
    [SerializeField] private ModifCreditUI interfaceModification;
    [SerializeField] private GameObject menuCredits;
    [SerializeField] private GameObject menuModifCredit;

    private Dictionary<LigneSelectable, DonneesPret> mapLignesPrets = new Dictionary<LigneSelectable, DonneesPret>();
    private DonneesPret pretSelectionne;

    private void Start()
    {
        if (boutonAllerVersModif != null)
        {
            boutonAllerVersModif.onClick.AddListener(AllerVersModif);
            boutonAllerVersModif.interactable = false;
        }
        
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
        
        if (boutonAllerVersModif != null) boutonAllerVersModif.interactable = false;
        if (texteDetails != null) texteDetails.text = "Sélectionnez un crédit pour voir les détails.";

        if (G == null || G.joueur == null || G.joueur.pretsImmobiliers == null) return;

        foreach (var pret in G.joueur.pretsImmobiliers)
        {
            if (pret.moisRestants <= 0) continue;

            // Colonnes: Montant Initial | Mensualité | Mois restants
            var ligne = tableauCredits.AjouterEtRetournerLigne(
                pret.montantEmprunte.ToString(),
                pret.mensualite.ToString(),
                $"{pret.moisRestants} mois"
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
            if (boutonAllerVersModif != null)
            {
                boutonAllerVersModif.interactable = true;
            }
        }
    }

    private void AllerVersModif()
    {
        if (pretSelectionne != null && interfaceModification != null)
        {
            // Transition de menu
            if (menuCredits != null) menuCredits.SetActive(false);
            if (menuModifCredit != null) menuModifCredit.SetActive(true);

            interfaceModification.ChargerPret(pretSelectionne);
        }
    }
}
