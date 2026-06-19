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

    [Test]
    public void RepartitionTemps_DefinitLesMinutesEtSecondesRestantes()
    {
        DonneesRepartitionTemps donnees = new DonneesRepartitionTemps();
        ServiceRepartitionTemps service =
            new ServiceRepartitionTemps(donnees);

        ResultatOperation resultat =
            service.DefinirAllocation(10, 5, 5, 10, 0);

        Assert.That(resultat.Succes, Is.True);
        Assert.That(service.EstAllocationValidee(), Is.True);
        Assert.That(donnees.CalculerTotalMinutes(), Is.EqualTo(30));
        Assert.That(
            donnees.banque.secondesRestantes,
            Is.EqualTo(600f));
        Assert.That(
            donnees.bourse.secondesRestantes,
            Is.EqualTo(600f));
    }

    [Test]
    public void RepartitionTemps_AllocationValideeActiveLaPhaseDeJeu()
    {
        GameData gameData = CreerGameData();
        try
        {
            ServiceRepartitionTemps service =
                new ServiceRepartitionTemps(gameData.joueur.tempsApplications);

            ResultatOperation resultat =
                service.DefinirAllocation(10, 5, 5, 5, 5);

            Assert.That(resultat.Succes, Is.True);
            Assert.That(service.EstAllocationValidee(), Is.True);
            Assert.That(ActionPlay.PeutPasserAuMoisSuivant(gameData), Is.True);
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void RepartitionTemps_EntrepreneuriatPeutRecevoirDuTemps()
    {
        DonneesRepartitionTemps donnees = new DonneesRepartitionTemps();
        ServiceRepartitionTemps service =
            new ServiceRepartitionTemps(donnees);

        service.DefinirAllocation(5, 5, 5, 5, 10);

        Assert.That(
            service.PeutOuvrir(TypeApplicationTemps.Entrepreneuriat),
            Is.True);
        Assert.That(
            service.ObtenirSecondesRestantes(
                TypeApplicationTemps.Entrepreneuriat),
            Is.EqualTo(600f));
    }

    [Test]
    public void RepartitionTemps_ZeroMinuteResteInaccessible()
    {
        DonneesRepartitionTemps donnees = new DonneesRepartitionTemps();
        ServiceRepartitionTemps service =
            new ServiceRepartitionTemps(donnees);

        service.DefinirAllocation(30, 0, 0, 0, 0);

        Assert.That(
            service.PeutOuvrir(TypeApplicationTemps.Banque),
            Is.True);
        Assert.That(
            service.PeutOuvrir(TypeApplicationTemps.Bourse),
            Is.False);
    }

    [Test]
    public void RepartitionTemps_RefuseUnBudgetIncomplet()
    {
        DonneesRepartitionTemps donnees = new DonneesRepartitionTemps();
        ServiceRepartitionTemps service =
            new ServiceRepartitionTemps(donnees);

        ResultatOperation resultat =
            service.DefinirAllocation(10, 5, 5, 5, 0);

        Assert.That(resultat.Succes, Is.False);
        Assert.That(resultat.Code, Is.EqualTo("budget_incomplet"));
        Assert.That(service.EstAllocationValidee(), Is.False);
        Assert.That(donnees.CalculerTotalMinutes(), Is.EqualTo(0));
    }

    [Test]
    public void RepartitionTemps_ConsommerBorneLeTempsAZero()
    {
        DonneesRepartitionTemps donnees = new DonneesRepartitionTemps();
        ServiceRepartitionTemps service =
            new ServiceRepartitionTemps(donnees);
        service.DefinirAllocation(30, 0, 0, 0, 0);

        bool resteDuTemps =
            service.Consommer(TypeApplicationTemps.Banque, 2000f);

        Assert.That(resteDuTemps, Is.False);
        Assert.That(
            service.ObtenirSecondesRestantes(TypeApplicationTemps.Banque),
            Is.EqualTo(0f));
    }

    [Test]
    public void PassageMensuel_ReinitialiseTempsApresSnapshot()
    {
        GameData gameData = CreerGameData();
        try
        {
            new ServiceRepartitionTemps(gameData.joueur.tempsApplications)
                .DefinirAllocation(10, 5, 5, 10, 0);

            new ServicePassageMensuel(gameData)
                .PasserAuMoisSuivant();

            Assert.That(
                gameData.historiqueSnapshots[0]
                    .joueur.tempsApplications.CalculerTotalMinutes(),
                Is.EqualTo(30));
            Assert.That(
                gameData.historiqueSnapshots[0]
                    .joueur.tempsApplications.allocationValidee,
                Is.True);
            Assert.That(
                gameData.joueur.tempsApplications.CalculerTotalMinutes(),
                Is.EqualTo(0));
            Assert.That(
                gameData.joueur.tempsApplications.allocationValidee,
                Is.False);
            Assert.That(
                gameData.joueur.tempsApplications.ATempsRestant(),
                Is.False);
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void Initialisation_ReinitialiseAllocationNonValidee()
    {
        GameData gameData = CreerGameData();
        try
        {
            ServiceRepartitionTemps service =
                new ServiceRepartitionTemps(gameData.joueur.tempsApplications);
            service.DefinirAllocation(10, 5, 5, 5, 5);

            new ServicePassageMensuel(gameData).InitialiserPartie();

            Assert.That(
                gameData.joueur.tempsApplications.CalculerTotalMinutes(),
                Is.EqualTo(0));
            Assert.That(
                gameData.joueur.tempsApplications.allocationValidee,
                Is.False);
            Assert.That(ActionPlay.PeutPasserAuMoisSuivant(gameData), Is.False);
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
