# Protocole de tests joueurs — *Dig For Real* (nom provisoire)

*12 novembre 2026. **Aucun test joueur n'a encore été réalisé** : ce document dit quoi faire, il ne contient aucun résultat.*

*Étiquettes : **[HYPOTHÈSE]** proposé, non mesuré · **[DÉPÔT]** repris du dépôt · **[CALCUL]** reproductible · **[NV]** de mémoire, non vérifié.*

## 1. Objectifs

| Question | Source |
|---|---|
| Q1. Comprend-on le jeu sans explication ? | Séances observées |
| Q2. Revient-on le lendemain, puis une semaine après ? | Journal de test |
| Q3. La progression bute-t-elle sur une couche ? | Journal de test |
| Q4. Les récompenses contre « pub simulée » intéressent-elles ? | Journal, questionnaires |

**Hors de portée** : la rentabilité ; les joueurs anonymes d'un store ; les 12-17 ans, visés par le jeu [DÉPÔT] mais exclus du test (testeurs majeurs uniquement).

## 2. Décisions à soumettre au porteur de projet

Rien n'est engagé sans son accord ; tout écart au cadre de l'organisation est d'abord consigné dans `docs/exception.md`.

| N° | Sujet | Option conforme, par défaut | Accord exprès requis pour |
|---|---|---|---|
| D1 | Distribution du build (§5) | Téléphone du porteur de projet, puis APK installé directement | TestFlight, Google Play |
| D2 | Outil de questionnaire | Formulaire approuvé par l'organisation ; sinon papier, ou fichier renvoyé par la messagerie de l'organisation | Outil non approuvé, même hébergé dans l'UE (écart à consigner) ; hors UE, DPO d'abord |
| D3 | Données des testeurs (§8) | Note d'information relue par la DPO avant de recruter | — |
| D4 | Contrepartie, recrutement | Aucune rémunération ; messages privés | Toute dépense, toute annonce publique |

## 3. Deux temps

| | (a) D'abord : séances observées | (b) Ensuite : campagne à distance |
|---|---|---|
| But | Trouver les blocages | Mesurer le retour et la progression, sur un build corrigé et gelé |
| Testeurs | 5, un par un, en personne | 15 à 20 journaux valides ; recruter 22 à 24 [HYPOTHÈSE] |
| Durée | 25 minutes | 7 jours, relevé à J+8 |

Cinq séances montrent les gros défauts, pas un taux : un défaut gênant 30 % des joueurs a 83 % de chances d'y apparaître ; à 10 %, 41 % [CALCUL].

## 4. Recrutement

| Profil (majeurs uniquement) | (a) | (b), sur 18 |
|---|---|---|
| Joue sur téléphone chaque semaine : cœur de cible | 3 | 10 |
| Joue rarement ou jamais : épreuve de compréhension | 1 | 4 |
| Habitué des jeux d'arcade sur mobile | 1 | 4 |

En (b), au moins 4 téléphones Android d'entrée de gamme [HYPOTHÈSE] : la surchauffe est un grief connu du genre [DÉPÔT].

**Biais des proches.** Un proche est poli et connaît le projet : il gonfle le plaisir et le retour. Aucun proche en (a) ; 5 au plus en (b), marqués « P » et lus à part [HYPOTHÈSE]. Viser le second cercle (amis d'amis, collègues d'autres équipes), par messages privés. Un testeur ne sert qu'une fois.

## 5. Distribution du build de test

