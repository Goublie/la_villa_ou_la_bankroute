using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class RatingBar : MonoBehaviour
{
    [Tooltip("Liste des boutons dans l'ordre (gauche à droite). Auto-remplie si laissée vide.")]
    public List<Button> ratingButtons;

    [Tooltip("La note actuelle (0 = aucun, N = max)")]
    public int currentRating = 0;

    [Tooltip("Si décoché, la barre est uniquement pour l'affichage (non cliquable).")]
    public bool isInteractable = false;

    [Tooltip("Événement déclenché quand le joueur change la note en cliquant")]
    public UnityEvent<int> onRatingChanged;

    private List<ToggleImageSwitcher> visualSwitchers;

    void Awake()
    {
        ratingButtons = new List<Button>();
        visualSwitchers = new List<ToggleImageSwitcher>();
        
        Button[] tousLesBoutons = GetComponentsInChildren<Button>(true);
        foreach(Button b in tousLesBoutons)
        {
            string nom = b.gameObject.name.ToLower();
            if (nom.Contains("piece") || nom.Contains("star"))
            {
                ratingButtons.Add(b);
            }
        }

        ratingButtons.Sort((a, b) => 
        {
            float ax = a.transform.localPosition.x;
            float bx = b.transform.localPosition.x;
            if (Mathf.Abs(ax - bx) > 0.1f) return ax.CompareTo(bx);
            
            return a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex());
        });

        if (ratingButtons.Count < 5)
        {
            Debug.LogWarning("Attention, il manque des Boutons sur tes pièces ! (Trouvé : " + ratingButtons.Count + ")");
        }

        for (int i = 0; i < ratingButtons.Count; i++)
        {
            int index = i; 
            ratingButtons[i].onClick.RemoveAllListeners();
            ratingButtons[i].onClick.AddListener(() => OnPieceClicked(index));
            
            ToggleImageSwitcher switcher = ratingButtons[i].GetComponent<ToggleImageSwitcher>();
            visualSwitchers.Add(switcher);
        }
        
        SetInteractable(isInteractable);
        SetRating(currentRating);
    }

    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        foreach (var b in ratingButtons)
        {
            if (b != null)
            {
                b.interactable = isInteractable;
            }
        }
    }

    private void OnPieceClicked(int index)
    {
        if (!isInteractable) return;

        int newRating = index + 1;
        
        // Comportement ergonomique : si on reclique sur la dernière pièce allumée, on l'éteint
        // Puisque nous connaissons le currentRating actuel, il suffit de vérifier si newRating est égal au currentRating.
        if (newRating == currentRating)
        {
            newRating = index;
        }

        SetRating(newRating);
        onRatingChanged?.Invoke(currentRating);
    }

    public void SetRating(int rating)
    {
        currentRating = Mathf.Clamp(rating, 0, ratingButtons.Count);

        for (int i = 0; i < ratingButtons.Count; i++)
        {
            if (i < visualSwitchers.Count && visualSwitchers[i] != null)
            {
                visualSwitchers[i].SetState(i < currentRating);
            }
        }
    }
}
