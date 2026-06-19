using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

/// <summary>
/// Verifie le cablage reel de RepartitionTemps dans la scene Jeu.
/// </summary>
public class RepartitionTempsPlayModeTests
{
    private const string SceneJeu = "Jeu";

    [UnitySetUp]
    public IEnumerator ChargerJeuAvecDonneesReinitialisees()
    {
        yield return SceneManager.LoadSceneAsync(SceneJeu);
        yield return null;

        Component controleur = TrouverComposantDansScene(
            TrouverType("PhaseRepartitionTempsController"));
        object gameData = LireChamp(controleur, "gameData");
        Invoquer(gameData, "ResetData");

        // Le second chargement garantit que tous les services de scene
        // resolvent l'agregat joueur nouvellement initialise.
        yield return SceneManager.LoadSceneAsync(SceneJeu);
        yield return null;
        yield return null;
    }

    [UnityTest]
    public IEnumerator Sliders_ReactualisentLesTextesSansMutationBancaire()
    {
        Type typeUi = TrouverType("RepartitionTempsUI");
        Type typeSliderNeutre = TrouverType("SliderTempsAvecValeur");
        Type typeSliderBancaire = TrouverType("SliderAvecTexte");
        Component ui = TrouverComposantDansScene(typeUi);
        object gameData = LireChamp(
            TrouverComposantDansScene(
                TrouverType("PhaseRepartitionTempsController")),
            "gameData");
        EtatBanque avant = CapturerEtatBanque(gameData);

        Component[] sliders = ui.GetComponentsInChildren(
            typeSliderNeutre,
            true);
        Component[] composantsBancaires = ui.GetComponentsInChildren(
            typeSliderBancaire,
            true);

        Assert.That(sliders, Has.Length.EqualTo(6));
        Assert.That(composantsBancaires, Is.Empty);

        foreach (Component composant in sliders)
        {
            object texte = LirePropriete(composant, "TexteValeur");
            StringAssert.Contains(
                "0 min",
                (string)LirePropriete(texte, "text"));
        }

        int total = 0;
        for (int index = 0; index < sliders.Length; index++)
        {
            int minutes = index + 1;
            total += minutes;
            object slider = LirePropriete(sliders[index], "Slider");
            EcrirePropriete(slider, "value", (float)minutes);

            object texte = LirePropriete(sliders[index], "TexteValeur");
            string valeurAffichee =
                (string)LirePropriete(texte, "text");
            StringAssert.Contains(minutes + " min", valeurAffichee);
        }

        Transform totalTransform = ui.transform.Find(
            "Fond/TimeAllocationContent/CenterContent/InfoPanel/TotalText");
        Assert.That(totalTransform, Is.Not.Null);
        Component totalTexte = totalTransform.GetComponent(
            "TextMeshProUGUI");
        Assert.That(totalTexte, Is.Not.Null);
        StringAssert.Contains(
            total + " / 30 min",
            (string)LirePropriete(totalTexte, "text"));

        object premierSlider = LirePropriete(sliders[0], "Slider");
        EcrirePropriete(premierSlider, "value", 30f);
        object premierTexte = LirePropriete(sliders[0], "TexteValeur");
        StringAssert.Contains(
            "10 min",
            (string)LirePropriete(premierTexte, "text"));
        StringAssert.Contains(
            "30 / 30 min",
            (string)LirePropriete(totalTexte, "text"));

        object joueur = LireMembre(gameData, "joueur");
        object donneesTemps = LireMembre(joueur, "tempsApplications");
        object serviceTemps = Activator.CreateInstance(
            TrouverType("ServiceRepartitionTemps"),
            new[] { donneesTemps });
        Invoquer(serviceTemps, "ReinitialiserAllocation");
        ui.gameObject.SetActive(false);
        ui.gameObject.SetActive(true);

        yield return null;

        foreach (Component composant in sliders)
        {
            object texte = LirePropriete(composant, "TexteValeur");
            StringAssert.Contains(
                "0 min",
                (string)LirePropriete(texte, "text"));
        }
        StringAssert.Contains(
            "0 / 30 min",
            (string)LirePropriete(totalTexte, "text"));

        EtatBanque apres = CapturerEtatBanque(gameData);
        Assert.That(apres.SoldeCourant, Is.EqualTo(avant.SoldeCourant));
        Assert.That(apres.SoldeEpargne, Is.EqualTo(avant.SoldeEpargne));
        Assert.That(
            apres.CompteEpargnePresent,
            Is.EqualTo(avant.CompteEpargnePresent));
        Assert.That(apres.NombreComptes, Is.EqualTo(avant.NombreComptes));
        Assert.That(apres.OperationsCourant, Is.EqualTo(avant.OperationsCourant));
        Assert.That(apres.OperationsEpargne, Is.EqualTo(avant.OperationsEpargne));
        Assert.That(apres.PatrimoineBancaire, Is.EqualTo(avant.PatrimoineBancaire));
    }

