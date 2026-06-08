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
        ActionPlay.OnMoisPasse += RecupSolde;
    }
    
    void OnDisable()
    {
        ActionPlay.OnMoisPasse -= RecupSolde;
    }
    
    void Start()
    {
        RecupSolde();
        if (slider != null)
        {
            slider.onValueChanged.AddListener((valeur) => ActualiseMontant(valeur));
        }
    }

    private void RecupSolde()
    {
        if (G != null && G.joueur != null && G.joueur.comptes != null && G.joueur.comptes.ContainsKey("courant"))
        {
            soldeCompte = G.joueur.comptes["courant"].GetSolde();
        }
    }

    void ActualiseMontant(float valeur)
    {
        if (G == null || G.joueur == null || G.joueur.comptes == null || !G.joueur.comptes.ContainsKey("courant"))
        {
            Debug.LogWarning("[SliderAvecTexte] GameData, joueur ou compte courant null lors de l'action.");
            return;
        }

        if (slider == null) return;
        float vraieValeur = slider.value;
    
        argent montantCalcule = soldeCompte * vraieValeur;
        
        argent depense = -montantCalcule; 
        
        G.joueur.comptes["courant"].GetHistorique().ModifieOuAjoute(nom, depense);
        
        // 3. Mise à jour "Visuelle" (Le Tableau)
        G.joueur.comptes["courant"].CalculSortie();
        G.joueur.comptes["courant"].CalculSolde();

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