using System.Collections.Generic;
using NUnit.Framework;

public class MoteurRechercheFaisceauWhatIfTests
{
    [Test]
    public void GenererAllocations_ToutesSommentCentPourCent()
    {
        List<List<AllocationActifWhatIf>> allocations =
            MoteurRechercheFaisceauWhatIf.GenererAllocationsCandidates(
                new List<string> { "b", "a" },
                25);

        Assert.That(allocations, Is.Not.Empty);
        foreach (List<AllocationActifWhatIf> allocation in allocations)
        {
            int total = 0;
            foreach (AllocationActifWhatIf ligne in allocation)
            {
                total += ligne.pourcentage;
                Assert.That(ligne.pourcentage % 25, Is.Zero);
            }

            Assert.That(total, Is.EqualTo(100));
        }
    }

    [Test]
    public void GenererAllocations_ContientCashEtConcentrationTotale()
    {
        List<List<AllocationActifWhatIf>> allocations =
            MoteurRechercheFaisceauWhatIf.GenererAllocationsCandidates(
                new List<string> { "a" },
                25);

        Assert.That(
            ContientAllocation(allocations, "cash", 100, "a", 0),
            Is.True);
        Assert.That(
            ContientAllocation(allocations, "cash", 0, "a", 100),
            Is.True);
    }

    [Test]
    public void ActifPositif_EstSelectionneSansPenalite()
    {
        ConfigurationWhatIf config = ConfigurationSimple();
        ResultatRechercheFaisceauWhatIf resultat =
            MoteurRechercheFaisceauWhatIf.Rechercher(
                100000,
                new Dictionary<string, int> { { "cash", 100 } },
                new List<PrevisionActifWhatIf>
                {
                    Prevision("croissance", 10f, 0f, 0f, 1f)
                },
                config,
                4);

        Assert.That(resultat.decisionRetenue, Is.Not.Null);
        Assert.That(
            Pourcentage(resultat.decisionRetenue, "croissance"),
            Is.EqualTo(100));
    }

    [Test]
    public void ActifNegatif_LaisseLeCapitalEnCash()
    {
        ConfigurationWhatIf config = ConfigurationSimple();
        ResultatRechercheFaisceauWhatIf resultat =
            MoteurRechercheFaisceauWhatIf.Rechercher(
                100000,
                new Dictionary<string, int> { { "cash", 100 } },
                new List<PrevisionActifWhatIf>
                {
                    Prevision("baisse", -10f, 0f, 0f, 1f)
                },
                config,
                0);

        Assert.That(
            Pourcentage(resultat.decisionRetenue, "cash"),
            Is.EqualTo(100));
    }

    [Test]
    public void PenaliteRisque_PrefereActifPlusSur()
    {
        ConfigurationWhatIf config = ConfigurationSimple();
        config.penaliteRisque = 1f;

        ResultatRechercheFaisceauWhatIf resultat =
            MoteurRechercheFaisceauWhatIf.Rechercher(
                100000,
                new Dictionary<string, int> { { "cash", 100 } },
                new List<PrevisionActifWhatIf>
                {
                    Prevision("risque", 5f, 80f, 0f, 1f),
                    Prevision("sur", 5f, 1f, 0f, 1f)
                },
                config,
                0);

        Assert.That(
            Pourcentage(resultat.decisionRetenue, "sur"),
            Is.EqualTo(100));
        Assert.That(
            Pourcentage(resultat.decisionRetenue, "risque"),
            Is.Zero);
    }

    [Test]
    public void CoutReallocation_PeutConserverLeCash()
    {
        ConfigurationWhatIf config = ConfigurationSimple();
        config.coutTransactionCentimes = 5000;

        ResultatRechercheFaisceauWhatIf resultat =
            MoteurRechercheFaisceauWhatIf.Rechercher(
                100000,
                new Dictionary<string, int> { { "cash", 100 } },
                new List<PrevisionActifWhatIf>
                {
                    Prevision("faible_gain", 1f, 0f, 0f, 1f)
                },
                config,
                0);

        Assert.That(
            Pourcentage(resultat.decisionRetenue, "cash"),
            Is.EqualTo(100));
    }

