using UnityEngine;
using UnityEngine.UI;

public class TableauScroll : Tableau
{
    [SerializeField] Ligne prefabLigne;
    
    [SerializeField] Transform conteneurLignes; 

    public override void AppliquerApparence()
    {
        base.AppliquerApparence();
        if (conteneurLignes != null)
        {
            Image imgConteneur = conteneurLignes.GetComponent<Image>();
            if (imgConteneur != null) imgConteneur.color = couleurLigne;
        }
    }

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
            
            // Appliquer l'apparence globale
            nouvelleLigne.SetApparence(couleurFondCases, couleurLigne, couleurTexte);
            
            // Appliquer le bon nombre de colonnes et la configuration
            nouvelleLigne.AjusterNombreColonnes(nombreColonnes);
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