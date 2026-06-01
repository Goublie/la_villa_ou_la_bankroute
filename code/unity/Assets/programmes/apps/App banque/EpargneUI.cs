using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class EpargneUI : MonoBehaviour
{
    //Informations à afficher dans l'onglet epargne
    [SerializeField] GameData G; //Le compte d'épargne
    
    private Epargne epgn;

    //Liuex d'entrée des informations
    [SerializeField] TMP_InputField inputCredit; //L'entrée du montant à crédit
    [SerializeField] TMP_InputField inputDebit; //L'entrée du montant à débiter

    //Lieux d'affichage des informations
    [SerializeField] TableauScroll tableauEpgn; //Le tableau d'affichage des opérations
    [SerializeField] TextMeshProUGUI texteSoldeEpgn; //Le texte affichant le solde du compte d'épargne
    [SerializeField] TextMeshProUGUI texteTaux; //Le texte affichant le taux de rendement du compte d'épargne

    void Start()
    {
        G.comptes.Add("epargne",new Epargne(0.03f,12));
        epgn = (Epargne)G.comptes["epargne"];
        G.investissements.Add(epgn.invest);
        ActualiserAffichage();
    }

    //Actualise l'affichage du solde et du taux
    public void ActualiserAffichage()
    {
        if (epgn == null)
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
        
        texteSoldeEpgn.text = "Solde : " + epgn.GetSolde().ToString();
        texteTaux.text = "Taux : " + (epgn.GetTaux() * 100).ToString("F2") + " %";
        ActualiserTableau();
    }

    //Ajoute une ligne au tableau d'affichage des opérations
    public void ActualiserTableau()
    {
        Historique histo = epgn.GetHistorique();
        tableauEpgn.Vider();
        for(int i = 0; i < histo.GetSize(); i++)
        {
            tableauEpgn.Add(histo.libelles[i], histo.montants[i].ToString(), "");
            Debug.Log("Tableau actualisé");
        }
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

        //On transfere les montant
        G.comptes["courant"].Transferer(epgn, "courant vers epargne", "Credit",  somme);
        ActualiserAffichage();
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
        epgn.Transferer(G.comptes["courant"], "Debit", "Versement depuis le compte epargne",  somme);
        ActualiserAffichage();
        Debug.Log("Saisie d'un débit");
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