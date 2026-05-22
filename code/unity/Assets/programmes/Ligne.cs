using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class Ligne : MonoBehaviour
{

    public List<TextMeshProUGUI> ligne;

    public void Start()
    {
        ligne = new List<TextMeshProUGUI>(GetComponentsInChildren<TextMeshProUGUI>());
    }

    public void set(int indice, string text)
    {
        ligne[indice].text = text;
    }

    public void set(int indice, int data)
    {
        set(indice, data.ToString() + "€");
    }
}
