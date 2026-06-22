using System;
using System.Collections.Generic;

/// <summary>
/// Bilan explicable des flux reels reproduits dans la trajectoire What If.
/// </summary>
[Serializable]
public sealed class ResultatFluxMensuelsWhatIf
{
    public bool succes;
    public int transactionsAnalysees;
    public int transactionsReproduites;
    public int transactionsIgnorees;
    public int revenusCentimes;
    public int depensesCentimes;
    public int fluxNetCentimes;
    public int liquiditesAvantCentimes;
    public int liquiditesApresCentimes;
    public int valeurLiquidationCentimes;
    public int deficitNonCouvertCentimes;
    public List<ResultatClassificationFluxWhatIf> classifications =
        new List<ResultatClassificationFluxWhatIf>();
    public List<string> diagnostics = new List<string>();
}

/// <summary>
/// Reproduit uniquement les revenus et depenses externes du joueur reel.
/// Les decisions boursieres, transferts internes et rendements sont exclus.
/// </summary>
/// <remarks>
/// Ce service ne modifie jamais DonneesJoueur. En cas de depense superieure
/// aux liquidites alternatives, il vend proportionnellement des positions
/// au prix deja observe, sans consulter de prix futur.
/// </remarks>
public static class ServiceFluxMensuelsWhatIf
{
    private const float EpsilonQuantite = 0.000001f;

    public static ResultatFluxMensuelsWhatIf Analyser(
        DonneesJoueur joueur)
    {
        List<Transaction> transactions = new List<Transaction>();

        if (joueur?.comptes != null)
        {
            List<string> ids = new List<string>(joueur.comptes.Keys);
            ids.Sort(StringComparer.Ordinal);

            foreach (string id in ids)
            {
                CompteBanquaire compte = joueur.comptes[id];
                List<Transaction> historique =
                    compte?.GetHistorique()?.GetHistorique();
                if (historique == null)
                {
                    continue;
                }

                foreach (Transaction transaction in historique)
                {
                    if (transaction != null)
                    {
                        transactions.Add(transaction);
                    }
                }
            }
        }

        return Analyser(transactions);
    }

    public static ResultatFluxMensuelsWhatIf Analyser(
        IEnumerable<Transaction> transactions)
    {
        ResultatFluxMensuelsWhatIf resultat =
            new ResultatFluxMensuelsWhatIf
            {
                succes = true
            };

        long revenus = 0;
        long depenses = 0;

        if (transactions != null)
        {
            foreach (Transaction transaction in transactions)
            {
                ResultatClassificationFluxWhatIf classification =
                    ClassificateurFluxWhatIf.Classifier(transaction);
                resultat.classifications.Add(classification);
                resultat.transactionsAnalysees++;

                if (!classification.doitEtreReproduit)
                {
                    resultat.transactionsIgnorees++;
                    continue;
                }

                resultat.transactionsReproduites++;
                if (classification.montantCentimes >= 0)
                {
                    revenus += classification.montantCentimes;
                }
                else
                {
                    depenses += -(long)classification.montantCentimes;
                }
            }
        }

        resultat.revenusCentimes = LimiterEnEntier(revenus);
        resultat.depensesCentimes = LimiterEnEntier(depenses);
        resultat.fluxNetCentimes = LimiterSigne(
            revenus - depenses);

        resultat.diagnostics.Add(
            resultat.transactionsReproduites +
            " flux reproduit(s), " +
            resultat.transactionsIgnorees +
            " flux ignore(s), net " +
            resultat.fluxNetCentimes +
            " centimes.");

        return resultat;
    }

    public static ResultatFluxMensuelsWhatIf Appliquer(
        DonneesWhatIf donnees,
        DonneesJoueur joueurReel,
        IReadOnlyDictionary<string, int> prixObservesCentimes,
        int indexMois)
    {
        return Appliquer(
            donnees,
            Analyser(joueurReel),
            prixObservesCentimes,
            indexMois);
    }

