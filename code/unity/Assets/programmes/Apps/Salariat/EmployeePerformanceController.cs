using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Facade Unity de la carte de performance salariee.
/// Gère également la synchronisation des données textuelles du poste actuel.
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
    public Button boutonNegocierSalaire;
    public GameObject panelNegociationSalaire;
    public GameObject panelNegociationEchec;
    public GameData gameData;
    public TextMeshProUGUI texteSalaireAnnuelBrut;

    private ServiceSalariat service;
    private DonneesSalariat donnees;
    private int toursRestantsAvantNegociation = 0;

    // ◄ AJOUT : Cache interne pour retrouver automatiquement les textes du Tableau de Bord
    private TextMeshProUGUI entrepriseTextInternal;
    private TextMeshProUGUI heuresTextInternal;

    public int currentJobHours => donnees != null ? donnees.heuresSemaine : 0;

    private void Start()
    {
        ActionPlay.OnMoisPasse += OnMonthPassed;

        ControleEtTrouveTextesPoste();
        ResoudreService();
        RefreshUI();
        ActualiserEtatBoutonNegociation();
    }

    private void OnDestroy()
    {
        ActionPlay.OnMoisPasse -= OnMonthPassed;
    }

    private void OnEnable()
    {
        ControleEtTrouveTextesPoste();
        ResoudreService();
        RefreshUI();
        ActualiserEtatBoutonNegociation();
    }

    /// <summary>
    /// ◄ AJOUT : Cherche dynamiquement les composants de texte s'ils ne sont pas déjà liés
    /// </summary>
    private void ControleEtTrouveTextesPoste()
    {
        if (panelPosteActuel != null)
        {
            if (entrepriseTextInternal == null)
            {
                Transform t = panelPosteActuel.transform.Find("Entreprise");
                if (t != null) entrepriseTextInternal = t.GetComponent<TextMeshProUGUI>();
            }
            if (heuresTextInternal == null)
            {
                Transform t = panelPosteActuel.transform.Find("Heures");
                if (t != null) heuresTextInternal = t.GetComponent<TextMeshProUGUI>();
            }
            if (texteSalaireAnnuelBrut == null)
            {
                Transform t = panelPosteActuel.transform.Find("Salaire_brut");
                if (t != null) texteSalaireAnnuelBrut = t.GetComponent<TextMeshProUGUI>();
            }
        }
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
        ActualiserEtatBoutonNegociation();

        if (donnees.burnout >= 100)
        {
            TriggerBurnout();
        }
    }

    private void ActualiserEtatBoutonNegociation()
    {
        if (boutonNegocierSalaire != null)
        {
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
            toursRestantsAvantNegociation = 12;
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
        return resultat;
    }

    public void UpdateExperience(int value) { if (ResoudreService()) { donnees.experience = Mathf.Clamp(value, 0, 100); RefreshUI(); } }
    public void UpdateFatigue(int value) { if (ResoudreService()) { donnees.fatigue = Mathf.Clamp(value, 0, 100); RefreshUI(); } }
    public void UpdateBurnout(int value) { if (ResoudreService()) { donnees.burnout = Mathf.Clamp(value, 0, 100); RefreshUI(); } }

    private void RefreshUI()
    {
        ControleEtTrouveTextesPoste();

        if (donnees == null)
        {
            ApplyValue(experienceSlider, experienceValueText, 0);
            ApplyValue(fatigueSlider, fatigueValueText, 0);
            ApplyValue(burnoutSlider, burnoutValueText, 0);

            if (entrepriseTextInternal != null) entrepriseTextInternal.text = "Entreprise : Aucune";
            if (heuresTextInternal != null) heuresTextInternal.text = "Heures : 0 heure / semaine";
            if (texteSalaireAnnuelBrut != null) texteSalaireAnnuelBrut.text = "Salaire brut : 0 € / an";
            return;
        }

        ApplyValue(experienceSlider, experienceValueText, donnees.experience);
        ApplyValue(fatigueSlider, fatigueValueText, donnees.fatigue);
        ApplyValue(burnoutSlider, burnoutValueText, donnees.burnout);

        // ◄ CONFIGURATION CRITIQUE : Synchronisation immédiate des textes avec les données chargées
        if (donnees.aEmploi)
        {
            if (entrepriseTextInternal != null)
                entrepriseTextInternal.text = "Entreprise : " + donnees.entreprise;

            if (heuresTextInternal != null)
                heuresTextInternal.text = "Heures : " + donnees.heuresSemaine + " heures / semaine";

            if (texteSalaireAnnuelBrut != null && gameData != null && gameData.joueur != null)
            {
                argent salaireAnnuel = gameData.joueur.salaire * 12f;
                texteSalaireAnnuelBrut.text = "Salaire brut : " + salaireAnnuel.ToString("N0") + " € / an";
            }
        }
        else
        {
            if (entrepriseTextInternal != null) entrepriseTextInternal.text = "Entreprise : Aucune";
            if (heuresTextInternal != null) heuresTextInternal.text = "Heures : 0 heure / semaine";
            if (texteSalaireAnnuelBrut != null) texteSalaireAnnuelBrut.text = "Salaire brut : 0 € / an";
        }
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