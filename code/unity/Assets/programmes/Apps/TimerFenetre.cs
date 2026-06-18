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
    public enum AppType
    {
        Aucun,
        Banque,
        Actualites,
        Salariat,
        Bourse,
        Entrepreneuriat
    }

    [Header("Configuration")]
    public AppType typeApplication = AppType.Aucun;
    public GameData gameData;

    [Header("Raccourci (optionnel)")]
    public Button raccourciBureau;

    private TextMeshProUGUI chronoText;
    private ServiceRepartitionTemps service;

    private void OnEnable()
    {
        if (typeApplication == AppType.Aucun)
        {
            return;
        }

        ResoudreService();
        ResoudreChrono();

        if (service == null || !service.PeutOuvrir(Convertir(typeApplication)))
        {
            UpdateRaccourci(false);
            gameObject.SetActive(false);
            return;
        }

        ActualiserChrono();
    }

    private void Update()
    {
        if (typeApplication == AppType.Aucun ||
            !ResoudreService())
        {
            return;
        }

        TypeApplicationTemps type = Convertir(typeApplication);
        bool resteDuTemps = service.Consommer(type, Time.deltaTime);
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
            service.ObtenirSecondesRestantes(Convertir(typeApplication)));
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

    private static TypeApplicationTemps Convertir(AppType type)
    {
        switch (type)
        {
            case AppType.Banque:
                return TypeApplicationTemps.Banque;
            case AppType.Actualites:
                return TypeApplicationTemps.Actualites;
            case AppType.Salariat:
                return TypeApplicationTemps.Salariat;
            case AppType.Bourse:
                return TypeApplicationTemps.Bourse;
            case AppType.Entrepreneuriat:
                return TypeApplicationTemps.Entrepreneuriat;
            default:
                return TypeApplicationTemps.Aucun;
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