    public static ResultatFluxMensuelsWhatIf Appliquer(
        DonneesWhatIf donnees,
        ResultatFluxMensuelsWhatIf analyse,
        IReadOnlyDictionary<string, int> prixObservesCentimes,
        int indexMois)
    {
        if (donnees == null)
        {
            ResultatFluxMensuelsWhatIf echec =
                CopierAnalyse(analyse);
            echec.succes = false;
            echec.diagnostics.Add("Donnees What If absentes.");
            return echec;
        }

        donnees.InitialiserSiNecessaire();
        ResultatFluxMensuelsWhatIf resultat =
            CopierAnalyse(analyse);
        resultat.liquiditesAvantCentimes =
            Math.Max(0, donnees.liquiditesCentimes);

        if (resultat.fluxNetCentimes >= 0)
        {
            donnees.liquiditesCentimes = AdditionSaturee(
                donnees.liquiditesCentimes,
                resultat.fluxNetCentimes);
            resultat.liquiditesApresCentimes =
                donnees.liquiditesCentimes;
            resultat.succes = true;
            ActualiserValeurPortefeuille(
                donnees,
                prixObservesCentimes,
                indexMois);
            return resultat;
        }

        int depenseRestante =
            resultat.fluxNetCentimes == int.MinValue
                ? int.MaxValue
                : -resultat.fluxNetCentimes;

        int prisSurLiquidites = Math.Min(
            Math.Max(0, donnees.liquiditesCentimes),
            depenseRestante);
        donnees.liquiditesCentimes -= prisSurLiquidites;
        depenseRestante -= prisSurLiquidites;

        if (depenseRestante > 0)
        {
            resultat.valeurLiquidationCentimes =
                LiquiderPositions(
                    donnees,
                    prixObservesCentimes,
                    depenseRestante);
            depenseRestante = Math.Max(
                0,
                depenseRestante -
                resultat.valeurLiquidationCentimes);
        }

        resultat.deficitNonCouvertCentimes = depenseRestante;
        resultat.liquiditesApresCentimes =
            Math.Max(0, donnees.liquiditesCentimes);
        resultat.succes = depenseRestante == 0;

        if (resultat.valeurLiquidationCentimes > 0)
        {
            resultat.diagnostics.Add(
                "Liquidation alternative de " +
                resultat.valeurLiquidationCentimes +
                " centimes pour financer les flux externes.");
        }

        if (depenseRestante > 0)
        {
            resultat.diagnostics.Add(
                "Deficit alternatif non couvert : " +
                depenseRestante +
                " centimes.");
        }

        ActualiserValeurPortefeuille(
            donnees,
            prixObservesCentimes,
            indexMois);
        return resultat;
    }

    private static int LiquiderPositions(
        DonneesWhatIf donnees,
        IReadOnlyDictionary<string, int> prix,
        int montantRecherche)
    {
        if (montantRecherche <= 0 ||
            donnees.portefeuille?.positions == null)
        {
            return 0;
        }

        List<PositionValorisee> positions =
            new List<PositionValorisee>();

        foreach (PositionBourse position in donnees.portefeuille.positions)
        {
            if (position == null ||
                position.quantite <= EpsilonQuantite ||
                string.IsNullOrWhiteSpace(position.actifId))
            {
                continue;
            }

            int prixCentimes = ObtenirPrix(prix, position.actifId);
            if (prixCentimes <= 0)
            {
                continue;
            }

            int valeur = LimiterEnEntier(
                Math.Round(position.quantite * prixCentimes));
            if (valeur <= 0)
            {
                continue;
            }

            positions.Add(
                new PositionValorisee(
                    position,
                    prixCentimes,
                    valeur));
        }

        positions.Sort(
            (gauche, droite) =>
                string.CompareOrdinal(
                    gauche.position.actifId,
                    droite.position.actifId));

        int totalLiquidable = 0;
        foreach (PositionValorisee position in positions)
        {
            totalLiquidable = AdditionSaturee(
                totalLiquidable,
                position.valeurCentimes);
        }

        int cible = Math.Min(montantRecherche, totalLiquidable);
        if (cible <= 0)
        {
            return 0;
        }

        int[] ventes = new int[positions.Count];
        int distribue = 0;

        for (int i = 0; i < positions.Count; i++)
        {
            int vente = (int)Math.Floor(
                cible *
                (positions[i].valeurCentimes /
                 (double)totalLiquidable));
            vente = Math.Min(vente, positions[i].valeurCentimes);
            ventes[i] = vente;
            distribue += vente;
        }

        int reste = cible - distribue;
        for (int i = 0; i < positions.Count && reste > 0; i++)
        {
            int capacite =
                positions[i].valeurCentimes - ventes[i];
            int ajout = Math.Min(capacite, reste);
            ventes[i] += ajout;
            reste -= ajout;
        }

        int leve = 0;
        for (int i = 0; i < positions.Count; i++)
        {
            int venteCentimes = ventes[i];
            if (venteCentimes <= 0)
            {
                continue;
            }

            PositionValorisee valorisee = positions[i];
            float quantiteVendue =
                venteCentimes >= valorisee.valeurCentimes
                    ? valorisee.position.quantite
                    : venteCentimes /
                      (float)valorisee.prixCentimes;

            valorisee.position.RetirerQuantite(quantiteVendue);
            leve = AdditionSaturee(leve, venteCentimes);
        }

        donnees.portefeuille.positions.RemoveAll(
            position =>
                position == null ||
                position.quantite <= EpsilonQuantite);

        return leve;
    }

