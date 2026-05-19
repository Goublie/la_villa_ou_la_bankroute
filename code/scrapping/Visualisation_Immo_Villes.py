import os
import json
import pandas as pd
import matplotlib.pyplot as plt

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

DOSSIER_DONNEES = os.path.join(BASE_DIR, "immo_villes_clean")
DOSSIER_GRAPHIQUES = os.path.join(BASE_DIR, "graphiques_immo_villes")

FICHIER_JEU = os.path.join(DOSSIER_DONNEES, "immo_villes_mensuel_format_jeu.json")

os.makedirs(DOSSIER_GRAPHIQUES, exist_ok=True)


NOMS_AFFICHAGE = {
    "paris": "Paris",
    "lyon": "Lyon",
    "marseille": "Marseille",
    "bordeaux": "Bordeaux",
    "toulouse": "Toulouse",
    "nantes": "Nantes"
}


def charger_donnees_jeu():
    """
    Charge le JSON global compatible jeu :
    {
      "paris": [...],
      "lyon": [...],
      ...
    }
    """
    if not os.path.exists(FICHIER_JEU):
        raise FileNotFoundError(f"Fichier introuvable : {FICHIER_JEU}")

    with open(FICHIER_JEU, "r", encoding="utf-8") as fichier:
        data = json.load(fichier)

    lignes = []

    for ville, valeurs in data.items():
        for ligne in valeurs:
            lignes.append({
                "ville": ville,
                "ville_affichage": NOMS_AFFICHAGE.get(ville, ville.title()),
                "annee": int(ligne["Annee"]),
                "mois": int(ligne["Mois"]),
                "periode": ligne["Periode"],
                "prix_m2": float(ligne["Prix_m2"]),
                "nb_transactions": int(ligne["Nb_transactions"])
            })

    df = pd.DataFrame(lignes)

    df["date"] = pd.to_datetime(df["periode"] + "-01")
    df = df.sort_values(["ville", "date"])

    return df


def sauvegarder_graphique(nom_fichier):
    chemin = os.path.join(DOSSIER_GRAPHIQUES, nom_fichier)
    plt.tight_layout()
    plt.savefig(chemin, dpi=150)
    plt.show()
    print(f"Graphique sauvegardé : {chemin}")


def tracer_prix_m2_mensuel(df):
    """
    Trace les prix médians mensuels en euros/m².
    """
    plt.figure(figsize=(14, 7))

    for ville in sorted(df["ville"].unique()):
        df_ville = df[df["ville"] == ville]

        plt.plot(
            df_ville["date"],
            df_ville["prix_m2"],
            linewidth=1.8,
            label=NOMS_AFFICHAGE.get(ville, ville.title())
        )

    plt.title("Prix médian mensuel au m² par ville")
    plt.xlabel("Date")
    plt.ylabel("Prix médian au m² (€)")
    plt.legend()
    plt.grid(True)

    sauvegarder_graphique("01_prix_m2_mensuel_par_ville.png")


def tracer_prix_m2_base_100(df):
    """
    Compare les villes en base 100.
    Utile pour voir quelle ville a le plus progressé relativement.
    """
    plt.figure(figsize=(14, 7))

    for ville in sorted(df["ville"].unique()):
        df_ville = df[df["ville"] == ville].copy()

        prix_depart = df_ville["prix_m2"].iloc[0]
        df_ville["base_100"] = df_ville["prix_m2"] / prix_depart * 100

        plt.plot(
            df_ville["date"],
            df_ville["base_100"],
            linewidth=1.8,
            label=NOMS_AFFICHAGE.get(ville, ville.title())
        )

    plt.title("Évolution des prix immobiliers par ville — base 100")
    plt.xlabel("Date")
    plt.ylabel("Indice base 100")
    plt.legend()
    plt.grid(True)

    sauvegarder_graphique("02_prix_m2_base_100_par_ville.png")


