import os
import json
import numpy as np

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
ANNEES_SIMULATION = 40
JOURS_PAR_MOIS = 21
MOIS_SIMULATION = ANNEES_SIMULATION * 12

ACTIFS = ["bourse_cac40", "or", "nvidia", "google", "dollar", "bitcoin", "totalenergies"]

for actif in ACTIFS:
    # Chargement
    with open(os.path.join(BASE_DIR, f"{actif}.json")) as f:
        data = json.load(f)

    y = np.array([d["Close"] for d in data])

    # CAGR calculé sur les données réelles
    prix_depart = y[0]
    prix_fin    = y[-1]
    nb_annees   = len(y) / 252  # 252 jours de bourse par an
    cagr        = (prix_fin / prix_depart) ** (1 / nb_annees) - 1
    print(f"{actif:20s} | CAGR = {cagr*100:.2f}% / an")

    CAGR_LONG_TERME = {
        "bourse_cac40":   0.04,
        "or":             0.035,
        "nvidia":         0.08,   
        "google":         0.07,   
        "dollar":        -0.014,
        "bitcoin":        0.05,   
        "totalenergies":  0.04,
    }

    # Volatilité mensuelle historique
    variations = []
    for i in range(0, len(y) - JOURS_PAR_MOIS, JOURS_PAR_MOIS):
        variation = (y[i + JOURS_PAR_MOIS] - y[i]) / y[i]
        variations.append(variation)
    variations = np.array(variations)
    vol_mean = variations.mean()
    vol_std  = variations.std()

    # Génération des 40 ans
    simulation = []
    prix_actuel = y[-1]

    for m in range(MOIS_SIMULATION):
        t = m / 12
        poids = min(t / 10, 1.0)

        if actif == "dollar":
            cagr_effectif = cagr
        else:
            cagr_effectif = cagr * (1 - poids) + CAGR_LONG_TERME[actif] * poids

        croissance_mensuelle = (1 + cagr_effectif) ** (1/12)

        vol_effective = vol_std * (1 / (1 + t * 0.1))
        bruit = np.clip(np.random.normal(0, vol_effective), -0.10, 0.10)

        prix_actuel = prix_actuel * croissance_mensuelle * (1 + bruit)

        for j in range(JOURS_PAR_MOIS):
            simulation.append({
                "Jour": m * JOURS_PAR_MOIS + j,
                "Close": round(float(prix_actuel), 2)
            })

    # Export
    with open(os.path.join(BASE_DIR, f"{actif}_simulation.json"), "w") as f:
        json.dump(simulation, f, indent=2)

    print(f"{actif:20s} | {len(simulation)} jours simulés")

print("\nSimulations terminées !")