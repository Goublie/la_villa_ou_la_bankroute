using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Logic_RechercheEmploi : MonoBehaviour
{
    // A single job offer entry.
    private struct JobOffer
    {
        public string company;   // Company name & location
        public string title;     // Job title
        public string salary;    // Salary (plain, colored via rich text)

        public JobOffer(string company, string title, string salary)
        {
            this.company = company;
            this.title = title;
            this.salary = salary;
        }
    }

    private Button chercherEmploiButton;
    private GameObject panelOffresEmploi;
    private GameObject panelActionsRapides;
    private GameObject panelPosteActuel;

    private Transform listeOffres;     // Container with the VerticalLayoutGroup
    private GameObject jobOfferTemplate; // The 'JobOffer_Item' used as a template

    private bool populated;

    private void Start()
    {
        // --- Locate the button (inside Panel_Actions_Rapides) ---
        Transform buttonTransform = transform.Find("Panel_Actions_Rapides/Chercher_emploi");
        if (buttonTransform != null)
        {
            chercherEmploiButton = buttonTransform.GetComponent<Button>();
        }

        // --- Locate the three main panels ---
        panelActionsRapides = FindChildGameObject("Panel_Actions_Rapides");
        panelOffresEmploi = FindChildGameObject("Panel_Offres_d'emploi");
        // The current-job panel is named 'Panel_Poste8actuel' in the prefab; match by prefix to be safe.
        panelPosteActuel = FindChildByPrefix("Panel_Poste");

        // --- Cache the list container and the row template ---
        if (panelOffresEmploi != null)
        {
            listeOffres = panelOffresEmploi.transform.Find("Liste_Offres");
            if (listeOffres != null)
            {
                Transform templateTransform = listeOffres.Find("JobOffer_Item");
                if (templateTransform != null)
                {
                    jobOfferTemplate = templateTransform.gameObject;
                    // Hide the template; clones are activated instead.
                    jobOfferTemplate.SetActive(false);
                }
            }
        }

        // --- The offers panel starts hidden ---
        if (panelOffresEmploi != null)
        {
            panelOffresEmploi.SetActive(false);
        }

        // --- Wire the button ---
        if (chercherEmploiButton != null)
        {
            chercherEmploiButton.onClick.AddListener(OnChercherEmploiClicked);
        }
    }

    private void OnChercherEmploiClicked()
    {
        // Hide the other two panels so the offers list takes the full screen.
        if (panelActionsRapides != null) panelActionsRapides.SetActive(false);
        if (panelPosteActuel != null) panelPosteActuel.SetActive(false);

        // Show the offers panel.
        if (panelOffresEmploi != null) panelOffresEmploi.SetActive(true);

        // Fill the list once.
        PopulateOffers();
    }

    private void PopulateOffers()
    {
        if (populated || listeOffres == null || jobOfferTemplate == null) return;

        JobOffer[] offers =
        {
            new JobOffer("Startup IA - Paris", "Lead Developer", "€72,000 / an"),
            new JobOffer("Banque Tech - Lyon", "Backend Engineer", "€65,000 / an"),
            new JobOffer("BigTech Corp - Remote", "Senior Software Engineer", "€95,000 / an"),
        };

        foreach (JobOffer offer in offers)
        {
            GameObject item = Instantiate(jobOfferTemplate, listeOffres);
            item.name = "JobOffer_" + offer.company;
            item.SetActive(true);

            SetText(item.transform, "InfoColumn/CompanyName", offer.company);
            SetText(item.transform, "InfoColumn/JobTitle", offer.title);
            // Salary in green via rich text tag.
            SetText(item.transform, "InfoColumn/Salary", "<color=green>" + offer.salary + "</color>");
        }

        populated = true;
    }

    private void SetText(Transform root, string path, string value)
    {
        Transform t = root.Find(path);
        if (t == null) return;
        TextMeshProUGUI tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmp.text = value;
    }

    private GameObject FindChildGameObject(string childName)
    {
        Transform t = transform.Find(childName);
        return t != null ? t.gameObject : null;
    }

    private GameObject FindChildByPrefix(string prefix)
    {
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith(prefix))
                return child.gameObject;
        }
        return null;
    }
}
