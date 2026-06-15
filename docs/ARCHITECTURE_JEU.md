# Architecture du jeu

## Objectif

Le code separe l'etat sauvegardable, les regles metier, l'orchestration du
temps et les composants Unity. Une fenetre peut rester fermee sans interrompre
la valorisation du patrimoine ou l'evolution mensuelle.

La Banque et le Livret A servent de reference pour les flux financiers :
validation, debit, credit, historique, resultat d'operation et valeur
patrimoniale.

## Couches

### Donnees

`GameData` est la racine partagee entre les scenes. Il contient :

- `DonneesJoueur`, agregat de l'etat personnel et financier ;
- `DonneesEnvironnement`, variables economiques externes ;
- le calendrier ;
- les `SnapshotEtatJeu` du mode What If.

`DonneesJoueur` porte les comptes, placements fixes, ressources personnelles,
`DonneesBourse` et `DonneesEntrepreneuriat`. Ces classes ne referencent aucun
`MonoBehaviour`, texte, bouton ou prefab.

L'etat est persistant pendant la partie et copiable profondement. Une
sauvegarde durable sur disque n'est pas encore fournie. Elle devra convertir
explicitement les dictionnaires et les types polymorphes au lieu de supposer
que Unity les serialise directement.

### Modeles

Les modeles representent les concepts du jeu :

- `argent` stocke les montants signes en centimes ;
- `CompteBanquaire`, `Epargne`, `Transaction` et `Historique` portent les
  concepts bancaires ;
- `Investissement` reste un placement a rendement fixe ;
- `DefinitionActifFinancier`, `PositionBourse` et
  `ImpactEvenementMarche` portent la Bourse ;
- `ProjetEntrepreneurial` et `ProfilProjetEntrepreneurial` portent
  Entrepreneuriat ;
- `ResultatOperation` et `ResultatPassageMensuel` decrivent les commandes.

Une action n'herite pas d'`Investissement`. Elle possede une quantite, un prix
variable et un cout historique, ce qui justifie un modele distinct compose
avec les services bancaires.

### Services

Les services ne manipulent pas l'interface :

- `ServiceBanque` cree les comptes uniques, transfere, debite et credite ;
- `ServiceLivretA` lit les taux et traduit l'index absolu en mois civil ;
- `ServiceBourse` valide les ordres, gere les positions et valorise le
  portefeuille ;
- `ServiceEntrepreneuriat` applique les couts, ressources, probabilites et
  valorisations du projet ;
- `ServicePatrimoine` additionne les sources de valeur une seule fois ;
- `ServicePassageMensuel` garantit l'ordre du changement de mois ;
- `ServiceEvenementsEconomiques` route les impacts vers les services metier.

Les services retournent un `ResultatOperation`. Ils ne modifient jamais un
texte d'interface.

### Interfaces Unity

`BourseUI`, `EntrepreneuriatUI`, `EpargneUI` et `CourantUI` conservent leurs
champs serialises et leurs methodes de boutons pour proteger les references
des prefabs. Elles resolvent les dependances, appellent un service puis
rafraichissent les textes.

`GraphiqueBourseUI` reste dans la presentation : il adapte XCharts et limite
la serie aux douze derniers points connus, sans exposer les mois futurs.

`ActionPlay` est un adaptateur. Il appelle `ServicePassageMensuel`, notifie
ensuite les composants visuels et demande la scene d'introspection en janvier.

## Argent et flux

L'unite metier est le centime entier :

- `argent(100000)` represente 1 000 euros ;
- les prix historiques Bourse sont lus en euros puis convertis a la frontiere
  du service ;
- les couts de position, tresoreries et valorisations sont stockes en
  centimes ;
- les pourcentages sont documentes comme taux decimaux ou points selon le
  modele.

Un achat Bourse suit cet ordre :

1. valider l'actif, le mois et le minimum ;
2. debiter le compte courant avec `ServiceBanque` ;
3. augmenter la quantite et le capital investi ;
4. revaloriser le portefeuille ;
5. retourner le resultat.

Une vente partielle retire le cout historique au prorata de la quantite
vendue. Le cout moyen du reliquat reste donc stable. Si le credit bancaire
echoue, la position est restauree.

Le financement entrepreneurial consomme d'abord la tresorerie du projet puis
le compte courant. Une injection est un transfert economique du cash personnel
vers la valeur de l'entreprise.

## Banque et Livret A

Le compte courant et le Livret A sont uniques dans `ServiceBanque`.
`Epargne` est a la fois un compte transferable et le proprietaire de son
moteur d'interets fixes.

Le solde du compte est l'unique valeur patrimoniale du Livret A. Son
`Investissement` interne calcule les interets mais n'est pas recompte dans la
liste des placements autonomes.

Les interets sont accumules mensuellement et capitalises en decembre. Le taux
applicable vient des donnees du projet avec un taux de secours documente.

Le catalogue Bourse conserve un actif defensif nomme `livret_a` pour comparer
des historiques. Cet actif de marche est distinct du compte bancaire et ne
represente pas un second Livret A possede.

## Patrimoine

`IPatrimoine` est le seul contrat patrimonial commun actuellement necessaire.
`ServicePatrimoine` additionne :

- les comptes ;
- les placements fixes autonomes ;
- la valeur de marche Bourse ;
- la valorisation de l'entreprise.

Le service ignore explicitement le moteur d'interets deja porte par un compte
`Epargne`. Les valeurs de Bourse et d'Entrepreneuriat sont mises a jour par
l'orchestrateur, pas par l'ouverture des fenetres.

Immobilier pourra rejoindre ce calcul en exposant un agregat implementant
`IPatrimoine`, sans ajouter de logique immobiliere dans le service central.

