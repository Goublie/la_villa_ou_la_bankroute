using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Facade Unity de l'application Bourse.
/// </summary>
/// <remarks>
/// Cette classe conserve les references Inspector, le formatage et XCharts.
/// Les ordres, positions, couts moyens et valorisations sont delegues a
/// <see cref="ServiceBourse"/>.
/// </remarks>
public class BourseUI : MonoBehaviour
{
    private enum ModeVente
    {
        Montant,
        Quantite
    }

    // Conservee dans l'UI pour ne pas invalider la valeur serialisee du prefab.
    private enum CategorieActif
    {
        Indices,
        Actions,
        Crypto,
        Energie,
        Defensif
    }

    private static readonly int[] MontantsOrdreCentimes =
    {
        10000,
        25000,
        50000,
        100000,
        250000
    };

    private const int MontantMinimumEuros =
        ServiceBourse.MontantMinimumOrdreCentimes / 100;
    private const int MontantParDefautEuros = 500;

    [Header("Donnees partagees")]
    [SerializeField] private GameData gameData;

    [Header("Selection et marche")]
    [SerializeField] private TMP_Text categorieText;
    [SerializeField] private TMP_Text actifText;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private TMP_Text historiqueText;
    [SerializeField] private RectTransform graphiqueRoot;

    [Header("Ordre et portefeuille")]
    [SerializeField] private TMP_Text cashText;
    [SerializeField] private TMP_Text ordreText;
    [SerializeField] private TMP_Text portefeuilleText;
    [SerializeField] private TMP_Text journalText;
    [SerializeField] private TMP_Text modeVenteText;
    [SerializeField] private TMP_InputField montantInput;

    [Header("Etat de l'interface")]
    [SerializeField] private CategorieActif categorieSelectionnee;
    [SerializeField] private int indexActifSelectionne;
    [SerializeField] private int indexMontantOrdre = 2;
    [SerializeField] private ModeVente modeVente;

    private readonly List<DefinitionActifFinancier> actifs =
        new List<DefinitionActifFinancier>();
    private readonly List<DefinitionActifFinancier> actifsCategorie =
        new List<DefinitionActifFinancier>();

    private CompteBanquaire compteCourant;
    private ServiceBanque serviceBanque;
    private ServiceBourse serviceBourse;
    private DonneesBourse donneesBourse;
    private HUDManager hudManager;
    private ActionPlay actionPlay;
    private GraphiqueBourseUI graphique;
    private bool ecouteSoldeActive;

    private void Awake()
    {
        ResoudreDependances();
        ChargerActifs();
        InitialiserSaisieMontant();
    }

    private void OnEnable()
    {
        ResoudreDependances();
        ChargerActifs();
        AbonnerSolde();
        ActionPlay.OnMoisPasse += MettreAJourMarchePourNouveauMois;
        VerifierMoisObserve();
        ActualiserAffichage();
    }

    private void OnDisable()
    {
        ActionPlay.OnMoisPasse -= MettreAJourMarchePourNouveauMois;
        DesabonnerSolde();
    }

    /// <summary>
    /// Selectionne le prochain montant d'ordre predefini.
    /// </summary>
    public void MontantSuivant()
    {
        modeVente = ModeVente.Montant;
        indexMontantOrdre =
            (indexMontantOrdre + 1) % MontantsOrdreCentimes.Length;
        if (montantInput != null)
        {
            montantInput.SetTextWithoutNotify(
                (MontantsOrdreCentimes[indexMontantOrdre] / 100)
                .ToString());
        }

        ActualiserAffichage();
    }

    /// <summary>
    /// Bascule la vente entre montant en euros et quantite entiere.
    /// </summary>
    public void BasculerModeVente()
    {
        modeVente = modeVente == ModeVente.Montant
            ? ModeVente.Quantite
            : ModeVente.Montant;
        if (montantInput != null)
        {
            montantInput.SetTextWithoutNotify(
                modeVente == ModeVente.Montant
                    ? MontantParDefautEuros.ToString()
                    : "1");
        }

        ActualiserAffichage();
    }

    public void SelectionnerCAC40() => SelectionnerActif("cac40");
    public void SelectionnerNvidia() => SelectionnerActif("nvidia");
    public void SelectionnerAlphabet() => SelectionnerActif("alphabet");
    public void SelectionnerBitcoin() => SelectionnerActif("bitcoin");
    public void SelectionnerTotalEnergies() =>
        SelectionnerActif("totalenergies");
    public void SelectionnerLivretA() => SelectionnerActif("livret_a");

