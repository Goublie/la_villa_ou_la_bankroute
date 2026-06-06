using UnityEngine;
using UnityEngine.UI;

public class Luminosite_Globale : MonoBehaviour
{
    // Instance statique pour que le script soit accessible facilement par les autres scripts
    public static Luminosite_Globale Instance { get; private set; }

    [SerializeField] private Image filtreNoir;

    void Awake()
    {
        // Système de Singleton : on garde cet objet unique et immortel entre les scènes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Fonction que le menu d'options va appeler
    public void AppliquerLuminosite(float valeurSlider)
    {
        if (filtreNoir != null)
        {
            Color c = filtreNoir.color;
            // Plus le slider est haut (1), plus l'alpha est bas (0 = transparent/lumineux)
            // Plus le slider est bas (0), plus l'alpha est haut (0.8 = sombre)
            c.a = Mathf.Lerp(0.8f, 0f, valeurSlider);
            filtreNoir.color = c;
        }
    }

    // Optionnel : Permet au menu d'options de connaître l'opacité actuelle au démarrage
    public float ObtenirValeurActuelle()
    {
        if (filtreNoir != null)
        {
            // Opération inverse pour redonner une valeur entre 0 et 1 au slider
            return Mathf.InverseLerp(0.8f, 0f, filtreNoir.color.a);
        }
        return 1f;
    }
}