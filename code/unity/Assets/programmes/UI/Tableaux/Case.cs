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
        AssurerComposantTexte();
        return composantTexte != null ? composantTexte.text : string.Empty;
    }

    // Affiche le texte text dans la case
    public void Set(string texte)
    {
        AssurerComposantTexte();
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
        if (composantTexte != null)
            Set("");
    }

    public void Awake()
    {
        AssurerComposantTexte();
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
        AssurerComposantTexte();
        if (composantTexte != null)
        {
            composantTexte.color = couleur;
        }
    }

    private void AssurerComposantTexte()
    {
        if (composantTexte == null)
        {
            composantTexte = GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }
}
