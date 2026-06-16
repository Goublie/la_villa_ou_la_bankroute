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
        if (env == null)
        {
            env = new DonneesEnvironnement();
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
        historiqueSnapshots.Clear();
    }

    /// <summary>
    /// Extrait la trajectoire financière réelle du joueur sur la même période (les 12 derniers mois).
    /// </summary>
    public List<PointPatrimoine> ObtenirHistoriquePatrimoineReel()
    {
        List<PointPatrimoine> resultats = new List<PointPatrimoine>();
        if (historiqueSnapshots == null) return resultats;

        int totalSnapshots = historiqueSnapshots.Count;
        // On récupère les 12 mois de l'année, en sautant le snapshot initial de baseline
        int startIndex = totalSnapshots >= 13 ? (totalSnapshots - 12) : 1;

        for (int i = startIndex; i < totalSnapshots; i++)
        {
            SnapshotEtatJeu snap = historiqueSnapshots[i];
            
            resultats.Add(new PointPatrimoine
            {
                indexMois = snap.indexMois,
                moisCalendrier = snap.moisCalendrier,
                patrimoineTotal = snap.joueur != null ? snap.joueur.CalculPatrimoineTotal() : new argent(0)
            });
        }

        return resultats;
    }
}

[System.Serializable]
public struct PointPatrimoine
{
    public int indexMois;
    public Mois moisCalendrier;
    public argent patrimoineTotal;
}
