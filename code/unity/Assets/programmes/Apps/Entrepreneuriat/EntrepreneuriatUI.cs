using TMPro;
using UnityEngine;

public class EntrepreneuriatUI : MonoBehaviour
{
    private const int MaximumStat = 100;
    private const int InjectionCentimes = 100000;

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

    private readonly struct OptionProjet
    {
        public readonly string nom;
        public readonly int difficulte;
        public readonly int potentiel;
        public readonly int concurrence;
        public readonly int coutEuros;
        public readonly int probabilite;

        public OptionProjet(
            string nom,
            int difficulte,
            int potentiel,
            int concurrence,
            int coutEuros,
            int probabilite)
        {
            this.nom = nom;
            this.difficulte = difficulte;
            this.potentiel = potentiel;
            this.concurrence = concurrence;
            this.coutEuros = coutEuros;
            this.probabilite = probabilite;
        }
    }

    private struct ProfilProjet
    {
        public int difficulte;
        public int potentielMarche;
        public int risqueConcurrentiel;
        public int compatibilite;
        public int coutLancementCentimes;
        public int probabiliteBase;
        public int valorisationBaseCentimes;
    }

    private static readonly OptionProjet[] Secteurs =
    {
        new OptionProjet("Finance", 12, 14, 12, 2500, -3),
        new OptionProjet("Santé", 18, 18, 5, 4500, -6),
        new OptionProjet("Éducation", -6, 2, -4, -1000, 5),
        new OptionProjet("Immobilier", 7, 8, 4, 1800, -1),
        new OptionProjet("Transport", 14, 10, 5, 3500, -4),
        new OptionProjet("Commerce", 0, 6, 12, 0, 0),
        new OptionProjet("Divertissement", 2, 8, 10, 500, 0),
        new OptionProjet("Cybersécurité", 17, 16, 3, 3200, -4),
        new OptionProjet("Énergie", 20, 20, 3, 5500, -8),
        new OptionProjet("Réseaux sociaux", 4, 15, 24, 1000, -4)
    };

    private static readonly OptionProjet[] Publics =
    {
        new OptionProjet("Étudiants", -5, 0, 0, -700, 4),
        new OptionProjet("Jeunes actifs", 0, 8, 7, 0, 1),
        new OptionProjet("Familles", 3, 10, 5, 500, -1),
        new OptionProjet("Seniors", 5, 4, -2, 500, -1),
        new OptionProjet("Entreprises", 8, 13, -5, 1500, 2),
        new OptionProjet("Indépendants", 0, 2, -2, 0, 2),
        new OptionProjet("Investisseurs", 10, 11, 5, 1000, -3),
        new OptionProjet("Sportifs", 3, 5, 0, 400, 0),
        new OptionProjet("Créateurs de contenu", 2, 9, 8, 0, 0),
        new OptionProjet("Grand public", 8, 20, 18, 2200, -5)
    };

    private static readonly OptionProjet[] Technologies =
    {
        new OptionProjet("Application mobile", -4, 5, 8, -500, 4),
        new OptionProjet("Plateforme web", -2, 4, 5, -300, 3),
        new OptionProjet("Intelligence artificielle", 18, 18, 5, 4000, -7),
        new OptionProjet("Blockchain", 20, 15, 10, 4500, -9),
        new OptionProjet("Objets connectés", 16, 12, 3, 3800, -6),
        new OptionProjet("Marketplace", 7, 12, 18, 1500, -2),
        new OptionProjet("SaaS", 7, 13, 5, 1200, 1),
        new OptionProjet("Data analyse", 10, 12, 3, 2000, -2),
        new OptionProjet("Automatisation", 8, 10, 2, 1500, 0),
        new OptionProjet("Jeu vidéo / simulation", 10, 14, 14, 2500, -3)
    };

    [Header("Données partagées")]
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

    [Header("État du projet")]
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
        "Choisissez un secteur, un public et une technologie pour créer votre projet.";

    private CompteBanquaire compteCourant;
    private HUDManager hudManager;
    private ActionPlay actionPlay;
    private ProfilProjet profilActuel;
    private bool ecouteSoldeActive;

    private void Awake()
    {
        ResoudreDependances();
        TrouverCompteCourant();
        RecalculerProjet();
        BornerValeurs();
    }

