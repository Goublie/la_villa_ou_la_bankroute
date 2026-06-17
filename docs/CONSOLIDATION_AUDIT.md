# Audit de consolidation

Branche de travail : `integration/version-complete`.

Base de verite architecturale : `origin/refactor/architecture-apps`
au commit `7c7b64e`.

Date de l'audit : 2026-06-16.

## Regles appliquees

- Aucun travail direct sur `main` ou `refactor/architecture-apps`.
- Aucun `push --force`.
- Aucune suppression de branche, tag ou reference.
- Pas de fusion globale aveugle.
- Les versions de `GameData`, `DonneesJoueur`, `argent`,
  `IPatrimoine`, `IEvolutionMensuelle`, `ResultatOperation`, Banque,
  Bourse, Entrepreneuriat, patrimoine, snapshots et passage mensuel de
  `refactor/architecture-apps` restent prioritaires.
- Les modifications TextMesh Pro, ProjectSettings, UserSettings et racines
  Unity accidentelles sont classees comme bruit sauf preuve contraire.

## Inventaire des references

### Branches locales visibles

| Branche | Sommet | Statut |
| --- | --- | --- |
| `feature/bourse` | `7f5103d` | deja integree |
| `feature/entrepreneuriat` | `47bc771` | deja integree |
| `integration/version-complete` | `7c7b64e` | branche de travail |
| `main` | `12216fd` | ancetre deja integre dans l'architecture |
| `refactor/architecture-apps` | `7c7b64e` | reference architecturale |

Les branches locales annoncees `Python`, l'ancienne branche Entrepreneuriat,
`entrepreneuriat`, `marketing` et `scene/jeu` ne sont pas presentes comme refs
locales dans ce clone au moment de l'audit. Elles n'ont pas ete supprimees par
cette mission.

### Branches distantes visibles

| Branche | Sommet | Commits propres vs refactor | Cherry uniques | Traitement |
| --- | ---: | ---: | ---: | --- |
| `origin/actu` | `59c9b3c` | 0 | 0 | A |
| `origin/feature/bourse` | `7f5103d` | 0 | 0 | A |
| `origin/feature/entrepreneuriat` | `47bc771` | 0 | 0 | A |
| `origin/refactor/architecture-apps` | `7c7b64e` | 0 | 0 | base |
| `origin/Salariat` | `2ba25ee` | 5 | 5 | D |
| `origin/gameover` | `25d9e11` | 8 | 8 | C |
| `origin/main` | `7cdd830` | 7 | 7 | C |
| `origin/repartitionTemps` | `2a07199` | 13 | 13 | D |
| `origin/scene/jeu` | `7c3c99e` | 15 | 15 | C |
| `origin/scene/menu` | `11e783a` | 8 | 8 | C |
| `origin/scene/retrospective` | `b4c9ae3` | 8 | 8 | D |

Categorie :

- A : deja integree.
- B : a fusionner.
- C : a selectionner.
- D : a porter manuellement.
- E : bruit ou doublon.

## Matrice par branche

### `origin/actu` - A. Deja integree

- Sommet : `59c9b3c`.
- Merge-base : `59c9b3c`.
- Commits uniques : 0.
- Fichiers importants : aucun diff restant.
- Fonctionnalites : Actualites et tableaux deja presentes dans la base
  refactorisee.
- Traitement conseille : ne pas refusionner.

### `origin/feature/bourse` - A. Deja integree

- Sommet : `7f5103d`.
- Merge-base : `7f5103d`.
- Commits uniques : 0.
- Fichiers importants : aucun diff restant.
- Fonctionnalites : Bourse, ressources JSON, portefeuille, courbes, achat,
  vente, patrimoine et tests sont deja presents dans une version meilleure.
- Traitement conseille : ne pas refusionner.

### `origin/feature/entrepreneuriat` - A. Deja integree

- Sommet : `47bc771`.
- Merge-base : `47bc771`.
- Commits uniques : 0.
- Fichiers importants : aucun diff restant.
- Fonctionnalites : fenetre Entrepreneuriat et selection de projet deja
  reprises puis refactorisees.
- Traitement conseille : ne pas refusionner.

### `origin/Salariat` - D. A porter manuellement

- Sommet : `2ba25ee`.
- Merge-base avec refactor : `e7cf24f`.
- Commits uniques : 5.
- Commits :
  - `c188981` amelioration panel relationnel ;
  - `22c815e` V2 amelioration panel relationnel ;
  - `b41801e` amelioration relationnel Salariat ;
  - `9e1b103` correction bug negocier salaire ;
  - `2ba25ee` ajout du panel Formation.
- Fichiers utiles :
  - `code/unity/Assets/prefabs/Apps/Salariat/Salariat.prefab` ;
  - `DemissionController.cs` ;
  - `EmployeePerformanceController.cs` ;
  - `FormationController.cs` ;
  - `RelationalController.cs` ;
  - `SalariatUI.cs`.
- Bruit a ecarter :
  - TextMesh Pro ;
  - `EditorBuildSettings.asset` ;
  - `ProjectSettings.asset`.
