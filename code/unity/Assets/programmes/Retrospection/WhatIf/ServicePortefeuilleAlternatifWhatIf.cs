using System;
using System.Collections.Generic;

/// <summary>
/// Résultat détaillé d'une réallocation du portefeuille alternatif.
/// </summary>
[Serializable]
public sealed class ResultatReallocationWhatIf
{
    public bool succes;
    public int patrimoineAvantCentimes;
    public int patrimoineApresCentimes;
    public int coutTransactionCentimes;
    public int liquiditesCentimes;
    public int valeurBourseCentimes;
    public int ordresExecutes;
    public List<string> diagnostics = new List<string>();
}

/// <summary>
/// Gère uniquement le portefeuille alternatif du moteur What If.
/// </summary>
/// <remarks>
/// Ce service ne connaît ni GameData, ni compte bancaire réel, ni interface.
/// Il transforme exclusivement DonneesWhatIf à partir de prix déjà observés.
/// </remarks>
public static class ServicePortefeuilleAlternatifWhatIf
{
    /// <summary>
    /// Initialise une seule fois le capital alternatif.
    /// </summary>
    public static void Initialiser(
        DonneesWhatIf donnees,
        int capitalInitialCentimes,
        int indexMois)
    {
        if (donnees == null)
        {
            throw new ArgumentNullException(nameof(donnees));
        }

        donnees.InitialiserSiNecessaire();
        if (donnees.initialisee)
        {
            return;
        }

        int capital = Math.Max(0, capitalInitialCentimes);
        donnees.initialisee = true;
        donnees.moisInitialisation = Math.Max(0, indexMois);
        donnees.dernierMoisTraite = Math.Max(-1, indexMois - 1);
        donnees.capitalInitialCentimes = capital;
        donnees.liquiditesCentimes = capital;
        donnees.portefeuille.positions = new List<PositionBourse>();
        donnees.portefeuille.DefinirValeurMarche(0, Math.Max(0, indexMois));
    }

    /// <summary>
    /// Réalloue le patrimoine alternatif au prix déjà connu du mois courant.
    /// </summary>
    public static ResultatReallocationWhatIf Reallouer(
        DonneesWhatIf donnees,
        DecisionWhatIf decision,
        IReadOnlyDictionary<string, int> prixCentimesParActif,
        int indexMois)
    {
        ResultatReallocationWhatIf resultat =
            new ResultatReallocationWhatIf();

        if (donnees == null)
        {
            resultat.diagnostics.Add("Données What If absentes.");
            return resultat;
        }

        donnees.InitialiserSiNecessaire();
        if (!donnees.initialisee)
        {
            resultat.diagnostics.Add(
                "Le portefeuille alternatif n'est pas initialisé.");
            return resultat;
        }

        if (decision == null)
        {
            resultat.diagnostics.Add("Décision What If absente.");
            return resultat;
        }

        Dictionary<string, int> valeursActuelles =
            CalculerValeursPositions(
                donnees.portefeuille,
                prixCentimesParActif);
        int valeurBourseAvant = SommeValeurs(valeursActuelles);
        int patrimoineAvant = AdditionSaturee(
            donnees.liquiditesCentimes,
            valeurBourseAvant);
        resultat.patrimoineAvantCentimes = patrimoineAvant;

        if (patrimoineAvant <= 0)
        {
            donnees.liquiditesCentimes = 0;
            donnees.portefeuille.positions.Clear();
            donnees.portefeuille.DefinirValeurMarche(
                0,
                Math.Max(0, indexMois));
            resultat.succes = true;
            resultat.diagnostics.Add("Patrimoine alternatif nul.");
            return resultat;
        }

        Dictionary<string, int> ciblePourcent =
            NormaliserAllocation(decision.allocations);

        int cout = CalculerCoutReallocation(
            donnees.liquiditesCentimes,
            valeursActuelles,
            ciblePourcent,
            patrimoineAvant,
            donnees.configuration.coutTransactionCentimes);
        int capitalDisponible = Math.Max(0, patrimoineAvant - cout);

        List<PositionBourse> nouvellesPositions =
            new List<PositionBourse>();
        int totalInvesti = 0;

        List<string> actifs = new List<string>(ciblePourcent.Keys);
        actifs.Sort(StringComparer.Ordinal);

        foreach (string actifId in actifs)
        {
            if (actifId == MoteurRechercheFaisceauWhatIf.LiquiditesId)
            {
                continue;
            }

            int pourcentage = ciblePourcent[actifId];
            if (pourcentage <= 0)
            {
                continue;
            }

            int montantCible = LimiterEnEntier(
                Math.Floor(
                    capitalDisponible *
                    (pourcentage / 100d)));

            if (montantCible < ServiceBourse.MontantMinimumOrdreCentimes)
            {
                resultat.diagnostics.Add(
                    actifId +
                    " : montant inférieur au minimum, conservé en liquidités.");
                continue;
            }

            int prixCentimes = ObtenirPrixCentimes(
                prixCentimesParActif,
                actifId);
            if (prixCentimes <= 0)
            {
                resultat.diagnostics.Add(
                    actifId +
                    " : prix indisponible, conservé en liquidités.");
                continue;
            }

            int montantEffectif = Math.Min(
                montantCible,
                capitalDisponible - totalInvesti);
            if (montantEffectif < ServiceBourse.MontantMinimumOrdreCentimes)
            {
                continue;
            }

            float quantite =
                montantEffectif / (float)prixCentimes;
            PositionBourse position = new PositionBourse(actifId);
            position.AjouterAchat(quantite, montantEffectif);
            nouvellesPositions.Add(position);
            totalInvesti = AdditionSaturee(
                totalInvesti,
                montantEffectif);
            resultat.ordresExecutes++;
        }

        donnees.portefeuille.positions = nouvellesPositions;
        donnees.liquiditesCentimes = Math.Max(
            0,
            capitalDisponible - totalInvesti);
        donnees.portefeuille.DefinirValeurMarche(
            totalInvesti,
            Math.Max(0, indexMois));

        EnregistrerDecision(donnees, decision, indexMois);

        resultat.succes = true;
        resultat.coutTransactionCentimes = cout;
        resultat.liquiditesCentimes = donnees.liquiditesCentimes;
        resultat.valeurBourseCentimes = totalInvesti;
        resultat.patrimoineApresCentimes = AdditionSaturee(
            resultat.liquiditesCentimes,
            resultat.valeurBourseCentimes);
        return resultat;
    }

