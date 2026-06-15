using System;
using System.Collections.Generic;

/// <summary>
/// Position detenue sur un actif financier.
/// </summary>
[Serializable]
public class PositionBourse
{
    /// <summary>
    /// Identifiant stable du catalogue.
    /// </summary>
    public string actifId;

    /// <summary>
    /// Quantite detenue. Les fractions sont autorisees.
    /// </summary>
    public float quantite;

    /// <summary>
    /// Capital restant investi dans la position, en centimes.
    /// </summary>
    public int coutTotalCentimes;

    public PositionBourse(string actifId)
    {
        this.actifId = actifId;
    }

    /// <summary>
    /// Ajoute une quantite achetee et son cout d'acquisition.
    /// </summary>
    public void AjouterAchat(float quantiteAchetee, int coutCentimes)
    {
        if (quantiteAchetee <= 0f || coutCentimes <= 0)
        {
            return;
        }

        quantite += quantiteAchetee;
        coutTotalCentimes += coutCentimes;
    }

    /// <summary>
    /// Retire une quantite et retourne le cout historique correspondant.
    /// </summary>
    /// <remarks>
    /// Pour une vente partielle, le cout cede est calcule au prorata de la
    /// quantite. Cette methode conserve ainsi le cout moyen de la quantite
    /// restante sans le recalculer a partir du prix de marche.
    /// </remarks>
    public int RetirerQuantite(float quantiteVendue)
    {
        if (quantiteVendue <= 0f || quantite <= 0f)
        {
            return 0;
        }

        float quantiteAvant = quantite;
        bool venteTotale = quantiteVendue >= quantiteAvant - 0.000001f;
        float quantiteEffective = venteTotale
            ? quantiteAvant
            : quantiteVendue;
        int coutCede = venteTotale
            ? coutTotalCentimes
            : (int)Math.Round(
                coutTotalCentimes *
                (quantiteEffective / quantiteAvant));

        quantite = Math.Max(0f, quantiteAvant - quantiteEffective);
        coutTotalCentimes = Math.Max(0, coutTotalCentimes - coutCede);
        return coutCede;
    }

    /// <summary>
    /// Retourne le cout moyen d'une unite en centimes.
    /// </summary>
    public float CalculerCoutMoyenCentimes()
    {
        return quantite > 0f
            ? coutTotalCentimes / quantite
            : 0f;
    }

    /// <summary>
    /// Produit une copie profonde pour un snapshot.
    /// </summary>
    public PositionBourse Copier()
    {
        return new PositionBourse(actifId)
        {
            quantite = quantite,
            coutTotalCentimes = coutTotalCentimes
        };
    }
}

/// <summary>
/// Impact economique temporaire ou permanent applique a un actif.
/// </summary>
[Serializable]
public class ImpactEvenementMarche
{
    public string evenementId;
    public string actifId;
    public int moisDebut;
    public int dureeMois = -1;
    public float coefficientPrix = 1f;
    public float tendanceMensuellePourcent;
    public float coefficientVolatilite = 1f;

    /// <summary>
    /// Indique si l'impact s'applique au mois absolu fourni.
    /// </summary>
    public bool EstActif(int mois)
    {
        return mois >= moisDebut &&
            (dureeMois < 0 || mois < moisDebut + dureeMois);
    }

    /// <summary>
    /// Produit une copie profonde pour un snapshot ou une simulation.
    /// </summary>
    public ImpactEvenementMarche Copier()
    {
        return new ImpactEvenementMarche
        {
            evenementId = evenementId,
            actifId = actifId,
            moisDebut = moisDebut,
            dureeMois = dureeMois,
            coefficientPrix = coefficientPrix,
            tendanceMensuellePourcent = tendanceMensuellePourcent,
            coefficientVolatilite = coefficientVolatilite
        };
    }
}

/// <summary>
/// Agregat persistant du portefeuille boursier du joueur.
/// </summary>
[Serializable]
public class DonneesBourse : IPatrimoine
{
    public List<PositionBourse> positions = new List<PositionBourse>();
    public List<ImpactEvenementMarche> impactsMarche =
        new List<ImpactEvenementMarche>();
    public int dernierMoisObserve = -1;
    public string dernierMessage = "Selectionnez un actif pour commencer.";
    public int valeurMarcheCentimes;
    public int moisValorisation = -1;

    /// <summary>
    /// Recherche une position par identifiant d'actif.
    /// </summary>
    public PositionBourse TrouverPosition(string actifId)
    {
        AssurerCollections();
        return positions.Find(
            position =>
                position != null &&
                position.actifId == actifId);
    }

    /// <summary>
    /// Retourne la position existante ou en cree une vide.
    /// </summary>
    public PositionBourse ObtenirOuCreerPosition(string actifId)
    {
        PositionBourse position = TrouverPosition(actifId);
        if (position != null)
        {
            return position;
        }

        position = new PositionBourse(actifId);
        positions.Add(position);
        return position;
    }

    /// <summary>
    /// Retire les positions sans quantite significative.
    /// </summary>
    public void SupprimerPositionsVides()
    {
        AssurerCollections();
        positions.RemoveAll(
            position =>
                position == null ||
                position.quantite <= 0.000001f);
    }

    /// <summary>
    /// Memorise la valeur de marche calculee pour un mois.
    /// </summary>
    public void DefinirValeurMarche(int valeurCentimes, int mois)
    {
        valeurMarcheCentimes = Math.Max(0, valeurCentimes);
        moisValorisation = Math.Max(0, mois);
    }

    /// <summary>
    /// Calcule le capital historique encore investi, en centimes.
    /// </summary>
    public int CalculerCapitalInvestiCentimes()
    {
        AssurerCollections();
        long total = 0;
        foreach (PositionBourse position in positions)
        {
            if (position != null && position.quantite > 0f)
            {
                total += Math.Max(0, position.coutTotalCentimes);
            }
        }

        return total > int.MaxValue ? int.MaxValue : (int)total;
    }

    /// <inheritdoc />
    public argent GetValeurPatrimoine()
    {
        return new argent(valeurMarcheCentimes);
    }

    /// <summary>
    /// Retourne les gains ou pertes non realises en centimes.
    /// </summary>
    public argent GetGainsPertesLatents()
    {
        return new argent(
            valeurMarcheCentimes -
            CalculerCapitalInvestiCentimes());
    }

    /// <summary>
    /// Produit une copie profonde des positions, impacts et caches de valeur.
    /// </summary>
    public DonneesBourse Copier()
    {
        DonneesBourse copie = new DonneesBourse
        {
            dernierMoisObserve = dernierMoisObserve,
            dernierMessage = dernierMessage,
            valeurMarcheCentimes = valeurMarcheCentimes,
            moisValorisation = moisValorisation,
            positions = new List<PositionBourse>(),
            impactsMarche = new List<ImpactEvenementMarche>()
        };

        AssurerCollections();
        foreach (PositionBourse position in positions)
        {
            if (position != null)
            {
                copie.positions.Add(position.Copier());
            }
        }

        foreach (ImpactEvenementMarche impact in impactsMarche)
        {
            if (impact != null)
            {
                copie.impactsMarche.Add(impact.Copier());
            }
        }

        return copie;
    }

    private void AssurerCollections()
    {
        if (positions == null)
        {
            positions = new List<PositionBourse>();
        }

        if (impactsMarche == null)
        {
            impactsMarche = new List<ImpactEvenementMarche>();
        }
    }
}
