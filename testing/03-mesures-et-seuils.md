# Mesures et seuils de décision — *Dig For Real* (nom provisoire)

*12 novembre 2026.*

> **Aucun test joueur n'a encore été réalisé.** Ce document ne contient aucun résultat : le tableau du §5 est vide. Les seuls journaux passés dans le script sont quatre journaux **fictifs**, qui servent à le vérifier (`outils/README.md`).

*Étiquettes [HYPOTHÈSE], [DÉPÔT] et [CALCUL] : définies dans `01`.*

## 1. Source et règles de collecte

- Seule source automatique : le journal de test (`DFR-JOURNAL 1`), copié par le testeur. J0 = `premier_lancement`.
- **Un seul relevé par testeur, à J+8 ou après.** Ouvrir le jeu, même pour copier le journal, compte comme une session et un jour joué [DÉPÔT, code du jeu] : le dernier jour de `jours_joues` est celui de la copie, et ne compte jamais comme un retour.
- Fichier `CODE_AAAA-MM-JJ.txt` ; calcul par `outils/agreger_journaux.py --exclure-session-releve`.
- Unité statistique : **le testeur**, qui compte une fois quel que soit son nombre de parties.

## 2. Définitions

| Mesure | Calcul par testeur | Au global |
|---|---|---|
| Retour à J+1 | `jours_joues` contient `premier_lancement` + 1 jour, avant le jour de la copie | Part des testeurs |
| Retour à J+7 | Idem, + 7 jours. « Indéterminé » si le journal est copié à J+7 ou avant | Part des testeurs, indéterminés exclus |
| Retour à J+7 ou jeu fini | Retour à J+7, **ou** `meilleure_couche` = 12 | Part des testeurs |
| Jours joués | Nombre de dates de `jours_joues` de J0 à J+7 (1 à 8) | Médiane |
| Durée de session | Médiane de `durees_sessions_s` (20 dernières sessions), session de relevé exclue | Médiane |
| Descentes par session | `descentes_commencees` / `sessions`, session de relevé exclue | Médiane |
| Décrochage précoce | `duree_totale_s` < 180 **et** aucune date de `jours_joues` de J+1 à J+7 | Part des testeurs |
| Boucle bouclée | `ameliorations_achetees` ≥ 1 ; délai : `delai_premiere_amelioration_s` | Part des testeurs ; médiane du délai |
| Premier contact | `delai_premier_contact_s` | Médiane |
| Descentes abandonnées | `abandons` / `descentes_commencees` | Médiane |
| Taux de victoire | `victoires` / (`victoires` + `pannes`) | Médiane |
| Progression | `meilleure_couche` (couches franchies, 0 à 12) ; `tentatives_par_couche` | Médiane et répartition ; essais médians par couche |
| Portails malus | `portails_malus` / (`portails_bonus` + `portails_malus`) | Médiane |
| Pubs simulées | `pubs_simulees_acceptees` / `pubs_simulees_proposees` | Médiane ; part des testeurs ayant accepté au moins une fois |
| Fluidité | `ips_moyennes` | Minimum ; testeurs sous 30 |

- **Abandons pendant les premières minutes** : cernés par le décrochage précoce, la boucle bouclée et le premier contact. Le journal ne date pas chaque abandon : la seconde exacte se lit sur la grille d'observation (`02`).
- Portails malus : au hasard, environ 15 %, un malus figurant dans 3 paires sur 10 [CALCUL, cahier des charges §5.3].

**Vérifié dans le code du jeu le 12 novembre 2026**, à revoir s'il change ; `descentes_terminees` vaut victoires + pannes, les abandons étant comptés à part (`outils/README.md`).

## 3. Seuils proposés — tous [HYPOTHÈSE]

Fixés avant tout test, sans référence de marché vérifiée : les repères de `launch/00-reference-stores-monetisation.md` (§5 : J1 de 35 à 40 %, J7 de 10 % en hypercasual) sont eux-mêmes des hypothèses peu fiables et décrivent des joueurs anonymes. Un panel recruté et relancé revient davantage : nos seuils sont plus hauts et ne s'y comparent pas.

