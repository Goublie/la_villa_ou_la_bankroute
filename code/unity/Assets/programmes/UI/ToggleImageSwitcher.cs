using UnityEngine;
using UnityEngine.UI;

public class ToggleImageSwitcher : MonoBehaviour
{
    public Image imageComposant; // Glissez l'image ici
    public Sprite etatHaut;      // Sprite pour ON
    public Sprite etatBas;       // Sprite pour OFF

    private Toggle toggle;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        
        // Auto-assign imageComposant if it is not dragged in the inspector
        if (imageComposant == null)
        {
            imageComposant = GetComponent<Image>();
            if (imageComposant == null)
            {
                imageComposant = GetComponentInChildren<Image>();
            }
        }

        if (imageComposant != null)
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
            Debug.Log($"[ToggleImageSwitcher] {gameObject.name} initialized. Initial state: {toggle.isOn}, Image: {imageComposant.name}", this);
            // État initial
            OnToggleValueChanged(toggle.isOn);
        }
        else
        {
            Debug.LogError($"[ToggleImageSwitcher] Aucun composant Image trouvé sur {gameObject.name}", this);
        }
    }

    void OnToggleValueChanged(bool isOn)
    {
        Debug.Log($"[ToggleImageSwitcher] {gameObject.name} clicked. New state (isOn): {isOn}", this);
        if (imageComposant != null)
        {
            imageComposant.sprite = isOn ? etatHaut : etatBas;
            Debug.Log($"[ToggleImageSwitcher] {gameObject.name} sprite changed to: {(isOn ? (etatHaut ? etatHaut.name : "null") : (etatBas ? etatBas.name : "null"))}", this);
        }
    }
}