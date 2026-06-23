using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Facade Unity de la carte de performance salariee.
/// </summary>
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
    public Button boutonNegocierSalaire; // ◄ AJOUT : Glisse ici ton bouton de négociation pour pouvoir le griser
    public GameObject panelNegociationSalaire;
    public GameObject panelNegociationEchec;
    public GameData gameData;
    public TextMeshProUGUI texteSalaireAnnuelBrut;

    private ServiceSalariat service;
    private DonneesSalariat donnees;
    private int toursRestantsAvantNegociation = 0; // ◄ AJOUT : Compteur de cooldown

    public int currentJobHours => donnees != null ? donnees.heuresSemaine : 0;

    private void Start()
    {
        // ◄ SÉCURITÉ CRITIQUE : Écoute les tours ici pour ne rien rater si le joueur change d'onglet
        ActionPlay.OnMoisPasse += OnMonthPassed;

        ResoudreService();
        RefreshUI();
        ActualiserEtatBoutonNegociation();
    }

    private void OnDestroy()
    {
        // Désabonnement à la destruction du script
        ActionPlay.OnMoisPasse -= OnMonthPassed;
    }

    private void OnEnable()
    {
        ResoudreService();
        RefreshUI();
        ActualiserEtatBoutonNegociation();
    }

    public void StartNewJob(int stressLevel, int hoursPerWeek)
    {
        if (!ResoudreService()) return;

        if (!donnees.aEmploi)
        {
            service.AccepterPoste("Poste actuel", gameData.joueur.salaire.centimes, hoursPerWeek, stressLevel, 1, 1);
        }
        else
        {
            service.ActualiserContextePoste(stressLevel, hoursPerWeek);
        }

        RefreshUI();
    }

    public void UpdateJobHours(int newHours)
    {
        if (ResoudreService())
        {
            service.ActualiserContextePoste(donnees.stressPoste, newHours);
            RefreshUI();
        }
    }

    private void OnMonthPassed()
    {
        Debug.Log("🔔 L'événement OnMoisPasse a été reçu par la carte de performance !");

        // ◄ AJOUT : On diminue le temps d'attente à chaque tour
        if (toursRestantsAvantNegociation > 0)
        {
            toursRestantsAvantNegociation--;
        }

        if (!ResoudreService())
        {
            Debug.LogWarning("❌ Impossible de charger les données du joueur.");
            return;
        }

        RefreshUI();
        ActualiserEtatBoutonNegociation(); // ◄ Met à jour le bouton grisé ou non

        if (donnees.burnout >= 100)
        {
            TriggerBurnout();
        }
    }

    /// <summary>
    /// ◄ AJOUT : Gère l'état interactif du bouton de négociation
    /// </summary>
    private void ActualiserEtatBoutonNegociation()
    {
        if (boutonNegocierSalaire != null)
        {
            // Le bouton est cliquable uniquement si le cooldown est terminé (à 0)
            boutonNegocierSalaire.interactable = (toursRestantsAvantNegociation <= 0);
        }
    }

    private void TriggerBurnout()
    {
        if (panelBurnoutGameOver != null) panelBurnoutGameOver.SetActive(true);
        AfficherTableauBord(false);
        gameObject.SetActive(false);
    }

    public void OnBurnoutRetourClicked()
    {
        if (panelBurnoutGameOver != null) panelBurnoutGameOver.SetActive(false);

        if (demissionController != null) demissionController.OnOuiClicked();
        else if (ResoudreService()) service.Demissionner();

        RefreshUI();
        AfficherTableauBord(true);
        gameObject.SetActive(true);
    }

    public void ModifyFatigue(int amount)
    {
        if (ResoudreService()) { service.ModifierFatigue(amount); RefreshUI(); }
    }

    public void ModifyExperience(int amount)
    {
        if (ResoudreService()) { service.ModifierExperience(amount); RefreshUI(); }
    }

    public void OnNegocierSalaireClicked()
    {
        if (!ResoudreService()) return;

        // Sécurité supplémentaire : si le cooldown est actif, on bloque l'ouverture
        if (toursRestantsAvantNegociation > 0) return;

        if (donnees.experience > 70)
        {
            if (panelNegociationSalaire != null) panelNegociationSalaire.SetActive(true);
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
        if (panelNegociationSalaire != null) panelNegociationSalaire.SetActive(false);
        AfficherTableauBord(true);
        gameObject.SetActive(true);
    }

    public void CloseNegociationEchecPanel()
    {
        if (panelNegociationEchec != null) panelNegociationEchec.SetActive(false);
        AfficherTableauBord(true);
        gameObject.SetActive(true);
    }

    public void OnAccepterNegociationClicked()
    {
        if (ResoudreService())
        {
            service.NegocierSalaire();

            // ◄ AJOUT : Déclenche les 12 tours de blocage
            toursRestantsAvantNegociation = 12;

            ActualiserSalaireAnnuel();
            RefreshUI();
            ActualiserEtatBoutonNegociation();
        }

        CloseNegociationPanel();
    }

    public ResultatOperation ModifierTempsTravail(int deltaHeures, int deltaSalaireCentimes, int deltaSatisfaction)
    {
        if (!ResoudreService())
        {
            return ResultatOperation.Echec("Donnees salariat indisponibles.", "donnees_absentes");
        }

        ResultatOperation resultat = service.ModifierTempsTravail(deltaHeures, deltaSalaireCentimes, deltaSatisfaction);
        RefreshUI();
        ActualiserSalaireAnnuel();
        return resultat;
    }

    public void UpdateExperience(int value) { if (ResoudreService()) { donnees.experience = Mathf.Clamp(value, 0, 100); RefreshUI(); } }
    public void UpdateFatigue(int value) { if (ResoudreService()) { donnees.fatigue = Mathf.Clamp(value, 0, 100); RefreshUI(); } }
    public void UpdateBurnout(int value) { if (ResoudreService()) { donnees.burnout = Mathf.Clamp(value, 0, 100); RefreshUI(); } }

    private void RefreshUI()
    {
        if (donnees == null)
        {
            ApplyValue(experienceSlider, experienceValueText, 0);
            ApplyValue(fatigueSlider, fatigueValueText, 0);
            ApplyValue(burnoutSlider, burnoutValueText, 0);
            return;
        }

        ApplyValue(experienceSlider, experienceValueText, donnees.experience);
        ApplyValue(fatigueSlider, fatigueValueText, donnees.fatigue);
        ApplyValue(burnoutSlider, burnoutValueText, donnees.burnout);
    }

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

    private void AfficherTableauBord(bool visible)
    {
        if (panelPosteActuel != null) panelPosteActuel.SetActive(visible);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(visible);
        if (panelRelationnel != null) panelRelationnel.SetActive(visible);
    }

    private void ActualiserSalaireAnnuel()
    {
        if (texteSalaireAnnuelBrut == null || gameData == null || gameData.joueur == null) return;
        argent salaireAnnuel = gameData.joueur.salaire * 12f;
        texteSalaireAnnuelBrut.text = "Salaire brut : " + salaireAnnuel.ToString("N0") + " € / an";
    }

    private bool ResoudreService()
    {
        if (gameData == null)
        {
            ActionPlay actionPlay = Object.FindFirstObjectByType<ActionPlay>();
            if (actionPlay != null) gameData = actionPlay.gameData;
        }

        if (gameData == null || gameData.joueur == null) return false;

        gameData.joueur.InitialiserSiNecessaire();
        donnees = gameData.joueur.salariat;
        service = new ServiceSalariat(donnees, gameData.joueur);
        return true;
    }
}