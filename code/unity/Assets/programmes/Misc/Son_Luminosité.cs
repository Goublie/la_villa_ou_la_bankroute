using UnityEngine;
using UnityEngine.UI;

public class Son_Luminosité : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider sliderSon;
    [SerializeField] private Slider sliderLuminosite;

    [Header("Composants à modifier")]
    [SerializeField] private AudioSource musiqueFond;
    [SerializeField] private AudioSource sfxSource; // <── NOUVEAU : La source pour les bruitages
    [SerializeField] private Image filtreLuminosite;

    void Start()
    {
        // --- Configuration du Son ---
        if (sliderSon != null)
        {
            // On initialise le slider avec le volume actuel de la musique par défaut
            if (musiqueFond != null)
            {
                sliderSon.value = musiqueFond.volume;
            }

            // On écoute les changements du slider en temps réel
            sliderSon.onValueChanged.AddListener(ChangerVolumeGeneral);
        }

        // --- Configuration de la Luminosité ---
        if (filtreLuminosite != null && sliderLuminosite != null)
        {
            sliderLuminosite.minValue = 0f;
            sliderLuminosite.maxValue = 1f;
            sliderLuminosite.value = 1f;

            sliderLuminosite.onValueChanged.AddListener(ChangerLuminosite);
        }
        if (Luminosite_Globale.Instance != null)
        {
            sliderLuminosite.value = Luminosite_Globale.Instance.ObtenirValeurActuelle();
        }
    }

    public void ChangerVolumeGeneral(float valeur)
    {
        // On applique la valeur du slider à la musique de fond
        if (musiqueFond != null)
        {
            musiqueFond.volume = valeur;
        }

        // On applique EXACTEMENT la même valeur à la source des SFX !
        if (sfxSource != null)
        {
            sfxSource.volume = valeur;
        }
    }

    // Remplace l'ancienne fonction dans Gestion_Options par celle-là :
    public void ChangerLuminosite(float valeur)
    {
        // On communique directement avec l'objet immortel
        if (Luminosite_Globale.Instance != null)
        {
            Luminosite_Globale.Instance.AppliquerLuminosite(valeur);
        }
    }
}