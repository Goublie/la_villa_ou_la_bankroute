using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using System.Reflection;

/// <summary>
/// Script utilitaire AVANCÉ pour modifier la structure du Prefab Salariat.
/// </summary>
public class SalariatPrefabGenerator : EditorWindow
{
    [MenuItem("Outils/Générer Prefab Salariat Complet")]
    public static void ShowWindow()
    {
        GetWindow<SalariatPrefabGenerator>("Générateur Salariat");
    }

    private void OnGUI()
    {
        GUILayout.Label("Construction Complète du Prefab Salariat", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Ce script va créer les boutons, la jauge et le panel manquants dans le Prefab.", MessageType.Info);
        
        if (GUILayout.Button("Reconstruire / Modifier le Prefab Salariat"))
        {
            ReconstruirePrefab();
        }
    }

    private void ReconstruirePrefab()
    {
        string prefabPath = "Assets/prefabs/fenetre/Salariat.prefab";
        string sliderPrefabPath = "Assets/prefabs/sliderAvecTexte.prefab";

        GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject sliderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sliderPrefabPath);

        if (prefabAsset == null) { Debug.LogError("Prefab Salariat introuvable !"); return; }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
        
        try {
            // 1. Script Principal
            SalariatUI ui = instance.GetComponent<SalariatUI>();
            if (ui == null) ui = instance.AddComponent<SalariatUI>();

            // 2. Recherche du Panel Conteneur principal
            Transform rootPanel = instance.transform.Find("Panel_Poste8actuel");
            if (rootPanel == null) rootPanel = instance.transform;

            // 3. Gestion des Textes (recherche par nom)
            SetPrivateField(ui, "txtEntreprise", FindOrCreateTMP(rootPanel, "Entreprise", "Nom de l'entreprise"));
            SetPrivateField(ui, "txtAnciennete", FindOrCreateTMP(rootPanel, "Ancienneté", "0 mois"));
            SetPrivateField(ui, "txtSalaire", FindOrCreateTMP(rootPanel, "Salaire_brut", "0 € Brut"));
            SetPrivateField(ui, "txtHeures", FindOrCreateTMP(rootPanel, "Heures", "35h / semaine"));

            // 4. Gestion de la Jauge (instanciation du prefab slider)
            Transform jaugeObj = rootPanel.Find("JaugeSatisfaction");
            if (jaugeObj == null && sliderPrefab != null)
            {
                GameObject newSlider = (GameObject)PrefabUtility.InstantiatePrefab(sliderPrefab, rootPanel);
                newSlider.name = "JaugeSatisfaction";
                jaugeObj = newSlider.transform;
                jaugeObj.localPosition = new Vector3(0, -200, 0); // Position arbitraire
            }

            if (jaugeObj != null)
            {
                Slider s = jaugeObj.GetComponentInChildren<Slider>();
                SetPrivateField(ui, "jaugeSatisfaction", s);
                if (s != null && s.fillRect != null)
                    SetPrivateField(ui, "fillSatisfaction", s.fillRect.GetComponent<Image>());
            }

            // 5. Création d'un conteneur pour les boutons si absent
            Transform actionsRoot = instance.transform.Find("ActionsRapides");
            if (actionsRoot == null)
            {
                GameObject go = new GameObject("ActionsRapides", typeof(RectTransform));
                go.transform.SetParent(instance.transform, false);
                actionsRoot = go.transform;
                VerticalLayoutGroup vlg = go.AddComponent<VerticalLayoutGroup>();
                vlg.childControlHeight = true; vlg.childControlWidth = true;
                vlg.childForceExpandHeight = false;
                ((RectTransform)actionsRoot).anchoredPosition = new Vector2(400, 0);
                ((RectTransform)actionsRoot).sizeDelta = new Vector2(300, 600);
            }

            // 6. Création des 6 boutons
            SetPrivateField(ui, "btnTravaillerPlus", CreateButton(actionsRoot, "Btn_TravaillerPlus", "Travailler plus"));
            SetPrivateField(ui, "btnChercherEmploi", CreateButton(actionsRoot, "Btn_ChercherEmploi", "Chercher un emploi"));
            SetPrivateField(ui, "btnNegocierSalaire", CreateButton(actionsRoot, "Btn_NegocierSalaire", "Négocier salaire"));
            SetPrivateField(ui, "btnNetworking", CreateButton(actionsRoot, "Btn_Networking", "Networking"));
            SetPrivateField(ui, "btnFormation", CreateButton(actionsRoot, "Btn_Formation", "Formation"));
            SetPrivateField(ui, "btnDemissionner", CreateButton(actionsRoot, "Btn_Demissionner", "Démissionner"));

            // 7. Panel Recherche (Masqué)
            Transform searchPanel = instance.transform.Find("PanelOffres");
            if (searchPanel == null)
            {
                GameObject go = new GameObject("PanelOffres", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(instance.transform, false);
                go.GetComponent<Image>().color = new Color(0,0,0,0.8f);
                ((RectTransform)go.transform).anchorMin = Vector2.zero;
                ((RectTransform)go.transform).anchorMax = Vector2.one;
                ((RectTransform)go.transform).sizeDelta = Vector2.zero;
                searchPanel = go.transform;
                go.SetActive(false);
            }
            SetPrivateField(ui, "panelOffresEmploi", searchPanel.gameObject);

            // Sauvegarde
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Debug.Log("Prefab Salariat RECONSTRUIT avec succès !");
        }
        catch (System.Exception e) {
            Debug.LogError("Erreur lors de la reconstruction : " + e.Message);
        }
        finally {
            DestroyImmediate(instance);
        }
    }

    private TextMeshProUGUI FindOrCreateTMP(Transform parent, string name, string defaultText)
    {
        Transform t = parent.Find(name);
        if (t == null)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            t = go.transform;
        }
        TextMeshProUGUI tmp = t.GetComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        return tmp;
    }

    private Button CreateButton(Transform parent, string name, string label)
    {
        Transform t = parent.Find(name);
        if (t == null)
        {
            // Note: Simplification ici, dans un vrai projet on utiliserait un prefab de bouton
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            t = go.transform;
            
            GameObject txtGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(t, false);
            TextMeshProUGUI tmp = txtGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            ((RectTransform)txtGo.transform).anchorMin = Vector2.zero;
            ((RectTransform)txtGo.transform).anchorMax = Vector2.one;
            ((RectTransform)txtGo.transform).sizeDelta = Vector2.zero;
        }
        return t.GetComponent<Button>();
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        if (value == null) return;
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null) field.SetValue(target, value);
        else Debug.LogWarning("Champ non trouvé : " + fieldName);
    }
}
