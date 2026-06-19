using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XCharts.Runtime;

/// <summary>
/// Facade Unity de la fenetre de repartition du temps mensuel.
/// </summary>
/// <remarks>
/// Les sliders ne modifient pas directement <see cref="DonneesJoueur"/>. La
/// validation passe par <see cref="ServiceRepartitionTemps"/> pour garantir que
/// la somme des minutes correspond exactement au budget mensuel.
/// </remarks>
public class RepartitionTempsUI : MonoBehaviour
{
    /// <summary>
    /// Notification emise apres une allocation mensuelle validee avec succes.
    /// </summary>
    /// <remarks>
    /// Le controleur de phase s'y abonne pour fermer le verrou modal et
    /// reactiver les applications ayant du temps. Les donnees sont deja
    /// persistees par <see cref="ServiceRepartitionTemps"/> quand l'evenement
    /// est declenche.
    /// </remarks>
    public static event Action AllocationValidee;

    public GameData gameData;

    private readonly List<SliderAllocation> sliders =
        new List<SliderAllocation>();

    private ServiceRepartitionTemps service;
    private DonneesRepartitionTemps donnees;
    private PieChart pieChart;
    private TextMeshProUGUI totalText;
    private Button cancelButton;
    private bool miseAJourInterne;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReinitialiserEvenements()
    {
        AllocationValidee = null;
    }

    private void Start()
    {
        ResoudreService();
        ResoudreReferences();
        ConfigurerSliders();
        SynchroniserDepuisDonnees();
        ActualiserAffichage();
    }

    private void OnEnable()
    {
        if (sliders.Count > 0)
        {
            SynchroniserDepuisDonnees();
            ActualiserAffichage();
        }
    }

