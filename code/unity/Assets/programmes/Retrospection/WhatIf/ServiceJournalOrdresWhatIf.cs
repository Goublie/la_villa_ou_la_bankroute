using System;
using System.Collections.Generic;

/// <summary>
/// Centralise l'enregistrement et la lecture des ordres du portefeuille What If.
/// </summary>
public static class ServiceJournalOrdresWhatIf
{
    private const float EpsilonQuantite = 0.000001f;
    private const string RaisonReallocation = "Reallocation mensuelle";

    public static void EnregistrerReallocation(
        DonneesWhatIf donnees,
        IReadOnlyList<PositionBourse> anciennesPositions,
        IReadOnlyList<PositionBourse> nouvellesPositions,
        IReadOnlyDictionary<string, int> prixCentimesParActif,
        int indexMois,
        int coutTransactionCentimes)
    {
        if (donnees == null)
        {
            return;
        }

        donnees.InitialiserSiNecessaire();

        int mois = Math.Max(0, indexMois);
        donnees.ordres.RemoveAll(
            ordre =>
                ordre != null &&
                ordre.indexMois == mois &&
                ordre.raison == RaisonReallocation);

        Dictionary<string, float> anciennes =
            AgregerQuantites(anciennesPositions);
        Dictionary<string, float> nouvelles =
            AgregerQuantites(nouvellesPositions);

        HashSet<string> actifs =
            new HashSet<string>(StringComparer.Ordinal);

        foreach (string actifId in anciennes.Keys)
        {
            actifs.Add(actifId);
        }

        foreach (string actifId in nouvelles.Keys)
        {
            actifs.Add(actifId);
        }

        List<string> ids = new List<string>(actifs);
        ids.Sort(StringComparer.Ordinal);

        List<OrdreHistoriqueWhatIf> ordres =
            new List<OrdreHistoriqueWhatIf>();

        foreach (string actifId in ids)
        {
            float ancienneQuantite =
                ObtenirQuantite(anciennes, actifId);
            float nouvelleQuantite =
                ObtenirQuantite(nouvelles, actifId);
            float difference =
                nouvelleQuantite - ancienneQuantite;

            if (Math.Abs(difference) <= EpsilonQuantite)
            {
                continue;
            }

            int prixCentimes =
                ObtenirPrix(prixCentimesParActif, actifId);
            if (prixCentimes <= 0)
            {
                continue;
            }

            float quantite = Math.Abs(difference);
            int montant = LimiterEnEntier(
                Math.Round(quantite * prixCentimes));

            if (montant <= 0)
            {
                continue;
            }

            ordres.Add(
                new OrdreHistoriqueWhatIf
                {
                    indexMois = mois,
                    type = difference > 0f
                        ? TypeOrdreHistoriqueWhatIf.Achat
                        : TypeOrdreHistoriqueWhatIf.Vente,
                    actifId = actifId,
                    quantite = quantite,
                    prixUnitaireCentimes = prixCentimes,
                    montantCentimes = montant,
                    raison = RaisonReallocation
                });
        }

        RepartirCoutTransaction(
            ordres,
            Math.Max(0, coutTransactionCentimes));

        foreach (OrdreHistoriqueWhatIf ordre in ordres)
        {
            donnees.ordres.Add(ordre);
        }

        Trier(donnees.ordres);
    }

    public static void EnregistrerVenteForcee(
        DonneesWhatIf donnees,
        int indexMois,
        string actifId,
        float quantite,
        int prixUnitaireCentimes,
        int montantCentimes,
        string raison)
    {
        if (donnees == null ||
            string.IsNullOrWhiteSpace(actifId) ||
            quantite <= EpsilonQuantite ||
            prixUnitaireCentimes <= 0 ||
            montantCentimes <= 0)
        {
            return;
        }

        donnees.InitialiserSiNecessaire();
        donnees.ordres.Add(
            new OrdreHistoriqueWhatIf
            {
                indexMois = Math.Max(0, indexMois),
                type = TypeOrdreHistoriqueWhatIf.VenteForcee,
                actifId = actifId,
                quantite = quantite,
                prixUnitaireCentimes =
                    Math.Max(0, prixUnitaireCentimes),
                montantCentimes = Math.Max(0, montantCentimes),
                coutTransactionCentimes = 0,
                raison = string.IsNullOrWhiteSpace(raison)
                    ? "Couverture des depenses externes"
                    : raison.Trim()
            });

        Trier(donnees.ordres);
    }

