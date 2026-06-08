using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the "Travailler plus" confirmation flow. Lives on
/// 'Panel_Confirmation_TravaillerPlus'. The main "Travailler plus" button in
/// Panel_Actions_Rapides opens this popup; the "Retour" button closes it and
/// restores the dashboard panels.
/// </summary>
public class TravaillerPlusController : MonoBehaviour
{
    [Header("References")]
    public Button boutonTravaillerPlusMain; // The existing "Travailler plus" button inside Panel_Actions_Rapides
    public Button boutonRetour;             // The back button inside this new panel
    public Button boutonOui;                // The "Oui" confirmation button inside this panel
    public GameObject boutonOuiMoins;       // The new "Oui" (work less) button inside this panel
    public TextMeshProUGUI texteMoinsHeures; // The new "work 5h less / week?" question text
    public TextMeshProUGUI texteHeuresTravail; // Working-hours label in the 'Panel_PosteActuel' dashboard
    public TextMeshProUGUI texteSalaireTaskbar; // Monthly salary label in the bottom blue taskbar
    public GameObject panelPosteActuel;        // Reference to Panel_Poste8actuel
    public GameObject panelActionsRapides;     // Reference to Panel_Actions_Rapides
    public GameObject panelPerformanceEmploye; // Reference to Panel_PerformanceEmploye (kept in sync with the dashboard)

    [Header("Overtime references")]
    public JobSatisfactionController satisfactionController; // 'Panel_Satisfaction'
    public InterviewPanelController interviewController;     // Provides access to GameData (the bank)
    public EmployeePerformanceController performanceController; // 'Panel_PerformanceEmploye' controller

    private void Start()
    {
        if (boutonTravaillerPlusMain != null) boutonTravaillerPlusMain.onClick.AddListener(OnTravaillerPlusClicked);
        if (boutonRetour != null) boutonRetour.onClick.AddListener(OnRetourClicked);
        if (boutonOui != null) boutonOui.onClick.AddListener(OnOuiClicked);
    }

    private void OnTravaillerPlusClicked()
    {
        // Open the confirmation popup.
        gameObject.SetActive(true);

        // The "work less" question and button are always visible/active, regardless
        // of the player's current working hours.
        if (boutonOuiMoins != null) boutonOuiMoins.SetActive(true);
        if (texteMoinsHeures != null) texteMoinsHeures.gameObject.SetActive(true);
    }

    private void OnRetourClicked()
    {
        // Close the popup and make sure the dashboard panels are visible again.
        gameObject.SetActive(false);

        if (panelPosteActuel != null) panelPosteActuel.SetActive(true);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(true);
        if (panelPerformanceEmploye != null) panelPerformanceEmploye.SetActive(true);
    }

    /// <summary>
    /// Confirms working overtime: +5 hours/week, +800 €/month gross salary,
    /// refreshed UI labels and -15 job satisfaction.
    /// </summary>
    private void OnOuiClicked()
    {
        if (performanceController == null) return;

        // 1. Increase weekly hours by 5.
        performanceController.UpdateJobHours(performanceController.currentJobHours + 5);

        // 2. Increase the monthly gross salary by 800 € (yearly gross rises by 800 * 12).
        performanceController.gameData.salaire += 800;

        // 3. Refresh all UI text components (standardized formats).
        if (texteHeuresTravail != null)
            texteHeuresTravail.text = "Heures : " + performanceController.currentJobHours + " heures / semaine";
        if (performanceController.texteSalaireAnnuelBrut != null)
            performanceController.texteSalaireAnnuelBrut.text = "Salaire Brut : " + (performanceController.gameData.salaire * 12).ToString("N0") + "€ / an";
        if (texteSalaireTaskbar != null)
            texteSalaireTaskbar.text = performanceController.gameData.salaire.ToString("F2") + " €";

        // 4. Working more lowers job satisfaction (clamped 0-100 inside the controller).
        if (satisfactionController != null) satisfactionController.ModifySatisfaction(-15);

        // 5. Close this popup and restore the dashboard panels.
        gameObject.SetActive(false);
        if (panelPosteActuel != null) panelPosteActuel.SetActive(true);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(true);
        if (panelPerformanceEmploye != null) panelPerformanceEmploye.SetActive(true);
    }

    /// <summary>
    /// Confirms working fewer hours: -5 hours/week, -800 €/month gross salary,
    /// +15 job satisfaction. Only effective while the employee works more than 35h.
    /// </summary>
    public void OnOuiMoinsClicked()
    {
        if (performanceController == null) return;

        // Already at the legal minimum: nothing to do.
        if (performanceController.currentJobHours == 35) return;

        // 1. Reduce weekly hours by 5.
        performanceController.UpdateJobHours(performanceController.currentJobHours - 5);

        // 2. Reduce the monthly gross salary by 800 € (yearly gross drops by 800 * 12).
        performanceController.gameData.salaire -= 800;

        // 3. Refresh all UI text components (standardized formats).
        if (texteHeuresTravail != null)
            texteHeuresTravail.text = "Heures : " + performanceController.currentJobHours + " heures / semaine";
        if (performanceController.texteSalaireAnnuelBrut != null)
            performanceController.texteSalaireAnnuelBrut.text = "Salaire Brut : " + (performanceController.gameData.salaire * 12).ToString("N0") + "€ / an";
        if (texteSalaireTaskbar != null)
            texteSalaireTaskbar.text = performanceController.gameData.salaire.ToString("F2") + " €";

        // 4. Working less improves job satisfaction (clamped 0-100 inside the controller).
        if (satisfactionController != null) satisfactionController.ModifySatisfaction(15);

        // 5. Close this popup and restore the dashboard panels.
        gameObject.SetActive(false);
        if (panelPosteActuel != null) panelPosteActuel.SetActive(true);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(true);
        if (panelPerformanceEmploye != null) panelPerformanceEmploye.SetActive(true);
    }
}
