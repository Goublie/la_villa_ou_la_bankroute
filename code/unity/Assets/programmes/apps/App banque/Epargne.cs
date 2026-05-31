using System;
using System.Collections.Generic;

public class Epargne : CompteBanquaire
{
    public Investissement invest;

    public Epargne(float _taux, int _duree) : base()
    {
        invest = new Investissement(solde, _taux, _duree);
        EcouterInterets();
    }

    public Epargne(Historique _historique, float _taux, int _duree) : base(_historique)
    {
        invest = new Investissement(solde, _taux, _duree);
        EcouterInterets();
    }

    private void EcouterInterets()
    {
        invest.OnBeneficesVerses += (interets) => base.AjoutHistorique("interets", interets);
    }

    public override void AjoutHistorique(string source, argent montant)
    {
        base.AjoutHistorique(source,montant);
        invest.sommeInvestie += montant;
    }

    ///////////////
    /// GETTERS ///
    ///////////////
    public float GetTaux()
    {
        return invest.taux;
    }
}