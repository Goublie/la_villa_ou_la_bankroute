using System.Collections.Generic;
using NUnit.Framework;

public class ServiceRetrospectionDetailleeWhatIfTests
{
    [Test]
    public void OrdresDouzeDerniersMois_SontFiltresEtTries()
    {
        DonneesWhatIf donnees = new DonneesWhatIf();
        donnees.InitialiserSiNecessaire();

        for (int mois = 0; mois < 15; mois++)
        {
            donnees.ordres.Add(
                Ordre(
                    mois,
                    TypeOrdreHistoriqueWhatIf.Achat,
                    "actif",
                    100));
        }

        List<LigneOperationRetrospectionWhatIf> lignes =
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireLignesOrdres(donnees, 14);

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
                .ConstruireLignesOrdres(donnees, 2)[0];

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
                1,
                TypeOrdreHistoriqueWhatIf.VenteForcee,
                "bitcoin",
                10000));

        LigneOperationRetrospectionWhatIf ligne =
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireLignesOrdres(donnees, 1)[0];

        Assert.That(ligne.operation, Is.EqualTo("VENTE FORCÉE"));
    }

    [Test]
    public void EvenementsConfirmes_ExcluentLesPlusAnciens()
    {
        DonneesEvenements evenements = new DonneesEvenements();
        evenements.evenementsConfirmes.Add(
            Evenement("ancien", "Ancien événement", 0, "Personnels"));
        evenements.evenementsConfirmes.Add(
            Evenement("recent", "Événement récent", 5, "Personnels"));

        string texte =
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireSectionEvenementsConfirmes(
                    evenements,
                    new DonneesWhatIf(),
                    12);

        Assert.That(texte, Does.Not.Contain("Ancien événement"));
        Assert.That(texte, Does.Contain("Événement récent"));
    }

    [Test]
    public void EvenementBoursier_AfficheSesImpacts()
    {
        DonneesEvenements evenements = new DonneesEvenements();
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

        Assert.That(texte, Does.Contain("nvidia -15 %"));
    }

    [Test]
    public void EvenementBoursier_IndiqueLePremierMoisDePriseEnCompte()
    {
        DonneesEvenements evenements = new DonneesEvenements();
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

        DecisionWhatIf decision =
            new DecisionWhatIf
            {
                indexMois = 4,
                evenementsConnusIds = new List<string>
                {
                    "evt_1"
                }
            };

        donnees.decisions.Add(decision);

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
        DonneesEvenements evenements = new DonneesEvenements();
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
            impacts = new List<ImpactDefinitionEvenement>()
        };
    }
}