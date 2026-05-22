using UnityEngine;
using UnityEngine.UI;

public class Gestion_Options : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider sliderSon;
    [SerializeField] private Slider sliderLuminosite;

    [Header("Composants à modifier")]
    [SerializeField] private AudioSource musiqueFond;
    [SerializeField] private Image filtreLuminosite;

    void Start()
    {
        // --- Configuration du Son ---
        if (musiqueFond != null && sliderSon != null)
        {
            // On initialise le slider avec le volume actuel de la musique
            sliderSon.value = musiqueFond.volume;

            // On écoute les changements du slider en temps réel
            sliderSon.onValueChanged.AddListener(ChangerVolume);
        }

        // --- Configuration de la Luminosité ---
        if (filtreLuminosite != null && sliderLuminosite != null)
        {
            // On suppose que la luminosité par défaut est au max (1)
            // Donc le filtre noir a une opacité de 0 (invisible)
            sliderLuminosite.minValue = 0f;
            sliderLuminosite.maxValue = 1f;
            sliderLuminosite.value = 1f;

            sliderLuminosite.onValueChanged.AddListener(ChangerLuminosite);
        }
    }

    public void ChangerVolume(float valeur)
    {
        if (musiqueFond != null)
        {
            musiqueFond.volume = valeur;
        }
    }

    public void ChangerLuminosite(float valeur)
    {
        if (filtreLuminosite != null)
        {
            Color couleurActuelle = filtreLuminosite.color;

            // Plus le slider est haut (proche de 1), plus l'alpha est bas (proche de 0 = lumineux)
            // Plus le slider est bas (proche de 0), plus l'alpha est haut (proche de 0.8 = sombre)
            couleurActuelle.a = Mathf.Lerp(0.8f, 0f, valeur);

            filtreLuminosite.color = couleurActuelle;
        }
    }
}