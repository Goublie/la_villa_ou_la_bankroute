using UnityEngine;
using UnityEngine.Events;

public class TableauScrollSelectable : TableauScroll
{
    [Header("Evénements")]
    public UnityEvent<LigneSelectable> OnSelectionChanged;

    private LigneSelectable ligneSelectionnee;

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
