using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Audio_Manager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;

    [Header("Audio Clips")]
    public AudioClip background;
    public AudioClip appuier_boutton;

    [Header("UI Buttons")]
    public Button playButton;
    public Button optionsButton;

    public void Start()
    {
        DontDestroyOnLoad(gameObject);
        
        // Lancement de la musique de fond
        if (musicSource != null && background != null)
        {
            musicSource.clip = background;
            musicSource.loop = true;
            musicSource.Play();
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayBruitBouton);
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(PlayBruitBouton);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (SFXSource != null && clip != null)
        {
            SFXSource.PlayOneShot(clip);
        }
    }

    public void PlayBruitBouton()
    {
        Debug.Log("Clic détecté !");

        // On vérifie que tout est là avant de lancer la transition
        if (musicSource != null && SFXSource != null && appuier_boutton != null)
        {
            StartCoroutine(MuteMusicDuringSFX());
        }
    }

    private IEnumerator MuteMusicDuringSFX()
    {
        // 1. On coupe le son de la musique de fond (ou on la met en Pause si tu préfères)
        musicSource.mute = true;
        // Note : Si tu veux qu'elle s'arrête net au lieu de continuer en silence, 
        // remplace par : musicSource.Pause();

        // 2. On joue le bruitage du bouton (UNE SEULE FOIS ICI)
        SFXSource.PlayOneShot(appuier_boutton);

        // 3. On attend que le bruitage soit complètement terminé
        yield return new WaitForSeconds(appuier_boutton.length);

        // 4. Réactive le son de la musique de fond
        musicSource.mute = false;
        // Note : Si tu as utilisé Pause() au-dessus, remplace par : musicSource.UnPause();
    }
}