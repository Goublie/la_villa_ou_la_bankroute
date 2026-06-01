import os
import re
import json
import requests
import pandas as pd

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
DOSSIER_SORTIE = os.path.join(BASE_DIR, "livret_a_clean")
os.makedirs(DOSSIER_SORTIE, exist_ok=True)

ANNEE_MIN = 2015
ANNEE_MAX = 2025

URL_LIVRET_A_WEBSTAT = (
    "https://webstat.banque-france.fr/fr/catalogue/mir1/"
    "MIR1.M.FR.B.L23FRLA.D.R.A.2230U6.EUR.O"
)


def convertir_nombre(valeur):
    """
    Convertit une valeur texte en nombre flottant.
    Gère les virgules françaises, les pourcentages et les espaces insécables.
    """
    if pd.isna(valeur):
        return None

    texte = str(valeur).strip()
    texte = texte.replace("\xa0", " ")
    texte = texte.replace("%", "")
    texte = texte.replace(",", ".")

    # Garde uniquement chiffres, signe négatif et point décimal.
    texte = re.sub(r"[^0-9.\-]", "", texte)

    if texte in ("", "-", "."):
        return None

    try:
        return float(texte)
    except ValueError:
        return None


def parser_periode_francaise(texte):
    """
    Convertit une période française en date mensuelle.

    Exemples :
    - "janv. 2020" -> 2020-01-01
    - "févr. 2022" -> 2022-02-01
    - "août 2025" -> 2025-08-01
    """
    if pd.isna(texte):
        return pd.NaT

    texte = str(texte).lower().strip()
    texte = texte.replace(".", "")
    texte = texte.replace("é", "e")
    texte = texte.replace("è", "e")
    texte = texte.replace("ê", "e")
    texte = texte.replace("û", "u")
    texte = texte.replace("ù", "u")
    texte = texte.replace("ô", "o")
    texte = texte.replace("août", "aout")

    mois_map = {
        "janv": 1, "janvier": 1,
        "fevr": 2, "fevrier": 2,
        "mars": 3,
        "avr": 4, "avril": 4,
        "mai": 5,
        "juin": 6,
        "juil": 7, "juillet": 7,
        "aout": 8,
        "sept": 9, "septembre": 9,
        "oct": 10, "octobre": 10,
        "nov": 11, "novembre": 11,
        "dec": 12, "decembre": 12,
    }

    match = re.search(r"([a-z]+)\s+(\d{4})", texte)
    if not match:
        return pd.NaT

    mois_txt = match.group(1)
    annee = int(match.group(2))

    if mois_txt not in mois_map:
        return pd.NaT

    return pd.Timestamp(year=annee, month=mois_map[mois_txt], day=1)


def telecharger(url: str, chemin: str) -> bool:
    """
    Télécharge une page ou un fichier.
    Retourne True si le téléchargement réussit, False sinon.
    """
    try:
        print(f"Téléchargement : {url}")
        reponse = requests.get(url, timeout=60)
        reponse.raise_for_status()

        with open(chemin, "wb") as fichier:
            fichier.write(reponse.content)

        print(f"OK : {chemin}")
        return True

    except Exception as erreur:
        print(f"Téléchargement impossible : {erreur}")
        return False


def construire_serie_mensuelle_depuis_changements(df_changements):
    """
    Transforme une liste de changements de taux en série mensuelle complète.

    Exemple : si le taux change le 2022-02-01, alors tous les mois à partir
    de février 2022 prennent ce taux jusqu'au prochain changement.
    """
    df_changements = df_changements.copy()
    df_changements["date_changement"] = pd.to_datetime(
        df_changements["date_changement"],
        errors="coerce"
    )

    df_changements = df_changements.dropna(
        subset=["date_changement", "taux_livret_a_reel_pct"]
    )

    df_changements = df_changements.sort_values("date_changement")

    if df_changements.empty:
        raise ValueError("Aucun changement de taux exploitable.")

    dates_mensuelles = pd.date_range(
        start=f"{ANNEE_MIN}-01-01",
        end=f"{ANNEE_MAX}-12-01",
        freq="MS"
    )

    df_mensuel = pd.DataFrame({"date": dates_mensuelles})

    df_mensuel = pd.merge_asof(
        df_mensuel.sort_values("date"),
        df_changements.sort_values("date_changement"),
        left_on="date",
        right_on="date_changement",
        direction="backward"
    )

    df_mensuel = df_mensuel.dropna(subset=["taux_livret_a_reel_pct"])

    df_mensuel["periode"] = df_mensuel["date"].dt.to_period("M").astype(str)
    df_mensuel["annee"] = df_mensuel["date"].dt.year
    df_mensuel["mois"] = df_mensuel["date"].dt.month

    df_mensuel["taux_livret_a_reel_pct"] = (
        df_mensuel["taux_livret_a_reel_pct"].astype(float).round(4)
    )

    return df_mensuel[
        [
            "date",
            "periode",
            "annee",
            "mois",
            "taux_livret_a_reel_pct"
        ]
    ].copy()


