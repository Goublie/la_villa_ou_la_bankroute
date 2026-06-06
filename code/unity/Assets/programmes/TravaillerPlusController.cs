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
    public GameObject panelPosteActuel;        // Reference to Panel_Poste8actuel
    public GameObject panelActionsRapides;     // Reference to Panel_Actions_Rapides
    public GameObject panelPerformanceEmploye; // Reference to Panel_PerformanceEmploye (kept in sync with the dashboard)

    [Header("Overtime references")]
    public JobSatisfactionController satisfactionController; // 'Panel_Satisfaction'
    public InterviewPanelController interviewController;     // Provides access to GameData (the bank)

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
    /// Confirms working overtime: +5 hours/week, proportionally higher salary,
    /// updated bank entry and -15 job satisfaction.
    /// </summary>
    private void OnOuiClicked()
    {
        // Hide this confirmation popup and restore the dashboard panels.
        gameObject.SetActive(false);
        if (panelPosteActuel != null) panelPosteActuel.SetActive(true);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(true);
        if (panelPerformanceEmploye != null) panelPerformanceEmploye.SetActive(true);

        if (panelPosteActuel == null) return;

        // --- Locate the dashboard text fields inside 'Panel_Poste8actuel' ---
        TMP_Text heuresText = FindText(panelPosteActuel.transform, "Heures");
        TMP_Text salaireText = FindText(panelPosteActuel.transform, "Salaire_brut");

        // --- Hours: parse the current value, add 5 ---
        int oldHours = ExtractFirstInt(heuresText != null ? heuresText.text : null, 35);
        if (oldHours <= 0) oldHours = 35; // safety against divide-by-zero
        int newHours = oldHours + 5;

        if (heuresText != null)
            heuresText.text = "Heures : " + newHours + " heures / semaine";

        // --- Salary: scale proportionally with the hours increase ---
        if (salaireText != null)
        {
            // Strip thousands separators before parsing the annual amount.
            int oldAnnual = ExtractFirstInt(salaireText.text != null ? salaireText.text.Replace(",", "") : null, 0);
            int newAnnual = (int)(oldAnnual * ((float)newHours / oldHours));

            salaireText.text = "Salaire brut : €" +
                newAnnual.ToString("N0", System.Globalization.CultureInfo.InvariantCulture) + " / an";

            // Update the bank (GameData) with the new monthly salary, in centimes.
            if (interviewController != null && interviewController.gameData != null)
            {
                int monthlySalary = newAnnual / 12;
                // The bank works in centimes, so multiply by 100.
                interviewController.gameData.salaire = new argent(monthlySalary * 100);
            }
        }

        // --- Job satisfaction: working more lowers it ---
        if (satisfactionController != null)
            satisfactionController.ModifySatisfaction(-15);
    }

    /// <summary>
    /// Finds a direct child TMP_Text by name under <paramref name="root"/>.
    /// </summary>
    private TMP_Text FindText(Transform root, string childName)
    {
        Transform t = root.Find(childName);
        return t != null ? t.GetComponent<TMP_Text>() : null;
    }

    /// <summary>
    /// Extracts the first integer found in <paramref name="text"/>, or <paramref name="fallback"/>.
    /// </summary>
    private int ExtractFirstInt(string text, int fallback)
    {
        if (string.IsNullOrEmpty(text)) return fallback;

        System.Text.RegularExpressions.Match match =
            System.Text.RegularExpressions.Regex.Match(text, @"\d+");

        if (match.Success && int.TryParse(match.Value, out int value)) return value;
        return fallback;
    }
}
