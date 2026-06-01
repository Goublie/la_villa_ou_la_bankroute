import os
import json
import pandas as pd
import matplotlib.pyplot as plt

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

DOSSIER_DONNEES = os.path.join(BASE_DIR, "livret_a_clean")
DOSSIER_GRAPHIQUES = os.path.join(BASE_DIR, "graphiques_livret_a_simplifie")

os.makedirs(DOSSIER_GRAPHIQUES, exist_ok=True)

FICHIER_COMPLET = os.path.join(
    DOSSIER_DONNEES,
    "livret_a_historique_et_simulation_simplifie.json"
)

FICHIER_SIMULATION = os.path.join(
    DOSSIER_DONNEES,
    "livret_a_simulation_40ans_simplifie.json"
)


def charger_complet():
    if not os.path.exists(FICHIER_COMPLET):
        raise FileNotFoundError(f"Fichier introuvable : {FICHIER_COMPLET}")

    with open(FICHIER_COMPLET, "r", encoding="utf-8") as fichier:
        data = json.load(fichier)

    df = pd.DataFrame(data["livret_a"])
    df["date"] = pd.to_datetime(df["Periode"] + "-01")
    df["Taux_annuel_pct"] = df["Taux_annuel_pct"].astype(float)
    df["Rendement_mensuel_pct"] = df["Rendement_mensuel_pct"].astype(float)

    return df


def charger_simulation():
    if not os.path.exists(FICHIER_SIMULATION):
        raise FileNotFoundError(f"Fichier introuvable : {FICHIER_SIMULATION}")

    with open(FICHIER_SIMULATION, "r", encoding="utf-8") as fichier:
        data = json.load(fichier)

    df = pd.DataFrame(data["livret_a_simulation"])
    df["date"] = pd.to_datetime(df["Periode"] + "-01")
    df["Taux_annuel_pct"] = df["Taux_annuel_pct"].astype(float)
    df["Rendement_mensuel_pct"] = df["Rendement_mensuel_pct"].astype(float)
    df["Inflation_simulee_pct"] = df["Inflation_simulee_pct"].astype(float)

    return df


def sauvegarder_graphique(nom_fichier):
    chemin = os.path.join(DOSSIER_GRAPHIQUES, nom_fichier)
    plt.tight_layout()
    plt.savefig(chemin, dpi=150)
    plt.show()
    print(f"Graphique sauvegardé : {chemin}")


def tracer_historique_et_simulation(df):
    df_historique = df[df["Type"] == "historique_reel"]
    df_simulation = df[df["Type"] == "simulation"]

    plt.figure(figsize=(14, 7))

    plt.plot(
        df_historique["date"],
        df_historique["Taux_annuel_pct"],
        linewidth=2.2,
        label="Livret A historique réel"
    )

    plt.plot(
        df_simulation["date"],
        df_simulation["Taux_annuel_pct"],
        linewidth=2.0,
        linestyle="--",
        label="Livret A simulé simplifié"
    )

    plt.title("Livret A — historique réel + simulation simplifiée 40 ans")
    plt.xlabel("Date")
    plt.ylabel("Taux annuel (%)")
    plt.legend()
    plt.grid(True)

    sauvegarder_graphique("01_livret_a_historique_et_simulation.png")


def tracer_simulation_seule(df_simulation):
    plt.figure(figsize=(14, 7))

    plt.plot(
        df_simulation["date"],
        df_simulation["Taux_annuel_pct"],
        linewidth=2
    )

    plt.title("Livret A — simulation simplifiée sur 40 ans")
    plt.xlabel("Date")
    plt.ylabel("Taux annuel (%)")
    plt.grid(True)

    sauvegarder_graphique("02_livret_a_simulation_40ans.png")


def tracer_inflation_et_taux(df_simulation):
    plt.figure(figsize=(14, 7))

    plt.plot(
        df_simulation["date"],
        df_simulation["Inflation_simulee_pct"],
        linewidth=1.8,
        label="Inflation simulée"
    )

    plt.plot(
        df_simulation["date"],
        df_simulation["Taux_annuel_pct"],
        linewidth=2.2,
        linestyle="--",
        label="Livret A simulé"
    )

    plt.title("Livret A simplifié — inflation simulée vs taux appliqué")
    plt.xlabel("Date")
    plt.ylabel("Taux annuel (%)")
    plt.legend()
    plt.grid(True)

    sauvegarder_graphique("03_inflation_simulee_vs_livret_a.png")


def tracer_rendement_mensuel(df_simulation):
    plt.figure(figsize=(14, 7))

    plt.plot(
        df_simulation["date"],
        df_simulation["Rendement_mensuel_pct"],
        linewidth=2
    )

    plt.title("Livret A — rendement mensuel simulé")
    plt.xlabel("Date")
    plt.ylabel("Rendement mensuel (%)")
    plt.grid(True)

    sauvegarder_graphique("04_rendement_mensuel_simule.png")


def exporter_controle(df_simulation):
    resume = (
        df_simulation.groupby("Annee")
        .agg(
            taux_moyen_annuel=("Taux_annuel_pct", "mean"),
            taux_min=("Taux_annuel_pct", "min"),
            taux_max=("Taux_annuel_pct", "max"),
            inflation_moyenne=("Inflation_simulee_pct", "mean")
        )
        .reset_index()
    )

    for colonne in ["taux_moyen_annuel", "taux_min", "taux_max", "inflation_moyenne"]:
        resume[colonne] = resume[colonne].round(4)

    chemin_csv = os.path.join(
        DOSSIER_GRAPHIQUES,
        "resume_livret_a_simulation_annuelle.csv"
    )

    resume.to_csv(chemin_csv, index=False, encoding="utf-8-sig")

    print(f"Résumé annuel sauvegardé : {chemin_csv}")
    print("\nRésumé annuel :")
    print(resume)


def main():
    print("Visualisation du modèle Livret A simplifié...\n")

    df_complet = charger_complet()
    df_simulation = charger_simulation()

    exporter_controle(df_simulation)

    tracer_historique_et_simulation(df_complet)
    tracer_simulation_seule(df_simulation)
    tracer_inflation_et_taux(df_simulation)
    tracer_rendement_mensuel(df_simulation)

    print("\nVisualisation Livret A simplifiée terminée !")
    print(f"Dossier de sortie : {DOSSIER_GRAPHIQUES}")


if __name__ == "__main__":
    main()