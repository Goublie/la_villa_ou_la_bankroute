import os
import io
import json
import requests
import pandas as pd
import matplotlib.pyplot as plt

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

DOSSIER_SORTIE = os.path.join(BASE_DIR, "inflation_clean")
DOSSIER_GRAPHIQUES = os.path.join(BASE_DIR, "graphiques_inflation")

os.makedirs(DOSSIER_SORTIE, exist_ok=True)
os.makedirs(DOSSIER_GRAPHIQUES, exist_ok=True)

# Données mensuelles IPC France, base 2015.
# Source indiquée par data.gouv : INSEE série 001759970.
URL_IPC = "https://www.data.gouv.fr/fr/datasets/r/f1f82e0e-4665-4909-ad66-0897a5972a77"

ANNEE_MIN = 2015
ANNEE_MAX = 2025


def telecharger_ipc():
    """
    Télécharge le CSV d'IPC depuis data.gouv.
    Si le téléchargement échoue, tu peux télécharger le CSV manuellement
    et le placer dans inflation_clean/ipc_france.csv.
    """
    chemin_local = os.path.join(DOSSIER_SORTIE, "ipc_france.csv")

    try:
        print("Téléchargement des données IPC...")

        reponse = requests.get(URL_IPC, timeout=30)
        reponse.raise_for_status()

        contenu = reponse.content.decode("utf-8-sig")

        with open(chemin_local, "w", encoding="utf-8") as fichier:
            fichier.write(contenu)

        print(f"CSV téléchargé : {chemin_local}")

    except Exception as erreur:
        print("Téléchargement automatique impossible.")
        print("Erreur :", erreur)

        if not os.path.exists(chemin_local):
            raise FileNotFoundError(
                "Aucun fichier local trouvé. "
                "Télécharge le CSV manuellement depuis data.gouv et place-le ici : "
                f"{chemin_local}"
            )

        print(f"Utilisation du fichier local : {chemin_local}")

    return chemin_local


def lire_csv_ipc(chemin_csv):
    """
    Lit le CSV en détectant automatiquement le séparateur.
    """
    try:
        df = pd.read_csv(chemin_csv)
    except Exception:
        df = pd.read_csv(chemin_csv, sep=";")

    # Sécurité si pandas lit tout dans une seule colonne
    if len(df.columns) == 1:
        df = pd.read_csv(chemin_csv, sep=";")

    colonnes_attendues = {"periode", "date", "ipc"}

    if not colonnes_attendues.issubset(set(df.columns)):
        raise ValueError(
            f"Colonnes inattendues : {list(df.columns)}. "
            "Le fichier doit contenir periode, date et ipc."
        )

    return df


def nettoyer_ipc(df):
    """
    Nettoie les données IPC et calcule :
    - inflation mensuelle ;
    - inflation annuelle glissante ;
    - indice base 100 en janvier 2015.
    """

    df["periode"] = df["periode"].astype(str)
    df["date"] = pd.to_datetime(df["date"], errors="coerce")

    df["ipc"] = (
        df["ipc"]
        .astype(str)
        .str.replace(",", ".", regex=False)
    )

    df["ipc"] = pd.to_numeric(df["ipc"], errors="coerce")

    df = df.dropna(subset=["date", "ipc"])
    df = df.sort_values("date").reset_index(drop=True)

    df["annee"] = df["date"].dt.year
    df["mois"] = df["date"].dt.month

    # On garde 2014 aussi temporairement pour pouvoir calculer
    # l'inflation annuelle moyenne de 2015 par rapport à 2014.
    df_calcul = df[(df["annee"] >= ANNEE_MIN - 1) & (df["annee"] <= ANNEE_MAX)].copy()

    df_calcul["inflation_mensuelle_pct"] = df_calcul["ipc"].pct_change() * 100
    df_calcul["inflation_annuelle_glissante_pct"] = df_calcul["ipc"].pct_change(12) * 100

    # Base 100 au premier mois de 2015
    ipc_janvier_2015 = df_calcul[df_calcul["periode"] == "2015-01"]["ipc"].iloc[0]
    df_calcul["base_100_janvier_2015"] = df_calcul["ipc"] / ipc_janvier_2015 * 100

    # Sortie finale : uniquement 2015-2025
    df_final = df_calcul[
        (df_calcul["annee"] >= ANNEE_MIN)
        & (df_calcul["annee"] <= ANNEE_MAX)
    ].copy()

    colonnes_finales = [
        "annee",
        "mois",
        "periode",
        "date",
        "ipc",
        "base_100_janvier_2015",
        "inflation_mensuelle_pct",
        "inflation_annuelle_glissante_pct"
    ]

    df_final = df_final[colonnes_finales]

    colonnes_arrondi = [
        "ipc",
        "base_100_janvier_2015",
        "inflation_mensuelle_pct",
        "inflation_annuelle_glissante_pct"
    ]

    for colonne in colonnes_arrondi:
        df_final[colonne] = df_final[colonne].round(4)

    return df_calcul, df_final


def calculer_inflation_annuelle(df_calcul):
    """
    Calcule l'inflation annuelle moyenne :
    moyenne IPC année N comparée à moyenne IPC année N-1.
    """
    annuel = (
        df_calcul.groupby("annee")
        .agg(
            ipc_moyen=("ipc", "mean"),
            ipc_janvier=("ipc", "first"),
            ipc_decembre=("ipc", "last")
        )
        .reset_index()
        .sort_values("annee")
    )

    annuel["inflation_annuelle_moyenne_pct"] = annuel["ipc_moyen"].pct_change() * 100
    annuel["inflation_decembre_decembre_pct"] = annuel["ipc_decembre"].pct_change() * 100

    annuel = annuel[
        (annuel["annee"] >= ANNEE_MIN)
        & (annuel["annee"] <= ANNEE_MAX)
    ].copy()

    for colonne in [
        "ipc_moyen",
        "ipc_janvier",
        "ipc_decembre",
        "inflation_annuelle_moyenne_pct",
        "inflation_decembre_decembre_pct"
    ]:
        annuel[colonne] = annuel[colonne].round(4)

    return annuel


