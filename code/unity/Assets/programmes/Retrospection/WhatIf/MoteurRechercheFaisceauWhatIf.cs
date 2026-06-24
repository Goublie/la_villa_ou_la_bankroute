using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Resultat complet d'une recherche en faisceau What If.
/// </summary>
[Serializable]
public sealed class ResultatRechercheFaisceauWhatIf
{
    public DecisionWhatIf decisionRetenue;
    public List<DecisionWhatIf> chemin = new List<DecisionWhatIf>();
    public int patrimoineProjeteCentimes;
    public int noeudsEvalues;
    public int candidatsParEtape;
    public int largeurMaxObservee;
    public string diagnostic;
}

/// <summary>
/// Explore un arbre discret d'allocations et conserve uniquement les meilleurs
/// chemins a chaque profondeur.
/// </summary>
/// <remarks>
/// Le moteur ne consulte aucun prix de marche. Il travaille uniquement a partir
/// des previsions deja calculees avec les informations connues au mois observe.
/// Il ne modifie ni le portefeuille reel, ni les previsions recues.
/// </remarks>
public static class MoteurRechercheFaisceauWhatIf
{
    public const string LiquiditesId = "cash";

    private sealed class NoeudFaisceau
    {
        public int capitalCentimes;
        public Dictionary<string, int> allocation =
            new Dictionary<string, int>(StringComparer.Ordinal);
        public List<DecisionWhatIf> chemin = new List<DecisionWhatIf>();
        public double penalitesCumulees;
        public int coutsCumulesCentimes;
        public double score;
        public string cleStrategie;
    }

    /// <summary>
    /// Cherche la meilleure premiere decision sur un horizon glissant.
    /// </summary>
    public static ResultatRechercheFaisceauWhatIf Rechercher(
        int capitalInitialCentimes,
        IReadOnlyDictionary<string, int> allocationActuellePourcent,
        IReadOnlyList<PrevisionActifWhatIf> previsions,
        ConfigurationWhatIf configuration,
        int indexMois)
    {
        ResultatRechercheFaisceauWhatIf resultat =
            new ResultatRechercheFaisceauWhatIf();

        if (capitalInitialCentimes <= 0)
        {
            resultat.diagnostic = "Capital initial nul ou negatif.";
            return resultat;
        }

        ConfigurationWhatIf config =
            configuration != null
                ? configuration.Copier()
                : new ConfigurationWhatIf();
        config.Normaliser();

        SortedDictionary<string, PrevisionActifWhatIf> previsionsParActif =
            ConstruirePrevisionsParActif(previsions);
        if (previsionsParActif.Count == 0)
        {
            resultat.diagnostic = "Aucune prevision exploitable.";
            return resultat;
        }

        List<string> actifs =
            new List<string>(previsionsParActif.Keys);
        List<List<AllocationActifWhatIf>> candidats =
            GenererAllocationsCandidates(
                actifs,
                config.pasAllocationPourcent);
        resultat.candidatsParEtape = candidats.Count;

        Dictionary<string, int> allocationInitiale =
            ConstruireAllocationInitiale(
                allocationActuellePourcent,
                actifs);

        NoeudFaisceau racine = new NoeudFaisceau
        {
            capitalCentimes = capitalInitialCentimes,
            allocation = allocationInitiale,
            score = capitalInitialCentimes,
            cleStrategie = ConstruireCleAllocation(allocationInitiale)
        };

        List<NoeudFaisceau> faisceau =
            new List<NoeudFaisceau> { racine };
        resultat.largeurMaxObservee = faisceau.Count;

        for (int profondeur = 0;
            profondeur < config.horizonMois;
            profondeur++)
        {
            List<NoeudFaisceau> suivants = new List<NoeudFaisceau>();

            foreach (NoeudFaisceau noeud in faisceau)
            {
                foreach (List<AllocationActifWhatIf> candidat in candidats)
                {
                    resultat.noeudsEvalues++;
                    suivants.Add(
                        DevelopperNoeud(
                            noeud,
                            candidat,
                            previsionsParActif,
                            config,
                            indexMois + profondeur));
                }
            }

            suivants.Sort(ComparerNoeuds);
            int nombreConserve = Math.Min(
                config.largeurFaisceau,
                suivants.Count);
            faisceau = suivants.GetRange(0, nombreConserve);
            resultat.largeurMaxObservee = Math.Max(
                resultat.largeurMaxObservee,
                faisceau.Count);
        }

        if (faisceau.Count == 0)
        {
            resultat.diagnostic = "Aucun chemin genere.";
            return resultat;
        }

        NoeudFaisceau meilleur = faisceau[0];
        resultat.patrimoineProjeteCentimes = meilleur.capitalCentimes;
        foreach (DecisionWhatIf decision in meilleur.chemin)
        {
            resultat.chemin.Add(decision.Copier());
        }

        if (resultat.chemin.Count > 0)
        {
            resultat.decisionRetenue = resultat.chemin[0].Copier();
        }

        resultat.diagnostic =
            "Recherche terminee : " +
            resultat.noeudsEvalues +
            " noeuds evalues, " +
            resultat.candidatsParEtape +
            " allocations par etape.";

        return resultat;
    }

