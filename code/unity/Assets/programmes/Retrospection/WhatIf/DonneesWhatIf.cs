using System;
using System.Collections.Generic;

/// <summary>
/// Parametres configurables du moteur What If.
/// </summary>
[Serializable]
public sealed class ConfigurationWhatIf
{
    public int horizonMois = 6;
    public int largeurFaisceau = 50;
    public int pasAllocationPourcent = 20;
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
    public int valeurImmobilierAlternativeCentimes;
    public int dettesAlternativesCentimes;
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
            valeurImmobilierAlternativeCentimes =
                valeurImmobilierAlternativeCentimes,
            dettesAlternativesCentimes = dettesAlternativesCentimes,
            ecartCumuleCentimes = ecartCumuleCentimes
        };
    }
}

/// <summary>
/// Etat persistant et isole de la strategie alternative.
/// </summary>
[Serializable]
public sealed class DonneesWhatIf
{
    public bool initialisee;
    public bool actifsPassifsImmobiliersInitialises;
    public int moisInitialisation = -1;
    public int dernierMoisTraite = -1;
    public int dernierMoisMensualitesPretsTraite = -1;
    public int capitalInitialCentimes;
    public int liquiditesCentimes;
    public DonneesBourse portefeuille = new DonneesBourse();
    public DonneesImmobilier immobilier = new DonneesImmobilier();
    public List<DonneesPret> pretsImmobiliers =
        new List<DonneesPret>();
    public List<string> empreintesPretsReelsSynchronises =
        new List<string>();
    public ConfigurationWhatIf configuration = new ConfigurationWhatIf();
    public List<DecisionWhatIf> decisions = new List<DecisionWhatIf>();
    public List<PointHistoriqueWhatIf> historique =
        new List<PointHistoriqueWhatIf>();
    public List<OrdreHistoriqueWhatIf> ordres =
        new List<OrdreHistoriqueWhatIf>();

    public void InitialiserSiNecessaire()
    {
        if (portefeuille == null)
        {
            portefeuille = new DonneesBourse();
        }

        if (immobilier == null)
        {
            immobilier = new DonneesImmobilier();
        }
        if (immobilier.biensPossedes == null)
        {
            immobilier.biensPossedes = new List<BienImmobilier>();
        }
        if (immobilier.annoncesActuelles == null)
        {
            immobilier.annoncesActuelles =
                new List<AnnonceImmobiliere>();
        }

        if (pretsImmobiliers == null)
        {
            pretsImmobiliers = new List<DonneesPret>();
        }

        if (empreintesPretsReelsSynchronises == null)
        {
            empreintesPretsReelsSynchronises = new List<string>();
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

        if (ordres == null)
        {
            ordres = new List<OrdreHistoriqueWhatIf>();
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
            actifsPassifsImmobiliersInitialises =
                actifsPassifsImmobiliersInitialises,
            moisInitialisation = moisInitialisation,
            dernierMoisTraite = dernierMoisTraite,
            dernierMoisMensualitesPretsTraite =
                dernierMoisMensualitesPretsTraite,
            capitalInitialCentimes = capitalInitialCentimes,
            liquiditesCentimes = liquiditesCentimes,
            portefeuille = portefeuille.Copier(),
            immobilier = immobilier.Copier(),
            pretsImmobiliers = new List<DonneesPret>(),
            empreintesPretsReelsSynchronises =
                new List<string>(empreintesPretsReelsSynchronises),
            configuration = configuration.Copier(),
            decisions = new List<DecisionWhatIf>(),
            historique = new List<PointHistoriqueWhatIf>(),
            ordres = new List<OrdreHistoriqueWhatIf>()
        };

        foreach (DonneesPret pret in pretsImmobiliers)
        {
            if (pret != null)
            {
                copie.pretsImmobiliers.Add(CopierPret(pret));
            }
        }

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

        foreach (OrdreHistoriqueWhatIf ordre in ordres)
        {
            if (ordre != null)
            {
                copie.ordres.Add(ordre.Copier());
            }
        }

        return copie;
    }

    private static DonneesPret CopierPret(DonneesPret source)
    {
        return new DonneesPret(
            new argent(source.montantEmprunte.centimes),
            source.dureeAns,
            source.tauxAnnuel,
            new argent(source.mensualite.centimes))
        {
            moisRestants = source.moisRestants,
            capitalRestantDu =
                new argent(source.capitalRestantDu.centimes)
        };
    }
}
