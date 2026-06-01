import os
import json
import numpy as np
import pandas as pd

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

# Pour que les simulations soient reproductibles.
# Change cette valeur si tu veux générer une autre économie.
np.random.seed(42)

ANNEES_SIMULATION = 40
MOIS_PAR_AN = 12
MOIS_SIMULATION = ANNEES_SIMULATION * MOIS_PAR_AN

DUREE_MEAN_REVERSION = 10  # en années
DECROISSANCE_VOLATILITE = 0.10

# -----------------------------
# PARAMÈTRES SPÉCIAUX NVIDIA
# -----------------------------

NVIDIA_DEBUT_CORRECTION = 2030.0
NVIDIA_DUREE_CORRECTION_MOIS = 18

# Correction totale simulée sur la période de crise IA.
# -0.55 signifie -55% au total, étalé progressivement sur 18 mois.
NVIDIA_CORRECTION_TOTALE = -0.55

NVIDIA_FIN_CORRECTION = NVIDIA_DEBUT_CORRECTION + NVIDIA_DUREE_CORRECTION_MOIS / 12
NVIDIA_FIN_REPRISE = 2035.0

NVIDIA_CAGR_BOOM_DEBUT = 0.25
NVIDIA_CAGR_BOOM_FIN = 0.12
NVIDIA_CAGR_CRASH = -0.05
NVIDIA_CAGR_REPRISE_DEBUT = 0.12
NVIDIA_CAGR_REPRISE_FIN = 0.06
NVIDIA_CAGR_MATURE = 0.04


# -----------------------------
# PARAMÈTRES SPÉCIAUX GOOGLE
# -----------------------------

# Google est une très grosse entreprise : elle peut croître,
# mais on évite une trajectoire de startup sur 40 ans.
GOOGLE_DUREE_TRANSITION = 8  # années
GOOGLE_CAGR_MAX_DEBUT = 0.10
GOOGLE_CAGR_MATURE = 0.05
GOOGLE_MULTIPLICATEUR_VOL = 0.90


# -----------------------------
# PARAMÈTRES SPÉCIAUX BITCOIN
# -----------------------------

# Bitcoin reste très volatil, mais il ne doit pas devenir un cheat code.
# On limite donc fortement la croissance issue du CAGR historique.
BITCOIN_DUREE_REVERSION_RAPIDE = 3   # années
BITCOIN_DUREE_REVERSION_TOTALE = 10  # années

BITCOIN_CAGR_MAX_DEBUT = 0.12
BITCOIN_CAGR_INTERMEDIAIRE = 0.05
BITCOIN_CAGR_LONG_TERME = 0.025

BITCOIN_MULTIPLICATEUR_VOL = 1.15


ACTIFS = {
    "bourse_cac40": {
        "nom_affichage": "CAC 40",
        "cagr_long_terme": 0.04,
        "decimales": 2,
        "vol_cap": 0.10
    },
    "or": {
        "nom_affichage": "Or",
        "cagr_long_terme": 0.035,
        "decimales": 2,
        "vol_cap": 0.10
    },
    "nvidia": {
        "nom_affichage": "Nvidia",
        "cagr_long_terme": 0.08,
        "decimales": 2,
        "vol_cap": 0.10
    },
    "google": {
        "nom_affichage": "Google",
        # Baisse de 0.07 à 0.05 : Google reste solide,
        # mais sa croissance long terme est moins explosive.
        "cagr_long_terme": 0.05,
        "decimales": 2,
        "vol_cap": 0.08
    },
    "dollar": {
        "nom_affichage": "Dollar USD/EUR",
        "cagr_long_terme": 0.00,
        "decimales": 4,
        "vol_cap": 0.05
    },
    "bitcoin": {
        "nom_affichage": "Bitcoin",
        # Baisse de 0.05 à 0.025 : Bitcoin garde un potentiel,
        # mais n’écrase plus automatiquement tous les autres actifs.
        "cagr_long_terme": 0.025,
        "decimales": 2,
        "vol_cap": 0.12
    },
    "totalenergies": {
        "nom_affichage": "TotalEnergies",
        "cagr_long_terme": 0.04,
        "decimales": 2,
        "vol_cap": 0.10
    }
}


def charger_donnees(nom_fichier: str) -> pd.DataFrame:
    """
    Charge un fichier JSON historique et retourne un DataFrame propre.
    """
    chemin = os.path.join(BASE_DIR, f"{nom_fichier}.json")

    with open(chemin, "r", encoding="utf-8") as fichier:
        data = json.load(fichier)

    df = pd.DataFrame(data)

    if "Date" not in df.columns or "Close" not in df.columns:
        raise ValueError(f"Le fichier {nom_fichier}.json doit contenir Date et Close.")

    df["Date"] = pd.to_datetime(df["Date"])
    df["Close"] = df["Close"].astype(float)

    df = df.dropna()
    df = df.drop_duplicates(subset="Date")
    df = df.sort_values("Date")

    if len(df) < 2:
        raise ValueError(f"Pas assez de données dans {nom_fichier}.json.")

    return df


