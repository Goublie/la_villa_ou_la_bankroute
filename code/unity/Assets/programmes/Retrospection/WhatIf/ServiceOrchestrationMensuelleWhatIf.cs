using System;
using System.Collections.Generic;

[Serializable]
public sealed class ResultatOuvertureMoisWhatIf
{
    public bool succes;
    public int indexMois;
    public int capitalAvantDecisionCentimes;
    public List<PrevisionActifWhatIf> previsions =
        new List<PrevisionActifWhatIf>();
    public ResultatRechercheFaisceauWhatIf recherche;
    public ResultatReallocationWhatIf reallocation;
    public string diagnostic;
}

/// <summary>
/// Enchaine prevision, recherche et reallocation pour le scenario alternatif.
/// Le joueur reel n'est jamais modifie.
/// </summary>
public static class ServiceOrchestrationMensuelleWhatIf
{
    public static ResultatOuvertureMoisWhatIf OuvrirMois(
        DonneesWhatIf donnees,
        int capitalInitialCentimes,
        IReadOnlyList<DefinitionActifFinancier> actifsConnus,
        DonneesBourse marcheConnu,
        IReadOnlyDictionary<string, int> prixObservesCentimes,
        int indexMois)
    {
        List<PrevisionActifWhatIf> previsions =
            ConstruirePrevisions(actifsConnus, marcheConnu, indexMois);

        return OuvrirDepuisPrevisions(
            donnees,
            capitalInitialCentimes,
            previsions,
            marcheConnu != null ? marcheConnu.impactsMarche : null,
            prixObservesCentimes,
            indexMois);
    }

    public static ResultatOuvertureMoisWhatIf OuvrirDepuisPrevisions(
        DonneesWhatIf donnees,
        int capitalInitialCentimes,
        IReadOnlyList<PrevisionActifWhatIf> previsions,
        IReadOnlyList<ImpactEvenementMarche> impactsConnus,
        IReadOnlyDictionary<string, int> prixObservesCentimes,
        int indexMois)
    {
        ResultatOuvertureMoisWhatIf resultat =
            new ResultatOuvertureMoisWhatIf
            {
                indexMois = Math.Max(0, indexMois)
            };

        if (donnees == null)
        {
            resultat.diagnostic = "Donnees What If absentes.";
            return resultat;
        }

        donnees.InitialiserSiNecessaire();
        ServicePortefeuilleAlternatifWhatIf.Initialiser(
            donnees,
            capitalInitialCentimes,
            resultat.indexMois);
        SynchroniserImpactsConnus(donnees.portefeuille, impactsConnus);

        resultat.previsions = CopierEtTrierPrevisions(previsions);
        if (resultat.previsions.Count == 0)
        {
            resultat.diagnostic = "Aucune prevision exploitable.";
            return resultat;
        }

        Dictionary<string, int> allocationCourante =
            ServicePortefeuilleAlternatifWhatIf
                .ConstruireAllocationCourante(
                    donnees,
                    prixObservesCentimes);

        resultat.capitalAvantDecisionCentimes =
            CalculerPatrimoineAlternatif(
                donnees,
                prixObservesCentimes);

        if (resultat.capitalAvantDecisionCentimes <= 0)
        {
            resultat.diagnostic =
                "Le patrimoine alternatif disponible est nul.";
            return resultat;
        }

        resultat.recherche =
            ServiceOptimisationMultiHorizonWhatIf.Rechercher(
                resultat.capitalAvantDecisionCentimes,
                allocationCourante,
                resultat.previsions,
                donnees.configuration,
                resultat.indexMois);

        if (resultat.recherche == null ||
            resultat.recherche.decisionRetenue == null)
        {
            resultat.diagnostic =
                resultat.recherche != null
                    ? resultat.recherche.diagnostic
                    : "La recherche What If n'a produit aucun resultat.";
            return resultat;
        }

        EnrichirDecisionsAvecImpactsConnus(
            resultat.recherche,
            impactsConnus,
            resultat.indexMois);

        resultat.reallocation =
            ServicePortefeuilleAlternatifWhatIf.Reallouer(
                donnees,
                resultat.recherche.decisionRetenue,
                prixObservesCentimes,
                resultat.indexMois);

        resultat.succes =
            resultat.reallocation != null &&
            resultat.reallocation.succes;
        resultat.diagnostic = resultat.succes
            ? "Mois " + resultat.indexMois +
              " prepare : " + resultat.previsions.Count +
              " previsions, " + resultat.recherche.noeudsEvalues +
              " noeuds, " + resultat.reallocation.ordresExecutes +
              " ordres alternatifs."
            : ConstruireDiagnosticEchec(resultat);
        return resultat;
    }

