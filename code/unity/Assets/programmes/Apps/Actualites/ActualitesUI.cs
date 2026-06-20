using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Projection d'une publication persistante dans le tableau Actualites.
/// </summary>
[System.Serializable]
public class ActualiteItem
{
    public string objetId;
    public string date;
    public string categorie;
    public string titre;
    public string impact;
    public string nouveau;
    [TextArea(3, 10)] public string description;
    public bool aCourbe;
    public Color couleurCategorie = Color.white;
}

/// <summary>
/// Affiche l'historique Actualites sans tirer ni modifier d'evenement.
/// </summary>
public class ActualitesUI : MonoBehaviour
{
    [Header("Donnees des Actualites")]
    public List<ActualiteItem> actualitesList = new List<ActualiteItem>();
    [SerializeField] private GameData gameData;

    [Header("Navigation / Onglets")]
    [SerializeField] private Button btnDernieresActualites;
    [SerializeField] private Button btnCategoriesTab;

    [Header("Panneau Categories (Gauche)")]
    [SerializeField] private Transform categoriesContainer;
    [SerializeField] private GameObject prefabCategoryBtn;

    [Header("Tableau des Actualites (Centre/Droit)")]
    [SerializeField] private Transform newsRowsContainer;
    [SerializeField] private GameObject prefabNewsRow;
    [SerializeField] private Tableau tableauActualites;

    [Header("Panneau de Details (Bas)")]
    [SerializeField] private TextMeshProUGUI txtDetailTitre;
    [SerializeField] private TextMeshProUGUI txtDetailDescription;
    [SerializeField] private GameObject panelCourbe;

    private string categorieFiltre = string.Empty;
    private readonly List<GameObject> activeRows = new List<GameObject>();
    private readonly List<GameObject> categoryButtons = new List<GameObject>();
    private bool initialisee;

    private void OnEnable()
    {
        ActionPlay.OnMoisPasse += RafraichirDepuisHistorique;
        if (initialisee)
        {
            ConfigurerNavigation();
            RafraichirDepuisHistorique();
        }
    }

    private void OnDisable()
    {
        ActionPlay.OnMoisPasse -= RafraichirDepuisHistorique;
        RetirerNavigation();
    }

    private IEnumerator Start()
    {
        InitialiserReferences();
        ConfigurerNavigation();
        initialisee = true;

        // Tableau initialise et vide ses lignes dans Start. Attendre une frame
        // garantit que l'historique est ecrit apres cette initialisation.
        yield return null;
        RafraichirDepuisHistorique();
    }

    /// <summary>
    /// Reconstruit la liste visible depuis les seules publications persistantes.
    /// </summary>
    /// <remarks>
    /// Cette methode est sans effet de bord metier et peut etre appelee apres
    /// une ouverture de fenetre ou un rechargement de scene.
    /// </remarks>
    public void RafraichirDepuisHistorique()
    {
        InitialiserReferences();
        actualitesList.Clear();
        if (gameData?.evenements != null)
        {
            gameData.evenements.InitialiserSiNecessaire();
            List<PublicationActualite> publications =
                CopierPublicationsTriees(gameData.evenements.publications);
            foreach (PublicationActualite publication in publications)
            {
                actualitesList.Add(
                    ConstruireItem(publication, gameData.evenements));
            }
        }

        GenererBoutonsCategories();
        AfficherActualites();
        SelectionnerPremierElementVisible();
    }

    /// <summary>
    /// Applique un filtre de presentation sans modifier l'historique.
    /// </summary>
    public void FiltrerParCategorie(string categorie)
    {
        categorieFiltre = categorie ?? string.Empty;
        AfficherActualites();
        SelectionnerPremierElementVisible();
    }

    private void InitialiserReferences()
    {
        if (gameData == null)
        {
            ActionPlay actionPlay = Object.FindFirstObjectByType<ActionPlay>(
                FindObjectsInactive.Include);
            gameData = actionPlay != null ? actionPlay.gameData : null;
        }

        if (tableauActualites == null)
        {
            tableauActualites = GetComponentInChildren<Tableau>(true);
        }

        if (btnDernieresActualites == null)
        {
            btnDernieresActualites = TrouverBouton("BoutonToutes");
        }
    }

