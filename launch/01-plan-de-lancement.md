# Plan de lancement — *Dig For Real* (nom provisoire)

> 12 novembre 2026. Règles et chiffres : `launch/00-reference-stores-monetisation.md`, avec ses étiquettes — **[FAIT]** lu sur la source officielle ; **[NV]** connu mais non relu, à traiter comme une hypothèse ; **[HYPOTHÈSE]** non sourcé. « À vérifier » : absent ou incertain dans le fichier 00.
>
> Ce plan n'engage rien. Toute étape payante ou publique est une **décision à soumettre au porteur de projet** (§6) ; aucune n'est prise à ce jour.

## 1. Partis pris

- **Version 1.0 sans régie, sans outil de statistiques, sans achat** : aucune donnée personnelle, donc une première publication qui ne dépend pas de la DPO. Pubs récompensées et Pass Chantier arrivent en version 1.1, après son avis (`03`, `04`).
- **Le porteur de projet déclenche lui-même** chaque envoi de build et chaque mise en production.
- **Aucun secret dans le dépôt ni dans les documents** : clés de signature, mots de passe et jetons restent dans un gestionnaire de mots de passe.
- Ni compte joueur, ni serveur, ni base de données (s'il en fallait une : PostgreSQL).

## 2. Étapes et critères de passage

S1 = semaine du 16 novembre 2026 ; S17 = semaine du 8 mars 2027.

| Étape | Semaines | Critères de passage | Décisions |
|---|---|---|---|
| 0. Régularisation | S1 | Fiche d'intention validée (EX-001) ; nom disponible ; DPO saisie | L1, L2, L3 |
| 1. Prototype | S1 | Boucle complète sur un iPhone et un Android réels ; 12 couches terminables ; aucun accès réseau | — |
| 2. Tests joueurs | S2–S5 | Décision « continuer » selon `testing/03`, §5 ; sinon corriger et retester, ou arrêter. Le test vidéo prévu par `testing/` suit, sous accord séparé | Celles de `testing/01`, §2 |
| 3. Comptes développeur | S6 | Identités vérifiées (délais à vérifier) ; statut « trader » déclaré ; Mac et appareils de test disponibles | L4 à L8, L14 |
| 4. Tests internes | S7 | Build signé installé par la piste interne Play (100 testeurs au plus [NV]) et par TestFlight interne (100 [FAIT]) ; aucun plantage bloquant | L9 |
| 5. Test fermé Google Play | S8–S10 | Exigé des comptes personnels créés après le 13 novembre 2023 : **au moins 12 testeurs inscrits en continu pendant 14 jours** [FAIT]. Un testeur qui sort repart de zéro [FAIT] : en inscrire 15 à 20 [HYPOTHÈSE]. Puis demande d'accès à la production : examen en 7 jours ou moins, prolongation possible si l'engagement est jugé faible [NV] | L9 |
| 6. TestFlight externe | S8–S10 | Premier build externe accepté en revue ; build valable 90 jours [FAIT] ; retours traités | L9 |
| 7. Lancement limité (2 pays) | S11–S15 | Accès production accordé ; App Review passée (90 % des soumissions en moins de 48 h [FAIT]) ; puis indicateurs ci-dessous | L10, L13 |
| 8. Lancement général | S17 | Indicateurs au vert ; listes du §4 cochées ; Union européenne d'abord (`03`, §5) ; version 1.1 si l'avis de la DPO est reçu, sinon 1.0 | L11, L12 |

**Lancement limité.** Pays proposés : **Belgique et Irlande** [HYPOTHÈSE] — même cadre RGPD et DSA que la France, une audience francophone et une anglophone, deux petits marchés. Lecture dans App Store Connect (cohortes [FAIT]) et la Play Console [NV].

| Indicateur | Feu vert | Arrêt ou refonte |
|---|---|---|
| Rétention J1 | 26 % ou plus | moins de 20 % |
| Rétention J7 | 7 % ou plus | moins de 3 % |
| Note moyenne | 4,0 ou plus | moins de 3,5 (note Google Play de *Real War*) |
| Stores | aucun rejet en cours | rejet pour similarité non levé |

- Seuils de rétention : premier quartile et médiane du fichier 00, §5 [HYPOTHÈSE, fiabilité faible] ; seuils de note [HYPOTHÈSE]. Entre les deux colonnes : deux semaines d'itération, puis nouvelle lecture.
- Volume : au moins 300 installations [HYPOTHÈSE] ; en dessous, un J7 de 3 % ne se distingue pas d'un J7 de 7 %. Si le volume manque : ouvrir la France, ou option L13.

## 3. Calendrier : points d'attention

- **Chemin critique : Google Play.** Compter 3 à 4 semaines au minimum entre le premier build en test fermé et la production [HYPOTHÈSE].
- **Échéances externes** [FAIT] : 12 janvier 2027, nouveaux descripteurs d'âge Apple à renseigner ; 1er février 2027, pages mémoire de 16 Ko exigées pour les mises à jour Android ; février 2027, seuils mémoire Android vitals ; avril 2027, SDK iOS 27 (Xcode 27, macOS Tahoe 26.6 ou plus) et captures iPhone Duo.
- **Aléas non chiffrés** : vérification d'identité des comptes, réponse de la DPO, fêtes de fin d'année. S16 sert de marge.

## 4. Listes de contrôle avant soumission

**App Store**

- [ ] Adhésion active ; statut « trader » déclaré et vérifié [FAIT]
- [ ] Build Xcode 26 ou plus, SDK iOS 26, cible iOS 13 au minimum ; manifeste de confidentialité (UnityFramework) [FAIT]
- [ ] App Privacy rempli ; URL de la politique de confidentialité [FAIT]
- [ ] Questionnaire d'âge complet, nouveaux descripteurs compris [FAIT]
- [ ] Règle 2.1, aucun contenu provisoire : retirer « Pub simulée », masquer le journal de test [FAIT]
- [ ] Règles 4.1 et 4.3 : nom, icône et captures sans marque tierce ; captures du vrai jeu, conformes 4+ (2.3.8) ; dossier d'originalité joint aux notes de revue [FAIT]
- [ ] Version 1.1 : contrat « Paid Apps », coordonnées bancaires, formulaire fiscal W-8BEN ; inscription au Small Business Program ; achat restaurable (3.1.1) ; pubs adaptées à l'âge et moyen de signaler une pub (2.5.18) [FAIT]

**Google Play**

- [ ] Identité vérifiée [FAIT] ; appareil Android réel, coordonnées publiques, déclaration « trader » [NV]
- [ ] Test fermé validé, accès production accordé [FAIT]
- [ ] API cible 36, format AAB, pages de 16 Ko (AGP 8.5.1 et NDK r28 ou plus) [FAIT] ; Play App Signing [NV]
- [ ] Formulaire Data safety [FAIT] ; URL de la politique, questionnaire IARC, public cible de 13 ans et plus [NV]
- [ ] Fiche sans marque tierce ni bloc de mots-clés
- [ ] Version 1.1 : Play Billing Library 8 ou plus [FAIT] ; « Contient des annonces », permission AD_ID seulement si l'identifiant est lu, app-ads.txt [NV]

**Les deux stores**

- [ ] Décision correspondante confirmée par le porteur de projet et consignée dans `docs/exception.md`
- [ ] Aucun secret dans le dépôt
- [ ] Version 1.0 : contrôle que le build n'émet aucune requête réseau
- [ ] Fiche relue : chaque phrase est vraie dans ce build (`02`)

## 5. Dossier d'originalité

Dan Lespect dit avoir essuyé plusieurs rejets pour similarité (déclaratif, `research/01`). Règles en cause : Apple 4.1 et 4.3 [FAIT] ; usurpation et contenu répétitif chez Google [NV]. À préparer avant la première soumission :

1. **Note d'une page**, en français et en anglais : ce qui est propre au jeu — conduite directe (pas un jeu « idle »), portails qui transforment l'engin en pleine descente, réactions en chaîne (TNT, lave, rochers), voyage fini en douze couches, aucune pub forcée.
2. **Preuves de création** : historique du dépôt ; terrain et graphismes produits par le code, sons de synthèse, police sous licence OFL. Aucun asset tiers.
3. **Tableau comparatif** avec les jeux proches (*Power Dig*, *Ground Digger*, *Drill & Collect*, *Dig Deep*…) : nom, icône, interface, mécaniques. À remplir après lecture de leurs fiches.
4. **Vidéo continue de 60 secondes** et captures datées du build soumis.
5. **Recherche d'antériorité du nom** : stores, INPI, EUIPO, USPTO (non faite à ce jour).
6. **Réponse type à un rejet 4.1 ou 4.3**, dans les deux langues, appuyée sur la vidéo et le tableau.
7. **Règles de conduite** : une seule application, un seul Bundle ID (4.3 a) ; hors stores, ne pas citer la marque dont la publicité a inspiré le concept sans analyse préalable [HYPOTHÈSE] ; pièges de fiche : `02`, §9.

## 6. Décisions à soumettre au porteur de projet

Numérotation propre au lancement (les décisions de `testing/01` en sont distinctes). « Exception » : la décision s'écarte d'une recommandation de l'organisation ; une fois confirmée, elle est consignée dans `docs/exception.md` (règle, risque, alternative, décision). « Obligation » : aucune exception possible.

| N° | Décision | Coût | Moment | Règle de l'organisation |
|---|---|---|---|---|
| L1 | Faire valider la fiche d'intention | 0 € | S1 | Régularise EX-001 |
| L2 | Vérifier le nom ; dépôt de marque éventuel | Recherche : 0 € ; dépôt : à vérifier | S1 | Dépense éventuelle |
| L3 | Saisir la DPO (`03`, §8) | 0 € | S1, avant tout recrutement de testeurs | Obligation : RGPD, transferts hors UE |
| L4 | Créer le statut juridique (micro-entreprise) avant le premier encaissement [NV] | À vérifier | Avant la version 1.1 | — |
| L5 | Ouvrir le compte Google Play | 25 USD, une fois [FAIT] | S6 | Dépense, compte payant |
| L6 | Adhérer à l'Apple Developer Program | 99 USD par an [FAIT] | S6 | Dépense, compte payant |
| L7 | Nom de domaine et page publique (politique de confidentialité, assistance) | 10 à 20 € par an [HYPOTHÈSE] | S6 | Hébergeur externe : exception |
| L8 | Coordonnées publiées au titre du statut « trader » : adresse personnelle ou boîte postale | Boîte postale : à chiffrer [HYPOTHÈSE] | S6 | Données personnelles du porteur de projet |
| L9 | Envoyer des builds en test et y inscrire des testeurs | 0 € | S7–S8 | Préproduction, service externe : exception. Adresses des testeurs : obligation RGPD, DPO d'abord |
| L10 | Lancement limité : publication publique dans deux pays | 0 € | S11 | Publication publique : exception |
| L11 | Version 1.1 : AdMob, fenêtre de consentement, Pass Chantier | 0 € [NV] | S13 au plus tôt : après avis de la DPO et feu vert du lancement limité | Service externe : exception. Transfert hors UE : obligation |
| L12 | Lancement général | 0 € | S17 | Publication publique : exception |
| L13 | Option : acheter 500 installations de test | 75 à 400 USD [HYPOTHÈSE : 0,15 à 0,8 USD l'unité] | S11 | Dépense ; service externe : exception |
| L14 | Matériel manquant (Mac compatible, iPhone, Android) | À chiffrer ; sans Mac, Xcode Cloud inclut 25 h par mois [FAIT] | S6 | Dépense éventuelle |

**Coût minimal pour publier sur les deux stores : 124 USD** (99 + 25) [FAIT], plus la page publique exigée pour la politique de confidentialité : environ **130 à 150 € la première année**, puis 110 à 120 € par an, hors matériel et hors boîte postale [HYPOTHÈSE].

**Alternative conforme** : tant qu'une décision n'est pas confirmée, le jeu reste un build local, installé sur les appareils du porteur de projet.