| Mode | Coût, compte | Donnée transmise à un tiers | Limites | Accord exprès |
|---|---|---|---|---|
| Téléphone du porteur de projet | Aucun. iPhone sans adhésion payante : 3 appareils, profil valable 7 jours [DÉPÔT] | Aucune | Sur place | Non |
| **APK Android installé directement** | Aucun | Aucune | Android seul ; installation hors store à autoriser ; vérification des développeurs Android étendue au monde en 2027 [DÉPÔT], effet à revérifier | Non, si le fichier passe de la main à la main ou par l'espace de partage approuvé, sans lien public |
| TestFlight (iOS) | Adhésion Apple, 99 USD par an [DÉPÔT] | E-mail du testeur, à Apple (États-Unis) | Revue du premier build externe ; un lien public est une publication [DÉPÔT] | **Oui** : dépense, compte payant, DPO d'abord, mention ajoutée à la note d'information |
| Test interne Google Play | Compte à 25 USD [DÉPÔT] | E-mail du testeur, à Google (États-Unis) | 100 testeurs au plus [NV] | **Oui** : idem |

Recommandation [HYPOTHÈSE] : téléphone du porteur pour (a), APK direct pour (b). Prix : un panel sans iPhone, car le profil iOS gratuit expire avant le relevé à J+8.

## 6. Déroulé d'une séance observée

Avant chaque testeur : jeu remis à zéro, grille imprimée, chronomètre.

| Minute | Étape | L'observateur |
|---|---|---|
| 0 à 3 | Accueil | Note d'information, consentement. « C'est le jeu que je teste, pas vous. Je ne vous aiderai pas : faites comme si vous étiez seul. » |
| 3 à 5 | Deux premières minutes | « Ouvrez ce jeu et jouez comme chez vous. » Se tait, remplit la grille (`02`, §2). À toute question : « Que feriez-vous si je n'étais pas là ? » |
| 5 à 15 | Jeu libre | Invite à penser à voix haute ; n'intervient qu'après 60 secondes de blocage, et le note. Le testeur arrête quand il veut. |
| 15 à 25 | Entretien, clôture | Questions ouvertes (`02`, §3), questionnaire J0, copie du journal de test (délais exacts). |

**Ce qu'on ne dit pas au testeur**

- Le but du jeu, les commandes, le rôle des portails, de la batterie, du garage.
- Que le jeu répond à des publicités : on demande à la fin « À quoi ce jeu vous fait-il penser ? ».
- Ce qu'on espère voir, les seuils, et, en campagne, qu'il faudrait jouer chaque jour. On dit : « Ne pas jouer est une réponse utile. »

On ne ment pas pour autant : la note d'information décrit tout ce qui est recueilli.

## 7. Déroulé de la campagne à distance

| Quand | Action |
|---|---|
| J−3 | Note d'information, consentement, code attribué (T01…), fréquence de jeu et modèle du téléphone notés |
| J0 | Installation et première partie, dans une fenêtre de deux jours ; questionnaire J0 |
| J+2, matin | Questionnaire J+1 (porte sur la veille) |
| J+8, matin | Questionnaire J+7 et journal : Réglages → « Copier mon journal de test » → coller |
| J+8 à J+10 | Une relance. Journal enregistré sous `T07_2026-12-13.txt` (code, date de réception) |

Envois décalés d'un jour, aucun autre message : chaque contact est un rappel qui fabrique le retour mesuré.

## 8. Données personnelles (RGPD)

Le journal ne contient aucun identifiant [DÉPÔT], mais le message qui le porte nomme son expéditeur. Tout est donc rangé sous un code, et le lien entre code et personne est détruit après la campagne.

- **Table de correspondance** (prénom, contact, mention « P », preuve du consentement) : fichier local hors dépôt git, détruit avec les messages reçus 30 jours au plus après la campagne.
- **Sous le code** (modèle du téléphone, fréquence de jeu, journal, réponses, grille) : fichiers locaux, 24 mois au plus, noms retirés des réponses libres. Rien dans le dépôt git ni dans un service en ligne non approuvé.
- **Non recueilli** : nom de famille, âge exact, genre, adresse, image, voix, localisation, identifiant d'appareil. Aucun compte utilisateur.

**Note d'information (prête à l'emploi)**

