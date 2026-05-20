using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class sliderAvecTexte : MonoBehaviour
{

    public Slider slider;
    public TMP_InputField inputField;

    void Start()
    {
        slider.onValueChanged.AddListener(actualiseInputField);
        inputField.onValueChanged.AddListener(actualiseSlider);
    }

    void actualiseInputField(float valeur)
    {
        inputField.SetTextWithoutNotify((valeur * 100).ToString());
    }

    void actualiseSlider(string valeur)
    {
        if (float.TryParse(valeur, out float resultat))
        {
            slider.SetValueWithoutNotify(resultat / 100);
        }
    }
}
