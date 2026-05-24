using UnityEngine;
using System.Collections.Generic;

public class Tableau : MonoBehaviour
{
    public List<Ligne> tableau;    

    public void Start()
    {
       tableau = new List<Ligne>(GetComponentsInChildren<Ligne>());

        //On vide le tableau au début du jeu
        vider();
    }

    //Renvoie true si toutes les lignes sont vides
    public bool estVide()
    {
        foreach (Ligne l in tableau)
        {
            if (!l.estVide())
            {
                return false;
            }
        }
        return true;
    }

    //Vide le tableau
    public void vider()
    {
        foreach (Ligne l in tableau)
        {
            l.vider();
        }
    }

    //Renvoie le texte affiché dans la case à l'indice (y,x)
    public string get(int y, int x)
    {
        return tableau[y].get(x);
    }

    //Ajoute le texte dans la première case de la première ligne vide du tableau et renvoie true en cas de réussite
    public bool add(string text)
    {
        foreach (Ligne l in tableau)
        {
            if (l.add(text))
            {
                return true;
            }
        }
        return false;
    }

    //Affiche le texte text dans la case à l'indice (y,x)
    public void set(int indiceLigne, int indiceColonne, string text)
    {
        tableau[indiceLigne].set(indiceColonne, text);
    }

    //Affiche un nombre dans la case à l'indice (y,x)
    public void set(int indiceLigne, int indiceColonne, int data)
    {
        set(indiceLigne, indiceColonne, data.ToString());
    }

    //Affiche un montant d'argent dans la case à l'indice (y,x)
    public void set(int indiceLigne, int indiceColonne, argent data)
    {
        set(indiceLigne, indiceColonne, data.ToString());
    }
}
