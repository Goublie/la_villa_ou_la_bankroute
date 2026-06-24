using System.Collections.Generic;
using NUnit.Framework;

public class ServicePortefeuilleAlternatifWhatIfTests
{
    [Test]
    public void Initialiser_CreeLeCapitalUneSeuleFois()
    {
        DonneesWhatIf donnees = new DonneesWhatIf();

        ServicePortefeuilleAlternatifWhatIf.Initialiser(
            donnees,
            100000,
            2);
        ServicePortefeuilleAlternatifWhatIf.Initialiser(
            donnees,
            500000,
            9);

        Assert.That(donnees.initialisee, Is.True);
        Assert.That(donnees.capitalInitialCentimes, Is.EqualTo(100000));
        Assert.That(donnees.liquiditesCentimes, Is.EqualTo(100000));
        Assert.That(donnees.moisInitialisation, Is.EqualTo(2));
    }

    [Test]
    public void Reallouer_InvestitToutDansUnActif()
    {
        DonneesWhatIf donnees = EtatInitial(100000);
        DecisionWhatIf decision = Decision(
            0,
            new AllocationActifWhatIf("a", 100));

        ResultatReallocationWhatIf resultat =
            ServicePortefeuilleAlternatifWhatIf.Reallouer(
                donnees,
                decision,
                Prix(("a", 10000)),
                0);

        Assert.That(resultat.succes, Is.True);
        Assert.That(resultat.patrimoineApresCentimes, Is.EqualTo(100000));
        Assert.That(donnees.liquiditesCentimes, Is.Zero);
        Assert.That(donnees.portefeuille.positions.Count, Is.EqualTo(1));
        Assert.That(
            donnees.portefeuille.positions[0].quantite,
            Is.EqualTo(10f).Within(0.0001f));
    }

    [Test]
    public void Reallouer_RespecteDeuxActifsEtLeCash()
    {
        DonneesWhatIf donnees = EtatInitial(100000);
        DecisionWhatIf decision = Decision(
            0,
            new AllocationActifWhatIf("a", 25),
            new AllocationActifWhatIf("b", 50),
            new AllocationActifWhatIf("cash", 25));

        ResultatReallocationWhatIf resultat =
            ServicePortefeuilleAlternatifWhatIf.Reallouer(
                donnees,
                decision,
                Prix(("a", 1000), ("b", 2000)),
                0);

        Assert.That(resultat.valeurBourseCentimes, Is.EqualTo(75000));
        Assert.That(resultat.liquiditesCentimes, Is.EqualTo(25000));
        Assert.That(resultat.ordresExecutes, Is.EqualTo(2));
    }

    [Test]
    public void Reallouer_MontantSousMinimumResteEnLiquidites()
    {
        DonneesWhatIf donnees = EtatInitial(1000);
        DecisionWhatIf decision = Decision(
            0,
            new AllocationActifWhatIf("a", 25),
            new AllocationActifWhatIf("cash", 75));

        ResultatReallocationWhatIf resultat =
            ServicePortefeuilleAlternatifWhatIf.Reallouer(
                donnees,
                decision,
                Prix(("a", 100)),
                0);

        Assert.That(resultat.ordresExecutes, Is.Zero);
        Assert.That(resultat.liquiditesCentimes, Is.EqualTo(1000));
        Assert.That(resultat.diagnostics, Is.Not.Empty);
    }

    [Test]
    public void Reallouer_PrixAbsentResteEnLiquidites()
    {
        DonneesWhatIf donnees = EtatInitial(100000);
        DecisionWhatIf decision = Decision(
            0,
            new AllocationActifWhatIf("inconnu", 100));

        ResultatReallocationWhatIf resultat =
            ServicePortefeuilleAlternatifWhatIf.Reallouer(
                donnees,
                decision,
                new Dictionary<string, int>(),
                0);

        Assert.That(resultat.ordresExecutes, Is.Zero);
        Assert.That(resultat.liquiditesCentimes, Is.EqualTo(100000));
        Assert.That(resultat.diagnostics[0], Does.Contain("prix"));
    }

    [Test]
    public void Reallouer_UnTourCompletDeduitLeCoutComplet()
    {
        DonneesWhatIf donnees = EtatInitial(100000);
        donnees.configuration.coutTransactionCentimes = 2000;
        DecisionWhatIf decision = Decision(
            0,
            new AllocationActifWhatIf("a", 100));

        ResultatReallocationWhatIf resultat =
            ServicePortefeuilleAlternatifWhatIf.Reallouer(
                donnees,
                decision,
                Prix(("a", 1000)),
                0);

        Assert.That(resultat.coutTransactionCentimes, Is.EqualTo(2000));
        Assert.That(resultat.patrimoineApresCentimes, Is.EqualTo(98000));
    }

    [Test]
    public void Reallouer_MemeAllocationNeDeduitAucunCout()
    {
        DonneesWhatIf donnees = EtatInitial(100000);
        donnees.configuration.coutTransactionCentimes = 2000;
        donnees.liquiditesCentimes = 0;
        donnees.portefeuille.positions.Add(
            Position("a", 10f, 100000));

        ResultatReallocationWhatIf resultat =
            ServicePortefeuilleAlternatifWhatIf.Reallouer(
                donnees,
                Decision(
                    1,
                    new AllocationActifWhatIf("a", 100)),
                Prix(("a", 10000)),
                1);

        Assert.That(resultat.coutTransactionCentimes, Is.Zero);
        Assert.That(resultat.patrimoineApresCentimes, Is.EqualTo(100000));
    }

