# Cahier des charges — *Dig For Real* (nom provisoire)

*Version 0.1 — 12 novembre 2026. Les valeurs chiffrées d'équilibrage font foi dans le code (`game/Assets/Game/Core/Game/Balance.cs`) ; ce document décrit les règles et les intentions.*

## 1. Le jeu en une phrase

**Tu conduis un engin de chantier qui s'enfonce dans le sol : tu creuses, tu ramasses le minerai, tu le vends, et l'engin devient énorme — jusqu'au cœur de la Terre.**

| | |
|---|---|
| Plateformes | iOS et Android (application native, moteur Unity) |
| Orientation | Portrait, une main, un doigt |
| Durée d'une partie | 40 secondes à 2 minutes |
| Public | Joueurs occasionnels, 12 ans et plus ; aucun contenu violent ni sensible |
| Modèle | Gratuit ; pubs récompensées au choix du joueur et achats intégrés (aucun SDK dans le prototype) |
| Connexion | Aucune : ni compte, ni serveur, ni base de données ; sauvegarde locale |

**Nom.** *Dig For Real* porte la promesse (« pour de vrai ») sans citer de marque tierce et sans reprendre le suffixe « Not Fake » de Dan Lespect. Disponibilité **non vérifiée** (stores, INPI, EUIPO, USPTO) : à contrôler avant toute publication.

## 2. Les cinq premières secondes

1. L'engin est posé à la surface ; une main animée indique « Glisse pour diriger ».
2. Au premier contact, il s'enfonce : la terre éclate en morceaux, le foret gronde, le téléphone vibre légèrement.
3. Au bout de trois secondes, premier filon : les pépites jaillissent et sont aspirées, le compteur grimpe.
4. Au bout de cinq secondes, premier portail : « +1 FORET » à gauche, « ÉNERGIE −5 s » à droite. Le choix est évident, la transformation immédiate.

Aucun texte à lire, aucun écran à valider avant de jouer.

## 3. Commandes

- **Un doigt, glissement horizontal** : l'engin suit le déplacement du doigt (déplacement relatif : le doigt ne cache jamais l'engin).
- La descente est automatique. Relâcher le doigt : l'engin continue tout droit.
- Aucune autre commande en partie. Bouton pause en haut à droite.

## 4. Boucle de jeu

```
FORER (une descente) ──► VENTE du minerai ──► GARAGE (améliorations) ──► FORER plus profond
```

### 4.1 Une descente

- L'engin détruit les blocs qu'il touche. Plus un bloc est dur, plus il résiste : la terre se traverse comme du beurre, la roche ralentit, le socle est indestructible et se contourne.
- **Batterie** : seule ressource à surveiller. Elle se vide avec le temps ; les cristaux la rechargent ; la lave et les rochers l'entament. À zéro, la descente s'arrête.
- **Minerai** : les filons libèrent des pépites, aspirées par l'aimant de l'engin.
- **Portails** : des paires de portails barrent le passage. On choisit l'un des deux en se plaçant à gauche ou à droite ; l'engin se transforme pour le reste de la descente.
- **Sceau** : au fond de chaque couche, une dalle blindée affiche ses points de résistance. La percer ouvre la couche suivante et recharge un peu la batterie ; la descente continue.

### 4.2 Victoire et défaite

- **Victoire** : le sceau de la couche de départ est percé pendant la descente. La couche suivante est débloquée ; la partie se poursuit tant qu'il reste de la batterie, pour le plaisir et le butin.
- **Défaite** (« Panne sèche ») : batterie vide avant d'avoir percé le sceau. Le minerai ramassé est quand même vendu : on ne repart jamais les mains vides.
- L'écran de fin dit la vérité : profondeur atteinte, distance restante jusqu'au sceau ou résistance restante du sceau.

### 4.3 Vente et garage

- Le minerai est vendu automatiquement à la remontée (compteur animé).
- Le garage propose six améliorations permanentes. Chacune se voit sur l'engin.

