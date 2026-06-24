using TMPro;
using UnityEngine;
using System.Collections;

/// <summary>
/// Affiche les ressources globales du joueur dans le HUD.
/// </summary>
public class HUDManager : MonoBehaviour
{
    public GameData gameData;
    public TextMeshProUGUI texteArgent;
    public TextMeshProUGUI texteEnergie;
    public TextMeshProUGUI texteSanteMentale;
    public TextMeshProUGUI texteMois;
    public TextMeshProUGUI texteGainPerte;

    [Header("Configuration Animations")]
    public float dureeAffichageDelta = 1.2f;
    public float dureeDefilementArgent = 0.6f;

    private CompteBanquaire compteCourantAbonne;
    private int dernierSoldeConnu = -1;
    private Coroutine coroutineAnimationGlobale;
    private Vector3 positionInitialeDelta;
    private bool enAttenteValidationTemps = false;

    private void Start()
    {
        if (texteGainPerte != null)
        {
            positionInitialeDelta = texteGainPerte.transform.localPosition;
            texteGainPerte.gameObject.SetActive(false);
        }
        ActualiserAffichage();
    }

    private void OnEnable()
    {
        AbonnerCompteCourant();
        ActionPlay.OnMoisPasse += OnMoisPasseEnclenche;
        RepartitionTempsUI.AllocationValidee += OnAllocationValidee;
    }

    private void OnDisable()
    {
        DesabonnerCompteCourant();
        ActionPlay.OnMoisPasse -= OnMoisPasseEnclenche;
        RepartitionTempsUI.AllocationValidee -= OnAllocationValidee;
    }

    /// <summary>
    /// CETTE FONCTION EST LA CLÉ : À appeler absolument au clic du bouton vert !
    /// </summary>
    public void VerrouillerAffichageAvantBanque()
    {
        enAttenteValidationTemps = true;
        
        // On force la sauvegarde du solde visuel actuel AVANT que la banque ne change quoi que ce soit
        if (compteCourantAbonne != null && dernierSoldeConnu == -1)
        {
            string soldeString = compteCourantAbonne.GetSolde().ToString();
            dernierSoldeConnu = ExtraireIntDepuisArgentString(soldeString);
        }
        Debug.Log($"[HUDManager] Verrou activé manuellement. Solde bloqué à l'écran : {dernierSoldeConnu}€");
    }
    
    private void OnMoisPasseEnclenche()
    {
        enAttenteValidationTemps = true;
        ActualiserAffichage();
    }

    private void OnAllocationValidee()
    {
        enAttenteValidationTemps = false;

        if (texteArgent != null && compteCourantAbonne != null)
        {
            string soldeString = compteCourantAbonne.GetSolde().ToString();
            int soldeActuel = ExtraireIntDepuisArgentString(soldeString);

            Debug.Log($"[HUDManager] Validation ! Comparaison : Ancien bloqué = {dernierSoldeConnu}€ | Nouveau banque = {soldeActuel}€");

            if (soldeActuel != dernierSoldeConnu)
            {
                if (coroutineAnimationGlobale != null)
                {
                    StopCoroutine(coroutineAnimationGlobale);
                }

                coroutineAnimationGlobale = StartCoroutine(SequenceAnimationArgent(dernierSoldeConnu, soldeActuel, soldeString));
                dernierSoldeConnu = soldeActuel;
            }
        }
    }

    public void ActualiserAffichage()
    {
        if (gameData == null || gameData.joueur == null) return;

        gameData.joueur.InitialiserSiNecessaire();
        AbonnerCompteCourant();

        if (texteArgent != null && compteCourantAbonne != null)
        {
            string soldeString = compteCourantAbonne.GetSolde().ToString();
            int soldeActuel = ExtraireIntDepuisArgentString(soldeString);
            
            if (dernierSoldeConnu == -1)
            {
                dernierSoldeConnu = soldeActuel;
                texteArgent.text = soldeString;
            }
            // IMPORTANT : Si le verrou est actif, on N'ÉCRASE PAS le texte à l'écran, on attend la validation !
            else if (!enAttenteValidationTemps && coroutineAnimationGlobale == null)
            {
                texteArgent.text = soldeString;
                dernierSoldeConnu = soldeActuel;
            }
        }

        if (texteEnergie != null) texteEnergie.text = gameData.joueur.energie + "%";
        if (texteSanteMentale != null) texteSanteMentale.text = gameData.joueur.santeMentale + "/100";

        if (texteMois != null)
        {
            int moisDepart = 7;
            int anneeDepart = 2026;
            int totalMois = moisDepart + gameData.nombreMoisPasses - 1;
            int moisCourant = (totalMois % 12) + 1;
            int anneeCourante = anneeDepart + (totalMois / 12);
            texteMois.text = moisCourant.ToString("D2") + "/" + anneeCourante;
        }
    }

    private IEnumerator SequenceAnimationArgent(int depart, int fin, string soldeStringFinal)
    {
        int delta = fin - depart;

        if (texteGainPerte != null)
        {
            if (delta > 0)
            {
                texteGainPerte.text = "+" + delta + "€";
                texteGainPerte.color = new Color(0.1f, 0.75f, 0.1f, 1f);
            }
            else
            {
                texteGainPerte.text = delta + "€";
                texteGainPerte.color = new Color(0.85f, 0.15f, 0.15f, 1f);
            }

            texteGainPerte.transform.localPosition = positionInitialeDelta;
            texteGainPerte.gameObject.SetActive(true);

            float tempsDelta = 0f;
            while (tempsDelta < dureeAffichageDelta)
            {
                tempsDelta += Time.deltaTime;
                float progression = tempsDelta / dureeAffichageDelta;
                texteGainPerte.transform.localPosition = positionInitialeDelta + new Vector3(0f, progression * 30f, 0f);
                Color couleurCourante = texteGainPerte.color;
                couleurCourante.a = Mathf.Lerp(1f, 0f, progression);
                texteGainPerte.color = couleurCourante;
                yield return null;
            }

            texteGainPerte.gameObject.SetActive(false);
        }

        float tempsEcoule = 0f;
        while (tempsEcoule < dureeDefilementArgent)
        {
            tempsEcoule += Time.deltaTime;
            float progression = tempsEcoule / dureeDefilementArgent;
            int valeurIntermediaire = Mathf.RoundToInt(Mathf.Lerp(depart, fin, progression));
            texteArgent.text = valeurIntermediaire.ToString() + "€";
            yield return null;
        }

        texteArgent.text = soldeStringFinal;
        coroutineAnimationGlobale = null; 
    }
    
    private int ExtraireIntDepuisArgentString(string text)
    {
        if (text.Contains(",")) text = text.Split(',')[0];
        else if (text.Contains(".")) text = text.Split('.')[0];
        string cleanText = "";
        foreach (char c in text)
        {
            if (char.IsDigit(c) || c == '-') cleanText += c;
        }
        if (int.TryParse(cleanText, out int resultat)) return resultat;
        return 0;
    }

    private void AbonnerCompteCourant()
    {
        if (gameData == null || gameData.joueur == null) return;
        CompteBanquaire compte = new ServiceBanque(gameData.joueur).ObtenirCompteCourant();
        if (ReferenceEquals(compte, compteCourantAbonne)) return;
        DesabonnerCompteCourant();
        compteCourantAbonne = compte;
        compteCourantAbonne.OnSoldeModifie += ActualiserAffichage;
    }

    private void DesabonnerCompteCourant()
    {
        if (compteCourantAbonne != null)
        {
            compteCourantAbonne.OnSoldeModifie -= ActualiserAffichage;
            compteCourantAbonne = null;
        }
    }
}