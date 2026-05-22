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

    //Affiche le texte text dans la case à l'indice indice
    public void set(int indice, string text)
    {
        ligne[indice].text = text;
    }

    //Affiche un nombre dans la case à l'indice indice
    public void set(int indice, int montant)
    {
        set(indice, montant.ToString());
    }

    //Affiche un montant en centimes dans la case à l'indice indice
    public void setMoney(int indice, int montant)
    {
        int centimes = montant % 100;
        int euros = montant / 100;
        string centimesStr = centimes < 10 ? "0" + centimes.ToString() : centimes.ToString();
        set(indice, euros.ToString() + "," + centimesStr + "€");
    }
}
