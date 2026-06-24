using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class BenchmarkOptimiseurWhatIfPlayModeTests
{
    [UnityTest]
    public IEnumerator DixParties_OptimiseurEstMesureFaceAuJoueurActif()
    {
        object rapport = ExecuterRapport();
        Type typeRapport = rapport.GetType();

        int nombreParties =
            LireChamp<int>(
                typeRapport,
                rapport,
                "nombreParties");

        int victoires =
            LireChamp<int>(
                typeRapport,
                rapport,
                "victoiresOptimiseur");

        long cumulOptimiseur =
            LireChamp<long>(
                typeRapport,
                rapport,
                "capitalCumuleOptimiseur");

        long cumulJoueur =
            LireChamp<long>(
                typeRapport,
                rapport,
                "capitalCumuleJoueur");

        string detail =
            LireChamp<string>(
                typeRapport,
                rapport,
                "detail");

        Debug.Log(detail);

        Assert.That(nombreParties, Is.EqualTo(10));
        Assert.That(victoires, Is.InRange(0, 10));
        Assert.That(cumulOptimiseur, Is.GreaterThan(0));
        Assert.That(cumulJoueur, Is.GreaterThan(0));

        yield return null;
    }

    [UnityTest]
    public IEnumerator DixParties_BenchmarkEstDeterministe()
    {
        bool deterministe =
            AppelerBooleen(
                "VerifierDeterminisme");

        Assert.That(
            deterministe,
            Is.True,
            "Le benchmark doit produire exactement les mêmes résultats.");

        yield return null;
    }

    [UnityTest]
    public IEnumerator DixParties_LesPrixFutursNeChangentPasLaDecisionCourante()
    {
        bool absenceFuite =
            AppelerBooleen(
                "VerifierAbsenceFuiteFutur");

        Assert.That(
            absenceFuite,
            Is.True,
            "Une donnée future a influencé une décision présente.");

        yield return null;
    }

    private static object ExecuterRapport()
    {
        Type type = TrouverTypeRuntime();
        MethodInfo methode =
            type.GetMethod(
                "ExecuterDixParties",
                BindingFlags.Public |
                BindingFlags.Static);

        Assert.That(
            methode,
            Is.Not.Null,
            "Méthode ExecuterDixParties introuvable.");

        object rapport =
            methode.Invoke(null, null);

        Assert.That(
            rapport,
            Is.Not.Null,
            "Le benchmark n'a renvoyé aucun rapport.");

        return rapport;
    }

    private static bool AppelerBooleen(
        string nomMethode)
    {
        Type type = TrouverTypeRuntime();
        MethodInfo methode =
            type.GetMethod(
                nomMethode,
                BindingFlags.Public |
                BindingFlags.Static);

        Assert.That(
            methode,
            Is.Not.Null,
            "Méthode introuvable : " +
            nomMethode);

        object resultat =
            methode.Invoke(null, null);

        Assert.That(
            resultat,
            Is.TypeOf<bool>());

        return (bool)resultat;
    }

    private static Type TrouverTypeRuntime()
    {
        Type type = Type.GetType(
            "BenchmarkOptimiseurWhatIfRuntime, Assembly-CSharp");

        Assert.That(
            type,
            Is.Not.Null,
            "BenchmarkOptimiseurWhatIfRuntime introuvable dans Assembly-CSharp.");

        return type;
    }

    private static T LireChamp<T>(
        Type type,
        object instance,
        string nomChamp)
    {
        FieldInfo champ =
            type.GetField(
                nomChamp,
                BindingFlags.Public |
                BindingFlags.Instance);

        Assert.That(
            champ,
            Is.Not.Null,
            "Champ introuvable : " +
            nomChamp);

        object valeur =
            champ.GetValue(instance);

        Assert.That(
            valeur,
            Is.TypeOf<T>());

        return (T)valeur;
    }
}