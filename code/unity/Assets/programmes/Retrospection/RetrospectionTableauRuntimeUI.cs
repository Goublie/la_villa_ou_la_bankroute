using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tableau autonome créé à l'exécution pour la rétrospection.
/// Il ne dépend pas du prefab TableauScroll historique.
/// </summary>
public sealed class RetrospectionTableauRuntimeUI : MonoBehaviour
{
    private static readonly Color FondPrincipal =
        new Color32(245, 245, 238, 255);

    private static readonly Color FondEntete =
        new Color32(22, 49, 126, 255);

    private static readonly Color FondSection =
        new Color32(211, 220, 241, 255);

    private static readonly Color FondLigneClaire =
        new Color32(255, 255, 255, 255);

    private static readonly Color FondLigneFoncee =
        new Color32(238, 240, 244, 255);

    private static readonly Color TexteSombre =
        new Color32(35, 35, 35, 255);

    private GameData gameData;
    private RectTransform racine;
    private RectTransform contenu;
    private bool interfaceConstruite;

    public RectTransform RacineGeneree => racine;
    public RectTransform ContenuGenere => contenu;

    public void Initialiser(GameData donnees)
    {
        gameData = donnees;
        ConstruireInterface();
        Actualiser();
    }

    private void OnEnable()
    {
        if (interfaceConstruite)
        {
            Actualiser();
        }
    }

    public void ConstruireInterface()
    {
        if (interfaceConstruite &&
            racine != null &&
            contenu != null)
        {
            return;
        }

        MasquerAncienneInterface();

        racine = CreerRectTransform(
            "TableauWhatIfRuntime",
            transform);

        racine.anchorMin = Vector2.zero;
        racine.anchorMax = Vector2.one;
        racine.offsetMin = new Vector2(20f, 18f);
        racine.offsetMax = new Vector2(-20f, -18f);
        racine.localScale = Vector3.one;

        Image fond = racine.gameObject.AddComponent<Image>();
        fond.color = FondPrincipal;

        CreerTitre(racine);
        CreerZoneDefilement(racine);

        interfaceConstruite = true;
    }

    public void Actualiser()
    {
        ConstruireInterface();
        ViderContenu();

        AjouterLigne(
            "ÉLÉMENT",
            "VALEUR / ACTION",
            "DÉTAIL",
            FondEntete,
            Color.white,
            46f,
            true);

        if (gameData == null)
        {
            AjouterLigne(
                "Bilan",
                "Données indisponibles",
                "GameData n'a pas été trouvé.",
                FondLigneClaire,
                TexteSombre,
                54f,
                false);
            return;
        }

        List<Optimizer.SimulationResult> historiqueReel =
            Optimizer.ObtenirHistoriqueReel(gameData);

        List<Optimizer.SimulationResult> historiqueWhatIf =
            Optimizer.ObtenirHistoriqueWhatIf(gameData);

        AjouterSection(
            "BILAN COMPARÉ",
            "Patrimoine réel et scénario What If");

        AjouterBilan(
            historiqueReel,
            historiqueWhatIf);

        AjouterSection(
            "ORDRES WHAT IF",
            "Achats, ventes et ventes forcées des 12 derniers mois");

        AjouterOrdres();

        AjouterSection(
            "ÉVÉNEMENTS CONFIRMÉS",
            "Événements des 12 derniers mois");

        AjouterEvenements();
    }

    private void AjouterBilan(
        List<Optimizer.SimulationResult> reel,
        List<Optimizer.SimulationResult> alternatif)
    {
        if (reel.Count == 0 || alternatif.Count == 0)
        {
            AjouterLigne(
                "Historique",
                "Insuffisant",
                "Aucun mois complet n'a encore été clôturé.",
                FondLigneClaire,
                TexteSombre,
                54f,
                false);
            return;
        }

        Optimizer.SimulationResult dernierReel =
            reel[reel.Count - 1];

        Optimizer.SimulationResult dernierAlternatif =
            alternatif[alternatif.Count - 1];

        int ecart = DifferenceSaturee(
            dernierAlternatif.patrimoineTotal.centimes,
            dernierReel.patrimoineTotal.centimes);

        AjouterLigne(
            "Stratégie What If",
            dernierAlternatif.patrimoineTotal.ToString(),
            "Scénario alternatif",
            FondLigneClaire,
            TexteSombre,
            54f,
            false);

        AjouterLigne(
            "Joueur réel",
            dernierReel.patrimoineTotal.ToString(),
            FormaterEcart(ecart),
            FondLigneFoncee,
            TexteSombre,
            54f,
            false);
    }

