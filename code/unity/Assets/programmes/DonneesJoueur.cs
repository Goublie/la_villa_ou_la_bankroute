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

    // L'indice de santé mentale régissant la fatigue et l'efficacité générale
    public int santeMentale = 100;

    // Liste des produits financiers et investissements actifs
    public List<Investissement> investissements = new List<Investissement>();
}
