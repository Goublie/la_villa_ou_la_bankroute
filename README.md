# LES REGLES D OR DU REPO

## LA SAINTE TRINITé DES COMMANDE
###JE NE JURERAI QUE PAR L ORDRE DE CES COMMANDES :
- git pull
- git status
- git add {NOM DE FICHIER} /!\ ATTENTION A CE QUE VOUS PUSHEZ
- git commit -m "MESAGE DE CE QU IL CHANGE"
- git push 

## UNE SCENE = UNE BRANCHE
Par exemple si je veux modifier la scène du menu je dois être dans sa branche dédiée (menu)
Pour modifier une scène je DOIS ETRE LE SEUL ET L UNIQUE PERSONNE à la modifiée (utiliser whatsapp, discord ou tout autre moyen de communication moderne (les pigeons sont has been)), si une TO DO est créée référais vous y
Si jamais il y a besoin de créer une nouvelle scène cela veut dire qu'une nouvelle branche doit être aussi créée.

## L'ajout d'onglets
Pour la propreté du repo, un ajout d'un onglet ou tout autre fonctionnalité complexe, une sous branche doit être créée.
- PAR EXEMPLE : L'ajout de l'onglet concernant les investissement se fait dans la branche Jeu dans une sous branche investissement
- Pour le bien du github il faut faire attention à n'ajouter uniquement que des prefabs.

## Ne pushez jamais de fichier .unity
Les fichiers .unity représentent les scènes, ils sont HORRIBLE à lire. AUDREMENT DIT et conformément au point 2 : 'UNE SCENE = UNE BRANCHE' TOUTES PERSONNES MODIFIANT UN FICHIER .unity EN DEHORS DE CETTE BRANCHE SE VERRA PENDU PAR LES OREILLES AU MILIEU DE L'ESIEE.
/!\ IL N EST PAS PERMIS DE MODIFIER LES SCENES DANS LES SOUS BRANCHES

## Pour synthétiser
- SAINTE TRINITé
- branche = scène = fichier.unity 
- sous branche = prefab
