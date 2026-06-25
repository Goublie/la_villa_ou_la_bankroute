using System;
using System.Collections.Generic;

[Serializable]
public sealed class ResultatSynchronisationPretsWhatIf
{
    public int nouveauxPrets;
    public List<DonneesPret> pretsCopies = new List<DonneesPret>();
    public List<string> diagnostics = new List<string>();
}

/// <summary>
/// Gere les actifs immobiliers et les dettes propres a la trajectoire What If.
/// </summary>
/// <remarks>
/// Le portefeuille alternatif reste totalement independant du joueur reel.
/// Les biens et prets ne sont jamais consideres comme du capital boursier
/// reallouable.
/// </remarks>
public static class ServicePatrimoineAlternatifWhatIf
{
    public static void InitialiserDepuisJoueur(
        DonneesWhatIf donnees,
        DonneesJoueur joueur,
        int patrimoineNetInitialCentimes,
        int indexMois)
    {
        if (donnees == null)
        {
            throw new ArgumentNullException(nameof(donnees));
        }

        donnees.InitialiserSiNecessaire();
        joueur?.InitialiserSiNecessaire();

        if (donnees.initialisee &&
            donnees.actifsPassifsImmobiliersInitialises)
        {
            InitialiserEmpreintesDepuisPretsAlternatifs(donnees);
            return;
        }

        donnees.immobilier = CopierImmobilierPossede(
            joueur != null ? joueur.immobilier : null);
        donnees.pretsImmobiliers = CopierPrets(
            joueur != null ? joueur.pretsImmobiliers : null);
        IndexerPretsSynchronises(
            donnees,
            joueur != null ? joueur.pretsImmobiliers : null);

        int valeurImmobilier = CalculerValeurImmobilier(donnees);
        int dettes = CalculerDettes(donnees);
        int patrimoineNet = Math.Max(0, patrimoineNetInitialCentimes);

        if (donnees.initialisee)
        {
            // Migration d'une sauvegarde creee avant la prise en compte de
            // l'immobilier et des dettes : le portefeuille financier existant
            // est conserve, seuls les actifs/passifs manquants sont ajoutes.
            donnees.actifsPassifsImmobiliersInitialises = true;
            donnees.capitalInitialCentimes = patrimoineNet;
            donnees.dernierMoisMensualitesPretsTraite =
                Math.Max(-1, indexMois - 1);
            return;
        }

        int patrimoineComplet = joueur != null
            ? ServicePatrimoine.Calculer(joueur).centimes
            : patrimoineNetInitialCentimes;
        int capitalMobilisable = LimiterNonNegatif(
            (long)patrimoineComplet - valeurImmobilier + dettes);

        donnees.initialisee = true;
        donnees.actifsPassifsImmobiliersInitialises = true;
        donnees.moisInitialisation = Math.Max(0, indexMois);
        donnees.dernierMoisTraite = Math.Max(-1, indexMois - 1);
        donnees.dernierMoisMensualitesPretsTraite =
            Math.Max(-1, indexMois - 1);
        donnees.capitalInitialCentimes = patrimoineNet;
        donnees.liquiditesCentimes = capitalMobilisable;
        donnees.portefeuille.positions = new List<PositionBourse>();
        donnees.portefeuille.DefinirValeurMarche(
            0,
            Math.Max(0, indexMois));
    }

    /// <summary>
    /// Copie uniquement les nouveaux prets contractes par le joueur apres
    /// l'initialisation du What If. Le moteur alternatif ne cree jamais de pret.
    /// </summary>
    public static ResultatSynchronisationPretsWhatIf
        SynchroniserNouveauxPretsDepuisJoueur(
            DonneesWhatIf donnees,
            DonneesJoueur joueur)
    {
        ResultatSynchronisationPretsWhatIf resultat =
            new ResultatSynchronisationPretsWhatIf();

        if (donnees == null || joueur == null)
        {
            return resultat;
        }

        donnees.InitialiserSiNecessaire();
        joueur.InitialiserSiNecessaire();
        InitialiserEmpreintesDepuisPretsAlternatifs(donnees);

        HashSet<string> dejaSynchronises =
            new HashSet<string>(
                donnees.empreintesPretsReelsSynchronises,
                StringComparer.Ordinal);

        List<PretAvecEmpreinte> pretsReels =
            ConstruirePretsAvecEmpreinte(joueur.pretsImmobiliers);

        foreach (PretAvecEmpreinte entree in pretsReels)
        {
            if (entree.pret == null ||
                entree.pret.capitalRestantDu.centimes <= 0 ||
                !dejaSynchronises.Add(entree.empreinte))
            {
                continue;
            }

            DonneesPret copie = CopierPret(entree.pret);
            donnees.pretsImmobiliers.Add(copie);
            donnees.empreintesPretsReelsSynchronises.Add(
                entree.empreinte);
            resultat.pretsCopies.Add(copie);
            resultat.nouveauxPrets++;
        }

        if (resultat.nouveauxPrets > 0)
        {
            resultat.diagnostics.Add(
                resultat.nouveauxPrets +
                " nouveau(x) pret(s) reel(s) copie(s) dans le What If.");
        }

        return resultat;
    }

