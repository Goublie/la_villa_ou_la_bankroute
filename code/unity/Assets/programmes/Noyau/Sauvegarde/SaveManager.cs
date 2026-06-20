using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class SaveManager : MonoBehaviour
{
    // Singleton pour un accès global et simple depuis tes autres services du Noyau
    public static SaveManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private GameData gameData;

    private string cheminSauvegarde;
    private JsonSerializerSettings jsonSettings;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Définit le chemin de sauvegarde persistant selon l'OS (Windows, Mac...)
        cheminSauvegarde = Path.Combine(Application.persistentDataPath, "sauvegarde_jeu.json");

        // Configuration pour que Newtonsoft gère l'héritage (ex: CompteBanquaire -> Epargne)
        jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented // JSON lisible pour vos tests
        };
    }

    /// <summary>
    /// Sauvegarde l'état actuel du jeu (Joueur, Environnement, Temps) sur le disque.
    /// </summary>
    public void SauvegarderPartie()
    {
        if (gameData == null)
        {
            Debug.LogError("[SaveManager] Impossible de sauvegarder : GameData n'est pas assigné !");
            return;
        }

        try
        {
            // On prépare le paquet sans l'historique des snapshots (What If) pour alléger le fichier
            PackageSauvegarde paquet = new PackageSauvegarde
            {
                joueur = gameData.joueur,
                env = gameData.env,
                nombreMoisPasses = gameData.nombreMoisPasses,
                moisActuel = gameData.moisActuel
            };

            // Conversion directe en texte JSON (gère les dictionnaires nativement)
            string json = JsonConvert.SerializeObject(paquet, jsonSettings);

            // Écriture physique sur le disque
            File.WriteAllText(cheminSauvegarde, json);
            Debug.Log($"[SaveManager] Partie sauvegardée avec succès dans : {cheminSauvegarde}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Échec de la sauvegarde : {e.Message}");
        }
    }

    /// <summary>
    /// Charge la sauvegarde depuis le disque et réinjecte les données dans le GameData.
    /// </summary>
    public bool ChargerPartie()
    {
        if (!File.Exists(cheminSauvegarde))
        {
            Debug.LogWarning("[SaveManager] Aucun fichier de sauvegarde trouvé.");
            return false;
        }

        try
        {
            // Lecture du fichier JSON
            string json = File.ReadAllText(cheminSauvegarde);

            // Reconstruction du paquet de données
            PackageSauvegarde paquet = JsonConvert.DeserializeObject<PackageSauvegarde>(json, jsonSettings);

            // Injection directe dans ton ScriptableObject de Runtime
            gameData.joueur = paquet.joueur;
            gameData.env = paquet.env;
            gameData.nombreMoisPasses = paquet.nombreMoisPasses;
            gameData.moisActuel = paquet.moisActuel;

            // Restauration des liaisons internes (méthode déjà présente dans ton DonneesJoueur.cs)
            gameData.joueur.InitialiserSiNecessaire();

            // On vide les anciens snapshots du What If car on reprend une nouvelle timeline
            if (gameData.historiqueSnapshots != null)
            {
                gameData.historiqueSnapshots.Clear();
            }

            Debug.Log("[SaveManager] Partie chargée et injectée avec succès !");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Échec du chargement : {e.Message}");
            return false;
        }
    }
}

/// <summary>
/// Structure intermédiaire contenant uniquement les données à écrire sur le disque.
/// </summary>
[Serializable]
public class PackageSauvegarde
{
    public DonneesJoueur joueur;
    public DonneesEnvironnement env;
    public int nombreMoisPasses;
    public Mois moisActuel;
}