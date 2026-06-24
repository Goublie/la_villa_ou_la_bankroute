using System;

/// <summary>
/// Porte les regles metier du parcours salarie.
/// </summary>
public sealed class ServiceSalariat : IEvolutionMensuelle
{
    // ◄ MODIFICATION : L'augmentation passe de 60000 (600€) à 20000 (200€) centimes
    public const int AugmentationNegociationCentimes = 20000;
    public const int VariationSalaireTempsTravailCentimes = 80000;

    private readonly DonneesSalariat donnees;
    private readonly DonneesJoueur joueur;

    public ServiceSalariat(DonneesSalariat donnees, DonneesJoueur joueur)
    {
        this.donnees = donnees ?? throw new ArgumentNullException(nameof(donnees));
        this.joueur = joueur ?? throw new ArgumentNullException(nameof(joueur));
        this.donnees.InitialiserSiNecessaire();
    }

    public ResultatOperation AccepterPoste(string entreprise, int salaireMensuelCentimes, int heuresSemaine, int stress, int prestige, int equilibre)
    {
        if (salaireMensuelCentimes < 0)
        {
            return ResultatOperation.Echec("Le salaire ne peut pas etre negatif.", "salaire_invalide");
        }

        donnees.aEmploi = true;
        donnees.entreprise = string.IsNullOrWhiteSpace(entreprise) ? "Entreprise inconnue" : entreprise;
        donnees.salaireMensuelCentimes = salaireMensuelCentimes;
        donnees.heuresSemaine = Math.Max(0, heuresSemaine);
        donnees.stressPoste = Math.Max(0, stress);
        donnees.ancienneteMois = 0;
        donnees.fatigue = 20;
        donnees.satisfaction = CalculerSatisfaction(stress, prestige, equilibre);
        SynchroniserSalaireJoueur();
        return ResultatOperation.Reussite("Poste accepte.", new argent(salaireMensuelCentimes), "poste_accepte");
    }

    public void ActualiserContextePoste(int stress, int heuresSemaine)
    {
        donnees.aEmploi = true;
        donnees.stressPoste = Math.Max(0, stress);
        donnees.heuresSemaine = Math.Max(0, heuresSemaine);
        donnees.InitialiserSiNecessaire();
    }

    public ResultatOperation Demissionner()
    {
        donnees.aEmploi = false;
        donnees.entreprise = "Aucune";
        donnees.salaireMensuelCentimes = 0;
        donnees.heuresSemaine = 0;
        donnees.stressPoste = 0;
        donnees.ancienneteMois = 0;
        donnees.fatigue = 20;
        donnees.satisfaction = 0;
        SynchroniserSalaireJoueur();
        return ResultatOperation.Reussite("Demission enregistree.", default, "demission");
    }

    /// <summary>
    /// Tente une negociation salariale a partir de l'experience.
    /// </summary>
    public ResultatOperation NegocierSalaire()
    {
        if (donnees.experience <= 70)
        {
            return ResultatOperation.Echec("Experience insuffisante pour negocier.", "experience_insuffisante");
        }

        donnees.salaireMensuelCentimes += AugmentationNegociationCentimes;

        // ◄ MODIFICATION : La barre d'expérience retourne à 0 après la négociation
        donnees.experience = 0;

        SynchroniserSalaireJoueur();
        return ResultatOperation.Reussite("Salaire augmente.", new argent(AugmentationNegociationCentimes), "negociation_reussie");
    }

    public void ModifierExperience(int delta) { donnees.experience = BornerScore(donnees.experience + delta); }
    public void ModifierFatigue(int delta) { donnees.fatigue = BornerScore(donnees.fatigue + delta); }
    public void ModifierSatisfaction(int delta) { donnees.satisfaction = BornerScore(donnees.satisfaction + delta); }
    public void ModifierRelationPatron(int delta) { donnees.relationPatron = BornerScore(donnees.relationPatron + delta); }
    public void ModifierRelationCollegues(int delta) { donnees.relationCollegues = BornerScore(donnees.relationCollegues + delta); }

    public ResultatOperation ModifierTempsTravail(int deltaHeures, int deltaSalaireCentimes, int deltaSatisfaction)
    {
        if (!donnees.aEmploi) return ResultatOperation.Echec("Aucun poste actif.", "poste_absent");

        int nouvellesHeures = Math.Max(35, donnees.heuresSemaine + deltaHeures);
        int nouveauSalaire = Math.Max(0, donnees.salaireMensuelCentimes + deltaSalaireCentimes);

        donnees.heuresSemaine = nouvellesHeures;
        donnees.salaireMensuelCentimes = nouveauSalaire;
        ModifierSatisfaction(deltaSatisfaction);
        SynchroniserSalaireJoueur();
        return ResultatOperation.Reussite("Temps de travail ajuste.", new argent(deltaSalaireCentimes), "temps_travail_ajuste");
    }

    public void AppliquerEvolutionMensuelle(int mois)
    {
        if (!donnees.aEmploi)
        {
            joueur.santeMentale = BornerScore(joueur.santeMentale + 10);
            return;
        }

        donnees.ancienneteMois++;

        if (donnees.fatigue >= 40)
        {
            joueur.santeMentale = BornerScore(joueur.santeMentale - 5);
        }

        if (donnees.satisfaction <= 50)
        {
            int perte = UnityEngine.Mathf.RoundToInt(40f * UnityEngine.Mathf.Exp(-0.073777f * donnees.satisfaction));
            joueur.santeMentale = BornerScore(joueur.santeMentale - perte);
        }
        else if (donnees.satisfaction >= 70)
        {
            joueur.santeMentale = BornerScore(joueur.santeMentale + 5);
        }

        if (donnees.relationCollegues >= 100) donnees.fatigue = BornerScore(donnees.fatigue - 5);

        donnees.experience = BornerScore(donnees.experience + 5);

        if (donnees.heuresSemaine >= 45) donnees.fatigue = BornerScore(donnees.fatigue + 10);
        else if (donnees.heuresSemaine >= 40) donnees.fatigue = BornerScore(donnees.fatigue + 5);

        donnees.InitialiserSiNecessaire();
    }

    public static int CalculerSatisfaction(int stress, int prestige, int equilibre)
    {
        return BornerScore(50 + (prestige * 15) + (equilibre * 15) - (stress * 20));
    }

    private void SynchroniserSalaireJoueur() { joueur.salaire = new argent(donnees.salaireMensuelCentimes); }
    private static int BornerScore(int valeur) { return Math.Max(0, Math.Min(100, valeur)); }
}