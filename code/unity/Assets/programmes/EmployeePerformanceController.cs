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

    // --- Current actual stat values (0-100) ---
    private int experienceScore;
    private int fatigueScore;
    private int burnoutScore;

    // --- Job context ---
    private int monthsAtCurrentJob;
    private bool hasJob;
    private int currentJobStress;
    private int currentJobHours;

    private void Start()
    {
        // Initialise every bar to its empty/default state.
        ApplyValue(experienceSlider, experienceValueText, 0);
        ApplyValue(fatigueSlider, fatigueValueText, 0);
        ApplyValue(burnoutSlider, burnoutValueText, 0);
    }

    private void OnEnable()
    {
        ActionPlay.moisPasse += OnMonthPassed;
    }

    private void OnDisable()
    {
        ActionPlay.moisPasse -= OnMonthPassed;
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
    /// Observer callback fired by <see cref="ActionPlay.moisPasse"/> every month.
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

        if (monthsAtCurrentJob % 5 == 0)
        {
            experienceScore += 15;

            if (currentJobStress == 2) burnoutScore += 5;
            if (currentJobStress >= 3) burnoutScore += 10;

            if (currentJobHours >= 45)
            {
                fatigueScore += 10;
                burnoutScore += 5;
            }
        }

        experienceScore = Mathf.Clamp(experienceScore, 0, 100);
        fatigueScore = Mathf.Clamp(fatigueScore, 0, 100);
        burnoutScore = Mathf.Clamp(burnoutScore, 0, 100);

        RefreshUI();
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
