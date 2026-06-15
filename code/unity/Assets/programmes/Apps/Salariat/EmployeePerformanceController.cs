using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the "Performance employé" stats card. Lives on
/// 'Panel_PerformanceEmploye'. Exposes three read-only progress bars
/// (Expérience, Fatigue, Burn-out) plus their numeric "X / 100" labels.
/// Other systems call the public Update* methods to push new values.
/// </summary>
public class EmployeePerformanceController : MonoBehaviour
{
    [Header("Sliders (progress bars)")]
    public Slider experienceSlider; // 'Barre_Experience'
    public Slider fatigueSlider;    // 'Barre_Fatigue'
    public Slider burnoutSlider;    // 'Barre_Burnout'

    [Header("Value labels")]
    public TextMeshProUGUI experienceValueText; // 'Valeur_Experience'
    public TextMeshProUGUI fatigueValueText;    // 'Valeur_Fatigue'
    public TextMeshProUGUI burnoutValueText;    // 'Valeur_Burnout'

    [Header("Burn-out game over")]
    public GameObject panelBurnoutGameOver; // 'Panel_BurnoutGameOver'
    public GameObject panelPosteActuel;     // 'Panel_Poste8actuel'
    public GameObject panelActionsRapides;  // 'Panel_Actions_Rapides'
    public GameObject panelRelationnel;     // 'Panel_Relationnel' (kept in sync with the dashboard)
    public DemissionController demissionController; // Handles job reset logic
    public RelationalController relationalController; // Handles relationship scores

    [Header("Salary negotiation")]
    public GameObject panelNegociationSalaire; // 'Panel_NegociationSalaire'
    public GameObject panelNegociationEchec;   // 'Panel_NegociationEchec' (error popup)
    public GameData gameData;                   // Shared game-state ScriptableObject (bank/salary)
    public TextMeshProUGUI texteSalaireAnnuelBrut; // Yearly gross salary label in 'Panel_Poste8actuel'

    // --- Current actual stat values (0-100) ---
    private int experienceScore;
    private int fatigueScore;
    private int burnoutScore;

    // --- Job context ---
    private int monthsAtCurrentJob;
    private bool hasJob;
    private int currentJobStress;
    public int currentJobHours { get; private set; }

    private void Start()
    {
        // Initialise every bar to its empty/default state.
        ApplyValue(experienceSlider, experienceValueText, 0);
        ApplyValue(fatigueSlider, fatigueValueText, 0);
        ApplyValue(burnoutSlider, burnoutValueText, 0);
    }

    private void OnEnable()
    {
        ActionPlay.OnMoisPasse += OnMonthPassed;
    }

    private void OnDisable()
    {
        ActionPlay.OnMoisPasse -= OnMonthPassed;
    }

    /// <summary>
    /// Called when the player accepts a new job. Resets fatigue/burn-out, stores
    /// the job context (stress level and weekly hours) and refreshes the UI.
    /// Experience is intentionally preserved across jobs.
    /// </summary>
    public void StartNewJob(int stressLevel, int hoursPerWeek)
    {
        hasJob = true;
        monthsAtCurrentJob = 0;
        fatigueScore = 20;
        burnoutScore = 0;
        currentJobStress = stressLevel;
        currentJobHours = hoursPerWeek;
        // Experience remains unchanged.

        RefreshUI();
    }

    /// <summary>Updates the stored weekly hours for the current job.</summary>
    public void UpdateJobHours(int newHours)
    {
        currentJobHours = newHours;
    }

