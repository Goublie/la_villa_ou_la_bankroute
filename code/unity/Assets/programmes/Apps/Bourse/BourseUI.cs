using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using XCharts.Runtime;

public class BourseUI : MonoBehaviour
{
    private enum CategorieActif
    {
        Indices,
        Actions,
        Crypto,
        Energie,
        Defensif
    }

    [Serializable]
    private class PointMarche
    {
        public int Mois;
        public float Close;
    }

    [Serializable]
    private class PointLivret
    {
        public float Rendement_mensuel_pct;
    }

    private sealed class ActifMarche
    {
        public string id;
        public string nom;
        public CategorieActif categorie;
        public string niveauRisque;
        public string description;
        public readonly List<float> prix = new List<float>();
    }

    private static readonly int[] MontantsOrdreCentimes =
    {
        10000,
        25000,
        50000,
        100000,
        250000
    };

    [Header("Données partagées")]
    [SerializeField] private GameData gameData;

    [Header("Sélection et marché")]
    [SerializeField] private TMP_Text categorieText;
    [SerializeField] private TMP_Text actifText;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private TMP_Text historiqueText;
    [SerializeField] private RectTransform graphiqueRoot;

    [Header("Ordre et portefeuille")]
    [SerializeField] private TMP_Text cashText;
    [SerializeField] private TMP_Text ordreText;
    [SerializeField] private TMP_Text portefeuilleText;
    [SerializeField] private TMP_Text journalText;

    [Header("État de l'interface")]
    [SerializeField] private CategorieActif categorieSelectionnee;
    [SerializeField] private int indexActifSelectionne;
    [SerializeField] private int indexMontantOrdre = 2;

    private readonly List<ActifMarche> actifs = new List<ActifMarche>();
    private readonly List<ActifMarche> actifsCategorie = new List<ActifMarche>();

    private CompteBanquaire compteCourant;
    private HUDManager hudManager;
    private ActionPlay actionPlay;
    private LineChart lineChart;
    private bool ecouteSoldeActive;

    private void Awake()
    {
        ResoudreDependances();
        ChargerActifs();
        TrouverCompteCourant();
    }

    private void OnEnable()
    {
        ResoudreDependances();
        ChargerActifs();
        TrouverCompteCourant();
        AbonnerSolde();
        ActionPlay.OnMoisPasse += MettreAJourMarchePourNouveauMois;
        VerifierMoisObserve();
        ActualiserAffichage();
    }

    private void OnDisable()
    {
        ActionPlay.OnMoisPasse -= MettreAJourMarchePourNouveauMois;
        DesabonnerSolde();
    }

    public void CategorieSuivante()
    {
        int nombreCategories = Enum.GetValues(typeof(CategorieActif)).Length;
        categorieSelectionnee =
            (CategorieActif)(((int)categorieSelectionnee + 1) % nombreCategories);
        indexActifSelectionne = 0;
        ActualiserAffichage();
    }

    public void ActifSuivant()
    {
        ActualiserListeCategorie();
        if (actifsCategorie.Count == 0)
        {
            return;
        }

        indexActifSelectionne = (indexActifSelectionne + 1) % actifsCategorie.Count;
        ActualiserAffichage();
    }

    public void MontantSuivant()
    {
        indexMontantOrdre = (indexMontantOrdre + 1) % MontantsOrdreCentimes.Length;
        ActualiserAffichage();
    }

