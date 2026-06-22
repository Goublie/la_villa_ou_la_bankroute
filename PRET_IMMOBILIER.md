# Prêt Immobilier — Formule du Taux

## Formule complète

```
taux_pret = taux_BCE + marge_banque + prime_anciennete + prime_salaire + prime_duree
```

---

## Détail de chaque composante

### taux_BCE
Lu dans `taux_bce.json` selon l'année de jeu courante.
Généré aléatoirement entre **0% et 4%** pour chacune des 40 années (seed fixe = reproductible).

---

### marge_banque
```
1.5% — fixe, jamais modifiée
```

---

### prime_anciennete (années de jeu écoulées)
```
0  → 5  ans : +1.0%    ← débutant, peu de stabilité financière
5  → 15 ans : +0.5%    ← profil qui se stabilise
15 → 30 ans :  0.0%    ← profil solide, historique financier établi
30 → 40 ans : +0.3%    ← proche fin de carrière, banque plus prudente
```

---

### prime_salaire (salaire annuel du joueur)
```
< 20 000€          : +1.0%    ← revenus faibles, risque élevé
20 000 → 40 000€   : +0.5%
40 000 → 70 000€   :  0.0%    ← profil idéal
> 70 000€          : -0.3%    ← bon profil, légère réduction
```

---

### prime_duree (durée du prêt choisie)
```
3  ans : -0.5%    ← très court, risque minimal pour la banque
5  ans :  0.0%    ← référence
10 ans : +0.3%
15 ans : +0.6%
20 ans : +1.0%    ← long terme, risque maximal
```

---

## Exemples

```
Année 3,  BCE = 2.1%, salaire 25 000€, durée 20 ans
→ 2.1 + 1.5 + 1.0 + 0.5 + 1.0 = 6.1%

Année 20, BCE = 1.5%, salaire 55 000€, durée 5 ans
→ 1.5 + 1.5 + 0.0 + 0.0 + 0.0 = 3.0%

Année 8,  BCE = 3.0%, salaire 18 000€, durée 15 ans
→ 3.0 + 1.5 + 0.5 + 1.0 + 0.6 = 6.6%
```

---

## Durées disponibles

| Durée | Prime |
|---|---|
| 3 ans | -0.5% |
| 5 ans | 0.0% |
| 10 ans | +0.3% |
| 15 ans | +0.6% |
| 20 ans | +1.0% |
