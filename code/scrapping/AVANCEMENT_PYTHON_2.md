# Avancement du projet — Simulation Économique

## Ce qui a été fait

### 1. Scrapping des données financières
Utilisation de la librairie Python `yfinance` pour récupérer les données historiques de plusieurs actifs financiers entre le **01/01/2015 et le 31/12/2024**.

Les actifs récupérés :
- CAC 40
- Or
- Nvidia
- Google
- Dollar (EUR/USD)
- Bitcoin
- TotalEnergies

Chaque actif est exporté dans un fichier `.json` propre avec deux colonnes : `Date` et `Close`.

**Problème rencontré :** `yfinance` génère des colonnes multi-niveaux (`('Date', '')`, `('Close', '^FCHI')`). Résolu en forçant le renommage avec `df.columns = ["Date", "Close"]`.

**Reproductibilité :** chemins dynamiques via `os.path.dirname(os.path.abspath(__file__))` pour que le code fonctionne sur n'importe quelle machine.

---

### 2. Nettoyage des données
Vérification effectuée sur le CAC 40 (référence) :
- ✅ 2559 lignes, aucune valeur manquante
- ✅ Aucune valeur aberrante
- ✅ Aucun doublon de dates

Tous les autres actifs provenant de la même source (`yfinance`) avec le même script, ils sont considérés propres.

---

### 3. Modèle ML — Simulation 40 ans avec CAGR + volatilité

**Approche initiale abandonnée — régression polynomiale pure :**
La régression polynomiale de degré 4 (R²=0.8385) modélise bien la tendance historique mais **diverge complètement hors de sa plage d'entraînement** (valeurs négatives ou astronomiques sur 40 ans). Augmenter le degré n'améliore pas le R² (surapprentissage).

**Approche retenue — CAGR + mean reversion + volatilité plafonnée :**

1. On calcule le **CAGR historique réel** de chaque actif sur 2015-2024
2. On converge progressivement vers un **CAGR long terme** plus réaliste sur 10 ans (mean reversion)
3. On ajoute une **volatilité mensuelle décroissante** avec le temps, plafonnée à ±10% par mois

```
Prix(t) = Prix_précédent × (1 + CAGR_effectif)^(1/12) × (1 + bruit_plafonné)
```

**CAGR historiques calculés :**
| Actif | CAGR historique | CAGR long terme retenu |
|---|---|---|
| CAC 40 | ~6% | 5% |
| Or | ~10% | 4% |
| Nvidia | 76% | 8% |
| Google | 22% | 7% |
| Dollar | -1.4% | -1.4% (conservé tel quel) |
| Bitcoin | 48% | 6% |
| TotalEnergies | 8% | 5% |

**Problèmes rencontrés :**
- Volatilité centrée sur `vol_mean` au lieu de 0 → accumulation exponentielle des erreurs → valeurs absurdes. Résolu en centrant sur 0 : `np.random.normal(0, vol_effective)`
- Explosion même avec mean reversion → volatilité plafonnée avec `np.clip(..., -0.10, 0.10)` et décroissante avec le temps

**Output :** 7 fichiers `{actif}_simulation.json` contenant 10 080 jours simulés (40 ans) chacun.

---

### 4. Setup GitHub
- `requirements.txt` — liste des librairies pour installation universelle
- `install_bib.bat` — double-clic Windows pour installer automatiquement
- Chemins dynamiques dans tous les scripts pour reproductibilité multi-OS

---

## Ce qui reste à faire

- [ ] Scrapper les données immobilières (DVF)
- [ ] Scrapper les données de salaires (INSEE)
- [ ] Scrapper les headlines économiques (RSS)
- [ ] Démarrer le développement Unity

---

## Stack technique utilisée

| Outil | Rôle |
|---|---|
| `yfinance` | Scrapping données financières |
| `pandas` | Manipulation et export des données |
| `numpy` | Calculs CAGR, volatilité, simulation |
| `scikit-learn` | Régression polynomiale (exploratoire) |
| `matplotlib` | Visualisation des courbes (dev uniquement) |
| `os` | Chemins dynamiques pour la reproductibilité |

---

## Structure des fichiers

```
projet/
├── scrapping.py
├── ml_model.py
├── requirements.txt
├── install_bib.bat
├── bourse_cac40.json
├── or.json
├── nvidia.json
├── google.json
├── dollar.json
├── bitcoin.json
├── totalenergies.json
├── bourse_cac40_simulation.json
├── or_simulation.json
├── nvidia_simulation.json
├── google_simulation.json
├── dollar_simulation.json
├── bitcoin_simulation.json
└── totalenergies_simulation.json
```
