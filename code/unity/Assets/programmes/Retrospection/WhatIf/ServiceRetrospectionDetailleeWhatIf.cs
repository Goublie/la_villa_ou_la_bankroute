using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>
/// Ligne déjà préparée pour le tableau de rétrospection.
/// </summary>
[Serializable]
public sealed class LigneOperationRetrospectionWhatIf
{
    public int indexMois;
    public string mois;
    public string operation;
    public string detail;
}

/// <summary>
/// Prépare les achats, ventes et événements confirmés à afficher sans modifier
/// les données persistantes du jeu.
/// </summary>
public static class ServiceRetrospectionDetailleeWhatIf
{
    private const int NombreMoisAffiches = 12;

    public static List<LigneOperationRetrospectionWhatIf>
        ConstruireLignesOrdres(
            DonneesWhatIf donnees,
            int moisCourant)
    {
        List<OrdreHistoriqueWhatIf> ordres =
            ServiceJournalOrdresWhatIf
                .ObtenirOrdresDouzeDerniersMois(
                    donnees,
                    Math.Max(0, moisCourant));

        ordres.Sort(ComparerOrdresRecents);

        List<LigneOperationRetrospectionWhatIf> resultat =
            new List<LigneOperationRetrospectionWhatIf>();

        foreach (OrdreHistoriqueWhatIf ordre in ordres)
        {
            if (ordre == null)
            {
                continue;
            }

            resultat.Add(
                new LigneOperationRetrospectionWhatIf
                {
                    indexMois = ordre.indexMois,
                    mois = FormaterMois(ordre.indexMois),
                    operation = FormaterTypeOrdre(ordre.type),
                    detail = ConstruireDetailOrdre(ordre)
                });
        }

        return resultat;
    }

    public static string ConstruireSectionEvenementsConfirmes(
        DonneesEvenements donneesEvenements,
        DonneesWhatIf donneesWhatIf,
        int moisCourant)
    {
        int maximum = Math.Max(0, moisCourant);
        int minimum = Math.Max(
            0,
            maximum - NombreMoisAffiches + 1);

        List<EvenementConfirmePartie> evenements =
            new List<EvenementConfirmePartie>();

        if (donneesEvenements?.evenementsConfirmes != null)
        {
            foreach (
                EvenementConfirmePartie evenement
                in donneesEvenements.evenementsConfirmes)
            {
                if (evenement == null ||
                    evenement.moisConfirmation < minimum ||
                    evenement.moisConfirmation > maximum)
                {
                    continue;
                }

                evenements.Add(evenement.Copier());
            }
        }

        evenements.Sort(
            (gauche, droite) =>
            {
                int comparaisonMois =
                    droite.moisConfirmation.CompareTo(
                        gauche.moisConfirmation);

                if (comparaisonMois != 0)
                {
                    return comparaisonMois;
                }

                return string.CompareOrdinal(
                    gauche.titre,
                    droite.titre);
            });

        StringBuilder texte = new StringBuilder();
        texte.Append(
            "<color=#4da6ff><b>Événements confirmés des 12 derniers mois</b></color>");

        if (evenements.Count == 0)
        {
            texte.Append(
                "\nAucun événement confirmé sur cette période.");
            return texte.ToString();
        }

        foreach (EvenementConfirmePartie evenement in evenements)
        {
            texte.Append("\n\n<b>");
            texte.Append(FormaterMois(evenement.moisConfirmation));
            texte.Append(" — ");
            texte.Append(
                string.IsNullOrWhiteSpace(evenement.titre)
                    ? "Événement sans titre"
                    : evenement.titre.Trim());
            texte.Append("</b>");

            if (!string.IsNullOrWhiteSpace(evenement.categorie))
            {
                texte.Append("\nCatégorie : ");
                texte.Append(evenement.categorie.Trim());
                texte.Append(".");
            }

            string impacts = ConstruireResumeImpacts(evenement.impacts);
            if (!string.IsNullOrEmpty(impacts))
            {
                texte.Append("\nImpacts : ");
                texte.Append(impacts);
            }

            if (evenement.categorie == CategoriesEvenements.Boursiers)
            {
                int premierMois = ObtenirPremierMoisPriseEnCompte(
                    evenement,
                    donneesWhatIf);

                if (premierMois >= 0)
                {
                    texte.Append(
                        "\nPris en compte par le moteur What If à partir de ");
                    texte.Append(FormaterMois(premierMois));
                    texte.Append(".");
                }
                else if (evenement.impacts == null ||
                         evenement.impacts.Count == 0)
                {
                    texte.Append(
                        "\nÉvénement boursier sans impact chiffré exploitable.");
                }
                else
                {
                    texte.Append(
                        "\nAucun impact actif retrouvé dans les décisions What If.");
                }
            }
            else
            {
                texte.Append(
                    "\nÉvénement conservé dans l'historique, hors optimisation boursière.");
            }
        }

        return texte.ToString();
    }

