using TMPro;
using UnityEngine;

public class EntrepreneuriatUI : MonoBehaviour
{
    private const int MaximumStat = 100;
    private const int InjectionCentimes = 100000;

    [Header("Donnees partagees")]
    [SerializeField] private GameData gameData;

    [Header("Affichage")]
    [SerializeField] private TMP_Text cashText;
    [SerializeField] private TMP_Text energieText;
    [SerializeField] private TMP_Text santeMentaleText;
    [SerializeField] private TMP_Text progressionProduitText;
    [SerializeField] private TMP_Text tractionMarcheText;
    [SerializeField] private TMP_Text reputationText;
    [SerializeField] private TMP_Text valorisationText;
    [SerializeField] private TMP_Text retourText;

    [Header("Etat de l'entreprise")]
    [SerializeField, Range(0, MaximumStat)] private int progressionProduit;
    [SerializeField, Range(0, MaximumStat)] private int tractionMarche;
    [SerializeField, Range(0, MaximumStat)] private int reputation = 5;
    [SerializeField] private string retourJoueur = "Choisissez une action pour développer votre entreprise.";

    private CompteBanquaire compteCourant;
    private HUDManager hudManager;
    private bool ecouteSoldeActive;

    private void Awake()
    {
        TrouverCompteCourant();
        hudManager = FindFirstObjectByType<HUDManager>();
        BornerValeurs();
    }

    private void OnEnable()
    {
        TrouverCompteCourant();
        AbonnerSolde();
        ActualiserAffichage();
    }

    private void OnDisable()
    {
        DesabonnerSolde();
    }

    public void DevelopperProduit()
    {
        if (!ConsommerRessources(15, 5))
        {
            return;
        }

        progressionProduit += 15;
        tractionMarche += progressionProduit >= 50 ? 4 : 1;
        reputation += 2;
        TerminerAction("Le produit progresse. Votre équipe tient une nouvelle version.");
    }

    public void EtudierMarche()
    {
        if (!ConsommerRessources(10, 3))
        {
            return;
        }

        tractionMarche += 12;
        reputation += 3;
        TerminerAction("L'étude révèle des attentes clients plus précises.");
    }

    public void InjecterMilleEuros()
    {
        if (!DebiterCompte(InjectionCentimes, "Injection dans l'entreprise"))
        {
            return;
        }

        progressionProduit += 10;
        tractionMarche += 4;
        reputation += 2;
        TerminerAction("1 000 € ont été injectés dans le développement.");
    }

    public void PitcherInvestisseurs()
    {
        if (progressionProduit < 30 || tractionMarche < 15)
        {
            TerminerAction("Le pitch est prématuré : visez 30 % de produit et 15 % de traction.");
            return;
        }

        if (!ConsommerRessources(20, 10))
        {
            return;
        }

        if (!TrouverCompteCourant())
        {
            TerminerAction("Le compte courant est indisponible.");
            return;
        }

        int leveeCentimes = 200000 + (reputation * 10000) + (tractionMarche * 5000);
        compteCourant.AjoutHistorique("Levée de fonds", new argent(leveeCentimes));
        tractionMarche += 5;
        reputation += 8;
        TerminerAction("Pitch réussi : " + new argent(leveeCentimes) + " levés.");
    }

    public void Pivoter()
    {
        if (progressionProduit == 0 && tractionMarche == 0)
        {
            TerminerAction("Construisez ou étudiez le marché avant de pivoter.");
            return;
        }

        if (!ConsommerRessources(15, 12))
        {
            return;
        }

        progressionProduit = Mathf.Max(10, progressionProduit - 10);
        tractionMarche += 12;
        reputation += 2;
        TerminerAction("Le positionnement change : moins de produit, mais un marché mieux ciblé.");
    }

    public void ReposerFondateur()
    {
        if (gameData == null)
        {
            TerminerAction("Les données de jeu sont indisponibles.");
            return;
        }

        gameData.joueur.energie += 30;
        gameData.joueur.santeMentale += 25;
        TerminerAction("Le fondateur récupère de l'énergie et de la clarté.");
    }

