using System;
using System.IO;
using UnityEngine;

public static class ServicePretImmobilier
{
    private const float MargeBanque = 1.5f; // 1.5% fixe

    /// <summary>
    /// Calcule le taux final du pret selon le profil du joueur et la conjoncture.
    /// </summary>
    public static float CalculerTaux(int anneeJeu, argent salaireAnnuel, int dureeAns, float tauxBCE)
    {
        float primeAnciennete = 0f;
        if (anneeJeu >= 0 && anneeJeu < 5) primeAnciennete = 1.0f;       // 0 -> 5 ans
        else if (anneeJeu >= 5 && anneeJeu < 15) primeAnciennete = 0.5f; // 5 -> 15 ans
        else if (anneeJeu >= 15 && anneeJeu < 30) primeAnciennete = 0.0f;// 15 -> 30 ans
        else if (anneeJeu >= 30) primeAnciennete = 0.3f;                 // 30 -> 40 ans

        // Conversion du salaire annuel en Euros pour correspondre au bareme
        float salaireEuros = salaireAnnuel.centimes / 100f;
        float primeSalaire = 0f;
        if (salaireEuros < 20000f) primeSalaire = 1.0f;
        else if (salaireEuros >= 20000f && salaireEuros < 40000f) primeSalaire = 0.5f;
        else if (salaireEuros >= 40000f && salaireEuros <= 70000f) primeSalaire = 0.0f;
        else if (salaireEuros > 70000f) primeSalaire = -0.3f;

        float primeDuree = 0f;
        switch (dureeAns)
        {
            case 3: primeDuree = -0.5f; break;
            case 5: primeDuree = 0.0f; break;
            case 10: primeDuree = 0.3f; break;
            case 15: primeDuree = 0.6f; break;
            case 20: primeDuree = 1.0f; break;
            default:
                Debug.LogWarning($"[ServicePret] Durée de {dureeAns} ans non standard. Prime durée à 0%.");
                break;
        }

        // Formule complète : taux_pret = taux_BCE + marge_banque + primes
        return tauxBCE + MargeBanque + primeAnciennete + primeSalaire + primeDuree;
    }

    /// <summary>
    /// Formule classique de mensualite amortissable : M = C × (t/12) / (1 - (1 + t/12)^-n)
    /// </summary>
    public static argent CalculerMensualite(argent montantEmprunte, float tauxAnnuel, int dureeAns)
    {
        if (montantEmprunte.centimes <= 0) return new argent(0);

        int nMois = dureeAns * 12;
        
        // Si le taux est strictement à 0, simple division linéaire
        if (tauxAnnuel <= 0f)
        {
            return new argent(montantEmprunte.centimes / nMois);
        }

        // t = taux mensuel sous forme de ratio (ex: 6% annuel -> 0.06 / 12)
        double tMensuel = (tauxAnnuel / 100.0) / 12.0;
        double capital = montantEmprunte.centimes;

        double num = capital * tMensuel;
        double den = 1.0 - Math.Pow(1.0 + tMensuel, -nMois);

        int mensualiteCentimes = Mathf.RoundToInt((float)(num / den));
        return new argent(mensualiteCentimes);
    }

    /// <summary>
    /// Initialise un nouvel objet DonneesPret calculé selon les règles de la banque.
    /// </summary>
    public static DonneesPret CreerPret(argent montantEmprunte, int dureeAns, int anneeJeu, argent salaireAnnuel, float tauxBCE)
    {
        float tauxFinal = CalculerTaux(anneeJeu, salaireAnnuel, dureeAns, tauxBCE);
        argent mensualite = CalculerMensualite(montantEmprunte, tauxFinal, dureeAns);
        
        return new DonneesPret(montantEmprunte, dureeAns, tauxFinal, mensualite);
    }