    /// <summary>
    /// Transmet un ordre d'achat par montant au service Bourse.
    /// </summary>
    public void Acheter()
    {
        if (modeVente == ModeVente.Quantite)
        {
            AfficherResultat(
                ResultatOperation.Echec(
                    "Pour acheter, utilisez le mode montant en euros.",
                    "mode_achat_invalide"));
            return;
        }

        if (!EssayerLireMontantOrdre(
                out int montantCentimes,
                out string erreur))
        {
            AfficherResultat(
                ResultatOperation.Echec(erreur, "saisie_invalide"));
            return;
        }

        if (!ResoudreServices())
        {
            AfficherResultat(
                ResultatOperation.Echec(
                    "Les donnees de marche sont indisponibles.",
                    "donnees_absentes"));
            return;
        }

        ResultatOperation resultat = serviceBourse.Acheter(
            ActifSelectionne,
            montantCentimes,
            MoisActuel,
            compteCourant,
            serviceBanque);

        // Baisse l'énergie et la santé mentale seulement si l'ordre a réussi
        if (resultat.Succes && gameData != null && gameData.joueur != null)
        {
            // Restreint la valeur entre 0 et 100
            gameData.joueur.energie = Mathf.Clamp(gameData.joueur.energie - 2, 0, 100);
            gameData.joueur.santeMentale = Mathf.Clamp(gameData.joueur.santeMentale - 2, 0, 100);
        }

        AfficherResultat(resultat);
    }

    /// <summary>
    /// Transmet une vente partielle par montant ou quantite.
    /// </summary>
    public void Vendre()
    {
        if (!ResoudreServices())
        {
            return;
        }

        DefinitionActifFinancier actif = ActifVenteSelectionne;
        if (actif == null)
        {
            AfficherResultat(
                ResultatOperation.Echec(
                    "Selectionnez un actif reellement detenu.",
                    "position_absente"));
            return;
        }

        ResultatOperation resultat;
        if (modeVente == ModeVente.Quantite)
        {
            if (!EssayerLireEntierPositif(
                    "quantite",
                    out int quantite,
                    out string erreur))
            {
                resultat = ResultatOperation.Echec(
                    erreur,
                    "saisie_invalide");
            }
            else
            {
                resultat = serviceBourse.VendreQuantite(
                    actif,
                    quantite,
                    MoisActuel,
                    compteCourant,
                    serviceBanque);
            }
        }
        else if (!EssayerLireMontantOrdre(
                     out int montantCentimes,
                     out string erreur))
        {
            resultat = ResultatOperation.Echec(
                erreur,
                "saisie_invalide");
        }
        else
        {
            resultat = serviceBourse.VendreMontant(
                actif,
                montantCentimes,
                MoisActuel,
                compteCourant,
                serviceBanque);
        }
        // AJOUTE LE BLOC if JUSTE AVANT AfficherResultat(resultat);
        if (resultat.Succes && gameData != null && gameData.joueur != null)
        {
            // Restreint la valeur entre 0 et 100
            gameData.joueur.energie = Mathf.Clamp(gameData.joueur.energie + 2, 0, 100);
            gameData.joueur.santeMentale = Mathf.Clamp(gameData.joueur.santeMentale + 2, 0, 100);
        }


        AfficherResultat(resultat);
    }

    /// <summary>
    /// Liquide la position de l'actif selectionne.
    /// </summary>
    public void ToutVendre()
    {
        if (!ResoudreServices())
        {
            return;
        }

        // 1. On stocke le résultat de l'opération
        ResultatOperation resultat = serviceBourse.ToutVendre(
            ActifVenteSelectionne,
            MoisActuel,
            compteCourant,
            serviceBanque);

        // 2. On vérifie le succès pour appliquer la fatigue et le stress
        if (resultat.Succes && gameData != null && gameData.joueur != null)
        {
            // Restreint la valeur entre 0 et 100
            gameData.joueur.energie = Mathf.Clamp(gameData.joueur.energie + 5, 0, 100);
            gameData.joueur.santeMentale = Mathf.Clamp(gameData.joueur.santeMentale + 5, 0, 100);
        }

        // 3. On affiche le résultat et on met à jour l'écran
        AfficherResultat(resultat);
    }

