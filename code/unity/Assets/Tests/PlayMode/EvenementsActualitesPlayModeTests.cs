using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
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
        int publicationsAvant = Compter(
            LireMembre(donnees, "publications"));
        uint aleatoireAvant = (uint)LireMembre(donnees, "etatAleatoire");
        AutoriserActualites(gameData);
        Component actualites = TrouverComposantDansScene(
            TrouverType("ActualitesUI"));

        actualites.gameObject.SetActive(true);
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
            Compter(LireMembre(donnees, "publications")),
            Is.EqualTo(publicationsAvant));
        Assert.That(
            (uint)LireMembre(donnees, "etatAleatoire"),
            Is.EqualTo(aleatoireAvant));
        Assert.That(
            Compter(LireMembre(actualites, "actualitesList")),
            Is.EqualTo(2));
        Assert.That(CompterLignesVisibles(actualites), Is.EqualTo(2));
    }

    [UnityTest]
    public IEnumerator Actualites_DateEtSelection_SuiventLeHudEtLaLigneCliquee()
    {
        Component actualites = OuvrirEtRafraichirActualites();
        yield return null;
        yield return null;

        Component hud = TrouverComposantDansScene(TrouverType("HUDManager"));
        object texteMois = LireMembre(hud, "texteMois");
        string dateHud = (string)LireMembre(texteMois, "text");
        IList items = (IList)LireMembre(actualites, "actualitesList");
        Assert.That(items, Has.Count.EqualTo(2));
        foreach (object item in items)
        {
            Assert.That((string)LireMembre(item, "date"), Is.EqualTo(dateHud));
        }

        List<Component> lignes = ObtenirLignesVisibles(actualites);
        Assert.That(lignes, Has.Count.EqualTo(2));
        object tableau = LireMembre(actualites, "tableauSelectable");
        Assert.That(
            Invoquer(tableau, "GetLigneSelectionnee"),
            Is.SameAs(lignes[0]));
        Assert.That(
            LireTexte(LireMembre(actualites, "txtDetailTitre")),
            Is.EqualTo((string)Invoquer(lignes[0], "Get", 2)));

        Cliquer(lignes[1]);
        yield return null;

        Assert.That(
            Invoquer(tableau, "GetLigneSelectionnee"),
            Is.SameAs(lignes[1]));
        Assert.That(
            LireTexte(LireMembre(actualites, "txtDetailTitre")),
            Is.EqualTo((string)Invoquer(lignes[1], "Get", 2)));
    }

    [UnityTest]
    public IEnumerator Actualites_EvenementConfirme_AfficheImportanceEtEffetsDeclares()
    {
        object gameData = ObtenirGameData();
        object donnees = LireMembre(gameData, "evenements");
        int mois = (int)LireMembre(gameData, "nombreMoisPasses");
        AjouterEvenementConfirmeTest(donnees, mois);

        Component actualites = OuvrirEtRafraichirActualites();
        yield return null;
        yield return null;

        Component ligne = TrouverLigneParTitre(actualites, "Événement UI confirmé");
        Assert.That(ligne, Is.Not.Null);
        Cliquer(ligne);
        yield return null;

        string description = LireTexte(
            LireMembre(actualites, "txtDetailDescription"));
        string effets = LireTexte(LireMembre(actualites, "txtDetailEffets"));
        Assert.That(description, Does.Contain("Importance : Forte"));
        Assert.That(description, Does.Contain("Catégorie : Boursiers"));
        Assert.That(effets, Does.Contain("Effets sur la partie"));
        Assert.That(effets, Does.Contain("Nvidia"));
        Assert.That(effets, Does.Contain("+12.5 %"));
    }

    [UnityTest]
    public IEnumerator Actualites_CategoriesReelles_FiltrentSansAncienLibelle()
    {
        object gameData = ObtenirGameData();
        object donnees = LireMembre(gameData, "evenements");
        int mois = (int)LireMembre(gameData, "nombreMoisPasses");
        AjouterPublicationRumeurTest(
            donnees,
            mois,
            "test-boursiers",
            "Boursiers");
        AjouterPublicationRumeurTest(
            donnees,
            mois,
            "test-personnels",
            "Personnels");
        AjouterPublicationRumeurTest(
            donnees,
            mois,
            "test-professionnels",
            "Professionnels");

        Component actualites = OuvrirEtRafraichirActualites();
        yield return null;
        yield return null;

        CollectionAssert.IsSubsetOf(
            new[] { "Toutes", "Boursiers", "Personnels", "Professionnels" },
            ObtenirTextesBoutonsCategories(actualites));
        CollectionAssert.DoesNotContain(
            ObtenirTousTextes(actualites),
            "Cate1");
        CollectionAssert.DoesNotContain(
            ObtenirTousTextes(actualites),
            "Cate2");
        CollectionAssert.DoesNotContain(
            ObtenirTousTextes(actualites),
            "????");

        foreach (string categorie in new[]
        {
            "Boursiers",
            "Personnels",
            "Professionnels"
        })
        {
            Invoquer(actualites, "FiltrerParCategorie", categorie);
            yield return null;

            IDictionary associations =
                (IDictionary)LireMembre(actualites, "itemsParLigne");
            Assert.That(associations.Count, Is.GreaterThan(0));
            foreach (DictionaryEntry association in associations)
            {
                Assert.That(
                    (string)LireMembre(association.Value, "categorie"),
                    Is.EqualTo(categorie));
            }

            Component premiere = ObtenirLignesVisibles(actualites)[0];
            object tableau = LireMembre(actualites, "tableauSelectable");
            Assert.That(
                Invoquer(tableau, "GetLigneSelectionnee"),
                Is.SameAs(premiere));
            Assert.That(
                LireTexte(LireMembre(actualites, "txtDetailTitre")),
                Is.EqualTo((string)Invoquer(premiere, "Get", 2)));
        }
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
        AutoriserActualites(gameData);
        Component actualites = TrouverComposantDansScene(
            TrouverType("ActualitesUI"));
        actualites.gameObject.SetActive(true);
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
        IList impacts = (IList)LireMembre(bourse, "impactsMarche");
        IList confirmations =
            (IList)LireMembre(donnees, "evenementsConfirmes");
        HashSet<string> clesImpacts = new HashSet<string>();
        int nombreImpactsAttendus = 0;
        foreach (object confirmation in confirmations)
        {
            if ((string)LireMembre(
                    confirmation,
                    "categorie") == "Boursiers" &&
                (bool)LireMembre(
                    confirmation,
                    "consommeParMoteurImpacts"))
            {
                nombreImpactsAttendus +=
                    Compter(LireMembre(confirmation, "impacts"));
            }
        }
        Assert.That(impacts.Count, Is.EqualTo(nombreImpactsAttendus));
        foreach (object impact in impacts)
        {
            string evenementId =
                (string)LireMembre(impact, "evenementId");
            string actifId = (string)LireMembre(impact, "actifId");
            Assert.That(
                clesImpacts.Add(evenementId + ":" + actifId),
                Is.True,
                "Un impact ne doit pas etre duplique pour un evenement et un actif.");
            Assert.That(
                (int)LireMembre(impact, "dureeMois"),
                Is.EqualTo(1));

            bool confirmationConsommee = false;
            foreach (object confirmation in confirmations)
            {
                if ((string)LireMembre(confirmation, "rumeurId") == evenementId &&
                    (bool)LireMembre(
                        confirmation,
                        "consommeParMoteurImpacts"))
                {
                    confirmationConsommee = true;
                    break;
                }
            }
            Assert.That(confirmationConsommee, Is.True);
        }
        yield return null;
    }

    private static object ObtenirGameData()
    {
        Component controleur = TrouverComposantDansScene(
            TrouverType("PhaseRepartitionTempsController"));
        return LireMembre(controleur, "gameData");
    }

    private static Component OuvrirEtRafraichirActualites()
    {
        AutoriserActualites(ObtenirGameData());
        Component actualites = TrouverComposantDansScene(
            TrouverType("ActualitesUI"));
        actualites.gameObject.SetActive(true);
        Assert.That(actualites.gameObject.activeInHierarchy, Is.True);
        return actualites;
    }

    private static void AutoriserActualites(object gameData)
    {
        object joueur = LireMembre(gameData, "joueur");
        object donneesTemps = LireMembre(joueur, "tempsApplications");
        object service = Activator.CreateInstance(
            TrouverType("ServiceRepartitionTemps"),
            new[] { donneesTemps });
        Invoquer(service, "DefinirAllocation", 0, 30, 0, 0, 0, 0);
        object typeActualites = Enum.Parse(
            TrouverType("TypeApplicationTemps"),
            "Actualites");
        Assert.That(
            (bool)Invoquer(service, "PeutOuvrir", typeActualites),
            Is.True,
            "L'allocation de test doit autoriser l'ouverture d'Actualites.");
    }

    private static List<Component> ObtenirLignesVisibles(Component actualites)
    {
        object tableau = LireMembre(actualites, "tableauActualites");
        IEnumerable lignes = (IEnumerable)LireMembre(tableau, "tableau");
        List<Component> visibles = new List<Component>();
        foreach (object ligne in lignes)
        {
            if (!(bool)Invoquer(ligne, "EstVide"))
            {
                visibles.Add((Component)ligne);
            }
        }

        visibles.Sort((gauche, droite) =>
            gauche.transform.GetSiblingIndex().CompareTo(
                droite.transform.GetSiblingIndex()));
        return visibles;
    }

    private static Component TrouverLigneParTitre(
        Component actualites,
        string titre)
    {
        foreach (Component ligne in ObtenirLignesVisibles(actualites))
        {
            if ((string)Invoquer(ligne, "Get", 2) == titre)
            {
                return ligne;
            }
        }
        return null;
    }

    private static void Cliquer(Component ligne)
    {
        PointerEventData donnees = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute<IPointerClickHandler>(
            ligne.gameObject,
            donnees,
            ExecuteEvents.pointerClickHandler);
    }

    private static string LireTexte(object composantTexte)
    {
        return (string)LireMembre(composantTexte, "text");
    }

    private static List<string> ObtenirTousTextes(Component racine)
    {
        Type typeTexte = TrouverType("TMPro.TextMeshProUGUI");
        UnityEngine.Object[] objets = Resources.FindObjectsOfTypeAll(typeTexte);
        List<string> textes = new List<string>();
        foreach (UnityEngine.Object objet in objets)
        {
            Component texte = objet as Component;
            if (texte != null &&
                (texte.transform == racine.transform ||
                    texte.transform.IsChildOf(racine.transform)))
            {
                textes.Add(LireTexte(texte));
            }
        }
        return textes;
    }

    private static List<string> ObtenirTextesBoutonsCategories(
        Component actualites)
    {
        IList boutons = (IList)LireMembre(actualites, "categoryButtons");
        List<string> textes = new List<string> { "Toutes" };
        foreach (object objet in boutons)
        {
            Component bouton = objet as Component;
            if (bouton == null || !bouton.gameObject.activeSelf)
            {
                continue;
            }

            Type typeTexte = TrouverType("TMPro.TextMeshProUGUI");
            Component texte = bouton.GetComponentInChildren(typeTexte, true);
            textes.Add(LireTexte(texte));
        }
        return textes;
    }

    private static void AjouterPublicationRumeurTest(
        object donnees,
        int mois,
        string id,
        string categorie)
    {
        object rumeur = Activator.CreateInstance(TrouverType("RumeurPartie"));
        EcrireMembre(rumeur, "id", id);
        EcrireMembre(rumeur, "evenementId", "evenement-" + id);
        EcrireMembre(rumeur, "sourceId", "source-test");
        EcrireMembre(rumeur, "categorie", categorie);
        EcrireMembre(rumeur, "titrePublic", "Titre " + categorie);
        EcrireMembre(rumeur, "textePublic", "Description " + categorie);
        EcrireMembre(rumeur, "moisApparition", mois);
        EcrireMembre(rumeur, "moisResolution", mois + 1);
        EcrireMembre(rumeur, "probabiliteConfirmation", 0.75f);
        ((IList)LireMembre(donnees, "rumeurs")).Add(rumeur);

        object publication = Activator.CreateInstance(
            TrouverType("PublicationActualite"));
        EcrireMembre(publication, "id", "publication-" + id);
        EcrireMembre(
            publication,
            "type",
            Enum.Parse(TrouverType("TypePublicationActualite"), "Rumeur"));
        EcrireMembre(publication, "objetId", id);
        EcrireMembre(publication, "sourceId", "source-test");
        EcrireMembre(publication, "sourceNom", "Source test");
        EcrireMembre(publication, "categorie", categorie);
        EcrireMembre(publication, "titre", "Titre " + categorie);
        EcrireMembre(publication, "texte", "Description " + categorie);
        EcrireMembre(publication, "moisPublication", mois);
        EcrireMembre(publication, "ordrePublication", 10000 + mois);
        EcrireMembre(
            publication,
            "etatRumeur",
            Enum.Parse(TrouverType("EtatRumeur"), "EnAttente"));
        ((IList)LireMembre(donnees, "publications")).Add(publication);
    }

    private static void AjouterEvenementConfirmeTest(object donnees, int mois)
    {
        const string rumeurId = "rumeur-evenement-ui";
        object evenement = Activator.CreateInstance(
            TrouverType("EvenementConfirmePartie"));
        EcrireMembre(evenement, "definitionId", "evenement-ui");
        EcrireMembre(evenement, "rumeurId", rumeurId);
        EcrireMembre(evenement, "sourceId", "source-ui");
        EcrireMembre(evenement, "categorie", "Boursiers");
        EcrireMembre(evenement, "importance", "Forte");
        EcrireMembre(evenement, "titre", "Événement UI confirmé");
        EcrireMembre(evenement, "message", "Description événement UI");
        EcrireMembre(evenement, "moisConfirmation", mois);
        object impact = Activator.CreateInstance(
            TrouverType("ImpactDefinitionEvenement"));
        EcrireMembre(impact, "actif", "Nvidia");
        EcrireMembre(impact, "variation", 0.125f);
        ((IList)LireMembre(evenement, "impacts")).Add(impact);
        ((IList)LireMembre(donnees, "evenementsConfirmes")).Add(evenement);

        object publication = Activator.CreateInstance(
            TrouverType("PublicationActualite"));
        EcrireMembre(publication, "id", "publication-evenement-ui");
        EcrireMembre(
            publication,
            "type",
            Enum.Parse(
                TrouverType("TypePublicationActualite"),
                "EvenementConfirme"));
        EcrireMembre(publication, "objetId", rumeurId);
        EcrireMembre(publication, "sourceId", "source-ui");
        EcrireMembre(publication, "sourceNom", "Source UI");
        EcrireMembre(publication, "categorie", "Boursiers");
        EcrireMembre(publication, "importance", "Forte");
        EcrireMembre(publication, "titre", "Événement UI confirmé");
        EcrireMembre(publication, "texte", "Description événement UI");
        EcrireMembre(publication, "moisPublication", mois);
        EcrireMembre(publication, "ordrePublication", 20000 + mois);
        EcrireMembre(
            publication,
            "etatRumeur",
            Enum.Parse(TrouverType("EtatRumeur"), "Confirmee"));
        ((IList)LireMembre(donnees, "publications")).Add(publication);
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

    private static void EcrireMembre(object cible, string nom, object valeur)
    {
        Type type = cible.GetType();
        FieldInfo champ = type.GetField(
            nom,
            BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.NonPublic);
        Assert.That(champ, Is.Not.Null, "Champ introuvable : " + nom);
        champ.SetValue(cible, valeur);
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
