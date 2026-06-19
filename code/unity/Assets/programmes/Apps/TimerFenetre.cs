using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Consomme le temps mensuel d'une fenetre ouverte et affiche un chrono local.
/// </summary>
/// <remarks>
/// Ce composant reste optionnel : il peut etre pose sur une fenetre applicative
/// sans modifier le passage mensuel. Le temps est stocke dans
/// <see cref="DonneesRepartitionTemps"/> et manipule par
/// <see cref="ServiceRepartitionTemps"/>.
/// </remarks>
public class TimerFenetre : MonoBehaviour
{
    [Header("Configuration")]
    public TypeApplicationTemps typeApplication = TypeApplicationTemps.Aucun;
    public GameData gameData;

    [Header("Raccourci (optionnel)")]
    public Button raccourciBureau;

    private TextMeshProUGUI chronoText;
    private ServiceRepartitionTemps service;

    private void OnEnable()
    {
        if (typeApplication == TypeApplicationTemps.Aucun)
        {
            return;
        }

        ResoudreService();
        ResoudreChrono();

        if (service == null || !service.PeutOuvrir(typeApplication))
        {
            UpdateRaccourci(false);
            gameObject.SetActive(false);
            return;
        }

        ActualiserChrono();
    }

    private void Update()
    {
        if (typeApplication == TypeApplicationTemps.Aucun ||
            !ResoudreService())
        {
            return;
        }

        bool resteDuTemps = service.Consommer(typeApplication, Time.deltaTime);
        ActualiserChrono();

        if (!resteDuTemps)
        {
            UpdateRaccourci(false);
            gameObject.SetActive(false);
        }
    }

    private void ActualiserChrono()
    {
        ResoudreChrono();
        if (chronoText == null || service == null)
        {
            return;
        }

        chronoText.text = FormatTime(
            service.ObtenirSecondesRestantes(typeApplication));
    }

    private bool ResoudreService()
    {
        if (service != null)
        {
            return true;
        }

        if (gameData == null)
        {
            ActionPlay actionPlay =
                Object.FindFirstObjectByType<ActionPlay>();
            if (actionPlay != null)
            {
                gameData = actionPlay.gameData;
            }
        }

        if (gameData == null || gameData.joueur == null)
        {
            return false;
        }

        gameData.joueur.InitialiserSiNecessaire();
        service =
            new ServiceRepartitionTemps(gameData.joueur.tempsApplications);
        return true;
    }

    private void ResoudreChrono()
    {
        if (chronoText == null)
        {
            chronoText = transform.Find("TopBar/Chrono")
                ?.GetComponent<TextMeshProUGUI>();
        }
    }

    private void UpdateRaccourci(bool interactable)
    {
        if (raccourciBureau != null)
        {
            raccourciBureau.interactable = interactable;
        }
    }

    private static string FormatTime(float timeInSeconds)
    {
        int secondesTotales = Mathf.Max(0, Mathf.FloorToInt(timeInSeconds));
        int minutes = secondesTotales / 60;
        int seconds = secondesTotales % 60;
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
