using System;

/// <summary>
/// Photographie profonde de l'etat metier a un mois donne.
/// </summary>
/// <remarks>
/// Le constructeur ne valorise ni ne modifie l'etat reel. Les services de
/// passage mensuel doivent avoir termine leurs calculs avant sa creation.
/// </remarks>
[Serializable]
public class SnapshotEtatJeu
{
    /// <summary>
    /// Nombre de mois ecoules depuis le debut de la partie.
    /// </summary>
    public int indexMois;

    /// <summary>
    /// Mois civil correspondant a la photographie.
    /// </summary>
    public Mois moisCalendrier;

    /// <summary>
    /// Copie profonde de l'agregat joueur.
    /// </summary>
    public DonneesJoueur joueur;

    /// <summary>
    /// Copie profonde de l'environnement economique.
    /// </summary>
    public DonneesEnvironnement env;

    /// <summary>
    /// Copie profonde des rumeurs, confirmations et publications connues.
    /// </summary>
    public DonneesEvenements evenements;

    /// <summary>
    /// Cree un snapshot sans conserver de reference mutable vers
    /// <paramref name="gameData"/>.
    /// </summary>
    public SnapshotEtatJeu(GameData gameData)
    {
        if (gameData == null)
        {
            throw new ArgumentNullException(nameof(gameData));
        }

        indexMois = gameData.nombreMoisPasses;
        moisCalendrier = gameData.moisActuel;
        joueur = gameData.joueur != null
            ? gameData.joueur.Copier()
            : new DonneesJoueur();
        env = gameData.env != null
            ? gameData.env.Copier()
            : new DonneesEnvironnement();
        evenements = gameData.evenements != null
            ? gameData.evenements.Copier()
            : new DonneesEvenements();
    }
}
