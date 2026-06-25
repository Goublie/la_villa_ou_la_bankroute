using System.Collections.Generic;
using NUnit.Framework;

public class ServicePatrimoineAlternatifWhatIfTests
{
    [Test]
    public void InitialiserDepuisJoueur_CopieLePret()
    {
        DonneesJoueur joueur = CreerJoueurAvecBienEtPret();
        DonneesWhatIf donnees = Initialiser(joueur);

        Assert.That(donnees.pretsImmobiliers.Count, Is.EqualTo(1));
        Assert.That(
            donnees.pretsImmobiliers[0],
            Is.Not.SameAs(joueur.pretsImmobiliers[0]));
        Assert.That(
            donnees.pretsImmobiliers[0].capitalRestantDu.centimes,
            Is.EqualTo(300000));
        Assert.That(
            donnees.pretsImmobiliers[0].mensualite.centimes,
            Is.EqualTo(10000));
        Assert.That(donnees.immobilier.biensPossedes.Count, Is.EqualTo(1));
        Assert.That(
            donnees.immobilier.biensPossedes[0],
            Is.Not.SameAs(joueur.immobilier.biensPossedes[0]));
    }


    [Test]
    public void InitialiserDepuisJoueur_MigreLAncienCapitalSansReinitialiserLePortefeuille()
    {
        DonneesJoueur joueur = CreerJoueurAvecBienEtPret();
        DonneesWhatIf donnees = new DonneesWhatIf
        {
            initialisee = true,
            capitalInitialCentimes = 100000,
            liquiditesCentimes = 40000
        };

        ServicePatrimoineAlternatifWhatIf.InitialiserDepuisJoueur(
            donnees,
            joueur,
            ServicePatrimoine.Calculer(joueur).centimes,
            5);

        Assert.That(donnees.liquiditesCentimes, Is.EqualTo(40000));
        Assert.That(donnees.capitalInitialCentimes, Is.EqualTo(300000));
        Assert.That(donnees.immobilier.biensPossedes.Count, Is.EqualTo(1));
        Assert.That(donnees.pretsImmobiliers.Count, Is.EqualTo(1));
        Assert.That(donnees.actifsPassifsImmobiliersInitialises, Is.True);
    }

    [Test]
    public void AppliquerMensualite_NeModifieJamaisLeJoueurReel()
    {
        DonneesJoueur joueur = CreerJoueurAvecBienEtPret();
        DonneesWhatIf donnees = Initialiser(joueur);
        AjouterMensualiteReelle(joueur);

        int soldeReelAvant = CompteCourant(joueur).GetSolde().centimes;
        int capitalReelAvant =
            joueur.pretsImmobiliers[0].capitalRestantDu.centimes;
        int moisReelsAvant = joueur.pretsImmobiliers[0].moisRestants;
        int valeurBienReelAvant =
            joueur.immobilier.biensPossedes[0].valeurActuelle.centimes;

        ServicePatrimoineAlternatifWhatIf.AppliquerFluxMensuels(
            donnees,
            joueur,
            PrixVides,
            0);

        Assert.That(
            CompteCourant(joueur).GetSolde().centimes,
            Is.EqualTo(soldeReelAvant));
        Assert.That(
            joueur.pretsImmobiliers[0].capitalRestantDu.centimes,
            Is.EqualTo(capitalReelAvant));
        Assert.That(
            joueur.pretsImmobiliers[0].moisRestants,
            Is.EqualTo(moisReelsAvant));
        Assert.That(
            joueur.immobilier.biensPossedes[0].valeurActuelle.centimes,
            Is.EqualTo(valeurBienReelAvant));
    }

    [Test]
    public void AppliquerMensualite_DiminueLesLiquiditesAlternatives()
    {
        DonneesJoueur joueur = CreerJoueurAvecBienEtPret();
        DonneesWhatIf donnees = Initialiser(joueur);
        AjouterMensualiteReelle(joueur);

        ServicePatrimoineAlternatifWhatIf.AppliquerFluxMensuels(
            donnees,
            joueur,
            PrixVides,
            0);

        Assert.That(donnees.liquiditesCentimes, Is.EqualTo(90000));
    }

    [Test]
    public void AppliquerMensualite_DiminueLaDetteAlternative()
    {
        DonneesJoueur joueur = CreerJoueurAvecBienEtPret();
        DonneesWhatIf donnees = Initialiser(joueur);
        AjouterMensualiteReelle(joueur);

        ServicePatrimoineAlternatifWhatIf.AppliquerFluxMensuels(
            donnees,
            joueur,
            PrixVides,
            0);

        Assert.That(
            donnees.pretsImmobiliers[0].capitalRestantDu.centimes,
            Is.EqualTo(290000));
        Assert.That(
            donnees.pretsImmobiliers[0].moisRestants,
            Is.EqualTo(29));
    }

    [Test]
    public void CalculerPatrimoineAlternatif_TientCompteDuBienEtDeLaDette()
    {
        DonneesJoueur joueur = CreerJoueurAvecBienEtPret();
        DonneesWhatIf donnees = Initialiser(joueur);

        int patrimoine =
            ServicePatrimoineAlternatifWhatIf.CalculerPatrimoineAlternatif(
                donnees,
                PrixVides);

        Assert.That(donnees.liquiditesCentimes, Is.EqualTo(100000));
        Assert.That(
            ServicePatrimoineAlternatifWhatIf.CalculerValeurImmobilier(
                donnees),
            Is.EqualTo(500000));
        Assert.That(
            ServicePatrimoineAlternatifWhatIf.CalculerDettes(donnees),
            Is.EqualTo(300000));
        Assert.That(patrimoine, Is.EqualTo(300000));
    }

