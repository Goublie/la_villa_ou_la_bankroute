import os
import json
import math
import pandas as pd
import numpy as np

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

DOSSIER_IMMO = os.path.join(BASE_DIR, "immo_villes_clean")

FICHIER_ENTREE = os.path.join(
    DOSSIER_IMMO,
    "immo_villes_mensuel_format_jeu.json"
)

FICHIER_SORTIE_SIMULATION = os.path.join(
    DOSSIER_IMMO,
    "immo_villes_simulation_40ans.json"
)

FICHIER_SORTIE_COMPLET = os.path.join(
    DOSSIER_IMMO,
    "immo_villes_historique_et_simulation_40ans.json"
)

FICHIER_SORTIE_PARAMETRES = os.path.join(
    DOSSIER_IMMO,
    "immo_villes_parametres_simulation.json"
)

# Simulation
NB_ANNEES_SIMULATION = 40
NB_MOIS_SIMULATION = NB_ANNEES_SIMULATION * 12
SEED = 42

# Le modèle revient progressivement vers le CAGR long terme.
# 10 ans = transition lente, adaptée à l'immobilier.
DUREE_RETOUR_LONG_TERME_ANNEES = 10
DUREE_RETOUR_LONG_TERME_MOIS = DUREE_RETOUR_LONG_TERME_ANNEES * 12

# On réduit volontairement la volatilité observée,
# car les prix mensuels DVF peuvent contenir du bruit de composition.
FACTEUR_VOLATILITE_HISTORIQUE = 0.55

# Configuration économique par ville.
# CAGR long terme nominal = inflation long terme + petite croissance réelle.
# Exemple : 0.020 signifie 2,0 % par an.
PARAMETRES_VILLES = {
    "paris": {
        "nom_affichage": "Paris",
        "cagr_long_terme": 0.020,
        "volatilite_min_mensuelle": 0.0025,
        "volatilite_max_mensuelle": 0.0100,
        "variation_max_mensuelle": 0.018,
        "amplitude_cycle_annuel": 0.004
    },
    "lyon": {
        "nom_affichage": "Lyon",
        "cagr_long_terme": 0.022,
        "volatilite_min_mensuelle": 0.0030,
        "volatilite_max_mensuelle": 0.0120,
        "variation_max_mensuelle": 0.020,
        "amplitude_cycle_annuel": 0.005
    },
    "marseille": {
        "nom_affichage": "Marseille",
        "cagr_long_terme": 0.026,
        "volatilite_min_mensuelle": 0.0035,
        "volatilite_max_mensuelle": 0.0140,
        "variation_max_mensuelle": 0.024,
        "amplitude_cycle_annuel": 0.006
    },
    "bordeaux": {
        "nom_affichage": "Bordeaux",
        "cagr_long_terme": 0.021,
        "volatilite_min_mensuelle": 0.0035,
        "volatilite_max_mensuelle": 0.0130,
        "variation_max_mensuelle": 0.022,
        "amplitude_cycle_annuel": 0.006
    },
    "toulouse": {
        "nom_affichage": "Toulouse",
        "cagr_long_terme": 0.023,
        "volatilite_min_mensuelle": 0.0030,
        "volatilite_max_mensuelle": 0.0120,
        "variation_max_mensuelle": 0.020,
        "amplitude_cycle_annuel": 0.005
    },
    "nantes": {
        "nom_affichage": "Nantes",
        "cagr_long_terme": 0.022,
        "volatilite_min_mensuelle": 0.0035,
        "volatilite_max_mensuelle": 0.0130,
        "variation_max_mensuelle": 0.022,
        "amplitude_cycle_annuel": 0.006
    }
}


def limiter(valeur, minimum, maximum):
    return max(minimum, min(maximum, valeur))


