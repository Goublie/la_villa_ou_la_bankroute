using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;

    [Header("Musiques de fond (Soundtrack)")]
    public AudioClip musiqueMenu; // ◄ Ta musique pour le menu principal
    public AudioClip musiqueJeu;  // ◄ Ta musique pour les niveaux de jeu

    [Header("Audio Clips SFX")]
    public AudioClip appuier_boutton;

    [Header("UI Buttons")]
    public Button playButton;
    public Button optionsButton;


    private void Awake()
    {
        // On rend l'Audio Manager immortel pour qu'il gère la soundtrack partout
        DontDestroyOnLoad(gameObject);

        // On s'abonne à l'événement de chargement de scène de Unity
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Bonne pratique : on se désabonne si l'objet est détruit pour éviter les fuites de mémoire
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Start()
    {
        // Configuration initiale des boutons de la scène Menu
        ConfigurerBoutons();
    }

    // Cette fonction se déclenche AUTOMATIQUEMENT dès qu'une scène change
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. On adapte la musique selon la scène actuelle
        if (scene.name == "Menu") // ◄ Remplace par le nom EXACT de ta scène Menu
        {
            ChangerMusiqueFond(musiqueMenu);

            // Comme on est revenu au menu, on doit retrouver et reconnecter les boutons
            ConfigurerBoutons();
        }
        else // Si on est dans le jeu (Niveau 1, Niveau 2, etc.)
        {
            ChangerMusiqueFond(musiqueJeu);
        }
    }

    private void ChangerMusiqueFond(AudioClip nouvelleMusique)
    {
        if (musicSource == null || nouvelleMusique == null) return;

        // Si la musique demandée est DEJA en train de jouer, on ne fait rien (évite de couper le son au redémarrage)
        if (musicSource.clip == nouvelleMusique) return;

        // On change de piste et on la lance en boucle
        musicSource.Stop();
        musicSource.clip = nouvelleMusique;
        musicSource.loop = true;
        musicSource.Play();
    }

    private void ConfigurerBoutons()
    {
        // On cherche les boutons dans la scène actuelle s'ils n'ont pas été assignés dans l'Inspector
        if (playButton == null) playButton = GameObject.Find("Jouer")?.GetComponent<Button>();
        if (optionsButton == null) optionsButton = GameObject.Find("Options")?.GetComponent<Button>();

        // On applique les écouteurs de son
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
