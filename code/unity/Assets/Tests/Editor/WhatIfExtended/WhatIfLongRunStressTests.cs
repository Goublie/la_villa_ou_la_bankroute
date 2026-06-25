using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

public class WhatIfLongRunStressTests
{
    [Test]
    [Timeout(60000)]
    public void TrajectoireCombinee_120Mois_ResteStable()
    {
        DonneesWhatIf donnees = CreerDonnees(10000000);
        DonneesJoueur joueur = new DonneesJoueur();

        donnees.pretsImmobiliers.Add(
            new DonneesPret(
                new argent(3000000),
                10,
                2f,
                new argent(30000)));

        AnnonceImmobiliere annonce =
            new AnnonceImmobiliere
            {
                Ville = Ville.Nantes,
                Type = TypeBien.Studio,
                SurfaceM2 = 40,
                EstMeuble = false,
                PrixVenteAffiche = new argent(2000000),
                LoyerMensuelPropose = new argent(12000),
                TauxRendementBrut = 0.072f
            };

        ResultatAchatImmobilierWhatIf achat =
            ServiceImmobilierAlternatifWhatIf
                .EvaluerEtAcheterComptant(
                    donnees,
                    new List<AnnonceImmobiliere> { annonce },
                    0);
        Assert.That(achat.achatEffectue, Is.True);

        Stopwatch chronometre = Stopwatch.StartNew();

        for (int mois = 0; mois < 120; mois++)
        {
            CompteBanquaire compte = CompteCourant(joueur);
            compte.ViderHistorique();

            int salaire = mois % 12 < 9 ? 250000 : 0;
            if (salaire > 0)
            {
                compte.AjoutHistorique(
                    "salaire",
                    new argent(salaire));
            }

            compte.AjoutHistorique(
                "depenses courantes",
                new argent(-180000));
            compte.AjoutHistorique(
                "pret immobilier",
                new argent(-30000));
            compte.AjoutHistorique(
                "loyer",
                new argent(12000));

            Dictionary<string, int> prix =
                ConstruirePrix(mois);

            ResultatFluxMensuelsWhatIf flux =
                ServicePatrimoineAlternatifWhatIf
                    .AppliquerFluxMensuels(
                        donnees,
                        joueur,
                        prix,
                        mois);

            Assert.That(flux, Is.Not.Null);
            Assert.That(donnees.liquiditesCentimes, Is.GreaterThanOrEqualTo(0));

            if (mois % 12 == 0)
            {
                ResultatRechercheFaisceauWhatIf recherche =
                    ServiceOptimisationMultiHorizonWhatIf.Rechercher(
                        ServicePatrimoineAlternatifWhatIf
                            .CalculerPatrimoineAlternatif(
                                donnees,
                                prix),
                        ServicePortefeuilleAlternatifWhatIf
                            .ConstruireAllocationCourante(
                                donnees,
                                prix),
                        CreerPrevisions(mois),
                        donnees.configuration,
                        mois);

                Assert.That(recherche, Is.Not.Null);
                Assert.That(recherche.decisionRetenue, Is.Not.Null);

                ResultatReallocationWhatIf reallocation =
                    ServicePortefeuilleAlternatifWhatIf.Reallouer(
                        donnees,
                        recherche.decisionRetenue,
                        prix,
                        mois);

                Assert.That(reallocation.succes, Is.True);
            }

            int patrimoine =
                ServicePatrimoineAlternatifWhatIf
                    .CalculerPatrimoineAlternatif(
                        donnees,
                        prix);

            Assert.That(patrimoine, Is.GreaterThanOrEqualTo(0));
            VerifierPositions(donnees);
        }

        chronometre.Stop();
        TestContext.WriteLine(
            "Simulation combinee de 120 mois : " +
            chronometre.ElapsedMilliseconds +
            " ms.");
        TestContext.WriteLine(
            "Patrimoine final : " +
            ServicePatrimoineAlternatifWhatIf
                .CalculerPatrimoineAlternatif(
                    donnees,
                    ConstruirePrix(119)) +
            " centimes.");

        Assert.That(
            chronometre.ElapsedMilliseconds,
            Is.LessThan(45000));
        Assert.That(
            donnees.empreintesAnnoncesImmobilieresAchetees,
            Has.Count.EqualTo(1));
    }

