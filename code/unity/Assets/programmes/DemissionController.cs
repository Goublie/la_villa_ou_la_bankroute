using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the resignation confirmation flow. Lives on 'Panel_Confirmation_Demission'.
/// The main "Démissioner" button opens the confirmation panel; "Oui" resets the current
/// job on the dashboard (and zeroes the salary in GameData) while "Non" simply closes it.
/// </summary>
public class DemissionController : MonoBehaviour
{
    [Header("Data")]
    public GameData gameData;

    [Header("References")]
    public Button boutonDemissionnerMain; // The existing resign button in Panel_Actions_Rapides
    public GameObject panelPosteActuel;   // Reference to Panel_Poste8actuel
    public Button boutonOui;
    public Button boutonNon;

    [Header("Job satisfaction")]
    public JobSatisfactionController satisfactionController;

    // Dashboard TMP fields inside 'panelPosteActuel'.
    private TMP_Text entrepriseText;
    private TMP_Text salaireText;
    private TMP_Text heuresText;

    private void Start()
    {
        // --- Locate the dashboard text fields ---
        if (panelPosteActuel != null)
        {
            entrepriseText = FindText(panelPosteActuel.transform, "Entreprise");
            salaireText = FindText(panelPosteActuel.transform, "Salaire_brut");
            heuresText = FindText(panelPosteActuel.transform, "Heures");
        }

        // --- Bind buttons ---
        if (boutonDemissionnerMain != null) boutonDemissionnerMain.onClick.AddListener(OnDemissionnerClicked);
        if (boutonNon != null) boutonNon.onClick.AddListener(OnNonClicked);
        if (boutonOui != null) boutonOui.onClick.AddListener(OnOuiClicked);
    }

    private void OnDemissionnerClicked()
    {
        // Open the confirmation panel.
        gameObject.SetActive(true);
    }

    private void OnNonClicked()
    {
        // Cancel: just close the panel.
        gameObject.SetActive(false);
    }

    private void OnOuiClicked()
    {
        // Confirm resignation: close the panel and clear the current job.
        gameObject.SetActive(false);

        if (entrepriseText != null) entrepriseText.text = "Entreprise : Aucune";
        if (salaireText != null) salaireText.text = "Salaire brut : 0€ / an";
        if (heuresText != null) heuresText.text = "Heures : 0 heures / semaine";

        // Zero out the salary in the bank/game data.
        if (gameData != null)
        {
            gameData.salaire = new argent(0);
        }

        // Reset the job-satisfaction bar to its empty state.
        if (satisfactionController != null)
            satisfactionController.ResetSatisfaction();
    }

    private TMP_Text FindText(Transform root, string childName)
    {
        Transform t = root.Find(childName);
        return t != null ? t.GetComponent<TMP_Text>() : null;
    }
}
