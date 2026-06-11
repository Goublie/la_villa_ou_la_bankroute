		 /$$$$$$$$ /$$$$$$          /$$$$$$$   /$$$$$$ 
		|__  $$__//$$__  $$        | $$__  $$ /$$__  $$
		   | $$  | $$  \ $$        | $$  \ $$| $$  \ $$
		   | $$  | $$  | $$ /$$$$$$| $$  | $$| $$  | $$
		   | $$  | $$  | $$|______/| $$  | $$| $$  | $$
		   | $$  | $$  | $$        | $$  | $$| $$  | $$
		   | $$  |  $$$$$$/        | $$$$$$$/|  $$$$$$/
		   |__/   \______/         |_______/  \______/ 
   
[] Application Bourse
	[] Interface
	[] Class Portefeuille (Doit implément IPatrimoine)
	[] BourseUI (Script qui gère la logique d'affichage)
	[] Class Actif (héritage investissement ?)

[] Application Actualités
	[] Lister différents types d'actualités
	[] Ecrire quelques actulaités pour chaque type
	[] Interface 
	[] Class Mere Evenement
		[] Class Enfant EvenementType1, EvenementType2 (Action Event ?)
	[] InterfaceUI (Scipt qui gère la logique d'affichage)
	[] Breaking News

[] Application Immo
	[] Interface 
	[] Class BienImmobilier (Doit implément IPatrimoine, ? héritage investissement ?)
	[] ImmobilierUI (Scipt qui gère la logique d'affichage)

[] Application Banque
	[] Interface Crédit
	[] CreditUI (Script qui gère la logique d'affichage)
	[] Logique du crédit (Script LogiqueCredit)
		[] Proba d'acceptation
		[] Calcul des mensualités
		[] Calcul des intérêts
	

[] Application Entreprenariat
	[] EntreprenariatUI (Script qui gère la logique d'affichage)
	[] 

[] Application Salariat
	[] 
	[] 

[] Scene Jeu
	[] Boucle de la musique
	[] Affichage du mois de l'année

[] Scene Retrospection / What-if
	[] RetrospectionTableauUI (s'occupe d'afficher l'onglet avec le tableau)
	[] RetrospectionEvenementUI (s'occupe d'afficher l'onglet avec les événements passés)

[] Scene Menu
	[] Modification prefab option (prefab variant de fenêtre, petite fenêtre type erreur)
	[] 

[] Scene de Game Over
	[] Bouton Rejouer
	[] 

[] Logique de jeu
	[] Sauvegarde d'une partie
		[] Choix du type de fichier
		[] Sauvegarde de la seed
		[] Sauvegarde GameData
	[] Impôts 
	[] Déclenchement du GameOver
