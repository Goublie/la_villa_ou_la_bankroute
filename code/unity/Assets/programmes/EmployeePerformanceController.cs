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

    private void Start()
    {
        // Initialise every bar to its empty/default state.
        ApplyValue(experienceSlider, experienceValueText, 0);
        ApplyValue(fatigueSlider, fatigueValueText, 0);
        ApplyValue(burnoutSlider, burnoutValueText, 0);
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
