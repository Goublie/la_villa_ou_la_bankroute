using System;

/// <summary>
/// Porte les regles de creation, developpement et valorisation d'un projet.
/// </summary>
public sealed class ServiceEntrepreneuriat : IEvolutionMensuelle
{
    public const int MaximumStat = 100;
    public const int InjectionCentimes = 100000;

    private readonly DonneesEntrepreneuriat donnees;
    private readonly DonneesJoueur joueur;
    private readonly CompteBanquaire compteCourant;
    private readonly ServiceBanque banque;

    /// <summary>
    /// Cree un service lie aux agregats persistants et au compte courant.
    /// </summary>
    public ServiceEntrepreneuriat(
        DonneesEntrepreneuriat donnees,
        DonneesJoueur joueur,
        CompteBanquaire compteCourant,
        ServiceBanque banque)
    {
        this.donnees = donnees ??
            throw new ArgumentNullException(nameof(donnees));
        this.joueur = joueur ??
            throw new ArgumentNullException(nameof(joueur));
        this.compteCourant = compteCourant;
        this.banque = banque;
        donnees.InitialiserSiNecessaire();
        BornerValeurs();
        MettreAJourValorisation();
    }

    public ProjetEntrepreneurial Projet => donnees.projet;

    /// <summary>
    /// Retourne le profil derive des trois choix et des pivots.
    /// </summary>
    public ProfilProjetEntrepreneurial CalculerProfil()
    {
        DefinitionChoixEntrepreneurial secteur =
            ObtenirDefinitionSecteur();
        DefinitionChoixEntrepreneurial publicCible =
            ObtenirDefinitionPublic();
        DefinitionChoixEntrepreneurial technologie =
            ObtenirDefinitionTechnologie();

        int difficulte = 30;
        int potentiel = 38;
        int concurrence = 28;
        int compatibilite = 50;
        int coutCentimes = 250000;

        AjouterFacteurs(
            secteur,
            ref difficulte,
            ref potentiel,
            ref concurrence,
            ref coutCentimes);
        AjouterFacteurs(
            publicCible,
            ref difficulte,
            ref potentiel,
            ref concurrence,
            ref coutCentimes);
        AjouterFacteurs(
            technologie,
            ref difficulte,
            ref potentiel,
            ref concurrence,
            ref coutCentimes);

        AppliquerSynergies(
            ref difficulte,
            ref potentiel,
            ref concurrence,
            ref compatibilite,
            ref coutCentimes);
        compatibilite += Projet.bonusCompatibilite;

        difficulte = Limiter(difficulte, 10, 95);
        potentiel = Limiter(potentiel, 15, 100);
        concurrence = Limiter(concurrence, 10, 95);
        compatibilite = Limiter(compatibilite, 15, 100);
        coutCentimes = Math.Max(100000, coutCentimes);

        int ajustementOptions =
            secteur.Probabilite +
            publicCible.Probabilite +
            technologie.Probabilite;
        int probabiliteBase = Limiter(
            Arrondir(
                70 +
                ajustementOptions +
                ((compatibilite - 50) * 0.3f) -
                (difficulte * 0.28f) -
                (concurrence * 0.1f)),
            8,
            85);
        int valorisationBase =
            (coutCentimes * 2) +
            (potentiel * 65000) +
            (compatibilite * 30000);

        return new ProfilProjetEntrepreneurial(
            difficulte,
            potentiel,
            concurrence,
            compatibilite,
            coutCentimes,
            probabiliteBase,
            valorisationBase);
    }

    /// <summary>
    /// Calcule la probabilite courante de succes, en pourcentage.
    /// </summary>
    public int CalculerProbabiliteFinale()
    {
        ProfilProjetEntrepreneurial profil = CalculerProfil();
        float probabilite = profil.ProbabiliteBase;
        probabilite += (joueur.energie - 50) * 0.12f;
        probabilite += (joueur.santeMentale - 50) * 0.12f;

        int cashDisponible =
            compteCourant != null
                ? compteCourant.GetSolde().centimes
                : 0;
        float couverture = profil.CoutLancementCentimes > 0
            ? (cashDisponible + Projet.tresorerieCentimes) /
                (float)profil.CoutLancementCentimes
            : 1f;
        probabilite += couverture >= 1f
            ? 8f
            : -18f * (1f - Limiter01(couverture));
        probabilite += Projet.progressionProduit * 0.15f;
        probabilite += Projet.tractionMarche * 0.18f;
        probabilite += Projet.reputation * 0.08f;
        probabilite += Projet.connaissanceMarche * 0.08f;
        probabilite -= Math.Max(0, profil.Difficulte - 50) * 0.08f;
        probabilite -= Math.Max(0, Projet.nombrePivots - 1) * 4f;

        return Limiter(Arrondir(probabilite), 1, 95);
    }

