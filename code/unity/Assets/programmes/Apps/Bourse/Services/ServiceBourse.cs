using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Execute les ordres, valorise le portefeuille et applique les evolutions
/// mensuelles du marche.
/// </summary>
public sealed class ServiceBourse : IEvolutionMensuelle
{
    public const int MontantMinimumOrdreCentimes = 1000;

    private readonly DonneesBourse donnees;

    /// <summary>
    /// Cree un service lie a un portefeuille persistant.
    /// </summary>
    public ServiceBourse(DonneesBourse donnees)
    {
        this.donnees = donnees ??
            throw new ArgumentNullException(nameof(donnees));
    }

    /// <summary>
    /// Achete pour un montant donne en debitant le compte courant.
    /// </summary>
    /// <remarks>
    /// Effets de bord : debit bancaire, creation ou augmentation de position,
    /// puis mise a jour de la valeur patrimoniale.
    /// </remarks>
    public ResultatOperation Acheter(
        DefinitionActifFinancier actif,
        int montantCentimes,
        int mois,
        CompteBanquaire compteCourant,
        ServiceBanque banque)
    {
        ResultatOperation validation = ValiderOrdre(
            actif,
            montantCentimes,
            mois);
        if (!validation.Succes)
        {
            return validation;
        }

        if (compteCourant == null || banque == null)
        {
            return ResultatOperation.Echec(
                "Le compte courant est indisponible.",
                "compte_absent");
        }

        ResultatOperation debit = banque.Debiter(
            compteCourant,
            new argent(montantCentimes),
            "Achat " + actif.Nom);
        if (!debit.Succes)
        {
            return debit;
        }

        int prixCentimes = ObtenirPrixCentimes(actif, mois);
        float quantiteAchetee =
            montantCentimes / (float)prixCentimes;
        donnees.ObtenirOuCreerPosition(actif.Id)
            .AjouterAchat(quantiteAchetee, montantCentimes);
        MettreAJourValorisation(mois);

        return ResultatOperation.Reussite(
            "Achat reussi : " +
            quantiteAchetee.ToString("N5") + " " + actif.Nom + ".",
            new argent(montantCentimes),
            "achat_effectue");
    }

    /// <summary>
    /// Vend une valeur cible exprimee en centimes.
    /// </summary>
    public ResultatOperation VendreMontant(
        DefinitionActifFinancier actif,
        int montantCentimes,
        int mois,
        CompteBanquaire compteCourant,
        ServiceBanque banque)
    {
        ResultatOperation validation = ValiderOrdre(
            actif,
            montantCentimes,
            mois);
        if (!validation.Succes)
        {
            return validation;
        }

        PositionBourse position = donnees.TrouverPosition(actif.Id);
        if (position == null || position.quantite <= 0f)
        {
            return ResultatOperation.Echec(
                "Aucune position ouverte sur cet actif.",
                "position_absente");
        }

        int prixCentimes = ObtenirPrixCentimes(actif, mois);
        int valeurPosition = CalculerValeurAvecPrixCentimes(
            position,
            prixCentimes);
        if (montantCentimes > valeurPosition)
        {
            return ResultatOperation.Echec(
                "La vente depasse la valeur de la position.",
                "position_insuffisante");
        }

        float quantiteDemandee =
            montantCentimes / (float)prixCentimes;
        return VendreInterne(
            actif,
            Math.Min(quantiteDemandee, position.quantite),
            mois,
            compteCourant,
            banque);
    }

    /// <summary>
    /// Vend une quantite, eventuellement fractionnaire, d'un actif detenu.
    /// </summary>
    public ResultatOperation VendreQuantite(
        DefinitionActifFinancier actif,
        float quantite,
        int mois,
        CompteBanquaire compteCourant,
        ServiceBanque banque)
    {
        if (quantite <= 0f)
        {
            return ResultatOperation.Echec(
                "La quantite doit etre strictement positive.",
                "quantite_invalide");
        }

        return VendreInterne(
            actif,
            quantite,
            mois,
            compteCourant,
            banque);
    }

    /// <summary>
    /// Liquide toute la position d'un actif.
    /// </summary>
    public ResultatOperation ToutVendre(
        DefinitionActifFinancier actif,
        int mois,
        CompteBanquaire compteCourant,
        ServiceBanque banque)
    {
        PositionBourse position =
            actif != null ? donnees.TrouverPosition(actif.Id) : null;
        if (position == null || position.quantite <= 0f)
        {
            return ResultatOperation.Echec(
                "Aucune position ouverte sur cet actif.",
                "position_absente");
        }

        return VendreInterne(
            actif,
            position.quantite,
            mois,
            compteCourant,
            banque);
    }

    /// <summary>
    /// Retourne le prix de marche d'un actif en euros.
    /// </summary>
    public float ObtenirPrix(
        DefinitionActifFinancier actif,
        int mois)
    {
        return actif == null
            ? 0f
            : MarcheBoursier.ObtenirPrix(
                actif.Id,
                Math.Max(0, mois),
                donnees);
    }