    /// <summary>
    /// Observer callback fired by <see cref="ActionPlay.OnMoisPasse"/> every month.
    /// Advances the employee's stats based on tenure, stress and hours worked.
    /// </summary>
    private void OnMonthPassed()
    {
        if (!hasJob) return;

        monthsAtCurrentJob++;

        if (fatigueScore >= 100)
        {
            burnoutScore += 15;
        }

        // Auto-detect the relational controller if not assigned in Inspector
        if (relationalController == null)
        {
            relationalController = FindObjectOfType<RelationalController>();
        }

        // Fatigue reduction bonus: if colleagues relationship is maxed (100), reduce fatigue by 5.
        if (relationalController != null && relationalController.ColleguesScore == 100)
        {
            fatigueScore = Mathf.Clamp(fatigueScore - 5, 0, 100);
        }

        if (monthsAtCurrentJob % 5 == 0)
        {
            // Hours-based progression rules. Burn-out is no longer affected by
            // job stress or hours: it only rises when fatigue is maxed out (above).
            if (currentJobHours < 40)
            {
                experienceScore += 10;
            }
            else if (currentJobHours == 40)
            {
                fatigueScore += 10;
                experienceScore += 15;
            }
            else if (currentJobHours >= 45)
            {
                fatigueScore += 20;
                experienceScore += 20;
            }
        }

        experienceScore = Mathf.Clamp(experienceScore, 0, 100);
        fatigueScore = Mathf.Clamp(fatigueScore, 0, 100);
        burnoutScore = Mathf.Clamp(burnoutScore, 0, 100);

        RefreshUI();

        if (burnoutScore >= 100)
        {
            TriggerBurnout();
        }
    }

