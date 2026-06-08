using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using System.Reflection;

/// <summary>
/// Script utilitaire pour générer et modifier la structure du Prefab Actualites.
/// </summary>
public class ActualitesPrefabGenerator : EditorWindow
{
    [MenuItem("Outils/Générer Prefab Actualités")]
    public static void ShowWindow()
    {
        GetWindow<ActualitesPrefabGenerator>("Générateur Actualités");
    }

    private void OnGUI()
    {
        GUILayout.Label("Construction Complète du Prefab Actualités", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Ce script va configurer la structure de la fenêtre d'actualités conformément à la maquette Figma (Sidebar catégories, Tableau 5 colonnes, détails, encart courbe verte).", MessageType.Info);
        
        if (GUILayout.Button("Reconstruire / Configurer le Prefab Actualités"))
        {
            ReconstruirePrefab();
        }
    }

    private void ReconstruirePrefab()
    {
        string prefabPath = "Assets/prefabs/fenetre/Actualites.prefab";
        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefabAsset == null)
        {
            Debug.LogError("Prefab Actualites introuvable à la route : " + prefabPath);
            return;
        }

        // Instancier le prefab temporairement dans la scène pour modification
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
        
        try
        {
            // 1. Script Principal
            ActualitesUI ui = instance.GetComponent<ActualitesUI>();
            if (ui == null) ui = instance.AddComponent<ActualitesUI>();

            // 2. Recherche et configuration du conteneur principal
            Transform rectTrans = instance.transform;
            
            // Modifier la couleur du fond global (Gris moyen)
            Transform fondTransform = rectTrans.Find("Fond");
            if (fondTransform != null)
            {
                Image imgFond = fondTransform.GetComponent<Image>();
                if (imgFond != null)
                {
                    imgFond.color = new Color(0.69f, 0.69f, 0.69f); // Gris moyen #B0B0B0
                }
            }

            // 3. Boutons d'onglets du haut
            GameObject tabCat = GetOrCreateChild(rectTrans, "Tab_Categories", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            ConfigureRect(tabCat.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(120, -25), new Vector2(200, 50), new Vector2(0.5f, 0.5f));
            tabCat.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f); // Gris clair
            SetPrivateField(ui, "btnCategoriesTab", tabCat.GetComponent<Button>());

            GameObject txtTabCat = GetOrCreateChild(tabCat.transform, "Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            ConfigureRect(txtTabCat.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            ConfigureTMP(txtTabCat.GetComponent<TextMeshProUGUI>(), "Catégories", 24, new Color(0.08f, 0.13f, 0.79f), TextAlignmentOptions.Center);

            GameObject tabNews = GetOrCreateChild(rectTrans, "Tab_DernieresActualites", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            ConfigureRect(tabNews.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(405, -25), new Vector2(350, 50), new Vector2(0.5f, 0.5f));
            tabNews.GetComponent<Image>().color = new Color(0.69f, 0.83f, 1f); // Bleu clair
            SetPrivateField(ui, "btnDernieresActualites", tabNews.GetComponent<Button>());

            GameObject txtTabNews = GetOrCreateChild(tabNews.transform, "Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            ConfigureRect(txtTabNews.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            ConfigureTMP(txtTabNews.GetComponent<TextMeshProUGUI>(), "Dernières Actualités", 24, new Color(0.08f, 0.13f, 0.79f), TextAlignmentOptions.Center);

            // 4. Sidebar gauche (Catégories)
            GameObject sidebar = GetOrCreateChild(rectTrans, "SidebarCategories", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup));
            ConfigureRect(sidebar.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(120, -510), new Vector2(200, 920), new Vector2(0.5f, 0.5f));
            sidebar.GetComponent<Image>().color = new Color(0.55f, 0.55f, 0.55f); // Gris plus foncé
            SetPrivateField(ui, "categoriesContainer", sidebar.transform);

            VerticalLayoutGroup vlgSidebar = sidebar.GetComponent<VerticalLayoutGroup>();
            vlgSidebar.spacing = 10;
            vlgSidebar.padding = new RectOffset(10, 10, 10, 10);
            vlgSidebar.childControlWidth = true;
            vlgSidebar.childControlHeight = false;
            vlgSidebar.childForceExpandWidth = true;
            vlgSidebar.childForceExpandHeight = false;

            // Template de bouton de catégorie
            GameObject catBtnTemplate = GetOrCreateChild(sidebar.transform, "CategoryBtnTemplate", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            catBtnTemplate.GetComponent<Image>().color = new Color(0.95f, 0.95f, 0.95f);
            catBtnTemplate.GetComponent<LayoutElement>().minHeight = 55;
            catBtnTemplate.GetComponent<LayoutElement>().preferredHeight = 55;
            GameObject txtCatBtn = GetOrCreateChild(catBtnTemplate.transform, "Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            ConfigureRect(txtCatBtn.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            ConfigureTMP(txtCatBtn.GetComponent<TextMeshProUGUI>(), "Catégorie", 20, Color.black, TextAlignmentOptions.Center);
            catBtnTemplate.SetActive(false); // Masqué par défaut (servira de prefab d'instanciation)
            SetPrivateField(ui, "prefabCategoryBtn", catBtnTemplate);

            // Ajouter des slots de déco vides pour le style comme sur le Figma (pour remplir la barre)
            for (int i = 0; i < 3; i++)
            {
                GameObject placeholder = GetOrCreateChild(sidebar.transform, $"Placeholder_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
                placeholder.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f, 0.5f);
                placeholder.GetComponent<LayoutElement>().minHeight = 55;
            }

            // 5. Panneau de contenu principal (Droit)
            GameObject panelContent = GetOrCreateChild(rectTrans, "PanelContent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ConfigureRect(panelContent.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(1070, -510), new Vector2(1670, 920), new Vector2(0.5f, 0.5f));
            panelContent.GetComponent<Image>().color = new Color(0.92f, 0.91f, 0.85f); // Beige clair #EAE7D9

            // 6. Tableau supérieur dans le panneau droit
            GameObject tableContainer = GetOrCreateChild(panelContent.transform, "TableauNews", typeof(RectTransform), typeof(CanvasRenderer), typeof(VerticalLayoutGroup));
            // Ancrage Top-Stretch, à 10px du haut, hauteur 450
            ConfigureRect(tableContainer.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -10), new Vector2(-40, 450), new Vector2(0.5f, 1));
            
            VerticalLayoutGroup vlgTable = tableContainer.GetComponent<VerticalLayoutGroup>();
            vlgTable.spacing = 2;
            vlgTable.padding = new RectOffset(5, 5, 5, 5);
            vlgTable.childControlWidth = true;
            vlgTable.childControlHeight = false;
            vlgTable.childForceExpandWidth = true;
            vlgTable.childForceExpandHeight = false;

            // En-tête de tableau (Header) - fond gris foncé pour les bordures
            GameObject headerRow = GetOrCreateChild(tableContainer.transform, "HeaderRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(Image));
            headerRow.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f); // Bordures sombres
            
            HorizontalLayoutGroup hlgHeader = headerRow.GetComponent<HorizontalLayoutGroup>();
            hlgHeader.spacing = 2; // Écartement fin pour simuler une grille
            hlgHeader.padding = new RectOffset(2, 2, 2, 2);
            hlgHeader.childControlWidth = true;
            hlgHeader.childControlHeight = true;
            hlgHeader.childForceExpandWidth = false;
            hlgHeader.childForceExpandHeight = true;
            headerRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50);

            string[] headers = { "Date", "Catégorie", "Titre", "Impact(beta)", "Nouveau(beta)" };
            float[] colWidths = { 170, 210, 760, 240, 240 };
            for (int i = 0; i < headers.Length; i++)
            {
                GameObject colHeader = GetOrCreateChild(headerRow.transform, $"Col_{headers[i]}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(TextMeshProUGUI), typeof(LayoutElement));
                colHeader.GetComponent<Image>().color = new Color(0.85f, 0.85f, 0.85f); // Fond de cellule en-tête gris clair
                
                TextMeshProUGUI tmp = colHeader.GetComponent<TextMeshProUGUI>();
                TextAlignmentOptions align = (i == 2) ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;
                ConfigureTMP(tmp, headers[i], 22, Color.black, align);
                if (i == 2) tmp.margin = new Vector4(10, 0, 10, 0); // Marge interne pour le titre
                
                colHeader.GetComponent<LayoutElement>().minWidth = colWidths[i];
                colHeader.GetComponent<LayoutElement>().preferredWidth = colWidths[i];
                colHeader.GetComponent<LayoutElement>().flexibleWidth = (i == 2) ? 1 : 0;
            }

            // News rows container (pour les actualités dynamiques)
            GameObject rowsContainer = GetOrCreateChild(tableContainer.transform, "RowsContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
            ConfigureRect(rowsContainer.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            VerticalLayoutGroup vlgRows = rowsContainer.GetComponent<VerticalLayoutGroup>();
            vlgRows.spacing = 5;
            vlgRows.childControlWidth = true;
            vlgRows.childControlHeight = false;
            vlgRows.childForceExpandWidth = true;
            vlgRows.childForceExpandHeight = false;
            SetPrivateField(ui, "newsRowsContainer", rowsContainer.transform);

            // News Row Template - fond sombre pour les bordures de la grille
            GameObject rowTemplate = GetOrCreateChild(tableContainer.transform, "NewsRowTemplate", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(HorizontalLayoutGroup), typeof(Button), typeof(Ligne), typeof(LayoutElement));
            rowTemplate.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f); // Bordures sombres
            rowTemplate.GetComponent<LayoutElement>().minHeight = 50;
            rowTemplate.GetComponent<LayoutElement>().preferredHeight = 50;
            
            HorizontalLayoutGroup hlgRow = rowTemplate.GetComponent<HorizontalLayoutGroup>();
            hlgRow.spacing = 2; // Grille
            hlgRow.padding = new RectOffset(2, 2, 2, 2);
            hlgRow.childControlWidth = true;
            hlgRow.childControlHeight = true;
            hlgRow.childForceExpandWidth = false;
            hlgRow.childForceExpandHeight = true;

            for (int i = 0; i < 5; i++)
            {
                GameObject caseObj = GetOrCreateChild(rowTemplate.transform, $"Case{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement), typeof(Case));
                caseObj.GetComponent<Image>().color = Color.white; // Cellules blanches
                caseObj.GetComponent<LayoutElement>().minWidth = colWidths[i];
                caseObj.GetComponent<LayoutElement>().preferredWidth = colWidths[i];
                caseObj.GetComponent<LayoutElement>().flexibleWidth = (i == 2) ? 1 : 0;

                GameObject txtCase = GetOrCreateChild(caseObj.transform, "Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                ConfigureRect(txtCase.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
                
                TextMeshProUGUI tmp = txtCase.GetComponent<TextMeshProUGUI>();
                TextAlignmentOptions align = (i == 2) ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;
                ConfigureTMP(tmp, $"case{i}", 18, Color.black, align);
                if (i == 2) tmp.margin = new Vector4(10, 0, 10, 0); // Marge interne pour le titre
            }
            rowTemplate.SetActive(false); // Masqué, sert de template
            SetPrivateField(ui, "prefabNewsRow", rowTemplate);

            // 7. Panneau de détails en bas
            GameObject panelDetails = GetOrCreateChild(panelContent.transform, "PanelDetails", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            // Ancrage Top-Stretch, à 480px du haut, hauteur 420 (laisse 20px de marge en bas sur 920px de hauteur totale)
            ConfigureRect(panelDetails.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -480), new Vector2(-40, 420), new Vector2(0.5f, 1));
            panelDetails.GetComponent<Image>().color = new Color(0.92f, 0.91f, 0.85f, 0f); // Transparent

            // Titre des détails (Ancrage Top-Left)
            GameObject txtDetailTitre = GetOrCreateChild(panelDetails.transform, "TxtDetailTitre", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            ConfigureRect(txtDetailTitre.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(1200, 60), new Vector2(0, 1));
            ConfigureTMP(txtDetailTitre.GetComponent<TextMeshProUGUI>(), "Titre de l'Actualité", 30, new Color(0.04f, 0.1f, 0.37f), TextAlignmentOptions.Left);
            txtDetailTitre.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
            SetPrivateField(ui, "txtDetailTitre", txtDetailTitre.GetComponent<TextMeshProUGUI>());

            // Description des détails (Ancrage Top-Left)
            GameObject txtDetailDesc = GetOrCreateChild(panelDetails.transform, "TxtDetailDescription", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            ConfigureRect(txtDetailDesc.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -90), new Vector2(1200, 310), new Vector2(0, 1));
            ConfigureTMP(txtDetailDesc.GetComponent<TextMeshProUGUI>(), "Description complète de l'actualité...", 24, Color.black, TextAlignmentOptions.TopLeft);
            txtDetailDesc.GetComponent<TextMeshProUGUI>().enableWordWrapping = true;
            SetPrivateField(ui, "txtDetailDescription", txtDetailDesc.GetComponent<TextMeshProUGUI>());

            // Pavé vert "Inserer courbe" (Ancrage Top-Right)
            GameObject panelCourbe = GetOrCreateChild(panelDetails.transform, "PanelCourbe", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            ConfigureRect(panelCourbe.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), new Vector2(380, 380), new Vector2(1, 1));
            panelCourbe.GetComponent<Image>().color = new Color(0.6f, 1f, 0f); // Vert fluo
            SetPrivateField(ui, "panelCourbe", panelCourbe);

            GameObject txtCourbe = GetOrCreateChild(panelCourbe.transform, "Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            ConfigureRect(txtCourbe.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            ConfigureTMP(txtCourbe.GetComponent<TextMeshProUGUI>(), "Inserer courbe", 28, Color.black, TextAlignmentOptions.Center);

            // Sauvegarde du Prefab
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Debug.Log("Prefab Actualités RECONSTRUIT avec succès !");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur lors de la reconstruction du prefab : " + e.Message + "\n" + e.StackTrace);
        }
        finally
        {
            DestroyImmediate(instance);
        }
    }

    private GameObject GetOrCreateChild(Transform parent, string name, params System.Type[] components)
    {
        Transform t = parent.Find(name);
        if (t == null)
        {
            GameObject go = new GameObject(name, components);
            go.transform.SetParent(parent, false);
            return go;
        }
        else
        {
            foreach (var type in components)
            {
                if (t.GetComponent(type) == null)
                {
                    t.gameObject.AddComponent(type);
                }
            }
            return t.gameObject;
        }
    }

    private void ConfigureRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 pivot)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = sizeDelta;
        rt.pivot = pivot;
        rt.localScale = Vector3.one;
    }

    private void ConfigureTMP(TextMeshProUGUI tmp, string text, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        if (value == null) return;
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogWarning($"Champ privé non trouvé : {fieldName} sur {target.GetType()}");
        }
    }
}