| Amélioration | Effet | Ce qu'on voit |
|---|---|---|
| Foret | Puissance de destruction | Foret plus long, plus acéré |
| Moteur | Vitesse de descente et de déplacement latéral | Échappements, flammes |
| Batterie | Durée d'une descente | Jauge plus longue, cellules sur la carrosserie |
| Châssis | Nombre de forets de départ (1 à 4) et résistance à la lave | L'engin s'élargit |
| Aimant | Rayon de ramassage | Bobines, halo |
| Raffinerie | Valeur du minerai vendu | Benne dorée |

## 5. Contenu

### 5.1 Douze couches, puis l'Abysse

Chaque couche est un niveau : une profondeur à atteindre, un sceau à percer, une nouveauté.

| N° | Couche | Nouveauté | Minerai |
|---|---|---|---|
| 1 | Terre | Diriger, creuser, portails | Charbon |
| 2 | Argile | Cristaux de batterie | Cuivre |
| 3 | Roche | TNT et réactions en chaîne | Fer |
| 4 | Cavernes | Chutes libres, rochers qui tombent | Argent |
| 5 | Filons | Roche dure, veines riches | Or |
| 6 | Cristaux | Forêts de cristaux, descentes rapides | Émeraude |
| 7 | Magma | Poches de lave qui s'écoulent | Rubis |
| 8 | Obsidienne | Bandes très dures, TNT indispensable | Saphir |
| 9 | Ruines | Murs anciens, coffres | Améthyste |
| 10 | Gouffre | Grandes cavernes, coulées de lave | Diamant |
| 11 | Noyau | Plaques de métal, tous les dangers réunis | Iridium |
| 12 | Le Cœur | Le sceau final | Étoile |

Après le Cœur : écran de fin, puis mode **Abysse** (descente sans fin, record de profondeur).

**Génération.** Chaque couche est produite par code à partir de quelques paramètres (longueur, dureté, proportions de matériaux, fréquence des dangers). Aucun niveau n'est dessiné à la main : ajouter une couche revient à ajouter une ligne de paramètres.

### 5.2 Éléments de terrain

| Élément | Comportement |
|---|---|
| Tendre, moyen, dur | Trois duretés par couche, aux couleurs de la couche |
| Socle | Indestructible ; oblige à choisir sa route |
| Filon | Libère des pépites |
| Cristal | Recharge la batterie |
| TNT | Explose quand on le touche, détruit autour de lui, déclenche les TNT voisins. Ne blesse pas l'engin : c'est un allié |
| Rocher | Tombe quand on creuse dessous ; un choc coûte de la batterie |
| Lave | S'écoule dans les galeries creusées, vide la batterie au contact, puis se fige en basalte |
| Coffre | Gros lot de pièces |
| Portail | Voir ci-dessous |
| Sceau | Dalle blindée de fin de couche |

### 5.3 Portails

