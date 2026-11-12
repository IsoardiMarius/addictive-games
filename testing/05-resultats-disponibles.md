# Résultats disponibles au 12 novembre 2026

## 1. Ce qui n'existe pas encore

**Aucun joueur humain n'a testé le jeu.** Il n'y a donc, à ce jour, aucun chiffre de compréhension, de plaisir, de durée de session réelle ni de retour à J+1 ou J+7. Tout ce qui suit provient de programmes : cela vérifie que le jeu fonctionne et que sa courbe de difficulté est plausible, pas qu'il plaît.

Le jeu n'a pas non plus été lancé dans Unity ni sur un téléphone (voir `game/README.md`).

## 2. Tests automatiques

Commande : `bash game/Tools/verifier.sh`. Résultat : **86 963 vérifications réussies, 0 échec.**

| Famille de tests | Ce qui est vérifié |
|---|---|
| Simulation déterministe | Même graine et mêmes commandes donnent exactement la même descente |
| Terrain | Sur 12 couches et 6 graines : zone d'entrée sans danger, sceau complet, au moins un portail, toujours 7 colonnes libres de socle |
| Sauvegarde | Relecture fidèle ; une sauvegarde vide ou corrompue ne produit jamais d'état invalide |
| Barème | Coûts strictement croissants, première amélioration abordable, couches de difficulté croissante |
| Journal de test | Format exact attendu par `testing/outils/agreger_journaux.py` |
| Police | Tous les caractères des textes français et anglais sont présents |
| Sons | Aucun échantillon invalide, aucune saturation, boucle du foret raccordée |
| Interface | Premier lancement, descente au doigt, victoire, vente doublée, achat, relance de l'application, réglages, remise à zéro |
| Formats d'écran | Du 320 × 480 à la tablette, images de durée nulle ou très longue |
| Équilibrage | Courbe de progression dans des bornes fixées (voir ci-dessous) |

Ces tests ont trouvé et fait corriger trois défauts avant toute livraison : du minerai dans la zone d'entrée, des amas de socle qui pouvaient bloquer un engin large, et deux erreurs de compilation côté Unity.

Le script d'agrégation a aussi été essayé sur un journal réellement exporté par le jeu : il le lit correctement.

## 3. Simulation par robot joueur

Deux robots dirigent l'engin avec une heuristique simple (éviter le dur, viser le minerai et les bons portails). Le robot « occasionnel » réagit plus lentement et vise moins bien. Commande : `dotnet run --project game/Tools/Headless -c Release -- balance 60 occasionnel`.

### Robot occasionnel, 60 parcours complets

| Couche | Descentes pour la franchir | Durée moyenne d'une descente | Niveau de foret au passage |
|---|---|---|---|
| 1 Terre | 1,0 | 37 s | 0 |
| 2 Argile | 1,0 | 37 s | 2 |
| 3 Roche | 1,4 | 39 s | 3 |
| 4 Cavernes | 1,6 | 41 s | 5 |
| 5 Filons | 3,4 | 44 s | 7 |
| 6 Cristaux | 1,3 | 55 s | 8 |
| 7 Magma | 4,4 | 40 s | 12 |
| 8 Obsidienne | 5,7 | 50 s | 15 |
| 9 Ruines | 1,9 | 59 s | 17 |
| 10 Gouffre | 3,1 | 56 s | 19 |
| 11 Noyau | 4,3 | 63 s | 23 |
| 12 Le Cœur | 3,7 | 62 s | 26 |

- Les 60 parcours atteignent la fin du jeu, en **32 descentes** et **27 minutes de descente** en moyenne (hors temps passé dans les menus).
- Le robot « habile » finit en 30 descentes et 26 minutes : l'écart d'adresse pèse peu, la progression dépend surtout des améliorations.
- La première descente est gagnée dans au moins 95 % des parcours simulés (seuil contrôlé par les tests automatiques).

### Ce que cela permet de dire, et ce que cela ne permet pas

| On peut dire | On ne peut pas dire |
|---|---|
| Le jeu se termine ; aucune couche n'est un mur infranchissable | Qu'un joueur réel aura envie d'aller au bout |
| Les premières couches s'enchaînent vite, puis l'effort demandé augmente | Que ce rythme est le bon : un humain apprend, se lasse, se trompe autrement |
| Une descente dure de 35 à 65 secondes | Que cette durée convient au public visé |
| Les couches 5, 7 et 8 sont les plus exigeantes | Si elles seront vécues comme un défi ou comme une punition |

Hypothèse de travail à confirmer : un contenu d'environ 45 à 60 minutes, menus compris, pour un joueur occasionnel.

## 4. Mesures techniques (ordinateur de développement, pas un téléphone)

| Mesure | Valeur |
|---|---|
| Temps de calcul du jeu par image | 0,03 ms |
| Rectangles dessinés par image | 645 en moyenne, 1 064 au maximum |
| Niveau sonore de la séquence de démonstration | −22,8 dB en moyenne, −3,8 dB en crête (pas de saturation) |

## 5. Prochaine étape

Appliquer le protocole de `testing/01-protocole-tests-joueurs.md` : cinq séances observées, puis 15 à 20 joueurs pendant sept jours, et remplir le tableau de `testing/03-mesures-et-seuils.md`. Préalable : avoir lancé le jeu dans Unity et construit un APK de test.
