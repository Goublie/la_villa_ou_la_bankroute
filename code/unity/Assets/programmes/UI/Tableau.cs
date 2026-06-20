using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Tableau : MonoBehaviour
{
    public List<Ligne> tableau;    

    [Tooltip("Ligne d'en-tête statique (optionnelle) à exclure de la liste de données.")]
    public Ligne ligneEnTete;

    [Tooltip("Laissez vide pour utiliser les largeurs par défaut du prefab Ligne. Sinon, renseignez la largeur en pixels pour chaque colonne (-1 pour flexible/étirable).")]
    public List<float> largeursColonnes;

    [Header("Apparence Globale")]
    public Color couleurFondCases = Color.white;
    public Color couleurLigne = Color.black;
    public Color couleurTexte = Color.black;

    protected virtual void OnValidate()
    {
        AppliquerApparence();
    }

    public virtual void AppliquerApparence()
    {
        Image imgTableau = GetComponent<Image>();
        if (imgTableau != null) imgTableau.color = couleurLigne;

        Ligne[] toutesLignes = GetComponentsInChildren<Ligne>(true);
        foreach (Ligne l in toutesLignes)
        {
            if (l != null) l.SetApparence(couleurFondCases, couleurLigne, couleurTexte);
        }
    }

    public void Start()
    {
        tableau = new List<Ligne>(GetComponentsInChildren<Ligne>());

        if (ligneEnTete != null)
        {
            tableau.Remove(ligneEnTete);
            AppliquerConfigurationColonnes(ligneEnTete);
        }

        // Appliquer la configuration des colonnes aux lignes existantes
        foreach (Ligne l in tableau)
        {
            AppliquerConfigurationColonnes(l);
        }

        //On vide le tableau au début du jeu
        Vider();
    }

    public void AppliquerConfigurationColonnes(Ligne ligne)
    {
        if (largeursColonnes == null || largeursColonnes.Count == 0) return;

        Case[] casesLigne = ligne.GetComponentsInChildren<Case>(true);
        for (int i = 0; i < casesLigne.Length && i < largeursColonnes.Count; i++)
        {
            LayoutElement le = casesLigne[i].GetComponent<LayoutElement>();
            if (le == null)
            {
                le = casesLigne[i].gameObject.AddComponent<LayoutElement>();
            }

            float width = largeursColonnes[i];
            if (width >= 0f)
            {
                le.preferredWidth = width;
                le.flexibleWidth = 0f; // Fixe
            }
            else
            {
                le.preferredWidth = -1f;
                le.flexibleWidth = 1f; // S'étire pour remplir le reste
            }
        }
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

    //Ajoute les valeurs dans la première ligne vide du tableau et renvoie true en cas de réussite
    public virtual bool Add(params object[] valeurs)
    {
        foreach (Ligne l in tableau)
        {
            if (l.EstVide())
            {
                l.Set(valeurs);
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

    //Modifie la ligne à l'indice indice avec les valeurs en argument
    public void Set(int indice, params object[] valeurs)
    {
        tableau[indice].Set(valeurs);
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