    /// <summary>
    /// Calcule la valorisation estimee du projet en centimes.
    /// </summary>
    public int CalculerValorisationCentimes()
    {
        ProfilProjetEntrepreneurial profil = CalculerProfil();
        if (!Projet.estCree)
        {
            return profil.ValorisationBaseCentimes;
        }

        long valeur =
            profil.ValorisationBaseCentimes +
            Projet.tresorerieCentimes +
            (Projet.progressionProduit * 50000L) +
            (Projet.tractionMarche * 80000L) +
            (Projet.reputation * 30000L);
        return LimiterEntierPositif(valeur);
    }

    /// <summary>
    /// Retourne le statut fonctionnel derive de l'avancement.
    /// </summary>
    public string CalculerStatutProjet()
    {
        if (Projet.progressionProduit < 25)
        {
            return "Idee";
        }

        if (Projet.progressionProduit < 60)
        {
            return "Prototype";
        }

        if (Projet.tractionMarche < 40)
        {
            return "Recherche de traction";
        }

        return Projet.tresorerieCentimes < 1000000
            ? "Levee"
            : "Croissance";
    }

    /// <summary>
    /// Selectionne le secteur suivant avant la creation du projet.
    /// </summary>
    public ResultatOperation ChoisirSecteurSuivant()
    {
        if (Projet.estCree)
        {
            return SelectionVerrouillee();
        }

        Projet.secteur = (SecteurEntrepreneurial)(
            ((int)Projet.secteur + 1) %
            CatalogueEntrepreneuriat.NombreSecteurs);
        return Reussite(
            "Secteur selectionne : " +
            ObtenirDefinitionSecteur().Nom + ".");
    }

    /// <summary>
    /// Selectionne le public suivant avant la creation du projet.
    /// </summary>
    public ResultatOperation ChoisirPublicSuivant()
    {
        if (Projet.estCree)
        {
            return SelectionVerrouillee();
        }

        Projet.publicCible = (PublicEntrepreneurial)(
            ((int)Projet.publicCible + 1) %
            CatalogueEntrepreneuriat.NombrePublics);
        return Reussite(
            "Public cible : " +
            ObtenirDefinitionPublic().Nom + ".");
    }

    /// <summary>
    /// Selectionne la technologie suivante avant la creation du projet.
    /// </summary>
    public ResultatOperation ChoisirTechnologieSuivante()
    {
        if (Projet.estCree)
        {
            return SelectionVerrouillee();
        }

        Projet.technologie = (TechnologieEntrepreneuriale)(
            ((int)Projet.technologie + 1) %
            CatalogueEntrepreneuriat.NombreTechnologies);
        return Reussite(
            "Technologie selectionnee : " +
            ObtenirDefinitionTechnologie().Nom + ".");
    }

    /// <summary>
    /// Initialise le projet avec les choix courants.
    /// </summary>
    public ResultatOperation CreerProjet()
    {
        if (Projet.estCree)
        {
            return ResultatOperation.Echec(
                "Le projet est deja cree.",
                "projet_existant");
        }

        Projet.estCree = true;
        Projet.progressionProduit = 5;
        Projet.tractionMarche = 0;
        Projet.reputation = 5;
        Projet.connaissanceMarche = 0;
        Projet.tresorerieCentimes = 0;
        Projet.nombrePivots = 0;
        Projet.bonusCompatibilite = 0;
        return Reussite(
            "Projet cree : " +
            ObtenirDefinitionSecteur().Nom + ", pour " +
            ObtenirDefinitionPublic().Nom + ", avec " +
            ObtenirDefinitionTechnologie().Nom + ".");
    }

