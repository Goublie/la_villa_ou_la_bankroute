# BRAINSTORMING:



Notre idée : Jeu de simulation solo, où tu incarnes un personnage personnalisable (version minimale pas de personnalisation) cherchant à atteindre l'indépendance financière. Chaque tour consomme de l'énergie ; quand elle est épuisée, la partie se termine. 



**Ressources à gérer** : Argent ;  Réseau ; XP ; Santé (physique \& mentale — si elle chute trop, tes performances sont pénalisées)



**Trois voies de progression** :



* Salariat : monter en grade grâce à l'XP, gérer son type de contrat
* Entrepreneuriat : construire une startup via des arbres de décisions embranchés (3 choix → 3 choix → 3 choix), guidé par son réseau
* Investissement : gérer un portefeuille (actions, immo, crypto) avec courbes de suivi



**Système bancaire** (belek sponso) : prêts, livret A, solde personnel



**IMPORTANT** : Tutoriel intégré présentant les onglets dans un ordre pédagogique, puis le joueur est libre.



**Mécaniques de jeu** :



* Événements aléatoires : licenciement, krach boursier, opportunité inattendue, maladie — pour forcer des décisions sous contrainte (directement modélisables avec des probabilités calibrées sur données réelles)
* Arbre de vie non-linéaire : des choix jeunes ont des répercussions tardives (ex : ne pas faire de réseau tôt = startup plus difficile à lancer)
* Mode "what if" : rejouer un moment clé avec un choix différent pour voir l'impact (super pour montrer les modèles ML en action)
* Flux d'actualités économiques (belek sponso) : Un fil de notifications simule des headlines inspirées de vrais médias (ex : "Le Monde : L'IA révolutionne la médecine"). Ces signaux sont des opportunités à fenêtre temporelle limitée : lancer une startup dans le secteur annoncé dans les prochains tours booste ses chances de succès ; investir en bourse sur le secteur concerné avant la hausse peut générer un rendement supérieur. Le joueur doit donc rester attentif et savoir lire l'info économique pour en tirer parti.
* Gestion du temps personnel : Chaque tour, le joueur dispose d'un capital temps limité à répartir entre ses différentes activités (travail salarié, startup, vie personnelle, investissements). Sur-investir dans une activité pénalise les autres : trop d'heures au bureau = startup à l'arrêt et santé en baisse ; trop de temps libre = XP professionnel stagnant.



**Partie technique :**



* Scrapping sur données (CAC 40, prix immo par ville, taux livret A par année en France…)
* Modélisation du salaire selon le secteur et l'XP (données INSEE ou LinkedeIn)

