using System.Collections.Generic;
using NUnit.Framework;

public class ServiceOrchestrationMensuelleWhatIfTests
{
    [Test]
    public void Ouvrir_EnchaineRechercheEtReallocation()
    {
        DonneesWhatIf donnees = ConfigurationTest();

        ResultatOuvertureMoisWhatIf resultat =
            ServiceOrchestrationMensuelleWhatIf.OuvrirDepuisPrevisions(
                donnees,
                100000,
                new List<PrevisionActifWhatIf>
                {
                    Prevision("croissance", 10f, 0f, 1f)
                },
                null,
                Prix(("croissance", 10000)),
                0);

        Assert.That(resultat.succes, Is.True);
        Assert.That(resultat.recherche.decisionRetenue, Is.Not.Null);
        Assert.That(donnees.portefeuille.positions.Count, Is.EqualTo(1));
        Assert.That(
            donnees.portefeuille.positions[0].actifId,
            Is.EqualTo("croissance"));
    }

    [Test]
    public void Ouvrir_RendementNegatifConserveLeCash()
    {
        DonneesWhatIf donnees = ConfigurationTest();

        ResultatOuvertureMoisWhatIf resultat =
            ServiceOrchestrationMensuelleWhatIf.OuvrirDepuisPrevisions(
                donnees,
                100000,
                new List<PrevisionActifWhatIf>
                {
                    Prevision("baisse", -10f, 0f, 1f)
                },
                null,
                Prix(("baisse", 10000)),
                0);

        Assert.That(resultat.succes, Is.True);
        Assert.That(donnees.liquiditesCentimes, Is.EqualTo(100000));
        Assert.That(donnees.portefeuille.positions, Is.Empty);
    }

    [Test]
    public void Ouvrir_SansPrevisionEchoueProprement()
    {
        DonneesWhatIf donnees = ConfigurationTest();

        ResultatOuvertureMoisWhatIf resultat =
            ServiceOrchestrationMensuelleWhatIf.OuvrirDepuisPrevisions(
                donnees,
                100000,
                new List<PrevisionActifWhatIf>(),
                null,
                new Dictionary<string, int>(),
                0);

        Assert.That(resultat.succes, Is.False);
        Assert.That(resultat.diagnostic, Does.Contain("prevision"));
        Assert.That(donnees.initialisee, Is.True);
    }

    [Test]
    public void Ouvrir_CopieLesImpactsConnus()
    {
        DonneesWhatIf donnees = ConfigurationTest();
        ImpactEvenementMarche impact = new ImpactEvenementMarche
        {
            evenementId = "evt",
            actifId = "a",
            moisDebut = 0,
            dureeMois = 3,
            tendanceMensuellePourcent = 2f
        };

        ResultatOuvertureMoisWhatIf resultat =
            ServiceOrchestrationMensuelleWhatIf.OuvrirDepuisPrevisions(
                donnees,
                100000,
                new List<PrevisionActifWhatIf>
                {
                    Prevision("a", 5f, 0f, 1f)
                },
                new List<ImpactEvenementMarche> { impact },
                Prix(("a", 10000)),
                0);

        Assert.That(resultat.succes, Is.True);
        Assert.That(donnees.portefeuille.impactsMarche.Count, Is.EqualTo(1));
        Assert.That(
            donnees.portefeuille.impactsMarche[0],
            Is.Not.SameAs(impact));
        Assert.That(
            resultat.recherche.decisionRetenue.evenementsConnusIds,
            Does.Contain("evt"));
    }

