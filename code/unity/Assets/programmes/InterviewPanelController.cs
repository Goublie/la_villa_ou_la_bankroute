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

    [Header("Screen references")]
    public GameObject panelOffresEmploi;   // 'Panel_Offres_d'emploi'
    public GameObject panelPosteActuel;    // 'Panel_Poste8actuel'
    public GameObject panelActionsRapides; // 'Panel_Actions_Rapides'

    // Buttons inside this interview panel.
    private Button retourButton;
    private Button ouiButton;

    // Value TMP fields inside 'Panel_Poste8actuel'.
    private TMP_Text entrepriseValeur;
    private TMP_Text salaireValeur;
    private TMP_Text heuresValeur;

    private void Start()
    {
        // --- Locate this panel's buttons ---
        Transform retourTransform = transform.Find("Bouton_Retour");
        if (retourTransform != null) retourButton = retourTransform.GetComponent<Button>();

        Transform ouiTransform = transform.Find("Bouton_Oui");
        if (ouiTransform != null) ouiButton = ouiTransform.GetComponent<Button>();

        // --- Locate the dashboard value fields ---
        if (panelPosteActuel != null)
        {
            entrepriseValeur = FindValue(panelPosteActuel.transform, "Entreprise_Valeur");
            salaireValeur = FindValue(panelPosteActuel.transform, "Salaire_brut_Valeur");
            heuresValeur = FindValue(panelPosteActuel.transform, "Heures_Valeur");
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

        // Update the dashboard values.
        if (entrepriseValeur != null) entrepriseValeur.text = companyName;
        if (salaireValeur != null) salaireValeur.text = jobSalary;
        if (heuresValeur != null) heuresValeur.text = "35 heures / semaine";
    }

    private TMP_Text FindValue(Transform root, string childName)
    {
        Transform t = root.Find(childName);
        return t != null ? t.GetComponent<TMP_Text>() : null;
    }
}