    /// <summary>
    /// Finance et execute une iteration de developpement produit.
    /// </summary>
    public ResultatOperation DevelopperProduit()
    {
        ResultatOperation projetValide = VerifierProjetCree();
        if (!projetValide.Succes)
        {
            return projetValide;
        }

        ProfilProjetEntrepreneurial profil = CalculerProfil();
        int coutCentimes = (120 + (profil.Difficulte * 6)) * 100;
        int energie = 10 + (profil.Difficulte / 15);
        int sante = 4 + (profil.Difficulte / 30);
        ResultatOperation ressources =
            VerifierRessources(energie, sante);
        if (!ressources.Succes)
        {
            return ressources;
        }

        ResultatOperation paiement =
            PayerDepenseProjet(
                coutCentimes,
                "Developpement du produit");
        if (!paiement.Succes)
        {
            return paiement;
        }

        ConsommerRessources(energie, sante);
        Projet.progressionProduit += Limiter(
            20 - (profil.Difficulte / 10),
            8,
            16);
        Projet.reputation +=
            Projet.progressionProduit >= 55 ? 3 : 1;
        return Reussite(
            "Le prototype avance. Cout : " +
            new argent(coutCentimes) + ".");
    }

    /// <summary>
    /// Achete une etude de marche et ameliore la connaissance du public.
    /// </summary>
    public ResultatOperation EtudierMarche()
    {
        ResultatOperation projetValide = VerifierProjetCree();
        if (!projetValide.Succes)
        {
            return projetValide;
        }

        ProfilProjetEntrepreneurial profil = CalculerProfil();
        int coutCentimes =
            (180 + (profil.RisqueConcurrentiel * 4)) * 100;
        ResultatOperation ressources = VerifierRessources(10, 4);
        if (!ressources.Succes)
        {
            return ressources;
        }

        ResultatOperation paiement =
            PayerDepenseProjet(coutCentimes, "Etude de marche");
        if (!paiement.Succes)
        {
            return paiement;
        }

        ConsommerRessources(10, 4);
        Projet.connaissanceMarche += 15;
        Projet.tractionMarche += Limiter(
            14 - (profil.RisqueConcurrentiel / 12),
            6,
            12);
        Projet.bonusCompatibilite +=
            profil.Compatibilite < 60 ? 4 : 2;
        return Reussite(
            "L'etude confirme une demande chez " +
            ObtenirDefinitionPublic().Nom.ToLowerInvariant() + ".");
    }

    /// <summary>
    /// Transfere 1 000 euros du compte courant vers la tresorerie du projet.
    /// </summary>
    public ResultatOperation InjecterMilleEuros()
    {
        ResultatOperation projetValide = VerifierProjetCree();
        if (!projetValide.Succes)
        {
            return projetValide;
        }

        if (banque == null)
        {
            return ResultatOperation.Echec(
                "Le service bancaire est indisponible.",
                "banque_absente");
        }

        ResultatOperation debit = banque.Debiter(
            compteCourant,
            new argent(InjectionCentimes),
            "Injection dans le projet");
        if (!debit.Succes)
        {
            return debit;
        }

        Projet.tresorerieCentimes += InjectionCentimes;
        return Reussite(
            "1 000 EUR ont ete transferes vers la tresorerie.");
    }

    /// <summary>
    /// Execute un pitch deterministe a partir de l'etat persistant.
    /// </summary>
    public ResultatOperation PitcherInvestisseurs()
    {
        ResultatOperation projetValide = VerifierProjetCree();
        if (!projetValide.Succes)
        {
            return projetValide;
        }

        if (Projet.progressionProduit < 20 ||
            Projet.tractionMarche < 10)
        {
            return ResultatOperation.Echec(
                "Les investisseurs attendent 20 % de produit et 10 % de traction.",
                "pitch_trop_tot");
        }

        ResultatOperation ressources = VerifierRessources(18, 10);
        if (!ressources.Succes)
        {
            return ressources;
        }

        ConsommerRessources(18, 10);
        int probabilite = Limiter(
            CalculerProbabiliteFinale() +
            (Projet.tractionMarche / 5) +
            (Projet.reputation / 8) -
            8,
            5,
            90);
        if (TirerPourcentage() < probabilite)
        {
            ProfilProjetEntrepreneurial profil = CalculerProfil();
            int leveeCentimes = LimiterEntierPositif(
                400000L +
                (profil.PotentielMarche * 12000L) +
                (Projet.tractionMarche * 10000L) +
                (Projet.reputation * 5000L));
            Projet.tresorerieCentimes += leveeCentimes;
            Projet.reputation += 10;
            Projet.tractionMarche += 5;
            return Reussite(
                "Pitch reussi : " +
                new argent(leveeCentimes) +
                " rejoignent la tresorerie.",
                new argent(leveeCentimes));
        }

        joueur.santeMentale -= 5;
        Projet.reputation -= 3;
        return EchecApresMutation(
            "Pitch refuse : le dossier manque encore de traction.",
            "pitch_refuse");
    }