| Bonus (bleu, flèche vers le haut) | Malus (rouge, flèche vers le bas) |
|---|---|
| +1 foret (jusqu'à 7) | −1 foret |
| Puissance +50 % | Puissance −30 % |
| Vitesse +25 % | Énergie −5 s |
| Énergie +8 s | |
| Aimant ×2 | |
| Butin ×2 | |

- Sept fois sur dix, les deux portails sont des bonus différents : c'est un vrai arbitrage. Trois fois sur dix, un bonus fait face à un malus.
- Les effets durent jusqu'à la fin de la descente.
- Bonus et malus se distinguent par la couleur **et** par une flèche et un signe, pour rester lisibles par les personnes daltoniennes.

## 6. Progression et rejouabilité

- **Court terme** : battre le sceau qui a résisté.
- **Moyen terme** : améliorer l'engin, découvrir la couche suivante et son nouveau danger.
- **Long terme** : atteindre le Cœur, puis le record de l'Abysse.
- **Défi du jour** (débloqué après la couche 2) : même terrain pour tout le monde, engin imposé. Aucun serveur : le terrain est tiré de la date. Le score se partage par capture d'écran.

**Ce que le jeu ne fait pas, par principe** : pas de vies ni d'énergie qui bloquent la partie, pas de compte à rebours artificiel, pas de série quotidienne dont la rupture serait punie, pas de coffre aléatoire payant, pas de « presque gagné » truqué, aucune pub forcée.

## 7. Difficulté

- La dureté des blocs et la résistance du sceau augmentent d'une couche à l'autre ; la puissance de l'engin suit les améliorations.
- La couche 1 est gagnable au premier essai. À partir de la couche 3, il faut en moyenne deux à quatre descentes et quelques améliorations pour passer.
- L'équilibrage est contrôlé par un robot joueur qui simule des centaines de descentes (voir `testing/`).

## 8. Monétisation prévue (hors prototype)

| Levier | Règle |
|---|---|
| Pub récompensée « Batterie de secours » | Proposée une fois par descente, à la panne ; relance la descente avec une demi-batterie |
| Pub récompensée « Vente ×2 » | Proposée à l'écran de fin |
| Interstitielles | **Aucune** |
| Achat « Pass Chantier » | Obtient les récompenses sans regarder de pub, définitivement |
| Achats cosmétiques | Peintures et habillages de l'engin |

Dans le prototype, les deux pubs récompensées sont **simulées et étiquetées comme telles** (« Pub simulée »), pour mesurer l'intérêt des testeurs sans intégrer aucun SDK.

## 9. Identité visuelle : « chantier néon »

- **Engin** : jaune de chantier, bandes noires de sécurité, chenilles, deux phares en guise d'yeux. C'est le personnage du jeu.
- **Sous-sol** : fond bleu nuit ; blocs en carrés arrondis, contours épais, un seul reflet ; chaque couche a sa teinte.
- **Minerais** : couleurs saturées et lumineuses, seuls éléments brillants de l'écran. L'œil va là où est la récompense.
- **Typographie** : une seule police grasse et ronde (licence libre OFL), chiffres très lisibles.
- **Sensations** : débris, secousses d'écran dosées, brefs ralentis sur les grosses explosions, sons produits par synthèse, vibrations. Une option réduit les secousses et les flashs.
- **Tout est dessiné par le code** : aucune image importée, donc aucun asset tiers, et un rendu net à toutes les résolutions.

## 10. Écrans

| Écran | Contenu |
|---|---|
| Accueil | Logo, engin, couche en cours, bouton FORER, garage, défi du jour, réglages |
| Partie | Batterie, profondeur, pièces, progression vers le sceau, pause |
| Fin de descente | Victoire ou panne, vente animée, statistiques, « Vente ×2 », rejouer, garage |
| Garage | Six améliorations, aperçu de l'engin |
| Réglages | Sons, musique, vibrations, secousses, langue (français, anglais), journal de test, remise à zéro |

## 11. Données et vie privée

- Le prototype **ne collecte et ne transmet aucune donnée personnelle**. Il n'a pas d'accès réseau.
- Sauvegarde : progression, réglages et compteurs de jeu, sur l'appareil uniquement.
- **Journal de test** : compteurs anonymes (nombre de descentes, durées, couches atteintes, jours joués) que le testeur peut **copier lui-même** depuis les réglages pour les transmettre. Aucun envoi automatique, aucun identifiant.
- Toute intégration future d'une régie publicitaire ou d'un outil de statistiques impose un cadrage RGPD et une saisine préalable de la DPO (voir `launch/` et `docs/exception.md`).

## 12. Périmètre du prototype

**Inclus** : menu, boucle complète, 12 couches, portails, TNT, rochers, lave, cristaux, coffres, sceaux, garage à six améliorations, victoire et défaite, sauvegarde locale, sons de synthèse, vibrations, français et anglais, journal de test, pubs récompensées simulées.

**Reporté** (voir `ROADMAP.md`) : Abysse et défi du jour s'ils ne tiennent pas dans le premier jet, collection de reliques, missions, habillages, SDK de pubs et d'achats, classements.
