using UnityEngine;
using UnityEngine.UI;
using TMPro;
using XCharts.Runtime;

public class RepartitionTempsUI : MonoBehaviour
{
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
        // 1. Automatic reference resolution of child UI components
        pieChart = transform.Find("Fond/TimeAllocationContent/CenterContent/InfoPanel/PieChart")?.GetComponent<PieChart>();
        totalText = transform.Find("Fond/TimeAllocationContent/CenterContent/InfoPanel/TotalText")?.GetComponent<TextMeshProUGUI>();
        cancelButton = transform.Find("cancel")?.GetComponent<Button>();

        sliderBanque = transform.Find("Fond/TimeAllocationContent/CenterContent/SlidersList/Slider_Banque/Slider")?.GetComponent<Slider>();
        sliderActualites = transform.Find("Fond/TimeAllocationContent/CenterContent/SlidersList/Slider_Actualites/Slider")?.GetComponent<Slider>();
        sliderSalariat = transform.Find("Fond/TimeAllocationContent/CenterContent/SlidersList/Slider_Salariat/Slider")?.GetComponent<Slider>();
        sliderBourse = transform.Find("Fond/TimeAllocationContent/CenterContent/SlidersList/Slider_Bourse/Slider")?.GetComponent<Slider>();

        // 2. Configure slider value change listeners (safeguarding sum to 100% / 1.0f)
        ConfigureSliderListener(sliderBanque, 0);
        ConfigureSliderListener(sliderActualites, 1);
        ConfigureSliderListener(sliderSalariat, 2);
        ConfigureSliderListener(sliderBourse, 3);

        // 3. Register close button (cancel) to print validation values in the console
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCloseAndValidate);
        }

        // 4. Perform initial visual update
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

        // Built-in safeguard constraint: enforce that the sum of all sliders never exceeds 100% (1.0f)
        if (totalOther + val > 1.0f)
        {
            val = 1.0f - totalOther;
            changedSlider.value = val; // Force clamp the slider value
        }

        // Update the corresponding slice in XCharts (expressed in minutes)
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

        // Convert the proportion (0.0 to 1.0) into minutes (0 to 30)
        float totalMinutes = sum * maxTotalTime;

        if (totalText != null)
        {
            totalText.text = "Temps alloue : " + Mathf.RoundToInt(totalMinutes) + " / " + maxTotalTime + " min";
        }

        if (pieChart != null)
        {
            pieChart.RefreshChart(); // Force XCharts redraw
        }
    }

    private void OnCloseAndValidate()
    {
        float valBanque = sliderBanque ? sliderBanque.value : 0f;
        float valActualites = sliderActualites ? sliderActualites.value : 0f;
        float valSalariat = sliderSalariat ? sliderSalariat.value : 0f;
        float valBourse = sliderBourse ? sliderBourse.value : 0f;

        // Convert the slider values into minutes for the validation output
        float minBanque = valBanque * maxTotalTime;
        float minActualites = valActualites * maxTotalTime;
        float minSalariat = valSalariat * maxTotalTime;
        float minBourse = valBourse * maxTotalTime;
        float total = minBanque + minActualites + minSalariat + minBourse;

        // Print final allocated values in minutes to the console
        Debug.Log("=== VALIDATION DE LA REPARTITION DU TEMPS ===");
        Debug.Log("Banque : " + Mathf.RoundToInt(minBanque) + " min");
        Debug.Log("Actualites : " + Mathf.RoundToInt(minActualites) + " min");
        Debug.Log("Salariat : " + Mathf.RoundToInt(minSalariat) + " min");
        Debug.Log("Bourse : " + Mathf.RoundToInt(minBourse) + " min");
        Debug.Log("Total alloue : " + Mathf.RoundToInt(total) + " / " + maxTotalTime + " min");
    }
}
