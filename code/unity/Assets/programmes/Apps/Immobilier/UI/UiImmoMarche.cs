using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère l'onglet Marché du module Immobilier (tableau des annonces disponibles).
/// </summary>
public class UiImmoMarche : MonoBehaviour
{
    [Header("Données partagées")]
    [SerializeField] private GameData gameData;

    [Header("Tableau / Grille")]
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Transform container;

    [Header("Boutons d'action")]
    [SerializeField] private Button boutonAcheter;

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
        
        if (boutonAcheter != null)
        {
            boutonAcheter.onClick.RemoveListener(AcheterSelection);
            boutonAcheter.onClick.AddListener(AcheterSelection);
        }

        ActualiserAffichage();
    }

    private void OnDisable()
    {
        ActionPlay.OnMoisPasse -= ActualiserAffichage;
        
        if (boutonAcheter != null)
        {
            boutonAcheter.onClick.RemoveListener(AcheterSelection);
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
    /// Actualise la liste des annonces immobilières sur le marché.
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
        MettreAJourEtatBoutonAcheter();

        var annonces = gameData.joueur.immobilier.annoncesActuelles;
        if (annonces == null) return;

        // 2. Instancier les nouvelles lignes
        for (int i = 0; i < annonces.Count; i++)
        {
            var annonce = annonces[i];
            if (annonce == null) continue;

            GameObject rowGo = Instantiate(rowPrefab, container);
            lignesInstanciees.Add(rowGo);

            // Remplir les colonnes (0: Ville, 1: Type, 2: Surface, 3: Prix, 4: Loyer, 5: Renta)
            SetCellText(rowGo.transform, 0, annonce.Ville.ToString());
            SetCellText(rowGo.transform, 1, FormaterTypeBien(annonce.Type));
            SetCellText(rowGo.transform, 2, $"{annonce.Definition.SurfaceM2} m²");
            SetCellText(rowGo.transform, 3, annonce.PrixVenteAffiche.ToString());
            SetCellText(rowGo.transform, 4, $"{annonce.LoyerMensuelPropose} / mois");
            SetCellText(rowGo.transform, 5, $"{annonce.TauxRendementBrut * 100f:F1} %");

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
    /// Sélectionne visuellement une ligne et mémorise l'index de l'annonce.
    /// </summary>
    private void SelectionnerLigne(int index)
    {
        if (index < 0 || index >= lignesInstanciees.Count) return;

        indexSelectionne = index;

        for (int i = 0; i < lignesInstanciees.Count; i++)
        {
            MettreAJourCouleurLigne(lignesInstanciees[i], i, i == indexSelectionne);
        }

        MettreAJourEtatBoutonAcheter();
        Debug.Log($"[UiImmoMarche] Annonce sélectionnée à l'index : {indexSelectionne}");
    }

    /// <summary>
    /// Gère l'achat du bien actuellement sélectionné.
    /// </summary>
    private void AcheterSelection()
    {
        if (gameData == null || gameData.joueur == null || gameData.joueur.immobilier == null) return;
        var annonces = gameData.joueur.immobilier.annoncesActuelles;

        if (indexSelectionne < 0 || indexSelectionne >= annonces.Count)
        {
            Debug.LogWarning("[UiImmoMarche] Aucun bien sélectionné pour l'achat.");
            return;
        }

        var annonce = annonces[indexSelectionne];
        bool succes = ServiceImmobilier.ExecuterAchatCash(gameData.joueur, annonce, gameData.nombreMoisPasses);

        if (succes)
        {
            Debug.Log("[UiImmoMarche] Achat réussi !");
            ActualiserAffichage();
        }
        else
        {
            Debug.LogWarning("[UiImmoMarche] Achat échoué (fonds insuffisants).");
        }
    }

    private void MettreAJourEtatBoutonAcheter()
    {
        if (boutonAcheter != null)
        {
            boutonAcheter.interactable = (indexSelectionne >= 0);
        }
    }

    private void MettreAJourCouleurLigne(GameObject ligne, int index, bool estSelectionnee)
    {
        Image img = ligne.GetComponent<Image>();
        if (img == null) return;

        if (estSelectionnee)
        {
            img.color = new Color32(200, 225, 255, 255); // Bleu clair sélection
        }
        else
        {
            // Alternance de couleur (zèbre)
            img.color = (index % 2 == 0)
                ? new Color32(245, 245, 245, 255)  // Blanc cassé
                : new Color32(255, 255, 255, 255); // Blanc pur
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

    // --- NAVIGATION ---
    public void AllerAMesBiens()
    {
        if (panneauAccueil != null) panneauAccueil.SetActive(false);
        if (panneauMarche != null) panneauMarche.SetActive(false);
        if (panneauMesBiens != null) panneauMesBiens.SetActive(true);
    }

    public void AllerALAccueil()
    {
        if (panneauMarche != null) panneauMarche.SetActive(false);
        if (panneauMesBiens != null) panneauMesBiens.SetActive(false);
        if (panneauAccueil != null) panneauAccueil.SetActive(true);
    }
}