    /// <summary>
    /// Point d'entree conserve pour les anciens boutons ou scripts.
    /// </summary>
    public void UpdateMarketForNewMonth()
    {
        MettreAJourMarchePourNouveauMois();
    }

    /// <summary>
    /// Retourne la valeur patrimoniale deja calculee par le domaine.
    /// </summary>
    /// <remarks>
    /// Conserve pour compatibilite ; l'UI n'implemente plus IPatrimoine.
    /// </remarks>
    public argent GetValeurPatrimoine()
    {
        if (!ResoudreServices())
        {
            return new argent(0);
        }

        serviceBourse.MettreAJourValorisation(MoisActuel);
        return donneesBourse.GetValeurPatrimoine();
    }

    /// <summary>
    /// Rafraichit tous les composants visuels depuis l'etat metier.
    /// </summary>
    public void ActualiserAffichage()
    {
        ResoudreDependances();
        ChargerActifs();
        ActualiserListeCategorie();

        DefinitionActifFinancier actif = ActifSelectionne;
        PositionBourse position =
            actif != null && donneesBourse != null
                ? donneesBourse.TrouverPosition(actif.Id)
                : null;

        AffecterTexte(categorieText, "<b>ACTIFS DISPONIBLES</b>");
        AffecterTexte(
            actifText,
            actif != null
                ? "<mark=#DCE8FFFF>  Selection : " + actif.Nom +
                    "  |  " + NomCategorie(actif.Categorie) + "  </mark>"
                : "Aucun actif disponible");
        AffecterTexte(
            cashText,
            "Cash disponible : " +
            (compteCourant != null
                ? compteCourant.GetSolde().ToString()
                : "indisponible"));
        AffecterTexte(ordreText, ConstruireInstructionOrdre());
        AffecterTexte(
            modeVenteText,
            modeVente == ModeVente.Montant
                ? "Vente : montant"
                : "Vente : quantite");
        ActualiserIndicationSaisie();

        if (actif != null)
        {
            AffecterTexte(
                detailsText,
                ConstruireDetailsActif(actif, position));
            ActualiserGraphique(actif);
        }
        else
        {
            AffecterTexte(
                detailsText,
                "Aucune donnee disponible pour cette categorie.");
            AffecterTexte(historiqueText, string.Empty);
        }

        AffecterTexte(
            portefeuilleText,
            ConstruireResumePortefeuille());
        AffecterTexte(
            journalText,
            "<b>Journal du marche</b>\n" +
            (donneesBourse != null
                ? donneesBourse.dernierMessage
                : "Donnees indisponibles."));
    }

    private int MoisActuel =>
        gameData != null ? Mathf.Max(0, gameData.nombreMoisPasses) : 0;

    private DefinitionActifFinancier ActifSelectionne
    {
        get
        {
            ActualiserListeCategorie();
            if (actifsCategorie.Count == 0)
            {
                return null;
            }

            indexActifSelectionne = Mathf.Clamp(
                indexActifSelectionne,
                0,
                actifsCategorie.Count - 1);
            return actifsCategorie[indexActifSelectionne];
        }
    }

    private DefinitionActifFinancier ActifVenteSelectionne
    {
        get
        {
            DefinitionActifFinancier actif = ActifSelectionne;
            PositionBourse position =
                donneesBourse != null && actif != null
                    ? donneesBourse.TrouverPosition(actif.Id)
                    : null;
            return position != null && position.quantite > 0f
                ? actif
                : null;
        }
    }

    private void ChargerActifs()
    {
        if (actifs.Count > 0)
        {
            return;
        }

        actifs.AddRange(CatalogueActifs.ObtenirActifs());
    }

    private void ActualiserListeCategorie()
    {
        actifsCategorie.Clear();
        foreach (DefinitionActifFinancier actif in actifs)
        {
            if (ConvertirCategorie(actif.Categorie) ==
                categorieSelectionnee)
            {
                actifsCategorie.Add(actif);
            }
        }

        if (indexActifSelectionne >= actifsCategorie.Count)
        {
            indexActifSelectionne = 0;
        }
    }

