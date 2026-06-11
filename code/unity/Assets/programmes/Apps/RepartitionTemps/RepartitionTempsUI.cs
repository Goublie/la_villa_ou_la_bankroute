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
            Debug.LogError("GameDate manquant dans le RepartitionTempsUI");
        }

        pieChart = transform.Find("Fond/TimeAllocationContent/CenterContent/InfoPanel/PieChart")?.GetComponent<PieChart>();
        totalText = transform.Find("Fond/TimeAllocationContent/CenterContent/InfoPanel/TotalText")?.GetComponent<TextMeshProUGUI>();
        cancelButton = transform.Find("cancel")?.GetComponent<Button>();

        sliderBanque = transform.Find("Fond/TimeAllocationContent/CenterContent/SlidersList/Slider_Banque/Slider")?.GetComponent<Slider>();
        sliderActualites = transform.Find("Fond/TimeAllocationContent/CenterContent/SlidersList/Slider_Actualites/Slider")?.GetComponent<Slider>();
        sliderSalariat = transform.Find("Fond/TimeAllocationContent/CenterContent/SlidersList/Slider_Salariat/Slider")?.GetComponent<Slider>();
        sliderBourse = transform.Find("Fond/TimeAllocationContent/CenterContent/SlidersList/Slider_Bourse/Slider")?.GetComponent<Slider>();

        //Configurer les ecouteurs de changement de valeur des sliders (securite somme max a 100% / 1.0f)
        ConfigureSliderListener(sliderBanque, 0);
        ConfigureSliderListener(sliderActualites, 1);
        ConfigureSliderListener(sliderSalariat, 2);
        ConfigureSliderListener(sliderBourse, 3);

        //Associer le bouton de fermeture (cancel) pour valider et afficher les valeurs dans la console
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCloseAndValidate);
        }

        //Effectuer la premiere mise a jour visuelle
        UpdateChartAndText();
    }

    private void ConfigureSliderListener(Slider slider, int index)
    {
        if (slider == null) return;
        
        slider.onValueChanged.AddListener((val) => OnSliderValueChanged(slider, index, val));
    }

    private void OnSliderValueChanged(Slider changedSlider, int index, float val)
    {
        float totalOther = GetTotalOtherSliders(changedSlider);

        // Contrainte de securite integree : s'assurer que la somme de tous les sliders ne depasse jamais 100% (1.0f)
        if (totalOther + val > 1.0f)
        {
            val = 1.0f - totalOther;
            changedSlider.value = val; // Forcer la valeur bridee du slider
        }

        // Mettre a jour la tranche correspondante dans XCharts (exprimee en minutes)
        if (pieChart != null)
        {
            pieChart.UpdateData(0, index, val * maxTotalTime);
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

        // Convertis les proportions en minutes pour l'affichage
        float totalMinutes = sum * maxTotalTime;
        int totalMinutesRounded = Mathf.RoundToInt(totalMinutes);
        int maxTotalTimeRounded = Mathf.RoundToInt(maxTotalTime);

        bool EstComplet = (totalMinutesRounded == maxTotalTimeRounded);

        if (totalText != null)
        {
            totalText.text = "Temps alloue : " + totalMinutesRounded + " / " + maxTotalTime + " min";
            
            // Indication visuelle : rouge si incomplet, vert si entierement alloue (couleurs pleines style XP)
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

        // Convertir les valeurs des sliders en minutes pour l'affichage de validation
        float minBanque = valBanque * maxTotalTime;
        float minActualites = valActualites * maxTotalTime;
        float minSalariat = valSalariat * maxTotalTime;
        float minBourse = valBourse * maxTotalTime;
        float total = minBanque + minActualites + minSalariat + minBourse;

        if (gameData == null)
        {
            Debug.LogError("GameData manquant dans le RepartitionTempsUI");
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

        // Print final allocated values in minutes to the console
        Debug.Log("=== VALIDATION DE LA REPARTITION DU TEMPS ===");
        Debug.Log("Banque : " + Mathf.RoundToInt(minBanque) + " min");
        Debug.Log("Actualites : " + Mathf.RoundToInt(minActualites) + " min");
        Debug.Log("Salariat : " + Mathf.RoundToInt(minSalariat) + " min");
        Debug.Log("Bourse : " + Mathf.RoundToInt(minBourse) + " min");
        Debug.Log("Total alloue : " + Mathf.RoundToInt(total) + " / " + maxTotalTime + " min");

        // Close the window to transition back to the desktop
        gameObject.SetActive(false);
    }
}
