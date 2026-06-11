using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerFenetre : MonoBehaviour
{
    public enum AppType { Aucun, Banque, Actualites, Salariat, Bourse }

    [Header("Configuration")]
    public AppType typeApplication = AppType.Aucun;
    public GameData gameData;

    [Header("Raccourci (Optionnel)")]
    public Button raccourciBureau;

    private TextMeshProUGUI chronoText;

    void OnEnable()
    {
        if (typeApplication == AppType.Aucun) return;

        if (gameData == null)
        {
            Debug.LogWarning("[TimerFenetre] GameData n'est pas assigne sur " + gameObject.name);
            return;
        }

        // Resoudre la reference au composant texte Chrono dynamiquement si non definie
        if (chronoText == null)
        {
            chronoText = transform.Find("Chrono")?.GetComponent<TextMeshProUGUI>();
        }

        // Verifier si le temps de l'application est deja epuise
        if (GetRemainingTime() <= 0f)
        {
            Debug.Log("[WindowTimer] " + typeApplication + " est bloquee (temps epuise). Fermeture immediate.");
            UpdateRaccourci(false);
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (typeApplication == AppType.Aucun || gameData == null) return;

        float remainingTime = GetRemainingTime();

        if (remainingTime > 0f)
        {
            // Diminuer le temps restant par le delta time
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                SetRemainingTime(remainingTime);
                UpdateRaccourci(false);
                Debug.Log("[WindowTimer] Temps epuise pour " + typeApplication + ". Fermeture de la fenetre.");
                gameObject.SetActive(false);
                return;
            }
            SetRemainingTime(remainingTime);
        }

        // Mettre a jour l'affichage du compte a rebours
        if (chronoText != null)
        {
            chronoText.text = FormatTime(remainingTime);
        }
    }

    private float GetRemainingTime()
    {
        if (gameData == null) return 0f;
        switch (typeApplication)
        {
            case AppType.Banque: return gameData.joueur.tempsRestantBanque;
            case AppType.Actualites: return gameData.joueur.tempsRestantActualites;
            case AppType.Salariat: return gameData.joueur.tempsRestantSalariat;
            case AppType.Bourse: return gameData.joueur.tempsRestantBourse;
            default: return 0f;
        }
    }

    private void SetRemainingTime(float value)
    {
        if (gameData == null) return;
        switch (typeApplication)
        {
            case AppType.Banque: gameData.joueur.tempsRestantBanque = value; break;
            case AppType.Actualites: gameData.joueur.tempsRestantActualites = value; break;
            case AppType.Salariat: gameData.joueur.tempsRestantSalariat = value; break;
            case AppType.Bourse: gameData.joueur.tempsRestantBourse = value; break;
        }
    }

    private void UpdateRaccourci(bool interactable)
    {
        if (raccourciBureau != null)
        {
            raccourciBureau.interactable = interactable;
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
