using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attache un écouteur d'événement au bouton de fermeture pour recharger la scène
/// de jeu sans réinitialiser les données (GameData).
/// </summary>
[RequireComponent(typeof(Button))]
public class FermerRetrospective : MonoBehaviour
{
    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => {
                if (ScenesManager.Instance != null)
                {
                    ScenesManager.Instance.ChargerJeu();
                }
                else
                {
                    Debug.LogWarning("ScenesManager introuvable, chargement direct de la scène 'Jeu'.");
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Jeu");
                }
            });
        }
    }
}
