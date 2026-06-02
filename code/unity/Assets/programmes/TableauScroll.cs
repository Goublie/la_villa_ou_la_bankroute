using UnityEngine;
using UnityEngine.UI;

public class TableauScroll : Tableau
{
    [SerializeField] Ligne prefabLigne;
    
    [SerializeField] Transform conteneurLignes; 

    public override bool Add(string text1, string text2="", string text3="")
    {
        // On cherche une ligne vide existante
        foreach (Ligne l in tableau)
        {
            if (l.EstVide())
            {
                l.Set(0, text1);
                l.Set(1, text2);
                l.Set(2, text3);
                
                //On force cette ligne à remonter tout en haut visuellement
                l.transform.SetAsFirstSibling(); 
                
                return true;
            }
        }

        // Si toutes les lignes sont pleines, on instancie.
        if (prefabLigne != null && conteneurLignes != null)
        {
            Ligne nouvelleLigne = Instantiate(prefabLigne, conteneurLignes, false);
            
            //On place la nouvelle ligne tout en haut visuellement 
            nouvelleLigne.transform.SetAsFirstSibling();
            
            nouvelleLigne.Set(0, text1);
            nouvelleLigne.Set(1, text2);
            nouvelleLigne.Set(2, text3);
            
            tableau.Add(nouvelleLigne); 
            
            return true;
        }
        else
        {
            Debug.LogError("Attention : Le préfabriqué ou le conteneur n'est pas assigné !");
            return false;
        }
    }
}