    private void AjouterOrdres()
    {
        List<LigneOperationRetrospectionWhatIf> lignes =
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireLignesOrdres(
                    gameData.whatIf,
                    gameData.nombreMoisPasses);

        if (lignes.Count == 0)
        {
            AjouterLigne(
                "-",
                "Aucune opération",
                "Le modèle n'a encore exécuté aucun ordre.",
                FondLigneClaire,
                TexteSombre,
                54f,
                false);
            return;
        }

        for (int index = 0; index < lignes.Count; index++)
        {
            LigneOperationRetrospectionWhatIf ligne =
                lignes[index];

            AjouterLigne(
                ligne.mois,
                ligne.operation,
                ligne.detail,
                index % 2 == 0
                    ? FondLigneClaire
                    : FondLigneFoncee,
                TexteSombre,
                62f,
                false);
        }
    }

    private void AjouterEvenements()
    {
        List<LigneOperationRetrospectionWhatIf> lignes =
            ServiceRetrospectionDetailleeWhatIf
                .ConstruireLignesEvenementsConfirmes(
                    gameData.evenements,
                    gameData.whatIf,
                    gameData.nombreMoisPasses);

        if (lignes.Count == 0)
        {
            AjouterLigne(
                "-",
                "Aucun événement confirmé",
                "Aucun événement confirmé sur les 12 derniers mois.",
                FondLigneClaire,
                TexteSombre,
                54f,
                false);
            return;
        }

        for (int index = 0; index < lignes.Count; index++)
        {
            LigneOperationRetrospectionWhatIf ligne =
                lignes[index];

            AjouterLigne(
                ligne.mois,
                ligne.operation,
                ligne.detail,
                index % 2 == 0
                    ? FondLigneClaire
                    : FondLigneFoncee,
                TexteSombre,
                62f,
                false);
        }
    }

    private void AjouterSection(
        string titre,
        string description)
    {
        AjouterLigne(
            titre,
            "",
            description,
            FondSection,
            TexteSombre,
            46f,
            true);
    }

    private void CreerTitre(RectTransform parent)
    {
        RectTransform titre = CreerRectTransform(
            "TitreTableau",
            parent);

        titre.anchorMin = new Vector2(0f, 1f);
        titre.anchorMax = new Vector2(1f, 1f);
        titre.pivot = new Vector2(0.5f, 1f);
        titre.anchoredPosition = Vector2.zero;
        titre.sizeDelta = new Vector2(0f, 58f);

        Image fondTitre =
            titre.gameObject.AddComponent<Image>();
        fondTitre.color =
            new Color32(221, 225, 232, 255);

        TextMeshProUGUI texte = CreerTexte(
            "Titre",
            titre,
            18f,
            FontStyles.Bold,
            TexteSombre,
            TextAlignmentOptions.MidlineLeft);

        texte.text =
            "Bilan et opérations du moteur What If   |   " +
            "Molette : faire défiler";

        RectTransform rectTexte =
            texte.rectTransform;
        rectTexte.offsetMin =
            new Vector2(18f, 0f);
        rectTexte.offsetMax =
            new Vector2(-18f, 0f);
    }

    private void CreerZoneDefilement(
        RectTransform parent)
    {
        RectTransform zone = CreerRectTransform(
            "ZoneDefilement",
            parent);

        zone.anchorMin = Vector2.zero;
        zone.anchorMax = Vector2.one;
        zone.offsetMin = new Vector2(0f, 0f);
        zone.offsetMax = new Vector2(0f, -62f);

        Image fondZone =
            zone.gameObject.AddComponent<Image>();
        fondZone.color = FondPrincipal;

        ScrollRect scroll =
            zone.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 32f;
        scroll.movementType =
            ScrollRect.MovementType.Clamped;

        RectTransform viewport = CreerRectTransform(
            "Viewport",
            zone);

        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = Vector2.zero;

        Image fondViewport =
            viewport.gameObject.AddComponent<Image>();
        fondViewport.color =
            new Color32(250, 250, 247, 255);

        viewport.gameObject.AddComponent<RectMask2D>();

        contenu = CreerRectTransform(
            "Contenu",
            viewport);

        contenu.anchorMin = new Vector2(0f, 1f);
        contenu.anchorMax = new Vector2(1f, 1f);
        contenu.pivot = new Vector2(0.5f, 1f);
        contenu.anchoredPosition = Vector2.zero;
        contenu.sizeDelta = Vector2.zero;

        VerticalLayoutGroup disposition =
            contenu.gameObject
                .AddComponent<VerticalLayoutGroup>();

        disposition.padding =
            new RectOffset(8, 8, 8, 8);
        disposition.spacing = 4f;
        disposition.childAlignment =
            TextAnchor.UpperCenter;
        disposition.childControlWidth = true;
        disposition.childControlHeight = true;
        disposition.childForceExpandWidth = true;
        disposition.childForceExpandHeight = false;

        ContentSizeFitter ajusteur =
            contenu.gameObject
                .AddComponent<ContentSizeFitter>();

        ajusteur.horizontalFit =
            ContentSizeFitter.FitMode.Unconstrained;
        ajusteur.verticalFit =
            ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport;
        scroll.content = contenu;
    }

