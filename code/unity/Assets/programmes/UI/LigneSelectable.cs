using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Ligne), typeof(Image))]
public class LigneSelectable : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    private TableauSelectionManager manager;
    private Image fondImage;
    private Ligne ligne;
    private bool aVerifieSelection = false;

    private void Awake()
    {
        ligne = GetComponent<Ligne>();
        fondImage = GetComponent<Image>();
        
        // Empêche Unity de supprimer le mesh si la couleur est transparente (alpha=0),
        // ce qui empêcherait la détection des clics !
        if (fondImage != null)
        {
            fondImage.canvasRenderer.cullTransparentMesh = false;
        }

        Image[] imagesEnfants = GetComponentsInChildren<Image>(true);
        foreach (Image img in imagesEnfants)
        {
            img.canvasRenderer.cullTransparentMesh = false;
        }
    }

    private void Start()
    {
        manager = GetComponentInParent<TableauSelectionManager>();
        
        if (manager != null && manager.GetLigneSelectionnee() != this)
        {
            SetSelectedCouleur(manager.couleurDefaut);
        }
    }

    private void Update()
    {
        // Auto-sélection de la première ligne au remplissage
        if (!aVerifieSelection && !ligne.EstVide())
        {
            aVerifieSelection = true;
            if (manager != null && manager.GetLigneSelectionnee() == null)
            {
                manager.CheckAutoSelection();
            }
        }
        else if (ligne.EstVide())
        {
            // Réinitialise le flag si la ligne est vidée
            aVerifieSelection = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Ligne cliquée ! EstVide : " + ligne.EstVide() + " | Manager = " + (manager != null ? "TROUVÉ" : "NULL"));
        
        if (manager != null)
        {
            manager.Selectionner(this);
            Debug.Log("Sélection appliquée avec la couleur : " + manager.couleurSelection);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Nécessaire pour que OnPointerClick soit correctement détecté par Unity 
        // quand des enfants (comme le texte) interceptent le raycast initial.
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void SetSelectedCouleur(Color couleur)
    {
        if (fondImage != null)
        {
            fondImage.color = couleur;
        }

        // Appliquer la couleur à toutes les images enfants (les cases) pour s'assurer que le changement est visible
        Image[] imagesEnfants = GetComponentsInChildren<Image>(true);
        foreach (Image img in imagesEnfants)
        {
            img.color = couleur;
        }
    }
}
