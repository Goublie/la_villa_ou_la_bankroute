using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Caracterise le catalogue et l'orchestration mensuelle des rumeurs.
/// </summary>
public class ArchitectureEvenementsTests
{
    [Test]
    public void CatalogueRessources_ContientLesDonneesAttendues()
    {
        ResultatChargementCatalogue chargement =
            CatalogueEvenements.ChargerDepuisResources();

        Assert.That(chargement.EstValide, Is.True,
            chargement.ConstruireMessageErreurs());
        Assert.That(chargement.Catalogue.Evenements, Has.Count.EqualTo(200));
        Assert.That(chargement.Catalogue.Sources, Has.Count.EqualTo(19));
        Assert.That(
            chargement.Catalogue.Evenements.Select(e => e.id).Distinct().Count(),
            Is.EqualTo(200));
        Assert.That(
            chargement.Catalogue.Sources.Select(s => s.id).Distinct().Count(),
            Is.EqualTo(19));
        Assert.That(
            chargement.Catalogue.Evenements.Count(
                e => e.categorie == CategoriesEvenements.Boursiers),
            Is.EqualTo(120));
        Assert.That(
            chargement.Catalogue.Evenements.Count(
                e => e.categorie == CategoriesEvenements.Personnels),
            Is.EqualTo(40));
        Assert.That(
            chargement.Catalogue.Evenements.Count(
                e => e.categorie == CategoriesEvenements.Professionnels),
            Is.EqualTo(40));
    }

    [Test]
    public void AuditCatalogue_SignaleImpactsIncompletsEtExtremes()
    {
        CatalogueEvenements catalogue = ChargerCatalogueReel();

        Assert.That(catalogue.EvenementsSansImpact, Has.Count.EqualTo(45));
        Assert.That(catalogue.CiblesImpactInconnues, Has.Count.EqualTo(51));
        Assert.That(catalogue.CiblesImpactInconnues, Does.Contain("Or"));
        Assert.That(catalogue.VariationsExtremes, Has.Count.EqualTo(42));
        Assert.That(
            catalogue.VariationsExtremes.Any(v => v.StartsWith("062:Bitcoin:")),
            Is.True);
        Assert.That(
            CatalogueEvenements.EssayerObtenirActifBourse(
                "Google",
                out string actifId),
            Is.True);
        Assert.That(actifId, Is.EqualTo("alphabet"));
    }

    [Test]
    public void Catalogue_DetecteIdentifiantsEtValeursInvalides()
    {
        string evenements =
            "[{\"id\":\"dup\",\"categorie\":\"Boursiers\"," +
            "\"titre\":\"A\",\"importance\":\"Faible\"," +
            "\"message\":\"A\",\"impacts\":[]}," +
            "{\"id\":\"dup\",\"categorie\":\"Inconnue\"," +
            "\"titre\":\"B\",\"importance\":\"Faible\"," +
            "\"message\":\"B\",\"impacts\":[]}]";
        string sources =
            "[{\"id\":\"src\",\"nom\":\"Source\"," +
            "\"type\":\"test\",\"domaines\":[\"Boursiers\"]," +
            "\"fiabilite\":1.2}]";

        ResultatChargementCatalogue resultat =
            CatalogueEvenements.ChargerDepuisJson(evenements, sources);

        Assert.That(resultat.EstValide, Is.False);
        Assert.That(
            resultat.Erreurs.Any(e => e.Contains("duplique")),
            Is.True);
        Assert.That(
            resultat.Erreurs.Any(e => e.Contains("Categorie invalide")),
            Is.True);
        Assert.That(
            resultat.Erreurs.Any(e => e.Contains("Fiabilite invalide")),
            Is.True);
    }

    [Test]
    public void Catalogue_RetourneUneErreurExplicitePourJsonIllisible()
    {
        ResultatChargementCatalogue resultat =
            CatalogueEvenements.ChargerDepuisJson("[{", "[]");

        Assert.That(resultat.EstValide, Is.False);
        Assert.That(resultat.Erreurs, Is.Not.Empty);
        Assert.That(
            resultat.ConstruireMessageErreurs(),
            Does.Contain("JSON Actualites illisible"));
    }

