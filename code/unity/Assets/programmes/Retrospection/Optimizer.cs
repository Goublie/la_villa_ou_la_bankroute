using System;
using System.Collections.Generic;

/// <summary>
/// Adapte l'historique persistant du moteur What If aux composants de
/// retrospective existants.
/// </summary>
public static class Optimizer
{
    [Serializable]
    public struct SimulationResult
    {
        public int indexMois;
        public Mois moisCalendrier;

        /// <summary>
        /// Pour le reel : patrimoine total. Pour le What If : liquidites.
        /// </summary>
        public argent soldeCourant;

        /// <summary>
        /// Pour le reel : zero. Pour le What If : valeur boursiere.
        /// </summary>
        public argent soldeEpargne;

        public argent patrimoineTotal;
    }

    /// <summary>
    /// Retourne les points du scenario alternatif reellement calcules mois par
    /// mois par le nouveau moteur What If.
    /// </summary>
    /// <remarks>
    /// Pour une ancienne sauvegarde qui ne possede pas encore d'historique
    /// What If, les snapshots sont exposes comme trajectoire neutre. Cette
    /// compatibilite evite d'inventer une optimisation retroactive.
    /// </remarks>
    public static List<SimulationResult> ObtenirHistoriqueWhatIf(
        GameData gameData)
    {
        List<PointHistoriqueWhatIf> points =
            ObtenirPointsWhatIf(gameData);

        if (points.Count == 0)
        {
            return ObtenirHistoriqueSnapshots(gameData);
        }

        List<SimulationResult> resultats =
            new List<SimulationResult>();

        foreach (PointHistoriqueWhatIf point in points)
        {
            resultats.Add(
                new SimulationResult
                {
                    indexMois = point.indexMois,
                    moisCalendrier = point.moisCalendrier,
                    soldeCourant = new argent(
                        Math.Max(
                            0,
                            point.liquiditesAlternativesCentimes)),
                    soldeEpargne = new argent(
                        Math.Max(
                            0,
                            point.valeurBourseAlternativeCentimes)),
                    patrimoineTotal = new argent(
                        Math.Max(
                            0,
                            point.patrimoineAlternatifCentimes))
                });
        }

        return resultats;
    }

    /// <summary>
    /// Ancien nom conserve pour ne pas casser une scene ou un ancien prefab.
    /// Le resultat provient maintenant du moteur What If, pas de la strategie
    /// Fourmi historique.
    /// </summary>
    public static List<SimulationResult> SimulerFourmi(
        GameData gameData)
    {
        return ObtenirHistoriqueWhatIf(gameData);
    }

    /// <summary>
    /// Retourne la trajectoire reelle enregistree en meme temps que le scenario
    /// alternatif, garantissant des points parfaitement alignes.
    /// </summary>
    public static List<SimulationResult> ObtenirHistoriqueReel(
        GameData gameData)
    {
        List<PointHistoriqueWhatIf> points =
            ObtenirPointsWhatIf(gameData);

        if (points.Count == 0)
        {
            return ObtenirHistoriqueSnapshots(gameData);
        }

        List<SimulationResult> resultats =
            new List<SimulationResult>();

        foreach (PointHistoriqueWhatIf point in points)
        {
            int patrimoine = Math.Max(
                0,
                point.patrimoineReelCentimes);

            resultats.Add(
                new SimulationResult
                {
                    indexMois = point.indexMois,
                    moisCalendrier = point.moisCalendrier,
                    soldeCourant = new argent(patrimoine),
                    soldeEpargne = new argent(0),
                    patrimoineTotal = new argent(patrimoine)
                });
        }

        return resultats;
    }

    private static List<PointHistoriqueWhatIf> ObtenirPointsWhatIf(
        GameData gameData)
    {
        List<PointHistoriqueWhatIf> points =
            new List<PointHistoriqueWhatIf>();

        if (gameData?.whatIf?.historique == null)
        {
            return points;
        }

        foreach (
            PointHistoriqueWhatIf point
            in gameData.whatIf.historique)
        {
            if (point != null)
            {
                points.Add(point.Copier());
            }
        }

        points.Sort(
            (gauche, droite) =>
                gauche.indexMois.CompareTo(droite.indexMois));

        LimiterAuxTreizeDerniers(points);
        return points;
    }

    /// <summary>
    /// Compatibilite lecture seule pour les anciennes parties et les tests
    /// d'architecture fondes sur SnapshotEtatJeu.
    /// </summary>
    private static List<SimulationResult> ObtenirHistoriqueSnapshots(
        GameData gameData)
    {
        List<SnapshotEtatJeu> snapshots =
            new List<SnapshotEtatJeu>();

        if (gameData?.historiqueSnapshots == null)
        {
            return new List<SimulationResult>();
        }

        foreach (
            SnapshotEtatJeu snapshot
            in gameData.historiqueSnapshots)
        {
            if (snapshot != null)
            {
                snapshots.Add(snapshot);
            }
        }

        snapshots.Sort(
            (gauche, droite) =>
                gauche.indexMois.CompareTo(droite.indexMois));

        if (snapshots.Count > 13)
        {
            snapshots.RemoveRange(
                0,
                snapshots.Count - 13);
        }

        List<SimulationResult> resultats =
            new List<SimulationResult>();

        foreach (SnapshotEtatJeu snapshot in snapshots)
        {
            int patrimoine = snapshot.joueur != null
                ? Math.Max(
                    0,
                    ServicePatrimoine.Calculer(
                        snapshot.joueur).centimes)
                : 0;

            resultats.Add(
                new SimulationResult
                {
                    indexMois = snapshot.indexMois,
                    moisCalendrier = snapshot.moisCalendrier,
                    soldeCourant = new argent(patrimoine),
                    soldeEpargne = new argent(0),
                    patrimoineTotal = new argent(patrimoine)
                });
        }

        return resultats;
    }

    private static void LimiterAuxTreizeDerniers<T>(
        List<T> elements)
    {
        if (elements != null && elements.Count > 13)
        {
            elements.RemoveRange(
                0,
                elements.Count - 13);
        }
    }
}