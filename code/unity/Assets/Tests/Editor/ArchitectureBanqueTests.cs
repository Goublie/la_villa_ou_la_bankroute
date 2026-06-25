using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Caracterise les invariants financiers partages par Banque et le noyau.
/// </summary>
public class ArchitectureBanqueTests
{
    [Test]
    public void Transferer_DeplaceLeMontantEntreDeuxComptes()
    {
        DonneesJoueur joueur = new DonneesJoueur();
        ServiceBanque banque = new ServiceBanque(joueur);
        CompteBanquaire courant = banque.ObtenirCompteCourant();
        Epargne livret = banque.ObtenirLivretA();

        ResultatOperation resultat = banque.Transferer(
            courant,
            livret,
            new argent(25000),
            "courant vers epargne",
            "Credit");

        Assert.That(resultat.Succes, Is.True);
        Assert.That(courant.GetSolde().centimes, Is.EqualTo(75000));
        Assert.That(livret.GetSolde().centimes, Is.EqualTo(25000));
    }

    [Test]
    public void Transferer_RefuseLesFondsInsuffisantsSansMutation()
    {
        DonneesJoueur joueur = new DonneesJoueur();
        ServiceBanque banque = new ServiceBanque(joueur);
        CompteBanquaire courant = banque.ObtenirCompteCourant();
        Epargne livret = banque.ObtenirLivretA();

        ResultatOperation resultat = banque.Transferer(
            courant,
            livret,
            new argent(200000),
            "courant vers epargne",
            "Credit");

        Assert.That(resultat.Succes, Is.False);
        Assert.That(resultat.Code, Is.EqualTo("fonds_insuffisants"));
        Assert.That(courant.GetSolde().centimes, Is.EqualTo(100000));
        Assert.That(livret.GetSolde().centimes, Is.EqualTo(0));
    }

    [Test]
    public void Transferer_PermetUnRetraitDuLivretA()
    {
        DonneesJoueur joueur = new DonneesJoueur();
        ServiceBanque banque = new ServiceBanque(joueur);
        CompteBanquaire courant = banque.ObtenirCompteCourant();
        Epargne livret = banque.ObtenirLivretA();
        banque.Transferer(
            courant,
            livret,
            new argent(50000),
            "courant vers epargne",
            "Credit");

        ResultatOperation retrait = banque.Transferer(
            livret,
            courant,
            new argent(20000),
            "Debit",
            "Versement depuis le compte epargne");

        Assert.That(retrait.Succes, Is.True);
        Assert.That(courant.GetSolde().centimes, Is.EqualTo(70000));
        Assert.That(livret.GetSolde().centimes, Is.EqualTo(30000));
    }

    [Test]
    public void ObtenirLivretA_RetourneToujoursLaMemeInstance()
    {
        DonneesJoueur joueur = new DonneesJoueur();
        ServiceBanque banque = new ServiceBanque(joueur);

        Epargne premier = banque.ObtenirLivretA();
        Epargne second = banque.ObtenirLivretA();

        Assert.That(second, Is.SameAs(premier));
        Assert.That(joueur.comptes.Count, Is.EqualTo(2));
    }

    [Test]
    public void Patrimoine_NeComptePasDeuxFoisLeLivretA()
    {
        DonneesJoueur joueur = new DonneesJoueur();
        ServiceBanque banque = new ServiceBanque(joueur);
        Epargne livret = banque.ObtenirLivretA();
        banque.Transferer(
            banque.ObtenirCompteCourant(),
            livret,
            new argent(40000),
            "courant vers epargne",
            "Credit");

        // Reproduit une ancienne sauvegarde dans laquelle le moteur du Livret
        // etait aussi inscrit comme investissement autonome.
        joueur.investissements.Add(livret.invest);

        Assert.That(
            joueur.CalculPatrimoineTotal().centimes,
            Is.EqualTo(100000));
    }

    [Test]
    public void Patrimoine_SoustraitLeCapitalRestantDuDuPret()
    {
        DonneesJoueur joueur = new DonneesJoueur();
        ServiceBanque banque = new ServiceBanque(joueur);
        int patrimoineAvant = joueur.CalculPatrimoineTotal().centimes;

        argent montantEmprunte = new argent(500000);
        banque.ObtenirCompteCourant().AjoutHistorique(
            "Versement prêt immobilier",
            montantEmprunte);

        joueur.pretsImmobiliers.Add(
            new DonneesPret(
                montantEmprunte,
                10,
                2f,
                new argent(5000)));

        Assert.That(
            joueur.CalculPatrimoineTotal().centimes,
            Is.EqualTo(patrimoineAvant));
    }

    [Test]
    public void Patrimoine_AjouteLaValeurDesBiensImmobiliers()
    {
        DonneesJoueur joueur = new DonneesJoueur();
        int patrimoineAvant = joueur.CalculPatrimoineTotal().centimes;

        BienImmobilier bien = new BienImmobilier
        {
            valeurActuelle = new argent(7500000)
        };

        joueur.immobilier.biensPossedes.Add(bien);

        Assert.That(
            joueur.CalculPatrimoineTotal().centimes,
            Is.EqualTo(patrimoineAvant + 7500000));
    }

    [Test]
    public void LivretA_CapitaliseLesInteretsEnDecembre()
    {
        DonneesJoueur joueur = new DonneesJoueur();
        ServiceBanque banque = new ServiceBanque(joueur);
        Epargne livret = banque.ObtenirLivretA();
        banque.Transferer(
            banque.ObtenirCompteCourant(),
            livret,
            new argent(100000),
            "courant vers epargne",
            "Credit");

        // Le jeu commence en juillet : les index 0 a 5 cloturent juillet
        // a decembre, mois pendant lequel les interets sont capitalises.
        for (int mois = 0; mois <= 5; mois++)
        {
            livret.AppliquerEvolutionMensuelle(mois);
        }

        Assert.That(livret.GetSolde().centimes, Is.GreaterThan(100000));
        Assert.That(
            livret.invest.sommeInvestie.centimes,
            Is.EqualTo(livret.GetSolde().centimes));
    }

    [Test]
    public void Snapshot_CopieLesAgregatsSansMutationCroisee()
    {
        GameData gameData = ScriptableObject.CreateInstance<GameData>();
        try
        {
            ServiceBanque banque = new ServiceBanque(gameData.joueur);
            Epargne livret = banque.ObtenirLivretA();
            banque.Transferer(
                banque.ObtenirCompteCourant(),
                livret,
                new argent(30000),
                "courant vers epargne",
                "Credit");

            SnapshotEtatJeu snapshot = new SnapshotEtatJeu(gameData);
            banque.ObtenirCompteCourant().AjoutHistorique(
                "depense",
                new argent(-10000));

            Assert.That(
                snapshot.joueur.comptes[
                    ServiceBanque.CompteCourantId]
                    .GetSolde().centimes,
                Is.EqualTo(70000));
            Assert.That(
                gameData.joueur.comptes[
                    ServiceBanque.CompteCourantId]
                    .GetSolde().centimes,
                Is.EqualTo(60000));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }
}
