import os
import json
import pandas as pd
import numpy as np

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

DOSSIER_SORTIE = os.path.join(BASE_DIR, "livret_a_clean")
os.makedirs(DOSSIER_SORTIE, exist_ok=True)

FICHIER_SORTIE_SIMULATION = os.path.join(
    DOSSIER_SORTIE,
    "livret_a_simulation_40ans_simplifie.json"
)

FICHIER_SORTIE_COMPLET = os.path.join(
    DOSSIER_SORTIE,
    "livret_a_historique_et_simulation_simplifie.json"
)

# Simulation
NB_ANNEES_SIMULATION = 40
NB_MOIS_SIMULATION = NB_ANNEES_SIMULATION * 12
DATE_DEBUT_SIMULATION = "2026-01-01"

# Paramètres du modèle simplifié
SEED = 42

TAUX_MIN = 0.50
TAUX_MAX = 3.00

INFLATION_LONG_TERME = 2.00
INFLATION_VOLATILITE = 0.35
INFLATION_RETOUR_MOYENNE = 0.75

PROBA_CHOC_INFLATION = 0.04
AMPLITUDE_CHOC_INFLATION = 1.10

VARIATION_MAX_PAR_REVISION = 0.50

# Historique réel simplifié du Livret A.
# Date = date d'entrée en vigueur du taux.
# Attention : les taux historiques récents réels comme 2.40 ou 1.70 ne sont pas des multiples de 0.25.
# Je les laisse tels quels dans l'historique réel.
# La contrainte "multiple de 0.25" est appliquée à la simulation future.
CHANGEMENTS_TAUX_HISTORIQUES = [
    ("2014-08-01", 1.00),
    ("2015-08-01", 0.75),
    ("2020-02-01", 0.50),
    ("2022-02-01", 1.00),
    ("2022-08-01", 2.00),
    ("2023-02-01", 3.00),
    ("2025-02-01", 2.40),
    ("2025-08-01", 1.70),
]


def arrondir_au_quart(taux):
    """
    Arrondit strictement au multiple de 0,25 le plus proche.

    Exemples :
    1.41 -> 1.50
    1.37 -> 1.25
    2.12 -> 2.00
    2.13 -> 2.25
    """
    return round(round(taux / 0.25) * 0.25, 2)


def limiter(valeur, minimum, maximum):
    return max(minimum, min(maximum, valeur))


def rendement_mensuel_depuis_taux_annuel(taux_annuel_pct):
    """
    Convertit un taux annuel en rendement mensuel composé.
    Exemple : 3 % annuel -> environ 0,2466 % par mois.
    """
    return ((1 + taux_annuel_pct / 100) ** (1 / 12) - 1) * 100


def generer_historique_mensuel():
    """
    Génère une série mensuelle historique du Livret A
    entre janvier 2015 et décembre 2025.
    """

    df_changements = pd.DataFrame(
        CHANGEMENTS_TAUX_HISTORIQUES,
        columns=["date_changement", "taux_annuel_pct"]
    )

    df_changements["date_changement"] = pd.to_datetime(
        df_changements["date_changement"]
    )

    dates = pd.date_range(start="2015-01-01", end="2025-12-01", freq="MS")
    df_mois = pd.DataFrame({"date": dates})

    df = pd.merge_asof(
        df_mois.sort_values("date"),
        df_changements.sort_values("date_changement"),
        left_on="date",
        right_on="date_changement",
        direction="backward"
    )

    df["periode"] = df["date"].dt.to_period("M").astype(str)
    df["annee"] = df["date"].dt.year
    df["mois"] = df["date"].dt.month

    df["rendement_mensuel_pct"] = df["taux_annuel_pct"].apply(
        rendement_mensuel_depuis_taux_annuel
    )

    df["type"] = "historique_reel"
    df["inflation_simulee_pct"] = None

    return df[
        [
            "date",
            "periode",
            "annee",
            "mois",
            "taux_annuel_pct",
            "rendement_mensuel_pct",
            "inflation_simulee_pct",
            "type"
        ]
    ]


