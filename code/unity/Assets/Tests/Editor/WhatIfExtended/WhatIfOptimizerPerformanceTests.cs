using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

public class WhatIfOptimizerPerformanceTests
{
    [Test]
    [Timeout(30000)]
    public void Optimiseur_DixProfilsStandards_RestentSousLeBudgetTemps()
    {
        const int repetitions = 10;
        const long budgetMaximumMillisecondes = 15000;

        ConfigurationWhatIf configuration =
            new ConfigurationWhatIf
            {
                horizonMois = 6,
                largeurFaisceau = 50,
                pasAllocationPourcent = 20,
                penaliteRisque = 0.15f,
                penaliteDrawdown = 0.25f
            };
        List<PrevisionActifWhatIf> previsions =
            CreerPrevisionsDiversifiees();

        Stopwatch chronometre = Stopwatch.StartNew();

        for (int index = 0; index < repetitions; index++)
        {
            ResultatRechercheFaisceauWhatIf resultat =
                ServiceOptimisationMultiHorizonWhatIf.Rechercher(
                    1000000 + index * 100000,
                    new Dictionary<string, int>
                    {
                        { "cash", 100 }
                    },
                    previsions,
                    configuration,
                    index);

            VerifierResultat(resultat);
        }

        chronometre.Stop();
        TestContext.WriteLine(
            "10 optimisations standards : " +
            chronometre.ElapsedMilliseconds +
            " ms.");

        Assert.That(
            chronometre.ElapsedMilliseconds,
            Is.LessThan(budgetMaximumMillisecondes));
    }

    [Test]
    [Timeout(30000)]
    public void Optimiseur_ProfilCharge_ResteSousLeBudgetTemps()
    {
        const long budgetMaximumMillisecondes = 15000;

        ConfigurationWhatIf configuration =
            new ConfigurationWhatIf
            {
                horizonMois = 9,
                largeurFaisceau = 100,
                pasAllocationPourcent = 20,
                penaliteRisque = 0.25f,
                penaliteDrawdown = 0.35f,
                coutTransactionCentimes = 100
            };

        Stopwatch chronometre = Stopwatch.StartNew();
        ResultatRechercheFaisceauWhatIf resultat =
            ServiceOptimisationMultiHorizonWhatIf.Rechercher(
                100000000,
                new Dictionary<string, int>
                {
                    { "cash", 20 },
                    { "croissance", 40 },
                    { "defensif", 40 }
                },
                CreerPrevisionsDiversifiees(),
                configuration,
                120);
        chronometre.Stop();

        VerifierResultat(resultat);
        TestContext.WriteLine(
            "Optimisation chargee : " +
            chronometre.ElapsedMilliseconds +
            " ms pour " +
            resultat.noeudsEvalues +
            " noeuds.");

        Assert.That(
            chronometre.ElapsedMilliseconds,
            Is.LessThan(budgetMaximumMillisecondes));
    }

    [Test]
    [Timeout(30000)]
    public void Optimiseur_MatriceDeConditions_NeProduitAucuneDecisionInvalide()
    {
        int executions = 0;
        Stopwatch chronometre = Stopwatch.StartNew();

        int[] capitaux =
        {
            10000,
            1000000,
            100000000
        };
        float[] penalites =
        {
            0f,
            0.15f,
            0.5f
        };

        foreach (int capital in capitaux)
        {
            foreach (float penalite in penalites)
            {
                ConfigurationWhatIf configuration =
                    new ConfigurationWhatIf
                    {
                        horizonMois = 6,
                        largeurFaisceau = 50,
                        pasAllocationPourcent = 20,
                        penaliteRisque = penalite,
                        penaliteDrawdown = penalite
                    };

                ResultatRechercheFaisceauWhatIf resultat =
                    ServiceOptimisationMultiHorizonWhatIf.Rechercher(
                        capital,
                        new Dictionary<string, int>
                        {
                            { "cash", 100 }
                        },
                        CreerPrevisionsDiversifiees(),
                        configuration,
                        executions);

                VerifierResultat(resultat);
                executions++;
            }
        }

        chronometre.Stop();
        TestContext.WriteLine(
            executions +
            " conditions capital/risque : " +
            chronometre.ElapsedMilliseconds +
            " ms.");

        Assert.That(executions, Is.EqualTo(9));
        Assert.That(
            chronometre.ElapsedMilliseconds,
            Is.LessThan(20000));
    }

    private static List<PrevisionActifWhatIf>
        CreerPrevisionsDiversifiees()
    {
        return new List<PrevisionActifWhatIf>
        {
            CreerPrevision("croissance", 2.5f, 1.8f, 5f),
            CreerPrevision("defensif", 0.8f, 0.4f, 1f),
            CreerPrevision("cyclique", 1.5f, 2.5f, 8f),
            CreerPrevision("baissier", -1.2f, 1.5f, 6f),
            CreerPrevision("stable", 0.3f, 0.1f, 0.5f)
        };
    }

    private static PrevisionActifWhatIf CreerPrevision(
        string actifId,
        float rendement,
        float risque,
        float drawdown)
    {
        return new PrevisionActifWhatIf
        {
            actifId = actifId,
            rendementMensuelEstimePourcent = rendement,
            volatiliteMensuellePourcent = risque,
            risqueEstimePourcent = risque,
            drawdownHistoriquePourcent = drawdown,
            confiance01 = 1f
        };
    }

    private static void VerifierResultat(
        ResultatRechercheFaisceauWhatIf resultat)
    {
        Assert.That(resultat, Is.Not.Null);
        Assert.That(resultat.decisionRetenue, Is.Not.Null);
        Assert.That(resultat.noeudsEvalues, Is.GreaterThan(0));
        Assert.That(
            resultat.patrimoineProjeteCentimes,
            Is.GreaterThanOrEqualTo(0));

        int somme = 0;
        foreach (
            AllocationActifWhatIf allocation
            in resultat.decisionRetenue.allocations)
        {
            Assert.That(allocation, Is.Not.Null);
            Assert.That(allocation.pourcentage, Is.InRange(0, 100));
            somme += allocation.pourcentage;
        }

        Assert.That(somme, Is.EqualTo(100));
    }
}
