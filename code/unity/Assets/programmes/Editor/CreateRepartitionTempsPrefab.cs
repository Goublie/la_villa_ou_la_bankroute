using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using XCharts.Runtime; // Required for PieChart

public class CreateRepartitionTempsPrefab
{
    [MenuItem("Tools/Generate RepartitionTemps Prefab")]
    public static void Generate()
    {
        string basePrefabPath = "Assets/prefabs/Noyau/Fenetre.prefab";
        GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePrefabPath);
        if (basePrefab == null)
        {
            Debug.LogError("Base prefab Fenetre not found!");
            return;
        }

        // Create the variant instance in memory
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
        instance.name = "RepartitionTemps";

        // Find the "Fond" child where UI elements go
        Transform fondTransform = instance.transform.Find("Fond");
        if (fondTransform == null)
        {
            Debug.LogError("Fond not found in Fenetre prefab!");
            Object.DestroyImmediate(instance);
            return;
        }

        // Modify the window title (No accents)
        Transform titleTransform = instance.transform.Find("Titre");
        if (titleTransform != null)
        {
            TextMeshProUGUI titleTxt = titleTransform.GetComponent<TextMeshProUGUI>();
            if (titleTxt != null)
            {
                titleTxt.text = "Gestion du Temps";
            }
        }

