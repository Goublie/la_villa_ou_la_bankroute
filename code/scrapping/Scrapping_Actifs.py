import os
import pandas as pd
import yfinance as yf

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

START_DATE = "2015-01-01"

# Attention : dans yfinance, la date de fin est exclusive.
# Pour récupérer jusqu'au 31/12/2024 inclus, on met donc 2025-01-01.
END_DATE = "2025-01-01"


ACTIFS = [
    {
        "nom_fichier": "bourse_cac40",
        "ticker": "^FCHI",
        "nom_affichage": "CAC 40",
        "inverse": False
    },
    {
        "nom_fichier": "nvidia",
        "ticker": "NVDA",
        "nom_affichage": "Nvidia",
        "inverse": False
    },
    {
        "nom_fichier": "or",
        "ticker": "GC=F",
        "nom_affichage": "Or",
        "inverse": False
    },
    {
        "nom_fichier": "google",
        "ticker": "GOOGL",
        "nom_affichage": "Google",
        "inverse": False
    },
    {
        "nom_fichier": "dollar",
        "ticker": "EURUSD=X",
        "nom_affichage": "Dollar USD/EUR",
        # EURUSD=X donne : 1 euro = X dollars.
        # Pour obtenir le prix de 1 dollar en euros, on inverse.
        "inverse": True
    },
    {
        "nom_fichier": "bitcoin",
        "ticker": "BTC-USD",
        "nom_affichage": "Bitcoin",
        "inverse": False
    },
    {
        "nom_fichier": "totalenergies",
        "ticker": "TTE.PA",
        "nom_affichage": "TotalEnergies",
        "inverse": False
    }
]


def extraire_close(df: pd.DataFrame) -> pd.Series:
    """
    Extrait proprement la colonne Close, même si yfinance retourne
    parfois des colonnes multi-niveaux.
    """
    if df.empty:
        raise ValueError("Le DataFrame reçu est vide.")

    if isinstance(df.columns, pd.MultiIndex):
        for level in range(df.columns.nlevels):
            if "Close" in df.columns.get_level_values(level):
                close_data = df.xs("Close", axis=1, level=level)

                if isinstance(close_data, pd.DataFrame):
                    return close_data.iloc[:, 0]

                return close_data

        raise ValueError("Impossible de trouver une colonne Close dans les données multi-niveaux.")

    if "Close" not in df.columns:
        raise ValueError("Impossible de trouver une colonne Close dans les données.")

    return df["Close"]


def telecharger_actif(actif: dict) -> None:
    """
    Télécharge un actif financier avec yfinance, nettoie les données,
    puis exporte un fichier JSON contenant Date et Close.
    """
    nom_fichier = actif["nom_fichier"]
    ticker = actif["ticker"]
    nom_affichage = actif["nom_affichage"]
    inverse = actif["inverse"]

    print(f"Téléchargement de {nom_affichage} ({ticker})...")

    donnees_brutes = yf.download(
        ticker,
        start=START_DATE,
        end=END_DATE,
        progress=False,
        auto_adjust=False
    )

    close = extraire_close(donnees_brutes)

    df = pd.DataFrame({
        "Date": donnees_brutes.index,
        "Close": close.values
    })

    # Nettoyage de base
    df = df.dropna()
    df = df.drop_duplicates(subset="Date")
    df = df.sort_values("Date")

    # Conversion EUR/USD vers USD/EUR pour le dollar
    if inverse:
        df["Close"] = 1 / df["Close"]

    # Format compatible JSON / Unity
    df["Date"] = pd.to_datetime(df["Date"]).dt.strftime("%Y-%m-%d")
    df["Close"] = df["Close"].astype(float)

    chemin_export = os.path.join(BASE_DIR, f"{nom_fichier}.json")

    df.to_json(
        chemin_export,
        orient="records",
        indent=2
    )

    print(f"{nom_affichage} exporté : {len(df)} lignes -> {nom_fichier}.json")


def main():
    print("Début du scraping des actifs financiers...\n")

    for actif in ACTIFS:
        try:
            telecharger_actif(actif)
        except Exception as erreur:
            print(f"Erreur avec {actif['nom_affichage']} : {erreur}")

    print("\nScraping terminé !")


if __name__ == "__main__":
    main()