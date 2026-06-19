using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Verifie l'integrite du Menu et la persistance des gestionnaires globaux.
/// </summary>
public class NavigationMenuPlayModeTests
{
    private const string SceneMenu = "Menu";
    private const string SceneJeu = "Jeu";
    private const string SceneGameOver = "GameOver";
    private const int FramesStabilisationMenu = 2;

    [UnitySetUp]
    public IEnumerator ChargerMenu()
    {
        yield return ChargerSceneMenu();
    }

    [UnityTest]
    public IEnumerator Menu_EstCompletEtRelieAuxGestionnairesUniques()
    {
        Scene menu = SceneManager.GetActiveScene();
        Assert.That(menu.name, Is.EqualTo(SceneMenu));
        VerifierAucunScriptManquant(menu);

        Component jouer = TrouverBouton(menu, "Jouer");
        Component quitter = TrouverBouton(menu, "Quitter");
        Component options = TrouverBouton(menu, "Options");
        Assert.That(LirePropriete(jouer, "interactable"), Is.True);
        Assert.That(LirePropriete(quitter, "interactable"), Is.True);
        Assert.That(LirePropriete(options, "interactable"), Is.True);
        AssertGestionnairesUniques();

        LogAssert.Expect(LogType.Log, "ScenesManager : Fermeture.");
        Cliquer(quitter);
        yield return null;
        AssertGestionnairesUniques();
    }

    [UnityTest]
    public IEnumerator Navigation_MenuJeuGameOverMenuJeu_ResteFonctionnelle()
    {
        Component jouer = TrouverBouton(
            SceneManager.GetActiveScene(),
            "Jouer");
        Cliquer(jouer);
        yield return AttendreScene(SceneJeu);
        AssertGestionnairesUniques();

        yield return SceneManager.LoadSceneAsync(SceneGameOver);
        yield return null;
        Assert.That(
            SceneManager.GetActiveScene().name,
            Is.EqualTo(SceneGameOver));
        AssertGestionnairesUniques();

        Component retourMenu = TrouverBouton(
            SceneManager.GetActiveScene(),
            "BoutonRetourMenu");
        yield return RetournerAuMenu(retourMenu);
        AssertGestionnairesUniques();

        Component jouerApresRetour = TrouverBouton(
            SceneManager.GetActiveScene(),
            "Jouer");
        Assert.That(
            LirePropriete(jouerApresRetour, "interactable"),
            Is.True);
        Cliquer(jouerApresRetour);
        yield return AttendreScene(SceneJeu);
        AssertGestionnairesUniques();
    }

    [UnityTest]
    public IEnumerator Audio_PlusieursRetoursMenu_NeMultiplientPasLesListeners()
    {
        Type typeAudioManager = TrouverType("AudioManager");
        Component audioManager = TrouverComposantUnique(typeAudioManager);
        int instanceId = audioManager.GetInstanceID();
        int compteurInitial = (int)LirePropriete(
            audioManager,
            "NombreClicsTraites");

        for (int index = 0; index < 3; index++)
        {
            yield return ChargerSceneMenu();
            AssertGestionnairesUniques();
            Assert.That(
                TrouverComposantUnique(typeAudioManager).GetInstanceID(),
                Is.EqualTo(instanceId));
        }

        Cliquer(TrouverBouton(SceneManager.GetActiveScene(), "Options"));
        yield return null;
        Assert.That(
            LirePropriete(audioManager, "NombreClicsTraites"),
            Is.EqualTo(compteurInitial + 1));

        yield return ChargerSceneMenu();
        Cliquer(TrouverBouton(SceneManager.GetActiveScene(), "Options"));
        yield return null;
        Assert.That(
            LirePropriete(audioManager, "NombreClicsTraites"),
            Is.EqualTo(compteurInitial + 2));
        AssertGestionnairesUniques();
    }

    private static IEnumerator ChargerSceneMenu()
    {
        AsyncOperation chargement = SceneManager.LoadSceneAsync(SceneMenu);
        while (!chargement.isDone)
        {
            yield return null;
        }

        yield return AttendreStabilisationMenu();
    }

    private static IEnumerator RetournerAuMenu(Component boutonRetour)
    {
        Cliquer(boutonRetour);
        yield return AttendreScene(SceneMenu);
        yield return AttendreStabilisationMenu();
    }

    private static IEnumerator AttendreStabilisationMenu()
    {
        for (int frame = 0; frame < FramesStabilisationMenu; frame++)
        {
            yield return null;
        }
    }

