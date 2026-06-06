using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the "Satisfaction travail" (job satisfaction) progress bar.
/// Lives on 'Panel_Satisfaction'. Exposes a UI Slider used as a read-only
/// progress bar plus the numeric value label, and recomputes a 0-100 score
/// from stress / prestige / equilibre inputs, colouring the fill accordingly.
/// </summary>
public class JobSatisfactionController : MonoBehaviour
{
    [Header("UI References")]
    public Slider satisfactionSlider;   // 'Barre_Satisfaction'
    public TextMeshProUGUI valueText;   // 'Texte_Valeur'

    /// <summary>
    /// Recompute the satisfaction score from the interview parameters and refresh the UI.
    /// Formula: 50 + (prestige * 15) + (equilibre * 15) - (stress * 20), clamped to [0, 100].
    /// </summary>
    public void UpdateSatisfaction(int stress, int prestige, int equilibre)
    {
        int score = 50 + (prestige * 15) + (equilibre * 15) - (stress * 20);
        score = Mathf.Clamp(score, 0, 100);

        if (satisfactionSlider != null) satisfactionSlider.value = score;
        if (valueText != null) valueText.text = score + " / 100";

        SetFillColor(GetColorForScore(score));
    }

    /// <summary>
    /// Manually modify the satisfaction score by <paramref name="amount"/> (can be negative).
    /// Adjusts the slider value directly, clamps it to [0, 100], and refreshes the
    /// "X / 100" label and fill colour using the existing colour logic.
    /// </summary>
    public void ModifySatisfaction(int amount)
    {
        if (satisfactionSlider == null) return;

        int score = Mathf.Clamp(Mathf.RoundToInt(satisfactionSlider.value) + amount, 0, 100);

        satisfactionSlider.value = score;
        if (valueText != null) valueText.text = score + " / 100";

        SetFillColor(GetColorForScore(score));
    }

    /// <summary>
    /// Reset the bar to its empty/default state (value 0, "0 / 100", white fill).
    /// </summary>
    public void ResetSatisfaction()
    {
        if (satisfactionSlider != null) satisfactionSlider.value = 0;
        if (valueText != null) valueText.text = "0 / 100";

        SetFillColor(Color.white);
    }

    private Color GetColorForScore(int score)
    {
        if (score == 0) return Color.white;
        if (score <= 35) return Color.red;
        if (score <= 70) return Color.yellow;
        return Color.green;
    }

    private void SetFillColor(Color color)
    {
        if (satisfactionSlider == null || satisfactionSlider.fillRect == null) return;

        Image fillImage = satisfactionSlider.fillRect.GetComponent<Image>();
        if (fillImage != null) fillImage.color = color;
    }
}