def charger_donnees_immo():
    """
    Charge le fichier immobilier mensuel global.
    Format attendu :
    {
      "paris": [
        {
          "Annee": 2015,
          "Mois": 1,
          "Periode": "2015-01",
          "Prix_m2": 7954.55,
          "Nb_transactions": 1850
        }
      ],
      ...
    }
    """

    if not os.path.exists(FICHIER_ENTREE):
        raise FileNotFoundError(f"Fichier introuvable : {FICHIER_ENTREE}")

    with open(FICHIER_ENTREE, "r", encoding="utf-8") as fichier:
        data = json.load(fichier)

    donnees = {}

    for ville, valeurs in data.items():
        lignes = []

        for ligne in valeurs:
            lignes.append({
                "ville": ville,
                "annee": int(ligne["Annee"]),
                "mois": int(ligne["Mois"]),
                "periode": str(ligne["Periode"]),
                "prix_m2": float(ligne["Prix_m2"]),
                "nb_transactions": int(ligne["Nb_transactions"])
            })

        df = pd.DataFrame(lignes)
        df["date"] = pd.to_datetime(df["periode"] + "-01")
        df = df.sort_values("date").reset_index(drop=True)

        donnees[ville] = df

    return donnees


def calculer_cagr(prix_depart, prix_fin, nb_mois):
    """
    Calcule le CAGR annualisé à partir de deux prix et d'une durée en mois.
    """
    nb_annees = nb_mois / 12

    if prix_depart <= 0 or prix_fin <= 0 or nb_annees <= 0:
        return 0.0

    return (prix_fin / prix_depart) ** (1 / nb_annees) - 1


def calculer_parametres_historiques(df, ville):
    """
    Calcule :
    - CAGR historique ;
    - volatilité mensuelle historique ;
    - dernier prix réel ;
    - dernière date réelle.
    """

    prix_depart = df["prix_m2"].iloc[0]
    prix_fin = df["prix_m2"].iloc[-1]

    nb_mois = len(df) - 1

    cagr_historique = calculer_cagr(
        prix_depart=prix_depart,
        prix_fin=prix_fin,
        nb_mois=nb_mois
    )

    df_temp = df.copy()
    df_temp["rendement_mensuel"] = df_temp["prix_m2"].pct_change()

    volatilite_historique_mensuelle = df_temp["rendement_mensuel"].std()

    if pd.isna(volatilite_historique_mensuelle):
        volatilite_historique_mensuelle = 0.005

    config = PARAMETRES_VILLES[ville]

    volatilite_utilisee = volatilite_historique_mensuelle * FACTEUR_VOLATILITE_HISTORIQUE

    volatilite_utilisee = limiter(
        volatilite_utilisee,
        config["volatilite_min_mensuelle"],
        config["volatilite_max_mensuelle"]
    )

    return {
        "prix_depart": float(prix_depart),
        "prix_fin": float(prix_fin),
        "date_depart": str(df["periode"].iloc[0]),
        "date_fin": str(df["periode"].iloc[-1]),
        "nb_mois_historique": int(nb_mois),
        "cagr_historique": float(cagr_historique),
        "volatilite_historique_mensuelle": float(volatilite_historique_mensuelle),
        "volatilite_utilisee_mensuelle": float(volatilite_utilisee)
    }


def convertir_cagr_annuel_en_rendement_mensuel(cagr_annuel):
    """
    Convertit un CAGR annuel en rendement mensuel composé.
    Exemple : 2,4 % annuel -> environ 0,1978 % mensuel.
    """
    return (1 + cagr_annuel) ** (1 / 12) - 1


def calculer_cagr_effectif(cagr_historique, cagr_long_terme, mois_simule):
    """
    Au début, le modèle utilise encore fortement le CAGR historique.
    Puis il revient progressivement vers le CAGR long terme.
    """

    poids_long_terme = min(
        mois_simule / DUREE_RETOUR_LONG_TERME_MOIS,
        1.0
    )

    cagr_effectif = (
        cagr_historique * (1 - poids_long_terme)
        + cagr_long_terme * poids_long_terme
    )

    return cagr_effectif


def calculer_effet_cycle(mois_simule, amplitude_annuelle, phase):
    """
    Ajoute un cycle immobilier lent.
    Ce n'est pas un événement brutal, juste un cycle long de marché.

    Le cycle est exprimé comme une petite contribution mensuelle.
    Période : environ 12 ans.
    """

    periode_cycle_mois = 12 * 12

    cycle_annuel = amplitude_annuelle * math.sin(
        2 * math.pi * mois_simule / periode_cycle_mois + phase
    )

    cycle_mensuel = cycle_annuel / 12

    return cycle_mensuel


