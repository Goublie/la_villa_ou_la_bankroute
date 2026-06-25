using System;
using System.Collections.Generic;

/// <summary>
/// Resultat non bloquant d'une etape du cycle mensuel What If.
/// </summary>
[Serializable]
public sealed class ResultatCycleMensuelWhatIf
{
    public bool succes;
    public int indexMois;
    public int patrimoineReelCentimes;
    public int nombrePrixObserves;
    public ResultatFluxMensuelsWhatIf flux;
    public ResultatOuvertureMoisWhatIf ouverture;
    public ResultatAchatImmobilierWhatIf achatImmobilier;
    public PointHistoriqueWhatIf cloture;
    public List<string> diagnostics = new List<string>();
}

/// <summary>
/// Adapte le moteur What If a GameData sans modifier la trajectoire reelle.
/// </summary>
/// <remarks>
/// Les prix sont construits uniquement pour le mois courant et avec les
/// impacts deja connus du portefeuille reel. Une erreur What If ne doit jamais
/// bloquer le passage mensuel principal.
/// </remarks>
public static class ServiceCycleMensuelWhatIf
{
    public static ResultatCycleMensuelWhatIf OuvrirMois(
        GameData gameData)
    {
        ResultatCycleMensuelWhatIf resultat =
            CreerResultat(gameData);

        if (!AssurerRacine(gameData, resultat))
        {
            return resultat;
        }

        try
        {
            Dictionary<string, int> prix =
                ConstruirePrixObserves(
                    gameData.joueur.bourse,
                    gameData.nombreMoisPasses);

            resultat.nombrePrixObserves = prix.Count;
            resultat.patrimoineReelCentimes =
                Math.Max(
                    0,
                    ServicePatrimoine.Calculer(
                        gameData.joueur).centimes);

            ServicePatrimoineAlternatifWhatIf.InitialiserDepuisJoueur(
                gameData.whatIf,
                gameData.joueur,
                resultat.patrimoineReelCentimes,
                gameData.nombreMoisPasses);

            ResultatSynchronisationPretsWhatIf synchronisationOuverture =
                ServicePatrimoineAlternatifWhatIf
                    .SynchroniserNouveauxPretsDepuisJoueur(
                        gameData.whatIf,
                        gameData.joueur);
            resultat.diagnostics.AddRange(
                synchronisationOuverture.diagnostics);

            resultat.achatImmobilier =
                ServiceImmobilierAlternatifWhatIf
                    .EvaluerEtAcheterComptant(
                        gameData.whatIf,
                        gameData.joueur.immobilier?.annoncesActuelles,
                        gameData.nombreMoisPasses);

            if (!string.IsNullOrWhiteSpace(
                    resultat.achatImmobilier?.diagnostic))
            {
                resultat.diagnostics.Add(
                    resultat.achatImmobilier.diagnostic);
            }

            resultat.ouverture =
                ServiceOrchestrationMensuelleWhatIf.OuvrirMois(
                    gameData.whatIf,
                    resultat.patrimoineReelCentimes,
                    CatalogueActifs.ObtenirActifs(),
                    gameData.joueur.bourse,
                    prix,
                    gameData.nombreMoisPasses);

            resultat.succes =
                resultat.ouverture != null &&
                resultat.ouverture.succes;

            if (!resultat.succes)
            {
                resultat.diagnostics.Add(
                    resultat.ouverture?.diagnostic ??
                    "Ouverture What If impossible.");
            }
        }
        catch (Exception exception)
        {
            resultat.succes = false;
            resultat.diagnostics.Add(
                "Ouverture What If ignoree : " +
                exception.Message);
        }

        return resultat;
    }

