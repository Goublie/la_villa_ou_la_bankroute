using System.Collections.Generic;
using NUnit.Framework;

public class ServiceJournalOrdresWhatIfTests
{
    [Test]
    public void ReallocationDepuisCash_EnregistreUnAchat()
    {
        DonneesWhatIf donnees = CreerDonnees(100000, 0);

        ServicePortefeuilleAlternatifWhatIf.Reallouer(
            donnees,
            Decision(0, "nvidia", 100),
            Prix("nvidia", 10000),
            0);

        List<OrdreHistoriqueWhatIf> ordres =
            ServiceJournalOrdresWhatIf.ObtenirOrdres(
                donnees,
                0,
                0);

        Assert.That(ordres.Count, Is.EqualTo(1));
        Assert.That(
            ordres[0].type,
            Is.EqualTo(TypeOrdreHistoriqueWhatIf.Achat));
        Assert.That(ordres[0].actifId, Is.EqualTo("nvidia"));
        Assert.That(ordres[0].quantite, Is.EqualTo(10f).Within(0.001f));
        Assert.That(ordres[0].montantCentimes, Is.EqualTo(100000));
    }

    [Test]
    public void ReallocationVersCash_EnregistreUneVente()
    {
        DonneesWhatIf donnees = CreerDonnees(100000, 0);
        Dictionary<string, int> prix = Prix("nvidia", 10000);

        ServicePortefeuilleAlternatifWhatIf.Reallouer(
            donnees,
            Decision(0, "nvidia", 100),
            prix,
            0);

        ServicePortefeuilleAlternatifWhatIf.Reallouer(
            donnees,
            Decision(1, MoteurRechercheFaisceauWhatIf.LiquiditesId, 100),
            prix,
            1);

        List<OrdreHistoriqueWhatIf> ordres =
            ServiceJournalOrdresWhatIf.ObtenirOrdres(
                donnees,
                1,
                1);

        Assert.That(ordres.Count, Is.EqualTo(1));
        Assert.That(
            ordres[0].type,
            Is.EqualTo(TypeOrdreHistoriqueWhatIf.Vente));
        Assert.That(ordres[0].actifId, Is.EqualTo("nvidia"));
        Assert.That(ordres[0].montantCentimes, Is.EqualTo(100000));
    }

    [Test]
    public void AllocationInchangee_NAjouteAucunOrdre()
    {
        DonneesWhatIf donnees = CreerDonnees(100000, 0);
        Dictionary<string, int> prix = Prix("nvidia", 10000);
        DecisionWhatIf decision = Decision(0, "nvidia", 100);

        ServicePortefeuilleAlternatifWhatIf.Reallouer(
            donnees,
            decision,
            prix,
            0);

        ServicePortefeuilleAlternatifWhatIf.Reallouer(
            donnees,
            Decision(1, "nvidia", 100),
            prix,
            1);

        Assert.That(
            ServiceJournalOrdresWhatIf.ObtenirOrdres(
                donnees,
                1,
                1),
            Is.Empty);
    }

    [Test]
    public void DepenseSansCash_EnregistreUneVenteForcee()
    {
        DonneesWhatIf donnees = CreerDonnees(100000, 0);
        Dictionary<string, int> prix = Prix("nvidia", 10000);

        ServicePortefeuilleAlternatifWhatIf.Reallouer(
            donnees,
            Decision(0, "nvidia", 100),
            prix,
            0);

        ResultatFluxMensuelsWhatIf analyse =
            new ResultatFluxMensuelsWhatIf
            {
                succes = true,
                depensesCentimes = 25000,
                fluxNetCentimes = -25000
            };

        ServiceFluxMensuelsWhatIf.Appliquer(
            donnees,
            analyse,
            prix,
            0);

        List<OrdreHistoriqueWhatIf> ordres =
            ServiceJournalOrdresWhatIf.ObtenirOrdres(
                donnees,
                0,
                0);

        OrdreHistoriqueWhatIf venteForcee =
            ordres.Find(
                ordre =>
                    ordre.type ==
                    TypeOrdreHistoriqueWhatIf.VenteForcee);

        Assert.That(venteForcee, Is.Not.Null);
        Assert.That(venteForcee.actifId, Is.EqualTo("nvidia"));
        Assert.That(
            venteForcee.quantite,
            Is.EqualTo(2.5f).Within(0.001f));
        Assert.That(
            venteForcee.montantCentimes,
            Is.EqualTo(25000));
    }

    [Test]
    public void Copier_ProduitUnJournalIndependant()
    {
        DonneesWhatIf donnees = CreerDonnees(100000, 0);

        ServicePortefeuilleAlternatifWhatIf.Reallouer(
            donnees,
            Decision(0, "nvidia", 100),
            Prix("nvidia", 10000),
            0);

        DonneesWhatIf copie = donnees.Copier();
        copie.ordres[0].montantCentimes = 1;

        Assert.That(
            donnees.ordres[0].montantCentimes,
            Is.EqualTo(100000));
        Assert.That(
            copie.ordres[0],
            Is.Not.SameAs(donnees.ordres[0]));
    }

    [Test]
    public void DouzeDerniersMois_ExclutLesOrdresPlusAnciens()
    {
        DonneesWhatIf donnees = CreerDonnees(100000, 0);

        for (int mois = 0; mois < 15; mois++)
        {
            donnees.ordres.Add(
                new OrdreHistoriqueWhatIf
                {
                    indexMois = mois,
                    type = TypeOrdreHistoriqueWhatIf.Achat,
                    actifId = "actif",
                    quantite = 1f,
                    prixUnitaireCentimes = 100,
                    montantCentimes = 100,
                    raison = "test"
                });
        }

        List<OrdreHistoriqueWhatIf> resultat =
            ServiceJournalOrdresWhatIf
                .ObtenirOrdresDouzeDerniersMois(
                    donnees,
                    14);

        Assert.That(resultat.Count, Is.EqualTo(12));
        Assert.That(resultat[0].indexMois, Is.EqualTo(3));
        Assert.That(resultat[11].indexMois, Is.EqualTo(14));
    }

    private static DonneesWhatIf CreerDonnees(
        int capital,
        int mois)
    {
        DonneesWhatIf donnees = new DonneesWhatIf();
        ServicePortefeuilleAlternatifWhatIf.Initialiser(
            donnees,
            capital,
            mois);
        return donnees;
    }

    private static DecisionWhatIf Decision(
        int mois,
        string actifId,
        int pourcentage)
    {
        DecisionWhatIf decision =
            new DecisionWhatIf
            {
                indexMois = mois,
                strategieId = actifId + "_" + pourcentage
            };

        decision.allocations.Add(
            new AllocationActifWhatIf(
                actifId,
                pourcentage));

        return decision;
    }

    private static Dictionary<string, int> Prix(
        string actifId,
        int prixCentimes)
    {
        return new Dictionary<string, int>
        {
            { actifId, prixCentimes }
        };
    }
}