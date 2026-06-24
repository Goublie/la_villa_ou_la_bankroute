using UnityEngine;
using System.Collections;

public class FenetreAnimation : MonoBehaviour
{
    [Header("Paramètres de l'animation")]
    public float dureeAnimation = 0.25f; // Très rapide pour ne pas frustrer le joueur
    public Vector3 tailleInitiale = new Vector3(0.1f, 0.1f, 0.1f);
    
    private Vector3 tailleFinale = Vector3.one;
    private Coroutine coroutineAnimation;

    // OnEnable se déclenche automatiquement dès que SetActive(true) est appelé !
    private void OnEnable()
    {
        // On force la taille de départ pour éviter un flash visuel
        transform.localScale = tailleInitiale;

        if (coroutineAnimation != null)
        {
            StopCoroutine(coroutineAnimation);
        }
        coroutineAnimation = StartCoroutine(SequenceOuverture());
    }

    private IEnumerator SequenceOuverture()
    {
        float tempsEcoule = 0f;

        while (tempsEcoule < dureeAnimation)
        {
            tempsEcoule += Time.deltaTime;
            float progression = tempsEcoule / dureeAnimation;

            // Formule mathématique pour ajouter un petit effet de rebond rétro à la fin (Overshoot)
            // Elle fait monter la valeur un peu au-dessus de 1 avant de se stabiliser
            float interpolationRebond = 1f - Mathf.Pow(1f - progression, 3f); 
            
            // Si tu veux un rebond encore plus prononcé (style "Pop") :
            // float interpolationRebond = Mathf.Sin(progression * Mathf.PI * 0.5f); // Option alternative linéaire lisse

            transform.localScale = Vector3.LerpUnclamped(tailleInitiale, tailleFinale, interpolationRebond);
            yield return null;
        }

        // On s'assure d'être parfaitement à la taille finale
        transform.localScale = tailleFinale;
        coroutineAnimation = null;
    }
}