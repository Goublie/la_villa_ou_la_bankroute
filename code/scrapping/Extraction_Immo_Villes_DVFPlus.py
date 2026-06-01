import os
import re
import json
import pandas as pd

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

# Mets tous tes CSV DVF+ dans ce dossier.
DOSSIER_ENTREE = os.path.join(BASE_DIR, "donnees_dvfplus")

DOSSIER_SORTIE = os.path.join(BASE_DIR, "immo_villes_clean")
os.makedirs(DOSSIER_SORTIE, exist_ok=True)

ANNEE_MIN = 2015
ANNEE_MAX = 2025

SURFACE_MIN = 9
SURFACE_MAX = 300

PRIX_M2_MIN = 1000
PRIX_M2_MAX = 40000

CHUNKSIZE = 200_000


# ============================================================
# CONFIGURATION DES VILLES
# ============================================================
# IMPORTANT :
# - Modifie seulement les noms de fichiers pour correspondre à tes vrais CSV.
# - Même si un fichier contient toute une région, ce n'est pas grave.
# - Le script filtre ensuite avec les codes communes.
# ============================================================

VILLES = {
    "paris": {
        "nom_affichage": "Paris",
        "fichier": "dvfplus_paris.csv",
        "codes_communes": ["75056"] + [f"751{str(i).zfill(2)}" for i in range(1, 21)],
        "avec_arrondissements": True
    },
    "lyon": {
        "nom_affichage": "Lyon",
        "fichier": "dvfplus_lyon.csv",
        "codes_communes": ["69123"] + [f"6938{i}" for i in range(1, 10)],
        "avec_arrondissements": True
    },
    "marseille": {
        "nom_affichage": "Marseille",
        "fichier": "dvfplus_paca.csv",
        "codes_communes": ["13055"] + [f"132{str(i).zfill(2)}" for i in range(1, 17)],
        "avec_arrondissements": True
    },
    "bordeaux": {
        "nom_affichage": "Bordeaux",
        "fichier": "dvfplus_bordeaux.csv",
        "codes_communes": ["33063"],
        "avec_arrondissements": False
    },
    "toulouse": {
        "nom_affichage": "Toulouse",
        "fichier": "dvfplus_occitanie.csv",
        "codes_communes": ["31555"],
        "avec_arrondissements": False
    },
    "nantes": {
        "nom_affichage": "Nantes",
        "fichier": "dvfplus_pays_de_la_loire.csv",
        "codes_communes": ["44109"],
        "avec_arrondissements": False
    }
}


COLONNES_UTILISEES = [
    "idmutation",
    "datemut",
    "anneemut",
    "moismut",
    "coddep",
    "libnatmut",
    "l_codinsee",
    "valeurfonc",
    "nblocapt",
    "nblocdep",
    "sbati",
    "sbatapt",
    "codtypbien",
    "libtypbien",
    "geompar_x",
    "geompar_y"
]

COLONNES_REQUISES = [
    "idmutation",
    "datemut",
    "anneemut",
    "moismut",
    "libnatmut",
    "l_codinsee",
    "valeurfonc",
    "sbatapt",
    "codtypbien",
    "libtypbien"
]


def lire_colonnes_disponibles(chemin_fichier):
    """
    Lit uniquement l'en-tête du CSV pour vérifier les colonnes disponibles.
    """
    entete = pd.read_csv(
        chemin_fichier,
        sep="|",
        nrows=0,
        encoding="utf-8",
        encoding_errors="replace"
    )

    return list(entete.columns)


def construire_regex_codes(codes_communes, capture=False):
    """
    Construit une regex robuste pour détecter un code commune dans l_codinsee.
    Fonctionne même si l_codinsee contient plusieurs codes dans une chaîne.
    """
    codes_tries = sorted(codes_communes, key=len, reverse=True)
    bloc_codes = "|".join(re.escape(code) for code in codes_tries)

    if capture:
        return rf"(?<!\d)({bloc_codes})(?!\d)"

    return rf"(?<!\d)(?:{bloc_codes})(?!\d)"


def convertir_numerique(df, colonnes):
    """
    Convertit certaines colonnes en numérique.
    Gère aussi les nombres avec virgule décimale.
    """
    for colonne in colonnes:
        if colonne in df.columns:
            df[colonne] = (
                df[colonne]
                .astype(str)
                .str.replace(",", ".", regex=False)
            )
            df[colonne] = pd.to_numeric(df[colonne], errors="coerce")

    return df


def calculer_arrondissement(ville, code_commune):
    """
    Calcule l'arrondissement pour Paris, Lyon et Marseille.
    Retourne None pour les villes sans arrondissement.
    """
    if pd.isna(code_commune):
        return None

    code = str(code_commune)

    if ville == "paris":
        if code.startswith("751") and len(code) == 5:
            return int(code[-2:])
        return None

    if ville == "lyon":
        if code.startswith("6938") and len(code) == 5:
            return int(code[-1])
        return None

    if ville == "marseille":
        if code.startswith("132") and len(code) == 5:
            return int(code[-2:])
        return None

    return None


