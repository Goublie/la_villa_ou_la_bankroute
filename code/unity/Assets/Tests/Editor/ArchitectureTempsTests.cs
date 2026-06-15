using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Caracterise l'ordre du passage mensuel et le routage des evenements.
/// </summary>
public class ArchitectureTempsTests
{
    [Test]
    public void PassageMensuel_SnapshotPrecedeSalaireEtValorisationOuverture()
    {
        GameData gameData = CreerGameData();
        try
        {
            gameData.joueur.salaire = new argent(10000);
            ServicePassageMensuel service =
                new ServicePassageMensuel(gameData);

            ResultatPassageMensuel resultat =
                service.PasserAuMoisSuivant();

            Assert.That(resultat.Succes, Is.True);
            Assert.That(resultat.IndexMoisCloture, Is.EqualTo(0));
            Assert.That(resultat.IndexMoisOuverture, Is.EqualTo(1));
            Assert.That(gameData.moisActuel, Is.EqualTo(Mois.Aout));
            Assert.That(
                gameData.joueur.comptes[ServiceBanque.CompteCourantId]
                    .GetSolde().centimes,
                Is.EqualTo(110000));
            Assert.That(
                gameData.historiqueSnapshots[0].joueur.comptes[
                    ServiceBanque.CompteCourantId]
                    .GetSolde().centimes,
                Is.EqualTo(100000));
            Assert.That(
                gameData.joueur.bourse.dernierMoisObserve,
                Is.EqualTo(1));
            Assert.That(
                gameData.joueur.entrepreneuriat.dernierMoisObserve,
                Is.EqualTo(1));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void PassageMensuel_NAppliqueUnPlacementFixeQuUneFois()
    {
        GameData gameData = CreerGameData();
        try
        {
            gameData.joueur.investissements.Add(
                new Investissement(new argent(100000), 0.12f, 1));

            new ServicePassageMensuel(gameData)
                .PasserAuMoisSuivant();

            Assert.That(
                gameData.joueur.investissements[0]
                    .sommeInvestie.centimes,
                Is.EqualTo(112000));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void PassageMensuel_SignaleLePassageDeDecembreAJanvier()
    {
        GameData gameData = CreerGameData();
        try
        {
            gameData.nombreMoisPasses = 5;
            gameData.moisActuel = Mois.Decembre;

            ResultatPassageMensuel resultat =
                new ServicePassageMensuel(gameData)
                    .PasserAuMoisSuivant();

            Assert.That(resultat.ChangementAnnee, Is.True);
            Assert.That(gameData.moisActuel, Is.EqualTo(Mois.Janvier));
            Assert.That(gameData.nombreMoisPasses, Is.EqualTo(6));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void Initialisation_EstIdempotenteEtNeVersePasLeSalaire()
    {
        GameData gameData = CreerGameData();
        try
        {
            gameData.joueur.salaire = new argent(10000);
            ServicePassageMensuel service =
                new ServicePassageMensuel(gameData);

            service.InitialiserPartie();
            service.InitialiserPartie();

            Assert.That(gameData.historiqueSnapshots.Count, Is.EqualTo(1));
            Assert.That(
                gameData.joueur.comptes[ServiceBanque.CompteCourantId]
                    .GetSolde().centimes,
                Is.EqualTo(100000));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void EvenementEconomique_CibleLeDomaineEtPersisteDansLeSnapshot()
    {
        GameData gameData = CreerGameData();
        try
        {
            ImpactEvenementMarche impact =
                new ImpactEvenementMarche
                {
                    evenementId = "baisse_test",
                    actifId = "nvidia",
                    moisDebut = 0,
                    dureeMois = 2,
                    coefficientPrix = 0.75f
                };

            ResultatOperation resultat =
                new ServiceEvenementsEconomiques(gameData)
                    .AppliquerImpactMarche(impact);
            SnapshotEtatJeu snapshot = new SnapshotEtatJeu(gameData);

            Assert.That(resultat.Succes, Is.True);
            Assert.That(gameData.joueur.bourse.impactsMarche.Count, Is.EqualTo(1));
            Assert.That(snapshot.joueur.bourse.impactsMarche.Count, Is.EqualTo(1));
            Assert.That(
                snapshot.joueur.bourse.impactsMarche[0],
                Is.Not.SameAs(gameData.joueur.bourse.impactsMarche[0]));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    private static GameData CreerGameData()
    {
        GameData gameData =
            ScriptableObject.CreateInstance<GameData>();
        gameData.ResetData();
        return gameData;
    }
}
