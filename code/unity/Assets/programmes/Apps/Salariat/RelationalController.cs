using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Facade Unity de la carte relationnelle du Salariat.
/// </summary>
public class RelationalController : MonoBehaviour
{
    [Header("Patron (Boss)")]
    public Slider sliderPatron;
    public TextMeshProUGUI textePatron;

    [Header("Collegues (Colleagues)")]
    public Slider sliderCollegues;
    public TextMeshProUGUI texteCollegues;

    [SerializeField] private GameData gameData;

    private ServiceSalariat service;
    private DonneesSalariat donnees;

    public int PatronScore =>
        donnees != null ? donnees.relationPatron : 0;

    public int ColleguesScore =>
        donnees != null ? donnees.relationCollegues : 0;

    private void Start()
    {
        ResoudreService();
        RefreshPatron();
        RefreshCollegues();
    }

    private void OnEnable()
    {
        ResoudreService();
        RefreshPatron();
        RefreshCollegues();
    }

    /// <summary>
    /// Modifie la relation patron puis rafraichit l'UI.
    /// </summary>
    public void ModifyPatronScore(int amount)
    {
        if (ResoudreService())
        {
            service.ModifierRelationPatron(amount);
            RefreshPatron();
        }
    }

    /// <summary>
    /// Modifie la relation collegues puis rafraichit l'UI.
    /// </summary>
    public void ModifyColleguesScore(int amount)
    {
        if (ResoudreService())
        {
            service.ModifierRelationCollegues(amount);
            RefreshCollegues();
        }
    }

    private void RefreshPatron()
    {
        int valeur = PatronScore;
        if (sliderPatron != null)
        {
            sliderPatron.value = valeur;
        }

        if (textePatron != null)
        {
            textePatron.text = valeur + " / 100";
        }
    }

    private void RefreshCollegues()
    {
        int valeur = ColleguesScore;
        if (sliderCollegues != null)
        {
            sliderCollegues.value = valeur;
        }

        if (texteCollegues != null)
        {
            texteCollegues.text = valeur + " / 100";
        }
    }

    private bool ResoudreService()
    {
        if (gameData == null)
        {
            EmployeePerformanceController performance =
                Object.FindFirstObjectByType<EmployeePerformanceController>();
            if (performance != null)
            {
                gameData = performance.gameData;
            }
        }

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
}
