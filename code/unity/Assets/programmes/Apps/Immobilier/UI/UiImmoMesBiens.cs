using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère l'onglet Mes Biens du module Immobilier (tableau du patrimoine possédé).
/// </summary>
public class UiImmoMesBiens : MonoBehaviour
{
    [Header("Données partagées")]
    [SerializeField] private GameData gameData;

    [Header("Tableau / Grille")]
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Transform container;

    [Header("Boutons d'action")]
    [SerializeField] private Button boutonVendre;

    [Header("Navigation Panels")]
    [SerializeField] private GameObject panneauAccueil;
    [SerializeField] private GameObject panneauMarche;
    [SerializeField] private GameObject panneauMesBiens;

    private ActionPlay actionPlay;
    private List<GameObject> lignesInstanciees = new List<GameObject>();
    private int indexSelectionne = -1;

    private void Awake()
    {
        ResoudreDependances();
    }

    private void OnEnable()
    {
        ResoudreDependances();
        ActionPlay.OnMoisPasse += ActualiserAffichage;

        if (boutonVendre != null)
        {
            boutonVendre.onClick.RemoveListener(VendreSelection);
            boutonVendre.onClick.AddListener(VendreSelection);
        }

        ActualiserAffichage();
    }

    private void OnDisable()
    {
        ActionPlay.OnMoisPasse -= ActualiserAffichage;

        if (boutonVendre != null)
        {
            boutonVendre.onClick.RemoveListener(VendreSelection);
        }
    }

    private void ResoudreDependances()
    {
        if (actionPlay == null)
        {
            actionPlay = FindFirstObjectByType<ActionPlay>();
            if (actionPlay != null && gameData == null)
            {
                gameData = actionPlay.gameData;
            }
        }
    }

