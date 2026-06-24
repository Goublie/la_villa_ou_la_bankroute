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
    [SerializeField] private TableauScrollSelectable tableauMarcheComponent;

    [Header("Boutons d'action")]
    [SerializeField] private Button boutonAcheter;

    [Header("Navigation Panels")]
    [SerializeField] private GameObject panneauAccueil;
    [SerializeField] private GameObject panneauMarche;
    [SerializeField] private GameObject panneauMesBiens;

    private ActionPlay actionPlay;
    private Dictionary<LigneSelectable, AnnonceImmobiliere> mapLignesAnnonces = new Dictionary<LigneSelectable, AnnonceImmobiliere>();
    private AnnonceImmobiliere annonceSelectionnee;

    private void Awake()
    {
        ResoudreDependances();
    }

    private void OnEnable()
    {
        ResoudreDependances();
        ActionPlay.OnMoisPasse += ActualiserAffichage;
        
        if (tableauMarcheComponent != null)
        {
            tableauMarcheComponent.OnSelectionChanged.RemoveListener(OnAnnonceSelectionnee);
            tableauMarcheComponent.OnSelectionChanged.AddListener(OnAnnonceSelectionnee);
        }

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
        
        if (tableauMarcheComponent != null)
        {
            tableauMarcheComponent.OnSelectionChanged.RemoveListener(OnAnnonceSelectionnee);
        }

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
        if (tableauMarcheComponent == null)
        {
            tableauMarcheComponent = GetComponentInChildren<TableauScrollSelectable>(true);
        }
    }

    /// <summary>
    /// Actualise la liste des annonces immobilières sur le marché.
    /// </summary>
    public void ActualiserAffichage()
    {
        if (gameData == null || gameData.joueur == null || gameData.joueur.immobilier == null) return;

        // 1. Nettoyer les anciennes lignes
        if (tableauMarcheComponent != null)
        {
            tableauMarcheComponent.Initialiser();
            foreach (var ligne in tableauMarcheComponent.tableau)
            {
                if (ligne != null) ligne.Vider();
            }
            tableauMarcheComponent.ReinitialiserSelection();
        }
        
        mapLignesAnnonces.Clear();
        annonceSelectionnee = null;
        MettreAJourEtatBoutonAcheter();

        var annonces = gameData.joueur.immobilier.annoncesActuelles;
        if (annonces == null || annonces.Count == 0)
        {
            ServiceImmobilier.RafraichirMarche(gameData.joueur, gameData.nombreMoisPasses, 3);
            annonces = gameData.joueur.immobilier.annoncesActuelles;
        }
        if (annonces == null) return;

        // 2. Remplir le tableau
        if (tableauMarcheComponent != null)
        {
            for (int i = 0; i < annonces.Count; i++)
            {
                var annonce = annonces[i];
                if (annonce == null) continue;

                // Remplir les colonnes selon les en-têtes du prefab :
                // 0: Type de bien, 1: Surface, 2: Nombre de pièces, 3: Localisation, 4: Meublé, 5: Prix, 6: Loyer
                var ligne = tableauMarcheComponent.AjouterEtRetournerLigne(
                    FormaterTypeBien(annonce.Type),
                    $"{annonce.SurfaceM2} m²",
                    ObtenirNombrePieces(annonce.Type),
                    annonce.Ville.ToString(),
                    annonce.EstMeuble ? "Oui" : "Non",
                    annonce.PrixVenteAffiche.ToString(),
                    annonce.LoyerMensuelPropose.ToString()
                );

                if (ligne != null)
                {
                    mapLignesAnnonces.Add(ligne, annonce);
                }
            }
            tableauMarcheComponent.CheckAutoSelection();
        }
    }

    /// <summary>
    /// Callback invoqué lors du changement de sélection d'une ligne du tableau.
    /// </summary>
    private void OnAnnonceSelectionnee(LigneSelectable ligne)
    {
        if (ligne != null && mapLignesAnnonces.TryGetValue(ligne, out AnnonceImmobiliere annonce))
        {
            annonceSelectionnee = annonce;
        }
        else
        {
            annonceSelectionnee = null;
        }

        MettreAJourEtatBoutonAcheter();
        if (annonceSelectionnee != null)
        {
            Debug.Log($"[UiImmoMarche] Annonce sélectionnée : {annonceSelectionnee.Type} à {annonceSelectionnee.Ville} ({annonceSelectionnee.SurfaceM2} m²)");
        }
    }

    /// <summary>
    /// Gère l'achat du bien actuellement sélectionné.
    /// </summary>
    private void AcheterSelection()
    {
        if (gameData == null || gameData.joueur == null || gameData.joueur.immobilier == null) return;

        if (annonceSelectionnee == null)
        {
            Debug.LogWarning("[UiImmoMarche] Aucun bien sélectionné pour l'achat.");
            return;
        }

        bool succes = ServiceImmobilier.ExecuterAchatCash(gameData.joueur, annonceSelectionnee, gameData.nombreMoisPasses);

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
            boutonAcheter.interactable = (annonceSelectionnee != null);
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

    private string ObtenirNombrePieces(TypeBien type)
    {
        return type switch
        {
            TypeBien.Studio => "1",
            TypeBien.AppartementT2 => "2",
            TypeBien.AppartementT4 => "4",
            _ => "-"
        };
    }

    private string EstMeuble(TypeBien type)
    {
        return type == TypeBien.Studio ? "Oui" : "Non";
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