- Conflits attendus :
  - logique mensuelle encore dependante de `ActionPlay.OnMoisPasse` ;
  - etat non centralise dans `DonneesJoueur` ;
  - prefab a preserver sans casser les references serializees.
- Traitement conseille :
  - restaurer les scripts/prefab utiles ;
  - extraire un agregat de donnees et un service Salariat si l'etat doit
    survivre aux scenes/snapshots ;
  - connecter l'evolution mensuelle via `IEvolutionMensuelle` ou conserver
    temporairement la notification UI uniquement si aucun etat persistant
    n'est implique ;
  - ne pas reprendre les changements TextMesh Pro et ProjectSettings.

### `origin/repartitionTemps` - D. A porter manuellement

- Sommet : `2a07199`.
- Merge-base avec refactor : `e7cf24f`.
- Commits uniques : 13.
- Fonctionnalites utiles :
  - onglet RepartitionTemps ;
  - sliders lies au graphique ;
  - obligation de repartir le temps ;
  - timer de fenetre ;
  - blocage du slider par unite.
- Fichiers utiles :
  - `code/unity/Assets/prefabs/Apps/RepartitionTemps/RepartitionTemps.prefab`;
  - `code/unity/Assets/programmes/Apps/RepartitionTemps/RepartitionTempsUI.cs`;
  - `code/unity/Assets/programmes/Apps/TimerFenetre.cs`.
- Bruit a ecarter :
  - `code/unity/Assets/programmes/Editor/AddChronoToFenetre.cs` ;
  - `code/unity/Assets/programmes/Editor/CreateRepartitionTempsPrefab.cs` ;
  - TextMesh Pro ;
  - `EditorBuildSettings.asset` ;
  - anciennes modifications de `GameData`, `argent`, `ActionPlay`,
    `DonneesJoueur`, `Investissement`, `Epargne`.
- Conflits attendus :
  - ancien `ManagerTemps` chevauche `ServicePassageMensuel` ;
  - anciennes corrections Banque/Livret A deja traitees dans l'architecture ;
  - modifications de `Fenetre.prefab`.
- Traitement conseille :
  - porter la fonctionnalite dans un agregat/service si elle modifie l'etat ;
  - ne pas reprendre `ManagerTemps` tel quel ;
  - ne pas conserver les scripts editor temporaires.

### `origin/main` - C. A selectionner

- Sommet : `7cdd830`.
- Merge-base avec refactor : `e7cf24f`.
- Commits uniques : 7.
- Fonctionnalites :
  - retour possible depuis Retrospective ;
  - preload/changement de scenes ;
  - options en prefab variant ;
  - premiers ajustements de Retrospective ;
  - `TO-DO.md`.
- Fichiers utiles :
  - `Menu.unity` ;
  - `Retrospective.unity` ;
  - `ScenesManager.cs` ;
  - `Options.prefab` et `.meta` ;
  - `FermerRetrospective.cs`.
- Bruit / ancienne architecture :
  - `GameData.cs`, `argent.cs`, `SnapshotEtatJeu.cs` ;
  - TextMesh Pro ;
  - `EditorBuildSettings.asset`.
- Traitement conseille :
  - selectionner seulement navigation, options et scripts de scene ;
  - adapter `ScenesManager` sans remplacer la version consolidee.

### `origin/scene/menu` - C. A selectionner

- Sommet : `11e783a`.
- Merge-base avec refactor : `e7cf24f`.
- Commits uniques : 8.
- Fonctionnalites : `origin/main` plus correction musique.
- Chevauchements : tres fort avec `origin/main` et `origin/gameover`.
- Fichiers utiles :
  - `Menu.unity` ;
  - `Retrospective.unity` ;
  - `Options.prefab` ;
  - `ScenesManager.cs`.
- Bruit / ancienne architecture :
  - `GameData.cs`, `argent.cs`, `SnapshotEtatJeu.cs` ;
  - TextMesh Pro ;
  - `EditorBuildSettings.asset`.
- Traitement conseille :
  - utiliser cette branche comme source menu la plus recente avant
    `origin/scene/jeu` ;
  - ne pas appliquer deux fois les memes changements depuis `main`.

### `origin/scene/jeu` - C. A selectionner

- Sommet : `7c3c99e`.
- Merge-base avec refactor : `d3d822b`.
- Commits uniques : 15.
- Fonctionnalites :
  - fusion de Menu dans Jeu ;
  - fusion Salariat dans Jeu ;
  - options et retrospective depuis les branches menu/main.
- Fichiers utiles :
  - `Menu.unity` ;
  - `Retrospective.unity` ;
  - `Salariat.prefab` ;
  - scripts Salariat recents ;
  - `ScenesManager.cs` ;
  - `Options.prefab`.
- Anciennes versions a ecarter :
  - `GameData.cs` ;
  - `argent.cs` ;
  - `SnapshotEtatJeu.cs`.
- Bruit :
  - TextMesh Pro fallback.
