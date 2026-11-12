# DONNÉES FICTIVES — pour tester le script uniquement

**Aucun joueur réel n'est à l'origine de ces fichiers. Aucun test joueur n'a été réalisé à ce jour (12 novembre 2026).**

Les chiffres ont été inventés pour vérifier `../agreger_journaux.py`. Ils ne doivent figurer dans aucun bilan, aucune présentation, aucune décision.

| Fichier | Cas vérifié |
|---|---|
| `DONNEES-FICTIVES-T01_2026-11-22.txt` | Reprend l'exemple qui définit le format du journal ; relevé à J+8 ; retour à J+1 et à J+7 |
| `DONNEES-FICTIVES-T02_2026-11-22.txt` | Joueur assidu : 24 sessions, dont seules les 20 dernières durées sont gardées |
| `DONNEES-FICTIVES-T03_2026-11-23.txt` | Décrochage précoce : une session de 95 secondes, aucune amélioration (`delai_premiere_amelioration_s=-1`) |
| `DONNEES-FICTIVES-T04_2026-11-18.txt` | Journal imparfait : phrase avant le journal, virgule décimale, trois dernières lignes manquantes, relevé trop tôt (J+4) |
| `DONNEES-FICTIVES-resultats.csv` | CSV produit par le script à partir de ces quatre journaux |

Le script affiche un bandeau « DONNÉES FICTIVES » dès qu'un nom de fichier contient « fictif » ou « fictive », et avertit si des journaux fictifs sont mélangés à des journaux réels.

Ne jamais ranger de journal réel dans ce dossier, ni ailleurs dans le dépôt.