    /// <summary>
    /// Genere toutes les allocations discretes dont la somme vaut 100 %.
    /// Les liquidites sont traitees comme un actif defensif sans rendement.
    /// </summary>
    public static List<List<AllocationActifWhatIf>>
        GenererAllocationsCandidates(
            IReadOnlyList<string> actifs,
            int pasPourcent)
    {
        int pas = NormaliserPas(pasPourcent);
        List<string> dimensions = new List<string> { LiquiditesId };
        HashSet<string> dejaAjoutes =
            new HashSet<string>(StringComparer.Ordinal);

        if (actifs != null)
        {
            List<string> tries = new List<string>();
            foreach (string actifId in actifs)
            {
                if (string.IsNullOrWhiteSpace(actifId) ||
                    actifId == LiquiditesId ||
                    !dejaAjoutes.Add(actifId))
                {
                    continue;
                }

                tries.Add(actifId);
            }

            tries.Sort(StringComparer.Ordinal);
            dimensions.AddRange(tries);
        }

        int unites = 100 / pas;
        List<List<AllocationActifWhatIf>> resultat =
            new List<List<AllocationActifWhatIf>>();
        GenererRecursivement(
            dimensions,
            pas,
            0,
            unites,
            new int[dimensions.Count],
            resultat);

        resultat.Sort(
            (gauche, droite) =>
                string.CompareOrdinal(
                    ConstruireCleAllocation(gauche),
                    ConstruireCleAllocation(droite)));
        return resultat;
    }

    private static NoeudFaisceau DevelopperNoeud(
        NoeudFaisceau parent,
        List<AllocationActifWhatIf> candidat,
        IReadOnlyDictionary<string, PrevisionActifWhatIf> previsions,
        ConfigurationWhatIf configuration,
        int indexMois)
    {
        Dictionary<string, int> nouvelleAllocation =
            ConvertirAllocation(candidat);

        float rendement = CalculerRendementPondere(
            nouvelleAllocation,
            previsions);
        float risque = CalculerRisquePondere(
            nouvelleAllocation,
            previsions);
        float drawdown = CalculerDrawdownPondere(
            nouvelleAllocation,
            previsions);

        int coutReallocation = CalculerCoutReallocation(
            parent.allocation,
            nouvelleAllocation,
            configuration.coutTransactionCentimes);

        double facteur = Math.Max(0.01d, 1d + rendement / 100d);
        int capitalAvantCout = LimiterEntierPositif(
            Math.Round(parent.capitalCentimes * facteur));
        int capitalApresCout = Math.Max(
            0,
            capitalAvantCout - coutReallocation);

        double penaliteRisque =
            capitalApresCout *
            configuration.penaliteRisque *
            (risque / 100d);
        double penaliteDrawdown =
            capitalApresCout *
            configuration.penaliteDrawdown *
            (drawdown / 100d);
        double nouvellesPenalites =
            parent.penalitesCumulees +
            penaliteRisque +
            penaliteDrawdown;

        DecisionWhatIf decision = new DecisionWhatIf
        {
            indexMois = indexMois,
            strategieId = ConstruireCleAllocation(nouvelleAllocation),
            rendementAttenduPourcent = rendement,
            risqueEstime = risque,
            drawdownEstime = drawdown,
            score =
                capitalApresCout -
                nouvellesPenalites,
            explication = ConstruireExplication(
                rendement,
                risque,
                drawdown,
                coutReallocation)
        };

        foreach (KeyValuePair<string, int> allocation in nouvelleAllocation)
        {
            decision.allocations.Add(
                new AllocationActifWhatIf(
                    allocation.Key,
                    allocation.Value));
        }

        NoeudFaisceau enfant = new NoeudFaisceau
        {
            capitalCentimes = capitalApresCout,
            allocation = nouvelleAllocation,
            penalitesCumulees = nouvellesPenalites,
            coutsCumulesCentimes =
                parent.coutsCumulesCentimes +
                coutReallocation,
            cleStrategie = decision.strategieId
        };
        enfant.score =
            enfant.capitalCentimes -
            enfant.penalitesCumulees;

        foreach (DecisionWhatIf precedente in parent.chemin)
        {
            enfant.chemin.Add(precedente.Copier());
        }
        enfant.chemin.Add(decision);

        return enfant;
    }

