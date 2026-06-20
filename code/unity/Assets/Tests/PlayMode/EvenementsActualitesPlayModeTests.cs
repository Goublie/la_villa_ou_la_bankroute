using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Verifie l'integration reelle des evenements dans Jeu et Actualites.
/// </summary>
public class EvenementsActualitesPlayModeTests
{
    private const string SceneMenu = "Menu";
    private const string SceneJeu = "Jeu";

    [UnitySetUp]
    public IEnumerator ReinitialiserPartieDansJeu()
    {
        yield return SceneManager.LoadSceneAsync(SceneJeu);
        yield return null;

        object gameData = ObtenirGameData();
        Invoquer(gameData, "ResetData");

        yield return SceneManager.LoadSceneAsync(SceneJeu);
        yield return null;
        yield return null;
    }

    [UnityTest]
    public IEnumerator Actualites_AfficheDeuxRumeursSansNouveauTirage()
    {
        object gameData = ObtenirGameData();
        object donnees = LireMembre(gameData, "evenements");
        int rumeursAvant = Compter(LireMembre(donnees, "rumeurs"));
        Component actualites = TrouverComposantDansScene(
            TrouverType("ActualitesUI"));

        actualites.gameObject.SetActive(true);
        Invoquer(actualites, "RafraichirDepuisHistorique");
        yield return null;
        yield return null;

        Assert.That(rumeursAvant, Is.EqualTo(2));
        Assert.That(
            Compter(LireMembre(donnees, "rumeurs")),
            Is.EqualTo(rumeursAvant));
        Assert.That(
            Compter(LireMembre(actualites, "actualitesList")),
            Is.EqualTo(2));
        Assert.That(
            Compter(LireMembre(donnees, "evenementsConfirmes")),
            Is.EqualTo(0));
        Assert.That(CompterLignesVisibles(actualites), Is.EqualTo(2));

        actualites.gameObject.SetActive(false);
        yield return null;
        actualites.gameObject.SetActive(true);
        yield return null;
        yield return null;

        Assert.That(
            Compter(LireMembre(donnees, "rumeurs")),
            Is.EqualTo(rumeursAvant));
        Assert.That(
            Compter(LireMembre(actualites, "actualitesList")),
            Is.EqualTo(2));
    }

    [UnityTest]
    public IEnumerator RechargementJeu_NeDupliqueNiTirageNiTraitementMensuel()
    {
        object avant = ObtenirGameData();
        object donneesAvant = LireMembre(avant, "evenements");
        uint etatAvant = (uint)LireMembre(donneesAvant, "etatAleatoire");

        yield return SceneManager.LoadSceneAsync(SceneJeu);
        yield return null;
        yield return null;

        object apres = ObtenirGameData();
        object donneesApres = LireMembre(apres, "evenements");
        Assert.That(Compter(LireMembre(donneesApres, "rumeurs")), Is.EqualTo(2));
        Assert.That(
            (int)LireMembre(donneesApres, "dernierMoisTraite"),
            Is.EqualTo(0));
        Assert.That(
            (uint)LireMembre(donneesApres, "etatAleatoire"),
            Is.EqualTo(etatAvant));
    }

    [UnityTest]
    public IEnumerator MenuJeuActualites_ResteNavigableEtConserveHistorique()
    {
        yield return SceneManager.LoadSceneAsync(SceneMenu);
        yield return null;
        Component jouer = TrouverBouton(SceneManager.GetActiveScene(), "Jouer");
        Invoquer(LireMembre(jouer, "onClick"), "Invoke");
        yield return AttendreScene(SceneJeu);

        object gameData = ObtenirGameData();
        object donnees = LireMembre(gameData, "evenements");
        Component actualites = TrouverComposantDansScene(
            TrouverType("ActualitesUI"));
        actualites.gameObject.SetActive(true);
        Invoquer(actualites, "RafraichirDepuisHistorique");
        yield return null;
        yield return null;

        Assert.That(Compter(LireMembre(donnees, "rumeurs")), Is.EqualTo(2));
        Assert.That(
            Compter(LireMembre(actualites, "actualitesList")),
            Is.EqualTo(2));
    }

