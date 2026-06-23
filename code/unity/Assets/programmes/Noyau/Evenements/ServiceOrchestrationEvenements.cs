using System;
using System.Collections.Generic;

/// <summary>
/// Resultat d'un traitement mensuel des rumeurs et confirmations.
/// </summary>
public readonly struct ResultatOrchestrationEvenements
{
    public ResultatOrchestrationEvenements(
        bool succes,
        string message,
        int rumeursCreees,
        int rumeursResolues,
        int evenementsConfirmes)
    {
        Succes = succes;
        Message = message;
        RumeursCreees = rumeursCreees;
        RumeursResolues = rumeursResolues;
        EvenementsConfirmes = evenementsConfirmes;
    }

    public bool Succes { get; }
    public string Message { get; }
    public int RumeursCreees { get; }
    public int RumeursResolues { get; }
    public int EvenementsConfirmes { get; }

    public static ResultatOrchestrationEvenements Echec(string message)
    {
        return new ResultatOrchestrationEvenements(
            false,
            message,
            0,
            0,
            0);
    }
}

/// <summary>
/// Orchestre les rumeurs et confirmations sans appliquer leurs impacts.
/// </summary>
/// <remarks>
/// Le marqueur dernierMoisTraite garantit qu'un rechargement de scene ou une
/// ouverture d'Actualites ne consomme aucun nouveau tirage.
/// </remarks>
public sealed class ServiceOrchestrationEvenements
{
    private readonly DonneesEvenements donnees;
    private readonly CatalogueEvenements catalogue;
    private readonly IGenerateurAleatoire aleatoire;

    /// <summary>
    /// Cree le service avec des dependances injectables et testables.
    /// </summary>
    public ServiceOrchestrationEvenements(
        DonneesEvenements donnees,
        CatalogueEvenements catalogue,
        IGenerateurAleatoire aleatoire)
    {
        this.donnees = donnees ??
            throw new ArgumentNullException(nameof(donnees));
        this.catalogue = catalogue ??
            throw new ArgumentNullException(nameof(catalogue));
        this.aleatoire = aleatoire ??
            throw new ArgumentNullException(nameof(aleatoire));
        this.donnees.InitialiserSiNecessaire();
        if (this.donnees.graineInitiale == 0u)
        {
            this.donnees.graineInitiale = aleatoire.Etat;
        }
    }

    /// <summary>
    /// Cree le service du jeu en reprenant l'etat aleatoire persistant.
    /// </summary>
    public static ServiceOrchestrationEvenements CreerPourJeu(
        DonneesEvenements donnees,
        CatalogueEvenements catalogue)
    {
        if (donnees == null)
        {
            throw new ArgumentNullException(nameof(donnees));
        }

        return new ServiceOrchestrationEvenements(
            donnees,
            catalogue,
            new GenerateurAleatoireJeu(donnees.etatAleatoire));
    }

    /// <summary>
    /// Traite chaque mois manquant jusqu'au mois demande, une seule fois.
    /// </summary>
    /// <remarks>
    /// Pour chaque mois : les rumeurs dues sont d'abord resolues, puis deux
    /// nouvelles rumeurs distinctes sont publiees. Aucun impact n'est applique.
    /// </remarks>
    public ResultatOrchestrationEvenements TraiterMois(int mois)
    {
        if (mois < 0)
        {
            return ResultatOrchestrationEvenements.Echec(
                "L'index de mois ne peut pas etre negatif.");
        }

        if (mois <= donnees.dernierMoisTraite)
        {
            return new ResultatOrchestrationEvenements(
                true,
                "Ce mois a deja ete traite.",
                0,
                0,
                0);
        }

        int creees = 0;
        int resolues = 0;
        int confirmees = 0;
        for (int moisCourant = donnees.dernierMoisTraite + 1;
            moisCourant <= mois;
            moisCourant++)
        {
            ResoudreRumeurs(moisCourant, ref resolues, ref confirmees);
            ResultatOrchestrationEvenements creation =
                CreerRumeurs(moisCourant);
            if (!creation.Succes)
            {
                SauvegarderEtatAleatoire();
                return creation;
            }

            creees += creation.RumeursCreees;
            donnees.dernierMoisTraite = moisCourant;
        }

        SauvegarderEtatAleatoire();
        return new ResultatOrchestrationEvenements(
            true,
            "Rumeurs et confirmations mises a jour.",
            creees,
            resolues,
            confirmees);
    }

