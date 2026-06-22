using TMPro;
using UnityEngine;

/// <summary>
/// Affiche les ressources globales du joueur dans le HUD.
/// </summary>
public class HUDManager : MonoBehaviour
{
    public GameData gameData;
    public TextMeshProUGUI texteArgent;
    public TextMeshProUGUI texteEnergie;
    public TextMeshProUGUI texteSanteMentale;
    public TextMeshProUGUI texteMois;

    private CompteBanquaire compteCourantAbonne;

    private void Start()
    {
        ActualiserAffichage();
    }

    private void OnEnable()
    {
        AbonnerCompteCourant();
        ActionPlay.OnMoisPasse += ActualiserAffichage;
    }

    private void OnDisable()
    {
        DesabonnerCompteCourant();
        ActionPlay.OnMoisPasse -= ActualiserAffichage;
    }

    /// <summary>
    /// Rafraichit le cash, l'energie et la sante mentale.
    /// </summary>
    public void ActualiserAffichage()
    {
        if (gameData == null || gameData.joueur == null)
        {
            return;
        }

        gameData.joueur.InitialiserSiNecessaire();
        AbonnerCompteCourant();
        if (texteArgent != null && compteCourantAbonne != null)
        {
            texteArgent.text = compteCourantAbonne.GetSolde().ToString();
        }

        if (texteEnergie != null)
        {
            texteEnergie.text = gameData.joueur.energie + "%";
        }

        if (texteSanteMentale != null)
        {
            texteSanteMentale.text =
                gameData.joueur.santeMentale + "/100";
        }

        if (texteMois != null)
        {
            int moisDepart = 7;
            int anneeDepart = 2026;

            int totalMois = moisDepart + gameData.nombreMoisPasses - 1;

            int moisCourant = (totalMois % 12) + 1;
            int anneeCourante = anneeDepart + (totalMois / 12);

            string strMois = moisCourant.ToString("D2");

            texteMois.text = strMois + "/" + anneeCourante;
        }
    }

    private void AbonnerCompteCourant()
    {
        if (gameData == null || gameData.joueur == null)
        {
            return;
        }

        CompteBanquaire compte =
            new ServiceBanque(gameData.joueur).ObtenirCompteCourant();
        if (ReferenceEquals(compte, compteCourantAbonne))
        {
            return;
        }

        DesabonnerCompteCourant();
        compteCourantAbonne = compte;
        compteCourantAbonne.OnSoldeModifie += ActualiserAffichage;
    }

    private void DesabonnerCompteCourant()
    {
        if (compteCourantAbonne != null)
        {
            compteCourantAbonne.OnSoldeModifie -= ActualiserAffichage;
            compteCourantAbonne = null;
        }
    }
}
