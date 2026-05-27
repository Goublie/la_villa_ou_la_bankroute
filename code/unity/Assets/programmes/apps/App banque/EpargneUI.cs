using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class EpargneUI : MonoBehaviour
{
    //Informations à afficher dans l'onglet epargne
    [SerializeField] CompteBanquaire compteEpgn; //Le compte d'épargne
    
    //Liuex d'entrée des informations
    [SerializeField] TMP_InputField inputCredit; //L'entrée du montant à crédit
    [SerializeField] TMP_InputField inputDebit; //L'entrée du montant à débiter

    //Lieux d'affichage des informations
    [SerializeField] Tableau tableauEpgn; //Le tableau d'affichage des opérations
    [SerializeField] TextMeshProUGUI texteSoldeEpgn; //Le texte affichant le solde du compte d'épargne
    [SerializeField] TextMeshProUGUI texteTaux; //Le texte affichant le taux de rendement du compte d'épargne

    //Actualise l'affichage du solde et du taux
    public void ActualiserAffichage()
    {
        if (compteEpgn == null)
        {
            Debug.LogError("Le compte epargne n'est pas assigné dans le GestionBanqueUI.");
            return;
        }
        if (inputCredit == null)
        {
            Debug.LogError("L'entrée du crédit n'est pas assignée dans le GestionBanqueUI.");
            return;
        }
        if (inputDebit == null)
        {
            Debug.LogError("L'entrée du débit n'est pas assignée dans le GestionBanqueUI.");
            return;
        }
        if (tableauEpgn == null)
        {
            Debug.LogError("Le tableau n'est pas assigné dans le GestionBanqueUI.");
            return;
        }
        if (texteSoldeEpgn == null)
        {
            Debug.LogError("Le texte du solde n'est pas assigné dans le GestionBanqueUI.");
            return;
        }
        if (texteTaux == null)
        {
            Debug.LogError("Le texte du taux n'est pas assigné dans le GestionBanqueUI.");
            return;
        }
        
        texteSoldeEpgn.text = "Solde : " + compteEpgn.epargne.sommeInvestie.ToString();
        texteTaux.text = "Taux : " + (compteEpgn.epargne.taux * 100).ToString("F2") + " %";
    }

    //Ajoute une ligne au tableau d'affichage des opérations
    public void ajouterTableau(string texteC1, string texteC2, string texteC3)
    {
        if (tableauEpgn == null)
        {
            Debug.LogError("Le tableau n'est pas assigné dans le GestionBanqueUI.");
            return;
        }
        tableauEpgn.add(texteC1, texteC2, texteC3);
    }

    //Gère la saisie du crédit
    public void saisieCredit()
    {
        if (inputCredit == null)
        {
            Debug.LogError("L'entrée du crédit n'est pas assignée dans le GestionBanqueUI.");
            return;
        }
        
        string montantStr = inputCredit.text;
        
        if (string.IsNullOrEmpty(montantStr))
        {
            return;
        }

        float montant;
        if (!float.TryParse(montantStr, out montant))
        {
            Debug.Log("Veuillez entrer un montant valide.");
            return;
        }

        if (montant < 0)
        {
            Debug.Log("Le montant doit être positif.");
            return;
        }

        argent somme = new argent(montant);

        //On l'affiche dans le tableau
        if (compteEpgn.crediter(somme))
        {
            ajouterTableau("Credit", "+ " + (somme).ToString(), "");   
        }
    }

    //Gère la saisie du débit
    public void saisieDebit()
    {
        //On récupère le montant entré par l'utilisateur
        if (inputDebit == null)
        {
            Debug.LogError("L'entrée du débit n'est pas assignée dans le GestionBanqueUI.");
            return;
        }
        string montantStr = inputDebit.text;
        
        if (string.IsNullOrEmpty(montantStr))
        {
            Debug.Log("Veuillez entrer un montant.");
            return;
        }

        float montant;

        if (!float.TryParse(montantStr, out montant))
        {
            Debug.Log("Veuillez entrer un montant valide.");
            return;
        }

        if (montant < 0)
        {
            Debug.Log("Le montant doit être positif.");
            return;
        }

        argent somme = new argent(montant);
        
        if(compteEpgn.debiter(somme)) // On débite le compte banquaire
        {
            ajouterTableau("Débit", "",(-somme).ToString()); //si le débit s'est bien passé, on affiche
        }
    }

    void OnEnable()
    {
        ActionPlay.moisPasse += ActualiserAffichage;
    }

    private void OnDisable()
    {
        ActionPlay.moisPasse -= ActualiserAffichage;
    }
}