    private static void ActualiserValeurPortefeuille(
        DonneesWhatIf donnees,
        IReadOnlyDictionary<string, int> prix,
        int indexMois)
    {
        int valeur = 0;

        if (donnees.portefeuille?.positions != null)
        {
            foreach (PositionBourse position in donnees.portefeuille.positions)
            {
                if (position == null ||
                    position.quantite <= EpsilonQuantite)
                {
                    continue;
                }

                int prixCentimes = ObtenirPrix(
                    prix,
                    position.actifId);
                if (prixCentimes <= 0)
                {
                    continue;
                }

                valeur = AdditionSaturee(
                    valeur,
                    LimiterEnEntier(
                        Math.Round(
                            position.quantite *
                            prixCentimes)));
            }
        }

        donnees.portefeuille.DefinirValeurMarche(
            valeur,
            Math.Max(0, indexMois));
    }

    private static ResultatFluxMensuelsWhatIf CopierAnalyse(
        ResultatFluxMensuelsWhatIf source)
    {
        ResultatFluxMensuelsWhatIf copie =
            new ResultatFluxMensuelsWhatIf();

        if (source == null)
        {
            copie.succes = true;
            return copie;
        }

        copie.succes = source.succes;
        copie.transactionsAnalysees = source.transactionsAnalysees;
        copie.transactionsReproduites =
            source.transactionsReproduites;
        copie.transactionsIgnorees = source.transactionsIgnorees;
        copie.revenusCentimes = source.revenusCentimes;
        copie.depensesCentimes = source.depensesCentimes;
        copie.fluxNetCentimes = source.fluxNetCentimes;

        if (source.classifications != null)
        {
            foreach (
                ResultatClassificationFluxWhatIf classification
                in source.classifications)
            {
                if (classification == null)
                {
                    continue;
                }

                copie.classifications.Add(
                    new ResultatClassificationFluxWhatIf
                    {
                        type = classification.type,
                        classificationCertaine =
                            classification.classificationCertaine,
                        doitEtreReproduit =
                            classification.doitEtreReproduit,
                        diagnostic = classification.diagnostic,
                        libelleOriginal =
                            classification.libelleOriginal,
                        libelleNormalise =
                            classification.libelleNormalise,
                        montantCentimes =
                            classification.montantCentimes
                    });
            }
        }

        if (source.diagnostics != null)
        {
            copie.diagnostics.AddRange(source.diagnostics);
        }

        return copie;
    }

    private static int ObtenirPrix(
        IReadOnlyDictionary<string, int> prix,
        string actifId)
    {
        return prix != null &&
               !string.IsNullOrWhiteSpace(actifId) &&
               prix.TryGetValue(actifId, out int valeur)
            ? Math.Max(0, valeur)
            : 0;
    }

    private static int AdditionSaturee(int gauche, int droite)
    {
        long somme = (long)gauche + droite;
        return LimiterSigne(somme);
    }

    private static int LimiterEnEntier(double valeur)
    {
        if (valeur >= int.MaxValue)
        {
            return int.MaxValue;
        }

        return valeur <= 0d ? 0 : (int)valeur;
    }

    private static int LimiterSigne(long valeur)
    {
        if (valeur > int.MaxValue)
        {
            return int.MaxValue;
        }

        return valeur < int.MinValue
            ? int.MinValue
            : (int)valeur;
    }

    private sealed class PositionValorisee
    {
        public readonly PositionBourse position;
        public readonly int prixCentimes;
        public readonly int valeurCentimes;

        public PositionValorisee(
            PositionBourse position,
            int prixCentimes,
            int valeurCentimes)
        {
            this.position = position;
            this.prixCentimes = prixCentimes;
            this.valeurCentimes = valeurCentimes;
        }
    }
}