    /// <summary>
    /// Reproduit les flux reels, neutralise leurs lignes de pret puis applique
    /// exactement une fois les mensualites de la copie alternative.
    /// </summary>
    public static ResultatFluxMensuelsWhatIf AppliquerFluxMensuels(
        DonneesWhatIf donnees,
        DonneesJoueur joueurReel,
        IReadOnlyDictionary<string, int> prixObservesCentimes,
        int indexMois)
    {
        return AppliquerFluxMensuels(
            donnees,
            joueurReel,
            prixObservesCentimes,
            indexMois,
            null);
    }

    public static ResultatFluxMensuelsWhatIf AppliquerFluxMensuels(
        DonneesWhatIf donnees,
        DonneesJoueur joueurReel,
        IReadOnlyDictionary<string, int> prixObservesCentimes,
        int indexMois,
        IReadOnlyList<DonneesPret> pretsDejaAmortisDansSource)
    {
        if (donnees == null)
        {
            return ServiceFluxMensuelsWhatIf.Appliquer(
                null,
                ServiceFluxMensuelsWhatIf.Analyser(joueurReel),
                prixObservesCentimes,
                indexMois);
        }

        donnees.InitialiserSiNecessaire();
        ResultatFluxMensuelsWhatIf analyse =
            ServiceFluxMensuelsWhatIf.Analyser(joueurReel);

        int lignesPretNeutralisees =
            NeutraliserMensualitesReelles(analyse);

        bool moisDejaTraite =
            donnees.dernierMoisMensualitesPretsTraite >= indexMois;
        int nombrePrets = 0;
        int mensualitesCentimes = moisDejaTraite
            ? 0
            : CalculerMensualitesActives(donnees, out nombrePrets);

        if (mensualitesCentimes > 0)
        {
            analyse.depensesCentimes = AdditionNonNegativeSaturee(
                analyse.depensesCentimes,
                mensualitesCentimes);
            analyse.fluxNetCentimes = SoustractionSaturee(
                analyse.fluxNetCentimes,
                mensualitesCentimes);
        }

        ResultatFluxMensuelsWhatIf resultat =
            ServiceFluxMensuelsWhatIf.Appliquer(
                donnees,
                analyse,
                prixObservesCentimes,
                indexMois);

        int pretsNonReamortis = 0;
        if (!moisDejaTraite)
        {
            pretsNonReamortis = AmortirPretsAlternatifs(
                donnees,
                indexMois,
                pretsDejaAmortisDansSource);
        }

        if (pretsNonReamortis > 0)
        {
            resultat.diagnostics.Add(
                pretsNonReamortis +
                " nouveau(x) pret(s) deja amorti(s) dans l'etat reel : " +
                "aucune seconde baisse de dette appliquee.");
        }

        if (lignesPretNeutralisees > 0)
        {
            resultat.diagnostics.Add(
                lignesPretNeutralisees +
                " ligne(s) de pret reel neutralisee(s).");
        }

        if (mensualitesCentimes > 0)
        {
            resultat.diagnostics.Add(
                nombrePrets + " mensualite(s) alternative(s), soit " +
                mensualitesCentimes +
                " centimes, appliquee(s) une seule fois.");
        }
        else if (moisDejaTraite)
        {
            resultat.diagnostics.Add(
                "Mensualites alternatives deja traitees pour le mois " +
                indexMois + ".");
        }

        return resultat;
    }