    /// <summary>
    /// Burn-out game over: shows the popup and hides the dashboard panels
    /// (including this performance card).
    /// </summary>
    private void TriggerBurnout()
    {
        if (panelBurnoutGameOver != null) panelBurnoutGameOver.SetActive(true);
        if (panelPosteActuel != null) panelPosteActuel.SetActive(false);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(false);
        if (panelRelationnel != null) panelRelationnel.SetActive(false);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Wired to 'Bouton_RetourBurnout'. Closes the popup, fully resets the job
    /// state (experience is preserved) and restores the dashboard panels.
    /// </summary>
    public void OnBurnoutRetourClicked()
    {
        if (panelBurnoutGameOver != null) panelBurnoutGameOver.SetActive(false);

        // Call the resignation system to reset the job data
        if (demissionController != null)
        {
            demissionController.OnOuiClicked();
        }

        // Force reset the "Poste Actuel" panel UI and GameData manually in case the DemissionController reference is missing or fails
        if (panelPosteActuel != null)
        {
            Transform tEntr = panelPosteActuel.transform.Find("Entreprise");
            if (tEntr != null && tEntr.GetComponent<TMPro.TMP_Text>() != null) tEntr.GetComponent<TMPro.TMP_Text>().text = "Entreprise : Aucune";

            Transform tSal = panelPosteActuel.transform.Find("Salaire_brut");
            if (tSal != null && tSal.GetComponent<TMPro.TMP_Text>() != null) tSal.GetComponent<TMPro.TMP_Text>().text = "Salaire : 0€ / an";

            Transform tHeures = panelPosteActuel.transform.Find("Heures");
            if (tHeures != null && tHeures.GetComponent<TMPro.TMP_Text>() != null) tHeures.GetComponent<TMPro.TMP_Text>().text = "Heures : 0 heures / semaine";
        }

        if (gameData != null && gameData.joueur != null)
        {
            gameData.joueur.salaire = new argent(0);
        }

        hasJob = false;
        monthsAtCurrentJob = 0;
        fatigueScore = 20;
        burnoutScore = 0;
        currentJobHours = 0;
        // Experience remains accumulated.

        RefreshUI();

        if (panelPosteActuel != null) panelPosteActuel.SetActive(true);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(true);
        if (panelRelationnel != null) panelRelationnel.SetActive(true);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Decreases or increases the fatigue score by <paramref name="amount"/>.
    /// Clamped to [0, 100] and refreshes the UI.
    /// </summary>
    public void ModifyFatigue(int amount)
    {
        fatigueScore = Mathf.Clamp(fatigueScore + amount, 0, 100);
        RefreshUI();
    }

    /// <summary>
    /// Wired to the main 'Bouton Négocier salaire'. Only opens the negotiation
    /// panel when the employee is experienced enough (experience strictly above 70).
    /// </summary>
    public void OnNegocierSalaireClicked()
    {
        if (experienceScore > 70)
        {
            if (panelNegociationSalaire != null) panelNegociationSalaire.SetActive(true);
        }
        else
        {
            if (panelNegociationEchec != null) panelNegociationEchec.SetActive(true);
        }

        // Hide the dashboard so only the negotiation popup is visible (matches the networking flow).
        if (panelPosteActuel != null) panelPosteActuel.SetActive(false);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(false);
        if (panelRelationnel != null) panelRelationnel.SetActive(false);
        gameObject.SetActive(false); // this script lives on Panel_PerformanceEmploye
    }

    /// <summary>Wired to the negotiation panel buttons. Closes 'Panel_NegociationSalaire'.</summary>
    public void CloseNegociationPanel()
    {
        if (panelNegociationSalaire != null) panelNegociationSalaire.SetActive(false);

        // Restore the dashboard (matches the networking flow).
        if (panelPosteActuel != null) panelPosteActuel.SetActive(true);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(true);
        if (panelRelationnel != null) panelRelationnel.SetActive(true);
        gameObject.SetActive(true);
    }

    /// <summary>Wired to 'Bouton_RetourEchec'. Closes the error popup 'Panel_NegociationEchec'.</summary>
    public void CloseNegociationEchecPanel()
    {
        if (panelNegociationEchec != null) panelNegociationEchec.SetActive(false);

        // Restore the dashboard (matches the networking flow).
        if (panelPosteActuel != null) panelPosteActuel.SetActive(true);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(true);
        if (panelRelationnel != null) panelRelationnel.SetActive(true);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Wired to 'Bouton_AccepterNegociation'. Raises the monthly salary by 600 €,
    /// refreshes the yearly-gross label (monthly × 12) and closes the panel.
    /// Note: <see cref="GameData.salaire"/> is an <see cref="argent"/> value stored
    /// in centimes, so +600 € is added via the float constructor (60000 centimes).
    /// </summary>
    public void OnAccepterNegociationClicked()
    {
        if (gameData != null && gameData.joueur != null)
        {
            // +600 € — the float ctor converts euros to centimes (600 * 100).
            gameData.joueur.salaire += new argent(600f);

            if (texteSalaireAnnuelBrut != null)
            {
                // Yearly gross = monthly × 12, converted from centimes to euros.
                texteSalaireAnnuelBrut.text = "Salaire : " + (gameData.joueur.salaire * 12).ToString("N0") + "€ / an";
            }
        }

        if (panelNegociationSalaire != null) panelNegociationSalaire.SetActive(false);

        // Restore the dashboard (matches the networking flow).
        if (panelPosteActuel != null) panelPosteActuel.SetActive(true);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(true);
        if (panelRelationnel != null) panelRelationnel.SetActive(true);
        gameObject.SetActive(true);
    }

    /// <summary>Pushes the current score values onto the sliders and labels.</summary>
    private void RefreshUI()
    {
        ApplyValue(experienceSlider, experienceValueText, experienceScore);
        ApplyValue(fatigueSlider, fatigueValueText, fatigueScore);
        ApplyValue(burnoutSlider, burnoutValueText, burnoutScore);
    }

    /// <summary>Set the Expérience bar (0-100).</summary>
    public void UpdateExperience(int value)
    {
        ApplyValue(experienceSlider, experienceValueText, value);
    }

    /// <summary>Set the Fatigue bar (0-100).</summary>
    public void UpdateFatigue(int value)
    {
        ApplyValue(fatigueSlider, fatigueValueText, value);
    }

    /// <summary>Set the Burn-out bar (0-100).</summary>
    public void UpdateBurnout(int value)
    {
        ApplyValue(burnoutSlider, burnoutValueText, value);
    }

    /// <summary>
    /// Clamps <paramref name="value"/> to [0, 100], updates the slider, the
    /// "X / 100" label and keeps the fill colour white (for now).
    /// </summary>
    private void ApplyValue(Slider slider, TextMeshProUGUI valueText, int value)
    {
        int score = Mathf.Clamp(value, 0, 100);

        if (slider != null) slider.value = score;
        if (valueText != null) valueText.text = score + " / 100";

        SetFillColor(slider, Color.white);
    }

    private void SetFillColor(Slider slider, Color color)
    {
        if (slider == null || slider.fillRect == null) return;

        Image fillImage = slider.fillRect.GetComponent<Image>();
        if (fillImage != null) fillImage.color = color;
    }
}
