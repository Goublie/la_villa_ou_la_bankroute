using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BourseMarcheUI : MonoBehaviour
{
    [Header("Elements UI")]
    [SerializeField] private TableauScrollSelectable tableauActifs;
    [SerializeField] private RectTransform graphiqueRoot;
    [SerializeField] private TMP_Text recapActifText;
    [SerializeField] private TMP_InputField montantInput;
    [SerializeField] private TMP_Text cashText;

    private ServiceBourse serviceBourse;
    private DonneesBourse donneesBourse;
    private CompteBanquaire compteCourant;
    private ServiceBanque serviceBanque;
    private GameData gameData;
    private HUDManager hudManager;
    private ActionPlay actionPlay;
    private GraphiqueBourseUI graphique;
    private bool ecouteSoldeActive;

    private Dictionary<LigneSelectable, DefinitionActifFinancier> mapActifs = 
        new Dictionary<LigneSelectable, DefinitionActifFinancier>();

    private DefinitionActifFinancier actifSelectionne;

    private void Awake()
    {
        InitialiserSaisieMontant();
        if (tableauActifs != null)
        {
            tableauActifs.OnSelectionChanged.AddListener(SurSelectionActif);
        }
    }

    private void OnEnable()
    {
        ResoudreDependances();
        AbonnerSolde();
        ActionPlay.OnMoisPasse += MettreAJourMarchePourNouveauMois;
        VerifierMoisObserve();
        
        RemplirTableau();
        ActualiserAffichage();
        
        if (tableauActifs != null)
        {
            tableauActifs.CheckAutoSelection();
        }
    }

    private void OnDisable()
    {
        ActionPlay.OnMoisPasse -= MettreAJourMarchePourNouveauMois;
        DesabonnerSolde();
    }

    private void SurSelectionActif(LigneSelectable ligne)
    {
        if (ligne != null && mapActifs.TryGetValue(ligne, out var actif))
        {
            actifSelectionne = actif;
            ActualiserAffichage();
        }
    }

    public void Acheter()
    {
        if (!EssayerLireMontantOrdre(out int montantCentimes, out string erreur))
        {
            AfficherResultat(ResultatOperation.Echec(erreur, "saisie_invalide"));
            return;
        }

        if (!ResoudreServices())
        {
            AfficherResultat(ResultatOperation.Echec("Les données de marché sont indisponibles.", "donnees_absentes"));
            return;
        }

        if (actifSelectionne == null)
        {
            AfficherResultat(ResultatOperation.Echec("Veuillez sélectionner un actif à acheter.", "actif_non_selectionne"));
            return;
        }

        AfficherResultat(serviceBourse.Acheter(
            actifSelectionne,
            montantCentimes,
            MoisActuel,
            compteCourant,
            serviceBanque));
    }

    private void ActualiserAffichage()
    {
        ResoudreDependances();

        if (cashText != null)
        {
            cashText.text = "Cash disponible : " + 
                (compteCourant != null ? compteCourant.GetSolde().ToString() : "indisponible");
        }

        if (actifSelectionne != null)
        {
            PositionBourse position = donneesBourse != null 
                ? donneesBourse.TrouverPosition(actifSelectionne.Id) 
                : null;
            
            if (recapActifText != null)
            {
                recapActifText.text = ConstruireDetailsActif(actifSelectionne, position);
            }
            
            ActualiserGraphique(actifSelectionne);
        }
        else
        {
            if (recapActifText != null)
            {
                recapActifText.text = "Aucun actif sélectionné.";
            }
        }
        
        // Rafraichir les prix dans le tableau (si les mois passent)
        RafraichirPrixTableau();
    }

    private void RemplirTableau()
    {
        if (tableauActifs == null || serviceBourse == null) return;

        tableauActifs.Vider();
        mapActifs.Clear();

        var actifs = CatalogueActifs.ObtenirActifs();
        foreach (var actif in actifs)
        {
            // Colonnes: Nom | Prix unitaire | Gain relatif | Gain brut
            var ligne = tableauActifs.AjouterEtRetournerLigne(
                actif.Nom,
                FormaterPrix(serviceBourse.ObtenirPrix(actif, MoisActuel)),
                FormaterPourcentage(serviceBourse.CalculerVariation(actif, MoisActuel, 1)),
                FormaterMontantSigne(Mathf.RoundToInt(serviceBourse.CalculerVariationAbsolue(actif, MoisActuel, 1) * 100))
            );

            if (ligne != null)
            {
                mapActifs[ligne] = actif;
            }
        }
    }
    
    private void RafraichirPrixTableau()
    {
        if (tableauActifs == null || serviceBourse == null) return;
        
        foreach (var kvp in mapActifs)
        {
            var ligne = kvp.Key;
            var actif = kvp.Value;
            // On met à jour la case 1 (Prix), la case 2 (Variation relative), et la case 3 (Variation absolue)
            ligne.Set(1, FormaterPrix(serviceBourse.ObtenirPrix(actif, MoisActuel)));
            ligne.Set(2, FormaterPourcentage(serviceBourse.CalculerVariation(actif, MoisActuel, 1)));
            ligne.Set(3, FormaterMontantSigne(Mathf.RoundToInt(serviceBourse.CalculerVariationAbsolue(actif, MoisActuel, 1) * 100)));
        }
    }

    private string ConstruireDetailsActif(DefinitionActifFinancier actif, PositionBourse position)
    {
        if (serviceBourse == null) return "Données indisponibles.";

        float prix = serviceBourse.ObtenirPrix(actif, MoisActuel);
        int valeurPosition = serviceBourse.CalculerValeurPositionCentimes(position, MoisActuel);
        int plusValue = serviceBourse.CalculerGainPerteCentimes(position, MoisActuel);

        return
            "<b>" + actif.Nom + "</b>  |  " + NomCategorie(actif.Categorie.ToString()) + "\n" +
            "Prix actuel : " + FormaterPrix(prix) +
            "   Variation mensuelle : " + FormaterPourcentage(serviceBourse.CalculerVariation(actif, MoisActuel, 1)) + "\n" +
            "Tendance annuelle : " + FormaterPourcentage(serviceBourse.CalculerVariation(actif, MoisActuel, 12)) +
            "   Rendement moyen : " + FormaterPourcentage(serviceBourse.CalculerRendementAnnualise(actif, MoisActuel)) + "\n" +
            "Risque : " + actif.NiveauRisque +
            "   Volatilité 12 mois : " + serviceBourse.CalculerVolatilite(actif, MoisActuel).ToString("F1") + " %\n" +
            actif.Description + "\n\n" +
            "Position : " + FormaterQuantite(position != null ? position.quantite : 0f) +
            "   Valeur : " + FormaterArgent(valeurPosition) +
            "   Plus-value : " + FormaterMontantSigne(plusValue);
    }

    private void ActualiserGraphique(DefinitionActifFinancier actif)
    {
        if (graphiqueRoot == null) return;
        
        graphique = graphique ?? new GraphiqueBourseUI(graphiqueRoot);
        graphique.Afficher(actif, serviceBourse, MoisActuel);
    }

    private int MoisActuel => gameData != null ? Mathf.Max(0, gameData.nombreMoisPasses) : 0;

    private void MettreAJourMarchePourNouveauMois()
    {
        if (!ResoudreServices()) return;
        serviceBourse.AppliquerEvolutionMensuelle(MoisActuel);
        ActualiserAffichage();
    }

    private void VerifierMoisObserve()
    {
        if (!ResoudreServices()) return;
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
        return serviceBourse != null && serviceBanque != null && compteCourant != null;
    }

    private void ResoudreDependances()
    {
        if (hudManager == null) hudManager = FindFirstObjectByType<HUDManager>();
        if (gameData == null && hudManager != null) gameData = hudManager.gameData;

        if (gameData == null)
        {
            actionPlay = actionPlay != null ? actionPlay : FindFirstObjectByType<ActionPlay>();
            if (actionPlay != null) gameData = actionPlay.gameData;
        }

        if (gameData == null)
        {
            RemplacerCompteCourant(null);
            return;
        }

        if (gameData.joueur == null) gameData.joueur = new DonneesJoueur();
        gameData.joueur.InitialiserSiNecessaire();

        serviceBanque = new ServiceBanque(gameData.joueur);
        donneesBourse = gameData.joueur.bourse;
        serviceBourse = new ServiceBourse(donneesBourse, gameData != null ? gameData.joueur : null);

        RemplacerCompteCourant(serviceBanque.ObtenirCompteCourant());
    }

    private void RemplacerCompteCourant(CompteBanquaire nouveauCompte)
    {
        if (ReferenceEquals(compteCourant, nouveauCompte)) return;

        DesabonnerSolde();
        compteCourant = nouveauCompte;
        if (isActiveAndEnabled) AbonnerSolde();
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
        if (hudManager != null) hudManager.ActualiserAffichage();
    }

    private void InitialiserSaisieMontant()
    {
        if (montantInput == null) return;
        montantInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        montantInput.characterValidation = TMP_InputField.CharacterValidation.Integer;
        montantInput.characterLimit = 8;
        if (string.IsNullOrWhiteSpace(montantInput.text))
        {
            montantInput.SetTextWithoutNotify("500");
        }
    }

    private bool EssayerLireMontantOrdre(out int montantCentimes, out string erreur)
    {
        montantCentimes = 0;
        erreur = string.Empty;

        if (montantInput == null)
        {
            erreur = "Le champ de saisie est indisponible.";
            return false;
        }

        string texte = montantInput.text != null ? montantInput.text.Trim() : string.Empty;
        if (!int.TryParse(texte, out int montantEuros))
        {
            erreur = "Veuillez saisir un montant valide.";
            return false;
        }

        if (montantEuros < 10)
        {
            erreur = "Le montant minimum est de 10 EUR.";
            return false;
        }

        if (montantEuros > int.MaxValue / 100)
        {
            erreur = "Le montant saisi est trop élevé.";
            return false;
        }

        montantCentimes = montantEuros * 100;
        return true;
    }

    private string NomCategorie(string cat)
    {
        switch (cat)
        {
            case "Indices": return "Indices boursiers";
            case "Actions": return "Actions d'entreprises";
            case "Crypto": return "Cryptomonnaies";
            case "Energie": return "Matières premières (Énergie)";
            case "Defensif": return "Actifs défensifs";
            default: return cat;
        }
    }

    private string FormaterPrix(float prix) => prix.ToString("N2") + " €";
    private string FormaterPourcentage(float p) => (p > 0 ? "+" : "") + p.ToString("N2") + " %";
    private string FormaterArgent(int centimes) => (centimes / 100f).ToString("N2") + " €";
    private string FormaterMontantSigne(int centimes)
    {
        string prefix = centimes > 0 ? "+" : "";
        string color = centimes > 0 ? "#2E8B57" : (centimes < 0 ? "#DC143C" : "#000000");
        return $"<color={color}>{prefix}{(centimes / 100f):N2} €</color>";
    }
    private string FormaterQuantite(float qte) => qte.ToString("N4");
}
