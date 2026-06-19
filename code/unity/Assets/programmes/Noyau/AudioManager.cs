using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gere une unique paire de sources audio persistantes et reconnecte les sons
/// des boutons du Menu apres chaque changement de scene.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;

    [Header("Musiques de fond (Soundtrack)")]
    public AudioClip musiqueMenu;
    public AudioClip musiqueJeu;

    [Header("Audio Clips SFX")]
    public AudioClip appuier_boutton;

    [Header("UI Buttons")]
    public Button playButton;
    public Button optionsButton;

    /// <summary>
    /// Nombre de demandes de son traitees par l'instance persistante.
    /// Cette valeur permet de verifier qu'un clic ne declenche qu'un listener.
    /// </summary>
    public int NombreClicsTraites { get; private set; }

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReinitialiserInstance()
    {
        Instance = null;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void Start()
    {
        if (Instance == this)
        {
            GererScene(SceneManager.GetActiveScene());
        }
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
        GererScene(scene);
    }

    private void GererScene(Scene scene)
    {
        ChangerMusiqueFond(
            scene.name == "Menu" ? musiqueMenu : musiqueJeu);
        ConfigurerBoutons(scene);
    }

    private void ChangerMusiqueFond(AudioClip nouvelleMusique)
    {
        if (musicSource == null || nouvelleMusique == null)
        {
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
    /// Joue le son de clic en coupant temporairement la musique de fond.
    /// </summary>
    public void PlayBruitBouton()
    {
        NombreClicsTraites++;
        if (musicSource != null &&
            SFXSource != null &&
            appuier_boutton != null)
        {
            StartCoroutine(MuteMusicDuringSFX());
        }
    }

    private IEnumerator MuteMusicDuringSFX()
    {
        musicSource.mute = true;
        SFXSource.PlayOneShot(appuier_boutton);
        yield return new WaitForSeconds(appuier_boutton.length);
        musicSource.mute = false;
    }

    private void AjouterListener(Button bouton)
    {
        if (bouton == null)
        {
            return;
        }

        bouton.onClick.RemoveListener(PlayBruitBouton);
        bouton.onClick.AddListener(PlayBruitBouton);
    }

    private void RetirerListenersBoutons()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(PlayBruitBouton);
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.RemoveListener(PlayBruitBouton);
        }

        playButton = null;
        optionsButton = null;
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
