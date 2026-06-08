using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the "Relationnel" stats card. Lives on 'Panel_Relationnel'.
/// Tracks the player's relationship scores with their Boss (Patron) and
/// Colleagues (Collègues) and exposes public methods so other systems can
/// modify those scores. Each score drives a 0-100 slider and a "X / 100" label.
/// </summary>
public class RelationalController : MonoBehaviour
{
    [Header("Patron (Boss)")]
    public Slider sliderPatron;            // 'Slider_Patron'
    public TextMeshProUGUI textePatron;    // value label of 'Slider_Patron'

    [Header("Collègues (Colleagues)")]
    public Slider sliderCollegues;         // 'Slider_Collegues'
    public TextMeshProUGUI texteCollegues; // value label of 'Slider_Collegues'

    // --- Current relationship scores (0-100) ---
    private int patronScore = 0;
    private int colleguesScore = 0;

    private void Start()
    {
        // Initialise both scores and force the UI to the empty/default state.
        patronScore = 0;
        colleguesScore = 0;

        RefreshPatron();
        RefreshCollegues();
    }

    /// <summary>Adds <paramref name="amount"/> to the Boss score (clamped 0-100) and refreshes the UI.</summary>
    public void ModifyPatronScore(int amount)
    {
        patronScore = Mathf.Clamp(patronScore + amount, 0, 100);
        RefreshPatron();
    }

    /// <summary>Adds <paramref name="amount"/> to the Colleagues score (clamped 0-100) and refreshes the UI.</summary>
    public void ModifyColleguesScore(int amount)
    {
        colleguesScore = Mathf.Clamp(colleguesScore + amount, 0, 100);
        RefreshCollegues();
    }

    private void RefreshPatron()
    {
        if (sliderPatron != null) sliderPatron.value = patronScore;
        if (textePatron != null) textePatron.text = patronScore + " / 100";
    }

    private void RefreshCollegues()
    {
        if (sliderCollegues != null) sliderCollegues.value = colleguesScore;
        if (texteCollegues != null) texteCollegues.text = colleguesScore + " / 100";
    }
}