    private void AjouterLigne(
        string colonneUne,
        string colonneDeux,
        string colonneTrois,
        Color couleurFond,
        Color couleurTexte,
        float hauteur,
        bool gras)
    {
        GameObject ligne = new GameObject(
            "Ligne",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(HorizontalLayoutGroup),
            typeof(LayoutElement));

        RectTransform rect =
            ligne.GetComponent<RectTransform>();
        rect.SetParent(contenu, false);
        rect.localScale = Vector3.one;

        Image image = ligne.GetComponent<Image>();
        image.color = couleurFond;

        LayoutElement dimensions =
            ligne.GetComponent<LayoutElement>();
        dimensions.minHeight = hauteur;
        dimensions.preferredHeight = hauteur;
        dimensions.flexibleHeight = 0f;

        HorizontalLayoutGroup disposition =
            ligne.GetComponent<HorizontalLayoutGroup>();

        disposition.padding =
            new RectOffset(12, 12, 5, 5);
        disposition.spacing = 8f;
        disposition.childAlignment =
            TextAnchor.MiddleLeft;
        disposition.childControlWidth = true;
        disposition.childControlHeight = true;
        disposition.childForceExpandWidth = true;
        disposition.childForceExpandHeight = true;

        FontStyles style =
            gras ? FontStyles.Bold : FontStyles.Normal;

        AjouterCellule(
            ligne.transform,
            colonneUne,
            1.15f,
            couleurTexte,
            style);

        AjouterCellule(
            ligne.transform,
            colonneDeux,
            1.25f,
            couleurTexte,
            style);

        AjouterCellule(
            ligne.transform,
            colonneTrois,
            3.6f,
            couleurTexte,
            style);
    }

    private static void AjouterCellule(
        Transform parent,
        string valeur,
        float largeurFlexible,
        Color couleur,
        FontStyles style)
    {
        RectTransform cellule = CreerRectTransform(
            "Cellule",
            parent);

        LayoutElement largeur =
            cellule.gameObject
                .AddComponent<LayoutElement>();

        largeur.flexibleWidth = largeurFlexible;
        largeur.minWidth = 0f;

        TextMeshProUGUI texte = CreerTexte(
            "Texte",
            cellule,
            15f,
            style,
            couleur,
            TextAlignmentOptions.MidlineLeft);

        texte.text = valeur ?? string.Empty;
        texte.enableWordWrapping = true;
        texte.overflowMode =
            TextOverflowModes.Ellipsis;
        texte.enableAutoSizing = true;
        texte.fontSizeMin = 10f;
        texte.fontSizeMax = 15f;
        texte.margin =
            new Vector4(4f, 1f, 4f, 1f);
    }

    private static TextMeshProUGUI CreerTexte(
        string nom,
        Transform parent,
        float taille,
        FontStyles style,
        Color couleur,
        TextAlignmentOptions alignement)
    {
        GameObject objet = new GameObject(
            nom,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));

        RectTransform rect =
            objet.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        TextMeshProUGUI texte =
            objet.GetComponent<TextMeshProUGUI>();

        texte.font = TMP_Settings.defaultFontAsset;
        texte.fontSize = taille;
        texte.fontStyle = style;
        texte.color = couleur;
        texte.alignment = alignement;
        texte.raycastTarget = false;

        return texte;
    }

    private static RectTransform CreerRectTransform(
        string nom,
        Transform parent)
    {
        GameObject objet = new GameObject(
            nom,
            typeof(RectTransform));

        RectTransform rect =
            objet.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.localScale = Vector3.one;

        return rect;
    }

    private void MasquerAncienneInterface()
    {
        for (int index = 0;
            index < transform.childCount;
            index++)
        {
            Transform enfant =
                transform.GetChild(index);

            if (enfant != null &&
                enfant != racine)
            {
                enfant.gameObject.SetActive(false);
            }
        }
    }

    private void ViderContenu()
    {
        if (contenu == null)
        {
            return;
        }

        for (int index = contenu.childCount - 1;
            index >= 0;
            index--)
        {
            GameObject enfant =
                contenu.GetChild(index).gameObject;

            if (Application.isPlaying)
            {
                Destroy(enfant);
            }
            else
            {
                DestroyImmediate(enfant);
            }
        }
    }

    private static string FormaterEcart(int avantageWhatIf)
    {
        if (avantageWhatIf > 0)
        {
            return "Le What If est devant de " +
                new argent(avantageWhatIf);
        }

        if (avantageWhatIf < 0)
        {
            int avantageReel =
                avantageWhatIf == int.MinValue
                    ? int.MaxValue
                    : -avantageWhatIf;

            return "Le joueur réel est devant de " +
                new argent(avantageReel);
        }

        return "Écart nul";
    }

    private static int DifferenceSaturee(
        int gauche,
        int droite)
    {
        long difference = (long)gauche - droite;

        if (difference > int.MaxValue)
        {
            return int.MaxValue;
        }

        return difference < int.MinValue
            ? int.MinValue
            : (int)difference;
    }
}