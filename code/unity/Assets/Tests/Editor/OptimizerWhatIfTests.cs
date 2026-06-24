using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class OptimizerWhatIfTests
{
    [Test]
    public void HistoriquesVides_SansPointsWhatIf()
    {
        GameData gameData = CreerGameData();
        try
        {
            Assert.That(
                Optimizer.ObtenirHistoriqueReel(gameData),
                Is.Empty);
            Assert.That(
                Optimizer.ObtenirHistoriqueWhatIf(gameData),
                Is.Empty);
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void HistoriquesReelEtAlternatif_SontAlignes()
    {
        GameData gameData = CreerGameData();
        try
        {
            AjouterPoint(
                gameData,
                2,
                Mois.Septembre,
                120000,
                135000,
                35000,
                100000);

            List<Optimizer.SimulationResult> reel =
                Optimizer.ObtenirHistoriqueReel(gameData);
            List<Optimizer.SimulationResult> alternatif =
                Optimizer.ObtenirHistoriqueWhatIf(gameData);

            Assert.That(reel.Count, Is.EqualTo(1));
            Assert.That(alternatif.Count, Is.EqualTo(1));
            Assert.That(
                reel[0].indexMois,
                Is.EqualTo(alternatif[0].indexMois));
            Assert.That(
                reel[0].moisCalendrier,
                Is.EqualTo(alternatif[0].moisCalendrier));
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void HistoriqueAlternatif_ExposeLiquiditesEtBourse()
    {
        GameData gameData = CreerGameData();
        try
        {
            AjouterPoint(
                gameData,
                0,
                Mois.Juillet,
                100000,
                125000,
                25000,
                100000);

            Optimizer.SimulationResult point =
                Optimizer.ObtenirHistoriqueWhatIf(gameData)[0];

            Assert.That(
                point.soldeCourant.centimes,
                Is.EqualTo(25000));
            Assert.That(
                point.soldeEpargne.centimes,
                Is.EqualTo(100000));
            Assert.That(
                point.patrimoineTotal.centimes,
                Is.EqualTo(125000));
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void HistoriqueReel_UtiliseLePatrimoineEnregistre()
    {
        GameData gameData = CreerGameData();
        try
        {
            AjouterPoint(
                gameData,
                0,
                Mois.Juillet,
                98765,
                125000,
                25000,
                100000);

            Optimizer.SimulationResult point =
                Optimizer.ObtenirHistoriqueReel(gameData)[0];

            Assert.That(
                point.patrimoineTotal.centimes,
                Is.EqualTo(98765));
            Assert.That(
                point.soldeCourant.centimes,
                Is.EqualTo(98765));
            Assert.That(
                point.soldeEpargne.centimes,
                Is.Zero);
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void Historique_EstTrieEtLimiteAuxTreizeDerniersPoints()
    {
        GameData gameData = CreerGameData();
        try
        {
            for (int index = 14; index >= 0; index--)
            {
                AjouterPoint(
                    gameData,
                    index,
                    (Mois)(index % 12),
                    100000 + index,
                    110000 + index,
                    10000,
                    100000);
            }

            List<Optimizer.SimulationResult> resultat =
                Optimizer.ObtenirHistoriqueWhatIf(gameData);

            Assert.That(resultat.Count, Is.EqualTo(13));
            Assert.That(resultat[0].indexMois, Is.EqualTo(2));
            Assert.That(resultat[12].indexMois, Is.EqualTo(14));
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void SimulerFourmi_DelegueVersLeNouveauWhatIf()
    {
        GameData gameData = CreerGameData();
        try
        {
            AjouterPoint(
                gameData,
                1,
                Mois.Aout,
                100000,
                140000,
                40000,
                100000);

            List<Optimizer.SimulationResult> ancienNom =
                Optimizer.SimulerFourmi(gameData);
            List<Optimizer.SimulationResult> nouveauNom =
                Optimizer.ObtenirHistoriqueWhatIf(gameData);

            Assert.That(
                ancienNom.Count,
                Is.EqualTo(nouveauNom.Count));
            Assert.That(
                ancienNom[0].patrimoineTotal.centimes,
                Is.EqualTo(
                    nouveauNom[0].patrimoineTotal.centimes));
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void Lecture_NeModifiePasLesPointsPersistants()
    {
        GameData gameData = CreerGameData();
        try
        {
            AjouterPoint(
                gameData,
                5,
                Mois.Decembre,
                100000,
                120000,
                20000,
                100000);

            PointHistoriqueWhatIf source =
                gameData.whatIf.historique[0];

            Optimizer.ObtenirHistoriqueReel(gameData);
            Optimizer.ObtenirHistoriqueWhatIf(gameData);

            Assert.That(
                gameData.whatIf.historique[0],
                Is.SameAs(source));
            Assert.That(
                source.patrimoineAlternatifCentimes,
                Is.EqualTo(120000));
        }
        finally
        {
            Detruire(gameData);
        }
    }

    [Test]
    public void CompatibiliteSnapshots_SansHistoriqueWhatIf()
    {
        GameData gameData = CreerGameData();
        try
        {
            gameData.historiqueSnapshots.Add(
                new SnapshotEtatJeu(gameData));

            gameData.nombreMoisPasses = 1;
            gameData.moisActuel = Mois.Aout;
            gameData.historiqueSnapshots.Add(
                new SnapshotEtatJeu(gameData));

            int snapshotsAvant =
                gameData.historiqueSnapshots.Count;
            int soldeAvant =
                gameData.joueur
                    .comptes[ServiceBanque.CompteCourantId]
                    .GetSolde().centimes;

            List<Optimizer.SimulationResult> reel =
                Optimizer.ObtenirHistoriqueReel(gameData);
            List<Optimizer.SimulationResult> alternatif =
                Optimizer.ObtenirHistoriqueWhatIf(gameData);

            Assert.That(reel.Count, Is.EqualTo(2));
            Assert.That(alternatif.Count, Is.EqualTo(2));
            Assert.That(
                gameData.historiqueSnapshots.Count,
                Is.EqualTo(snapshotsAvant));
            Assert.That(
                gameData.joueur
                    .comptes[ServiceBanque.CompteCourantId]
                    .GetSolde().centimes,
                Is.EqualTo(soldeAvant));
        }
        finally
        {
            Detruire(gameData);
        }
    }
    private static GameData CreerGameData()
    {
        GameData gameData =
            ScriptableObject.CreateInstance<GameData>();
        gameData.ResetData();
        return gameData;
    }

    private static void AjouterPoint(
        GameData gameData,
        int indexMois,
        Mois mois,
        int patrimoineReel,
        int patrimoineAlternatif,
        int liquidites,
        int valeurBourse)
    {
        gameData.whatIf.historique.Add(
            new PointHistoriqueWhatIf
            {
                indexMois = indexMois,
                moisCalendrier = mois,
                patrimoineReelCentimes = patrimoineReel,
                patrimoineAlternatifCentimes =
                    patrimoineAlternatif,
                liquiditesAlternativesCentimes = liquidites,
                valeurBourseAlternativeCentimes =
                    valeurBourse,
                ecartCumuleCentimes =
                    patrimoineAlternatif - patrimoineReel
            });
    }

    private static void Detruire(GameData gameData)
    {
        if (gameData != null)
        {
            Object.DestroyImmediate(gameData);
        }
    }
}