using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Écoute chaque chargement de scène et installe le tableau runtime.
/// La résolution de GameData réutilise en priorité les références déjà
/// sérialisées dans les composants de la scène.
/// </summary>
public static class BootstrapRetrospectionTableauRuntimeUI
{
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitialiserEcoute()
    {
        SceneManager.sceneLoaded -= SurSceneChargee;
        SceneManager.sceneLoaded += SurSceneChargee;
    }

    private static void SurSceneChargee(
        Scene scene,
        LoadSceneMode mode)
    {
        if (!string.Equals(
            scene.name,
            "Retrospective",
            StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Transform panneau =
            TrouverDansScene(
                scene,
                "PanneauTableaux");

        if (panneau == null)
        {
            Debug.LogWarning(
                "[What-If] PanneauTableaux introuvable.");
            return;
        }

        GameData donnees = ResoudreGameData(scene);

        RetrospectionTableauUI[] anciens =
            Resources.FindObjectsOfTypeAll<
                RetrospectionTableauUI>();

        foreach (RetrospectionTableauUI ancien in anciens)
        {
            if (ancien != null &&
                ancien.gameObject.scene == scene)
            {
                ancien.enabled = false;
            }
        }

        RetrospectionTableauRuntimeUI runtime =
            panneau.GetComponent<
                RetrospectionTableauRuntimeUI>();

        if (runtime == null)
        {
            runtime = panneau.gameObject.AddComponent<
                RetrospectionTableauRuntimeUI>();
        }

        runtime.Initialiser(donnees);

        if (donnees == null)
        {
            Debug.LogWarning(
                "[What-If] GameData reste introuvable pour la rétrospection.");
        }
        else
        {
            Debug.Log(
                "[What-If] GameData récupéré pour le tableau de rétrospection.");
        }
    }

    private static GameData ResoudreGameData(Scene scene)
    {
        RetrospectionGraphiqueUI[] graphiques =
            Resources.FindObjectsOfTypeAll<
                RetrospectionGraphiqueUI>();

        foreach (RetrospectionGraphiqueUI graphique in graphiques)
        {
            if (graphique != null &&
                graphique.gameObject.scene == scene &&
                graphique.gameData != null)
            {
                return graphique.gameData;
            }
        }

        RetrospectionEvenements[] evenements =
            Resources.FindObjectsOfTypeAll<
                RetrospectionEvenements>();

        foreach (RetrospectionEvenements evenement in evenements)
        {
            if (evenement != null &&
                evenement.gameObject.scene == scene &&
                evenement.gameData != null)
            {
                return evenement.gameData;
            }
        }

        RetrospectionTableauUI[] tableaux =
            Resources.FindObjectsOfTypeAll<
                RetrospectionTableauUI>();

        foreach (RetrospectionTableauUI tableau in tableaux)
        {
            if (tableau != null &&
                tableau.gameObject.scene == scene &&
                tableau.gameData != null)
            {
                return tableau.gameData;
            }
        }

        GameData ressource =
            Resources.Load<GameData>("GameData");

        if (ressource != null)
        {
            return ressource;
        }

        GameData[] charges =
            Resources.FindObjectsOfTypeAll<GameData>();

        foreach (GameData charge in charges)
        {
            if (charge != null)
            {
                return charge;
            }
        }

        return null;
    }

    private static Transform TrouverDansScene(
        Scene scene,
        string nom)
    {
        GameObject[] racines =
            scene.GetRootGameObjects();

        foreach (GameObject racine in racines)
        {
            Transform resultat =
                TrouverRecursivement(
                    racine.transform,
                    nom);

            if (resultat != null)
            {
                return resultat;
            }
        }

        return null;
    }

    private static Transform TrouverRecursivement(
        Transform parent,
        string nom)
    {
        if (string.Equals(
            parent.gameObject.name,
            nom,
            StringComparison.OrdinalIgnoreCase))
        {
            return parent;
        }

        for (int index = 0;
            index < parent.childCount;
            index++)
        {
            Transform resultat =
                TrouverRecursivement(
                    parent.GetChild(index),
                    nom);

            if (resultat != null)
            {
                return resultat;
            }
        }

        return null;
    }
}