using System;
using UnityEngine;

/// <summary>
/// Represente un placement a rendement fixe capitalise periodiquement.
/// </summary>
/// <remarks>
/// Cette classe n'est pas une classe mere des actions : une position boursiere
/// possede une quantite et un prix variable, donc un modele distinct.
/// </remarks>
[Serializable]
public class Investissement : IPatrimoine, IEvolutionMensuelle
{
    /// <summary>
    /// Capital place, exprime en centimes.
    /// </summary>
    public argent sommeInvestie;

    /// <summary>
    /// Taux de rendement de la periode, ou 0.03 pour 3 %.
    /// </summary>
    [SerializeField] public float taux;

    [SerializeField] private int dureeMois;
    [SerializeField] private int moisEcoules;
    [SerializeField] private float benefices;

    /// <summary>
    /// Signale la capitalisation effective d'interets en centimes.
    /// </summary>
    public event Action<argent> OnBeneficesVerses;

    /// <summary>
    /// Cree un placement a rendement fixe.
    /// </summary>
    /// <param name="sommeInvestie">Capital initial en centimes.</param>
    /// <param name="taux">Taux de rendement de la periode.</param>
    /// <param name="dureeMois">Nombre de mois composant la periode.</param>
    public Investissement(argent sommeInvestie, float taux, int dureeMois)
    {
        if (sommeInvestie.centimes < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sommeInvestie),
                "Le capital investi ne peut pas etre negatif.");
        }

        if (dureeMois <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dureeMois),
                "La duree doit etre strictement positive.");
        }

        this.sommeInvestie = sommeInvestie;
        this.taux = taux;
        this.dureeMois = dureeMois;
        moisEcoules = 0;
        benefices = 0f;
    }

    private Investissement(
        argent sommeInvestie,
        float taux,
        int dureeMois,
        int moisEcoules,
        float benefices)
    {
        this.sommeInvestie = sommeInvestie;
        this.taux = taux;
        this.dureeMois = dureeMois;
        this.moisEcoules = moisEcoules;
        this.benefices = benefices;
    }

    /// <summary>
    /// Accumule les interets d'un mois et les capitalise en fin de periode.
    /// </summary>
    /// <remarks>
    /// Effet de bord : peut augmenter <see cref="sommeInvestie"/> et lever
    /// <see cref="OnBeneficesVerses"/>.
    /// </remarks>
    public void ComposerBenefices()
    {
        AccumulerBeneficesMensuels();
        if (moisEcoules >= dureeMois)
        {
            VerserBeneficesAccumules();
        }
    }

    /// <summary>
    /// Accumule un mois d'interets sans les verser au capital.
    /// </summary>
    public void AccumulerBeneficesMensuels()
    {
        // Le taux porte sur toute la periode. La division mensuelle evite
        // d'appliquer le taux complet a chaque passage de mois.
        benefices += sommeInvestie.centimes * (taux / dureeMois);
        moisEcoules++;
    }

    /// <summary>
    /// Capitalise les interets accumules et remet le compteur de periode a zero.
    /// </summary>
    /// <returns>Interets verses en centimes.</returns>
    public argent VerserBeneficesAccumules()
    {
        int beneficesCentimes = Mathf.RoundToInt(benefices);
        argent versement = new argent(Mathf.Max(0, beneficesCentimes));
        sommeInvestie += versement;
        benefices = 0f;
        moisEcoules = 0;

        if (versement.centimes > 0)
        {
            OnBeneficesVerses?.Invoke(versement);
        }

        return versement;
    }

    /// <summary>
    /// Remplace le taux de rendement de la periode.
    /// </summary>
    public void DefinirTaux(float nouveauTaux)
    {
        taux = Mathf.Max(0f, nouveauTaux);
    }

    /// <inheritdoc />
    public void AppliquerEvolutionMensuelle(int mois)
    {
        ComposerBenefices();
    }

    /// <inheritdoc />
    public argent GetValeurPatrimoine()
    {
        return sommeInvestie;
    }

    /// <summary>
    /// Produit une copie profonde incluant les interets encore non verses.
    /// </summary>
    public Investissement Copier()
    {
        return new Investissement(
            new argent(sommeInvestie.centimes),
            taux,
            dureeMois,
            moisEcoules,
            benefices);
    }
}
