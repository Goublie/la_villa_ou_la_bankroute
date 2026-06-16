using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Facade d'affichage generale de l'onglet Salariat.
/// </summary>
/// <remarks>
/// Les controles metier restent dans <see cref="ServiceSalariat"/> et les
/// interactions du prefab sont portees par les controleurs specialises. Cette
/// classe lit seulement <see cref="DonneesSalariat"/> pour afficher un resume
/// coherent si elle est utilisee par une variante de l'UI.
/// </remarks>
public class SalariatUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private GameData gameData;

    [Header("Rubrique : Poste Actuel")]
    [SerializeField] private TextMeshProUGUI txtEntreprise;
    [SerializeField] private TextMeshProUGUI txtAnciennete;
    [SerializeField] private TextMeshProUGUI txtSalaire;
    [SerializeField] private TextMeshProUGUI txtHeures;
    [SerializeField] private Slider jaugeSatisfaction;
    [SerializeField] private Image fillSatisfaction;

    [Header("Fenetre : Chercher un Emploi")]
    [SerializeField] private GameObject panelOffresEmploi;

    private DonneesSalariat donnees;

    private void OnEnable()
    {
        ActionPlay.OnMoisPasse += InitialiserAffichage;
        InitialiserAffichage();
    }

    private void OnDisable()
    {
        ActionPlay.OnMoisPasse -= InitialiserAffichage;
    }

    /// <summary>
    /// Rafraichit le resume depuis l'agregat persistant du joueur.
    /// </summary>
    public void InitialiserAffichage()
    {
        if (!ResoudreDonnees())
        {
            AfficherEtatVide();
            return;
        }

        if (txtEntreprise != null)
        {
            txtEntreprise.text = donnees.entreprise;
        }

        if (txtAnciennete != null)
        {
            txtAnciennete.text = donnees.ancienneteMois + " mois";
        }

        if (txtSalaire != null)
        {
            txtSalaire.text =
                new argent(donnees.salaireMensuelCentimes).ToString() +
                " brut / mois";
            txtSalaire.color = Color.green;
        }

        if (txtHeures != null)
        {
            txtHeures.text = donnees.heuresSemaine + "h / semaine";
        }

        AfficherSatisfaction(donnees.satisfaction);
    }

    /// <summary>
    /// Active le panel de recherche d'emploi s'il existe dans le prefab.
    /// </summary>
    public void OuvrirRechercheEmploi()
    {
        if (panelOffresEmploi != null)
        {
            panelOffresEmploi.SetActive(true);
        }
    }

    private bool ResoudreDonnees()
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
        donnees = gameData.joueur.salariat;
        return donnees != null;
    }

    private void AfficherEtatVide()
    {
        if (txtEntreprise != null)
        {
            txtEntreprise.text = "Aucune";
        }

        if (txtAnciennete != null)
        {
            txtAnciennete.text = "0 mois";
        }

        if (txtSalaire != null)
        {
            txtSalaire.text = "0.00 EUR brut / mois";
        }

        if (txtHeures != null)
        {
            txtHeures.text = "0h / semaine";
        }

        AfficherSatisfaction(0);
    }

    private void AfficherSatisfaction(int score)
    {
        int valeur = Mathf.Clamp(score, 0, 100);
        if (jaugeSatisfaction != null)
        {
            jaugeSatisfaction.value = valeur;
        }

        if (fillSatisfaction == null)
        {
            return;
        }

        if (valeur < 35)
        {
            fillSatisfaction.color = Color.red;
        }
        else if (valeur < 65)
        {
            fillSatisfaction.color = new Color(1f, 0.5f, 0f);
        }
        else
        {
            fillSatisfaction.color = Color.green;
        }
    }
}