    [Test]
    public void Sources_RespectentStrictementLeursDomaines()
    {
        CatalogueEvenements catalogue = ChargerCatalogueReel();
        List<SourceActualite> boursieres =
            catalogue.ObtenirSourcesCompatibles(CategoriesEvenements.Boursiers);
        List<SourceActualite> personnelles =
            catalogue.ObtenirSourcesCompatibles(CategoriesEvenements.Personnels);

        Assert.That(boursieres, Has.Count.EqualTo(13));
        Assert.That(personnelles, Has.Count.EqualTo(3));
        Assert.That(
            boursieres.Any(s => s.id == "src_cercle_famille"),
            Is.False);
        Assert.That(
            personnelles.Any(s => s.id == "src_rh"),
            Is.False);
    }

    [Test]
    public void Catalogue_RefuseCategorieSansSourceCompatible()
    {
        string evenements = ConstruireJsonEvenements(
            ("evt", CategoriesEvenements.Professionnels, "Evenement"));
        string sources = ConstruireJsonSources(
            ("famille", CategoriesEvenements.Personnels, 0.8f));

        ResultatChargementCatalogue resultat =
            CatalogueEvenements.ChargerDepuisJson(evenements, sources);

        Assert.That(resultat.EstValide, Is.False);
        Assert.That(
            resultat.Erreurs.Any(e => e.Contains("Aucune source compatible")),
            Is.True);
    }

