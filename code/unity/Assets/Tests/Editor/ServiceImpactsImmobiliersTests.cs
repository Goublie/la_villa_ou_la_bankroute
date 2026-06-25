using NUnit.Framework;

public class ServiceImpactsImmobiliersTests
{
    [Test]
    public void CategorieImmobiliers_EstValide()
    {
        Assert.IsTrue(
            CategoriesEvenements.EstValide(
                CategoriesEvenements.Immobiliers));
    }

    [Test]
    public void Conversion_ProduitUnImpactPersistant()
    {
        ImpactDefinitionEvenement definition =
            new ImpactDefinitionEvenement
            {
                actif = "Immobilier",
                ville = "Toulouse",
                typeBien = "Studio",
                variation = 0.08f,
                variationLoyer = 0.03f,
                dureeMois = 6
            };

        bool succes = ServiceImpactsImmobiliers.EssayerConvertir(
            definition,
            "rumeur-immo",
            10,
            out ImpactEvenementImmobilier impact,
            out string erreur);

        Assert.IsTrue(succes, erreur);
        Assert.AreEqual(10, impact.moisDebut);
        Assert.AreEqual(6, impact.dureeMois);
        Assert.AreEqual(1.08f, impact.coefficientPrix, 0.0001f);
        Assert.AreEqual(1.03f, impact.coefficientLoyer, 0.0001f);
        Assert.IsTrue(impact.Cible(Ville.Toulouse, TypeBien.Studio));
        Assert.IsFalse(impact.Cible(Ville.Paris, TypeBien.Studio));
    }

    [Test]
    public void Impact_NEstActifQuePendantSaDuree()
    {
        ImpactEvenementImmobilier impact =
            new ImpactEvenementImmobilier
            {
                moisDebut = 4,
                dureeMois = 3
            };

        Assert.IsFalse(impact.EstActif(3));
        Assert.IsTrue(impact.EstActif(4));
        Assert.IsTrue(impact.EstActif(6));
        Assert.IsFalse(impact.EstActif(7));
    }

    [Test]
    public void Coefficients_SeMultiplientSurLesCiblesCompatibles()
    {
        DonneesImmobilier donnees = new DonneesImmobilier();
        donnees.impactsActifs.Add(
            new ImpactEvenementImmobilier
            {
                evenementId = "global",
                moisDebut = 0,
                dureeMois = 12,
                villeCible = "Tous",
                typeBienCible = "Tous",
                coefficientPrix = 1.10f,
                coefficientLoyer = 1.02f
            });
        donnees.impactsActifs.Add(
            new ImpactEvenementImmobilier
            {
                evenementId = "paris",
                moisDebut = 0,
                dureeMois = 12,
                villeCible = "Paris",
                typeBienCible = "Studio",
                coefficientPrix = 1.05f,
                coefficientLoyer = 1.04f
            });

        Assert.AreEqual(
            1.155f,
            ServiceImpactsImmobiliers.CalculerCoefficientPrix(
                donnees,
                Ville.Paris,
                TypeBien.Studio,
                2),
            0.0001f);
        Assert.AreEqual(
            1.10f,
            ServiceImpactsImmobiliers.CalculerCoefficientPrix(
                donnees,
                Ville.Lyon,
                TypeBien.Studio,
                2),
            0.0001f);
    }

    [Test]
    public void CopierImmobilier_IsoleLesImpacts()
    {
        DonneesImmobilier source = new DonneesImmobilier();
        source.impactsActifs.Add(
            new ImpactEvenementImmobilier
            {
                evenementId = "e1",
                coefficientPrix = 1.2f
            });

        DonneesImmobilier copie = source.Copier();
        copie.impactsActifs[0].coefficientPrix = 0.5f;

        Assert.AreEqual(1.2f, source.impactsActifs[0].coefficientPrix);
    }

    [Test]
    public void Conversion_RefuseUneVilleInconnue()
    {
        bool succes = ServiceImpactsImmobiliers.EssayerConvertir(
            new ImpactDefinitionEvenement
            {
                actif = "Immobilier",
                ville = "Atlantide",
                typeBien = "Tous",
                dureeMois = 4
            },
            "e2",
            0,
            out _,
            out string erreur);

        Assert.IsFalse(succes);
        StringAssert.Contains("Ville", erreur);
    }
}
