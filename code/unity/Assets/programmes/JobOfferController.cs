using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls a single job offer row: when its "Postuler" button is clicked it opens
/// the job's unique interview panel and hides the whole job-offers screen.
/// </summary>
public class JobOfferController : MonoBehaviour
{
    [Tooltip("The 'Postuler' button inside this job offer row.")]
    public Button applyButton;

    [Tooltip("The unique interview panel to open for this job.")]
    public GameObject targetInterviewPanel;

    // The 'Panel_Offres_d'emploi' screen that gets hidden when applying.
    private GameObject offresPanel;

    private void Start()
    {
        // Interview panel is hidden by default.
        if (targetInterviewPanel != null)
        {
            targetInterviewPanel.SetActive(false);
        }

        // Resolve the job-offers panel by walking up the hierarchy.
        Transform current = transform;
        while (current != null)
        {
            if (current.name == "Panel_Offres_d'emploi")
            {
                offresPanel = current.gameObject;
                break;
            }
            current = current.parent;
        }

        // Wire the button click.
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(OnApplyClicked);
        }
    }

    private void OnApplyClicked()
    {
        // Open this job's interview panel.
        if (targetInterviewPanel != null)
        {
            targetInterviewPanel.SetActive(true);
        }

        // Hide the entire job-offers screen to switch views.
        if (offresPanel != null)
        {
            offresPanel.SetActive(false);
        }
    }
}
