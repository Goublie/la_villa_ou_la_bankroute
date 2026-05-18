import requests
import json
import os
import time

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

VILLES = {
    "paris":       "75",
    "lyon":        "69",
    "marseille":   "13",
    "bordeaux":    "33",
    "toulouse":    "31",
    "nantes":      "44",
}

ANNEES = list(range(2020, 2026))

resultats = {}

for ville, dept in VILLES.items():
    resultats[ville] = []
    for annee in ANNEES:
        url = f"https://valoris-immo.fr/api/v1/prix-median?dept={dept}&annee={annee}&type_bien=appartement"
        response = requests.get(url)

        if response.status_code == 200:
            data = response.json()
            prix = data.get("prix_median_m2")
            resultats[ville].append({"Annee": annee, "Prix_m2": prix})
            print(f"{ville} {annee} → {prix} €/m²")
        else:
            print(f"{ville} {annee} → erreur {response.status_code}")

        time.sleep(0.5)

with open(os.path.join(BASE_DIR, "immo_dvf.json"), "w") as f:
    json.dump(resultats, f, indent=2)

print("\nFichier immo_dvf.json exporté !")