    [Test]
    public void CloturerMois_AppliqueLePrixObserve()
    {
        DonneesWhatIf donnees = EtatInitial(100000);
        ServicePortefeuilleAlternatifWhatIf.Reallouer(
            donnees,
            Decision(
                0,
                new AllocationActifWhatIf("a", 100)),
            Prix(("a", 10000)),
            0);

        PointHistoriqueWhatIf point =
            ServicePortefeuilleAlternatifWhatIf.CloturerMois(
                donnees,
                1,
                Mois.Aout,
                Prix(("a", 12000)),
                105000);

        Assert.That(point.valeurBourseAlternativeCentimes, Is.EqualTo(120000));
        Assert.That(point.patrimoineAlternatifCentimes, Is.EqualTo(120000));
        Assert.That(point.ecartCumuleCentimes, Is.EqualTo(15000));
        Assert.That(donnees.dernierMoisTraite, Is.EqualTo(1));
    }

    [Test]
    public void CloturerMois_RemplaceLeMemeMoisSansDoublon()
    {
        DonneesWhatIf donnees = EtatInitial(100000);

        ServicePortefeuilleAlternatifWhatIf.CloturerMois(
            donnees,
            0,
            Mois.Juillet,
            new Dictionary<string, int>(),
            100000);
        ServicePortefeuilleAlternatifWhatIf.CloturerMois(
            donnees,
            0,
            Mois.Juillet,
            new Dictionary<string, int>(),
            90000);

        Assert.That(donnees.historique.Count, Is.EqualTo(1));
        Assert.That(
            donnees.historique[0].patrimoineReelCentimes,
            Is.EqualTo(90000));
    }

    [Test]
    public void ConstruireAllocationCourante_SommeToujoursCent()
    {
        DonneesWhatIf donnees = EtatInitial(100000);
        donnees.liquiditesCentimes = 25000;
        donnees.portefeuille.positions.Add(
            Position("a", 5f, 25000));
        donnees.portefeuille.positions.Add(
            Position("b", 10f, 50000));

        Dictionary<string, int> allocation =
            ServicePortefeuilleAlternatifWhatIf.ConstruireAllocationCourante(
                donnees,
                Prix(("a", 5000), ("b", 5000)));

        int total = 0;
        foreach (int pourcentage in allocation.Values)
        {
            total += pourcentage;
        }

        Assert.That(total, Is.EqualTo(100));
        Assert.That(allocation["cash"], Is.EqualTo(25));
        Assert.That(allocation["a"], Is.EqualTo(25));
        Assert.That(allocation["b"], Is.EqualTo(50));
    }

    [Test]
    public void Reallouer_NeModifiePasLaDecisionSource()
    {
        DonneesWhatIf donnees = EtatInitial(100000);
        DecisionWhatIf decision = Decision(
            7,
            new AllocationActifWhatIf("a", 100));

        ServicePortefeuilleAlternatifWhatIf.Reallouer(
            donnees,
            decision,
            Prix(("a", 1000)),
            3);

        Assert.That(decision.indexMois, Is.EqualTo(7));
        Assert.That(decision.allocations[0].pourcentage, Is.EqualTo(100));
        Assert.That(donnees.decisions[0], Is.Not.SameAs(decision));
        Assert.That(donnees.decisions[0].indexMois, Is.EqualTo(3));
    }

    [Test]
    public void Reallouer_NeModifiePasLeDictionnaireDePrix()
    {
        DonneesWhatIf donnees = EtatInitial(100000);
        Dictionary<string, int> prix = Prix(("a", 1000));

        ServicePortefeuilleAlternatifWhatIf.Reallouer(
            donnees,
            Decision(
                0,
                new AllocationActifWhatIf("a", 100)),
            prix,
            0);

        Assert.That(prix.Count, Is.EqualTo(1));
        Assert.That(prix["a"], Is.EqualTo(1000));
    }

    [Test]
    public void CloturerMois_RetourneUneCopieDuPointStocke()
    {
        DonneesWhatIf donnees = EtatInitial(100000);

        PointHistoriqueWhatIf retourne =
            ServicePortefeuilleAlternatifWhatIf.CloturerMois(
                donnees,
                0,
                Mois.Juillet,
                new Dictionary<string, int>(),
                100000);
        retourne.patrimoineAlternatifCentimes = 1;

        Assert.That(
            donnees.historique[0].patrimoineAlternatifCentimes,
            Is.EqualTo(100000));
    }

    private static DonneesWhatIf EtatInitial(int capital)
    {
        DonneesWhatIf donnees = new DonneesWhatIf();
        ServicePortefeuilleAlternatifWhatIf.Initialiser(
            donnees,
            capital,
            0);
        return donnees;
    }

    private static DecisionWhatIf Decision(
        int mois,
        params AllocationActifWhatIf[] allocations)
    {
        DecisionWhatIf decision = new DecisionWhatIf
        {
            indexMois = mois,
            strategieId = "test"
        };
        decision.allocations.AddRange(allocations);
        return decision;
    }

    private static PositionBourse Position(
        string id,
        float quantite,
        int cout)
    {
        PositionBourse position = new PositionBourse(id);
        position.AjouterAchat(quantite, cout);
        return position;
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