    /// <summary>
    /// Applique le prelevement mensuel sur le compte courant du joueur.
    /// </summary>
    public static void AppliquerMensualite(DonneesPret pret, CompteBanquaire compteCourant)
    {
        if (pret.moisRestants <= 0) return;

        // Préparer le libellé dynamique pour l'historique bancaire
        int moisActuelDuPret = (pret.dureeAns * 12) - pret.moisRestants + 1;
        string libelle = $"Prêt Immo ({moisActuelDuPret}/{pret.dureeAns * 12})";

        // Débit (on passe une valeur négative à l'historique)
        compteCourant.AjoutHistorique(libelle, -pret.mensualite);

        // Amortissement (Mise à jour de l'état du prêt)
        pret.moisRestants--;
        
        if (pret.moisRestants <= 0)
        {
            pret.capitalRestantDu = new argent(0);
            compteCourant.AjoutHistorique("Prêt Immobilier : SOLDÉ", new argent(0));
        }
        else
        {
            // Approximation de l'amortissement du capital restant dû
            // (Idéal pour le gameplay : réduit au fur et à mesure)
            int nouveauRestant = pret.capitalRestantDu.centimes - pret.mensualite.centimes;
            pret.capitalRestantDu = new argent(Mathf.Max(0, nouveauRestant));
        }
    }

    /// <summary>
    /// Rembourse la totalité du capital restant dû et solde le prêt.
    /// </summary>
    public static void RemboursementAnticipe(DonneesPret pret, CompteBanquaire compteCourant)
    {
        if (pret.moisRestants <= 0) return;

        compteCourant.AjoutHistorique("Remboursement anticipé", -pret.capitalRestantDu);
        pret.moisRestants = 0;
        pret.capitalRestantDu = new argent(0);
        compteCourant.AjoutHistorique("Prêt Immobilier : SOLDÉ", new argent(0));
    }

    /// <summary>
    /// Recalcule la mensualité d'un prêt selon une nouvelle durée restante (raccourcissement).
    /// </summary>
    public static void RenegocierPret(DonneesPret pret, int nouveauxMoisRestants)
    {
        if (nouveauxMoisRestants <= 0 || nouveauxMoisRestants >= pret.moisRestants) return;

        if (pret.tauxAnnuel <= 0f)
        {
            pret.mensualite = new argent(pret.capitalRestantDu.centimes / nouveauxMoisRestants);
        }
        else
        {
            double tMensuel = (pret.tauxAnnuel / 100.0) / 12.0;
            double capital = pret.capitalRestantDu.centimes;

            double num = capital * tMensuel;
            double den = 1.0 - Math.Pow(1.0 + tMensuel, -nouveauxMoisRestants);

            pret.mensualite = new argent(Mathf.RoundToInt((float)(num / den)));
        }

        // On met à jour les mois restants sans toucher à dureeAns pour conserver l'historique
        pret.moisRestants = nouveauxMoisRestants;
    }
}

// ==========================================
// OUTIL DE LECTURE DU FICHIER taux_bce.json
// ==========================================
[Serializable]
public class WrapperTauxBCE
{
    // Classe calquée sur un JSON structuré en tableau ou dictionnaire
    // Exemple de format attendu : { "tauxParAnnee": [1.5, 2.1, 0.5, ...] }
    public float[] tauxParAnnee;
}

public static class ChargeurTauxBCE
{
    public static float[] ChargerFichierTaux()
    {
        string chemin = Path.Combine(Application.streamingAssetsPath, "taux_bce.json");
        
        // Alternative si ton Python génère dans persistentDataPath :
        // string chemin = Path.Combine(Application.persistentDataPath, "taux_bce.json");

        if (File.Exists(chemin))
        {
            try
            {
                string json = File.ReadAllText(chemin);
                WrapperTauxBCE wrapper = JsonUtility.FromJson<WrapperTauxBCE>(json);
                return wrapper.tauxParAnnee;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ChargeurTauxBCE] Erreur de lecture JSON : {e.Message}");
            }
        }
        
        Debug.LogWarning("[ChargeurTauxBCE] Fichier introuvable, génération d'un barème de secours par défaut.");
        return new float[41]; // Tableau vide par défaut si fichier manquant
    }
}