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
public class DonneesBourse
{
    public List<PositionBourse> positions = new List<PositionBourse>();
    public int dernierMoisObserve = -1;
    public string dernierMessage = "Sélectionnez un actif pour commencer.";

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

    public DonneesBourse Copier()
    {
        DonneesBourse copie = new DonneesBourse
        {
            dernierMoisObserve = dernierMoisObserve,
            dernierMessage = dernierMessage,
            positions = new List<PositionBourse>()
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

        return copie;
    }
}
