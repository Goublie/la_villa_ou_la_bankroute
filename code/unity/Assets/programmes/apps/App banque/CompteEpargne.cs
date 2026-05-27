using UnityEngine;
using System.Collections.Generic;

public class CompteBanquaire : MonoBehaviour
{
    public Investissement epargne; //système d'épargne du compte banquaire

    [SerializeField] public GameData gameData; //Référence aux données du jeu

    [SerializeField] public  EpargneUI EpargneUI;

    [SerializeField] public HUDManager HUD;

    void Start()
    {
        gameData.investissements.Add(epargne);
        EpargneUI.ActualiserAffichage();
    }

    //Retire de l'argent du solde du compte banquaire
    public bool debiter(argent montant)
    {
        if (montant.centimes < 0)
        {
            Debug.Log("Le montant à débiter doit être positif.");
            return false;
        }

        if (montant > epargne.sommeInvestie)
        {
            Debug.Log("Vous n'avez pas assez d'argent sur votre compte pour débiter ce montant.");
            return false;
        }

        epargne.sommeInvestie -= montant;
        gameData.argent += montant; // On ajoute le montant débité à l'argent du joueur
        HUD.ActualiserAffichage();
        EpargneUI.ActualiserAffichage();
        return true;
    }

    public bool crediter(argent montant)
    {
        if (montant.centimes < 0)
        {
            Debug.Log("Le montant à créditer doit être positif.");
            return false;
        }
        if (montant > gameData.argent)
        {
            Debug.Log("Vous n'avez pas assez d'argent pour créditer ce montant.");
            return false;
        }
        epargne.sommeInvestie += montant;
        gameData.argent -= montant; // On retire le montant du joueur
        HUD.ActualiserAffichage();
        EpargneUI.ActualiserAffichage();
        return true;
    }
}