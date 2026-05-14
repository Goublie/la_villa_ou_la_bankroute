import pandas as pd
import matplotlib.pyplot as plt

# Chargement du JSON
df = pd.read_json("C:/Users/Younes/OneDrive/Desktop/bourse_cac40.json")
df.columns = ["Date", "Close"]
df["Date"] = pd.to_datetime(df["Date"])

# Graphique
plt.figure(figsize=(14, 6))
plt.plot(df["Date"], df["Close"], color="#2196F3", linewidth=1)

plt.title("CAC 40 — Évolution 2015-2024", fontsize=16)
plt.xlabel("Date")
plt.ylabel("Prix de clôture (€)")
plt.grid(True, alpha=0.3)
plt.tight_layout()

plt.savefig("C:/Users/Younes/OneDrive/Desktop/cac40_graphique.png", dpi=150)
plt.show()
print("Graphique exporté !")