    /// <summary>
    /// Ajuste le positionnement au prix d'une perte de progression.
    /// </summary>
    public ResultatOperation Pivoter()
    {
        ResultatOperation projetValide = VerifierProjetCree();
        if (!projetValide.Succes)
        {
            return projetValide;
        }

        ResultatOperation ressources = VerifierRessources(14, 10);
        if (!ressources.Succes)
        {
            return ressources;
        }

        ProfilProjetEntrepreneurial profil = CalculerProfil();
        ConsommerRessources(14, 10);
        Projet.nombrePivots++;
        Projet.progressionProduit =
            Math.Max(5, Projet.progressionProduit - 10);
        Projet.connaissanceMarche += 8;
        Projet.bonusCompatibilite +=
            profil.Compatibilite < 65 ? 10 : 4;
        Projet.tractionMarche +=
            profil.Compatibilite < 65 ? 5 : 1;

        return Reussite(
            "Le positionnement est ajuste." +
            (Projet.nombrePivots > 2
                ? " Les pivots repetes inquietent le marche."
                : string.Empty));
    }

    /// <summary>
    /// Restaure les ressources personnelles sans progres direct du projet.
    /// </summary>
    public ResultatOperation ReposerFondateur()
    {
        joueur.energie += 30;
        joueur.santeMentale += 25;
        return Reussite(
            "Le fondateur recupere de l'energie et de la sante mentale.");
    }

    /// <inheritdoc />
    public void AppliquerEvolutionMensuelle(int mois)
    {
        // La V2 ne definit pas encore de revenus ou charges recurrentes.
        // Le passage mensuel borne et revalorise donc l'etat sans inventer de
        // nouvelle regle susceptible de modifier le gameplay valide.
        donnees.dernierMoisObserve = Math.Max(0, mois);
        BornerValeurs();
        MettreAJourValorisation();
    }

    /// <summary>
    /// Migre une fois les anciennes valeurs serialisees dans le prefab.
    /// </summary>
    public void MigrerDepuisPrefab(
        SecteurEntrepreneurial secteur,
        PublicEntrepreneurial publicCible,
        TechnologieEntrepreneuriale technologie,
        bool projetCree,
        int progressionProduit,
        int tractionMarche,
        int reputation,
        int connaissanceMarche,
        int tresorerieCentimes,
        int nombrePivots,
        int bonusCompatibilite,
        string message)
    {
        if (donnees.migrationPrefabEffectuee)
        {
            return;
        }

        Projet.secteur = secteur;
        Projet.publicCible = publicCible;
        Projet.technologie = technologie;
        Projet.estCree = projetCree;
        Projet.progressionProduit = progressionProduit;
        Projet.tractionMarche = tractionMarche;
        Projet.reputation = reputation;
        Projet.connaissanceMarche = connaissanceMarche;
        Projet.tresorerieCentimes = tresorerieCentimes;
        Projet.nombrePivots = nombrePivots;
        Projet.bonusCompatibilite = bonusCompatibilite;
        donnees.dernierMessage = string.IsNullOrEmpty(message)
            ? "Projet entrepreneurial initialise."
            : message;
        donnees.migrationPrefabEffectuee = true;
        BornerValeurs();
        MettreAJourValorisation();
    }

    public DefinitionChoixEntrepreneurial ObtenirDefinitionSecteur()
    {
        return CatalogueEntrepreneuriat.ObtenirSecteur(Projet.secteur);
    }

    public DefinitionChoixEntrepreneurial ObtenirDefinitionPublic()
    {
        return CatalogueEntrepreneuriat.ObtenirPublic(Projet.publicCible);
    }

    public DefinitionChoixEntrepreneurial ObtenirDefinitionTechnologie()
    {
        return CatalogueEntrepreneuriat.ObtenirTechnologie(
            Projet.technologie);
    }