def exporter_json(df_mensuel, df_annuel):
    """
    Exporte les fichiers JSON utiles.
    """

    chemin_mensuel = os.path.join(DOSSIER_SORTIE, "inflation_mensuel.json")
    chemin_annuel = os.path.join(DOSSIER_SORTIE, "inflation_annuel.json")
    chemin_jeu = os.path.join(DOSSIER_SORTIE, "inflation_mensuel_format_jeu.json")

    df_mensuel.to_json(
        chemin_mensuel,
        orient="records",
        indent=2,
        force_ascii=False,
        date_format="iso"
    )

    df_annuel.to_json(
        chemin_annuel,
        orient="records",
        indent=2,
        force_ascii=False
    )

    data_jeu = {
        "inflation_france": [
            {
                "Annee": int(row["annee"]),
                "Mois": int(row["mois"]),
                "Periode": str(row["periode"]),
                "IPC": float(row["ipc"]),
                "Base100_2015_01": float(row["base_100_janvier_2015"]),
                "Inflation_mensuelle_pct": None
                if pd.isna(row["inflation_mensuelle_pct"])
                else float(row["inflation_mensuelle_pct"]),
                "Inflation_annuelle_glissante_pct": None
                if pd.isna(row["inflation_annuelle_glissante_pct"])
                else float(row["inflation_annuelle_glissante_pct"])
            }
            for _, row in df_mensuel.iterrows()
        ]
    }

    with open(chemin_jeu, "w", encoding="utf-8") as fichier:
        json.dump(data_jeu, fichier, indent=2, ensure_ascii=False)

    print(f"Export JSON mensuel : {chemin_mensuel}")
    print(f"Export JSON annuel  : {chemin_annuel}")
    print(f"Export JSON jeu     : {chemin_jeu}")


def exporter_csv(df_mensuel, df_annuel):
    chemin_mensuel = os.path.join(DOSSIER_SORTIE, "inflation_mensuel.csv")
    chemin_annuel = os.path.join(DOSSIER_SORTIE, "inflation_annuel.csv")

    df_mensuel.to_csv(chemin_mensuel, index=False, encoding="utf-8-sig")
    df_annuel.to_csv(chemin_annuel, index=False, encoding="utf-8-sig")

    print(f"Export CSV mensuel : {chemin_mensuel}")
    print(f"Export CSV annuel  : {chemin_annuel}")


def tracer_graphiques(df_mensuel, df_annuel):
    """
    Génère quelques graphiques de contrôle.
    """

    # IPC base 100
    plt.figure(figsize=(13, 6))
    plt.plot(
        pd.to_datetime(df_mensuel["date"]),
        df_mensuel["base_100_janvier_2015"],
        linewidth=2
    )
    plt.title("Inflation France — IPC base 100 en janvier 2015")
    plt.xlabel("Date")
    plt.ylabel("Indice base 100")
    plt.grid(True)
    plt.tight_layout()

    chemin = os.path.join(DOSSIER_GRAPHIQUES, "01_ipc_base_100_janvier_2015.png")
    plt.savefig(chemin, dpi=150)
    plt.show()
    print(f"Graphique sauvegardé : {chemin}")

    # Inflation annuelle glissante
    plt.figure(figsize=(13, 6))
    plt.plot(
        pd.to_datetime(df_mensuel["date"]),
        df_mensuel["inflation_annuelle_glissante_pct"],
        linewidth=2
    )
    plt.axhline(0, linestyle="--")
    plt.title("Inflation France — glissement annuel mensuel")
    plt.xlabel("Date")
    plt.ylabel("Inflation sur 12 mois (%)")
    plt.grid(True)
    plt.tight_layout()

    chemin = os.path.join(DOSSIER_GRAPHIQUES, "02_inflation_annuelle_glissante.png")
    plt.savefig(chemin, dpi=150)
    plt.show()
    print(f"Graphique sauvegardé : {chemin}")

    # Inflation annuelle moyenne
    plt.figure(figsize=(11, 6))
    plt.bar(
        df_annuel["annee"].astype(str),
        df_annuel["inflation_annuelle_moyenne_pct"]
    )
    plt.title("Inflation France — moyenne annuelle")
    plt.xlabel("Année")
    plt.ylabel("Inflation moyenne annuelle (%)")
    plt.grid(True, axis="y")
    plt.tight_layout()

    chemin = os.path.join(DOSSIER_GRAPHIQUES, "03_inflation_annuelle_moyenne.png")
    plt.savefig(chemin, dpi=150)
    plt.show()
    print(f"Graphique sauvegardé : {chemin}")


def main():
    print("Scrapping inflation France 2015-2025...\n")

    chemin_csv = telecharger_ipc()

    df_brut = lire_csv_ipc(chemin_csv)

    df_calcul, df_mensuel = nettoyer_ipc(df_brut)

    df_annuel = calculer_inflation_annuelle(df_calcul)

    exporter_csv(df_mensuel, df_annuel)
    exporter_json(df_mensuel, df_annuel)

    tracer_graphiques(df_mensuel, df_annuel)

    print("\nRésumé inflation annuelle :")
    print(df_annuel)

    print("\nScrapping inflation terminé !")


if __name__ == "__main__":
    main()