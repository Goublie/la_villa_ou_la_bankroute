using System.Collections.Generic;
using NUnit.Framework;

public class ServiceOptimisationMultiHorizonWhatIfTests
{
    [Test]
    public void Rechercher_ProduitUneDecisionEtComparePlusieursHorizons()
    {
        ConfigurationWhatIf configuration =
            new ConfigurationWhatIf
            {
                horizonMois = 6,
                largeurFaisceau = 4,
                pasAllocationPourcent = 50,
                penaliteRisque = 0f,
                penaliteDrawdown = 0f
            };

        ResultatRechercheFaisceauWhatIf resultat =
            ServiceOptimisationMultiHorizonWhatIf.Rechercher(
                100000,
                new Dictionary<string, int> { { "cash", 100 } },
                new List<PrevisionActifWhatIf>
                {
                    CreerPrevision("croissance", 5f)
                },
                configuration,
                0);

        Assert.That(resultat, Is.Not.Null);
        Assert.That(resultat.decisionRetenue, Is.Not.Null);
        Assert.That(resultat.noeudsEvalues, Is.GreaterThan(0));
        Assert.That(
            resultat.diagnostic,
            Does.Contain("Optimisation multi-horizon"));
        Assert.That(
            Pourcentage(resultat.decisionRetenue, "croissance"),
            Is.EqualTo(100));
    }

    [Test]
    public void Rechercher_NeModifieJamaisLaConfigurationRecue()
    {
        ConfigurationWhatIf configuration =
            new ConfigurationWhatIf
            {
                horizonMois = 4,
                largeurFaisceau = 7,
                pasAllocationPourcent = 25,
                penaliteRisque = 0.2f,
                penaliteDrawdown = 0.3f,
                coutTransactionCentimes = 15
            };

        ServiceOptimisationMultiHorizonWhatIf.Rechercher(
            100000,
            null,
            new List<PrevisionActifWhatIf>
            {
                CreerPrevision("a", 1f)
            },
            configuration,
            0);

        Assert.That(configuration.horizonMois, Is.EqualTo(4));
        Assert.That(configuration.largeurFaisceau, Is.EqualTo(7));
        Assert.That(
            configuration.pasAllocationPourcent,
            Is.EqualTo(25));
        Assert.That(configuration.penaliteRisque, Is.EqualTo(0.2f));
        Assert.That(configuration.penaliteDrawdown, Is.EqualTo(0.3f));
        Assert.That(configuration.coutTransactionCentimes, Is.EqualTo(15));
    }

    private static PrevisionActifWhatIf CreerPrevision(
        string actifId,
        float rendement)
    {
        return new PrevisionActifWhatIf
        {
            actifId = actifId,
            rendementMensuelEstimePourcent = rendement,
            confiance01 = 1f
        };
    }

    private static int Pourcentage(
        DecisionWhatIf decision,
        string actifId)
    {
        foreach (AllocationActifWhatIf allocation in decision.allocations)
        {
            if (allocation != null && allocation.actifId == actifId)
            {
                return allocation.pourcentage;
            }
        }

        return 0;
    }
}
