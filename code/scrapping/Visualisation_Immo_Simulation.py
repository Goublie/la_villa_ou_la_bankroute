import os
import json
import pandas as pd
import matplotlib.pyplot as plt

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

DOSSIER_IMMO = os.path.join(BASE_DIR, "immo_villes_clean")
DOSSIER_GRAPHIQUES = os.path.join(BASE_DIR, "graphiques_immo_simulation")

os.makedirs(DOSSIER_GRAPHIQUES, exist_ok=True)

FICHIER_COMPLET = os.path.join(
    DOSSIER_IMMO,
    "immo_villes_historique_et_simulation_40ans.json"
)

FICHIER_SIMULATION = os.path.join(
    DOSSIER_IMMO,
    "immo_villes_simulation_40ans.json"
)

FICHIER_PARAMETRES = os.path.join(
    DOSSIER_IMMO,
    "immo_villes_parametres_simulation.json"
)

NOMS_AFFICHAGE = {
    "paris": "Paris",
    "lyon": "Lyon",
    "marseille": "Marseille",
    "bordeaux": "Bordeaux",
    "toulouse": "Toulouse",
    "nantes": "Nantes"
}


def charger_json(chemin):
    if not os.path.exists(chemin):
        raise FileNotFoundError(f"Fichier introuvable : {chemin}")

    with open(chemin, "r", encoding="utf-8") as fichier:
        return json.load(fichier)


def charger_donnees_completes():
    """
    Charge historique réel + simulation.
    """

    data = charger_json(FICHIER_COMPLET)

    lignes = []

    for ville, valeurs in data.items():
        for ligne in valeurs:
            lignes.append({
                "ville": ville,
                "ville_affichage": NOMS_AFFICHAGE.get(ville, ville.title()),
                "annee": int(ligne["Annee"]),
                "mois": int(ligne["Mois"]),
                "periode": str(ligne["Periode"]),
                "mois_simule": int(ligne["Mois_simule"]),
                "prix_m2": float(ligne["Prix_m2"]),
                "variation_mensuelle_pct": ligne["Variation_mensuelle_pct"],
                "cagr_effectif_annuel_pct": ligne["CAGR_effectif_annuel_pct"],
                "type": str(ligne["Type"])
            })

    df = pd.DataFrame(lignes)

    df["date"] = pd.to_datetime(df["periode"] + "-01")
    df["variation_mensuelle_pct"] = pd.to_numeric(
        df["variation_mensuelle_pct"],
        errors="coerce"
    )
    df["cagr_effectif_annuel_pct"] = pd.to_numeric(
        df["cagr_effectif_annuel_pct"],
        errors="coerce"
    )

    df = df.sort_values(["ville", "date"]).reset_index(drop=True)

    return df


def charger_parametres():
    if not os.path.exists(FICHIER_PARAMETRES):
        print("Fichier paramètres absent, certains graphiques seront ignorés.")
        return {}

    return charger_json(FICHIER_PARAMETRES)


def sauvegarder_graphique(nom_fichier):
    chemin = os.path.join(DOSSIER_GRAPHIQUES, nom_fichier)
    plt.tight_layout()
    plt.savefig(chemin, dpi=150)
    plt.show()
    print(f"Graphique sauvegardé : {chemin}")


def date_debut_simulation(df):
    simulation = df[df["type"] == "simulation"]

    if simulation.empty:
        return None

    return simulation["date"].min()


def tracer_historique_et_simulation_toutes_villes(df):
    """
    Prix au m² historique + simulation pour toutes les villes.
    """

    plt.figure(figsize=(15, 8))

    for ville in sorted(df["ville"].unique()):
        df_ville = df[df["ville"] == ville]

        plt.plot(
            df_ville["date"],
            df_ville["prix_m2"],
            linewidth=1.7,
            label=NOMS_AFFICHAGE.get(ville, ville.title())
        )

    debut_simulation = date_debut_simulation(df)

    if debut_simulation is not None:
        plt.axvline(
            debut_simulation,
            linestyle="--",
            linewidth=1.5,
            label="Début simulation"
        )

    plt.title("Immobilier — historique réel + simulation 40 ans")
    plt.xlabel("Date")
    plt.ylabel("Prix au m² (€)")
    plt.legend()
    plt.grid(True)

    sauvegarder_graphique("01_historique_et_simulation_toutes_villes.png")


