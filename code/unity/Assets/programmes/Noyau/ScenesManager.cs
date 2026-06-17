using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Point de navigation central entre les scenes principales du jeu.
/// </summary>
/// <remarks>
/// <see cref="InitJeu"/> demarre une nouvelle partie et reinitialise les
/// donnees persistantes. Les autres methodes ne font que changer de scene afin
/// de conserver l'etat courant, notamment au retour de Retrospective.
/// </remarks>
public class ScenesManager : MonoBehaviour
{
    private static ScenesManager _instance;

    [Header("Donnees de jeu a reinitialiser")]
    public GameData gameData;

    public static ScenesManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ScenesManager>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("ScenesManager (Auto-Generated)");
                    _instance = go.AddComponent<ScenesManager>();
                }
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Reinitialise l'etat metier puis charge la scene Jeu pour une nouvelle partie.
    /// </summary>
    /// <remarks>
    /// Effet de bord : appelle <see cref="GameData.ResetData"/> lorsque les
    /// donnees sont assignees dans la scene Menu.
    /// </remarks>
    public void InitJeu()
    {
        Debug.Log("ScenesManager : Initialisation d'une nouvelle partie...");
        if (gameData != null)
        {
            gameData.ResetData();
            Debug.Log("ScenesManager : Donnees de jeu reinitialisees.");
        }
        else
        {
            Debug.LogWarning("ScenesManager : GameData manquant, impossible de reinitialiser.");
        }

        ChargerJeu();
    }

    /// <summary>
    /// Charge la scene Jeu sans reinitialiser les donnees de la partie.
    /// </summary>
    public void ChargerJeu()
    {
        Debug.Log("ScenesManager : Chargement de 'Jeu'...");
        SceneManager.LoadScene("Jeu");
    }

    /// <summary>
    /// Charge la scene de bilan annuel Retrospective.
    /// </summary>
    public void ChargerIntrospection()
    {
        Debug.Log("ScenesManager : Chargement de 'Retrospective'...");
        SceneManager.LoadScene("Retrospective");
    }

    /// <summary>
    /// Retourne au menu principal sans modifier l'etat courant du jeu.
    /// </summary>
    public void ChargerMenu()
    {
        Debug.Log("ScenesManager : Chargement de 'Menu'...");
        SceneManager.LoadScene("Menu");
    }

    /// <summary>
    /// Quitte l'application. Sans effet visible dans l'editeur Unity.
    /// </summary>
    public void QuitterJeu()
    {
        Debug.Log("ScenesManager : Fermeture.");
        Application.Quit();
    }
}