    public static int CalculerPatrimoineAlternatif(
        DonneesWhatIf donnees,
        IReadOnlyDictionary<string, int> prixObservesCentimes)
    {
        if (donnees == null)
        {
            return 0;
        }

        donnees.InitialiserSiNecessaire();

        long total = Math.Max(0, donnees.liquiditesCentimes);
        total += CalculerValeurBourse(donnees, prixObservesCentimes);
        total += CalculerValeurImmobilier(donnees);
        total -= CalculerDettes(donnees);
        return LimiterSigne(total);
    }

    public static PointHistoriqueWhatIf CompleterPointHistorique(
        DonneesWhatIf donnees,
        PointHistoriqueWhatIf point,
        IReadOnlyDictionary<string, int> prixObservesCentimes,
        int patrimoineReelCentimes)
    {
        if (point == null || donnees == null)
        {
            return point;
        }

        int valeurImmobilier = CalculerValeurImmobilier(donnees);
        int dettes = CalculerDettes(donnees);
        int patrimoine = CalculerPatrimoineAlternatif(
            donnees,
            prixObservesCentimes);

        AppliquerValeurs(
            point,
            patrimoine,
            valeurImmobilier,
            dettes,
            patrimoineReelCentimes);

        if (donnees.historique != null)
        {
            foreach (PointHistoriqueWhatIf existant in donnees.historique)
            {
                if (existant != null &&
                    existant.indexMois == point.indexMois)
                {
                    AppliquerValeurs(
                        existant,
                        patrimoine,
                        valeurImmobilier,
                        dettes,
                        patrimoineReelCentimes);
                    break;
                }
            }
        }

        return point.Copier();
    }

    public static int CalculerValeurImmobilier(DonneesWhatIf donnees)
    {
        long total = 0;
        List<BienImmobilier> biens =
            donnees?.immobilier?.biensPossedes;

        if (biens != null)
        {
            foreach (BienImmobilier bien in biens)
            {
                if (bien != null)
                {
                    total += Math.Max(
                        0,
                        bien.GetValeurPatrimoine().centimes);
                }
            }
        }

        return LimiterNonNegatif(total);
    }

    public static int CalculerDettes(DonneesWhatIf donnees)
    {
        long total = 0;

        if (donnees?.pretsImmobiliers != null)
        {
            foreach (DonneesPret pret in donnees.pretsImmobiliers)
            {
                if (pret != null &&
                    pret.capitalRestantDu.centimes > 0)
                {
                    total += pret.capitalRestantDu.centimes;
                }
            }
        }

        return LimiterNonNegatif(total);
    }

    private static DonneesImmobilier CopierImmobilierPossede(
        DonneesImmobilier source)
    {
        DonneesImmobilier copie = new DonneesImmobilier();
        copie.annoncesActuelles.Clear();

        if (source?.biensPossedes != null)
        {
            foreach (BienImmobilier bien in source.biensPossedes)
            {
                if (bien != null)
                {
                    copie.biensPossedes.Add(bien.Copier());
                }
            }
        }

        return copie;
    }

    private static List<DonneesPret> CopierPrets(
        IReadOnlyList<DonneesPret> sources)
    {
        List<DonneesPret> copies = new List<DonneesPret>();

        if (sources != null)
        {
            foreach (DonneesPret source in sources)
            {
                if (source != null)
                {
                    copies.Add(CopierPret(source));
                }
            }
        }

        return copies;
    }

    private static DonneesPret CopierPret(DonneesPret source)
    {
        return new DonneesPret(
            new argent(source.montantEmprunte.centimes),
            source.dureeAns,
            source.tauxAnnuel,
            new argent(source.mensualite.centimes))
        {
            moisRestants = source.moisRestants,
            capitalRestantDu =
                new argent(source.capitalRestantDu.centimes)
        };
    }

    private sealed class PretAvecEmpreinte
    {
        public DonneesPret pret;
        public string empreinte;
    }

    private static void IndexerPretsSynchronises(
        DonneesWhatIf donnees,
        IReadOnlyList<DonneesPret> sources)
    {
        donnees.empreintesPretsReelsSynchronises.Clear();
        foreach (PretAvecEmpreinte entree in ConstruirePretsAvecEmpreinte(
                     sources))
        {
            donnees.empreintesPretsReelsSynchronises.Add(
                entree.empreinte);
        }
    }