    public static PointHistoriqueWhatIf CloturerMois(
        DonneesWhatIf donnees,
        int indexMois,
        Mois moisCalendrier,
        IReadOnlyDictionary<string, int> prixObservesCentimes,
        int patrimoineReelCentimes)
    {
        return ServicePortefeuilleAlternatifWhatIf.CloturerMois(
            donnees,
            indexMois,
            moisCalendrier,
            prixObservesCentimes,
            patrimoineReelCentimes);
    }

    public static List<PrevisionActifWhatIf> ConstruirePrevisions(
        IReadOnlyList<DefinitionActifFinancier> actifsConnus,
        DonneesBourse marcheConnu,
        int indexMois)
    {
        SortedDictionary<string, DefinitionActifFinancier> actifs =
            new SortedDictionary<string, DefinitionActifFinancier>(
                StringComparer.Ordinal);

        if (actifsConnus != null)
        {
            foreach (DefinitionActifFinancier actif in actifsConnus)
            {
                if (actif != null &&
                    !string.IsNullOrWhiteSpace(actif.Id))
                {
                    actifs[actif.Id] = actif;
                }
            }
        }

        List<PrevisionActifWhatIf> resultat =
            new List<PrevisionActifWhatIf>();
        foreach (DefinitionActifFinancier actif in actifs.Values)
        {
            resultat.Add(
                ServicePrevisionWhatIf.Estimer(
                    actif,
                    marcheConnu,
                    Math.Max(0, indexMois)));
        }

        return resultat;
    }

    private static List<PrevisionActifWhatIf> CopierEtTrierPrevisions(
        IReadOnlyList<PrevisionActifWhatIf> previsions)
    {
        SortedDictionary<string, PrevisionActifWhatIf> uniques =
            new SortedDictionary<string, PrevisionActifWhatIf>(
                StringComparer.Ordinal);

        if (previsions != null)
        {
            foreach (PrevisionActifWhatIf source in previsions)
            {
                if (source == null ||
                    string.IsNullOrWhiteSpace(source.actifId))
                {
                    continue;
                }

                uniques[source.actifId] = CopierPrevision(source);
            }
        }

        return new List<PrevisionActifWhatIf>(uniques.Values);
    }

    private static PrevisionActifWhatIf CopierPrevision(
        PrevisionActifWhatIf source)
    {
        return new PrevisionActifWhatIf
        {
            actifId = source.actifId,
            moisObservation = source.moisObservation,
            nombreObservations = source.nombreObservations,
            nombreImpactsActifs = source.nombreImpactsActifs,
            rendementMoyenMensuelPourcent =
                source.rendementMoyenMensuelPourcent,
            tendanceRecenteMensuellePourcent =
                source.tendanceRecenteMensuellePourcent,
            effetEvenementsConnusPourcent =
                source.effetEvenementsConnusPourcent,
            rendementMensuelEstimePourcent =
                source.rendementMensuelEstimePourcent,
            volatiliteMensuellePourcent =
                source.volatiliteMensuellePourcent,
            risqueEstimePourcent = source.risqueEstimePourcent,
            drawdownHistoriquePourcent =
                source.drawdownHistoriquePourcent,
            confiance01 = source.confiance01,
            explication = source.explication
        };
    }

    private static void SynchroniserImpactsConnus(
        DonneesBourse portefeuilleAlternatif,
        IReadOnlyList<ImpactEvenementMarche> impactsConnus)
    {
        if (portefeuilleAlternatif == null)
        {
            return;
        }

        portefeuilleAlternatif.impactsMarche =
            new List<ImpactEvenementMarche>();
        if (impactsConnus == null)
        {
            return;
        }

        foreach (ImpactEvenementMarche impact in impactsConnus)
        {
            if (impact != null)
            {
                portefeuilleAlternatif.impactsMarche.Add(
                    impact.Copier());
            }
        }
    }

