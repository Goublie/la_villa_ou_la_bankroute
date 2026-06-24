using System.Collections.Generic;
using NUnit.Framework;

public class ServicePrevisionWhatIfTests
{
    [Test]
    public void EstimerDepuisPrixConnus_IgnoreLesPrixFuturs()
    {
        List<float> futurTresHaut =
            new List<float> { 100f, 110f, 121f, 9999f };
        List<float> futurTresBas =
            new List<float> { 100f, 110f, 121f, 1f };

        PrevisionActifWhatIf premiere =
            ServicePrevisionWhatIf.EstimerDepuisPrixConnus(
                "test",
                futurTresHaut,
                null,
                2);
        PrevisionActifWhatIf seconde =
            ServicePrevisionWhatIf.EstimerDepuisPrixConnus(
                "test",
                futurTresBas,
                null,
                2);

        Assert.That(
            premiere.rendementMensuelEstimePourcent,
            Is.EqualTo(seconde.rendementMensuelEstimePourcent).Within(0.0001f));
        Assert.That(
            premiere.risqueEstimePourcent,
            Is.EqualTo(seconde.risqueEstimePourcent).Within(0.0001f));
        Assert.That(
            premiere.drawdownHistoriquePourcent,
            Is.EqualTo(seconde.drawdownHistoriquePourcent).Within(0.0001f));
    }

    [Test]
    public void EstimerDepuisPrixConnus_EstDeterministe()
    {
        List<float> prix =
            new List<float> { 100f, 105f, 102f, 110f, 108f };

        PrevisionActifWhatIf premiere =
            ServicePrevisionWhatIf.EstimerDepuisPrixConnus(
                "test",
                prix,
                null,
                4);
        PrevisionActifWhatIf seconde =
            ServicePrevisionWhatIf.EstimerDepuisPrixConnus(
                "test",
                prix,
                null,
                4);

        Assert.That(
            premiere.rendementMensuelEstimePourcent,
            Is.EqualTo(seconde.rendementMensuelEstimePourcent));
        Assert.That(
            premiere.risqueEstimePourcent,
            Is.EqualTo(seconde.risqueEstimePourcent));
        Assert.That(premiere.explication, Is.EqualTo(seconde.explication));
    }

    [Test]
    public void EvenementActif_AjouteSeulementSaTendanceConnue()
    {
        List<float> prix =
            new List<float> { 100f, 100f, 100f, 100f };
        List<ImpactEvenementMarche> impacts =
            new List<ImpactEvenementMarche>
            {
                new ImpactEvenementMarche
                {
                    evenementId = "evt_connu",
                    actifId = "test",
                    moisDebut = 1,
                    dureeMois = 6,
                    tendanceMensuellePourcent = 2.5f,
                    coefficientPrix = 0.5f,
                    coefficientVolatilite = 1f
                }
            };

        PrevisionActifWhatIf resultat =
            ServicePrevisionWhatIf.EstimerDepuisPrixConnus(
                "test",
                prix,
                impacts,
                3);

        Assert.That(resultat.nombreImpactsActifs, Is.EqualTo(1));
        Assert.That(
            resultat.effetEvenementsConnusPourcent,
            Is.EqualTo(2.5f).Within(0.0001f));
        Assert.That(
            resultat.rendementMensuelEstimePourcent,
            Is.EqualTo(2.5f).Within(0.0001f));
    }

    [Test]
    public void EvenementFutur_EstIgnore()
    {
        List<ImpactEvenementMarche> impacts =
            new List<ImpactEvenementMarche>
            {
                new ImpactEvenementMarche
                {
                    evenementId = "evt_futur",
                    actifId = "test",
                    moisDebut = 4,
                    dureeMois = 3,
                    tendanceMensuellePourcent = 50f,
                    coefficientVolatilite = 5f
                }
            };

        PrevisionActifWhatIf resultat =
            ServicePrevisionWhatIf.EstimerDepuisPrixConnus(
                "test",
                new List<float> { 100f, 101f, 102f },
                impacts,
                2);

        Assert.That(resultat.nombreImpactsActifs, Is.Zero);
        Assert.That(resultat.effetEvenementsConnusPourcent, Is.Zero);
    }

    [Test]
    public void EvenementAutreActif_EstIgnore()
    {
        List<ImpactEvenementMarche> impacts =
            new List<ImpactEvenementMarche>
            {
                new ImpactEvenementMarche
                {
                    evenementId = "evt_autre",
                    actifId = "autre",
                    moisDebut = 0,
                    dureeMois = -1,
                    tendanceMensuellePourcent = 50f
                }
            };

        PrevisionActifWhatIf resultat =
            ServicePrevisionWhatIf.EstimerDepuisPrixConnus(
                "test",
                new List<float> { 100f, 100f, 100f },
                impacts,
                2);

        Assert.That(resultat.nombreImpactsActifs, Is.Zero);
        Assert.That(resultat.effetEvenementsConnusPourcent, Is.Zero);
    }