## Passage mensuel

`IEvolutionMensuelle` est partage par les placements fixes, le Livret A, la
Bourse et Entrepreneuriat.

`ServicePassageMensuel` execute cet ordre :

1. appliquer les placements fixes et les interets du Livret A ;
2. appliquer la valorisation du marche et de l'entreprise ;
3. creer le snapshot profond du mois cloture ;
4. avancer l'index absolu et le mois civil ;
5. reporter les soldes, vider les historiques courants et verser le salaire ;
6. valoriser le nouveau mois ;
7. laisser `ActionPlay` notifier les interfaces.

Le salaire du nouveau mois n'apparait jamais dans le snapshot du mois
precedent. Un placement fixe n'est execute qu'une fois par passage.

`ActionPlay.OnMoisPasse` reste temporairement un evenement statique de
presentation pour les prefabs existants et Salariat. Il est reinitialise au
demarrage du runtime, et chaque composant se desabonne dans `OnDisable` ou
`OnDestroy`. Aucun service metier n'y est abonne.

## Bourse

`DonneesBourse` contient les positions, impacts persistants, messages et
valorisations cachees. `CatalogueActifs` centralise les six definitions et
leurs series historiques chargees depuis `Resources/Bourse`.

`ServiceBourse` gere :

- achat par montant ;
- vente par montant ou quantite ;
- liquidation d'une position ;
- capital investi et cout moyen ;
- valeur actuelle et gains ou pertes latents ;
- variation, volatilite et rendement annualise ;
- impacts de marche ;
- valorisation mensuelle.

`BourseUI` ne modifie pas directement les positions. Le graphique appartient
a `GraphiqueBourseUI`.

## Entrepreneuriat

`DonneesEntrepreneuriat` contient le projet, le dernier message, le mois
observe et l'etat aleatoire deterministe. Les anciennes valeurs serialisees
dans le prefab sont migrees une seule fois par la facade Unity.

`ServiceEntrepreneuriat` gere les choix, la creation, les depenses, le
developpement, l'etude de marche, les injections, les pitchs, les pivots, les
ressources et la valorisation.

La version actuelle ne definit pas encore de charges ou revenus recurrents.
L'evolution mensuelle borne et revalorise l'etat sans inventer une nouvelle
regle de gameplay.

## Snapshots et What If

`SnapshotEtatJeu` ne recalcule rien. Il copie l'etat apres la cloture metier :

- `DonneesJoueur.Copier()` ;
- comptes et historiques ;
- interets non verses ;
- positions et impacts Bourse ;
- projet et etat aleatoire ;
- environnement et calendrier.

Les evenements C# et references UI ne sont pas copies. Une simulation doit
partir du snapshot et construire ses propres services autour de la copie.
Avec le meme etat et les memes impacts, les tirages entrepreneuriaux restent
deterministes.

Une interface generique de copie n'a pas ete ajoutee : les agregats ont des
formes differentes et leurs methodes `Copier()` explicites rendent les
frontieres de copie visibles.

## Evenements

`ServiceEvenementsEconomiques` constitue le point d'entree futur des contenus
Actualites. Il enregistre aujourd'hui un `ImpactEvenementMarche` dans
`DonneesBourse`, puis revalorise le portefeuille.

Le flux cible est :

1. une actualite produit un impact metier ;
2. le service d'evenements route cet impact ;
3. le systeme cible le conserve dans ses donnees ;
4. l'evolution mensuelle l'applique ;
5. l'interface lit le nouvel etat.

`ActualitesUI` ne doit pas appeler `BourseUI` ou une autre fenetre. Aucune
interface generale d'impact n'est encore creee, car seul le marche partage
actuellement un contrat concret. Elle deviendra pertinente lorsqu'un second
systeme, par exemple Entrepreneuriat ou Immobilier, portera des impacts
persistants de meme nature.

## Ajouter un onglet

1. Creer un agregat serialisable sans reference UI.
2. Creer les modeles metier et documenter leurs unites.
3. Creer un service testable qui retourne `ResultatOperation`.
4. Implementer `IPatrimoine` uniquement si l'agregat possede une valeur.
5. Implementer `IEvolutionMensuelle` uniquement si le systeme evolue chaque
   mois.
6. Ajouter l'agregat dans `DonneesJoueur` et dans sa copie profonde.
7. Enregistrer sa valeur dans `ServicePatrimoine` et son evolution dans
   `ServicePassageMensuel`.
8. Garder le `MonoBehaviour` comme facade pour les champs Inspector, les clics
   et le rendu.
9. Ajouter des tests EditMode avant de modifier un prefab ou une scene.

## Choix issus de l'audit

- Banque et Livret A restent la base des flux financiers.
- `Investissement` reste specialise dans le rendement fixe.
- `IEvolutionMensuelle` et `IPatrimoine` sont les seuls contrats communs
  ajoutes ou conserves.
- Les interfaces de portefeuille, position, historique, impact ou copie n'ont
  pas ete creees sans second usage concret.
- Les snapshots utilisent des copies profondes par agregat.
- L'evenement mensuel statique est limite a la presentation et ne pilote plus
  la logique metier.
- Les noms et champs serialises des `MonoBehaviour` ont ete conserves afin de
  ne pas casser les prefabs.

## Limites connues

- La sauvegarde sur disque reste a construire autour d'un format explicite.
- Salariat utilise encore la notification mensuelle de presentation ; ses
  regles pourront migrer vers un service implementant `IEvolutionMensuelle`.
- Actualites ne cree pas encore les impacts depuis ses assets.
- Immobilier ne possede pas encore d'agregat metier dans cette branche.
- Les historiques bancaires sont exposes par une liste mutable pour
  compatibilite avec les interfaces existantes.
