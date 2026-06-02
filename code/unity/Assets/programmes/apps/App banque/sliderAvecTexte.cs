using UnityEngine;
using UnityEngine.UI;

public class SliderAvecTexte : MonoBehaviour
{
    [SerializeField] private GameData G;
    public Slider slider;
    
    [SerializeField] private string nom; 
    
    [SerializeField] private Tableau tableauUI; 
    
    private argent soldeCompte;

    void OnEnable()
    {
        ActionPlay.moisPasse += RecupSolde;
    }
    
    void OnDisable()
    {
        ActionPlay.moisPasse -= RecupSolde;
    }
    
    void Start()
    {
        RecupSolde();
        slider.onValueChanged.AddListener((valeur) => ActualiseMontant(valeur));
    }

    private void RecupSolde()
    {
        if(G != null)
        {
            soldeCompte = G.comptes["courant"].GetSolde();
        }
    }

    void ActualiseMontant(float valeur)
    {
        float vraieValeur = slider.value;
    
        argent montantCalcule = soldeCompte * vraieValeur;
        
        argent depense = -montantCalcule; 
        

        G.comptes["courant"].GetHistorique().ModifieOuAjoute(nom, depense);
        
        // 3. Mise à jour "Visuelle" (Le Tableau)
        G.comptes["courant"].CalculSortie();
        G.comptes["courant"].CalculSolde();

        Transaction transaction = new Transaction(nom, depense);
        if (tableauUI != null)
        {
            // Si MettreAJourLigne renvoie "false", c'est que la ligne n'existe pas !
            if (!tableauUI.MettreAJourLigne(nom, depense))
            {
                // Dans ce cas, on demande au tableau de créer une nouvelle ligne
                tableauUI.Add(transaction);
            }
        }
    }
}