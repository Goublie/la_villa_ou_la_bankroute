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
        if(transaction.montant.centimes <= 0)
        {
            return Add(transaction.libelle, "", transaction.montant.ToString());
        }
        return Add(transaction.libelle, transaction.montant.ToString());
    }

    //Met à jour la ligne du tableau dont le libelle correspond à libelleRecherche avec le nouveau montant et renvoie true en cas de réussite
    public bool MettreAJourLigne(string libelleRecherche, argent nouveauMontant)
    {
        foreach (Ligne l in tableau)
        {
            // On vérifie que la ligne n'est pas vide pour éviter les erreurs
            if (!l.EstVide() && l.Get(0) == libelleRecherche) 
            {
                if(nouveauMontant.centimes <= 0)
                {
                    l.Set(2, nouveauMontant); // Si le montant est négatif ou nul, on le met dans la troisième colonne
                    return true;
                }
                l.Set(1, nouveauMontant); // On met à jour la colonne du montant
                return true; 
            }
        }
        return false;
    }

    //Affiche le texte text dans la case à l'indice (y,x)
    public void Set(int indiceLigne, int indiceColonne, string text)
    {
        tableau[indiceLigne].Set(indiceColonne, text);
    }

    //Modifie la ligne à l'indice indice avec les textes en argument
    public void Set(int indice, string text1, string text2="", string text3="")
    {
        tableau[indice].Set(text1, text2, text3);
    }

    public void Set(int indice, Transaction transac)
    {
        Set(indice, transac.libelle, transac.montant.ToString());
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
