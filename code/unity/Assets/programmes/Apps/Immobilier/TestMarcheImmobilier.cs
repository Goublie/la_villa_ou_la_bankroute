using UnityEngine;

public class TestMarcheImmobilier : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== Début du test d'intégration Marché Immobilier ===");

        // Test 1 : Prix m2 au Mois 0 (Juillet 2026)
        float prixParisM0 = MarcheImmobilier.ObtenirPrixM2("paris", 0);
        Debug.Log($"[TEST 1] Paris Mois 0 (Juillet 2026) : {prixParisM0} €/m2");

        // Test 2 : Récupération d'un bien du catalogue
        var bien = CatalogueImmobilier.ObtenirBiens()[0]; // Studio Marais Paris
        argent coutInitialCentimes = CatalogueImmobilier.CalculerPrixAchatArgent(bien, 0);
        Debug.Log($"[TEST 2] Bien : {bien.Nom} | Prix calculé : {coutInitialCentimes}");

        // Test 3 : Avancement de 12 mois (Juillet 2027)
        float prixParisM12 = MarcheImmobilier.ObtenirPrixM2("paris", 12);
        Debug.Log($"[TEST 3] Paris Mois 12 (Juillet 2027) : {prixParisM12} €/m2");
    }
}