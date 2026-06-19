using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Caracterise les garanties de la retrospective et du mode What If.
/// </summary>
public class ArchitectureRetrospectiveTests
{
    [Test]
    public void WhatIf_LitLesSnapshotsSansModifierLeJoueurReel()
    {
        GameData gameData = ScriptableObject.CreateInstance<GameData>();
        try
        {
            gameData.ResetData();
            ServiceBanque banque = new ServiceBanque(gameData.joueur);
            CompteBanquaire courant = banque.ObtenirCompteCourant();
            Epargne livret = banque.ObtenirLivretA();

            banque.Transferer(
                courant,
                livret,
                new argent(25000),
                "courant vers epargne",
                "Credit");
            gameData.historiqueSnapshots.Add(new SnapshotEtatJeu(gameData));

            courant.AjoutHistorique("depense test", new argent(-10000));
            gameData.joueur.salaire = new argent(123456);
            gameData.joueur.energie = 42;
            gameData.joueur.santeMentale = 77;
            gameData.nombreMoisPasses = 1;
            gameData.moisActuel = Mois.Aout;
            gameData.historiqueSnapshots.Add(new SnapshotEtatJeu(gameData));

            int courantAvant = courant.GetSolde().centimes;
            int livretAvant = livret.GetSolde().centimes;
            int salaireAvant = gameData.joueur.salaire.centimes;
            int energieAvant = gameData.joueur.energie;
            int santeMentaleAvant = gameData.joueur.santeMentale;
            int snapshotsAvant = gameData.historiqueSnapshots.Count;

            List<Optimizer.SimulationResult> fourmi =
                Optimizer.SimulerFourmi(gameData);
            List<Optimizer.SimulationResult> reel =
                Optimizer.ObtenirHistoriqueReel(gameData);

            Assert.That(fourmi.Count, Is.EqualTo(2));
            Assert.That(reel.Count, Is.EqualTo(2));
            Assert.That(courant.GetSolde().centimes, Is.EqualTo(courantAvant));
            Assert.That(livret.GetSolde().centimes, Is.EqualTo(livretAvant));
            Assert.That(
                gameData.joueur.salaire.centimes,
                Is.EqualTo(salaireAvant));
            Assert.That(gameData.joueur.energie, Is.EqualTo(energieAvant));
            Assert.That(
                gameData.joueur.santeMentale,
                Is.EqualTo(santeMentaleAvant));
            Assert.That(
                gameData.historiqueSnapshots.Count,
                Is.EqualTo(snapshotsAvant));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }
}
