# DONNÉES FICTIVES — pour tester le script uniquement

**Aucun joueur réel n'est à l'origine de ces fichiers. Aucun test joueur n'a été réalisé à ce jour (12 novembre 2026).**

Les chiffres ont été inventés pour vérifier `../agreger_journaux.py`. Ils ne doivent figurer dans aucun bilan, aucune présentation, aucune décision.

| Fichier | Cas vérifié |
|---|---|
| `DONNEES-FICTIVES-T01_2026-11-22.txt` | Reprend tel quel l'exemple qui définit le format. Son dernier jour joué est J+7 : il a donc été copié à J+7, et son retour à J+7 est indéterminé (« ? ») |
| `DONNEES-FICTIVES-T02_2026-11-22.txt` | Joueur assidu, journal copié à J+8 : 24 sessions (seules les 20 dernières durées sont gardées), retour à J+1 et à J+7 |
| `DONNEES-FICTIVES-T03_2026-11-23.txt` | Décrochage précoce : une session de 95 secondes, puis la session de relevé à J+8 ; aucune amélioration (`delai_premiere_amelioration_s=-1`), aucune couche franchie |
| `DONNEES-FICTIVES-T04_2026-11-18.txt` | Journal imparfait, copié trop tôt (J+4) : phrase avant le journal, virgule décimale, trois dernières lignes manquantes |
| `DONNEES-FICTIVES-resultats.csv` | CSV produit par le script à partir de ces quatre journaux |

T01 suit l'exemple de format, où `descentes_terminees` vaut victoires + pannes. T02 à T04 suivent le code du jeu : `descentes_terminees` compte aussi les abandons, et le jour comme la session du relevé figurent dans le journal.

Le script affiche un bandeau « DONNÉES FICTIVES » dès qu'un nom de fichier contient « fictif » ou « fictive », et avertit si des journaux fictifs sont mélangés à des journaux réels.

Ne jamais ranger de journal réel dans ce dossier, ni ailleurs dans le dépôt.
