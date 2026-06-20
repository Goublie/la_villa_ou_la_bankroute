using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class Ligne : MonoBehaviour
{

    private List<Case> cases;

    [Header("Apparence")]
    [SerializeField] protected Color couleurFondCases = Color.white;
    [SerializeField] private Color couleurLigne = Color.black;
    [SerializeField] private Color couleurTexte = Color.black;

    public virtual void Awake()
    {
        cases = new List<Case>(GetComponentsInChildren<Case>());
        AppliquerCouleurs();
    }

    private void OnValidate()
    {
        // Permet de voir le changement de couleur directement dans l'éditeur
        AppliquerCouleurs();
    }

    private void AppliquerCouleurs()
    {
        Image fond = GetComponent<Image>();
        if (fond != null) fond.color = couleurLigne;

        if (cases == null || cases.Count == 0)
        {
            cases = new List<Case>(GetComponentsInChildren<Case>());
        }

        foreach (Case c in cases)
        {
            if (c != null)
            {
                c.SetCouleur(couleurFondCases);
                c.SetCouleurTexte(couleurTexte);
            }
        }
    }

    public void AppliquerCouleurCases(Color couleur)
    {
        if (cases == null) return;
        foreach (Case c in cases)
        {
            if (c != null) c.SetCouleur(couleur);
        }
    }

    public void SetApparence(Color fondCases, Color ligneBordure, Color texte)
    {
        this.couleurFondCases = fondCases;
        this.couleurLigne = ligneBordure;
        this.couleurTexte = texte;
        AppliquerCouleurs();
    }

    public string Get(int indice)
    {
        return cases[indice].GetTexte();
    }

    //Renvoie true si chasues case est vide
    public bool EstVide()
    {
        foreach (Case c in cases)
        {
            if (!c.EstVide())
            {
                return false;
            }
        }
        return true;
    }

    public void Vider()
    {
        foreach (Case c in cases)
        {
            if (c != null)
            {
                c.Vider();
            }
        }
    }

    //Ajoute le texte dans la première case vide de la ligne et reuturne true en cas de réussite
    public bool Add(string text)
    {
        foreach (Case c in cases)
        {
            if (c.EstVide())
            {
                c.Set(text);
                return true;
            }
        }
        return false;
    }

    //Ajoute le nombre dans la première case vide de la ligne
    public void Add(int montant)
    {
        Add(montant.ToString());
    }

    //Ajoute le montant d'argent dans la première case vide de la ligne
    public void Add(argent montant)
    {
        Add(montant.ToString());
    }

    //Affiche le texte text dans la case à l'indice indice
    public void Set(int indice, string text)
    {
        cases[indice].Set(text);
    }

    //Affiche un nombre dans la case à l'indice indice
    public void Set(int indice, int montant)
    {
        Set(indice, montant.ToString());
    }

    //Affiche un montant en centimes dans la case à l'indice indice
    public void Set(int indice, argent montant)
    {
        Set(indice, montant.ToString());
    }

    //Set toutes les valeurs de la ligne dans l'ordre des arguments de manière flexible
    public void Set(params object[] valeurs)
    {
        for (int i = 0; i < valeurs.Length && i < cases.Count; i++)
        {
            if (valeurs[i] == null)
            {
                cases[i].Vider();
            }
            else if (valeurs[i] is argent mt)
            {
                cases[i].Set(mt);
            }
            else if (valeurs[i] is int valInt)
            {
                cases[i].Set(valInt);
            }
            else
            {
                cases[i].Set(valeurs[i].ToString());
            }
        }
    }
}