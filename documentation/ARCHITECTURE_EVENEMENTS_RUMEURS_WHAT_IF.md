# Architecture des rumeurs, événements et du moteur What If

## Projet

**La Villa ou la Banqueroute**
Document de référence fonctionnel et technique
Branche cible : `feature/evenements-catalogue-orchestration`

---

## 1. Objectif général

Le système doit transformer le catalogue actuel d’environ 200 événements en un moteur de jeu mensuel complet fondé sur :

1. l’apparition de rumeurs ;
2. la fiabilité de leurs sources ;
3. la confirmation ou non de l’événement au tour suivant ;
4. l’application immédiate de ses impacts pendant le tour où il est confirmé ;
5. la conservation de tout l’historique dans Actualités et dans les snapshots ;
6. l’utilisation des événements confirmés par le moteur What If pour comparer les choix réels du joueur à une stratégie boursière alternative optimisée.

Le moteur doit respecter l’architecture globale du projet :

- les données persistantes vivent dans les agrégats métier ;
- les règles sont portées par des services ;
- l’interface affiche les données sans modifier directement l’état du jeu ;
- les snapshots restent des copies profondes ;
- l’aléatoire est reproductible dans les tests ;
- les rumeurs n’ont aucun impact direct ;
- seuls les événements confirmés influencent le jeu et le What If.

---

## 2. État actuel des données

Le projet contient actuellement :

- un catalogue d’environ **200 événements** ;
- environ **19 sources** ;
- une fiabilité déjà attribuée à chaque source ;
- pour les événements : identifiant, catégorie, titre, importance, message et impacts.

Limites actuelles à auditer avant implémentation :

- aucun objet métier de rumeur clairement séparé ;
- aucun lien explicite `rumeur -> événement` ;
- aucun `sourceId` dans les événements ;
- aucune probabilité de confirmation propre à chaque rumeur ;
- aucune règle de récurrence ;
- certains événements n’ont aucun impact ;
- certains impacts ciblent des actifs ou statistiques peut-être absents du moteur ;
- certaines variations paraissent potentiellement trop fortes ;
- plusieurs événements proches peuvent être redondants.

La première mission Codex doit produire un audit complet du catalogue avant de modifier les probabilités.

---

## 3. Cycle mensuel d’une rumeur

### 3.1 Apparition

À chaque nouveau mois, exactement **deux rumeurs distinctes** apparaissent.

Elles sont choisies aléatoirement parmi les rumeurs éligibles.

Une rumeur affichée contient au minimum :

- son identifiant ;
- l’identifiant de l’événement associé ;
- son titre public ;
- son texte public ;
- sa catégorie ;
- sa source ;
- la fiabilité publique ou qualitative de la source ;
- le mois d’apparition ;
- son état : `EnAttente`.

Une rumeur :

- ne modifie aucune courbe ;
- ne modifie aucune statistique ;
- n’est pas utilisée par le moteur What If ;
- ne doit pas être sélectionnée de nouveau tant qu’elle n’est pas résolue.

### 3.2 Résolution au mois suivant

Une rumeur apparue au mois `M` est résolue au mois `M + 1`.

Chaque rumeur est résolue indépendamment.

Deux issues sont possibles :

#### Rumeur confirmée

- l’événement associé est créé ;
- il est publié dans Actualités ;
- ses impacts deviennent actifs pendant ce même tour ;
- son état devient `Confirmee`.

#### Rumeur non confirmée

- aucun événement n’est créé ;
- aucun impact n’est appliqué ;
- la rumeur reste visible dans l’historique comme non confirmée ;
- son état devient `Invalidee`.

Il n’existe aucune autre condition de déclenchement.

---

## 4. Ordre recommandé dans un tour

Pour un mois `M`, l’ordre fonctionnel cible est :

1. clôture du mois précédent ;
2. création du snapshot de clôture ;
3. passage au mois `M` ;
4. résolution des deux rumeurs apparues au mois `M - 1` ;
5. publication immédiate des événements confirmés ;
6. affichage des événements au joueur dans Actualités ;
7. choix du joueur pour le mois `M` ;
8. application des impacts des événements confirmés pendant le mois `M` ;
9. évolution des courbes, statistiques et systèmes concernés ;
10. apparition de deux nouvelles rumeurs pour le mois `M + 1`.

