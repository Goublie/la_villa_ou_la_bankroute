// Contient les données du jeu //
using UnityEngine;
using System.Collections.Generic;

 public enum Mois
{
    Janvier, Fevrier, Mars, Avril, Mai, Juin,
    Juillet, Aout, Septembre, Octobre, Novembre, Decembre
}

[CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Objects/GameData")]
public class GameData : ScriptableObject
{
    public DonneesJoueur joueur = new DonneesJoueur();
    public DonneesEnvironnement env = new DonneesEnvironnement();

    public int nombreMoisPasses = 0; 
    public Mois moisActuel = Mois.Juillet; // Le jeu commence en Juillet
    public List<SnapshotEtatJeu> historiqueSnapshots = new List<SnapshotEtatJeu>(); // Historique des photographies mensuelles du jeu pour le mode What-if
    
    private void OnEnable()
    {
        if (joueur == null)
        {
            joueur = new DonneesJoueur();
        }
        joueur.InitialiserSiNecessaire();
        if (env == null)
        {
            env = new DonneesEnvironnement();
        }
        if (historiqueSnapshots == null)
        {
            historiqueSnapshots = new List<SnapshotEtatJeu>();
        }
    }
    
    /// <summary>
    /// Réinitialise toutes les données de jeu à leur état par défaut.
    /// Appelé lors du démarrage d'une nouvelle partie.
    /// </summary>
    public void ResetData()
    {
        joueur = new DonneesJoueur();
        env = new DonneesEnvironnement();
        nombreMoisPasses = 0;
        moisActuel = Mois.Juillet;
        if (historiqueSnapshots == null)
        {
            historiqueSnapshots = new List<SnapshotEtatJeu>();
        }
        else
        {
            historiqueSnapshots.Clear();
        }
    }
}