def tracer_transactions_mensuelles(df):
    """
    Trace le nombre de transactions mensuelles pour chaque ville.
    """
    plt.figure(figsize=(14, 7))

    for ville in sorted(df["ville"].unique()):
        df_ville = df[df["ville"] == ville]

        plt.plot(
            df_ville["date"],
            df_ville["nb_transactions"],
            linewidth=1.5,
            label=NOMS_AFFICHAGE.get(ville, ville.title())
        )

    plt.title("Nombre de transactions mensuelles par ville")
    plt.xlabel("Date")
    plt.ylabel("Nombre de transactions")
    plt.legend()
    plt.grid(True)

    sauvegarder_graphique("03_transactions_mensuelles_par_ville.png")


def tracer_transactions_annuelles(df):
    """
    Agrège les transactions par année et par ville.
    Utile pour voir l'effet Covid et la baisse du marché.
    """
    annuel = (
        df.groupby(["ville", "ville_affichage", "annee"])
        .agg(
            nb_transactions_annuel=("nb_transactions", "sum")
        )
        .reset_index()
        .sort_values(["ville", "annee"])
    )

    plt.figure(figsize=(14, 7))

    for ville in sorted(annuel["ville"].unique()):
        df_ville = annuel[annuel["ville"] == ville]

        plt.plot(
            df_ville["annee"],
            df_ville["nb_transactions_annuel"],
            marker="o",
            linewidth=1.8,
            label=NOMS_AFFICHAGE.get(ville, ville.title())
        )

    plt.title("Nombre de transactions annuelles par ville")
    plt.xlabel("Année")
    plt.ylabel("Nombre de transactions")
    plt.legend()
    plt.grid(True)

    sauvegarder_graphique("04_transactions_annuelles_par_ville.png")

    chemin_csv = os.path.join(DOSSIER_GRAPHIQUES, "transactions_annuelles_par_ville.csv")
    annuel.to_csv(chemin_csv, index=False, encoding="utf-8-sig")
    print(f"CSV sauvegardé : {chemin_csv}")


def tracer_prix_m2_annuel(df):
    """
    Calcule un prix annuel moyen à partir des prix mensuels.
    Ce n'est pas une nouvelle donnée DVF, juste une lecture annuelle
    pour avoir une courbe plus synthétique.
    """
    annuel = (
        df.groupby(["ville", "ville_affichage", "annee"])
        .agg(
            prix_m2_annuel=("prix_m2", "mean"),
            nb_transactions_annuel=("nb_transactions", "sum")
        )
        .reset_index()
        .sort_values(["ville", "annee"])
    )

    plt.figure(figsize=(14, 7))

    for ville in sorted(annuel["ville"].unique()):
        df_ville = annuel[annuel["ville"] == ville]

        plt.plot(
            df_ville["annee"],
            df_ville["prix_m2_annuel"],
            marker="o",
            linewidth=1.8,
            label=NOMS_AFFICHAGE.get(ville, ville.title())
        )

    plt.title("Prix moyen annuel au m² par ville")
    plt.xlabel("Année")
    plt.ylabel("Prix au m² (€)")
    plt.legend()
    plt.grid(True)

    sauvegarder_graphique("05_prix_m2_annuel_par_ville.png")

    chemin_csv = os.path.join(DOSSIER_GRAPHIQUES, "prix_m2_annuel_par_ville.csv")
    annuel.to_csv(chemin_csv, index=False, encoding="utf-8-sig")
    print(f"CSV sauvegardé : {chemin_csv}")


def tracer_classement_prix_final(df):
    """
    Classe les villes selon le dernier mois disponible.
    """
    derniere_date = df["date"].max()

    dernier_mois = (
        df[df["date"] == derniere_date]
        .copy()
        .sort_values("prix_m2", ascending=True)
    )

    plt.figure(figsize=(10, 6))

    plt.barh(
        dernier_mois["ville_affichage"],
        dernier_mois["prix_m2"]
    )

    plt.title(f"Classement des villes par prix au m² — {derniere_date.strftime('%Y-%m')}")
    plt.xlabel("Prix médian au m² (€)")
    plt.ylabel("Ville")
    plt.grid(True, axis="x")

    for index, valeur in enumerate(dernier_mois["prix_m2"]):
        plt.text(
            valeur,
            index,
            f" {valeur:,.0f} €".replace(",", " "),
            va="center"
        )

    sauvegarder_graphique("06_classement_prix_m2_dernier_mois.png")