Ce point doit être validé avec l’architecture réelle de `ServicePassageMensuel`.

L’objectif est que le joueur puisse connaître l’événement avant que son impact mensuel ne soit définitivement enregistré.

---

## 5. Probabilité de confirmation

### 5.1 Principe

La fiabilité de la source est le facteur principal.

La probabilité finale de confirmation ne doit pas être saisie au hasard ni calculée deux fois.

Modèle proposé :

```text
P_confirmation = clamp(
    fiabiliteSource + ajustementEvenement,
    probabiliteMinimale,
    probabiliteMaximale
)
```

Valeurs initiales recommandées :

```text
probabiliteMinimale = 0,05
probabiliteMaximale = 0,95
```

Exemples :

```text
Source fiable : 0,80
Événement rare : -0,20
Probabilité finale : 0,60
```

```text
Source peu fiable : 0,30
Événement courant : +0,10
Probabilité finale : 0,40
```

### 5.2 Pourquoi ne pas multiplier systématiquement

Une formule comme :

```text
probabilitéBase × fiabilitéSource
```

risque de rendre une grande partie des événements presque impossibles.

Exemple :

```text
0,40 × 0,30 = 0,12
```

Avec deux rumeurs par mois, cela produirait trop peu d’événements confirmés pour alimenter correctement le gameplay.

### 5.3 Audit obligatoire avant validation

Codex doit établir :

- la distribution actuelle des fiabilités ;
- le nombre moyen d’événements attendus par an ;
- la fréquence estimée par catégorie ;
- la fréquence estimée par importance ;
- les événements trop rares ;
- les événements trop fréquents ;
- les sources dominantes ;
- les événements aux impacts extrêmes ;
- les doublons.

La formule finale et les ajustements ne doivent être figés qu’après cet audit.

---


### 5.4 Compatibilité obligatoire entre source et catégorie

Une source ne peut être associée qu’à une rumeur appartenant à l’un de ses domaines déclarés.

Exemples :

```text
Source familiale -> rumeur Personnels uniquement
Source RH ou collègue -> rumeur Professionnels uniquement
Presse financière -> rumeur Boursiers uniquement
Presse généraliste -> uniquement les domaines explicitement déclarés
```

La sélection d’une source doit donc suivre cette règle :

```text
categorieRumeur ∈ domainesSource
```

Le moteur doit filtrer les sources compatibles avant tout tirage.

Il est interdit de :

- choisir une source familiale pour une rumeur boursière ;
- choisir une source professionnelle pour une rumeur personnelle ;
- attribuer une source hors domaine par simple hasard ;
- corriger silencieusement une incompatibilité de données.

Si aucune source compatible n’existe pour une rumeur, cette rumeur doit être considérée comme invalide lors de l’audit du catalogue.

Des tests doivent vérifier :

- la compatibilité source/catégorie ;
- l’impossibilité de produire une association incohérente ;
- la stabilité du filtrage avec une graine déterministe ;
- le signalement explicite des rumeurs sans source valide.

---

## 6. Sélection aléatoire

Le moteur doit utiliser une abstraction testable, par exemple :

```text
IGenerateurAleatoire
```

Deux implémentations sont attendues :

- générateur normal pour le jeu ;
- générateur déterministe avec graine pour les tests.

Contraintes :

- deux rumeurs distinctes par mois ;
- exclusion des rumeurs déjà en attente ;
- prévention des doublons immédiats ;
- possibilité future de pondérer par catégorie, source ou rareté ;
- reproduction exacte d’une séquence avec la même graine.

---

## 7. Modèles métier recommandés

### 7.1 SourceActualite

```text
id
nom
type
domaines
fiabilite
```

### 7.2 DefinitionEvenement

```text
id
categorie
titre
importance
messageEvenement
impacts
ajustementProbabilite
repetable
delaiMinimumReapparition
sourcesEligibles
```

### 7.3 DefinitionRumeur

```text
id
evenementId
titreRumeur
messageRumeur
sourceId
```

