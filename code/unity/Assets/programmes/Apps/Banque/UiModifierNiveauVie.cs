using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contrôleur du panneau "Modifier mon niveau de vie".
/// 
/// Fonctionnement :
///   - À l'ouverture (OnEnable) : lit les données réelles du joueur et met à jour les pièces.
///   - Pendant la navigation : modifie un état temporaire visible (pas encore sauvegardé).
///   - Bouton "Sauvegarder" : applique l'état temporaire dans les vraies données.
///   - Bouton "Annuler" : ferme sans rien changer.
/// 
/// Comment connecter dans l'Inspector :
///   Chaque rangée (Logement, Sport...) nécessite 5 Buttons (les pièces) et 1 TextMeshProUGUI (description).
///   Assigner les références dans l'Inspector ou laisser le script les chercher automatiquement par nom.
/// </summary>
public class UiModifierNiveauVie : MonoBehaviour
{
    [Header("Données")]
    [SerializeField] private GameData gameData;

    [Header("Panneau")]
    [SerializeField] private GameObject panneauNiveauVie;

    [Header("Pièces de monnaie — Logement (1 à 5)")]
    [SerializeField] private Button[] piecesLogement = new Button[5];

    [Header("Pièces de monnaie — Sport (1 à 5)")]
    [SerializeField] private Button[] piecesSport = new Button[5];

    [Header("Pièces de monnaie — Transport (1 à 5)")]
    [SerializeField] private Button[] piecesTransport = new Button[5];

    [Header("Pièces de monnaie — Alimentation (1 à 5)")]
    [SerializeField] private Button[] piecesAlimentation = new Button[5];

    [Header("Pièces de monnaie — Vie Sociale (1 à 5)")]
    [SerializeField] private Button[] piecesVieSociale = new Button[5];

    [Header("Descriptions (optionnel)")]
    [SerializeField] private TextMeshProUGUI texteDescriptionLogement;
    [SerializeField] private TextMeshProUGUI texteDescriptionSport;
    [SerializeField] private TextMeshProUGUI texteDescriptionTransport;
    [SerializeField] private TextMeshProUGUI texteDescriptionAlimentation;
    [SerializeField] private TextMeshProUGUI texteDescriptionVieSociale;

    [Header("Coût total prévisualisé")]
    [SerializeField] private TextMeshProUGUI texteCoutTotal;

    [Header("Couleurs des pièces")]
    [SerializeField] private Color couleurActive   = new Color(1f, 0.78f, 0f);   // Or
    [SerializeField] private Color couleurInactive = new Color(0.55f, 0.55f, 0.55f); // Gris

    // --- État temporaire (non sauvegardé tant qu'on n'a pas cliqué "Sauvegarder") ---
    private DonneesNiveauVie etatTemporaire = new DonneesNiveauVie();

    // -------------------------------------------------------------------

    private void Awake()
    {
        ResoudreDependances();
    }

    private void OnEnable()
    {
        ResoudreDependances();

        // Sécurité contre la sérialisation transparente de Unity (a = 0)
        if (couleurActive.a == 0f) couleurActive = new Color(1f, 0.78f, 0f, 1f);
        if (couleurInactive.a == 0f) couleurInactive = new Color(0.55f, 0.55f, 0.55f, 1f);

        // Charger les vraies valeurs du joueur dans l'état temporaire
        if (gameData?.joueur?.niveauVie != null)
        {
            etatTemporaire = gameData.joueur.niveauVie.Copier();
        }
        else
        {
            etatTemporaire = new DonneesNiveauVie();
        }

        // Connecter les boutons "pièce" de chaque catégorie
        ConnecterPieces(piecesLogement,     CategorieNiveauVie.Logement);
        ConnecterPieces(piecesSport,        CategorieNiveauVie.Sport);
        ConnecterPieces(piecesTransport,    CategorieNiveauVie.Transport);
        ConnecterPieces(piecesAlimentation, CategorieNiveauVie.Alimentation);
        ConnecterPieces(piecesVieSociale,   CategorieNiveauVie.VieSociale);

        ActualiserAffichage();
    }

    private void OnDisable()
    {
        // Déconnexion propre pour éviter les abonnements en double
        DeconnecterPieces(piecesLogement);
        DeconnecterPieces(piecesSport);
        DeconnecterPieces(piecesTransport);
        DeconnecterPieces(piecesAlimentation);
        DeconnecterPieces(piecesVieSociale);
    }