    /// <summary>
    /// Valorise le résultat réellement observé à la clôture et l'enregistre.
    /// </summary>
    public static PointHistoriqueWhatIf CloturerMois(
        DonneesWhatIf donnees,
        int indexMois,
        Mois moisCalendrier,
        IReadOnlyDictionary<string, int> prixCentimesParActif,
        int patrimoineReelCentimes)
    {
        if (donnees == null)
        {
            throw new ArgumentNullException(nameof(donnees));
        }

        donnees.InitialiserSiNecessaire();
        Dictionary<string, int> valeurs =
            CalculerValeursPositions(
                donnees.portefeuille,
                prixCentimesParActif);
        int valeurBourse = SommeValeurs(valeurs);
        int patrimoineAlternatif = AdditionSaturee(
            donnees.liquiditesCentimes,
            valeurBourse);

        donnees.portefeuille.DefinirValeurMarche(
            valeurBourse,
            Math.Max(0, indexMois));
        donnees.portefeuille.dernierMoisObserve =
            Math.Max(0, indexMois);
        donnees.dernierMoisTraite = Math.Max(
            donnees.dernierMoisTraite,
            indexMois);

        PointHistoriqueWhatIf point = new PointHistoriqueWhatIf
        {
            indexMois = indexMois,
            moisCalendrier = moisCalendrier,
            patrimoineReelCentimes = Math.Max(0, patrimoineReelCentimes),
            patrimoineAlternatifCentimes = patrimoineAlternatif,
            liquiditesAlternativesCentimes =
                donnees.liquiditesCentimes,
            valeurBourseAlternativeCentimes = valeurBourse,
            ecartCumuleCentimes = DifferenceSaturee(
                patrimoineAlternatif,
                Math.Max(0, patrimoineReelCentimes))
        };

        if (donnees.historique == null)
        {
            donnees.historique = new List<PointHistoriqueWhatIf>();
        }

        donnees.historique.RemoveAll(
            existant =>
                existant != null &&
                existant.indexMois == indexMois);
        donnees.historique.Add(point);
        donnees.historique.Sort(
            (gauche, droite) =>
                gauche.indexMois.CompareTo(droite.indexMois));

        return point.Copier();
    }

