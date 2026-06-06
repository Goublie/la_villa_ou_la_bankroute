using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public GameData gameData;
    public TextMeshProUGUI texteArgent; // C'est ta fameuse barre bleue centrale
    public TextMeshProUGUI texteEnergie;
    public TextMeshProUGUI texteSanteMentale;

    // L'Update vérifie les données en permanence, l'affichage sera toujours en temps réel
    void Update()
    {
        if (gameData == null) return;

        if (texteArgent != null)
        {
            // 1. On récupère le texte du salaire (ex: "4833,00 €")
            string texteBrut = gameData.salaire.ToString();

            // 2. On supprime TOUS les symboles '€' existants pour nettoyer la chaîne
            string texteNettoye = texteBrut.Replace("€", "").Trim();

            // 3. On affiche le score propre avec UN SEUL symbole € à la fin
            texteArgent.text = texteNettoye + " €";
        }

        if (texteEnergie != null)
            texteEnergie.text = gameData.energie.ToString() + "%";

        if (texteSanteMentale != null)
            texteSanteMentale.text = gameData.santeMentale.ToString() + "/100";
    }
}