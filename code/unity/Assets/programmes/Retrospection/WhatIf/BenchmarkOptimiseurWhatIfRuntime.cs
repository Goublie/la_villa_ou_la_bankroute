using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Exécute des simulations comparatives reproductibles entre le moteur What If
/// et un joueur actif en buy-and-hold.
/// </summary>
public static class BenchmarkOptimiseurWhatIfRuntime
{
    public const int NombreParties = 10;
    private const int NombreMois = 48;
    private const int MoisEchauffement = 12;
    private const int CapitalInitialCentimes = 100000;

    private static readonly string[] Actifs =
    {
        "stable",
        "croissance",
        "cyclique"
    };

    [Serializable]
    public sealed class Rapport
    {
        public int nombreParties;
        public int victoiresOptimiseur;
        public long capitalCumuleOptimiseur;
        public long capitalCumuleJoueur;
        public bool absenceFuiteFutur;
        public string detail;
    }

    private sealed class ResultatPartie
    {
        public int graine;
        public int actifJoueur;
        public int capitalOptimiseur;
        public int capitalJoueur;
    }

    public static Rapport ExecuterDixParties()
    {
        List<ResultatPartie> parties =
            new List<ResultatPartie>();

        for (int graine = 0;
            graine < NombreParties;
            graine++)
        {
            parties.Add(ExecuterPartie(graine));
        }

        int victoires = 0;
        long cumulOptimiseur = 0;
        long cumulJoueur = 0;

        foreach (ResultatPartie partie in parties)
        {
            if (partie.capitalOptimiseur >
                partie.capitalJoueur)
            {
                victoires++;
            }

            cumulOptimiseur += partie.capitalOptimiseur;
            cumulJoueur += partie.capitalJoueur;
        }

        return new Rapport
        {
            nombreParties = parties.Count,
            victoiresOptimiseur = victoires,
            capitalCumuleOptimiseur = cumulOptimiseur,
            capitalCumuleJoueur = cumulJoueur,
            absenceFuiteFutur = VerifierAbsenceFuiteFutur(),
            detail = ConstruireRapport(
                parties,
                victoires,
                cumulOptimiseur,
                cumulJoueur)
        };
    }

    public static bool VerifierDeterminisme()
    {
        Rapport premier = ExecuterDixPartiesSansVerifierFutur();
        Rapport second = ExecuterDixPartiesSansVerifierFutur();

        return premier.nombreParties == second.nombreParties &&
            premier.victoiresOptimiseur ==
                second.victoiresOptimiseur &&
            premier.capitalCumuleOptimiseur ==
                second.capitalCumuleOptimiseur &&
            premier.capitalCumuleJoueur ==
                second.capitalCumuleJoueur &&
            string.Equals(
                premier.detail,
                second.detail,
                StringComparison.Ordinal);
    }