| Mesure | Vert [HYPOTHÈSE] | Orange [HYPOTHÈSE] | Rouge [HYPOTHÈSE] |
|---|---|---|---|
| **Retour à J+1** (principale) | 60 % ou plus | 40 à 59 % | Moins de 40 % |
| **Retour à J+7 ou jeu fini** (principale) | 30 % ou plus | 15 à 29 % | Moins de 15 % |
| Jours joués | 4 ou plus | 2 ou 3 | 1 |
| Durée médiane de session | 240 s ou plus | 120 à 239 s | Moins de 120 s |
| Descentes par session | 4 ou plus | 2 à 3,9 | Moins de 2 |
| Décrochage précoce | 10 % ou moins | 11 à 29 % | 30 % ou plus |
| Testeurs ayant acheté une amélioration | 90 % ou plus | 70 à 89 % | Moins de 70 % |
| Délai avant la première amélioration | 180 s ou moins | 181 à 300 s | Plus de 300 s |
| Descentes abandonnées | 10 % ou moins | 11 à 20 % | Plus de 20 % |
| Taux de victoire | 25 à 50 % | 15 à 24 %, ou 51 à 70 % | Moins de 15 %, ou plus de 70 % |
| Couches franchies à J+8 (`meilleure_couche`) | 8 ou plus | 5 à 7 | 4 ou moins |
| Portails malus | 8 % ou moins | 9 à 14 % | 15 % ou plus |
| Pubs simulées acceptées | 30 % ou plus | 10 à 29 % | Moins de 10 % |
| Plaisir à J+7 (questionnaire), médiane | 4 sur 5 ou plus | 3 | 2 ou moins |

- **Règle de décision** (`01`, §11) : les deux mesures principales décident ; les autres désignent quoi corriger.
- **Contenu court** [DÉPÔT, `05`] : un robot finit le jeu en 32 descentes, soit 45 à 60 minutes menus compris (hypothèse), et le prototype n'a pas de mode sans fin. Un testeur assidu aura fini avant J+7 : « jeu fini » compte donc comme un retour.
- Couches franchies : les couches 5, 7 et 8 sont les plus exigeantes pour le robot [DÉPÔT, `05`] ; rester à 4, c'est s'arrêter avant la première.
- Taux de victoire : le robot franchit 12 couches en 32 descentes, soit environ une victoire sur trois [DÉPÔT, `05`].
- À lire à part : les testeurs sous 30 images par seconde.
- Pubs simulées : un plafond, une pub simulée ne coûtant pas 30 secondes d'attention. Un taux bas est instructif, un taux haut ne prouve rien.

## 4. Mise en garde statistique

Avec 15 à 20 testeurs, un pourcentage est un **signal directionnel**, pas une mesure.

| Taux observé | Sur 15 testeurs, le taux réel peut se situer | Sur 20 testeurs |
|---|---|---|
| 20 % | entre 7 et 45 % | entre 8 et 42 % |
| 40 % | entre 20 et 64 % | entre 22 et 61 % |
| 60 % | entre 36 et 80 % | entre 39 et 78 % |

*Intervalles de confiance à 95 %, méthode de Wilson [CALCUL] ; le script les affiche.*

- **Ordre de grandeur : plus ou moins 20 points.** Un testeur pèse 5 à 7 points : ne pas commenter un écart d'un ou deux testeurs.
- **Un seuil isolé ne décide rien.** Si le retour réel à J+1 valait 50 % (orange), un panel de 18 afficherait « vert » près d'une fois sur quatre, et « rouge » aussi souvent [CALCUL]. La règle de décision exige donc deux signaux concordants.
- **Ne pas comparer deux versions** sur 8 à 10 testeurs chacune : seul un écart de 50 à 60 points se distinguerait du hasard [CALCUL, test exact de Fisher].
- **Quatorze mesures** : une ou deux tomberont au vert ou au rouge par hasard.
- **Journaux non renvoyés** : leurs auteurs ont sans doute décroché. Donner aussi le taux « au pire », où ils comptent comme non revenus.
- **Biais que le nombre ne corrige pas** [HYPOTHÈSE] : testeurs volontaires, majeurs, sous Android, observés et relancés trois fois. Le retour mesuré surestime celui de joueurs anonymes.

## 5. Résultats — tableau VIDE, à remplir après la campagne

**Aucune case n'est remplie : aucun test n'a eu lieu.**

Campagne n° …… · version du jeu …… · J0 du …… au …… · journaux valides : …… sur …… recrutés · dont proches : ……

| Mesure | Résultat (x sur n, ou médiane) | Intervalle à 95 % | Couleur | Commentaire |
|---|---|---|---|---|
| Retour à J+1 | | | | |
| Retour à J+7 | | | | |
| Retour à J+7 ou jeu fini | | | | |
| Jours joués de J0 à J+7 | | — | | |
| Durée médiane de session | | — | | |
| Descentes par session | | — | | |
| Décrochage précoce | | | | |
| Testeurs ayant acheté une amélioration | | | | |
| Délais : premier toucher, première amélioration | | — | | |
| Descentes abandonnées | | — | | |
| Taux de victoire | | — | | |
| Couches franchies : médiane, répartition, testeurs ayant fini le jeu, couche où les essais s'accumulent | | — | | |
| Portails malus | | — | | |
| Pubs simulées : taux accepté, testeurs ayant accepté au moins une fois | | | | |
| Images par seconde, minimum | | — | | |
| Plaisir à J0, J+1, J+7 (questionnaires) | | — | | |

Décision : ☐ continuer ☐ corriger ☐ arrêter — date : …… — prise par : ……