    // -------------------------------------------------------------------
    // BOUTONS PRINCIPAUX
    // -------------------------------------------------------------------

    /// <summary>Applique les choix temporaires dans les vraies données et ferme.</summary>
    public void Sauvegarder()
    {
        if (gameData?.joueur == null) return;

        gameData.joueur.niveauVie = etatTemporaire.Copier();
        Debug.Log($"[UiModifierNiveauVie] Niveau de vie sauvegardé — " +
                  $"Logement:{etatTemporaire.logement} Sport:{etatTemporaire.sport} " +
                  $"Transport:{etatTemporaire.transport} Alim:{etatTemporaire.alimentation} " +
                  $"VieSociale:{etatTemporaire.vieSociale}");

        Fermer();
    }

    /// <summary>Réinitialise l'affichage aux valeurs par défaut (Sport, Transport, Vie Sociale à 0, et Logement, Alimentation à 1).</summary>
    public void Annuler()
    {
        etatTemporaire.logement = 1;
        etatTemporaire.sport = 0;
        etatTemporaire.transport = 0;
        etatTemporaire.alimentation = 1;
        etatTemporaire.vieSociale = 0;

        ActualiserAffichage();
    }

    // -------------------------------------------------------------------
    // LOGIQUE INTERNE
    // -------------------------------------------------------------------

