# Fiche d'intention — *Dig For Real* (nom provisoire)

> **Statut : projet de fiche, non validée.** À soumettre au PO de l'équipe, ou au K-lab s'il s'agit d'un projet pédagogique.
> Le prototype a été développé avant cette validation, à la demande du builder : voir l'exception EX-001 dans [`exception.md`](exception.md).
> Le gabarit officiel de l'organisation n'étant pas disponible dans ce dépôt, cette fiche suit une structure générique : la reporter dans le gabarit en vigueur si nécessaire.

## 1. Identification

| Rubrique | Contenu |
|---|---|
| Nom du projet | *Dig For Real* (provisoire, disponibilité non vérifiée) |
| Nature | Jeu mobile iOS et Android |
| Porteur (builder) | **À compléter** |
| Cadre | **À compléter** : projet personnel, pédagogique ou d'équipe |
| PO ou référent K-lab | **À compléter** |
| Date de la fiche | 12 novembre 2026 |
| Dépôt | `addictive-games` (branche `claude/relaxed-lovelace-21087f`) |

## 2. Intention

**Problème.** Des publicités de jeux mobiles montrent un mini-jeu attrayant (conduire un engin de chantier, creuser, vendre, améliorer) que le jeu installé ne propose pas. Le régulateur britannique a interdit de telles publicités le 4 novembre 2026.

**Proposition.** Un vrai jeu qui tient cette promesse, sans publicité forcée, en appliquant la méthode du développeur indépendant Dan Lespect à un concept différent du sien.

**Objectif à ce stade.** Vérifier, avec 15 à 20 testeurs puis des vidéos de jeu réel, si le concept retient les joueurs, avant tout investissement supplémentaire.

**Critère d'arrêt.** Défini dans `testing/03-mesures-et-seuils.md` : si les seuils ne sont pas atteints après une itération de corrections, le projet s'arrête ou change de concept.

## 3. Périmètre

| Inclus dans le prototype | Exclu à ce stade |
|---|---|
| Jeu complet hors ligne : 12 couches, améliorations, sauvegarde locale | Publication sur un store |
| Français et anglais | Publicités réelles, achats intégrés |
| Journal de test anonyme, copié manuellement par le testeur | Statistiques automatiques, comptes, serveur |

## 4. Utilisateurs

- Phase de test : 15 à 20 testeurs majeurs et volontaires.
- À terme, si le projet se poursuit : grand public, 12 ans et plus.

## 5. Données

| Question | Réponse pour le prototype |
|---|---|
| Données personnelles traitées par l'application | **Aucune** |
| Données de l'organisation utilisées | **Aucune** (ni étudiants, ni candidats, ni salariés, ni référentiel) |
| Accès réseau | **Aucun** |
| Stockage | Progression et compteurs de jeu, sur l'appareil du joueur uniquement |
| Base de données | Aucune (si une base devenait nécessaire : PostgreSQL) |
| Comptes utilisateurs | Aucun |
| Transfert hors de l'Union européenne | Aucun |

**Campagne de test.** Les coordonnées des testeurs et leurs réponses aux questionnaires sont des données personnelles. Le protocole (`testing/01-protocole-tests-joueurs.md`) prévoit la collecte minimale, l'information des personnes, leurs droits et la durée de conservation. L'outil de questionnaire doit être approuvé par l'organisation ou hébergé dans l'Union européenne.

**Évolutions soumises à la DPO avant toute mise en œuvre** : régie publicitaire, outil de statistiques, achats intégrés. Ces services traitent des identifiants et transfèrent le plus souvent des données hors de l'Union européenne. Détail : `launch/03-confidentialite-et-conformite.md`.

## 6. Intelligence artificielle

- Le jeu ne contient **aucun système d'IA** : ni analyse des émotions, ni notation, ni adaptation du comportement au joueur. La difficulté suit des règles fixes et publiques (`Balance.cs`).
- Aucun mécanisme manipulateur : pas de vies bloquantes, pas de compte à rebours artificiel, pas de série quotidienne punitive, pas de coffre aléatoire payant, pas de publicité forcée.
- L'IA a servi d'assistant de développement et de recherche. Les recherches documentaires sont étiquetées (fait, estimation, hypothèse) et leurs limites indiquées.

## 7. Technologie et hébergement

| Rubrique | Contenu |
|---|---|
| Moteur | Unity et C# |
| Hébergement | Aucun : application installée sur l'appareil |
| Dépendances externes | Aucune à l'exécution ; police Rubik sous licence libre OFL |
| Dépôt | Privé ; ne pas le rendre public sans décision explicite |
| Déploiement | Aucun déclenché ; les builds de test sont installés à la main |

## 8. Risques

| Risque | Mesure |
|---|---|
| Le prototype n'a jamais été lancé dans Unity ni sur un téléphone | Premier lancement guidé (`game/README.md`) ; 87 000 vérifications automatiques hors Unity |
| Le concept ne retient pas les joueurs | Tests avec critère d'arrêt avant tout investissement |
| Refus des stores pour similarité | Mécanique et habillage originaux ; dossier d'originalité (`launch/01-plan-de-lancement.md`) |
| Nom déjà pris ou marque déposée | Vérification à faire avant toute publication |
| Dérive vers des pratiques publicitaires intrusives | Règle de conception : aucune publicité forcée |
| Non-conformité RGPD lors de l'ajout de publicités | Saisine de la DPO avant toute intégration |

## 9. Décisions demandées

1. Valider l'intention et le périmètre du prototype (régularisation de l'exception EX-001).
2. Autoriser la campagne de test auprès de 15 à 20 personnes, selon le protocole décrit.
3. Indiquer l'outil de questionnaire approuvé.
4. Préciser le cadre à respecter pour une éventuelle publication : elle constituerait un écart aux recommandations de l'organisation et ferait l'objet d'une nouvelle entrée dans `docs/exception.md`.
