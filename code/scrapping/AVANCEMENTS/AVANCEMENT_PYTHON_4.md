# avancement_python_4 — Module immobilier DVF+

## 1. Objectif de cette étape

Après avoir stabilisé le module financier, j’ai commencé la partie **immobilier** du projet.

L’objectif était de récupérer des données réelles de prix immobiliers afin d’alimenter le jeu **La villa ou la banqueroute** avec des prix au m² crédibles pour plusieurs grandes villes françaises.

Les villes retenues sont :

```txt
Paris
Lyon
Marseille
Bordeaux
Toulouse
Nantes
```

Le but final était d’obtenir un fichier simple et directement exploitable dans Unity :

```txt
immo_villes_mensuel_format_jeu.json
```

---

## 2. Première tentative avec une API immobilière

Au départ, j’ai utilisé une API immobilière qui permettait de récupérer directement des prix médians au m².

Cette première version fonctionnait, mais elle présentait deux limites importantes :

```txt
1. l’API ne permettait pas de remonter avant 2020 ;
2. elle ne donnait que des données annuelles.
```

J’obtenais donc seulement quelques points par ville, par exemple :

```txt
2020
2021
2022
2023
2024
```

Cette solution était suffisante pour une preuve de concept, mais elle était trop limitée pour construire une vraie base immobilière exploitable dans le jeu.

Avec seulement quelques points annuels, il était difficile d’observer correctement les cycles du marché, les effets de crise ou les différences d’évolution entre villes.

---

## 3. Recherche d’une meilleure source de données

J’ai ensuite cherché à récupérer les anciennes données DVF, notamment pour les années 2015 à 2019.

Sur certaines pages officielles, les fichiers semblaient encore listés, mais les liens de téléchargement renvoyaient vers des erreurs 404. Cette source n’était donc pas exploitable directement.

Ce blocage m’a poussé à changer de source de données.

La solution retenue a été d’utiliser les fichiers **DVF+ open-data du Cerema**.

Cette source est beaucoup plus adaptée, car elle couvre une période longue :

```txt
2014 à 2025
```

J’ai choisi les fichiers au format :

```txt
CSV
```

car ils sont facilement lisibles avec Python et `pandas`.

---

## 4. Données récupérées

J’ai récupéré les fichiers correspondant aux régions ou départements nécessaires pour les villes étudiées :

```txt
Île-de-France         → Paris
Auvergne-Rhône-Alpes → Lyon
PACA                  → Marseille
Nouvelle-Aquitaine    → Bordeaux
Occitanie             → Toulouse
Pays de la Loire      → Nantes
```

Certains fichiers étaient déjà centrés sur une ville ou un département, tandis que d’autres contenaient les données d’une région entière.

Cela ne posait pas de problème, car le filtrage pouvait ensuite être fait avec les codes communes.

---

## 5. Extraction de Paris en premier

Avant de généraliser le traitement à toutes les villes, j’ai commencé par Paris.

J’ai créé un script :

```txt
Extraction_Immo_Paris_DVFPlus.py
```

Ce script permettait de :

```txt
lire le CSV DVF+ ;
garder uniquement les ventes ;
garder uniquement les appartements ;
filtrer les années 2015 à 2025 ;
calculer le prix au m² ;
supprimer les valeurs aberrantes ;
agréger les prix par mois ;
exporter les résultats en JSON.
```

J’ai choisi de garder uniquement les appartements simples, afin d’éviter de mélanger des ventes complexes : plusieurs lots, dépendances, locaux mixtes ou ventes groupées.

Le résultat était un fichier mensuel propre pour Paris :

```txt
immo_paris_mensuel_format_jeu.json
```

---

## 6. Choix du format mensuel

J’ai comparé trois pas de temps possibles pour les données immobilières :

```txt
annuel ;
trimestriel ;
mensuel.
```

Le format annuel était trop pauvre pour le jeu, surtout maintenant que les données DVF+ permettaient d’aller plus loin.

Le format trimestriel était réaliste pour analyser le marché immobilier, mais le jeu fonctionne en **tour par tour mensuel**.

J’ai donc choisi de garder un format mensuel, car il correspond mieux au rythme du gameplay.

J’ai aussi décidé de ne pas lisser les valeurs pendant l’extraction. Le but était de conserver les vraies médianes mensuelles issues des transactions DVF+.

Le format retenu est donc :

```json
{
  "Annee": 2015,
  "Mois": 1,
  "Periode": "2015-01",
  "Prix_m2": 7954.55,
  "Nb_transactions": 1850
}
```

---

## 7. Visualisation de Paris

Pour vérifier la qualité des données extraites, j’ai créé un script de visualisation :

```txt
Visualisation_Immo_Paris.py
```

Ce script générait plusieurs graphiques de contrôle :

```txt
prix mensuel au m² ;
nombre de transactions mensuelles ;
prix et transactions sur le même graphique ;
comparaison mensuel / trimestriel ;
prix par arrondissement ;
distribution des prix ;
distribution des surfaces.
```

Les graphiques ont permis de valider la cohérence des données.

Ils montraient notamment :

```txt
une hausse des prix jusqu’en 2020-2021 ;
une baisse après 2022 ;
une baisse du nombre de transactions pendant la période Covid.
```

Cette étape a confirmé que les données mensuelles de Paris étaient suffisamment solides pour être utilisées.

---