    private void OnEnable()
    {
        ResoudreDependances();
        TrouverCompteCourant();
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
        if (!SelectionModifiable())
        {
            return;
        }

        secteurSelectionne = (Secteur)(((int)secteurSelectionne + 1) % Secteurs.Length);
        TerminerAction("Secteur sélectionné : " + OptionSecteur.nom + ".");
    }

    public void ChoisirPublicSuivant()
    {
        if (!SelectionModifiable())
        {
            return;
        }

        publicSelectionne = (PublicCible)(((int)publicSelectionne + 1) % Publics.Length);
        TerminerAction("Public ciblé : " + OptionPublic.nom + ".");
    }

    public void ChoisirTechnologieSuivante()
    {
        if (!SelectionModifiable())
        {
            return;
        }

        technologieSelectionnee =
            (Technologie)(((int)technologieSelectionnee + 1) % Technologies.Length);
        TerminerAction("Technologie sélectionnée : " + OptionTechnologie.nom + ".");
    }

    public void CreerProjet()
    {
        if (projetCree)
        {
            TerminerAction(
                "Le projet est déjà créé. Développez-le ou utilisez Pivoter pour l'ajuster.");
            return;
        }

        projetCree = true;
        progressionProduit = 5;
        tractionMarche = 0;
        reputation = 5;
        connaissanceMarche = 0;
        tresorerieProjetCentimes = 0;
        nombrePivots = 0;
        bonusCompatibilite = 0;
        RecalculerProjet();

        TerminerAction(
            "Projet créé : " + OptionSecteur.nom + ", pour " + OptionPublic.nom +
            ", avec " + OptionTechnologie.nom + ". Injectez des fonds ou lancez une première action.");
    }

    public void DevelopperProduit()
    {
        if (!VerifierProjetCree())
        {
            return;
        }

        int coutCentimes = (120 + (profilActuel.difficulte * 6)) * 100;
        int energieNecessaire = 10 + (profilActuel.difficulte / 15);
        int santeNecessaire = 4 + (profilActuel.difficulte / 30);

        if (!PeutConsommerRessources(energieNecessaire, santeNecessaire))
        {
            return;
        }

        if (!PayerDepenseProjet(coutCentimes, "Développement du produit"))
        {
            return;
        }

        ConsommerRessources(energieNecessaire, santeNecessaire);

        progressionProduit += Mathf.Clamp(20 - (profilActuel.difficulte / 10), 8, 16);
        reputation += progressionProduit >= 55 ? 3 : 1;
        TerminerAction(
            "Le prototype avance. Coût : " + FormaterArgent(coutCentimes) +
            ". La difficulté technique ralentit légèrement l'équipe.");
    }

    public void EtudierMarche()
    {
        if (!VerifierProjetCree())
        {
            return;
        }

        int coutCentimes = (180 + (profilActuel.risqueConcurrentiel * 4)) * 100;
        if (!PeutConsommerRessources(10, 4))
        {
            return;
        }

        if (!PayerDepenseProjet(coutCentimes, "Étude de marché"))
        {
            return;
        }

        ConsommerRessources(10, 4);

        connaissanceMarche += 15;
        tractionMarche += Mathf.Clamp(14 - (profilActuel.risqueConcurrentiel / 12), 6, 12);
        bonusCompatibilite += profilActuel.compatibilite < 60 ? 4 : 2;
        TerminerAction(
            "L'étude confirme une demande chez " + OptionPublic.nom.ToLowerInvariant() +
            ". Votre connaissance du marché progresse.");
    }

    public void InjecterMilleEuros()
    {
        if (!VerifierProjetCree())
        {
            return;
        }

        if (!DebiterCompte(InjectionCentimes, "Injection dans le projet"))
        {
            return;
        }

        tresorerieProjetCentimes += InjectionCentimes;
        TerminerAction(
            "1 000 € ont été transférés vers la trésorerie du projet. " +
            "Le risque de sous-financement diminue.");
    }