> **Test du jeu « Dig For Real » — information des testeurs**
>
> **Qui ?** [Nom du porteur de projet], responsable du traitement — [adresse e-mail]. [Contact de la DPO.]
>
> **Pourquoi ?** Améliorer un prototype de jeu mobile. Base légale : votre consentement.
>
> **Quelles données ?** Votre prénom et un moyen de vous joindre ; le modèle de votre téléphone ; votre fréquence de jeu ; vos réponses aux questionnaires ; en séance, les notes de l'observateur ; le « journal de test » que vous copiez vous-même depuis le jeu (jours joués, nombre et durée des parties, progression, choix dans le jeu, fluidité). Le jeu n'accède pas à Internet et n'envoie rien de lui-même.
>
> **Comment ?** Vos réponses sont rangées sous un code. Seul [nom] peut le relier à vous, par une liste supprimée au plus tard le [fin de campagne + 30 jours]. Les réponses, alors anonymes, sont conservées 24 mois au plus.
>
> **Pour qui ?** [Nom] uniquement. Rien n'est vendu, publié, ni transféré hors de l'Union européenne.
>
> **Vos droits.** Participation libre, sans rémunération, réservée aux majeurs. Vous pouvez arrêter et retirer votre consentement à tout moment, et demander l'accès à vos données, leur rectification, leur effacement, la limitation de leur traitement ou leur portabilité, à [adresse]. Après suppression de la liste, vos réponses ne peuvent plus être retrouvées. Vous pouvez saisir la CNIL (cnil.fr).

**Formule de consentement**

> ☐ J'ai 18 ans ou plus.
>
> ☐ J'ai lu la note d'information du [date].
>
> ☐ J'accepte de participer et que mes réponses et mon journal de test soient utilisés, sous un code, pour améliorer le jeu.
>
> ☐ *(Séance observée)* J'accepte que l'observateur prenne des notes écrites pendant que je joue ; ni photo, ni enregistrement.
>
> Date, prénom, signature ou réponse écrite « J'accepte » : ……

## 9. Calendrier indicatif

[HYPOTHÈSE : build installable et décisions prises le 20 novembre 2026. Sinon décaler en bloc, hors des fêtes (19 décembre au 3 janvier).]

| Dates | Étape |
|---|---|
| 12 au 20 novembre | Décisions, relecture DPO, premiers recrutements |
| 23 au 25 novembre | 5 séances observées |
| 26 novembre au 3 décembre | Corrections, séances de contrôle, build gelé, recrutement de la campagne |
| 5 au 14 décembre | Campagne : J0 les 5 et 6, journaux les 13 et 14 |
| 15 au 18 décembre | Analyse, décision |
| 13 janvier 2027 au plus tard | Destruction de la table de correspondance |

## 10. Analyse

1. Séances : reporter les grilles le jour même ; classer les problèmes par nombre de testeurs touchés, puis par gravité.
2. Campagne : lancer `outils/agreger_journaux.py`, remplir le tableau vide de `03` (§5), expliquer chaque taux par les réponses libres ; lire à part les proches et les téléphones lents.
3. Ne pas toucher aux seuils après avoir vu les résultats.

## 11. Critères de décision [HYPOTHÈSE]

Seuils fixés avant les tests ; détail et incertitude dans `03`.

| Décision | Séances observées | Campagne |
|---|---|---|
| **Continuer** | Aucun blocage commun à 2 testeurs ; 4 testeurs sur 5 achètent seuls une amélioration en moins de 3 minutes | Retour à J+1 de 60 % ou plus **et** à J+7 de 30 % ou plus ; aucune mesure secondaire au rouge |
| **Corriger, puis retester** | Un blocage commun à 2 testeurs : corriger, puis 2 ou 3 séances avec de nouveaux testeurs | Tout autre cas ; deux cycles au plus |
| **Arrêter, ou changer de concept** | 4 testeurs sur 5 cessent de jouer avant la fin et notent leur plaisir à 2 sur 5 ou moins | Retour à J+1 sous 40 % **et** à J+7 sous 15 % **et** plaisir médian de 2 sur 5 ou moins ; ou échec après deux cycles |

« Continuer » ouvre la préparation du test vidéo (`04`), lui-même soumis à accord.