### 7.4 RumeurPartie

```text
rumeurId
evenementId
sourceId
moisApparition
moisResolution
probabiliteCalculee
tirageEffectue
etat
```

États possibles :

```text
EnAttente
Confirmee
Invalidee
```

### 7.5 EvenementPartie

```text
evenementId
moisConfirmation
moisDebutImpact
duree
etat
impactsAppliques
```

États possibles :

```text
Confirme
Actif
Termine
```

---

## 8. Actualités

L’application Actualités doit afficher progressivement uniquement ce qui s’est réellement produit dans la partie.

### 8.1 Affichage d’une rumeur

- date ;
- badge `Rumeur` ;
- titre ;
- source ;
- niveau qualitatif de fiabilité ;
- texte public ;
- état `En attente`, `Confirmée` ou `Non confirmée`.

### 8.2 Affichage d’un événement

- date ;
- badge `Événement confirmé` ;
- titre ;
- catégorie ;
- importance ;
- source de la rumeur d’origine ;
- message de confirmation ;
- paramètres publics ;
- durée éventuelle ;
- état actif ou terminé.

Les coefficients techniques internes ne doivent pas obligatoirement être affichés au joueur.

### 8.3 Historique

L’historique doit être trié du plus récent au plus ancien et rester présent pendant toute la partie.

---

## 9. Impacts

### 9.1 Principe général

Chaque impact doit être typé.

Le moteur ne doit pas reposer sur une longue chaîne de conditions textuelles.

Exemples de familles d’impacts :

- marché boursier ;
- environnement économique ;
- statistiques du joueur ;
- immobilier ;
- salariat ;
- entrepreneuriat futur.

### 9.2 Bourse

Un impact boursier doit préciser :

```text
actifId
variation
duree
modeApplication
```

Modes possibles :

```text
VariationImmediate
CoefficientTemporaire
TendanceMensuelle
Volatilite
```

### 9.3 Statistiques du joueur

Exemples :

```text
energie
santeMentale
sante
bonheur
reseau
argent
```

Les noms réellement supportés doivent être vérifiés avec `DonneesJoueur`.

### 9.4 Entrepreneuriat futur

Même si aucun événement entrepreneurial complet n’existe encore, l’architecture doit pouvoir recevoir plus tard :

```text
secteurCible
publicCible
niveauSupport
bonusReussite
bonusDemande
bonusRevenus
duree
```

Cette extension ne doit pas obliger à reconstruire le moteur général.

---

## 10. Cumul des événements

Deux événements actifs peuvent se cumuler.

### 10.1 Variations absolues

Les variations absolues s’additionnent :

```text
-5 énergie + -10 énergie = -15 énergie
```

### 10.2 Coefficients

Les coefficients se multiplient :

```text
×1,10 puis ×0,90 = ×0,99
```

### 10.3 Durées

Chaque événement conserve :

- son propre début ;
- sa propre durée ;
- sa propre date d’expiration.

L’expiration d’un événement ne doit pas retirer l’effet encore actif d’un autre.

### 10.4 Protection contre la double application

Chaque événement doit enregistrer ce qui a été appliqué afin d’éviter :

- l’application deux fois au même mois ;
- l’application au chargement de l’interface ;
- l’application au rechargement d’une scène ;
- l’application au moment du snapshot.

---

## 11. Snapshots

Les snapshots doivent contenir :

- l’historique des rumeurs ;
- les rumeurs en attente ;
- les événements confirmés ;
- les événements actifs ;
- les événements terminés ;
- les impacts encore actifs ;
- la graine ou l’état nécessaire à la reproduction.

Les copies doivent être profondes.

Modifier un snapshot ne doit jamais modifier la partie réelle.

---

## 12. What If : objectif

Le What If doit comparer deux patrimoines :

1. le patrimoine réel obtenu par les choix du joueur ;
2. le patrimoine alternatif obtenu par une stratégie boursière optimisée.

Le moteur alternatif possède exactement les mêmes connaissances que le joueur.

Il connaît :

- les événements déjà confirmés ;
- les prix déjà observés ;
- l’historique disponible jusqu’au mois courant ;
- le portefeuille alternatif courant.