    [Test]
    public void Ouvrir_IgnoreImpactFuturDansLaDecision()
    {
        DonneesWhatIf donnees = ConfigurationTest();
        ImpactEvenementMarche futur = new ImpactEvenementMarche
        {
            evenementId = "futur",
            actifId = "a",
            moisDebut = 4,
            dureeMois = 2
        };

        ResultatOuvertureMoisWhatIf resultat =
            ServiceOrchestrationMensuelleWhatIf.OuvrirDepuisPrevisions(
                donnees,
                100000,
                new List<PrevisionActifWhatIf>
                {
                    Prevision("a", 5f, 0f, 1f)
                },
                new List<ImpactEvenementMarche> { futur },
                Prix(("a", 10000)),
                0);

        Assert.That(
            resultat.recherche.decisionRetenue.evenementsConnusIds,
            Does.Not.Contain("futur"));
    }

    [Test]
    public void Ouvrir_RemplaceLaDecisionDuMemeMois()
    {
        DonneesWhatIf donnees = ConfigurationTest();
        List<PrevisionActifWhatIf> previsions =
            new List<PrevisionActifWhatIf>
            {
                Prevision("a", 5f, 0f, 1f)
            };

        ServiceOrchestrationMensuelleWhatIf.OuvrirDepuisPrevisions(
            donnees, 100000, previsions, null, Prix(("a", 10000)), 0);
        ServiceOrchestrationMensuelleWhatIf.OuvrirDepuisPrevisions(
            donnees, 100000, previsions, null, Prix(("a", 10000)), 0);

        Assert.That(donnees.decisions.Count, Is.EqualTo(1));
    }

    [Test]
    public void Ouvrir_UtiliseLaValeurCouranteDuPortefeuille()
    {
        DonneesWhatIf donnees = ConfigurationTest();
        ServicePortefeuilleAlternatifWhatIf.Initialiser(
            donnees,
            100000,
            0);
        donnees.liquiditesCentimes = 0;
        PositionBourse position = new PositionBourse("a");
        position.AjouterAchat(10f, 100000);
        donnees.portefeuille.positions.Add(position);

        ResultatOuvertureMoisWhatIf resultat =
            ServiceOrchestrationMensuelleWhatIf.OuvrirDepuisPrevisions(
                donnees,
                100000,
                new List<PrevisionActifWhatIf>
                {
                    Prevision("a", 5f, 0f, 1f)
                },
                null,
                Prix(("a", 12000)),
                1);

        Assert.That(
            resultat.capitalAvantDecisionCentimes,
            Is.EqualTo(120000));
    }

    [Test]
    public void Ouvrir_EstDeterministe()
    {
        ResultatOuvertureMoisWhatIf premier =
            ServiceOrchestrationMensuelleWhatIf.OuvrirDepuisPrevisions(
                ConfigurationTest(),
                100000,
                new List<PrevisionActifWhatIf>
                {
                    Prevision("b", 4f, 1f, 1f),
                    Prevision("a", 4f, 1f, 1f)
                },
                null,
                Prix(("a", 10000), ("b", 10000)),
                0);

        ResultatOuvertureMoisWhatIf second =
            ServiceOrchestrationMensuelleWhatIf.OuvrirDepuisPrevisions(
                ConfigurationTest(),
                100000,
                new List<PrevisionActifWhatIf>
                {
                    Prevision("a", 4f, 1f, 1f),
                    Prevision("b", 4f, 1f, 1f)
                },
                null,
                Prix(("a", 10000), ("b", 10000)),
                0);

        Assert.That(
            premier.recherche.decisionRetenue.strategieId,
            Is.EqualTo(second.recherche.decisionRetenue.strategieId));
    }

    [Test]
    public void Ouvrir_NeModifiePasLesEntrees()
    {
        DonneesWhatIf donnees = ConfigurationTest();
        PrevisionActifWhatIf prevision = Prevision("a", 5f, 1f, 0.8f);
        ImpactEvenementMarche impact = new ImpactEvenementMarche
        {
            evenementId = "evt",
            actifId = "a",
            moisDebut = 0,
            tendanceMensuellePourcent = 3f
        };
        Dictionary<string, int> prix = Prix(("a", 10000));

        ServiceOrchestrationMensuelleWhatIf.OuvrirDepuisPrevisions(
            donnees,
            100000,
            new List<PrevisionActifWhatIf> { prevision },
            new List<ImpactEvenementMarche> { impact },
            prix,
            0);

        Assert.That(prevision.rendementMensuelEstimePourcent, Is.EqualTo(5f));
        Assert.That(impact.tendanceMensuellePourcent, Is.EqualTo(3f));
        Assert.That(prix["a"], Is.EqualTo(10000));
    }