    private string ConstruireDetailsActif(
        DefinitionActifFinancier actif,
        PositionBourse position)
    {
        if (serviceBourse == null)
        {
            return "Donnees indisponibles.";
        }

        float prix = serviceBourse.ObtenirPrix(actif, MoisActuel);
        int valeurPosition =
            serviceBourse.CalculerValeurPositionCentimes(
                position,
                MoisActuel);
        int plusValue =
            serviceBourse.CalculerGainPerteCentimes(
                position,
                MoisActuel);

        return
            "<b>" + actif.Nom + "</b>  |  " +
            NomCategorie(actif.Categorie) + "\n" +
            "Prix actuel : " + FormaterPrix(prix) +
            "   Variation mensuelle : " +
            FormaterPourcentage(
                serviceBourse.CalculerVariation(
                    actif,
                    MoisActuel,
                    1)) + "\n" +
            "Tendance annuelle : " +
            FormaterPourcentage(
                serviceBourse.CalculerVariation(
                    actif,
                    MoisActuel,
                    12)) +
            "   Rendement moyen : " +
            FormaterPourcentage(
                serviceBourse.CalculerRendementAnnualise(
                    actif,
                    MoisActuel)) + "\n" +
            "Risque : " + actif.NiveauRisque +
            "   Volatilite 12 mois : " +
            serviceBourse.CalculerVolatilite(
                actif,
                MoisActuel).ToString("F1") + " %\n" +
            actif.Description + "\n\n" +
            "Position : " +
            FormaterQuantite(position != null ? position.quantite : 0f) +
            "   Valeur : " + FormaterArgent(valeurPosition) +
            "   Plus-value : " + FormaterMontantSigne(plusValue);
    }

    private string ConstruireResumePortefeuille()
    {
        if (donneesBourse == null || serviceBourse == null)
        {
            return "Portefeuille indisponible.";
        }

        int valeurTotale =
            donneesBourse.GetValeurPatrimoine().centimes;
        int coutTotal =
            donneesBourse.CalculerCapitalInvestiCentimes();
        int performance =
            donneesBourse.GetGainsPertesLatents().centimes;
        float performancePourcent = coutTotal > 0
            ? performance * 100f / coutTotal
            : 0f;

        StringBuilder resultat = new StringBuilder();
        DefinitionActifFinancier actifVente = ActifVenteSelectionne;
        string actifVenteId =
            actifVente != null ? actifVente.Id : null;
        resultat.Append("<b>PORTEFEUILLE</b>\n")
            .Append("Valeur : ").Append(FormaterArgent(valeurTotale))
            .Append("   Investi : ").Append(FormaterArgent(coutTotal))
            .Append("\nP/L total : ")
            .Append(
                FormaterPerformanceColoree(
                    performance,
                    performancePourcent))
            .Append(
                "\n<size=16>----------------------------------------------------</size>");

        int positionsActives = 0;
        if (donneesBourse.positions != null)
        {
            foreach (PositionBourse position in donneesBourse.positions)
            {
                DefinitionActifFinancier actif =
                    position != null
                        ? CatalogueActifs.Trouver(position.actifId)
                        : null;
                if (position == null ||
                    actif == null ||
                    position.quantite <= 0f)
                {
                    continue;
                }

                positionsActives++;
                int valeurPosition =
                    serviceBourse.CalculerValeurPositionCentimes(
                        position,
                        MoisActuel);
                int gainPerte =
                    serviceBourse.CalculerGainPerteCentimes(
                        position,
                        MoisActuel);
                float gainPertePourcent =
                    serviceBourse.CalculerGainPertePourcent(
                        position,
                        MoisActuel);

                resultat.Append("\n")
                    .Append(
                        actif.Id == actifVenteId
                            ? "<color=#164EA6>[VENTE]</color> "
                            : string.Empty)
                    .Append("<b>").Append(actif.Nom).Append("</b>")
                    .Append("  |  Qte : ")
                    .Append(FormaterQuantite(position.quantite))
                    .Append("  |  Prix : ")
                    .Append(
                        FormaterPrix(
                            serviceBourse.ObtenirPrix(
                                actif,
                                MoisActuel)))
                    .Append("\nValeur : ")
                    .Append(FormaterArgent(valeurPosition))
                    .Append("  |  Investi : ")
                    .Append(
                        FormaterArgent(position.coutTotalCentimes))
                    .Append("  |  P/L : ")
                    .Append(
                        FormaterPerformanceColoree(
                            gainPerte,
                            gainPertePourcent));
            }
        }

        if (positionsActives == 0)
        {
            resultat.Append("\n\n<i>Aucune position ouverte.</i>");
        }

        return resultat.ToString();
    }

