using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // Singleton pour un accès global et simple depuis tes autres services du Noyau
    public static SaveManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private GameData gameData;

    private string cheminSauvegarde;

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
    }

    /// <summary>
    /// Sauvegarde l'état actuel de la partie sur le disque.
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
            PackageSauvegarde paquet = new PackageSauvegarde();
            paquet.nombreMoisPasses = gameData.nombreMoisPasses;
            paquet.moisActuel = gameData.moisActuel;

            // 1. On sérialise l'environnement et le joueur en chaînes JSON individuelles
            // JsonUtility va ignorer le dictionnaire 'comptes' automatiquement (ce qui nous arrange ici)
            paquet.envJson = JsonUtility.ToJson(gameData.env);
            paquet.joueurJson = JsonUtility.ToJson(gameData.joueur);

            // 2. On aplatit manuellement le dictionnaire de comptes pour conserver le type exact (ex: Epargne)
            paquet.comptesSauvegardes = new List<CompteSauvegardeDto>();
            if (gameData.joueur.comptes != null)
            {
                foreach (KeyValuePair<string, CompteBanquaire> kvp in gameData.joueur.comptes)
                {
                    if (kvp.Value == null) continue;

                    CompteSauvegardeDto dto = new CompteSauvegardeDto
                    {
                        cleDictionnaire = kvp.Key,
                        typeComplet = kvp.Value.GetType().AssemblyQualifiedName, // Sauvegarde la variante de classe exacte
                        donneesJson = JsonUtility.ToJson(kvp.Value) // Sérialise l'objet du compte
                    };
                    paquet.comptesSauvegardes.Add(dto);
                }
            }

            // 3. Conversion du paquet global en texte et écriture sur le disque
            string jsonGlobal = JsonUtility.ToJson(paquet, true);
            File.WriteAllText(cheminSauvegarde, jsonGlobal);

            Debug.Log($"[SaveManager] Partie sauvegardée avec succès dans : {cheminSauvegarde}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Échec lors de la sauvegarde : {e.Message}");
        }
    }

    /// <summary>
    /// Charge le fichier de sauvegarde et réinjecte les données dans le GameData de Runtime.
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
            // 1. Lecture du fichier JSON global
            string jsonGlobal = File.ReadAllText(cheminSauvegarde);
            PackageSauvegarde paquet = JsonUtility.FromJson<PackageSauvegarde>(jsonGlobal);

            // 2. Restauration des données primitives du calendrier
            gameData.nombreMoisPasses = paquet.nombreMoisPasses;
            gameData.moisActuel = paquet.moisActuel;

            // 3. Reconstruction des objets racine
            gameData.env = JsonUtility.FromJson<DonneesEnvironnement>(paquet.envJson);
            gameData.joueur = JsonUtility.FromJson<DonneesJoueur>(paquet.joueurJson);

            // 4. Reconstruction dynamique et typée du dictionnaire de comptes bancaires
            gameData.joueur.comptes = new Dictionary<string, CompteBanquaire>();
            foreach (CompteSauvegardeDto item in paquet.comptesSauvegardes)
            {
                Type typeExact = Type.GetType(item.typeComplet);
                if (typeExact != null)
                {
                    // L'astuce magique : on force JsonUtility à instancier le vrai type d'origine (ex: Epargne)
                    CompteBanquaire compteReconstruit = (CompteBanquaire)JsonUtility.FromJson(item.donneesJson, typeExact);
                    gameData.joueur.comptes.Add(item.cleDictionnaire, compteReconstruit);
                }
            }

            // 5. Nettoyage de l'historique What If et ré-initialisation métier
            if (gameData.historiqueSnapshots != null)
            {
                gameData.historiqueSnapshots.Clear();
            }
            gameData.joueur.InitialiserSiNecessaire();

            Debug.Log("[SaveManager] Partie chargée avec succès !");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Échec lors du chargement : {e.Message}");
            return false;
        }
    }
}

[Serializable]
public class PackageSauvegarde
{
    public int nombreMoisPasses;
    public Mois moisActuel;
    public string envJson;
    public string joueurJson;
    public List<CompteSauvegardeDto> comptesSauvegardes;
}

[Serializable]
public class CompteSauvegardeDto
{
    public string cleDictionnaire; 
    public string typeComplet;      
    public string donneesJson;     
}