using System;

/// <summary>
/// Porte les regles metier du parcours salarie.
/// </summary>
/// <remarks>
/// Le service modifie uniquement les agregats de donnees. Les controleurs UI
/// restent responsables des panels, boutons et textes du prefab Salariat.
/// </remarks>
public sealed class ServiceSalariat : IEvolutionMensuelle
{
    public const int AugmentationNegociationCentimes = 60000;
    public const int VariationSalaireTempsTravailCentimes = 80000;

    private readonly DonneesSalariat donnees;
    private readonly DonneesJoueur joueur;

    /// <summary>
    /// Cree un service lie aux donnees salariees et au joueur.
    /// </summary>
    public ServiceSalariat(
        DonneesSalariat donnees,
        DonneesJoueur joueur)
    {
        this.donnees = donnees ??
            throw new ArgumentNullException(nameof(donnees));
        this.joueur = joueur ??
            throw new ArgumentNullException(nameof(joueur));
        this.donnees.InitialiserSiNecessaire();
    }

    /// <summary>
    /// Enregistre l'acceptation d'un poste et synchronise le salaire bancaire.
    /// </summary>
    /// <param name="entreprise">Nom affiche de l'entreprise.</param>
    /// <param name="salaireMensuelCentimes">Salaire mensuel en centimes.</param>
    /// <param name="heuresSemaine">Temps de travail hebdomadaire en heures.</param>
    /// <param name="stress">Niveau de stress de l'offre.</param>
    /// <param name="prestige">Niveau de prestige de l'offre.</param>
    /// <param name="equilibre">Niveau d'equilibre vie pro / perso.</param>
    public ResultatOperation AccepterPoste(
        string entreprise,
        int salaireMensuelCentimes,
        int heuresSemaine,
        int stress,
        int prestige,
        int equilibre)
    {
        if (salaireMensuelCentimes < 0)
        {
            return ResultatOperation.Echec(
                "Le salaire ne peut pas etre negatif.",
                "salaire_invalide");
        }

        donnees.aEmploi = true;
        donnees.entreprise = string.IsNullOrWhiteSpace(entreprise)
            ? "Entreprise inconnue"
            : entreprise;
        donnees.salaireMensuelCentimes = salaireMensuelCentimes;
        donnees.heuresSemaine = Math.Max(0, heuresSemaine);
        donnees.stressPoste = Math.Max(0, stress);
        donnees.ancienneteMois = 0;
        donnees.fatigue = 20;
        donnees.burnout = 0;
        donnees.satisfaction =
            CalculerSatisfaction(stress, prestige, equilibre);
        SynchroniserSalaireJoueur();
        return ResultatOperation.Reussite(
            "Poste accepte.",
            new argent(salaireMensuelCentimes),
            "poste_accepte");
    }

    /// <summary>
    /// Met a jour le stress et les heures du poste courant sans toucher au salaire.
    /// </summary>
    public void ActualiserContextePoste(int stress, int heuresSemaine)
    {
        donnees.aEmploi = true;
        donnees.stressPoste = Math.Max(0, stress);
        donnees.heuresSemaine = Math.Max(0, heuresSemaine);
        donnees.InitialiserSiNecessaire();
    }

    /// <summary>
    /// Termine le poste courant, remet le salaire a zero et conserve l'experience.
    /// </summary>
    public ResultatOperation Demissionner()
    {
        donnees.aEmploi = false;
        donnees.entreprise = "Aucune";
        donnees.salaireMensuelCentimes = 0;
        donnees.heuresSemaine = 0;
        donnees.stressPoste = 0;
        donnees.ancienneteMois = 0;
        donnees.fatigue = 20;
        donnees.burnout = 0;
        donnees.satisfaction = 0;
        SynchroniserSalaireJoueur();
        return ResultatOperation.Reussite(
            "Demission enregistree.",
            default,
            "demission");
    }

    /// <summary>
    /// Tente une negociation salariale a partir de l'experience.
    /// </summary>
    public ResultatOperation NegocierSalaire()
    {
        if (donnees.experience <= 70)
        {
            return ResultatOperation.Echec(
                "Experience insuffisante pour negocier.",
                "experience_insuffisante");
        }

        donnees.salaireMensuelCentimes +=
            AugmentationNegociationCentimes;
        SynchroniserSalaireJoueur();
        return ResultatOperation.Reussite(
            "Salaire augmente.",
            new argent(AugmentationNegociationCentimes),
            "negociation_reussie");
    }