    public void Acheter()
    {
        ActifMarche actif = ActifSelectionne;
        DonneesBourse bourse = ObtenirDonneesBourse();
        int montantCentimes = MontantOrdreCentimes;

        if (actif == null || bourse == null)
        {
            EcrireJournal("Les données de marché sont indisponibles.");
            return;
        }

        if (!TrouverCompteCourant())
        {
            EcrireJournal("Le compte courant est indisponible.");
            return;
        }

        if (compteCourant.GetSolde().centimes < montantCentimes)
        {
            EcrireJournal(
                "Cash insuffisant pour investir " + FormaterArgent(montantCentimes) + ".");
            return;
        }

        float prix = PrixActuel(actif);
        if (prix <= 0f)
        {
            EcrireJournal("Le prix de cet actif est indisponible.");
            return;
        }

        float quantiteAchetee = montantCentimes / (prix * 100f);
        PositionBourse position = bourse.ObtenirOuCreerPosition(actif.id);
        position.quantite += quantiteAchetee;
        position.coutTotalCentimes += montantCentimes;

        compteCourant.AjoutHistorique(
            "Achat " + actif.nom,
            new argent(-montantCentimes));

        EcrireJournal(
            "Achat réussi : " + FormaterQuantite(quantiteAchetee) + " " + actif.nom +
            " pour " + FormaterArgent(montantCentimes) + ".");
        RafraichirHUD();
    }

    public void Vendre()
    {
        ActifMarche actif = ActifSelectionne;
        DonneesBourse bourse = ObtenirDonneesBourse();
        PositionBourse position = bourse != null && actif != null
            ? bourse.TrouverPosition(actif.id)
            : null;

        if (position == null || position.quantite <= 0f)
        {
            EcrireJournal("Vous ne détenez aucune position sur cet actif.");
            return;
        }

        float prix = PrixActuel(actif);
        if (prix <= 0f)
        {
            EcrireJournal("Le prix de cet actif est indisponible.");
            return;
        }

        float quantiteDemandee = MontantOrdreCentimes / (prix * 100f);
        float quantiteVendue = Mathf.Min(position.quantite, quantiteDemandee);
        VendrePosition(actif, position, quantiteVendue, false);
    }

    public void ToutVendre()
    {
        ActifMarche actif = ActifSelectionne;
        DonneesBourse bourse = ObtenirDonneesBourse();
        PositionBourse position = bourse != null && actif != null
            ? bourse.TrouverPosition(actif.id)
            : null;

        if (position == null || position.quantite <= 0f)
        {
            EcrireJournal("Vous ne détenez aucune position sur cet actif.");
            return;
        }

        VendrePosition(actif, position, position.quantite, true);
    }

    public void UpdateMarketForNewMonth()
    {
        MettreAJourMarchePourNouveauMois();
    }

    public void ActualiserAffichage()
    {
        ResoudreDependances();
        ChargerActifs();
        TrouverCompteCourant();
        ActualiserListeCategorie();

        ActifMarche actif = ActifSelectionne;
        DonneesBourse bourse = ObtenirDonneesBourse();
        PositionBourse position = actif != null && bourse != null
            ? bourse.TrouverPosition(actif.id)
            : null;

        AffecterTexte(
            categorieText,
            "<mark=#DCE8FFFF>  Catégorie : " +
            NomCategorie(categorieSelectionnee) + "  >  </mark>");
        AffecterTexte(
            actifText,
            actif != null
                ? "<mark=#DCE8FFFF>  Actif : " + actif.nom + "  >  </mark>"
                : "Aucun actif disponible");
        AffecterTexte(
            cashText,
            "Cash disponible : " +
            (compteCourant != null ? compteCourant.GetSolde().ToString() : "indisponible"));
        AffecterTexte(
            ordreText,
            "<mark=#FFF1B8FF>  Montant de l'ordre : " +
            FormaterArgent(MontantOrdreCentimes) + "  >  </mark>");

        if (actif != null)
        {
            AffecterTexte(detailsText, ConstruireDetailsActif(actif, position));
            ActualiserGraphique(actif);
        }
        else
        {
            AffecterTexte(detailsText, "Aucune donnée disponible pour cette catégorie.");
            AffecterTexte(historiqueText, string.Empty);
        }

        AffecterTexte(portefeuilleText, ConstruireResumePortefeuille());
        AffecterTexte(
            journalText,
            "<b>Journal du marché</b>\n" +
            (bourse != null ? bourse.dernierMessage : "Données indisponibles."));
    }

