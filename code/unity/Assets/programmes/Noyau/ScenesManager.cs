using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScenesManager : MonoBehaviour
{
    private static ScenesManager _instance;

    [Header("Données de jeu à réinitialiser")]
    public GameData gameData;

    private AsyncOperation _preloadedSceneOperation;
    private string _preloadedSceneName;
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


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[ScenesManager] Scène chargée : {scene.name}. Planification du préchargement différé...");
        
        // Nettoyage de l'opération terminée
        _preloadedSceneOperation = null;
        _preloadedSceneName = null;

        // Lancement asynchrone différé à la frame suivante
        StopAllCoroutines();
        StartCoroutine(PlanifierPrechargementRoutine(scene.name));
    }

    private IEnumerator PlanifierPrechargementRoutine(string currentSceneName)
    {
        // On attend la frame suivante pour s'assurer que Unity ait complètement terminé la transition actuelle
        yield return null;

        if (currentSceneName == "Menu")
        {
            PrechargerScene("Jeu");
        }
        else if (currentSceneName == "Jeu")
        {
            PrechargerScene("Retrospective");
        }
        else if (currentSceneName == "Retrospective")
        {
            PrechargerScene("Jeu");
        }
    }

    /// <summary>
    /// Démarre le préchargement d'une scène en arrière-plan sans l'activer.
    /// </summary>
    public void PrechargerScene(string sceneName)
    {
        if (_preloadedSceneName == sceneName && _preloadedSceneOperation != null)
        {
            return;
        }

        _preloadedSceneName = sceneName;
        _preloadedSceneOperation = SceneManager.LoadSceneAsync(sceneName);
        if (_preloadedSceneOperation != null)
        {
            _preloadedSceneOperation.allowSceneActivation = false;
            Debug.Log($"[ScenesManager] Scène '{sceneName}' en cours de préchargement en arrière-plan...");
        }
    }

    /// <summary>
    /// Active la scène si elle est préchargée, sinon effectue un chargement classique.
    /// </summary>
    private void ActiverOuChargerScene(string sceneName)
    {
        if (_preloadedSceneName == sceneName && _preloadedSceneOperation != null)
        {
            Debug.Log($"[ScenesManager] Transition instantanée : activation de la scène préchargée '{sceneName}'...");
            _preloadedSceneOperation.allowSceneActivation = true;
            _preloadedSceneOperation = null;
            _preloadedSceneName = null;
        }
        else
        {
            Debug.Log($"[ScenesManager] La scène '{sceneName}' n'est pas préchargée ou prête. Chargement classique...");
            SceneManager.LoadScene(sceneName);
        }
    }

    public void InitJeu()
    {
        Debug.Log("ScenesManager : Initialisation d'une nouvelle partie...");
        if (gameData != null)
        {
            gameData.ResetData();
            Debug.Log("ScenesManager : Données de jeu réinitialisées.");
        }
        else
        {
            Debug.LogWarning("ScenesManager : GameData manquant, impossible de réinitialiser.");
        }
        ChargerJeu();
    }

    public void ChargerJeu()
    {
        Debug.Log("ScenesManager : Chargement de 'Jeu'...");
        ActiverOuChargerScene("Jeu");
    }

    public void ChargerIntrospection()
    {
        Debug.Log("ScenesManager : Chargement de 'Retrospective'...");
        ActiverOuChargerScene("Retrospective");    
    }

    public void ChargerMenu()
    {
        Debug.Log("ScenesManager : Chargement de 'Menu'...");
        ActiverOuChargerScene("Menu");
    }

    public void QuitterJeu()
    {
        Debug.Log("ScenesManager : Fermeture.");
        Application.Quit();
    }
}
