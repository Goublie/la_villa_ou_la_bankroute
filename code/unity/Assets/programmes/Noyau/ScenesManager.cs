using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    private Button boutonJouer;
    private Button boutonContinuer; // Ajout pour la sauvegarde
    private Button boutonQuitter;

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

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReinitialiserInstance()
    {
        _instance = null;
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
        if (_instance == this)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void Start()
    {
        if (_instance == this)
        {
            ConfigurerBoutonsMenu(SceneManager.GetActiveScene());
        }
    }

    private void OnDisable()
    {
        if (_instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            RetirerListenersMenu();
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConfigurerBoutonsMenu(scene);
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
    /// Charge la sauvegarde existante puis bascule vers la scene de jeu.
    /// </summary>
    public void ContinuerJeu()
    {
        Debug.Log("ScenesManager : Tentative de chargement de la sauvegarde...");
        if (SaveManager.Instance != null && SaveManager.Instance.ChargerPartie())
        {
            ChargerJeu();
        }
        else
        {
            Debug.LogError("ScenesManager : Échec du chargement de la partie.");
        }
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
    /// Charge la scene terminale GameOver.
    /// </summary>
    public void ChargerGameOver()
    {
        Debug.Log("ScenesManager : Chargement de 'GameOver'...");
        SceneManager.LoadScene("GameOver");
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

    private void ConfigurerBoutonsMenu(Scene scene)
    {
        RetirerListenersMenu();
        if (scene.name != "Menu")
        {
            return;
        }

        boutonJouer = TrouverBouton(scene, "Jouer");
        boutonContinuer = TrouverBouton(scene, "Continuer"); // Recherche automatique du bouton
        boutonQuitter = TrouverBouton(scene, "Quitter");

        if (boutonJouer != null)
        {
            boutonJouer.onClick.RemoveListener(InitJeu);
            boutonJouer.onClick.AddListener(InitJeu);
        }

        if (boutonContinuer != null)
        {
            // Vérification de l'existence du fichier physique validé par Antigravity
            string cheminSauvegarde = Path.Combine(Application.persistentDataPath, "sauvegarde_jeu.json");
            bool sauvegardeExiste = File.Exists(cheminSauvegarde);

            boutonContinuer.interactable = sauvegardeExiste; // Grisé si pas de fichier

            boutonContinuer.onClick.RemoveListener(ContinuerJeu);
            boutonContinuer.onClick.AddListener(ContinuerJeu);
        }

        if (boutonQuitter != null)
        {
            boutonQuitter.onClick.RemoveListener(QuitterJeu);
            boutonQuitter.onClick.AddListener(QuitterJeu);
        }
    }

    private void RetirerListenersMenu()
    {
        if (boutonJouer != null)
        {
            boutonJouer.onClick.RemoveListener(InitJeu);
        }

        if (boutonContinuer != null)
        {
            boutonContinuer.onClick.RemoveListener(ContinuerJeu);
        }

        if (boutonQuitter != null)
        {
            boutonQuitter.onClick.RemoveListener(QuitterJeu);
        }

        boutonJouer = null;
        boutonContinuer = null;
        boutonQuitter = null;
    }

    private static Button TrouverBouton(Scene scene, string nomObjet)
    {
        Button[] boutons = Object.FindObjectsByType<Button>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        foreach (Button bouton in boutons)
        {
            if (bouton != null &&
                bouton.gameObject.scene == scene &&
                bouton.name == nomObjet)
            {
                return bouton;
            }
        }

        return null;
    }
}