using System.Collections.Generic;
using NUnit.Framework;

public class ServiceFluxMensuelsWhatIfTests
{
    [Test]
    public void Analyser_ReproduitSalaireEtDepense()
    {
        ResultatFluxMensuelsWhatIf resultat =
            ServiceFluxMensuelsWhatIf.Analyser(
                new List<Transaction>
                {
                    new Transaction("salaire", 200000),
                    new Transaction("loyer", -80000)
                });

        Assert.That(resultat.revenusCentimes, Is.EqualTo(200000));
        Assert.That(resultat.depensesCentimes, Is.EqualTo(80000));
        Assert.That(resultat.fluxNetCentimes, Is.EqualTo(120000));
        Assert.That(resultat.transactionsReproduites, Is.EqualTo(2));
    }

    [Test]
    public void Analyser_IgnoreLesDeuxFacesDUnTransfertInterne()
    {
        ResultatFluxMensuelsWhatIf resultat =
            ServiceFluxMensuelsWhatIf.Analyser(
                new List<Transaction>
                {
                    new Transaction(
                        "courant vers epargne",
                        -50000),
                    new Transaction("credit", 50000)
                });

        Assert.That(resultat.fluxNetCentimes, Is.Zero);
        Assert.That(resultat.transactionsIgnorees, Is.EqualTo(2));
    }

    [Test]
    public void Analyser_IgnoreBourseEtInterets()
    {
        ResultatFluxMensuelsWhatIf resultat =
            ServiceFluxMensuelsWhatIf.Analyser(
                new List<Transaction>
                {
                    new Transaction("achat nvidia", -100000),
                    new Transaction("vente bitcoin", 75000),
                    new Transaction("interets", 1200)
                });

        Assert.That(resultat.fluxNetCentimes, Is.Zero);
        Assert.That(resultat.transactionsIgnorees, Is.EqualTo(3));
    }

    [Test]
    public void Analyser_ReproduitUneDepenseInconnueParPrudence()
    {
        ResultatFluxMensuelsWhatIf resultat =
            ServiceFluxMensuelsWhatIf.Analyser(
                new[]
                {
                    new Transaction("frais surprise", -3456)
                });

        Assert.That(resultat.depensesCentimes, Is.EqualTo(3456));
        Assert.That(resultat.fluxNetCentimes, Is.EqualTo(-3456));
        Assert.That(
            resultat.classifications[0].classificationCertaine,
            Is.False);
    }

    [Test]
    public void Analyser_ReproduitUneDecisionHorsBourse()
    {
        ResultatFluxMensuelsWhatIf resultat =
            ServiceFluxMensuelsWhatIf.Analyser(
                new[]
                {
                    new Transaction(
                        "developpement du produit",
                        -25000)
                });

        Assert.That(resultat.fluxNetCentimes, Is.EqualTo(-25000));
        Assert.That(
            resultat.classifications[0].type,
            Is.EqualTo(TypeFluxWhatIf.DecisionHorsBourse));
    }

    [Test]
    public void AnalyserJoueur_AgregueTousLesComptes()
    {
        DonneesJoueur joueur = new DonneesJoueur();
        joueur.comptes[ServiceBanque.CompteCourantId]
            .AjoutHistorique("salaire", new argent(100000));

        CompteBanquaire secondaire =
            new CompteBanquaire(new argent(0));
        secondaire.AjoutHistorique(
            "remboursement",
            new argent(5000));
        joueur.comptes["secondaire"] = secondaire;

        ResultatFluxMensuelsWhatIf resultat =
            ServiceFluxMensuelsWhatIf.Analyser(joueur);

        Assert.That(resultat.revenusCentimes, Is.EqualTo(105000));
        Assert.That(resultat.transactionsAnalysees, Is.EqualTo(2));
    }

    [Test]
    public void Analyser_NeModifiePasLesTransactions()
    {
        Transaction transaction =
            new Transaction("  Salaire  ", 12345);
        List<Transaction> transactions =
            new List<Transaction> { transaction };

        ServiceFluxMensuelsWhatIf.Analyser(transactions);

        Assert.That(transaction.libelle, Is.EqualTo("Salaire"));
        Assert.That(transaction.montant.centimes, Is.EqualTo(12345));
        Assert.That(transactions.Count, Is.EqualTo(1));
    }

