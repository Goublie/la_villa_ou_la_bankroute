using UnityEngine;
using TMPro; 

public class HUDManager : MonoBehaviour
{

    public GameData gameData;
    public TextMeshProUGUI texteArgent;
    public TextMeshProUGUI texteEnergie;
    public TextMeshProUGUI texteSanteMentale;

    void Start()
    {
        ActualiserAffichage();
    }

    void OnEnable()
    {
        gameData.joueur.comptes["courant"].OnSoldeModifie += ActualiserAffichage;
        ActionPlay.OnMoisPasse += ActualiserAffichage;
    }

   void OnDisable()
    {
        gameData.joueur.comptes["courant"].OnSoldeModifie -= ActualiserAffichage;
        ActionPlay.OnMoisPasse -= ActualiserAffichage;
    }

    // Fonction qui rafraîchit l'interface à l'écran
    public void ActualiserAffichage()
    {
        if (gameData == null) return;

        if (texteArgent != null && gameData.joueur != null && gameData.joueur.comptes != null && gameData.joueur.comptes.ContainsKey("courant"))
        {
            Debug.Log(gameData.joueur.comptes["courant"].GetSolde().ToString());
            texteArgent.text = gameData.joueur.comptes["courant"].GetSolde().ToString();
        }

        if (texteEnergie != null && gameData.joueur != null) 
            texteEnergie.text =  gameData.joueur.energie.ToString() + "%";

        if (texteSanteMentale != null && gameData.joueur != null) 
            texteSanteMentale.text = gameData.joueur.santeMentale.ToString() + "/100";
    }
}