    private ResultatOperation PayerDepenseProjet(
        int montantCentimes,
        string libelle)
    {
        int depuisTresorerie = Math.Min(
            Projet.tresorerieCentimes,
            montantCentimes);
        int resteAPayer = montantCentimes - depuisTresorerie;
        if (resteAPayer > 0 &&
            (compteCourant == null ||
             compteCourant.GetSolde().centimes < resteAPayer))
        {
            return ResultatOperation.Echec(
                "Financement insuffisant. Il manque " +
                new argent(resteAPayer) + ".",
                "financement_insuffisant");
        }

        if (resteAPayer > 0)
        {
            if (banque == null)
            {
                return ResultatOperation.Echec(
                    "Le service bancaire est indisponible.",
                    "banque_absente");
            }

            ResultatOperation debit = banque.Debiter(
                compteCourant,
                new argent(resteAPayer),
                libelle);
            if (!debit.Succes)
            {
                return debit;
            }
        }

        Projet.tresorerieCentimes -= depuisTresorerie;
        return ResultatOperation.Reussite(
            "Depense projet reglee.",
            new argent(montantCentimes));
    }

    private ResultatOperation VerifierProjetCree()
    {
        return Projet.estCree
            ? ResultatOperation.Reussite("Projet disponible.")
            : ResultatOperation.Echec(
                "Creez d'abord un projet.",
                "projet_absent");
    }

    private ResultatOperation VerifierRessources(
        int energie,
        int sante)
    {
        if (joueur.energie < energie)
        {
            return ResultatOperation.Echec(
                "Energie insuffisante.",
                "energie_insuffisante");
        }

        if (joueur.santeMentale < sante)
        {
            return ResultatOperation.Echec(
                "Sante mentale insuffisante.",
                "sante_insuffisante");
        }

        return ResultatOperation.Reussite("Ressources disponibles.");
    }

    private void ConsommerRessources(int energie, int sante)
    {
        joueur.energie -= energie;
        joueur.santeMentale -= sante;
    }

    private ResultatOperation SelectionVerrouillee()
    {
        return ResultatOperation.Echec(
            "Le projet est deja lance. Utilisez Pivoter.",
            "selection_verrouillee");
    }

    private ResultatOperation Reussite(
        string message,
        argent montant = default)
    {
        BornerValeurs();
        MettreAJourValorisation();
        donnees.dernierMessage = message;
        return ResultatOperation.Reussite(message, montant);
    }

    private ResultatOperation EchecApresMutation(
        string message,
        string code)
    {
        BornerValeurs();
        MettreAJourValorisation();
        donnees.dernierMessage = message;
        return ResultatOperation.Echec(message, code);
    }

    private void MettreAJourValorisation()
    {
        Projet.DefinirValorisation(
            CalculerValorisationCentimes());
    }

    private void BornerValeurs()
    {
        Projet.progressionProduit =
            Limiter(Projet.progressionProduit, 0, MaximumStat);
        Projet.tractionMarche =
            Limiter(Projet.tractionMarche, 0, MaximumStat);
        Projet.reputation =
            Limiter(Projet.reputation, 0, MaximumStat);
        Projet.connaissanceMarche =
            Limiter(Projet.connaissanceMarche, 0, MaximumStat);
        Projet.tresorerieCentimes =
            Math.Max(0, Projet.tresorerieCentimes);
        Projet.nombrePivots =
            Math.Max(0, Projet.nombrePivots);
        joueur.energie = Limiter(joueur.energie, 0, MaximumStat);
        joueur.santeMentale =
            Limiter(joueur.santeMentale, 0, MaximumStat);
    }

    private int TirerPourcentage()
    {
        // Generateur congruentiel persistant : une copie de snapshot reprend
        // exactement la meme sequence sans toucher a UnityEngine.Random.
        donnees.etatAleatoire =
            unchecked(
                (1664525u * donnees.etatAleatoire) +
                1013904223u);
        return (int)(donnees.etatAleatoire % 100u);
    }

