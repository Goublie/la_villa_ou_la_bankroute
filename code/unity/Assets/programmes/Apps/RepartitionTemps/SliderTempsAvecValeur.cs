using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Relie un slider de repartition du temps a son libelle numerique, sans
/// dependance aux donnees ou services metier.
/// </summary>
public sealed class SliderTempsAvecValeur : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text texteValeur;

    private string libelle;
    private bool listenerAjoute;

    /// <summary>
    /// Signale une modification utilisateur, exprimee en minutes.
    /// </summary>
    public event Action<SliderTempsAvecValeur, float> ValeurModifiee;

    public Slider Slider => slider;
    public TMP_Text TexteValeur => texteValeur;

    private void Awake()
    {
        ResoudreReferences();
        MemoriserLibelle();
    }

    private void OnEnable()
    {
        ResoudreReferences();
        MemoriserLibelle();
        AjouterListener();
        ActualiserAffichage();
    }

    private void OnDisable()
    {
        RetirerListener();
    }

    /// <summary>
    /// Configure les bornes du slider en minutes entieres.
    /// </summary>
    public void Configurer(int maximumMinutes)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = Mathf.Max(0, maximumMinutes);
        slider.wholeNumbers = true;
        ActualiserAffichage();
    }

    /// <summary>
    /// Applique une valeur sans emettre d'action utilisateur, puis rafraichit
    /// immediatement le texte associe.
    /// </summary>
    public void DefinirValeurSansNotification(float minutes)
    {
        if (slider == null)
        {
            return;
        }

        slider.SetValueWithoutNotify(
            Mathf.Clamp(minutes, slider.minValue, slider.maxValue));
        ActualiserAffichage();
    }

    /// <summary>
    /// Met a jour le libelle a partir de la valeur courante du slider.
    /// </summary>
    public void ActualiserAffichage()
    {
        if (slider == null || texteValeur == null)
        {
            return;
        }

        texteValeur.text = libelle + " : " +
            Mathf.RoundToInt(slider.value) + " min";
    }

    private void OnValeurSliderModifiee(float valeur)
    {
        ActualiserAffichage();
        ValeurModifiee?.Invoke(this, valeur);
    }

    private void AjouterListener()
    {
        if (slider == null || listenerAjoute)
        {
            return;
        }

        slider.onValueChanged.AddListener(OnValeurSliderModifiee);
        listenerAjoute = true;
    }

    private void RetirerListener()
    {
        if (slider == null || !listenerAjoute)
        {
            return;
        }

        slider.onValueChanged.RemoveListener(OnValeurSliderModifiee);
        listenerAjoute = false;
    }

    private void ResoudreReferences()
    {
        if (slider == null)
        {
            slider = GetComponentInChildren<Slider>(true);
        }

        if (texteValeur == null)
        {
            texteValeur = GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void MemoriserLibelle()
    {
        if (!string.IsNullOrWhiteSpace(libelle) || texteValeur == null)
        {
            return;
        }

        libelle = texteValeur.text == null
            ? string.Empty
            : texteValeur.text.Trim();
        const string suffixe = "(minutes)";
        if (libelle.EndsWith(suffixe, StringComparison.OrdinalIgnoreCase))
        {
            libelle = libelle
                .Substring(0, libelle.Length - suffixe.Length)
                .TrimEnd();
        }
    }
}
