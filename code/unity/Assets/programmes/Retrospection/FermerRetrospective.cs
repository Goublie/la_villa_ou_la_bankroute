using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Bouton de sortie de la scene Retrospective vers la scene Jeu.
/// </summary>
/// <remarks>
/// Le retour passe par <see cref="ScenesManager"/> lorsque l'instance persiste,
/// afin de conserver la logique de navigation centrale. Le chargement direct
/// reste un secours pour les tests de scene isoles.
/// </remarks>
public class FermerRetrospective : MonoBehaviour
{
    private void Start()
    {
        Button bouton = GetComponent<Button>();
        if (bouton != null)
        {
            bouton.onClick.AddListener(Fermer);
        }
    }

    /// <summary>
    /// Charge la scene Jeu sans reinitialiser <see cref="GameData"/>.
    /// </summary>
    public void Fermer()
    {
        if (ScenesManager.Instance != null)
        {
            ScenesManager.Instance.ChargerJeu();
            return;
        }

        Debug.LogWarning(
            "[Retrospective] ScenesManager introuvable, chargement direct de Jeu.");
        SceneManager.LoadScene("Jeu");
    }
}
