using UnityEngine;
using UnityEngine.UI;

public class ToggleImageSwitcher : MonoBehaviour
{
    public Image imageComposant; // Glissez l'image ici
    public Sprite etatHaut;      // Sprite pour ON
    public Sprite etatBas;       // Sprite pour OFF

    void Awake()
    {
        // Auto-assign imageComposant if it is not dragged in the inspector
        if (imageComposant == null)
        {
            imageComposant = GetComponent<Image>();
            if (imageComposant == null)
            {
                imageComposant = GetComponentInChildren<Image>();
            }
        }

        if (imageComposant == null)
        {
            Debug.LogError($"[ToggleImageSwitcher] Aucun composant Image trouvé sur {gameObject.name}", this);
        }
    }

    public void SetState(bool isOn)
    {
        if (imageComposant != null)
        {
            imageComposant.sprite = isOn ? etatHaut : etatBas;
        }
    }
}