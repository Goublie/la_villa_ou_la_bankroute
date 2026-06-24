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
        ActualiserValeursOuverture(gameData.nombreMoisPasses);

        if (gameData.historiqueSnapshots.Count == 0)
        {
            new ServiceRepartitionTemps(gameData.joueur.tempsApplications)
                .ReinitialiserAllocation();
            EnregistrerSnapshot();
        }

        return ResultatOperation.Reussite(
            "Partie initialisee.",
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
    /// 5. report des comptes et salaire ;
    /// 6. valorisations d'ouverture.
    /// Le salaire du nouveau mois ne peut ainsi jamais contaminer le snapshot
    /// du mois cloture.
    /// </remarks>
    public ResultatPassageMensuel PasserAuMoisSuivant()
    {
        AssurerRacine();
        int indexCloture = gameData.nombreMoisPasses;
        Mois moisCloture = gameData.moisActuel;

        AppliquerEvolutionsCloture(indexCloture);
        EnregistrerSnapshot();

        bool changementAnnee = moisCloture == Mois.Decembre;
        gameData.nombreMoisPasses = indexCloture + 1;
        gameData.moisActuel = changementAnnee
            ? Mois.Janvier
            : (Mois)((int)moisCloture + 1);

        OuvrirNouveauMois(gameData.nombreMoisPasses);

        return new ResultatPassageMensuel(
            true,
            "Le mois suivant est ouvert.",
            indexCloture,
            gameData.nombreMoisPasses,
            changementAnnee);
    }

    private void AppliquerEvolutionsCloture(int mois)
    {
        DonneesJoueur joueur = gameData.joueur;
        ServiceBanque banque = new ServiceBanque(joueur);
        Epargne livret = ObtenirLivretExistant(joueur, banque, mois);

        List<IEvolutionMensuelle> evolutions =
            new List<IEvolutionMensuelle>();
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
            CompteBanquaire compteCourant = banque.ObtenirCompteCourant();
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
        // AJOUT : ESTIMATION MENSUELLE & RECALCUL ANNUEL DES LOYERS (INDEXATION)
        // ===================================================================
        if (joueur.immobilier != null && joueur.immobilier.biensPossedes != null)
        {
            // 1. On met à jour la valeur du patrimoine chaque mois (pour l'UI et le score global via IPatrimoine)
            foreach (BienImmobilier bien in joueur.immobilier.biensPossedes)
            {
                if (bien != null)
                {
                    bien.valeurActuelle = ServiceImmobilier.CalculerValeurActuelle(bien, mois);
                }
            }

            // 2. Indexation annuelle des loyers (tous les 12 mois écoulés, hors initialisation mois 0)
            if (mois > 0 && mois % 12 == 0)
            {
                ServiceImmobilier.ActualiserLoyersAnnuels(joueur, mois);
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

        if (gameData.historiqueSnapshots == null)
        {
            gameData.historiqueSnapshots =
                new List<SnapshotEtatJeu>();
        }
    }
}