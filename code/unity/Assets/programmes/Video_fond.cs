using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Video_fond : MonoBehaviour
{
    private RawImage img;
    private VideoPlayer player;

    private void Start()
    {
        img = GetComponent<RawImage>();
        player = GetComponent<VideoPlayer>();

        if (img != null && player != null)
        {
            // Étape de sécurité : on s'abonne à l'événement "Vidéo prête"
            player.prepareCompleted += OnVideoPrepared;
            player.Prepare();
        }
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        // Unity connaît maintenant les dimensions exactes (ex: 1920x1080)
        RenderTexture texture = new RenderTexture((int)source.width, (int)source.height, 16);
        source.targetTexture = texture;
        img.texture = texture;

        // On force le lancement
        source.Play();
    }
}