Il ignore :

- toutes les rumeurs ;
- leur probabilité ;
- leur future confirmation ;
- les futurs événements ;
- les futurs prix réels.

---

## 13. What If : arbre de choix

### 13.1 Pourquoi un arbre

L’arbre permet de comparer des suites de décisions et non seulement un actif isolé.

Chaque nœud représente :

```text
mois
patrimoine
liquidites
portefeuille
prixConnus
evenementsConfirmes
impactsActifs
historiqueDisponible
```

Chaque branche représente une décision d’investissement.

### 13.2 Problème de combinatoire

Un arbre complet est impossible.

Avec six décisions seulement sur 480 mois :

```text
6^480
```

chemins seraient théoriquement possibles.

Il faut donc utiliser un arbre élagué.

---

## 14. Méthode retenue : recherche en faisceau

La méthode principale sera une **recherche en faisceau** avec horizon glissant.

### 14.1 Principe

À chaque mois :

1. construire un ensemble de portefeuilles candidats ;
2. projeter leur évolution sur un horizon limité ;
3. noter chaque chemin ;
4. ne conserver que les meilleurs ;
5. appliquer uniquement la première décision du meilleur chemin ;
6. recommencer le mois suivant avec les nouvelles informations.

### 14.2 Horizon initial recommandé

```text
3 mois
```

Cette valeur devra être rendue configurable.

### 14.3 Largeur du faisceau

Valeur initiale recommandée :

```text
20 à 30 meilleurs chemins conservés à chaque niveau
```

### 14.4 Portefeuilles candidats

Exemples :

- conserver le portefeuille ;
- tout placer en liquidités ;
- portefeuille prudent ;
- portefeuille équilibré ;
- portefeuille offensif ;
- concentration sur un actif ;
- rééquilibrage partiel ;
- conservation du meilleur actif connu ;
- diversification sur les deux meilleurs actifs connus.

Les répartitions doivent être discrétisées pour éviter une infinité de branches.

Exemple :

```text
0 %, 25 %, 50 %, 75 %, 100 %
```

---

## 15. Fonction d’évaluation

Chaque chemin reçoit un score :

```text
Score =
patrimoinePrevu
- lambdaRisque × risque
- lambdaDrawdown × baisseMaximale
- coutsTransactions
```

Variables :

- `patrimoinePrevu` : valeur finale projetée ;
- `risque` : volatilité du chemin ;
- `baisseMaximale` : perte maximale intermédiaire ;
- `coutsTransactions` : frais et pénalités de rééquilibrage ;
- `lambdaRisque` : aversion au risque ;
- `lambdaDrawdown` : pénalité de chute importante.

Le patrimoine reste l’objectif principal.

Le risque évite une stratégie constamment placée à 100 % sur l’actif le plus volatil.

---

## 16. Prévision sans connaissance du futur

Le moteur ne doit jamais utiliser les vrais prix futurs pour choisir.

Il peut utiliser :

- moyenne historique ;
- tendance récente ;
- volatilité récente ;
- effets publics des événements confirmés ;
- durée restante des événements actifs ;
- corrélations déjà observées ;
- paramètres connus des actifs.

Après le choix, le véritable résultat du mois est appliqué à la stratégie alternative.

Le mois suivant, l’arbre est recalculé.

---

## 17. Explication des décisions

Pour chaque mois, le What If doit pouvoir présenter :

```text
Événement connu
Choix réel du joueur
Choix alternatif
Raison du choix alternatif
Risque estimé
Patrimoine réel
Patrimoine alternatif
Écart cumulé
```

Exemple :

```text
Événement confirmé : pénurie de silicium
Choix du joueur : CAC 40
Choix alternatif : Nvidia
Raison : effet positif confirmé sur Nvidia et rendement attendu supérieur
Écart cumulé : +2 430 €
```

Le modèle doit être explicable et testable.

---

## 18. Courbes du What If

Le graphique final doit afficher au minimum :

- courbe `Patrimoine réel` ;
- courbe `Patrimoine optimisé selon événements connus`.

Les deux courbes doivent :