def simuler_ville(ville, df_historique, rng):
    """
    Simule les prix immobiliers d'une ville sur 40 ans.
    """

    if ville not in PARAMETRES_VILLES:
        raise ValueError(f"Ville non configurée : {ville}")

    config = PARAMETRES_VILLES[ville]
    params_hist = calculer_parametres_historiques(df_historique, ville)

    cagr_historique = params_hist["cagr_historique"]
    cagr_long_terme = config["cagr_long_terme"]
    volatilite_mensuelle = params_hist["volatilite_utilisee_mensuelle"]

    prix_actuel = params_hist["prix_fin"]
    derniere_date = df_historique["date"].iloc[-1]

    phase = rng.uniform(0, 2 * math.pi)

    lignes_simulation = []

    for mois_simule in range(1, NB_MOIS_SIMULATION + 1):
        date_future = derniere_date + pd.DateOffset(months=mois_simule)

        cagr_effectif = calculer_cagr_effectif(
            cagr_historique=cagr_historique,
            cagr_long_terme=cagr_long_terme,
            mois_simule=mois_simule
        )

        rendement_mensuel_cagr = convertir_cagr_annuel_en_rendement_mensuel(
            cagr_effectif
        )

        bruit = rng.normal(0, volatilite_mensuelle)

        cycle = calculer_effet_cycle(
            mois_simule=mois_simule,
            amplitude_annuelle=config["amplitude_cycle_annuel"],
            phase=phase
        )

        variation_mensuelle = rendement_mensuel_cagr + bruit + cycle

        variation_mensuelle = limiter(
            variation_mensuelle,
            -config["variation_max_mensuelle"],
            config["variation_max_mensuelle"]
        )

        prix_actuel = prix_actuel * (1 + variation_mensuelle)

        # Sécurité : le prix ne doit jamais devenir absurde.
        prix_actuel = max(prix_actuel, 500)

        lignes_simulation.append({
            "ville": ville,
            "annee": int(date_future.year),
            "mois": int(date_future.month),
            "periode": date_future.to_period("M").strftime("%Y-%m"),
            "mois_simule": int(mois_simule),
            "prix_m2": round(float(prix_actuel), 2),
            "variation_mensuelle_pct": round(float(variation_mensuelle * 100), 4),
            "cagr_effectif_annuel_pct": round(float(cagr_effectif * 100), 4),
            "type": "simulation"
        })

    df_simulation = pd.DataFrame(lignes_simulation)

    return df_simulation, params_hist


def preparer_historique_pour_export(ville, df_historique):
    """
    Convertit l'historique réel au même format que la simulation.
    """

    df = df_historique.copy()

    df["mois_simule"] = 0
    df["variation_mensuelle_pct"] = df["prix_m2"].pct_change() * 100
    df["cagr_effectif_annuel_pct"] = None
    df["type"] = "historique_reel"

    df["variation_mensuelle_pct"] = df["variation_mensuelle_pct"].round(4)

    return df[
        [
            "ville",
            "annee",
            "mois",
            "periode",
            "mois_simule",
            "prix_m2",
            "variation_mensuelle_pct",
            "cagr_effectif_annuel_pct",
            "type"
        ]
    ]