    private static void InitialiserEmpreintesDepuisPretsAlternatifs(
        DonneesWhatIf donnees)
    {
        if (donnees.empreintesPretsReelsSynchronises.Count > 0 ||
            donnees.pretsImmobiliers == null ||
            donnees.pretsImmobiliers.Count == 0)
        {
            return;
        }

        foreach (PretAvecEmpreinte entree in ConstruirePretsAvecEmpreinte(
                     donnees.pretsImmobiliers))
        {
            donnees.empreintesPretsReelsSynchronises.Add(
                entree.empreinte);
        }
    }

    private static List<PretAvecEmpreinte> ConstruirePretsAvecEmpreinte(
        IReadOnlyList<DonneesPret> prets)
    {
        List<PretAvecEmpreinte> resultat =
            new List<PretAvecEmpreinte>();
        Dictionary<string, int> occurrences =
            new Dictionary<string, int>(StringComparer.Ordinal);

        if (prets == null)
        {
            return resultat;
        }

        foreach (DonneesPret pret in prets)
        {
            if (pret == null)
            {
                continue;
            }

            string baseEmpreinte =
                pret.montantEmprunte.centimes + "|" +
                pret.dureeAns + "|" +
                ObtenirBitsTaux(pret.tauxAnnuel) + "|" +
                pret.mensualite.centimes;

            int occurrence = occurrences.TryGetValue(
                baseEmpreinte,
                out int valeur)
                ? valeur + 1
                : 1;
            occurrences[baseEmpreinte] = occurrence;

            resultat.Add(
                new PretAvecEmpreinte
                {
                    pret = pret,
                    empreinte = baseEmpreinte + "#" + occurrence
                });
        }

        return resultat;
    }

    private static int ObtenirBitsTaux(float taux)
    {
        byte[] octets = BitConverter.GetBytes(taux);
        return BitConverter.ToInt32(octets, 0);
    }

    private static int NeutraliserMensualitesReelles(
        ResultatFluxMensuelsWhatIf analyse)
    {
        if (analyse?.classifications == null)
        {
            return 0;
        }

        int neutralisees = 0;

        foreach (
            ResultatClassificationFluxWhatIf classification
            in analyse.classifications)
        {
            if (classification == null ||
                !classification.doitEtreReproduit ||
                !EstMensualitePret(classification.libelleNormalise))
            {
                continue;
            }

            int montant = classification.montantCentimes;
            if (montant < 0)
            {
                int depense = montant == int.MinValue
                    ? int.MaxValue
                    : -montant;
                analyse.depensesCentimes = Math.Max(
                    0,
                    analyse.depensesCentimes - depense);
                analyse.fluxNetCentimes = AdditionSaturee(
                    analyse.fluxNetCentimes,
                    depense);
            }
            else
            {
                analyse.revenusCentimes = Math.Max(
                    0,
                    analyse.revenusCentimes - montant);
                analyse.fluxNetCentimes = SoustractionSaturee(
                    analyse.fluxNetCentimes,
                    montant);
            }

            classification.classificationCertaine = true;
            classification.doitEtreReproduit = false;
            classification.diagnostic =
                "Mensualite geree par la copie de pret alternative.";
            analyse.transactionsReproduites = Math.Max(
                0,
                analyse.transactionsReproduites - 1);
            analyse.transactionsIgnorees++;
            neutralisees++;
        }

        return neutralisees;
    }

    private static bool EstMensualitePret(string libelleNormalise)
    {
        if (string.IsNullOrWhiteSpace(libelleNormalise))
        {
            return false;
        }

        return libelleNormalise.StartsWith(
                   "pret immo",
                   StringComparison.Ordinal) ||
               libelleNormalise.StartsWith(
                   "pret immobilier",
                   StringComparison.Ordinal);
    }

    private static int CalculerMensualitesActives(
        DonneesWhatIf donnees,
        out int nombrePrets)
    {
        long total = 0;
        nombrePrets = 0;

        if (donnees.pretsImmobiliers != null)
        {
            foreach (DonneesPret pret in donnees.pretsImmobiliers)
            {
                if (pret == null ||
                    pret.moisRestants <= 0 ||
                    pret.capitalRestantDu.centimes <= 0)
                {
                    continue;
                }

                total += Math.Max(0, pret.mensualite.centimes);
                nombrePrets++;
            }
        }

        return LimiterNonNegatif(total);
    }

