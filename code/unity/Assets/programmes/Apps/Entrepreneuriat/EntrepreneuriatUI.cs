using TMPro;
using UnityEngine;

/// <summary>
/// Facade Unity de l'application Entrepreneuriat.
/// </summary>
/// <remarks>
/// Les champs d'etat historiques sont conserves uniquement pour migrer les
/// valeurs deja serialisees dans le prefab. L'etat autoritaire vit ensuite
/// dans <see cref="DonneesEntrepreneuriat"/>.
/// </remarks>
public class EntrepreneuriatUI : MonoBehaviour
{
    private const int MaximumStat = 100;

    private enum Secteur
    {
        Finance,
        Sante,
        Education,
        Immobilier,
        Transport,
        Commerce,
        Divertissement,
        Cybersecurite,
        Energie,
        ReseauxSociaux
    }

    private enum PublicCible
    {
        Etudiants,
        JeunesActifs,
        Familles,
        Seniors,
        Entreprises,
        Independants,
        Investisseurs,
        Sportifs,
        CreateursContenu,
        GrandPublic
    }

    private enum Technologie
    {
        ApplicationMobile,
        PlateformeWeb,
        IntelligenceArtificielle,
        Blockchain,
        ObjetsConnectes,
        Marketplace,
        Saas,
        DataAnalyse,
        Automatisation,
        JeuSimulation
    }

    [Header("Donnees partagees")]
    [SerializeField] private GameData gameData;

    [Header("Affichage joueur et projet")]
    [SerializeField] private TMP_Text cashText;
    [SerializeField] private TMP_Text energieText;
    [SerializeField] private TMP_Text santeMentaleText;
    [SerializeField] private TMP_Text progressionProduitText;
    [SerializeField] private TMP_Text tractionMarcheText;
    [SerializeField] private TMP_Text reputationText;
    [SerializeField] private TMP_Text valorisationText;
    [SerializeField] private TMP_Text retourText;

    [Header("Migration de l'ancien etat du prefab")]
    [SerializeField] private Secteur secteurSelectionne;
    [SerializeField] private PublicCible publicSelectionne;
    [SerializeField] private Technologie technologieSelectionnee;
    [SerializeField] private bool projetCree;
    [SerializeField, Range(0, MaximumStat)] private int progressionProduit;
    [SerializeField, Range(0, MaximumStat)] private int tractionMarche;
    [SerializeField, Range(0, MaximumStat)] private int reputation = 5;
    [SerializeField, Range(0, MaximumStat)] private int connaissanceMarche;
    [SerializeField] private int tresorerieProjetCentimes;
    [SerializeField] private int nombrePivots;
    [SerializeField] private int bonusCompatibilite;
    [SerializeField] private string retourJoueur =
        "Choisissez un secteur, un public et une technologie.";

    private CompteBanquaire compteCourant;
    private ServiceBanque serviceBanque;
    private ServiceEntrepreneuriat service;
    private DonneesEntrepreneuriat donnees;
    private HUDManager hudManager;
    private ActionPlay actionPlay;
    private bool ecouteSoldeActive;
    private DropdownChoixEntrepreneuriat[] dropdownsChoix;

    private void Awake()
    {
        dropdownsChoix = GetComponentsInChildren<DropdownChoixEntrepreneuriat>(true);
        ResoudreDependances();
        MigrerEtatPrefab();
    }

    private void OnEnable()
    {
        ResoudreDependances();
        MigrerEtatPrefab();
        AbonnerSolde();
        ActionPlay.OnMoisPasse += ActualiserAffichage;
        ActualiserAffichage();
    }

    private void OnDisable()
    {
        ActionPlay.OnMoisPasse -= ActualiserAffichage;
        DesabonnerSolde();
    }

    public void ChoisirSecteurSuivant()
    {
        Executer(service?.ChoisirSecteurSuivant());
    }

    public void ChoisirPublicSuivant()
    {
        Executer(service?.ChoisirPublicSuivant());
    }

    public void ChoisirTechnologieSuivante()
    {
        Executer(service?.ChoisirTechnologieSuivante());
    }

    public void ActualiserDepuisDropdowns()
    {
        if (service == null || service.Projet.estCree || dropdownsChoix == null) return;
        
        foreach (var dd in dropdownsChoix)
        {
            if (dd.dropdown != null)
            {
                if (dd.typeChoix == TypeChoixDropdown.Secteur)
                    service.Projet.secteur = (SecteurEntrepreneurial)dd.ObtenirIndexSelectionne();
                else if (dd.typeChoix == TypeChoixDropdown.Public)
                    service.Projet.publicCible = (PublicEntrepreneurial)dd.ObtenirIndexSelectionne();
                else if (dd.typeChoix == TypeChoixDropdown.Technologie)
                    service.Projet.technologie = (TechnologieEntrepreneuriale)dd.ObtenirIndexSelectionne();
            }
        }
        
        ActualiserAffichage();
    }

    public void CreerProjet()
    {
        UnityEngine.Debug.Log(">> Bouton CreerProjet cliqué !");
        ActualiserDepuisDropdowns();
        UnityEngine.Debug.Log(">> ActualiserDepuisDropdowns termine.");
        var resultat = service?.CreerProjet();
        UnityEngine.Debug.Log(">> Resultat CreerProjet : " + (resultat.HasValue ? resultat.Value.Message : "NULL"));
        Executer(resultat);
        UnityEngine.Debug.Log(">> Fin de CreerProjet dans UI.");
    }

    public void DevelopperProduit()
    {
        Executer(service?.DevelopperProduit());
    }

    public void EtudierMarche()
    {
        Executer(service?.EtudierMarche());
    }

    public void InjecterMilleEuros()
    {
        Executer(service?.InjecterMilleEuros());
    }