    public void PitcherInvestisseurs()
    {
        if (!VerifierProjetCree())
        {
            return;
        }

        if (progressionProduit < 20 || tractionMarche < 10)
        {
            TerminerAction(
                "Les investisseurs attendent au moins 20 % de produit et 10 % de traction.");
            return;
        }

        if (!ConsommerRessources(18, 10))
        {
            return;
        }

        int probabilitePitch = Mathf.Clamp(
            CalculerProbabiliteFinale() + (tractionMarche / 5) + (reputation / 8) - 8,
            5,
            90);

        if (Random.Range(0, 100) < probabilitePitch)
        {
            int leveeCentimes =
                400000 +
                (profilActuel.potentielMarche * 12000) +
                (tractionMarche * 10000) +
                (reputation * 5000);

            tresorerieProjetCentimes += leveeCentimes;
            reputation += 10;
            tractionMarche += 5;
            TerminerAction(
                "Pitch réussi : " + FormaterArgent(leveeCentimes) +
                " rejoignent la trésorerie du projet.");
            return;
        }

        DonneesJoueur joueur = ObtenirJoueur();
        if (joueur != null)
        {
            joueur.santeMentale -= 5;
        }

        reputation -= 3;
        TerminerAction(
            "Pitch refusé. Les investisseurs veulent davantage de traction et un dossier plus solide.");
    }

    public void Pivoter()
    {
        if (!VerifierProjetCree())
        {
            return;
        }

        if (!ConsommerRessources(14, 10))
        {
            return;
        }

        nombrePivots++;
        progressionProduit = Mathf.Max(5, progressionProduit - 10);
        connaissanceMarche += 8;
        bonusCompatibilite += profilActuel.compatibilite < 65 ? 10 : 4;
        tractionMarche += profilActuel.compatibilite < 65 ? 5 : 1;

        string avertissement = nombrePivots > 2
            ? " Attention : les pivots répétés commencent à inquiéter le marché."
            : string.Empty;

        TerminerAction(
            "Le positionnement est ajusté pour mieux relier " + OptionSecteur.nom +
            " et " + OptionPublic.nom.ToLowerInvariant() + "." + avertissement);
    }

    public void ReposerFondateur()
    {
        DonneesJoueur joueur = ObtenirJoueur();
        if (joueur == null)
        {
            TerminerAction("Les données du joueur sont indisponibles.");
            return;
        }

        joueur.energie += 30;
        joueur.santeMentale += 25;
        TerminerAction(
            "Le fondateur prend du recul. L'énergie et la santé mentale remontent, " +
            "sans progrès direct sur le projet.");
    }

    public void ActualiserAffichage()
    {
        ResoudreDependances();
        TrouverCompteCourant();
        RecalculerProjet();

        DonneesJoueur joueur = ObtenirJoueur();
        int probabiliteFinale = CalculerProbabiliteFinale();

        AffecterTexte(
            cashText,
            "Cash personnel : " +
            (compteCourant != null ? compteCourant.GetSolde().ToString() : "indisponible"));
        AffecterTexte(
            energieText,
            "Énergie : " + (joueur != null ? joueur.energie + "/100" : "indisponible"));
        AffecterTexte(
            santeMentaleText,
            "Santé mentale : " +
            (joueur != null ? joueur.santeMentale + "/100" : "indisponible"));

        if (projetCree)
        {
            AffecterTexte(
                progressionProduitText,
                "Progression produit : " + progressionProduit + " %");
            AffecterTexte(tractionMarcheText, "Traction marché : " + tractionMarche + " %");
            AffecterTexte(reputationText, "Réputation projet : " + reputation + " %");
            AffecterTexte(
                valorisationText,
                "Réussite : " + probabiliteFinale + " % (base " +
                profilActuel.probabiliteBase + " %)\nValorisation : " +
                FormaterArgent(CalculerValorisationCentimes()));
        }
        else
        {
            AffecterTexte(
                progressionProduitText,
                "<mark=#DCE8FFFF>  Secteur : " + OptionSecteur.nom + "  >  </mark>");
            AffecterTexte(
                tractionMarcheText,
                "<mark=#DCE8FFFF>  Public : " + OptionPublic.nom + "  >  </mark>");
            AffecterTexte(
                reputationText,
                "<mark=#DCE8FFFF>  Technologie : " + OptionTechnologie.nom + "  >  </mark>");
            AffecterTexte(
                valorisationText,
                "<mark=#2E8B57FF><color=#FFFFFF><b>  CRÉER LE PROJET  </b></color></mark>");
        }

        AffecterTexte(
            retourText,
            ConstruireDetailsProjet() + "\n\n<b>Journal</b>\n" + retourJoueur);
    }

    private OptionProjet OptionSecteur => Secteurs[(int)secteurSelectionne];
    private OptionProjet OptionPublic => Publics[(int)publicSelectionne];
    private OptionProjet OptionTechnologie => Technologies[(int)technologieSelectionnee];

