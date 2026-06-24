using NUnit.Framework;

public class ClassificateurFluxWhatIfTests
{
    [Test]
    public void Salaire_EstReproduitCommeRevenuExterne()
    {
        ResultatClassificationFluxWhatIf resultat =
            ClassificateurFluxWhatIf.Classifier("salaire", 150000);

        Assert.That(resultat.type, Is.EqualTo(TypeFluxWhatIf.RevenuExterne));
        Assert.That(resultat.classificationCertaine, Is.True);
        Assert.That(resultat.doitEtreReproduit, Is.True);
    }

    [TestCase("Achat Nvidia", -50000)]
    [TestCase("Vente Bitcoin", 65000)]
    public void OrdreBoursier_EstRemplaceParLeMoteur(
        string libelle,
        int montant)
    {
        ResultatClassificationFluxWhatIf resultat =
            ClassificateurFluxWhatIf.Classifier(libelle, montant);

        Assert.That(
            resultat.type,
            Is.EqualTo(TypeFluxWhatIf.DecisionBoursiere));
        Assert.That(resultat.classificationCertaine, Is.True);
        Assert.That(resultat.doitEtreReproduit, Is.False);
    }

    [TestCase("courant vers epargne", -25000)]
    [TestCase("Cr\u00E9dit", 25000)]
    [TestCase("Debit", -10000)]
    [TestCase("Versement depuis le compte epargne", 10000)]
    public void TransfertInterne_NeModifiePasLePatrimoineAlternatif(
        string libelle,
        int montant)
    {
        ResultatClassificationFluxWhatIf resultat =
            ClassificateurFluxWhatIf.Classifier(libelle, montant);

        Assert.That(
            resultat.type,
            Is.EqualTo(TypeFluxWhatIf.TransfertInterne));
        Assert.That(resultat.doitEtreReproduit, Is.False);
    }

    [Test]
    public void Interets_SontRecalculesEtNonRecopies()
    {
        ResultatClassificationFluxWhatIf resultat =
            ClassificateurFluxWhatIf.Classifier(
                "int\u00E9r\u00EAts",
                1200);

        Assert.That(
            resultat.type,
            Is.EqualTo(TypeFluxWhatIf.RendementInterne));
        Assert.That(resultat.classificationCertaine, Is.True);
        Assert.That(resultat.doitEtreReproduit, Is.False);
    }

    [TestCase("Injection dans le projet")]
    [TestCase("Developpement du produit")]
    [TestCase("Etude de marche")]
    public void DecisionHorsBourse_EstConservee(string libelle)
    {
        ResultatClassificationFluxWhatIf resultat =
            ClassificateurFluxWhatIf.Classifier(libelle, -100000);

        Assert.That(
            resultat.type,
            Is.EqualTo(TypeFluxWhatIf.DecisionHorsBourse));
        Assert.That(resultat.classificationCertaine, Is.True);
        Assert.That(resultat.doitEtreReproduit, Is.True);
    }

    [Test]
    public void DepenseInconnue_EstReproduiteAvecDiagnostic()
    {
        ResultatClassificationFluxWhatIf resultat =
            ClassificateurFluxWhatIf.Classifier(
                "loyer etudiant",
                -65000);

        Assert.That(
            resultat.type,
            Is.EqualTo(TypeFluxWhatIf.DepenseExterne));
        Assert.That(resultat.classificationCertaine, Is.False);
        Assert.That(resultat.doitEtreReproduit, Is.True);
        Assert.That(resultat.diagnostic, Is.Not.Empty);
    }

    [Test]
    public void EntreeInconnue_EstReproduiteAvecDiagnostic()
    {
        ResultatClassificationFluxWhatIf resultat =
            ClassificateurFluxWhatIf.Classifier("remboursement", 5000);

        Assert.That(
            resultat.type,
            Is.EqualTo(TypeFluxWhatIf.RevenuExterne));
        Assert.That(resultat.classificationCertaine, Is.False);
        Assert.That(resultat.doitEtreReproduit, Is.True);
        Assert.That(resultat.diagnostic, Is.Not.Empty);
    }

    [Test]
    public void TransactionNulle_EstIgnoreeSansException()
    {
        ResultatClassificationFluxWhatIf resultat =
            ClassificateurFluxWhatIf.Classifier((Transaction)null);

        Assert.That(resultat.type, Is.EqualTo(TypeFluxWhatIf.Inconnu));
        Assert.That(resultat.doitEtreReproduit, Is.False);
        Assert.That(resultat.diagnostic, Is.Not.Empty);
    }

    [Test]
    public void FluxNul_EstIgnore()
    {
        ResultatClassificationFluxWhatIf resultat =
            ClassificateurFluxWhatIf.Classifier("operation", 0);

        Assert.That(resultat.type, Is.EqualTo(TypeFluxWhatIf.Inconnu));
        Assert.That(resultat.doitEtreReproduit, Is.False);
    }
}