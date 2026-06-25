using System.Collections.Generic;
using NUnit.Framework;

public class WhatIfScenarioMatrixTests
{
    [TestCase(0, 0, 0)]
    [TestCase(200000, 0, 200000)]
    [TestCase(0, 80000, -80000)]
    [TestCase(250000, 100000, 150000)]
    public void Flux_SalaireOuAbsenceDeSalaire_EstReproduitEquitablement(
        int salaireCentimes,
        int depensesCentimes,
        int fluxAttenduCentimes)
    {
        const int liquiditesInitiales = 1000000;
        DonneesWhatIf donnees = CreerDonnees(liquiditesInitiales);
        List<Transaction> transactions = new List<Transaction>();

        if (salaireCentimes > 0)
        {
            transactions.Add(
                new Transaction("salaire", salaireCentimes));
        }

        if (depensesCentimes > 0)
        {
            transactions.Add(
                new Transaction("depense courante", -depensesCentimes));
        }

        ResultatFluxMensuelsWhatIf analyse =
            ServiceFluxMensuelsWhatIf.Analyser(transactions);
        ResultatFluxMensuelsWhatIf resultat =
            ServiceFluxMensuelsWhatIf.Appliquer(
                donnees,
                analyse,
                new Dictionary<string, int>(),
                0);

        Assert.That(resultat.succes, Is.True);
        Assert.That(
            resultat.fluxNetCentimes,
            Is.EqualTo(fluxAttenduCentimes));
        Assert.That(
            donnees.liquiditesCentimes,
            Is.EqualTo(
                liquiditesInitiales + fluxAttenduCentimes));
    }

    [Test]
    public void Pret_MensualiteAlternative_EstDebiteeUneSeuleFoisParMois()
    {
        DonneesWhatIf donnees = CreerDonnees(1000000);
        donnees.pretsImmobiliers.Add(
            new DonneesPret(
                new argent(1200000),
                10,
                2f,
                new argent(15000)));

        DonneesJoueur joueur = new DonneesJoueur();
        CompteCourant(joueur).AjoutHistorique(
            "pret immobilier",
            new argent(-15000));

        int avant = donnees.liquiditesCentimes;
        ResultatFluxMensuelsWhatIf premier =
            ServicePatrimoineAlternatifWhatIf.AppliquerFluxMensuels(
                donnees,
                joueur,
                new Dictionary<string, int>(),
                0);
        int apresPremier = donnees.liquiditesCentimes;

        ResultatFluxMensuelsWhatIf second =
            ServicePatrimoineAlternatifWhatIf.AppliquerFluxMensuels(
                donnees,
                joueur,
                new Dictionary<string, int>(),
                0);

        Assert.That(premier.fluxNetCentimes, Is.EqualTo(-15000));
        Assert.That(apresPremier, Is.EqualTo(avant - 15000));
        Assert.That(second.fluxNetCentimes, Is.Zero);
        Assert.That(donnees.liquiditesCentimes, Is.EqualTo(apresPremier));
    }

    [Test]
    public void Immobilier_AchatComptant_NeCreePasDePerteImmediate()
    {
        DonneesWhatIf donnees = CreerDonnees(2000000);
        AnnonceImmobiliere annonce =
            CreerAnnonce(
                Ville.Paris,
                TypeBien.Studio,
                600000,
                3000);

        int patrimoineAvant =
            ServicePatrimoineAlternatifWhatIf
                .CalculerPatrimoineAlternatif(
                    donnees,
                    new Dictionary<string, int>());

        ResultatAchatImmobilierWhatIf achat =
            ServiceImmobilierAlternatifWhatIf
                .EvaluerEtAcheterComptant(
                    donnees,
                    new List<AnnonceImmobiliere> { annonce },
                    4);

        BienImmobilier bien =
            donnees.immobilier.biensPossedes[0];
        bien.valeurActuelle =
            ServiceImmobilier.CalculerValeurActuelle(
                bien,
                4,
                donnees.immobilier);

        int patrimoineApres =
            ServicePatrimoineAlternatifWhatIf
                .CalculerPatrimoineAlternatif(
                    donnees,
                    new Dictionary<string, int>());

        Assert.That(achat.achatEffectue, Is.True);
        Assert.That(
            patrimoineApres,
            Is.EqualTo(patrimoineAvant),
            "Un achat comptant ne doit transformer que du cash en immobilier.");
    }

    [Test]
    public void Immobilier_LoyerAlternatif_EstAjouteUneSeuleFois()
    {
        DonneesWhatIf donnees = CreerDonnees(1000000);
        donnees.immobilier.biensPossedes.Add(
            CreerBienAvecLoyer(12000));

        DonneesJoueur joueur = new DonneesJoueur();
        CompteCourant(joueur).AjoutHistorique(
            "loyer",
            new argent(12000));
        CompteCourant(joueur).AjoutHistorique(
            "loyer",
            new argent(12000));

        int avant = donnees.liquiditesCentimes;
        ResultatFluxMensuelsWhatIf premier =
            ServicePatrimoineAlternatifWhatIf.AppliquerFluxMensuels(
                donnees,
                joueur,
                new Dictionary<string, int>(),
                3);
        int apresPremier = donnees.liquiditesCentimes;

        ResultatFluxMensuelsWhatIf second =
            ServicePatrimoineAlternatifWhatIf.AppliquerFluxMensuels(
                donnees,
                joueur,
                new Dictionary<string, int>(),
                3);

        Assert.That(premier.fluxNetCentimes, Is.EqualTo(12000));
        Assert.That(apresPremier, Is.EqualTo(avant + 12000));
        Assert.That(second.fluxNetCentimes, Is.Zero);
        Assert.That(donnees.liquiditesCentimes, Is.EqualTo(apresPremier));
    }