    private bool SelectionModifiable()
    {
        if (!projetCree)
        {
            return true;
        }

        TerminerAction(
            "Le projet est déjà lancé. Utilisez Pivoter pour ajuster son positionnement.");
        return false;
    }

    private bool VerifierProjetCree()
    {
        if (projetCree)
        {
            return true;
        }

        TerminerAction("Créez d'abord un projet à partir des trois choix stratégiques.");
        return false;
    }

    private void RecalculerProjet()
    {
        profilActuel = CalculerProfilProjet();
    }

    private ProfilProjet CalculerProfilProjet()
    {
        ProfilProjet profil = new ProfilProjet
        {
            difficulte = 30,
            potentielMarche = 38,
            risqueConcurrentiel = 28,
            compatibilite = 50,
            coutLancementCentimes = 250000,
            probabiliteBase = 0,
            valorisationBaseCentimes = 0
        };

        AjouterFacteurs(ref profil, OptionSecteur);
        AjouterFacteurs(ref profil, OptionPublic);
        AjouterFacteurs(ref profil, OptionTechnologie);
        AppliquerSynergies(ref profil);

        profil.compatibilite += bonusCompatibilite;
        profil.difficulte = Mathf.Clamp(profil.difficulte, 10, 95);
        profil.potentielMarche = Mathf.Clamp(profil.potentielMarche, 15, 100);
        profil.risqueConcurrentiel = Mathf.Clamp(profil.risqueConcurrentiel, 10, 95);
        profil.compatibilite = Mathf.Clamp(profil.compatibilite, 15, 100);
        profil.coutLancementCentimes = Mathf.Max(100000, profil.coutLancementCentimes);

        int ajustementOptions =
            OptionSecteur.probabilite +
            OptionPublic.probabilite +
            OptionTechnologie.probabilite;

        profil.probabiliteBase = Mathf.Clamp(
            Mathf.RoundToInt(
                70 +
                ajustementOptions +
                ((profil.compatibilite - 50) * 0.3f) -
                (profil.difficulte * 0.28f) -
                (profil.risqueConcurrentiel * 0.1f)),
            8,
            85);

        profil.valorisationBaseCentimes =
            (profil.coutLancementCentimes * 2) +
            (profil.potentielMarche * 65000) +
            (profil.compatibilite * 30000);

        return profil;
    }

    private static void AjouterFacteurs(ref ProfilProjet profil, OptionProjet option)
    {
        profil.difficulte += option.difficulte;
        profil.potentielMarche += option.potentiel;
        profil.risqueConcurrentiel += option.concurrence;
        profil.coutLancementCentimes += option.coutEuros * 100;
    }

    private void AppliquerSynergies(ref ProfilProjet profil)
    {
        if (secteurSelectionne == Secteur.Sante &&
            publicSelectionne == PublicCible.Seniors)
        {
            profil.compatibilite += 14;
            profil.potentielMarche += 8;
        }

        if (secteurSelectionne == Secteur.Sante &&
            technologieSelectionnee == Technologie.IntelligenceArtificielle)
        {
            profil.compatibilite += 8;
            profil.difficulte += 8;
            profil.coutLancementCentimes += 250000;
        }

        if (secteurSelectionne == Secteur.Education &&
            publicSelectionne == PublicCible.Etudiants &&
            technologieSelectionnee == Technologie.ApplicationMobile)
        {
            profil.compatibilite += 22;
            profil.risqueConcurrentiel -= 4;
        }

        if (secteurSelectionne == Secteur.Cybersecurite &&
            publicSelectionne == PublicCible.Entreprises &&
            technologieSelectionnee == Technologie.Saas)
        {
            profil.compatibilite += 22;
            profil.potentielMarche += 10;
        }

        if (secteurSelectionne == Secteur.Finance &&
            publicSelectionne == PublicCible.Investisseurs &&
            technologieSelectionnee == Technologie.Blockchain)
        {
            profil.compatibilite += 18;
            profil.potentielMarche += 12;
            profil.risqueConcurrentiel += 8;
            profil.difficulte += 6;
        }

        if (secteurSelectionne == Secteur.Commerce &&
            publicSelectionne == PublicCible.GrandPublic &&
            technologieSelectionnee == Technologie.Marketplace)
        {
            profil.compatibilite += 16;
            profil.potentielMarche += 10;
            profil.risqueConcurrentiel += 12;
        }

        if (secteurSelectionne == Secteur.Divertissement &&
            publicSelectionne == PublicCible.JeunesActifs &&
            technologieSelectionnee == Technologie.JeuSimulation)
        {
            profil.compatibilite += 20;
            profil.potentielMarche += 10;
        }

        profil.compatibilite += CompatibiliteGenerale();
    }