def lire_livret_a_historique():
    """
    Récupère le taux historique réel du Livret A.

    Méthode :
    1. Tentative de lecture de la page Webstat Banque de France.
    2. Si la page n'est pas exploitable automatiquement, utilisation d'un fallback
       manuel avec les changements de taux récents utiles au projet.

    Sortie :
    - une série mensuelle complète de ANNEE_MIN à ANNEE_MAX ;
    - chaque mois contient le taux annuel du Livret A réellement appliqué.
    """
    print("Chargement du taux réel historique du Livret A...")

    chemin_local = os.path.join(DOSSIER_SORTIE, "livret_a_webstat.html")

    if not os.path.exists(chemin_local):
        telecharger(URL_LIVRET_A_WEBSTAT, chemin_local)

    try:
        tables = pd.read_html(chemin_local)

        meilleure_table = None

        for table in tables:
            colonnes = [str(colonne).lower() for colonne in table.columns]

            contient_periode = any(
                "période" in colonne or "periode" in colonne
                for colonne in colonnes
            )

            contient_valeur = any(
                "valeur" in colonne or "value" in colonne
                for colonne in colonnes
            )

            if contient_periode and contient_valeur:
                meilleure_table = table.copy()
                break

        if meilleure_table is not None:
            mapping = {}

            for colonne in meilleure_table.columns:
                nom_colonne = str(colonne).lower()

                if "période" in nom_colonne or "periode" in nom_colonne:
                    mapping[colonne] = "periode_fr"

                elif "valeur" in nom_colonne or "value" in nom_colonne:
                    mapping[colonne] = "valeur"

            meilleure_table = meilleure_table.rename(columns=mapping)

            if "periode_fr" in meilleure_table.columns and "valeur" in meilleure_table.columns:
                meilleure_table["date_changement"] = meilleure_table["periode_fr"].apply(
                    parser_periode_francaise
                )
                meilleure_table["taux_livret_a_reel_pct"] = meilleure_table["valeur"].apply(
                    convertir_nombre
                )

                meilleure_table = meilleure_table.dropna(
                    subset=["date_changement", "taux_livret_a_reel_pct"]
                )

                if not meilleure_table.empty:
                    df_mensuel = construire_serie_mensuelle_depuis_changements(
                        meilleure_table[["date_changement", "taux_livret_a_reel_pct"]]
                    )

                    print("Taux Livret A réel chargé depuis Webstat.")
                    print(df_mensuel.head())
                    print(df_mensuel.tail())

                    return df_mensuel

    except Exception as erreur:
        print("Lecture Webstat impossible, fallback historique utilisé.")
        print("Erreur Webstat :", erreur)

    print("Utilisation du fallback historique réel du Livret A.")

    changements_taux = [
        ("2014-08-01", 1.00),
        ("2015-08-01", 0.75),
        ("2020-02-01", 0.50),
        ("2022-02-01", 1.00),
        ("2022-08-01", 2.00),
        ("2023-02-01", 3.00),
        ("2025-02-01", 2.40),
        ("2025-08-01", 1.70),
    ]

    df_changements = pd.DataFrame(
        changements_taux,
        columns=["date_changement", "taux_livret_a_reel_pct"]
    )

    df_mensuel = construire_serie_mensuelle_depuis_changements(df_changements)

    print("Fallback Livret A réel généré.")
    print(df_mensuel.head())
    print(df_mensuel.tail())

    return df_mensuel


def exporter(df: pd.DataFrame, nom: str):
    """
    Exporte un DataFrame en CSV et JSON.
    """
    chemin_csv = os.path.join(DOSSIER_SORTIE, f"{nom}.csv")
    chemin_json = os.path.join(DOSSIER_SORTIE, f"{nom}.json")

    df.to_csv(chemin_csv, index=False, encoding="utf-8-sig")

    df.to_json(
        chemin_json,
        orient="records",
        indent=2,
        force_ascii=False,
        date_format="iso"
    )

    print(f"Export CSV  : {chemin_csv}")
    print(f"Export JSON : {chemin_json}")


def exporter_format_jeu(df: pd.DataFrame):
    """
    Exporte un JSON simplifié pour le jeu.
    """
    chemin = os.path.join(DOSSIER_SORTIE, "livret_a_taux_reel_format_jeu.json")

    data = {
        "livret_a": [
            {
                "Annee": int(row["annee"]),
                "Mois": int(row["mois"]),
                "Periode": str(row["periode"]),
                "Taux_reel_pct": float(row["taux_livret_a_reel_pct"])
            }
            for _, row in df.iterrows()
        ]
    }

    with open(chemin, "w", encoding="utf-8") as fichier:
        json.dump(data, fichier, indent=2, ensure_ascii=False)

    print(f"Export JSON jeu : {chemin}")


def main():
    print("Scrapping historique réel du Livret A...\n")

    df_livret_a = lire_livret_a_historique()

    exporter(df_livret_a, "livret_a_taux_reel_mensuel")
    exporter_format_jeu(df_livret_a)

    print("\nAperçu du taux réel du Livret A :")
    print(df_livret_a.head())
    print("...")
    print(df_livret_a.tail())

    print("\nTerminé.")


if __name__ == "__main__":
    main()
