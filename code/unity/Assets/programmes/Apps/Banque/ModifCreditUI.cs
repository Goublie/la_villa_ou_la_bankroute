using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModifCreditUI : MonoBehaviour
{
    [Header("Données")]
    [SerializeField] private GameData G;
    [SerializeField] private CreditsUI listeCredits;
    
    [Header("Composants")]
    [SerializeField] private Slider sliderTempsRestant; // Permet de réduire la durée
    [SerializeField] private Slider sliderMensualite;   // Reflète l'augmentation de la mensualité
    
    [Header("Affichage")]
    [SerializeField] private Tableau tableauAncien;
    [SerializeField] private Tableau tableauSimulation;

    private DonneesPret pretCible;
    private bool estEnMiseAJourAutomatique = false;

    private void Start()
    {
        if (sliderTempsRestant != null) sliderTempsRestant.onValueChanged.AddListener(OnTempsChanged);
        if (sliderMensualite != null) sliderMensualite.onValueChanged.AddListener(OnMensualiteChanged);
    }

    private void OnEnable()
    {
        if (listeCredits != null)
        {
            ChargerPret(listeCredits.ObtenirPretSelectionne());
        }
    }

    public void ChargerPret(DonneesPret pret)
    {
        pretCible = pret;
        if (pretCible == null || pretCible.moisRestants <= 0) return;

        estEnMiseAJourAutomatique = true;
        
        if (tableauAncien != null)
        {
            foreach (var ligne in tableauAncien.tableau) ligne.Vider();
            if (tableauAncien.tableau.Count > 0)
            {
                tableauAncien.tableau[0].Set(
                    pretCible.capitalRestantDu.ToString(),
                    pretCible.mensualite.ToString(),
                    $"{pretCible.moisRestants} mois",
                    $"{pretCible.tauxAnnuel:F2} %"
                );
            }
        }

        sliderTempsRestant.minValue = 0;
        sliderTempsRestant.maxValue = pretCible.moisRestants;
        sliderTempsRestant.value = pretCible.moisRestants;

        // Le max de mensualité correspond à 1 mois restant (remboursement en 1 fois)
        argent mensualiteMax = new argent(pretCible.capitalRestantDu.centimes);
        sliderMensualite.minValue = pretCible.mensualite.centimes / 100f;
        sliderMensualite.maxValue = mensualiteMax.centimes / 100f;
        sliderMensualite.value = pretCible.mensualite.centimes / 100f;

        estEnMiseAJourAutomatique = false;

        RafraichirSimulationDepuisTemps();
    }

    private void OnTempsChanged(float value)
    {
        if (estEnMiseAJourAutomatique || pretCible == null) return;
        RafraichirSimulationDepuisTemps();
    }

    private void OnMensualiteChanged(float value)
    {
        if (estEnMiseAJourAutomatique || pretCible == null) return;
        RafraichirSimulationDepuisMensualite();
    }

    private void RafraichirSimulationDepuisTemps()
    {
        int moisVoulus = Mathf.RoundToInt(sliderTempsRestant.value);
        
        if (moisVoulus == 0)
        {
            // Remboursement total
            AfficherTextes(0, pretCible.capitalRestantDu, "Remboursement intégral immédiat !");
            estEnMiseAJourAutomatique = true;
            sliderMensualite.value = pretCible.capitalRestantDu.centimes / 100f;
            estEnMiseAJourAutomatique = false;
        }
        else
        {
            // Renégociation simulée
            DonneesPret sim = new DonneesPret(pretCible.montantEmprunte, pretCible.dureeAns, pretCible.tauxAnnuel, pretCible.mensualite);
            sim.capitalRestantDu = new argent(pretCible.capitalRestantDu.centimes);
            sim.moisRestants = pretCible.moisRestants;

            ServicePretImmobilier.RenegocierPret(sim, moisVoulus);

            AfficherTextes(moisVoulus, sim.mensualite, $"Nouvelle mensualité : {sim.mensualite.ToString()}");
            estEnMiseAJourAutomatique = true;
            sliderMensualite.value = sim.mensualite.centimes / 100f;
            estEnMiseAJourAutomatique = false;
        }
    }

    private void RafraichirSimulationDepuisMensualite()
    {
        float cibleMensualite = sliderMensualite.value;

        int meilleurMois = 1;
        float diffMin = float.MaxValue;
        argent meilleureMens = new argent(0);

        for (int m = 1; m <= pretCible.moisRestants; m++)
        {
            DonneesPret sim = new DonneesPret(pretCible.montantEmprunte, pretCible.dureeAns, pretCible.tauxAnnuel, pretCible.mensualite);
            sim.capitalRestantDu = new argent(pretCible.capitalRestantDu.centimes);
            sim.moisRestants = pretCible.moisRestants;
            ServicePretImmobilier.RenegocierPret(sim, m);

            float diff = Mathf.Abs((sim.mensualite.centimes / 100f) - cibleMensualite);
            if (diff < diffMin)
            {
                diffMin = diff;
                meilleurMois = m;
                meilleureMens = sim.mensualite;
            }
        }

        estEnMiseAJourAutomatique = true;
        sliderTempsRestant.value = meilleurMois;
        estEnMiseAJourAutomatique = false;

        AfficherTextes(meilleurMois, meilleureMens, $"Nouvelle mensualité : {meilleureMens.ToString()}");
    }

    private void AfficherTextes(int moisRestants, argent mensualite, string info)
    {
        if (tableauSimulation != null)
        {
            foreach (var ligne in tableauSimulation.tableau)
            {
                ligne.Vider();
            }
            if (tableauSimulation.tableau.Count > 0)
            {
                tableauSimulation.tableau[0].Set(
                    pretCible.capitalRestantDu.ToString(),
                    mensualite.ToString(),
                    $"{moisRestants} mois",
                    $"{pretCible.tauxAnnuel:F2} %"
                );
            }
        }
    }

    public void ValiderModification()
    {
        if (pretCible == null) return;
        int moisVoulus = Mathf.RoundToInt(sliderTempsRestant.value);

        if (moisVoulus == 0)
        {
            if (G.joueur.comptes.TryGetValue("courant", out CompteBanquaire courant))
            {
                ServicePretImmobilier.RemboursementAnticipe(pretCible, courant);
            }
        }
        else if (moisVoulus < pretCible.moisRestants)
        {
            ServicePretImmobilier.RenegocierPret(pretCible, moisVoulus);
        }

        pretCible = null;
        Retour();
    }

    public void Retour()
    {
        if (listeCredits != null) listeCredits.RafraichirListe();
    }
}
