# Game Design Document — *La Villa ou la Banqueroute*

**Version :** 1.0 — Document de conception complet  
**Statut :** document de référence pour conception, production, communication et promotion  
**Plateforme cible :** PC  
**Moteur :** Unity 2D  
**Genre :** simulation de vie, gestion financière, stratégie tour par tour  
**Mode :** solo  
**Nom du projet :** *La Villa ou la Banqueroute* — titre provisoire

---

# 1. Résumé exécutif

## 1.1 Pitch court

*La Villa ou la Banqueroute* est un jeu de simulation stratégique dans lequel le joueur incarne un jeune diplômé qui entre dans la vie active et doit construire sa réussite financière sans sacrifier sa santé mentale, sa vie personnelle et ses relations. Chaque mois, il répartit son **énergie** entre carrière, investissement, entrepreneuriat, réseau, famille et repos. Cette énergie représente sa capacité réelle à s’impliquer dans chaque domaine, davantage qu’un simple temps horaire. Chaque choix produit des conséquences immédiates et à long terme.

## 1.2 Pitch promotionnel

Dans *La Villa ou la Banqueroute*, le joueur ne cherche pas seulement à devenir riche : il cherche à réussir sa vie sans se détruire. Inspiré des jeux de management comme *Football Manager*, des jeux de vie comme *BitLife* et des simulateurs économiques, le jeu propose une expérience de gestion réaliste, profonde et accessible.

Le joueur commence comme jeune diplômé en informatique. Mois après mois, il doit arbitrer entre travail salarié, investissements, création de startup, vie personnelle, santé mentale, dettes, opportunités, crises et actualités économiques, en décidant où placer son énergie. Une promotion peut coûter de l’énergie familiale. Une startup peut exploser ou ruiner plusieurs années d’efforts. Une crise économique peut détruire un portefeuille. Une mauvaise gestion du stress peut mener au burn-out. Une news publique sur le Bitcoin, l’immobilier ou l’intelligence artificielle peut devenir une opportunité décisive pour les joueurs attentifs.

Le cœur de l’expérience repose sur une question simple : **jusqu’où es-tu prêt à aller pour réussir ?**

## 1.3 Vision du jeu

Le jeu doit donner au joueur l’impression de piloter une vie entière comme un projet stratégique. Il ne s’agit pas d’un simple idle game ou d’un simulateur financier froid. L’ambition est de créer un jeu de gestion vivant, dans lequel l’argent, la carrière, la santé mentale, la famille et les opportunités sont liés.

Le joueur doit ressentir que :

- son énergie est sa ressource la plus rare ;
- les décisions faciles à court terme peuvent coûter cher à long terme ;
- la réussite financière sans équilibre personnel n’est pas forcément une victoire ;
- les actualités économiques peuvent devenir des opportunités ;
- chaque partie raconte une trajectoire différente.

---

# 2. Identité du jeu

## 2.1 Titre provisoire

**La Villa ou la Banqueroute**

Le titre exprime immédiatement la tension centrale du jeu : réussite ou échec, ambition ou chute, ascension sociale ou effondrement financier. Il fonctionne bien pour une version étudiante/prototype car il est mémorable et légèrement provocateur.

Des titres alternatifs pourront être testés plus tard :

- *Millionnaire ou Burn-out* ;
- *Vie Active* ;
- *Le Prix du Succès* ;
- *Temps, Argent, Santé* ;
- *Capital Vie* ;
- *La Vie en Solde*.

## 2.2 Genre

Le jeu mélange plusieurs genres :

- simulation de vie ;
- gestion financière ;
- stratégie tour par tour ;
- jeu de management ;
- simulation économique ;
- jeu narratif systémique.

## 2.3 Public cible

Le jeu vise principalement :

- les étudiants et jeunes adultes ;
- les joueurs intéressés par la gestion, la finance, la carrière et l’entrepreneuriat ;
- les amateurs de jeux de simulation accessibles mais profonds ;
- les joueurs de *Football Manager*, *BitLife*, *Game Dev Tycoon*, *The Sims*, *Democracy*, *Capitalism Lab* ou jeux de tycoon ;
- les joueurs curieux des questions de réussite, d’investissement, de travail et d’équilibre de vie.

## 2.4 Promesse joueur

Le jeu promet au joueur :

> “Construis ta réussite financière, mais fais attention : ton énergie, ta santé mentale et tes proches valent aussi cher que ton argent.”

## 2.5 Expérience émotionnelle recherchée

Le joueur doit ressentir :

- l’ambition de progresser ;
- la pression des arbitrages ;
- la satisfaction d’un bon choix ;
- la frustration constructive d’un mauvais investissement ;
- l’attachement à sa trajectoire de vie ;
- la tension entre réussite économique et équilibre personnel ;
- l’envie de rejouer pour optimiser une autre trajectoire.

---

# 3. Piliers de conception

## 3.1 Pilier 1 — L’énergie est la ressource centrale

Le jeu ne doit pas seulement demander au joueur comment dépenser son argent. Il doit surtout lui demander comment utiliser son énergie mensuelle.

Cette énergie représente la capacité physique, mentale et émotionnelle du personnage à s’investir dans ses activités. Elle ne correspond pas seulement à des heures disponibles : deux personnes peuvent avoir le même temps libre, mais pas la même énergie pour travailler, entreprendre, gérer leurs proches ou analyser les marchés.

Chaque mois, le joueur dispose d’un capital limité de points d’énergie. Il doit choisir entre :

- travailler ;
- construire son réseau ;
- développer une startup ;
- analyser les marchés ;
- investir son énergie dans sa famille ;
- se reposer ;
- gérer sa vie personnelle.

