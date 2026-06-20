using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Facade Unity de la carte de performance salariee.
/// </summary>
/// <remarks>
/// Les scores sont stockes dans <see cref="DonneesSalariat"/> et modifies par
/// <see cref="ServiceSalariat"/>. Ce composant se limite aux panels, sliders et
/// boutons du prefab Salariat.
/// </remarks>
public class EmployeePerformanceController : MonoBehaviour
{
    [Header("Sliders (progress bars)")]
    public Slider experienceSlider;
    public Slider fatigueSlider;
    public Slider burnoutSlider;

    [Header("Value labels")]
    public TextMeshProUGUI experienceValueText;
    public TextMeshProUGUI fatigueValueText;
    public TextMeshProUGUI burnoutValueText;

    [Header("Burn-out game over")]
    public GameObject panelBurnoutGameOver;
    public GameObject panelPosteActuel;
    public GameObject panelActionsRapides;
    public GameObject panelRelationnel;
    public DemissionController demissionController;
    public RelationalController relationalController;

    [Header("Salary negotiation")]
    public GameObject panelNegociationSalaire;
    public GameObject panelNegociationEchec;
    public GameData gameData;
    public TextMeshProUGUI texteSalaireAnnuelBrut;

    private ServiceSalariat service;
    private DonneesSalariat donnees;

    public int currentJobHours =>
        donnees != null ? donnees.heuresSemaine : 0;

    private void Start()
    {
        ResoudreService();
        RefreshUI();
    }

    private void OnEnable()
    {
        ActionPlay.OnMoisPasse += OnMonthPassed;
        ResoudreService();
        RefreshUI();
    }

    private void OnDisable()
    {
        ActionPlay.OnMoisPasse -= OnMonthPassed;
    }

    /// <summary>
    /// Signale l'acceptation d'un poste par un ancien flux du prefab.
    /// </summary>
    public void StartNewJob(int stressLevel, int hoursPerWeek)
    {
        if (!ResoudreService())
        {
            return;
        }

        if (!donnees.aEmploi)
        {
            service.AccepterPoste(
                "Poste actuel",
                gameData.joueur.salaire.centimes,
                hoursPerWeek,
                stressLevel,
                1,
                1);
        }
        else
        {
            service.ActualiserContextePoste(stressLevel, hoursPerWeek);
        }

        RefreshUI();
    }

    /// <summary>
    /// Met a jour les heures du poste courant.
    /// </summary>
    public void UpdateJobHours(int newHours)
    {
        if (ResoudreService())
        {
            service.ActualiserContextePoste(
                donnees.stressPoste,
                newHours);
            RefreshUI();
        }
    }

    private void OnMonthPassed()
    {
        Debug.Log("🔔 L'événement OnMoisPasse a été reçu par la carte de performance !");

        if (!ResoudreService())
        {
            Debug.LogWarning("❌ Impossible de charger les données du joueur.");
            return;
        }

        Debug.Log("📊 Valeur de l'expérience en base de données AVANT rafraîchissement : " + donnees.experience);

        RefreshUI();

        if (donnees.burnout >= 100)
        {
            TriggerBurnout();
        }
    }