def simuler_inflation(inflation_precedente, rng):
    """
    Simule une inflation annuelle simplifiée.
    Elle revient progressivement vers 2 %, avec parfois un choc.
    """

    inflation = (
        INFLATION_LONG_TERME
        + INFLATION_RETOUR_MOYENNE * (inflation_precedente - INFLATION_LONG_TERME)
        + rng.normal(0, INFLATION_VOLATILITE)
    )

    if rng.random() < PROBA_CHOC_INFLATION:
        inflation += rng.normal(0, AMPLITUDE_CHOC_INFLATION)

    inflation = limiter(inflation, -0.50, 6.00)

    return inflation


def calculer_nouveau_taux(taux_actuel, inflation, rng):
    """
    Modèle simplifié du taux Livret A.

    Idée :
    - le taux dépend de l'inflation ;
    - il ne suit pas toute l'inflation ;
    - l'État lisse les hausses et les baisses ;
    - le taux reste borné entre 0,5 % et 3 % ;
    - le taux final est obligatoirement un multiple de 0,25.
    """

    # Taux théorique simplifié influencé par l'inflation.
    # Si inflation ≈ 2 %, le taux cible tourne autour de 2 %.
    taux_cible = 0.50 + 0.75 * inflation
    taux_cible = limiter(taux_cible, TAUX_MIN, TAUX_MAX)

    # Inertie politique : le taux ne va pas immédiatement vers la cible.
    taux_propose = 0.65 * taux_actuel + 0.35 * taux_cible

    # Petit bruit réglementaire / décisionnel.
    taux_propose += rng.normal(0, 0.12)

    # Limite de variation par révision.
    variation = taux_propose - taux_actuel
    variation = limiter(
        variation,
        -VARIATION_MAX_PAR_REVISION,
        VARIATION_MAX_PAR_REVISION
    )

    taux_nouveau = taux_actuel + variation

    # Sécurité bornes.
    taux_nouveau = limiter(taux_nouveau, TAUX_MIN, TAUX_MAX)

    # Correction demandée :
    # le taux appliqué doit être un multiple strict de 0,25.
    taux_nouveau = arrondir_au_quart(taux_nouveau)

    # Sécurité après arrondi.
    taux_nouveau = limiter(taux_nouveau, TAUX_MIN, TAUX_MAX)

    return taux_nouveau


def simuler_livret_a():
    """
    Simule le Livret A sur 40 ans à partir de janvier 2026.

    Le taux ne change qu'en février et août,
    ce qui correspond à une logique de révision semestrielle.
    """

    rng = np.random.default_rng(SEED)

    dates = pd.date_range(
        start=DATE_DEBUT_SIMULATION,
        periods=NB_MOIS_SIMULATION,
        freq="MS"
    )

    # Dernier taux réel connu dans notre historique simplifié.
    # On force le point de départ simulé au quart de point pour garder la simulation propre.
    taux_actuel = arrondir_au_quart(1.70)

    inflation_actuelle = 2.00

    lignes = []

    for index, date in enumerate(dates, start=1):
        mois = date.month

        # Révision semestrielle du taux.
        if mois in [2, 8]:
            inflation_actuelle = simuler_inflation(inflation_actuelle, rng)
            taux_actuel = calculer_nouveau_taux(
                taux_actuel,
                inflation_actuelle,
                rng
            )
        else:
            # L'inflation continue d'évoluer même si le taux ne change pas.
            inflation_actuelle = simuler_inflation(inflation_actuelle, rng)

            # Sécurité : même hors mois de révision, le taux reste au quart.
            taux_actuel = arrondir_au_quart(taux_actuel)

        rendement_mensuel = rendement_mensuel_depuis_taux_annuel(taux_actuel)

        lignes.append({
            "date": date,
            "periode": date.to_period("M").strftime("%Y-%m"),
            "annee": int(date.year),
            "mois": int(date.month),
            "mois_simule": int(index),
            "taux_annuel_pct": float(arrondir_au_quart(taux_actuel)),
            "rendement_mensuel_pct": round(float(rendement_mensuel), 6),
            "inflation_simulee_pct": round(float(inflation_actuelle), 4),
            "type": "simulation"
        })

    df_simulation = pd.DataFrame(lignes)

    verifier_taux_au_quart(df_simulation)

    return df_simulation


