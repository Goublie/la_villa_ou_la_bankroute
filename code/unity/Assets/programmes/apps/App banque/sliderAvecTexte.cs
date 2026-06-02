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
        argent montantCalcule = soldeCompte * valeur;
        
        argent depense = -montantCalcule; 
        
        // 2. Mise à jour "Invisible" (Base de données)
        G.comptes["courant"].GetHistorique().ModifieOuAjoute(nom, depense);
        
        // 3. Mise à jour "Visuelle" (Le Tableau)
        if (tableauUI != null)
        {
            // On essaie de mettre à jour le texte de la ligne.
            // Si MettreAJourLigne renvoie "false", c'est que la ligne n'existe pas !
            if (tableauUI.MettreAJourLigne(nom, depense.ToString()) == false)
            {
                // Dans ce cas, on demande au tableau de créer une nouvelle ligne
                tableauUI.Add(nom, depense.ToString(), "");
            }
        }
    }
}