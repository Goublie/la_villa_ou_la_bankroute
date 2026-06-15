using System;

/// <summary>
/// Represente le Livret A du joueur : un compte transferable associe a un
/// placement a rendement fixe.
/// </summary>
/// <remarks>
/// Le compte est l'unique source de valeur patrimoniale. L'objet
/// <see cref="invest"/> sert uniquement au calcul des interets et ne doit pas
/// etre ajoute une seconde fois a la liste des investissements du joueur.
/// </remarks>
[Serializable]
public class Epargne : CompteBanquaire, IEvolutionMensuelle
{
    /// <summary>
    /// Moteur de calcul des interets du Livret A.
    /// </summary>
    public Investissement invest;

    /// <summary>
    /// Cree un Livret A vide.
    /// </summary>
    public Epargne(float tauxParDefaut, int dureeMois)
        : base()
    {
        InitialiserInvestissement(
            new Investissement(solde, tauxParDefaut, dureeMois));
    }

    /// <summary>
    /// Constructeur conserve pour compatibilite avec les anciennes UI.
    /// </summary>
    public Epargne(GameData gameData, float tauxParDefaut, int dureeMois)
        : this(
            ServiceLivretA.ObtenirTauxAnnuel(
                gameData != null ? gameData.nombreMoisPasses : 0,
                tauxParDefaut),
            dureeMois)
    {
    }

    /// <summary>
    /// Reconstruit un Livret A depuis un historique.
    /// </summary>
    public Epargne(
        Historique historique,
        GameData gameData,
        float tauxParDefaut,
        int dureeMois)
        : base(historique)
    {
        float tauxInitial = ServiceLivretA.ObtenirTauxAnnuel(
            gameData != null ? gameData.nombreMoisPasses : 0,
            tauxParDefaut);
        InitialiserInvestissement(
            new Investissement(solde, tauxInitial, dureeMois));
    }

    private Epargne(
        Historique historique,
        argent totalEntree,
        argent totalSortie,
        argent solde,
        argent soldeFinMois,
        Investissement investissement)
        : base(
            historique,
            totalEntree,
            totalSortie,
            solde,
            soldeFinMois)
    {
        InitialiserInvestissement(investissement);
    }

    /// <summary>
    /// Ajoute un mouvement au Livret A et maintient son capital remunere
    /// synchronise avec le solde du compte.
    /// </summary>
    public override void AjoutHistorique(string source, argent montant)
    {
        base.AjoutHistorique(source, montant);
        invest.sommeInvestie += montant;
    }

    /// <summary>
    /// Applique le taux du mois et capitalise les interets en decembre.
    /// </summary>
    /// <param name="mois">
    /// Index absolu du mois ecoule depuis juillet 2026.
    /// </param>
    public void AppliquerEvolutionMensuelle(int mois)
    {
        invest.DefinirTaux(
            ServiceLivretA.ObtenirTauxAnnuel(mois, invest.taux));
        invest.AccumulerBeneficesMensuels();

        if (ServiceLivretA.ObtenirMoisCalendrier(mois) == Mois.Decembre)
        {
            invest.VerserBeneficesAccumules();
        }
    }

    /// <summary>
    /// Retourne le taux annuel courant, ou 0.0175 pour 1,75 %.
    /// </summary>
    public float GetTaux()
    {
        return invest.taux;
    }

    /// <summary>
    /// Produit une copie profonde du compte, de son historique et des interets
    /// encore non capitalises.
    /// </summary>
    public override CompteBanquaire Copier()
    {
        return new Epargne(
            historique.Copier(),
            new argent(GetTotalEntree().centimes),
            new argent(GetTotalSortie().centimes),
            new argent(solde.centimes),
            new argent(GetSoldeFinMois().centimes),
            invest.Copier());
    }

    private void InitialiserInvestissement(Investissement investissement)
    {
        invest = investissement ??
            new Investissement(new argent(0), 0.0175f, 12);
        invest.OnBeneficesVerses += EnregistrerInterets;
    }

    private void EnregistrerInterets(argent interets)
    {
        // Le placement a deja augmente son capital. Appeler base evite de
        // l'augmenter une seconde fois via la surcharge AjoutHistorique.
        base.AjoutHistorique("interets", interets);
    }
}
