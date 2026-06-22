using System;
using System.Collections.Generic;

/// <summary>
/// ParamÃ¨tres configurables du moteur What If.
/// </summary>
[Serializable]
public sealed class ConfigurationWhatIf
{
    public int horizonMois = 3;
    public int largeurFaisceau = 25;
    public int pasAllocationPourcent = 25;
    public float penaliteRisque = 0.15f;
    public float penaliteDrawdown = 0.25f;
    public int coutTransactionCentimes;

    public void Normaliser()
    {
        horizonMois = Math.Min(12, Math.Max(1, horizonMois));
        largeurFaisceau = Math.Min(200, Math.Max(2, largeurFaisceau));
        pasAllocationPourcent = Math.Min(
            100,
            Math.Max(1, pasAllocationPourcent));
        penaliteRisque = Math.Max(0f, penaliteRisque);
        penaliteDrawdown = Math.Max(0f, penaliteDrawdown);
        coutTransactionCentimes = Math.Max(0, coutTransactionCentimes);
    }

    public ConfigurationWhatIf Copier()
    {
        return new ConfigurationWhatIf
        {
            horizonMois = horizonMois,
            largeurFaisceau = largeurFaisceau,
            pasAllocationPourcent = pasAllocationPourcent,
            penaliteRisque = penaliteRisque,
            penaliteDrawdown = penaliteDrawdown,
            coutTransactionCentimes = coutTransactionCentimes
        };
    }
}

[Serializable]
public sealed class AllocationActifWhatIf
{
    public string actifId;
    public int pourcentage;

    public AllocationActifWhatIf()
    {
    }

    public AllocationActifWhatIf(string actifId, int pourcentage)
    {
        this.actifId = actifId;
        this.pourcentage = pourcentage;
    }

    public AllocationActifWhatIf Copier()
    {
        return new AllocationActifWhatIf(actifId, pourcentage);
    }
}

[Serializable]
public sealed class DecisionWhatIf
{
    public int indexMois;
    public string strategieId;
    public List<AllocationActifWhatIf> allocations =
        new List<AllocationActifWhatIf>();
    public List<string> evenementsConnusIds = new List<string>();
    public string explication;
    public float rendementAttenduPourcent;
    public float risqueEstime;
    public float drawdownEstime;
    public double score;

    public DecisionWhatIf Copier()
    {
        DecisionWhatIf copie = new DecisionWhatIf
        {
            indexMois = indexMois,
            strategieId = strategieId,
            explication = explication,
            rendementAttenduPourcent = rendementAttenduPourcent,
            risqueEstime = risqueEstime,
            drawdownEstime = drawdownEstime,
            score = score
        };

        if (allocations != null)
        {
            foreach (AllocationActifWhatIf allocation in allocations)
            {
                if (allocation != null)
                {
                    copie.allocations.Add(allocation.Copier());
                }
            }
        }

        if (evenementsConnusIds != null)
        {
            copie.evenementsConnusIds.AddRange(evenementsConnusIds);
        }

        return copie;
    }
}

[Serializable]
public sealed class PointHistoriqueWhatIf
{
    public int indexMois;
    public Mois moisCalendrier;
    public int patrimoineReelCentimes;
    public int patrimoineAlternatifCentimes;
    public int liquiditesAlternativesCentimes;
    public int valeurBourseAlternativeCentimes;
    public int ecartCumuleCentimes;

    public PointHistoriqueWhatIf Copier()
    {
        return new PointHistoriqueWhatIf
        {
            indexMois = indexMois,
            moisCalendrier = moisCalendrier,
            patrimoineReelCentimes = patrimoineReelCentimes,
            patrimoineAlternatifCentimes = patrimoineAlternatifCentimes,
            liquiditesAlternativesCentimes =
                liquiditesAlternativesCentimes,
            valeurBourseAlternativeCentimes =
                valeurBourseAlternativeCentimes,
            ecartCumuleCentimes = ecartCumuleCentimes
        };
    }
}

/// <summary>
/// Ã‰tat persistant et isolÃ© de la stratÃ©gie boursiÃ¨re alternative.
/// </summary>
[Serializable]
public sealed class DonneesWhatIf
{
    public bool initialisee;
    public int moisInitialisation = -1;
    public int dernierMoisTraite = -1;
    public int capitalInitialCentimes;
    public int liquiditesCentimes;
    public DonneesBourse portefeuille = new DonneesBourse();
    public ConfigurationWhatIf configuration = new ConfigurationWhatIf();
    public List<DecisionWhatIf> decisions = new List<DecisionWhatIf>();
    public List<PointHistoriqueWhatIf> historique =
        new List<PointHistoriqueWhatIf>();

    public void InitialiserSiNecessaire()
    {
        if (portefeuille == null)
        {
            portefeuille = new DonneesBourse();
        }

        if (configuration == null)
        {
            configuration = new ConfigurationWhatIf();
        }
        configuration.Normaliser();

        if (decisions == null)
        {
            decisions = new List<DecisionWhatIf>();
        }

        if (historique == null)
        {
            historique = new List<PointHistoriqueWhatIf>();
        }

        capitalInitialCentimes = Math.Max(0, capitalInitialCentimes);
        liquiditesCentimes = Math.Max(0, liquiditesCentimes);
    }

    public DonneesWhatIf Copier()
    {
        InitialiserSiNecessaire();

        DonneesWhatIf copie = new DonneesWhatIf
        {
            initialisee = initialisee,
            moisInitialisation = moisInitialisation,
            dernierMoisTraite = dernierMoisTraite,
            capitalInitialCentimes = capitalInitialCentimes,
            liquiditesCentimes = liquiditesCentimes,
            portefeuille = portefeuille.Copier(),
            configuration = configuration.Copier(),
            decisions = new List<DecisionWhatIf>(),
            historique = new List<PointHistoriqueWhatIf>()
        };

        foreach (DecisionWhatIf decision in decisions)
        {
            if (decision != null)
            {
                copie.decisions.Add(decision.Copier());
            }
        }

        foreach (PointHistoriqueWhatIf point in historique)
        {
            if (point != null)
            {
                copie.historique.Add(point.Copier());
            }
        }

        return copie;
    }
}
