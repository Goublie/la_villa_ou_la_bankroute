using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Structure représentant une actualité dans le jeu.
/// </summary>
[System.Serializable]
public class ActualiteItem
{
    public string date;
    public string categorie;
    public string titre;
    public string impact;
    public string nouveau;
    [TextArea(3, 10)]
    public string description;
    public bool aCourbe; // Affiche ou non le panel "Inserer courbe"
    public Color couleurCategorie = Color.white;
}

/// <summary>
/// Gère l'interface et la logique de la rubrique Actualités (News).
/// </summary>
public class ActualitesUI : MonoBehaviour
{
    [Header("Données des Actualités")]
    public List<ActualiteItem> actualitesList = new List<ActualiteItem>();

    [Header("Navigation / Onglets")]
    [SerializeField] private Button btnDernieresActualites;
    [SerializeField] private Button btnCategoriesTab; // Onglet optionnel du haut

    [Header("Panneau Catégories (Gauche)")]
    [SerializeField] private Transform categoriesContainer;
    [SerializeField] private GameObject prefabCategoryBtn; // Prefab de bouton de catégorie

    [Header("Tableau des Actualités (Centre/Droit)")]
    [SerializeField] private Transform newsRowsContainer;
    [SerializeField] private GameObject prefabNewsRow; // Prefab de ligne d'actualité (avec 5 cases)

    [Header("Panneau de Détails (Bas)")]
    [SerializeField] private TextMeshProUGUI txtDetailTitre;
    [SerializeField] private TextMeshProUGUI txtDetailDescription;
    [SerializeField] private GameObject panelCourbe; // Pavé vert "Inserer courbe"

    private string categorieFiltre = ""; // Filtre de catégorie actif ("" = aucun filtre)
    private List<GameObject> activeRows = new List<GameObject>();
    private List<GameObject> categoryButtons = new List<GameObject>();

    void Start()
    {
        InitialiserActualitesDeTest();
        ConfigurerNavigation();
        GenererBoutonsCategories();
        AfficherActualites();

        // Sélectionner la première actualité par défaut
        if (actualitesList.Count > 0)
        {
            AfficherDetails(actualitesList[0]);
        }
    }

    
    private void LoadActualitesFromResources()
    {
        // Charge tous les assets ActualiteSO placés dans Resources/Actualites
        var assets = Resources.LoadAll<ActualiteSO>("Actualites");
        // Réinitialise la liste pour éviter les doublons
        actualitesList.Clear();
        foreach (var so in assets)
        {
            if (so == null || !so.actif) continue;
            // Convertit le ScriptableObject en ActualiteItem utilisé par l'UI
            var item = new ActualiteItem
            {
                date = so.dateDebut.ToString("dd/MM/yyyy"),
                categorie = so.type.ToString(),
                titre = so.titre,
                impact = so.impactFinancier.ToString(),
                nouveau = "",
                description = so.description,
                aCourbe = false,
                // Couleur selon le type d'actualité (Public = vert, Private = cyan)
                couleurCategorie = so.type == ActualiteSO.TypeActualite.Public ? new Color(0.2f, 0.7f, 0.2f) : new Color(0.0f, 0.7f, 0.7f)
            };
            actualitesList.Add(item);
        }
    }
    

    private void ConfigurerNavigation()
    {
        if (btnDernieresActualites != null)
        {
            btnDernieresActualites.onClick.AddListener(() =>
            {
                FiltrerParCategorie("");
            });
        }

        if (btnCategoriesTab != null)
        {
            btnCategoriesTab.onClick.AddListener(() =>
            {
                Debug.Log("Onglet Catégories cliqué.");
            });
        }
    }