Le joueur ne peut pas tout faire. La frustration positive vient de cette limite d’énergie.

## 3.2 Pilier 2 — La réussite a un coût

Chaque stratégie doit avoir un prix.

- Le salariat est stable mais peut devenir lent et stressant.
- L’entrepreneuriat peut enrichir vite mais consomme de l’énergie, de l’argent et de la santé mentale.
- L’investissement peut générer de gros gains mais expose aux pertes.
- La famille protège mentalement mais demande de l’énergie.
- Le réseau ne rapporte pas tout de suite mais débloque des opportunités.
- La dette accélère certains projets mais fragilise le joueur.

## 3.3 Pilier 3 — Réalisme accessible

Le jeu doit être crédible sans être lourd. Les mécaniques doivent s’inspirer du réel, mais rester compréhensibles.

Exemples :

- salaire annuel et évolution de carrière plausibles ;
- crédit, dette, solvabilité et mensualités ;
- marchés financiers simplifiés ;
- immobilier par grandes villes ;
- événements économiques cohérents ;
- vie personnelle impactée par les choix du joueur.

## 3.4 Pilier 4 — Les événements racontent la partie

Le jeu doit générer des événements qui donnent une identité à chaque partie.

Deux familles d’informations doivent être séparées :

1. **Événements du jeu** : privés, personnels, directement liés au joueur.
2. **News publiques** : actualités économiques, sociales ou technologiques influençant les marchés et opportunités.

Cette séparation rend l’interface plus claire et plus crédible.

## 3.5 Pilier 5 — Comprendre ses erreurs

Le mode “what-if” est un élément différenciant. Le joueur doit pouvoir comprendre ce qu’il aurait pu faire autrement.

Le jeu ne doit pas seulement sanctionner. Il doit expliquer, comparer et donner envie de rejouer.

---

# 4. Concept complet

## 4.1 Situation de départ

Le joueur commence entre 22 et 25 ans, juste après l’obtention d’un diplôme d’ingénieur en informatique. Pour la première version complète, le profil de départ standard est :

- jeune diplômé ;
- secteur informatique ;
- premier emploi en région parisienne ;
- revenus modestes mais prometteurs ;
- peu de patrimoine ;
- dette éventuelle faible ou nulle selon le scénario ;
- réseau limité ;
- santé mentale correcte ;
- famille présente mais à entretenir.

Plus tard, plusieurs profils de départ pourront exister :

- diplômé sans réseau ;
- alternant devenu salarié ;
- freelance précoce ;
- étudiant endetté ;
- héritier modeste ;
- autodidacte sans diplôme ;
- profil étranger arrivant en France.

## 4.2 Objectif général

L’objectif n’est pas une victoire unique ni une note finale. Le joueur cherche à atteindre l’indépendance financière ou une situation de vie suffisamment stable, puis le jeu lui présente un bilan détaillé de sa trajectoire.

Ce bilan analyse notamment :

- patrimoine net ;
- stabilité financière ;
- niveau de dette ;
- revenus passifs ;
- réussite professionnelle ;
- réussite entrepreneuriale ;
- qualité des investissements ;
- santé mentale ;
- équilibre familial ;
- capacité à gérer les crises.

Le joueur ne reçoit pas un score final chiffré. Il obtient une lecture graphique et narrative de sa partie, afin de comprendre ce qu’il a réussi, ce qu’il a sacrifié et ce qu’il aurait pu faire autrement.

## 4.3 Durée d’une partie

Un tour représente **un mois**.

La partie complète peut durer jusqu’à environ **500 tours**, ce qui correspond à une vie active entière, de 23 ans à environ 65 ans.

Le joueur peut aussi choisir des formats plus courts :

- Mode démo : 36 mois ;
- Mode carrière courte : 120 mois ;
- Mode vie complète : 500 mois.

## 4.4 Conditions de fin

La partie peut se terminer par :

- fin naturelle de carrière ;
- faillite ;
- santé mentale à zéro pendant trop longtemps ;
- impossibilité de payer les charges ;
- retraite anticipée ;
- objectif financier atteint ;
- abandon volontaire.

## 4.5 Ton général

Le ton est réaliste, sérieux et légèrement satirique.

Le jeu doit pouvoir parler de sujets forts comme la dette, le burn-out, le décès d’un proche ou la faillite, mais sans être morbide. L’humour doit venir de certaines situations, des formulations d’événements et des contrastes absurdes de la vie moderne, pas d’une moquerie gratuite.

---

# 5. Boucle de gameplay

## 5.1 Boucle principale mensuelle

Chaque mois suit une séquence claire :

1. Début du mois.
2. Affichage des événements privés.
3. Affichage des news publiques.
4. Mise à jour des revenus et dépenses.
5. Évolution des marchés et investissements.
6. Choix de répartition de l’énergie.
7. Application des effets des choix.
8. Mise à jour carrière, startup, banque, famille, santé mentale et réseau.
9. Sauvegarde de l’état pour le mode what-if.
10. Passage au mois suivant.

## 5.2 Boucle courte joueur

La boucle ressentie par le joueur est :

> Lire → arbitrer → valider → observer les conséquences → adapter sa stratégie.

## 5.3 Boucle longue joueur

Sur plusieurs années, la boucle devient :

> Construire une carrière → accumuler du capital → prendre des risques → gérer les crises → optimiser sa trajectoire → atteindre une forme de liberté.

## 5.4 Bouton de passage du temps

Le jeu utilise un bouton central inspiré du principe de *BitLife*, mais adapté au rythme mensuel.

Bouton principal : **Passer 1 mois**

Bouton secondaire possible : **Simuler +1 an**

