using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization; // Requis pour bloquer les événements C#

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private GameData gameData;

    private string cheminSauvegarde;
    private string cheminTemporaire;
    private string cheminBackup;
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

        // Chemins physiques validés sur Windows
        cheminSauvegarde = Path.Combine(Application.persistentDataPath, "sauvegarde_jeu.json");
        cheminTemporaire = cheminSauvegarde + ".tmp";
        cheminBackup = cheminSauvegarde + ".bak";

        // Configuration Newtonsoft avec parade anti-bug d'événements (Antigravity Check)
        jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            ContractResolver = new IgnoreEventsContractResolver() // Sécurité totale pour OnSoldeModifie
        };
    }

    /// <summary>
    /// Sauvegarde l'état du jeu de manière ATOMIQUE (Anti-corruption de fichier)
    /// </summary>
    public void SauvegarderPartie()
    {
        if (gameData == null) return;

        try
        {
            PackageSauvegarde paquet = new PackageSauvegarde
            {
                joueur = gameData.joueur,
                env = gameData.env,
                nombreMoisPasses = gameData.nombreMoisPasses,
                moisActuel = gameData.moisActuel
            };

            string json = JsonConvert.SerializeObject(paquet, jsonSettings);

            // 1. Écriture sécurisée dans le fichier temporaire
            File.WriteAllText(cheminTemporaire, json);

            if (!File.Exists(cheminTemporaire) || new FileInfo(cheminTemporaire).Length == 0)
            {
                throw new IOException("Fichier temporaire vide. Écriture avortée (Espace disque plein ?)");
            }

            // 2. Permutation atomique des fichiers pour éviter les fichiers tronqués au crash
            if (File.Exists(cheminSauvegarde))
            {
                if (File.Exists(cheminBackup)) File.Delete(cheminBackup);
                File.Move(cheminSauvegarde, cheminBackup);
            }

            File.Move(cheminTemporaire, cheminSauvegarde);

            if (File.Exists(cheminBackup)) File.Delete(cheminBackup);

            Debug.Log($"[SaveManager] Sauvegarde réussie dans : {cheminSauvegarde}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Échec de la sauvegarde (Données préservées) : {e.Message}");
            if (File.Exists(cheminTemporaire)) File.Delete(cheminTemporaire);
        }
    }

    /// <summary>
    /// Charge la sauvegarde avec vérification de validité
    /// </summary>
    public bool ChargerPartie()
    {
        string cheminAUtiliser = cheminSauvegarde;
        if (!File.Exists(cheminSauvegarde) && File.Exists(cheminBackup))
        {
            cheminAUtiliser = cheminBackup;
        }

        if (!File.Exists(cheminAUtiliser))
        {
            Debug.LogWarning("[SaveManager] Aucun fichier de sauvegarde trouvé.");
            return false;
        }

        try
        {
            string json = File.ReadAllText(cheminAUtiliser);

            if (string.IsNullOrEmpty(json)) throw new Exception("Fichier de sauvegarde vide.");

            PackageSauvegarde paquet = JsonConvert.DeserializeObject<PackageSauvegarde>(json, jsonSettings);

            if (paquet == null || paquet.joueur == null) throw new Exception("JSON corrompu.");

            // Injection des données rechargées
            gameData.nombreMoisPasses = paquet.nombreMoisPasses;
            gameData.moisActuel = paquet.moisActuel;
            gameData.env = paquet.env;
            gameData.joueur = paquet.joueur;

            // Recréation des abonnements propres (comme spécifié dans DonneesJoueur.cs)
            gameData.joueur.InitialiserSiNecessaire();

            if (gameData.historiqueSnapshots != null) gameData.historiqueSnapshots.Clear();

            Debug.Log("[SaveManager] Partie chargée avec succès !");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Échec du chargement : {e.Message}");
            return false;
        }
    }
}

[Serializable]
public class PackageSauvegarde
{
    public DonneesJoueur joueur;
    public DonneesEnvironnement env;
    public int nombreMoisPasses;
    public Mois moisActuel;
}

/// <summary>
/// Force Newtonsoft à ignorer TOUS les événements C# pour éviter les crashs de sérialisation
/// </summary>
public class IgnoreEventsContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
        
        // Si la propriété est un délégué ou un événement C# (comme Action, Delegate, etc.)
        if (typeof(Delegate).IsAssignableFrom(property.PropertyType))
        {
            property.Ignored = true; // Newtonsoft l'ignore proprement
        }
        return property;
    }
}