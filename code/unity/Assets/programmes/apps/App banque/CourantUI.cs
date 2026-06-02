using UnityEngine;
using TMPro; 

public class CourantUI : MonoBehaviour
{
    public GameData G; // données du jeu
    
    private CompteBanquaire compteCrnt;
    [SerializeField] TableauScroll tab; 

    [SerializeField] TextMeshProUGUI solde;

    void Awake()
    {
        compteCrnt = G.comptes["courant"];
    }

    void OnEnable()
    {
        ActionPlay.moisPasse += ActualiseAffichage;
        
        if(compteCrnt != null) // Petite sécurité supplémentaire
        {
            compteCrnt.OnSoldeModifie += ActualiseAffichage;
        }
        
        ActualiseAffichage(); // On force le premier affichage à l'activation
    }

    void OnDisable()
    {
        ActionPlay.moisPasse -= ActualiseAffichage;
        
        if(compteCrnt != null)
        {
            compteCrnt.OnSoldeModifie -= ActualiseAffichage;
        }
    }

    private void ActualiseAffichage()
    {
        solde.text = compteCrnt.GetSolde().ToString();
        ActualiserTableau();
    }

    //Actualise l'affichage de la ligne à l'indice indice du tabeau d'affichage
    public void ActualiserAffichageLigne(int indice)
    {
        if (tab == null)
        {
            Debug.LogError("Le tableau n'est pas assigné dans le CourantUI.");
            return;
        }

        Transaction transac = compteCrnt.GetHistorique().GetHistorique()[indice];
        tab.Set(indice, transac);

    }

    public void ActualiserTableau()
    {
        Historique histo = compteCrnt.GetHistorique();
        tab.Vider();
        for(int i = 0; i < histo.GetSize(); i++)
        {
            tab.Add(histo.GetHistorique()[i]);
            Debug.Log("Tableau actualisé");
        }
    }
}