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
        // Lancement de la musique de fond si elle est configurée
        if (musicSource != null && background != null)
        {
            musicSource.clip = background;
            musicSource.loop = true;
            musicSource.Play();
        }

        // CORRECTION : On utilise bien playButton ici, avec une vérification de sécurité
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
        Debug.Log("Clic détecté !"); // Voir si le message apparaît dans la console
        SFXSource.PlayOneShot(appuier_boutton);
        /*// Sécurité : on vérifie que les sources et le clip existent avant de lancer la coroutine
        if (musicSource != null && SFXSource != null && appuier_boutton != null)
        {
            StartCoroutine(MuteMusicDuringSFX());
        }*/
    }

    private IEnumerator MuteMusicDuringSFX()
    {
        // Coupe le son de la musique de fond
        musicSource.mute = true;

        // Joue le bruitage du bouton
        SFXSource.PlayOneShot(appuier_boutton);

        // Attend la fin exacte de la durée du son
        yield return new WaitForSeconds(appuier_boutton.length);

        // Réactive le son de la musique de fond
        musicSource.mute = false;
    }
}