using System;
using UnityEngine;

[Serializable]
public class Investissement
{
    public argent sommeInvestie; //La somme investie initialement
    [SerializeField] public float taux; //Le taux de rendement sur la période 1=100%
    [SerializeField] int dureeMois; //La durée à laquele les intérets peuvent se cumuler
    [SerializeField] int moisEcoules; //Le nombre de mois écoulés depuis le début de l'investissement
    float benefices; //Les bénéfices accumulés, calculés chaque mois
    public event Action<argent> OnBeneficesVerses;

    public Investissement(argent sommeInvestie, float taux, int dureeMois)
    {
        if (sommeInvestie.centimes < 0)
        {
            Debug.Log("La somme investie doit être positive.");
            return;
        }
        if (dureeMois <= 0)
        {
            Debug.Log("La durée de l'investissement doit être positive.");
            return;
        }

        this.sommeInvestie = sommeInvestie;
        this.taux = taux;
        this.dureeMois = dureeMois;
        this.benefices = 0;
        this.moisEcoules = 0;
    }

    //Cacul les bénéfices, puis si la période d'investissement est écoulée, compose les bénéfices
    public void ComposerBenefices()
    {
        Debug.Log("Calcul benef");        
        // Calcul des bénéfices mensuels
        benefices += sommeInvestie.centimes * (taux / dureeMois);
        moisEcoules++;

        if (moisEcoules >= dureeMois)
        {
            // On arrondit pour éviter les erreurs de type float
            int benef = Mathf.RoundToInt(benefices);
            sommeInvestie.centimes += benef;


            // Réinitialisation
            OnBeneficesVerses?.Invoke(new argent(benef));

            benefices = 0;
            moisEcoules = 0; 
        }
    }
}