    public static string FormaterMois(int indexMois)
    {
        int index =
            ((int)Mois.Juillet + Math.Max(0, indexMois)) % 12;

        switch ((Mois)index)
        {
            case Mois.Janvier:
                return "Janvier";
            case Mois.Fevrier:
                return "Février";
            case Mois.Mars:
                return "Mars";
            case Mois.Avril:
                return "Avril";
            case Mois.Mai:
                return "Mai";
            case Mois.Juin:
                return "Juin";
            case Mois.Juillet:
                return "Juillet";
            case Mois.Aout:
                return "Août";
            case Mois.Septembre:
                return "Septembre";
            case Mois.Octobre:
                return "Octobre";
            case Mois.Novembre:
                return "Novembre";
            case Mois.Decembre:
                return "Décembre";
            default:
                return "Mois " + (Math.Max(0, indexMois) + 1);
        }
    }

    private static int ComparerOrdresRecents(
        OrdreHistoriqueWhatIf gauche,
        OrdreHistoriqueWhatIf droite)
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
            droite.indexMois.CompareTo(gauche.indexMois);

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
    }

    private static string FormaterTypeOrdre(
        TypeOrdreHistoriqueWhatIf type)
    {
        switch (type)
        {
            case TypeOrdreHistoriqueWhatIf.Achat:
                return "ACHAT";
            case TypeOrdreHistoriqueWhatIf.Vente:
                return "VENTE";
            case TypeOrdreHistoriqueWhatIf.VenteForcee:
                return "VENTE FORCÉE";
            default:
                return "OPÉRATION";
        }
    }

    private static string ConstruireDetailOrdre(
        OrdreHistoriqueWhatIf ordre)
    {
        CultureInfo culture = CultureInfo.InvariantCulture;
        StringBuilder texte = new StringBuilder();

        texte.Append(
            string.IsNullOrWhiteSpace(ordre.actifId)
                ? "Actif inconnu"
                : ordre.actifId.Trim());

        texte.Append(" : ");
        texte.Append(
            Math.Max(0f, ordre.quantite)
                .ToString("0.####", culture));
        texte.Append(" unité(s) à ");
        texte.Append(
            new argent(
                Math.Max(0, ordre.prixUnitaireCentimes)));
        texte.Append(" = ");
        texte.Append(
            new argent(Math.Max(0, ordre.montantCentimes)));

        if (ordre.coutTransactionCentimes > 0)
        {
            texte.Append(" | frais ");
            texte.Append(
                new argent(ordre.coutTransactionCentimes));
        }

        if (!string.IsNullOrWhiteSpace(ordre.raison))
        {
            texte.Append(" | ");
            texte.Append(ordre.raison.Trim());
        }

        return texte.ToString();
    }

    private static string ConstruireResumeImpacts(
        IReadOnlyList<ImpactDefinitionEvenement> impacts)
    {
        if (impacts == null || impacts.Count == 0)
        {
            return string.Empty;
        }

        List<string> lignes = new List<string>();

        foreach (ImpactDefinitionEvenement impact in impacts)
        {
            if (impact == null ||
                string.IsNullOrWhiteSpace(impact.actif))
            {
                continue;
            }

            float pourcentage = impact.variation * 100f;
            string signe = pourcentage > 0f ? "+" : string.Empty;

            lignes.Add(
                impact.actif.Trim() +
                " " +
                signe +
                pourcentage.ToString(
                    "0.##",
                    CultureInfo.InvariantCulture) +
                " %");
        }

        lignes.Sort(StringComparer.Ordinal);
        return string.Join(", ", lignes);
    }

    private static int ObtenirPremierMoisPriseEnCompte(
        EvenementConfirmePartie evenement,
        DonneesWhatIf donneesWhatIf)
    {
        if (evenement == null ||
            donneesWhatIf?.decisions == null)
        {
            return -1;
        }

        HashSet<string> ids =
            new HashSet<string>(StringComparer.Ordinal);

        if (!string.IsNullOrWhiteSpace(evenement.definitionId))
        {
            ids.Add(evenement.definitionId);
        }

        if (!string.IsNullOrWhiteSpace(evenement.rumeurId))
        {
            ids.Add(evenement.rumeurId);
        }

        int premierMois = int.MaxValue;

        foreach (DecisionWhatIf decision in donneesWhatIf.decisions)
        {
            if (decision == null ||
                decision.indexMois < evenement.moisConfirmation ||
                decision.evenementsConnusIds == null)
            {
                continue;
            }

            bool trouve = false;

            foreach (string id in decision.evenementsConnusIds)
            {
                if (!string.IsNullOrWhiteSpace(id) &&
                    ids.Contains(id))
                {
                    trouve = true;
                    break;
                }
            }

            if (trouve)
            {
                premierMois = Math.Min(
                    premierMois,
                    decision.indexMois);
            }
        }

        return premierMois == int.MaxValue
            ? -1
            : premierMois;
    }
}