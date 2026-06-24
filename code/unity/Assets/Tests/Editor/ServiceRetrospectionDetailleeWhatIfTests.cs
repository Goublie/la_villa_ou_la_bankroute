using System.Collections.Generic;
using NUnit.Framework;

public class ServiceRetrospectionDetailleeWhatIfTests
{
    [Test]
    public void OrdresDouzeDerniersMois_SontFiltresEtTries()
    {
        DonneesWhatIf donnees = new DonneesWhatIf();
        donnees.InitialiserSiNecessaire();

        int montantValide =
            System.Math.Max(
                ServiceBourse.MontantMinimumOrdreCentimes,
                100);

        for (int mois = 0; mois < 15; mois++)
        {
            donnees.ordres.Add(
                Ordre(
                    mois,
                    TypeOrdreHistoriqueWhatIf.Achat,
                    "actif",
                    montantValide));
        }

        List<LigneOperationRetrospectionWhatIf> lignes =
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireLignesOrdres(
                    donnees,
                    14);

        Assert.That(lignes.Count, Is.EqualTo(12));
        Assert.That(lignes[0].indexMois, Is.EqualTo(14));
        Assert.That(lignes[11].indexMois, Is.EqualTo(3));
    }

    [Test]
    public void OrdreAchat_ProduitUneLigneLisible()
    {
        DonneesWhatIf donnees = new DonneesWhatIf();
        donnees.InitialiserSiNecessaire();

        donnees.ordres.Add(
            new OrdreHistoriqueWhatIf
            {
                indexMois = 2,
                type = TypeOrdreHistoriqueWhatIf.Achat,
                actifId = "nvidia",
                quantite = 2.5f,
                prixUnitaireCentimes = 10000,
                montantCentimes = 25000,
                coutTransactionCentimes = 500,
                raison = "Réallocation mensuelle"
            });

        LigneOperationRetrospectionWhatIf ligne =
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireLignesOrdres(
                    donnees,
                    2)[0];

        Assert.That(ligne.operation, Is.EqualTo("ACHAT"));
        Assert.That(ligne.detail, Does.Contain("nvidia"));
        Assert.That(ligne.detail, Does.Contain("2.5"));
        Assert.That(ligne.detail, Does.Contain("frais"));
    }

    [Test]
    public void VenteForcee_EstDistinguee()
    {
        DonneesWhatIf donnees = new DonneesWhatIf();
        donnees.InitialiserSiNecessaire();

        donnees.ordres.Add(
            Ordre(
                0,
                TypeOrdreHistoriqueWhatIf.Achat,
                "bitcoin",
                10000));

        donnees.ordres.Add(
            Ordre(
                1,
                TypeOrdreHistoriqueWhatIf.VenteForcee,
                "bitcoin",
                10000));

        List<LigneOperationRetrospectionWhatIf> lignes =
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireLignesOrdres(
                    donnees,
                    1);

        Assert.That(
            lignes[0].operation,
            Is.EqualTo("VENTE FORCÉE"));
    }

    [Test]
    public void VenteBlanche_NEstJamaisAffichee()
    {
        DonneesWhatIf donnees = new DonneesWhatIf();
        donnees.InitialiserSiNecessaire();

        donnees.ordres.Add(
            new OrdreHistoriqueWhatIf
            {
                indexMois = 5,
                type = TypeOrdreHistoriqueWhatIf.Vente,
                actifId = "totalenergies",
                quantite = 18.7371f,
                prixUnitaireCentimes = 4812,
                montantCentimes = 90163,
                raison = "Reallocation mensuelle"
            });

        Assert.That(
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireLignesOrdres(
                    donnees,
                    5),
            Is.Empty);
    }

    [Test]
    public void MicroOrdre_NEstJamaisAffiche()
    {
        DonneesWhatIf donnees = new DonneesWhatIf();
        donnees.InitialiserSiNecessaire();

        donnees.ordres.Add(
            new OrdreHistoriqueWhatIf
            {
                indexMois = 1,
                type = TypeOrdreHistoriqueWhatIf.Achat,
                actifId = "nvidia",
                quantite = 0.00003f,
                prixUnitaireCentimes = 19834,
                montantCentimes =
                    System.Math.Max(
                        0,
                        ServiceBourse.MontantMinimumOrdreCentimes - 1),
                raison = "Reallocation mensuelle"
            });

        Assert.That(
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireLignesOrdres(
                    donnees,
                    1),
            Is.Empty);
    }