    [Test]
    public void HorizonDeuxMois_ProduitCapitalComposeExact()
    {
        ConfigurationWhatIf config = ConfigurationSimple();
        config.horizonMois = 2;

        ResultatRechercheFaisceauWhatIf resultat =
            MoteurRechercheFaisceauWhatIf.Rechercher(
                10000,
                new Dictionary<string, int> { { "cash", 100 } },
                new List<PrevisionActifWhatIf>
                {
                    Prevision("croissance", 10f, 0f, 0f, 1f)
                },
                config,
                0);

        Assert.That(resultat.patrimoineProjeteCentimes, Is.EqualTo(12100));
        Assert.That(resultat.chemin.Count, Is.EqualTo(2));
    }

    [Test]
    public void Recherche_EstDeterministeEtOrdreActifsIndifferent()
    {
        ConfigurationWhatIf config = ConfigurationSimple();
        config.horizonMois = 3;
        config.largeurFaisceau = 4;

        List<PrevisionActifWhatIf> ordreUn =
            new List<PrevisionActifWhatIf>
            {
                Prevision("b", 4f, 2f, 1f, 1f),
                Prevision("a", 4f, 2f, 1f, 1f)
            };
        List<PrevisionActifWhatIf> ordreDeux =
            new List<PrevisionActifWhatIf>
            {
                Prevision("a", 4f, 2f, 1f, 1f),
                Prevision("b", 4f, 2f, 1f, 1f)
            };

        ResultatRechercheFaisceauWhatIf premier =
            MoteurRechercheFaisceauWhatIf.Rechercher(
                100000,
                null,
                ordreUn,
                config,
                0);
        ResultatRechercheFaisceauWhatIf second =
            MoteurRechercheFaisceauWhatIf.Rechercher(
                100000,
                null,
                ordreDeux,
                config,
                0);

        Assert.That(
            premier.decisionRetenue.strategieId,
            Is.EqualTo(second.decisionRetenue.strategieId));
        Assert.That(
            premier.patrimoineProjeteCentimes,
            Is.EqualTo(second.patrimoineProjeteCentimes));
        Assert.That(
            premier.noeudsEvalues,
            Is.EqualTo(second.noeudsEvalues));
    }

    [Test]
    public void Recherche_RespecteLaLargeurDuFaisceau()
    {
        ConfigurationWhatIf config = ConfigurationSimple();
        config.horizonMois = 3;
        config.largeurFaisceau = 3;

        ResultatRechercheFaisceauWhatIf resultat =
            MoteurRechercheFaisceauWhatIf.Rechercher(
                100000,
                null,
                new List<PrevisionActifWhatIf>
                {
                    Prevision("a", 2f, 1f, 1f, 1f),
                    Prevision("b", 3f, 2f, 2f, 1f)
                },
                config,
                0);

        Assert.That(resultat.largeurMaxObservee, Is.LessThanOrEqualTo(3));
        Assert.That(resultat.noeudsEvalues, Is.GreaterThan(0));
    }

    [Test]
    public void Recherche_NeModifiePasLesEntrees()
    {
        ConfigurationWhatIf config = ConfigurationSimple();
        Dictionary<string, int> allocation =
            new Dictionary<string, int>
            {
                { "cash", 50 },
                { "a", 50 }
            };
        PrevisionActifWhatIf prevision =
            Prevision("a", 5f, 2f, 1f, 0.8f);
        List<PrevisionActifWhatIf> previsions =
            new List<PrevisionActifWhatIf> { prevision };

        MoteurRechercheFaisceauWhatIf.Rechercher(
            100000,
            allocation,
            previsions,
            config,
            0);

        Assert.That(allocation["cash"], Is.EqualTo(50));
        Assert.That(allocation["a"], Is.EqualTo(50));
        Assert.That(prevision.rendementMensuelEstimePourcent, Is.EqualTo(5f));
        Assert.That(prevision.confiance01, Is.EqualTo(0.8f));
        Assert.That(config.horizonMois, Is.EqualTo(1));
    }

