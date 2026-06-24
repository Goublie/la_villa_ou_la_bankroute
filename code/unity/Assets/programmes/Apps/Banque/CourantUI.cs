using TMPro;
using UnityEngine;

/// <summary>
/// Affiche le solde et l'historique du compte courant.
/// </summary>
public class CourantUI : MonoBehaviour
{
    public GameData G;

    [SerializeField] private TableauScroll tab;
    [SerializeField] private TextMeshProUGUI solde;

    private CompteBanquaire compteCrnt;

    private void Awake()
    {
        ResoudreCompte();
    }

    private void OnEnable()
    {
        ActionPlay.OnMoisPasse += NouveauMois;
        ResoudreCompte();
        if (compteCrnt != null)
        {
            compteCrnt.OnSoldeModifie += RafraichirJusteLeSolde;
        }

        NouveauMois();
    }

    private void OnDisable()
    {
        ActionPlay.OnMoisPasse -= NouveauMois;
        if (compteCrnt != null)
        {
            compteCrnt.OnSoldeModifie -= RafraichirJusteLeSolde;
        }
    }

    /// <summary>
    /// Reconstruit le tableau des operations du mois.
    /// </summary>
    public void ActualiserTableau()
    {
        if (compteCrnt == null || tab == null)
        {
            return;
        }

        // Force le tableau à 4 colonnes pour gérer le libellé, la date, le crédit et le débit
        tab.nombreColonnes = 4;
        tab.largeursColonnes = new System.Collections.Generic.List<float> { 200f, 150f, 150f, -1f };
        tab.AppliquerStructure();

        Historique historique = compteCrnt.GetHistorique();
        tab.Vider();
        tab.GarantirLignesMinimum();

        for (int i = historique.GetSize() - 1; i >= 0; i--)
        {
            Transaction t = historique.GetHistorique()[i];
            string dateStr = FormaterDate(t.indexMois);
            string creditStr = t.montant.centimes > 0 ? $"<color=green>+{t.montant.ToString()}</color>" : "";
            string debitStr = t.montant.centimes < 0 ? $"<color=red>{t.montant.ToString()}</color>" : "";
            
            tab.Add(t.libelle, dateStr, creditStr, debitStr);
        }
    }

    private string FormaterDate(int indexMois)
    {
        if (indexMois < 0) return "---";
        int moisAbsolu = 6 + indexMois; // Juillet = index 6
        int annee = 2026 + (moisAbsolu / 12);
        int moisCivil = (moisAbsolu % 12) + 1;
        return $"{moisCivil:D2}/{annee}";
    }

    private void RafraichirJusteLeSolde()
    {
        if (solde != null && compteCrnt != null)
        {
            solde.text = compteCrnt.GetSolde().ToString();
        }
    }

    private void NouveauMois()
    {
        ResoudreCompte();
        RafraichirJusteLeSolde();
        ActualiserTableau();
    }

    private void ResoudreCompte()
    {
        if (G == null || G.joueur == null)
        {
            compteCrnt = null;
            return;
        }

        compteCrnt =
            new ServiceBanque(G.joueur).ObtenirCompteCourant();
    }
}
