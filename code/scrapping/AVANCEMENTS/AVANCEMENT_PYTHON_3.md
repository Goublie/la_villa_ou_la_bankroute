# AVANCEMENT_PYTHON_3 — Avancée du module Python : scrapping, simulation financière et choix de modélisation

## 1. Objectif général de cette partie Python

L’objectif de cette partie du projet était de construire un premier moteur économique pour le jeu **La villa ou la banqueroute**.  
L’idée n’était pas simplement de récupérer des données financières, mais de créer une base de simulation crédible, exploitable ensuite dans Unity.

Le but était donc de produire des courbes d’évolution sur plusieurs dizaines d’années pour différents actifs financiers, afin de permettre au joueur d’investir dans des supports variés :

- le CAC 40 ;
- l’or ;
- Nvidia ;
- Google ;
- le dollar ;
- le Bitcoin ;
- TotalEnergies.

Ces actifs n’ont pas été choisis au hasard. Ils représentent chacun une catégorie économique différente : marché boursier classique, valeur refuge, tech, devise, crypto-actif, énergie fossile/transition énergétique. Cela permet au jeu d’avoir une économie plus riche qu’un simple système d’épargne linéaire.

---

## 2. Première étape : scrapping des données financières

Nous avons commencé par créer un fichier Python de scrapping nommé :

```txt
Scrapping_Actifs.py
```

Ce fichier utilise la bibliothèque `yfinance` pour récupérer les données historiques des actifs financiers.  
Pour chaque actif, nous avons choisi de récupérer principalement la colonne `Close`, c’est-à-dire le prix de clôture.

La première version du script avait pour objectif de rester simple :

```txt
ticker financier → téléchargement yfinance → extraction Date / Close → export JSON
```

Les données ont été exportées sous forme de fichiers `.json`, par exemple :

```txt
bourse_cac40.json
bitcoin.json
google.json
nvidia.json
or.json
dollar.json
totalenergies.json
```

Chaque fichier contient des données de la forme :

```json
{
  "Date": "2024-01-01",
  "Close": 123.45
}
```

Ce choix du JSON a été fait pour préparer l’intégration Unity. En effet, Unity et C# peuvent facilement lire des fichiers JSON, ce qui rend cette structure plus pratique qu’un fichier Excel ou CSV pour un jeu.

---

## 3. Nettoyage et premiers contrôles

Après le scrapping, nous avons vérifié que les données étaient utilisables :

- pas de valeurs manquantes importantes ;
- pas de doublons ;
- dates correctement triées ;
- valeurs de prix positives ;
- structure identique pour tous les actifs.

Un premier contrôle avait notamment été fait sur le CAC 40, qui servait de référence. Les données étaient propres, ce qui a permis de considérer que le script de scrapping produisait une base exploitable.

Cependant, un problème technique est apparu avec `yfinance` : selon les actifs, les colonnes pouvaient parfois être retournées sous forme de colonnes multi-niveaux. Nous avons donc ajouté une fonction spécifique pour récupérer correctement la colonne `Close`, même lorsque `yfinance` ne renvoie pas exactement la structure attendue.

---

## 4. Première tentative : régression polynomiale

Au début, nous avons tenté une approche plus “Machine Learning” classique avec une régression polynomiale.

L’idée était de prendre les données historiques d’un actif, puis d’ajuster une courbe mathématique capable de prolonger son évolution dans le futur.

Nous avions testé une régression polynomiale de degré 4. Sur les données historiques, le résultat semblait correct : le modèle obtenait un bon score de type R² et suivait assez bien la tendance passée.

Mais en projetant cette courbe sur 40 ans, le modèle a montré un gros défaut : il extrapolait très mal.

Le problème était le suivant :

```txt
La courbe collait assez bien au passé,
mais devenait irréaliste dans le futur.
```

Certains actifs pouvaient exploser artificiellement ou prendre des valeurs absurdes. Cette étape a donc été un échec utile : elle nous a montré qu’un modèle mathématiquement séduisant n’est pas forcément adapté à une simulation de jeu.

Nous avons alors décidé d’abandonner la régression polynomiale comme modèle principal.

---

## 5. Changement de stratégie : simulation par CAGR et volatilité

Après cet échec, nous avons changé d’approche.

Au lieu de chercher à prédire exactement le futur avec une courbe de régression, nous avons construit une simulation stochastique contrôlée.

Le nouveau fichier principal est devenu :

```txt
Modele_ML_CAGR.py
```

Même si le nom contient “ML”, le modèle actuel est plutôt une simulation financière calibrée sur des données historiques qu’un vrai modèle de machine learning.

Le modèle repose sur trois éléments :

```txt
1. le CAGR historique ;
2. un retour progressif vers un CAGR long terme ;
3. une volatilité mensuelle inspirée des données réelles.
```

