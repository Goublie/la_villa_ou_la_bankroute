using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NouveauCreditUI : MonoBehaviour
{
    [Header("Données")]
    [SerializeField] private GameData G;
    [SerializeField] private CreditsUI listeCredits;

    [Header("Composants")]
    [SerializeField] private TMP_InputField inputMontant;
    [SerializeField] private Slider sliderDuree;      // de 3 à 30 ans
    [SerializeField] private Slider sliderMensualite; // en euros, borné selon la durée et le montant
    
    [Header("Affichage")]
    [SerializeField] private Tableau tableauSimulation;

    private bool estEnMiseAJourAutomatique = false;

    private void Start()
    {
        if (inputMontant != null) inputMontant.onValueChanged.AddListener(OnMontantChanged);
        if (sliderDuree != null) sliderDuree.onValueChanged.AddListener(OnDureeChanged);
        if (sliderMensualite != null) sliderMensualite.onValueChanged.AddListener(OnMensualiteChanged);

        if (sliderDuree != null)
        {
            sliderDuree.minValue = 3;
            sliderDuree.maxValue = 30;
            sliderDuree.value = 15;
        }

        RafraichirLimitesMensualite();
        RafraichirSimulation();
    }

    private void OnMontantChanged(string value)
    {
        RafraichirLimitesMensualite();
        RafraichirSimulation();
    }

    private void OnDureeChanged(float value)
    {
        if (estEnMiseAJourAutomatique) return;
        RafraichirSimulationDepuisDuree();
    }

    private void OnMensualiteChanged(float value)
    {
        if (estEnMiseAJourAutomatique) return;
        RafraichirSimulationDepuisMensualite();
    }

    private argent ObtenirMontant()
    {
        if (inputMontant != null && int.TryParse(inputMontant.text, out int m))
        {
            return new argent(m * 100);
        }
        return new argent(0);
    }

    private void RafraichirLimitesMensualite()
    {
        if (sliderMensualite == null) return;
        
        argent montant = ObtenirMontant();
        if (montant.centimes <= 0)
        {
            sliderMensualite.minValue = 0;
            sliderMensualite.maxValue = 0;
            return;
        }

        // Taux max (durée max = mensualité la plus basse)
        float tauxMaxDuree = ServicePretImmobilier.CalculerTaux(0, G.joueur.salaire, 30, 2.5f);
        argent mensualiteMin = ServicePretImmobilier.CalculerMensualite(montant, tauxMaxDuree, 30);

        // Taux min (durée min = mensualité la plus haute)
        float tauxMinDuree = ServicePretImmobilier.CalculerTaux(0, G.joueur.salaire, 3, 2.5f);
        argent mensualiteMax = ServicePretImmobilier.CalculerMensualite(montant, tauxMinDuree, 3);

        estEnMiseAJourAutomatique = true;
        sliderMensualite.minValue = mensualiteMin.centimes / 100f;
        sliderMensualite.maxValue = mensualiteMax.centimes / 100f;
        estEnMiseAJourAutomatique = false;
    }

    private void RafraichirSimulationDepuisDuree()
    {
        if (sliderDuree == null || sliderMensualite == null) return;

        int duree = Mathf.RoundToInt(sliderDuree.value);
        argent montant = ObtenirMontant();
        
        float tauxBCE = 2.5f; 
        float taux = ServicePretImmobilier.CalculerTaux(0, G.joueur.salaire, duree, tauxBCE);
        argent mensualite = ServicePretImmobilier.CalculerMensualite(montant, taux, duree);

        estEnMiseAJourAutomatique = true;
        sliderMensualite.value = mensualite.centimes / 100f;
        estEnMiseAJourAutomatique = false;

        AfficherTextes(duree, mensualite, taux);
    }

    private void RafraichirSimulationDepuisMensualite()
    {
        if (sliderDuree == null || sliderMensualite == null) return;

        float cibleEuros = sliderMensualite.value;
        argent montant = ObtenirMontant();
        float tauxBCE = 2.5f;

        int meilleureDuree = 3;
        float diffMin = float.MaxValue;
        argent meilleureMensualite = new argent(0);
        float meilleurTaux = 0f;

        for (int d = 3; d <= 30; d++)
        {
            float tauxTest = ServicePretImmobilier.CalculerTaux(0, G.joueur.salaire, d, tauxBCE);
            argent mens = ServicePretImmobilier.CalculerMensualite(montant, tauxTest, d);
            float diff = Mathf.Abs((mens.centimes / 100f) - cibleEuros);
            if (diff < diffMin)
            {
                diffMin = diff;
                meilleureDuree = d;
                meilleureMensualite = mens;
                meilleurTaux = tauxTest;
            }
        }

        estEnMiseAJourAutomatique = true;
        sliderDuree.value = meilleureDuree;
        estEnMiseAJourAutomatique = false;

        AfficherTextes(meilleureDuree, meilleureMensualite, meilleurTaux);
    }

    private void RafraichirSimulation()
    {
        RafraichirSimulationDepuisDuree();
    }

    private void AfficherTextes(int duree, argent mensualite, float taux)
    {
        if (tableauSimulation != null)
        {
            foreach (var ligne in tableauSimulation.tableau)
            {
                ligne.Vider();
            }
            if (tableauSimulation.tableau.Count > 0)
            {
                argent montant = ObtenirMontant();
                int moisRestants = duree * 12;
                tableauSimulation.tableau[0].Set(
                    montant.ToString(),
                    mensualite.ToString(),
                    $"{moisRestants}",
                    $"{taux:F2} %"
                );
            }
        }
    }

    public void ContracterPret()
    {
        argent montant = ObtenirMontant();
        if (montant.centimes <= 0 || sliderDuree == null) return;

        int duree = Mathf.RoundToInt(sliderDuree.value);

        float tauxBCE = 2.5f; 
        DonneesPret nouveauPret = ServicePretImmobilier.CreerPret(montant, duree, 0, G.joueur.salaire, tauxBCE);

        G.joueur.pretsImmobiliers.Add(nouveauPret);

        if (G.joueur.comptes.TryGetValue("courant", out CompteBanquaire courant))
        {
            courant.AjoutHistorique($"Emprunt Immobilier ({duree} ans)", montant);
        }

        Debug.Log($"Prêt contracté : {montant.ToString()} sur {duree} ans à {nouveauPret.tauxAnnuel:F2}%");

        if (inputMontant != null) inputMontant.text = "0";
        if (listeCredits != null) listeCredits.RafraichirListe();
    }
}