    private void ActualiserGraphique(DefinitionActifFinancier actif)
    {
        graphique = graphique ??
            new GraphiqueBourseUI(graphiqueRoot);
        bool affiche = graphique.Afficher(
            actif,
            serviceBourse,
            MoisActuel);
        AffecterTexte(
            historiqueText,
            affiche
                ? string.Empty
                : graphique.ConstruireTexteSecours(
                    actif,
                    serviceBourse,
                    MoisActuel));
    }

    private void MettreAJourMarchePourNouveauMois()
    {
        if (!ResoudreServices())
        {
            return;
        }

        serviceBourse.AppliquerEvolutionMensuelle(MoisActuel);
        ActualiserAffichage();
    }

    private void VerifierMoisObserve()
    {
        if (!ResoudreServices())
        {
            return;
        }

        if (donneesBourse.dernierMoisObserve != MoisActuel)
        {
            serviceBourse.AppliquerEvolutionMensuelle(MoisActuel);
        }
    }

    private void AfficherResultat(ResultatOperation resultat)
    {
        if (ResoudreServices())
        {
            serviceBourse.EnregistrerMessage(resultat.Message);
        }

        RafraichirHUD();
        ActualiserAffichage();
    }

    private bool ResoudreServices()
    {
        ResoudreDependances();
        return serviceBourse != null &&
            serviceBanque != null &&
            compteCourant != null;
    }

    private void ResoudreDependances()
    {
        if (hudManager == null)
        {
            hudManager = FindFirstObjectByType<HUDManager>();
        }

        if (gameData == null && hudManager != null)
        {
            gameData = hudManager.gameData;
        }

        if (gameData == null)
        {
            actionPlay = actionPlay != null
                ? actionPlay
                : FindFirstObjectByType<ActionPlay>();
            if (actionPlay != null)
            {
                gameData = actionPlay.gameData;
            }
        }

        if (gameData == null)
        {
            RemplacerCompteCourant(null);
            return;
        }

        if (gameData.joueur == null)
        {
            gameData.joueur = new DonneesJoueur();
        }

        gameData.joueur.InitialiserSiNecessaire();
        serviceBanque = new ServiceBanque(gameData.joueur);
        donneesBourse = gameData.joueur.bourse;
        serviceBourse = new ServiceBourse(donneesBourse);
        RemplacerCompteCourant(
            serviceBanque.ObtenirCompteCourant());
    }

    private void RemplacerCompteCourant(
        CompteBanquaire nouveauCompte)
    {
        if (ReferenceEquals(compteCourant, nouveauCompte))
        {
            return;
        }

        DesabonnerSolde();
        compteCourant = nouveauCompte;
        if (isActiveAndEnabled)
        {
            AbonnerSolde();
        }
    }

    private void AbonnerSolde()
    {
        if (!ecouteSoldeActive && compteCourant != null)
        {
            compteCourant.OnSoldeModifie += ActualiserAffichage;
            ecouteSoldeActive = true;
        }
    }

    private void DesabonnerSolde()
    {
        if (ecouteSoldeActive && compteCourant != null)
        {
            compteCourant.OnSoldeModifie -= ActualiserAffichage;
        }

        ecouteSoldeActive = false;
    }

    private void RafraichirHUD()
    {
        if (hudManager != null)
        {
            hudManager.ActualiserAffichage();
        }
    }

    private void InitialiserSaisieMontant()
    {
        if (montantInput == null)
        {
            return;
        }

        montantInput.contentType =
            TMP_InputField.ContentType.IntegerNumber;
        montantInput.characterValidation =
            TMP_InputField.CharacterValidation.Integer;
        montantInput.characterLimit = 8;
        if (string.IsNullOrWhiteSpace(montantInput.text))
        {
            montantInput.SetTextWithoutNotify(
                MontantParDefautEuros.ToString());
        }
    }

    private void ActualiserIndicationSaisie()
    {
        if (montantInput == null)
        {
            return;
        }

        TMP_Text indication = montantInput.placeholder as TMP_Text;
        if (indication != null)
        {
            indication.text = modeVente == ModeVente.Montant
                ? "Montant en EUR"
                : "Quantite entiere";
        }
    }