    private void ConnecterPieces(Button[] pieces, CategorieNiveauVie categorie)
    {
        if (pieces == null) return;
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null) continue;
            pieces[i].onClick.RemoveAllListeners();
            int niveau = i + 1; // les pièces sont indexées 1 à 5
            pieces[i].onClick.AddListener(() => OnPieceCliquee(categorie, niveau));
        }
    }

    private void DeconnecterPieces(Button[] pieces)
    {
        if (pieces == null) return;
        foreach (var piece in pieces)
        {
            piece?.onClick.RemoveAllListeners();
        }
    }

    private void OnPieceCliquee(CategorieNiveauVie categorie, int niveau)
    {
        int niveauActuel = 0;
        switch (categorie)
        {
            case CategorieNiveauVie.Logement:     niveauActuel = etatTemporaire.logement;     break;
            case CategorieNiveauVie.Sport:        niveauActuel = etatTemporaire.sport;        break;
            case CategorieNiveauVie.Transport:    niveauActuel = etatTemporaire.transport;    break;
            case CategorieNiveauVie.Alimentation: niveauActuel = etatTemporaire.alimentation; break;
            case CategorieNiveauVie.VieSociale:   niveauActuel = etatTemporaire.vieSociale;   break;
        }

        int nouveauNiveau = niveau;
        if (niveauActuel == niveau)
        {
            // Clic sur l'échelon déjà actif : toggle ou descente
            if (niveau == 1)
            {
                // Logement et Alimentation obligatoires (min 1)
                bool estObligatoire = (categorie == CategorieNiveauVie.Logement || categorie == CategorieNiveauVie.Alimentation);
                nouveauNiveau = estObligatoire ? 1 : 0;
            }
            else
            {
                nouveauNiveau = niveau - 1;
            }
        }

        switch (categorie)
        {
            case CategorieNiveauVie.Logement:     etatTemporaire.logement     = Mathf.Clamp(nouveauNiveau, 1, 5); break;
            case CategorieNiveauVie.Sport:        etatTemporaire.sport        = Mathf.Clamp(nouveauNiveau, 0, 5); break;
            case CategorieNiveauVie.Transport:    etatTemporaire.transport    = Mathf.Clamp(nouveauNiveau, 0, 5); break;
            case CategorieNiveauVie.Alimentation: etatTemporaire.alimentation = Mathf.Clamp(nouveauNiveau, 1, 5); break;
            case CategorieNiveauVie.VieSociale:   etatTemporaire.vieSociale   = Mathf.Clamp(nouveauNiveau, 0, 5); break;
        }
        ActualiserAffichage();
    }

    private void ActualiserAffichage()
    {
        // Mettre à jour les couleurs des pièces pour chaque catégorie
        ActualiserPieces(piecesLogement,     etatTemporaire.logement,     CategorieNiveauVie.Logement);
        ActualiserPieces(piecesSport,        etatTemporaire.sport,        CategorieNiveauVie.Sport);
        ActualiserPieces(piecesTransport,    etatTemporaire.transport,    CategorieNiveauVie.Transport);
        ActualiserPieces(piecesAlimentation, etatTemporaire.alimentation, CategorieNiveauVie.Alimentation);
        ActualiserPieces(piecesVieSociale,   etatTemporaire.vieSociale,   CategorieNiveauVie.VieSociale);

        // Mettre à jour les descriptions
        ActualiserDescription(texteDescriptionLogement,     CategorieNiveauVie.Logement,     etatTemporaire.logement);
        ActualiserDescription(texteDescriptionSport,        CategorieNiveauVie.Sport,        etatTemporaire.sport);
        ActualiserDescription(texteDescriptionTransport,    CategorieNiveauVie.Transport,    etatTemporaire.transport);
        ActualiserDescription(texteDescriptionAlimentation, CategorieNiveauVie.Alimentation, etatTemporaire.alimentation);
        ActualiserDescription(texteDescriptionVieSociale,   CategorieNiveauVie.VieSociale,   etatTemporaire.vieSociale);

        // Coût total prévisualisé
        if (texteCoutTotal != null)
        {
            argent cout = GestionnaireNiveauVie.CalculerCoutMensuel(etatTemporaire);
            texteCoutTotal.text = $"Coût mensuel total : {cout}";
        }
    }

    private void ActualiserPieces(Button[] pieces, int niveauActif, CategorieNiveauVie categorie)
    {
        if (pieces == null) return;
        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i] == null) continue;
            bool estActive = (i + 1) <= niveauActif;
            Color c = estActive ? couleurActive : couleurInactive;

            // 1. Modifier la couleur de l'image de fond
            Image img = pieces[i].GetComponent<Image>();
            if (img != null)
            {
                img.color = c;
            }

            // 2. Modifier également le ColorBlock du bouton pour les transitions Color Tint
            Button btn = pieces[i];
            if (btn != null)
            {
                ColorBlock cb = btn.colors;
                cb.normalColor = c;
                cb.highlightedColor = c;
                cb.pressedColor = c;
                cb.selectedColor = c;
                cb.disabledColor = c;
                btn.colors = cb;
            }
        }
    }

    private void ActualiserDescription(TextMeshProUGUI texte, CategorieNiveauVie categorie, int niveau)
    {
        if (texte == null) return;
        argent cout = GestionnaireNiveauVie.CoutCategorie(categorie, niveau);
        texte.text = ObtenirDescriptionNiveau(categorie, niveau) + $" ({cout}/mois)";
    }

    private string ObtenirDescriptionNiveau(CategorieNiveauVie categorie, int niveau)
    {
        return categorie switch
        {
            CategorieNiveauVie.Logement => niveau switch
            {
                1 => "Studio en colocation",
                2 => "Studio indépendant",
                3 => "Appartement confortable",
                4 => "Grand appartement (F4)",
                5 => "Villa de luxe",
                _ => ""
            },
            CategorieNiveauVie.Sport => niveau switch
            {
                0 => "Aucune activité",
                1 => "Entraînement à la maison",
                2 => "Salle de sport basique",
                3 => "Club de sport & équipement",
                4 => "Coach personnel",
                5 => "Programme d'athlète élite",
                _ => ""
            },
            CategorieNiveauVie.Transport => niveau switch
            {
                0 => "Marche à pied uniquement",
                1 => "Transports en commun",
                2 => "Vélo / Trottinette électrique",
                3 => "Voiture d'occasion",
                4 => "Voiture familiale neuve",
                5 => "Voiture de sport / chauffeur",
                _ => ""
            },
            CategorieNiveauVie.Alimentation => niveau switch
            {
                1 => "Repas à l'économie / cuisine maison",
                2 => "Courses standards supermarché",
                3 => "Produits frais et de qualité",
                4 => "Aliments bio & restaurants",
                5 => "Traiteur gastronomique",
                _ => ""
            },
            CategorieNiveauVie.VieSociale => niveau switch
            {
                0 => "Sorties inexistantes",
                1 => "Sorties très rares",
                2 => "Verres entre amis occasionnels",
                3 => "Vie sociale active",
                4 => "Restaurants & soirées",
                5 => "Lifestyle VIP",
                _ => ""
            },
            _ => ""
        };
    }

    private void Fermer()
    {
        if (panneauNiveauVie != null)
        {
            panneauNiveauVie.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void ResoudreDependances()
    {
        if (gameData == null)
        {
            ActionPlay actionPlay = FindFirstObjectByType<ActionPlay>();
            if (actionPlay != null) gameData = actionPlay.gameData;
        }
    }
}
