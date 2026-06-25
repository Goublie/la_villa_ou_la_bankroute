using System.Collections.Generic;
using NUnit.Framework;

public class ServiceImmobilierAlternatifWhatIfTests
{
    [Test]
    public void AcheterComptant_ChoisitLeMeilleurRendementSansCreerDePret()
    {
        DonneesWhatIf donnees = CreerDonnees(1000000);
        List<AnnonceImmobiliere> annonces =
            new List<AnnonceImmobiliere>
            {
                CreerAnnonce(
                    Ville.Paris,
                    TypeBien.Studio,
                    500000,
                    2500),
                CreerAnnonce(
                    Ville.Lyon,
                    TypeBien.AppartementT2,
                    600000,
                    4000)
            };

        ResultatAchatImmobilierWhatIf resultat =
            ServiceImmobilierAlternatifWhatIf
                .EvaluerEtAcheterComptant(
                    donnees,
                    annonces,
                    4);

        Assert.That(resultat.succes, Is.True);
        Assert.That(resultat.achatEffectue, Is.True);
        Assert.That(donnees.liquiditesCentimes, Is.EqualTo(400000));
        Assert.That(donnees.immobilier.biensPossedes.Count, Is.EqualTo(1));
        Assert.That(
            donnees.immobilier.biensPossedes[0].ville,
            Is.EqualTo(Ville.Lyon));
        Assert.That(
            donnees.immobilier.biensPossedes[0].loyerMensuel.centimes,
            Is.EqualTo(4000));
        Assert.That(donnees.pretsImmobiliers, Is.Empty);
        Assert.That(annonces, Has.Count.EqualTo(2));
    }

    [Test]
    public void AcheterComptant_ConserveVingtPourCentDeReserve()
    {
        DonneesWhatIf donnees = CreerDonnees(100000);
        AnnonceImmobiliere annonce =
            CreerAnnonce(
                Ville.Nantes,
                TypeBien.Studio,
                90000,
                500);

        ResultatAchatImmobilierWhatIf resultat =
            ServiceImmobilierAlternatifWhatIf
                .EvaluerEtAcheterComptant(
                    donnees,
                    new List<AnnonceImmobiliere> { annonce },
                    0);

        Assert.That(resultat.succes, Is.True);
        Assert.That(resultat.achatEffectue, Is.False);
        Assert.That(donnees.liquiditesCentimes, Is.EqualTo(100000));
        Assert.That(donnees.immobilier.biensPossedes, Is.Empty);
    }

    [Test]
    public void AcheterComptant_NAcheteJamaisDeuxFoisLaMemeAnnonce()
    {
        DonneesWhatIf donnees = CreerDonnees(2000000);
        AnnonceImmobiliere annonce =
            CreerAnnonce(
                Ville.Toulouse,
                TypeBien.Studio,
                500000,
                3000);

        ResultatAchatImmobilierWhatIf premier =
            ServiceImmobilierAlternatifWhatIf
                .EvaluerEtAcheterComptant(
                    donnees,
                    new List<AnnonceImmobiliere> { annonce },
                    1);
        ResultatAchatImmobilierWhatIf second =
            ServiceImmobilierAlternatifWhatIf
                .EvaluerEtAcheterComptant(
                    donnees,
                    new List<AnnonceImmobiliere> { annonce },
                    2);

        Assert.That(premier.achatEffectue, Is.True);
        Assert.That(second.achatEffectue, Is.False);
        Assert.That(donnees.immobilier.biensPossedes.Count, Is.EqualTo(1));
    }

    [Test]
    public void FluxMensuels_RemplaceLesLoyersReelsParLesLoyersAlternatifsUneFois()
    {
        DonneesJoueur joueur = new DonneesJoueur();
        joueur.immobilier.biensPossedes.Add(
            CreerBienAvecLoyer(1000));

        DonneesWhatIf donnees = new DonneesWhatIf();
        ServicePatrimoineAlternatifWhatIf.InitialiserDepuisJoueur(
            donnees,
            joueur,
            ServicePatrimoine.Calculer(joueur).centimes,
            0);
        donnees.immobilier.biensPossedes.Add(
            CreerBienAvecLoyer(500));

        int liquiditesAvant = donnees.liquiditesCentimes;
        CompteCourant(joueur).AjoutHistorique(
            "loyer",
            new argent(1000));
        CompteCourant(joueur).AjoutHistorique(
            "loyer",
            new argent(1000));

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

        Assert.That(premier.fluxNetCentimes, Is.EqualTo(1500));
        Assert.That(
            apresPremier,
            Is.EqualTo(liquiditesAvant + 1500));
        Assert.That(second.fluxNetCentimes, Is.Zero);
        Assert.That(donnees.liquiditesCentimes, Is.EqualTo(apresPremier));
    }

    [Test]
    public void InitialiserDepuisJoueur_SynchroniseLesImpactsConfirmes()
    {
        DonneesJoueur joueur = new DonneesJoueur();
        ImpactEvenementImmobilier impact =
            new ImpactEvenementImmobilier
            {
                evenementId = "immo_confirme",
                moisDebut = 2,
                dureeMois = 6,
                coefficientPrix = 1.10f,
                coefficientLoyer = 1.04f
            };
        joueur.immobilier.impactsActifs.Add(impact);

        DonneesWhatIf donnees = new DonneesWhatIf();
        ServicePatrimoineAlternatifWhatIf.InitialiserDepuisJoueur(
            donnees,
            joueur,
            ServicePatrimoine.Calculer(joueur).centimes,
            2);

        Assert.That(donnees.immobilier.impactsActifs.Count, Is.EqualTo(1));
        Assert.That(
            donnees.immobilier.impactsActifs[0],
            Is.Not.SameAs(impact));

        impact.coefficientPrix = 1.20f;
        ServicePatrimoineAlternatifWhatIf.InitialiserDepuisJoueur(
            donnees,
            joueur,
            ServicePatrimoine.Calculer(joueur).centimes,
            3);

        Assert.That(
            donnees.immobilier.impactsActifs[0].coefficientPrix,
            Is.EqualTo(1.20f));
    }

    private static DonneesWhatIf CreerDonnees(int liquidites)
    {
        DonneesWhatIf donnees = new DonneesWhatIf
        {
            initialisee = true,
            actifsPassifsImmobiliersInitialises = true,
            liquiditesCentimes = liquidites
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

    private static CompteBanquaire CompteCourant(
        DonneesJoueur joueur)
    {
        return joueur.comptes[ServiceBanque.CompteCourantId];
    }
}