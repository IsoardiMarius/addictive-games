# Confidentialité et conformité — *Dig For Real* (nom provisoire)

> 12 novembre 2026. **Ce document n'est pas un avis juridique** : c'est un dossier de préparation, à faire valider par la DPO de l'organisation. Étiquettes du fichier 00 : [FAIT], [NV] (non relu, à traiter comme une hypothèse), [HYPOTHÈSE]. « À vérifier » : absent ou incertain dans le fichier 00.

## 1. Partis pris

- **Version 1.0** : aucune donnée personnelle, aucun accès réseau.
- **Version 1.1, le minimum** : pubs récompensées non personnalisées, aucun identifiant publicitaire demandé, aucun outil de statistiques tiers.
- **Rien n'est conservé par l'éditeur** (ni serveur, ni compte, ni base de données) ; aucun SDK n'est initialisé avant le choix du joueur [FAIT].
- **Ordre imposé pour toute intégration** : avis de la DPO (obligation, sans exception possible), confirmation du porteur de projet, entrée dans `docs/exception.md`.

## 2. Inventaire des données

| | (a) Prototype et version 1.0 | (b) Avec régie publicitaire (AdMob) | (c) Avec outil de statistiques |
|---|---|---|---|
| **Données** | Progression, réglages, compteurs du journal de test, sur l'appareil seulement | Adresse IP (pays, zone approximative), modèle d'appareil, système, langue, identifiants techniques du SDK, interactions publicitaires, choix de consentement, diagnostics. Identifiant publicitaire : non demandé | Identifiant d'installation, modèle, système, pays déduit de l'adresse IP, événements de jeu, sessions, achats, revenus publicitaires (liste typique, fichier 00, §6) |
| **Finalité** | Faire fonctionner le jeu | Afficher les pubs récompensées choisies par le joueur, les mesurer, prévenir la fraude | Mesurer l'audience, équilibrer le jeu |
| **Base légale** | Sans objet : écriture locale nécessaire au service [HYPOTHÈSE] | Consentement préalable, exigé même pour la pub non personnalisée et sa mesure [FAIT] | Consentement pour Firebase ou Google Analytics [FAIT] ; exemption seulement si l'outil remplit les critères de la CNIL [FAIT] |
| **Durée** | Jusqu'à la remise à zéro ou la désinstallation | Rien chez l'éditeur ; durées de la régie et validité du choix : à vérifier | Au plus 13 mois pour les traceurs et 25 mois pour les données (plafonds de l'exemption) [FAIT] |
| **Destinataires** | Aucun | Google et les partenaires listés dans la fenêtre de consentement | Le fournisseur de l'outil |
| **Transfert hors UE** | Aucun | Oui, États-Unis [NV] : **DPO d'abord** | Oui pour Firebase [FAIT] ; non pour une instance hébergée dans l'UE, comme PostHog [FAIT] ; autres : à vérifier. **DPO d'abord** |
| **Droits** | Le joueur efface tout lui-même | Retrait du consentement à tout moment dans les réglages [FAIT] ; accès et effacement par le contact de l'éditeur, relayés à la régie | Retrait ou opposition dans les réglages ; accès et effacement par identifiant d'installation |

- **(c) Recommandation** : au lancement, statistiques des stores seulement — ni SDK ni transfert ajoutés par l'éditeur [FAIT pour Apple, NV pour Google]. Un outil auto-hébergé supposerait un serveur (hébergeur externe : exception) et une base PostgreSQL, à l'exclusion de toute autre.
- **Testeurs.** Tests joueurs (étape 2) : règles de `testing/01`, §8 — réponses rangées sous un code, table de correspondance supprimée 30 jours au plus après la campagne, rien hors UE. Tests internes, test fermé Google Play et TestFlight (étapes 4 à 6) : l'adresse du compte de chaque testeur passe par Google ou Apple, donc transfert possible hors UE : **DPO avant la première adresse saisie**, et note d'information complétée. TestFlight par lien public : aucune adresse à saisir [FAIT], mais c'est une publication (exception). Testeurs majeurs dans tous les cas.
- **Achats** (Pass Chantier) : paiement traité par Apple et Google ; le jeu ne garde que le droit acquis, sur l'appareil. Intégration directe (StoreKit 2, Play Billing) : aucun tiers de plus. RevenueCat ou Unity IAP : flux de données à vérifier, DPO d'abord.

## 3. Consentement

1. **Au lancement** : aucun SDK initialisé, aucun écran à valider avant la première partie.
2. **Au premier appui sur une offre de pub** : fenêtre de consentement (UMP, plateforme certifiée par Google [NV]). « Refuser » est aussi visible qu'« Accepter » ; le détail par finalité est au second niveau [FAIT].
3. **Accord** : le SDK s'initialise et la pub se charge. **Refus** : aucun SDK, aucune pub ; le jeu reste complet. Pour que le refus ne soit pas une pénalité : un bonus quotidien sans pub [HYPOTHÈSE de conception, point DPO].
4. **App Tracking Transparency (iOS)** : sans suivi ni identifiant publicitaire, pas d'invite — à vérifier, car un SDK qui combine les données compte comme du suivi [FAIT]. Avec des pubs personnalisées : fenêtre de consentement d'abord, invite ATT ensuite [NV] ; ne jamais conditionner une fonction à l'accord ni le récompenser (5.1.2 i) [FAIT]. En France, invite alternative à partir d'iOS 27.2, sortie non confirmée [FAIT].
5. **Réglages → Confidentialité**, en permanence : revoir son choix, lire la politique [FAIT].

Une permission système ne vaut pas consentement [NV] : l'invite ATT ne remplace jamais l'étape 2.

## 4. Déclarations des stores

| | Version 1.0 | Version 1.1 (AdMob, Pass Chantier) |
|---|---|---|
| **App Privacy (Apple)** | « Données non collectées », après contrôle que le build n'émet rien, moteur Unity compris | Identifiants de l'appareil, données publicitaires, interactions, diagnostics, achats : catégories typiques [FAIT]. Finalités : publicité tierce, analyse. « Suivi » : non si les pubs restent non personnalisées (à vérifier) |
| **Data safety (Google)** | Aucune donnée collectée ni partagée | Collecte et partage par le SDK à déclarer [FAIT] : identifiants, interactions, diagnostics ; finalités : publicité, analyse, prévention de la fraude. « Contient des annonces » [NV] |
| **Source des réponses** | Contrôle réseau du build | Déclarations publiées par Google pour son SDK (à vérifier), recopiées sans les minimiser |

L'URL de la politique de confidentialité est obligatoire dès la version 1.0 (Apple [FAIT], Google [NV]). Chez Google, tout écart entre la déclaration et le comportement réel expose à des mesures d'application [FAIT].

## 5. Mineurs et classification

- **Classification attendue** : 4+ et PEGI 3 [HYPOTHÈSE] (`02`, §8).
- **Public déclaré** : général, 13 ans et plus [NV] ; le jeu ne cible pas les enfants. Ni catégorie Enfants d'Apple [FAIT], ni programme Familles de Google [NV].
- **Moins de 15 ans en France** : consentement parental (RGPD, article 8) [NV] ; pas de publicité fondée sur le profilage de mineurs (DSA, article 28) [NV]. D'où le choix : pubs non personnalisées pour tous, aucun profilage. Les signaux d'âge de Google Play sont interdits d'usage publicitaire [FAIT].
- **Pubs adaptées à l'âge** (2.5.18) [FAIT] : filtre de contenu de la régie au niveau le plus strict (réglage à vérifier).
- **États-Unis** : COPPA [NV] et lois des États sur la vérification d'âge (Utah, Texas, Louisiane [FAIT] ; Californie au 1er janvier 2027 [NV]). Portée pour un éditeur français : à faire valider. D'ici là, distribution limitée à l'Union européenne.

## 6. Statut « trader » (DSA)

- **Apple** : déclaration obligatoire ; sans statut vérifié, mises à jour bloquées et application retirée dans l'UE [FAIT]. L'évaluation revient au développeur ; les revenus d'achats et de publicité sont des indices [FAIT].
- **Pour ce projet** : jeu financé par la pub et les achats, édité en micro-entreprise : trader, très probablement [HYPOTHÈSE]. Se déclarer trader dès la version 1.0, puisque la monétisation est prévue.
- **Conséquence** : adresse ou boîte postale, téléphone et e-mail, vérifiés puis publiés sur la fiche dans les 27 pays de l'UE [FAIT]. Google affiche l'adresse complète d'un compte personnel qui monétise [NV]. D'où la décision L8 : e-mail et numéro dédiés, boîte postale ou domiciliation (coût à chiffrer [HYPOTHÈSE]).

## 7. Plan de la politique de confidentialité

Page publique, en français et en anglais, accessible depuis les fiches et les réglages du jeu (hébergement : décision L7).

1. Qui est responsable : nom du porteur de projet (affiché comme vendeur [FAIT]), contact.
2. En bref : ce que le jeu collecte selon la version — rien en 1.0.
3. Données, finalités, bases légales, durées de conservation (tableau du §2).
4. Destinataires : liste nominative des SDK et de leurs partenaires.
5. Transferts hors UE et garanties (cadre DPF, clauses) [NV].
6. Tes droits (accès, rectification, effacement, opposition, retrait du consentement, réclamation auprès de la CNIL) et la façon de les exercer, dans le jeu ou par e-mail.
7. Mineurs ; achats, traités par Apple et Google.
8. Date de version, modifications, mentions légales de l'éditeur [NV].

## 8. Points à soumettre à la DPO avant toute intégration

1. **Testeurs** : note d'information de `testing/01` ; adresses saisies dans la Play Console et TestFlight (transfert).
2. **AdMob** : transfert vers les États-Unis, certification DPF et clauses [NV] ; rôle de Google (responsable, responsable conjoint ou sous-traitant au sens de l'article 28) ; engagements du fournisseur [FAIT : l'éditeur doit auditer ses SDK].
3. **Fenêtre de consentement** : conformité de l'écran UMP, affichage différé au premier appui, preuve et durée de validité du choix, délai avant une nouvelle sollicitation.
4. **Refus** : absence de pub, bonus sans pub, place du Pass Chantier (« payer ou consentir »).
5. **Pubs non personnalisées sans identifiant publicitaire** : suffisant pour se passer de l'invite ATT ?
6. **Mineurs** : public de 13 ans et plus, moins de 15 ans en France, utilité d'un écran d'âge.
7. **Statistiques** : confirmer « stores seulement » ; conditions d'un outil exempté hébergé dans l'UE.
8. **Achats** : intégration directe ; RevenueCat et Unity IAP exclus sauf avis contraire.
9. **Politique de confidentialité** : relecture du texte, hébergement de la page.
10. **Pays de distribution** : Union européenne d'abord ; États-Unis et autres pays après avis.
11. **Registres** : SDK et contrats [FAIT] ; registre des traitements, à prévoir avec la DPO.