    public static ResultatCycleMensuelWhatIf CloturerMois(
        GameData gameData,
        int indexMois,
        Mois moisCalendrier)
    {
        ResultatCycleMensuelWhatIf resultat =
            CreerResultat(gameData);
        resultat.indexMois = Math.Max(0, indexMois);

        if (!AssurerRacine(gameData, resultat))
        {
            return resultat;
        }

        try
        {
            Dictionary<string, int> prix =
                ConstruirePrixObserves(
                    gameData.joueur.bourse,
                    resultat.indexMois);
            resultat.nombrePrixObserves = prix.Count;

            resultat.patrimoineReelCentimes =
                Math.Max(
                    0,
                    ServicePatrimoine.Calculer(
                        gameData.joueur).centimes);

            bool dejaInitialisee = gameData.whatIf.initialisee;
            bool actifsPassifsDejaInitialises =
                gameData.whatIf.actifsPassifsImmobiliersInitialises;

            ServicePatrimoineAlternatifWhatIf.InitialiserDepuisJoueur(
                gameData.whatIf,
                gameData.joueur,
                resultat.patrimoineReelCentimes,
                resultat.indexMois);

            ResultatSynchronisationPretsWhatIf synchronisationCloture =
                ServicePatrimoineAlternatifWhatIf
                    .SynchroniserNouveauxPretsDepuisJoueur(
                        gameData.whatIf,
                        gameData.joueur);
            resultat.diagnostics.AddRange(
                synchronisationCloture.diagnostics);

            if (dejaInitialisee && actifsPassifsDejaInitialises)
            {
                resultat.flux =
                    ServicePatrimoineAlternatifWhatIf.AppliquerFluxMensuels(
                        gameData.whatIf,
                        gameData.joueur,
                        prix,
                        resultat.indexMois,
                        synchronisationCloture.pretsCopies);
            }
            else
            {
                resultat.flux =
                    new ResultatFluxMensuelsWhatIf
                    {
                        succes = true,
                        liquiditesAvantCentimes =
                            gameData.whatIf.liquiditesCentimes,
                        liquiditesApresCentimes =
                            gameData.whatIf.liquiditesCentimes
                    };
                resultat.flux.diagnostics.Add(
                    "Initialisation tardive : les flux du mois ne sont pas " +
                    "rejoues car le patrimoine reel les contient deja.");
            }

            resultat.cloture =
                ServiceOrchestrationMensuelleWhatIf.CloturerMois(
                    gameData.whatIf,
                    resultat.indexMois,
                    moisCalendrier,
                    prix,
                    resultat.patrimoineReelCentimes);

            resultat.cloture =
                ServicePatrimoineAlternatifWhatIf.CompleterPointHistorique(
                    gameData.whatIf,
                    resultat.cloture,
                    prix,
                    resultat.patrimoineReelCentimes);

            resultat.succes = resultat.cloture != null;

            if (resultat.flux != null &&
                !resultat.flux.succes)
            {
                resultat.diagnostics.AddRange(
                    resultat.flux.diagnostics);
            }

            if (!resultat.succes)
            {
                resultat.diagnostics.Add(
                    "Cloture What If impossible.");
            }
        }
        catch (Exception exception)
        {
            resultat.succes = false;
            resultat.diagnostics.Add(
                "Cloture What If ignoree : " +
                exception.Message);
        }

        return resultat;
    }

    /// <summary>
    /// Construit une photographie de prix en centimes pour le mois observe.
    /// </summary>
    public static Dictionary<string, int> ConstruirePrixObserves(
        DonneesBourse marcheConnu,
        int indexMois)
    {
        Dictionary<string, int> resultat =
            new Dictionary<string, int>(StringComparer.Ordinal);

        IReadOnlyList<DefinitionActifFinancier> actifs =
            CatalogueActifs.ObtenirActifs();

        if (actifs == null)
        {
            return resultat;
        }

        foreach (DefinitionActifFinancier actif in actifs)
        {
            if (actif == null ||
                string.IsNullOrWhiteSpace(actif.Id))
            {
                continue;
            }

            float prixEuros = MarcheBoursier.ObtenirPrix(
                actif.Id,
                Math.Max(0, indexMois),
                marcheConnu);

            int prixCentimes = LimiterEnEntier(
                Math.Round(Math.Max(0f, prixEuros) * 100d));

            if (prixCentimes > 0)
            {
                resultat[actif.Id] = prixCentimes;
            }
        }

        return resultat;
    }

    private static ResultatCycleMensuelWhatIf CreerResultat(
        GameData gameData)
    {
        return new ResultatCycleMensuelWhatIf
        {
            indexMois =
                gameData != null
                    ? Math.Max(0, gameData.nombreMoisPasses)
                    : 0
        };
    }

    private static bool AssurerRacine(
        GameData gameData,
        ResultatCycleMensuelWhatIf resultat)
    {
        if (gameData == null)
        {
            resultat.diagnostics.Add(
                "GameData absent.");
            return false;
        }

        if (gameData.joueur == null)
        {
            gameData.joueur = new DonneesJoueur();
        }
        gameData.joueur.InitialiserSiNecessaire();

        if (gameData.whatIf == null)
        {
            gameData.whatIf = new DonneesWhatIf();
        }
        gameData.whatIf.InitialiserSiNecessaire();

        return true;
    }

    private static int LimiterEnEntier(double valeur)
    {
        if (valeur >= int.MaxValue)
        {
            return int.MaxValue;
        }

        return valeur <= 0d
            ? 0
            : (int)valeur;
    }
}
