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
        // On vérifie si le compte d'épargne existe déjà dans les données globales du jeu (ScriptableObject)
        // afin d'éviter une exception de clé dupliquée (ArgumentException) et de conserver le solde existant
        // lors des ouvertures répétées de l'interface ou des rechargements de scène.
        if (!G.comptes.ContainsKey("epargne"))
        {
            // Initialisation du compte d'épargne en passant la référence de GameData (G) pour la courbe de taux.
            // On fournit 0.0175f (1.75%) comme taux d'intérêt initial réglementé de juillet 2026.
            G.comptes.Add("epargne", new Epargne(G, 0.0175f, 12));
            epgn = (Epargne)G.comptes["epargne"];
            
            // Ajout du produit d'épargne à la liste globale des investissements pour le calcul mensuel des intérêts
            G.investissements.Add(epgn.invest);
        }
        else
        {
            epgn = (Epargne)G.comptes["epargne"];
        }

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

    //Actualise le tableau et ajoute une ligne au tableau d'affichage des opérations si nécessaire
    public void ActualiserTableau()
    {
        Historique histo = epgn.GetHistorique();
        tableauEpgn.Vider();
        
        for(int i = histo.GetSize() - 1; i >= 0; i--)
        {
            tableauEpgn.Add(histo.GetHistorique()[i]);
            Debug.Log("Tableau épargne actualisé");
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
        ActionPlay.OnMoisPasse += ActualiserAffichage;
    }

    private void OnDisable()
    {
        ActionPlay.OnMoisPasse -= ActualiserAffichage;
    }
}