Le bouton “Passer 1 mois” doit rester un élément fort de l’interface. Il représente le rythme du jeu et donne une satisfaction immédiate.

---

# 6. Interface globale

## 6.1 Direction UX générale

L’interface doit être pensée pour PC. Elle doit s’inspirer davantage de *Football Manager* que d’un jeu mobile.

Caractéristiques :

- menu latéral fixe ;
- panneaux d’information ;
- tableaux ;
- rapports ;
- jauges ;
- graphiques ;
- centre d’information ;
- boutons de décision ;
- navigation par onglets.

## 6.2 Écran d’accueil

L’accueil représente le “bureau de gestion” du joueur.

Il doit contenir :

- résumé financier ;
- santé mentale ;
- famille ;
- stress ;
- réseau ;
- XP ;
- carrière actuelle ;
- portefeuille ;
- startup active ;
- événements privés récents ;
- news publiques récentes ;
- raccourcis vers les onglets.

## 6.3 Menu principal

Onglets principaux :

1. Accueil
2. Salariat
3. Investissement
4. Banque
5. Entrepreneuriat
6. Vie personnelle & santé
7. Réseau
8. Actualités
9. What-if
10. Historique / statistiques
11. Paramètres

## 6.4 Centre d’information

Le centre d’information ne doit pas être constamment ouvert sur l’écran. Pour éviter une interface trop chargée, il fonctionne comme un **panneau déroulant** ou un **tiroir latéral**.

Par défaut, l’écran principal affiche seulement des icônes, pastilles ou alertes compactes :

- icône “événements privés” avec un nombre de notifications ;
- icône “news publiques” avec un nombre de titres importants ;
- indicateur visuel en cas d’événement grave ou urgent.

Quand le joueur clique sur l’un de ces raccourcis, le panneau se déroule et affiche les détails. Cette solution mélange les deux directions visuelles précédentes :

- un **bureau PC avec raccourcis** vers les grands onglets ;
- une interface plus **Football Manager**, dense et structurée, quand le panneau est ouvert.

Le centre d’information est séparé en deux parties.

### Événements du jeu

Informations privées liées directement au personnage.

Exemples :

- “Votre mère est décédée.”
- “Votre manager vous impose une surcharge de travail.”
- “Votre conjoint vous reproche votre absence.”
- “Un ancien camarade vous propose une opportunité.”
- “Votre santé mentale se dégrade.”

### News publiques

Informations économiques ou sociales visibles par tous.

Exemples :

- “Le Bitcoin explose après une adoption institutionnelle.”
- “Les taux immobiliers augmentent.”
- “L’IA médicale attire des capitaux.”
- “Le marché immobilier ralentit à Paris.”
- “Une crise énergétique secoue l’Europe.”

Cette séparation est essentielle pour la lisibilité et la crédibilité. Les événements privés concernent la vie du personnage ; les news publiques concernent le monde extérieur et influencent les marchés, les startups et les opportunités.

---

# 7. Variables du jeu

## 7.1 Variables principales

| Variable | Description | Impact gameplay |
|---|---|---|
| Argent disponible | Solde utilisable immédiatement | Achat, remboursement, investissement |
| Patrimoine net | Actifs moins dettes | Bilan financier et indépendance financière |
| Revenus mensuels | Salaire + revenus passifs + startup | Stabilité financière |
| Dépenses mensuelles | Loyer, charges, crédits, famille | Risque de faillite |
| Dette | Montant à rembourser | Stress, solvabilité |
| Santé mentale | Équilibre psychologique | Performance, événements, fin possible |
| Stress | Pression accumulée | Burn-out, famille, santé mentale |
| Famille | Qualité de la vie personnelle | Soutien, crises, bilan personnel |
| Réseau | Contacts utiles | Opportunités, emploi, startup, investissement |
| XP globale | Progression du joueur | Promotions, décisions, efficacité |
| Énergie mensuelle | Points d’implication à répartir chaque mois | Actions disponibles et intensité d’investissement personnel |

## 7.2 Ressource énergie

Chaque mois, le joueur reçoit **100 points d’énergie**.

Ces points ne représentent pas seulement du temps horaire. Ils représentent la capacité réelle du personnage à fournir un effort utile dans chaque domaine : concentration, motivation, charge mentale, disponibilité émotionnelle et endurance.

Ces points sont répartis entre :

- travail salarié ;
- startup ;
- investissement / analyse marchés ;
- famille ;
- réseau ;
- repos.

## 7.3 Santé mentale

La santé mentale est une variable critique. Elle ne doit pas être décorative.

Elle influence :

- performance au travail ;
- réussite entrepreneuriale ;
- qualité des décisions ;
- probabilité d’événements négatifs ;
- capacité à supporter les crises ;
- fin de partie.

Elle est affectée par :

- stress ;
- dette ;
- événements privés ;
- pertes financières ;
- échec de startup ;
- énergie consacrée au repos ;
- famille ;
- réussites importantes.

## 7.4 Famille

La famille représente les relations personnelles, proches, couple, enfants et soutien social.

Elle influence :

- santé mentale ;
- événements privés ;
- stabilité long terme ;
- bilan final ;
- soutien en cas de crise.

## 7.5 Réseau

Le réseau influence :

- offres d’emploi ;
- opportunités d’investissement ;
- chances de réussite d’une startup ;
- accès à des mentors ;
- négociation salariale ;
- financement.

---

# 8. Système d’énergie

## 8.1 Répartition mensuelle

Le joueur répartit 100 points d’énergie avant de passer au mois suivant.

Exemple :

- Travail : 40
- Startup : 20
- Famille : 15
- Réseau : 10
- Analyse marchés : 10
- Repos : 5

