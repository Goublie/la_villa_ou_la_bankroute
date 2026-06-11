using UnityEngine;
using UnityEditor;
using TMPro;

public class AddChronoToFenetre
{
    [MenuItem("Tools/Add Chrono to Fenetre Prefab")]
    public static void Run()
    {
        string path = "Assets/prefabs/Noyau/Fenetre.prefab";
        GameObject prefab = PrefabUtility.LoadPrefabContents(path);
        
        if (prefab == null)
        {
            Debug.LogError("Could not load Fenetre.prefab at path: " + path);
            return;
        }

        // Verifier si le composant Chrono existe deja
        Transform existing = prefab.transform.Find("Chrono");
        if (existing != null)
        {
            Debug.Log("Chrono child already exists on Fenetre prefab.");
            PrefabUtility.UnloadPrefabContents(prefab);
            return;
        }

        // Creer l'objet enfant Chrono
        GameObject chronoGo = new GameObject("Chrono");
        chronoGo.transform.SetParent(prefab.transform, false);

        // Ajouter le RectTransform
        RectTransform rt = chronoGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.sizeDelta = new Vector2(250f, 50f);
        // Positionner le chrono a gauche du bouton de fermeture (cancel).
        // Le bouton de fermeture cancel faisant 35x35 avec un ancrage a 1,1 (pivot 0.5, 0.5),
        // nous decallons legerement le texte du Chrono pour eviter les chevauchements.
        rt.anchoredPosition = new Vector2(-70f, -10f);

        // Ajouter le composant TextMeshProUGUI
        TextMeshProUGUI text = chronoGo.AddComponent<TextMeshProUGUI>();
        text.text = "";
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Right;
        
        // Definir la police et la taille depuis le composant Titre pour conserver la coherence de style
        TextMeshProUGUI titleText = prefab.transform.Find("Titre")?.GetComponent<TextMeshProUGUI>();
        if (titleText != null)
        {
            text.font = titleText.font;
            text.fontSize = titleText.fontSize;
            text.color = new Color(0.8f, 0.1f, 0.1f); // rouge solide de style XP
        }
        else
        {
            text.color = Color.red;
        }

        PrefabUtility.SaveAsPrefabAsset(prefab, path);
        PrefabUtility.UnloadPrefabContents(prefab);
        Debug.Log("Successfully added Chrono component to Fenetre prefab!");
    }
}
