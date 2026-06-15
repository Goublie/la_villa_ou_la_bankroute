using System.Collections.Generic;

/// <summary>
/// Centralise la creation des comptes et les transferts bancaires.
/// </summary>
public sealed class ServiceBanque
{
    public const string CompteCourantId = "courant";
    public const string LivretAId = "epargne";
    public const int SoldeInitialCourantCentimes = 100000;
    public const float TauxInitialLivretA = 0.0175f;

    private readonly DonneesJoueur joueur;

    /// <summary>
    /// Cree un service lie a l'agregat du joueur.
    /// </summary>
    public ServiceBanque(DonneesJoueur joueur)
    {
        this.joueur = joueur;
        AssurerComptes();
    }

    /// <summary>
    /// Retourne le compte courant unique, en le creant si necessaire.
    /// </summary>
    public CompteBanquaire ObtenirCompteCourant()
    {
        AssurerComptes();
        if (!joueur.comptes.TryGetValue(
            CompteCourantId,
            out CompteBanquaire compte) ||
            compte == null)
        {
            compte = new CompteBanquaire(
                new argent(SoldeInitialCourantCentimes));
            joueur.comptes[CompteCourantId] = compte;
        }

        return compte;
    }

    /// <summary>
    /// Retourne le Livret A unique, en le creant si necessaire.
    /// </summary>
    public Epargne ObtenirLivretA(int mois = 0)
    {
        AssurerComptes();
        if (joueur.comptes.TryGetValue(
                LivretAId,
                out CompteBanquaire compte) &&
            compte is Epargne epargne)
        {
            RetirerDoubleComptage(epargne);
            return epargne;
        }

        Epargne nouveauLivret = new Epargne(
            ServiceLivretA.ObtenirTauxAnnuel(
                mois,
                TauxInitialLivretA),
            12);
        joueur.comptes[LivretAId] = nouveauLivret;
        RetirerDoubleComptage(nouveauLivret);
        return nouveauLivret;
    }

    /// <summary>
    /// Transfere un montant entre deux comptes du joueur.
    /// </summary>
    /// <remarks>
    /// Effet de bord : les deux soldes et historiques sont modifies uniquement
    /// lorsque le resultat est un succes.
    /// </remarks>
    public ResultatOperation Transferer(
        CompteBanquaire source,
        CompteBanquaire destination,
        argent montant,
        string libelleSource,
        string libelleDestination)
    {
        if (source == null || destination == null)
        {
            return ResultatOperation.Echec(
                "Compte bancaire introuvable.",
                "compte_absent");
        }

        if (montant.centimes <= 0)
        {
            return ResultatOperation.Echec(
                "Le montant doit etre strictement positif.",
                "montant_invalide");
        }

        if (source.GetSolde() < montant)
        {
            return ResultatOperation.Echec(
                "Fonds insuffisants.",
                "fonds_insuffisants");
        }

        source.AjoutHistorique(libelleSource, -montant);
        destination.AjoutHistorique(libelleDestination, montant);
        return ResultatOperation.Reussite(
            "Transfert effectue.",
            montant,
            "transfert_effectue");
    }

    private void AssurerComptes()
    {
        if (joueur == null)
        {
            return;
        }

        if (joueur.comptes == null)
        {
            joueur.comptes =
                new Dictionary<string, CompteBanquaire>();
        }
    }

    private void RetirerDoubleComptage(Epargne epargne)
    {
        if (joueur.investissements == null || epargne == null)
        {
            return;
        }

        joueur.investissements.RemoveAll(
            investissement =>
                ReferenceEquals(investissement, epargne.invest));
    }
}
