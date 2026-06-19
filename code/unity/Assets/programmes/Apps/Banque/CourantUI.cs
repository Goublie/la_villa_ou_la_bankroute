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

        Historique historique = compteCrnt.GetHistorique();
        tab.Vider();
        for (int i = historique.GetSize() - 1; i >= 0; i--)
        {
            tab.Add(historique.GetHistorique()[i]);
        }
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