    private int CompatibiliteGenerale()
    {
        int bonus = 0;

        if (publicSelectionne == PublicCible.Entreprises &&
            (technologieSelectionnee == Technologie.Saas ||
             technologieSelectionnee == Technologie.Automatisation ||
             technologieSelectionnee == Technologie.DataAnalyse))
        {
            bonus += 8;
        }

        if ((publicSelectionne == PublicCible.Etudiants ||
             publicSelectionne == PublicCible.JeunesActifs) &&
            (technologieSelectionnee == Technologie.ApplicationMobile ||
             technologieSelectionnee == Technologie.PlateformeWeb))
        {
            bonus += 7;
        }

        if (publicSelectionne == PublicCible.GrandPublic &&
            (technologieSelectionnee == Technologie.ApplicationMobile ||
             technologieSelectionnee == Technologie.Marketplace ||
             technologieSelectionnee == Technologie.JeuSimulation))
        {
            bonus += 6;
        }

        if ((secteurSelectionne == Secteur.Transport ||
             secteurSelectionne == Secteur.Energie ||
             secteurSelectionne == Secteur.Sante) &&
            technologieSelectionnee == Technologie.ObjetsConnectes)
        {
            bonus += 7;
        }

        return bonus;
    }

    private int CalculerProbabiliteFinale()
    {
        float probabilite = profilActuel.probabiliteBase;
        DonneesJoueur joueur = ObtenirJoueur();

        if (joueur != null)
        {
            probabilite += (joueur.energie - 50) * 0.12f;
            probabilite += (joueur.santeMentale - 50) * 0.12f;
        }

        int cashDisponible = compteCourant != null ? compteCourant.GetSolde().centimes : 0;
        float couverture = profilActuel.coutLancementCentimes > 0
            ? (cashDisponible + tresorerieProjetCentimes) /
              (float)profilActuel.coutLancementCentimes
            : 1f;

        probabilite += couverture >= 1f ? 8f : -18f * (1f - Mathf.Clamp01(couverture));
        probabilite += progressionProduit * 0.15f;
        probabilite += tractionMarche * 0.18f;
        probabilite += reputation * 0.08f;
        probabilite += connaissanceMarche * 0.08f;
        probabilite -= Mathf.Max(0, profilActuel.difficulte - 50) * 0.08f;
        probabilite -= Mathf.Max(0, nombrePivots - 1) * 4f;

        return Mathf.Clamp(Mathf.RoundToInt(probabilite), 1, 95);
    }

    private int CalculerValorisationCentimes()
    {
        if (!projetCree)
        {
            return profilActuel.valorisationBaseCentimes;
        }

        return
            profilActuel.valorisationBaseCentimes +
            tresorerieProjetCentimes +
            (progressionProduit * 50000) +
            (tractionMarche * 80000) +
            (reputation * 30000);
    }

    private string ConstruireDetailsProjet()
    {
        string titre = projetCree ? "<b>PROJET ACTIF</b>" : "<b>PROJET À CRÉER</b>";
        string statut = projetCree ? CalculerStatutProjet() : "Sélection";

        return
            "<b>Créez et développez votre projet</b>\n" +
            titre + "\n" +
            OptionSecteur.nom + " • " + OptionPublic.nom + " • " + OptionTechnologie.nom + "\n" +
            "Statut : " + statut + "\n" +
            "Difficulté : " + profilActuel.difficulte +
            "/100   Potentiel : " + profilActuel.potentielMarche + "/100\n" +
            "Concurrence : " + profilActuel.risqueConcurrentiel +
            "/100   Compatibilité : " + profilActuel.compatibilite + "/100\n" +
            "Coût de lancement : " + FormaterArgent(profilActuel.coutLancementCentimes) + "\n" +
            "Trésorerie projet : " + FormaterArgent(tresorerieProjetCentimes) +
            "   Pivots : " + nombrePivots;
    }

