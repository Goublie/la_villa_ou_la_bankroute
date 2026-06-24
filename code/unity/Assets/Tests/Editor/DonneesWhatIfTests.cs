using NUnit.Framework;

public class DonneesWhatIfTests
{
    [Test]
    public void Copier_CreeUneCopieProfondeEtIndependante()
    {
        DonneesWhatIf original = new DonneesWhatIf
        {
            initialisee = true,
            moisInitialisation = 2,
            dernierMoisTraite = 4,
            capitalInitialCentimes = 100000,
            liquiditesCentimes = 30000
        };

        original.configuration.horizonMois = 3;
        original.portefeuille.positions.Add(
            new PositionBourse("nvidia")
            {
                quantite = 2f,
                coutTotalCentimes = 70000
            });

        DecisionWhatIf decision = new DecisionWhatIf
        {
            indexMois = 4,
            strategieId = "equilibree",
            explication = "Diversification test.",
            score = 123.45
        };
        decision.allocations.Add(
            new AllocationActifWhatIf("nvidia", 50));
        decision.allocations.Add(
            new AllocationActifWhatIf("cac40", 50));
        decision.evenementsConnusIds.Add("evt_test");
        original.decisions.Add(decision);

        original.historique.Add(
            new PointHistoriqueWhatIf
            {
                indexMois = 4,
                moisCalendrier = Mois.Novembre,
                patrimoineReelCentimes = 99000,
                patrimoineAlternatifCentimes = 105000
            });

        DonneesWhatIf copie = original.Copier();

        copie.liquiditesCentimes = 1;
        copie.configuration.horizonMois = 9;
        copie.portefeuille.positions[0].quantite = 99f;
        copie.decisions[0].allocations[0].pourcentage = 100;
        copie.decisions[0].evenementsConnusIds[0] = "evt_modifie";
        copie.historique[0].patrimoineAlternatifCentimes = 1;

        Assert.That(original.liquiditesCentimes, Is.EqualTo(30000));
        Assert.That(original.configuration.horizonMois, Is.EqualTo(3));
        Assert.That(
            original.portefeuille.positions[0].quantite,
            Is.EqualTo(2f));
        Assert.That(
            original.decisions[0].allocations[0].pourcentage,
            Is.EqualTo(50));
        Assert.That(
            original.decisions[0].evenementsConnusIds[0],
            Is.EqualTo("evt_test"));
        Assert.That(
            original.historique[0].patrimoineAlternatifCentimes,
            Is.EqualTo(105000));
    }

    [Test]
    public void InitialiserSiNecessaire_RepareToutesLesCollections()
    {
        DonneesWhatIf donnees = new DonneesWhatIf
        {
            portefeuille = null,
            configuration = null,
            decisions = null,
            historique = null,
            capitalInitialCentimes = -10,
            liquiditesCentimes = -20
        };

        donnees.InitialiserSiNecessaire();

        Assert.That(donnees.portefeuille, Is.Not.Null);
        Assert.That(donnees.configuration, Is.Not.Null);
        Assert.That(donnees.decisions, Is.Not.Null);
        Assert.That(donnees.historique, Is.Not.Null);
        Assert.That(donnees.capitalInitialCentimes, Is.Zero);
        Assert.That(donnees.liquiditesCentimes, Is.Zero);
    }

    [Test]
    public void Configuration_Normaliser_BorneLesValeursDangereuses()
    {
        ConfigurationWhatIf configuration = new ConfigurationWhatIf
        {
            horizonMois = 0,
            largeurFaisceau = 1,
            pasAllocationPourcent = 500,
            penaliteRisque = -2f,
            penaliteDrawdown = -3f,
            coutTransactionCentimes = -50
        };

        configuration.Normaliser();

        Assert.That(configuration.horizonMois, Is.EqualTo(1));
        Assert.That(configuration.largeurFaisceau, Is.EqualTo(2));
        Assert.That(configuration.pasAllocationPourcent, Is.EqualTo(100));
        Assert.That(configuration.penaliteRisque, Is.Zero);
        Assert.That(configuration.penaliteDrawdown, Is.Zero);
        Assert.That(configuration.coutTransactionCentimes, Is.Zero);
    }
}