Cette énergie représente l’intensité d’implication du personnage dans chaque domaine. Le joueur peut avoir “du temps” dans sa vie, mais ne pas avoir assez d’énergie mentale pour l’utiliser efficacement.

## 8.2 Effets généraux

### Travail

Augmente :

- revenus ;
- XP ;
- chance de promotion ;
- performance.

Diminue :

- santé mentale si excessif ;
- famille si trop élevé ;
- énergie disponible pour les autres domaines.

### Startup

Augmente :

- progression startup ;
- chance de succès ;
- XP entrepreneuriale implicite.

Diminue :

- santé mentale si trop élevé ;
- énergie disponible pour la famille ;
- argent si investissement requis.

### Famille

Augmente :

- bonheur familial ;
- santé mentale ;
- soutien en cas de crise.

Diminue :

- énergie disponible pour carrière et business.

### Réseau

Augmente :

- opportunités ;
- offres d’emploi ;
- bonus startup ;
- événements positifs.

### Analyse des marchés

Augmente :

- qualité des indices ;
- précision des signaux ;
- réduction du risque d’erreur.

### Repos

Augmente :

- santé mentale ;
- réduction du stress ;
- récupération d’énergie pour les mois suivants ;
- protection contre burn-out.

## 8.3 Rendements décroissants

Chaque activité a un rendement décroissant.

Exemple :

- 40 points d’énergie au travail : efficace ;
- 60 points d’énergie au travail : très productif mais stressant ;
- 80 points d’énergie au travail : gain supplémentaire faible, stress très élevé.

Cela évite les stratégies abusives.

# 9. Salariat

## 9.1 Rôle dans le jeu

Le salariat est la voie la plus stable. Il donne des revenus prévisibles et permet de construire la base financière du joueur.

## 9.2 Progression professionnelle

Le joueur commence comme jeune ingénieur informatique.

Grades possibles :

1. Junior Software Engineer
2. Software Engineer
3. Confirmed Engineer
4. Senior Software Engineer
5. Lead Developer
6. Engineering Manager
7. Head of Engineering
8. CTO salarié

## 9.3 Variables carrière

- poste ;
- salaire annuel ;
- performance ;
- XP ;
- stress professionnel ;
- ancienneté ;
- probabilité de promotion ;
- probabilité de licenciement ;
- attractivité marché.

## 9.4 Promotions

Les promotions dépendent de :

- XP ;
- performance ;
- énergie consacrée au travail ;
- santé mentale ;
- réseau ;
- contexte économique ;
- ancienneté.

## 9.5 Changement d’entreprise

Changer d’entreprise peut apporter :

- salaire plus élevé ;
- meilleure progression ;
- stress plus fort ;
- risque d’échec d’intégration ;
- opportunités réseau.

## 9.6 Événements salariat

- promotion ;
- prime exceptionnelle ;
- projet difficile ;
- manager toxique ;
- licenciement économique ;
- fermeture d’entreprise ;
- offre concurrente ;
- surcharge de travail ;
- burn-out professionnel ;
- conflit d’équipe.

---

# 10. Investissement

## 10.1 Rôle dans le jeu

L’investissement permet au joueur de faire fructifier son argent et de viser l’indépendance financière. Il doit être simple à comprendre mais suffisamment profond pour créer de vraies décisions.

## 10.2 Actifs disponibles

Actifs prévus :

- actions / indices ;
- immobilier par grandes villes françaises ;
- or ;
- liquidités ;
- crypto ;
- produits bancaires simples ;
- éventuellement obligations plus tard.

## 10.3 Marchés

Chaque actif possède :

- prix ;
- variation mensuelle ;
- volatilité ;
- risque ;
- tendance ;
- sensibilité aux news.

## 10.4 Analyse des marchés

Plus le joueur consacre d’énergie à l’analyse, plus il reçoit d’indices fiables.

Exemples :

- “Le secteur santé-tech semble sous-évalué.”
- “Le Bitcoin montre une volatilité inhabituelle.”
- “L’immobilier parisien ralentit.”
- “Les taux élevés rendent le crédit moins attractif.”

## 10.5 Risque

Le joueur peut gagner ou perdre beaucoup. La crypto doit être très volatile. L’immobilier doit être plus lent mais plus stable. Les actions doivent varier selon les news et les cycles économiques.

## 10.6 Événements investissement

- krach boursier ;
- explosion du Bitcoin ;
- bulle crypto ;
- hausse des taux ;
- baisse immobilière ;
- opportunité sur un secteur ;
- arnaque ;
- crise énergétique ;
- boom technologique.

---

# 11. Banque et dettes

## 11.1 Rôle

La banque permet au joueur d’emprunter, d’épargner et de gérer sa solvabilité.

## 11.2 Mécaniques bancaires

- compte courant ;
- épargne ;
- prêts ;
- remboursements ;
- mensualités ;
- taux d’intérêt ;
- capacité d’emprunt ;
- solvabilité.

## 11.3 Solvabilité

La solvabilité dépend de :

- revenus ;
- dépenses ;
- dette ;
- stabilité d’emploi ;
- patrimoine ;
- historique de remboursement ;
- stress financier.

## 11.4 Utilisation des prêts

Les prêts peuvent financer :

- immobilier ;
- startup ;
- investissement ;
- formation future ;
- survie financière temporaire.

## 11.5 Risque de dette

La dette peut accélérer la réussite mais augmente :

- stress ;
- risque de faillite ;
- refus bancaire ;
- vulnérabilité en cas de crise.

---

# 12. Entrepreneuriat

## 12.1 Rôle

L’entrepreneuriat est la voie à haut risque et haut potentiel. Il doit permettre de grands succès mais aussi de vrais échecs.

## 12.2 Système de combinaison