    private void ConfigurerNavigation()
    {
        RetirerNavigation();
        if (btnDernieresActualites != null)
        {
            btnDernieresActualites.onClick.AddListener(AfficherToutes);
        }

        if (btnCategoriesTab != null)
        {
            btnCategoriesTab.onClick.AddListener(AfficherToutes);
        }
    }

    private void RetirerNavigation()
    {
        if (btnDernieresActualites != null)
        {
            btnDernieresActualites.onClick.RemoveListener(AfficherToutes);
        }

        if (btnCategoriesTab != null)
        {
            btnCategoriesTab.onClick.RemoveListener(AfficherToutes);
        }
    }

    private void AfficherToutes()
    {
        FiltrerParCategorie(string.Empty);
    }

    private void GenererBoutonsCategories()
    {
        foreach (GameObject bouton in categoryButtons)
        {
            if (bouton != null)
            {
                Destroy(bouton);
            }
        }
        categoryButtons.Clear();

        if (categoriesContainer == null || prefabCategoryBtn == null)
        {
            return;
        }

        HashSet<string> categories = new HashSet<string>();
        foreach (ActualiteItem item in actualitesList)
        {
            if (!string.IsNullOrWhiteSpace(item.categorie))
            {
                categories.Add(item.categorie);
            }
        }

        foreach (string categorie in categories)
        {
            GameObject objet = Instantiate(
                prefabCategoryBtn,
                categoriesContainer,
                false);
            objet.name = "Btn_Cat_" + categorie;
            objet.SetActive(true);
            TextMeshProUGUI texte =
                objet.GetComponentInChildren<TextMeshProUGUI>();
            if (texte != null)
            {
                texte.text = categorie;
            }

            Button bouton = objet.GetComponent<Button>();
            if (bouton != null)
            {
                string filtre = categorie;
                bouton.onClick.AddListener(
                    () => FiltrerParCategorie(filtre));
            }
            categoryButtons.Add(objet);
        }
    }

    private void AfficherActualites()
    {
        if (tableauActualites != null && tableauActualites.tableau != null)
        {
            tableauActualites.Vider();
            if (tableauActualites is TableauScroll)
            {
                // TableauScroll replace chaque ajout en tete. Parcourir du plus
                // ancien au plus recent conserve l'ordre visuel attendu.
                for (int index = actualitesList.Count - 1; index >= 0; index--)
                {
                    AjouterAuTableauSiVisible(actualitesList[index]);
                }
            }
            else
            {
                foreach (ActualiteItem item in actualitesList)
                {
                    AjouterAuTableauSiVisible(item);
                }
            }
            return;
        }

        AfficherAvecAncienPrefab();
    }

    private void AjouterAuTableauSiVisible(ActualiteItem item)
    {
        if (!EstVisible(item))
        {
            return;
        }

        tableauActualites.Add(
            item.date,
            item.categorie,
            item.titre,
            item.impact,
            item.nouveau);
    }

    private void AfficherAvecAncienPrefab()
    {
        foreach (GameObject ligne in activeRows)
        {
            if (ligne != null)
            {
                Destroy(ligne);
            }
        }
        activeRows.Clear();

        if (newsRowsContainer == null || prefabNewsRow == null)
        {
            return;
        }

        foreach (ActualiteItem item in actualitesList)
        {
            if (!EstVisible(item))
            {
                continue;
            }

            GameObject ligneObjet = Instantiate(
                prefabNewsRow,
                newsRowsContainer,
                false);
            ligneObjet.name = "Row_Actualite_" + item.objetId;
            ligneObjet.SetActive(true);
            Ligne ligne = ligneObjet.GetComponent<Ligne>();
            ligne?.Set(
                item.date,
                item.categorie,
                item.titre,
                item.impact,
                item.nouveau);

            Button bouton = ligneObjet.GetComponent<Button>() ??
                ligneObjet.AddComponent<Button>();
            ActualiteItem selection = item;
            bouton.onClick.AddListener(() => AfficherDetails(selection));
            activeRows.Add(ligneObjet);
        }
    }

    private bool EstVisible(ActualiteItem item)
    {
        return item != null &&
            (string.IsNullOrEmpty(categorieFiltre) ||
                item.categorie == categorieFiltre);
    }

