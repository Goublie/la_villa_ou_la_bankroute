using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Caracterise les regles persistantes du parcours Salariat.
/// </summary>
public class ArchitectureSalariatTests
{
    [Test]
    public void AccepterPoste_SynchroniseEtatEtSalaire()
    {
        ContexteSalariat contexte = new ContexteSalariat();

        ResultatOperation resultat = contexte.Service.AccepterPoste(
            "Tech Solutions",
            350000,
            35,
            2,
            3,
            2);

        Assert.That(resultat.Succes, Is.True);
        Assert.That(contexte.Donnees.aEmploi, Is.True);
        Assert.That(contexte.Donnees.entreprise, Is.EqualTo("Tech Solutions"));
        Assert.That(contexte.Donnees.heuresSemaine, Is.EqualTo(35));
        Assert.That(contexte.Joueur.salaire.centimes, Is.EqualTo(350000));
        Assert.That(
            contexte.Donnees.satisfaction,
            Is.EqualTo(ServiceSalariat.CalculerSatisfaction(2, 3, 2)));
    }

    [Test]
    public void EvolutionMensuelle_ProgresseHorsInterfaceTousLesCinqMois()
    {
        ContexteSalariat contexte = new ContexteSalariat();
        contexte.Service.AccepterPoste(
            "Global Services",
            300000,
            45,
            2,
            2,
            2);
        contexte.Donnees.ancienneteMois = 4;

        contexte.Service.AppliquerEvolutionMensuelle(0);

        Assert.That(contexte.Donnees.ancienneteMois, Is.EqualTo(5));
        Assert.That(contexte.Donnees.experience, Is.EqualTo(20));
        Assert.That(contexte.Donnees.fatigue, Is.EqualTo(40));
    }

    [Test]
    public void RelationColleguesMaximale_ReduitLaFatigueMensuelle()
    {
        ContexteSalariat contexte = new ContexteSalariat();
        contexte.Service.AccepterPoste(
            "Local Startup",
            250000,
            35,
            1,
            1,
            1);
        contexte.Donnees.fatigue = 50;
        contexte.Donnees.relationCollegues = 100;

        contexte.Service.AppliquerEvolutionMensuelle(0);

        Assert.That(contexte.Donnees.fatigue, Is.EqualTo(45));
    }

    [Test]
    public void Negociation_ExigeUneExperienceSuffisante()
    {
        ContexteSalariat contexte = new ContexteSalariat();
        contexte.Service.AccepterPoste(
            "Innovate Corp",
            400000,
            40,
            2,
            3,
            2);

        contexte.Donnees.experience = 70;
        ResultatOperation echec = contexte.Service.NegocierSalaire();

        contexte.Donnees.experience = 71;
        ResultatOperation succes = contexte.Service.NegocierSalaire();

        Assert.That(echec.Succes, Is.False);
        Assert.That(echec.Code, Is.EqualTo("experience_insuffisante"));
        Assert.That(succes.Succes, Is.True);
        Assert.That(
            contexte.Donnees.salaireMensuelCentimes,
            Is.EqualTo(400000 + ServiceSalariat.AugmentationNegociationCentimes));
        Assert.That(
            contexte.Joueur.salaire.centimes,
            Is.EqualTo(contexte.Donnees.salaireMensuelCentimes));
    }

    [Test]
    public void Snapshot_CopieSalariatSansMutationCroisee()
    {
        GameData gameData = ScriptableObject.CreateInstance<GameData>();
        try
        {
            gameData.ResetData();
            ServiceSalariat service = new ServiceSalariat(
                gameData.joueur.salariat,
                gameData.joueur);
            service.AccepterPoste("Tech Solutions", 350000, 35, 1, 2, 3);
            gameData.joueur.salariat.experience = 42;

            SnapshotEtatJeu snapshot = new SnapshotEtatJeu(gameData);
            gameData.joueur.salariat.experience = 80;
            gameData.joueur.salariat.entreprise = "Autre";

            Assert.That(
                snapshot.joueur.salariat.experience,
                Is.EqualTo(42));
            Assert.That(
                snapshot.joueur.salariat.entreprise,
                Is.EqualTo("Tech Solutions"));
            Assert.That(
                gameData.joueur.salariat.experience,
                Is.EqualTo(80));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void PassageMensuel_AppliqueSalariatPuisVerseSalaire()
    {
        GameData gameData = ScriptableObject.CreateInstance<GameData>();
        try
        {
            gameData.ResetData();
            ServiceSalariat salariat = new ServiceSalariat(
                gameData.joueur.salariat,
                gameData.joueur);
            salariat.AccepterPoste("Global Services", 300000, 35, 1, 1, 1);
            gameData.joueur.salariat.ancienneteMois = 4;

            new ServicePassageMensuel(gameData).PasserAuMoisSuivant();

            Assert.That(
                gameData.joueur.salariat.ancienneteMois,
                Is.EqualTo(5));
            Assert.That(
                gameData.joueur.salariat.experience,
                Is.EqualTo(10));
            Assert.That(
                gameData.joueur.comptes[ServiceBanque.CompteCourantId]
                    .GetSolde().centimes,
                Is.EqualTo(100000 + 300000));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    private sealed class ContexteSalariat
    {
        public readonly DonneesJoueur Joueur = new DonneesJoueur();
        public readonly DonneesSalariat Donnees;
        public readonly ServiceSalariat Service;

        public ContexteSalariat()
        {
            Donnees = Joueur.salariat;
            Service = new ServiceSalariat(Donnees, Joueur);
        }
    }
}