def tracer_base_100_complet(df):
    """
    Base 100 depuis janvier 2015.
    Permet de comparer les progressions relatives.
    """

    plt.figure(figsize=(15, 8))

    for ville in sorted(df["ville"].unique()):
        df_ville = df[df["ville"] == ville].copy()

        prix_depart = df_ville["prix_m2"].iloc[0]
        df_ville["base_100"] = df_ville["prix_m2"] / prix_depart * 100

        plt.plot(
            df_ville["date"],
            df_ville["base_100"],
            linewidth=1.7,
            label=NOMS_AFFICHAGE.get(ville, ville.title())
        )

    debut_simulation = date_debut_simulation(df)

    if debut_simulation is not None:
        plt.axvline(
            debut_simulation,
            linestyle="--",
            linewidth=1.5,
            label="Début simulation"
        )

    plt.title("Immobilier — historique + simulation en base 100")
    plt.xlabel("Date")
    plt.ylabel("Indice base 100")
    plt.legend()
    plt.grid(True)

    sauvegarder_graphique("02_base_100_historique_et_simulation.png")


def tracer_base_100_simulation_seule(df):
    """
    Base 100 au début de la simulation.
    Permet de comparer les villes uniquement sur les 40 ans simulés.
    """

    df_sim = df[df["type"] == "simulation"].copy()

    plt.figure(figsize=(15, 8))

    for ville in sorted(df_sim["ville"].unique()):
        df_ville = df_sim[df_sim["ville"] == ville].copy()

        prix_depart = df_ville["prix_m2"].iloc[0]
        df_ville["base_100_simulation"] = df_ville["prix_m2"] / prix_depart * 100

        plt.plot(
            df_ville["date"],
            df_ville["base_100_simulation"],
            linewidth=1.8,
            label=NOMS_AFFICHAGE.get(ville, ville.title())
        )

    plt.title("Immobilier — simulation 40 ans en base 100")
    plt.xlabel("Date")
    plt.ylabel("Indice base 100 au début de simulation")
    plt.legend()
    plt.grid(True)

    sauvegarder_graphique("03_base_100_simulation_seule.png")


def tracer_prix_final_simulation(df):
    """
    Classement des villes selon le prix au m² final simulé.
    """

    df_sim = df[df["type"] == "simulation"].copy()

    dernieres_lignes = (
        df_sim.sort_values("date")
        .groupby(["ville", "ville_affichage"])
        .tail(1)
        .sort_values("prix_m2", ascending=True)
    )

    plt.figure(figsize=(10, 6))

    plt.barh(
        dernieres_lignes["ville_affichage"],
        dernieres_lignes["prix_m2"]
    )

    plt.title("Prix au m² final après 40 ans de simulation")
    plt.xlabel("Prix au m² (€)")
    plt.ylabel("Ville")
    plt.grid(True, axis="x")

    for index, valeur in enumerate(dernieres_lignes["prix_m2"]):
        plt.text(
            valeur,
            index,
            f" {valeur:,.0f} €".replace(",", " "),
            va="center"
        )

    sauvegarder_graphique("04_classement_prix_final_simulation.png")


def tracer_progression_simulation(df):
    """
    Classement des villes selon leur progression pendant la simulation.
    """

    df_sim = df[df["type"] == "simulation"].copy()

    lignes = []

    for ville in sorted(df_sim["ville"].unique()):
        df_ville = df_sim[df_sim["ville"] == ville].sort_values("date")

        prix_depart = df_ville["prix_m2"].iloc[0]
        prix_fin = df_ville["prix_m2"].iloc[-1]

        progression = (prix_fin / prix_depart - 1) * 100

        lignes.append({
            "ville": ville,
            "ville_affichage": NOMS_AFFICHAGE.get(ville, ville.title()),
            "prix_depart": prix_depart,
            "prix_fin": prix_fin,
            "progression_pct": progression
        })

    classement = pd.DataFrame(lignes).sort_values("progression_pct", ascending=True)

    plt.figure(figsize=(10, 6))

    plt.barh(
        classement["ville_affichage"],
        classement["progression_pct"]
    )

    plt.title("Progression des prix au m² pendant les 40 ans simulés")
    plt.xlabel("Progression (%)")
    plt.ylabel("Ville")
    plt.grid(True, axis="x")

    for index, valeur in enumerate(classement["progression_pct"]):
        plt.text(
            valeur,
            index,
            f" {valeur:.1f} %",
            va="center"
        )

    chemin_csv = os.path.join(
        DOSSIER_GRAPHIQUES,
        "classement_progression_simulation.csv"
    )

    classement.to_csv(chemin_csv, index=False, encoding="utf-8-sig")
    print(f"CSV sauvegardé : {chemin_csv}")

    sauvegarder_graphique("05_classement_progression_simulation.png")


