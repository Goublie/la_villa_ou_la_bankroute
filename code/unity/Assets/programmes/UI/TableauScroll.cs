using UnityEngine;
using UnityEngine.UI;

public class TableauScroll : Tableau
{
    [SerializeField] Ligne prefabLigne;
    
    [SerializeField] Transform conteneurLignes; 

    public override bool Add(params object[] valeurs)
    {
        // On cherche une ligne vide existante
        foreach (Ligne l in tableau)
        {
            if (l.EstVide())
            {
                l.Set(valeurs);
                
                //On force cette ligne à remonter tout en haut visuellement
                l.transform.SetAsFirstSibling(); 
                
                return true;
            }
        }

        // Si toutes les lignes sont pleines, on instancie.
        if (prefabLigne != null && conteneurLignes != null)
        {
            Ligne nouvelleLigne = Instantiate(prefabLigne, conteneurLignes, false);
            
            // Appliquer la configuration des colonnes à la nouvelle ligne
            AppliquerConfigurationColonnes(nouvelleLigne);
            
            //On place la nouvelle ligne tout en haut visuellement 
            nouvelleLigne.transform.SetAsFirstSibling();
            
            nouvelleLigne.Set(valeurs);
            
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