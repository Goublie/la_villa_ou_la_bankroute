using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls a single interview panel. The "Oui" button accepts the job (updates the
/// dashboard 'Panel_Poste8actuel' and returns to the main screen) while the "Retour"
/// button cancels and goes back to the job-offers list.
/// </summary>
public class InterviewPanelController : MonoBehaviour
{
    [Header("Job data")]
    public string companyName;
    public string jobSalary;

    [Tooltip("Référence vers les données globales du jeu")]
    public GameData gameData;

    [Header("Screen references")]
    public GameObject panelOffresEmploi;   // 'Panel_Offres_d'emploi'
    public GameObject panelPosteActuel;    // 'Panel_Poste8actuel'
    public GameObject panelActionsRapides; // 'Panel_Actions_Rapides'

    [Header("Job satisfaction")]
    public JobSatisfactionController satisfactionController;
    [Range(1, 3)] public int stressStars;
    [Range(1, 3)] public int prestigeStars;
    [Range(1, 3)] public int equilibreStars;

    // Buttons inside this interview panel.
    private Button retourButton;
    private Button ouiButton;

    // Dashboard label TMP fields inside 'Panel_Poste8actuel'.
    private TMP_Text entrepriseText;
    private TMP_Text salaireText;
    private TMP_Text heuresText;

    private void Start()
    {
        // --- Locate this panel's buttons ---
        Transform retourTransform = transform.Find("Bouton_Retour");
        if (retourTransform != null) retourButton = retourTransform.GetComponent<Button>();

        Transform ouiTransform = transform.Find("Bouton_Oui");
        if (ouiTransform != null) ouiButton = ouiTransform.GetComponent<Button>();

        // --- Locate the dashboard text fields inside 'Panel_Poste8actuel' ---
        if (panelPosteActuel != null)
        {
            entrepriseText = FindText(panelPosteActuel.transform, "Entreprise");
            salaireText = FindText(panelPosteActuel.transform, "Salaire_brut");
            heuresText = FindText(panelPosteActuel.transform, "Heures");
        }

        // --- Wire buttons ---
        if (retourButton != null) retourButton.onClick.AddListener(OnRetourClicked);
        if (ouiButton != null) ouiButton.onClick.AddListener(OnOuiClicked);
    }

    private void OnRetourClicked()
    {
        // Cancel the interview: go back to the offers list.
        gameObject.SetActive(false);
        if (panelOffresEmploi != null) panelOffresEmploi.SetActive(true);
    }

    private void OnOuiClicked()
    {
        // Accept the job: close the interview and return to the dashboard.
        gameObject.SetActive(false);
        if (panelPosteActuel != null) panelPosteActuel.SetActive(true);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(true);

        // Update the job-satisfaction bar based on this offer's parameters.
        if (satisfactionController != null)
            satisfactionController.UpdateSatisfaction(stressStars, prestigeStars, equilibreStars);

        // Update the dashboard texts (label prefix preserved for readability).
        if (entrepriseText != null) entrepriseText.text = "Entreprise : " + companyName;
        if (salaireText != null) salaireText.text = "Salaire brut : " + jobSalary;
        if (heuresText != null) heuresText.text = "Heures : 35 heures / semaine";

        // --- MISE À JOUR DE LA BANQUE (GAMEDATA) ---
        // --- MISE À JOUR DE LA BANQUE (GAMEDATA) ---
        if (gameData != null)
        {
            int monthlySalary = GetMonthlySalary(jobSalary);
            // On multiplie par 100 car la banque fonctionne en centimes !
            gameData.salaire = new argent(monthlySalary * 100);
            Debug.Log("Nouveau salaire mensuel de " + monthlySalary + " enregistré dans GameData pour " + companyName);
        }
    }

    /// <summary>
    /// Prend le salaire sous forme de texte (ex: "€72,000 / an") et le convertit en salaire mensuel (int).
    /// </summary>
    private int GetMonthlySalary(string rawSalary)
    {
        // On nettoie la chaîne pour ne garder que les chiffres
        string cleanString = rawSalary.Replace("€", "").Replace(",", "").Replace(" / an", "").Trim();

        // On convertit le texte propre en nombre
        if (int.TryParse(cleanString, out int yearlySalary))
        {
            return yearlySalary / 12;
        }
        else
        {
            Debug.LogWarning("Erreur : Impossible de convertir le salaire de l'offre -> " + rawSalary);
            return 0;
        }
    }

    private TMP_Text FindText(Transform root, string childName)
    {
        Transform t = root.Find(childName);
        return t != null ? t.GetComponent<TMP_Text>() : null;
    }
}