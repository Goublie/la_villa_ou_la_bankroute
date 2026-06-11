using UnityEngine;
using UnityEngine.UI;

public class ManagerTemps : MonoBehaviour
{
    [Header("Donnees")]
    public GameData gameData;
    public ActionPlay actionPlay;

    [Header("UI Allocation")]
    public GameObject repartitionTempsWindow;

    [Header("Raccourcis Bureau")]
    public Button boutonBanque;
    public Button boutonActualites;
    public Button boutonSalariat;
    public Button boutonBourse;

    private bool allocationEnCours = true;

    void OnEnable()
    {
        ActionPlay.OnMoisPasse += ForceOuvrirRepartition;
    }

    void OnDisable()
    {
        ActionPlay.OnMoisPasse -= ForceOuvrirRepartition;
    }

    void Start()
    {
        if (repartitionTempsWindow == null)
        {
            repartitionTempsWindow = GameObject.Find("RepartitionTemps");
            if (repartitionTempsWindow != null)
            {
                Debug.Log("[ManagerTemps] RepartitionTempsWindow trouve dynamiquement par nom.");
            }
        }

        if (gameData == null)
        {
            Debug.LogError("[ManagerTemps] GameData n'est pas assigne dans l'inspecteur !");
            return;
        }

        // Verifier s'il y a deja du temps alloue (chargement de partie / retour scene)
        if (AUnTempsAlloue())
        {
            allocationEnCours = false;
            if (repartitionTempsWindow != null)
            {
                repartitionTempsWindow.SetActive(false);
            }
            UpdateBoutonsBureau();
        }
        else
        {
            allocationEnCours = true;
            if (repartitionTempsWindow != null)
            {
                repartitionTempsWindow.SetActive(true);
            }
            UpdateBoutonsBureau(false);
        }
    }

    void Update()
    {
        if (gameData == null) return;

        if (allocationEnCours)
        {
            // Desactive les raccourcis pendant l'allocation pour forcer l'usage de la fenetre de repartition
            UpdateBoutonsBureau(false);

            // Si la fenetre d'allocation est fermee (ou absente) et qu'on a bien alloue du temps, on debute le mois de jeu
            bool windowClosed = (repartitionTempsWindow == null) || !repartitionTempsWindow.activeInHierarchy;
            if (windowClosed && AUnTempsAlloue())
            {
                allocationEnCours = false;
                Debug.Log("[ManagerTemps] Allocation du temps validee. Lancement de la phase active du mois.");
                UpdateBoutonsBureau();
            }
            return;
        }

        // --- PHASE DE JEU ACTIVEDES FENETRES ---

        // Verifier si toutes les applications ont epuise leur temps
        if (!AUnTempsAlloue())
        {
            Debug.Log("[ManagerTemps] Tout le temps de ce mois est epuise. Passage automatique au mois suivant.");
            
            // On repasse en mode allocation pour bloquer les frames suivantes pendant le changement de scene / chargement
            allocationEnCours = true;

            if (actionPlay != null)
            {
                actionPlay.Jouer();
            }
            else
            {
                ActionPlay ap = FindObjectOfType<ActionPlay>();
                if (ap != null)
                {
                    ap.Jouer();
                }
                else
                {
                    Debug.LogError("[ManagerTemps] Impossible de passer le mois : composant ActionPlay introuvable !");
                }
            }
            return;
        }

        // Met a jour l'interactivite des raccourcis de bureau en fonction du temps restant
        UpdateBoutonsBureau();
    }

    private bool AUnTempsAlloue()
    {
        if (gameData == null) return false;
        return (gameData.joueur.tempsRestantBanque > 0f) ||
               (gameData.joueur.tempsRestantActualites > 0f) ||
               (gameData.joueur.tempsRestantSalariat > 0f) ||
               (gameData.joueur.tempsRestantBourse > 0f);
    }

    private void ForceOuvrirRepartition()
    {
        Debug.Log("[ManagerTemps] Evenement OnMoisPasse detecte. Passage en phase d'allocation de temps.");
        allocationEnCours = true;
        
        if (repartitionTempsWindow == null)
        {
            repartitionTempsWindow = GameObject.Find("RepartitionTemps");
        }

        if (repartitionTempsWindow != null)
        {
            repartitionTempsWindow.SetActive(true);
        }
        UpdateBoutonsBureau(false);
    }

    private void UpdateBoutonsBureau()
    {
        if (gameData == null) return;
        
        bool tBanque = gameData.joueur.tempsRestantBanque > 0f;
        bool tActualites = gameData.joueur.tempsRestantActualites > 0f;
        bool tSalariat = gameData.joueur.tempsRestantSalariat > 0f;
        bool tBourse = gameData.joueur.tempsRestantBourse > 0f;

        if (boutonBanque != null && boutonBanque.interactable != tBanque)
        {
            Debug.Log("[ManagerTemps] Modification bouton Banque -> interactable: " + tBanque + " (temps restant: " + gameData.joueur.tempsRestantBanque + "s)");
            boutonBanque.interactable = tBanque;
        }
        if (boutonActualites != null && boutonActualites.interactable != tActualites)
        {
            Debug.Log("[ManagerTemps] Modification bouton Actualites -> interactable: " + tActualites + " (temps restant: " + gameData.joueur.tempsRestantActualites + "s)");
            boutonActualites.interactable = tActualites;
        }
        if (boutonSalariat != null && boutonSalariat.interactable != tSalariat)
        {
            Debug.Log("[ManagerTemps] Modification bouton Salariat -> interactable: " + tSalariat + " (temps restant: " + gameData.joueur.tempsRestantSalariat + "s)");
            boutonSalariat.interactable = tSalariat;
        }
        if (boutonBourse != null && boutonBourse.interactable != tBourse)
        {
            Debug.Log("[ManagerTemps] Modification bouton Bourse -> interactable: " + tBourse + " (temps restant: " + gameData.joueur.tempsRestantBourse + "s)");
            boutonBourse.interactable = tBourse;
        }
    }

    private void UpdateBoutonsBureau(bool interactable)
    {
        if (boutonBanque != null && boutonBanque.interactable != interactable) boutonBanque.interactable = interactable;
        if (boutonActualites != null && boutonActualites.interactable != interactable) boutonActualites.interactable = interactable;
        if (boutonSalariat != null && boutonSalariat.interactable != interactable) boutonSalariat.interactable = interactable;
        if (boutonBourse != null && boutonBourse.interactable != interactable) boutonBourse.interactable = interactable;
    }
}
