using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestionnaire audio persistant qui gère les musiques de fond (Menu et Jeu)
/// et s'assure qu'elles bouclent correctement dans leurs scènes respectives.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Musiques")]
    public AudioClip musiqueMenu;
    public AudioClip musiqueJeu;

    private AudioSource audioSource;

    private void Awake()
    {
        // Système de Singleton pour ne pas détruire l'AudioSource entre les scènes
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
        // Désabonnement pour éviter les fuites de mémoire
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
}