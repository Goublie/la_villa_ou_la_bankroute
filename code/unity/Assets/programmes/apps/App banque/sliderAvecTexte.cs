using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderAvecTexte : MonoBehaviour
{

    public Slider slider;
    public TextMeshProUGUI textCell;

    private int SoldeCompte;

    void Start()
    {
        slider.onValueChanged.AddListener((valeur) => actualiseMontant(textCell, valeur));
    }

    // Met à jour le montant affiché dans la case en fonction de la valeur du slider
    void actualiseMontant(TextMeshProUGUI cell, float valeur)
    {
        if (cell == null) return;

        float montant = valeur * SoldeCompte;
        
        cell.text = montant.ToString("F2") + "€";
    }

}
