using System;
using System.Collections.Generic;

/// <summary>
/// Resume une consommation des confirmations par le domaine Bourse.
/// </summary>
public sealed class ResultatConsommationImpactsBoursiers
{
    private readonly List<string> diagnostics = new List<string>();

    /// <summary>
    /// Nombre d'evenements entierement appliques puis marques comme consommes.
    /// </summary>
    public int EvenementsConsommes { get; internal set; }

    /// <summary>
    /// Nombre d'impacts de marche persistants ajoutes.
    /// </summary>
    public int ImpactsAppliques { get; internal set; }

    /// <summary>
    /// Diagnostics des evenements refuses sans mutation partielle.
    /// </summary>
    public IReadOnlyList<string> Diagnostics => diagnostics;

    /// <summary>
    /// Indique qu'aucune confirmation boursiere n'a ete rejetee.
    /// </summary>
    public bool Succes => diagnostics.Count == 0;

    internal void AjouterDiagnostic(string diagnostic)
    {
        if (!string.IsNullOrWhiteSpace(diagnostic))
        {
            diagnostics.Add(diagnostic);
        }
    }

    /// <summary>
    /// Construit un message exploitable par l'orchestrateur ou les tests.
    /// </summary>
    public string ConstruireMessageDiagnostics()
    {
        return diagnostics.Count == 0
            ? string.Empty
            : string.Join(" | ", diagnostics);
    }
}

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

    /// <summary>
    /// Convertit et applique une seule fois les confirmations boursieres dues.
    /// </summary>
    /// <param name="orchestration">
    /// Service proprietaire du marqueur de consommation des confirmations.
    /// </param>
    /// <param name="mois">Index absolu du mois en cours.</param>
    /// <returns>
    /// Compteurs et diagnostics. Un evenement invalide est memorise comme rejete et
    /// ne laisse aucun de ses impacts dans la Bourse.
    /// </returns>
    /// <remarks>
    /// La politique est atomique par evenement : tous ses impacts de categorie
    /// Boursiers doivent etre convertibles avant la premiere mutation. Les
    /// autres categories ne sont jamais lues par ce service.
    /// </remarks>
    public ResultatConsommationImpactsBoursiers
        ConsommerConfirmationsBoursieres(
            ServiceOrchestrationEvenements orchestration,
            int mois)
    {
        if (orchestration == null)
        {
            throw new ArgumentNullException(nameof(orchestration));
        }

        ResultatConsommationImpactsBoursiers resultat =
            new ResultatConsommationImpactsBoursiers();
        if (gameData.evenements == null)
        {
            gameData.evenements = new DonneesEvenements();
        }
        gameData.evenements.InitialiserSiNecessaire();

        if (gameData.joueur == null)
        {
            gameData.joueur = new DonneesJoueur();
        }
        gameData.joueur.InitialiserSiNecessaire();
        ServiceBourse bourse = new ServiceBourse(gameData.joueur.bourse);

        List<EvenementConfirmePartie> confirmations =
            gameData.evenements.ObtenirConfirmationsBoursieresAConsommer();
        foreach (EvenementConfirmePartie evenement in confirmations)
        {
            if (evenement == null || evenement.moisConfirmation > mois)
            {
                continue;
            }

            if (evenement.impacts == null ||
                evenement.impacts.Count == 0)
            {
                string erreurSansImpact =
                    "Evenement boursier sans impact : " +
                    evenement.rumeurId + ".";
                resultat.AjouterDiagnostic(erreurSansImpact);
                if (!orchestration.MarquerConfirmationRejetee(
                        evenement.rumeurId,
                        erreurSansImpact))
                {
                    resultat.AjouterDiagnostic(
                        "Impossible de memoriser le rejet de l'evenement " +
                        evenement.rumeurId + ".");
                }
                continue;
            }

            if (!EssayerConvertirImpacts(
                    evenement,
                    out List<ImpactEvenementMarche> impacts,
                    out string erreur))
            {
                resultat.AjouterDiagnostic(erreur);
                if (!orchestration.MarquerConfirmationRejetee(
                        evenement.rumeurId,
                        erreur))
                {
                    resultat.AjouterDiagnostic(
                        "Impossible de memoriser le rejet de l'evenement " +
                        evenement.rumeurId + ".");
                }
                continue;
            }

            foreach (ImpactEvenementMarche impact in impacts)
            {
                bourse.AppliquerImpactEvenement(impact);
            }

            if (!orchestration.MarquerConfirmationConsommee(
                    evenement.rumeurId))
            {
                MarcheBoursier.RetirerImpactsEvenement(
                    gameData.joueur.bourse,
                    evenement.rumeurId);
                resultat.AjouterDiagnostic(
                    "Impossible de marquer l'evenement " +
                    evenement.rumeurId + " comme consomme.");
                continue;
            }

            resultat.EvenementsConsommes++;
            resultat.ImpactsAppliques += impacts.Count;
        }

        return resultat;
    }

    private static bool EssayerConvertirImpacts(
        EvenementConfirmePartie evenement,
        out List<ImpactEvenementMarche> impacts,
        out string erreur)
    {
        impacts = new List<ImpactEvenementMarche>();
        erreur = string.Empty;
        if (string.IsNullOrWhiteSpace(evenement.rumeurId))
        {
            erreur = "Confirmation sans identifiant de rumeur.";
            return false;
        }

        HashSet<string> actifs = new HashSet<string>(StringComparer.Ordinal);
        foreach (ImpactDefinitionEvenement definition in evenement.impacts)
        {
            if (definition == null ||
                !CatalogueEvenements.EssayerObtenirActifBourse(
                    definition.actif,
                    out string actifId))
            {
                erreur = "Cible boursiere inconnue pour l'evenement " +
                    evenement.rumeurId + " : " +
                    (definition?.actif ?? "<vide>") + ".";
                return false;
            }

            float coefficientPrix = 1f + definition.variation;
            if (float.IsNaN(definition.variation) ||
                float.IsInfinity(definition.variation) ||
                float.IsNaN(coefficientPrix) ||
                float.IsInfinity(coefficientPrix) ||
                coefficientPrix <= 0f)
            {
                erreur = "Variation invalide pour l'evenement " +
                    evenement.rumeurId + " et l'actif " + actifId + ".";
                return false;
            }

            if (!actifs.Add(actifId))
            {
                erreur = "Plusieurs impacts du meme evenement ciblent " +
                    actifId + ".";
                return false;
            }

            impacts.Add(new ImpactEvenementMarche
            {
                evenementId = evenement.rumeurId,
                actifId = actifId,
                moisDebut = evenement.moisConfirmation,
                dureeMois = 1,
                coefficientPrix = coefficientPrix,
                tendanceMensuellePourcent = 0f,
                coefficientVolatilite = 1f
            });
        }

        return impacts.Count > 0;
    }
}
