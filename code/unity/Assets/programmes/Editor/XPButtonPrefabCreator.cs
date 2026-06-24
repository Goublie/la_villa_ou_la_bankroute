using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class XPButtonPrefabCreator : EditorWindow
{
    [MenuItem("Tools/Créer le Préfab Bouton XP")]
    public static void CreerPrefab()
    {
        // 1. Configurer les paramètres d'importation des textures en Sprite avec bordures
        string[] paths = new string[] {
            "Assets/image/xp_button_normal.png",
            "Assets/image/xp_button_hover.png",
            "Assets/image/xp_button_pressed.png"
        };

        foreach (string path in paths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                // Définir des bordures de 3px sur tous les côtés pour un 9-slicing parfait
                importer.spriteBorder = new Vector4(3, 3, 3, 3);
                importer.SaveAndReimport();
            }
            else
            {
                Debug.LogWarning("Texture introuvable à : " + path);
            }
        }

        // 2. Créer l'objet bouton dans la hiérarchie temporaire
        GameObject buttonGo = new GameObject("XPButton");
        RectTransform buttonRt = buttonGo.AddComponent<RectTransform>();
        buttonRt.sizeDelta = new Vector2(100, 30); // taille par défaut standard

        Image image = buttonGo.AddComponent<Image>();
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/image/xp_button_normal.png");
        image.type = Image.Type.Sliced;

        Button button = buttonGo.AddComponent<Button>();
        button.transition = Selectable.Transition.SpriteSwap;
        
        SpriteState state = new SpriteState();
        state.highlightedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/image/xp_button_hover.png");
        state.pressedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/image/xp_button_pressed.png");
        state.selectedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/image/xp_button_hover.png");
        button.spriteState = state;

        // 3. Ajouter l'enfant de texte (TextMeshPro)
        GameObject textGo = new GameObject("Text (TMP)");
        textGo.transform.SetParent(buttonGo.transform, false);
        
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero; // Remplissage étiré

        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = "Bouton";
        tmp.fontSize = 14;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;

        // 4. Enregistrer sous forme de Prefab
        string prefabFolder = "Assets/prefabs";
        if (!Directory.Exists(Path.Combine(Application.dataPath, "prefabs")))
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "prefabs"));
        }

        string prefabPath = prefabFolder + "/XPButton.prefab";
        PrefabUtility.SaveAsPrefabAsset(buttonGo, prefabPath);
        
        // Nettoyer la hiérarchie de la scène
        Object.DestroyImmediate(buttonGo);

        Debug.Log("Le préfab Bouton XP a été créé avec succès à : " + prefabPath);
        EditorUtility.DisplayDialog("Création du Prefab", "Le préfab XPButton a été généré avec succès dans Assets/prefabs !", "Génial !");
    }
}