Le CAGR représente le taux de croissance annuel moyen d’un actif.  
La volatilité permet d’ajouter de l’incertitude et des variations réalistes.

L’équation générale de simulation est devenue :

```txt
prix suivant = prix actuel × croissance mensuelle × bruit de volatilité
```

Ce choix est beaucoup plus adapté au jeu, car il permet d’obtenir des courbes crédibles, contrôlables, et pas seulement des extrapolations aveugles.

---

## 6. Premier problème : les faux jours simulés

Dans une première version du modèle, nous simulions une valeur mensuelle, mais nous la répétions sur 21 jours.

Le fichier contenait donc des lignes comme :

```json
{
  "Jour": 0,
  "Close": 7382.77
}
```

puis le même prix était répété pendant 21 jours.

Cela créait un problème conceptuel : le fichier semblait journalier, mais la simulation était en réalité mensuelle. Visuellement, cela produisait des courbes en escaliers.

Nous avons donc corrigé ce point en passant à un vrai format mensuel :

```json
{
  "Mois": 0,
  "Close": 7382.77
}
```

La simulation sur 40 ans contient maintenant :

```txt
40 ans × 12 mois = 480 mois
```

avec un point initial en plus :

```txt
Mois 0 à Mois 480
```

Ce format est beaucoup plus propre pour Unity et beaucoup plus cohérent avec le rythme économique prévu pour le jeu.

---

## 7. Correction du point de départ des simulations

Un autre problème a été détecté : dans l’ancienne version, la simulation appliquait une première variation avant d’enregistrer le premier point.

Cela signifiait que :

```txt
Mois 0 ≠ dernier prix réel connu
```

La simulation pouvait donc commencer avec un saut artificiel.

Nous avons corrigé cela pour que :

```txt
Mois 0 = dernier prix historique scrappé
```

Puis seulement ensuite, à partir du mois 1, le modèle applique la croissance et la volatilité.

C’est important, car cela rend la transition entre la courbe réelle et la courbe simulée beaucoup plus propre.

---

## 8. Correction du calcul du CAGR

Au départ, le CAGR était calculé avec une hypothèse de 252 jours par an, ce qui est classique pour les marchés boursiers.

Mais cela posait problème pour certains actifs, notamment le Bitcoin, qui cote aussi les week-ends.

Nous avons donc corrigé le calcul pour utiliser la vraie durée calendrier entre la première et la dernière date :

```python
nb_annees = (date_fin - date_depart).days / 365.25
```

Cette correction rend le modèle plus robuste, car elle fonctionne pour tous les actifs :

- actions ;
- indices ;
- or ;
- devises ;
- Bitcoin.

---

## 9. Correction du dollar

Un autre point important concernait le dollar.

Le ticker utilisé dans `yfinance` était :

```txt
EURUSD=X
```

Ce ticker représente :

```txt
1 euro = X dollars
```

Or, pour un jeu français/européen, il est plus logique de représenter :

```txt
1 dollar = X euros
```

Nous avons donc inversé la valeur :

```python
df["Close"] = 1 / df["Close"]
```

Le fichier `dollar.json` représente maintenant le dollar en euros, ce qui est plus cohérent pour l’économie du jeu.

Nous avons aussi augmenté la précision du dollar à 4 décimales, car un arrondi à 2 décimales était trop brutal pour une devise.

---

## 10. Ajout d’une seed aléatoire

La simulation utilise du hasard pour générer la volatilité.

Au départ, chaque exécution du script produisait donc des courbes différentes. Cela peut être intéressant pour un jeu, mais c’est très gênant pendant le développement, car il devient difficile de comparer deux versions.

Nous avons donc ajouté :

```python
np.random.seed(42)
```

Cela rend les simulations reproductibles.

Plus tard, dans le jeu, on pourra remplacer cette seed fixe par une seed liée à la partie du joueur, afin de générer une économie différente à chaque nouvelle partie tout en gardant une logique contrôlée.

---

## 11. Création du script de visualisation

Pour évaluer les résultats, nous avons créé un script séparé :

```txt
Visualisation_Courbes.py
```

Ce fichier ne modifie pas les simulations. Il sert uniquement à charger :

```txt
les données historiques scrappées
+
les données simulées
```

et à tracer les graphiques.

Le script produit une courbe par actif, avec :

- la partie historique réelle ;
- une ligne verticale indiquant le début de la simulation ;
- la partie simulée sur 40 ans.

Il produit aussi un graphique comparatif en base 100.  
Ce graphique est très important, car il permet de comparer tous les actifs malgré leurs valeurs très différentes.

Par exemple, Bitcoin peut valoir plusieurs dizaines de milliers, alors que le dollar vaut environ 1. La base 100 permet donc de comparer les performances relatives.

---

## 12. Problème rencontré avec pandas

Lors du lancement de la visualisation, nous avons rencontré une erreur liée à `pandas` :