    [Test]
    public void Appliquer_FluxPositifAjouteDesLiquidites()
    {
        DonneesWhatIf donnees = CreerDonnees(10000, 10000);
        ResultatFluxMensuelsWhatIf analyse =
            CreerAnalyse(2500);

        ResultatFluxMensuelsWhatIf resultat =
            ServiceFluxMensuelsWhatIf.Appliquer(
                donnees,
                analyse,
                new Dictionary<string, int>(),
                1);

        Assert.That(resultat.succes, Is.True);
        Assert.That(donnees.liquiditesCentimes, Is.EqualTo(12500));
        Assert.That(resultat.liquiditesAvantCentimes, Is.EqualTo(10000));
    }

    [Test]
    public void Appliquer_DepenseUtiliseDAbordLesLiquidites()
    {
        DonneesWhatIf donnees = CreerDonnees(10000, 8000);
        ResultatFluxMensuelsWhatIf analyse =
            CreerAnalyse(-3000);

        ResultatFluxMensuelsWhatIf resultat =
            ServiceFluxMensuelsWhatIf.Appliquer(
                donnees,
                analyse,
                new Dictionary<string, int>(),
                1);

        Assert.That(resultat.succes, Is.True);
        Assert.That(donnees.liquiditesCentimes, Is.EqualTo(5000));
        Assert.That(resultat.valeurLiquidationCentimes, Is.Zero);
    }

    [Test]
    public void Appliquer_DepenseLiquideUnePositionAuPrixObserve()
    {
        DonneesWhatIf donnees = CreerDonnees(11000, 1000);
        PositionBourse position = new PositionBourse("nvidia");
        position.AjouterAchat(10f, 10000);
        donnees.portefeuille.positions.Add(position);

        ResultatFluxMensuelsWhatIf resultat =
            ServiceFluxMensuelsWhatIf.Appliquer(
                donnees,
                CreerAnalyse(-5000),
                new Dictionary<string, int>
                {
                    { "nvidia", 1000 }
                },
                2);

        Assert.That(resultat.succes, Is.True);
        Assert.That(resultat.valeurLiquidationCentimes, Is.EqualTo(4000));
        Assert.That(donnees.liquiditesCentimes, Is.Zero);
        Assert.That(position.quantite, Is.EqualTo(6f).Within(0.001f));
    }

    [Test]
    public void Appliquer_PrixAbsentSignaleLeDeficitSansInventerDeValeur()
    {
        DonneesWhatIf donnees = CreerDonnees(10000, 0);
        PositionBourse position = new PositionBourse("nvidia");
        position.AjouterAchat(10f, 10000);
        donnees.portefeuille.positions.Add(position);

        ResultatFluxMensuelsWhatIf resultat =
            ServiceFluxMensuelsWhatIf.Appliquer(
                donnees,
                CreerAnalyse(-4000),
                new Dictionary<string, int>(),
                3);

        Assert.That(resultat.succes, Is.False);
        Assert.That(resultat.deficitNonCouvertCentimes, Is.EqualTo(4000));
        Assert.That(position.quantite, Is.EqualTo(10f));
    }

    [Test]
    public void Appliquer_DepenseSuperieureAuPatrimoineVideSansValeurNegative()
    {
        DonneesWhatIf donnees = CreerDonnees(5000, 1000);
        PositionBourse position = new PositionBourse("bitcoin");
        position.AjouterAchat(4f, 4000);
        donnees.portefeuille.positions.Add(position);

        ResultatFluxMensuelsWhatIf resultat =
            ServiceFluxMensuelsWhatIf.Appliquer(
                donnees,
                CreerAnalyse(-9000),
                new Dictionary<string, int>
                {
                    { "bitcoin", 1000 }
                },
                4);

        Assert.That(resultat.succes, Is.False);
        Assert.That(resultat.deficitNonCouvertCentimes, Is.EqualTo(4000));
        Assert.That(donnees.liquiditesCentimes, Is.Zero);
        Assert.That(donnees.portefeuille.positions, Is.Empty);
        Assert.That(
            donnees.portefeuille.GetValeurPatrimoine().centimes,
            Is.Zero);
    }

    private static DonneesWhatIf CreerDonnees(
        int capitalInitial,
        int liquidites)
    {
        DonneesWhatIf donnees = new DonneesWhatIf();
        ServicePortefeuilleAlternatifWhatIf.Initialiser(
            donnees,
            capitalInitial,
            0);
        donnees.liquiditesCentimes = liquidites;
        return donnees;
    }

    private static ResultatFluxMensuelsWhatIf CreerAnalyse(
        int fluxNet)
    {
        return new ResultatFluxMensuelsWhatIf
        {
            succes = true,
            fluxNetCentimes = fluxNet
        };
    }
}