        // Create a main container inside Fond to hold the layout components
        GameObject container = new GameObject("TimeAllocationContent", typeof(RectTransform));
        container.transform.SetParent(fondTransform, false);
        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 0);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.anchoredPosition = new Vector2(0, -25);
        containerRect.sizeDelta = new Vector2(-100, -100); // Margins adjusted since bottom button is removed

        // Add a vertical layout group to organize sections (Description, Content)
        VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(25, 25, 25, 25);
        layout.spacing = 30;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        // Section 1: Description Text (No accents)
        GameObject descGo = new GameObject("DescriptionText", typeof(RectTransform));
        descGo.transform.SetParent(container.transform, false);
        TextMeshProUGUI descText = descGo.AddComponent<TextMeshProUGUI>();
        if (titleTransform != null)
        {
            TextMeshProUGUI titleTxt = titleTransform.GetComponent<TextMeshProUGUI>();
            descText.font = titleTxt.font;
            descText.fontSharedMaterial = titleTxt.fontSharedMaterial;
        }
        descText.text = "Veuillez repartir votre temps mensuel (30 minutes) entre vos differentes applications avant de lancer le mois.";
        descText.fontSize = 28;
        descText.color = Color.black;
        descText.alignment = TextAlignmentOptions.Center;
        
        LayoutElement descLayout = descGo.AddComponent<LayoutElement>();
        descLayout.preferredHeight = 70;
        descLayout.flexibleHeight = 0;

        // Section 2: Center Content (Horizontal: Left list of sliders, Right info/chart panel)
        GameObject centerGo = new GameObject("CenterContent", typeof(RectTransform));
        centerGo.transform.SetParent(container.transform, false);
        HorizontalLayoutGroup centerLayout = centerGo.AddComponent<HorizontalLayoutGroup>();
        centerLayout.spacing = 60;
        centerLayout.childAlignment = TextAnchor.MiddleCenter;
        centerLayout.childControlHeight = true;
        centerLayout.childControlWidth = true;
        centerLayout.childForceExpandHeight = true;
        centerLayout.childForceExpandWidth = false;
        
        LayoutElement centerLayoutElement = centerGo.AddComponent<LayoutElement>();
        centerLayoutElement.preferredHeight = 420; // Slightly taller
        centerLayoutElement.flexibleHeight = 1;

        // Left Container: Sliders List
        GameObject slidersGo = new GameObject("SlidersList", typeof(RectTransform));
        slidersGo.transform.SetParent(centerGo.transform, false);
        VerticalLayoutGroup slidersLayout = slidersGo.AddComponent<VerticalLayoutGroup>();
        slidersLayout.spacing = 20;
        slidersLayout.childAlignment = TextAnchor.MiddleCenter;
        slidersLayout.childControlHeight = true;
        slidersLayout.childControlWidth = true;
        slidersLayout.childForceExpandHeight = false;
        slidersLayout.childForceExpandWidth = true;
        
        LayoutElement slidersLayoutElement = slidersGo.AddComponent<LayoutElement>();
        slidersLayoutElement.preferredWidth = 750;
        slidersLayoutElement.flexibleWidth = 2;
        slidersLayoutElement.flexibleHeight = 1;

        // Load the slider prefab
        string sliderPrefabPath = "Assets/prefabs/UI/sliderAvecTexte.prefab";
        GameObject sliderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sliderPrefabPath);

        string[] apps = { "Banque", "Actualites", "Salariat", "Bourse" };
        foreach (string appName in apps)
        {
            GameObject sliderInstance;
            if (sliderPrefab != null)
            {
                sliderInstance = (GameObject)PrefabUtility.InstantiatePrefab(sliderPrefab, slidersGo.transform);
                sliderInstance.name = "Slider_" + appName;
                
                LayoutElement rowLayout = sliderInstance.GetComponent<LayoutElement>();
                if (rowLayout == null) rowLayout = sliderInstance.AddComponent<LayoutElement>();
                rowLayout.preferredHeight = 60;
                rowLayout.flexibleHeight = 0;

                // Set text label and make font size look good
                Transform labelT = sliderInstance.transform.Find("Text (TMP)");
                if (labelT != null)
                {
                    TextMeshProUGUI lbl = labelT.GetComponent<TextMeshProUGUI>();
                    if (lbl != null)
                    {
                        lbl.text = appName + " (minutes)";
                        lbl.fontSize = 28;
                        lbl.color = Color.black;
                    }
                }
            }
            else
            {
                sliderInstance = new GameObject("Slider_" + appName, typeof(RectTransform));
                sliderInstance.transform.SetParent(slidersGo.transform, false);
            }
        }

        // Right Container: Info/PieChart
        GameObject rightGo = new GameObject("InfoPanel", typeof(RectTransform));
        rightGo.transform.SetParent(centerGo.transform, false);
        VerticalLayoutGroup rightLayout = rightGo.AddComponent<VerticalLayoutGroup>();
        rightLayout.childAlignment = TextAnchor.MiddleCenter;
        rightLayout.spacing = 20;
        rightLayout.childControlHeight = true;
        rightLayout.childControlWidth = true;
        rightLayout.childForceExpandHeight = false;
        rightLayout.childForceExpandWidth = true;
        
        LayoutElement rightLayoutElement = rightGo.AddComponent<LayoutElement>();
        rightLayoutElement.preferredWidth = 350;
        rightLayoutElement.flexibleWidth = 1;
        rightLayoutElement.flexibleHeight = 1;

        // Section 2b: XCharts PieChart
        GameObject chartGo = new GameObject("PieChart", typeof(RectTransform));
        chartGo.transform.SetParent(rightGo.transform, false);
        PieChart pieChart = chartGo.AddComponent<PieChart>();
        
        // Configure Pie Chart
        pieChart.EnsureChartComponent<Title>().show = false; // Hide chart title
        
        var legend = pieChart.EnsureChartComponent<Legend>();
        legend.show = true;
        legend.orient = Orient.Horizonal;
        
        // Remove default sample data and setup our 4 categories
        pieChart.ClearData();
        while (pieChart.series.Count < 1)
        {
            pieChart.AddSerie<Pie>();
        }
        while (pieChart.series.Count > 1)
        {
            pieChart.series.RemoveAt(pieChart.series.Count - 1);
        }
        var serie = pieChart.series[0];
        serie.serieName = "RepartitionTemps";
        serie.show = true;
        
        pieChart.AddData(0, 0, "Banque");
        pieChart.AddData(0, 0, "Actualites");
        pieChart.AddData(0, 0, "Salariat");
        pieChart.AddData(0, 0, "Bourse");
        
        LayoutElement chartLayoutElement = chartGo.AddComponent<LayoutElement>();
        chartLayoutElement.preferredWidth = 260;
        chartLayoutElement.preferredHeight = 260;
        chartLayoutElement.flexibleHeight = 0;
        chartLayoutElement.flexibleWidth = 0;

        // Total Time text (No accents)
        GameObject totalGo = new GameObject("TotalText", typeof(RectTransform));
        totalGo.transform.SetParent(rightGo.transform, false);
        TextMeshProUGUI totalText = totalGo.AddComponent<TextMeshProUGUI>();
        if (titleTransform != null)
        {
            TextMeshProUGUI titleTxt = titleTransform.GetComponent<TextMeshProUGUI>();
            totalText.font = titleTxt.font;
            totalText.fontSharedMaterial = titleTxt.fontSharedMaterial;
        }
        totalText.text = "Temps alloue : 0 / 30 min";
        totalText.fontSize = 28;
        totalText.color = Color.black;
        totalText.alignment = TextAlignmentOptions.Center;
        
        LayoutElement totalLayout = totalGo.AddComponent<LayoutElement>();
        totalLayout.preferredHeight = 45;
        totalLayout.flexibleHeight = 0;

        // Save as Prefab Variant
        string outDir = "Assets/prefabs/Apps/RepartitionTemps";
        if (!System.IO.Directory.Exists(outDir))
        {
            System.IO.Directory.CreateDirectory(outDir);
        }
        string outputPath = outDir + "/RepartitionTemps.prefab";
        
        // Save the variant
        PrefabUtility.SaveAsPrefabAsset(instance, outputPath);
        Debug.Log("Successfully created Prefab Variant at: " + outputPath);

        // Cleanup instance in scene
        Object.DestroyImmediate(instance);

        AssetDatabase.Refresh();
    }
}
