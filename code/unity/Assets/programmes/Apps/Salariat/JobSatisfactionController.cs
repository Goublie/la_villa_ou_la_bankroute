using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Affiche et met a jour la satisfaction professionnelle du joueur salarie.
/// </summary>
/// <remarks>
/// La valeur source est <see cref="DonneesSalariat.satisfaction"/>. Les methodes
/// publiques restent compatibles avec l'ancien prefab, mais elles deleguent les
/// modifications au <see cref="ServiceSalariat"/> lorsque <see cref="GameData"/>
/// est disponible.
/// </remarks>
public class JobSatisfactionController : MonoBehaviour
{
    [Header("Data")]
    public GameData gameData;

    [Header("UI References")]
    public Slider satisfactionSlider;
    public TextMeshProUGUI valueText;

    private DonneesSalariat donnees;
    private ServiceSalariat service;

    private void Start()
    {
        ResoudreService();
        RefreshFromData();
    }

    /// <summary>
    /// Calcule la satisfaction d'une offre et persiste le score en points 0-100.
    /// </summary>
    public void UpdateSatisfaction(int stress, int prestige, int equilibre)
    {
        int score = ServiceSalariat.CalculerSatisfaction(
            stress,
            prestige,
            equilibre);

        if (ResoudreService())
        {
            donnees.satisfaction = score;
        }

        Render(score);
    }

    /// <summary>
    /// Modifie la satisfaction courante de <paramref name="amount"/> points.
    /// </summary>
    public void ModifySatisfaction(int amount)
    {
        if (ResoudreService())
        {
            service.ModifierSatisfaction(amount);
            Render(donnees.satisfaction);
            return;
        }

        int score = satisfactionSlider == null
            ? 0
            : Mathf.RoundToInt(satisfactionSlider.value);
        Render(Mathf.Clamp(score + amount, 0, 100));
    }

    /// <summary>
    /// Repeint la jauge depuis l'agregat persistant sans modifier l'etat metier.
    /// </summary>
    public void RefreshFromData()
    {
        if (ResoudreService())
        {
            Render(donnees.satisfaction);
            return;
        }

        int score = satisfactionSlider == null
            ? 0
            : Mathf.RoundToInt(satisfactionSlider.value);
        Render(score);
    }

    /// <summary>
    /// Remet la satisfaction a zero, notamment lors d'une demission.
    /// </summary>
    public void ResetSatisfaction()
    {
        if (ResoudreService())
        {
            donnees.satisfaction = 0;
        }

        Render(0);
    }

    private void Render(int score)
    {
        int valeur = Mathf.Clamp(score, 0, 100);

        if (satisfactionSlider != null)
        {
            satisfactionSlider.value = valeur;
        }

        if (valueText != null)
        {
            valueText.text = valeur + " / 100";
        }

        SetFillColor(GetColorForScore(valeur));
    }

    private bool ResoudreService()
    {
        if (gameData == null)
        {
            ActionPlay actionPlay =
                Object.FindFirstObjectByType<ActionPlay>();
            if (actionPlay != null)
            {
                gameData = actionPlay.gameData;
            }
        }

        if (gameData == null || gameData.joueur == null)
        {
            return false;
        }

        gameData.joueur.InitialiserSiNecessaire();
        donnees = gameData.joueur.salariat;
        service = new ServiceSalariat(donnees, gameData.joueur);
        return true;
    }

    private Color GetColorForScore(int score)
    {
        if (score == 0)
        {
            return Color.white;
        }

        if (score <= 35)
        {
            return Color.red;
        }

        if (score <= 70)
        {
            return Color.yellow;
        }

        return Color.green;
    }

    private void SetFillColor(Color color)
    {
        if (satisfactionSlider == null || satisfactionSlider.fillRect == null)
        {
            return;
        }

        Image fillImage = satisfactionSlider.fillRect.GetComponent<Image>();
        if (fillImage != null)
        {
            fillImage.color = color;
        }
    }
}
