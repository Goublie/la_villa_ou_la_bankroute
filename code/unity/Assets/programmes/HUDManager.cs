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
        gameData.comptes["courant"].OnSoldeModifie += ActualiserAffichage;
        ActionPlay.moisPasse += ActualiserAffichage;
    }

   void OnDisable()
    {
        gameData.comptes["courant"].OnSoldeModifie -= ActualiserAffichage;
        ActionPlay.moisPasse -= ActualiserAffichage;
    }

    // Fonction qui rafraîchit l'interface à l'écran
    public void ActualiserAffichage()
    {
        if (gameData == null) return;

        if (texteArgent != null)
        {
            Debug.Log(gameData.comptes["courant"].GetSolde().ToString());
            texteArgent.text = gameData.comptes["courant"].GetSolde().ToString();
        }

        if (texteEnergie != null) 
            texteEnergie.text =  gameData.energie.ToString() + "%";

        if (texteSanteMentale != null) 
            texteSanteMentale.text = gameData.santeMentale.ToString() + "/100";
    }
}