    [Test]
    public void CoefficientVolatilite_AmplifieLeRisqueMaisPasLeRendement()
    {
        List<float> prix =
            new List<float> { 100f, 110f, 99f, 108.9f };
        List<ImpactEvenementMarche> impacts =
            new List<ImpactEvenementMarche>
            {
                new ImpactEvenementMarche
                {
                    evenementId = "evt_volatilite",
                    actifId = "test",
                    moisDebut = 0,
                    dureeMois = -1,
                    tendanceMensuellePourcent = 0f,
                    coefficientVolatilite = 1.5f
                }
            };

        PrevisionActifWhatIf sansImpact =
            ServicePrevisionWhatIf.EstimerDepuisPrixConnus(
                "test",
                prix,
                null,
                3);
        PrevisionActifWhatIf avecImpact =
            ServicePrevisionWhatIf.EstimerDepuisPrixConnus(
                "test",
                prix,
                impacts,
                3);

        Assert.That(
            avecImpact.rendementMensuelEstimePourcent,
            Is.EqualTo(sansImpact.rendementMensuelEstimePourcent)
                .Within(0.0001f));
        Assert.That(
            avecImpact.risqueEstimePourcent,
            Is.EqualTo(sansImpact.risqueEstimePourcent * 1.5f)
                .Within(0.001f));
    }

    [Test]
    public void DrawdownHistorique_EstCalculeSurLesPrixConnus()
    {
        PrevisionActifWhatIf resultat =
            ServicePrevisionWhatIf.EstimerDepuisPrixConnus(
                "test",
                new List<float> { 100f, 120f, 90f, 95f },
                null,
                3);

        Assert.That(
            resultat.drawdownHistoriquePourcent,
            Is.EqualTo(25f).Within(0.0001f));
    }

    [Test]
    public void HistoriqueInsuffisant_ProduitUnePrevisionNeutre()
    {
        PrevisionActifWhatIf resultat =
            ServicePrevisionWhatIf.EstimerDepuisPrixConnus(
                "test",
                new List<float> { 100f },
                null,
                0);

        Assert.That(resultat.nombreObservations, Is.Zero);
        Assert.That(resultat.rendementMensuelEstimePourcent, Is.Zero);
        Assert.That(resultat.risqueEstimePourcent, Is.Zero);
        Assert.That(resultat.confiance01, Is.Zero);
    }

    [Test]
    public void Confiance_ResteToujoursEntreZeroEtUn()
    {
        PrevisionActifWhatIf resultat =
            ServicePrevisionWhatIf.EstimerDepuisPrixConnus(
                "test",
                new List<float>
                {
                    100f, 130f, 70f, 140f, 60f, 150f, 55f,
                    160f, 50f, 170f, 45f, 180f, 40f
                },
                null,
                12);

        Assert.That(resultat.confiance01, Is.InRange(0f, 1f));
    }

    [Test]
    public void Estimer_NeModifiePasLePortefeuilleConnu()
    {
        DefinitionActifFinancier actif = CatalogueActifs.Trouver("cac40");
        Assert.That(actif, Is.Not.Null);

        DonneesBourse donnees = new DonneesBourse();
        donnees.positions.Add(
            new PositionBourse("cac40")
            {
                quantite = 2f,
                coutTotalCentimes = 100000
            });
        donnees.impactsMarche.Add(
            new ImpactEvenementMarche
            {
                evenementId = "evt_test",
                actifId = "cac40",
                moisDebut = 0,
                dureeMois = 3,
                tendanceMensuellePourcent = 1f
            });

        DonneesBourse avant = donnees.Copier();

        ServicePrevisionWhatIf.Estimer(actif, donnees, 2);

        Assert.That(donnees.positions.Count, Is.EqualTo(avant.positions.Count));
        Assert.That(
            donnees.positions[0].quantite,
            Is.EqualTo(avant.positions[0].quantite));
        Assert.That(
            donnees.positions[0].coutTotalCentimes,
            Is.EqualTo(avant.positions[0].coutTotalCentimes));
        Assert.That(
            donnees.impactsMarche.Count,
            Is.EqualTo(avant.impactsMarche.Count));
        Assert.That(
            donnees.impactsMarche[0].evenementId,
            Is.EqualTo(avant.impactsMarche[0].evenementId));
        Assert.That(
            donnees.valeurMarcheCentimes,
            Is.EqualTo(avant.valeurMarcheCentimes));
        Assert.That(
            donnees.moisValorisation,
            Is.EqualTo(avant.moisValorisation));
    }
}