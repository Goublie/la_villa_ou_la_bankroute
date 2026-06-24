using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Case : MonoBehaviour
{
    private TextMeshProUGUI composantTexte;
    private Image fondImage;

    //renvoie le texte affiché dans la case
    public string GetTexte()
    {
        return composantTexte.text;
    }

    // Affiche le texte text dans la case
    public void Set(string texte)
    {
        if (composantTexte == null) composantTexte = GetComponentInChildren<TextMeshProUGUI>();
        if (composantTexte != null)
        {
            composantTexte.text = texte;
        }
    }

    //Affiche un nombre dans la case
    public void Set(int montant)
    {
        Set(montant.ToString());
    }

    //Affiche un montant d'argent
    public void Set(argent montant)
    {
        Set(montant.ToString());
    }

    //Renvoie vrai si la case est vide
    public bool EstVide()
    {
        return GetTexte() == "";
    }

    //Vide la case
    public void Vider()
    {
        if (composantTexte == null) composantTexte = GetComponentInChildren<TextMeshProUGUI>();
        if (composantTexte != null)
            Set("");
    }

    public void Awake()
    {
        composantTexte = GetComponentInChildren<TextMeshProUGUI>();
        fondImage = GetComponent<Image>();
    }

    public void SetCouleur(Color couleur)
    {
        if (fondImage == null) fondImage = GetComponent<Image>();
        if (fondImage != null)
        {
            fondImage.color = couleur;
        }
    }

    public void SetCouleurTexte(Color couleur)
    {
        if (composantTexte == null) composantTexte = GetComponentInChildren<TextMeshProUGUI>();
        if (composantTexte != null)
        {
            composantTexte.color = couleur;
        }
    }
}
