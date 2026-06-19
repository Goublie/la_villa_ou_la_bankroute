using NUnit.Framework;

/// <summary>
/// Caracterise les ordres et la valorisation du domaine Bourse.
/// </summary>
public class ArchitectureBourseTests
{
    [Test]
    public void Acheter_DebiteLeCashEtCreeUnePosition()
    {
        ContexteBourse contexte = new ContexteBourse();

        ResultatOperation resultat = contexte.Service.Acheter(
            contexte.Actif,
            10000,
            0,
            contexte.Courant,
            contexte.Banque);

        PositionBourse position =
            contexte.Donnees.TrouverPosition(contexte.Actif.Id);
        Assert.That(resultat.Succes, Is.True);
        Assert.That(contexte.Courant.GetSolde().centimes, Is.EqualTo(90000));
        Assert.That(position, Is.Not.Null);
        Assert.That(position.quantite, Is.GreaterThan(0f));
        Assert.That(position.coutTotalCentimes, Is.EqualTo(10000));
    }

    [Test]
    public void Acheter_RefuseLesFondsInsuffisants()
    {
        ContexteBourse contexte = new ContexteBourse();

        ResultatOperation resultat = contexte.Service.Acheter(
            contexte.Actif,
            200000,
            0,
            contexte.Courant,
            contexte.Banque);

        Assert.That(resultat.Succes, Is.False);
        Assert.That(resultat.Code, Is.EqualTo("fonds_insuffisants"));
        Assert.That(contexte.Donnees.positions, Is.Empty);
    }

    [Test]
    public void VendreQuantite_ConserveLeCoutMoyenDuReliquat()
    {
        ContexteBourse contexte = new ContexteBourse();
        contexte.Service.Acheter(
            contexte.Actif,
            50000,
            0,
            contexte.Courant,
            contexte.Banque);
        PositionBourse position =
            contexte.Donnees.TrouverPosition(contexte.Actif.Id);
        float quantiteInitiale = position.quantite;

        ResultatOperation resultat = contexte.Service.VendreQuantite(
            contexte.Actif,
            quantiteInitiale / 2f,
            0,
            contexte.Courant,
            contexte.Banque);

        Assert.That(resultat.Succes, Is.True);
        Assert.That(
            position.quantite,
            Is.EqualTo(quantiteInitiale / 2f).Within(0.00001f));
        Assert.That(position.coutTotalCentimes, Is.EqualTo(25000));
    }

    [Test]
    public void ToutVendre_SupprimeLaPosition()
    {
        ContexteBourse contexte = new ContexteBourse();
        contexte.Service.Acheter(
            contexte.Actif,
            30000,
            0,
            contexte.Courant,
            contexte.Banque);

        ResultatOperation resultat = contexte.Service.ToutVendre(
            contexte.Actif,
            0,
            contexte.Courant,
            contexte.Banque);

        Assert.That(resultat.Succes, Is.True);
        Assert.That(
            contexte.Donnees.TrouverPosition(contexte.Actif.Id),
            Is.Null);
    }

    [Test]
    public void VendreQuantite_RefuseUnePositionInsuffisante()
    {
        ContexteBourse contexte = new ContexteBourse();
        contexte.Service.Acheter(
            contexte.Actif,
            10000,
            0,
            contexte.Courant,
            contexte.Banque);
        PositionBourse position =
            contexte.Donnees.TrouverPosition(contexte.Actif.Id);

        ResultatOperation resultat = contexte.Service.VendreQuantite(
            contexte.Actif,
            position.quantite + 1f,
            0,
            contexte.Courant,
            contexte.Banque);

        Assert.That(resultat.Succes, Is.False);
        Assert.That(resultat.Code, Is.EqualTo("position_insuffisante"));
    }

    [Test]
    public void EvolutionMensuelle_MetAJourLaValeurPatrimoniale()
    {
        ContexteBourse contexte = new ContexteBourse();
        contexte.Service.Acheter(
            contexte.Actif,
            40000,
            0,
            contexte.Courant,
            contexte.Banque);

        contexte.Service.AppliquerEvolutionMensuelle(1);

        Assert.That(contexte.Donnees.moisValorisation, Is.EqualTo(1));
        Assert.That(
            contexte.Donnees.GetValeurPatrimoine().centimes,
            Is.GreaterThan(0));
    }

    [Test]
    public void Valorisation_ExposeCapitalEtGainLatentCoherents()
    {
        ContexteBourse contexte = new ContexteBourse();
        contexte.Service.Acheter(
            contexte.Actif,
            40000,
            0,
            contexte.Courant,
            contexte.Banque);
        contexte.Service.AppliquerEvolutionMensuelle(1);
        PositionBourse position =
            contexte.Donnees.TrouverPosition(contexte.Actif.Id);

        int valeur =
            contexte.Service.CalculerValeurPositionCentimes(position, 1);
        int gain =
            contexte.Service.CalculerGainPerteCentimes(position, 1);

        Assert.That(
            contexte.Donnees.CalculerCapitalInvestiCentimes(),
            Is.EqualTo(40000));
        Assert.That(gain, Is.EqualTo(valeur - 40000));
        Assert.That(
            contexte.Donnees.GetGainsPertesLatents().centimes,
            Is.EqualTo(gain));
    }

    [Test]
    public void ImpactEvenement_ModifieLePrixSansMuterLaDefinition()
    {
        ContexteBourse contexte = new ContexteBourse();
        float prixInitial =
            contexte.Service.ObtenirPrix(contexte.Actif, 0);

        contexte.Service.AppliquerImpactEvenement(
            new ImpactEvenementMarche
            {
                evenementId = "test_baisse",
                actifId = contexte.Actif.Id,
                moisDebut = 0,
                dureeMois = 1,
                coefficientPrix = 0.5f
            });

        float prixImpacte =
            contexte.Service.ObtenirPrix(contexte.Actif, 0);
        Assert.That(
            prixImpacte,
            Is.EqualTo(prixInitial * 0.5f).Within(0.01f));
    }

    [Test]
    public void MoisInitial_NUtilisePasLesDonneesFutures()
    {
        ContexteBourse contexte = new ContexteBourse();

        float variation =
            contexte.Service.CalculerVariation(
                contexte.Actif,
                0,
                12);

        Assert.That(variation, Is.EqualTo(0f).Within(0.0001f));
    }

    private sealed class ContexteBourse
    {
        public readonly DonneesJoueur Joueur = new DonneesJoueur();
        public readonly DonneesBourse Donnees;
        public readonly ServiceBanque Banque;
        public readonly ServiceBourse Service;
        public readonly CompteBanquaire Courant;
        public readonly DefinitionActifFinancier Actif;

        public ContexteBourse()
        {
            Donnees = Joueur.bourse;
            Banque = new ServiceBanque(Joueur);
            Courant = Banque.ObtenirCompteCourant();
            Service = new ServiceBourse(Donnees);
            Actif = CatalogueActifs.Trouver("nvidia");
            Assert.That(Actif, Is.Not.Null);
        }
    }
}
