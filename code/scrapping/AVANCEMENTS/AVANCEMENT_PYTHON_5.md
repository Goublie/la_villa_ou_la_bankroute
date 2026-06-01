# avancement_python_5 — Prédictions immobilières, inflation et Livret A

## 1. Objectif de cette étape

Après avoir construit une base immobilière mensuelle propre pour plusieurs villes, j’ai voulu aller plus loin : ne plus seulement observer l’historique, mais aussi produire des **trajectoires futures exploitables dans le jeu**.

L’objectif était donc double :

```txt
1. prédire les prix immobiliers sur les 40 prochaines années ;
2. comparer cette évolution avec l’inflation et avec des placements sans risque comme le Livret A.
```

Cette étape m’a permis de mieux comprendre une question importante pour le jeu :

> Est-ce qu’un joueur gagne réellement de l’argent avec l’immobilier, ou est-ce qu’il voit seulement le prix de son bien augmenter parce que tout devient plus cher avec l’inflation ?

---

## 2. Prédiction immobilière sur 40 ans

J’ai commencé par mettre en place un modèle de prédiction pour les prix immobiliers.

Le fichier principal créé est :

```txt
Modele_Immo_CAGR.py
```

Ce modèle part du fichier déjà généré précédemment :

```txt
immo_villes_clean/immo_villes_mensuel_format_jeu.json
```

Ce fichier contient les prix mensuels au m² des villes sélectionnées entre 2015 et 2025.

Les villes utilisées sont :

```txt
Paris
Lyon
Marseille
Bordeaux
Toulouse
Nantes
```

---

## 3. Logique du modèle immobilier

Pour prédire l’immobilier, j’ai repris l’idée générale déjà utilisée pour les actifs financiers : utiliser une croissance composée de type CAGR.

Cependant, j’ai adapté le modèle à l’immobilier, car l’immobilier ne se comporte pas comme une action ou une cryptomonnaie.

L’immobilier est généralement :

```txt
plus lent ;
moins volatil ;
plus sensible aux cycles longs ;
plus dépendant de la ville étudiée.
```

Le modèle immobilier utilise donc :

```txt
CAGR historique de chaque ville
+ retour progressif vers un CAGR long terme
+ volatilité mensuelle contrôlée
+ plafonnement des variations mensuelles
+ petit cycle immobilier lent
```

L’idée n’est pas de prédire parfaitement le vrai prix de l’immobilier en 2065, mais de produire une trajectoire crédible pour une simulation de jeu.

---

## 4. CAGR historique et CAGR long terme

Pour chaque ville, j’ai calculé un **CAGR historique** à partir des données réelles 2015-2025.

Le CAGR historique mesure le rythme moyen annuel auquel le prix au m² a augmenté sur la période passée.

Mais j’ai évité de prolonger aveuglément ce CAGR historique sur 40 ans.

Par exemple, si une ville a beaucoup monté entre 2015 et 2021, il serait irréaliste de supposer qu’elle va continuer exactement au même rythme pendant 40 ans.

J’ai donc ajouté un **CAGR long terme** par ville. Ce CAGR long terme représente une tendance plus raisonnable à long horizon.

Le modèle commence avec une forte influence du CAGR historique, puis revient progressivement vers le CAGR long terme.

Cette transition se fait sur environ 10 ans, ce qui correspond mieux à la lenteur des cycles immobiliers.

---

## 5. Personnalité des villes

J’ai donné une logique différente à chaque ville.

Paris est une ville déjà très chère et mature. Elle ne doit pas exploser dans le futur.

Lyon est dynamique, mais a déjà fortement progressé.

Marseille a un potentiel de rattrapage, car elle reste moins chère au départ.

Bordeaux a beaucoup monté par le passé, donc le modèle doit éviter de prolonger cette hausse trop fortement.

Toulouse a une trajectoire plus régulière.

Nantes reste attractive, mais avec une certaine prudence après sa hausse passée.

Ces différences permettent de ne pas avoir six villes qui évoluent exactement de la même manière.

---

## 6. Fichiers produits pour l’immobilier

Le modèle immobilier génère principalement :

```txt
immo_villes_clean/immo_villes_simulation_40ans.json
```

Ce fichier est le fichier utile pour le jeu. Il contient uniquement les prix simulés sur 40 ans.

J’ai aussi généré :

```txt
immo_villes_clean/immo_villes_historique_et_simulation_40ans.json
immo_villes_clean/immo_villes_parametres_simulation.json
```

Le premier sert à vérifier la continuité entre l’historique réel et la simulation.

Le second garde les paramètres utilisés : CAGR historique, CAGR long terme, volatilité, prix final simulé, etc.

---

## 7. Visualisation de la prédiction immobilière

Pour vérifier les résultats, j’ai créé :

```txt
Visualisation_Immo_Simulation.py
```

Ce script génère plusieurs graphiques de contrôle :

```txt
historique réel + simulation pour toutes les villes ;
évolution en base 100 ;
simulation seule en base 100 ;
classement final des villes après 40 ans ;
progression simulée par ville ;
CAGR effectif utilisé pendant la simulation ;
variations mensuelles simulées ;
graphique détaillé par ville.
```

