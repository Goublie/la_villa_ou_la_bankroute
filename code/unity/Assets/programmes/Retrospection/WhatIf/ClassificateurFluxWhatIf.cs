using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Nature d'un mouvement financier vu par le moteur What If.
/// </summary>
public enum TypeFluxWhatIf
{
    Inconnu,
    RevenuExterne,
    DepenseExterne,
    TransfertInterne,
    DecisionBoursiere,
    RendementInterne,
    DecisionHorsBourse
}

/// <summary>
/// Resultat explicable d'une classification de transaction.
/// </summary>
[Serializable]
public sealed class ResultatClassificationFluxWhatIf
{
    public TypeFluxWhatIf type;
    public bool classificationCertaine;
    public bool doitEtreReproduit;
    public string diagnostic;
    public string libelleOriginal;
    public string libelleNormalise;
    public int montantCentimes;
}

/// <summary>
/// Classe les transactions des snapshots sans modifier la Banque existante.
/// </summary>
/// <remarks>
/// Le moteur optimise uniquement les decisions boursieres. Les revenus,
/// depenses de vie et decisions hors Bourse sont donc reproduits. Les achats,
/// ventes, transferts internes et interets sont remplaces ou recalcules.
/// Toute regle heuristique reste visible grace a un diagnostic.
/// </remarks>
public static class ClassificateurFluxWhatIf
{
    private static readonly HashSet<string> LibellesTransfertInterne =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "courant vers epargne",
            "credit",
            "debit",
            "versement depuis le compte epargne",
            "epargne vers courant"
        };

    private static readonly HashSet<string> LibellesDecisionHorsBourse =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "injection dans le projet",
            "developpement du produit",
            "etude de marche"
        };

    /// <summary>
    /// Classe une transaction copiee depuis un snapshot.
    /// </summary>
    public static ResultatClassificationFluxWhatIf Classifier(
        Transaction transaction)
    {
        if (transaction == null)
        {
            return CreerResultat(
                TypeFluxWhatIf.Inconnu,
                false,
                false,
                string.Empty,
                string.Empty,
                0,
                "Transaction absente : aucun flux ne peut etre reproduit.");
        }

        return Classifier(
            transaction.libelle,
            transaction.montant.centimes);
    }

    /// <summary>
    /// Surcharge testable sans dependre de la creation d'un compte bancaire.
    /// </summary>
    public static ResultatClassificationFluxWhatIf Classifier(
        string libelle,
        int montantCentimes)
    {
        string normalise = NormaliserLibelle(libelle);

        if (montantCentimes == 0)
        {
            return CreerResultat(
                TypeFluxWhatIf.Inconnu,
                false,
                false,
                libelle,
                normalise,
                montantCentimes,
                "Flux nul ignore.");
        }

        if (normalise == "salaire" ||
            normalise.StartsWith("salaire ", StringComparison.Ordinal))
        {
            return CreerResultat(
                TypeFluxWhatIf.RevenuExterne,
                true,
                true,
                libelle,
                normalise,
                montantCentimes,
                "Salaire reproduit dans la trajectoire alternative.");
        }

        if (normalise == "interet" ||
            normalise == "interets" ||
            normalise.StartsWith("interet ", StringComparison.Ordinal) ||
            normalise.StartsWith("interets ", StringComparison.Ordinal))
        {
            return CreerResultat(
                TypeFluxWhatIf.RendementInterne,
                true,
                false,
                libelle,
                normalise,
                montantCentimes,
                "Rendement interne recalcule selon la strategie alternative.");
        }

        if (LibellesTransfertInterne.Contains(normalise))
        {
            return CreerResultat(
                TypeFluxWhatIf.TransfertInterne,
                true,
                false,
                libelle,
                normalise,
                montantCentimes,
                "Transfert interne ignore pour eviter un double comptage.");
        }

        if (normalise.StartsWith("achat ", StringComparison.Ordinal) ||
            normalise.StartsWith("vente ", StringComparison.Ordinal))
        {
            return CreerResultat(
                TypeFluxWhatIf.DecisionBoursiere,
                true,
                false,
                libelle,
                normalise,
                montantCentimes,
                "Decision boursiere reelle remplacee par la decision What If.");
        }

        if (LibellesDecisionHorsBourse.Contains(normalise))
        {
            return CreerResultat(
                TypeFluxWhatIf.DecisionHorsBourse,
                true,
                true,
                libelle,
                normalise,
                montantCentimes,
                "Decision hors Bourse conservee pour une comparaison equitable.");
        }

        if (montantCentimes < 0)
        {
            return CreerResultat(
                TypeFluxWhatIf.DepenseExterne,
                false,
                true,
                libelle,
                normalise,
                montantCentimes,
                "Libelle non repertorie : sortie reproduite par prudence.");
        }

        return CreerResultat(
            TypeFluxWhatIf.RevenuExterne,
            false,
            true,
            libelle,
            normalise,
            montantCentimes,
            "Libelle non repertorie : entree reproduite par prudence.");
    }

    private static ResultatClassificationFluxWhatIf CreerResultat(
        TypeFluxWhatIf type,
        bool certaine,
        bool reproduire,
        string libelleOriginal,
        string libelleNormalise,
        int montantCentimes,
        string diagnostic)
    {
        return new ResultatClassificationFluxWhatIf
        {
            type = type,
            classificationCertaine = certaine,
            doitEtreReproduit = reproduire,
            diagnostic = diagnostic ?? string.Empty,
            libelleOriginal = libelleOriginal ?? string.Empty,
            libelleNormalise = libelleNormalise ?? string.Empty,
            montantCentimes = montantCentimes
        };
    }

    private static string NormaliserLibelle(string libelle)
    {
        if (string.IsNullOrWhiteSpace(libelle))
        {
            return string.Empty;
        }

        string decompose = libelle
            .Trim()
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        StringBuilder sansAccents = new StringBuilder(decompose.Length);
        foreach (char caractere in decompose)
        {
            UnicodeCategory categorie =
                CharUnicodeInfo.GetUnicodeCategory(caractere);
            if (categorie != UnicodeCategory.NonSpacingMark)
            {
                sansAccents.Append(caractere);
            }
        }

        return Regex.Replace(
                sansAccents.ToString().Normalize(NormalizationForm.FormC),
                @"\s+",
                " ")
            .Trim();
    }
}