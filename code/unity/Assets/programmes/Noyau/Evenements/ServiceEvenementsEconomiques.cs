using System;

/// <summary>
/// Point d'entree metier des futurs contenus Actualites.
/// </summary>
/// <remarks>
/// Le service cible les agregats et services metier, jamais les interfaces.
/// Aucune interface d'impact generique n'est creee tant qu'un second systeme
/// ne partage pas un contrat concret avec la Bourse.
/// </remarks>
public sealed class ServiceEvenementsEconomiques
{
    private readonly GameData gameData;

    /// <summary>
    /// Cree un routeur d'evenements lie a la partie courante.
    /// </summary>
    public ServiceEvenementsEconomiques(GameData gameData)
    {
        this.gameData = gameData ??
            throw new ArgumentNullException(nameof(gameData));
    }

    /// <summary>
    /// Enregistre un impact de marche et revalorise immediatement le portefeuille.
    /// </summary>
    /// <param name="impact">
    /// Impact persistant : cible, debut, duree et coefficients de prix.
    /// </param>
    /// <returns>Succes ou cause de rejet sans mutation partielle.</returns>
    /// <remarks>
    /// Effet de bord : ajoute l'impact aux donnees Bourse. Les prix futurs le
    /// reutilisent pendant sa periode d'activite et les snapshots le copient.
    /// </remarks>
    public ResultatOperation AppliquerImpactMarche(
        ImpactEvenementMarche impact)
    {
        if (impact == null ||
            string.IsNullOrWhiteSpace(impact.evenementId) ||
            string.IsNullOrWhiteSpace(impact.actifId))
        {
            return ResultatOperation.Echec(
                "L'impact de marche est incomplet.",
                "impact_invalide");
        }

        if (gameData.joueur == null)
        {
            gameData.joueur = new DonneesJoueur();
        }
        gameData.joueur.InitialiserSiNecessaire();

        ServiceBourse bourse =
            new ServiceBourse(gameData.joueur.bourse);
        bourse.AppliquerImpactEvenement(impact);
        bourse.AppliquerEvolutionMensuelle(
            gameData.nombreMoisPasses);
        return ResultatOperation.Reussite(
            "Impact de marche enregistre.",
            default,
            "impact_marche_applique");
    }
}