    private int MontantOrdreCentimes
    {
        get
        {
            indexMontantOrdre = Mathf.Clamp(
                indexMontantOrdre,
                0,
                MontantsOrdreCentimes.Length - 1);
            return MontantsOrdreCentimes[indexMontantOrdre];
        }
    }

    private ActifMarche ActifSelectionne
    {
        get
        {
            ActualiserListeCategorie();
            if (actifsCategorie.Count == 0)
            {
                return null;
            }

            indexActifSelectionne = Mathf.Clamp(
                indexActifSelectionne,
                0,
                actifsCategorie.Count - 1);
            return actifsCategorie[indexActifSelectionne];
        }
    }

    private void ChargerActifs()
    {
        if (actifs.Count > 0)
        {
            return;
        }

        AjouterActif(
            "cac40",
            "CAC 40",
            CategorieActif.Indices,
            "Modéré",
            "Indice des grandes capitalisations françaises.",
            ChargerCourbe("Bourse/cac40"));
        AjouterActif(
            "nvidia",
            "Nvidia",
            CategorieActif.Actions,
            "Élevé",
            "Action technologique à forte croissance et forte volatilité.",
            ChargerCourbe("Bourse/nvidia"));
        AjouterActif(
            "alphabet",
            "Alphabet",
            CategorieActif.Actions,
            "Modéré",
            "Groupe technologique diversifié, plus stable que les valeurs de rupture.",
            ChargerCourbe("Bourse/alphabet"));
        AjouterActif(
            "bitcoin",
            "Bitcoin",
            CategorieActif.Crypto,
            "Très élevé",
            "Cryptoactif très volatil, sensible au sentiment de marché.",
            ChargerCourbe("Bourse/bitcoin"));
        AjouterActif(
            "totalenergies",
            "TotalEnergies",
            CategorieActif.Energie,
            "Modéré",
            "Action énergétique sensible aux prix des matières premières et à la transition.",
            ChargerCourbe("Bourse/totalenergies"));
        AjouterActif(
            "livret_a",
            "Livret A",
            CategorieActif.Defensif,
            "Faible",
            "Placement réglementé stable dont le rendement suit les données de taux du projet.",
            ChargerCourbeLivretA());
    }

    private void AjouterActif(
        string id,
        string nom,
        CategorieActif categorie,
        string niveauRisque,
        string description,
        List<float> prix)
    {
        if (prix == null || prix.Count == 0)
        {
            Debug.LogWarning("[Bourse] Données absentes pour " + nom + ".");
            return;
        }

        ActifMarche actif = new ActifMarche
        {
            id = id,
            nom = nom,
            categorie = categorie,
            niveauRisque = niveauRisque,
            description = description
        };
        actif.prix.AddRange(prix);
        actifs.Add(actif);
    }