    [Test]
    public void EvenementsConfirmes_ExcluentLesPlusAnciens()
    {
        DonneesEvenements evenements =
            new DonneesEvenements();

        evenements.evenementsConfirmes.Add(
            Evenement(
                "ancien",
                "Ancien événement",
                0,
                "Personnels"));

        evenements.evenementsConfirmes.Add(
            Evenement(
                "recent",
                "Événement récent",
                5,
                "Personnels"));

        string texte =
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireSectionEvenementsConfirmes(
                    evenements,
                    new DonneesWhatIf(),
                    12);

        Assert.That(
            texte,
            Does.Not.Contain("Ancien événement"));

        Assert.That(
            texte,
            Does.Contain("Événement récent"));
    }

    [Test]
    public void EvenementBoursier_AfficheSesImpacts()
    {
        DonneesEvenements evenements =
            new DonneesEvenements();

        EvenementConfirmePartie evenement =
            Evenement(
                "evt_bourse",
                "Crise technologique",
                2,
                CategoriesEvenements.Boursiers);

        evenement.impacts.Add(
            new ImpactDefinitionEvenement
            {
                actif = "nvidia",
                variation = -0.15f
            });

        evenements.evenementsConfirmes.Add(evenement);

        string texte =
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireSectionEvenementsConfirmes(
                    evenements,
                    new DonneesWhatIf(),
                    2);

        Assert.That(
            texte,
            Does.Contain("nvidia -15 %"));
    }

    [Test]
    public void EvenementsConfirmes_ProduisentDesLignesDeTableau()
    {
        DonneesEvenements evenements =
            new DonneesEvenements();

        EvenementConfirmePartie evenement =
            Evenement(
                "evt_tableau",
                "Annonce confirmée",
                4,
                CategoriesEvenements.Boursiers);

        evenement.impacts.Add(
            new ImpactDefinitionEvenement
            {
                actif = "nvidia",
                variation = 0.08f
            });

        evenements.evenementsConfirmes.Add(evenement);

        List<LigneOperationRetrospectionWhatIf> lignes =
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireLignesEvenementsConfirmes(
                    evenements,
                    new DonneesWhatIf(),
                    4);

        Assert.That(lignes.Count, Is.EqualTo(1));
        Assert.That(
            lignes[0].operation,
            Is.EqualTo("Annonce confirmée"));
        Assert.That(
            lignes[0].detail,
            Does.Contain("nvidia +8 %"));
    }

    [Test]
    public void EvenementBoursier_IndiqueLePremierMoisDePriseEnCompte()
    {
        DonneesEvenements evenements =
            new DonneesEvenements();

        EvenementConfirmePartie evenement =
            Evenement(
                "evt_1",
                "Annonce boursière",
                2,
                CategoriesEvenements.Boursiers);

        evenement.impacts.Add(
            new ImpactDefinitionEvenement
            {
                actif = "cac40",
                variation = 0.05f
            });

        evenements.evenementsConfirmes.Add(evenement);

        DonneesWhatIf donnees = new DonneesWhatIf();
        donnees.InitialiserSiNecessaire();

        donnees.decisions.Add(
            new DecisionWhatIf
            {
                indexMois = 4,
                evenementsConnusIds =
                    new List<string>
                    {
                        "evt_1"
                    }
            });

        string texte =
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireSectionEvenementsConfirmes(
                    evenements,
                    donnees,
                    4);

        Assert.That(texte, Does.Contain("Pris en compte"));
        Assert.That(texte, Does.Contain("Novembre"));
    }

    [Test]
    public void EvenementNonBoursier_EstDeclareHorsOptimisation()
    {
        DonneesEvenements evenements =
            new DonneesEvenements();

        evenements.evenementsConfirmes.Add(
            Evenement(
                "evt_perso",
                "Événement familial",
                1,
                CategoriesEvenements.Personnels));

        string texte =
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireSectionEvenementsConfirmes(
                    evenements,
                    new DonneesWhatIf(),
                    1);

        Assert.That(
            texte,
            Does.Contain("hors optimisation boursière"));
    }

    [Test]
    public void AucunEvenement_AfficheUnMessageExplicite()
    {
        string texte =
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireSectionEvenementsConfirmes(
                    new DonneesEvenements(),
                    new DonneesWhatIf(),
                    0);

        Assert.That(
            texte,
            Does.Contain("Aucun événement confirmé"));
    }

    private static OrdreHistoriqueWhatIf Ordre(
        int mois,
        TypeOrdreHistoriqueWhatIf type,
        string actifId,
        int montant)
    {
        return new OrdreHistoriqueWhatIf
        {
            indexMois = mois,
            type = type,
            actifId = actifId,
            quantite = 1f,
            prixUnitaireCentimes = montant,
            montantCentimes = montant,
            raison = "test"
        };
    }

    private static EvenementConfirmePartie Evenement(
        string id,
        string titre,
        int mois,
        string categorie)
    {
        return new EvenementConfirmePartie
        {
            definitionId = id,
            titre = titre,
            moisConfirmation = mois,
            categorie = categorie,
            impacts =
                new List<ImpactDefinitionEvenement>()
        };
    }
}