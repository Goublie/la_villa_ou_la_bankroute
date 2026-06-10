using System;
using System.Collections.Generic;

[Serializable]
public class PositionBourse
{
    public string actifId;
    public float quantite;
    public int coutTotalCentimes;

    public PositionBourse(string actifId)
    {
        this.actifId = actifId;
    }

    public PositionBourse Copier()
    {
        return new PositionBourse(actifId)
        {
            quantite = quantite,
            coutTotalCentimes = coutTotalCentimes
        };
    }
}

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

    public bool EstActif(int mois)
    {
        return mois >= moisDebut &&
            (dureeMois < 0 || mois < moisDebut + dureeMois);
    }

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

[Serializable]
public class DonneesBourse : IPatrimoine
{
    public List<PositionBourse> positions = new List<PositionBourse>();
    public List<ImpactEvenementMarche> impactsMarche =
        new List<ImpactEvenementMarche>();
    public int dernierMoisObserve = -1;
    public string dernierMessage = "Sélectionnez un actif pour commencer.";
    public int valeurMarcheCentimes;
    public int moisValorisation = -1;

    public PositionBourse TrouverPosition(string actifId)
    {
        if (positions == null)
        {
            positions = new List<PositionBourse>();
        }

        return positions.Find(position => position != null && position.actifId == actifId);
    }

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

    public void SupprimerPositionsVides()
    {
        if (positions == null)
        {
            positions = new List<PositionBourse>();
            return;
        }

        positions.RemoveAll(position => position == null || position.quantite <= 0.000001f);
    }

    public void DefinirValeurMarche(int valeurCentimes, int mois)
    {
        valeurMarcheCentimes = Math.Max(0, valeurCentimes);
        moisValorisation = Math.Max(0, mois);
    }

    public int CalculerCapitalInvestiCentimes()
    {
        long total = 0;
        if (positions != null)
        {
            foreach (PositionBourse position in positions)
            {
                if (position != null && position.quantite > 0f)
                {
                    total += Math.Max(0, position.coutTotalCentimes);
                }
            }
        }

        return total > int.MaxValue ? int.MaxValue : (int)total;
    }

    public argent GetValeurPatrimoine()
    {
        return new argent(valeurMarcheCentimes);
    }

    public argent GetGainsPertesLatents()
    {
        return new argent(valeurMarcheCentimes - CalculerCapitalInvestiCentimes());
    }

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

        if (positions != null)
        {
            foreach (PositionBourse position in positions)
            {
                if (position != null)
                {
                    copie.positions.Add(position.Copier());
                }
            }
        }

        if (impactsMarche != null)
        {
            foreach (ImpactEvenementMarche impact in impactsMarche)
            {
                if (impact != null)
                {
                    copie.impactsMarche.Add(impact.Copier());
                }
            }
        }

        return copie;
    }
}
