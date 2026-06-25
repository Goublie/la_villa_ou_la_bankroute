using System;
using System.Collections.Generic;

/// <summary>
/// Bilan de la consommation des confirmations de categorie Immobiliers.
/// </summary>
public sealed class ResultatConsommationImpactsImmobiliers
{
    private readonly List<string> diagnostics = new List<string>();

    public int EvenementsConsommes { get; internal set; }
    public int ImpactsAppliques { get; internal set; }
    public IReadOnlyList<string> Diagnostics => diagnostics;
    public bool Succes => diagnostics.Count == 0;

    internal void AjouterDiagnostic(string diagnostic)
    {
        if (!string.IsNullOrWhiteSpace(diagnostic))
        {
            diagnostics.Add(diagnostic);
        }
    }

    public string ConstruireMessageDiagnostics()
    {
        return diagnostics.Count == 0
            ? string.Empty
            : string.Join(" | ", diagnostics);
    }
}

/// <summary>
/// Traduit les confirmations immobilières en impacts persistants.
/// Le service ne touche jamais à la Bourse ni à l'interface.
/// </summary>
public sealed class ServiceEvenementsImmobiliers
{
    private readonly GameData gameData;

    public ServiceEvenementsImmobiliers(GameData gameData)
    {
        this.gameData = gameData ??
            throw new ArgumentNullException(nameof(gameData));
    }

    public ResultatConsommationImpactsImmobiliers
        ConsommerConfirmationsImmobilieres(
            ServiceOrchestrationEvenements orchestration,
            int mois)
    {
        if (orchestration == null)
        {
            throw new ArgumentNullException(nameof(orchestration));
        }

        ResultatConsommationImpactsImmobiliers resultat =
            new ResultatConsommationImpactsImmobiliers();

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
        gameData.joueur.immobilier.InitialiserSiNecessaire();

        List<EvenementConfirmePartie> confirmations =
            gameData.evenements
                .ObtenirConfirmationsImmobilieresAConsommer();

        foreach (EvenementConfirmePartie evenement in confirmations)
        {
            if (evenement == null || evenement.moisConfirmation > mois)
            {
                continue;
            }

            if (evenement.impacts == null ||
                evenement.impacts.Count == 0)
            {
                Rejeter(
                    orchestration,
                    resultat,
                    evenement,
                    "Evenement immobilier sans impact.");
                continue;
            }

            List<ImpactEvenementImmobilier> impacts =
                new List<ImpactEvenementImmobilier>();
            bool valide = true;
            string erreur = string.Empty;

            foreach (ImpactDefinitionEvenement definition in evenement.impacts)
            {
                if (!ServiceImpactsImmobiliers.EssayerConvertir(
                        definition,
                        evenement.rumeurId,
                        evenement.moisConfirmation,
                        out ImpactEvenementImmobilier impact,
                        out erreur))
                {
                    valide = false;
                    break;
                }

                impacts.Add(impact);
            }

            if (!valide || impacts.Count == 0)
            {
                Rejeter(
                    orchestration,
                    resultat,
                    evenement,
                    erreur);
                continue;
            }

            gameData.joueur.immobilier.impactsActifs.RemoveAll(
                impact =>
                    impact != null &&
                    impact.evenementId == evenement.rumeurId);
            gameData.joueur.immobilier.impactsActifs.AddRange(impacts);

            if (!orchestration.MarquerConfirmationConsommee(
                    evenement.rumeurId))
            {
                gameData.joueur.immobilier.impactsActifs.RemoveAll(
                    impact =>
                        impact != null &&
                        impact.evenementId == evenement.rumeurId);
                resultat.AjouterDiagnostic(
                    "Impossible de marquer l'evenement immobilier " +
                    evenement.rumeurId +
                    " comme consomme.");
                continue;
            }

            resultat.EvenementsConsommes++;
            resultat.ImpactsAppliques += impacts.Count;
        }

        ServiceImpactsImmobiliers.RetirerImpactsTermines(
            gameData.joueur.immobilier,
            mois);

        return resultat;
    }

    private static void Rejeter(
        ServiceOrchestrationEvenements orchestration,
        ResultatConsommationImpactsImmobiliers resultat,
        EvenementConfirmePartie evenement,
        string diagnostic)
    {
        string message =
            string.IsNullOrWhiteSpace(diagnostic)
                ? "Impact immobilier invalide."
                : diagnostic;

        resultat.AjouterDiagnostic(message);
        if (!orchestration.MarquerConfirmationRejetee(
                evenement.rumeurId,
                message))
        {
            resultat.AjouterDiagnostic(
                "Impossible de memoriser le rejet de l'evenement " +
                evenement.rumeurId + ".");
        }
    }
}
