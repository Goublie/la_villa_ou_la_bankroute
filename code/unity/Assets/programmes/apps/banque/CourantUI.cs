using UnityEngine;
using TMPro; 

public class CourantUI : MonoBehaviour
{
    public GameData G; 
    
    private CompteBanquaire compteCrnt;
    [SerializeField] TableauScroll tab; 

    [SerializeField] TextMeshProUGUI solde;

    void Awake()
    {
        compteCrnt = G.joueur.comptes["courant"];
    }

    void OnEnable()
    {
        // Quand le mois passe, on fait la mise à jour TOTALE (Texte + Tableau)
        ActionPlay.OnMoisPasse += NouveauMois;
        
        if(compteCrnt != null) 
        {
            // Quand le solde bouge (via slider), on met JUSTE le texte à jour
            compteCrnt.OnSoldeModifie += RafraichirJusteLeSolde;
        }
        
        NouveauMois(); // On force l'affichage complet à l'ouverture de l'onglet
    }

    void OnDisable()
    {
        ActionPlay.OnMoisPasse -= NouveauMois;
        
        if(compteCrnt != null)
        {
            compteCrnt.OnSoldeModifie -= RafraichirJusteLeSolde;
        }
    }

    private void RafraichirJusteLeSolde()
    {
        solde.text = compteCrnt.GetSolde().ToString();
    }

    private void NouveauMois()
    {
        RafraichirJusteLeSolde();
        ActualiserTableau();
    }

    public void ActualiserTableau()
    {
        Historique histo = compteCrnt.GetHistorique();
        tab.Vider();
        
        // On lit la liste à l'envers pour que le plus récent finisse en haut
        for(int i = histo.GetSize() - 1; i >= 0; i--)
        {
            tab.Add(histo.GetHistorique()[i]);
        }
    }
}