```txt
ValueError: 'M' is no longer supported for offsets. Please use 'ME' instead.
```

L’ancienne notation :

```python
resample("M")
```

n’était plus acceptée dans la version récente de `pandas`.

Nous avons donc corrigé en utilisant :

```python
resample("ME")
```

qui signifie “Month End”, c’est-à-dire fin de mois.

Cette correction a permis de relancer correctement les graphiques.

---

## 13. Analyse des premières courbes

Une fois les courbes générées, nous avons observé plusieurs choses.

Les courbes du CAC 40, de l’or et du dollar étaient globalement cohérentes.

Le CAC 40 avait une croissance modérée.  
L’or jouait bien son rôle de valeur refuge.  
Le dollar restait relativement stable, ce qui correspond bien à une devise.

En revanche, certains actifs posaient problème :

```txt
Bitcoin montait beaucoup trop fort.
Google devenait trop puissant.
Nvidia gardait une croissance trop optimiste.
```

Ces constats ont guidé les corrections suivantes.

---

## 14. Correction de Nvidia : bulle IA

Nvidia était un cas particulier.

L’entreprise a fortement profité du boom de l’intelligence artificielle. Le problème est que si on utilisait directement sa croissance récente pour extrapoler sur 40 ans, Nvidia devenait beaucoup trop puissant.

Nous avons donc décidé de modéliser Nvidia comme un actif soumis à une bulle technologique, en nous inspirant de la bulle Internet des années 2000.

Le scénario retenu est :

```txt
2025–2029 : boom IA ;
2030–mi-2031 : correction de bulle ;
2031–2035 : reprise progressive ;
après 2035 : croissance mature.
```

Nous n’avons pas voulu faire disparaître Nvidia ni provoquer un effondrement total. L’idée était plutôt de dire :

```txt
Nvidia reste une entreprise importante,
mais la croissance exceptionnelle liée à l’IA ne dure pas éternellement.
```

Nous avons donc ajouté une fonction spéciale pour Nvidia dans le modèle.

Elle ajuste :

- le CAGR ;
- la volatilité ;
- un choc mensuel négatif pendant la correction.

Cela a donné une courbe beaucoup plus réaliste : Nvidia reste un actif très performant, mais il devient aussi plus risqué.

---

## 15. Réflexion sur TotalEnergies

Nous nous sommes ensuite interrogés sur TotalEnergies.

Le problème est que l’entreprise est encore fortement liée aux énergies fossiles. Elle investit dans les nouvelles énergies, mais reste exposée au pétrole et au gaz.

Nous avons donc conclu que TotalEnergies devait rester un actif intéressant, mais avec une incertitude future liée à :

- la transition énergétique ;
- la réglementation climatique ;
- les taxes carbone ;
- les crises énergétiques ;
- la demande mondiale en pétrole et gaz.

Pour l’instant, nous n’avons pas appliqué de correction lourde directement dans le modèle.  
Nous avons préféré garder TotalEnergies relativement modéré, en considérant que les grands chocs seront plutôt gérés plus tard par le système d’événements.

C’est logique, car TotalEnergies peut souffrir dans un scénario de transition rapide, mais aussi profiter fortement d’une crise énergétique.

---

## 16. Correction de Google

Google posait un autre problème.

La première courbe montrait une croissance trop forte. Google devenait extrêmement performant sur 40 ans, comme si l’entreprise gardait le rythme d’une société encore jeune.

Or Google est déjà une entreprise immense. Elle peut encore croître, notamment grâce à l’IA, au cloud, à YouTube, à la publicité et à Android, mais elle ne peut pas raisonnablement garder indéfiniment une croissance de startup.

Nous avons donc ajouté une correction spécifique à Google.

La logique est :

```txt
Google reste une valeur tech solide,
mais sa croissance est progressivement plafonnée.
```

Nous avons abaissé son CAGR long terme et ajouté une fonction de maturité qui limite sa croissance effective dans le temps.

Résultat : Google reste un très bon actif, mais ne devient plus complètement irréaliste.

---

## 17. Correction du Bitcoin

Bitcoin était le plus gros problème des premières simulations.

Avant correction, Bitcoin écrasait tous les autres actifs. Sur le graphique en base 100, il devenait clairement un “cheat code” pour le joueur.

Le problème ne venait pas seulement de son rendement long terme, mais surtout du fait que son CAGR historique très élevé influençait trop longtemps la simulation.

Nous avons donc ajouté une correction spéciale pour Bitcoin.

La logique est :

```txt
Bitcoin reste très volatil ;
Bitcoin peut encore fortement monter ;
mais Bitcoin ne doit pas dominer automatiquement tous les autres actifs.
```

Nous avons donc :

- abaissé son CAGR long terme ;
- plafonné sa croissance initiale ;
- accéléré son retour vers une croissance plus prudente ;
- conservé une volatilité élevée.