## 8. Généralisation à toutes les villes

Une fois Paris validé, j’ai généralisé le traitement avec un nouveau script :

```txt
Extraction_Immo_Villes_DVFPlus.py
```

L’objectif était d’éviter d’avoir un script différent pour chaque ville.

Le script utilise un dictionnaire de configuration contenant :

```txt
le nom de la ville ;
le nom du fichier CSV source ;
les codes communes à garder ;
la présence ou non d’arrondissements.
```

Cela permet de traiter aussi bien :

```txt
des fichiers déjà limités à une ville ;
des fichiers régionaux contenant beaucoup d’autres communes.
```

Dans le cas des fichiers régionaux, le script filtre uniquement les lignes correspondant au code commune de la ville étudiée.

---

## 9. Problèmes rencontrés pendant l’organisation

Pendant la mise en place du script multi-villes, j’ai rencontré quelques problèmes techniques simples mais bloquants.

Il y avait notamment :

```txt
des noms de fichiers différents de ceux attendus par le script ;
une confusion entre dvf_plus et dvfplus ;
un dossier nommé avec un espace au lieu d’un underscore.
```

Ces erreurs empêchaient Python de trouver les fichiers CSV.

Après avoir corrigé l’organisation des dossiers et les noms de fichiers, l’extraction multi-villes a fonctionné correctement.

La structure finale utilisée est du type :

```txt
donnees_dvfplus/
├── dvfplus_paris.csv
├── dvfplus_lyon.csv
├── dvfplus_bordeaux.csv
├── dvfplus_paca.csv
├── dvfplus_occitanie.csv
└── dvfplus_pays_de_la_loire.csv
```

---

## 10. Résultat obtenu

Le script multi-villes a généré le fichier principal :

```txt
immo_villes_mensuel_format_jeu.json
```

Ce fichier contient les six villes avec leurs prix mensuels au m² de :

```txt
janvier 2015 à décembre 2025
```

Cela représente :

```txt
132 mois par ville
```

Chaque ville possède le même format simple :

```json
{
  "Annee": 2015,
  "Mois": 1,
  "Periode": "2015-01",
  "Prix_m2": 3236.69,
  "Nb_transactions": 414
}
```

Ce fichier est le plus important pour Unity, car il est compact, clair et directement exploitable.

---

## 11. Fichiers détaillés conservés

En plus du fichier global pour le jeu, j’ai conservé des fichiers mensuels détaillés par ville :

```txt
immo_paris_mensuel.json
immo_lyon_mensuel.json
immo_marseille_mensuel.json
immo_bordeaux_mensuel.json
immo_toulouse_mensuel.json
immo_nantes_mensuel.json
```

Ces fichiers contiennent plus d’informations que le fichier de jeu :

```txt
prix médian ;
prix moyen ;
q25 ;
q75 ;
nombre de transactions.
```

Ils servent surtout à vérifier les données, à produire des graphiques et à garder une trace plus complète du traitement.

---

## 12. Visualisation multi-villes

J’ai ensuite préparé un script de visualisation globale :

```txt
Visualisation_Immo_Villes.py
```

Ce script sert à générer des graphiques de contrôle pour toutes les villes.

Il permet notamment de visualiser :

```txt
les prix mensuels au m² par ville ;
l’évolution en base 100 ;
les transactions mensuelles ;
les transactions annuelles ;
le classement final des villes ;
la progression entre 2015 et 2025 ;
un graphique prix + transactions pour chaque ville.
```

Ces graphiques sont surtout utiles pour la validation humaine, car ils permettent de repérer rapidement une incohérence ou une tendance étrange.

---

## 13. Choix avant le push Git

Avant de pousser les fichiers sur Git, j’ai séparé ce qui était utile au projet de ce qui devait rester uniquement en local.

Les fichiers utiles pour le projet sont :

```txt
Extraction_Immo_Villes_DVFPlus.py
Visualisation_Immo_Villes.py
immo_villes_mensuel_format_jeu.json
immo_paris_mensuel.json
immo_lyon_mensuel.json
immo_marseille_mensuel.json
immo_bordeaux_mensuel.json
immo_toulouse_mensuel.json
immo_nantes_mensuel.json
```

Les fichiers à garder en local sont :

```txt
fichiers CSV DVF+ bruts ;
transactions détaillées ;
fichiers trimestriels ;
fichiers par arrondissement ;
graphiques générés.
```

Les CSV bruts sont trop lourds et peuvent être régénérés. Les graphiques sont également régénérables à partir des scripts de visualisation.

---

## 14. Bilan

Cette étape a permis de transformer la partie immobilière du projet.

Au départ, la première API ne donnait que :

```txt
5 points annuels par ville
```

Grâce aux fichiers DVF+, j’ai maintenant obtenu :

```txt
132 points mensuels par ville
6 villes étudiées
une période de 2015 à 2025
un format JSON compatible avec Unity
```

Le module immobilier est maintenant structuré et exploitable.

Il contient :

```txt
une extraction multi-villes ;
un fichier global prêt pour le jeu ;
des fichiers détaillés pour l’analyse ;
un script de visualisation pour vérifier les résultats.
```

Le fichier principal pour Unity est :

```txt
immo_villes_mensuel_format_jeu.json
```

Cette base pourra ensuite servir à construire la simulation immobilière du jeu et à intégrer les variations de prix dans le système économique global.
