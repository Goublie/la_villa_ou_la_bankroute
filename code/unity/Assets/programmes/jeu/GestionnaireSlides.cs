using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GestionnaireSlides : MonoBehaviour
{
    public List<Slider> listeSliders = new List<Slider>();
    private float maxTotalSlides = 100f;
    private float currentTotalSlides = 0f;

    void Start()
    {
        foreach (Slider slider in listeSliders)
        {
            // Configuration des sliders
            slider.minValue = 0f;
            slider.maxValue = maxTotalSlides;

            slider.onValueChanged.AddListener((valeur) => onSliderModif(slider, valeur));
        }
    }

    void onSliderModif(Slider _slider, float _valeur)
    {
        //Calcul du total des sliders
        currentTotalSlides = 0f;
        foreach (Slider slider in listeSliders)
        {
            if (slider != _slider)
            {
                currentTotalSlides += slider.value;
            }
        }
        
        //Vérification que le total ne dépasse pas le maximum
        if (currentTotalSlides + _valeur > maxTotalSlides)
        {
            _slider.value = maxTotalSlides - currentTotalSlides;
        }
    }
}
