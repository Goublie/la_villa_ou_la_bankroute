using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Case : MonoBehaviour
{
    private TextMeshProUGUI composantTexte;

    //renvoie le texte affiché dans la case
    public string getTexte()
    {
        return composantTexte.text;
    }

    // Affiche le texte text dans la case
    public void set(string texte)
    {
        composantTexte.text = texte;
    }

    //Affiche un nombre dans la case
    public void set(int montant)
    {
        set(montant.ToString());
    }

    //Affiche un montant d'argent
    public void set(argent montant)
    {
        set(montant.ToString());
    }

    //Renvoie vrai si la case est vide
    public bool estVide()
    {
        return getTexte() == "";
    }

    //Vide la case
    public void vider()
    {
        set("");
    }

    public void Start()
    {
        composantTexte = GetComponentInChildren<TextMeshProUGUI>();
        //On vide la case au début du jeu
        vider();
    }
}