Ces graphiques m’ont permis de valider visuellement que les trajectoires n’étaient pas absurdes.

La visualisation est importante, car un fichier JSON peut sembler correct alors qu’une courbe montre immédiatement un problème.

---

## 8. Questionnement sur l’inflation

En analysant les scénarios immobiliers, je me suis posé une question importante :

> Si l’inflation est plus élevée que la croissance du prix d’un bien immobilier, est-ce que l’investisseur gagne vraiment de l’argent ?

La réponse est non, pas forcément.

Si un bien immobilier prend 1,5 % par an, mais que l’inflation est à 3 % par an, alors le prix du bien augmente en euros, mais il perd en valeur réelle.

Autrement dit, le joueur peut avoir l’impression de gagner de l’argent parce que le prix nominal du bien monte, alors qu’en pouvoir d’achat il s’appauvrit.

Cette réflexion m’a poussé à récupérer aussi des données d’inflation.

---

## 9. Scraping de l’inflation

J’ai donc ajouté un module de scraping de l’inflation.

Le fichier utilisé est :

```txt
Scrapping_Inflation.py
```

L’idée était de récupérer l’IPC, c’est-à-dire l’indice des prix à la consommation.

L’IPC n’est pas directement l’inflation. L’inflation correspond à la variation de l’IPC.

J’ai donc récupéré l’indice, puis calculé :

```txt
l’inflation mensuelle ;
l’inflation annuelle glissante ;
l’indice base 100 depuis janvier 2015.
```

Le fichier principal généré pour le jeu est :

```txt
inflation_clean/inflation_mensuel_format_jeu.json
```

J’ai aussi gardé des fichiers plus détaillés :

```txt
inflation_clean/inflation_mensuel.json
inflation_clean/inflation_annuel.json
```

---

## 10. Comparaison immobilier / inflation

Après avoir récupéré l’inflation, j’ai créé un script de comparaison entre la croissance annuelle immobilière et l’inflation.

Le but était de voir si chaque ville battait réellement l’inflation ou non.

L’idée était simple :

```txt
croissance réelle approximative = croissance immobilière - inflation
```

Si le résultat est positif, l’immobilier bat l’inflation.

Si le résultat est négatif, l’immobilier monte moins vite que le niveau général des prix.

Cette étape était importante pour donner plus de profondeur économique au jeu.

---

## 11. Passage au Livret A

Après l’inflation, je me suis intéressé au Livret A.

L’idée était de disposer d’un placement sans risque dans le jeu, afin de comparer l’immobilier et les actifs risqués avec une épargne réglementée.

Au départ, j’ai voulu reproduire la formule officielle ou semi-officielle du taux du Livret A.

La formule étudiée utilisait plusieurs éléments :

```txt
inflation sur les douze derniers mois ;
indice INSEE des prix à la consommation ;
Euribor 3 mois ;
Eonia ;
arrondi au quart de point.
```

J’ai donc commencé à préparer un fichier qui récupérait toutes ces données.

---

## 12. Problèmes rencontrés avec la formule du Livret A

Cette tentative a rapidement posé plusieurs problèmes.

D’abord, certaines données sont difficiles à récupérer automatiquement proprement.

Ensuite, l’Eonia n’existe plus depuis 2022. Il a été remplacé par un système lié au €STR, ce qui oblige à utiliser un proxy si je veux continuer à appliquer l’ancienne formule.

Enfin, le problème principal est apparu en comparant le taux théorique calculé avec le vrai taux du Livret A.

Le taux théorique obtenu avec la formule ne correspondait pas toujours au taux réellement appliqué.

Cela m’a permis de comprendre un point important :

> Le Livret A n’est pas un taux de marché libre. C’est un taux réglementé.

Même si une formule existe, l’État peut intervenir, lisser, geler ou plafonner le taux.

Par exemple, pendant la période de forte inflation, la formule pouvait conduire à un taux théorique plus élevé, mais le taux réel est resté limité.

Donc pour le jeu, reproduire toute la formule donnait une courbe trop théorique et pas forcément plus réaliste.

---

## 13. Abandon du modèle Livret A trop complexe

Après cette analyse, j’ai décidé d’abandonner le modèle complexe basé sur la formule complète.

Ce modèle demandait beaucoup de données et produisait une courbe qui ne représentait pas forcément le taux réellement appliqué.

La conclusion était simple :

```txt
si l’État peut intervenir sur le taux, alors la formule seule ne suffit pas à prédire le Livret A.
```

Pour un jeu, il est donc plus pertinent de simuler le Livret A comme un taux réglementé, stable, lissé et borné.

---

## 14. Modèle simplifié du Livret A

J’ai ensuite créé un modèle simplifié :

```txt
Modele_LivretA_Simplifie_40ans.py
```

Ce modèle ne cherche plus à reproduire toute la formule officielle.

Il simule plutôt un comportement réaliste du Livret A :

```txt
taux borné ;
taux relativement stable ;
révision seulement deux fois par an ;
influence de l’inflation ;
inertie politique ;
arrondi au quart de point.
```

