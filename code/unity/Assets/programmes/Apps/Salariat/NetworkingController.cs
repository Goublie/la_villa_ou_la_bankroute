using UnityEngine;

/// <summary>
/// Handles the "Faire du networking" confirmation flow. Lives on
/// 'Panel_Confirmation_Networking'. The main "Faire du networking" button in
/// Panel_Actions_Rapides opens this popup; the "Retour" button closes it and
/// restores the dashboard panels. The "Oui" effect logic will be added later.
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

    /// <summary>
    /// Opens the networking confirmation popup and hides the three dashboard panels.
    /// Wired to the main "Faire du networking" button in Panel_Actions_Rapides.
    /// </summary>
    public void OpenNetworkingPanel()
    {
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

        // Close the popup and restore the dashboard panels.
        CloseNetworkingPanel();
    }
}