    /// <summary>
    /// Retourne des copies des publications, de la plus recente a la plus ancienne.
    /// </summary>
    public List<PublicationActualite> ObtenirPublicationsTriees()
    {
        donnees.InitialiserSiNecessaire();
        List<PublicationActualite> resultat =
            new List<PublicationActualite>();
        foreach (PublicationActualite publication in donnees.publications)
        {
            if (publication != null)
            {
                resultat.Add(publication.Copier());
            }
        }

        resultat.Sort((gauche, droite) =>
        {
            int mois = droite.moisPublication.CompareTo(gauche.moisPublication);
            return mois != 0
                ? mois
                : droite.ordrePublication.CompareTo(gauche.ordrePublication);
        });
        return resultat;
    }

    /// <summary>
    /// Marque une confirmation comme consommee par le futur moteur d'impacts.
    /// </summary>
    public bool MarquerConfirmationConsommee(string rumeurId)
    {
        return MarquerTraitementImpacts(
            rumeurId,
            EtatTraitementImpactsEvenement.Applique,
            string.Empty);
    }


    public bool MarquerConfirmationRejetee(
        string rumeurId,
        string diagnostic)
    {
        return MarquerTraitementImpacts(
            rumeurId,
            EtatTraitementImpactsEvenement.RejeteInvalide,
            diagnostic);
    }

    private bool MarquerTraitementImpacts(
        string rumeurId,
        EtatTraitementImpactsEvenement etat,
        string diagnostic)
    {
        EvenementConfirmePartie evenement =
            donnees.evenementsConfirmes.Find(
                element => element != null && element.rumeurId == rumeurId);
        if (evenement == null ||
            evenement.consommeParMoteurImpacts ||
            evenement.etatTraitementImpacts !=
                EtatTraitementImpactsEvenement.EnAttente)
        {
            return false;
        }

        evenement.etatTraitementImpacts = etat;
        evenement.diagnosticTraitementImpacts =
            diagnostic ?? string.Empty;
        evenement.consommeParMoteurImpacts =
            etat == EtatTraitementImpactsEvenement.Applique;
        return true;
    }

    private ResultatOrchestrationEvenements CreerRumeurs(int mois)
    {
        HashSet<string> evenementsEnAttente = new HashSet<string>();
        foreach (RumeurPartie rumeur in donnees.rumeurs)
        {
            if (rumeur != null && rumeur.etat == EtatRumeur.EnAttente)
            {
                evenementsEnAttente.Add(rumeur.evenementId);
            }
        }

        List<DefinitionEvenement> eligibles =
            new List<DefinitionEvenement>();
        foreach (DefinitionEvenement definition in catalogue.Evenements)
        {
            if (definition != null &&
                !evenementsEnAttente.Contains(definition.id) &&
                catalogue.ObtenirSourcesCompatibles(definition.categorie).Count > 0)
            {
                eligibles.Add(definition);
            }
        }

        if (eligibles.Count < ParametresEvenements.RumeursParMois)
        {
            return ResultatOrchestrationEvenements.Echec(
                "Moins de deux evenements eligibles disposent d'une source " +
                "compatible.");
        }

        for (int index = 0;
            index < ParametresEvenements.RumeursParMois;
            index++)
        {
            int indexDefinition = aleatoire.ProchainEntier(0, eligibles.Count);
            DefinitionEvenement definition = eligibles[indexDefinition];
            eligibles.RemoveAt(indexDefinition);

            List<SourceActualite> sources =
                catalogue.ObtenirSourcesCompatibles(definition.categorie);
            SourceActualite source = sources[
                aleatoire.ProchainEntier(0, sources.Count)];
            AjouterRumeur(mois, definition, source);
        }

        return new ResultatOrchestrationEvenements(
            true,
            "Deux rumeurs ont ete publiees.",
            ParametresEvenements.RumeursParMois,
            0,
            0);
    }

    private void AjouterRumeur(
        int mois,
        DefinitionEvenement definition,
        SourceActualite source)
    {
        int sequence = donnees.prochaineSequenceRumeur++;
        RumeurPartie rumeur = new RumeurPartie
        {
            id = "rumeur-" + mois.ToString("D4") + "-" +
                sequence.ToString("D4"),
            evenementId = definition.id,
            sourceId = source.id,
            categorie = definition.categorie,
            titrePublic = "Rumeur : " + definition.titre,
            textePublic =
                "Des informations non confirmees evoquent : " +
                definition.titre + ".",
            moisApparition = mois,
            moisResolution = mois + 1,
            probabiliteConfirmation = BornerProbabilite(source.fiabilite),
            etat = EtatRumeur.EnAttente
        };
        donnees.rumeurs.Add(rumeur);
        donnees.publications.Add(new PublicationActualite
        {
            id = "publication-" + donnees.prochaineSequencePublication,
            type = TypePublicationActualite.Rumeur,
            objetId = rumeur.id,
            sourceId = source.id,
            sourceNom = source.nom,
            categorie = rumeur.categorie,
            titre = rumeur.titrePublic,
            texte = rumeur.textePublic,
            moisPublication = mois,
            ordrePublication = donnees.prochaineSequencePublication++,
            etatRumeur = EtatRumeur.EnAttente
        });
    }

