using System;
using UnityEngine;

/// <summary>
/// Adaptateur Unity qui transmet le clic de passage de mois au service
/// d'orchestration puis notifie les interfaces.
/// </summary>
public class ActionPlay : MonoBehaviour
{
    public GameData gameData;

    /// <summary>
    /// Notification visuelle envoyee apres toutes les mutations metier.
    /// </summary>
    /// <remarks>
    /// Aucun service metier ne doit dependre de cet evenement statique. Il est
    /// conserve pour les MonoBehaviour existants et leurs references prefab.
    /// </remarks>
    public static event Action OnMoisPasse;

    private ServicePassageMensuel servicePassageMensuel;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReinitialiserNotifications()
    {
        // Le reset au demarrage du runtime evite qu'un Awake efface des
        // abonnements valides selon l'ordre d'initialisation de la scene.
        OnMoisPasse = null;
    }

    private void Start()
    {
        if (!EssayerCreerService())
        {
            return;
        }

        ResultatOperation resultat =
            servicePassageMensuel.InitialiserPartie();
        if (!resultat.Succes)
        {
            Debug.LogError("[Temps] " + resultat.Message);
        }
    }

    /// <summary>
    /// Termine le mois courant, ouvre le suivant puis rafraichit les UI.
    /// </summary>
    /// <remarks>
    /// Effets de bord : fait evoluer les agregats, cree un snapshot, avance
    /// le calendrier, verse le salaire et peut charger l'introspection lors
    /// du passage de decembre a janvier.
    /// </remarks>
    public void Jouer()
    {
        if (!EssayerCreerService())
        {
            return;
        }

        ResultatPassageMensuel resultat =
            servicePassageMensuel.PasserAuMoisSuivant();
        if (!resultat.Succes)
        {
            Debug.LogError("[Temps] " + resultat.Message);
            return;
        }

        OnMoisPasse?.Invoke();
        if (DoitChargerGameOver(gameData) &&
            ScenesManager.Instance != null)
        {
            ScenesManager.Instance.ChargerGameOver();
            return;
        }

        if (resultat.ChangementAnnee &&
            ScenesManager.Instance != null)
        {
            ScenesManager.Instance.ChargerIntrospection();
        }
    }

    private bool EssayerCreerService()
    {
        if (servicePassageMensuel != null)
        {
            return true;
        }

        if (gameData == null)
        {
            Debug.LogError("[Temps] GameData manquant.");
            return false;
        }

        servicePassageMensuel =
            new ServicePassageMensuel(gameData);
        return true;
    }

    /// <summary>
    /// Indique si la partie doit basculer vers GameOver apres le passage mensuel.
    /// </summary>
    /// <remarks>
    /// Le test est porte par l'adaptateur de scene, pas par SalariatUI, afin
    /// qu'un burnout atteignant 100 soit traite meme si la fenetre Salariat est
    /// fermee ou inactive.
    /// </remarks>
    public static bool DoitChargerGameOver(GameData gameData)
    {
        if (gameData == null || gameData.joueur == null)
        {
            return false;
        }

        DonneesSalariat salariat = gameData.joueur.salariat;
        return salariat != null && salariat.burnout >= 100;
    }
}