    public void ActualiserAffichage()
    {
        TrouverCompteCourant();

        if (cashText != null)
        {
            cashText.text = "Cash disponible : " +
                (compteCourant != null ? compteCourant.GetSolde().ToString() : "indisponible");
        }

        if (energieText != null)
        {
            energieText.text = "Énergie : " + (gameData != null && gameData.joueur != null ? gameData.joueur.energie + "/100" : "indisponible");
        }

        if (santeMentaleText != null)
        {
            santeMentaleText.text = "Santé mentale : " +
                (gameData != null && gameData.joueur != null ? gameData.joueur.santeMentale + "/100" : "indisponible");
        }

        if (progressionProduitText != null)
        {
            progressionProduitText.text = "Progression produit : " + progressionProduit + " %";
        }

        if (tractionMarcheText != null)
        {
            tractionMarcheText.text = "Traction marché : " + tractionMarche + " %";
        }

        if (reputationText != null)
        {
            reputationText.text = "Réputation : " + reputation + " %";
        }

        if (valorisationText != null)
        {
            valorisationText.text = "Valorisation estimée : " +
                new argent(CalculerValorisationCentimes()).ToString();
        }

        if (retourText != null)
        {
            retourText.text = retourJoueur;
        }
    }

    private bool ConsommerRessources(int energieNecessaire, int santeMentaleNecessaire)
    {
        if (gameData == null)
        {
            TerminerAction("Les données de jeu sont indisponibles.");
            return false;
        }

        if (gameData.joueur.energie < energieNecessaire)
        {
            TerminerAction("Énergie insuffisante pour cette action.");
            return false;
        }

        if (gameData.joueur.santeMentale < santeMentaleNecessaire)
        {
            TerminerAction("Santé mentale insuffisante pour cette action.");
            return false;
        }

        gameData.joueur.energie -= energieNecessaire;
        gameData.joueur.santeMentale -= santeMentaleNecessaire;
        return true;
    }

    private bool DebiterCompte(int montantCentimes, string libelle)
    {
        if (!TrouverCompteCourant())
        {
            TerminerAction("Le compte courant est indisponible.");
            return false;
        }

        if (compteCourant.GetSolde().centimes < montantCentimes)
        {
            TerminerAction("Cash insuffisant pour injecter 1 000 €.");
            return false;
        }

        compteCourant.AjoutHistorique(libelle, new argent(-montantCentimes));
        return true;
    }

    private bool TrouverCompteCourant()
    {
        if (compteCourant != null)
        {
            return true;
        }

        if (gameData == null || gameData.joueur == null || gameData.joueur.comptes == null)
        {
            return false;
        }

        return gameData.joueur.comptes.TryGetValue("courant", out compteCourant) && compteCourant != null;
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

    private void TerminerAction(string message)
    {
        retourJoueur = message;
        BornerValeurs();
        ActualiserAffichage();

        if (hudManager == null)
        {
            hudManager = FindFirstObjectByType<HUDManager>();
        }

        if (hudManager != null)
        {
            hudManager.ActualiserAffichage();
        }
    }

    private void BornerValeurs()
    {
        progressionProduit = Mathf.Clamp(progressionProduit, 0, MaximumStat);
        tractionMarche = Mathf.Clamp(tractionMarche, 0, MaximumStat);
        reputation = Mathf.Clamp(reputation, 0, MaximumStat);

        if (gameData != null && gameData.joueur != null)
        {
            gameData.joueur.energie = Mathf.Clamp(gameData.joueur.energie, 0, MaximumStat);
            gameData.joueur.santeMentale = Mathf.Clamp(gameData.joueur.santeMentale, 0, MaximumStat);
        }
    }

    private int CalculerValorisationCentimes()
    {
        int valorisationEuros = 10000 +
            (progressionProduit * 350) +
            (tractionMarche * 500) +
            (reputation * 250);

        return valorisationEuros * 100;
    }
}
