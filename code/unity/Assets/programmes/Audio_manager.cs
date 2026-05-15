using UnityEngine;

public class Audio_Manager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("Audio Clips")]
    public AudioClip background;
    public AudioClip appuier_boutton;


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
        // On utilise directement le clip 'appuier_boutton' que tu as déjà rempli
        SFXSource.PlayOneShot(appuier_boutton);
    }
}