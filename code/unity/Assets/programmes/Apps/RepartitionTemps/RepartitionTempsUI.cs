using UnityEngine;
using UnityEngine.UI;
using TMPro;
using XCharts.Runtime;

public class RepartitionTempsUI : MonoBehaviour
{
    public GameData gameData;
    private PieChart pieChart;
    private TextMeshProUGUI totalText;
    private Button cancelButton;

    private Slider sliderBanque;
    private Slider sliderActualites;
    private Slider sliderSalariat;
    private Slider sliderBourse;

    private float maxTotalTime = 30f;

    void Start()
    {
        if (gameData == null)
        {
       
            Debug.LogError("[RepartitionTempsUI] GameData non attaché");
        }

        // Resolution des references
        pieChart = transform.Find("Fond/TimeAllocationContent/CenterContent/InfoPanel/PieChart")?.GetComponent<PieChart>();
        totalText = transform.Find("Fond/TimeAllocationContent/CenterContent/InfoPanel/TotalText")?.GetComponent<TextMeshProUGUI>();
        cancelButton = transform.Find("cancel")?.GetComponent<Button>();

        sliderBanque = transform.Find("Fond/TimeAllocationContent/CenterContent/SlidersList/Slider_Banque/Slider")?.GetComponent<Slider>();
        sliderActualites = transform.Find("Fond/TimeAllocationContent/CenterContent/SlidersList/Slider_Actualites/Slider")?.GetComponent<Slider>();
        sliderSalariat = transform.Find("Fond/TimeAllocationContent/CenterContent/SlidersList/Slider_Salariat/Slider")?.GetComponent<Slider>();
        sliderBourse = transform.Find("Fond/TimeAllocationContent/CenterContent/SlidersList/Slider_Bourse/Slider")?.GetComponent<Slider>();

        // Configuration des sliders (min/max a 30, snappe sur les entiers) et de leurs ecouteurs
        ConfigureSlider(sliderBanque, 0);
        ConfigureSlider(sliderActualites, 1);
        ConfigureSlider(sliderSalariat, 2);
        ConfigureSlider(sliderBourse, 3);

        // Ecoute du bouton de fermeture
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCloseAndValidate);
        }

        // Premiere mise a jour visuelle
        UpdateChartAndText();
    }

    private void ConfigureSlider(Slider slider, int index)
    {
        if (slider == null) return;
        
        slider.minValue = 0f;
        slider.maxValue = maxTotalTime;
        slider.wholeNumbers = true; // Force le blocage a chaque unite (minute)
        
        slider.onValueChanged.AddListener((val) => OnSliderValueChanged(slider, index, val));
    }

    private void OnSliderValueChanged(Slider changedSlider, int index, float val)
    {
        float totalOther = GetTotalOtherSliders(changedSlider);

        // Contrainte de securite integree : s'assurer que la somme totale ne depasse pas maxTotalTime (30 min)
        if (totalOther + val > maxTotalTime)
        {
            val = maxTotalTime - totalOther;
            changedSlider.value = val; // Force la valeur bridee snappee a l'unite
        }

        // Mettre a jour la tranche correspondante dans XCharts (directement en minutes)
        if (pieChart != null)
        {
            pieChart.UpdateData(0, index, val);
        }

        UpdateChartAndText();
    }

    private float GetTotalOtherSliders(Slider excludeSlider)
    {
        float sum = 0f;
        if (sliderBanque != excludeSlider && sliderBanque != null) sum += sliderBanque.value;
        if (sliderActualites != excludeSlider && sliderActualites != null) sum += sliderActualites.value;
        if (sliderSalariat != excludeSlider && sliderSalariat != null) sum += sliderSalariat.value;
        if (sliderBourse != excludeSlider && sliderBourse != null) sum += sliderBourse.value;
        return sum;
    }

    private void UpdateChartAndText()
    {
        float sum = 0f;
        if (sliderBanque != null) sum += sliderBanque.value;
        if (sliderActualites != null) sum += sliderActualites.value;
        if (sliderSalariat != null) sum += sliderSalariat.value;
        if (sliderBourse != null) sum += sliderBourse.value;

        int totalMinutesRounded = Mathf.RoundToInt(sum);
        int maxTotalTimeRounded = Mathf.RoundToInt(maxTotalTime);

        bool EstComplet = (totalMinutesRounded == maxTotalTimeRounded);

        if (totalText != null)
        {
            totalText.text = "Temps alloue : " + totalMinutesRounded + " / " + maxTotalTimeRounded + " min";
            
            // Indication visuelle style XP
            totalText.color = EstComplet ? new Color(0.1f, 0.5f, 0.1f) : new Color(0.8f, 0.1f, 0.1f);
        }

        // Desactiver le bouton de fermeture tant que les 30 minutes ne sont pas reparties
        if (cancelButton != null)
        {
            cancelButton.interactable = EstComplet;
        }

        if (pieChart != null)
        {
            pieChart.RefreshChart(); // Forcer le rafraichissement visuel d'XCharts
        }
    }

    private void OnCloseAndValidate()
    {
        float valBanque = sliderBanque ? sliderBanque.value : 0f;
        float valActualites = sliderActualites ? sliderActualites.value : 0f;
        float valSalariat = sliderSalariat ? sliderSalariat.value : 0f;
        float valBourse = sliderBourse ? sliderBourse.value : 0f;

        // Les valeurs sont deja en minutes
        float minBanque = valBanque;
        float minActualites = valActualites;
        float minSalariat = valSalariat;
        float minBourse = valBourse;
        float total = minBanque + minActualites + minSalariat + minBourse;

        if (gameData == null)
        {
            ManagerTemps mt = FindFirstObjectByType<ManagerTemps>();
            if (mt != null) gameData = mt.gameData;
        }

        if (gameData != null)
        {
            gameData.joueur.tempsInitialBanque = Mathf.RoundToInt(minBanque);
            gameData.joueur.tempsInitialActualites = Mathf.RoundToInt(minActualites);
            gameData.joueur.tempsInitialSalariat = Mathf.RoundToInt(minSalariat);
            gameData.joueur.tempsInitialBourse = Mathf.RoundToInt(minBourse);

            gameData.joueur.tempsRestantBanque = minBanque * 60f;
            gameData.joueur.tempsRestantActualites = minActualites * 60f;
            gameData.joueur.tempsRestantSalariat = minSalariat * 60f;
            gameData.joueur.tempsRestantBourse = minBourse * 60f;
            
            Debug.Log("[RepartitionTempsUI] Temps alloues enregistres avec succes dans GameData.");
        }
        else
        {
            Debug.LogError("[RepartitionTempsUI] Validation impossible : GameData est null !");
        }

        // Affichage console de validation
        Debug.Log("=== VALIDATION DE LA REPARTITION DU TEMPS ===");
        Debug.Log("Banque : " + Mathf.RoundToInt(minBanque) + " min");
        Debug.Log("Actualites : " + Mathf.RoundToInt(minActualites) + " min");
        Debug.Log("Salariat : " + Mathf.RoundToInt(minSalariat) + " min");
        Debug.Log("Bourse : " + Mathf.RoundToInt(minBourse) + " min");
        Debug.Log("Total alloue : " + Mathf.RoundToInt(total) + " / " + maxTotalTime + " min");

        // Fermer la fenetre pour retourner au bureau
        gameObject.SetActive(false);
    }
}