    private static SortedDictionary<string, PrevisionActifWhatIf>
        ConstruirePrevisionsParActif(
            IReadOnlyList<PrevisionActifWhatIf> previsions)
    {
        SortedDictionary<string, PrevisionActifWhatIf> resultat =
            new SortedDictionary<string, PrevisionActifWhatIf>(
                StringComparer.Ordinal);

        if (previsions == null)
        {
            return resultat;
        }

        foreach (PrevisionActifWhatIf prevision in previsions)
        {
            if (prevision == null ||
                string.IsNullOrWhiteSpace(prevision.actifId) ||
                prevision.actifId == LiquiditesId)
            {
                continue;
            }

            resultat[prevision.actifId] = prevision;
        }

        return resultat;
    }

    private static Dictionary<string, int> ConstruireAllocationInitiale(
        IReadOnlyDictionary<string, int> allocationActuelle,
        IReadOnlyList<string> actifs)
    {
        Dictionary<string, int> resultat =
            new Dictionary<string, int>(StringComparer.Ordinal);
        int totalActifs = 0;

        if (actifs != null)
        {
            foreach (string actifId in actifs)
            {
                int pourcentage = 0;
                if (allocationActuelle != null &&
                    allocationActuelle.TryGetValue(
                        actifId,
                        out int valeur))
                {
                    pourcentage = Limiter(valeur, 0, 100);
                }

                resultat[actifId] = pourcentage;
                totalActifs += pourcentage;
            }
        }

        if (totalActifs > 100)
        {
            int totalNormalise = 0;
            List<string> ids = new List<string>(resultat.Keys);
            ids.Sort(StringComparer.Ordinal);
            for (int index = 0; index < ids.Count; index++)
            {
                int valeur = index == ids.Count - 1
                    ? 100 - totalNormalise
                    : (int)Math.Floor(
                        resultat[ids[index]] * 100d / totalActifs);
                resultat[ids[index]] = Math.Max(0, valeur);
                totalNormalise += resultat[ids[index]];
            }

            totalActifs = 100;
        }

        int cashExplicite = 0;
        if (allocationActuelle != null &&
            allocationActuelle.TryGetValue(
                LiquiditesId,
                out int valeurCash))
        {
            cashExplicite = Limiter(valeurCash, 0, 100);
        }

        resultat[LiquiditesId] = Math.Min(
            100 - totalActifs,
            cashExplicite > 0
                ? cashExplicite
                : 100 - totalActifs);

        int total = SommeAllocation(resultat);
        if (total < 100)
        {
            resultat[LiquiditesId] += 100 - total;
        }

        return TrierAllocation(resultat);
    }