    [Test]
    public void Ouvrir_PrixAbsentConserveLeCapitalEnCash()
    {
        DonneesWhatIf donnees = ConfigurationTest();

        ResultatOuvertureMoisWhatIf resultat =
            ServiceOrchestrationMensuelleWhatIf.OuvrirDepuisPrevisions(
                donnees,
                100000,
                new List<PrevisionActifWhatIf>
                {
                    Prevision("a", 10f, 0f, 1f)
                },
                null,
                new Dictionary<string, int>(),
                0);

        Assert.That(resultat.succes, Is.True);
        Assert.That(donnees.liquiditesCentimes, Is.EqualTo(100000));
        Assert.That(donnees.portefeuille.positions, Is.Empty);
    }

    [Test]
    public void Cloturer_EnregistreLePointHistorique()
    {
        DonneesWhatIf donnees = ConfigurationTest();
        ServiceOrchestrationMensuelleWhatIf.OuvrirDepuisPrevisions(
            donnees,
            100000,
            new List<PrevisionActifWhatIf>
            {
                Prevision("a", 10f, 0f, 1f)
            },
            null,
            Prix(("a", 10000)),
            0);

        PointHistoriqueWhatIf point =
            ServiceOrchestrationMensuelleWhatIf.CloturerMois(
                donnees,
                0,
                Mois.Juillet,
                Prix(("a", 11000)),
                105000);

        Assert.That(donnees.historique.Count, Is.EqualTo(1));
        Assert.That(point.patrimoineAlternatifCentimes, Is.EqualTo(110000));
        Assert.That(point.ecartCumuleCentimes, Is.EqualTo(5000));
    }

    [Test]
    public void Ouvrir_DiagnosticResumeLeTraitement()
    {
        ResultatOuvertureMoisWhatIf resultat =
            ServiceOrchestrationMensuelleWhatIf.OuvrirDepuisPrevisions(
                ConfigurationTest(),
                100000,
                new List<PrevisionActifWhatIf>
                {
                    Prevision("a", 5f, 0f, 1f)
                },
                null,
                Prix(("a", 10000)),
                2);

        Assert.That(resultat.diagnostic, Does.Contain("Mois 2"));
        Assert.That(resultat.diagnostic, Does.Contain("previsions"));
        Assert.That(resultat.diagnostic, Does.Contain("noeuds"));
    }

    private static DonneesWhatIf ConfigurationTest()
    {
        DonneesWhatIf donnees = new DonneesWhatIf();
        donnees.configuration.horizonMois = 1;
        donnees.configuration.largeurFaisceau = 10;
        donnees.configuration.pasAllocationPourcent = 25;
        donnees.configuration.penaliteRisque = 0f;
        donnees.configuration.penaliteDrawdown = 0f;
        donnees.configuration.coutTransactionCentimes = 0;
        return donnees;
    }

    private static PrevisionActifWhatIf Prevision(
        string id,
        float rendement,
        float risque,
        float confiance)
    {
        return new PrevisionActifWhatIf
        {
            actifId = id,
            rendementMensuelEstimePourcent = rendement,
            risqueEstimePourcent = risque,
            confiance01 = confiance
        };
    }

    private static Dictionary<string, int> Prix(
        params (string id, int prix)[] valeurs)
    {
        Dictionary<string, int> resultat =
            new Dictionary<string, int>();
        foreach ((string id, int prix) valeur in valeurs)
        {
            resultat[valeur.id] = valeur.prix;
        }

        return resultat;
    }
}