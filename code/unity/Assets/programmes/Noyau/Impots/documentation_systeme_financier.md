# Système d'Imposition — Documentation

## Ce qui a été fait

### DonneesImposition.cs
Classe de données fiscales cumulées pour une année fiscale.

**Champs :**
- `anneeFiscale` — année concernée
- `revenusSalariauxCumules` — salaires bruts perçus dans l'année
- `revenusLocatifsCumules` — loyers perçus dans l'année
- `beneficesEntrepriseCumules` — bénéfices nets de la startup
- `plusValuesBourseCumulees` — plus-values boursières réalisées
- `taxeFonciereAnnuelle` — impôt foncier annuel
- `impotPreleveALaSource` — total des impôts déjà déduits mensuellement
- `dureeDetentionParBien` — dictionnaire `bienId → mois` pour le calcul de la plus-value immo

**Propriété calculée :**
- `RevenuGlobalImposable` — salaire + 70% des loyers (abattement micro-foncier 30%) + bénéfices

**Méthode :**
- `ReinitialiserPourNouvelleAnnee(int)` — remet tous les cumuls à zéro en décembre

---

### ServiceImposition.cs
Service métier de calcul de l'impôt salarial mensuel.

**Barème progressif 2024 ramené au mois (en centimes) :**

| Tranche mensuelle | Taux |
|---|---|
| 0 → 94 116 cts (941 €) | 0% |
| 94 116 → 239 975 cts (2 400 €) | 11% |
| 239 975 → 686 175 cts (6 862 €) | 30% |
| 686 175 → 1 475 883 cts (14 759 €) | 41% |
| Au-delà | 45% |

**Méthode implémentée :**
- `CalculerImpotMensuel(long salaireBrut)` — applique directement les tranches sur le salaire brut en centimes, retourne un `ResultatPrelevement` contenant `salaireNet` et `impotPreleve`

**Choix techniques :**
- Tous les montants en **centimes** (`long`) pour éviter les erreurs d'arrondi des `float`
- Calcul **direct et mensuel** — pas de régularisation annuelle, pas d'estimation
- Tableau de tuples `(long seuilMensuel, float taux)` `static readonly` pour le barème

---

## Ce qui reste à faire

### ServiceImposition.cs
- [ ] `CalculerImpotActionsFlaTax(long plusValueNette)` — Flat Tax 30% sur les plus-values boursières annuelles
- [ ] `CalculerImpotStartup(long beneficeNet)` — IS 15% jusqu'à 42 500 €, 25% au-delà
- [ ] `CalculerImpotLocatif(long revenusLocatifs)` — barème progressif sur 70% des loyers
- [ ] `CalculerTaxeFonciere(long valeurBien)` — impôt annuel forfaitaire sur les biens possédés
- [ ] `CalculerPlusValueImmo(long prixAchat, long prixVente, int moisDetention)` — 19% IR + 17.2% PS avec abattements dégressifs (exonération IR à 22 ans, PS à 30 ans)

### Intégration
- [ ] Modifier `ServicePassageMensuel` — appeler `CalculerImpotMensuel` à chaque versement de salaire, débiter l'impôt et créditer uniquement le net
- [ ] Modifier `DonneesJoueur` — ajouter `public DonneesImposition imposition`
- [ ] Déclencher `taxeFonciere` une fois par an pour chaque bien possédé
- [ ] Déclencher `CalculerPlusValueImmo` lors de la vente d'un bien immobilier
- [ ] Déclencher `CalculerImpotActionsFlaTax` lors de la vente d'actions

### UI
- [ ] `ImpositionUI.cs` — affichage des tranches, taux marginal et taux moyen
- [ ] Panneau "Avis d'imposition" dans le bilan annuel

### Tests
- [ ] `ArchitectureImpositionTests.cs` — tester les tranches avec des salaires représentatifs
- [ ] Tester la Flat Tax boursière
- [ ] Tester le calcul de plus-value immo selon différentes durées de détention
