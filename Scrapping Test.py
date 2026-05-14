import yfinance as yf
import pandas as pd

# Le CAC 40 s'appelle "^FCHI" sur Yahoo Finance
cac = yf.download("^FCHI", start="2015-01-01", end="2024-12-31")

# On garde uniquement le prix de clôture
cac = cac[["Close"]]

# On regarde ce qu'on a
print(cac.head())
print(cac.shape)  # nombre de lignes et colonnes

cac = cac.reset_index()
cac["Date"] = cac["Date"].dt.strftime("%Y-%m-%d")
cac.to_json("C:/Users/Younes/OneDrive/Desktop/bourse_cac40.json", orient="records", indent=2)
print("Fichier exporté !")