using UnityEngine;
using UnityEngine.Events;
using TMPro;

[System.Serializable]
public class SecteurRatingEvent : UnityEvent<string, int> { }

public class DepenseSecteur : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("La zone de texte qui affichera le titre")]
    public TextMeshProUGUI titleText;
    
    [Tooltip("La zone de texte qui affichera la description")]
    public TextMeshProUGUI descriptionText;
    
    [Tooltip("Le composant RatingBar de ce prefab")]
    public RatingBar ratingBar;

    [Header("Contenu")]
    [Tooltip("Titre du secteur (ex: Santé, Éducation)")]
    public string titreSecteur = "Nouveau Secteur";
    
    [Tooltip("Description du secteur")]
    [TextArea(3, 10)]
    public string descriptionSecteur = "Description...";

    [Header("Événements")]
    [Tooltip("Se déclenche quand la note change. Renvoie (Titre, Nouvelle Note)")]
    public SecteurRatingEvent onRatingChangedEvent;

    // L'Action C# standard est également disponible si d'autres scripts préfèrent s'y abonner en code plutôt que via l'inspecteur
    public System.Action<string, int> OnRatingChangedAction;

    private void OnValidate()
    {
        // OnValidate permet de voir les modifications en temps réel dans l'éditeur Unity !
        UpdateUI();
    }

    private void Awake()
    {
        UpdateUI();

        if (ratingBar != null)
        {
            // On s'abonne à l'événement de la RatingBar
            ratingBar.onRatingChanged.AddListener(HandleRatingChanged);
        }
        else
        {
            Debug.LogWarning($"[DepenseSecteur] Aucune RatingBar n'est assignée sur {gameObject.name} !");
        }
    }

    private void HandleRatingChanged(int nouvelleNote)
    {
        // On avertit le reste du jeu que ce secteur a été modifié
        onRatingChangedEvent?.Invoke(titreSecteur, nouvelleNote);
        OnRatingChangedAction?.Invoke(titreSecteur, nouvelleNote);
    }

    /// <summary>
    /// Met à jour l'interface visuelle avec les valeurs actuelles du script.
    /// </summary>
    public void UpdateUI()
    {
        if (titleText != null) titleText.text = titreSecteur;
        if (descriptionText != null) descriptionText.text = descriptionSecteur;
    }

    /// <summary>
    /// Permet de modifier le titre facilement depuis un autre script.
    /// </summary>
    public void SetTitre(string nouveauTitre)
    {
        titreSecteur = nouveauTitre;
        UpdateUI();
    }

    /// <summary>
    /// Permet de modifier la description facilement depuis un autre script.
    /// </summary>
    public void SetDescription(string nouvelleDescription)
    {
        descriptionSecteur = nouvelleDescription;
        UpdateUI();
    }
    
    /// <summary>
    /// Permet de forcer une note depuis un autre script.
    /// </summary>
    public void SetRating(int note)
    {
        if (ratingBar != null)
        {
            ratingBar.SetRating(note);
        }
    }
}
