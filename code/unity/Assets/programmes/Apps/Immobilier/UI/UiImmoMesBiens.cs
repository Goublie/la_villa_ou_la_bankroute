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
    [SerializeField] private TableauScrollSelectable tableauMesBiensComponent;

    [Header("Boutons d'action")]
    [SerializeField] private Button boutonVendre;

    [Header("Navigation Panels")]
    [SerializeField] private GameObject panneauAccueil;
    [SerializeField] private GameObject panneauMarche;
    [SerializeField] private GameObject panneauMesBiens;

    private ActionPlay actionPlay;
    private Dictionary<LigneSelectable, BienImmobilier> mapLignesBiens = new Dictionary<LigneSelectable, BienImmobilier>();
    private BienImmobilier bienSelectionne;

    private void Awake()
    {
        ResoudreDependances();
    }

    private void OnEnable()
    {
        ResoudreDependances();
        ActionPlay.OnMoisPasse += ActualiserAffichage;

        if (tableauMesBiensComponent != null)
        {
            tableauMesBiensComponent.OnSelectionChanged.RemoveListener(OnBienSelectionne);
            tableauMesBiensComponent.OnSelectionChanged.AddListener(OnBienSelectionne);
        }

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

        if (tableauMesBiensComponent != null)
        {
            tableauMesBiensComponent.OnSelectionChanged.RemoveListener(OnBienSelectionne);
        }

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
        if (tableauMesBiensComponent == null)
        {
            tableauMesBiensComponent = GetComponentInChildren<TableauScrollSelectable>(true);
        }
    }

    /// <summary>
    /// Actualise la liste des biens possédés par le joueur.
    /// </summary>
    public void ActualiserAffichage()
    {
        if (gameData == null || gameData.joueur == null || gameData.joueur.immobilier == null) return;

        // 1. Nettoyer les anciennes lignes
        if (tableauMesBiensComponent != null)
        {
            tableauMesBiensComponent.Initialiser();
            foreach (var ligne in tableauMesBiensComponent.tableau)
            {
                if (ligne != null) ligne.Vider();
            }
            tableauMesBiensComponent.ReinitialiserSelection();
        }
        
        mapLignesBiens.Clear();
        bienSelectionne = null;
        MettreAJourEtatBoutons();

        var biens = gameData.joueur.immobilier.biensPossedes;
        if (biens == null) return;

        // 2. Remplir le tableau
        if (tableauMesBiensComponent != null)
        {
            for (int i = 0; i < biens.Count; i++)
            {
                var bien = biens[i];
                if (bien == null) continue;

                // Remplir les colonnes selon les en-têtes du prefab :
                // 0: Type de bien, 1: Surface, 2: Nombre de pièces, 3: Localisation, 4: Meublé, 5: Prix (valeur actuelle), 6: Loyer
                var ligne = tableauMesBiensComponent.AjouterEtRetournerLigne(
                    FormaterTypeBien(bien.type),
                    $"{bien.surfaceM2} m²",
                    ObtenirNombrePieces(bien.type),
                    bien.ville.ToString(),
                    bien.estMeuble ? "Oui" : "Non",
                    bien.valeurActuelle.ToString(),
                    bien.loyerMensuel.ToString()
                );

                if (ligne != null)
                {
                    mapLignesBiens.Add(ligne, bien);
                }
            }
            tableauMesBiensComponent.CheckAutoSelection();
        }
    }

    /// <summary>
    /// Callback invoqué lors du changement de sélection d'une ligne du tableau.
    /// </summary>
    private void OnBienSelectionne(LigneSelectable ligne)
    {
        if (ligne != null && mapLignesBiens.TryGetValue(ligne, out BienImmobilier bien))
        {
            bienSelectionne = bien;
        }
        else
        {
            bienSelectionne = null;
        }

        MettreAJourEtatBoutons();
        if (bienSelectionne != null)
        {
            Debug.Log($"[UiImmoMesBiens] Bien sélectionné : {bienSelectionne.type} à {bienSelectionne.ville}");
        }
    }

    /// <summary>
    /// Gère la vente du bien sélectionné.
    /// </summary>
    private void VendreSelection()
    {
        if (gameData == null || gameData.joueur == null || gameData.joueur.immobilier == null) return;

        if (bienSelectionne == null)
        {
            Debug.LogWarning("[UiImmoMesBiens] Aucun bien sélectionné pour la vente.");
            return;
        }

        // 1. Créditer le compte courant de la valeur actuelle du bien
        ServiceBanque banque = new ServiceBanque(gameData.joueur);
        CompteBanquaire compteCourant = banque.ObtenirCompteCourant();
        banque.Crediter(compteCourant, bienSelectionne.valeurActuelle, "Vente Immo");

        // 2. Retirer le bien du patrimoine du joueur
        gameData.joueur.immobilier.biensPossedes.Remove(bienSelectionne);

        // 3. Forcer la mise à jour immédiate du patrimoine total
        gameData.joueur.CalculPatrimoineTotal();

        Debug.Log($"[UiImmoMesBiens] Vente réussie de {bienSelectionne.type} à {bienSelectionne.ville} pour {bienSelectionne.valeurActuelle}");

        ActualiserAffichage();
    }

    private void MettreAJourEtatBoutons()
    {
        if (boutonVendre != null)
        {
            boutonVendre.interactable = (bienSelectionne != null);
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