    private string CalculerStatutProjet()
    {
        if (progressionProduit < 25)
        {
            return "Idée";
        }

        if (progressionProduit < 60)
        {
            return "Prototype";
        }

        if (tractionMarche < 40)
        {
            return "Recherche de traction";
        }

        if (tresorerieProjetCentimes < 1000000)
        {
            return "Levée";
        }

        return "Croissance";
    }

    private bool PeutConsommerRessources(
        int energieNecessaire,
        int santeMentaleNecessaire)
    {
        DonneesJoueur joueur = ObtenirJoueur();
        if (joueur == null)
        {
            TerminerAction("Les données du joueur sont indisponibles.");
            return false;
        }

        if (joueur.energie < energieNecessaire)
        {
            TerminerAction("Énergie insuffisante pour cette action.");
            return false;
        }

        if (joueur.santeMentale < santeMentaleNecessaire)
        {
            TerminerAction("Santé mentale insuffisante pour cette action.");
            return false;
        }

        return true;
    }

    private bool ConsommerRessources(int energieNecessaire, int santeMentaleNecessaire)
    {
        if (!PeutConsommerRessources(energieNecessaire, santeMentaleNecessaire))
        {
            return false;
        }

        DonneesJoueur joueur = ObtenirJoueur();
        joueur.energie -= energieNecessaire;
        joueur.santeMentale -= santeMentaleNecessaire;
        return true;
    }

    private bool PayerDepenseProjet(int montantCentimes, string libelle)
    {
        int depuisTresorerie = Mathf.Min(tresorerieProjetCentimes, montantCentimes);
        int resteAPayer = montantCentimes - depuisTresorerie;

        if (resteAPayer > 0 && !PeutDebiterCompte(resteAPayer))
        {
            TerminerAction(
                "Financement insuffisant. Il manque " + FormaterArgent(resteAPayer) +
                " pour cette action.");
            return false;
        }

        tresorerieProjetCentimes -= depuisTresorerie;
        if (resteAPayer > 0)
        {
            compteCourant.AjoutHistorique(libelle, new argent(-resteAPayer));
        }

        return true;
    }

    private bool DebiterCompte(int montantCentimes, string libelle)
    {
        if (!PeutDebiterCompte(montantCentimes))
        {
            TerminerAction("Cash insuffisant pour injecter " + FormaterArgent(montantCentimes) + ".");
            return false;
        }

        compteCourant.AjoutHistorique(libelle, new argent(-montantCentimes));
        return true;
    }

    private bool PeutDebiterCompte(int montantCentimes)
    {
        return
            TrouverCompteCourant() &&
            compteCourant.GetSolde().centimes >= montantCentimes;
    }

    private bool TrouverCompteCourant()
    {
        DonneesJoueur joueur = ObtenirJoueur();
        if (joueur == null || joueur.comptes == null)
        {
            RemplacerCompteCourant(null);
            return false;
        }

        joueur.comptes.TryGetValue("courant", out CompteBanquaire compte);
        RemplacerCompteCourant(compte);
        return compteCourant != null;
    }

    private DonneesJoueur ObtenirJoueur()
    {
        ResoudreDependances();
        if (gameData == null)
        {
            return null;
        }

        if (gameData.joueur == null)
        {
            gameData.joueur = new DonneesJoueur();
        }

        return gameData.joueur;
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
            if (actionPlay == null)
            {
                actionPlay = FindFirstObjectByType<ActionPlay>();
            }

            if (actionPlay != null)
            {
                gameData = actionPlay.gameData;
            }
        }
    }

    private void RemplacerCompteCourant(CompteBanquaire nouveauCompte)
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

    private void TerminerAction(string message)
    {
        retourJoueur = message;
        BornerValeurs();
        RecalculerProjet();
        ActualiserAffichage();

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
        connaissanceMarche = Mathf.Clamp(connaissanceMarche, 0, MaximumStat);
        tresorerieProjetCentimes = Mathf.Max(0, tresorerieProjetCentimes);
        nombrePivots = Mathf.Max(0, nombrePivots);

        DonneesJoueur joueur = ObtenirJoueur();
        if (joueur != null)
        {
            joueur.energie = Mathf.Clamp(joueur.energie, 0, MaximumStat);
            joueur.santeMentale = Mathf.Clamp(joueur.santeMentale, 0, MaximumStat);
        }
    }

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