- Traitement conseille :
  - source principale pour la scene principale recente ;
  - extraire uniquement les apports absents, jamais remplacer `Jeu.unity`
    globalement sans controle.

### `origin/scene/retrospective` - D. A porter manuellement

- Sommet : `b4c9ae3`.
- Merge-base avec refactor : `d3d822b`.
- Commits uniques : 8.
- Fonctionnalites :
  - hierarchy/prefab Retrospection ;
  - optimizer ;
  - graphique et tableau Retrospection ;
  - retour vers la scene de jeu.
- Fichiers utiles :
  - `Retrospection.prefab` ;
  - `FermerRetrospective.cs` ;
  - `Optimizer.cs` ;
  - `RetrospectionEvenements.cs` ;
  - `RetrospectionGraphiqueUI.cs` ;
  - `RetrospectionTableauUI.cs`.
- Conflits attendus :
  - anciennes modifications `Epargne`, `Investissement`, `ActionPlay`,
    `GameData`, `argent`, `SnapshotEtatJeu` ;
  - integration avec snapshots refactorises.
- Traitement conseille :
  - porter Retrospection sur les snapshots profonds existants ;
  - ne pas reprendre les anciennes corrections Livret A deja couvertes.

### `origin/gameover` - C. A selectionner

- Sommet : `25d9e11`.
- Merge-base avec refactor : `e7cf24f`.
- Commits uniques : 8.
- Fonctionnalite utile :
  - `code/unity/Assets/Scenes/GameOver.unity` et `.meta` ;
  - lien potentiel avec `ScenesManager`.
- Fichiers a ecarter :
  - `Packages/` a la racine ;
  - `ProjectSettings/` a la racine ;
  - TextMesh Pro et polices ajoutees sauf preuve d'usage direct ;
  - anciennes versions `GameData`, `argent`, `SnapshotEtatJeu`.
- Chevauchements :
  - reprend `origin/main` et ses modifications menu/retrospective.
- Traitement conseille :
  - restaurer la scene GameOver et adapter la navigation ;
  - ne pas integrer la seconde racine Unity accidentelle.

### `feature/bourse`, `feature/entrepreneuriat`, `main` locaux

- `feature/bourse` et `feature/entrepreneuriat` sont identiques a leurs
  remotes et deja integres.
- `main` local est un ancien ancetre deja inclus par la base refactorisee.
- Traitement conseille : ne pas fusionner.

### Branche locale `origin`

- Ref locale nommee `origin`, sommet `7cdd830`, equivalente a
  `origin/main`.
- Traitement conseille : doublon de `origin/main`, ne pas fusionner
  separement.

## Objets inaccessibles

Commande executee :

```text
git fsck --full --no-reflogs --unreachable
```

Commits inaccessibles trouves :

| Commit | Message | Diagnostic |
| --- | --- | --- |
| `e6420be` | Merge origin/jeu dans l'ancienne branche Entrepreneuriat | ancien etat de travail Entrepreneuriat avec beaucoup de bruit et ancienne architecture |
| `574b257` | index sur l'ancienne branche Entrepreneuriat | etat d'index/stash |
| `934bd7a` | sauvegarde pre-merge liee au fichier audio | etat stash lie au fichier audio |
| `5b8ecfa` | Ajoute l'application bourse | commit initial amende ensuite par `feature/bourse` |
| `7973422` | Merge scene/jeu et adaptation de l'entreprenariat | ancien sommet local Entrepreneuriat identifiable via reflog |
| `5cd4f4e` | WIP sur l'ancienne branche Entrepreneuriat | etat stash lie au fichier audio |
| `8c389d3` | index sur l'ancienne branche Entrepreneuriat | etat d'index/stash |

Tag local de securite cree, non pousse :

- `archive/entrepreneuriat-local-avant-consolidation` -> `7973422`.

Provenance : le reflog montre `7973422` comme commit de merge local juste
avant le checkout vers `feature/entrepreneuriat`, avec des modifications de
Jeu et Entrepreneuriat. Certitude : elevee pour un ancien sommet local
Entrepreneuriat, insuffisante pour l'attribuer a `jeu`, `menu`, `marketing` ou
`pablo`.

Aucun sommet fiable des anciennes branches `jeu`, `menu`, `marketing` ou
`pablo` n'a ete identifie parmi les commits inaccessibles. Aucun tag n'a ete
cree pour ces noms.

## Ordre de consolidation retenu

1. Salariat : portage manuel, car `origin/scene/jeu` le contient deja mais
   l'architecture doit rester celle de `refactor/architecture-apps`.
2. Menu, options et navigation : selectionner depuis `origin/scene/jeu` /
   `origin/scene/menu`.
3. Retrospective : portage sur les snapshots profonds.
4. Game Over : restaurer la scene et connecter `ScenesManager`.
5. RepartitionTemps : porter sans `ManagerTemps` ancien ni scripts builder.
6. Apports restants de `scene/jeu` et `main` : uniquement si absents apres les
   lots precedents.

Chaque lot devra compiler, passer les tests EditMode, etre audite puis etre
commite et pousse avant le lot suivant.
