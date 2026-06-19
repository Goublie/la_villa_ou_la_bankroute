using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Caracterise les regles et la persistance du domaine Entrepreneuriat.
/// </summary>
public class ArchitectureEntrepreneuriatTests
{
    [Test]
    public void CreerProjet_InitialiseLEtatPersistant()
    {
        ContexteEntrepreneuriat contexte =
            new ContexteEntrepreneuriat();

        ResultatOperation resultat = contexte.Service.CreerProjet();

        Assert.That(resultat.Succes, Is.True);
        Assert.That(contexte.Service.Projet.estCree, Is.True);
        Assert.That(
            contexte.Service.Projet.progressionProduit,
            Is.EqualTo(5));
        Assert.That(
            contexte.Joueur.entrepreneuriat
                .GetValeurPatrimoine().centimes,
            Is.GreaterThan(0));
    }

    [Test]
    public void InjecterMilleEuros_TransfereLeCashVersLeProjet()
    {
        ContexteEntrepreneuriat contexte =
            new ContexteEntrepreneuriat();
        contexte.Service.CreerProjet();

        ResultatOperation resultat =
            contexte.Service.InjecterMilleEuros();

        Assert.That(resultat.Succes, Is.True);
        Assert.That(contexte.Courant.GetSolde().centimes, Is.EqualTo(0));
        Assert.That(
            contexte.Service.Projet.tresorerieCentimes,
            Is.EqualTo(100000));
    }

    [Test]
    public void DevelopperProduit_ConsommeDesRessourcesEtProgresse()
    {
        ContexteEntrepreneuriat contexte =
            new ContexteEntrepreneuriat();
        contexte.Courant.AjoutHistorique(
            "capital de test",
            new argent(1000000));
        contexte.Service.CreerProjet();
        contexte.Service.InjecterMilleEuros();
        int progressionInitiale =
            contexte.Service.Projet.progressionProduit;

        ResultatOperation resultat =
            contexte.Service.DevelopperProduit();

        Assert.That(resultat.Succes, Is.True);
        Assert.That(
            contexte.Service.Projet.progressionProduit,
            Is.GreaterThan(progressionInitiale));
        Assert.That(contexte.Joueur.energie, Is.LessThan(100));
        Assert.That(contexte.Joueur.santeMentale, Is.LessThan(100));
    }

    [Test]
    public void DevelopperProduit_RefuseSansProjetSansMutation()
    {
        ContexteEntrepreneuriat contexte =
            new ContexteEntrepreneuriat();
        int cashInitial = contexte.Courant.GetSolde().centimes;

        ResultatOperation resultat =
            contexte.Service.DevelopperProduit();

        Assert.That(resultat.Succes, Is.False);
        Assert.That(resultat.Code, Is.EqualTo("projet_absent"));
        Assert.That(
            contexte.Courant.GetSolde().centimes,
            Is.EqualTo(cashInitial));
        Assert.That(
            contexte.Service.Projet.progressionProduit,
            Is.EqualTo(0));
    }

    [Test]
    public void ReposerFondateur_BorneLesRessourcesA100()
    {
        ContexteEntrepreneuriat contexte =
            new ContexteEntrepreneuriat();

        contexte.Service.ReposerFondateur();
        contexte.Service.ReposerFondateur();

        Assert.That(contexte.Joueur.energie, Is.EqualTo(100));
        Assert.That(contexte.Joueur.santeMentale, Is.EqualTo(100));
    }

    [Test]
    public void Pitch_EstDeterministeDepuisUneMemeCopie()
    {
        DonneesJoueur origine = new DonneesJoueur();
        ServiceBanque banqueOrigine = new ServiceBanque(origine);
        ServiceEntrepreneuriat serviceOrigine =
            new ServiceEntrepreneuriat(
                origine.entrepreneuriat,
                origine,
                banqueOrigine.ObtenirCompteCourant(),
                banqueOrigine);
        serviceOrigine.CreerProjet();
        serviceOrigine.Projet.progressionProduit = 60;
        serviceOrigine.Projet.tractionMarche = 40;
        serviceOrigine.Projet.reputation = 30;

        DonneesJoueur premierJoueur = origine.Copier();
        DonneesJoueur secondJoueur = origine.Copier();
        ServiceEntrepreneuriat premier =
            CreerService(premierJoueur);
        ServiceEntrepreneuriat second =
            CreerService(secondJoueur);

        ResultatOperation premierResultat =
            premier.PitcherInvestisseurs();
        ResultatOperation secondResultat =
            second.PitcherInvestisseurs();

        Assert.That(
            secondResultat.Code,
            Is.EqualTo(premierResultat.Code));
        Assert.That(
            second.Projet.tresorerieCentimes,
            Is.EqualTo(premier.Projet.tresorerieCentimes));
        Assert.That(
            secondJoueur.energie,
            Is.EqualTo(premierJoueur.energie));
    }

    [Test]
    public void Snapshot_ConserveLeProjetSansPartagerSesMutations()
    {
        GameData gameData = ScriptableObject.CreateInstance<GameData>();
        try
        {
            ServiceEntrepreneuriat service =
                CreerService(gameData.joueur);
            service.CreerProjet();
            service.Projet.progressionProduit = 35;
            service.AppliquerEvolutionMensuelle(0);

            SnapshotEtatJeu snapshot = new SnapshotEtatJeu(gameData);
            service.Projet.progressionProduit = 80;

            Assert.That(
                snapshot.joueur.entrepreneuriat.projet
                    .progressionProduit,
                Is.EqualTo(35));
            Assert.That(
                gameData.joueur.entrepreneuriat.projet
                    .progressionProduit,
                Is.EqualTo(80));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void Patrimoine_InclutLaValorisationDeLEntreprise()
    {
        ContexteEntrepreneuriat contexte =
            new ContexteEntrepreneuriat();
        contexte.Service.CreerProjet();
        contexte.Service.AppliquerEvolutionMensuelle(0);
        int valeurEntreprise =
            contexte.Service.Projet.valorisationCentimes;

        Assert.That(
            contexte.Joueur.CalculPatrimoineTotal().centimes,
            Is.EqualTo(100000 + valeurEntreprise));
    }

    private static ServiceEntrepreneuriat CreerService(
        DonneesJoueur joueur)
    {
        ServiceBanque banque = new ServiceBanque(joueur);
        return new ServiceEntrepreneuriat(
            joueur.entrepreneuriat,
            joueur,
            banque.ObtenirCompteCourant(),
            banque);
    }

    private sealed class ContexteEntrepreneuriat
    {
        public readonly DonneesJoueur Joueur = new DonneesJoueur();
        public readonly ServiceBanque Banque;
        public readonly CompteBanquaire Courant;
        public readonly ServiceEntrepreneuriat Service;

        public ContexteEntrepreneuriat()
        {
            Banque = new ServiceBanque(Joueur);
            Courant = Banque.ObtenirCompteCourant();
            Service = new ServiceEntrepreneuriat(
                Joueur.entrepreneuriat,
                Joueur,
                Courant,
                Banque);
        }
    }
}
