using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mois civils utilises par le calendrier du jeu.
/// </summary>
public enum Mois
{
    Janvier,
    Fevrier,
    Mars,
    Avril,
    Mai,
    Juin,
    Juillet,
    Aout,
    Septembre,
    Octobre,
    Novembre,
    Decembre
}

/// <summary>
/// Racine ScriptableObject de l'etat courant et des snapshots de la partie.
/// </summary>
/// <remarks>
/// L'objet est partage entre les scenes. Les donnees metier vivent dans les
/// agregats serialisables et ne referencent aucun composant UI. Une sauvegarde
/// durable sur disque devra serialiser ces agregats explicitement ; elle ne
/// doit pas s'appuyer sur les dictionnaires Unity non pris en charge.
/// </remarks>
[CreateAssetMenu(
    fileName = "GameData",
    menuName = "Scriptable Objects/GameData")]
public class GameData : ScriptableObject
{
    /// <summary>
    /// Agregat personnel, financier et professionnel du joueur.
    /// </summary>
    public DonneesJoueur joueur = new DonneesJoueur();

    /// <summary>
    /// Variables economiques externes partagees par les systemes.
    /// </summary>
    public DonneesEnvironnement env = new DonneesEnvironnement();

    /// <summary>
    /// Historique persistant des rumeurs, confirmations et publications.
    /// </summary>
    public DonneesEvenements evenements = new DonneesEvenements();

    /// <summary>
    /// Etat persistant et complet de la strategie alternative What If.
    /// </summary>
    /// <remarks>
    /// Cet agregat reste separe du joueur reel. Les snapshots mensuels
    /// conservent uniquement les connaissances historiques necessaires.
    /// </remarks>
    public DonneesWhatIf whatIf = new DonneesWhatIf();

    /// <summary>
    /// Index absolu des mois ecoules depuis juillet 2026.
    /// </summary>
    public int nombreMoisPasses;

    /// <summary>
    /// Mois civil courant. Une nouvelle partie commence en juillet.
    /// </summary>
    public Mois moisActuel = Mois.Juillet;

    /// <summary>
    /// Photographies profondes mensuelles utilisees par le mode What If.
    /// </summary>
    public List<SnapshotEtatJeu> historiqueSnapshots =
        new List<SnapshotEtatJeu>();

    private void OnEnable()
    {
        AssurerAgregats();
    }

    /// <summary>
    /// Remplace l'etat courant par une nouvelle partie en juillet.
    /// </summary>
    /// <remarks>
    /// Effet de bord : supprime tous les snapshots et toutes les progressions
    /// de la partie precedente.
    /// </remarks>
    public void ResetData()
    {
        joueur = new DonneesJoueur();
        env = new DonneesEnvironnement();
        evenements = new DonneesEvenements();
        whatIf = new DonneesWhatIf();
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

    private void AssurerAgregats()
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

        if (evenements == null)
        {
            evenements = new DonneesEvenements();
        }
        evenements.InitialiserSiNecessaire();

        if (whatIf == null)
        {
            whatIf = new DonneesWhatIf();
        }
        whatIf.InitialiserSiNecessaire();

        if (historiqueSnapshots == null)
        {
            historiqueSnapshots = new List<SnapshotEtatJeu>();
        }
    }
}