    public static List<OrdreHistoriqueWhatIf>
        ObtenirOrdresDouzeDerniersMois(
            DonneesWhatIf donnees,
            int moisCourant)
    {
        int moisMaximum = Math.Max(0, moisCourant);
        int moisMinimum = Math.Max(0, moisMaximum - 11);

        return ObtenirOrdres(
            donnees,
            moisMinimum,
            moisMaximum);
    }

    public static List<OrdreHistoriqueWhatIf> ObtenirOrdres(
        DonneesWhatIf donnees,
        int moisMinimum,
        int moisMaximum)
    {
        List<OrdreHistoriqueWhatIf> resultat =
            new List<OrdreHistoriqueWhatIf>();

        if (donnees?.ordres == null)
        {
            return resultat;
        }

        int minimum = Math.Max(0, moisMinimum);
        int maximum = Math.Max(minimum, moisMaximum);

        foreach (OrdreHistoriqueWhatIf ordre in donnees.ordres)
        {
            if (ordre == null ||
                ordre.indexMois < minimum ||
                ordre.indexMois > maximum)
            {
                continue;
            }

            resultat.Add(ordre.Copier());
        }

        Trier(resultat);
        return resultat;
    }

    private static Dictionary<string, float> AgregerQuantites(
        IReadOnlyList<PositionBourse> positions)
    {
        Dictionary<string, float> resultat =
            new Dictionary<string, float>(StringComparer.Ordinal);

        if (positions == null)
        {
            return resultat;
        }

        foreach (PositionBourse position in positions)
        {
            if (position == null ||
                string.IsNullOrWhiteSpace(position.actifId) ||
                position.quantite <= EpsilonQuantite)
            {
                continue;
            }

            float ancienne = resultat.TryGetValue(
                position.actifId,
                out float valeur)
                ? valeur
                : 0f;

            resultat[position.actifId] =
                ancienne + position.quantite;
        }

        return resultat;
    }

    private static float ObtenirQuantite(
        IReadOnlyDictionary<string, float> quantites,
        string actifId)
    {
        return quantites != null &&
            quantites.TryGetValue(actifId, out float valeur)
            ? Math.Max(0f, valeur)
            : 0f;
    }

    private static int ObtenirPrix(
        IReadOnlyDictionary<string, int> prix,
        string actifId)
    {
        return prix != null &&
            prix.TryGetValue(actifId, out int valeur)
            ? Math.Max(0, valeur)
            : 0;
    }

    private static void RepartirCoutTransaction(
        List<OrdreHistoriqueWhatIf> ordres,
        int coutTotal)
    {
        if (ordres == null ||
            ordres.Count == 0 ||
            coutTotal <= 0)
        {
            return;
        }

        long montantTotal = 0;
        foreach (OrdreHistoriqueWhatIf ordre in ordres)
        {
            montantTotal += Math.Max(0, ordre.montantCentimes);
        }

        if (montantTotal <= 0)
        {
            return;
        }

        int distribue = 0;
        for (int index = 0; index < ordres.Count; index++)
        {
            int cout = index == ordres.Count - 1
                ? coutTotal - distribue
                : LimiterEnEntier(
                    Math.Floor(
                        coutTotal *
                        (ordres[index].montantCentimes /
                         (double)montantTotal)));

            cout = Math.Max(0, cout);
            ordres[index].coutTransactionCentimes = cout;
            distribue += cout;
        }
    }

    private static void Trier(List<OrdreHistoriqueWhatIf> ordres)
    {
        if (ordres == null)
        {
            return;
        }

        ordres.Sort(
            (gauche, droite) =>
            {
                if (ReferenceEquals(gauche, droite))
                {
                    return 0;
                }

                if (gauche == null)
                {
                    return 1;
                }

                if (droite == null)
                {
                    return -1;
                }

                int comparaisonMois =
                    gauche.indexMois.CompareTo(droite.indexMois);
                if (comparaisonMois != 0)
                {
                    return comparaisonMois;
                }

                int comparaisonType =
                    gauche.type.CompareTo(droite.type);
                if (comparaisonType != 0)
                {
                    return comparaisonType;
                }

                return string.CompareOrdinal(
                    gauche.actifId,
                    droite.actifId);
            });
    }

    private static int LimiterEnEntier(double valeur)
    {
        if (valeur >= int.MaxValue)
        {
            return int.MaxValue;
        }

        return valeur <= 0d
            ? 0
            : (int)valeur;
    }
}