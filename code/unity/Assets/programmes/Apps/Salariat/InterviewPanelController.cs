using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controle un panel d'entretien et transmet l'acceptation d'offre au service Salariat.
/// </summary>
/// <remarks>
/// Ce composant conserve les references Unity du prefab et met a jour les panels visibles.
/// L'etat metier du poste est stocke dans <see cref="DonneesSalariat"/> via
/// <see cref="ServiceSalariat"/>, afin que la Banque et le passage mensuel lisent la meme source.
/// </remarks>
public class InterviewPanelController : MonoBehaviour
{
    [Header("Job data")]
    public string companyName;
    public string jobSalary;

    [Tooltip("Reference vers les donnees globales du jeu")]
    public GameData gameData;

    [Header("Screen references")]
    public GameObject panelOffresEmploi;
    public GameObject panelPosteActuel;
    public GameObject panelActionsRapides;
    public GameObject panelPerformanceEmploye;
    public GameObject panelRelationnel;

    [Header("Employee performance")]
    public EmployeePerformanceController performanceController;

    [Header("Job satisfaction")]
    public JobSatisfactionController satisfactionController;
    [Range(1, 3)] public int stressStars;
    [Range(1, 3)] public int prestigeStars;
    [Range(1, 3)] public int equilibreStars;

    private Button retourButton;
    private Button ouiButton;

    private TMP_Text entrepriseText;
    private TMP_Text salaireText;
    private TMP_Text heuresText;

    private void Start()
    {
        Transform retourTransform = transform.Find("Bouton_Retour");
        if (retourTransform != null)
        {
            retourButton = retourTransform.GetComponent<Button>();
        }

        Transform ouiTransform = transform.Find("Bouton_Oui");
        if (ouiTransform != null)
        {
            ouiButton = ouiTransform.GetComponent<Button>();
        }

        if (panelPosteActuel != null)
        {
            entrepriseText = FindText(panelPosteActuel.transform, "Entreprise");
            salaireText = FindText(panelPosteActuel.transform, "Salaire_brut");
            heuresText = FindText(panelPosteActuel.transform, "Heures");
        }

        if (retourButton != null)
        {
            retourButton.onClick.AddListener(OnRetourClicked);
        }

        if (ouiButton != null)
        {
            ouiButton.onClick.AddListener(OnOuiClicked);
        }
    }

    private void OnRetourClicked()
    {
        gameObject.SetActive(false);
        if (panelOffresEmploi != null)
        {
            panelOffresEmploi.SetActive(true);
        }
    }

    private void OnOuiClicked()
    {
        gameObject.SetActive(false);
        AfficherTableauBord(true);

        if (satisfactionController != null)
        {
            satisfactionController.UpdateSatisfaction(
                stressStars,
                prestigeStars,
                equilibreStars);
        }

        GameData donneesJeu = ResoudreGameData();
        if (donneesJeu != null && donneesJeu.joueur != null)
        {
            donneesJeu.joueur.InitialiserSiNecessaire();
            int salaireMensuelCentimes = GetMonthlySalaryCentimes(jobSalary);

            new ServiceSalariat(donneesJeu.joueur.salariat, donneesJeu.joueur)
                .AccepterPoste(
                    companyName,
                    salaireMensuelCentimes,
                    35,
                    stressStars,
                    prestigeStars,
                    equilibreStars);
        }

        if (performanceController != null)
        {
            performanceController.StartNewJob(stressStars, 35);
        }

        if (entrepriseText != null)
        {
            entrepriseText.text = "Entreprise : " + companyName;
        }

        if (salaireText != null)
        {
            // ◄ FIX FORMATTAGE : On enlève les résidus (ex: "€", "/ an") de la string originale pour la recomposer proprement
            string cleanSalary = jobSalary.Replace("€", "").Replace("EUR", "").Replace("/ an", "").Replace(",", " ").Trim();
            salaireText.text = "Salaire brut : " + cleanSalary + " € / an";
        }

        if (heuresText != null)
        {
            heuresText.text = "Heures : 35 heures / semaine";
        }
    }

    /// <summary>
    /// Convertit un salaire annuel affiche en salaire mensuel exprime en centimes.
    /// </summary>
    /// <param name="rawSalary">Texte de salaire, par exemple "72 000 EUR / an".</param>
    /// <returns>Salaire mensuel en centimes, ou 0 si la valeur n'est pas lisible.</returns>
    private int GetMonthlySalaryCentimes(string rawSalary)
    {
        string chiffres = string.Empty;
        foreach (char caractere in rawSalary)
        {
            if (char.IsDigit(caractere))
            {
                chiffres += caractere;
            }
        }

        if (int.TryParse(chiffres, out int yearlySalary))
        {
            return (yearlySalary / 12) * 100;
        }

        Debug.LogWarning(
            "Impossible de convertir le salaire de l'offre -> " + rawSalary);
        return 0;
    }

    private TMP_Text FindText(Transform root, string childName)
    {
        Transform t = root.Find(childName);
        return t != null ? t.GetComponent<TMP_Text>() : null;
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

        if (panelPerformanceEmploye != null)
        {
            panelPerformanceEmploye.SetActive(visible);
        }

        if (panelRelationnel != null)
        {
            panelRelationnel.SetActive(visible);
        }
    }

    private GameData ResoudreGameData()
    {
        if (gameData != null)
        {
            return gameData;
        }

        if (performanceController != null &&
            performanceController.gameData != null)
        {
            gameData = performanceController.gameData;
            return gameData;
        }

        ActionPlay actionPlay = Object.FindFirstObjectByType<ActionPlay>();
        if (actionPlay != null)
        {
            gameData = actionPlay.gameData;
        }

        return gameData;
    }
}