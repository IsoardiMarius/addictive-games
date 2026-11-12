# Outils — agrégation des journaux de test

`agreger_journaux.py` lit tous les fichiers `.txt` d'un dossier (un journal `DFR-JOURNAL 1` par fichier), calcule les mesures définies dans `../03-mesures-et-seuils.md` par testeur et au global, affiche un tableau et écrit un CSV.

- Python 3.8 ou plus, bibliothèque standard uniquement. Aucun accès réseau : rien n'est envoyé nulle part.
- Vérifié le 12 novembre 2026 avec Python 3.11, 3.12 et 3.13 (sorties identiques).

## Utilisation

```
python3 testing/outils/agreger_journaux.py DOSSIER [--csv FICHIER.csv] [--exclure-session-releve]
```

| Argument | Rôle |
|---|---|
| `DOSSIER` | Dossier contenant les journaux. **Pour des journaux réels, un dossier hors du dépôt git.** |
| `--csv` | CSV à écrire. Par défaut : `DOSSIER/resultats.csv` ; pour des journaux fictifs, `DOSSIER/DONNEES-FICTIVES-resultats.csv` |
| `--exclure-session-releve` | Ne compte pas la dernière session de chaque journal, celle que le testeur a ouverte pour copier son journal. **À utiliser pour la campagne à distance** ; à ne pas utiliser pour une séance observée, où le journal est copié pendant la partie. |

Codes de sortie : `0` au moins un journal exploité · `1` aucun journal exploitable, ou CSV non écrit · `2` dossier ou option invalide.

## Préparer les fichiers

1. Un fichier par testeur, nommé `CODE_AAAA-MM-JJ.txt` : le code du testeur (jamais un nom de personne), puis la date de réception. Exemple : `T07_2026-12-13.txt`. La date sert à l'archivage ; le script lit le jour de la copie dans le journal lui-même.
2. Coller le texte tel qu'il a été reçu. Ce qui entoure le journal (formule de politesse, signature) est ignoré.
3. Faire copier le journal à J+8 ou après. Copié à J+7 ou avant, il donne « ? » (indéterminé) pour le retour à J+7, et le testeur sort du calcul.
4. Si deux fichiers portent le même code, seul le plus récent est gardé : le journal est cumulatif.

## Ce qui est calculé