    private void GenererBoutonsCategories()
    {
        if (categoriesContainer == null || prefabCategoryBtn == null) return;

        // Nettoyer d'abord
        foreach (var btn in categoryButtons)
        {
            Destroy(btn);
        }
        categoryButtons.Clear();

        // Récupérer les catégories uniques
        HashSet<string> categoriesUniques = new HashSet<string>();
        Dictionary<string, Color> couleursCategories = new Dictionary<string, Color>();

        foreach (var item in actualitesList)
        {
            if (!string.IsNullOrEmpty(item.categorie))
            {
                categoriesUniques.Add(item.categorie);
                if (!couleursCategories.ContainsKey(item.categorie))
                {
                    couleursCategories[item.categorie] = item.couleurCategorie;
                }
            }
        }

        // Créer les boutons de catégorie
        foreach (string cat in categoriesUniques)
        {
            GameObject btnObj = Instantiate(prefabCategoryBtn, categoriesContainer, false);
            btnObj.name = $"Btn_Cat_{cat}";
            btnObj.SetActive(true);
            
            TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.text = cat;
                txt.color = couleursCategories[cat];
            }

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                string catFiltre = cat;
                btn.onClick.AddListener(() => FiltrerParCategorie(catFiltre));
            }

            categoryButtons.Add(btnObj);
        }
    }

    private void AfficherActualites()
    {
        if (newsRowsContainer == null || prefabNewsRow == null) return;

        // Nettoyer le tableau existant
        foreach (var row in activeRows)
        {
            Destroy(row);
        }
        activeRows.Clear();

        // Filtrer et instancier
        foreach (var item in actualitesList)
        {
            // Vérifier le filtre
            if (!string.IsNullOrEmpty(categorieFiltre) && item.categorie != categorieFiltre)
                continue;

            GameObject rowObj = Instantiate(prefabNewsRow, newsRowsContainer, false);
            rowObj.name = $"Row_Actualite_{item.date.Replace("/", "-")}";
            rowObj.SetActive(true);

            // Remplir les cases (Ligne C#)
            Ligne ligne = rowObj.GetComponent<Ligne>();
            if (ligne != null)
            {
                ligne.Set(0, item.date);
                ligne.Set(1, item.categorie);
                ligne.Set(2, item.titre);
                ligne.Set(3, item.impact);
                ligne.Set(4, item.nouveau);

                // Appliquer la couleur de la catégorie sur la case de la catégorie (colonne 1)
                Transform caseCat = rowObj.transform.Find("Case1");
                if (caseCat != null)
                {
                    TextMeshProUGUI txtCat = caseCat.GetComponentInChildren<TextMeshProUGUI>();
                    if (txtCat != null)
                    {
                        txtCat.color = item.couleurCategorie;
                    }
                }
            }

            // Ajouter le clic pour la sélection
            Button btn = rowObj.GetComponent<Button>();
            if (btn == null)
            {
                btn = rowObj.AddComponent<Button>();
            }

            ActualiteItem clickedItem = item;
            btn.onClick.AddListener(() => AfficherDetails(clickedItem));

            activeRows.Add(rowObj);
        }
    }

    public void FiltrerParCategorie(string categorie)
    {
        categorieFiltre = categorie;
        AfficherActualites();

        // Sélectionner le premier élément filtré s'il y en a un
        bool selectionFait = false;
        foreach (var item in actualitesList)
        {
            if (string.IsNullOrEmpty(categorie) || item.categorie == categorie)
            {
                AfficherDetails(item);
                selectionFait = true;
                break;
            }
        }

        if (!selectionFait)
        {
            ViderDetails();
        }
    }

    private void AfficherDetails(ActualiteItem item)
    {
        if (txtDetailTitre != null)
            txtDetailTitre.text = item.titre;

        if (txtDetailDescription != null)
            txtDetailDescription.text = item.description;

        if (panelCourbe != null)
            panelCourbe.SetActive(item.aCourbe);
    }

    private void ViderDetails()
    {
        if (txtDetailTitre != null) txtDetailTitre.text = "";
        if (txtDetailDescription != null) txtDetailDescription.text = "";
        if (panelCourbe != null) panelCourbe.SetActive(false);
    }
}