    private void ResoudreRumeurs(
        int mois,
        ref int resolues,
        ref int confirmees)
    {
        List<RumeurPartie> aResoudre = donnees.rumeurs.FindAll(
            rumeur => rumeur != null &&
                rumeur.etat == EtatRumeur.EnAttente &&
                rumeur.moisResolution == mois);
        aResoudre.Sort((gauche, droite) =>
            string.CompareOrdinal(gauche.id, droite.id));

        foreach (RumeurPartie rumeur in aResoudre)
        {
            bool confirmee =
                aleatoire.ProchaineValeur() < rumeur.probabiliteConfirmation;
            rumeur.tirageEffectue = true;
            rumeur.resultatConfirmation = confirmee;
            rumeur.etat = confirmee
                ? EtatRumeur.Confirmee
                : EtatRumeur.Invalidee;
            MettreAJourPublicationRumeur(rumeur);
            resolues++;

            if (confirmee)
            {
                CreerEvenementConfirme(rumeur, mois);
                confirmees++;
            }
        }
    }

    private void MettreAJourPublicationRumeur(RumeurPartie rumeur)
    {
        PublicationActualite publication = donnees.publications.Find(
            element => element != null &&
                element.type == TypePublicationActualite.Rumeur &&
                element.objetId == rumeur.id);
        if (publication != null)
        {
            publication.etatRumeur = rumeur.etat;
        }
    }

    private void CreerEvenementConfirme(RumeurPartie rumeur, int mois)
    {
        DefinitionEvenement definition =
            catalogue.TrouverEvenement(rumeur.evenementId);
        SourceActualite source = catalogue.TrouverSource(rumeur.sourceId);
        if (definition == null || source == null)
        {
            throw new InvalidOperationException(
                "La rumeur resolue ne correspond plus au catalogue.");
        }

        EvenementConfirmePartie evenement = new EvenementConfirmePartie
        {
            definitionId = definition.id,
            rumeurId = rumeur.id,
            sourceId = source.id,
            categorie = definition.categorie,
            importance = definition.importance,
            titre = definition.titre,
            message = definition.message,
            moisConfirmation = mois,
            etat = EtatEvenementPartie.Confirme,
            consommeParMoteurImpacts = false,
            etatTraitementImpacts =
                EtatTraitementImpactsEvenement.EnAttente,
            diagnosticTraitementImpacts = string.Empty,
            impacts = CopierImpacts(definition.impacts)
        };
        donnees.evenementsConfirmes.Add(evenement);
        donnees.publications.Add(new PublicationActualite
        {
            id = "publication-" + donnees.prochaineSequencePublication,
            type = TypePublicationActualite.EvenementConfirme,
            objetId = rumeur.id,
            sourceId = source.id,
            sourceNom = source.nom,
            categorie = definition.categorie,
            importance = definition.importance,
            titre = definition.titre,
            texte = definition.message,
            moisPublication = mois,
            ordrePublication = donnees.prochaineSequencePublication++,
            etatRumeur = EtatRumeur.Confirmee
        });
    }

    private static List<ImpactDefinitionEvenement> CopierImpacts(
        List<ImpactDefinitionEvenement> impacts)
    {
        List<ImpactDefinitionEvenement> copies =
            new List<ImpactDefinitionEvenement>();
        if (impacts == null)
        {
            return copies;
        }

        foreach (ImpactDefinitionEvenement impact in impacts)
        {
            if (impact != null)
            {
                copies.Add(impact.Copier());
            }
        }

        return copies;
    }

    private static float BornerProbabilite(float probabilite)
    {
        return Math.Max(
            ParametresEvenements.ProbabiliteMinimale,
            Math.Min(
                ParametresEvenements.ProbabiliteMaximale,
                probabilite));
    }

    private void SauvegarderEtatAleatoire()
    {
        donnees.etatAleatoire = aleatoire.Etat;
    }
}
