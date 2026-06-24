using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class RetrospectionTableauRuntimeUITests
{
    [Test]
    public void ConstruireInterface_MasqueLesEnfantsHistoriques()
    {
        GameObject panneau =
            CreerPanneauAvecAnciensEnfants(
                out GameObject ancienUn,
                out GameObject ancienDeux);

        try
        {
            RetrospectionTableauRuntimeUI runtime =
                panneau.AddComponent<
                    RetrospectionTableauRuntimeUI>();

            runtime.ConstruireInterface();

            Assert.That(ancienUn.activeSelf, Is.False);
            Assert.That(ancienDeux.activeSelf, Is.False);
            Assert.That(runtime.RacineGeneree, Is.Not.Null);
            Assert.That(
                runtime.RacineGeneree.gameObject.activeSelf,
                Is.True);
        }
        finally
        {
            Object.DestroyImmediate(panneau);
        }
    }

    [Test]
    public void ConstruireInterface_EstIdempotente()
    {
        GameObject panneau =
            CreerPanneauAvecAnciensEnfants(
                out _,
                out _);

        try
        {
            RetrospectionTableauRuntimeUI runtime =
                panneau.AddComponent<
                    RetrospectionTableauRuntimeUI>();

            runtime.ConstruireInterface();
            RectTransform premiere =
                runtime.RacineGeneree;

            runtime.ConstruireInterface();

            Assert.That(
                runtime.RacineGeneree,
                Is.SameAs(premiere));

            int racinesGenerees = 0;

            for (int index = 0;
                index < panneau.transform.childCount;
                index++)
            {
                if (panneau.transform
                    .GetChild(index)
                    .gameObject.name ==
                    "TableauWhatIfRuntime")
                {
                    racinesGenerees++;
                }
            }

            Assert.That(racinesGenerees, Is.EqualTo(1));
        }
        finally
        {
            Object.DestroyImmediate(panneau);
        }
    }

    [Test]
    public void ActualiserSansGameData_CreeUnContenuCompact()
    {
        GameObject panneau =
            CreerPanneauAvecAnciensEnfants(
                out _,
                out _);

        try
        {
            RetrospectionTableauRuntimeUI runtime =
                panneau.AddComponent<
                    RetrospectionTableauRuntimeUI>();

            runtime.Initialiser(null);

            Assert.That(runtime.ContenuGenere, Is.Not.Null);
            Assert.That(
                runtime.ContenuGenere.childCount,
                Is.EqualTo(2));

            LayoutElement ligne =
                runtime.ContenuGenere
                    .GetChild(1)
                    .GetComponent<LayoutElement>();

            Assert.That(ligne, Is.Not.Null);
            Assert.That(
                ligne.preferredHeight,
                Is.LessThanOrEqualTo(62f));
        }
        finally
        {
            Object.DestroyImmediate(panneau);
        }
    }

    [Test]
    public void InterfaceUtiliseUnScrollRectVertical()
    {
        GameObject panneau =
            CreerPanneauAvecAnciensEnfants(
                out _,
                out _);

        try
        {
            RetrospectionTableauRuntimeUI runtime =
                panneau.AddComponent<
                    RetrospectionTableauRuntimeUI>();

            runtime.ConstruireInterface();

            ScrollRect scroll =
                runtime.RacineGeneree
                    .GetComponentInChildren<ScrollRect>(
                        true);

            Assert.That(scroll, Is.Not.Null);
            Assert.That(scroll.vertical, Is.True);
            Assert.That(scroll.horizontal, Is.False);
            Assert.That(scroll.content, Is.Not.Null);
            Assert.That(scroll.viewport, Is.Not.Null);
        }
        finally
        {
            Object.DestroyImmediate(panneau);
        }
    }

    private static GameObject
        CreerPanneauAvecAnciensEnfants(
            out GameObject ancienUn,
            out GameObject ancienDeux)
    {
        GameObject panneau = new GameObject(
            "PanneauTableaux",
            typeof(RectTransform));

        ancienUn = new GameObject(
            "TableauJoueur",
            typeof(RectTransform));

        ancienDeux = new GameObject(
            "TableauOpti",
            typeof(RectTransform));

        ancienUn.transform.SetParent(
            panneau.transform,
            false);

        ancienDeux.transform.SetParent(
            panneau.transform,
            false);

        return panneau;
    }
}