def tracer_cagr_effectif(df):
    """
    Vérifie le retour progressif vers le CAGR long terme.
    """

    df_sim = df[df["type"] == "simulation"].copy()

    plt.figure(figsize=(15, 8))

    for ville in sorted(df_sim["ville"].unique()):
        df_ville = df_sim[df_sim["ville"] == ville]

        plt.plot(
            df_ville["date"],
            df_ville["cagr_effectif_annuel_pct"],
            linewidth=1.8,
            label=NOMS_AFFICHAGE.get(ville, ville.title())
        )

    plt.title("CAGR effectif annuel utilisé pendant la simulation")
    plt.xlabel("Date")
    plt.ylabel("CAGR effectif annuel (%)")
    plt.legend()
    plt.grid(True)

    sauvegarder_graphique("06_cagr_effectif_simulation.png")


def tracer_variations_mensuelles(df):
    """
    Vérifie que les variations mensuelles restent raisonnables.
    """

    df_sim = df[df["type"] == "simulation"].copy()

    plt.figure(figsize=(15, 8))

    for ville in sorted(df_sim["ville"].unique()):
        df_ville = df_sim[df_sim["ville"] == ville]

        plt.plot(
            df_ville["date"],
            df_ville["variation_mensuelle_pct"],
            linewidth=1.2,
            label=NOMS_AFFICHAGE.get(ville, ville.title())
        )

    plt.axhline(0, linestyle="--", linewidth=1)

    plt.title("Variations mensuelles simulées des prix immobiliers")
    plt.xlabel("Date")
    plt.ylabel("Variation mensuelle (%)")
    plt.legend()
    plt.grid(True)

    sauvegarder_graphique("07_variations_mensuelles_simulees.png")


def tracer_par_ville(df):
    """
    Un graphique séparé par ville :
    historique réel + simulation.
    """

    for ville in sorted(df["ville"].unique()):
        df_ville = df[df["ville"] == ville].copy()
        nom_ville = NOMS_AFFICHAGE.get(ville, ville.title())

        df_hist = df_ville[df_ville["type"] == "historique_reel"]
        df_sim = df_ville[df_ville["type"] == "simulation"]

        plt.figure(figsize=(13, 6))

        plt.plot(
            df_hist["date"],
            df_hist["prix_m2"],
            linewidth=2.2,
            label="Historique réel"
        )

        plt.plot(
            df_sim["date"],
            df_sim["prix_m2"],
            linewidth=2.0,
            linestyle="--",
            label="Simulation"
        )

        debut_simulation = date_debut_simulation(df_ville)

        if debut_simulation is not None:
            plt.axvline(
                debut_simulation,
                linestyle=":",
                linewidth=1.5,
                label="Début simulation"
            )

        plt.title(f"{nom_ville} — prix au m² historique + simulation")
        plt.xlabel("Date")
        plt.ylabel("Prix au m² (€)")
        plt.legend()
        plt.grid(True)

        sauvegarder_graphique(f"08_{ville}_historique_et_simulation.png")