    public void PitcherInvestisseurs()
    {
        Executer(service?.PitcherInvestisseurs());
    }

    public void Pivoter()
    {
        Executer(service?.Pivoter());
    }

    public void ReposerFondateur()
    {
        Executer(service?.ReposerFondateur());
    }

    /// <summary>
    /// Rafraichit les textes depuis les donnees persistantes.
    /// </summary>
    public void ActualiserAffichage()
    {
        if (!ResoudreDependances())
        {
            return;
        }

        ProjetEntrepreneurial projet = service.Projet;
        ProfilProjetEntrepreneurial profil =
            service.CalculerProfil();
        int probabilite = service.CalculerProbabiliteFinale();

        AffecterTexte(
            cashText,
            "Cash personnel : " +
            (compteCourant != null
                ? compteCourant.GetSolde().ToString()
                : "indisponible"));
        AffecterTexte(
            energieText,
            "Energie : " + gameData.joueur.energie + "/100");
        AffecterTexte(
            santeMentaleText,
            "Sante mentale : " +
            gameData.joueur.santeMentale + "/100");

        if (dropdownsChoix != null)
        {
            foreach(var dd in dropdownsChoix)
            {
                if (dd.dropdown != null)
                {
                    dd.dropdown.interactable = !projet.estCree;
                }
            }
        }

        if (projet.estCree)
        {
            AffecterTexte(
                progressionProduitText,
                "Progression produit : " +
                projet.progressionProduit + " %");
            AffecterTexte(
                tractionMarcheText,
                "Traction marche : " +
                projet.tractionMarche + " %");
            AffecterTexte(
                reputationText,
                "Reputation projet : " +
                projet.reputation + " %");
            AffecterTexte(
                valorisationText,
                "Reussite : " + probabilite +
                " % (base " + profil.ProbabiliteBase +
                " %)\nValorisation : " +
                FormaterArgent(
                    service.CalculerValorisationCentimes()));
        }
        else
        {
            AffecterTexte(progressionProduitText, "Progression produit : -- %");
            AffecterTexte(tractionMarcheText, "Traction marche : -- %");
            AffecterTexte(reputationText, "Reputation projet : -- %");
            AffecterTexte(valorisationText, "Reussite : -- %\nValorisation : --");
        }

        AffecterTexte(
            retourText,
            ConstruireDetailsProjet(projet, profil) +
            "\n\n<b>Journal</b>\n" +
            donnees.dernierMessage);
    }

    private void Executer(ResultatOperation? resultatNullable)
    {
        ResultatOperation resultat = resultatNullable ??
            ResultatOperation.Echec(
                "Les donnees du projet sont indisponibles.",
                "service_absent");

        if (donnees != null)
        {
            donnees.dernierMessage = resultat.Message;
        }

        service?.AppliquerEvolutionMensuelle(MoisActuel);
        ActualiserAffichage();
        hudManager?.ActualiserAffichage();
    }

    private string ConstruireDetailsProjet(
        ProjetEntrepreneurial projet,
        ProfilProjetEntrepreneurial profil)
    {
        string titre =
            projet.estCree
                ? "<b>PROJET ACTIF</b>"
                : "<b>PROJET A CREER</b>";
        string statut =
            projet.estCree
                ? service.CalculerStatutProjet()
                : "Selection";

        return
            "<b>Creez et developpez votre projet</b>\n" +
            titre + "\n" +
            service.ObtenirDefinitionSecteur().Nom + " | " +
            service.ObtenirDefinitionPublic().Nom + " | " +
            service.ObtenirDefinitionTechnologie().Nom + "\n" +
            "Statut : " + statut + "\n" +
            "Difficulte : " + profil.Difficulte +
            "/100   Potentiel : " +
            profil.PotentielMarche + "/100\n" +
            "Concurrence : " + profil.RisqueConcurrentiel +
            "/100   Compatibilite : " +
            profil.Compatibilite + "/100\n" +
            "Cout de lancement : " +
            FormaterArgent(profil.CoutLancementCentimes) + "\n" +
            "Tresorerie projet : " +
            FormaterArgent(projet.tresorerieCentimes) +
            "   Pivots : " + projet.nombrePivots;
    }

    private bool ResoudreDependances()
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
            return false;
        }

        if (gameData.joueur == null)
        {
            gameData.joueur = new DonneesJoueur();
        }

        gameData.joueur.InitialiserSiNecessaire();
        serviceBanque = new ServiceBanque(gameData.joueur);
        donnees = gameData.joueur.entrepreneuriat;
        RemplacerCompteCourant(
            serviceBanque.ObtenirCompteCourant());
        service = new ServiceEntrepreneuriat(
            donnees,
            gameData.joueur,
            compteCourant,
            serviceBanque);
        return true;
    }

    private void MigrerEtatPrefab()
    {
        if (!ResoudreDependances())
        {
            return;
        }

        service.MigrerDepuisPrefab(
            (SecteurEntrepreneurial)(int)secteurSelectionne,
            (PublicEntrepreneurial)(int)publicSelectionne,
            (TechnologieEntrepreneuriale)(int)technologieSelectionnee,
            projetCree,
            progressionProduit,
            tractionMarche,
            reputation,
            connaissanceMarche,
            tresorerieProjetCentimes,
            nombrePivots,
            bonusCompatibilite,
            retourJoueur);
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

    private int MoisActuel =>
        gameData != null ? Mathf.Max(0, gameData.nombreMoisPasses) : 0;

    private static void AffecterTexte(TMP_Text cible, string valeur)
    {
        if (cible != null)
        {
            cible.text = valeur;
        }
    }

    private static string FormaterArgent(int centimes)
    {
        return new argent(centimes).ToString();
    }
}