    [Test]
    public void ConfianceNulle_NePrometAucunRendement()
    {
        ConfigurationWhatIf config = ConfigurationSimple();

        ResultatRechercheFaisceauWhatIf resultat =
            MoteurRechercheFaisceauWhatIf.Rechercher(
                100000,
                null,
                new List<PrevisionActifWhatIf>
                {
                    Prevision("incertain", 20f, 0f, 0f, 0f)
                },
                config,
                0);

        Assert.That(resultat.patrimoineProjeteCentimes, Is.EqualTo(100000));
    }

    [Test]
    public void AllocationInitialeSuperieureACent_EstNormaliseeSansErreur()
    {
        ConfigurationWhatIf config = ConfigurationSimple();

        ResultatRechercheFaisceauWhatIf resultat =
            MoteurRechercheFaisceauWhatIf.Rechercher(
                100000,
                new Dictionary<string, int>
                {
                    { "a", 80 },
                    { "b", 80 }
                },
                new List<PrevisionActifWhatIf>
                {
                    Prevision("a", 1f, 0f, 0f, 1f),
                    Prevision("b", 1f, 0f, 0f, 1f)
                },
                config,
                0);

        Assert.That(resultat.decisionRetenue, Is.Not.Null);
        Assert.That(
            SommePourcentages(resultat.decisionRetenue),
            Is.EqualTo(100));
    }

    private static ConfigurationWhatIf ConfigurationSimple()
    {
        return new ConfigurationWhatIf
        {
            horizonMois = 1,
            largeurFaisceau = 25,
            pasAllocationPourcent = 25,
            penaliteRisque = 0f,
            penaliteDrawdown = 0f,
            coutTransactionCentimes = 0
        };
    }

    private static PrevisionActifWhatIf Prevision(
        string id,
        float rendement,
        float risque,
        float drawdown,
        float confiance)
    {
        return new PrevisionActifWhatIf
        {
            actifId = id,
            rendementMensuelEstimePourcent = rendement,
            risqueEstimePourcent = risque,
            drawdownHistoriquePourcent = drawdown,
            confiance01 = confiance
        };
    }

    private static int Pourcentage(
        DecisionWhatIf decision,
        string actifId)
    {
        if (decision == null || decision.allocations == null)
        {
            return 0;
        }

        foreach (AllocationActifWhatIf allocation in decision.allocations)
        {
            if (allocation != null && allocation.actifId == actifId)
            {
                return allocation.pourcentage;
            }
        }

        return 0;
    }

    private static int SommePourcentages(DecisionWhatIf decision)
    {
        int total = 0;
        foreach (AllocationActifWhatIf allocation in decision.allocations)
        {
            if (allocation != null)
            {
                total += allocation.pourcentage;
            }
        }

        return total;
    }

    private static bool ContientAllocation(
        IReadOnlyList<List<AllocationActifWhatIf>> allocations,
        string premierId,
        int premierPourcentage,
        string secondId,
        int secondPourcentage)
    {
        foreach (List<AllocationActifWhatIf> allocation in allocations)
        {
            int premier = 0;
            int second = 0;
            foreach (AllocationActifWhatIf ligne in allocation)
            {
                if (ligne.actifId == premierId)
                {
                    premier = ligne.pourcentage;
                }
                else if (ligne.actifId == secondId)
                {
                    second = ligne.pourcentage;
                }
            }

            if (premier == premierPourcentage &&
                second == secondPourcentage)
            {
                return true;
            }
        }

        return false;
    }
}