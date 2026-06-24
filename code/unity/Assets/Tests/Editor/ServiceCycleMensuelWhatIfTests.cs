using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ServiceCycleMensuelWhatIfTests
{
    [Test]
    public void ConstruirePrixObserves_RetourneDesPrixPositifs()
    {
        Dictionary<string, int> prix =
            ServiceCycleMensuelWhatIf.ConstruirePrixObserves(
                new DonneesBourse(),
                0);

        Assert.That(prix.Count, Is.GreaterThanOrEqualTo(5));
        Assert.That(prix["cac40"], Is.GreaterThan(0));
        Assert.That(prix["nvidia"], Is.GreaterThan(0));
    }

    [Test]
    public void OuvrirMois_InitialiseDepuisLePatrimoineReel()
    {
        GameData gameData = CreerGameData();
        try
        {
            int patrimoine =
                ServicePatrimoine.Calculer(
                    gameData.joueur).centimes;

            ResultatCycleMensuelWhatIf resultat =
                ServiceCycleMensuelWhatIf.OuvrirMois(
                    gameData);

            Assert.That(resultat.succes, Is.True);
            Assert.That(gameData.whatIf.initialisee, Is.True);
            Assert.That(
                gameData.whatIf.capitalInitialCentimes,
                Is.EqualTo(patrimoine));
            Assert.That(gameData.whatIf.decisions.Count, Is.EqualTo(1));
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void OuvrirMois_NeModifiePasLeJoueurReel()
    {
        GameData gameData = CreerGameData();
        try
        {
            int soldeAvant =
                gameData.joueur
                    .comptes[ServiceBanque.CompteCourantId]
                    .GetSolde().centimes;
            int positionsAvant =
                gameData.joueur.bourse.positions.Count;

            ServiceCycleMensuelWhatIf.OuvrirMois(gameData);

            Assert.That(
                gameData.joueur
                    .comptes[ServiceBanque.CompteCourantId]
                    .GetSolde().centimes,
                Is.EqualTo(soldeAvant));
            Assert.That(
                gameData.joueur.bourse.positions.Count,
                Is.EqualTo(positionsAvant));
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void OuvrirMois_CopieLesImpactsConnus()
    {
        GameData gameData = CreerGameData();
        try
        {
            gameData.joueur.bourse.impactsMarche.Add(
                new ImpactEvenementMarche
                {
                    evenementId = "evenement-test",
                    actifId = "nvidia",
                    moisDebut = 0,
                    dureeMois = 2,
                    coefficientPrix = 0.9f
                });

            ServiceCycleMensuelWhatIf.OuvrirMois(gameData);

            Assert.That(
                gameData.whatIf.portefeuille.impactsMarche.Count,
                Is.EqualTo(1));
            Assert.That(
                gameData.whatIf.portefeuille.impactsMarche[0],
                Is.Not.SameAs(
                    gameData.joueur.bourse.impactsMarche[0]));
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void CloturerMois_ReproduitLesFluxEtAjouteUnPoint()
    {
        GameData gameData = CreerGameData();
        try
        {
            ServiceCycleMensuelWhatIf.OuvrirMois(gameData);
            int avant =
                gameData.whatIf.liquiditesCentimes;

            CompteBanquaire courant =
                gameData.joueur
                    .comptes[ServiceBanque.CompteCourantId];
            courant.AjoutHistorique(
                "salaire",
                new argent(20000));
            courant.AjoutHistorique(
                "loyer",
                new argent(-5000));

            ResultatCycleMensuelWhatIf resultat =
                ServiceCycleMensuelWhatIf.CloturerMois(
                    gameData,
                    0,
                    Mois.Juillet);

            Assert.That(resultat.succes, Is.True);
            Assert.That(resultat.flux.fluxNetCentimes, Is.EqualTo(15000));
            Assert.That(
                gameData.whatIf.historique.Count,
                Is.EqualTo(1));
            Assert.That(
                gameData.whatIf.liquiditesCentimes,
                Is.GreaterThanOrEqualTo(avant));
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void CloturerMois_NeVidePasLesHistoriquesReels()
    {
        GameData gameData = CreerGameData();
        try
        {
            ServiceCycleMensuelWhatIf.OuvrirMois(gameData);
            CompteBanquaire courant =
                gameData.joueur
                    .comptes[ServiceBanque.CompteCourantId];
            courant.AjoutHistorique(
                "depense-test",
                new argent(-1000));

            int avant =
                courant.GetHistorique().GetSize();

            ServiceCycleMensuelWhatIf.CloturerMois(
                gameData,
                0,
                Mois.Juillet);

            Assert.That(
                courant.GetHistorique().GetSize(),
                Is.EqualTo(avant));
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void CloturerMois_RemplaceLePointDuMemeMois()
    {
        GameData gameData = CreerGameData();
        try
        {
            ServiceCycleMensuelWhatIf.OuvrirMois(gameData);

            ServiceCycleMensuelWhatIf.CloturerMois(
                gameData,
                0,
                Mois.Juillet);
            ServiceCycleMensuelWhatIf.CloturerMois(
                gameData,
                0,
                Mois.Juillet);

            Assert.That(
                gameData.whatIf.historique.Count,
                Is.EqualTo(1));
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void InitialiserPartie_BrancheAutomatiquementLeWhatIf()
    {
        GameData gameData = CreerGameData();
        try
        {
            new ServicePassageMensuel(gameData)
                .InitialiserPartie();

            Assert.That(gameData.whatIf.initialisee, Is.True);
            Assert.That(
                gameData.whatIf.decisions.Count,
                Is.EqualTo(1));
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void PasserAuMoisSuivant_CloturePuisOuvreLeWhatIf()
    {
        GameData gameData = CreerGameData();
        try
        {
            ServicePassageMensuel service =
                new ServicePassageMensuel(gameData);
            service.InitialiserPartie();

            gameData.joueur
                .comptes[ServiceBanque.CompteCourantId]
                .AjoutHistorique(
                    "loyer",
                    new argent(-1000));

            service.PasserAuMoisSuivant();

            Assert.That(
                gameData.whatIf.historique.Count,
                Is.EqualTo(1));
            Assert.That(
                gameData.whatIf.decisions.Count,
                Is.EqualTo(2));
            Assert.That(
                gameData.whatIf.dernierMoisTraite,
                Is.EqualTo(0));
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void Cycle_NullGameDataRetourneUnEchecPropre()
    {
        ResultatCycleMensuelWhatIf ouverture =
            ServiceCycleMensuelWhatIf.OuvrirMois(null);
        ResultatCycleMensuelWhatIf cloture =
            ServiceCycleMensuelWhatIf.CloturerMois(
                null,
                0,
                Mois.Juillet);

        Assert.That(ouverture.succes, Is.False);
        Assert.That(cloture.succes, Is.False);
        Assert.That(ouverture.diagnostics, Is.Not.Empty);
        Assert.That(cloture.diagnostics, Is.Not.Empty);
    }

    private static GameData CreerGameData()
    {
        GameData gameData =
            ScriptableObject.CreateInstance<GameData>();
        gameData.ResetData();
        return gameData;
    }

    private static void Detruire(GameData gameData)
    {
        if (gameData != null)
        {
            Object.DestroyImmediate(gameData);
        }
    }
}