    [UnityTest]
    public IEnumerator Phase_ImmobilierSuitLaMemeRegleQueBanque()
    {
        Component controleur = TrouverComposantDansScene(
            TrouverType("PhaseRepartitionTempsController"));
        object gameData = LireChamp(controleur, "gameData");
        object joueur = LireMembre(gameData, "joueur");
        object donneesTemps = LireMembre(joueur, "tempsApplications");
        object service = Activator.CreateInstance(
            TrouverType("ServiceRepartitionTemps"),
            new[] { donneesTemps });

        Invoquer(
            service,
            "DefinirAllocation",
            30,
            0,
            0,
            0,
            0,
            0);
        Invoquer(controleur, "ActualiserPhase", false);

        object boutonBanque = LireChamp(controleur, "boutonBanque");
        object boutonImmobilier = LireChamp(controleur, "boutonImmobilier");
        Assert.That(boutonBanque, Is.Not.Null);
        Assert.That(boutonImmobilier, Is.Not.Null);
        Assert.That(LirePropriete(boutonBanque, "interactable"), Is.True);
        Assert.That(LirePropriete(boutonImmobilier, "interactable"), Is.False);

        Invoquer(
            service,
            "DefinirAllocation",
            15,
            0,
            0,
            0,
            0,
            15);
        Invoquer(controleur, "ActualiserPhase", false);

        Assert.That(LirePropriete(boutonBanque, "interactable"), Is.True);
        Assert.That(LirePropriete(boutonImmobilier, "interactable"), Is.True);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Banque_ConserveSesComposantsSansScriptManquant()
    {
        Component courantUi = TrouverComposantDansScene(
            TrouverType("CourantUI"));
        Component epargneUi = TrouverComposantDansScene(
            TrouverType("EpargneUI"));
        Transform racineBanque = TrouverAncetreCommun(
            courantUi.transform,
            epargneUi.transform);

        Assert.That(racineBanque, Is.Not.Null);
        Assert.That(
            racineBanque.GetComponentsInChildren(
                TrouverType("SliderTempsAvecValeur"),
                true),
            Is.Empty);
        Assert.That(
            racineBanque.GetComponentsInChildren(
                TrouverType("SliderAvecTexte"),
                true),
            Is.Empty);

        Transform[] objets =
            racineBanque.GetComponentsInChildren<Transform>(true);
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

        yield return null;
    }

    private static EtatBanque CapturerEtatBanque(object gameData)
    {
        object joueur = LireMembre(gameData, "joueur");
        IDictionary comptes = (IDictionary)LireMembre(joueur, "comptes");
        object courant = comptes["courant"];
        bool compteEpargnePresent = comptes.Contains("epargne");
        object epargne = compteEpargnePresent
            ? comptes["epargne"]
            : null;
        int soldeCourant = LireCentimes(Invoquer(courant, "GetSolde"));
        int soldeEpargne = compteEpargnePresent
            ? LireCentimes(Invoquer(epargne, "GetSolde"))
            : 0;
        int patrimoine =
            LireCentimes(Invoquer(courant, "GetValeurPatrimoine"));
        if (compteEpargnePresent)
        {
            patrimoine +=
                LireCentimes(Invoquer(epargne, "GetValeurPatrimoine"));
        }

        return new EtatBanque(
            soldeCourant,
            soldeEpargne,
            CompterOperations(courant),
            compteEpargnePresent ? CompterOperations(epargne) : 0,
            patrimoine,
            compteEpargnePresent,
            comptes.Count);
    }

    private static int CompterOperations(object compte)
    {
        object historique = Invoquer(compte, "GetHistorique");
        ICollection operations =
            (ICollection)Invoquer(historique, "GetHistorique");
        return operations.Count;
    }

    private static int LireCentimes(object montant)
    {
        return (int)LireMembre(montant, "centimes");
    }

    private static Transform TrouverAncetreCommun(
        Transform premier,
        Transform second)
    {
        HashSet<Transform> ancetres = new HashSet<Transform>();
        Transform courant = premier;
        while (courant != null)
        {
            ancetres.Add(courant);
            courant = courant.parent;
        }

        courant = second;
        while (courant != null)
        {
            if (ancetres.Contains(courant))
            {
                return courant;
            }

            courant = courant.parent;
        }

        return null;
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

    private static Component TrouverComposantDansScene(Type type)
    {
        UnityEngine.Object[] objets = Resources.FindObjectsOfTypeAll(type);
        foreach (UnityEngine.Object objet in objets)
        {
            Component composant = objet as Component;
            if (composant != null && composant.gameObject.scene.IsValid())
            {
                return composant;
            }
        }

        Assert.Fail("Composant introuvable dans la scene : " + type.Name);
        return null;
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
        FieldInfo champ = cible.GetType().GetField(
            nom,
            BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.NonPublic);
        if (champ != null)
        {
            return champ.GetValue(cible);
        }

        return LirePropriete(cible, nom);
    }

    private static object LireChamp(object cible, string nom)
    {
        FieldInfo champ = cible.GetType().GetField(
            nom,
            BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.NonPublic);
        Assert.That(champ, Is.Not.Null, "Champ introuvable : " + nom);
        return champ.GetValue(cible);
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

    private static void EcrirePropriete(
        object cible,
        string nom,
        object valeur)
    {
        PropertyInfo propriete = cible.GetType().GetProperty(
            nom,
            BindingFlags.Instance | BindingFlags.Public |
            BindingFlags.NonPublic);
        Assert.That(
            propriete,
            Is.Not.Null,
            "Propriete introuvable : " + nom);
        propriete.SetValue(cible, valeur);
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

    private readonly struct EtatBanque
    {
        public readonly int SoldeCourant;
        public readonly int SoldeEpargne;
        public readonly int OperationsCourant;
        public readonly int OperationsEpargne;
        public readonly int PatrimoineBancaire;
        public readonly bool CompteEpargnePresent;
        public readonly int NombreComptes;

        public EtatBanque(
            int soldeCourant,
            int soldeEpargne,
            int operationsCourant,
            int operationsEpargne,
            int patrimoineBancaire,
            bool compteEpargnePresent,
            int nombreComptes)
        {
            SoldeCourant = soldeCourant;
            SoldeEpargne = soldeEpargne;
            OperationsCourant = operationsCourant;
            OperationsEpargne = operationsEpargne;
            PatrimoineBancaire = patrimoineBancaire;
            CompteEpargnePresent = compteEpargnePresent;
            NombreComptes = nombreComptes;
        }
    }
}
