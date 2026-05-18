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

---

### 2. Modèle ML — Régression polynomiale
Sur la base des données du CAC 40, on a entraîné une **régression polynomiale de degré 4** avec `scikit-learn` pour modéliser la tendance générale de la courbe.

**Score R² obtenu : 0.8385** — la tendance de fond est bien capturée.

Les coefficients du modèle sont exportés dans un fichier `cac40_model.json` pour être réutilisés plus tard (notamment dans Unity/C#).

---

### 3. Modélisation de la volatilité (à coder)
La courbe ML est volontairement lisse — elle capure la tendance mais pas les fluctuations mensuelles.

L'idée validée : calculer les **variations mensuelles historiques réelles** de chaque actif (ex: +3.2%, -1.8%...) et les stocker sous forme de distribution. En jeu, à chaque tour on tire aléatoirement une variation dans cette distribution et on l'applique autour de la courbe lisse.

```
Prix final = Courbe ML (tendance) + Bruit (volatilité historique)
```

Cela garantit une simulation réaliste sans dénaturer la tendance du modèle.

---

## Ce qui reste à faire

- [ ] Coder la volatilité mensuelle pour chaque actif
- [ ] Appliquer le modèle ML à tous les actifs (pas seulement le CAC 40)
- [ ] Scrapper les données immobilières (DVF)
- [ ] Scrapper les données de salaires (INSEE)
- [ ] Scrapper les headlines économiques (RSS)
- [ ] Démarrer le développement Unity

---

## Stack technique utilisée

| `yfinance` | Scrapping données financières |
| `pandas` | Manipulation et export des données |
| `scikit-learn` | Régression polynomiale |
| `matplotlib` | Visualisation des courbes (dev uniquement) |
| `os` | Chemins dynamiques pour la reproductibilité |