Une startup est définie par trois choix :

### Thème

- Tech
- Santé
- Écologie

### Public

- Particuliers
- Entreprises
- Institutions

### Technique

- Social / communauté
- Full digital
- Robotique / hardware

Cela crée 27 combinaisons de base.

## 12.3 Score de startup

Chaque combinaison possède un score de base. Ce score est modifié par :

- énergie consacrée ;
- argent investi ;
- réseau ;
- santé mentale ;
- news publiques ;
- XP ;
- dette ;
- contexte économique.

Formule de principe :

```text
ScoreStartup = BaseCombinaison
             + BonusTemps
             + BonusArgent
             + BonusRéseau
             + BonusNews
             + BonusXP
             + BonusSantéMentale
             - MalusStress
             - MalusDette
             - MalusMarché
```

## 12.4 Phases

Une startup passe par :

1. Idée
2. Prototype
3. Lancement
4. Premiers clients
5. Croissance
6. Rentabilité
7. Revente ou échec

## 12.5 Résultats possibles

- échec rapide ;
- stagnation ;
- petite entreprise rentable ;
- forte croissance ;
- revente ;
- licorne exceptionnelle ;
- faillite liée à un mauvais financement.

## 12.6 Événements startup

- rencontre d’un associé ;
- bug majeur ;
- investisseur intéressé ;
- concurrent agressif ;
- buzz médiatique ;
- pivot nécessaire ;
- burn-out fondateur ;
- levée de fonds ;
- échec produit.

---

# 13. Vie personnelle et santé

## 13.1 Rôle

La vie personnelle est un système stratégique à part entière. Elle ne doit pas être un simple décor.

## 13.2 Santé mentale

La santé mentale influence presque tous les systèmes. Elle baisse avec stress, surcharge, échecs et pertes. Elle remonte avec repos, stabilité, famille et réussites.

## 13.3 Famille

La famille représente :

- parents ;
- couple ;
- enfants ;
- proches ;
- soutien émotionnel.

## 13.4 Événements privés

Exemples :

- décès de la mère ;
- maladie d’un proche ;
- naissance ;
- mariage ;
- divorce ;
- dispute familiale ;
- anniversaire oublié ;
- soutien financier familial ;
- rupture ;
- tromperie ;
- solitude ;
- déménagement ;
- période de dépression ;
- regain de motivation.

## 13.5 Déclencheurs conditionnels

Certains événements privés doivent être influencés par les choix du joueur.

Exemples :

```text
Si Famille < 30 pendant 6 mois :
    risque de crise familiale fortement augmenté

Si Énergie Travail + Énergie Startup > 75 pendant 3 mois :
    risque de burn-out augmenté

Si Santé mentale < 25 :
    performance travail et startup réduite

Si Famille > 80 :
    chance de soutien familial en crise augmentée
```

---

# 14. Réseau

## 14.1 Rôle

Le réseau est une ressource lente mais puissante. Il agit comme un multiplicateur d’opportunités.

## 14.2 Sources de réseau

Le réseau augmente avec :

- énergie dédiée ;
- événements ;
- carrière ;
- conférences ;
- anciens camarades ;
- mentorat ;
- réussite professionnelle ;
- startup.

## 14.3 Effets

Un bon réseau peut :

- améliorer les offres d’emploi ;
- débloquer des investisseurs ;
- réduire le risque startup ;
- donner des opportunités d’investissement ;
- faciliter la négociation bancaire ;
- offrir du soutien lors d’une crise.

## 14.4 Types de contacts

- recruteur ;
- mentor ;
- ancien camarade ;
- investisseur ;
- associé potentiel ;
- expert métier ;
- banquier ;
- entrepreneur ;
- manager.

---

# 15. Événements et news

## 15.1 Séparation fondamentale

Le jeu distingue clairement :

## Événements du jeu

Privés, personnels, liés au personnage.

Exemples :

- “Votre mère est décédée.”
- “Votre manager vous surcharge.”
- “Votre conjoint menace de partir.”
- “Un ami vous propose de rejoindre son projet.”
- “Vous tombez malade.”

## News publiques

Économiques, technologiques ou sociales. Elles influencent les marchés et opportunités.

Exemples :

- “Le Bitcoin explose.”
- “L’IA médicale attire des capitaux.”
- “Les taux immobiliers augmentent.”
- “Le marché immobilier ralentit à Lyon.”
- “Une pandémie perturbe l’économie.”

## 15.2 Probabilités

Les événements ne doivent pas être totalement arbitraires. Ils dépendent des variables.

Exemples :

- dette élevée → stress financier ;
- famille basse → crise familiale ;
- réseau élevé → opportunité ;
- énergie investie dans l’analyse des marchés élevée → meilleurs signaux ;
- santé mentale basse → burn-out ;
- exposition crypto élevée → impact fort des news crypto.

## 15.3 Fenêtre des news

Une news publique a généralement une durée d’effet de 4 mois.

Pendant cette fenêtre, elle peut :

- booster un secteur ;
- réduire certains actifs ;
- modifier les taux ;
- créer une opportunité startup ;
- changer le risque marché.

---

# 16. Mode What-if

## 16.1 Concept

Le mode what-if permet au joueur de comprendre ce qu’il aurait pu faire autrement.

Il analyse une période récente, par exemple les 10 derniers mois, et compare les choix du joueur avec d’autres stratégies.

## 16.2 Objectif joueur

Le joueur doit recevoir des explications comme :

- “Tu as placé trop d’énergie dans le travail et négligé le repos.”
- “Un meilleur choix aurait été d’augmenter l’énergie consacrée à l’analyse de marché avant d’acheter du Bitcoin.”
- “Tu aurais réduit le risque de divorce en consacrant 10 points d’énergie de plus à la famille.”
- “Ta startup avait de meilleures chances si tu avais exploité la news IA médicale.”

