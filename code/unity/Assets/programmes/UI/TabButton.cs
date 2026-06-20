using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public TabGroup tabGroup;
    public Image background;
    public TextMeshProUGUI textLabel;

    [Header("États (Sprites)")]
    [Tooltip("Glisse ici tes sprites découpés pour chaque état")]
    public Sprite spriteNormal;
    public Sprite spriteHover;
    public Sprite spritePressed;
    public Sprite spriteActif;

    [Header("États (Couleurs de secours)")]
    public Color couleurNormal = new Color(0.92f, 0.92f, 0.92f);
    public Color couleurHover = new Color(0.96f, 0.96f, 0.96f);
    public Color couleurPressed = new Color(0.85f, 0.85f, 0.85f);
    public Color couleurActif = Color.white;

    private bool estActif = false;
    private bool estHover = false;
    private bool estPressed = false;

    void Awake()
    {
        if (background == null) background = GetComponent<Image>();
        if (textLabel == null) textLabel = GetComponentInChildren<TextMeshProUGUI>();
        
        if (tabGroup == null)
        {
            tabGroup = GetComponentInParent<TabGroup>();
        }

        if (tabGroup != null)
        {
            tabGroup.Subscribe(this);
        }
        
        UpdateVisuel();
    }

    public void SetActif(bool actif)
    {
        estActif = actif;
        UpdateVisuel();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (tabGroup != null) tabGroup.OnTabSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        estHover = true;
        UpdateVisuel();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        estHover = false;
        UpdateVisuel();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        estPressed = true;
        UpdateVisuel();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        estPressed = false;
        UpdateVisuel();
    }

    private void UpdateVisuel()
    {
        if (background == null) return;

        if (estActif)
        {
            AppliquerApparence(spriteActif, couleurActif);
        }
        else if (estPressed)
        {
            AppliquerApparence(spritePressed, couleurPressed);
        }
        else if (estHover)
        {
            AppliquerApparence(spriteHover, couleurHover);
        }
        else
        {
            AppliquerApparence(spriteNormal, couleurNormal);
        }
    }

    private void AppliquerApparence(Sprite sprite, Color fallbackColor)
    {
        if (sprite != null)
        {
            background.sprite = sprite;
            background.color = Color.white; // On reset la couleur pour afficher le sprite pur
        }
        else
        {
            background.sprite = null;
            background.color = fallbackColor;
        }
    }
}
