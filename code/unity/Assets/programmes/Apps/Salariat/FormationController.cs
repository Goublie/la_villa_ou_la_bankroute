using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère la validation de la formation avec un système de temps de recharge (cooldown).
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

    [Header("Gestion du Cooldown (Temps de recharge)")]
    public Button boutonOuvrirFormation; // Le bouton qui ouvre le panel (dans Actions Rapides)

    [Tooltip("Nombre de tours à attendre après une formation")]
    [SerializeField] private int cooldownInitial = 3;
    private int toursRestantsAvantFormation = 0;

    private void Start()
    {
        // ◄ MODIFICATION CRITIQUE : On s'abonne ici pour que le script écoute 
        // même si son GameObject est désactivé visuellement.
        ActionPlay.OnMoisPasse += DiminuerCooldown;
        ActualiserEtatBouton();
    }

    private void OnDestroy()
    {
        // On se désabonne à la destruction du script pour éviter les fuites de mémoire.
        ActionPlay.OnMoisPasse -= DiminuerCooldown;
    }

    /// <summary>
    /// Réduit le temps d'attente de 1 à chaque passage de mois.
    /// </summary>
    private void DiminuerCooldown()
    {
        if (toursRestantsAvantFormation > 0)
        {
            toursRestantsAvantFormation--;
            ActualiserEtatBouton();
        }
    }

    /// <summary>
    /// Met à jour l'état visuel du bouton selon le cooldown actuel.
    /// </summary>
    private void ActualiserEtatBouton()
    {
        if (boutonOuvrirFormation != null)
        {
            // Le bouton redevient cliquable uniquement si le cooldown est à 0
            boutonOuvrirFormation.interactable = (toursRestantsAvantFormation <= 0);
        }
    }

    /// <summary>
    /// Ouvre le panel de formation et masque le tableau de bord.
    /// </summary>
    public void OpenFormationPanel()
    {
        // Sécurité : si le cooldown n'est pas terminé, on refuse d'ouvrir
        if (toursRestantsAvantFormation > 0) return;

        if (panelFormation != null) panelFormation.SetActive(true);
        if (panelPosteActuel != null) panelPosteActuel.SetActive(false);
        if (panelActionsRapides != null) panelActionsRapides.SetActive(false);
        if (panelPerformanceEmploye != null) panelPerformanceEmploye.SetActive(false);
        if (panelRelationnel != null) panelRelationnel.SetActive(false);
    }

    /// <summary>
    /// Ferme le panel et restaure le tableau de bord.
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
    /// Valide la formation : +5 Expérience, +10 Patron et lance le cooldown.
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

        // 3. Déclenche le temps d'attente (3 tours au total pour couvrir le tour actuel + 2 tours de pause)
        toursRestantsAvantFormation = cooldownInitial;
        ActualiserEtatBouton();

        // 4. Ferme le menu et retourne au jeu
        CloseFormationPanel();
    }
}