    [Test]
    [Timeout(30000)]
    public void DepensesSansSalaire_36Mois_NeCreentJamaisDeValeurNegative()
    {
        DonneesWhatIf donnees = CreerDonnees(500000);
        PositionBourse position = new PositionBourse("defensif");
        position.AjouterAchat(100f, 1000000);
        donnees.portefeuille.positions.Add(position);

        bool deficitObserve = false;

        for (int mois = 0; mois < 36; mois++)
        {
            ResultatFluxMensuelsWhatIf analyse =
                new ResultatFluxMensuelsWhatIf
                {
                    succes = true,
                    fluxNetCentimes = -100000
                };

            ResultatFluxMensuelsWhatIf resultat =
                ServiceFluxMensuelsWhatIf.Appliquer(
                    donnees,
                    analyse,
                    new Dictionary<string, int>
                    {
                        { "defensif", 10000 }
                    },
                    mois);

            deficitObserve |= resultat.deficitNonCouvertCentimes > 0;

            Assert.That(donnees.liquiditesCentimes, Is.GreaterThanOrEqualTo(0));
            Assert.That(
                ServicePatrimoineAlternatifWhatIf
                    .CalculerPatrimoineAlternatif(
                        donnees,
                        new Dictionary<string, int>
                        {
                            { "defensif", 10000 }
                        }),
                Is.GreaterThanOrEqualTo(0));
            VerifierPositions(donnees);
        }

        Assert.That(
            deficitObserve,
            Is.True,
            "Le test doit finir par épuiser le patrimoine disponible.");
    }

    [Test]
    public void Copier_ApresScenarioComplexe_ProduitUneCopieProfonde()
    {
        DonneesWhatIf original = CreerDonnees(3000000);
        original.pretsImmobiliers.Add(
            new DonneesPret(
                new argent(1000000),
                5,
                2f,
                new argent(20000)));
        original.immobilier.biensPossedes.Add(
            new BienImmobilier
            {
                ville = Ville.Lyon,
                type = TypeBien.Studio,
                surfaceM2 = 35,
                prixAchat = new argent(1500000),
                valeurActuelle = new argent(1500000),
                loyerInitial = new argent(8000),
                loyerMensuel = new argent(8000),
                estLoue = true
            });
        original.decisions.Add(
            new DecisionWhatIf
            {
                indexMois = 2,
                strategieId = "test",
                allocations =
                    new List<AllocationActifWhatIf>
                    {
                        new AllocationActifWhatIf("cash", 40),
                        new AllocationActifWhatIf("croissance", 60)
                    }
            });

        DonneesWhatIf copie = original.Copier();

        copie.liquiditesCentimes = 1;
        copie.pretsImmobiliers[0].capitalRestantDu =
            new argent(1);
        copie.immobilier.biensPossedes[0].valeurActuelle =
            new argent(1);
        copie.decisions[0].allocations[0].pourcentage = 1;

        Assert.That(original.liquiditesCentimes, Is.EqualTo(3000000));
        Assert.That(
            original.pretsImmobiliers[0].capitalRestantDu.centimes,
            Is.Not.EqualTo(1));
        Assert.That(
            original.immobilier.biensPossedes[0]
                .valeurActuelle.centimes,
            Is.EqualTo(1500000));
        Assert.That(
            original.decisions[0].allocations[0].pourcentage,
            Is.EqualTo(40));
    }

    private static DonneesWhatIf CreerDonnees(int liquidites)
    {
        DonneesWhatIf donnees = new DonneesWhatIf
        {
            initialisee = true,
            actifsPassifsImmobiliersInitialises = true,
            liquiditesCentimes = liquidites,
            capitalInitialCentimes = liquidites,
            configuration = new ConfigurationWhatIf
            {
                horizonMois = 6,
                largeurFaisceau = 50,
                pasAllocationPourcent = 20,
                penaliteRisque = 0.15f,
                penaliteDrawdown = 0.25f
            }
        };
        donnees.InitialiserSiNecessaire();
        return donnees;
    }

    private static Dictionary<string, int> ConstruirePrix(int mois)
    {
        return new Dictionary<string, int>
        {
            { "croissance", 10000 + mois * 80 },
            { "defensif", 10000 + mois * 20 }
        };
    }

    private static List<PrevisionActifWhatIf> CreerPrevisions(int mois)
    {
        float cycle = mois % 24 < 12 ? 1f : -0.5f;

        return new List<PrevisionActifWhatIf>
        {
            new PrevisionActifWhatIf
            {
                actifId = "croissance",
                rendementMensuelEstimePourcent = 1.5f + cycle,
                volatiliteMensuellePourcent = 1.5f,
                risqueEstimePourcent = 1.5f,
                drawdownHistoriquePourcent = 5f,
                confiance01 = 1f
            },
            new PrevisionActifWhatIf
            {
                actifId = "defensif",
                rendementMensuelEstimePourcent = 0.5f,
                volatiliteMensuellePourcent = 0.2f,
                risqueEstimePourcent = 0.2f,
                drawdownHistoriquePourcent = 1f,
                confiance01 = 1f
            }
        };
    }

    private static void VerifierPositions(DonneesWhatIf donnees)
    {
        if (donnees.portefeuille?.positions == null)
        {
            return;
        }

        foreach (PositionBourse position in donnees.portefeuille.positions)
        {
            Assert.That(position, Is.Not.Null);
            Assert.That(float.IsNaN(position.quantite), Is.False);
            Assert.That(float.IsInfinity(position.quantite), Is.False);
            Assert.That(position.quantite, Is.GreaterThanOrEqualTo(0f));
        }
    }

    private static CompteBanquaire CompteCourant(
        DonneesJoueur joueur)
    {
        return joueur.comptes[ServiceBanque.CompteCourantId];
    }
}