## 16.3 Fonctionnement V1

Le système peut commencer avec une simulation de scénarios :

1. enregistrer l’état du joueur ;
2. générer plusieurs allocations alternatives ;
3. simuler 10 mois ;
4. comparer argent, santé mentale, famille, risque et progression vers l’indépendance financière ;
5. afficher les meilleurs scénarios.

## 16.4 Fonctionnement avancé

Plus tard, le mode peut utiliser :

- modèles de recommandation ;
- prédiction de marché ;
- classification du risque ;
- optimisation multi-objectifs ;
- visualisation comparative.

---

# 17. Bilan de fin de partie et graphiques

## 17.1 Principe

Le jeu n’utilise pas de score final chiffré. L’objectif est d’éviter de réduire toute la trajectoire du joueur à une note unique.

La partie se conclut lorsque le joueur atteint l’indépendance financière, prend sa retraite, tombe en faillite, ou arrive à une autre condition de fin majeure.

À la fin, le jeu affiche un **bilan détaillé** sous forme de graphiques et de commentaires explicatifs. Le joueur doit comprendre ce qu’il a construit, ce qu’il a sacrifié, et comment ses décisions ont influencé sa trajectoire.

## 17.2 Bilan financier

Graphiques possibles :

- évolution du patrimoine net ;
- revenus mensuels dans le temps ;
- part salaire / investissement / startup ;
- évolution de la dette ;
- progression vers l’indépendance financière ;
- rendement des actifs.

## 17.3 Bilan personnel

Graphiques possibles :

- santé mentale sur la durée ;
- stress moyen par période ;
- bonheur familial ;
- nombre de crises personnelles ;
- périodes de burn-out ou de surcharge.

## 17.4 Bilan professionnel et entrepreneurial

Graphiques possibles :

- progression salariale ;
- changements de poste ;
- évolution du réseau ;
- progression des startups ;
- argent investi dans l’entrepreneuriat ;
- résultats des projets lancés.

## 17.5 Interprétation finale

Au lieu d’un score, le jeu donne une lecture qualitative de la partie.

Exemples de profils de fin :

- Indépendance financière équilibrée ;
- Millionnaire épuisé ;
- Entrepreneur résilient ;
- Investisseur prudent ;
- Carrière stable ;
- Liberté financière obtenue au prix de la vie personnelle ;
- Patrimoine solide mais santé mentale fragile ;
- Trajectoire risquée mais gagnante.

Ces profils ne sont pas des notes. Ils servent à raconter la vie simulée du joueur et à donner envie de recommencer avec une autre stratégie.

# 18. Direction artistique

## 18.1 Style visuel

Le style doit être :

- PC ;
- lisible ;
- semi-réaliste ;
- légèrement cartoon ;
- inspiré jeu de management ;
- dense mais clair.

## 18.2 Inspirations

- *Football Manager* pour les menus, rapports, tableaux, densité et interface PC.
- *BitLife* pour l’idée simple du passage du temps.
- *Game Dev Tycoon* pour l’entrepreneuriat accessible.
- Dashboards financiers modernes pour les graphiques.
- Jeux de simulation de vie pour les événements personnels.

## 18.3 Interface

Couleurs recommandées :

- bleu nuit ;
- cyan ;
- vert pour positif ;
- rouge pour négatif ;
- jaune/orange pour risque ;
- gris bleuté pour information neutre.

## 18.4 Lisibilité

Le joueur doit comprendre en un regard :

- son argent ;
- son stress ;
- sa santé mentale ;
- sa famille ;
- ses opportunités ;
- les news importantes ;
- ce qu’il peut faire maintenant.

---

# 19. Son et ambiance

## 19.1 Musique

Ambiance musicale discrète, moderne, légère, plutôt “bureau de gestion / stratégie”.

Elle ne doit pas fatiguer lors de longues sessions.

## 19.2 Sons UI

Sons courts :

- clic bouton ;
- notification privée ;
- news publique ;
- gain financier ;
- perte financière ;
- événement grave ;
- passage de mois.

## 19.3 Différenciation audio

Les événements privés et les news publiques peuvent avoir des sons différents.

- événement privé : son plus intime, notification personnelle ;
- news publique : son média / breaking news.

---

# 20. Données, scraping et ML

## 20.1 Rôle des données

Le jeu doit s’appuyer sur des données économiques préparées pour renforcer le réalisme.

Données possibles :

- salaires par secteur ;
- prix immobiliers par ville ;
- taux d’intérêt ;
- inflation ;
- historique crypto ;
- indices boursiers ;
- taux d’épargne.

## 20.2 Architecture recommandée

Unity ne doit pas scraper directement.

Architecture :

```text
Python récupère et prépare les données
→ export CSV/JSON
→ Unity lit les fichiers
→ le jeu simule les effets
```

## 20.3 ML

Le ML peut servir à :

- prédire une tendance de marché ;
- générer des recommandations what-if ;
- estimer un risque ;
- évaluer une startup ;
- comparer des stratégies.

## 20.4 Priorité

Le scraping, le ML et le what-if sont importants pour l’identité du projet. Ils doivent être intégrés dans la conception complète, même si leur première version reste simple.

---

# 21. Architecture technique Unity

## 21.1 Principe

Le projet doit séparer clairement :

- données ;
- logique de gameplay ;
- interface ;
- sauvegarde ;
- modèles externes.

## 21.2 Structure proposée