    /// <summary>
    /// Calcule la valeur actuelle d'une position en centimes.
    /// </summary>
    public int CalculerValeurPositionCentimes(
        PositionBourse position,
        int mois)
    {
        DefinitionActifFinancier actif =
            position != null
                ? CatalogueActifs.Trouver(position.actifId)
                : null;
        return CalculerValeurAvecPrixCentimes(
            position,
            ObtenirPrixCentimes(actif, mois));
    }

    /// <summary>
    /// Calcule les gains ou pertes latents d'une position en centimes.
    /// </summary>
    public int CalculerGainPerteCentimes(
        PositionBourse position,
        int mois)
    {
        return position == null
            ? 0
            : CalculerValeurPositionCentimes(position, mois) -
                position.coutTotalCentimes;
    }

    /// <summary>
    /// Calcule la performance latente en pourcentage.
    /// </summary>
    public float CalculerGainPertePourcent(
        PositionBourse position,
        int mois)
    {
        if (position == null || position.coutTotalCentimes <= 0)
        {
            return 0f;
        }

        return CalculerGainPerteCentimes(position, mois) *
            100f / position.coutTotalCentimes;
    }

    /// <summary>
    /// Calcule la variation entre le mois courant et un mois passe.
    /// </summary>
    public float CalculerVariation(
        DefinitionActifFinancier actif,
        int mois,
        int reculMois)
    {
        if (actif == null || actif.Prix.Count == 0)
        {
            return 0f;
        }

        int indexActuel = LimiterMois(actif, mois);
        int indexPasse = Math.Max(0, indexActuel - Math.Max(0, reculMois));
        float prixPasse = ObtenirPrix(actif, indexPasse);
        float prixActuel = ObtenirPrix(actif, indexActuel);
        return prixPasse > 0f
            ? ((prixActuel / prixPasse) - 1f) * 100f
            : 0f;
    }

    /// <summary>
    /// Calcule l'ecart-type des rendements mensuels sur douze mois maximum.
    /// </summary>
    public float CalculerVolatilite(
        DefinitionActifFinancier actif,
        int mois)
    {
        if (actif == null)
        {
            return 0f;
        }

        int indexActuel = LimiterMois(actif, mois);
        int debut = Math.Max(1, indexActuel - 11);
        int nombre = indexActuel - debut + 1;
        if (nombre <= 1)
        {
            return 0f;
        }

        List<float> rendements = new List<float>();
        for (int index = debut; index <= indexActuel; index++)
        {
            float precedent = ObtenirPrix(actif, index - 1);
            float courant = ObtenirPrix(actif, index);
            rendements.Add(
                precedent > 0f
                    ? ((courant / precedent) - 1f) * 100f
                    : 0f);
        }

        float moyenne = 0f;
        foreach (float rendement in rendements)
        {
            moyenne += rendement;
        }
        moyenne /= rendements.Count;

        float variance = 0f;
        foreach (float rendement in rendements)
        {
            float ecart = rendement - moyenne;
            variance += ecart * ecart;
        }

        return Mathf.Sqrt(variance / rendements.Count);
    }

    /// <summary>
    /// Calcule le rendement annualise depuis le debut de la serie.
    /// </summary>
    public float CalculerRendementAnnualise(
        DefinitionActifFinancier actif,
        int mois)
    {
        if (actif == null)
        {
            return 0f;
        }

        int indexActuel = LimiterMois(actif, mois);
        float prixInitial = ObtenirPrix(actif, 0);
        if (indexActuel < 1 || prixInitial <= 0f)
        {
            return 0f;
        }

        float annees = indexActuel / 12f;
        return (
            Mathf.Pow(
                ObtenirPrix(actif, indexActuel) / prixInitial,
                1f / annees) -
            1f) * 100f;
    }

    /// <summary>
    /// Met a jour la valeur patrimoniale cachee du portefeuille.
    /// </summary>
    public void MettreAJourValorisation(int mois)
    {
        MarcheBoursier.MettreAJourValorisation(
            donnees,
            Math.Max(0, mois));
    }

    /// <inheritdoc />
    public void AppliquerEvolutionMensuelle(int mois)
    {
        MettreAJourValorisation(mois);
        donnees.dernierMoisObserve = Math.Max(0, mois);
        donnees.dernierMessage = ConstruireMessageMarche(mois);
    }

    /// <summary>
    /// Enregistre un impact metier qui sera pris en compte dans les prix.
    /// </summary>
    public void AppliquerImpactEvenement(ImpactEvenementMarche impact)
    {
        MarcheBoursier.AppliquerImpactEvenement(donnees, impact);
    }

    /// <summary>
    /// Memorise le dernier retour affiche par l'application.
    /// </summary>
    public void EnregistrerMessage(string message)
    {
        donnees.dernierMessage = message ?? string.Empty;
    }

