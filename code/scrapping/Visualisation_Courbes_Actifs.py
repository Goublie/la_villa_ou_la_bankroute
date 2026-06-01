import os
import json
import pandas as pd
import matplotlib.pyplot as plt

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

DOSSIER_GRAPHIQUES = os.path.join(BASE_DIR, "graphiques")
os.makedirs(DOSSIER_GRAPHIQUES, exist_ok=True)

ACTIFS = {
    "bourse_cac40": "CAC 40",
    "or": "Or",
    "nvidia": "Nvidia",
    "google": "Google",
    "dollar": "Dollar USD/EUR",
    "bitcoin": "Bitcoin",
    "totalenergies": "TotalEnergies"
}


def charger_json(nom_fichier):
    """
    Charge un fichier JSON et le transforme en DataFrame pandas.
    """
    chemin = os.path.join(BASE_DIR, nom_fichier)

    with open(chemin, "r", encoding="utf-8") as fichier:
        data = json.load(fichier)

    return pd.DataFrame(data)


def convertir_historique_mensuel(df):
    """
    Convertit les données historiques journalières en données mensuelles.
    On garde le dernier prix disponible de chaque mois.
    """
    df["Date"] = pd.to_datetime(df["Date"])
    df["Close"] = df["Close"].astype(float)

    df = df.sort_values("Date")
    df = df.set_index("Date")

    # Dernier prix de chaque mois
    df_mensuel = df["Close"].resample("ME").last().dropna().reset_index()

    return df_mensuel


def convertir_simulation_en_dates(df_simulation, derniere_date_historique):
    """
    Convertit la simulation en dates réelles pour pouvoir la tracer
    juste après la courbe historique.

    Fonctionne avec le nouveau format :
    - Mois
    - Close

    Et reste compatible avec l'ancien format :
    - Jour
    - Close
    """
    df_simulation["Close"] = df_simulation["Close"].astype(float)

    if "Mois" in df_simulation.columns:
        df_simulation["Date"] = df_simulation["Mois"].apply(
            lambda mois: derniere_date_historique + pd.DateOffset(months=int(mois))
        )

    elif "Jour" in df_simulation.columns:
        df_simulation["Date"] = df_simulation["Jour"].apply(
            lambda jour: derniere_date_historique + pd.DateOffset(days=int(jour))
        )

    else:
        raise ValueError("La simulation doit contenir une colonne 'Mois' ou 'Jour'.")

    df_simulation = df_simulation.sort_values("Date")

    return df_simulation


def tracer_actif(nom_actif, nom_affichage):
    """
    Trace la courbe historique réelle puis la courbe simulée.
    """
    fichier_historique = f"{nom_actif}.json"
    fichier_simulation = f"{nom_actif}_simulation.json"

    chemin_historique = os.path.join(BASE_DIR, fichier_historique)
    chemin_simulation = os.path.join(BASE_DIR, fichier_simulation)

    if not os.path.exists(chemin_historique):
        print(f"Fichier manquant : {fichier_historique}")
        return

    if not os.path.exists(chemin_simulation):
        print(f"Fichier manquant : {fichier_simulation}")
        return

    df_historique = charger_json(fichier_historique)
    df_simulation = charger_json(fichier_simulation)

    df_historique = convertir_historique_mensuel(df_historique)

    derniere_date_historique = df_historique["Date"].iloc[-1]
    df_simulation = convertir_simulation_en_dates(
        df_simulation,
        derniere_date_historique
    )

    plt.figure(figsize=(12, 6))

    plt.plot(
        df_historique["Date"],
        df_historique["Close"],
        label="Données réelles scrappées"
    )

    plt.plot(
        df_simulation["Date"],
        df_simulation["Close"],
        label="Simulation 40 ans"
    )

    plt.axvline(
        x=derniere_date_historique,
        linestyle="--",
        label="Début simulation"
    )

    plt.title(f"{nom_affichage} — Historique réel + simulation")
    plt.xlabel("Date")
    plt.ylabel("Prix / Valeur")
    plt.legend()
    plt.grid(True)
    plt.tight_layout()

    nom_image = f"{nom_actif}_historique_simulation.png"
    chemin_image = os.path.join(DOSSIER_GRAPHIQUES, nom_image)

    plt.savefig(chemin_image, dpi=150)
    plt.show()

    print(f"Graphique sauvegardé : {chemin_image}")


def tracer_comparaison_normalisee():
    """
    Trace toutes les simulations sur un même graphique en base 100.
    C'est utile pour comparer les dynamiques entre actifs.
    """
    plt.figure(figsize=(12, 6))

    for nom_actif, nom_affichage in ACTIFS.items():
        fichier_simulation = f"{nom_actif}_simulation.json"
        chemin_simulation = os.path.join(BASE_DIR, fichier_simulation)

        if not os.path.exists(chemin_simulation):
            print(f"Fichier manquant : {fichier_simulation}")
            continue

        df_simulation = charger_json(fichier_simulation)

        if "Mois" not in df_simulation.columns:
            print(f"{fichier_simulation} n'utilise pas le format mensuel.")
            continue

        df_simulation["Close"] = df_simulation["Close"].astype(float)

        prix_initial = df_simulation["Close"].iloc[0]
        df_simulation["Base100"] = df_simulation["Close"] / prix_initial * 100

        plt.plot(
            df_simulation["Mois"],
            df_simulation["Base100"],
            label=nom_affichage
        )

    plt.title("Comparaison des simulations — base 100")
    plt.xlabel("Mois simulé")
    plt.ylabel("Indice base 100")
    plt.legend()
    plt.grid(True)
    plt.tight_layout()

    chemin_image = os.path.join(DOSSIER_GRAPHIQUES, "comparaison_simulations_base100.png")

    plt.savefig(chemin_image, dpi=150)
    plt.show()

    print(f"Graphique sauvegardé : {chemin_image}")


def main():
    print("Génération des graphiques...\n")

    for nom_actif, nom_affichage in ACTIFS.items():
        print(f"Graphique : {nom_affichage}")
        tracer_actif(nom_actif, nom_affichage)
        print()

    print("Graphique comparatif normalisé...")
    tracer_comparaison_normalisee()

    print("\nTous les graphiques sont terminés !")


if __name__ == "__main__":
    main()