```text
Assets/
  Scenes/
    MainMenu.unity
    Game.unity
  Scripts/
    Core/
      GameManager.cs
      TurnManager.cs
      SaveManager.cs
    Models/
      PlayerState.cs
      FinanceState.cs
      CareerState.cs
      StartupState.cs
      InvestmentState.cs
      FamilyState.cs
      NetworkState.cs
      NewsItem.cs
      GameEvent.cs
    Systems/
      EnergyAllocationSystem.cs
      CareerSystem.cs
      InvestmentSystem.cs
      BankingSystem.cs
      StartupSystem.cs
      FamilySystem.cs
      NetworkSystem.cs
      EventSystem.cs
      NewsSystem.cs
      WhatIfSystem.cs
      EndGameReportSystem.cs
    UI/
      DashboardUI.cs
      CareerUI.cs
      InvestmentUI.cs
      BankingUI.cs
      StartupUI.cs
      FamilyUI.cs
      NetworkUI.cs
      NewsUI.cs
      WhatIfUI.cs
  Data/
    salaries.csv
    investments.csv
    real_estate.csv
    events.json
    news.json
    startup_combinations.csv
```

## 21.3 Systèmes principaux

### TurnManager

Gère le passage d’un mois.

### EnergyAllocationSystem

Transforme les points d’énergie en effets.

### CareerSystem

Gère salariat, salaire, XP, promotions, licenciement.

### InvestmentSystem

Gère portefeuille, prix, achat, vente, risque.

### BankingSystem

Gère prêts, mensualités, solvabilité.

### StartupSystem

Gère combinaisons, phases, score, réussite.

### EventSystem

Gère événements privés.

### NewsSystem

Gère news publiques.

### WhatIfSystem

Simule des trajectoires alternatives.

### EndGameReportSystem

Produit le bilan final sous forme de graphiques et de commentaires explicatifs.

---

# 22. Sauvegarde et historique

## 22.1 Sauvegarde

Le jeu doit sauvegarder :

- état joueur ;
- mois actuel ;
- finances ;
- investissements ;
- carrière ;
- startup ;
- famille ;
- réseau ;
- événements ;
- news actives ;
- historique des décisions.

## 22.2 Historique

L’historique est important pour le what-if.

Chaque mois, le jeu enregistre :

- allocation d’énergie ;
- variables avant/après ;
- événements ;
- news ;
- décisions financières ;
- progression vers l’indépendance financière.

---

# 23. Contenu prévu

## 23.1 Événements privés

Catégories :

- santé ;
- famille ;
- carrière ;
- réseau ;
- finances personnelles ;
- startup ;
- logement ;
- crises psychologiques.

## 23.2 News publiques

Catégories :

- crypto ;
- immobilier ;
- actions ;
- taux ;
- inflation ;
- technologie ;
- santé ;
- écologie ;
- crises mondiales ;
- emploi.

## 23.3 Startups

Thèmes de départ :

- Tech ;
- Santé ;
- Écologie.

Extensions possibles :

- Éducation ;
- Finance ;
- Jeux vidéo ;
- Immobilier ;
- Industrie ;
- IA générative ;
- cybersécurité.

---

# 24. Progression et rejouabilité

## 24.1 Rejouabilité

La rejouabilité vient de :

- événements aléatoires conditionnels ;
- news différentes ;
- stratégies variées ;
- startups différentes ;
- investissements différents ;
- bilans graphiques de fin ;
- mode what-if ;
- profils de départ futurs.

## 24.2 Stratégies possibles

- carrière stable ;
- investisseur prudent ;
- entrepreneur agressif ;
- spéculateur crypto ;
- équilibre famille/travail ;
- dette et levier ;
- FIRE / indépendance financière ;
- carrière très haut salaire ;
- multi-startups.

## 24.3 Méta-progression possible

Plus tard, le jeu peut ajouter :

- succès ;
- statistiques globales ;
- scénarios ;
- défis ;
- profils débloquables ;
- classements locaux.

---

# 25. Version complète visée

## 25.1 Features essentielles du jeu complet

- simulation mensuelle jusqu’à 500 tours ;
- salariat complet ;
- banque et dette ;
- investissement multi-actifs ;
- entrepreneuriat avec phases ;
- santé mentale ;
- famille ;
- réseau ;
- événements privés ;
- news publiques ;
- bilan de fin de partie avec graphiques ;
- what-if ;
- données économiques ;
- tutoriel ;
- sauvegarde ;
- fin de partie détaillée.

## 25.2 Features avancées

- profils de départ ;
- personnalisation du personnage ;
- immobilier avancé ;
- plusieurs secteurs professionnels ;
- événements narratifs complexes ;
- ML plus avancé ;
- graphiques détaillés ;
- comparateur de trajectoires ;
- mentor virtuel.

---

# 26. Tutoriel

## 26.1 Objectif

Le tutoriel doit rendre le jeu accessible malgré sa profondeur.

## 26.2 Structure

Ordre recommandé :

1. Accueil
2. Salariat
3. Santé mentale et famille
4. Banque
5. Investissement
6. Actualités
7. Entrepreneuriat
8. Réseau
9. What-if
10. Liberté complète

## 26.3 Mentor

Un personnage mentor peut guider le joueur.

Nom temporaire : **Nora**.

Rôle : ancienne entrepreneure devenue conseillère financière.

Ton : clair, direct, légèrement ironique.

Exemple :

> “Ton diplôme t’ouvre des portes, mais il ne paiera pas ton loyer. Chaque mois, ton vrai capital, c’est ton énergie.”

---

# 27. Positionnement promotionnel

## 27.1 Pourquoi le jeu se distingue

Le jeu se distingue parce qu’il mélange :

- simulation de vie ;
- stratégie financière ;
- santé mentale ;
- entrepreneuriat ;
- données économiques ;
- analyse what-if ;
- interface de management PC.

