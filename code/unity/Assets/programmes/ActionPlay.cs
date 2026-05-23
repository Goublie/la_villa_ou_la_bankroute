using UnityEngine;

public class ActionPlay : MonoBehaviour
{
    public GameData gameData; // Référence vers ton ScriptableObject
    public HUDManager hudManager; // Référence vers le HUDManager pour rafraîchir l'affichage

    public void incrementerMois()
    {
        if (gameData != null)
        {
            gameData.moisPasse++;
        }
        if (hudManager != null)
        {
            hudManager.ActualiserAffichage();
        }
    }
}
