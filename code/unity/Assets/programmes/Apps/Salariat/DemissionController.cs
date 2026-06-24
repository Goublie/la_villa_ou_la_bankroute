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
    public GameObject panelPosteActuel;        // Reference to Panel_Poste8actuel
    public GameObject panelActionsRapides;     // Reference to Panel_Actions_Rapides
    public GameObject panelPerformanceEmploye; // Reference to Panel_PerformanceEmploye (kept in sync with the dashboard)
    public GameObject panelRelationnel;        // Reference to Panel_Relationnel (kept in sync with the dashboard)
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

    public void OnDemissionnerClicked()
    {
        // Open the confirmation panel.
        gameObject.SetActive(true);

        // Hide the dashboard so only this popup is visible (matches the networking flow).
        if (panelPosteActuel != null) panelPosteActuel.SetActive(false);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(false);
        if (panelPerformanceEmploye != null) panelPerformanceEmploye.SetActive(false);
        if (panelRelationnel != null) panelRelationnel.SetActive(false);
    }

    private void OnNonClicked()
    {
        // Cancel: close the panel and restore the dashboard.
        gameObject.SetActive(false);

        if (panelPosteActuel != null) panelPosteActuel.SetActive(true);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(true);
        if (panelPerformanceEmploye != null) panelPerformanceEmploye.SetActive(true);
        if (panelRelationnel != null) panelRelationnel.SetActive(true);
    }

    public void OnOuiClicked()
    {
        // Confirm resignation: close the panel and clear the current job.
        gameObject.SetActive(false);

        // Restore the dashboard (matches the networking flow).
        if (panelPosteActuel != null) panelPosteActuel.SetActive(true);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(true);
        if (panelPerformanceEmploye != null) panelPerformanceEmploye.SetActive(true);
        if (panelRelationnel != null) panelRelationnel.SetActive(true);

        if (entrepriseText != null) entrepriseText.text = "Entreprise : Aucune";

        // ◄ FIX FORMATTAGE
        if (salaireText != null) salaireText.text = "Salaire brut : 0 € / an";

        if (heuresText != null) heuresText.text = "Heures : 0 heures / semaine";

        GameData donneesJeu = ResoudreGameData();
        if (donneesJeu != null && donneesJeu.joueur != null)
        {
            donneesJeu.joueur.InitialiserSiNecessaire();
            new ServiceSalariat(
                donneesJeu.joueur.salariat,
                donneesJeu.joueur)
                .Demissionner();
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

    private GameData ResoudreGameData()
    {
        if (gameData != null)
        {
            return gameData;
        }

        ActionPlay actionPlay = Object.FindFirstObjectByType<ActionPlay>();
        if (actionPlay != null)
        {
            gameData = actionPlay.gameData;
        }

        return gameData;
    }
}