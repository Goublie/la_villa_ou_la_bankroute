using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class LigneSelectable : Ligne, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Sélection")]
    [SerializeField] private Color couleurSelection = Color.gray;

    private TableauScrollSelectable manager;
    private Image fondImage;
    private bool aVerifieSelection = false;

    public override void Awake()
    {
        //initialise les cases
        base.Awake();
        
        fondImage = GetComponent<Image>();
        
        //force la détection des clics
        if (fondImage != null)
        {
            fondImage.canvasRenderer.cullTransparentMesh = false;
        }
    }

    private void Start()
    {
        manager = GetComponentInParent<TableauScrollSelectable>();
        
        if (manager != null && manager.GetLigneSelectionnee() != this)
        {
            SetSelectionState(false);
        }
    }

    private void Update()
    {
        // Auto-sélection de la première ligne au remplissage
        if (!aVerifieSelection && !EstVide())
        {
            aVerifieSelection = true;
            if (manager != null && manager.GetLigneSelectionnee() == null)
            {
                manager.CheckAutoSelection();
            }
        }
        else if (EstVide())
        {
            // Réinitialise le flag si la ligne est vidée
            aVerifieSelection = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager != null)
        {
            manager.Selectionner(this);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }

    public void OnPointerUp(PointerEventData eventData)
    {
    }

    public void SetSelectionState(bool estSelectionne)
    {
        if (estSelectionne)
        {
            AppliquerCouleurCases(couleurSelection);
        }
        else
        {
            // On restaure la couleur de fond configurée dans la Ligne
            AppliquerCouleurCases(couleurFondCases);
        }
    }
}