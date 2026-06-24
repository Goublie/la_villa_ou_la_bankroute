using System;
using System.Collections.Generic;

/// <summary>
/// Orchestre dans un ordre deterministe la cloture et l'ouverture mensuelles.
/// </summary>
/// <remarks>
/// Ce service ne depend d'aucune fenetre Unity. Les valorisations sont donc
/// correctes meme si Banque, Bourse ou Entrepreneuriat restent fermees.
/// </remarks>
public sealed class ServicePassageMensuel
{
    private readonly GameData gameData;

    /// <summary>
    /// Cree un orchestrateur lie a l'agregat racine de la partie.
    /// </summary>
    public ServicePassageMensuel(GameData gameData)
    {
        this.gameData = gameData ??
            throw new ArgumentNullException(nameof(gameData));
    }

    /// <summary>
    /// Repare les donnees, valorise le mois courant et cree le snapshot initial.
    /// </summary>
    /// <remarks>
    /// Cette methode est idempotente : elle n'ajoute le snapshot initial que
    /// si l'historique est vide et n'applique aucun interet ni salaire.
    /// </remarks>
    public ResultatOperation InitialiserPartie()
    {
        AssurerRacine();
        if (!EssayerCreerServiceEvenements(
                out ServiceOrchestrationEvenements evenements,
                out string erreurCatalogue))
        {
            return ResultatOperation.Echec(
                erreurCatalogue,
                "catalogue_evenements_invalide");
        }

        ResultatOrchestrationEvenements resultatEvenements =
            evenements.TraiterMois(gameData.nombreMoisPasses);
        if (!resultatEvenements.Succes)
        {
            return ResultatOperation.Echec(
                resultatEvenements.Message,
                "orchestration_evenements_impossible");
        }

        ResultatConsommationImpactsBoursiers resultatImpacts =
            ConsommerImpactsBoursiers(
                evenements,
                gameData.nombreMoisPasses);
        ActualiserValeursOuverture(gameData.nombreMoisPasses);
        ServiceCycleMensuelWhatIf.OuvrirMois(gameData);

        if (gameData.historiqueSnapshots.Count == 0)
        {
            new ServiceRepartitionTemps(gameData.joueur.tempsApplications)
                .ReinitialiserAllocation();
            EnregistrerSnapshot();
        }

        return ResultatOperation.Reussite(
            CompleterMessageAvecDiagnostics(
                "Partie initialisee.",
                resultatImpacts),
            default,
            "temps_initialise");
    }

    /// <summary>
    /// Clot le mois courant et ouvre le mois suivant.
    /// </summary>
    /// <returns>
    /// Un resultat indiquant les index absolus et une eventuelle transition
    /// annuelle.
    /// </returns>
    /// <remarks>
    /// Ordre garanti :
    /// 1. placements fixes et Livret A ;
    /// 2. marche et entreprise ;
    /// 3. snapshot profond de cloture ;
    /// 4. calendrier ;
    /// 5. resolution des rumeurs et publications du nouveau mois ;
    /// 6. consommation atomique des impacts boursiers confirmes ;
    /// 7. report des comptes et salaire ;
    /// 8. valorisations d'ouverture.
    /// Le salaire du nouveau mois ne peut ainsi jamais contaminer le snapshot
    /// du mois cloture.
    /// </remarks>
    public ResultatPassageMensuel PasserAuMoisSuivant()
    {
        AssurerRacine();
        if (!EssayerCreerServiceEvenements(
                out ServiceOrchestrationEvenements evenements,
                out string erreurCatalogue))
        {
            return ResultatPassageMensuel.Echec(
                erreurCatalogue,
                gameData.nombreMoisPasses);
        }

        int indexCloture = gameData.nombreMoisPasses;
        Mois moisCloture = gameData.moisActuel;

        AppliquerEvolutionsCloture(indexCloture);
        ServiceCycleMensuelWhatIf.CloturerMois(
            gameData,
            indexCloture,
            moisCloture);
        EnregistrerSnapshot();

        bool changementAnnee = moisCloture == Mois.Decembre;
        gameData.nombreMoisPasses = indexCloture + 1;
        gameData.moisActuel = changementAnnee
            ? Mois.Janvier
            : (Mois)((int)moisCloture + 1);

        ResultatOrchestrationEvenements resultatEvenements =
            evenements.TraiterMois(gameData.nombreMoisPasses);
        if (!resultatEvenements.Succes)
        {
            return ResultatPassageMensuel.Echec(
                resultatEvenements.Message,
                gameData.nombreMoisPasses);
        }

        ResultatConsommationImpactsBoursiers resultatImpacts =
            ConsommerImpactsBoursiers(
                evenements,
                gameData.nombreMoisPasses);
        OuvrirNouveauMois(gameData.nombreMoisPasses);
        ServiceCycleMensuelWhatIf.OuvrirMois(gameData);

        return new ResultatPassageMensuel(
            true,
            CompleterMessageAvecDiagnostics(
                "Le mois suivant est ouvert.",
                resultatImpacts),
            indexCloture,
            gameData.nombreMoisPasses,
            changementAnnee);
    }