    [Test]
    public void InvestissementFixe_EstInclusDansLeCapitalInitialAlternatif()
    {
        DonneesJoueur joueur = new DonneesJoueur();
        joueur.investissements.Add(
            new Investissement(
                new argent(500000),
                0.12f,
                1));

        int patrimoineReel =
            ServicePatrimoine.Calculer(joueur).centimes;
        DonneesWhatIf donnees = new DonneesWhatIf();

        ServicePatrimoineAlternatifWhatIf.InitialiserDepuisJoueur(
            donnees,
            joueur,
            patrimoineReel,
            0);

        int patrimoineAlternatif =
            ServicePatrimoineAlternatifWhatIf
                .CalculerPatrimoineAlternatif(
                    donnees,
                    new Dictionary<string, int>());

        Assert.That(
            patrimoineAlternatif,
            Is.EqualTo(patrimoineReel));
    }

    [Test]
    public void Bourse_ActifPositif_EstPrefereAUnActifNegatif()
    {
        ConfigurationWhatIf configuration =
            new ConfigurationWhatIf
            {
                horizonMois = 6,
                largeurFaisceau = 50,
                pasAllocationPourcent = 20,
                penaliteRisque = 0f,
                penaliteDrawdown = 0f
            };

        ResultatRechercheFaisceauWhatIf resultat =
            ServiceOptimisationMultiHorizonWhatIf.Rechercher(
                1000000,
                new Dictionary<string, int>
                {
                    { "cash", 100 }
                },
                new List<PrevisionActifWhatIf>
                {
                    CreerPrevision("croissance", 3f, 0.5f),
                    CreerPrevision("baisse", -2f, 0.5f)
                },
                configuration,
                0);

        Assert.That(resultat, Is.Not.Null);
        Assert.That(resultat.decisionRetenue, Is.Not.Null);
        Assert.That(
            Pourcentage(resultat.decisionRetenue, "croissance"),
            Is.GreaterThan(
                Pourcentage(resultat.decisionRetenue, "baisse")));
    }

    [Test]
    public void ScenarioComplet_SalairePretEtLoyer_ProduitLeFluxAttendu()
    {
        DonneesWhatIf donnees = CreerDonnees(2000000);
        donnees.pretsImmobiliers.Add(
            new DonneesPret(
                new argent(1200000),
                10,
                2f,
                new argent(15000)));
        donnees.immobilier.biensPossedes.Add(
            CreerBienAvecLoyer(12000));

        DonneesJoueur joueur = new DonneesJoueur();
        CompteBanquaire compte = CompteCourant(joueur);
        compte.AjoutHistorique("salaire", new argent(250000));
        compte.AjoutHistorique(
            "depense courante",
            new argent(-100000));
        compte.AjoutHistorique(
            "pret immobilier",
            new argent(-15000));
        compte.AjoutHistorique("loyer", new argent(12000));

        ResultatFluxMensuelsWhatIf resultat =
            ServicePatrimoineAlternatifWhatIf.AppliquerFluxMensuels(
                donnees,
                joueur,
                new Dictionary<string, int>(),
                7);

        Assert.That(resultat.succes, Is.True);
        Assert.That(
            resultat.fluxNetCentimes,
            Is.EqualTo(147000));
        Assert.That(donnees.liquiditesCentimes, Is.EqualTo(2147000));
    }

    private static DonneesWhatIf CreerDonnees(int liquidites)
    {
        DonneesWhatIf donnees = new DonneesWhatIf
        {
            initialisee = true,
            actifsPassifsImmobiliersInitialises = true,
            liquiditesCentimes = liquidites,
            capitalInitialCentimes = liquidites
        };
        donnees.InitialiserSiNecessaire();
        return donnees;
    }

    private static AnnonceImmobiliere CreerAnnonce(
        Ville ville,
        TypeBien type,
        int prix,
        int loyer)
    {
        return new AnnonceImmobiliere
        {
            Ville = ville,
            Type = type,
            SurfaceM2 = 40,
            EstMeuble = false,
            PrixVenteAffiche = new argent(prix),
            LoyerMensuelPropose = new argent(loyer),
            TauxRendementBrut =
                prix > 0 ? loyer * 12f / prix : 0f
        };
    }

    private static BienImmobilier CreerBienAvecLoyer(int loyer)
    {
        return new BienImmobilier
        {
            prixAchat = new argent(0),
            valeurActuelle = new argent(0),
            estLoue = true,
            loyerInitial = new argent(loyer),
            loyerMensuel = new argent(loyer)
        };
    }

    private static PrevisionActifWhatIf CreerPrevision(
        string actifId,
        float rendement,
        float risque)
    {
        return new PrevisionActifWhatIf
        {
            actifId = actifId,
            rendementMensuelEstimePourcent = rendement,
            volatiliteMensuellePourcent = risque,
            risqueEstimePourcent = risque,
            confiance01 = 1f
        };
    }

    private static int Pourcentage(
        DecisionWhatIf decision,
        string actifId)
    {
        if (decision?.allocations == null)
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

    private static CompteBanquaire CompteCourant(
        DonneesJoueur joueur)
    {
        return joueur.comptes[ServiceBanque.CompteCourantId];
    }
}