def tracer_resume_parametres(parametres):
    """
    Graphique des CAGR historiques vs CAGR long terme.
    """

    if not parametres:
        return

    lignes = []

    for ville, valeurs in parametres.items():
        lignes.append({
            "ville": ville,
            "ville_affichage": NOMS_AFFICHAGE.get(ville, ville.title()),
            "cagr_historique_pct": float(valeurs["cagr_historique_pct"]),
            "cagr_long_terme_pct": float(valeurs["cagr_long_terme_pct"]),
            "prix_fin_historique": float(valeurs["prix_fin_historique"]),
            "prix_fin_simulation": float(valeurs["prix_fin_simulation"])
        })

    df_params = pd.DataFrame(lignes).sort_values("ville_affichage")

    x = range(len(df_params))

    plt.figure(figsize=(11, 6))

    largeur = 0.35

    plt.bar(
        [i - largeur / 2 for i in x],
        df_params["cagr_historique_pct"],
        width=largeur,
        label="CAGR historique"
    )

    plt.bar(
        [i + largeur / 2 for i in x],
        df_params["cagr_long_terme_pct"],
        width=largeur,
        label="CAGR long terme"
    )

    plt.xticks(list(x), df_params["ville_affichage"])
    plt.title("CAGR historique vs CAGR long terme utilisé")
    plt.xlabel("Ville")
    plt.ylabel("CAGR annuel (%)")
    plt.legend()
    plt.grid(True, axis="y")

    chemin_csv = os.path.join(
        DOSSIER_GRAPHIQUES,
        "resume_parametres_simulation.csv"
    )

    df_params.to_csv(chemin_csv, index=False, encoding="utf-8-sig")
    print(f"CSV sauvegardé : {chemin_csv}")

    sauvegarder_graphique("09_cagr_historique_vs_long_terme.png")


def exporter_resume_global(df):
    """
    Exporte un tableau de contrôle global.
    """

    lignes = []

    for ville in sorted(df["ville"].unique()):
        df_ville = df[df["ville"] == ville].sort_values("date")

        df_hist = df_ville[df_ville["type"] == "historique_reel"]
        df_sim = df_ville[df_ville["type"] == "simulation"]

        prix_2015 = df_hist["prix_m2"].iloc[0]
        prix_fin_hist = df_hist["prix_m2"].iloc[-1]
        prix_fin_sim = df_sim["prix_m2"].iloc[-1]

        progression_hist = (prix_fin_hist / prix_2015 - 1) * 100
        progression_sim = (prix_fin_sim / prix_fin_hist - 1) * 100
        progression_totale = (prix_fin_sim / prix_2015 - 1) * 100

        lignes.append({
            "ville": ville,
            "ville_affichage": NOMS_AFFICHAGE.get(ville, ville.title()),
            "prix_2015": round(prix_2015, 2),
            "prix_fin_historique": round(prix_fin_hist, 2),
            "prix_fin_simulation": round(prix_fin_sim, 2),
            "progression_historique_pct": round(progression_hist, 2),
            "progression_simulation_pct": round(progression_sim, 2),
            "progression_totale_pct": round(progression_totale, 2),
            "variation_mensuelle_min_pct": round(df_sim["variation_mensuelle_pct"].min(), 4),
            "variation_mensuelle_max_pct": round(df_sim["variation_mensuelle_pct"].max(), 4)
        })

    resume = pd.DataFrame(lignes)

    chemin_csv = os.path.join(
        DOSSIER_GRAPHIQUES,
        "resume_global_simulation_immo.csv"
    )

    resume.to_csv(chemin_csv, index=False, encoding="utf-8-sig")

    print("\nRésumé global simulation immobilière :")
    print(resume)

    print(f"\nCSV sauvegardé : {chemin_csv}")


def main():
    print("Visualisation de la simulation immobilière 40 ans...\n")

    df = charger_donnees_completes()
    parametres = charger_parametres()

    exporter_resume_global(df)

    tracer_historique_et_simulation_toutes_villes(df)
    tracer_base_100_complet(df)
    tracer_base_100_simulation_seule(df)
    tracer_prix_final_simulation(df)
    tracer_progression_simulation(df)
    tracer_cagr_effectif(df)
    tracer_variations_mensuelles(df)
    tracer_par_ville(df)
    tracer_resume_parametres(parametres)

    print("\nTous les graphiques de simulation immobilière sont terminés !")
    print(f"Dossier de sortie : {DOSSIER_GRAPHIQUES}")


if __name__ == "__main__":
    main()