    private void AppliquerSynergies(
        ref int difficulte,
        ref int potentiel,
        ref int concurrence,
        ref int compatibilite,
        ref int coutCentimes)
    {
        if (Projet.secteur == SecteurEntrepreneurial.Sante &&
            Projet.publicCible == PublicEntrepreneurial.Seniors)
        {
            compatibilite += 14;
            potentiel += 8;
        }

        if (Projet.secteur == SecteurEntrepreneurial.Sante &&
            Projet.technologie ==
                TechnologieEntrepreneuriale.IntelligenceArtificielle)
        {
            compatibilite += 8;
            difficulte += 8;
            coutCentimes += 250000;
        }

        if (Projet.secteur == SecteurEntrepreneurial.Education &&
            Projet.publicCible == PublicEntrepreneurial.Etudiants &&
            Projet.technologie ==
                TechnologieEntrepreneuriale.ApplicationMobile)
        {
            compatibilite += 22;
            concurrence -= 4;
        }

        if (Projet.secteur == SecteurEntrepreneurial.Cybersecurite &&
            Projet.publicCible == PublicEntrepreneurial.Entreprises &&
            Projet.technologie == TechnologieEntrepreneuriale.Saas)
        {
            compatibilite += 22;
            potentiel += 10;
        }

        if (Projet.secteur == SecteurEntrepreneurial.Finance &&
            Projet.publicCible == PublicEntrepreneurial.Investisseurs &&
            Projet.technologie ==
                TechnologieEntrepreneuriale.Blockchain)
        {
            compatibilite += 18;
            potentiel += 12;
            concurrence += 8;
            difficulte += 6;
        }

        if (Projet.secteur == SecteurEntrepreneurial.Commerce &&
            Projet.publicCible == PublicEntrepreneurial.GrandPublic &&
            Projet.technologie ==
                TechnologieEntrepreneuriale.Marketplace)
        {
            compatibilite += 16;
            potentiel += 10;
            concurrence += 12;
        }

        if (Projet.secteur == SecteurEntrepreneurial.Divertissement &&
            Projet.publicCible == PublicEntrepreneurial.JeunesActifs &&
            Projet.technologie ==
                TechnologieEntrepreneuriale.JeuSimulation)
        {
            compatibilite += 20;
            potentiel += 10;
        }

        compatibilite += CompatibiliteGenerale();
    }

    private int CompatibiliteGenerale()
    {
        int bonus = 0;
        if (Projet.publicCible == PublicEntrepreneurial.Entreprises &&
            (Projet.technologie == TechnologieEntrepreneuriale.Saas ||
             Projet.technologie ==
                TechnologieEntrepreneuriale.Automatisation ||
             Projet.technologie ==
                TechnologieEntrepreneuriale.DataAnalyse))
        {
            bonus += 8;
        }

        if ((Projet.publicCible == PublicEntrepreneurial.Etudiants ||
             Projet.publicCible == PublicEntrepreneurial.JeunesActifs) &&
            (Projet.technologie ==
                TechnologieEntrepreneuriale.ApplicationMobile ||
             Projet.technologie ==
                TechnologieEntrepreneuriale.PlateformeWeb))
        {
            bonus += 7;
        }

        if (Projet.publicCible == PublicEntrepreneurial.GrandPublic &&
            (Projet.technologie ==
                TechnologieEntrepreneuriale.ApplicationMobile ||
             Projet.technologie ==
                TechnologieEntrepreneuriale.Marketplace ||
             Projet.technologie ==
                TechnologieEntrepreneuriale.JeuSimulation))
        {
            bonus += 6;
        }

        if ((Projet.secteur == SecteurEntrepreneurial.Transport ||
             Projet.secteur == SecteurEntrepreneurial.Energie ||
             Projet.secteur == SecteurEntrepreneurial.Sante) &&
            Projet.technologie ==
                TechnologieEntrepreneuriale.ObjetsConnectes)
        {
            bonus += 7;
        }

        return bonus;
    }

    private static void AjouterFacteurs(
        DefinitionChoixEntrepreneurial choix,
        ref int difficulte,
        ref int potentiel,
        ref int concurrence,
        ref int coutCentimes)
    {
        difficulte += choix.Difficulte;
        potentiel += choix.Potentiel;
        concurrence += choix.Concurrence;
        coutCentimes += choix.CoutEuros * 100;
    }

    private static int Arrondir(float valeur)
    {
        return (int)Math.Round(
            valeur,
            MidpointRounding.AwayFromZero);
    }

    private static int Limiter(int valeur, int minimum, int maximum)
    {
        return Math.Min(Math.Max(valeur, minimum), maximum);
    }

    private static float Limiter01(float valeur)
    {
        return Math.Min(Math.Max(valeur, 0f), 1f);
    }

    private static int LimiterEntierPositif(long valeur)
    {
        if (valeur <= 0)
        {
            return 0;
        }

        return valeur >= int.MaxValue ? int.MaxValue : (int)valeur;
    }
}