    private void SelectionnerPremierElementVisible()
    {
        foreach (ActualiteItem item in actualitesList)
        {
            if (EstVisible(item))
            {
                AfficherDetails(item);
                return;
            }
        }

        ViderDetails();
    }

    private void AfficherDetails(ActualiteItem item)
    {
        if (txtDetailTitre != null)
        {
            txtDetailTitre.text = item.titre;
        }

        if (txtDetailDescription != null)
        {
            txtDetailDescription.text = item.description;
        }

        if (panelCourbe != null)
        {
            panelCourbe.SetActive(item.aCourbe);
        }
    }

    private void ViderDetails()
    {
        if (txtDetailTitre != null)
        {
            txtDetailTitre.text = string.Empty;
        }

        if (txtDetailDescription != null)
        {
            txtDetailDescription.text = string.Empty;
        }

        if (panelCourbe != null)
        {
            panelCourbe.SetActive(false);
        }
    }

    private Button TrouverBouton(string nom)
    {
        Button[] boutons = GetComponentsInChildren<Button>(true);
        foreach (Button bouton in boutons)
        {
            if (bouton != null && bouton.name == nom)
            {
                return bouton;
            }
        }

        return null;
    }

    private static List<PublicationActualite> CopierPublicationsTriees(
        List<PublicationActualite> publications)
    {
        List<PublicationActualite> copies =
            new List<PublicationActualite>();
        if (publications != null)
        {
            foreach (PublicationActualite publication in publications)
            {
                if (publication != null)
                {
                    copies.Add(publication.Copier());
                }
            }
        }

        copies.Sort((gauche, droite) =>
        {
            int comparaisonMois =
                droite.moisPublication.CompareTo(gauche.moisPublication);
            return comparaisonMois != 0
                ? comparaisonMois
                : droite.ordrePublication.CompareTo(
                    gauche.ordrePublication);
        });
        return copies;
    }

    private static ActualiteItem ConstruireItem(
        PublicationActualite publication,
        DonneesEvenements donnees)
    {
        bool rumeur = publication.type == TypePublicationActualite.Rumeur;
        RumeurPartie rumeurPartie = rumeur
            ? donnees.rumeurs.Find(
                element => element != null &&
                    element.id == publication.objetId)
            : null;
        string etat = rumeur
            ? FormaterEtatRumeur(publication.etatRumeur)
            : "Confirme";
        string fiabilite = rumeurPartie != null
            ? FormaterFiabilite(rumeurPartie.probabiliteConfirmation)
            : string.Empty;
        string description = publication.texte +
            "\n\nSource : " + publication.sourceNom +
            "\nCategorie : " + publication.categorie;
        if (rumeur)
        {
            description += "\nFiabilite : " + fiabilite +
                "\nEtat : " + etat;
        }
        else
        {
            description += "\nImportance : " + publication.importance;
        }

        return new ActualiteItem
        {
            objetId = publication.objetId,
            date = FormaterMois(publication.moisPublication),
            categorie = rumeur ? "Rumeur" : "Evenement confirme",
            titre = publication.titre,
            impact = rumeur ? publication.sourceNom : publication.importance,
            nouveau = etat,
            description = description,
            aCourbe = false,
            couleurCategorie = rumeur
                ? new Color(0.9f, 0.55f, 0.15f)
                : new Color(0.2f, 0.7f, 0.2f)
        };
    }

    private static string FormaterEtatRumeur(EtatRumeur etat)
    {
        switch (etat)
        {
            case EtatRumeur.Confirmee:
                return "Confirmee";
            case EtatRumeur.Invalidee:
                return "Non confirmee";
            default:
                return "En attente";
        }
    }

    private static string FormaterFiabilite(float probabilite)
    {
        string niveau = probabilite >= 0.75f
            ? "elevee"
            : probabilite >= 0.5f
                ? "moyenne"
                : "faible";
        return niveau + " (" + Mathf.RoundToInt(probabilite * 100f) + " %)";
    }

    private static string FormaterMois(int indexMois)
    {
        int indexDepuisJanvier = 6 + Mathf.Max(0, indexMois);
        int mois = indexDepuisJanvier % 12 + 1;
        int annee = 2026 + indexDepuisJanvier / 12;
        return mois.ToString("D2") + "/" + annee;
    }
}
