using System;
using System.Collections.Generic;
using System.Globalization;

[Serializable]
public sealed class ResultatAchatImmobilierWhatIf
{
    public bool succes;
    public bool achatEffectue;
    public int indexMois;
    public int prixAchatCentimes;
    public int loyerMensuelCentimes;
    public float rendementBrutAnnuelPourcent;
    public string empreinteAnnonce;
    public string diagnostic;
}

/// <summary>
/// Recherche une opportunite immobiliere connue et l'achete sans creer de pret.
/// </summary>
/// <remarks>
/// Seules les annonces visibles au mois courant sont lues. Le service n'utilise
/// aucune donnee future et ne modifie jamais le patrimoine reel.
/// </remarks>
public static class ServiceImmobilierAlternatifWhatIf
{
    private const float RendementBrutMinimum = 0.05f;
    private const float PartMaximumLiquidites = 0.80f;

    public static ResultatAchatImmobilierWhatIf
        EvaluerEtAcheterComptant(
            DonneesWhatIf donnees,
            IReadOnlyList<AnnonceImmobiliere> annoncesConnues,
            int indexMois)
    {
        ResultatAchatImmobilierWhatIf resultat =
            new ResultatAchatImmobilierWhatIf
            {
                indexMois = Math.Max(0, indexMois)
            };

        if (donnees == null)
        {
            resultat.diagnostic = "Donnees What If absentes.";
            return resultat;
        }

        donnees.InitialiserSiNecessaire();
        resultat.succes = true;

        if (donnees.dernierMoisAchatImmobilier >= resultat.indexMois)
        {
            resultat.diagnostic =
                "Decision immobiliere alternative deja traitee ce mois.";
            return resultat;
        }

        if (annoncesConnues == null || annoncesConnues.Count == 0)
        {
            resultat.diagnostic =
                "Aucune annonce immobiliere connue ce mois.";
            return resultat;
        }

        int liquidites = Math.Max(0, donnees.liquiditesCentimes);
        int budgetMaximum = (int)Math.Floor(
            liquidites * PartMaximumLiquidites);

        if (budgetMaximum <= 0)
        {
            resultat.diagnostic =
                "Aucune liquidite disponible pour un achat comptant.";
            return resultat;
        }

        HashSet<string> dejaAchetees =
            new HashSet<string>(
                donnees.empreintesAnnoncesImmobilieresAchetees,
                StringComparer.Ordinal);

        AnnonceImmobiliere meilleure = null;
        string meilleureEmpreinte = string.Empty;
        float meilleurRendement = float.MinValue;

        foreach (AnnonceImmobiliere annonce in annoncesConnues)
        {
            if (annonce == null ||
                annonce.PrixVenteAffiche.centimes <= 0 ||
                annonce.LoyerMensuelPropose.centimes <= 0)
            {
                continue;
            }

            string empreinte = ConstruireEmpreinte(annonce);
            if (dejaAchetees.Contains(empreinte) ||
                annonce.PrixVenteAffiche.centimes > budgetMaximum)
            {
                continue;
            }

            float rendement = CalculerRendementBrut(annonce);
            if (rendement < RendementBrutMinimum)
            {
                continue;
            }

            bool meilleur =
                meilleure == null ||
                rendement > meilleurRendement + 0.000001f ||
                (Math.Abs(rendement - meilleurRendement) <= 0.000001f &&
                 annonce.PrixVenteAffiche.centimes <
                 meilleure.PrixVenteAffiche.centimes) ||
                (Math.Abs(rendement - meilleurRendement) <= 0.000001f &&
                 annonce.PrixVenteAffiche.centimes ==
                 meilleure.PrixVenteAffiche.centimes &&
                 string.CompareOrdinal(
                     empreinte,
                     meilleureEmpreinte) < 0);

            if (!meilleur)
            {
                continue;
            }

            meilleure = annonce;
            meilleureEmpreinte = empreinte;
            meilleurRendement = rendement;
        }

        if (meilleure == null)
        {
            resultat.diagnostic =
                "Aucune annonce connue ne respecte le budget comptant et " +
                "le rendement minimal de 5 %.";
            return resultat;
        }

        int prix = meilleure.PrixVenteAffiche.centimes;
        if (prix > donnees.liquiditesCentimes)
        {
            resultat.diagnostic =
                "Liquidites insuffisantes pour l'opportunite retenue.";
            return resultat;
        }

        BienImmobilier bien = new BienImmobilier
        {
            idUnique = "what-if|" + meilleureEmpreinte,
            ville = meilleure.Ville,
            type = meilleure.Type,
            surfaceM2 = meilleure.SurfaceM2,
            estMeuble = meilleure.EstMeuble,
            moisAchat = resultat.indexMois,
            indicePrixReferenceAchat =
                ServiceImmobilier.CalculerIndicePrixReference(
                    meilleure.Ville,
                    meilleure.Type,
                    resultat.indexMois,
                    donnees.immobilier),
            valeurReferenceAchatCentimes = prix,            prixAchat = new argent(prix),
            valeurActuelle = new argent(prix),
            estLoue = true,
            tauxRendementInitial = meilleure.TauxRendementBrut,
            loyerInitial =
                new argent(meilleure.LoyerMensuelPropose.centimes),
            loyerMensuel =
                new argent(meilleure.LoyerMensuelPropose.centimes)
        };

        donnees.liquiditesCentimes = Math.Max(
            0,
            donnees.liquiditesCentimes - prix);
        donnees.immobilier.biensPossedes.Add(bien);
        donnees.empreintesAnnoncesImmobilieresAchetees.Add(
            meilleureEmpreinte);
        donnees.dernierMoisAchatImmobilier = resultat.indexMois;

        resultat.achatEffectue = true;
        resultat.prixAchatCentimes = prix;
        resultat.loyerMensuelCentimes =
            meilleure.LoyerMensuelPropose.centimes;
        resultat.rendementBrutAnnuelPourcent =
            meilleurRendement * 100f;
        resultat.empreinteAnnonce = meilleureEmpreinte;
        resultat.diagnostic =
            "Achat immobilier alternatif comptant : " +
            meilleure.Type +
            " a " +
            meilleure.Ville +
            ", rendement brut " +
            resultat.rendementBrutAnnuelPourcent.ToString(
                "0.00",
                CultureInfo.InvariantCulture) +
            " %, sans nouveau pret.";

        return resultat;
    }

    public static string ConstruireEmpreinte(
        AnnonceImmobiliere annonce)
    {
        if (annonce == null)
        {
            return string.Empty;
        }

        return
            annonce.Ville + "|" +
            annonce.Type + "|" +
            annonce.SurfaceM2 + "|" +
            (annonce.EstMeuble ? "1" : "0") + "|" +
            annonce.PrixVenteAffiche.centimes + "|" +
            annonce.LoyerMensuelPropose.centimes;
    }

    private static float CalculerRendementBrut(
        AnnonceImmobiliere annonce)
    {
        return annonce == null ||
               annonce.PrixVenteAffiche.centimes <= 0
            ? 0f
            : annonce.LoyerMensuelPropose.centimes *
              12f /
              annonce.PrixVenteAffiche.centimes;
    }
}