    [Test]
    public void AppliquerMensualite_NEstPasCompteeDeuxFois()
    {
        DonneesJoueur joueur = CreerJoueurAvecBienEtPret();
        DonneesWhatIf donnees = Initialiser(joueur);
        AjouterMensualiteReelle(joueur);

        ResultatFluxMensuelsWhatIf analyseReelle =
            ServiceFluxMensuelsWhatIf.Analyser(joueur);
        Assert.That(analyseReelle.fluxNetCentimes, Is.Zero);
        Assert.That(analyseReelle.transactionsIgnorees, Is.EqualTo(1));
        Assert.That(
            analyseReelle.classifications[0].type,
            Is.EqualTo(TypeFluxWhatIf.MensualitePretImmobilier));

        ResultatFluxMensuelsWhatIf premier =
            ServicePatrimoineAlternatifWhatIf.AppliquerFluxMensuels(
                donnees,
                joueur,
                PrixVides,
                0);
        int liquiditesApresPremierPassage = donnees.liquiditesCentimes;
        int detteApresPremierPassage =
            donnees.pretsImmobiliers[0].capitalRestantDu.centimes;

        ResultatFluxMensuelsWhatIf second =
            ServicePatrimoineAlternatifWhatIf.AppliquerFluxMensuels(
                donnees,
                joueur,
                PrixVides,
                0);

        Assert.That(premier.fluxNetCentimes, Is.EqualTo(-10000));
        Assert.That(second.fluxNetCentimes, Is.Zero);
        Assert.That(liquiditesApresPremierPassage, Is.EqualTo(90000));
        Assert.That(
            donnees.liquiditesCentimes,
            Is.EqualTo(liquiditesApresPremierPassage));
        Assert.That(
            donnees.pretsImmobiliers[0].capitalRestantDu.centimes,
            Is.EqualTo(detteApresPremierPassage));
    }


    [Test]
    public void NouveauPretApresInitialisation_AjouteArgentEtDetteSansGainFictif()
    {
        DonneesJoueur joueur = new DonneesJoueur();
        DonneesWhatIf donnees = Initialiser(joueur);
        int patrimoineAvant =
            ServicePatrimoineAlternatifWhatIf.CalculerPatrimoineAlternatif(
                donnees,
                PrixVides);
        int liquiditesAvant = donnees.liquiditesCentimes;

        DonneesPret pretReel = new DonneesPret(
            new argent(100000),
            1,
            0f,
            new argent(10000))
        {
            moisRestants = 11,
            capitalRestantDu = new argent(90000)
        };
        joueur.pretsImmobiliers.Add(pretReel);
        CompteCourant(joueur).AjoutHistorique(
            "Versement emprunt immobilier",
            new argent(100000));
        CompteCourant(joueur).AjoutHistorique(
            "Pret Immo (1/12)",
            new argent(-10000));

        ResultatSynchronisationPretsWhatIf synchronisation =
            ServicePatrimoineAlternatifWhatIf
                .SynchroniserNouveauxPretsDepuisJoueur(
                    donnees,
                    joueur);
        ServicePatrimoineAlternatifWhatIf.AppliquerFluxMensuels(
            donnees,
            joueur,
            PrixVides,
            1,
            synchronisation.pretsCopies);

        Assert.That(synchronisation.nouveauxPrets, Is.EqualTo(1));
        Assert.That(
            donnees.liquiditesCentimes,
            Is.EqualTo(liquiditesAvant + 90000));
        Assert.That(
            donnees.pretsImmobiliers[0].capitalRestantDu.centimes,
            Is.EqualTo(90000));
        Assert.That(
            donnees.pretsImmobiliers[0].moisRestants,
            Is.EqualTo(11));
        Assert.That(
            ServicePatrimoineAlternatifWhatIf.CalculerDettes(donnees),
            Is.EqualTo(90000));
        Assert.That(
            ServicePatrimoineAlternatifWhatIf.CalculerPatrimoineAlternatif(
                donnees,
                PrixVides),
            Is.EqualTo(patrimoineAvant));
        Assert.That(
            pretReel.capitalRestantDu.centimes,
            Is.EqualTo(90000));
    }

    private static readonly Dictionary<string, int> PrixVides =
        new Dictionary<string, int>();

    private static DonneesWhatIf Initialiser(DonneesJoueur joueur)
    {
        DonneesWhatIf donnees = new DonneesWhatIf();
        int patrimoine = ServicePatrimoine.Calculer(joueur).centimes;

        ServicePatrimoineAlternatifWhatIf.InitialiserDepuisJoueur(
            donnees,
            joueur,
            patrimoine,
            0);

        return donnees;
    }

    private static DonneesJoueur CreerJoueurAvecBienEtPret()
    {
        DonneesJoueur joueur = new DonneesJoueur();
        joueur.immobilier.biensPossedes.Add(
            new BienImmobilier
            {
                valeurActuelle = new argent(500000),
                prixAchat = new argent(500000),
                loyerMensuel = new argent(0),
                loyerInitial = new argent(0)
            });

        joueur.pretsImmobiliers.Add(
            new DonneesPret(
                new argent(300000),
                10,
                0f,
                new argent(10000))
            {
                moisRestants = 30,
                capitalRestantDu = new argent(300000)
            });

        return joueur;
    }

    private static void AjouterMensualiteReelle(DonneesJoueur joueur)
    {
        CompteCourant(joueur).AjoutHistorique(
            "Pret Immo (1/120)",
            new argent(-10000));
    }

    private static CompteBanquaire CompteCourant(DonneesJoueur joueur)
    {
        return joueur.comptes[ServiceBanque.CompteCourantId];
    }
}
