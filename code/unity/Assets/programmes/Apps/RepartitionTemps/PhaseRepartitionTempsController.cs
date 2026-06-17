using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controle la phase obligatoire de repartition du temps au debut de chaque mois.
/// </summary>
/// <remarks>
/// Ce composant reste volontairement dans la couche Unity : le service metier
/// porte l'etat persistant, tandis que ce controleur ouvre la fenetre, place un
/// bloqueur modal et active les boutons de bureau selon le temps restant. Le
/// verrou du bouton de passage mensuel est double par
/// <see cref="ActionPlay.PeutPasserAuMoisSuivant"/> pour rester robuste si une
/// reference de scene est manquante.
/// </remarks>
public class PhaseRepartitionTempsController : MonoBehaviour
{
    [Header("Donnees")]
    public GameData gameData;

    [Header("Fenetre modale")]
    public GameObject fenetreRepartitionTemps;

    [Header("Boutons")]
    public Button boutonPassageMois;
    public Button boutonBanque;
    public Button boutonActualites;
    public Button boutonSalariat;
    public Button boutonBourse;
    public Button boutonEntrepreneuriat;

    private const float IntervalleActualisationSecondes = 0.5f;

    private ServiceRepartitionTemps service;
    private GameObject bloqueurModal;
    private float prochaineActualisation;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReinitialiserEtatStatique()
    {
        // Les scenes de test peuvent recharger le runtime sans redemarrer le
        // domaine. La logique statique se limite a des recherches ponctuelles,
        // donc aucun etat n'est conserve entre deux parties.
    }

    private void OnEnable()
    {
        RepartitionTempsUI.AllocationValidee += OnAllocationValidee;
    }

    private void OnDisable()
    {
        RepartitionTempsUI.AllocationValidee -= OnAllocationValidee;
    }

    private void Start()
    {
        ActualiserPhase(true);
    }

    private void Update()
    {
        if (Time.unscaledTime < prochaineActualisation)
        {
            return;
        }

        prochaineActualisation =
            Time.unscaledTime + IntervalleActualisationSecondes;
        ActualiserPhase(true);
    }

    /// <summary>
    /// Ouvre la phase de repartition si l'allocation courante n'est pas validee.
    /// </summary>
    /// <param name="donneesPartie">Donnees racine a utiliser si le controleur
    /// n'a pas encore resolu sa reference.</param>
    /// <returns>True si un controleur de scene a ete trouve.</returns>
    public static bool OuvrirPhaseObligatoireSiNecessaire(
        GameData donneesPartie)
    {
        PhaseRepartitionTempsController controleur =
            Object.FindFirstObjectByType<PhaseRepartitionTempsController>(
                FindObjectsInactive.Include);
        if (controleur == null)
        {
            return false;
        }

        if (donneesPartie != null)
        {
            controleur.gameData = donneesPartie;
        }

        controleur.ActualiserPhase(true);
        return true;
    }

    /// <summary>
    /// Recalcule l'accessibilite de la phase et des boutons applicatifs.
    /// </summary>
    /// <param name="ouvrirSiNonValidee">Lorsque true, force l'ouverture de la
    /// fenetre si la repartition mensuelle attend encore une validation.</param>
    public void ActualiserPhase(bool ouvrirSiNonValidee)
    {
        if (!ResoudreService())
        {
            AppliquerInteractable(false);
            return;
        }

        bool allocationValidee = service.EstAllocationValidee();
        if (!allocationValidee && ouvrirSiNonValidee)
        {
            OuvrirFenetreRepartition();
        }

        AppliquerInteractable(allocationValidee);
        ActiverBloqueurModal(!allocationValidee);
    }

    private void OnAllocationValidee()
    {
        ActualiserPhase(false);
    }

    private void OuvrirFenetreRepartition()
    {
        ResoudreFenetre();
        if (fenetreRepartitionTemps == null)
        {
            return;
        }

        fenetreRepartitionTemps.SetActive(true);
        fenetreRepartitionTemps.transform.SetAsLastSibling();
    }

    private void AppliquerInteractable(bool allocationValidee)
    {
        ResoudreBoutons();
        SetInteractable(boutonPassageMois, allocationValidee);
        SetInteractable(
            boutonBanque,
            allocationValidee &&
                service.PeutOuvrir(TypeApplicationTemps.Banque));
        SetInteractable(
            boutonActualites,
            allocationValidee &&
                service.PeutOuvrir(TypeApplicationTemps.Actualites));
        SetInteractable(
            boutonSalariat,
            allocationValidee &&
                service.PeutOuvrir(TypeApplicationTemps.Salariat));
        SetInteractable(
            boutonBourse,
            allocationValidee &&
                service.PeutOuvrir(TypeApplicationTemps.Bourse));
        SetInteractable(
            boutonEntrepreneuriat,
            allocationValidee &&
                service.PeutOuvrir(TypeApplicationTemps.Entrepreneuriat));
    }

    private void ActiverBloqueurModal(bool actif)
    {
        ResoudreFenetre();
        if (fenetreRepartitionTemps == null ||
            fenetreRepartitionTemps.transform.parent == null)
        {
            return;
        }

        if (bloqueurModal == null)
        {
            bloqueurModal = new GameObject(
                "RepartitionTempsModalBlocker",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            bloqueurModal.transform.SetParent(
                fenetreRepartitionTemps.transform.parent,
                false);

            RectTransform rect =
                bloqueurModal.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = bloqueurModal.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.01f);
            image.raycastTarget = true;
        }

        bloqueurModal.SetActive(actif);
        if (actif)
        {
            bloqueurModal.transform.SetSiblingIndex(
                fenetreRepartitionTemps.transform.GetSiblingIndex());
            fenetreRepartitionTemps.transform.SetAsLastSibling();
        }
    }

    private bool ResoudreService()
    {
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
        service = new ServiceRepartitionTemps(
            gameData.joueur.tempsApplications);
        return true;
    }

    private void ResoudreFenetre()
    {
        if (fenetreRepartitionTemps != null)
        {
            return;
        }

        RepartitionTempsUI ui =
            Object.FindFirstObjectByType<RepartitionTempsUI>(
                FindObjectsInactive.Include);
        if (ui != null)
        {
            fenetreRepartitionTemps = ui.gameObject;
        }
    }

    private void ResoudreBoutons()
    {
        if (boutonPassageMois == null)
        {
            boutonPassageMois = TrouverBouton("play");
        }
        if (boutonBanque == null)
        {
            boutonBanque = TrouverBouton("BankBouton");
        }
        if (boutonActualites == null)
        {
            boutonActualites = TrouverBouton("ActuBouton");
        }
        if (boutonSalariat == null)
        {
            boutonSalariat = TrouverBouton("SalariatBouton");
        }
        if (boutonBourse == null)
        {
            boutonBourse = TrouverBouton("BourseBouton");
        }
        if (boutonEntrepreneuriat == null)
        {
            boutonEntrepreneuriat = TrouverBouton("EntreprenariatApp");
        }
    }

    private static Button TrouverBouton(string nomObjet)
    {
        Button[] boutons = Object.FindObjectsByType<Button>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        foreach (Button bouton in boutons)
        {
            if (bouton != null && bouton.name == nomObjet)
            {
                return bouton;
            }
        }

        return null;
    }

    private static void SetInteractable(Button bouton, bool interactable)
    {
        if (bouton != null)
        {
            bouton.interactable = interactable;
        }
    }
}