    private void TriggerBurnout()
    {
        if (panelBurnoutGameOver != null)
        {
            panelBurnoutGameOver.SetActive(true);
        }

        AfficherTableauBord(false);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Ferme le message de burn-out et remet le poste courant a zero.
    /// </summary>
    public void OnBurnoutRetourClicked()
    {
        if (panelBurnoutGameOver != null)
        {
            panelBurnoutGameOver.SetActive(false);
        }

        if (demissionController != null)
        {
            demissionController.OnOuiClicked();
        }
        else if (ResoudreService())
        {
            service.Demissionner();
        }

        RefreshUI();
        AfficherTableauBord(true);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Modifie la fatigue en points.
    /// </summary>
    public void ModifyFatigue(int amount)
    {
        if (ResoudreService())
        {
            service.ModifierFatigue(amount);
            RefreshUI();
        }
    }

    /// <summary>
    /// Modifie l'experience en points.
    /// </summary>
    public void ModifyExperience(int amount)
    {
        if (ResoudreService())
        {
            service.ModifierExperience(amount);
            RefreshUI();
        }
    }

    /// <summary>
    /// Ouvre la negociation si l'experience est suffisante.
    /// </summary>
    public void OnNegocierSalaireClicked()
    {
        if (!ResoudreService())
        {
            return;
        }

        if (donnees.experience > 70)
        {
            if (panelNegociationSalaire != null)
            {
                panelNegociationSalaire.SetActive(true);
            }
        }
        else if (panelNegociationEchec != null)
        {
            panelNegociationEchec.SetActive(true);
        }

        AfficherTableauBord(false);
        gameObject.SetActive(false);
    }

    public void CloseNegociationPanel()
    {
        if (panelNegociationSalaire != null)
        {
            panelNegociationSalaire.SetActive(false);
        }

        AfficherTableauBord(true);
        gameObject.SetActive(true);
    }

    public void CloseNegociationEchecPanel()
    {
        if (panelNegociationEchec != null)
        {
            panelNegociationEchec.SetActive(false);
        }

        AfficherTableauBord(true);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Valide une negociation salariale et synchronise les labels.
    /// </summary>
    public void OnAccepterNegociationClicked()
    {
        if (ResoudreService())
        {
            service.NegocierSalaire();
            ActualiserSalaireAnnuel();
            RefreshUI();
        }

        CloseNegociationPanel();
    }

    /// <summary>
    /// Ajuste heures, salaire et satisfaction depuis le panel Travailler plus.
    /// </summary>
    public ResultatOperation ModifierTempsTravail(
        int deltaHeures,
        int deltaSalaireCentimes,
        int deltaSatisfaction)
    {
        if (!ResoudreService())
        {
            return ResultatOperation.Echec(
                "Donnees salariat indisponibles.",
                "donnees_absentes");
        }

        ResultatOperation resultat =
            service.ModifierTempsTravail(
                deltaHeures,
                deltaSalaireCentimes,
                deltaSatisfaction);
        RefreshUI();
        ActualiserSalaireAnnuel();
        return resultat;
    }

    public void UpdateExperience(int value)
    {
        if (ResoudreService())
        {
            donnees.experience = Mathf.Clamp(value, 0, 100);
            RefreshUI();
        }
    }

    public void UpdateFatigue(int value)
    {
        if (ResoudreService())
        {
            donnees.fatigue = Mathf.Clamp(value, 0, 100);
            RefreshUI();
        }
    }

    public void UpdateBurnout(int value)
    {
        if (ResoudreService())
        {
            donnees.burnout = Mathf.Clamp(value, 0, 100);
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (donnees == null)
        {
            ApplyValue(experienceSlider, experienceValueText, 0);
            ApplyValue(fatigueSlider, fatigueValueText, 0);
            ApplyValue(burnoutSlider, burnoutValueText, 0);
            return;
        }

        ApplyValue(
            experienceSlider,
            experienceValueText,
            donnees.experience);
        ApplyValue(
            fatigueSlider,
            fatigueValueText,
            donnees.fatigue);
        ApplyValue(
            burnoutSlider,
            burnoutValueText,
            donnees.burnout);
    }

    private void ApplyValue(
        Slider slider,
        TextMeshProUGUI valueText,
        int value)
    {
        int score = Mathf.Clamp(value, 0, 100);
        if (slider != null)
        {
            slider.value = score;
        }

        if (valueText != null)
        {
            valueText.text = score + " / 100";
        }

        SetFillColor(slider, Color.white);
    }

    private void SetFillColor(Slider slider, Color color)
    {
        if (slider == null || slider.fillRect == null)
        {
            return;
        }

        Image fillImage = slider.fillRect.GetComponent<Image>();
        if (fillImage != null)
        {
            fillImage.color = color;
        }
    }

    private void AfficherTableauBord(bool visible)
    {
        if (panelPosteActuel != null)
        {
            panelPosteActuel.SetActive(visible);
        }

        if (panelActionsRapides != null)
        {
            panelActionsRapides.SetActive(visible);
        }

        if (panelRelationnel != null)
        {
            panelRelationnel.SetActive(visible);
        }
    }

    private void ActualiserSalaireAnnuel()
    {
        if (texteSalaireAnnuelBrut == null ||
            gameData == null ||
            gameData.joueur == null)
        {
            return;
        }

        argent salaireAnnuel = gameData.joueur.salaire * 12f;
        // ◄ FIX FORMATTAGE
        texteSalaireAnnuelBrut.text =
            "Salaire brut : " + salaireAnnuel.ToString("N0") + " € / an";
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
}