    private static int AmortirPretsAlternatifs(
        DonneesWhatIf donnees,
        int indexMois,
        IReadOnlyList<DonneesPret> pretsDejaAmortisDansSource)
    {
        int nonReamortis = 0;
        if (donnees.pretsImmobiliers != null)
        {
            for (
                int index = donnees.pretsImmobiliers.Count - 1;
                index >= 0;
                index--)
            {
                DonneesPret pret = donnees.pretsImmobiliers[index];
                if (pret == null)
                {
                    donnees.pretsImmobiliers.RemoveAt(index);
                    continue;
                }

                if (ContientReference(
                        pretsDejaAmortisDansSource,
                        pret))
                {
                    nonReamortis++;
                    continue;
                }

                if (pret.moisRestants <= 0 ||
                    pret.capitalRestantDu.centimes <= 0)
                {
                    donnees.pretsImmobiliers.RemoveAt(index);
                    continue;
                }

                pret.moisRestants--;
                if (pret.moisRestants <= 0)
                {
                    pret.capitalRestantDu = new argent(0);
                    donnees.pretsImmobiliers.RemoveAt(index);
                    continue;
                }

                int capitalApres = Math.Max(
                    0,
                    pret.capitalRestantDu.centimes -
                    Math.Max(0, pret.mensualite.centimes));
                pret.capitalRestantDu = new argent(capitalApres);
            }
        }

        donnees.dernierMoisMensualitesPretsTraite =
            Math.Max(
                donnees.dernierMoisMensualitesPretsTraite,
                indexMois);
        return nonReamortis;
    }

    private static bool ContientReference(
        IReadOnlyList<DonneesPret> prets,
        DonneesPret recherche)
    {
        if (prets == null)
        {
            return false;
        }

        for (int index = 0; index < prets.Count; index++)
        {
            if (ReferenceEquals(prets[index], recherche))
            {
                return true;
            }
        }

        return false;
    }

    private static int CalculerValeurBourse(
        DonneesWhatIf donnees,
        IReadOnlyDictionary<string, int> prixObservesCentimes)
    {
        if (donnees.portefeuille?.positions == null)
        {
            return 0;
        }

        if (prixObservesCentimes == null)
        {
            return Math.Max(
                0,
                donnees.portefeuille.GetValeurPatrimoine().centimes);
        }

        long total = 0;
        foreach (PositionBourse position in donnees.portefeuille.positions)
        {
            if (position == null ||
                position.quantite <= 0f ||
                string.IsNullOrWhiteSpace(position.actifId) ||
                !prixObservesCentimes.TryGetValue(
                    position.actifId,
                    out int prix) ||
                prix <= 0)
            {
                continue;
            }

            total += (long)Math.Round(position.quantite * prix);
        }

        return LimiterNonNegatif(total);
    }

    private static void AppliquerValeurs(
        PointHistoriqueWhatIf point,
        int patrimoine,
        int valeurImmobilier,
        int dettes,
        int patrimoineReelCentimes)
    {
        point.patrimoineReelCentimes =
            Math.Max(0, patrimoineReelCentimes);
        point.patrimoineAlternatifCentimes = patrimoine;
        point.valeurImmobilierAlternativeCentimes =
            valeurImmobilier;
        point.dettesAlternativesCentimes = dettes;
        point.ecartCumuleCentimes = SoustractionSaturee(
            patrimoine,
            point.patrimoineReelCentimes);
    }

    private static int AdditionNonNegativeSaturee(
        int gauche,
        int droite)
    {
        return LimiterNonNegatif(
            (long)Math.Max(0, gauche) + Math.Max(0, droite));
    }

    private static int AdditionSaturee(int gauche, int droite)
    {
        return LimiterSigne((long)gauche + droite);
    }

    private static int SoustractionSaturee(int gauche, int droite)
    {
        return LimiterSigne((long)gauche - droite);
    }

    private static int LimiterNonNegatif(long valeur)
    {
        if (valeur >= int.MaxValue)
        {
            return int.MaxValue;
        }

        return valeur <= 0 ? 0 : (int)valeur;
    }

    private static int LimiterSigne(long valeur)
    {
        if (valeur > int.MaxValue)
        {
            return int.MaxValue;
        }

        if (valeur < int.MinValue)
        {
            return int.MinValue;
        }

        return (int)valeur;
    }
}
