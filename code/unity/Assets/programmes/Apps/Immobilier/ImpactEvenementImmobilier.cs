using System;
using System.Collections.Generic;

/// <summary>
/// Impact immobilier persistant issu d'un evenement confirme.
/// Il ne contient aucune reference vers l'interface Unity.
/// </summary>
[Serializable]
public sealed class ImpactEvenementImmobilier
{
    public string evenementId;
    public int moisDebut;
    public int dureeMois = 1;
    public string villeCible = ServiceImpactsImmobiliers.CibleToutes;
    public string typeBienCible = ServiceImpactsImmobiliers.CibleToutes;
    public float coefficientPrix = 1f;
    public float coefficientLoyer = 1f;

    public bool EstActif(int mois)
    {
        int duree = Math.Max(1, dureeMois);
        return mois >= moisDebut && mois < moisDebut + duree;
    }

    public bool Cible(Ville ville, TypeBien type)
    {
        return ServiceImpactsImmobiliers.Correspond(
                   villeCible,
                   ville.ToString()) &&
               ServiceImpactsImmobiliers.Correspond(
                   typeBienCible,
                   type.ToString());
    }

    public ImpactEvenementImmobilier Copier()
    {
        return new ImpactEvenementImmobilier
        {
            evenementId = evenementId,
            moisDebut = moisDebut,
            dureeMois = dureeMois,
            villeCible = villeCible,
            typeBienCible = typeBienCible,
            coefficientPrix = coefficientPrix,
            coefficientLoyer = coefficientLoyer
        };
    }
}

/// <summary>
/// Fonctions pures de conversion, ciblage et calcul des impacts immobiliers.
/// </summary>
public static class ServiceImpactsImmobiliers
{
    public const string CibleToutes = "Tous";
    private const float CoefficientMinimum = 0.05f;
    private const float CoefficientMaximum = 5f;

    public static bool EssayerConvertir(
        ImpactDefinitionEvenement definition,
        string evenementId,
        int moisDebut,
        out ImpactEvenementImmobilier impact,
        out string erreur)
    {
        impact = null;
        erreur = string.Empty;

        if (definition == null ||
            string.IsNullOrWhiteSpace(evenementId) ||
            !string.Equals(
                definition.actif,
                "Immobilier",
                StringComparison.OrdinalIgnoreCase))
        {
            erreur = "Impact immobilier incomplet.";
            return false;
        }

        string ville = NormaliserCible(definition.ville);
        string typeBien = NormaliserCible(definition.typeBien);

        if (!EstVilleValide(ville))
        {
            erreur = "Ville immobiliere inconnue : " + ville + ".";
            return false;
        }

        if (!EstTypeBienValide(typeBien))
        {
            erreur = "Type de bien immobilier inconnu : " + typeBien + ".";
            return false;
        }

        int duree = definition.dureeMois <= 0
            ? 1
            : definition.dureeMois;
        if (duree > 120)
        {
            erreur = "La duree d'un impact immobilier ne peut pas depasser 120 mois.";
            return false;
        }

        float coefficientPrix = 1f + definition.variation;
        float coefficientLoyer = 1f + definition.variationLoyer;
        if (!EstCoefficientValide(coefficientPrix) ||
            !EstCoefficientValide(coefficientLoyer))
        {
            erreur = "Coefficient immobilier invalide pour " + evenementId + ".";
            return false;
        }

        impact = new ImpactEvenementImmobilier
        {
            evenementId = evenementId,
            moisDebut = Math.Max(0, moisDebut),
            dureeMois = duree,
            villeCible = ville,
            typeBienCible = typeBien,
            coefficientPrix = coefficientPrix,
            coefficientLoyer = coefficientLoyer
        };
        return true;
    }

    public static float CalculerCoefficientPrix(
        DonneesImmobilier donnees,
        Ville ville,
        TypeBien type,
        int mois)
    {
        return CalculerCoefficient(
            donnees,
            ville,
            type,
            mois,
            impact => impact.coefficientPrix);
    }

    public static float CalculerCoefficientLoyer(
        DonneesImmobilier donnees,
        Ville ville,
        TypeBien type,
        int mois)
    {
        return CalculerCoefficient(
            donnees,
            ville,
            type,
            mois,
            impact => impact.coefficientLoyer);
    }

    public static List<ImpactEvenementImmobilier> ObtenirImpactsActifs(
        DonneesImmobilier donnees,
        int mois)
    {
        List<ImpactEvenementImmobilier> resultat =
            new List<ImpactEvenementImmobilier>();

        donnees?.InitialiserSiNecessaire();
        if (donnees?.impactsActifs == null)
        {
            return resultat;
        }

        foreach (ImpactEvenementImmobilier impact in donnees.impactsActifs)
        {
            if (impact != null && impact.EstActif(mois))
            {
                resultat.Add(impact.Copier());
            }
        }

        resultat.Sort((gauche, droite) =>
        {
            int comparaison = string.CompareOrdinal(
                gauche.evenementId,
                droite.evenementId);
            return comparaison != 0
                ? comparaison
                : gauche.moisDebut.CompareTo(droite.moisDebut);
        });
        return resultat;
    }

    public static void RetirerImpactsTermines(
        DonneesImmobilier donnees,
        int mois)
    {
        donnees?.InitialiserSiNecessaire();
        donnees?.impactsActifs?.RemoveAll(
            impact => impact == null || !impact.EstActif(mois));
    }

    public static bool Correspond(string cible, string valeur)
    {
        string cibleNormalisee = NormaliserCible(cible);
        return cibleNormalisee == CibleToutes ||
            string.Equals(
                cibleNormalisee,
                valeur,
                StringComparison.OrdinalIgnoreCase);
    }

    private static float CalculerCoefficient(
        DonneesImmobilier donnees,
        Ville ville,
        TypeBien type,
        int mois,
        Func<ImpactEvenementImmobilier, float> selecteur)
    {
        donnees?.InitialiserSiNecessaire();
        if (donnees?.impactsActifs == null)
        {
            return 1f;
        }

        double coefficient = 1d;
        foreach (ImpactEvenementImmobilier impact in donnees.impactsActifs)
        {
            if (impact == null ||
                !impact.EstActif(mois) ||
                !impact.Cible(ville, type))
            {
                continue;
            }

            coefficient *= selecteur(impact);
        }

        return (float)Math.Clamp(
            coefficient,
            CoefficientMinimum,
            CoefficientMaximum);
    }

    private static string NormaliserCible(string valeur)
    {
        return string.IsNullOrWhiteSpace(valeur)
            ? CibleToutes
            : valeur.Trim();
    }

    private static bool EstVilleValide(string valeur)
    {
        if (valeur == CibleToutes)
        {
            return true;
        }

        return Enum.TryParse(valeur, true, out Ville _);
    }

    private static bool EstTypeBienValide(string valeur)
    {
        if (valeur == CibleToutes)
        {
            return true;
        }

        return Enum.TryParse(valeur, true, out TypeBien _);
    }

    private static bool EstCoefficientValide(float coefficient)
    {
        return !float.IsNaN(coefficient) &&
            !float.IsInfinity(coefficient) &&
            coefficient > 0f;
    }
}
