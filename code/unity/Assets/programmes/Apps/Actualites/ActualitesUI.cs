using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
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
    public string type;
    public string categorie;
    public string titre;
    public string impact;
    public string nouveau;
    [TextArea(2, 8)] public string effets;
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
    [SerializeField] private HUDManager hudManager;

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
    [SerializeField] private TextMeshProUGUI txtDetailEffets;
    [SerializeField] private GameObject panelCourbe;

    private static readonly string[] CategoriesMetier =
    {
        CategoriesEvenements.Boursiers,
        CategoriesEvenements.Personnels,
        CategoriesEvenements.Professionnels
    };

    private string categorieFiltre = string.Empty;
    private readonly List<GameObject> activeRows = new List<GameObject>();
    private readonly List<Button> categoryButtons = new List<Button>();
    private readonly Dictionary<LigneSelectable, ActualiteItem> itemsParLigne =
        new Dictionary<LigneSelectable, ActualiteItem>();
    private TableauScrollSelectable tableauSelectable;
    private bool selectionTableauConfiguree;
    private bool avertissementDateAffiche;
    private bool initialisee;

    private void OnEnable()
    {
        ActionPlay.OnMoisPasse += RafraichirDepuisHistorique;
        if (initialisee)
        {
            ConfigurerNavigation();
            ConfigurerSelectionTableau();
            RafraichirDepuisHistorique();
        }
    }

    private void OnDisable()
    {
        ActionPlay.OnMoisPasse -= RafraichirDepuisHistorique;
        RetirerNavigation();
        RetirerSelectionTableau();
    }

    private IEnumerator Start()
    {
        InitialiserReferences();
        ConfigurerNavigation();
        ConfigurerSelectionTableau();
        ConfigurerPresentation();
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

        ConfigurerPresentation();
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
            ActionPlay actionPlay = UnityEngine.Object.FindFirstObjectByType<ActionPlay>(
                FindObjectsInactive.Include);
            gameData = actionPlay != null ? actionPlay.gameData : null;
        }

        if (tableauActualites == null)
        {
            tableauActualites = GetComponentInChildren<Tableau>(true);
        }

        tableauSelectable = tableauActualites as TableauScrollSelectable;

        if (hudManager == null)
        {
            hudManager = UnityEngine.Object.FindFirstObjectByType<HUDManager>(
                FindObjectsInactive.Include);
        }

        if (btnDernieresActualites == null)
        {
            btnDernieresActualites = TrouverBouton("BoutonToutes");
        }

        if (txtDetailEffets == null)
        {
            TextMeshProUGUI[] textes =
                GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI texte in textes)
            {
                if (texte != null && texte.name == "Effets")
                {
                    txtDetailEffets = texte;
                    break;
                }
            }
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
        if (categoriesContainer == null)
        {
            return;
        }

        if (categoryButtons.Count == 0)
        {
            Button[] boutons = categoriesContainer.GetComponentsInChildren<Button>(
                true);
            foreach (Button bouton in boutons)
            {
                if (bouton != null && bouton != btnDernieresActualites)
                {
                    categoryButtons.Add(bouton);
                }
            }
        }

        HashSet<string> categories = new HashSet<string>();
        foreach (ActualiteItem item in actualitesList)
        {
            if (!string.IsNullOrWhiteSpace(item.categorie))
            {
                categories.Add(item.categorie);
            }
        }

        Button modele = categoryButtons.Count > 0
            ? categoryButtons[0]
            : prefabCategoryBtn != null
                ? prefabCategoryBtn.GetComponent<Button>()
                : null;
        while (categoryButtons.Count < CategoriesMetier.Length && modele != null)
        {
            Button clone = Instantiate(modele, modele.transform.parent, false);
            categoryButtons.Add(clone);
        }

        for (int index = 0; index < categoryButtons.Count; index++)
        {
            Button bouton = categoryButtons[index];
            if (bouton == null)
            {
                continue;
            }

            if (index >= CategoriesMetier.Length)
            {
                bouton.gameObject.SetActive(false);
                continue;
            }

            string categorie = CategoriesMetier[index];
            bouton.name = "BoutonCategorie" + categorie;
            bouton.onClick.RemoveAllListeners();
            bouton.onClick.AddListener(() => FiltrerParCategorie(categorie));
            bouton.gameObject.SetActive(categories.Contains(categorie));

            TextMeshProUGUI texte =
                bouton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (texte != null)
            {
                texte.text = categorie;
            }
        }
    }

    private void AfficherActualites()
    {
        if (tableauActualites != null && tableauActualites.tableau != null)
        {
            itemsParLigne.Clear();
            tableauSelectable?.ReinitialiserSelection();
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

        object[] valeurs =
        {
            item.date,
            item.type,
            item.titre,
            item.impact,
            item.nouveau
        };

        if (tableauSelectable != null)
        {
            LigneSelectable ligne =
                tableauSelectable.AjouterEtRetournerLigne(valeurs);
            if (ligne != null)
            {
                itemsParLigne[ligne] = item;
                ConfigurerLigne(ligne);
            }
            return;
        }

        tableauActualites.Add(valeurs);
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
                item.type,
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
        if (tableauSelectable != null && itemsParLigne.Count > 0)
        {
            LigneSelectable premiereLigne = null;
            int premierIndex = int.MaxValue;
            foreach (KeyValuePair<LigneSelectable, ActualiteItem> association
                in itemsParLigne)
            {
                LigneSelectable ligne = association.Key;
                if (ligne == null)
                {
                    continue;
                }

                int index = ligne.transform.GetSiblingIndex();
                if (index < premierIndex)
                {
                    premierIndex = index;
                    premiereLigne = ligne;
                }
            }

            if (premiereLigne != null)
            {
                tableauSelectable.Selectionner(premiereLigne);
                return;
            }
        }

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

        if (txtDetailEffets != null)
        {
            txtDetailEffets.text =
                "<b>Effets sur la partie</b>\n" + item.effets;
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

        if (txtDetailEffets != null)
        {
            txtDetailEffets.text = "<b>Effets sur la partie</b>";
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

    private ActualiteItem ConstruireItem(
        PublicationActualite publication,
        DonneesEvenements donnees)
    {
        bool rumeur = publication.type == TypePublicationActualite.Rumeur;
        RumeurPartie rumeurPartie = rumeur
            ? donnees.rumeurs.Find(
                element => element != null &&
                    element.id == publication.objetId)
            : null;
        EvenementConfirmePartie evenementConfirme = !rumeur
            ? donnees.evenementsConfirmes.Find(
                element => element != null &&
                    element.rumeurId == publication.objetId)
            : null;
        string etat = rumeur
            ? FormaterEtatRumeur(publication.etatRumeur)
            : "Confirmé";
        string fiabilite = rumeurPartie != null
            ? FormaterFiabilite(rumeurPartie.probabiliteConfirmation)
            : string.Empty;
        string description = publication.texte +
            "\n\nSource : " + publication.sourceNom +
            "\nCatégorie : " + publication.categorie;
        if (rumeur)
        {
            description += "\nFiabilité : " + fiabilite +
                "\nÉtat : " + etat;
        }
        else
        {
            description += "\nImportance : " + publication.importance;
        }

        return new ActualiteItem
        {
            objetId = publication.objetId,
            date = FormaterMois(publication.moisPublication),
            type = rumeur ? "Rumeur" : "Événement confirmé",
            categorie = publication.categorie,
            titre = publication.titre,
            impact = rumeur ? publication.sourceNom : publication.importance,
            nouveau = etat,
            effets = ConstruireEffets(
                rumeur,
                publication.etatRumeur,
                evenementConfirme),
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
                return "Confirmée";
            case EtatRumeur.Invalidee:
                return "Non confirmée";
            default:
                return "En attente";
        }
    }

    private static string FormaterFiabilite(float probabilite)
    {
        string niveau = probabilite >= 0.75f
            ? "élevée"
            : probabilite >= 0.5f
                ? "moyenne"
                : "faible";
        return niveau + " (" + Mathf.RoundToInt(probabilite * 100f) + " %)";
    }

    private string FormaterMois(int indexMois)
    {
        string dateHud = hudManager != null && hudManager.texteMois != null
            ? hudManager.texteMois.text
            : string.Empty;
        DateTime dateActuelle;
        if (DateTime.TryParseExact(
            dateHud?.Trim(),
            "MM/yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out dateActuelle))
        {
            int moisActuel = gameData != null
                ? Mathf.Max(0, gameData.nombreMoisPasses)
                : 0;
            return dateActuelle
                .AddMonths(Mathf.Max(0, indexMois) - moisActuel)
                .ToString("MM/yyyy", CultureInfo.InvariantCulture);
        }

        if (!avertissementDateAffiche)
        {
            avertissementDateAffiche = true;
            Debug.LogWarning(
                "[Actualites] Date HUD indisponible : affichage de l'index mensuel.",
                this);
        }
        return "Mois " + (Mathf.Max(0, indexMois) + 1);
    }

    private static string ConstruireEffets(
        bool rumeur,
        EtatRumeur etatRumeur,
        EvenementConfirmePartie evenement)
    {
        if (rumeur)
        {
            if (etatRumeur == EtatRumeur.Invalidee)
            {
                return "Rumeur non confirmée : aucun effet sur la partie.";
            }

            if (etatRumeur == EtatRumeur.EnAttente)
            {
                return "Aucun effet tant que cette rumeur n'est pas confirmée.";
            }

            return "Rumeur confirmée : les effets sont détaillés dans la " +
                "publication d'événement associée.";
        }

        if (evenement == null ||
            evenement.impacts == null ||
            evenement.impacts.Count == 0)
        {
            return "Aucun effet déclaré pour cet événement.";
        }

        StringBuilder texte = new StringBuilder();
        foreach (ImpactDefinitionEvenement impact in evenement.impacts)
        {
            if (impact == null)
            {
                continue;
            }

            if (texte.Length > 0)
            {
                texte.AppendLine();
            }

            float pourcentage = impact.variation * 100f;
            texte.Append("• ")
                .Append(impact.actif)
                .Append(" : ")
                .Append(pourcentage >= 0f ? "+" : string.Empty)
                .Append(pourcentage.ToString(
                    "0.#",
                    CultureInfo.InvariantCulture))
                .Append(" %");
        }

        return texte.Length > 0
            ? texte.ToString()
            : "Aucun effet déclaré pour cet événement.";
    }

    private void ConfigurerSelectionTableau()
    {
        InitialiserReferences();
        if (tableauSelectable == null || selectionTableauConfiguree)
        {
            return;
        }

        if (tableauSelectable.OnSelectionChanged == null)
        {
            tableauSelectable.OnSelectionChanged =
                new UnityEngine.Events.UnityEvent<LigneSelectable>();
        }

        tableauSelectable.OnSelectionChanged.AddListener(
            GererSelectionLigne);
        selectionTableauConfiguree = true;
    }

    private void RetirerSelectionTableau()
    {
        if (tableauSelectable != null && selectionTableauConfiguree)
        {
            tableauSelectable.OnSelectionChanged.RemoveListener(
                GererSelectionLigne);
        }
        selectionTableauConfiguree = false;
    }

    private void GererSelectionLigne(LigneSelectable ligne)
    {
        ActualiteItem item;
        if (ligne != null && itemsParLigne.TryGetValue(ligne, out item))
        {
            AfficherDetails(item);
        }
    }

    private void ConfigurerPresentation()
    {
        if (tableauActualites != null)
        {
            tableauActualites.nombreColonnes = 5;
            tableauActualites.largeursColonnes = new List<float>
            {
                115f,
                190f,
                -1f,
                285f,
                175f
            };
            tableauActualites.AppliquerStructure();
            if (tableauActualites.ligneEnTete != null)
            {
                tableauActualites.ligneEnTete.Set(
                    "Date",
                    "Type",
                    "Titre",
                    "Source / Importance",
                    "État");
            }
        }

        if (txtDetailEffets != null)
        {
            txtDetailEffets.color = new Color(0.04f, 0.1f, 0.37f, 1f);
            txtDetailEffets.fontSize = 24f;
            txtDetailEffets.textWrappingMode = TextWrappingModes.Normal;
            txtDetailEffets.overflowMode = TextOverflowModes.Ellipsis;
        }
    }

    private static void ConfigurerLigne(LigneSelectable ligne)
    {
        LayoutElement layout = ligne.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = ligne.gameObject.AddComponent<LayoutElement>();
        }
        layout.preferredHeight = 56f;

        TextMeshProUGUI[] textes =
            ligne.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI texte in textes)
        {
            texte.textWrappingMode = TextWrappingModes.Normal;
            texte.overflowMode = TextOverflowModes.Ellipsis;
            texte.margin = new Vector4(6f, 2f, 6f, 2f);
        }
    }
}
