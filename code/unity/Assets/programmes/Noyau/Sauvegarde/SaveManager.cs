using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Requis pour s'abonner aux changements de scènes
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

    private void Start()
    {
        LierBoutonSauvegarde();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LierBoutonSauvegarde();
    }

    private void LierBoutonSauvegarde()
    {
        GameObject buttonGo = GameObject.Find("SauvegardeBouton");
        if (buttonGo != null)
        {
            Button btn = buttonGo.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(SauvegarderPartie);
                btn.onClick.AddListener(SauvegarderPartie);
                Debug.Log("[SaveManager] Bouton Sauvegarde lié dynamiquement au Singleton actif.");
            }
        }
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
            Debug.LogWarning($"[SaveManager] Aucun fichier de sauvegarde trouvé à l'emplacement : {cheminAUtiliser}");
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
/// Force Newtonsoft à inclure les champs privés/protégés d'Unity et à ignorer les événements C#
/// </summary>
public class IgnoreEventsContractResolver : DefaultContractResolver
{
    protected override System.Collections.Generic.IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        // On force Newtonsoft à aller chercher TOUS les champs (publics, privés, protégés, instance)
        var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        var members = type.GetMembers(flags);
        
        var properties = new System.Collections.Generic.List<JsonProperty>();

        foreach (var member in members)
        {
            if (member.MemberType == MemberTypes.Field)
            {
                // On crée la propriété pour chaque champ trouvé
                var prop = CreateProperty(member, memberSerialization);
                if (prop != null)
                {
                    properties.Add(prop);
                }
            }
        }

        return properties; // Une List<T> implémente bien IList<T>, le compilateur accepte ça
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);
        
        // Configuration par défaut pour les champs non-publics détectés
        if (member is FieldInfo field && !field.IsPublic)
        {
            property.Readable = true;
            property.Writable = true;
        }

        // Sécurité anti-bug : Si la propriété est un délégué ou un événement C# (comme Action, Delegate, etc.)
        if (typeof(Delegate).IsAssignableFrom(property.PropertyType))
        {
            property.Ignored = true; 
        }
        
        return property;
    }
}