- partir du même patrimoine initial ;
- utiliser les mêmes dates ;
- utiliser les mêmes événements confirmés ;
- ignorer les rumeurs ;
- ne jamais modifier la partie réelle ;
- rester reproductibles.

---

## 19. Découpage en branches

### Branche 1

```text
feature/evenements-catalogue-orchestration
```

Contenu :

- audit des JSON ;
- catalogue ;
- sources ;
- rumeurs ;
- deux rumeurs par mois ;
- résolution au mois suivant ;
- probabilités ;
- historique ;
- Actualités ;
- snapshots ;
- tests.

### Branche 2

```text
feature/evenements-impacts-jeu
```

Contenu :

- impacts boursiers ;
- impacts statistiques ;
- cumul ;
- durée ;
- expiration ;
- prévention des doubles applications ;
- extension future Entrepreneuriat ;
- tests.

### Branche 3

```text
feature/what-if-evenements-bourse
```

Contenu :

- arbre de décision ;
- recherche en faisceau ;
- horizon glissant ;
- fonction de score ;
- deux patrimoines ;
- deux courbes ;
- explications ;
- tests.

---

## 20. Tests obligatoires

### Catalogue

- 200 identifiants uniques ;
- toutes les sources résolues ;
- catégories valides ;
- probabilités bornées ;
- impacts valides ;
- actifs inconnus signalés ;
- données invalides refusées proprement.

### Rumeurs

- exactement deux rumeurs par mois ;
- rumeurs distinctes ;
- aucune rumeur en attente sélectionnée deux fois ;
- résolution exactement au mois suivant ;
- confirmation reproductible avec une graine ;
- aucune mutation du jeu avant confirmation.

### Événements

- impact appliqué le mois de confirmation ;
- aucun impact en cas d’invalidation ;
- cumul correct ;
- expiration correcte ;
- aucun double déclenchement ;
- copie profonde dans les snapshots.

### Actualités

- apparition chronologique ;
- rumeur affichée ;
- confirmation affichée ;
- invalidation visible ;
- données publiques distinctes des paramètres internes.

### What If

- aucune lecture des rumeurs ;
- aucune connaissance du futur ;
- aucune mutation de la partie réelle ;
- mêmes données initiales ;
- arbre reproductible ;
- élagage stable ;
- calcul du patrimoine exact ;
- deux courbes alignées ;
- explication disponible pour chaque décision.

---

## 21. Points à auditer avant implémentation

1. Qualité réelle des 200 événements.
2. Qualité des 19 sources.
3. Distribution des fiabilités.
4. Événements sans impact.
5. Actifs inconnus du moteur.
6. Variations excessives.
7. Doublons.
8. Catégories incompatibles.
9. Règles de récurrence.
10. Durées manquantes.
11. Source de vérité des courbes.
12. Ordre réel du passage mensuel.
13. Copie dans les snapshots.
14. Chargement actuel d’ActualitesUI.
15. Calcul actuel de l’Optimizer et de ServicePatrimoine.

---

## 22. Décisions validées

- Deux rumeurs distinctes apparaissent chaque mois.
- Une rumeur est résolue le mois suivant.
- Sa probabilité dépend principalement de la fiabilité de sa source.
- Aucun événement n’a d’autre condition de réalisation.
- Une rumeur n’a aucun impact.
- Un événement confirmé agit pendant le tour de sa confirmation.
- Deux événements similaires peuvent cumuler leurs effets.
- Le What If ignore les rumeurs.
- Le What If possède les mêmes informations que le joueur.
- Le What If utilise un arbre de choix élagué.
- La méthode retenue est une recherche en faisceau à horizon glissant.
- Le modèle doit expliquer ses décisions.
- Le moteur doit rester extensible aux futurs événements Entrepreneuriat.

---

## 23. Backlog après ce chantier

Après les événements et le What If événementiel :

1. vérification Banque / salaire / HUD ;
2. fondations financières Entrepreneuriat ;
3. compte professionnel ;
4. investisseurs et dilution ;
5. multi-entreprises ;
6. vente et suppression ;
7. tableau secteur / public / support ;
8. Immobilier ;
9. nettoyage architectural ;
10. tests de régression globale.
