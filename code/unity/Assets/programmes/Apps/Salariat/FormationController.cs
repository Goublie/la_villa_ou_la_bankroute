using UnityEngine;

/// <summary>
/// Gère la validation de la formation. S'inspire du fonctionnement du NetworkingController.
/// </summary>
public class FormationController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject panelFormation;          // Ton popup 'Panel_Fromation'
    public GameObject panelPosteActuel;        // 'Panel_Poste8actuel'
    public GameObject panelActionsRapides;     // 'Panel_Actions_Rapides'
    public GameObject panelPerformanceEmploye; // 'Panel_PerformanceEmploye'
    public GameObject panelRelationnel;        // 'Panel_Relationnel'

    [Header("Systèmes de Stats")]
    public EmployeePerformanceController performanceController; // Gestionnaire d'expérience
    public RelationalController relationalController;           // Gestionnaire des relations (Patron)

    /// <summary>
    /// Ouvre le panel de formation et masque le tableau de bord.
    /// À lier au bouton "Formation" du Panel_Actions_Rapides.
    /// </summary>
    public void OpenFormationPanel()
    {
        if (panelFormation != null) panelFormation.SetActive(true);
        if (panelPosteActuel != null) panelPosteActuel.SetActive(false);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(false);
        if (panelPerformanceEmploye != null) panelPerformanceEmploye.SetActive(false);
        if (panelRelationnel != null) panelRelationnel.SetActive(false);
    }

    /// <summary>
    /// Ferme le panel et restaure le tableau de bord.
    /// À lier au 'Bouton_Retour' du panel de formation.
    /// </summary>
    public void CloseFormationPanel()
    {
        if (panelFormation != null) panelFormation.SetActive(false);
        if (panelPosteActuel != null) panelPosteActuel.SetActive(true);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(true);
        if (panelPerformanceEmploye != null) panelPerformanceEmploye.SetActive(true);
        if (panelRelationnel != null) panelRelationnel.SetActive(true);
    }

    /// <summary>
    /// Valide la formation : +5 Expérience, +10 Patron.
    /// À lier au 'Bouton_Accepter' (le bouton Oui).
    /// </summary>
    public void OnOuiFormationClicked()
    {
        // 1. Applique le bonus d'expérience (+5)
        if (performanceController != null)
        {
            performanceController.ModifyExperience(5);
        }

        // 2. Applique le bonus de relation avec le Patron (+10)
        if (relationalController != null)
        {
            relationalController.ModifyPatronScore(10);
        }

        // 3. Ferme le menu et retourne au jeu
        CloseFormationPanel();
    }
}