    private static List<float> ChargerCourbe(string cheminResources)
    {
        TextAsset fichier = Resources.Load<TextAsset>(cheminResources);
        if (fichier == null)
        {
            return new List<float>();
        }

        try
        {
            List<PointMarche> points =
                JsonConvert.DeserializeObject<List<PointMarche>>(fichier.text);
            List<float> prix = new List<float>();
            if (points != null)
            {
                foreach (PointMarche point in points)
                {
                    if (point != null && point.Close > 0f)
                    {
                        prix.Add(point.Close);
                    }
                }
            }

            return prix;
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "[Bourse] Lecture impossible pour " + cheminResources + " : " +
                exception.Message);
            return new List<float>();
        }
    }

    private static List<float> ChargerCourbeLivretA()
    {
        TextAsset fichier =
            Resources.Load<TextAsset>("livret_a_simulation_40ans_simplifie");
        if (fichier == null)
        {
            return new List<float>();
        }

        try
        {
            Dictionary<string, List<PointLivret>> donnees =
                JsonConvert.DeserializeObject<Dictionary<string, List<PointLivret>>>(
                    fichier.text);
            if (donnees == null ||
                !donnees.TryGetValue("livret_a_simulation", out List<PointLivret> points) ||
                points == null ||
                points.Count == 0)
            {
                return new List<float>();
            }

            List<float> prix = new List<float> { 100f };
            float valeur = 100f;
            const int decalageJuillet2026 = 6;

            for (int mois = 0; mois < 480; mois++)
            {
                int index = Mathf.Min(decalageJuillet2026 + mois, points.Count - 1);
                valeur *= 1f + (points[index].Rendement_mensuel_pct / 100f);
                prix.Add(valeur);
            }

            return prix;
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "[Bourse] Lecture impossible pour le Livret A : " + exception.Message);
            return new List<float>();
        }
    }

    private void ActualiserListeCategorie()
    {
        actifsCategorie.Clear();
        foreach (ActifMarche actif in actifs)
        {
            if (actif.categorie == categorieSelectionnee)
            {
                actifsCategorie.Add(actif);
            }
        }

        if (indexActifSelectionne >= actifsCategorie.Count)
        {
            indexActifSelectionne = 0;
        }
    }

    private string ConstruireDetailsActif(
        ActifMarche actif,
        PositionBourse position)
    {
        float prix = PrixActuel(actif);
        float variationMensuelle = CalculerVariation(actif, 1);
        float variationAnnuelle = CalculerVariation(actif, 12);
        float volatilite = CalculerVolatilite(actif);
        float rendementMoyen = CalculerRendementAnnualise(actif);
        float quantite = position != null ? position.quantite : 0f;
        int valeurPosition = Mathf.RoundToInt(quantite * prix * 100f);
        int plusValue = position != null ? valeurPosition - position.coutTotalCentimes : 0;

        return
            "<b>" + actif.nom + "</b>  •  " + NomCategorie(actif.categorie) + "\n" +
            "Prix actuel : " + FormaterPrix(prix) +
            "   Variation mensuelle : " + FormaterPourcentage(variationMensuelle) + "\n" +
            "Tendance annuelle : " + FormaterPourcentage(variationAnnuelle) +
            "   Rendement moyen : " + FormaterPourcentage(rendementMoyen) + "\n" +
            "Risque : " + actif.niveauRisque +
            "   Volatilité 12 mois : " + volatilite.ToString("F1") + " %\n" +
            actif.description + "\n\n" +
            "Position : " + FormaterQuantite(quantite) +
            "   Valeur : " + FormaterArgent(valeurPosition) +
            "   Plus-value : " + FormaterMontantSigne(plusValue);
    }

    private string ConstruireResumePortefeuille()
    {
        DonneesBourse bourse = ObtenirDonneesBourse();
        if (bourse == null)
        {
            return "Portefeuille indisponible.";
        }

        int valeurTotale = 0;
        int coutTotal = 0;
        int positionsActives = 0;

        if (bourse.positions != null)
        {
            foreach (PositionBourse position in bourse.positions)
            {
                ActifMarche actif = TrouverActif(position != null ? position.actifId : null);
                if (position == null || actif == null || position.quantite <= 0f)
                {
                    continue;
                }

                positionsActives++;
                valeurTotale += Mathf.RoundToInt(
                    position.quantite * PrixActuel(actif) * 100f);
                coutTotal += position.coutTotalCentimes;
            }
        }

        int performance = valeurTotale - coutTotal;
        float performancePourcent = coutTotal > 0
            ? performance * 100f / coutTotal
            : 0f;

        return
            "<b>Portefeuille</b>\n" +
            "Positions : " + positionsActives +
            "   Valeur totale : " + FormaterArgent(valeurTotale) + "\n" +
            "Capital investi : " + FormaterArgent(coutTotal) + "\n" +
            "Performance : " + FormaterMontantSigne(performance) +
            " (" + FormaterPourcentage(performancePourcent) + ")";
    }

    private void ActualiserGraphique(ActifMarche actif)
    {
        if (graphiqueRoot == null || actif == null)
        {
            AffecterTexte(historiqueText, ConstruireHistoriqueTexte(actif));
            return;
        }

        if (lineChart == null)
        {
            lineChart = graphiqueRoot.GetComponent<LineChart>();
        }

        if (lineChart == null)
        {
            lineChart = graphiqueRoot.gameObject.AddComponent<LineChart>();
        }

        if (lineChart == null)
        {
            AffecterTexte(historiqueText, ConstruireHistoriqueTexte(actif));
            return;
        }

        AffecterTexte(historiqueText, string.Empty);
        lineChart.ClearData();
        while (lineChart.series.Count < 1)
        {
            lineChart.AddSerie<Line>();
        }
        while (lineChart.series.Count > 1)
        {
            lineChart.series.RemoveAt(lineChart.series.Count - 1);
        }

        lineChart.series[0].serieName = actif.nom;
        lineChart.series[0].show = true;

        int moisActuel = IndexPrixActuel(actif);
        int premierMois = Mathf.Max(0, moisActuel - 11);
        for (int mois = premierMois; mois <= moisActuel; mois++)
        {
            lineChart.AddXAxisData("M" + mois);
            lineChart.AddData(0, actif.prix[mois]);
        }

        lineChart.SetAllDirty();
    }

    private string ConstruireHistoriqueTexte(ActifMarche actif)
    {
        if (actif == null)
        {
            return string.Empty;
        }

        int moisActuel = IndexPrixActuel(actif);
        int premierMois = Mathf.Max(0, moisActuel - 5);
        string resultat = "<b>Historique récent</b>\n";
        for (int mois = premierMois; mois <= moisActuel; mois++)
        {
            if (mois > premierMois)
            {
                resultat += "  •  ";
            }

            resultat += "M" + mois + " " + FormaterPrix(actif.prix[mois]);
        }

        return resultat;
    }

    private void VendrePosition(
        ActifMarche actif,
        PositionBourse position,
        float quantiteVendue,
        bool venteTotale)
    {
        if (!TrouverCompteCourant())
        {
            EcrireJournal("Le compte courant est indisponible.");
            return;
        }

        float quantiteAvant = position.quantite;
        float proportionVendue = quantiteAvant > 0f
            ? quantiteVendue / quantiteAvant
            : 0f;
        int produitVente = Mathf.RoundToInt(quantiteVendue * PrixActuel(actif) * 100f);
        int coutCede = venteTotale
            ? position.coutTotalCentimes
            : Mathf.RoundToInt(position.coutTotalCentimes * proportionVendue);

        position.quantite = Mathf.Max(0f, position.quantite - quantiteVendue);
        position.coutTotalCentimes = Mathf.Max(0, position.coutTotalCentimes - coutCede);

        DonneesBourse bourse = ObtenirDonneesBourse();
        bourse.SupprimerPositionsVides();
        compteCourant.AjoutHistorique(
            "Vente " + actif.nom,
            new argent(produitVente));

        int resultat = produitVente - coutCede;
        EcrireJournal(
            "Vente réussie : " + FormaterQuantite(quantiteVendue) + " " + actif.nom +
            " pour " + FormaterArgent(produitVente) +
            ". Résultat réalisé : " + FormaterMontantSigne(resultat) + ".");
        RafraichirHUD();
    }

    private void MettreAJourMarchePourNouveauMois()
    {
        DonneesBourse bourse = ObtenirDonneesBourse();
        if (bourse == null)
        {
            return;
        }

        bourse.dernierMoisObserve = gameData != null ? gameData.nombreMoisPasses : 0;
        bourse.dernierMessage = ConstruireMessageMarche();
        ActualiserAffichage();
    }

    private void VerifierMoisObserve()
    {
        DonneesBourse bourse = ObtenirDonneesBourse();
        if (bourse == null || gameData == null)
        {
            return;
        }

        if (bourse.dernierMoisObserve != gameData.nombreMoisPasses)
        {
            bourse.dernierMoisObserve = gameData.nombreMoisPasses;
            bourse.dernierMessage = ConstruireMessageMarche();
        }
    }

    private string ConstruireMessageMarche()
    {
        ActifMarche mouvementPrincipal = null;
        float variationPrincipale = 0f;

        foreach (ActifMarche actif in actifs)
        {
            float variation = CalculerVariation(actif, 1);
            if (mouvementPrincipal == null ||
                Mathf.Abs(variation) > Mathf.Abs(variationPrincipale))
            {
                mouvementPrincipal = actif;
                variationPrincipale = variation;
            }
        }

        if (mouvementPrincipal == null)
        {
            return "Le marché attend de nouvelles données.";
        }

        if (Mathf.Abs(variationPrincipale) >= 8f)
        {
            return
                "Forte volatilité sur " + mouvementPrincipal.nom + " : " +
                FormaterPourcentage(variationPrincipale) + " ce mois.";
        }

        if (variationPrincipale >= 0f)
        {
            return
                "Marché en hausse : " + mouvementPrincipal.nom + " progresse de " +
                FormaterPourcentage(variationPrincipale) + ".";
        }

        return
            "Pression vendeuse : " + mouvementPrincipal.nom + " recule de " +
            Mathf.Abs(variationPrincipale).ToString("F1") + " %.";
    }

    private void EcrireJournal(string message)
    {
        DonneesBourse bourse = ObtenirDonneesBourse();
        if (bourse != null)
        {
            bourse.dernierMessage = message;
        }

        ActualiserAffichage();
    }

    private DonneesBourse ObtenirDonneesBourse()
    {
        ResoudreDependances();
        if (gameData == null)
        {
            return null;
        }

        if (gameData.joueur == null)
        {
            gameData.joueur = new DonneesJoueur();
        }

        if (gameData.joueur.bourse == null)
        {
            gameData.joueur.bourse = new DonneesBourse();
        }

        return gameData.joueur.bourse;
    }

    private bool TrouverCompteCourant()
    {
        DonneesBourse bourse = ObtenirDonneesBourse();
        if (bourse == null ||
            gameData.joueur.comptes == null ||
            !gameData.joueur.comptes.TryGetValue(
                "courant",
                out CompteBanquaire nouveauCompte))
        {
            RemplacerCompteCourant(null);
            return false;
        }

        RemplacerCompteCourant(nouveauCompte);
        return compteCourant != null;
    }

    private void ResoudreDependances()
    {
        if (hudManager == null)
        {
            hudManager = FindFirstObjectByType<HUDManager>();
        }

        if (gameData == null && hudManager != null)
        {
            gameData = hudManager.gameData;
        }

        if (gameData == null)
        {
            if (actionPlay == null)
            {
                actionPlay = FindFirstObjectByType<ActionPlay>();
            }

            if (actionPlay != null)
            {
                gameData = actionPlay.gameData;
            }
        }
    }

    private void RemplacerCompteCourant(CompteBanquaire nouveauCompte)
    {
        if (ReferenceEquals(compteCourant, nouveauCompte))
        {
            return;
        }

        DesabonnerSolde();
        compteCourant = nouveauCompte;

        if (isActiveAndEnabled)
        {
            AbonnerSolde();
        }
    }

    private void AbonnerSolde()
    {
        if (!ecouteSoldeActive && compteCourant != null)
        {
            compteCourant.OnSoldeModifie += ActualiserAffichage;
            ecouteSoldeActive = true;
        }
    }

    private void DesabonnerSolde()
    {
        if (ecouteSoldeActive && compteCourant != null)
        {
            compteCourant.OnSoldeModifie -= ActualiserAffichage;
        }

        ecouteSoldeActive = false;
    }

    private void RafraichirHUD()
    {
        if (hudManager != null)
        {
            hudManager.ActualiserAffichage();
        }
    }

    private float PrixActuel(ActifMarche actif)
    {
        return actif != null && actif.prix.Count > 0
            ? actif.prix[IndexPrixActuel(actif)]
            : 0f;
    }

    private int IndexPrixActuel(ActifMarche actif)
    {
        int mois = gameData != null ? gameData.nombreMoisPasses : 0;
        return Mathf.Clamp(mois, 0, Mathf.Max(0, actif.prix.Count - 1));
    }

    private float CalculerVariation(ActifMarche actif, int nombreMois)
    {
        int indexActuel = IndexPrixActuel(actif);
        int indexPasse = Mathf.Max(0, indexActuel - nombreMois);
        float prixPasse = actif.prix[indexPasse];
        return prixPasse > 0f
            ? ((actif.prix[indexActuel] / prixPasse) - 1f) * 100f
            : 0f;
    }

    private float CalculerVolatilite(ActifMarche actif)
    {
        int indexActuel = IndexPrixActuel(actif);
        int debut = Mathf.Max(1, indexActuel - 11);
        int nombre = indexActuel - debut + 1;
        if (nombre <= 1)
        {
            return 0f;
        }

        float moyenne = 0f;
        for (int index = debut; index <= indexActuel; index++)
        {
            moyenne += ((actif.prix[index] / actif.prix[index - 1]) - 1f) * 100f;
        }
        moyenne /= nombre;

        float variance = 0f;
        for (int index = debut; index <= indexActuel; index++)
        {
            float rendement =
                ((actif.prix[index] / actif.prix[index - 1]) - 1f) * 100f;
            variance += (rendement - moyenne) * (rendement - moyenne);
        }

        return Mathf.Sqrt(variance / nombre);
    }

    private static float CalculerRendementAnnualise(ActifMarche actif)
    {
        if (actif == null || actif.prix.Count < 2 || actif.prix[0] <= 0f)
        {
            return 0f;
        }

        float annees = (actif.prix.Count - 1) / 12f;
        return
            (Mathf.Pow(actif.prix[actif.prix.Count - 1] / actif.prix[0], 1f / annees) -
             1f) * 100f;
    }

    private ActifMarche TrouverActif(string actifId)
    {
        if (string.IsNullOrEmpty(actifId))
        {
            return null;
        }

        return actifs.Find(actif => actif.id == actifId);
    }

    private static string NomCategorie(CategorieActif categorie)
    {
        switch (categorie)
        {
            case CategorieActif.Indices:
                return "Indices";
            case CategorieActif.Actions:
                return "Actions";
            case CategorieActif.Crypto:
                return "Crypto";
            case CategorieActif.Energie:
                return "Énergie";
            case CategorieActif.Defensif:
                return "Défensif";
            default:
                return categorie.ToString();
        }
    }

    private static void AffecterTexte(TMP_Text cible, string valeur)
    {
        if (cible != null)
        {
            cible.text = valeur;
        }
    }

    private static string FormaterPrix(float euros)
    {
        return euros.ToString(euros >= 1000f ? "N0" : "N2") + " €";
    }

    private static string FormaterArgent(int centimes)
    {
        return new argent(centimes).ToString();
    }

    private static string FormaterQuantite(float quantite)
    {
        return quantite.ToString(quantite >= 10f ? "N2" : "N5");
    }

    private static string FormaterPourcentage(float pourcentage)
    {
        return (pourcentage >= 0f ? "+" : string.Empty) +
            pourcentage.ToString("F1") + " %";
    }

    private static string FormaterMontantSigne(int centimes)
    {
        return (centimes >= 0 ? "+" : string.Empty) + FormaterArgent(centimes);
    }
}
