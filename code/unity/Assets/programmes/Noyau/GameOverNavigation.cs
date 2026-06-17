using UnityEngine;

/// <summary>
/// Actions de navigation exposees par la scene GameOver.
/// </summary>
/// <remarks>
/// La scene peut etre ouverte depuis une partie existante ou directement en
/// test. Les appels passent donc par <see cref="ScenesManager.Instance"/>, qui
/// reutilise l'instance persistante quand elle existe et sait charger les
/// scenes sans reinitialiser l'etat courant par accident.
/// </remarks>
public class GameOverNavigation : MonoBehaviour
{
    /// <summary>
    /// Retourne au menu principal.
    /// </summary>
    public void RetourMenu()
    {
        ScenesManager.Instance.ChargerMenu();
    }

    /// <summary>
    /// Demande la fermeture de l'application.
    /// </summary>
    public void Quitter()
    {
        ScenesManager.Instance.QuitterJeu();
    }
}
