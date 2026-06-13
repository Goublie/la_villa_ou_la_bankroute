using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class Ligne : MonoBehaviour
{

    private List<Case> cases;

    public void Awake()
    {
        cases = new List<Case>(GetComponentsInChildren<Case>());
        //On vide la ligne au début du jeu
        Vider();
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