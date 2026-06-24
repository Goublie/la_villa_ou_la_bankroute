using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class GameDataWhatIfTests
{
    [Test]
    public void Creation_InitialiseEtatWhatIf()
    {
        GameData gameData = CreerGameData();
        try
        {
            Assert.That(gameData.whatIf, Is.Not.Null);
            Assert.That(gameData.whatIf.portefeuille, Is.Not.Null);
            Assert.That(gameData.whatIf.configuration, Is.Not.Null);
            Assert.That(gameData.whatIf.decisions, Is.Not.Null);
            Assert.That(gameData.whatIf.historique, Is.Not.Null);
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void ResetData_RecreeUnEtatWhatIfVide()
    {
        GameData gameData = CreerGameData();
        try
        {
            DonneesWhatIf ancien = gameData.whatIf;
            ancien.initialisee = true;
            ancien.capitalInitialCentimes = 120000;
            ancien.liquiditesCentimes = 40000;
            ancien.decisions.Add(
                new DecisionWhatIf { indexMois = 2 });
            ancien.historique.Add(
                new PointHistoriqueWhatIf { indexMois = 2 });
            gameData.historiqueSnapshots.Add(null);

            gameData.ResetData();

            Assert.That(gameData.whatIf, Is.Not.SameAs(ancien));
            Assert.That(gameData.whatIf.initialisee, Is.False);
            Assert.That(gameData.whatIf.capitalInitialCentimes, Is.Zero);
            Assert.That(gameData.whatIf.liquiditesCentimes, Is.Zero);
            Assert.That(gameData.whatIf.decisions, Is.Empty);
            Assert.That(gameData.whatIf.historique, Is.Empty);
            Assert.That(gameData.historiqueSnapshots, Is.Empty);
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void OnEnable_RepareEtatWhatIfAbsent()
    {
        GameData gameData = CreerGameData();
        try
        {
            gameData.whatIf = null;

            InvoquerOnEnable(gameData);

            Assert.That(gameData.whatIf, Is.Not.Null);
            Assert.That(gameData.whatIf.configuration, Is.Not.Null);
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void OnEnable_RepareCollectionsWhatIfAbsentes()
    {
        GameData gameData = CreerGameData();
        try
        {
            gameData.whatIf.portefeuille = null;
            gameData.whatIf.configuration = null;
            gameData.whatIf.decisions = null;
            gameData.whatIf.historique = null;

            InvoquerOnEnable(gameData);

            Assert.That(gameData.whatIf.portefeuille, Is.Not.Null);
            Assert.That(gameData.whatIf.configuration, Is.Not.Null);
            Assert.That(gameData.whatIf.decisions, Is.Not.Null);
            Assert.That(gameData.whatIf.historique, Is.Not.Null);
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void OnEnable_NormaliseConfigurationWhatIf()
    {
        GameData gameData = CreerGameData();
        try
        {
            gameData.whatIf.configuration.horizonMois = 0;
            gameData.whatIf.configuration.largeurFaisceau = 0;
            gameData.whatIf.configuration.pasAllocationPourcent = 0;
            gameData.whatIf.configuration.penaliteRisque = -2f;
            gameData.whatIf.configuration.penaliteDrawdown = -3f;
            gameData.whatIf.configuration.coutTransactionCentimes = -500;

            InvoquerOnEnable(gameData);

            Assert.That(
                gameData.whatIf.configuration.horizonMois,
                Is.EqualTo(1));
            Assert.That(
                gameData.whatIf.configuration.largeurFaisceau,
                Is.EqualTo(2));
            Assert.That(
                gameData.whatIf.configuration.pasAllocationPourcent,
                Is.EqualTo(1));
            Assert.That(
                gameData.whatIf.configuration.penaliteRisque,
                Is.Zero);
            Assert.That(
                gameData.whatIf.configuration.penaliteDrawdown,
                Is.Zero);
            Assert.That(
                gameData.whatIf.configuration.coutTransactionCentimes,
                Is.Zero);
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void ResetData_NeReutilisePasLePortefeuilleAlternatif()
    {
        GameData gameData = CreerGameData();
        try
        {
            DonneesBourse ancienPortefeuille =
                gameData.whatIf.portefeuille;
            ancienPortefeuille.positions.Add(
                new PositionBourse("nvidia"));

            gameData.ResetData();

            Assert.That(
                gameData.whatIf.portefeuille,
                Is.Not.SameAs(ancienPortefeuille));
            Assert.That(
                gameData.whatIf.portefeuille.positions,
                Is.Empty);
        }
        finally
        {
            Detruire(gameData);
        }
    }

    private static GameData CreerGameData()
    {
        return ScriptableObject.CreateInstance<GameData>();
    }

    private static void InvoquerOnEnable(GameData gameData)
    {
        MethodInfo methode = typeof(GameData).GetMethod(
            "OnEnable",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.That(methode, Is.Not.Null);
        methode.Invoke(gameData, null);
    }

    private static void Detruire(GameData gameData)
    {
        if (gameData != null)
        {
            Object.DestroyImmediate(gameData);
        }
    }
}