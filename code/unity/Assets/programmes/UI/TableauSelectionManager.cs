using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(TableauScroll))]
public class TableauSelectionManager : MonoBehaviour
{
    [Header("Evénements")]
    public UnityEvent<LigneSelectable> OnSelectionChanged;

    private LigneSelectable ligneSelectionnee;
    private TableauScroll tableauScroll;

    private void Awake()
    {
        tableauScroll = GetComponent<TableauScroll>();
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
        if (ligneSelectionnee == null && tableauScroll != null && tableauScroll.tableau != null)
        {
            foreach (Ligne l in tableauScroll.tableau)
            {
                if (!l.EstVide())
                {
                    LigneSelectable ls = l.GetComponent<LigneSelectable>();
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
