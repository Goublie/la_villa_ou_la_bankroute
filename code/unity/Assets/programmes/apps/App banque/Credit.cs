using UnityEngine;
using TMPro; 
using UnityEngine.UI;

public class Credit : MonoBehaviour
{

    [SerializeField] GameData gameData; 

    [SerializeField] HUDManager hudManager;

    [SerializeField] Tableau tab;

    public TMP_InputField entreeArgent;

    void Start()
    {
        entreeArgent = GetComponentsInChildren<TMP_InputField>()[0];
    }

    public void crediter()
    {
        Debug.Log("Créditer");
        //On récupère le montant entré par l'utilisateur
        string montantStr = entreeArgent.text;
        
        if (string.IsNullOrEmpty(montantStr))
        {
            Debug.Log("Veuillez entrer un montant.");
            return;
        }

        float montant;
        if (!float.TryParse(montantStr, out montant))
        {
            Debug.Log("Veuillez entrer un montant valide.");
            return;
        }

        if (montant < 0)
        {
            Debug.Log("Le montant doit être positif.");
            return;
        }

        argent somme = new argent(montant);

        if (somme > gameData.argent)
        {
            Debug.Log("Vous n'avez pas assez d'argent pour créditer ce montant.");
            return;
        }

        tab.add("Credit", "+ " + (somme).ToString());

        gameData.argent -= somme; // On retire le montant crédité de l'argent du joueur
        hudManager.ActualiserAffichage(); // On met à jour l'affichage du HUD
    }
}