def calculer_cagr(df: pd.DataFrame) -> float:
    """
    Calcule le CAGR avec la vraie durée calendrier.
    Cette méthode fonctionne pour les actions, les indices, l'or,
    les devises et le Bitcoin.
    """
    prix_depart = df["Close"].iloc[0]
    prix_fin = df["Close"].iloc[-1]

    date_depart = df["Date"].iloc[0]
    date_fin = df["Date"].iloc[-1]

    nb_annees = (date_fin - date_depart).days / 365.25

    if nb_annees <= 0:
        raise ValueError("Durée historique invalide.")

    cagr = (prix_fin / prix_depart) ** (1 / nb_annees) - 1

    return cagr


def calculer_volatilite_mensuelle(df: pd.DataFrame) -> float:
    """
    Calcule la volatilité mensuelle historique réelle.
    On prend le dernier prix disponible de chaque mois,
    puis on calcule les variations mensuelles.
    """
    serie = df.set_index("Date")["Close"]

    prix_mensuels = serie.resample("ME").last().dropna()
    variations_mensuelles = prix_mensuels.pct_change().dropna()

    if len(variations_mensuelles) == 0:
        raise ValueError("Impossible de calculer la volatilité mensuelle.")

    vol_std = variations_mensuelles.std()

    return float(vol_std)


def interpoler(valeur_depart: float, valeur_fin: float, progression: float) -> float:
    """
    Interpolation linéaire entre deux valeurs.
    progression doit être entre 0 et 1.
    """
    progression = max(0.0, min(progression, 1.0))
    return valeur_depart * (1 - progression) + valeur_fin * progression


def appliquer_cycle_bulle_ia_nvidia(
    cagr_effectif: float,
    vol_effective: float,
    annee_simulee: float
) -> tuple[float, float, float]:
    """
    Applique un scénario spécifique à Nvidia.

    L'idée est de reproduire un cycle proche d'une bulle technologique :
    - forte croissance avant 2030 ;
    - correction violente mais étalée ;
    - reprise progressive ;
    - croissance mature ensuite.

    Retourne :
    - cagr_effectif ajusté ;
    - volatilité ajustée ;
    - choc_mensuel supplémentaire appliqué directement au prix.
    """

    choc_mensuel = 0.0

    if annee_simulee < NVIDIA_DEBUT_CORRECTION:
        progression = (annee_simulee - 2025.0) / (NVIDIA_DEBUT_CORRECTION - 2025.0)

        cagr_boom = interpoler(
            NVIDIA_CAGR_BOOM_DEBUT,
            NVIDIA_CAGR_BOOM_FIN,
            progression
        )

        cagr_effectif = min(cagr_effectif, cagr_boom)
        vol_effective *= 1.20

    elif NVIDIA_DEBUT_CORRECTION <= annee_simulee < NVIDIA_FIN_CORRECTION:
        cagr_effectif = NVIDIA_CAGR_CRASH

        choc_mensuel = (
            (1 + NVIDIA_CORRECTION_TOTALE) ** (1 / NVIDIA_DUREE_CORRECTION_MOIS)
            - 1
        )

        vol_effective *= 2.50

    elif NVIDIA_FIN_CORRECTION <= annee_simulee < NVIDIA_FIN_REPRISE:
        progression = (
            (annee_simulee - NVIDIA_FIN_CORRECTION)
            / (NVIDIA_FIN_REPRISE - NVIDIA_FIN_CORRECTION)
        )

        cagr_effectif = interpoler(
            NVIDIA_CAGR_REPRISE_DEBUT,
            NVIDIA_CAGR_REPRISE_FIN,
            progression
        )

        multiplicateur_vol = interpoler(1.80, 1.10, progression)
        vol_effective *= multiplicateur_vol

    else:
        cagr_effectif = NVIDIA_CAGR_MATURE
        vol_effective *= 0.90

    return cagr_effectif, vol_effective, choc_mensuel


def appliquer_maturite_google(
    cagr_effectif: float,
    vol_effective: float,
    t: float
) -> tuple[float, float]:
    """
    Corrige Google pour éviter qu'il se comporte comme une startup
    pendant 40 ans.

    Google reste un actif de croissance, mais sa taille énorme limite
    naturellement son rendement long terme.
    """

    if t < GOOGLE_DUREE_TRANSITION:
        progression = t / GOOGLE_DUREE_TRANSITION

        cagr_max = interpoler(
            GOOGLE_CAGR_MAX_DEBUT,
            GOOGLE_CAGR_MATURE,
            progression
        )

        cagr_effectif = min(cagr_effectif, cagr_max)

    else:
        cagr_effectif = min(cagr_effectif, GOOGLE_CAGR_MATURE)

    vol_effective *= GOOGLE_MULTIPLICATEUR_VOL

    return cagr_effectif, vol_effective


