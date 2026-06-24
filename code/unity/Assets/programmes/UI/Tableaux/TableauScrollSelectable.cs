using UnityEngine;
using UnityEngine.Events;

public class TableauScrollSelectable : TableauScroll
{
    [Header("Evénements")]
    public UnityEvent<LigneSelectable> OnSelectionChanged;

    private LigneSelectable ligneSelectionnee;

    /// <summary>
    /// Ajoute les valeurs et retourne la ligne selectable effectivement remplie.
    /// </summary>
    /// <remarks>
    /// Cette extension conserve le comportement historique de <see cref="Add" />
    /// et permet aux ecrans qui projettent des objets metier d'associer chaque
    /// ligne a son modele sans comparer les textes affiches.
    /// </remarks>
    public new LigneSelectable AjouterEtRetournerLigne(params object[] valeurs)
    {
        Ligne ligneCible = null;
        int nombreLignesAvant = tableau != null ? tableau.Count : 0;

        if (tableau != null)
        {
            foreach (Ligne ligne in tableau)
            {
                if (ligne != null && ligne.EstVide())
                {
                    ligneCible = ligne;
                    break;
                }
            }
        }

        if (!base.Add(valeurs))
        {
            return null;
        }

        if (ligneCible == null &&
            tableau != null &&
            tableau.Count > nombreLignesAvant)
        {
            ligneCible = tableau[tableau.Count - 1];
        }

        return ligneCible as LigneSelectable;
    }

    /// <summary>
    /// Retire le surlignage courant avant de reconstruire les lignes visibles.
    /// </summary>
    public void ReinitialiserSelection()
    {
        if (ligneSelectionnee != null)
        {
            ligneSelectionnee.SetSelectionState(false);
        }

        ligneSelectionnee = null;
    }

    public void Selectionner(LigneSelectable nouvelleLigne)
    {
        if (ligneSelectionnee != null && ligneSelectionnee != nouvelleLigne)
        {
            ligneSelectionnee.SetSelectionState(false);
        }

        ligneSelectionnee = nouvelleLigne;

        if (ligneSelectionnee != null)
        {
            ligneSelectionnee.SetSelectionState(true);
            OnSelectionChanged?.Invoke(ligneSelectionnee);
        }
    }

    public LigneSelectable GetLigneSelectionnee()
    {
        return ligneSelectionnee;
    }

    public void CheckAutoSelection()
    {
        // Si aucune ligne n'est sélectionnée, on sélectionne la première ligne non-vide
        if (ligneSelectionnee == null && tableau != null)
        {
            foreach (Ligne l in tableau)
            {
                if (!l.EstVide())
                {
                    LigneSelectable ls = l as LigneSelectable;
                    if (ls != null)
                    {
                        Selectionner(ls);
                        return;
                    }
                }
            }
        }
    }
}