- **Par testeur** : retour à J+1 et à J+7 (d'après `premier_lancement` et `jours_joues`), « retour à J+7 ou jeu fini », jours joués de J0 à J+7, sessions, durée médiane de session, descentes par session, taux de victoire, descentes abandonnées, couches franchies, achat d'au moins une amélioration, taux d'acceptation des pubs simulées. Le CSV ajoute les délais, la part de portails malus, les images par seconde et le décrochage précoce.
- **Au global** : chaque testeur compte une fois. Part des testeurs avec son intervalle de confiance à 95 % (méthode de Wilson), ou médiane des testeurs. Le total « regroupé » (tous les événements additionnés) est donné à titre indicatif.
- **Répartition de `meilleure_couche`**, et nombre d'essais par couche pour repérer un mur de difficulté.
- **CSV** : une ligne par testeur, puis une ligne `GLOBAL` (médiane des testeurs, ou « oui/réponses »). Format français : séparateur point-virgule, virgule décimale, UTF-8. Retirer la ligne `GLOBAL` avant de recalculer des moyennes dans un tableur.

## Ce que le script tient du code du jeu

Vérifié le 12 novembre 2026 dans `game/Assets/Game/Core/Game/` (`GameApp.cs`, `GameApp.Screens.cs`, `Progress.cs`). **Si le jeu change l'un de ces points, il doit changer le numéro de format (`DFR-JOURNAL 2`) et le script doit suivre.**

| Fait constaté dans le code | Conséquence |
|---|---|
| Ouvrir le jeu, même pour copier le journal, ajoute le jour courant à `jours_joues` | Le dernier jour de la liste est celui de la copie. Le script ne le compte jamais comme un retour : un journal copié à J+7 donne « ? » à J+7. D'où le relevé à J+8 |
| Une session commence au lancement, ou au retour après plus de 5 minutes en arrière-plan | Ouvrir le jeu pour copier le journal ajoute une session courte : option `--exclure-session-releve` |
| `meilleure_couche` est le nombre de couches franchies, de 0 à 12 | 0 est une valeur normale ; 12 signifie que le testeur a fini le jeu |
| Le prototype n'a pas de mode sans fin, et un robot le finit en 32 descentes (`../05-resultats-disponibles.md`) | Un testeur assidu aura fini avant J+7 : la mesure « retour à J+7 ou jeu fini » compte « jeu fini » comme un retour |
| `tentatives_par_couche` compte chaque descente commencée sur la couche | Sa somme vaut `descentes_commencees` |
| Un délai de `-1` signale un événement qui n'a pas eu lieu ; `ips_moyennes=0.0`, aucune mesure ; point décimal | Lus comme « non disponible » ; la virgule décimale reste tolérée |
| `duree_totale_s` est le temps d'ouverture du jeu, menus compris | Le décrochage précoce (moins de 180 s) se lit sur ce total |
| La « remise à zéro » des réglages garde le journal et ne rejoue pas l'initiation | Avant une séance observée, effacer les données de l'application ou la réinstaller |

**Écart tranché le 12 novembre 2026.** Le jeu compte `descentes_terminees` = victoires + pannes, comme dans l'exemple qui définit le format ; les abandons sont comptés à part, et un test automatique le contrôle. Le script accepte aussi l'ancienne forme (victoires + pannes + abandons) et calcule le taux de victoire sur victoires + pannes.

## Fichiers mal formés

Le script ne s'arrête jamais sur un fichier : il le signale et traite les autres.

| Cas | Comportement |
|---|---|
| Fichier vide, binaire, sans en-tête `DFR-JOURNAL 1`, autre version de format, trop volumineux, dossier nommé `x.txt` | Écarté, raison affichée |
| Clé absente, valeur illisible (`sessions=sept`), date impossible | Journal gardé ; mesures concernées « n/d » ; avertissement |
| Texte avant ou après le journal, clé inconnue, clé en double, deux journaux dans un fichier | Ignoré ; avertissement |
| Virgule décimale (`2,4`), fins de ligne Windows, UTF-8 avec marque d'ordre d'octets, UTF-16, extension `.TXT` | Accepté |
| Compteurs incohérents (plus de pubs acceptées que proposées, par exemple) | Avertissement ; taux concerné « n/d » |
| Même journal sous deux codes, versions du jeu mélangées, journaux fictifs mêlés à des réels | Avertissement de lot |

Contrôle fait le 12 novembre 2026 sur 17 fichiers d'essai volontairement abîmés (non conservés dans le dépôt) : 9 retenus, 8 écartés, aucun arrêt du script.

## Vérification sur les exemples fictifs

> **DONNÉES FICTIVES — pour tester le script uniquement.** Les chiffres ci-dessous ne décrivent aucun joueur : aucun test joueur n'a été réalisé.

Commande lancée depuis la racine du dépôt, le 12 novembre 2026, avec Python 3.13.18 :

```
python3 testing/outils/agreger_journaux.py testing/outils/exemples-fictifs
```

Sortie obtenue (code de sortie 0) :

```
JOURNAUX DE TEST - 4 fichier(s) .txt lu(s) dans testing/outils/exemples-fictifs : 4 retenu(s), 0 écarté(s)
*** DONNÉES FICTIVES (d'après les noms de fichiers) : ces chiffres ne décrivent aucun joueur réel. ***

PAR TESTEUR
  Testeur               J+1  J+7  Jours  Sess.  Méd. sess.  Desc./sess.  Victoires  Abandons  Couches  Amél.  Pubs simulées
  DONNEES-FICTIVES-T01  oui  ?    n/d    7      300 s       5,9          23 %       5 %       6        oui    39 % (12/31)
  DONNEES-FICTIVES-T02  oui  oui  7      24     384 s       6,8          26 %       3 %       10       oui    63 % (88/139)
  DONNEES-FICTIVES-T03  non  non  1      2      56,5 s      1            0 %        50 %      0        non    0 % (0/1)
  DONNEES-FICTIVES-T04  oui  ?    n/d    6      202,5 s     3,7          29 %       5 %       4        oui    n/d
  Légende : J+1, J+7 = a rejoué ce jour-là (« ? » = journal copié ce jour-là ou avant) ;
  Jours = jours joués de J0 à J+7 ; Sess. = sessions ; Méd. sess. = durée médiane d'une
  session ; Desc./sess. = descentes par session ; Victoires = victoires / (victoires +
  pannes) ; Abandons = descentes quittées en cours / descentes commencées ;
  Couches = couches franchies (meilleure_couche) ; Amél. = a acheté une amélioration ;
  n/d = non disponible.

GLOBAL (4 testeur(s) ; chaque testeur compte une fois)
  Retour à J+1                                              3/4 = 75 %  IC 95 % : 30 % à 95 %
  Retour à J+7                                              1/2 = 50 %  IC 95 % : 9 % à 91 % ; 2 indéterminé(s)
  Retour à J+7 ou jeu fini (12 couches franchies)           1/2 = 50 %  IC 95 % : 9 % à 91 % ; 2 indéterminé(s)
  Jours joués de J0 à J+7, médiane                          4           sur 8 possibles
  Durée médiane de session, médiane des testeurs            251,2 s     de 56,5 à 384 s
  Descentes par session, médiane des testeurs               4,8         regroupé : 228/39 = 5,8
  Taux de victoire, médiane des testeurs                    25 %        regroupé : 56/219 = 26 %
  Descentes abandonnées, médiane des testeurs               5 %         regroupé : 9/228 = 4 %
  Décrochage précoce (moins de 180 s de jeu, aucun retour)  1/4 = 25 %  IC 95 % : 5 % à 70 %
  Testeurs ayant acheté une amélioration                    3/4 = 75 %  IC 95 % : 30 % à 95 %
  Délai avant la première amélioration, médiane             71 s        3 testeur(s) concerné(s)
  Délai avant le premier toucher, médiane                   2,8 s
  Couches franchies (meilleure_couche), médiane             5
  Testeurs ayant fini le jeu                                0/4 = 0 %   IC 95 % : 0 % à 49 %
  Pubs simulées acceptées, médiane des testeurs             39 %        regroupé : 100/171 = 58 %
  Testeurs ayant accepté au moins une pub simulée           2/3 = 67 %  IC 95 % : 21 % à 94 % ; 1 indéterminé(s)
  Portails malus choisis, médiane des testeurs              11 %        regroupé : 44/550 = 8 % ; au hasard : environ 15 %
  Images par seconde, minimum                               31,4        0 testeur(s) sous 30
  « regroupé » = tous les événements additionnés : chiffre dominé par les gros joueurs.

RÉPARTITION DE meilleure_couche (couches franchies) ET TENTATIVES PAR COUCHE
  N   Testeurs ayant franchi N couches  Testeurs ayant tenté la couche N  Tentatives sur la couche N : médiane (max.)
  0   1  #                              -                                 -
  1   0                                 4                                 2,5 (6)
  2   0                                 3                                 5 (9)
  3   0                                 3                                 6 (12)
  4   1  #                              3                                 8 (14)
  5   0                                 2                                 11,5 (17)
  6   1  #                              2                                 12,5 (16)
  7   0                                 1                                 20 (20)
  8   0                                 1                                 24 (24)
  9   0                                 1                                 22 (22)
  10  1  #                              1                                 14 (14)
  11  0                                 1                                 9 (9)
  12  0                                 0                                 -

AVERTISSEMENTS
  DONNEES-FICTIVES-T01 :
    - journal copié à J+7 : le retour à J+7 est indéterminé
  DONNEES-FICTIVES-T04 :
    - 1 ligne(s) hors journal ignorée(s)
    - clé(s) absente(s) : pubs_simulees_proposees, pubs_simulees_acceptees, ips_moyennes

Rappel : avec 4 testeur(s), ces chiffres sont des signaux directionnels, pas des taux
fiables. Lire les intervalles (IC 95 %) et testing/03-mesures-et-seuils.md, section 4.
CSV écrit : testing/outils/exemples-fictifs/DONNEES-FICTIVES-resultats.csv
```

**Lecture.**

- `DONNEES-FICTIVES-T01` reprend l'exemple qui définit le format : premier lancement le 14 novembre, joué les 15 et 21. Retour à J+1 : oui. Retour à J+7 : « ? », car le 21 est le dernier jour du journal, donc le jour de sa copie ; rien ne dit que le testeur a joué ce jour-là. Contrôle à la main des autres valeurs : médiane de `312;140;420;98;301;263;300` = 300 s ; 41 descentes / 7 sessions = 5,9 ; 9 victoires / (9 + 30) = 23 % ; 2 abandons / 41 = 5 % ; 12 pubs simulées acceptées / 31 = 39 %. Le script donne les mêmes valeurs.
- Avec `--exclure-session-releve` (deuxième exécution, même jour) : T01 passe à 6 sessions, 282 s et 6,8 descentes par session ; T02 à 23, 388 s et 7,1 ; T03 à 1, 95 s et 2 ; T04 à 5, 220 s et 4,4. Au global : 251 s de durée médiane et 5,6 descentes par session. T03 montre l'intérêt de l'option : sans elle, la session de 18 secondes ouverte pour copier le journal divise par deux ses descentes par session.