    private ResultatOperation VendreInterne(
        DefinitionActifFinancier actif,
        float quantiteDemandee,
        int mois,
        CompteBanquaire compteCourant,
        ServiceBanque banque)
    {
        if (actif == null)
        {
            return ResultatOperation.Echec(
                "Actif indisponible.",
                "actif_absent");
        }

        PositionBourse position = donnees.TrouverPosition(actif.Id);
        if (position == null || position.quantite <= 0f)
        {
            return ResultatOperation.Echec(
                "Aucune position ouverte sur cet actif.",
                "position_absente");
        }

        if (quantiteDemandee <= 0f ||
            quantiteDemandee > position.quantite + 0.000001f)
        {
            return ResultatOperation.Echec(
                "La quantite demandee depasse la position.",
                "position_insuffisante");
        }

        if (compteCourant == null || banque == null)
        {
            return ResultatOperation.Echec(
                "Le compte courant est indisponible.",
                "compte_absent");
        }

        int prixCentimes = ObtenirPrixCentimes(actif, mois);
        if (prixCentimes <= 0)
        {
            return ResultatOperation.Echec(
                "Le prix de cet actif est indisponible.",
                "prix_indisponible");
        }

        float quantiteVendue = Math.Min(
            quantiteDemandee,
            position.quantite);
        int produitVente = (int)Math.Round(
            quantiteVendue * prixCentimes);

        // Le cout cede doit etre calcule avant la mutation de la position.
        // PositionBourse conserve ainsi le cout moyen du reliquat.
        int coutCede = position.RetirerQuantite(quantiteVendue);
        ResultatOperation credit = banque.Crediter(
            compteCourant,
            new argent(produitVente),
            "Vente " + actif.Nom);
        if (!credit.Succes)
        {
            // Le credit ne peut echouer qu'avant toute mutation bancaire.
            // Restaurer la position garantit l'atomicite de l'ordre.
            position.AjouterAchat(quantiteVendue, coutCede);
            return credit;
        }

        donnees.SupprimerPositionsVides();
        MettreAJourValorisation(mois);
        int resultatRealise = produitVente - coutCede;
        return ResultatOperation.Reussite(
            "Vente reussie : resultat realise " +
            new argent(resultatRealise) + ".",
            new argent(produitVente),
            "vente_effectuee");
    }

    private ResultatOperation ValiderOrdre(
        DefinitionActifFinancier actif,
        int montantCentimes,
        int mois)
    {
        if (actif == null)
        {
            return ResultatOperation.Echec(
                "Actif indisponible.",
                "actif_absent");
        }

        if (montantCentimes < MontantMinimumOrdreCentimes)
        {
            return ResultatOperation.Echec(
                "Le montant est inferieur au minimum.",
                "montant_trop_faible");
        }

        if (ObtenirPrixCentimes(actif, mois) <= 0)
        {
            return ResultatOperation.Echec(
                "Le prix de cet actif est indisponible.",
                "prix_indisponible");
        }

        return ResultatOperation.Reussite("Ordre valide.");
    }

    private string ConstruireMessageMarche(int mois)
    {
        DefinitionActifFinancier mouvementPrincipal = null;
        float variationPrincipale = 0f;
        foreach (DefinitionActifFinancier actif in CatalogueActifs.ObtenirActifs())
        {
            float variation = CalculerVariation(actif, mois, 1);
            if (mouvementPrincipal == null ||
                Math.Abs(variation) > Math.Abs(variationPrincipale))
            {
                mouvementPrincipal = actif;
                variationPrincipale = variation;
            }
        }

        if (mouvementPrincipal == null)
        {
            return "Le marche attend de nouvelles donnees.";
        }

        return mouvementPrincipal.Nom + " : " +
            (variationPrincipale >= 0f ? "+" : string.Empty) +
            variationPrincipale.ToString("F1") + " % ce mois.";
    }

    private int ObtenirPrixCentimes(
        DefinitionActifFinancier actif,
        int mois)
    {
        if (actif == null)
        {
            return 0;
        }

        double centimes = Math.Round(
            ObtenirPrix(actif, mois) * 100d);
        if (centimes <= 0d)
        {
            return 0;
        }

        return centimes >= int.MaxValue
            ? int.MaxValue
            : (int)centimes;
    }

    private static int CalculerValeurAvecPrixCentimes(
        PositionBourse position,
        int prixCentimes)
    {
        if (position == null || prixCentimes <= 0)
        {
            return 0;
        }

        double valeur = Math.Round(position.quantite * prixCentimes);
        return valeur >= int.MaxValue
            ? int.MaxValue
            : Math.Max(0, (int)valeur);
    }

    private static int LimiterMois(
        DefinitionActifFinancier actif,
        int mois)
    {
        return actif == null || actif.Prix.Count == 0
            ? 0
            : Math.Min(Math.Max(0, mois), actif.Prix.Count - 1);
    }
}