def nettoyer_chunk(chunk, ville, config):
    """
    Nettoie un morceau de fichier DVF+ pour une ville donnée.
    On garde uniquement :
    - les ventes ;
    - les appartements seuls ;
    - les années demandées ;
    - les codes communes de la ville ;
    - les surfaces et valeurs foncières exploitables.
    """

    codes_communes = config["codes_communes"]

    chunk = convertir_numerique(
        chunk,
        [
            "anneemut",
            "moismut",
            "valeurfonc",
            "sbatapt",
            "sbati",
            "nblocapt",
            "nblocdep",
            "geompar_x",
            "geompar_y"
        ]
    )

    chunk["libnatmut"] = chunk["libnatmut"].astype(str).str.strip()
    chunk["codtypbien"] = chunk["codtypbien"].astype(str).str.strip()
    chunk["l_codinsee"] = chunk["l_codinsee"].astype(str).str.strip()

    regex_match = construire_regex_codes(codes_communes, capture=False)
    regex_extract = construire_regex_codes(codes_communes, capture=True)

    filtre_code_commune = chunk["l_codinsee"].str.contains(
        regex_match,
        regex=True,
        na=False
    )

    filtre = (
        filtre_code_commune
        & (chunk["libnatmut"] == "Vente")
        & (chunk["anneemut"].between(ANNEE_MIN, ANNEE_MAX))
        & (chunk["moismut"].between(1, 12))
        & (chunk["codtypbien"] == "121")
        & (chunk["sbatapt"] > 0)
        & (chunk["valeurfonc"] > 0)
    )

    df = chunk.loc[filtre].copy()

    if df.empty:
        return df

    df["code_commune_match"] = df["l_codinsee"].str.extract(
        regex_extract,
        expand=False
    )

    df["prix_m2"] = df["valeurfonc"] / df["sbatapt"]

    df = df[
        (df["sbatapt"].between(SURFACE_MIN, SURFACE_MAX))
        & (df["prix_m2"].between(PRIX_M2_MIN, PRIX_M2_MAX))
    ].copy()

    if df.empty:
        return df

    df["ville"] = ville

    df["annee"] = df["anneemut"].astype(int)
    df["mois"] = df["moismut"].astype(int)

    df["periode_mois"] = (
        df["annee"].astype(str)
        + "-"
        + df["mois"].astype(str).str.zfill(2)
    )

    df["trimestre"] = ((df["mois"] - 1) // 3 + 1).astype(int)

    df["periode_trimestre"] = (
        df["annee"].astype(str)
        + "-T"
        + df["trimestre"].astype(str)
    )

    if config["avec_arrondissements"]:
        df["arrondissement"] = df["code_commune_match"].apply(
            lambda code: calculer_arrondissement(ville, code)
        )
    else:
        df["arrondissement"] = None

    colonnes_finales = [
        "ville",
        "idmutation",
        "datemut",
        "annee",
        "mois",
        "periode_mois",
        "trimestre",
        "periode_trimestre",
        "l_codinsee",
        "code_commune_match",
        "arrondissement",
        "libtypbien",
        "valeurfonc",
        "sbatapt",
        "prix_m2"
    ]

    if "geompar_x" in df.columns:
        colonnes_finales.append("geompar_x")

    if "geompar_y" in df.columns:
        colonnes_finales.append("geompar_y")

    df = df[colonnes_finales].copy()

    df = df.rename(columns={
        "datemut": "date_mutation",
        "l_codinsee": "code_commune_source",
        "code_commune_match": "code_commune",
        "libtypbien": "type_bien",
        "valeurfonc": "valeur_fonciere",
        "sbatapt": "surface_m2",
        "geompar_x": "x",
        "geompar_y": "y"
    })

    return df


def agreger_mensuel(df):
    """
    Calcule les vraies médianes mensuelles, sans lissage.
    """
    mensuel = (
        df.groupby(["ville", "annee", "mois", "periode_mois"])
        .agg(
            prix_m2_median=("prix_m2", "median"),
            prix_m2_moyen=("prix_m2", "mean"),
            q25=("prix_m2", lambda x: x.quantile(0.25)),
            q75=("prix_m2", lambda x: x.quantile(0.75)),
            nb_transactions=("prix_m2", "size")
        )
        .reset_index()
        .sort_values(["ville", "annee", "mois"])
    )

    colonnes_prix = ["prix_m2_median", "prix_m2_moyen", "q25", "q75"]

    for colonne in colonnes_prix:
        mensuel[colonne] = mensuel[colonne].round(2)

    return mensuel


def agreger_trimestriel(df):
    """
    Calcule les médianes trimestrielles, utiles pour contrôle.
    """
    trimestriel = (
        df.groupby(["ville", "annee", "trimestre", "periode_trimestre"])
        .agg(
            prix_m2_median=("prix_m2", "median"),
            prix_m2_moyen=("prix_m2", "mean"),
            q25=("prix_m2", lambda x: x.quantile(0.25)),
            q75=("prix_m2", lambda x: x.quantile(0.75)),
            nb_transactions=("prix_m2", "size")
        )
        .reset_index()
        .sort_values(["ville", "annee", "trimestre"])
    )

    colonnes_prix = ["prix_m2_median", "prix_m2_moyen", "q25", "q75"]

    for colonne in colonnes_prix:
        trimestriel[colonne] = trimestriel[colonne].round(2)

    return trimestriel


def agreger_arrondissement_mensuel(df):
    """
    Calcule les médianes mensuelles par arrondissement.
    Sert uniquement pour Paris, Lyon et Marseille.
    """
    df_arr = df[df["arrondissement"].notna()].copy()

    if df_arr.empty:
        return pd.DataFrame()

    df_arr["arrondissement"] = df_arr["arrondissement"].astype(int)

    arrondissements = (
        df_arr.groupby(["ville", "arrondissement", "annee", "mois", "periode_mois"])
        .agg(
            prix_m2_median=("prix_m2", "median"),
            prix_m2_moyen=("prix_m2", "mean"),
            q25=("prix_m2", lambda x: x.quantile(0.25)),
            q75=("prix_m2", lambda x: x.quantile(0.75)),
            nb_transactions=("prix_m2", "size")
        )
        .reset_index()
        .sort_values(["ville", "arrondissement", "annee", "mois"])
    )

    colonnes_prix = ["prix_m2_median", "prix_m2_moyen", "q25", "q75"]

    for colonne in colonnes_prix:
        arrondissements[colonne] = arrondissements[colonne].round(2)

    return arrondissements


def exporter_dataframe(df, dossier, nom_fichier):
    """
    Exporte un DataFrame en CSV et JSON.
    """
    os.makedirs(dossier, exist_ok=True)

    chemin_csv = os.path.join(dossier, f"{nom_fichier}.csv")
    chemin_json = os.path.join(dossier, f"{nom_fichier}.json")

    df.to_csv(chemin_csv, index=False, encoding="utf-8-sig")

    df.to_json(
        chemin_json,
        orient="records",
        indent=2,
        force_ascii=False
    )

    print(f"Export CSV  : {chemin_csv}")
    print(f"Export JSON : {chemin_json}")


def exporter_format_jeu(mensuel_par_ville, chemin_export):
    """
    Exporte un fichier unique compatible avec le jeu :
    {
      "paris": [...],
      "lyon": [...],
      ...
    }
    """
    data = {}

    for ville, df_mensuel in mensuel_par_ville.items():
        data[ville] = [
            {
                "Annee": int(row["annee"]),
                "Mois": int(row["mois"]),
                "Periode": str(row["periode_mois"]),
                "Prix_m2": float(row["prix_m2_median"]),
                "Nb_transactions": int(row["nb_transactions"])
            }
            for _, row in df_mensuel.iterrows()
        ]

    with open(chemin_export, "w", encoding="utf-8") as fichier:
        json.dump(data, fichier, indent=2, ensure_ascii=False)

    print(f"Export JSON jeu global : {chemin_export}")


def verifier_mois_manquants(df_mensuel, ville):
    """
    Vérifie si certains mois sont absents.
    On ne les remplit pas : on signale seulement.
    """
    if df_mensuel.empty:
        print(f"Attention : aucune donnée mensuelle pour {ville}.")
        return

    debut = f"{ANNEE_MIN}-01"
    fin = f"{ANNEE_MAX}-12"

    tous_les_mois = pd.period_range(debut, fin, freq="M")
    mois_observes = pd.PeriodIndex(df_mensuel["periode_mois"], freq="M")

    mois_manquants = sorted(set(tous_les_mois) - set(mois_observes))

    if mois_manquants:
        print(f"Attention : {ville} a {len(mois_manquants)} mois manquants.")
        print("Exemples :", [str(m) for m in mois_manquants[:10]])
    else:
        print(f"{ville} : aucun mois manquant entre {debut} et {fin}.")


def traiter_ville(ville, config):
    """
    Traite une ville complète :
    - lecture du CSV associé ;
    - filtrage par code commune ;
    - nettoyage ;
    - exports transactions, mensuel, trimestriel, arrondissements si pertinent.
    """
    nom_affichage = config["nom_affichage"]
    fichier = config["fichier"]

    chemin_fichier = os.path.join(DOSSIER_ENTREE, fichier)

    print("\n" + "=" * 70)
    print(f"Traitement : {nom_affichage}")
    print("=" * 70)

    if not os.path.exists(chemin_fichier):
        print(f"Fichier introuvable : {chemin_fichier}")
        return None

    colonnes_disponibles = lire_colonnes_disponibles(chemin_fichier)

    colonnes_manquantes = [
        colonne for colonne in COLONNES_REQUISES
        if colonne not in colonnes_disponibles
    ]

    if colonnes_manquantes:
        print(f"Colonnes manquantes dans {fichier} : {colonnes_manquantes}")
        return None

    usecols = [
        colonne for colonne in COLONNES_UTILISEES
        if colonne in colonnes_disponibles
    ]

    morceaux_nettoyes = []

    total_lignes_lues = 0
    total_lignes_gardees = 0

    lecteur = pd.read_csv(
        chemin_fichier,
        sep="|",
        usecols=usecols,
        chunksize=CHUNKSIZE,
        dtype=str,
        encoding="utf-8",
        encoding_errors="replace",
        low_memory=False
    )

    for index_chunk, chunk in enumerate(lecteur, start=1):
        total_lignes_lues += len(chunk)

        df_clean = nettoyer_chunk(chunk, ville, config)

        total_lignes_gardees += len(df_clean)

        if not df_clean.empty:
            morceaux_nettoyes.append(df_clean)

        print(
            f"Chunk {index_chunk} : "
            f"{len(chunk)} lignes lues, "
            f"{len(df_clean)} lignes gardées"
        )

    if not morceaux_nettoyes:
        print(f"Aucune donnée exploitable trouvée pour {nom_affichage}.")
        return None

    df_final = pd.concat(morceaux_nettoyes, ignore_index=True)

    avant_doublons = len(df_final)
    df_final = df_final.drop_duplicates(subset="idmutation")
    apres_doublons = len(df_final)

    print("\nExtraction terminée.")
    print(f"Lignes lues au total        : {total_lignes_lues}")
    print(f"Lignes gardées avant doublons : {avant_doublons}")
    print(f"Lignes gardées après doublons : {apres_doublons}")

    dossier_ville = os.path.join(DOSSIER_SORTIE, ville)
    os.makedirs(dossier_ville, exist_ok=True)

    exporter_dataframe(
        df_final,
        dossier_ville,
        f"immo_{ville}_transactions_utiles"
    )

    mensuel = agreger_mensuel(df_final)
    trimestriel = agreger_trimestriel(df_final)
    arrondissements = agreger_arrondissement_mensuel(df_final)

    exporter_dataframe(
        mensuel,
        dossier_ville,
        f"immo_{ville}_mensuel"
    )

    exporter_dataframe(
        trimestriel,
        dossier_ville,
        f"immo_{ville}_trimestriel"
    )

    if not arrondissements.empty:
        exporter_dataframe(
            arrondissements,
            dossier_ville,
            f"immo_{ville}_arrondissement_mensuel"
        )

    verifier_mois_manquants(mensuel, ville)

    print(f"\nRésumé mensuel {nom_affichage} :")
    print(mensuel.head())
    print("...")
    print(mensuel.tail())

    return {
        "transactions": df_final,
        "mensuel": mensuel,
        "trimestriel": trimestriel,
        "arrondissements": arrondissements
    }


def main():
    print("Début de l'extraction immobilière DVF+ multi-villes...\n")

    print(f"Dossier d'entrée : {DOSSIER_ENTREE}")
    print(f"Dossier de sortie : {DOSSIER_SORTIE}\n")

    resultats = {}
    mensuel_par_ville = {}

    for ville, config in VILLES.items():
        resultat = traiter_ville(ville, config)

        if resultat is not None:
            resultats[ville] = resultat
            mensuel_par_ville[ville] = resultat["mensuel"]

    if not mensuel_par_ville:
        print("\nAucune ville n'a pu être traitée.")
        return

    chemin_json_jeu = os.path.join(
        DOSSIER_SORTIE,
        "immo_villes_mensuel_format_jeu.json"
    )

    exporter_format_jeu(mensuel_par_ville, chemin_json_jeu)

    # Exports globaux détaillés pour contrôle
    tous_mensuels = pd.concat(mensuel_par_ville.values(), ignore_index=True)

    exporter_dataframe(
        tous_mensuels,
        DOSSIER_SORTIE,
        "immo_villes_mensuel_detaille"
    )

    tous_trimestriels = pd.concat(
        [resultats[ville]["trimestriel"] for ville in resultats],
        ignore_index=True
    )

    exporter_dataframe(
        tous_trimestriels,
        DOSSIER_SORTIE,
        "immo_villes_trimestriel_detaille"
    )

    print("\nExtraction multi-villes terminée !")
    print(f"Fichier principal pour le jeu : {chemin_json_jeu}")


if __name__ == "__main__":
    main()