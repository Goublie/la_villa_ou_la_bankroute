using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    // ◄ FIX : Le Singleton permet d'accéder facilement à l'AudioManager et évite les doublons
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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    public void Start()
    {
        ConfigurerBoutons();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ◄ DEBUG : Regarde ta console Unity pour voir si ce message s'affiche au changement de scène
        Debug.Log("AudioManager : Nouvelle scène détectée -> " + scene.name);

        if (scene.name == "Menu")
        {
            ChangerMusiqueFond(musiqueMenu);
            ConfigurerBoutons();
        }
        else
        {
            ChangerMusiqueFond(musiqueJeu);
        }
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

        // On force l'AudioSource à boucler
        musicSource.loop = true;

        // Si c'est déjà la même musique qui joue, on ne la coupe pas
        if (musicSource.clip == nouvelleMusique)
        {
            if (!musicSource.isPlaying) musicSource.Play();
            return;
        }

        // On change de piste et on lance
        Debug.Log("AudioManager : Lancement de la musique -> " + nouvelleMusique.name);
        musicSource.Stop();
        musicSource.clip = nouvelleMusique;
        musicSource.Play();
    }

    private void ConfigurerBoutons()
    {
        if (playButton == null) playButton = GameObject.Find("Jouer")?.GetComponent<Button>();
        if (optionsButton == null) optionsButton = GameObject.Find("Options")?.GetComponent<Button>();

        if (playButton != null) playButton.onClick.AddListener(PlayBruitBouton);
        if (optionsButton != null) optionsButton.onClick.AddListener(PlayBruitBouton);
    }

    public void PlayBruitBouton()
    {
        if (musicSource != null && SFXSource != null && appuier_boutton != null)
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
}