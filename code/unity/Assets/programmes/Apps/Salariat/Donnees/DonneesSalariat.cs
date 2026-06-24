using System;

/// <summary>
/// Agregat persistant du parcours salarie du joueur.
/// </summary>
/// <remarks>
/// Les valeurs de performance sont exprimees en points de 0 a 100. Le salaire
/// mensuel reste aussi duplique dans <see cref="DonneesJoueur.salaire"/> pour
/// alimenter la Banque ; ce service maintient les deux valeurs synchronisees.
/// </remarks>
[Serializable]
public class DonneesSalariat
{
    public bool aEmploi;
    public string entreprise = "Aucune";
    public int salaireMensuelCentimes;
    public int heuresSemaine;
    public int stressPoste;
    public int ancienneteMois;
    public int experience;
    public int fatigue = 20;
    public int relationPatron;
    public int relationCollegues;
    public int satisfaction;

    /// <summary>
    /// Repare les valeurs absentes ou hors bornes apres une migration.
    /// </summary>
    public void InitialiserSiNecessaire()
    {
        if (string.IsNullOrWhiteSpace(entreprise))
        {
            entreprise = "Aucune";
        }

        experience = BornerScore(experience);
        fatigue = BornerScore(fatigue);
        relationPatron = BornerScore(relationPatron);
        relationCollegues = BornerScore(relationCollegues);
        satisfaction = BornerScore(satisfaction);
        salaireMensuelCentimes = Math.Max(0, salaireMensuelCentimes);
        heuresSemaine = Math.Max(0, heuresSemaine);
        stressPoste = Math.Max(0, stressPoste);
        ancienneteMois = Math.Max(0, ancienneteMois);
    }

    /// <summary>
    /// Produit une copie profonde pour les snapshots et simulations What If.
    /// </summary>
    public DonneesSalariat Copier()
    {
        InitialiserSiNecessaire();
        return new DonneesSalariat
        {
            aEmploi = aEmploi,
            entreprise = entreprise,
            salaireMensuelCentimes = salaireMensuelCentimes,
            heuresSemaine = heuresSemaine,
            stressPoste = stressPoste,
            ancienneteMois = ancienneteMois,
            experience = experience,
            fatigue = fatigue,
            relationPatron = relationPatron,
            relationCollegues = relationCollegues,
            satisfaction = satisfaction
        };
    }

    private static int BornerScore(int valeur)
    {
        return Math.Max(0, Math.Min(100, valeur));
    }
}
