using System;
using System.Globalization;
using TMPro;
using UnityEngine;

/// <summary>
/// Adapte les controles Unity de l'onglet Livret A au service bancaire.
/// </summary>
public class EpargneUI : MonoBehaviour
{
    [SerializeField] private GameData G;
    [SerializeField] private TMP_InputField inputCredit;
    [SerializeField] private TMP_InputField inputDebit;
    [SerializeField] private TableauScroll tableauEpgn;
    [SerializeField] private TextMeshProUGUI texteSoldeEpgn;
    [SerializeField] private TextMeshProUGUI texteTaux;

    private Epargne epgn;
    private ServiceBanque serviceBanque;

    private void Awake()
    {
        ResoudreEtat();
    }

    private void Start()
    {
        ActualiserAffichage();
    }

    private void OnEnable()
    {
        ActionPlay.OnMoisPasse += ActualiserAffichage;
    }

    private void OnDisable()
    {
        ActionPlay.OnMoisPasse -= ActualiserAffichage;
    }

    /// <summary>
    /// Rafraichit le solde, le taux et l'historique visibles.
    /// </summary>
    public void ActualiserAffichage()
    {
        if (!ResoudreEtat())
        {
            return;
        }

        if (texteSoldeEpgn != null)
        {
            texteSoldeEpgn.text = "Solde : " + epgn.GetSolde();
        }

        if (texteTaux != null)
        {
            texteTaux.text =
                "Taux : " + (epgn.GetTaux() * 100f).ToString("F2") + " %";
        }

        ActualiserTableau();
    }

    /// <summary>
    /// Reconstruit le tableau des operations si sa reference est assignee.
    /// </summary>
    public void ActualiserTableau()
    {
        if (epgn == null || tableauEpgn == null)
        {
            return;
        }

        Historique historique = epgn.GetHistorique();
        tableauEpgn.Vider();
        for (int i = historique.GetSize() - 1; i >= 0; i--)
        {
            tableauEpgn.Add(historique.GetHistorique()[i]);
        }
    }

    /// <summary>
    /// Transfere le montant saisi du compte courant vers le Livret A.
    /// </summary>
    public void saisieCredit()
    {
        ExecuterTransfert(inputCredit, versEpargne: true);
    }

    /// <summary>
    /// Transfere le montant saisi du Livret A vers le compte courant.
    /// </summary>
    public void saisieDebit()
    {
        ExecuterTransfert(inputDebit, versEpargne: false);
    }

    private void ExecuterTransfert(
        TMP_InputField champ,
        bool versEpargne)
    {
        if (!ResoudreEtat() ||
            !EssayerLireMontant(champ, out argent montant))
        {
            return;
        }

        CompteBanquaire courant =
            serviceBanque.ObtenirCompteCourant();
        ResultatOperation resultat = versEpargne
            ? serviceBanque.Transferer(
                courant,
                epgn,
                montant,
                "courant vers epargne",
                "Credit")
            : serviceBanque.Transferer(
                epgn,
                courant,
                montant,
                "Debit",
                "Versement depuis le compte epargne");

        if (!resultat.Succes)
        {
            Debug.LogWarning("[Banque] " + resultat.Message);
        }

        ActualiserAffichage();
    }

    private bool ResoudreEtat()
    {
        if (G == null || G.joueur == null)
        {
            return false;
        }

        G.joueur.InitialiserSiNecessaire();
        serviceBanque = serviceBanque ?? new ServiceBanque(G.joueur);
        epgn = serviceBanque.ObtenirLivretA(G.nombreMoisPasses);
        return epgn != null;
    }

    private static bool EssayerLireMontant(
        TMP_InputField champ,
        out argent montant)
    {
        montant = new argent(0);
        if (champ == null || string.IsNullOrWhiteSpace(champ.text))
        {
            return false;
        }

        string saisie = champ.text.Trim();
        if (!decimal.TryParse(
                saisie,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out decimal euros) &&
            !decimal.TryParse(
                saisie,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out euros))
        {
            return false;
        }

        decimal centimes = decimal.Round(
            euros * 100m,
            0,
            MidpointRounding.AwayFromZero);
        if (centimes <= 0m || centimes > int.MaxValue)
        {
            return false;
        }

        montant = new argent((int)centimes);
        return true;
    }
}
