using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class Ligne : MonoBehaviour
{

    private List<Case> cases;

    public void Start()
    {
        cases = new List<Case>(GetComponentsInChildren<Case>());
        //On vide la ligne au début du jeu
        vider();
    }

    public string get(int indice)
    {
        return cases[indice].getTexte();
    }

    //Renvoie true si chasues case est vide
    public bool estVide()
    {
        foreach (Case c in cases)
        {
            if (!c.estVide())
            {
                return false;
            }
        }
        return true;
    }

    public void vider()
    {
        foreach (Case c in cases)
        {
            if (c != null)
            {
                c.vider();
            }
        }
    }

    //Ajoute le texte dans la première case vide de la ligne et reuturne true en cas de réussite
    public bool add(string text)
    {
        foreach (Case c in cases)
        {
            if (c.estVide())
            {
                c.set(text);
                return true;
            }
        }
        return false;
    }

    //Ajoute le nombre dans la première case vide de la ligne
    public void add(int montant)
    {
        add(montant.ToString());
    }

    //Ajoute le montant d'argent dans la première case vide de la ligne
    public void add(argent montant)
    {
        add(montant.ToString());
    }

    //Affiche le texte text dans la case à l'indice indice
    public void set(int indice, string text)
    {
        cases[indice].set(text);
    }

    //Affiche un nombre dans la case à l'indice indice
    public void set(int indice, int montant)
    {
        set(indice, montant.ToString());
    }

    //Affiche un montant en centimes dans la case à l'indice indice
    public void set(int indice, argent montant)
    {
        set(indice, montant.ToString());
    }
}