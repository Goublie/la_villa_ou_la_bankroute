using UnityEngine;
using UnityEngine.UI;
public class TableauScroll : Tableau
{

    [SerializeField] Transform conteneurLignes;
    [SerializeField] Ligne prefabLigne;

    public override bool Add(string text1, string text2="", string text3="")
    {
        // On cherche une ligne vide existant
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

        // Si toutes les lignes sont pleines.
        
        if (prefabLigne != null && conteneurLignes != null)
        {
            // On crée une nouvelle ligne enfant
            Ligne nouvelleLigne = Instantiate(prefabLigne, conteneurLignes, false);
            
            nouvelleLigne.transform.SetAsFirstSibling();

            // On remplit ses cases
            nouvelleLigne.Set(0, text1);
            nouvelleLigne.Set(1, text2);
            nouvelleLigne.Set(2, text3);
            
            // On l'ajoute à notre liste interne pour pouvoir la vider plus tard
            tableau.Add(nouvelleLigne); 
            
            return true;
        }
        else
        {
            Debug.LogError("Attention : Aucun préfabriqué 'Ligne' n'est assigné au Tableau !");
            return false;
        }
    }

    public override bool Add(Transaction transaction)
    {
        return Add(transaction.libelle, transaction.montant.ToString());
    }
}