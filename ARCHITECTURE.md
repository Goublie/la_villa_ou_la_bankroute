# Architecture du Projet — La Villa ou la Bankroute

> Document généré le 19 juin 2026. Décrit l'intégralité de la base de code telle qu'elle existe à cette date.

---

## Table des matières

1. [Vue d'ensemble](#1-vue-densemble)
2. [Structure du dépôt](#2-structure-du-dépôt)
3. [Couche Données / Scrapping Python](#3-couche-données--scrapping-python)
4. [Projet Unity — organisation générale](#4-projet-unity--organisation-générale)
5. [Noyau](#5-noyau-noyau)
6. [Applications](#6-applications-apps)
7. [Rétrospective](#7-rétrospective-retrospection)
8. [UI générique](#8-ui-générique-ui)
9. [Misc](#9-misc-misc)
10. [Scènes Unity](#10-scènes-unity)
11. [Tests automatisés](#11-tests-automatisés)
12. [Assets non-code](#12-assets-non-code)
13. [Flux de données principaux](#13-flux-de-données-principaux)
14. [Bugs connus](#14-bugs-connus)

---

## 1. Vue d'ensemble

**La Villa ou la Bankroute** est un serious-game éducatif de simulation financière développé sous **Unity (C#)**. Le joueur gère ses finances personnelles mois par mois : salaire, impôts, épargne, bourse, entrepreneuriat. L'objectif pédagogique est de comprendre les mécanismes réels de l'économie personnelle française.

### Philosophie d'architecture

| Principe | Description |
|---|---|
| **Séparation UI / Domaine** | Aucun MonoBehaviour dans la couche métier. Les services et données sont de purs objets C# testables. |
| **Agrégat racine** | `GameData` (ScriptableObject) est le seul point d'entrée partagé entre scènes. |
| **Snapshots immuables** | Chaque fin de mois est photographiée dans `SnapshotEtatJeu` pour le mode rétrospectif et le simulateur What If. |
| **Passage mensuel déterministe** | `ServicePassageMensuel` orchestre la clôture et l'ouverture dans un ordre garanti. |
| **Argent en centimes** | Le struct `argent` stocke toutes les valeurs en centimes entiers pour éviter les erreurs flottantes. |

---

## 2. Structure du dépôt

```
la_villa_ou_la_bankroute/
│
├── ARCHITECTURE.md          ← Ce fichier
├── README.md
├── requirements.txt         ← Dépendances Python (scrapping)
├── install_bib.bat          ← Script d'installation des libs Python
│
├── code/
│   ├── unity/               ← Projet Unity complet
│   └── scrapping/           ← Scripts Python de collecte de données
│
├── docs/
│   ├── ARCHITECTURE_JEU.md  ← Documentation architecture (ancienne)
│   └── CONSOLIDATION_AUDIT.md
│
└── fichiers/
    ├── gdd_jeu.md           ← Game Design Document complet
    ├── BRAINSTORMING.md
    └── marketing/
```

---

## 3. Couche Données / Scrapping Python

Répertoire : `code/scrapping/`

Cette couche est **indépendante d'Unity**. Elle alimente le jeu avec des données économiques réelles françaises.

### Scripts de collecte

| Fichier | Rôle |
|---|---|
| `Scrapping.py` | Point d'entrée général |
| `Scrapping_Actifs.py` | Cours historiques d'actifs financiers (CAC40, etc.) |
| `Scrapping_Inflation.py` | Données INSEE sur l'inflation |
| `Scrapping_Livret_A_Historique.py` | Taux historiques du Livret A |
| `Extraction_Immo_Villes_DVFPlus.py` | Prix immobiliers par ville (données DVF+) |

### Modèles de projection

| Fichier | Rôle |
|---|---|
| `Modele_Actifs_CAGR.py` | Calcul du CAGR (taux de croissance annuel composé) pour les actifs |
| `Modele_Immo_CAGR.py` | Modèle de projection immobilière par ville |
| `Modele_Livret_A_Simplifie_40ans.py` | Simulation Livret A sur 40 ans |

### Visualisation

| Fichier | Rôle |
|---|---|
| `Visualisation_Courbes_Actifs.py` | Graphiques d'évolution des actifs |
| `Visualisation_Immo_Simulation.py` | Courbes de simulation immobilière |
| `Visualisation_Immo_Villes.py` | Comparatif prix par ville |
| `Visualisation_Livret_A_Simplifie.py` | Courbe Livret A simplifiée |

### Répertoires de données générées

```
scrapping/
├── ACTIFS/       ← CSV / JSON des cours d'actifs
├── IMMOBILIER/   ← Données prix immobilier
├── INFLATION/    ← Séries inflation
├── LIVRET_A/     ← Historique taux Livret A
├── GRAPHIQUES/   ← Images générées
├── PREDICTIONS/  ← Modèles de prédiction exportés
└── AVANCEMENTS/  ← Journaux de progression
```

---

## 4. Projet Unity — organisation générale

Répertoire : `code/unity/`

- **Moteur** : Unity (version LTS)
- **Langage** : C# (.NET Standard)
- **Bibliothèques tierces** : XCharts (graphiques), TextMesh Pro, AI Toolkit, MobileDependencyResolver
- **Solution** : `unity.slnx` avec 5 projets C# (Assembly-CSharp, Editor, XCharts x3)

### Arborescence principale des scripts

```
Assets/programmes/
├── Noyau/           ← Domaine métier pur (sans MonoBehaviour)
├── Apps/            ← Logique et UI de chaque module fonctionnel
│   ├── Banque/
│   ├── Bourse/
│   ├── Salariat/
│   ├── Entrepreneuriat/
│   ├── Actualites/
│   └── RepartitionTemps/
├── Retrospection/   ← Mode rétrospectif et simulateur What If
├── UI/              ← Composants UI génériques réutilisables
└── Misc/            ← Utilitaires globaux (son, luminosité, événements)
```

---

## 5. Noyau (`Noyau/`)

Le noyau contient **toute la logique métier** sans dépendance Unity (sauf `argent.cs` qui utilise `Mathf`).

### Agrégat racine

#### `GameData.cs` — ScriptableObject racine
- Partagé entre toutes les scènes via Unity Asset
- Contient : `DonneesJoueur`, `DonneesEnvironnement`, `nombreMoisPasses`, `moisActuel`, `historiqueSnapshots`
- Méthode `ResetData()` : remet le jeu à l'état initial (juillet, solde = 1 000 €)
- Enum `Mois` défini dans ce fichier (Janvier → Décembre)

#### `DonneesJoueur.cs` — Agrégat joueur
Classe sérialisable contenant **toutes les données persistantes** du joueur :

| Champ | Type | Rôle |
|---|---|---|
| `comptes` | `Dictionary<string, CompteBanquaire>` | Comptes bancaires (clés : "courant", "epargne") |
| `salaire` | `argent` | Salaire mensuel brut en centimes |
| `energie` | `int` | Énergie (0–100) |
| `santeMentale` | `int` | Santé mentale (0–100) |
| `investissements` | `List<Investissement>` | Placements fixes autonomes |
| `bourse` | `DonneesBourse` | Portefeuille boursier |
| `entrepreneuriat` | `DonneesEntrepreneuriat` | État de l'entreprise |
| `salariat` | `DonneesSalariat` | Parcours salarié |
| `tempsApplications` | `DonneesRepartitionTemps` | Répartition du temps mensuel |

Méthodes clés : `InitialiserSiNecessaire()`, `CalculPatrimoineTotal()`, `Copier()` (copie profonde pour les snapshots).

#### `DonneesEnvironnement.cs`
Variables économiques externes : taux d'épargne Livret A courant.

### Type fondamental

#### `argent.cs` — Struct monétaire
- Stocke les valeurs en **centimes entiers** (`int centimes`)
- `argent(int centimes)` et `argent(float euros)` (arrondi via `Mathf.RoundToInt`)
- Opérateurs : `+`, `-`, `-` (unaire), `*` (float), `>`, `<`, `>=`, `<=`, `==`, `!=`
- `ToString()` : formate en euros avec 2 décimales + symbole €

#### `Investissement.cs`
Placement financier à rendement fixe, implémente `IEvolutionMensuelle`.

### Contrats (interfaces)

#### `Contrats/IEvolutionMensuelle.cs`
Interface implémentée par tous les systèmes appliqués en fin de mois :
`ServiceSalariat`, `ServiceBourse`, `ServiceEntrepreneuriat`, `Epargne`, `Investissement`

#### `IPatrimoine.cs`
Interface exposant `GetValeurPatrimoine()` → `argent`. Implémentée par `CompteBanquaire`.

### Services du noyau

#### `Temps/ServicePassageMensuel.cs` — Orchestrateur mensuel
**Point central du jeu.** Gère la transition entre deux mois dans un ordre déterministe :

```
PasserAuMoisSuivant()
  1. AppliquerEvolutionsCloture(mois)
     ├── Intérêts Livret A
     ├── Évolutions des investissements
     ├── ServiceBourse.AppliquerEvolutionMensuelle()
     ├── ServiceSalariat.AppliquerEvolutionMensuelle()  ← crédite le salaire
     └── ServiceEntrepreneuriat.AppliquerEvolutionMensuelle()
  2. EnregistrerSnapshot()
  3. Incrémenter calendrier (moisActuel, nombreMoisPasses)
  4. OuvrirNouveauMois()
     ├── ViderHistorique() sur tous les comptes
     ├── Créditer salaire brut (libellé "salaire")
     ├── Débiter impôt (ServiceImposition)
     └── ServiceRepartitionTemps.ReinitialiserAllocation()
```

> ⚠️ Le salaire est crédité deux fois : dans ServiceSalariat ET dans OuvrirNouveauMois. Voir section Bugs.

#### `Temps/ServiceRepartitionTemps.cs`
Gère la réinitialisation mensuelle de l'allocation de temps entre applications.

#### `Temps/DonneesRepartitionTemps.cs`
Données sérialisables de la répartition du temps (heures par app).

#### `Temps/ResultatPassageMensuel.cs`
DTO retourné par `PasserAuMoisSuivant()` : index clôture, index ouverture, flag changement d'année.

#### `Impots/ServiceImposition.cs`
Calcule l'impôt mensuel par prélèvement à la source selon les tranches progressives françaises.
- Méthode : `CalculerImpotMensuel(int salaireBrutCentimes)` → `ResultatPrelevement`
- Applique la déduction forfaitaire de 10 % avant le calcul des tranches

#### `Patrimoine/ServicePatrimoine.cs`
Calcule le patrimoine net total du joueur en agrégeant tous les comptes, sans double-comptage du Livret A.

#### `Resultats/ResultatOperation.cs`
DTO générique : `Reussite(message, montant, code)` / `Echec(message, code)`.

#### `Evenements/ServiceEvenementsEconomiques.cs`
Génère des événements économiques aléatoires pouvant affecter le marché ou le joueur.

#### `HUDManager.cs`
MonoBehaviour gérant le HUD in-game (mois, énergie, santé mentale, patrimoine).

#### `ActionPlay.cs`
MonoBehaviour gérant le bouton "Passer le mois". Déclenche l'événement statique `OnMoisPasse` après `ServicePassageMensuel.PasserAuMoisSuivant()`.

#### `AudioManager.cs`
Singleton gérant la musique de fond et les effets sonores.

#### `ScenesManager.cs`
Gère les transitions de scènes (Menu → Jeu → GameOver → Rétrospective).

---

## 6. Applications (`Apps/`)

### 6.1 Banque

Répertoire : `Apps/Banque/`

#### Modèles de données

| Fichier | Rôle |
|---|---|
| `CompteBanquaire.cs` | Compte monétaire avec historique signé, totaux entrée/sortie, solde. Méthodes : `AjoutHistorique()`, `ViderHistorique()`, `Transferer()`, `Copier()`. |
| `Epargne.cs` | Sous-classe de `CompteBanquaire` pour le Livret A avec intérêts composés mensuels. |
| `Historique.cs` | Liste de `Transaction` avec `Add()`, `ModifieOuAjoute()` (pour sliders), `Clear()`, `Copier()`. |
| `Transaction.cs` | Paire `(libelle: string, montant: argent)`. Sérialisable. |
| `SnapshotEtatJeu.cs` | Photographie profonde de `GameData` à un instant T. |

#### Services

| Fichier | Rôle |
|---|---|
| `Services/ServiceBanque.cs` | Crée et retourne les comptes. Expose `Crediter()`, `Debiter()`, `Transferer()`. Solde initial : 1 000 €. |
| `Services/ServiceLivretA.cs` | Calcule le taux annuel du Livret A en fonction du mois absolu. |

#### UI

| Fichier | Rôle |
|---|---|
| `CourantUI.cs` | Affiche le solde et l'historique du compte courant. |
| `EpargneUI.cs` | Affiche le solde et les intérêts projetés du Livret A. |
| `sliderAvecTexte.cs` | Slider de dépense mensuelle (loyer, style de vie...). Écrit dans l'historique via `ModifieOuAjoute()`. |
| `GestionnaireSlides.cs` | Orchestrateur de plusieurs `SliderAvecTexte`. |
| `PrevisionLivretA.cs` | Calcule et affiche la projection de croissance du Livret A. |

### 6.2 Bourse

Répertoire : `Apps/Bourse/`

| Fichier | Rôle |
|---|---|
| `DonneesBourse.cs` | État du portefeuille : positions ouvertes, valorisation, historique des ordres. |
| `Modeles/DefinitionActifFinancier.cs` | Définition statique d'un actif (nom, ticker, CAGR, volatilité). |
| `Modeles/CatalogueActifs.cs` | Registre de tous les actifs disponibles à l'achat. |
| `Services/ServiceBourse.cs` | Implémente `IEvolutionMensuelle`. Simule les variations de cours, calcule plus/moins-values. |
| `BourseUI.cs` | Interface principale (28 Ko) : liste des actifs, achat/vente, portefeuille. |
| `MarcheBoursier.cs` | Affichage du marché en temps réel. |
| `UI/GraphiqueBourseUI.cs` | Graphique XCharts d'évolution d'un actif. |

### 6.3 Salariat

Répertoire : `Apps/Salariat/`

| Fichier | Rôle |
|---|---|
| `Donnees/DonneesSalariat.cs` | Poste actuel, salaire, expérience, satisfaction, réseau professionnel. |
| `Services/ServiceSalariat.cs` | Implémente `IEvolutionMensuelle`. Crédite le salaire, gère l'évolution de carrière. |
| `SalariatUI.cs` | Panneau principal de l'onglet Salariat. |
| `Logic_RechercheEmploi.cs` | Recherche d'emploi et filtrage des offres. |
| `JobOfferController.cs` | Affiche une offre d'emploi individuelle. |
| `InterviewPanelController.cs` | Mini-jeu d'entretien d'embauche. |
| `EmployeePerformanceController.cs` | Suivi de la performance mensuelle (10 Ko). |
| `FormationController.cs` | Gestion des formations professionnelles. |
| `DemissionController.cs` | Gère la démission et ses conséquences financières. |
| `JobSatisfactionController.cs` | Calcule et affiche l'indice de satisfaction au travail. |
| `NetworkingController.cs` | Système de networking professionnel. |
| `RelationalController.cs` | Gestion des relations avec collègues et hiérarchie. |
| `TravaillerPlusController.cs` | Heures supplémentaires et impact énergie/santé. |

### 6.4 Entrepreneuriat

Répertoire : `Apps/Entrepreneuriat/`

| Fichier | Rôle |
|---|---|
| `Donnees/DonneesEntrepreneuriat.cs` | Capital, chiffre d'affaires, charges, projet en cours. |
| `Modeles/ProjetEntrepreneurial.cs` | Définition d'un projet (durée, coût, revenus attendus). |
| `Modeles/CatalogueEntrepreneuriat.cs` | Liste de tous les projets disponibles. |
| `Services/ServiceEntrepreneuriat.cs` | Implémente `IEvolutionMensuelle`. Gère le cycle de vie de l'entreprise (28 Ko). |
| `EntrepreneuriatUI.cs` | Interface principale : choix de projet, tableau de bord, bilan mensuel. |

### 6.5 Actualités

Répertoire : `Apps/Actualites/`

| Fichier | Rôle |
|---|---|
| `ActualitesUI.cs` | Affiche les actualités économiques contextuelles influençant le marché. |
| `ScriptableObjects/ActualiteSO.cs` | ScriptableObject définissant une actualité (titre, contenu, impact). |

### 6.6 Répartition du temps

Répertoire : `Apps/RepartitionTemps/`

Le joueur doit allouer son temps mensuel entre les différentes applications avant de passer au mois suivant.

| Fichier | Rôle |
|---|---|
| `RepartitionTempsUI.cs` | Interface principale des sliders de répartition (10 Ko). |
| `PhaseRepartitionTempsController.cs` | Contrôle le flux de répartition (validation, verrouillage). |
| `SliderTempsAvecValeur.cs` | Slider spécialisé affichant les heures allouées à une app. |

---

## 7. Rétrospective (`Retrospection/`)

Module d'analyse post-mois donnant accès à l'historique et au simulateur "What If".

| Fichier | Rôle |
|---|---|
| `Optimizer.cs` | Simulateur What If : rejoue l'historique avec des paramètres modifiés pour comparer des scénarios. |
| `RetrospectionEvenements.cs` | Affiche les événements économiques survenus dans les mois précédents. |
| `RetrospectionGraphiqueUI.cs` | Graphique XCharts de l'évolution du patrimoine dans le temps. |
| `RetrospectionTableauUI.cs` | Tableau des transactions passées mois par mois. |
| `FermerRetrospective.cs` | Bouton de retour vers la scène principale. |

---

## 8. UI générique (`UI/`)

Composants UI réutilisables dans toutes les applications.

| Fichier | Rôle |
|---|---|
| `Tableau.cs` | Tableau dynamique de lignes (ajout, mise à jour, suppression). |
| `Ligne.cs` | Une ligne du tableau avec libellé et montant coloré (+/−). |
| `Case.cs` | Cellule individuelle d'une ligne de tableau. |
| `TableauScroll.cs` | Conteneur scrollable pour `Tableau`. |
| `ToggleImageSwitcher.cs` | Bascule entre deux images selon l'état d'un Toggle Unity. |

---

## 9. Misc (`Misc/`)

| Fichier | Rôle |
|---|---|
| `Luminosite_globale.cs` | Contrôle la luminosité globale de l'écran. |
| `Son_Luminosite.cs` | Panneau de paramètres son + luminosité. |
| `GameEvent.cs` | Stub vide — non utilisé actuellement. |
| `TimerFenetre.cs` (dans Apps/) | Gère le minuteur des fenêtres d'application. |

---

## 10. Scènes Unity

Répertoire : `Assets/Scenes/`

| Scène | Rôle |
|---|---|
| `Menu.unity` | Écran d'accueil : Jouer, Options, Quitter. Réinitialise `GameData.ResetData()`. |
| `Jeu.unity` | Scène principale (460 Ko). Toutes les applications, le HUD, le passage mensuel. |
| `Retrospective.unity` | Mode rétrospectif : graphiques, tableau des mois passés, simulateur What If. |
| `GameOver.unity` | Écran de fin de partie (solde < 0 ou énergie = 0). |

### Flux de navigation entre scènes

```
Menu
 └── [Jouer] ──────────────────────────► Jeu
                                          ├── [Passer le mois] → (reste sur Jeu)
                                          ├── [Rétrospective] ──► Retrospective
                                          │                         └── [Retour] → Jeu
                                          └── [Game Over] ────────► GameOver
                                                                      └── [Rejouer] → Menu
```

---

## 11. Tests automatisés

Répertoire : `Assets/Tests/`

### Tests d'éditeur (NUnit, sans Unity runtime)

Répertoire : `Tests/Editor/`

| Fichier | Ce qu'il teste |
|---|---|
| `ArchitectureBanqueTests.cs` | `CompteBanquaire`, `ServiceBanque`, `Historique`, `ViderHistorique` |
| `ArchitectureBourseTests.cs` | `ServiceBourse`, `DonneesBourse`, calcul de plus-values |
| `ArchitectureEntrepreneuriatTests.cs` | `ServiceEntrepreneuriat`, cycle de vie d'un projet |
| `ArchitectureSalariatTests.cs` | `ServiceSalariat`, négociation, évolution de carrière |
| `ArchitectureTempsTests.cs` | `ServicePassageMensuel`, `ServiceRepartitionTemps`, snapshots |
| `ArchitectureRetrospectiveTests.cs` | `Optimizer`, snapshots, mode What If |

### Tests Play Mode (avec Unity runtime)

Répertoire : `Tests/PlayMode/`

| Fichier | Ce qu'il teste |
|---|---|
| `RepartitionTempsPlayModeTests.cs` | Flux complet de répartition du temps avec MonoBehaviours |

---

## 12. Assets non-code

### Fonts
Polices personnalisées pour l'interface (style rétro/terminal).

### Image
Sprites et textures UI (icônes d'applications, arrière-plans).

### Musique (`musique/`)
Musiques de fond et effets sonores gérés par `AudioManager`.

### Prefabs (`prefabs/`)

```
prefabs/
├── Apps/           ← Prefabs des fenêtres d'application
├── Noyau/          ← Prefabs système (HUD, TimerFenetre)
├── Retrospection/  ← Prefabs de la rétrospective
└── UI/             ← Prefabs des composants UI génériques
```

### Resources
Assets chargés dynamiquement via `Resources.Load()`.

### Vidéo
`Video-Project-2.mp4` — vidéo de fond animée du bureau (`Video_fond.cs`).

---

## 13. Flux de données principaux

### Flux passage mensuel

```
[Bouton "Passer le mois"]
         │
         ▼
ActionPlay.cs
         │ ServicePassageMensuel.PasserAuMoisSuivant()
         ▼
┌──────────────────────────────────────────┐
│ AppliquerEvolutionsCloture()             │
│   ├── Investissements.AppliquerEvolution │
│   ├── Epargne (Livret A) intérêts        │
│   ├── ServiceBourse.AppliquerEvolution   │
│   ├── ServiceSalariat → Crediter salaire │ ← (1ère fois)
│   └── ServiceEntrepreneuriat             │
├──────────────────────────────────────────┤
│ EnregistrerSnapshot()                    │
│   └── new SnapshotEtatJeu(gameData)      │
├──────────────────────────────────────────┤
│ Incrémenter calendrier                   │
├──────────────────────────────────────────┤
│ OuvrirNouveauMois()                      │
│   ├── ViderHistorique() sur tous comptes │
│   ├── Crediter(salaire brut)             │ ← (2ème fois — BUG)
│   ├── ServiceImposition → Debiter(impôt) │
│   └── ReinitialiserAllocation()          │
└──────────────────────────────────────────┘
         │
         ▼
ActionPlay.OnMoisPasse (événement statique)
         │
         ▼
 SliderAvecTexte.RecupSolde()
 CourantUI.Rafraichir()
 EpargneUI.Rafraichir()
 HUDManager.Rafraichir()
 ...
```

### Flux d'achat en bourse

```
BourseUI → ServiceBourse.AcheterActif()
         → ServiceBanque.Debiter(compte courant, prix)
         → DonneesBourse.AjouterPosition(actif, quantité)
```

### Flux snapshot / rétrospective

```
GameData.historiqueSnapshots (List<SnapshotEtatJeu>)
         │
         ├── RetrospectionGraphiqueUI ← graphique patrimoine
         ├── RetrospectionTableauUI   ← tableau mensuel
         └── Optimizer                ← clone + rejeu What If
```

---

## 14. Bugs connus

### Bug critique — Double crédit du salaire

**Fichiers** : `ServicePassageMensuel.cs` lignes 122–124 et 156–176

`ServiceSalariat.AppliquerEvolutionMensuelle()` crédite le salaire dans `AppliquerEvolutionsCloture()`, puis `OuvrirNouveauMois()` le crédite une deuxième fois. Résultat : **double salaire chaque mois**.

C'est la cause principale du solde aberrant visible à l'ouverture si des données corrompues persistent dans le ScriptableObject entre deux sessions Unity Editor.

**Correction** : supprimer le crédit dans `ServiceSalariat.AppliquerEvolutionMensuelle()` OU dans `OuvrirNouveauMois()`, pas les deux.

---

### Bug — Dépenses sliders non persistantes entre les mois

**Fichier** : `sliderAvecTexte.cs`

Les sliders (loyer, style de vie...) écrivent dans `Historique` via `ModifieOuAjoute()`. Mais `ViderHistorique()` efface tout à chaque mois. Les sliders ne se rééditent que sur `onValueChanged` : si la valeur ne change pas, la dépense disparaît.

**Correction** : stocker les valeurs de slider dans `DonneesJoueur` et les réappliquer dans `OuvrirNouveauMois()`.

---

### Bug — Négociation salariale illimitée

**Fichier** : `ServiceSalariat.cs`

Un joueur avec expérience > 70 peut cliquer infiniment sur "Négocier" et gagner +600 € à chaque clic, sans cooldown ni consommation d'expérience.

---

### Bug — Simulateur What If inexact

**Fichier** : `Optimizer.cs`

Seules les transactions libellées "courant vers epargne" sont exclues des dépenses. Les achats boursiers sont traités comme des pertes de consommation. Les ventes d'actifs ne sont pas reconnues comme revenus.

---

*Fin du document ARCHITECTURE.md*