    public static bool VerifierAbsenceFuiteFutur()
    {
        const int moisObservation = 24;

        for (int graine = 0;
            graine < NombreParties;
            graine++)
        {
            List<float>[] prixOriginaux =
                GenererPrix(graine);

            List<float>[] prixAlteres =
                CopierPrix(prixOriginaux);

            AltererUniquementLeFutur(
                prixAlteres,
                moisObservation,
                graine);

            ResultatRechercheFaisceauWhatIf original =
                MoteurRechercheFaisceauWhatIf.Rechercher(
                    CapitalInitialCentimes,
                    CreerAllocationCash(),
                    ConstruirePrevisions(
                        prixOriginaux,
                        moisObservation),
                    CreerConfiguration(),
                    moisObservation);

            ResultatRechercheFaisceauWhatIf altere =
                MoteurRechercheFaisceauWhatIf.Rechercher(
                    CapitalInitialCentimes,
                    CreerAllocationCash(),
                    ConstruirePrevisions(
                        prixAlteres,
                        moisObservation),
                    CreerConfiguration(),
                    moisObservation);

            if (original.decisionRetenue == null ||
                altere.decisionRetenue == null ||
                !string.Equals(
                    original.decisionRetenue.strategieId,
                    altere.decisionRetenue.strategieId,
                    StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static Rapport ExecuterDixPartiesSansVerifierFutur()
    {
        List<ResultatPartie> parties =
            new List<ResultatPartie>();

        for (int graine = 0;
            graine < NombreParties;
            graine++)
        {
            parties.Add(ExecuterPartie(graine));
        }

        int victoires = 0;
        long cumulOptimiseur = 0;
        long cumulJoueur = 0;

        foreach (ResultatPartie partie in parties)
        {
            if (partie.capitalOptimiseur >
                partie.capitalJoueur)
            {
                victoires++;
            }

            cumulOptimiseur += partie.capitalOptimiseur;
            cumulJoueur += partie.capitalJoueur;
        }

        return new Rapport
        {
            nombreParties = parties.Count,
            victoiresOptimiseur = victoires,
            capitalCumuleOptimiseur = cumulOptimiseur,
            capitalCumuleJoueur = cumulJoueur,
            absenceFuiteFutur = true,
            detail = ConstruireRapport(
                parties,
                victoires,
                cumulOptimiseur,
                cumulJoueur)
        };
    }

    private static ResultatPartie ExecuterPartie(int graine)
    {
        List<float>[] prix = GenererPrix(graine);

        int capitalOptimiseur = CapitalInitialCentimes;
        int capitalJoueur = CapitalInitialCentimes;

        Dictionary<string, int> allocationOptimiseur =
            CreerAllocationCash();

        Dictionary<string, int> allocationJoueur =
            CreerAllocationCash();

        int actifJoueur = graine % Actifs.Length;
        Dictionary<string, int> cibleJoueur =
            CreerAllocationConcentree(actifJoueur);

        ConfigurationWhatIf configuration =
            CreerConfiguration();

        for (int mois = MoisEchauffement;
            mois < NombreMois - 1;
            mois++)
        {
            ResultatRechercheFaisceauWhatIf recherche =
                MoteurRechercheFaisceauWhatIf.Rechercher(
                    capitalOptimiseur,
                    allocationOptimiseur,
                    ConstruirePrevisions(prix, mois),
                    configuration,
                    mois);

            if (recherche.decisionRetenue == null)
            {
                throw new InvalidOperationException(
                    "Aucune décision What If à la partie " +
                    (graine + 1) +
                    ", mois " +
                    mois +
                    ".");
            }

            Dictionary<string, int> cibleOptimiseur =
                ConvertirAllocation(
                    recherche.decisionRetenue.allocations);

            VerifierAllocation(cibleOptimiseur);
            VerifierAllocation(cibleJoueur);

            capitalOptimiseur =
                AppliquerMoisObserve(
                    capitalOptimiseur,
                    allocationOptimiseur,
                    cibleOptimiseur,
                    prix,
                    mois,
                    configuration.coutTransactionCentimes);

            capitalJoueur =
                AppliquerMoisObserve(
                    capitalJoueur,
                    allocationJoueur,
                    cibleJoueur,
                    prix,
                    mois,
                    configuration.coutTransactionCentimes);

            allocationOptimiseur = cibleOptimiseur;
            allocationJoueur =
                CopierAllocation(cibleJoueur);
        }

        return new ResultatPartie
        {
            graine = graine,
            actifJoueur = actifJoueur,
            capitalOptimiseur = capitalOptimiseur,
            capitalJoueur = capitalJoueur
        };
    }

    private static List<float>[] GenererPrix(int graine)
    {
        System.Random aleatoire =
            new System.Random(20260623 + graine * 7919);

        List<float>[] prix =
        {
            new List<float> { 100f },
            new List<float> { 100f },
            new List<float> { 100f }
        };

        float[] regimesCroissance =
        {
            1.6f,
            0.8f,
            -1.2f,
            1.0f
        };

        for (int mois = 1;
            mois < NombreMois;
            mois++)
        {
            int regime =
                ((mois / 6) + graine) %
                regimesCroissance.Length;

            float rendementStable =
                0.35f +
                AleatoireEntre(
                    aleatoire,
                    -0.7f,
                    0.7f);

            float rendementCroissance =
                regimesCroissance[regime] +
                AleatoireEntre(
                    aleatoire,
                    -2.2f,
                    2.2f);

            float rendementCyclique =
                0.3f +
                1.8f *
                (float)Math.Sin(
                    (mois + graine) / 3.2d) +
                AleatoireEntre(
                    aleatoire,
                    -1.5f,
                    1.5f);

            float[] rendements =
            {
                rendementStable,
                rendementCroissance,
                rendementCyclique
            };

            for (int actif = 0;
                actif < prix.Length;
                actif++)
            {
                float nouveauPrix =
                    prix[actif][mois - 1] *
                    (1f + rendements[actif] / 100f);

                prix[actif].Add(
                    Math.Max(1f, nouveauPrix));
            }
        }

        return prix;
    }

    private static List<PrevisionActifWhatIf>
        ConstruirePrevisions(
            IReadOnlyList<float>[] prix,
            int mois)
    {
        List<PrevisionActifWhatIf> resultat =
            new List<PrevisionActifWhatIf>();

        for (int actif = 0;
            actif < Actifs.Length;
            actif++)
        {
            resultat.Add(
                ServicePrevisionWhatIf
                    .EstimerDepuisPrixConnus(
                        Actifs[actif],
                        prix[actif],
                        null,
                        mois));
        }

        return resultat;
    }

    private static ConfigurationWhatIf CreerConfiguration()
    {
        return new ConfigurationWhatIf
        {
            horizonMois = 3,
            largeurFaisceau = 25,
            pasAllocationPourcent = 25,
            penaliteRisque = 0.15f,
            penaliteDrawdown = 0.25f,
            coutTransactionCentimes = 20
        };
    }

    private static Dictionary<string, int>
        CreerAllocationCash()
    {
        return new Dictionary<string, int>(
            StringComparer.Ordinal)
        {
            {
                MoteurRechercheFaisceauWhatIf.LiquiditesId,
                100
            },
            { Actifs[0], 0 },
            { Actifs[1], 0 },
            { Actifs[2], 0 }
        };
    }

    private static Dictionary<string, int>
        CreerAllocationConcentree(int actifChoisi)
    {
        Dictionary<string, int> allocation =
            CreerAllocationCash();

        allocation[
            MoteurRechercheFaisceauWhatIf.LiquiditesId] = 0;

        allocation[Actifs[actifChoisi]] = 100;

        return allocation;
    }

    private static Dictionary<string, int>
        ConvertirAllocation(
            IReadOnlyList<AllocationActifWhatIf> allocations)
    {
        Dictionary<string, int> resultat =
            CreerAllocationCash();

        if (allocations == null)
        {
            return resultat;
        }

        foreach (AllocationActifWhatIf allocation in allocations)
        {
            if (allocation == null ||
                string.IsNullOrWhiteSpace(
                    allocation.actifId))
            {
                continue;
            }

            resultat[allocation.actifId] =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        allocation.pourcentage));
        }

        return resultat;
    }

    private static int AppliquerMoisObserve(
        int capitalCentimes,
        IReadOnlyDictionary<string, int> ancienneAllocation,
        IReadOnlyDictionary<string, int> nouvelleAllocation,
        IReadOnlyList<float>[] prix,
        int mois,
        int coutPleinTourCentimes)
    {
        int cout = CalculerCoutReallocation(
            ancienneAllocation,
            nouvelleAllocation,
            coutPleinTourCentimes);

        int capitalDisponible =
            Math.Max(0, capitalCentimes - cout);

        double facteur =
            ObtenirPourcentage(
                nouvelleAllocation,
                MoteurRechercheFaisceauWhatIf.LiquiditesId) /
            100d;

        for (int actif = 0;
            actif < Actifs.Length;
            actif++)
        {
            double poids =
                ObtenirPourcentage(
                    nouvelleAllocation,
                    Actifs[actif]) /
                100d;

            double ratio =
                prix[actif][mois + 1] /
                prix[actif][mois];

            facteur += poids * ratio;
        }

        return LimiterEntierPositif(
            Math.Round(
                capitalDisponible * facteur));
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

        HashSet<string> ids =
            new HashSet<string>(
                StringComparer.Ordinal);

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
            sommeEcarts += Math.Abs(
                ObtenirPourcentage(
                    nouvelleAllocation,
                    id) -
                ObtenirPourcentage(
                    ancienneAllocation,
                    id));
        }

        float rotationPourcent =
            Math.Min(100f, sommeEcarts / 2f);

        return (int)Math.Round(
            coutPleinTourCentimes *
            rotationPourcent /
            100f);
    }

    private static int ObtenirPourcentage(
        IReadOnlyDictionary<string, int> allocation,
        string actifId)
    {
        return allocation != null &&
            allocation.TryGetValue(
                actifId,
                out int valeur)
            ? valeur
            : 0;
    }

    private static void VerifierAllocation(
        IReadOnlyDictionary<string, int> allocation)
    {
        int total = 0;

        foreach (int pourcentage in allocation.Values)
        {
            if (pourcentage < 0 ||
                pourcentage > 100)
            {
                throw new InvalidOperationException(
                    "Pourcentage d'allocation invalide.");
            }

            total += pourcentage;
        }

        if (total != 100)
        {
            throw new InvalidOperationException(
                "L'allocation ne somme pas à 100 % : " +
                total +
                ".");
        }
    }

    private static Dictionary<string, int>
        CopierAllocation(
            IReadOnlyDictionary<string, int> source)
    {
        Dictionary<string, int> copie =
            new Dictionary<string, int>(
                StringComparer.Ordinal);

        foreach (
            KeyValuePair<string, int> ligne
            in source)
        {
            copie[ligne.Key] = ligne.Value;
        }

        return copie;
    }

    private static List<float>[] CopierPrix(
        IReadOnlyList<float>[] source)
    {
        List<float>[] copie =
            new List<float>[source.Length];

        for (int index = 0;
            index < source.Length;
            index++)
        {
            copie[index] =
                new List<float>(source[index]);
        }

        return copie;
    }

    private static void AltererUniquementLeFutur(
        IReadOnlyList<float>[] prix,
        int moisObservation,
        int graine)
    {
        for (int actif = 0;
            actif < prix.Length;
            actif++)
        {
            List<float> liste =
                prix[actif] as List<float>;

            if (liste == null)
            {
                throw new InvalidOperationException(
                    "Liste de prix mutable attendue.");
            }

            for (int mois = moisObservation + 1;
                mois < liste.Count;
                mois++)
            {
                float multiplicateur =
                    ((mois + actif + graine) % 2 == 0)
                        ? 7f
                        : 0.12f;

                liste[mois] =
                    Math.Max(
                        1f,
                        liste[mois] *
                        multiplicateur);
            }
        }
    }

    private static string ConstruireRapport(
        IReadOnlyList<ResultatPartie> parties,
        int victoires,
        long cumulOptimiseur,
        long cumulJoueur)
    {
        StringBuilder texte = new StringBuilder();

        texte.AppendLine(
            "Benchmark What If contre joueur actif buy-and-hold :");

        foreach (ResultatPartie partie in parties)
        {
            texte.Append("Partie ");
            texte.Append(partie.graine + 1);
            texte.Append(" | joueur sur ");
            texte.Append(Actifs[partie.actifJoueur]);
            texte.Append(" | What If ");
            texte.Append(partie.capitalOptimiseur / 100d);
            texte.Append(" EUR | joueur ");
            texte.Append(partie.capitalJoueur / 100d);
            texte.AppendLine(" EUR");
        }

        texte.Append("Victoires What If : ");
        texte.Append(victoires);
        texte.Append("/");
        texte.Append(parties.Count);
        texte.Append(" | cumul What If : ");
        texte.Append(cumulOptimiseur / 100d);
        texte.Append(" EUR | cumul joueur : ");
        texte.Append(cumulJoueur / 100d);
        texte.Append(" EUR");

        return texte.ToString();
    }

    private static float AleatoireEntre(
        System.Random aleatoire,
        float minimum,
        float maximum)
    {
        return minimum +
            (float)aleatoire.NextDouble() *
            (maximum - minimum);
    }

    private static int LimiterEntierPositif(double valeur)
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