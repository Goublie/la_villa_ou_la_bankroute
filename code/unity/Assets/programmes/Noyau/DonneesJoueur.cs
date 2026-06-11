using System;
using System.Collections.Generic;

/// <summary>
/// Encapsule l'ensemble des données personnelles, financières et professionnelles propres au joueur.
/// </summary>
[System.Serializable]
public class DonneesJoueur
{
    // Comptes bancaires du joueur (solde et historique des flux)
    public Dictionary<string, CompteBanquaire> comptes = new Dictionary<string, CompteBanquaire>() { { "courant", new CompteBanquaire(new argent(100000)) } };

    // Le revenu d'activité régulier brut mensuel
    public argent salaire = new argent(0);

    // Le capital temps restant à allouer chaque mois
    public int energie = 100;

    // Temps initial alloue (en minutes) pour chaque application pour le mois en cours
    public int tempsInitialBanque = 0;
    public int tempsInitialActualites = 0;
    public int tempsInitialSalariat = 0;
    public int tempsInitialBourse = 0;

    // Temps restant (en secondes) pour chaque application pour le mois en cours
    public float tempsRestantBanque = 0f;
    public float tempsRestantActualites = 0f;
    public float tempsRestantSalariat = 0f;
    public float tempsRestantBourse = 0f;

    // L'indice de santé mentale régissant la fatigue et l'efficacité générale
    public int santeMentale = 100;

    // Liste des produits financiers et investissements actifs
    public List<Investissement> investissements = new List<Investissement>();

    /// <summary>
    /// Calcule le patrimoine total du joueur en sommant la valeur de tous ses actifs
    /// qui implémentent l'interface IPatrimoine (comptes bancaires et investissements).
    /// </summary>
    public argent CalculPatrimoineTotal()
    {
        argent total = new argent(0);

        if (comptes != null)
        {
            foreach (var compte in comptes.Values)
            {
                if (compte != null)
                {
                    total += compte.GetValeurPatrimoine();
                }
            }
        }

        if (investissements != null)
        {
            foreach (var invest in investissements)
            {
                if (invest != null)
                {
                    total += invest.GetValeurPatrimoine();
                }
            }
        }

        return total;
    }
}
