using UnityEngine;
using UnityEngine.UI; // ◄ INDISPENSABLE pour manipuler le bouton grisé

/// <summary>
/// Handles the "Faire du networking" confirmation flow.
/// </summary>
public class NetworkingController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelConfirmationNetworking; // This popup ('Panel_Confirmation_Networking')
    public GameObject panelPosteActuel;            // 'Panel_Poste8actuel'
    public GameObject panelActionsRapides;         // 'Panel_Actions_Rapides'
    public GameObject panelPerformanceEmploye;     // 'Panel_PerformanceEmploye'
    public GameObject panelRelationnel;            // 'Panel_Relationnel'

    [Header("Relationship system")]
    public RelationalController relationalController; // 'Panel_Relationnel' controller

    [Header("Gestion du Cooldown (Temps de recharge)")]
    public Button boutonOuvrirNetworking; // ◄ AJOUT : Le bouton principal dans Panel_Actions_Rapides

    [Tooltip("Nombre de tours à attendre après avoir fait du networking")]
    [SerializeField] private int cooldownInitial = 3;
    private int toursRestantsAvantNetworking = 0; // ◄ AJOUT : Compteur de tours restants

    private void Start()
    {
        // ◄ SÉCURITÉ CRITIQUE : Écoute le passage des mois en continu, même si le panel est désactivé
        ActionPlay.OnMoisPasse += DiminuerCooldown;
        ActualiserEtatBouton();
    }

    private void OnDestroy()
    {
        // Désabonnement à la destruction pour éviter les fuites de mémoire
        ActionPlay.OnMoisPasse -= DiminuerCooldown;
    }

    /// <summary>
    /// Réduit le temps d'attente de 1 à chaque passage de mois.
    /// </summary>
    private void DiminuerCooldown()
    {
        if (toursRestantsAvantNetworking > 0)
        {
            toursRestantsAvantNetworking--;
            ActualiserEtatBouton();
        }
    }

    /// <summary>
    /// Met à jour l'état visuel du bouton (grisé ou actif).
    /// </summary>
    private void ActualiserEtatBouton()
    {
        if (boutonOuvrirNetworking != null)
        {
            // Redevient cliquable uniquement si le cooldown est à 0
            boutonOuvrirNetworking.interactable = (toursRestantsAvantNetworking <= 0);
        }
    }

    /// <summary>
    /// Opens the networking confirmation popup and hides the three dashboard panels.
    /// Wired to the main "Faire du networking" button in Panel_Actions_Rapides.
    /// </summary>
    public void OpenNetworkingPanel()
    {
        // Sécurité : Si le cooldown est actif, on empêche l'ouverture
        if (toursRestantsAvantNetworking > 0) return;

        if (panelConfirmationNetworking != null) panelConfirmationNetworking.SetActive(true);
        if (panelPosteActuel != null) panelPosteActuel.SetActive(false);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(false);
        if (panelPerformanceEmploye != null) panelPerformanceEmploye.SetActive(false);
        if (panelRelationnel != null) panelRelationnel.SetActive(false);
    }

    /// <summary>
    /// Closes the networking popup and restores the three dashboard panels.
    /// Wired to 'Bouton_Retour_Networking'.
    /// </summary>
    public void CloseNetworkingPanel()
    {
        if (panelConfirmationNetworking != null) panelConfirmationNetworking.SetActive(false);
        if (panelPosteActuel != null) panelPosteActuel.SetActive(true);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(true);
        if (panelPerformanceEmploye != null) panelPerformanceEmploye.SetActive(true);
        if (panelRelationnel != null) panelRelationnel.SetActive(true);
    }

    /// <summary>
    /// Confirms the networking action. Effect logic to be implemented later.
    /// Wired to 'Bouton_Oui_Networking'. Returns to the dashboard for now.
    /// </summary>
    public void OnOuiNetworkingClicked()
    {
        if (relationalController != null)
        {
            // Networking strengthens relationships: colleagues more than the boss.
            relationalController.ModifyColleguesScore(15);
            relationalController.ModifyPatronScore(5);
        }

        // ◄ AJOUT : Déclenche les 3 tours de blocage (tour actuel + 2 tours d'attente)
        toursRestantsAvantNetworking = cooldownInitial;
        ActualiserEtatBouton();

        // Close the popup and restore the dashboard panels.
        CloseNetworkingPanel();
    }
}