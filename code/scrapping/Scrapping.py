import yfinance as yf
import pandas as pd
import os

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

# CAC 40
cac = yf.download("^FCHI", start="2015-01-01", end="2024-12-31")
cac = cac[["Close"]]
cac = cac.reset_index()
cac.columns = ["Date", "Close"] 
cac["Date"] = cac["Date"].dt.strftime("%Y-%m-%d")
cac.to_json(os.path.join(BASE_DIR, "bourse_cac40.json"), orient="records", indent=2)
print("CAC 40 exporté !")

# Nvidia
nvidia = yf.download("NVDA", start="2015-01-01", end="2024-12-31")
nvidia = nvidia[["Close"]].reset_index()
nvidia.columns = ["Date", "Close"]  #Nettoyage
nvidia["Date"] = nvidia["Date"].dt.strftime("%Y-%m-%d")
nvidia.to_json(os.path.join(BASE_DIR, "nvidia.json"), orient="records", indent=2)   #Export
print("Nvidia exporté !")

# Or
or_ = yf.download("GC=F", start="2015-01-01", end="2024-12-31")
or_ = or_[["Close"]].reset_index()
or_.columns = ["Date", "Close"]
or_["Date"] = or_["Date"].dt.strftime("%Y-%m-%d")
or_.to_json(os.path.join(BASE_DIR, "or.json"), orient="records", indent=2)
print("Or exporté !")

# Google
google = yf.download("GOOGL", start="2015-01-01", end="2024-12-31")
google = google[["Close"]].reset_index()
google.columns = ["Date", "Close"]
google["Date"] = google["Date"].dt.strftime("%Y-%m-%d")
google.to_json(os.path.join(BASE_DIR, "google.json"), orient="records", indent=2)
print("Google exporté !")

# Dollar (EUR/USD)
dollar = yf.download("EURUSD=X", start="2015-01-01", end="2024-12-31")
dollar = dollar[["Close"]].reset_index()
dollar.columns = ["Date", "Close"]
dollar["Date"] = dollar["Date"].dt.strftime("%Y-%m-%d")
dollar.to_json(os.path.join(BASE_DIR, "dollar.json"), orient="records", indent=2)
print("Dollar exporté !")

# Bitcoin
bitcoin = yf.download("BTC-USD", start="2015-01-01", end="2024-12-31")
bitcoin = bitcoin[["Close"]].reset_index()
bitcoin.columns = ["Date", "Close"]
bitcoin["Date"] = bitcoin["Date"].dt.strftime("%Y-%m-%d")
bitcoin.to_json(os.path.join(BASE_DIR, "bitcoin.json"), orient="records", indent=2)
print("Bitcoin exporté !")

# TotalEnergies
total = yf.download("TTE.PA", start="2015-01-01", end="2024-12-31")
total = total[["Close"]].reset_index()
total.columns = ["Date", "Close"]
total["Date"] = total["Date"].dt.strftime("%Y-%m-%d")
total.to_json(os.path.join(BASE_DIR, "totalenergies.json"), orient="records", indent=2)
print("TotalEnergies exporté !")