    private bool EssayerLireMontantOrdre(
        out int montantCentimes,
        out string erreur)
    {
        montantCentimes = 0;
        if (!EssayerLireEntierPositif(
                "montant",
                out int montantEuros,
                out erreur))
        {
            return false;
        }

        if (montantEuros < MontantMinimumEuros)
        {
            erreur =
                "Le montant minimum est de " +
                MontantMinimumEuros + " EUR.";
            return false;
        }

        if (montantEuros > int.MaxValue / 100)
        {
            erreur = "Le montant saisi est trop eleve.";
            return false;
        }

        montantCentimes = montantEuros * 100;
        return true;
    }

    private bool EssayerLireEntierPositif(
        string nomValeur,
        out int valeur,
        out string erreur)
    {
        valeur = 0;
        erreur = string.Empty;
        if (montantInput == null)
        {
            erreur = "Le champ de saisie est indisponible.";
            return false;
        }

        string texte = montantInput.text != null
            ? montantInput.text.Trim()
            : string.Empty;
        if (texte.Length == 0)
        {
            erreur =
                "Saisissez une valeur entiere positive.";
            return false;
        }

        foreach (char caractere in texte)
        {
            if (caractere < '0' || caractere > '9')
            {
                erreur =
                    "La " + nomValeur +
                    " doit etre un entier positif, sans decimales.";
                return false;
            }
        }

        if (!int.TryParse(texte, out valeur) || valeur <= 0)
        {
            erreur =
                "La " + nomValeur +
                " doit etre superieure a zero.";
            return false;
        }

        return true;
    }

    private void SelectionnerActif(string actifId)
    {
        DefinitionActifFinancier actif =
            CatalogueActifs.Trouver(actifId);
        if (actif == null)
        {
            AfficherResultat(
                ResultatOperation.Echec(
                    "Cet actif est indisponible.",
                    "actif_absent"));
            return;
        }

        categorieSelectionnee = ConvertirCategorie(actif.Categorie);
        ActualiserListeCategorie();
        indexActifSelectionne = actifsCategorie.IndexOf(actif);
        ActualiserAffichage();
    }

    private string ConstruireInstructionOrdre()
    {
        DefinitionActifFinancier actifVente =
            ActifVenteSelectionne;
        if (actifVente == null)
        {
            return
                "<mark=#FFF1B8FF>  Achat : montant EUR  |  " +
                "Vente : selectionnez une position detenue  </mark>";
        }

        return modeVente == ModeVente.Montant
            ? "<mark=#FFF1B8FF>  Achat / vente " +
                actifVente.Nom +
                " : montant entier (min. " +
                MontantMinimumEuros + " EUR)  </mark>"
            : "<mark=#FFF1B8FF>  Vente " +
                actifVente.Nom +
                " : quantite entiere  |  Achat : mode montant  </mark>";
    }

    private static CategorieActif ConvertirCategorie(
        CategorieActifFinancier categorie)
    {
        return (CategorieActif)(int)categorie;
    }

    private static string NomCategorie(
        CategorieActifFinancier categorie)
    {
        switch (categorie)
        {
            case CategorieActifFinancier.Indices:
                return "Indices";
            case CategorieActifFinancier.Actions:
                return "Actions tech";
            case CategorieActifFinancier.Crypto:
                return "Crypto";
            case CategorieActifFinancier.Energie:
                return "Energie";
            case CategorieActifFinancier.Defensif:
                return "Defensif";
            default:
                return categorie.ToString();
        }
    }

    private static string FormaterPerformanceColoree(
        int centimes,
        float pourcentage)
    {
        string couleur = centimes >= 0 ? "#187A2F" : "#B02020";
        return "<color=" + couleur + ">" +
            FormaterMontantSigne(centimes) +
            " (" + FormaterPourcentage(pourcentage) + ")</color>";
    }

    private static void AffecterTexte(TMP_Text cible, string valeur)
    {
        if (cible != null)
        {
            cible.text = valeur;
        }
    }

    private static string FormaterPrix(float euros)
    {
        return euros.ToString(euros >= 1000f ? "N0" : "N2") + " EUR";
    }

    private static string FormaterArgent(int centimes)
    {
        return new argent(centimes).ToString();
    }

    private static string FormaterQuantite(float quantite)
    {
        return quantite.ToString(quantite >= 10f ? "N2" : "N5");
    }

    private static string FormaterPourcentage(float pourcentage)
    {
        return (pourcentage >= 0f ? "+" : string.Empty) +
            pourcentage.ToString("F1") + " %";
    }

    private static string FormaterMontantSigne(int centimes)
    {
        return (centimes >= 0 ? "+" : string.Empty) +
            FormaterArgent(centimes);
    }
}