    /// <summary>
    /// Actualise la liste des biens possédés par le joueur.
    /// </summary>
    public void ActualiserAffichage()
    {
        if (gameData == null || gameData.joueur == null || gameData.joueur.immobilier == null) return;

        // 1. Nettoyer les anciennes lignes
        foreach (var ligne in lignesInstanciees)
        {
            if (ligne != null) Destroy(ligne);
        }
        lignesInstanciees.Clear();
        indexSelectionne = -1;
        MettreAJourEtatBoutons();

        var biens = gameData.joueur.immobilier.biensPossedes;
        if (biens == null) return;

        // 2. Instancier les nouvelles lignes
        for (int i = 0; i < biens.Count; i++)
        {
            var bien = biens[i];
            if (bien == null) continue;

            GameObject rowGo = Instantiate(rowPrefab, container);
            lignesInstanciees.Add(rowGo);

            int surface = ObtenirSurface(bien.ville, bien.type);
            argent plusValue = bien.valeurActuelle - bien.prixAchat;

            // Remplir les colonnes (0: Ville, 1: Type, 2: Surface, 3: Prix Achat, 4: Valeur Actuelle, 5: Loyer, 6: Plus-value)
            SetCellText(rowGo.transform, 0, bien.ville.ToString());
            SetCellText(rowGo.transform, 1, FormaterTypeBien(bien.type));
            SetCellText(rowGo.transform, 2, $"{surface} m²");
            SetCellText(rowGo.transform, 3, bien.prixAchat.ToString());
            SetCellText(rowGo.transform, 4, bien.valeurActuelle.ToString());
            SetCellText(rowGo.transform, 5, bien.loyerMensuel.ToString());
            SetCellText(rowGo.transform, 6, FormaterPlusValue(plusValue));

            // Configurer le bouton / clic de sélection
            int index = i;
            Button btn = rowGo.GetComponent<Button>();
            if (btn == null)
            {
                btn = rowGo.AddComponent<Button>();
            }
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SelectionnerLigne(index));

            // Appliquer la couleur d'origine (alternance zèbre)
            MettreAJourCouleurLigne(rowGo, index, false);
        }
    }

    /// <summary>
    /// Sélectionne visuellement une ligne et mémorise l'index du bien.
    /// </summary>
    private void SelectionnerLigne(int index)
    {
        if (index < 0 || index >= lignesInstanciees.Count) return;

        indexSelectionne = index;

        for (int i = 0; i < lignesInstanciees.Count; i++)
        {
            MettreAJourCouleurLigne(lignesInstanciees[i], i, i == indexSelectionne);
        }

        MettreAJourEtatBoutons();
        Debug.Log($"[UiImmoMesBiens] Bien sélectionné à l'index : {indexSelectionne}");
    }

    /// <summary>
    /// Gère la vente du bien sélectionné.
    /// </summary>
    private void VendreSelection()
    {
        if (gameData == null || gameData.joueur == null || gameData.joueur.immobilier == null) return;
        var biens = gameData.joueur.immobilier.biensPossedes;

        if (indexSelectionne < 0 || indexSelectionne >= biens.Count)
        {
            Debug.LogWarning("[UiImmoMesBiens] Aucun bien sélectionné pour la vente.");
            return;
        }

        var bien = biens[indexSelectionne];

        // 1. Créditer le compte courant de la valeur actuelle du bien
        ServiceBanque banque = new ServiceBanque(gameData.joueur);
        CompteBanquaire compteCourant = banque.ObtenirCompteCourant();
        banque.Crediter(compteCourant, bien.valeurActuelle, "Vente Immo");

        // 2. Retirer le bien du patrimoine du joueur
        biens.RemoveAt(indexSelectionne);

        // 3. Forcer la mise à jour immédiate du patrimoine total
        gameData.joueur.CalculPatrimoineTotal();

        Debug.Log($"[UiImmoMesBiens] Vente réussie de {bien.type} à {bien.ville} pour {bien.valeurActuelle}");

        ActualiserAffichage();
    }

    private void MettreAJourEtatBoutons()
    {
        if (boutonVendre != null)
        {
            boutonVendre.interactable = (indexSelectionne >= 0);
        }
    }

    private void MettreAJourCouleurLigne(GameObject ligne, int index, bool estSelectionnee)
    {
        Image img = ligne.GetComponent<Image>();
        if (img == null) return;

        if (estSelectionnee)
        {
            img.color = new Color32(200, 225, 255, 255); // Bleu sélection
        }
        else
        {
            img.color = (index % 2 == 0)
                ? new Color32(245, 245, 245, 255)
                : new Color32(255, 255, 255, 255);
        }
    }

    private void SetCellText(Transform row, int cellIndex, string text)
    {
        if (cellIndex < row.childCount)
        {
            TMP_Text tmp = row.GetChild(cellIndex).GetComponentInChildren<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = text;
            }
        }
    }

    private string FormaterTypeBien(TypeBien type)
    {
        return type switch
        {
            TypeBien.Studio => "Studio",
            TypeBien.AppartementT2 => "Appartement T2",
            TypeBien.AppartementT4 => "Appartement T4",
            TypeBien.ImmeubleRapport => "Immeuble de rapport",
            TypeBien.LocalCommercial => "Local commercial",
            _ => type.ToString()
        };
    }

    private string FormaterPlusValue(argent plusValue)
    {
        int centimes = plusValue.centimes;
        string formatted = plusValue.ToString();
        if (centimes > 0)
        {
            return $"<color=#187A2F>+{formatted}</color>";
        }
        else if (centimes < 0)
        {
            return $"<color=#B02020>{formatted}</color>"; // La valeur négative de l'argent contient déjà le signe "-"
        }
        else
        {
            return "<color=grey>0,00 €</color>";
        }
    }

    // Recherche de la surface du bien basé sur le catalogue immuable
    private int ObtenirSurface(Ville ville, TypeBien type)
    {
        string villeCle = ville.ToString().ToLower();
        var catalogue = CatalogueImmobilier.ObtenirBiens();
        if (catalogue == null) return 0;

        foreach (var def in catalogue)
        {
            if (def.VilleId.ToLower() == villeCle && MaperTypeCatalogueVersJoueur(def.TypeBien) == type)
            {
                return def.SurfaceM2;
            }
        }
        return 0;
    }

    private TypeBien MaperTypeCatalogueVersJoueur(TypeBienImmobilier typeCatalogue)
    {
        return typeCatalogue switch
        {
            TypeBienImmobilier.Studio => TypeBien.Studio,
            TypeBienImmobilier.Appartement => TypeBien.AppartementT2,
            TypeBienImmobilier.ImmeubleRapport => TypeBien.ImmeubleRapport,
            TypeBienImmobilier.LocalCommercial => TypeBien.LocalCommercial,
            _ => TypeBien.Studio
        };
    }

    // --- NAVIGATION ---
    public void AllerAuMarche()
    {
        if (panneauAccueil != null) panneauAccueil.SetActive(false);
        if (panneauMesBiens != null) panneauMesBiens.SetActive(false);
        if (panneauMarche != null) panneauMarche.SetActive(true);
    }

    public void AllerALAccueil()
    {
        if (panneauMarche != null) panneauMarche.SetActive(false);
        if (panneauMesBiens != null) panneauMesBiens.SetActive(false);
        if (panneauAccueil != null) panneauAccueil.SetActive(true);
    }
}
