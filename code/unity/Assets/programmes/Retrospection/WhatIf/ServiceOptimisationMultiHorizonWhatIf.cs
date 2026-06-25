using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Compare plusieurs horizons de recherche avant de retenir la premiere
/// decision la plus robuste.
/// </summary>
/// <remarks>
/// Chaque recherche utilise uniquement les previsions deja construites avec
/// les informations connues au mois courant. Aucun prix futur n'est consulte.
/// </remarks>
public static class ServiceOptimisationMultiHorizonWhatIf
{
    private const int HorizonCourtMois = 3;
    private const int HorizonLongMinimumMois = 6;
    private const int HorizonLongMaximumMois = 9;
    private const int LargeurMinimum = 50;
    private const int PasMaximumPourcent = 20;
    private const double EpsilonComparaison = 0.0000001d;

    public static ResultatRechercheFaisceauWhatIf Rechercher(
        int capitalInitialCentimes,
        IReadOnlyDictionary<string, int> allocationActuellePourcent,
        IReadOnlyList<PrevisionActifWhatIf> previsions,
        ConfigurationWhatIf configuration,
        int indexMois)
    {
        ConfigurationWhatIf baseConfig =
            configuration != null
                ? configuration.Copier()
                : new ConfigurationWhatIf();
        baseConfig.Normaliser();

        int horizonLong = Math.Min(
            HorizonLongMaximumMois,
            Math.Max(
                HorizonLongMinimumMois,
                baseConfig.horizonMois));

        List<int> horizons = new List<int> { HorizonCourtMois };
        if (horizonLong != HorizonCourtMois)
        {
            horizons.Add(horizonLong);
        }

        ResultatRechercheFaisceauWhatIf meilleur = null;
        double meilleurScoreMensuel = double.NegativeInfinity;
        int meilleurHorizon = 0;
        int totalNoeuds = 0;
        List<string> diagnostics = new List<string>();

        foreach (int horizon in horizons)
        {
            ConfigurationWhatIf config = baseConfig.Copier();
            config.horizonMois = horizon;
            config.largeurFaisceau = Math.Max(
                LargeurMinimum,
                config.largeurFaisceau);
            config.pasAllocationPourcent = Math.Min(
                PasMaximumPourcent,
                config.pasAllocationPourcent);
            config.Normaliser();

            ResultatRechercheFaisceauWhatIf courant =
                MoteurRechercheFaisceauWhatIf.Rechercher(
                    capitalInitialCentimes,
                    allocationActuellePourcent,
                    previsions,
                    config,
                    indexMois);

            if (courant == null)
            {
                continue;
            }

            totalNoeuds += Math.Max(0, courant.noeudsEvalues);
            double scoreMensuel = CalculerScoreMensuelNormalise(
                courant,
                capitalInitialCentimes,
                horizon);

            diagnostics.Add(
                horizon +
                " mois=" +
                scoreMensuel.ToString("P2", CultureInfo.InvariantCulture));

            bool meilleurScore =
                scoreMensuel > meilleurScoreMensuel + EpsilonComparaison;
            bool egalitePlusLongue =
                Math.Abs(scoreMensuel - meilleurScoreMensuel) <=
                    EpsilonComparaison &&
                horizon > meilleurHorizon;

            if (courant.decisionRetenue != null &&
                (meilleur == null ||
                 meilleurScore ||
                 egalitePlusLongue))
            {
                meilleur = courant;
                meilleurScoreMensuel = scoreMensuel;
                meilleurHorizon = horizon;
            }
        }

        if (meilleur == null)
        {
            ConfigurationWhatIf secours = baseConfig.Copier();
            secours.largeurFaisceau = Math.Max(
                LargeurMinimum,
                secours.largeurFaisceau);
            secours.pasAllocationPourcent = Math.Min(
                PasMaximumPourcent,
                secours.pasAllocationPourcent);
            secours.Normaliser();

            meilleur = MoteurRechercheFaisceauWhatIf.Rechercher(
                capitalInitialCentimes,
                allocationActuellePourcent,
                previsions,
                secours,
                indexMois);
            return meilleur;
        }

        meilleur.noeudsEvalues = totalNoeuds;
        meilleur.diagnostic =
            "Optimisation multi-horizon retenue sur " +
            meilleurHorizon +
            " mois. Comparaison : " +
            string.Join(", ", diagnostics) +
            ". Total : " +
            totalNoeuds +
            " noeuds evalues.";

        return meilleur;
    }

    private static double CalculerScoreMensuelNormalise(
        ResultatRechercheFaisceauWhatIf resultat,
        int capitalInitialCentimes,
        int horizonMois)
    {
        if (resultat == null ||
            resultat.decisionRetenue == null ||
            capitalInitialCentimes <= 0)
        {
            return double.NegativeInfinity;
        }

        double scoreFinal = resultat.patrimoineProjeteCentimes;
        if (resultat.chemin != null && resultat.chemin.Count > 0)
        {
            DecisionWhatIf derniere =
                resultat.chemin[resultat.chemin.Count - 1];
            if (derniere != null && derniere.score > 0d)
            {
                scoreFinal = derniere.score;
            }
        }

        if (scoreFinal <= 0d)
        {
            return -1d;
        }

        double ratio = scoreFinal / capitalInitialCentimes;
        return Math.Pow(
            ratio,
            1d / Math.Max(1, horizonMois)) - 1d;
    }
}
