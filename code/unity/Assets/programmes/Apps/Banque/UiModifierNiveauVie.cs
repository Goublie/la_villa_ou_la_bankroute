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

    /// <summary>Ferme le panneau sans rien changer.</summary>
    public void Annuler()
    {
        Fermer();
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
        switch (categorie)
        {
            case CategorieNiveauVie.Logement:     etatTemporaire.logement     = niveau; break;
            case CategorieNiveauVie.Sport:        etatTemporaire.sport        = niveau; break;
            case CategorieNiveauVie.Transport:    etatTemporaire.transport    = niveau; break;
            case CategorieNiveauVie.Alimentation: etatTemporaire.alimentation = niveau; break;
            case CategorieNiveauVie.VieSociale:   etatTemporaire.vieSociale   = niveau; break;
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
            Image img = pieces[i].GetComponent<Image>();
            if (img != null)
            {
                img.color = estActive ? couleurActive : couleurInactive;
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
                1 => "Logement partagé / coloc",
                2 => "Studio modeste",
                3 => "Appartement confortable",
                4 => "Grand appartement",
                5 => "Villa / résidence de luxe",
                _ => ""
            },
            CategorieNiveauVie.Sport => niveau switch
            {
                1 => "Aucune activité sportive",
                2 => "Salle de sport basique",
                3 => "Club de sport + équipement",
                4 => "Coach personnel",
                5 => "Programme élite",
                _ => ""
            },
            CategorieNiveauVie.Transport => niveau switch
            {
                1 => "Transports en commun",
                2 => "Vélo / trottinette",
                3 => "Voiture économique",
                4 => "Voiture confortable",
                5 => "Voiture de luxe / chauffeur",
                _ => ""
            },
            CategorieNiveauVie.Alimentation => niveau switch
            {
                1 => "Budget serré / cuisine maison",
                2 => "Courses normales",
                3 => "Produits de qualité",
                4 => "Bio + restaurants réguliers",
                5 => "Gastronomie / traiteur",
                _ => ""
            },
            CategorieNiveauVie.VieSociale => niveau switch
            {
                1 => "Sorties rares",
                2 => "Sorties occasionnelles",
                3 => "Vie sociale active",
                4 => "Restaurants & événements",
                5 => "Lifestyle haut de gamme",
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
