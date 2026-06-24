using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Type de donnees a charger dans le dropdown.
/// </summary>
public enum TypeChoixDropdown
{
    Secteur,
    Public,
    Technologie
}

/// <summary>
/// Remplit automatiquement un TMP_Dropdown avec les options du CatalogueEntrepreneuriat.
/// A attacher sur la racine du prefab DropDownTitre ou directement sur le Dropdown_XP.
/// </summary>
public class DropdownChoixEntrepreneuriat : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Quel type de donnees afficher dans ce dropdown ?")]
    public TypeChoixDropdown typeChoix = TypeChoixDropdown.Secteur;

    [Tooltip("Lien vers le composant TMP_Dropdown (laisse vide pour chercher automatiquement dans les enfants)")]
    public TMP_Dropdown dropdown;

    private void Start()
    {
        // Recherche automatique du Dropdown s'il n'est pas defini
        if (dropdown == null)
        {
            dropdown = GetComponentInChildren<TMP_Dropdown>();
        }

        if (dropdown != null)
        {
            PeuplerDropdown();
            // Ecouter les changements pour mettre a jour l'interface automatiquement
            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
        else
        {
            Debug.LogWarning("Aucun TMP_Dropdown trouve pour DropdownChoixEntrepreneuriat sur " + gameObject.name);
        }
    }

    private void OnDropdownValueChanged(int nouvelIndex)
    {
        // Chercher le composant principal de l'UI
        EntrepreneuriatUI ui = GetComponentInParent<EntrepreneuriatUI>();
        if (ui != null)
        {
            ui.ActualiserDepuisDropdowns();
        }
    }

    /// <summary>
    /// Vide le dropdown et le remplit avec les donnees du catalogue.
    /// </summary>
    public void PeuplerDropdown()
    {
        dropdown.ClearOptions();
        List<string> options = new List<string>();

        switch (typeChoix)
        {
            case TypeChoixDropdown.Secteur:
                for (int i = 0; i < CatalogueEntrepreneuriat.NombreSecteurs; i++)
                {
                    options.Add(CatalogueEntrepreneuriat.ObtenirSecteur((SecteurEntrepreneurial)i).Nom);
                }
                break;

            case TypeChoixDropdown.Public:
                for (int i = 0; i < CatalogueEntrepreneuriat.NombrePublics; i++)
                {
                    options.Add(CatalogueEntrepreneuriat.ObtenirPublic((PublicEntrepreneurial)i).Nom);
                }
                break;

            case TypeChoixDropdown.Technologie:
                for (int i = 0; i < CatalogueEntrepreneuriat.NombreTechnologies; i++)
                {
                    options.Add(CatalogueEntrepreneuriat.ObtenirTechnologie((TechnologieEntrepreneuriale)i).Nom);
                }
                break;
        }

        dropdown.AddOptions(options);
    }

    /// <summary>
    /// Permet de recuperer l'index actuellement selectionne (qui correspond a l'enum).
    /// </summary>
    public int ObtenirIndexSelectionne()
    {
        return dropdown != null ? dropdown.value : 0;
    }
}