def verifier_taux_au_quart(df_simulation):
    """
    Vérifie que tous les taux simulés sont bien des multiples de 0,25.
    """

    erreurs = []

    for taux in df_simulation["taux_annuel_pct"]:
        multiple = round(taux / 0.25)
        taux_theorique = round(multiple * 0.25, 2)

        if round(taux, 2) != taux_theorique:
            erreurs.append(taux)

    if erreurs:
        raise ValueError(
            "Certains taux simulés ne sont pas des multiples de 0,25 : "
            f"{erreurs[:10]}"
        )

    print("Vérification OK : tous les taux simulés sont des multiples de 0,25.")


def exporter(df_historique, df_simulation):
    df_complet = pd.concat(
        [df_historique, df_simulation],
        ignore_index=True
    )

    df_complet["date"] = pd.to_datetime(df_complet["date"])

    data_simulation = {
        "livret_a_simulation": [
            {
                "Mois": int(row["mois_simule"]),
                "Annee": int(row["annee"]),
                "MoisCalendrier": int(row["mois"]),
                "Periode": str(row["periode"]),
                "Taux_annuel_pct": float(arrondir_au_quart(row["taux_annuel_pct"])),
                "Rendement_mensuel_pct": float(row["rendement_mensuel_pct"]),
                "Inflation_simulee_pct": float(row["inflation_simulee_pct"])
            }
            for _, row in df_simulation.iterrows()
        ]
    }

    with open(FICHIER_SORTIE_SIMULATION, "w", encoding="utf-8") as fichier:
        json.dump(data_simulation, fichier, indent=2, ensure_ascii=False)

    data_complet = {
        "livret_a": []
    }

    for _, row in df_complet.iterrows():
        inflation = row["inflation_simulee_pct"]

        if pd.isna(inflation):
            inflation = None
        else:
            inflation = float(inflation)

        data_complet["livret_a"].append({
            "Periode": str(row["periode"]),
            "Annee": int(row["annee"]),
            "Mois": int(row["mois"]),
            "Taux_annuel_pct": float(row["taux_annuel_pct"]),
            "Rendement_mensuel_pct": float(row["rendement_mensuel_pct"]),
            "Inflation_simulee_pct": inflation,
            "Type": str(row["type"])
        })

    with open(FICHIER_SORTIE_COMPLET, "w", encoding="utf-8") as fichier:
        json.dump(data_complet, fichier, indent=2, ensure_ascii=False)

    chemin_csv = os.path.join(
        DOSSIER_SORTIE,
        "livret_a_historique_et_simulation_simplifie.csv"
    )

    df_complet.to_csv(chemin_csv, index=False, encoding="utf-8-sig")

    print(f"Export simulation JSON : {FICHIER_SORTIE_SIMULATION}")
    print(f"Export complet JSON    : {FICHIER_SORTIE_COMPLET}")
    print(f"Export complet CSV     : {chemin_csv}")


def main():
    print("Simulation simplifiée du Livret A sur 40 ans...\n")

    df_historique = generer_historique_mensuel()
    df_simulation = simuler_livret_a()

    exporter(df_historique, df_simulation)

    print("\nRésumé simulation :")
    print(df_simulation.head())
    print("...")
    print(df_simulation.tail())

    print("\nSimulation Livret A simplifiée terminée !")


if __name__ == "__main__":
    main()