    /// <summary>
    /// Modifie l'experience en points et borne la valeur entre 0 et 100.
    /// </summary>
    public void ModifierExperience(int delta)
    {
        donnees.experience = BornerScore(donnees.experience + delta);
    }

    /// <summary>
    /// Modifie la fatigue en points et borne la valeur entre 0 et 100.
    /// </summary>
    public void ModifierFatigue(int delta)
    {
        donnees.fatigue = BornerScore(donnees.fatigue + delta);
    }

    /// <summary>
    /// Modifie la satisfaction en points et borne la valeur entre 0 et 100.
    /// </summary>
    public void ModifierSatisfaction(int delta)
    {
        donnees.satisfaction = BornerScore(donnees.satisfaction + delta);
    }

    /// <summary>
    /// Modifie la relation avec le patron en points.
    /// </summary>
    public void ModifierRelationPatron(int delta)
    {
        donnees.relationPatron =
            BornerScore(donnees.relationPatron + delta);
    }

    /// <summary>
    /// Modifie la relation avec les collegues en points.
    /// </summary>
    public void ModifierRelationCollegues(int delta)
    {
        donnees.relationCollegues =
            BornerScore(donnees.relationCollegues + delta);
    }

    /// <summary>
    /// Ajuste les heures, le salaire et la satisfaction du poste courant.
    /// </summary>
    public ResultatOperation ModifierTempsTravail(
        int deltaHeures,
        int deltaSalaireCentimes,
        int deltaSatisfaction)
    {
        if (!donnees.aEmploi)
        {
            return ResultatOperation.Echec(
                "Aucun poste actif.",
                "poste_absent");
        }

        int nouvellesHeures =
            Math.Max(35, donnees.heuresSemaine + deltaHeures);
        int nouveauSalaire =
            Math.Max(0, donnees.salaireMensuelCentimes +
                deltaSalaireCentimes);

        donnees.heuresSemaine = nouvellesHeures;
        donnees.salaireMensuelCentimes = nouveauSalaire;
        ModifierSatisfaction(deltaSatisfaction);
        SynchroniserSalaireJoueur();
        return ResultatOperation.Reussite(
            "Temps de travail ajuste.",
            new argent(deltaSalaireCentimes),
            "temps_travail_ajuste");
    }

    /// <inheritdoc />
    public void AppliquerEvolutionMensuelle(int mois)
    {
        if (!donnees.aEmploi)
        {
            return;
        }

        donnees.ancienneteMois++;

        if (donnees.fatigue >= 100)
        {
            donnees.burnout += 15;
        }

        if (donnees.relationCollegues >= 100)
        {
            donnees.fatigue -= 5;
        }

        // Le rythme de progression original etait quinquennal. Le service
        // conserve ce gameplay, mais le rend independant de l'ouverture de l'UI.
        if (donnees.ancienneteMois % 5 == 0)
        {
            if (donnees.heuresSemaine < 40)
            {
                donnees.experience += 10;
            }
            else if (donnees.heuresSemaine == 40)
            {
                donnees.fatigue += 10;
                donnees.experience += 15;
            }
            else if (donnees.heuresSemaine >= 45)
            {
                donnees.fatigue += 20;
                donnees.experience += 20;
            }
        }

        donnees.InitialiserSiNecessaire();
    }

    /// <summary>
    /// Calcule la satisfaction d'un poste depuis les etoiles de l'offre.
    /// </summary>
    public static int CalculerSatisfaction(
        int stress,
        int prestige,
        int equilibre)
    {
        return BornerScore(
            50 + (prestige * 15) + (equilibre * 15) -
            (stress * 20));
    }

    private void SynchroniserSalaireJoueur()
    {
        joueur.salaire = new argent(donnees.salaireMensuelCentimes);
    }

    private static int BornerScore(int valeur)
    {
        return Math.Max(0, Math.Min(100, valeur));
    }
}