    private static float CalculerRendementPondere(
        IReadOnlyDictionary<string, int> allocation,
        IReadOnlyDictionary<string, PrevisionActifWhatIf> previsions)
    {
        float total = 0f;
        foreach (KeyValuePair<string, int> ligne in allocation)
        {
            if (ligne.Key == LiquiditesId ||
                !previsions.TryGetValue(
                    ligne.Key,
                    out PrevisionActifWhatIf prevision))
            {
                continue;
            }

            float confiance = Limiter(prevision.confiance01, 0f, 1f);
            total +=
                (ligne.Value / 100f) *
                prevision.rendementMensuelEstimePourcent *
                confiance;
        }

        return total;
    }

    private static float CalculerRisquePondere(
        IReadOnlyDictionary<string, int> allocation,
        IReadOnlyDictionary<string, PrevisionActifWhatIf> previsions)
    {
        float total = 0f;
        foreach (KeyValuePair<string, int> ligne in allocation)
        {
            if (ligne.Key == LiquiditesId ||
                !previsions.TryGetValue(
                    ligne.Key,
                    out PrevisionActifWhatIf prevision))
            {
                continue;
            }

            total +=
                (ligne.Value / 100f) *
                Math.Max(0f, prevision.risqueEstimePourcent);
        }

        return total;
    }

    private static float CalculerDrawdownPondere(
        IReadOnlyDictionary<string, int> allocation,
        IReadOnlyDictionary<string, PrevisionActifWhatIf> previsions)
    {
        float total = 0f;
        foreach (KeyValuePair<string, int> ligne in allocation)
        {
            if (ligne.Key == LiquiditesId ||
                !previsions.TryGetValue(
                    ligne.Key,
                    out PrevisionActifWhatIf prevision))
            {
                continue;
            }

            total +=
                (ligne.Value / 100f) *
                Math.Max(0f, prevision.drawdownHistoriquePourcent);
        }

        return total;
    }

    private static int CalculerCoutReallocation(
        IReadOnlyDictionary<string, int> ancienneAllocation,
        IReadOnlyDictionary<string, int> nouvelleAllocation,
        int coutPleinTourCentimes)
    {
        if (coutPleinTourCentimes <= 0)
        {
            return 0;
        }

        HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
        if (ancienneAllocation != null)
        {
            foreach (string id in ancienneAllocation.Keys)
            {
                ids.Add(id);
            }
        }
        if (nouvelleAllocation != null)
        {
            foreach (string id in nouvelleAllocation.Keys)
            {
                ids.Add(id);
            }
        }

        int sommeEcarts = 0;
        foreach (string id in ids)
        {
            int ancien = ObtenirPourcentage(ancienneAllocation, id);
            int nouveau = ObtenirPourcentage(nouvelleAllocation, id);
            sommeEcarts += Math.Abs(nouveau - ancien);
        }

        float rotationPourcent = Math.Min(100f, sommeEcarts / 2f);
        return (int)Math.Round(
            coutPleinTourCentimes * rotationPourcent / 100f);
    }

    private static void GenererRecursivement(
        IReadOnlyList<string> dimensions,
        int pas,
        int index,
        int unitesRestantes,
        int[] unitesCourantes,
        List<List<AllocationActifWhatIf>> resultat)
    {
        if (index == dimensions.Count - 1)
        {
            unitesCourantes[index] = unitesRestantes;
            List<AllocationActifWhatIf> allocation =
                new List<AllocationActifWhatIf>();
            for (int dimension = 0;
                dimension < dimensions.Count;
                dimension++)
            {
                allocation.Add(
                    new AllocationActifWhatIf(
                        dimensions[dimension],
                        unitesCourantes[dimension] * pas));
            }

            resultat.Add(allocation);
            return;
        }

        for (int unites = 0;
            unites <= unitesRestantes;
            unites++)
        {
            unitesCourantes[index] = unites;
            GenererRecursivement(
                dimensions,
                pas,
                index + 1,
                unitesRestantes - unites,
                unitesCourantes,
                resultat);
        }
    }

