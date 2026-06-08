using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

// Classe représentant un compte d'épargne (modélisé selon le Livret A).
// Le taux d'intérêt suit une courbe de prédiction issue de la simulation.
public class Epargne : CompteBanquaire
{
    public Investissement invest;
    private GameData gameData;

    private static List<PrevisionLivretA> predictions;
    private static bool isLoaded = false;

    public Epargne(GameData _gameData, float _tauxParDefaut, int _duree) : base()
    {
        this.gameData = _gameData;
        ChargerPredictions();

        float tauxInitial = _tauxParDefaut;
        if (isLoaded && predictions.Count > 6)
        {
            tauxInitial = predictions[6].Taux_annuel_pct / 100f;
        }

        invest = new Investissement(solde, tauxInitial, _duree);
        EcouterInterets();

        ActionPlay.OnMoisPasse += MettreAJourTaux;
    }

    public Epargne(Historique _historique, GameData _gameData, float _tauxParDefaut, int _duree) : base(_historique)
    {
        this.gameData = _gameData;
        ChargerPredictions();

        float tauxInitial = _tauxParDefaut;
        if (isLoaded && predictions.Count > 6)
        {
            tauxInitial = predictions[6].Taux_annuel_pct / 100f;
        }

        invest = new Investissement(solde, tauxInitial, _duree);
        EcouterInterets();

        ActionPlay.OnMoisPasse += MettreAJourTaux;
    }

    private void EcouterInterets()
    {
        invest.OnBeneficesVerses += (interets) => base.AjoutHistorique("interets", interets);
    }

    public override void AjoutHistorique(string source, argent montant)
    {
        base.AjoutHistorique(source, montant);
        invest.sommeInvestie += montant;
    }

    private void ChargerPredictions()
    {
        if (isLoaded) return;

        try
        {
            TextAsset jsonAsset = Resources.Load<TextAsset>("livret_a_simulation_40ans_simplifie");
            if (jsonAsset == null)
            {
                Debug.LogError("[Économie] Impossible de trouver le fichier 'livret_a_simulation_40ans_simplifie' dans Resources.");
                return;
            }

            // Désérialisation générique directe avec Newtonsoft.Json (pas besoin de classe wrapper intermédiaire)
            var data = JsonConvert.DeserializeObject<Dictionary<string, List<PrevisionLivretA>>>(jsonAsset.text);
            if (data != null && data.ContainsKey("livret_a_simulation"))
            {
                predictions = data["livret_a_simulation"];
                isLoaded = true;
                Debug.Log($"[Économie] Chargement de {predictions.Count} mois de prédictions pour le Livret A.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[Économie] Erreur de décodage des prédictions Newtonsoft : {e.Message}");
        }
    }

    // Met à jour le taux d'intérêt selon le nombre de mois écoulés dans le jeu
    private void MettreAJourTaux()
    {
        if (!isLoaded || gameData == null) return;

        // Début du jeu en juillet 2026 (index 6, correspondant au Mois 7 du fichier JSON)
        int indexPrediction = 6 + gameData.nombreMoisPasses;

        if (indexPrediction >= predictions.Count)
        {
            indexPrediction = predictions.Count - 1;
        }

        if (indexPrediction >= 0 && indexPrediction < predictions.Count)
        {
            PrevisionLivretA entree = predictions[indexPrediction];
            float nouveauTaux = entree.Taux_annuel_pct / 100f;
            invest.taux = nouveauTaux;
            Debug.Log($"[Économie] Taux Livret A mis à jour pour {entree.Periode} -> {entree.Taux_annuel_pct}% (taux = {nouveauTaux})");
        }
    }

    public float GetTaux()
    {
        return invest.taux;
    }
}