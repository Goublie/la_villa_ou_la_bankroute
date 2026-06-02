using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderAvecTexte : MonoBehaviour
{
    [SerializeField] private GameData G;
    public Slider slider;
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

    // Met à jour le montant affiché dans la case en fonction de la valeur du slider
    void ActualiseMontant(float valeur)
    {
        argent montant = valeur * soldeCompte;
        
        Debug.Log("valeur : " + valeur.ToString());
        Debug.Log("solde : " + soldeCompte.ToString());
    }
}
