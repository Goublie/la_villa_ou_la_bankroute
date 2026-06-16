using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pilote le flux de confirmation "travailler plus / moins" du prefab Salariat.
/// </summary>
/// <remarks>
/// Le composant ne calcule pas lui-meme les effets de bord metier : il delegue
/// heures, salaire et satisfaction a <see cref="EmployeePerformanceController"/>,
/// qui appelle <see cref="ServiceSalariat"/>.
/// </remarks>
public class TravaillerPlusController : MonoBehaviour
{
    [Header("References")]
    public Button boutonTravaillerPlusMain;
    public Button boutonRetour;
    public Button boutonOui;
    public GameObject boutonOuiMoins;
    public TextMeshProUGUI texteMoinsHeures;
    public TextMeshProUGUI texteHeuresTravail;
    public TextMeshProUGUI texteSalaireTaskbar;
    public GameObject panelPosteActuel;
    public GameObject panelActionsRapides;
    public GameObject panelPerformanceEmploye;
    public GameObject panelRelationnel;

    [Header("Overtime references")]
    public JobSatisfactionController satisfactionController;
    public InterviewPanelController interviewController;
    public EmployeePerformanceController performanceController;

    private void Start()
    {
        if (boutonTravaillerPlusMain != null)
        {
            boutonTravaillerPlusMain.onClick.AddListener(OnTravaillerPlusClicked);
        }

        if (boutonRetour != null)
        {
            boutonRetour.onClick.AddListener(OnRetourClicked);
        }

        if (boutonOui != null)
        {
            boutonOui.onClick.AddListener(OnOuiClicked);
        }
    }

    public void OnTravaillerPlusClicked()
    {
        gameObject.SetActive(true);
        AfficherTableauBord(false);

        if (boutonOuiMoins != null)
        {
            boutonOuiMoins.SetActive(true);
        }

        if (texteMoinsHeures != null)
        {
            texteMoinsHeures.gameObject.SetActive(true);
        }
    }

    private void OnRetourClicked()
    {
        FermerEtRestaurerTableauBord();
    }

    /// <summary>
    /// Applique +5 heures par semaine, +800 EUR mensuels et -15 satisfaction.
    /// </summary>
    private void OnOuiClicked()
    {
        if (performanceController == null)
        {
            return;
        }

        ResultatOperation resultat = performanceController.ModifierTempsTravail(
            5,
            ServiceSalariat.VariationSalaireTempsTravailCentimes,
            -15);

        if (resultat.Succes)
        {
            ActualiserLabels();
        }

        FermerEtRestaurerTableauBord();
    }

    /// <summary>
    /// Applique -5 heures par semaine, -800 EUR mensuels et +15 satisfaction.
    /// </summary>
    public void OnOuiMoinsClicked()
    {
        if (performanceController == null ||
            performanceController.currentJobHours <= 35)
        {
            return;
        }

        ResultatOperation resultat = performanceController.ModifierTempsTravail(
            -5,
            -ServiceSalariat.VariationSalaireTempsTravailCentimes,
            15);

        if (resultat.Succes)
        {
            ActualiserLabels();
        }

        FermerEtRestaurerTableauBord();
    }

    private void ActualiserLabels()
    {
        if (performanceController == null ||
            performanceController.gameData == null ||
            performanceController.gameData.joueur == null)
        {
            return;
        }

        argent salaireMensuel = performanceController.gameData.joueur.salaire;
        if (texteHeuresTravail != null)
        {
            texteHeuresTravail.text =
                "Heures : " + performanceController.currentJobHours +
                " heures / semaine";
        }

        if (performanceController.texteSalaireAnnuelBrut != null)
        {
            performanceController.texteSalaireAnnuelBrut.text =
                "Salaire : " + (salaireMensuel * 12f).ToString("N0") +
                " EUR / an";
        }

        if (texteSalaireTaskbar != null)
        {
            texteSalaireTaskbar.text = salaireMensuel.ToString();
        }

        if (satisfactionController != null)
        {
            satisfactionController.RefreshFromData();
        }
    }

    private void FermerEtRestaurerTableauBord()
    {
        gameObject.SetActive(false);
        AfficherTableauBord(true);
    }

    private void AfficherTableauBord(bool visible)
    {
        if (panelPosteActuel != null)
        {
            panelPosteActuel.SetActive(visible);
        }

        if (panelActionsRapides != null)
        {
            panelActionsRapides.SetActive(visible);
        }

        if (panelPerformanceEmploye != null)
        {
            panelPerformanceEmploye.SetActive(visible);
        }

        if (panelRelationnel != null)
        {
            panelRelationnel.SetActive(visible);
        }
    }
}
