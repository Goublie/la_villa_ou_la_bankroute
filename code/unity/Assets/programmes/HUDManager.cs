using UnityEngine;
using TMPro; 

public class HUDManager : MonoBehaviour
{

    public GameData gameData; // Référence vers ton ScriptableObject

    public TextMeshProUGUI texteArgent;
    public TextMeshProUGUI texteEnergie;
    public TextMeshProUGUI texteSanteMentale;

    void Start()
    {
        ActualiserAffichage();
    }


    // Fonction qui rafraîchit l'interface à l'écran
    public void ActualiserAffichage()
    {
        if (gameData == null) return;

        if (texteArgent != null)
        {
            int euros = gameData.argent / 100; // Convertir les centimes en euros
            int centimes = gameData.argent % 100; // Récupérer les centimes restants
            string affichageArgent = euros.ToString() + "," + (centimes<10 ? "0" :"" )+ centimes.ToString() + " €";
            texteArgent.text = affichageArgent;
        }

        if (texteEnergie != null) 
            texteEnergie.text =  gameData.energie.ToString() + "%";

        if (texteSanteMentale != null) 
            texteSanteMentale.text = gameData.santeMentale.ToString() + "/100";
    }
}