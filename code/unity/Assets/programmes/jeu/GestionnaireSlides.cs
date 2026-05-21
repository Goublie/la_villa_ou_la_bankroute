using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GestionnaireSlides : MonoBehaviour
{
    public List<SliderAvecTexte> listeSliders = new List<SliderAvecTexte>();
    private float maxTotalSlides = 100f;
    private float currentTotalSlides = 0f;

    void Start()
    {
        //On récupère
        listeSliders = new List<SliderAvecTexte>(GetComponentsInChildren<SliderAvecTexte>());

        foreach (SliderAvecTexte sliderT in listeSliders)
        {
            // Configuration des sliders
            sliderT.slider.minValue = 0f;
            sliderT.slider.maxValue = maxTotalSlides;

            sliderT.slider.onValueChanged.AddListener((valeur) => onSliderModif(sliderT, valeur));
        }
    }

    void onSliderModif(SliderAvecTexte _sliderT, float _valeur)
    {
        //Calcul du total des sliders
        currentTotalSlides = 0f;
        foreach (SliderAvecTexte sliderT in listeSliders)
        {
            if (sliderT != _sliderT)
            {
                currentTotalSlides += sliderT.slider.value;
            }
        }
        
        //Vérification que le total ne dépasse pas le maximum
        if (currentTotalSlides + _valeur > maxTotalSlides)
        {
            _sliderT.slider.value = maxTotalSlides - currentTotalSlides;
        }
    }
}
