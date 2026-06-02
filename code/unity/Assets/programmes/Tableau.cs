using UnityEngine;
using System.Collections.Generic;

public class Tableau : MonoBehaviour
{
    public List<Ligne> tableau;    

    public void Start()
    {
       tableau = new List<Ligne>(GetComponentsInChildren<Ligne>());

        //On vide le tableau au début du jeu
        Vider();
    }

    //Renvoie true si toutes les lignes sont vides
    public bool EstVide()
    {
        foreach (Ligne l in tableau)
        {
            if (!l.EstVide())
            {
                return false;
            }
        }
        return true;
    }

    //Vide le tableau
    public void Vider()
    {
        foreach (Ligne l in tableau)
        {
            if (l != null)
            {
                l.Vider();
            }
        }
    }

    //Renvoie le texte affiché dans la case à l'indice (y,x)
    public string get(int y, int x)
    {
        return tableau[y].Get(x);
    }

    //Ajoute le texte dans la première case de la première ligne vide du tableau et renvoie true en cas de réussite
    public virtual bool Add(string text1, string text2="", string text3="")
    {
        foreach (Ligne l in tableau)
        {
            if (l.EstVide())
            {
                l.Set(0, text1);
                l.Set(1, text2);
                l.Set(2, text3);
                return true;
            }
        }
        return false;
    }

    public virtual bool Add(Transaction transaction)
    {
        return Add(transaction.libelle, transaction.montant.ToString());
    }

    //Affiche le texte text dans la case à l'indice (y,x)
    public void Set(int indiceLigne, int indiceColonne, string text)
    {
        tableau[indiceLigne].Set(indiceColonne, text);
    }

    //Affiche un nombre dans la case à l'indice (y,x)
    public void Set(int indiceLigne, int indiceColonne, int data)
    {
        Set(indiceLigne, indiceColonne, data.ToString());
    }

    //Affiche un montant d'argent dans la case à l'indice (y,x)
    public void Set(int indiceLigne, int indiceColonne, argent data)
    {
        Set(indiceLigne, indiceColonne, data.ToString());
    }
}
