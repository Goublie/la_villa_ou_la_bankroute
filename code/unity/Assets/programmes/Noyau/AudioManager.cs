using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestionnaire audio persistant qui gère les musiques de fond (Menu et Jeu)
/// et s'assure qu'elles bouclent correctement dans leurs scènes respectives.
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ◄ FIX : Le Singleton permet d'accéder facilement à l'AudioManager et évite les doublons
    public static AudioManager Instance { get; private set; }

    [Header("Musiques")]
    public AudioClip musiqueMenu;
    public AudioClip musiqueJeu;

    private AudioSource audioSource;

    private void Awake()
    {
        // ◄ FIX CRUCIAL : Si un AudioManager existe déjà, on détruit le nouveau pour garder l'ancien
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Récupération ou ajout de l'AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configuration indispensable pour que la musique tourne en boucle
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        // On s'abonne à l'événement de chargement de scène d'Unity
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            RetirerListenersBoutons();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ◄ DEBUG : Pour voir si ce message s'affiche au changement de scène
        Debug.Log("AudioManager : Nouvelle scène détectée -> " + scene.name);
        GererScene(scene);
    }

    private void GererScene(Scene scene)
    {
        ChangerMusiqueFond(scene.name == "Menu" ? musiqueMenu : musiqueJeu);
        ConfigurerBoutons(scene);
    }

    private void ChangerMusiqueFond(AudioClip nouvelleMusique)
    {
        if (musicSource == null)
        {
            Debug.LogError("AudioManager : L'AudioSource 'musicSource' n'est pas assignée !");
            return;
        }

        // ◄ FIX SÉCURITÉ : Si l'AudioSource est décochée, le code la recoche automatiquement
        if (!musicSource.enabled)
        {
            Debug.LogWarning("AudioManager : 'musicSource' était désactivée ! Réactivation automatique.");
            musicSource.enabled = true;
        }

        if (nouvelleMusique == null)
        {
            Debug.LogWarning("AudioManager : Impossible de changer de musique car le clip est NULL !");
            return;
        }

        musicSource.loop = true;
        if (musicSource.clip == nouvelleMusique)
        {
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
            return;
        }

        Debug.Log("AudioManager : Lancement de la musique -> " + nouvelleMusique.name);
        musicSource.Stop();
        musicSource.clip = nouvelleMusique;
        musicSource.Play();
    }

    private void ConfigurerBoutons(Scene scene)
    {
        RetirerListenersBoutons();
        if (scene.name != "Menu")
        {
            return;
        }

        playButton = TrouverBouton(scene, "Jouer");
        optionsButton = TrouverBouton(scene, "Options");
        
        AjouterListener(playButton);
        AjouterListener(optionsButton);
    }

    /// <summary>
    /// Déclenché automatiquement à chaque fois qu'une scène est chargée.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // On vérifie le nom de la scène (ici "Menu" d'après ton historique Git)
        if (scene.name == "Menu")
        {
            JouerMusique(musiqueMenu);
        }
        else
        {
            // Si ce n'est pas le menu, c'est qu'on est dans le jeu !
            JouerMusique(musiqueJeu);
        }
    }

    /// <summary>
    /// Change de piste audio uniquement si la nouvelle piste est différente de celle en cours.
    /// </summary>
    public void JouerMusique(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager : Tentative de jouer un clip nul (AudioClip manquant dans l'Inspecteur ?)");
            return;
        }

        // Sécurité : Si la musique demandée est déjà en train de jouer, on ne fait rien (évite les coupures)
        if (audioSource.clip == clip && audioSource.isPlaying)
        {
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
    }

    /// <summary>
    /// Permet de couper ou relancer la musique (optionnel, utile pour des menus d'options)
    /// </summary>
    public void SetMute(bool DevenirMuet)
    {
        if (audioSource != null)
        {
            audioSource.mute = DevenirMuet;
        }
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
