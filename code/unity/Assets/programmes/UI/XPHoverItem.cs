using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Gere le style de survol (fond bleu "#316AC5", texte blanc) pour les elements
/// d'un Dropdown de style Windows XP Luna.
/// </summary>
public class XPHoverItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    private Image background;
    private TMP_Text label;

    private Color normalBg = Color.white;
    private Color hoverBg = new Color32(49, 106, 197, 255); // #316AC5
    private Color normalText = Color.black;
    private Color hoverText = Color.white;

    private void Awake()
    {
        background = transform.Find("Item Background")?.GetComponent<Image>();
        label = transform.Find("Item Label")?.GetComponent<TMP_Text>();
        
        // Initialiser avec l'etat normal
        SetHover(false);
    }

    public void OnPointerEnter(PointerEventData eventData) => SetHover(true);
    
    public void OnPointerExit(PointerEventData eventData) => SetHover(false);
    
    public void OnSelect(BaseEventData eventData) => SetHover(true);
    
    public void OnDeselect(BaseEventData eventData) => SetHover(false);

    private void SetHover(bool isHover)
    {
        if (background != null)
        {
            background.color = isHover ? hoverBg : normalBg;
        }
        
        if (label != null)
        {
            label.color = isHover ? hoverText : normalText;
        }
    }
}