    private static IEnumerator AttendreScene(string nomScene)
    {
        const int nombreFramesMaximum = 180;
        for (int frame = 0; frame < nombreFramesMaximum; frame++)
        {
            if (SceneManager.GetActiveScene().name == nomScene)
            {
                yield return null;
                yield break;
            }

            yield return null;
        }

        Assert.Fail("La scene n'a pas ete chargee : " + nomScene);
    }

    private static void AssertGestionnairesUniques()
    {
        Assert.That(
            CompterComposants(TrouverType("ScenesManager")),
            Is.EqualTo(1),
            "Le nombre de ScenesManager actifs doit rester egal a un.");
        Assert.That(
            CompterComposants(TrouverType("AudioManager")),
            Is.EqualTo(1),
            "Le nombre d'AudioManager actifs doit rester egal a un.");
    }

    private static int CompterComposants(Type type)
    {
        int total = 0;
        UnityEngine.Object[] objets = Resources.FindObjectsOfTypeAll(type);
        foreach (UnityEngine.Object objet in objets)
        {
            Behaviour comportement = objet as Behaviour;
            if (comportement != null &&
                comportement.gameObject.scene.IsValid() &&
                comportement.isActiveAndEnabled)
            {
                total++;
            }
        }

        return total;
    }

    private static Component TrouverComposantUnique(Type type)
    {
        Component resultat = null;
        UnityEngine.Object[] objets = Resources.FindObjectsOfTypeAll(type);
        foreach (UnityEngine.Object objet in objets)
        {
            Behaviour comportement = objet as Behaviour;
            if (comportement == null ||
                !comportement.gameObject.scene.IsValid() ||
                !comportement.isActiveAndEnabled)
            {
                continue;
            }

            Assert.That(
                resultat,
                Is.Null,
                "Plusieurs composants actifs trouves : " + type.Name);
            resultat = comportement;
        }

        Assert.That(resultat, Is.Not.Null, "Composant absent : " + type.Name);
        return resultat;
    }

    private static Component TrouverBouton(Scene scene, string nomObjet)
    {
        Type typeBouton = TrouverType("UnityEngine.UI.Button");
        UnityEngine.Object[] objets = Resources.FindObjectsOfTypeAll(typeBouton);
        foreach (UnityEngine.Object objet in objets)
        {
            Component bouton = objet as Component;
            if (bouton != null &&
                bouton.gameObject.scene == scene &&
                bouton.name == nomObjet)
            {
                return bouton;
            }
        }

        Assert.Fail("Bouton introuvable : " + nomObjet);
        return null;
    }

    private static void Cliquer(Component bouton)
    {
        object evenement = LirePropriete(bouton, "onClick");
        Invoquer(evenement, "Invoke");
    }

    private static void VerifierAucunScriptManquant(Scene scene)
    {
        foreach (GameObject racine in scene.GetRootGameObjects())
        {
            Transform[] objets = racine.GetComponentsInChildren<Transform>(true);
            foreach (Transform objet in objets)
            {
                Component[] composants = objet.GetComponents<Component>();
                for (int index = 0; index < composants.Length; index++)
                {
                    Assert.That(
                        composants[index],
                        Is.Not.Null,
                        "Script manquant sur " + ObtenirChemin(objet));
                }
            }
        }
    }

    private static string ObtenirChemin(Transform objet)
    {
        string chemin = objet.name;
        Transform parent = objet.parent;
        while (parent != null)
        {
            chemin = parent.name + "/" + chemin;
            parent = parent.parent;
        }

        return chemin;
    }

    private static Type TrouverType(string nom)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(nom);
            if (type != null)
            {
                return type;
            }
        }

        Assert.Fail("Type introuvable : " + nom);
        return null;
    }

    private static object LirePropriete(object cible, string nom)
    {
        PropertyInfo propriete = cible.GetType().GetProperty(
            nom,
            BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.NonPublic);
        Assert.That(
            propriete,
            Is.Not.Null,
            "Propriete introuvable : " + nom);
        return propriete.GetValue(cible);
    }

    private static object Invoquer(
        object cible,
        string nom,
        params object[] arguments)
    {
        Type[] typesArguments = new Type[arguments.Length];
        for (int index = 0; index < arguments.Length; index++)
        {
            typesArguments[index] = arguments[index].GetType();
        }

        MethodInfo methode = cible.GetType().GetMethod(
            nom,
            BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.NonPublic,
            null,
            typesArguments,
            null);
        Assert.That(methode, Is.Not.Null, "Methode introuvable : " + nom);
        return methode.Invoke(cible, arguments);
    }

}
