using UnityEditor;
using UnityEngine;
using System;

public class ActualiteAssetCreator : EditorWindow
{
    [MenuItem("Tools/Créer des Actualités d'exemple")] // Adds a menu item under Tools
    public static void ShowWindow()
    {
        GetWindow<ActualiteAssetCreator>("Créateur d'Actualités");
    }

    private void OnGUI()
    {
        GUILayout.Label("Générer des Actualités d'exemple", EditorStyles.boldLabel);
        if (GUILayout.Button("Créer 3 Actualités"))
        {
            CreateSampleAssets();
        }
    }

    private static void CreateSampleAssets()
    {
        string folderPath = "Assets/Resources/Actualites";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Resources", "Actualites");
        }
        // 1. Public news
        ActualiteSO publicNews = ScriptableObject.CreateInstance<ActualiteSO>();
        publicNews.titre = "Lancement du nouveau produit";
        publicNews.description = "Le nouveau produit est disponible en magasin."
            + "\nDécouvrez ses caractéristiques innovantes et profiter d'une offre de lancement.";
        publicNews.dateDebut = DateTime.Now;
        publicNews.dateFin = DateTime.Now.AddMonths(1);
        publicNews.type = ActualiteSO.TypeActualite.Public;
        publicNews.impactFinancier = 5000;
        publicNews.actif = true;
        AssetDatabase.CreateAsset(publicNews, $"{folderPath}/PublicNews.asset");

        // 2. Private event
        ActualiteSO privateEvent = ScriptableObject.CreateInstance<ActualiteSO>();
        privateEvent.titre = "Réunion du conseil d'administration";
        privateEvent.description = "Discussion sur la stratégie de croissance pour le prochain trimestre.";
        privateEvent.dateDebut = DateTime.Now.AddDays(2);
        privateEvent.dateFin = DateTime.Now.AddDays(2).AddHours(2);
        privateEvent.type = ActualiteSO.TypeActualite.Private;
        privateEvent.impactFinancier = 0;
        privateEvent.actif = true;
        AssetDatabase.CreateAsset(privateEvent, $"{folderPath}/PrivateEvent.asset");

        // 3. Public announcement
        ActualiteSO announcement = ScriptableObject.CreateInstance<ActualiteSO>();
        announcement.titre = "Mise à jour du serveur";
        announcement.description = "Le serveur sera mis à jour ce week-end. Les joueurs peuvent s'attendre à de meilleures performances.";
        announcement.dateDebut = DateTime.Now.AddDays(5);
        announcement.dateFin = DateTime.Now.AddDays(5).AddHours(3);
        announcement.type = ActualiteSO.TypeActualite.Public;
        announcement.impactFinancier = 0;
        announcement.actif = true;
        AssetDatabase.CreateAsset(announcement, $"{folderPath}/ServerUpdate.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Actualités d'exemple créées dans " + folderPath);
    }
}