    private static void EnrichirDecisionsAvecImpactsConnus(
        ResultatRechercheFaisceauWhatIf recherche,
        IReadOnlyList<ImpactEvenementMarche> impactsConnus,
        int indexMois)
    {
        List<string> ids = ObtenirIdsImpactsActifs(
            impactsConnus,
            indexMois);

        string contexte = ConstruireContexteEvenements(ids);

        if (recherche.decisionRetenue != null)
        {
            recherche.decisionRetenue.evenementsConnusIds =
                new List<string>(ids);
            recherche.decisionRetenue.explication =
                AjouterContexte(
                    recherche.decisionRetenue.explication,
                    contexte);
        }

        if (recherche.chemin != null)
        {
            foreach (DecisionWhatIf decision in recherche.chemin)
            {
                if (decision != null)
                {
                    decision.evenementsConnusIds =
                        new List<string>(ids);
                    decision.explication =
                        AjouterContexte(
                            decision.explication,
                            contexte);
                }
            }
        }
    }

    private static List<string> ObtenirIdsImpactsActifs(
        IReadOnlyList<ImpactEvenementMarche> impacts,
        int indexMois)
    {
        HashSet<string> uniques =
            new HashSet<string>(StringComparer.Ordinal);

        if (impacts != null)
        {
            foreach (ImpactEvenementMarche impact in impacts)
            {
                if (impact != null &&
                    impact.EstActif(indexMois) &&
                    !string.IsNullOrWhiteSpace(impact.evenementId))
                {
                    uniques.Add(impact.evenementId);
                }
            }
        }

        List<string> resultat = new List<string>(uniques);
        resultat.Sort(StringComparer.Ordinal);
        return resultat;
    }

    private static string ConstruireContexteEvenements(
        IReadOnlyCollection<string> ids)
    {
        int nombre = ids != null ? ids.Count : 0;
        return nombre == 0
            ? "Aucun evenement confirme actif n'a influence cette decision."
            : nombre +
              " evenement(s) confirme(s) actif(s) ont influence cette decision.";
    }

    private static string AjouterContexte(
        string explication,
        string contexte)
    {
        string baseTexte = explication ?? string.Empty;
        string ajout = contexte ?? string.Empty;

        if (string.IsNullOrWhiteSpace(ajout) ||
            baseTexte.Contains(ajout))
        {
            return baseTexte;
        }

        return string.IsNullOrWhiteSpace(baseTexte)
            ? ajout
            : baseTexte.TrimEnd() + " " + ajout;
    }

    private static int CalculerPatrimoineAlternatif(
        DonneesWhatIf donnees,
        IReadOnlyDictionary<string, int> prixObservesCentimes)
    {
        long total = Math.Max(0, donnees.liquiditesCentimes);

        if (donnees.portefeuille?.positions != null)
        {
            foreach (PositionBourse position in donnees.portefeuille.positions)
            {
                if (position == null ||
                    position.quantite <= 0f ||
                    string.IsNullOrWhiteSpace(position.actifId))
                {
                    continue;
                }

                int prix = ObtenirPrix(
                    prixObservesCentimes,
                    position.actifId);
                if (prix <= 0)
                {
                    continue;
                }

                total += (long)Math.Round(
                    position.quantite * prix);
                if (total >= int.MaxValue)
                {
                    return int.MaxValue;
                }
            }
        }

        return (int)Math.Max(0, total);
    }

    private static int ObtenirPrix(
        IReadOnlyDictionary<string, int> prix,
        string actifId)
    {
        return prix != null &&
            prix.TryGetValue(actifId, out int valeur)
            ? Math.Max(0, valeur)
            : 0;
    }

    private static string ConstruireDiagnosticEchec(
        ResultatOuvertureMoisWhatIf resultat)
    {
        if (resultat.reallocation?.diagnostics != null &&
            resultat.reallocation.diagnostics.Count > 0)
        {
            return string.Join(" ", resultat.reallocation.diagnostics);
        }

        return "La reallocation alternative a echoue.";
    }
}