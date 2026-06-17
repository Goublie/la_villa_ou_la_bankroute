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
    private static PhaseRepartitionTempsController instanceCourante;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReinitialiserEtatStatique()
    {
        instanceCourante = null;
    }

    private void OnEnable()
    {
        if (instanceCourante != null && instanceCourante != this)
        {
            Debug.LogError(
                "[Temps] Plusieurs PhaseRepartitionTempsController actifs.");
        }

        instanceCourante = this;
        RepartitionTempsUI.AllocationValidee += OnAllocationValidee;
    }

    private void OnDisable()
    {
        RepartitionTempsUI.AllocationValidee -= OnAllocationValidee;
        if (instanceCourante == this)
        {
            instanceCourante = null;
        }
    }

    private void Start()
    {
        ActualiserPhase(true, false);
    }

    private void Update()
    {
        if (Time.unscaledTime < prochaineActualisation)
        {
            return;
        }

        prochaineActualisation =
            Time.unscaledTime + IntervalleActualisationSecondes;
        ActualiserPhase(true, false);
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
            TrouverControleur(true);
        if (controleur == null)
        {
            return false;
        }

        if (donneesPartie != null)
        {
            controleur.gameData = donneesPartie;
        }

        bool phaseDisponible = controleur.ActualiserPhase(true, true);
        if (!phaseDisponible)
        {
            Debug.LogError(
                "[Temps] La phase RepartitionTemps n'a pas pu etre ouverte.");
        }

        return phaseDisponible;
    }

    /// <summary>
    /// Recalcule l'accessibilite de la phase et des boutons applicatifs.
    /// </summary>
    /// <param name="ouvrirSiNonValidee">Lorsque true, force l'ouverture de la
    /// fenetre si la repartition mensuelle attend encore une validation.</param>
    public bool ActualiserPhase(bool ouvrirSiNonValidee)
    {
        return ActualiserPhase(ouvrirSiNonValidee, false);
    }

    private bool ActualiserPhase(bool ouvrirSiNonValidee, bool journaliserErreurs)
    {
        if (!ResoudreService())
        {
            AppliquerInteractable(false);
            if (journaliserErreurs)
            {
                Debug.LogError(
                    "[Temps] GameData introuvable pour RepartitionTemps.");
            }

            return false;
        }

        bool allocationValidee = service.EstAllocationValidee();
        bool fenetreDisponible = true;
        if (!allocationValidee && ouvrirSiNonValidee)
        {
            fenetreDisponible = OuvrirFenetreRepartition(journaliserErreurs);
        }

        AppliquerInteractable(allocationValidee);
        bool bloqueurDisponible =
            ActiverBloqueurModal(!allocationValidee, journaliserErreurs);
        return allocationValidee ||
            (fenetreDisponible && bloqueurDisponible && FenetreVisible());
    }

    private void OnAllocationValidee()
    {
        ActualiserPhase(false, false);
    }

    private bool OuvrirFenetreRepartition(bool journaliserErreurs)
    {
        ResoudreFenetre(journaliserErreurs);
        if (fenetreRepartitionTemps == null)
        {
            return false;
        }

        ActiverParentsFenetre(journaliserErreurs);
        fenetreRepartitionTemps.SetActive(true);
        fenetreRepartitionTemps.transform.SetAsLastSibling();
        return FenetreVisible();
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

    private bool ActiverBloqueurModal(bool actif, bool journaliserErreurs)
    {
        ResoudreFenetre(journaliserErreurs);
        if (fenetreRepartitionTemps == null ||
            fenetreRepartitionTemps.transform.parent == null)
        {
            if (journaliserErreurs)
            {
                Debug.LogError(
                    "[Temps] Parent de la fenetre RepartitionTemps introuvable.");
            }

            return false;
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

        return true;
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

    private void ResoudreFenetre(bool journaliserErreurs)
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

        if (fenetreRepartitionTemps == null && journaliserErreurs)
        {
            Debug.LogError(
                "[Temps] Fenetre RepartitionTemps introuvable dans la scene.");
        }
        else if (fenetreRepartitionTemps == gameObject && journaliserErreurs)
        {
            Debug.LogError(
                "[Temps] Le controleur ne doit pas etre attache a la fenetre RepartitionTemps.");
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

    private static PhaseRepartitionTempsController TrouverControleur(
        bool journaliserErreurs)
    {
        if (instanceCourante != null)
        {
            return instanceCourante;
        }

        PhaseRepartitionTempsController[] controleurs =
            Object.FindObjectsByType<PhaseRepartitionTempsController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
        if (controleurs.Length == 0)
        {
            if (journaliserErreurs)
            {
                Debug.LogError(
                    "[Temps] Aucun PhaseRepartitionTempsController trouve dans la scene.");
            }

            return null;
        }

        if (controleurs.Length > 1 && journaliserErreurs)
        {
            Debug.LogError(
                "[Temps] Plusieurs PhaseRepartitionTempsController trouves dans la scene.");
        }

        foreach (PhaseRepartitionTempsController controleur in controleurs)
        {
            if (controleur != null && controleur.isActiveAndEnabled)
            {
                instanceCourante = controleur;
                return controleur;
            }
        }

        if (journaliserErreurs)
        {
            Debug.LogError(
                "[Temps] PhaseRepartitionTempsController trouve mais inactif.");
        }

        return controleurs[0];
    }

    private void ActiverParentsFenetre(bool journaliserErreurs)
    {
        if (fenetreRepartitionTemps == null)
        {
            return;
        }

        Transform parent = fenetreRepartitionTemps.transform.parent;
        if (parent == null)
        {
            if (journaliserErreurs)
            {
                Debug.LogError(
                    "[Temps] La fenetre RepartitionTemps n'a pas de parent actif.");
            }

            return;
        }

        while (parent != null)
        {
            if (!parent.gameObject.activeSelf)
            {
                parent.gameObject.SetActive(true);
            }

            parent = parent.parent;
        }
    }

    private bool FenetreVisible()
    {
        return fenetreRepartitionTemps != null &&
            fenetreRepartitionTemps.activeInHierarchy;
    }
}
