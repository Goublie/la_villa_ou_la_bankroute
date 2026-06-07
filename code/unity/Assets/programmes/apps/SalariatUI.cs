using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Structure représentant une offre d'emploi.
/// </summary>
[System.Serializable]
public class OffreEmploi
{
    public string nomEntreprise;
    public int salaireAnnuel;
    public int heuresSemaine;
    public int prestige; // Score sur 5
}

/// <summary>
/// Gère l'interface et la logique de la rubrique Salariat.
/// </summary>
public class SalariatUI : MonoBehaviour
{
    [Header("Données de Test")]
    public List<OffreEmploi> offresTest = new List<OffreEmploi>();

    [Header("Rubrique : Poste Actuel")]
    [SerializeField] private string entrepriseNom = "Entreprise Alpha";
    [SerializeField] private int ancienneteMois = 6;
    [SerializeField] private int salaireBrut = 2800;
    [SerializeField] private int heuresTravail = 35;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI txtEntreprise;
    [SerializeField] private TextMeshProUGUI txtAnciennete;
    [SerializeField] private TextMeshProUGUI txtSalaire;
    [SerializeField] private TextMeshProUGUI txtHeures;
    
    [SerializeField] private Slider jaugeSatisfaction;
    [SerializeField] private Image fillSatisfaction; // L'image de remplissage pour changer la couleur

    [Header("Rubrique : Actions Rapides")]
    [SerializeField] private Button btnTravaillerPlus;
    [SerializeField] private Button btnChercherEmploi;
    [SerializeField] private Button btnNegocierSalaire;
    [SerializeField] private Button btnNetworking;
    [SerializeField] private Button btnFormation;
    [SerializeField] private Button btnDemissionner;

    [Header("Fenêtre : Chercher un Emploi")]
    [SerializeField] private GameObject panelOffresEmploi;

    void Start()
    {
        InitialiserDonneesTest();
        ConfigurerBoutons();
        InitialiserAffichage();
        
        // On masque le panel de recherche par défaut
        if (panelOffresEmploi != null)
            panelOffresEmploi.SetActive(false);
    }

    private void InitialiserDonneesTest()
    {
        if (offresTest.Count == 0)
        {
            offresTest.Add(new OffreEmploi { nomEntreprise = "Tech Solutions", salaireAnnuel = 42000, heuresSemaine = 39, prestige = 4 });
            offresTest.Add(new OffreEmploi { nomEntreprise = "Global Services", salaireAnnuel = 35000, heuresSemaine = 35, prestige = 3 });
            offresTest.Add(new OffreEmploi { nomEntreprise = "Innovate Corp", salaireAnnuel = 50000, heuresSemaine = 40, prestige = 5 });
            offresTest.Add(new OffreEmploi { nomEntreprise = "Local Startup", salaireAnnuel = 30000, heuresSemaine = 35, prestige = 2 });
        }
    }

    private void ConfigurerBoutons()
    {
        if (btnChercherEmploi != null)
            btnChercherEmploi.onClick.AddListener(OuvrirRechercheEmploi);

        // Actions non implémentées pour le moment
        btnTravaillerPlus?.onClick.AddListener(() => Debug.Log("Action non implémentée : Travailler plus"));
        btnNegocierSalaire?.onClick.AddListener(() => Debug.Log("Action non implémentée : Négocier le salaire"));
        btnNetworking?.onClick.AddListener(() => Debug.Log("Action non implémentée : Faire du networking"));
        btnFormation?.onClick.AddListener(() => Debug.Log("Action non implémentée : Formation"));
        btnDemissionner?.onClick.AddListener(() => Debug.Log("Action non implémentée : Démissionner"));
    }

    private void InitialiserAffichage()
    {
        if (txtEntreprise != null) txtEntreprise.text = entrepriseNom;
        if (txtAnciennete != null) txtAnciennete.text = $"{ancienneteMois} mois";
        
        if (txtSalaire != null)
        {
            txtSalaire.text = $"{salaireBrut} € Brut";
            txtSalaire.color = Color.green; // Salaire affiché en vert
        }

        if (txtHeures != null) txtHeures.text = $"{heuresTravail}h / semaine";

        CalculerSatisfaction();
    }

    /// <summary>
    /// Calcule la satisfaction initiale. 
    /// Logique : augmente avec le salaire, baisse avec les heures de travail.
    /// </summary>
    private void CalculerSatisfaction()
    {
        // Calcul arbitraire pour illustrer la consigne
        float scoreSalaire = (salaireBrut / 5000f) * 100f;
        float malusHeures = (heuresTravail - 35) * 5f;
        float satisfaction = Mathf.Clamp(scoreSalaire - malusHeures + 50f, 0f, 100f);

        if (jaugeSatisfaction != null)
        {
            jaugeSatisfaction.value = satisfaction;
            CalculerCouleurJauge(satisfaction);
        }
    }

    /// <summary>
    /// Change la couleur de la jauge selon la valeur de satisfaction.
    /// </summary>
    private void CalculerCouleurJauge(float value)
    {
        if (fillSatisfaction == null) return;

        if (value < 35)
            fillSatisfaction.color = Color.red;
        else if (value < 65)
            fillSatisfaction.color = new Color(1f, 0.5f, 0f); // Orange
        else
            fillSatisfaction.color = Color.green;
    }

    /// <summary>
    /// Active le panel de recherche d'emploi.
    /// </summary>
    public void OuvrirRechercheEmploi()
    {
        if (panelOffresEmploi != null)
        {
            panelOffresEmploi.SetActive(true);
            AfficherOffres();
        }
    }

    /// <summary>
    /// Simule l'affichage des offres d'emploi dans la console.
    /// </summary>
    private void AfficherOffres()
    {
        Debug.Log("--- Liste des Offres d'Emploi Disponibles ---");
        foreach (var offre in offresTest)
        {
            Debug.Log($"Entreprise : {offre.nomEntreprise} | Salaire : {offre.salaireAnnuel}€/an | Heures : {offre.heuresSemaine}h | Prestige : {offre.prestige}/5");
        }
    }
}