    [Test]
    public void Apparition_CreeExactementDeuxRumeursDistinctesSansImpact()
    {
        GameData gameData = CreerGameData();
        try
        {
            ServiceOrchestrationEvenements service = CreerServiceMinimal(
                gameData,
                new GenerateurAleatoireDeterministe(42));
            int energieAvant = gameData.joueur.energie;

            ResultatOrchestrationEvenements resultat = service.TraiterMois(0);

            Assert.That(resultat.Succes, Is.True);
            Assert.That(resultat.RumeursCreees, Is.EqualTo(2));
            Assert.That(
                gameData.evenements.ObtenirRumeurs(EtatRumeur.EnAttente),
                Has.Count.EqualTo(2));
            Assert.That(
                gameData.evenements.rumeurs.Select(r => r.evenementId)
                    .Distinct().Count(),
                Is.EqualTo(2));
            Assert.That(gameData.evenements.evenementsConfirmes, Is.Empty);
            Assert.That(gameData.joueur.bourse.impactsMarche, Is.Empty);
            Assert.That(gameData.joueur.energie, Is.EqualTo(energieAvant));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void Apparition_AssocieToujoursUneSourceCompatible()
    {
        GameData gameData = CreerGameData();
        try
        {
            CatalogueEvenements catalogue = ChargerCatalogueReel();
            ServiceOrchestrationEvenements service =
                new ServiceOrchestrationEvenements(
                    gameData.evenements,
                    catalogue,
                    new GenerateurAleatoireDeterministe(123));

            service.TraiterMois(0);

            foreach (RumeurPartie rumeur in gameData.evenements.rumeurs)
            {
                SourceActualite source = catalogue.TrouverSource(rumeur.sourceId);
                Assert.That(source, Is.Not.Null);
                Assert.That(source.AccepteCategorie(rumeur.categorie), Is.True);
            }
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void Apparition_ExclutUnEvenementDejaEnAttente()
    {
        GameData gameData = CreerGameData();
        try
        {
            gameData.evenements.rumeurs.Add(new RumeurPartie
            {
                id = "existante",
                evenementId = "evt_a",
                sourceId = "source",
                categorie = CategoriesEvenements.Boursiers,
                moisApparition = 0,
                moisResolution = 5,
                etat = EtatRumeur.EnAttente
            });
            ServiceOrchestrationEvenements service = CreerServiceMinimal(
                gameData,
                new GenerateurFixe(0d));

            service.TraiterMois(0);

            Assert.That(
                gameData.evenements.rumeurs.Count(
                    r => r.evenementId == "evt_a"),
                Is.EqualTo(1));
            Assert.That(gameData.evenements.rumeurs, Has.Count.EqualTo(3));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void Resolution_ConfirmeAuMoisSuivantEtConserveImpactsBruts()
    {
        GameData gameData = CreerGameData();
        try
        {
            ServiceOrchestrationEvenements service = CreerServiceMinimal(
                gameData,
                new GenerateurFixe(0d));
            service.TraiterMois(0);

            ResultatOrchestrationEvenements resultat = service.TraiterMois(1);

            Assert.That(resultat.RumeursResolues, Is.EqualTo(2));
            Assert.That(resultat.EvenementsConfirmes, Is.EqualTo(2));
            Assert.That(
                gameData.evenements.ObtenirRumeurs(EtatRumeur.Confirmee),
                Has.Count.EqualTo(2));
            Assert.That(
                gameData.evenements.ObtenirRumeurs(EtatRumeur.EnAttente),
                Has.Count.EqualTo(2));
            Assert.That(
                gameData.evenements.evenementsConfirmes,
                Has.Count.EqualTo(2));
            Assert.That(
                gameData.evenements.evenementsConfirmes[0].moisConfirmation,
                Is.EqualTo(1));
            Assert.That(
                gameData.evenements.evenementsConfirmes[0].rumeurId,
                Is.EqualTo(gameData.evenements.rumeurs[0].id));
            Assert.That(
                gameData.evenements.evenementsConfirmes[0].sourceId,
                Is.EqualTo(gameData.evenements.rumeurs[0].sourceId));
            Assert.That(
                gameData.evenements.evenementsConfirmes[0].impacts,
                Is.Not.Empty);
            Assert.That(gameData.joueur.bourse.impactsMarche, Is.Empty);
            Assert.That(
                gameData.evenements.publications,
                Has.Count.EqualTo(6));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void Resolution_InvalideSansCreerDeFauxEvenement()
    {
        GameData gameData = CreerGameData();
        try
        {
            ServiceOrchestrationEvenements service = CreerServiceMinimal(
                gameData,
                new GenerateurFixe(0.999d));
            service.TraiterMois(0);

            service.TraiterMois(1);

            Assert.That(
                gameData.evenements.ObtenirRumeurs(EtatRumeur.Invalidee),
                Has.Count.EqualTo(2));
            Assert.That(gameData.evenements.evenementsConfirmes, Is.Empty);
            Assert.That(
                gameData.evenements.publications.Count(
                    p => p.type == TypePublicationActualite.EvenementConfirme),
                Is.EqualTo(0));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void Orchestration_MemeGraineProduitLaMemeHistoire()
    {
        GameData gauche = CreerGameData();
        GameData droite = CreerGameData();
        try
        {
            ServiceOrchestrationEvenements serviceGauche =
                CreerServiceMinimal(
                    gauche,
                    new GenerateurAleatoireDeterministe(9876));
            ServiceOrchestrationEvenements serviceDroite =
                CreerServiceMinimal(
                    droite,
                    new GenerateurAleatoireDeterministe(9876));

            serviceGauche.TraiterMois(3);
            serviceDroite.TraiterMois(3);

            CollectionAssert.AreEqual(
                gauche.evenements.rumeurs.Select(
                    r => r.evenementId + ":" + r.sourceId + ":" + r.etat),
                droite.evenements.rumeurs.Select(
                    r => r.evenementId + ":" + r.sourceId + ":" + r.etat));
            Assert.That(
                gauche.evenements.etatAleatoire,
                Is.EqualTo(droite.evenements.etatAleatoire));
        }
        finally
        {
            Object.DestroyImmediate(gauche);
            Object.DestroyImmediate(droite);
        }
    }

    [Test]
    public void Orchestration_NeRetraiteJamaisLeMemeMois()
    {
        GameData gameData = CreerGameData();
        try
        {
            ServiceOrchestrationEvenements service = CreerServiceMinimal(
                gameData,
                new GenerateurFixe(0d));
            service.TraiterMois(1);
            uint etat = gameData.evenements.etatAleatoire;
            int rumeurs = gameData.evenements.rumeurs.Count;
            int evenements = gameData.evenements.evenementsConfirmes.Count;
            int publications = gameData.evenements.publications.Count;

            ResultatOrchestrationEvenements second = service.TraiterMois(1);

            Assert.That(second.RumeursCreees, Is.EqualTo(0));
            Assert.That(second.RumeursResolues, Is.EqualTo(0));
            Assert.That(second.EvenementsConfirmes, Is.EqualTo(0));
            Assert.That(gameData.evenements.rumeurs, Has.Count.EqualTo(rumeurs));
            Assert.That(
                gameData.evenements.evenementsConfirmes,
                Has.Count.EqualTo(evenements));
            Assert.That(
                gameData.evenements.publications,
                Has.Count.EqualTo(publications));
            Assert.That(gameData.evenements.etatAleatoire, Is.EqualTo(etat));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void ResetData_EffaceToutHistoriqueEtPermetUneNouvellePartie()
    {
        GameData gameData = CreerGameData();
        try
        {
            ServiceOrchestrationEvenements service = CreerServiceMinimal(
                gameData,
                new GenerateurFixe(0d));
            service.TraiterMois(1);
            Assert.That(gameData.evenements.rumeurs, Is.Not.Empty);
            Assert.That(gameData.evenements.evenementsConfirmes, Is.Not.Empty);
            Assert.That(gameData.evenements.publications, Is.Not.Empty);

            gameData.ResetData();

            Assert.That(gameData.evenements.rumeurs, Is.Empty);
            Assert.That(gameData.evenements.evenementsConfirmes, Is.Empty);
            Assert.That(gameData.evenements.publications, Is.Empty);
            Assert.That(gameData.evenements.dernierMoisTraite, Is.EqualTo(-1));
            Assert.That(gameData.evenements.graineInitiale, Is.EqualTo(0u));
            Assert.That(gameData.evenements.etatAleatoire, Is.EqualTo(0u));
            Assert.That(
                gameData.evenements.prochaineSequenceRumeur,
                Is.EqualTo(0));
            Assert.That(
                gameData.evenements.prochaineSequencePublication,
                Is.EqualTo(0));

            CreerServiceMinimal(gameData, new GenerateurFixe(0d))
                .TraiterMois(0);
            Assert.That(gameData.evenements.rumeurs, Has.Count.EqualTo(2));
            Assert.That(
                gameData.evenements.ObtenirRumeurs(EtatRumeur.EnAttente),
                Has.Count.EqualTo(2));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [TestCase(0f, ParametresEvenements.ProbabiliteMinimale)]
    [TestCase(1f, ParametresEvenements.ProbabiliteMaximale)]
    public void Probabilite_EstBorneeEtNeCompteQueLaSource(
        float fiabilite,
        float attendu)
    {
        GameData gameData = CreerGameData();
        try
        {
            CatalogueEvenements catalogue = CreerCatalogueMinimal(fiabilite);
            ServiceOrchestrationEvenements service =
                new ServiceOrchestrationEvenements(
                    gameData.evenements,
                    catalogue,
                    new GenerateurFixe(0d));

            service.TraiterMois(0);

            Assert.That(
                gameData.evenements.rumeurs[0].probabiliteConfirmation,
                Is.EqualTo(attendu).Within(0.0001f));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void Snapshot_CopieProfondementRumeursEvenementsEtPublications()
    {
        GameData gameData = CreerGameData();
        try
        {
            ServiceOrchestrationEvenements service = CreerServiceMinimal(
                gameData,
                new GenerateurFixe(0d));
            service.TraiterMois(1);
            SnapshotEtatJeu snapshot = new SnapshotEtatJeu(gameData);

            gameData.evenements.rumeurs[0].titrePublic = "Modifie";
            gameData.evenements.evenementsConfirmes[0]
                .impacts[0].variation = 99f;
            snapshot.evenements.publications[0].titre = "Snapshot modifie";

            Assert.That(
                snapshot.evenements.rumeurs[0].titrePublic,
                Is.Not.EqualTo("Modifie"));
            Assert.That(
                snapshot.evenements.evenementsConfirmes[0]
                    .impacts[0].variation,
                Is.Not.EqualTo(99f));
            Assert.That(
                gameData.evenements.publications[0].titre,
                Is.Not.EqualTo("Snapshot modifie"));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void Snapshot_RestaureExactementLaSuiteAleatoireMensuelle()
    {
        GameData original = CreerGameData();
        GameData restaure = CreerGameData();
        try
        {
            CatalogueEvenements catalogue = CreerCatalogueMinimal(0.8f);
            ServiceOrchestrationEvenements serviceOriginal =
                new ServiceOrchestrationEvenements(
                    original.evenements,
                    catalogue,
                    new GenerateurAleatoireDeterministe(2026));
            serviceOriginal.TraiterMois(1);
            SnapshotEtatJeu snapshot = new SnapshotEtatJeu(original);
            restaure.evenements = snapshot.evenements.Copier();

            serviceOriginal.TraiterMois(2);
            ServiceOrchestrationEvenements.CreerPourJeu(
                    restaure.evenements,
                    catalogue)
                .TraiterMois(2);

            CollectionAssert.AreEqual(
                original.evenements.rumeurs.Select(
                    r => r.evenementId + ":" + r.sourceId + ":" + r.etat),
                restaure.evenements.rumeurs.Select(
                    r => r.evenementId + ":" + r.sourceId + ":" + r.etat));
            Assert.That(
                restaure.evenements.etatAleatoire,
                Is.EqualTo(original.evenements.etatAleatoire));
        }
        finally
        {
            Object.DestroyImmediate(original);
            Object.DestroyImmediate(restaure);
        }
    }

    [Test]
    public void WhatIf_NAccedeQuAuxConfirmationsConnuesEtCopiees()
    {
        GameData gameData = CreerGameData();
        try
        {
            ServiceOrchestrationEvenements service = CreerServiceMinimal(
                gameData,
                new GenerateurFixe(0d));
            service.TraiterMois(1);

            List<EvenementConfirmePartie> connus =
                gameData.evenements.CopierConfirmationsJusqua(1);
            connus[0].titre = "Simulation";

            Assert.That(connus, Has.Count.EqualTo(2));
            Assert.That(
                gameData.evenements.evenementsConfirmes[0].titre,
                Is.Not.EqualTo("Simulation"));
            Assert.That(
                gameData.evenements.ObtenirRumeurs(EtatRumeur.EnAttente),
                Has.Count.EqualTo(2));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void ConsommationFuture_NExposeChaqueConfirmationQuUneFois()
    {
        GameData gameData = CreerGameData();
        try
        {
            ServiceOrchestrationEvenements service = CreerServiceMinimal(
                gameData,
                new GenerateurFixe(0d));
            service.TraiterMois(1);
            string rumeurId = gameData.evenements.evenementsConfirmes[0].rumeurId;

            Assert.That(
                gameData.evenements.ObtenirConfirmationsAConsommer(),
                Has.Count.EqualTo(2));
            Assert.That(service.MarquerConfirmationConsommee(rumeurId), Is.True);
            Assert.That(service.MarquerConfirmationConsommee(rumeurId), Is.False);
            Assert.That(
                gameData.evenements.ObtenirConfirmationsAConsommer(),
                Has.Count.EqualTo(1));
            Assert.That(gameData.joueur.bourse.impactsMarche, Is.Empty);
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void PassageMensuel_OrchestreUneSeuleFoisEtSnapshotAvantResolution()
    {
        GameData gameData = CreerGameData();
        try
        {
            gameData.evenements.etatAleatoire = 12345u;
            ServicePassageMensuel service = new ServicePassageMensuel(gameData);
            service.InitialiserPartie();
            service.InitialiserPartie();

            Assert.That(gameData.evenements.rumeurs, Has.Count.EqualTo(2));
            Assert.That(gameData.historiqueSnapshots, Has.Count.EqualTo(1));

            service.PasserAuMoisSuivant();

            Assert.That(gameData.evenements.rumeurs, Has.Count.EqualTo(4));
            Assert.That(gameData.evenements.dernierMoisTraite, Is.EqualTo(1));
            Assert.That(
                gameData.evenements.ObtenirRumeurs(EtatRumeur.EnAttente),
                Has.Count.EqualTo(2));
            Assert.That(
                gameData.historiqueSnapshots[1].evenements
                    .ObtenirRumeurs(EtatRumeur.EnAttente),
                Has.Count.EqualTo(2));
            Assert.That(gameData.joueur.bourse.impactsMarche, Is.Empty);
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void Publications_SontTrieesDuPlusRecentAuPlusAncien()
    {
        GameData gameData = CreerGameData();
        try
        {
            ServiceOrchestrationEvenements service = CreerServiceMinimal(
                gameData,
                new GenerateurFixe(0d));
            service.TraiterMois(1);

            List<PublicationActualite> publications =
                service.ObtenirPublicationsTriees();

            Assert.That(publications[0].moisPublication, Is.EqualTo(1));
            Assert.That(
                publications[0].ordrePublication,
                Is.GreaterThan(publications[1].ordrePublication));
            Assert.That(
                publications[publications.Count - 1].moisPublication,
                Is.EqualTo(0));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    private static CatalogueEvenements ChargerCatalogueReel()
    {
        ResultatChargementCatalogue chargement =
            CatalogueEvenements.ChargerDepuisResources();
        Assert.That(chargement.EstValide, Is.True,
            chargement.ConstruireMessageErreurs());
        return chargement.Catalogue;
    }

    private static ServiceOrchestrationEvenements CreerServiceMinimal(
        GameData gameData,
        IGenerateurAleatoire aleatoire)
    {
        return new ServiceOrchestrationEvenements(
            gameData.evenements,
            CreerCatalogueMinimal(0.8f),
            aleatoire);
    }

    private static CatalogueEvenements CreerCatalogueMinimal(float fiabilite)
    {
        string evenements = ConstruireJsonEvenements(
            ("evt_a", CategoriesEvenements.Boursiers, "Evenement A"),
            ("evt_b", CategoriesEvenements.Boursiers, "Evenement B"),
            ("evt_c", CategoriesEvenements.Boursiers, "Evenement C"),
            ("evt_d", CategoriesEvenements.Boursiers, "Evenement D"));
        string sources = ConstruireJsonSources(
            ("source", CategoriesEvenements.Boursiers, fiabilite));
        ResultatChargementCatalogue chargement =
            CatalogueEvenements.ChargerDepuisJson(evenements, sources);
        Assert.That(chargement.EstValide, Is.True,
            chargement.ConstruireMessageErreurs());
        return chargement.Catalogue;
    }

    private static string ConstruireJsonEvenements(
        params (string id, string categorie, string titre)[] definitions)
    {
        return "[" + string.Join(",", definitions.Select(definition =>
            "{\"id\":\"" + definition.id + "\"," +
            "\"categorie\":\"" + definition.categorie + "\"," +
            "\"titre\":\"" + definition.titre + "\"," +
            "\"importance\":\"Faible\"," +
            "\"message\":\"Message\"," +
            "\"impacts\":[{\"actif\":\"Nvidia\"," +
            "\"variation\":0.1}]}")) + "]";
    }

    private static string ConstruireJsonSources(
        params (string id, string domaine, float fiabilite)[] definitions)
    {
        return "[" + string.Join(",", definitions.Select(definition =>
            "{\"id\":\"" + definition.id + "\"," +
            "\"nom\":\"Source\",\"type\":\"test\"," +
            "\"domaines\":[\"" + definition.domaine + "\"]," +
            "\"fiabilite\":" +
            definition.fiabilite.ToString(
                System.Globalization.CultureInfo.InvariantCulture) + "}")) +
            "]";
    }

    private static GameData CreerGameData()
    {
        GameData gameData = ScriptableObject.CreateInstance<GameData>();
        gameData.ResetData();
        return gameData;
    }

    private sealed class GenerateurFixe : IGenerateurAleatoire
    {
        private readonly double valeur;

        public GenerateurFixe(double valeur)
        {
            this.valeur = valeur;
        }

        public uint Etat { get; private set; } = 1u;

        public int ProchainEntier(int minimumInclus, int maximumExclus)
        {
            Etat++;
            return minimumInclus;
        }

        public double ProchaineValeur()
        {
            Etat++;
            return valeur;
        }
    }
}