Le taux simulé reste donc dans une zone raisonnable et ne part pas dans des pics absurdes.

J’ai aussi corrigé le modèle pour que les taux simulés soient toujours des multiples de 0,25 :

```txt
0,50 %
0,75 %
1,00 %
1,25 %
1,50 %
2,00 %
2,25 %
3,00 %
```

Cela donne un comportement plus lisible et plus cohérent avec un taux réglementé.

---

## 15. Visualisation du Livret A simplifié

Pour vérifier le modèle, j’ai créé :

```txt
Visualisation_LivretA_Simplifie.py
```

Ce script génère des graphiques permettant de visualiser :

```txt
l’historique réel + la simulation ;
la simulation seule sur 40 ans ;
l’inflation simulée face au taux du Livret A ;
le rendement mensuel simulé.
```

Le fichier principal pour le jeu est :

```txt
livret_a_clean/livret_a_simulation_40ans_simplifie.json
```

J’ai aussi généré :

```txt
livret_a_clean/livret_a_historique_et_simulation_simplifie.json
```

Ce fichier sert à vérifier la continuité entre l’historique récent et la simulation future.

---

## 16. Nettoyage du scraping historique du Livret A

Comme j’avais abandonné l’ancien fichier complexe du Livret A, j’ai ensuite voulu en extraire uniquement la partie utile : la récupération de l’historique réel du taux du Livret A.

J’ai donc nettoyé l’ancien fichier pour retirer :

```txt
le calcul de formule ;
l’IPC ;
l’Euribor ;
l’Eonia ;
le €STR ;
les exports liés au taux théorique.
```

Le fichier propre obtenu est :

```txt
Scrapping_LivretA_Historique.py
```

Ce fichier sert uniquement à produire l’historique réel du taux du Livret A.

Il génère :

```txt
livret_a_clean/livret_a_taux_reel_mensuel.json
livret_a_clean/livret_a_taux_reel_format_jeu.json
```

J’ai aussi créé un script de visualisation pour vérifier cet historique :

```txt
Visualisation_LivretA_Historique.py
```

Ce script permet de voir les changements de taux détectés et de vérifier que les dates importantes sont cohérentes.

---

## 17. Problèmes rencontrés pendant cette étape

Cette étape a été assez complexe, car plusieurs choses se sont croisées.

J’ai d’abord dû clarifier la différence entre :

```txt
IPC ;
inflation ;
taux théorique ;
taux réel ;
taux réglementé ;
taux simulé.
```

J’ai aussi rencontré des problèmes techniques avec certaines sources, notamment pour lire automatiquement certains fichiers ou certaines pages.

Le plus gros problème conceptuel a été le Livret A : je pensais au départ qu’une formule suffisait à retrouver le taux réel, mais ce n’est pas le cas.

J’ai donc modifié l’approche pour privilégier un modèle plus simple, plus stable et plus adapté au jeu.

---

## 18. État actuel

À la fin de cette étape, j’ai maintenant :

```txt
une simulation immobilière sur 40 ans ;
un fichier de visualisation de cette simulation ;
un scraping de l’inflation ;
une comparaison immobilier / inflation ;
un historique réel du Livret A ;
un modèle simplifié du Livret A sur 40 ans ;
une visualisation du Livret A simulé.
```

Les fichiers importants pour le jeu sont :

```txt
immo_villes_clean/immo_villes_simulation_40ans.json
inflation_clean/inflation_mensuel_format_jeu.json
livret_a_clean/livret_a_simulation_40ans_simplifie.json
livret_a_clean/livret_a_taux_reel_format_jeu.json
```

Les autres fichiers servent surtout à vérifier, documenter ou visualiser les résultats.

---

## 19. Bilan

Cette étape a permis de passer d’une simple base immobilière historique à un début de système économique plus complet.

J’ai maintenant plusieurs briques économiques importantes :

```txt
immobilier ;
inflation ;
Livret A ;
comparaison rendement nominal / rendement réel.
```

Le point le plus important est la compréhension du rôle de l’inflation.

Un actif peut monter en euros sans forcément enrichir réellement le joueur si l’inflation monte plus vite.

Cette idée pourra être très utile dans le gameplay, car elle permet de créer des choix économiques plus intéressants :

```txt
acheter un bien immobilier ;
garder de l’argent sur un Livret A ;
prendre plus de risque sur des actifs financiers ;
subir une période d’inflation élevée ;
comparer rendement nominal et rendement réel.
```

Le modèle n’est pas encore parfait, mais il est maintenant suffisamment structuré pour être intégré progressivement dans la logique économique du jeu.

---

## 20. Étapes suivantes possibles

Les prochaines étapes pourront être :

```txt
1. intégrer les fichiers JSON dans Unity ;
2. relier les prix immobiliers aux achats du joueur ;
3. intégrer le Livret A comme placement sécurisé ;
4. afficher rendement nominal et rendement réel ;
5. ajouter des événements économiques qui modifient inflation, immobilier et Livret A ;
6. équilibrer les rendements pour rendre le jeu intéressant.
```