Peu de jeux relient de manière aussi directe carrière, argent, santé mentale, famille et marchés.

## 27.2 Argumentaire court

*La Villa ou la Banqueroute* transforme la vie active en jeu de stratégie. Chaque mois, le joueur doit choisir comment investir son énergie : travailler plus, voir sa famille, analyser les marchés, construire son réseau ou lancer une startup. Mais chaque décision a un prix. Le jeu propose une simulation réaliste et accessible de la réussite moderne, où devenir riche ne suffit pas : il faut aussi rester debout.

## 27.3 Points forts à mettre en avant

- Gestion de l’énergie comme ressource principale.
- Séparation claire entre événements privés et news publiques.
- Système de carrière, banque, investissement et startup interconnectés.
- Santé mentale et famille comme mécaniques centrales.
- What-if pour comprendre ses erreurs.
- Style PC inspiré des jeux de management.
- Potentiel éducatif et stratégique.

## 27.4 Phrase d’accroche

> “Chaque mois, un choix. Chaque choix, une conséquence. La villa ou la banqueroute.”

Autres variantes :

> “Deviens riche, mais essaie de rester vivant à l’intérieur.”

> “Le vrai capital, ce n’est pas seulement l’argent. C’est ton énergie.”

> “Travaille, investis, entreprends… mais n’oublie pas d’appeler ta mère.”

---

# 28. Roadmap de production

## 28.1 Phase 1 — Prototype de simulation

Objectif : faire tourner le jeu sans contenu massif.

À faire :

- système de tours ;
- état joueur ;
- allocation d’énergie ;
- variables principales ;
- événements simples ;
- dashboard minimal.

## 28.2 Phase 2 — Interface PC

Objectif : obtenir une interface claire inspirée management.

À faire :

- menu latéral ;
- accueil ;
- centre d’information ;
- séparation événements/news ;
- bouton passer 1 mois ;
- tableaux et jauges.

## 28.3 Phase 3 — Systèmes principaux

À faire :

- salariat ;
- banque ;
- investissement ;
- famille/santé ;
- réseau.

## 28.4 Phase 4 — Entrepreneuriat

À faire :

- système 3 × 3 × 3 ;
- phases startup ;
- probabilité de réussite ;
- événements startup.

## 28.5 Phase 5 — Données et ML

À faire :

- data pipeline Python ;
- exports CSV/JSON ;
- premiers modèles ;
- intégration Unity.

## 28.6 Phase 6 — What-if

À faire :

- sauvegarde historique ;
- simulation alternative ;
- comparaison ;
- recommandations.

## 28.7 Phase 7 — Équilibrage et promotion

À faire :

- playtests ;
- équilibrage ;
- trailer prototype ;
- page de présentation ;
- screenshots ;
- build PC.

---

# 29. Répartition d’équipe recommandée

Pour une équipe de 5 :

## Membre 1 — Lead Unity / intégration

- scènes ;
- structure projet ;
- intégration des systèmes ;
- build PC.

## Membre 2 — Gameplay systems

- TurnManager ;
- TimeAllocationSystem ;
- EventSystem ;
- EndGameReportSystem.

## Membre 3 — UI/UX

- écrans ;
- tableaux ;
- centre d’information ;
- tutoriel.

## Membre 4 — Data / ML

- scraping ;
- nettoyage ;
- modèles Python ;
- exports.

## Membre 5 — Game design / contenu

- événements ;
- news ;
- équilibre ;
- textes ;
- tests.

---

# 30. Risques de production

## 30.1 Risque : trop d’ambition

Le jeu comporte beaucoup de systèmes. Il faut construire une base propre avant d’ajouter du contenu.

Solution : produire d’abord une boucle mensuelle fonctionnelle.

## 30.2 Risque : interface trop dense

L’inspiration *Football Manager* peut rendre l’interface lourde.

Solution : garder des tableaux clairs, des résumés et des couleurs d’état.

## 30.3 Risque : ML trop complexe

Le ML peut bloquer le projet si l’équipe commence par là.

Solution : d’abord un système de règles + simulation, puis ML progressivement.

## 30.4 Risque : équilibrage difficile

Les variables sont nombreuses.

Solution : logs, playtests, courbes et ajustements progressifs.

---

# 31. Définition du succès

Le projet est réussi si :

- le joueur comprend rapidement quoi faire ;
- les choix ont des conséquences visibles ;
- le bouton “Passer 1 mois” donne envie de continuer ;
- les événements privés rendent la partie personnelle ;
- les news publiques influencent vraiment les décisions ;
- l’argent n’est pas le seul objectif ;
- le joueur a envie de recommencer pour faire mieux ;
- le jeu est présentable comme un concept sérieux et original.

---

# 32. Conclusion

*La Villa ou la Banqueroute* est un jeu sur la réussite moderne. Il parle d’argent, mais aussi d’énergie, de santé mentale, de famille, d’ambition, de risque et de conséquences.

Son potentiel vient de son mélange : un jeu de gestion financière, une simulation de vie, un outil de stratégie personnelle et une expérience narrative systémique.

Le cœur du jeu peut se résumer ainsi :

```text
Énergie limitée
→ choix difficiles
→ conséquences personnelles et économiques
→ adaptation stratégique
→ trajectoire de vie unique
```

La promesse est forte : permettre au joueur de vivre une vie active complète, d’expérimenter plusieurs chemins vers la réussite, puis de comprendre ce qu’il aurait pu faire autrement.

**Phrase finale :**

> Dans ce jeu, tu peux gagner de l’argent. Mais la vraie question est : combien de ton énergie es-tu prêt à dépenser ?