def tracer_classement_progression(df):
    """
    Classe les villes selon leur progression entre le premier et le dernier mois.
    """
    lignes = []

    for ville in sorted(df["ville"].unique()):
        df_ville = df[df["ville"] == ville].sort_values("date")

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

    plt.title("Progression du prix au m² entre 2015-01 et 2025-12")
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

    sauvegarder_graphique("07_classement_progression_2015_2025.png")

    chemin_csv = os.path.join(DOSSIER_GRAPHIQUES, "classement_progression_2015_2025.csv")
    classement.to_csv(chemin_csv, index=False, encoding="utf-8-sig")
    print(f"CSV sauvegardé : {chemin_csv}")


def tracer_prix_et_transactions_par_ville(df):
    """
    Génère un graphique séparé pour chaque ville :
    prix au m² + nombre de transactions sur deux axes.
    """
    for ville in sorted(df["ville"].unique()):
        df_ville = df[df["ville"] == ville].copy()
        nom_ville = NOMS_AFFICHAGE.get(ville, ville.title())

        fig, ax1 = plt.subplots(figsize=(13, 6))

        ax1.plot(
            df_ville["date"],
            df_ville["prix_m2"],
            linewidth=1.8,
            marker="o",
            markersize=2.5,
            label="Prix médian au m²"
        )

        ax1.set_xlabel("Date")
        ax1.set_ylabel("Prix médian au m² (€)")
        ax1.grid(True)

        ax2 = ax1.twinx()

        ax2.bar(
            df_ville["date"],
            df_ville["nb_transactions"],
            width=25,
            alpha=0.25,
            label="Transactions"
        )

        ax2.set_ylabel("Nombre de transactions")

        plt.title(f"{nom_ville} — Prix au m² et transactions mensuelles")

        fig.tight_layout()

        chemin = os.path.join(
            DOSSIER_GRAPHIQUES,
            f"08_{ville}_prix_et_transactions.png"
        )

        plt.savefig(chemin, dpi=150)
        plt.show()

        print(f"Graphique sauvegardé : {chemin}")


def verifier_integrite(df):
    """
    Vérifie rapidement si chaque ville a bien 132 mois.
    """
    print("\nVérification des données :")

    resume = (
        df.groupby("ville")
        .agg(
            premiere_periode=("periode", "min"),
            derniere_periode=("periode", "max"),
            nb_mois=("periode", "count"),
            prix_min=("prix_m2", "min"),
            prix_max=("prix_m2", "max"),
            transactions_min=("nb_transactions", "min"),
            transactions_max=("nb_transactions", "max")
        )
        .reset_index()
    )

    print(resume)

    chemin_csv = os.path.join(DOSSIER_GRAPHIQUES, "resume_controle_villes.csv")
    resume.to_csv(chemin_csv, index=False, encoding="utf-8-sig")

    print(f"\nRésumé sauvegardé : {chemin_csv}")


def main():
    print("Génération des graphiques immobiliers multi-villes...\n")

    df = charger_donnees_jeu()

    verifier_integrite(df)

    tracer_prix_m2_mensuel(df)
    tracer_prix_m2_base_100(df)
    tracer_transactions_mensuelles(df)
    tracer_transactions_annuelles(df)
    tracer_prix_m2_annuel(df)
    tracer_classement_prix_final(df)
    tracer_classement_progression(df)
    tracer_prix_et_transactions_par_ville(df)

    print("\nTous les graphiques multi-villes sont terminés !")
    print(f"Dossier de sortie : {DOSSIER_GRAPHIQUES}")


if __name__ == "__main__":
    main()