    private void AppliquerEvolutionsCloture(int mois)
    {
        DonneesJoueur joueur = gameData.joueur;
        ServiceBanque banque = new ServiceBanque(joueur);
        CompteBanquaire compteCourant = banque.ObtenirCompteCourant(); // Récupération du compte courant
        Epargne livret = ObtenirLivretExistant(joueur, banque, mois);

        List<IEvolutionMensuelle> evolutions = new List<IEvolutionMensuelle>();
        
        // --- INTÉGRATION PRÊTS IMMOBILIERS ---
        if (joueur.pretsImmobiliers != null)
        {
            // On boucle à l'envers ou on nettoie après pour pouvoir supprimer les prêts terminés de la liste
            for (int i = joueur.pretsImmobiliers.Count - 1; i >= 0; i--)
            {
                DonneesPret pret = joueur.pretsImmobiliers[i];
                if (pret != null)
                {
                    // Applique le débit mensuel et l'amortissement du capital
                    ServicePretImmobilier.AppliquerMensualite(pret, compteCourant);

                    // Si le prêt est arrivé à échéance, on le retire des prêts actifs
                    if (pret.moisRestants <= 0)
                    {
                        joueur.pretsImmobiliers.RemoveAt(i);
                    }
                }
            }
        }
        if (joueur.immobilier != null && joueur.immobilier.biensPossedes != null)
        {
            foreach (BienImmobilier bien in joueur.immobilier.biensPossedes)
            {
                if (bien != null && bien.estLoue && bien.loyerMensuel.centimes > 0)
                {
                    banque.Crediter(compteCourant, bien.loyerMensuel, "loyer");
                }
            }
        }
        // ==========================================

        // ==========================================
        // PRÉLÈVEMENT MENSUEL DU NIVEAU DE VIE
        // ==========================================
        if (joueur.niveauVie != null)
        {
            argent coutNiveauVie = GestionnaireNiveauVie.CalculerCoutMensuel(joueur.niveauVie);
            if (coutNiveauVie.centimes > 0)
            {
                // On débite même si fonds insuffisants (découvert) — on force via AjoutHistorique
                compteCourant.AjoutHistorique("Dépenses courantes", -coutNiveauVie);
            }

            // Effets sur l'énergie et la santé mentale
            GestionnaireNiveauVie.AppliquerEffetsMensuels(joueur.niveauVie, joueur);
        }
        // ==========================================
        
        if (joueur.investissements != null)
        {
            foreach (Investissement investissement in joueur.investissements)
            {
                if (investissement != null)
                {
                    evolutions.Add(investissement);
                }
            }
        }

        if (livret != null)
        {
            evolutions.Add(livret);
        }

        if (joueur.bourse != null)
        {
            evolutions.Add(new ServiceBourse(joueur.bourse));
        }

        if (joueur.salariat != null)
        {
            evolutions.Add(new ServiceSalariat(joueur.salariat, joueur));
        }

        ServiceEntrepreneuriat entrepreneuriat =
            CreerServiceEntrepreneuriat(joueur, banque);
        if (entrepreneuriat != null)
        {
            evolutions.Add(entrepreneuriat);
        }

        foreach (IEvolutionMensuelle evolution in evolutions)
        {
            evolution.AppliquerEvolutionMensuelle(mois);
        }

        // ==========================================
        // AJOUT : ENCAISSEMENT DES LOYERS IMMOBILIERS
        // ==========================================
        if (joueur.immobilier != null && joueur.immobilier.biensPossedes != null)
        {
            foreach (BienImmobilier bien in joueur.immobilier.biensPossedes)
            {
                if (bien != null && bien.estLoue && bien.loyerMensuel.centimes > 0)
                {
                    banque.Crediter(compteCourant, bien.loyerMensuel, "loyer");
                }
            }
        }
        // ==========================================

        gameData.env.tauxEpargne =
            ServiceLivretA.ObtenirTauxAnnuel(
                mois,
                gameData.env.tauxEpargne);
    }

    private void OuvrirNouveauMois(int mois)
    {
        DonneesJoueur joueur = gameData.joueur;
        if (joueur.comptes != null)
        {
            foreach (CompteBanquaire compte in joueur.comptes.Values)
            {
                compte?.ViderHistorique();
            }
        }

        ServiceBanque banque = new ServiceBanque(joueur);
        if (joueur.salaire.centimes > 0)
        {
            banque.Crediter(
                banque.ObtenirCompteCourant(),
                joueur.salaire,
                "salaire");
        }

        if (joueur.tempsApplications != null)
        {
            new ServiceRepartitionTemps(joueur.tempsApplications)
                .ReinitialiserAllocation();
        }

        ActualiserValeursOuverture(mois, banque);
    }