    [UnityTest]
    public IEnumerator PlusieursMois_ProduisentDeuxRumeursEtUnTraitementParMois()
    {
        object gameData = ObtenirGameData();
        Type typeService = TrouverType("ServicePassageMensuel");
        object service = Activator.CreateInstance(
            typeService,
            new[] { gameData });

        Invoquer(service, "PasserAuMoisSuivant");
        Invoquer(service, "PasserAuMoisSuivant");
        Invoquer(service, "PasserAuMoisSuivant");
        object donnees = LireMembre(gameData, "evenements");
        object joueur = LireMembre(gameData, "joueur");
        object bourse = LireMembre(joueur, "bourse");

        Assert.That(Compter(LireMembre(donnees, "rumeurs")), Is.EqualTo(8));
        Assert.That(
            (int)LireMembre(donnees, "dernierMoisTraite"),
            Is.EqualTo(3));
        Assert.That(CompterRumeursEnAttente(donnees), Is.EqualTo(2));
        Assert.That(
            Compter(LireMembre(bourse, "impactsMarche")),
            Is.EqualTo(0));
        yield return null;
    }

    private static object ObtenirGameData()
    {
        Component controleur = TrouverComposantDansScene(
            TrouverType("PhaseRepartitionTempsController"));
        return LireMembre(controleur, "gameData");
    }

    private static int CompterRumeursEnAttente(object donnees)
    {
        IEnumerable rumeurs =
            (IEnumerable)LireMembre(donnees, "rumeurs");
        int total = 0;
        foreach (object rumeur in rumeurs)
        {
            if (LireMembre(rumeur, "etat").ToString() == "EnAttente")
            {
                total++;
            }
        }

        return total;
    }

    private static int CompterLignesVisibles(Component actualites)
    {
        object tableau = LireMembre(actualites, "tableauActualites");
        Assert.That(tableau, Is.Not.Null);
        IEnumerable lignes = (IEnumerable)LireMembre(tableau, "tableau");
        int total = 0;
        foreach (object ligne in lignes)
        {
            bool vide = (bool)Invoquer(ligne, "EstVide");
            if (!vide)
            {
                total++;
            }
        }

        return total;
    }

    private static int Compter(object collection)
    {
        return ((ICollection)collection).Count;
    }

    private static Component TrouverComposantDansScene(Type type)
    {
        Scene scene = SceneManager.GetActiveScene();
        UnityEngine.Object[] objets = Resources.FindObjectsOfTypeAll(type);
        foreach (UnityEngine.Object objet in objets)
        {
            Component composant = objet as Component;
            if (composant != null && composant.gameObject.scene == scene)
            {
                return composant;
            }
        }

        Assert.Fail("Composant introuvable dans la scene : " + type.Name);
        return null;
    }

    private static Component TrouverBouton(Scene scene, string nom)
    {
        Type typeBouton = TrouverType("UnityEngine.UI.Button");
        UnityEngine.Object[] objets = Resources.FindObjectsOfTypeAll(typeBouton);
        foreach (UnityEngine.Object objet in objets)
        {
            Component bouton = objet as Component;
            if (bouton != null &&
                bouton.gameObject.scene == scene &&
                bouton.name == nom)
            {
                return bouton;
            }
        }

        Assert.Fail("Bouton introuvable : " + nom);
        return null;
    }

    private static IEnumerator AttendreScene(string nom)
    {
        for (int frame = 0; frame < 180; frame++)
        {
            if (SceneManager.GetActiveScene().name == nom)
            {
                yield return null;
                yield return null;
                yield break;
            }
            yield return null;
        }

        Assert.Fail("Scene non chargee : " + nom);
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

    private static object LireMembre(object cible, string nom)
    {
        Type type = cible.GetType();
        FieldInfo champ = type.GetField(
            nom,
            BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.NonPublic);
        if (champ != null)
        {
            return champ.GetValue(cible);
        }

        PropertyInfo propriete = type.GetProperty(
            nom,
            BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.NonPublic);
        Assert.That(propriete, Is.Not.Null, "Membre introuvable : " + nom);
        return propriete.GetValue(cible);
    }

    private static object Invoquer(
        object cible,
        string nom,
        params object[] arguments)
    {
        Type[] types = new Type[arguments.Length];
        for (int index = 0; index < arguments.Length; index++)
        {
            types[index] = arguments[index].GetType();
        }

        MethodInfo methode = cible.GetType().GetMethod(
            nom,
            BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.NonPublic,
            null,
            types,
            null);
        Assert.That(methode, Is.Not.Null, "Methode introuvable : " + nom);
        return methode.Invoke(cible, arguments);
    }
}