Cette correction a fortement amélioré l’équilibre du modèle. Bitcoin reste un actif risqué et intéressant, mais il ne détruit plus l’équilibrage du jeu.

---

## 18. Résultat final actuel

Après toutes ces corrections, le graphique comparatif en base 100 est devenu beaucoup plus cohérent.

Chaque actif a maintenant une personnalité claire :

```txt
CAC 40 : actif classique, croissance modérée ;
Or : valeur refuge, croissance stable ;
Dollar : devise macro, faible variation ;
Nvidia : actif tech très performant mais risqué ;
Google : grande tech solide, croissance maîtrisée ;
Bitcoin : actif très volatil mais plus équilibré ;
TotalEnergies : actif énergétique cyclique et incertain.
```

Le plus important est qu’aucun actif ne domine totalement tous les autres.

Avant les corrections, Bitcoin et Google rendaient le jeu déséquilibré.  
Maintenant, chaque actif peut être intéressant selon la stratégie du joueur.

---

## 19. État actuel du module financier

À ce stade, nous pouvons considérer que nous avons une **V1 stable du module financier**.

Cette V1 contient :

```txt
Scrapping_Actifs.py
Modele_ML_CAGR.py
Visualisation_Courbes.py
fichiers JSON historiques
fichiers JSON simulés
graphiques de validation
```

Le module permet de :

1. récupérer des données historiques ;
2. les nettoyer ;
3. les exporter en JSON ;
4. calculer une tendance historique ;
5. générer une simulation mensuelle sur 40 ans ;
6. corriger certains actifs spécifiques ;
7. visualiser les résultats ;
8. préparer l’intégration Unity.

---

## 20. Ce qui a guidé nos choix

Nos choix ont été guidés par une idée centrale :

```txt
Le modèle ne doit pas prédire parfaitement l’avenir.
Il doit générer une économie crédible, jouable et équilibrée.
```

C’est pour cela que nous avons abandonné la régression polynomiale.

Un modèle peut être bon sur le passé, mais mauvais pour le jeu s’il explose dans le futur.  
À l’inverse, un modèle plus simple, mais mieux contrôlé, peut être beaucoup plus pertinent.

Nous avons donc privilégié :

- la stabilité ;
- la lisibilité ;
- le contrôle ;
- le réalisme global ;
- l’équilibrage de gameplay.

---

## 21. Différence entre prédiction et simulation

Il est important de préciser que les courbes actuelles ne sont pas des prédictions financières au sens strict.

Elles sont plutôt des simulations économiques inspirées du réel.

On ne prétend pas savoir combien vaudra Bitcoin, Google ou le CAC 40 dans 40 ans.  
On cherche plutôt à créer un monde économique plausible dans lequel le joueur peut prendre des décisions.

La formulation correcte serait donc :

```txt
Nous utilisons les données historiques pour calibrer une simulation financière,
mais le modèle est ajusté afin de rester cohérent pour un jeu vidéo.
```

---

## 22. Prochaine étape : les événements économiques

La prochaine grande étape sera l’ajout d’un système d’événements.

Actuellement, les courbes représentent la trajectoire économique de base.  
Les événements viendront ensuite modifier temporairement ces courbes.

Par exemple :

```txt
crise financière ;
krach crypto ;
bulle IA ;
correction IA ;
crise énergétique ;
inflation ;
récession ;
hausse des taux ;
transition énergétique ;
choc immobilier.
```

Ces événements ne remplaceront pas le modèle actuel.  
Ils viendront se superposer à lui.

Le futur fichier prévu est :

```txt
evenements.json
```

Il permettra de définir des événements séparément du code principal.

Exemple possible :

```json
{
  "nom": "Crise énergétique",
  "debut": 2042,
  "duree_mois": 12,
  "impact": {
    "totalenergies": 0.25,
    "cac40": -0.06,
    "or": 0.08
  }
}
```

Cette approche est intéressante car elle permet de modifier le scénario économique sans casser le modèle de base.

---

## 23. Bilan

Cette étape a permis de transformer une simple expérimentation Python en véritable module de simulation économique.

Nous sommes passés par plusieurs phases :

```txt
scrapping simple ;
nettoyage des données ;
tentative de régression polynomiale ;
constat d’échec sur l’extrapolation ;
passage à un modèle CAGR + volatilité ;
correction du format mensuel ;
correction du dollar ;
ajout d’une seed ;
visualisation des courbes ;
correction de Nvidia ;
correction de Google ;
correction de Bitcoin ;
obtention d’une V1 stable.
```

Le résultat actuel est satisfaisant : les courbes sont cohérentes, lisibles et utilisables pour un jeu.

Le module financier peut maintenant servir de base solide pour l’intégration Unity et pour l’ajout d’événements économiques scénarisés.