    private void ActualiserValeursOuverture(
        int mois,
        ServiceBanque banque = null)
    {
        DonneesJoueur joueur = gameData.joueur;
        banque = banque ?? new ServiceBanque(joueur);
        gameData.env.tauxEpargne =
            ServiceLivretA.ObtenirTauxAnnuel(
                mois,
                gameData.env.tauxEpargne);

        if (joueur.bourse != null)
        {
            new ServiceBourse(joueur.bourse)
                .AppliquerEvolutionMensuelle(mois);
        }

        CreerServiceEntrepreneuriat(joueur, banque)?
            .AppliquerEvolutionMensuelle(mois);

        // ===================================================================
        // AJOUT : ESTIMATION MENSUELLE & RECALCUL ANNUEL DES LOYERS (INDEXATION) & ROTATION DU MARCHÉ
        // ===================================================================
        if (joueur.immobilier != null)
        {
            if (joueur.immobilier.biensPossedes != null)
            {
                // 1. On met à jour la valeur du patrimoine chaque mois
                foreach (BienImmobilier bien in joueur.immobilier.biensPossedes)
                {
                    if (bien != null)
                    {
                        bien.valeurActuelle = ServiceImmobilier.CalculerValeurActuelle(bien, mois);
                    }
                }

                // 2. Indexation annuelle des loyers (tous les 12 mois, sauf à l'initialisation mois 0)
                if (mois > 0 && mois % 12 == 0)
                {
                    ServiceImmobilier.ActualiserLoyersAnnuels(joueur, mois);
                }
            }

            // 3. Rafraîchissement automatique du marché toutes les échéances de 6 mois, et au mois 0 s'il n'y a pas d'annonces
            if ((mois == 0 && (joueur.immobilier.annoncesActuelles == null || joueur.immobilier.annoncesActuelles.Count == 0)) || (mois > 0 && mois % 6 == 0))
            {
                ServiceImmobilier.RafraichirMarche(joueur, mois, 3);
            }
        }
        // ===================================================================
    }

    private static Epargne ObtenirLivretExistant(
        DonneesJoueur joueur,
        ServiceBanque banque,
        int mois)
    {
        if (joueur.comptes == null ||
            !joueur.comptes.TryGetValue(
                ServiceBanque.LivretAId,
                out CompteBanquaire compte) ||
            !(compte is Epargne))
        {
            return null;
        }

        return banque.ObtenirLivretA(mois);
    }

    private static ServiceEntrepreneuriat CreerServiceEntrepreneuriat(
        DonneesJoueur joueur,
        ServiceBanque banque)
    {
        if (joueur == null || joueur.entrepreneuriat == null)
        {
            return null;
        }

        return new ServiceEntrepreneuriat(
            joueur.entrepreneuriat,
            joueur,
            banque.ObtenirCompteCourant(),
            banque);
    }

    private void EnregistrerSnapshot()
    {
        gameData.historiqueSnapshots.Add(
            new SnapshotEtatJeu(gameData));
    }

    private ResultatConsommationImpactsBoursiers
        ConsommerImpactsBoursiers(
            ServiceOrchestrationEvenements orchestration,
            int mois)
    {
        return new ServiceEvenementsEconomiques(gameData)
            .ConsommerConfirmationsBoursieres(orchestration, mois);
    }

    private static string CompleterMessageAvecDiagnostics(
        string message,
        ResultatConsommationImpactsBoursiers resultat)
    {
        string diagnostics = resultat?.ConstruireMessageDiagnostics();
        return string.IsNullOrEmpty(diagnostics)
            ? message
            : message + " Diagnostics impacts : " + diagnostics;
    }

    private void AssurerRacine()
    {
        if (gameData.joueur == null)
        {
            gameData.joueur = new DonneesJoueur();
        }
        gameData.joueur.InitialiserSiNecessaire();

        if (gameData.env == null)
        {
            gameData.env = new DonneesEnvironnement();
        }

        if (gameData.evenements == null)
        {
            gameData.evenements = new DonneesEvenements();
        }
        gameData.evenements.InitialiserSiNecessaire();

        if (gameData.whatIf == null)
        {
            gameData.whatIf = new DonneesWhatIf();
        }
        gameData.whatIf.InitialiserSiNecessaire();

        if (gameData.historiqueSnapshots == null)
        {
            gameData.historiqueSnapshots =
                new List<SnapshotEtatJeu>();
        }
    }

    private bool EssayerCreerServiceEvenements(
        out ServiceOrchestrationEvenements service,
        out string erreur)
    {
        ResultatChargementCatalogue chargement =
            CatalogueEvenements.ChargerDepuisResources();
        if (!chargement.EstValide)
        {
            service = null;
            erreur = chargement.ConstruireMessageErreurs();
            return false;
        }

        service = ServiceOrchestrationEvenements.CreerPourJeu(
            gameData.evenements,
            chargement.Catalogue);
        erreur = string.Empty;
        return true;
    }
}
