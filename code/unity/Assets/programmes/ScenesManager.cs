using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    // Singleton pour un accès global facile : ScenesManager.Instance.ChargerJeu()
    public static ScenesManager Instance { get; private set; }

    [SerializeField] private GameData G;

    private void Awake()
    {
        // On s'assure qu'il n'y a qu'une seule instance et qu'elle ne meurt jamais
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Header("Données de jeu à réinitialiser")]
    public GameData gameData;

    public void ChargerJeu()
    {
        Debug.Log("ScenesManager : Chargement de 'Jeu'...");
        if (gameData != null)
        {
            gameData.ResetData();
            Debug.Log("ScenesManager : Données de jeu réinitialisées.");
        }
        else
        {
            Debug.LogWarning("ScenesManager : GameData manquant, impossible de réinitialiser.");
        }
        SceneManager.LoadScene("Jeu");
    }


    // Charge la scène de bilan annuel (Introspection).
    public void ChargerIntrospection()
    {
        Debug.Log("Passage à l'analyse annuelle (Scène End)...");
        SceneManager.LoadScene("End");    
    }

    // Retourne au menu principal.
    public void ChargerMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    // Quitte proprement l'application.
    public void QuitterJeu()
    {
        Debug.Log("Fermeture du jeu.");
        Application.Quit();
    }
}