def exporter_resultats(resultats_simulation, resultats_complets, parametres):
    """
    Exporte :
    - simulation seule pour le jeu ;
    - historique + simulation pour vérification ;
    - paramètres calculés.
    """

    data_simulation = {}

    for ville, df_sim in resultats_simulation.items():
        data_simulation[ville] = [
            {
                "Mois": int(row["mois_simule"]),
                "Annee": int(row["annee"]),
                "MoisCalendrier": int(row["mois"]),
                "Periode": str(row["periode"]),
                "Prix_m2": float(row["prix_m2"])
            }
            for _, row in df_sim.iterrows()
        ]

    with open(FICHIER_SORTIE_SIMULATION, "w", encoding="utf-8") as fichier:
        json.dump(data_simulation, fichier, indent=2, ensure_ascii=False)

    data_complet = {}

    for ville, df_full in resultats_complets.items():
        data_complet[ville] = [
            {
                "Annee": int(row["annee"]),
                "Mois": int(row["mois"]),
                "Periode": str(row["periode"]),
                "Mois_simule": int(row["mois_simule"]),
                "Prix_m2": float(row["prix_m2"]),
                "Variation_mensuelle_pct": None
                if pd.isna(row["variation_mensuelle_pct"])
                else float(row["variation_mensuelle_pct"]),
                "CAGR_effectif_annuel_pct": None
                if pd.isna(row["cagr_effectif_annuel_pct"])
                else float(row["cagr_effectif_annuel_pct"]),
                "Type": str(row["type"])
            }
            for _, row in df_full.iterrows()
        ]

    with open(FICHIER_SORTIE_COMPLET, "w", encoding="utf-8") as fichier:
        json.dump(data_complet, fichier, indent=2, ensure_ascii=False)

    with open(FICHIER_SORTIE_PARAMETRES, "w", encoding="utf-8") as fichier:
        json.dump(parametres, fichier, indent=2, ensure_ascii=False)

    print(f"Export simulation jeu : {FICHIER_SORTIE_SIMULATION}")
    print(f"Export complet        : {FICHIER_SORTIE_COMPLET}")
    print(f"Export paramètres     : {FICHIER_SORTIE_PARAMETRES}")


def main():
    print("Simulation immobilière CAGR sur 40 ans...\n")

    donnees = charger_donnees_immo()

    rng = np.random.default_rng(SEED)

    resultats_simulation = {}
    resultats_complets = {}
    parametres = {}

    for ville, df_historique in donnees.items():
        print("=" * 70)
        print(f"Ville : {ville}")
        print("=" * 70)

        df_simulation, params_hist = simuler_ville(ville, df_historique, rng)

        df_historique_export = preparer_historique_pour_export(
            ville,
            df_historique
        )

        df_complet = pd.concat(
            [df_historique_export, df_simulation],
            ignore_index=True
        )

        resultats_simulation[ville] = df_simulation
        resultats_complets[ville] = df_complet

        config = PARAMETRES_VILLES[ville]

        parametres[ville] = {
            "nom_affichage": config["nom_affichage"],
            "cagr_historique_pct": round(params_hist["cagr_historique"] * 100, 4),
            "cagr_long_terme_pct": round(config["cagr_long_terme"] * 100, 4),
            "volatilite_historique_mensuelle_pct": round(
                params_hist["volatilite_historique_mensuelle"] * 100,
                4
            ),
            "volatilite_utilisee_mensuelle_pct": round(
                params_hist["volatilite_utilisee_mensuelle"] * 100,
                4
            ),
            "variation_max_mensuelle_pct": round(
                config["variation_max_mensuelle"] * 100,
                4
            ),
            "prix_depart": round(params_hist["prix_depart"], 2),
            "prix_fin_historique": round(params_hist["prix_fin"], 2),
            "prix_fin_simulation": round(df_simulation["prix_m2"].iloc[-1], 2),
            "periode_depart": params_hist["date_depart"],
            "periode_fin_historique": params_hist["date_fin"],
            "periode_fin_simulation": str(df_simulation["periode"].iloc[-1])
        }

        print(f"CAGR historique : {parametres[ville]['cagr_historique_pct']} %")
        print(f"CAGR long terme : {parametres[ville]['cagr_long_terme_pct']} %")
        print(
            "Volatilité utilisée mensuelle : "
            f"{parametres[ville]['volatilite_utilisee_mensuelle_pct']} %"
        )
        print(f"Prix fin historique : {parametres[ville]['prix_fin_historique']} €/m²")
        print(f"Prix fin simulation : {parametres[ville]['prix_fin_simulation']} €/m²")
        print()

    exporter_resultats(
        resultats_simulation=resultats_simulation,
        resultats_complets=resultats_complets,
        parametres=parametres
    )

    print("\nSimulation immobilière terminée !")


if __name__ == "__main__":
    main()