def appliquer_regulation_bitcoin(
    cagr_effectif: float,
    vol_effective: float,
    t: float
) -> tuple[float, float]:
    """
    Corrige Bitcoin pour éviter une croissance démesurée.

    L'idée :
    - Bitcoin reste très volatil ;
    - il peut encore fortement monter au début ;
    - mais son CAGR historique ne doit pas dominer la simulation pendant 10 ans ;
    - à long terme, on le ramène vers une croissance prudente.
    """

    if t < BITCOIN_DUREE_REVERSION_RAPIDE:
        progression = t / BITCOIN_DUREE_REVERSION_RAPIDE

        cagr_max = interpoler(
            BITCOIN_CAGR_MAX_DEBUT,
            BITCOIN_CAGR_INTERMEDIAIRE,
            progression
        )

    elif t < BITCOIN_DUREE_REVERSION_TOTALE:
        progression = (
            (t - BITCOIN_DUREE_REVERSION_RAPIDE)
            / (BITCOIN_DUREE_REVERSION_TOTALE - BITCOIN_DUREE_REVERSION_RAPIDE)
        )

        cagr_max = interpoler(
            BITCOIN_CAGR_INTERMEDIAIRE,
            BITCOIN_CAGR_LONG_TERME,
            progression
        )

    else:
        cagr_max = BITCOIN_CAGR_LONG_TERME

    cagr_effectif = min(cagr_effectif, cagr_max)

    # Bitcoin reste nerveux, même avec un rendement long terme plus prudent.
    vol_effective *= BITCOIN_MULTIPLICATEUR_VOL

    return cagr_effectif, vol_effective


def simuler_actif(nom_fichier: str, config: dict) -> list:
    """
    Génère une simulation mensuelle sur 40 ans.
    Mois 0 = dernier prix réel connu.
    Mois 1 à 480 = prix simulés.
    """
    df = charger_donnees(nom_fichier)

    nom_affichage = config["nom_affichage"]
    cagr_long_terme = config["cagr_long_terme"]
    decimales = config["decimales"]
    vol_cap = config["vol_cap"]

    cagr_historique = calculer_cagr(df)
    vol_std = calculer_volatilite_mensuelle(df)

    prix_actuel = float(df["Close"].iloc[-1])
    date_debut_simulation = df["Date"].iloc[-1]

    simulation = []

    simulation.append({
        "Mois": 0,
        "Close": round(prix_actuel, decimales)
    })

    for mois in range(1, MOIS_SIMULATION + 1):
        date_simulee = date_debut_simulation + pd.DateOffset(months=mois)
        annee_simulee = date_simulee.year + (date_simulee.month - 1) / 12

        t = mois / MOIS_PAR_AN

        poids = min(t / DUREE_MEAN_REVERSION, 1.0)

        cagr_effectif = (
            cagr_historique * (1 - poids)
            + cagr_long_terme * poids
        )

        vol_effective = vol_std * (1 / (1 + t * DECROISSANCE_VOLATILITE))

        choc_mensuel = 0.0

        if nom_fichier == "nvidia":
            cagr_effectif, vol_effective, choc_mensuel = appliquer_cycle_bulle_ia_nvidia(
                cagr_effectif,
                vol_effective,
                annee_simulee
            )

        elif nom_fichier == "google":
            cagr_effectif, vol_effective = appliquer_maturite_google(
                cagr_effectif,
                vol_effective,
                t
            )

        elif nom_fichier == "bitcoin":
            cagr_effectif, vol_effective = appliquer_regulation_bitcoin(
                cagr_effectif,
                vol_effective,
                t
            )

        croissance_mensuelle = (1 + cagr_effectif) ** (1 / MOIS_PAR_AN)

        bruit = np.random.normal(0, vol_effective)
        bruit = np.clip(bruit, -vol_cap, vol_cap)

        prix_actuel = prix_actuel * croissance_mensuelle * (1 + bruit)

        prix_actuel = prix_actuel * (1 + choc_mensuel)

        prix_actuel = max(prix_actuel, 0.0001)

        simulation.append({
            "Mois": mois,
            "Close": round(float(prix_actuel), decimales)
        })

    print(
        f"{nom_affichage:18s} | "
        f"CAGR historique = {cagr_historique * 100:7.2f}% | "
        f"Vol mensuelle = {vol_std * 100:6.2f}% | "
        f"{len(simulation)} points"
    )

    return simulation


def exporter_simulation(nom_fichier: str, simulation: list) -> None:
    """
    Exporte la simulation dans un fichier JSON.
    """
    chemin_export = os.path.join(BASE_DIR, f"{nom_fichier}_simulation.json")

    with open(chemin_export, "w", encoding="utf-8") as fichier:
        json.dump(simulation, fichier, indent=2)

    print(f"Export : {nom_fichier}_simulation.json")


def main():
    print("Début des simulations financières...\n")

    for nom_fichier, config in ACTIFS.items():
        try:
            simulation = simuler_actif(nom_fichier, config)
            exporter_simulation(nom_fichier, simulation)
            print()
        except Exception as erreur:
            print(f"Erreur avec {nom_fichier} : {erreur}\n")

    print("Toutes les simulations sont terminées !")


if __name__ == "__main__":
    main()