    /// <summary>
    /// Produit l'allocation courante en pourcentages pour la prochaine recherche.
    /// </summary>
    public static Dictionary<string, int> ConstruireAllocationCourante(
        DonneesWhatIf donnees,
        IReadOnlyDictionary<string, int> prixCentimesParActif)
    {
        Dictionary<string, int> resultat =
            new Dictionary<string, int>(StringComparer.Ordinal);

        if (donnees == null)
        {
            resultat[MoteurRechercheFaisceauWhatIf.LiquiditesId] = 100;
            return resultat;
        }

        donnees.InitialiserSiNecessaire();
        Dictionary<string, int> valeurs =
            CalculerValeursPositions(
                donnees.portefeuille,
                prixCentimesParActif);
        int valeurBourse = SommeValeurs(valeurs);
        int patrimoine = AdditionSaturee(
            donnees.liquiditesCentimes,
            valeurBourse);

        if (patrimoine <= 0)
        {
            resultat[MoteurRechercheFaisceauWhatIf.LiquiditesId] = 100;
            return resultat;
        }

        List<string> ids = new List<string>(valeurs.Keys);
        ids.Sort(StringComparer.Ordinal);
        int totalActifs = 0;

        foreach (string actifId in ids)
        {
            int pourcentage = (int)Math.Floor(
                valeurs[actifId] * 100d / patrimoine);
            pourcentage = Limiter(pourcentage, 0, 100);
            resultat[actifId] = pourcentage;
            totalActifs += pourcentage;
        }

        resultat[MoteurRechercheFaisceauWhatIf.LiquiditesId] =
            Math.Max(0, 100 - totalActifs);
        return resultat;
    }

    private static Dictionary<string, int> NormaliserAllocation(
        IReadOnlyList<AllocationActifWhatIf> allocations)
    {
        SortedDictionary<string, int> brutes =
            new SortedDictionary<string, int>(StringComparer.Ordinal);

        if (allocations != null)
        {
            foreach (AllocationActifWhatIf allocation in allocations)
            {
                if (allocation == null ||
                    string.IsNullOrWhiteSpace(allocation.actifId) ||
                    allocation.pourcentage <= 0)
                {
                    continue;
                }

                int ancien = brutes.TryGetValue(
                    allocation.actifId,
                    out int valeur)
                    ? valeur
                    : 0;
                brutes[allocation.actifId] = AdditionSaturee(
                    ancien,
                    allocation.pourcentage);
            }
        }

        if (brutes.Count == 0)
        {
            return new Dictionary<string, int>(StringComparer.Ordinal)
            {
                { MoteurRechercheFaisceauWhatIf.LiquiditesId, 100 }
            };
        }

        int total = SommeValeurs(brutes);
        Dictionary<string, int> resultat =
            new Dictionary<string, int>(StringComparer.Ordinal);
        int distribue = 0;
        List<string> ids = new List<string>(brutes.Keys);
        ids.Sort(StringComparer.Ordinal);

        if (total <= 100)
        {
            foreach (string id in ids)
            {
                int valeur = Limiter(brutes[id], 0, 100);
                resultat[id] = valeur;
                distribue += valeur;
            }

            int cashExistant = resultat.TryGetValue(
                MoteurRechercheFaisceauWhatIf.LiquiditesId,
                out int cash)
                ? cash
                : 0;
            resultat[MoteurRechercheFaisceauWhatIf.LiquiditesId] =
                cashExistant + Math.Max(0, 100 - distribue);
            return resultat;
        }

        for (int index = 0; index < ids.Count; index++)
        {
            string id = ids[index];
            int valeur = index == ids.Count - 1
                ? 100 - distribue
                : (int)Math.Floor(brutes[id] * 100d / total);
            valeur = Math.Max(0, valeur);
            resultat[id] = valeur;
            distribue += valeur;
        }

        if (!resultat.ContainsKey(
                MoteurRechercheFaisceauWhatIf.LiquiditesId))
        {
            resultat[MoteurRechercheFaisceauWhatIf.LiquiditesId] = 0;
        }

        return resultat;
    }

