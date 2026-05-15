using UnityEngine;
using System.Collections;

public class Audio_Manager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("Audio Clips")]
    public AudioClip background;
    public AudioClip appuier_boutton;

    private void Awake()
    {
        // Dit à Unity de ne pas détruire cet objet en changeant de scène
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    public void PlayBruitBouton()
    {
        // On lance la Coroutine au lieu d'un simple Play
        StartCoroutine(MuteMusicDuringSFX());
    }

    private IEnumerator MuteMusicDuringSFX()
    {
        // 1. On baisse ou on coupe la musique
        musicSource.mute = true;

        // 2. On joue le bruitage
        SFXSource.PlayOneShot(appuier_boutton);

        // 3. On attend la durée précise du clip de bruitage
        yield return new WaitForSeconds(appuier_boutton.length);

        // 4. On remet la musique
        musicSource.mute = false;
    }
}