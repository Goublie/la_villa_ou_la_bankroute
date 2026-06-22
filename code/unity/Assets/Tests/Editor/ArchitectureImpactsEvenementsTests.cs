using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Caracterise la liaison entre confirmations et impacts de marche.
/// </summary>
public class ArchitectureImpactsEvenementsTests
{
    [Test]
    public void ConfirmationNvidia_ModifieLePrixAuMoisDeConfirmation()
    {
        GameData gameData = CreerGameData();
        try
        {
            float prixInitial = PrixSansImpact("nvidia", 2);
            AjouterConfirmation(
                gameData,
                "rumeur-nvidia",
                2,
                Impact("nvidia", 0.10f));

            ResultatConsommationImpactsBoursiers resultat =
                Consommer(gameData, 2);

            Assert.That(resultat.Succes, Is.True);
            Assert.That(resultat.EvenementsConsommes, Is.EqualTo(1));
            Assert.That(resultat.ImpactsAppliques, Is.EqualTo(1));
            Assert.That(
                MarcheBoursier.ObtenirPrix(
                    "nvidia",
                    2,
                    gameData.joueur.bourse),
                Is.EqualTo(prixInitial * 1.10f).Within(0.01f));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void MemeEvenement_NEstConsommeQuUneFois()
    {
        GameData gameData = CreerGameData();
        try
        {
            AjouterConfirmation(
                gameData,
                "rumeur-unique",
                1,
                Impact("Nvidia", 0.20f));

            ResultatConsommationImpactsBoursiers premier =
                Consommer(gameData, 1);
            float prixPremier = MarcheBoursier.ObtenirPrix(
                "nvidia",
                1,
                gameData.joueur.bourse);
            ResultatConsommationImpactsBoursiers second =
                Consommer(gameData, 1);

            Assert.That(premier.EvenementsConsommes, Is.EqualTo(1));
            Assert.That(second.EvenementsConsommes, Is.EqualTo(0));
            Assert.That(gameData.joueur.bourse.impactsMarche, Has.Count.EqualTo(1));
            Assert.That(
                MarcheBoursier.ObtenirPrix(
                    "nvidia",
                    1,
                    gameData.joueur.bourse),
                Is.EqualTo(prixPremier).Within(0.001f));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void RumeurInvalidee_NeModifiePasLaBourse()
    {
        GameData gameData = CreerGameData();
        try
        {
            AjouterRumeur(gameData, "rumeur-invalidee", EtatRumeur.Invalidee);

            ResultatConsommationImpactsBoursiers resultat =
                Consommer(gameData, 1);

            Assert.That(resultat.ImpactsAppliques, Is.EqualTo(0));
            Assert.That(gameData.joueur.bourse.impactsMarche, Is.Empty);
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void RumeurEnAttente_NeModifiePasLaBourse()
    {
        GameData gameData = CreerGameData();
        try
        {
            AjouterRumeur(gameData, "rumeur-attente", EtatRumeur.EnAttente);

            ResultatConsommationImpactsBoursiers resultat =
                Consommer(gameData, 1);

            Assert.That(resultat.ImpactsAppliques, Is.EqualTo(0));
            Assert.That(gameData.joueur.bourse.impactsMarche, Is.Empty);
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void EvenementProfessionnel_ResteDisponiblePourSonFuturMoteur()
    {
        GameData gameData = CreerGameData();
        try
        {
            EvenementConfirmePartie evenement = AjouterConfirmation(
                gameData,
                "rumeur-professionnelle",
                1,
                Impact("Revenus", 0.25f));
            evenement.categorie = CategoriesEvenements.Professionnels;

            ResultatConsommationImpactsBoursiers resultat =
                Consommer(gameData, 1);

            Assert.That(resultat.Succes, Is.True);
            Assert.That(resultat.Diagnostics, Is.Empty);
            Assert.That(gameData.joueur.bourse.impactsMarche, Is.Empty);
            Assert.That(evenement.consommeParMoteurImpacts, Is.False);
            Assert.That(
                evenement.etatTraitementImpacts,
                Is.EqualTo(
                    EtatTraitementImpactsEvenement.EnAttente));
            Assert.That(evenement.diagnosticTraitementImpacts, Is.Empty);
            Assert.That(
                gameData.evenements.ObtenirConfirmationsAConsommer(),
                Has.Member(evenement));
            Assert.That(
                gameData.evenements
                    .ObtenirConfirmationsBoursieresAConsommer(),
                Is.Empty);

            ResultatConsommationImpactsBoursiers second =
                Consommer(gameData, 2);
            Assert.That(second.Succes, Is.True);
            Assert.That(second.Diagnostics, Is.Empty);
            Assert.That(
                evenement.etatTraitementImpacts,
                Is.EqualTo(
                    EtatTraitementImpactsEvenement.EnAttente));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void DeuxEvenementsSurMemeActif_CumulentLeursCoefficients()
    {
        GameData gameData = CreerGameData();
        try
        {
            float prixInitial = PrixSansImpact("nvidia", 3);
            AjouterConfirmation(
                gameData,
                "rumeur-hausse",
                3,
                Impact("Nvidia", 0.10f));
            AjouterConfirmation(
                gameData,
                "rumeur-baisse",
                3,
                Impact("nvidia", -0.20f));

            ResultatConsommationImpactsBoursiers resultat =
                Consommer(gameData, 3);

            Assert.That(resultat.EvenementsConsommes, Is.EqualTo(2));
            Assert.That(gameData.joueur.bourse.impactsMarche, Has.Count.EqualTo(2));
            Assert.That(
                MarcheBoursier.ObtenirPrix(
                    "nvidia",
                    3,
                    gameData.joueur.bourse),
                Is.EqualTo(prixInitial * 1.10f * 0.80f).Within(0.01f));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void ImpactSurAutreActif_NeModifiePasNvidia()
    {
        GameData gameData = CreerGameData();
        try
        {
            float prixInitial = PrixSansImpact("nvidia", 1);
            AjouterConfirmation(
                gameData,
                "rumeur-alphabet",
                1,
                Impact("Google", 0.50f));

            Consommer(gameData, 1);

            Assert.That(
                MarcheBoursier.ObtenirPrix(
                    "nvidia",
                    1,
                    gameData.joueur.bourse),
                Is.EqualTo(prixInitial).Within(0.001f));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [TestCase("CAC 40", "cac40")]
    [TestCase("cac-40", "cac40")]
    [TestCase(" Google ", "alphabet")]
    [TestCase("ALPHABET", "alphabet")]
    [TestCase("Total Energies", "totalenergies")]
    public void AliasCatalogue_EstNormalise(
        string alias,
        string attendu)
    {
        bool reconnu = CatalogueEvenements.EssayerObtenirActifBourse(
            alias,
            out string actifId);

        Assert.That(reconnu, Is.True);
        Assert.That(actifId, Is.EqualTo(attendu));
    }

    [Test]
    public void EvenementBoursierSansImpact_EstRejeteUneSeuleFois()
    {
        GameData gameData = CreerGameData();
        try
        {
            EvenementConfirmePartie evenement = AjouterConfirmation(
                gameData,
                "rumeur-sans-impact",
                1);

            ResultatConsommationImpactsBoursiers premier =
                Consommer(gameData, 1);

            Assert.That(premier.Succes, Is.False);
            Assert.That(
                premier.Diagnostics[0],
                Does.Contain("sans impact"));
            Assert.That(gameData.joueur.bourse.impactsMarche, Is.Empty);
            Assert.That(evenement.consommeParMoteurImpacts, Is.False);
            Assert.That(
                evenement.etatTraitementImpacts,
                Is.EqualTo(
                    EtatTraitementImpactsEvenement.RejeteInvalide));

            ResultatConsommationImpactsBoursiers second =
                Consommer(gameData, 2);
            Assert.That(second.Succes, Is.True);
            Assert.That(second.Diagnostics, Is.Empty);
            Assert.That(second.ImpactsAppliques, Is.EqualTo(0));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void CibleInconnue_RefuseToutLEvenementSansMutation()
    {
        GameData gameData = CreerGameData();
        try
        {
            EvenementConfirmePartie evenement = AjouterConfirmation(
                gameData,
                "rumeur-inconnue",
                1,
                Impact("Nvidia", 0.10f),
                Impact("Or", 0.20f));

            ResultatConsommationImpactsBoursiers resultat =
                Consommer(gameData, 1);

            Assert.That(resultat.Succes, Is.False);
            Assert.That(resultat.Diagnostics[0], Does.Contain("Or"));
            Assert.That(gameData.joueur.bourse.impactsMarche, Is.Empty);
            Assert.That(evenement.consommeParMoteurImpacts, Is.False);
            Assert.That(
                evenement.etatTraitementImpacts,
                Is.EqualTo(
                    EtatTraitementImpactsEvenement.RejeteInvalide));
            Assert.That(
                evenement.diagnosticTraitementImpacts,
                Does.Contain("Or"));

            ResultatConsommationImpactsBoursiers second =
                Consommer(gameData, 2);
            Assert.That(second.Succes, Is.True);
            Assert.That(second.ImpactsAppliques, Is.EqualTo(0));
            Assert.That(second.Diagnostics, Is.Empty);
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void VariationInvalide_RefuseUnCoefficientNonPositif()
    {
        GameData gameData = CreerGameData();
        try
        {
            AjouterConfirmation(
                gameData,
                "rumeur-coefficient-invalide",
                1,
                Impact("Nvidia", -1f));

            ResultatConsommationImpactsBoursiers resultat =
                Consommer(gameData, 1);

            Assert.That(resultat.Succes, Is.False);
            Assert.That(gameData.joueur.bourse.impactsMarche, Is.Empty);
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void Impact_ExpireApresSaDuree()
    {
        GameData gameData = CreerGameData();
        try
        {
            AjouterConfirmation(
                gameData,
                "rumeur-courte",
                2,
                Impact("Nvidia", 0.25f));
            Consommer(gameData, 2);

            Assert.That(
                MarcheBoursier.ObtenirPrix(
                    "nvidia",
                    2,
                    gameData.joueur.bourse),
                Is.EqualTo(PrixSansImpact("nvidia", 2) * 1.25f)
                    .Within(0.01f));
            Assert.That(
                MarcheBoursier.ObtenirPrix(
                    "nvidia",
                    3,
                    gameData.joueur.bourse),
                Is.EqualTo(PrixSansImpact("nvidia", 3)).Within(0.001f));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void Snapshot_CopieProfondementLesImpactsActifs()
    {
        GameData gameData = CreerGameData();
        try
        {
            AjouterConfirmation(
                gameData,
                "rumeur-snapshot",
                0,
                Impact("Nvidia", 0.15f));
            Consommer(gameData, 0);

            SnapshotEtatJeu snapshot = new SnapshotEtatJeu(gameData);
            ImpactEvenementMarche original =
                gameData.joueur.bourse.impactsMarche[0];
            ImpactEvenementMarche copie =
                snapshot.joueur.bourse.impactsMarche[0];
            copie.coefficientPrix = 9f;

            Assert.That(copie, Is.Not.SameAs(original));
            Assert.That(original.coefficientPrix, Is.EqualTo(1.15f));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void Snapshot_CopieEtatEtDiagnosticDesImpactsRejetes()
    {
        GameData gameData = CreerGameData();
        try
        {
            EvenementConfirmePartie evenement = AjouterConfirmation(
                gameData,
                "rumeur-rejet-snapshot",
                1,
                Impact("Or", 0.20f));
            Consommer(gameData, 1);

            SnapshotEtatJeu snapshot = new SnapshotEtatJeu(gameData);
            EvenementConfirmePartie copie =
                snapshot.evenements.evenementsConfirmes.Find(
                    element =>
                        element != null &&
                        element.rumeurId == evenement.rumeurId);

            Assert.That(copie, Is.Not.Null);
            Assert.That(copie, Is.Not.SameAs(evenement));
            Assert.That(
                copie.etatTraitementImpacts,
                Is.EqualTo(
                    EtatTraitementImpactsEvenement.RejeteInvalide));
            Assert.That(
                copie.diagnosticTraitementImpacts,
                Is.EqualTo(evenement.diagnosticTraitementImpacts));

            copie.diagnosticTraitementImpacts = "modifie dans le snapshot";
            Assert.That(
                evenement.diagnosticTraitementImpacts,
                Does.Contain("Or"));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void RecreerLesServices_NeDoublePasLImpact()
    {
        GameData gameData = CreerGameData();
        try
        {
            AjouterConfirmation(
                gameData,
                "rumeur-reinitialisation",
                0,
                Impact("Nvidia", 0.10f));

            Consommer(gameData, 0);
            ResultatConsommationImpactsBoursiers resultat =
                Consommer(gameData, 0);

            Assert.That(resultat.EvenementsConsommes, Is.EqualTo(0));
            Assert.That(gameData.joueur.bourse.impactsMarche, Has.Count.EqualTo(1));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    [Test]
    public void PassageMensuel_ValoriseLOuvertureAvecLEvenementConfirme()
    {
        GameData gameData = CreerGameData();
        try
        {
            gameData.evenements.dernierMoisTraite = 0;
            gameData.evenements.rumeurs.Add(new RumeurPartie
            {
                id = "rumeur-passage",
                evenementId = "014",
                sourceId = "src_waffle_street",
                categorie = CategoriesEvenements.Boursiers,
                titrePublic = "Resultats Nvidia",
                textePublic = "Une publication est attendue.",
                moisApparition = 0,
                moisResolution = 1,
                probabiliteConfirmation = 1f,
                etat = EtatRumeur.EnAttente
            });
            gameData.joueur.bourse.positions.Add(
                new PositionBourse("nvidia")
                {
                    quantite = 1f,
                    coutTotalCentimes = 10000
                });
            int valeurSansImpact = Mathf.RoundToInt(
                PrixSansImpact("nvidia", 1) * 100f);

            ResultatPassageMensuel resultat =
                new ServicePassageMensuel(gameData)
                    .PasserAuMoisSuivant();

            EvenementConfirmePartie confirmation =
                gameData.evenements.evenementsConfirmes.Find(
                    evenement => evenement.rumeurId == "rumeur-passage");
            int valeurAttendue = Mathf.RoundToInt(
                MarcheBoursier.ObtenirPrix(
                    "nvidia",
                    1,
                    gameData.joueur.bourse) * 100f);
            Assert.That(resultat.Succes, Is.True);
            Assert.That(confirmation, Is.Not.Null);
            Assert.That(confirmation.consommeParMoteurImpacts, Is.True);
            Assert.That(gameData.joueur.bourse.impactsMarche, Has.Count.EqualTo(2));
            Assert.That(gameData.joueur.bourse.moisValorisation, Is.EqualTo(1));
            Assert.That(
                gameData.joueur.bourse.valeurMarcheCentimes,
                Is.EqualTo(valeurAttendue));
            Assert.That(valeurAttendue, Is.GreaterThan(valeurSansImpact));
        }
        finally
        {
            Object.DestroyImmediate(gameData);
        }
    }

    private static ResultatConsommationImpactsBoursiers Consommer(
        GameData gameData,
        int mois)
    {
        ResultatChargementCatalogue chargement =
            CatalogueEvenements.ChargerDepuisResources();
        Assert.That(
            chargement.EstValide,
            Is.True,
            chargement.ConstruireMessageErreurs());
        ServiceOrchestrationEvenements orchestration =
            ServiceOrchestrationEvenements.CreerPourJeu(
                gameData.evenements,
                chargement.Catalogue);
        return new ServiceEvenementsEconomiques(gameData)
            .ConsommerConfirmationsBoursieres(orchestration, mois);
    }

    private static EvenementConfirmePartie AjouterConfirmation(
        GameData gameData,
        string rumeurId,
        int mois,
        params ImpactDefinitionEvenement[] impacts)
    {
        EvenementConfirmePartie evenement = new EvenementConfirmePartie
        {
            definitionId = "definition-" + rumeurId,
            rumeurId = rumeurId,
            sourceId = "source-test",
            categorie = CategoriesEvenements.Boursiers,
            importance = "Moderee",
            titre = "Evenement de test",
            message = "Message de test",
            moisConfirmation = mois,
            etat = EtatEvenementPartie.Confirme,
            impacts = new List<ImpactDefinitionEvenement>(impacts)
        };
        gameData.evenements.evenementsConfirmes.Add(evenement);
        return evenement;
    }

    private static ImpactDefinitionEvenement Impact(
        string actif,
        float variation)
    {
        return new ImpactDefinitionEvenement
        {
            actif = actif,
            variation = variation
        };
    }

    private static void AjouterRumeur(
        GameData gameData,
        string id,
        EtatRumeur etat)
    {
        gameData.evenements.rumeurs.Add(new RumeurPartie
        {
            id = id,
            evenementId = "definition-" + id,
            categorie = CategoriesEvenements.Boursiers,
            etat = etat,
            moisApparition = 0,
            moisResolution = 1
        });
    }

    private static float PrixSansImpact(string actifId, int mois)
    {
        return MarcheBoursier.ObtenirPrix(
            actifId,
            mois,
            new DonneesBourse());
    }

    private static GameData CreerGameData()
    {
        GameData gameData = ScriptableObject.CreateInstance<GameData>();
        gameData.ResetData();
        return gameData;
    }
}