    private static Dictionary<string, int> CalculerValeursPositions(
        DonneesBourse portefeuille,
        IReadOnlyDictionary<string, int> prixCentimesParActif)
    {
        Dictionary<string, int> resultat =
            new Dictionary<string, int>(StringComparer.Ordinal);

        if (portefeuille?.positions == null)
        {
            return resultat;
        }

        foreach (PositionBourse position in portefeuille.positions)
        {
            if (position == null ||
                string.IsNullOrWhiteSpace(position.actifId) ||
                position.quantite <= 0f)
            {
                continue;
            }

            int prixCentimes = ObtenirPrixCentimes(
                prixCentimesParActif,
                position.actifId);
            if (prixCentimes <= 0)
            {
                continue;
            }

            int valeur = LimiterEnEntier(
                Math.Round(position.quantite * prixCentimes));
            int ancienne = resultat.TryGetValue(
                position.actifId,
                out int existante)
                ? existante
                : 0;
            resultat[position.actifId] = AdditionSaturee(
                ancienne,
                valeur);
        }

        return resultat;
    }

    private static int CalculerCoutReallocation(
        int liquiditesActuelles,
        IReadOnlyDictionary<string, int> valeursActuelles,
        IReadOnlyDictionary<string, int> ciblePourcent,
        int patrimoine,
        int coutPleinTourCentimes)
    {
        if (patrimoine <= 0 || coutPleinTourCentimes <= 0)
        {
            return 0;
        }

        HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal)
        {
            MoteurRechercheFaisceauWhatIf.LiquiditesId
        };

        foreach (string id in valeursActuelles.Keys)
        {
            ids.Add(id);
        }

        foreach (string id in ciblePourcent.Keys)
        {
            ids.Add(id);
        }

        long sommeEcarts = 0;
        foreach (string id in ids)
        {
            int actuel = id == MoteurRechercheFaisceauWhatIf.LiquiditesId
                ? Math.Max(0, liquiditesActuelles)
                : ObtenirValeur(valeursActuelles, id);
            int cible = LimiterEnEntier(
                Math.Round(
                    patrimoine *
                    (ObtenirValeur(ciblePourcent, id) / 100d)));
            sommeEcarts += Math.Abs((long)cible - actuel);
        }

        double rotation = Math.Min(
            1d,
            sommeEcarts / (2d * patrimoine));
        return LimiterEnEntier(
            Math.Round(coutPleinTourCentimes * rotation));
    }

    private static void EnregistrerDecision(
        DonneesWhatIf donnees,
        DecisionWhatIf decision,
        int indexMois)
    {
        if (donnees.decisions == null)
        {
            donnees.decisions = new List<DecisionWhatIf>();
        }

        donnees.decisions.RemoveAll(
            existante =>
                existante != null &&
                existante.indexMois == indexMois);

        DecisionWhatIf copie = decision.Copier();
        copie.indexMois = indexMois;
        donnees.decisions.Add(copie);
        donnees.decisions.Sort(
            (gauche, droite) =>
                gauche.indexMois.CompareTo(droite.indexMois));
    }

    private static int ObtenirPrixCentimes(
        IReadOnlyDictionary<string, int> prix,
        string actifId)
    {
        if (prix != null &&
            prix.TryGetValue(actifId, out int valeur))
        {
            return Math.Max(0, valeur);
        }

        return 0;
    }

    private static int ObtenirValeur(
        IReadOnlyDictionary<string, int> valeurs,
        string id)
    {
        return valeurs != null &&
            valeurs.TryGetValue(id, out int valeur)
            ? Math.Max(0, valeur)
            : 0;
    }

    private static int SommeValeurs(
        IReadOnlyDictionary<string, int> valeurs)
    {
        long total = 0;
        foreach (int valeur in valeurs.Values)
        {
            total += Math.Max(0, valeur);
        }

        return total >= int.MaxValue
            ? int.MaxValue
            : (int)total;
    }

    private static int AdditionSaturee(int gauche, int droite)
    {
        long total = (long)Math.Max(0, gauche) + Math.Max(0, droite);
        return total >= int.MaxValue
            ? int.MaxValue
            : (int)total;
    }

    private static int DifferenceSaturee(int gauche, int droite)
    {
        long difference = (long)gauche - droite;
        if (difference > int.MaxValue)
        {
            return int.MaxValue;
        }

        if (difference < int.MinValue)
        {
            return int.MinValue;
        }

        return (int)difference;
    }

    private static int LimiterEnEntier(double valeur)
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
}