    private void ResoudreReferences()
    {
        pieChart = transform
            .Find("Fond/TimeAllocationContent/CenterContent/InfoPanel/PieChart")
            ?.GetComponent<PieChart>();
        totalText = transform
            .Find("Fond/TimeAllocationContent/CenterContent/InfoPanel/TotalText")
            ?.GetComponent<TextMeshProUGUI>();
        cancelButton = transform.Find("TopBar/cancel")?.GetComponent<Button>();

        sliders.Clear();
        AjouterSlider(TypeApplicationTemps.Banque, "Slider_Banque");
        AjouterSlider(TypeApplicationTemps.Actualites, "Slider_Actualites");
        AjouterSlider(TypeApplicationTemps.Salariat, "Slider_Salariat");
        AjouterSlider(TypeApplicationTemps.Bourse, "Slider_Bourse");
        AjouterSlider(TypeApplicationTemps.Immobilier, "Slider_Immo");
        AjouterSlider(
            TypeApplicationTemps.Entrepreneuriat,
            "Slider_Entrepreneuriat");

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(OnCloseAndValidate);
            cancelButton.onClick.AddListener(OnCloseAndValidate);
        }
    }

    private void AjouterSlider(TypeApplicationTemps type, string nom)
    {
        Slider slider = transform
            .Find(
                "Fond/TimeAllocationContent/CenterContent/SlidersList/" +
                nom +
                "/Slider")
            ?.GetComponent<Slider>();
        if (slider != null)
        {
            sliders.Add(new SliderAllocation(type, slider));
        }
    }

    private void ConfigurerSliders()
    {
        int budget = ObtenirBudget();
        for (int index = 0; index < sliders.Count; index++)
        {
            SliderAllocation entree = sliders[index];
            entree.slider.minValue = 0f;
            entree.slider.maxValue = budget;
            entree.slider.wholeNumbers = true;
            entree.indexGraphique = index;
            entree.slider.onValueChanged.RemoveAllListeners();
            entree.slider.onValueChanged.AddListener(
                valeur => OnSliderValueChanged(entree, valeur));
        }
    }

    private void OnSliderValueChanged(
        SliderAllocation entree,
        float valeur)
    {
        if (miseAJourInterne)
        {
            return;
        }

        int budget = ObtenirBudget();
        float autres = CalculerTotalSans(entree.slider);
        if (autres + valeur > budget)
        {
            miseAJourInterne = true;
            valeur = budget - autres;
            entree.slider.value = valeur;
            miseAJourInterne = false;
        }

        ActualiserAffichage();
    }

    private void ActualiserAffichage()
    {
        int total = Mathf.RoundToInt(CalculerTotal());
        int budget = ObtenirBudget();
        bool allocationComplete = total == budget;

        if (totalText != null)
        {
            totalText.text =
                "Temps alloue : " + total + " / " + budget + " min";
            totalText.color = allocationComplete
                ? new Color(0.1f, 0.5f, 0.1f)
                : new Color(0.8f, 0.1f, 0.1f);
        }

        if (cancelButton != null)
        {
            cancelButton.interactable = allocationComplete;
        }

        ActualiserGraphique();
    }

    private void ActualiserGraphique()
    {
        if (pieChart == null)
        {
            return;
        }

        AssurerDonneesGraphique();
        for (int index = 0; index < sliders.Count; index++)
        {
            pieChart.UpdateData(
                0,
                sliders[index].indexGraphique,
                sliders[index].slider.value);
        }

        pieChart.RefreshChart();
    }

    private void OnCloseAndValidate()
    {
        if (!ResoudreService())
        {
            Debug.LogError(
                "[RepartitionTempsUI] Validation impossible : GameData absent.");
            return;
        }

        ResultatOperation resultat = service.DefinirAllocation(
            Lire(TypeApplicationTemps.Banque),
            Lire(TypeApplicationTemps.Actualites),
            Lire(TypeApplicationTemps.Salariat),
            Lire(TypeApplicationTemps.Bourse),
            Lire(TypeApplicationTemps.Entrepreneuriat),
            Lire(TypeApplicationTemps.Immobilier));
        if (!resultat.Succes)
        {
            Debug.LogWarning("[RepartitionTempsUI] " + resultat.Message);
            ActualiserAffichage();
            return;
        }

        AllocationValidee?.Invoke();
        gameObject.SetActive(false);
    }

    private void SynchroniserDepuisDonnees()
    {
        if (donnees == null)
        {
            return;
        }

        miseAJourInterne = true;
        foreach (SliderAllocation entree in sliders)
        {
            AllocationTempsApplication allocation =
                donnees.Obtenir(entree.type);
            entree.slider.value =
                allocation == null ? 0f : allocation.minutesInitiales;
        }
        miseAJourInterne = false;
    }

    private void AssurerDonneesGraphique()
    {
        Serie serie = pieChart.GetSerie(0);
        if (serie == null)
        {
            serie = pieChart.AddSerie<Pie>("RepartitionTemps");
        }

        if (serie == null || serie.dataCount == sliders.Count)
        {
            return;
        }

        // Le prefab a deja porte quatre entrees. On regenere la liste pour
        // garantir que le graphique et les sliders restent toujours alignes.
        serie.ClearData();
        foreach (SliderAllocation entree in sliders)
        {
            pieChart.AddData(0, entree.slider.value, NomGraphique(entree.type));
        }
    }

    private int Lire(TypeApplicationTemps type)
    {
        foreach (SliderAllocation entree in sliders)
        {
            if (entree.type == type)
            {
                return Mathf.RoundToInt(entree.slider.value);
            }
        }

        return 0;
    }

    private float CalculerTotal()
    {
        float total = 0f;
        foreach (SliderAllocation entree in sliders)
        {
            total += entree.slider.value;
        }

        return total;
    }

    private float CalculerTotalSans(Slider sliderExclu)
    {
        float total = 0f;
        foreach (SliderAllocation entree in sliders)
        {
            if (entree.slider != sliderExclu)
            {
                total += entree.slider.value;
            }
        }

        return total;
    }

    private int ObtenirBudget()
    {
        if (donnees != null)
        {
            return donnees.budgetMensuelMinutes;
        }

        return DonneesRepartitionTemps.BudgetMensuelMinutes;
    }

    private static string NomGraphique(TypeApplicationTemps type)
    {
        return type.ToString();
    }

    private bool ResoudreService()
    {
        if (gameData == null)
        {
            ActionPlay actionPlay =
                UnityEngine.Object.FindFirstObjectByType<ActionPlay>();
            if (actionPlay != null)
            {
                gameData = actionPlay.gameData;
            }
        }

        if (gameData == null || gameData.joueur == null)
        {
            return false;
        }

        gameData.joueur.InitialiserSiNecessaire();
        donnees = gameData.joueur.tempsApplications;
        service = new ServiceRepartitionTemps(donnees);
        return true;
    }

    private sealed class SliderAllocation
    {
        public readonly TypeApplicationTemps type;
        public readonly Slider slider;
        public int indexGraphique;

        public SliderAllocation(TypeApplicationTemps type, Slider slider)
        {
            this.type = type;
            this.slider = slider;
        }
    }
}