    private static Dictionary<string, int> ConvertirAllocation(
        IReadOnlyList<AllocationActifWhatIf> allocation)
    {
        Dictionary<string, int> resultat =
            new Dictionary<string, int>(StringComparer.Ordinal);
        if (allocation != null)
        {
            foreach (AllocationActifWhatIf ligne in allocation)
            {
                if (ligne == null || string.IsNullOrWhiteSpace(ligne.actifId))
                {
                    continue;
                }

                resultat[ligne.actifId] =
                    Limiter(ligne.pourcentage, 0, 100);
            }
        }

        return TrierAllocation(resultat);
    }

    private static Dictionary<string, int> TrierAllocation(
        IReadOnlyDictionary<string, int> allocation)
    {
        List<string> ids = new List<string>(allocation.Keys);
        ids.Sort(StringComparer.Ordinal);
        Dictionary<string, int> resultat =
            new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (string id in ids)
        {
            resultat[id] = allocation[id];
        }

        return resultat;
    }

    private static int ComparerNoeuds(
        NoeudFaisceau gauche,
        NoeudFaisceau droite)
    {
        int comparaisonScore = droite.score.CompareTo(gauche.score);
        if (comparaisonScore != 0)
        {
            return comparaisonScore;
        }

        int comparaisonCapital =
            droite.capitalCentimes.CompareTo(gauche.capitalCentimes);
        if (comparaisonCapital != 0)
        {
            return comparaisonCapital;
        }

        return string.CompareOrdinal(
            gauche.cleStrategie,
            droite.cleStrategie);
    }

    private static string ConstruireCleAllocation(
        IReadOnlyDictionary<string, int> allocation)
    {
        List<string> ids = new List<string>(allocation.Keys);
        ids.Sort(StringComparer.Ordinal);
        List<string> morceaux = new List<string>();
        foreach (string id in ids)
        {
            morceaux.Add(id + "_" + allocation[id]);
        }

        return string.Join("__", morceaux);
    }

    private static string ConstruireCleAllocation(
        IReadOnlyList<AllocationActifWhatIf> allocation)
    {
        Dictionary<string, int> dictionnaire =
            ConvertirAllocation(allocation);
        return ConstruireCleAllocation(dictionnaire);
    }

    private static string ConstruireExplication(
        float rendement,
        float risque,
        float drawdown,
        int coutCentimes)
    {
        CultureInfo culture = CultureInfo.InvariantCulture;
        return
            "Rendement estime " +
            rendement.ToString("0.00", culture) +
            " %, risque " +
            risque.ToString("0.00", culture) +
            " %, drawdown " +
            drawdown.ToString("0.00", culture) +
            " %, cout de reallocation " +
            coutCentimes +
            " centimes.";
    }

    private static int ObtenirPourcentage(
        IReadOnlyDictionary<string, int> allocation,
        string id)
    {
        if (allocation != null &&
            allocation.TryGetValue(id, out int valeur))
        {
            return valeur;
        }

        return 0;
    }

    private static int SommeAllocation(
        IReadOnlyDictionary<string, int> allocation)
    {
        int total = 0;
        foreach (int valeur in allocation.Values)
        {
            total += valeur;
        }

        return total;
    }

    private static int NormaliserPas(int pas)
    {
        int valeur = Limiter(pas, 1, 100);
        if (100 % valeur == 0)
        {
            return valeur;
        }

        int meilleur = 1;
        int meilleureDistance = int.MaxValue;
        for (int candidat = 1; candidat <= 100; candidat++)
        {
            if (100 % candidat != 0)
            {
                continue;
            }

            int distance = Math.Abs(candidat - valeur);
            if (distance < meilleureDistance)
            {
                meilleureDistance = distance;
                meilleur = candidat;
            }
        }

        return meilleur;
    }

    private static int LimiterEntierPositif(double valeur)
    {
        if (valeur >= int.MaxValue)
        {
            return int.MaxValue;
        }

        return valeur <= 0d ? 0 : (int)valeur;
    }

    private static int Limiter(int valeur, int minimum, int maximum)
    {
        return Math.Min(maximum, Math.Max(minimum, valeur));
    }

    private static float Limiter(float valeur, float minimum, float maximum)
    {
        return Math.Min(maximum, Math.Max(minimum, valeur));
    }
}