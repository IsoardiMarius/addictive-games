# Référence stores et monétisation — jeu mobile gratuit (pubs + achats intégrés), développeur individuel en France

> **État au 12 novembre 2026.** Document factuel destiné au plan de lancement et aux scénarios économiques. Ce n'est ni un conseil juridique ni un conseil fiscal.

## 0. Méthode, limites, légende

- **Sources lues en session le 12/11/2026** : developer.apple.com, developer.android.com, dépôts GitHub officiels (Unity, Godot, Capacitor, CNIL, Urssaf/betagouv, SDK des régies), pub.dev. L'outil de lecture ne renvoie que des **résumés ou extraits**, pas les pages complètes.
- **Limite majeure** : le quota de recherche web de la session (partagé avec d'autres agents) s'est épuisé après **6 recherches abouties** sur les 40 et plus prévues. support.google.com, cnil.fr, developers.google.com, unity.com, les sites des régies et des instituts de benchmarks étaient inaccessibles.
- **Conséquences** : Apple est très bien couvert ; Android technique bien couvert ; les politiques Play Console, AdMob, la recommandation CNIL 2024 et la TVA sont en partie restituées de mémoire ; **aucun repère de marché n'a pu être sourcé** (§4.3 et §5 sont des hypothèses) ; angle mort possible sur les annonces de juin à novembre 2026 hors Apple et Android.

| Étiquette | Sens |
|---|---|
| **[FAIT]** | Lu en session sur la source officielle citée, ou au moins 2 sources concordantes |
| **[ESTIMATION]** | Repère de marché d'un tiers, sourcé et daté — **aucune dans cette version** |
| **[HYPOTHÈSE]** | Raisonnement ou ordre de grandeur non sourcé |
| **[NV]** | Règle ou chiffre connu du modèle (connaissances arrêtées à juin 2026), **non relu en session** : à traiter comme une hypothèse jusqu'à vérification |

Les codes [A…], [G…], [T…], [F…] renvoient à la liste des sources (§10).

## 1. Apple App Store

### 1.1 Compte et programme développeur

| Sujet | Règle | Statut |
|---|---|---|
| Prix | 99 USD par année d'adhésion, en devise locale lorsque disponible (99 € en France [NV]). Exonération réservée aux associations, établissements d'enseignement et entités publiques | [FAIT] [A1][A2] |
| Individuel | Nom légal personnel affiché comme vendeur ; ni D-U-N-S ni entité juridique ; majorité légale, compte Apple avec double authentification, carte bancaire à son nom | [FAIT] [A2] |
| Organisation | Entité juridique, D-U-N-S (gratuit), site web et e-mail au nom de domaine, pouvoir d'engager l'entité | [FAIT] [A2] |
| Entrepreneur individuel | « If you are a sole proprietor/single-person business, you must join as an individual » : une micro-entreprise s'inscrit en individuel. Conversion en organisation possible sur demande | [FAIT] [A2] |
| Sans adhésion payante | Xcode et test sur appareil limités (3 appareils, 10 App ID, profils expirant en 7 jours) ; ni TestFlight ni App Store | [FAIT] [A26] |

### 1.2 Commission et paiement

| Périmètre | Taux | Statut |
|---|---|---|
| Hors UE (défaut) | 30 % ; 15 % pour le Small Business Program (SBP) et les abonnements après 1 an | [FAIT] [A1][A3] |
| **UE depuis le 01/10/2026** (conditions unifiées, annexe 14 du contrat) — achat intégré Apple | **26 %** ; **15 %** en SBP et pour les abonnements après 1 an. « No changes are required on your part » si l'on reste sur l'achat intégré | [FAIT] [A4][A5][A28] |
| UE — paiement alternatif dans l'app | 20 % / 10 % ; TVA et rapport mensuel à la charge du développeur ; options de paiement à maintenir 12 mois ; contrôle parental pour les mineurs | [FAIT] [A4] |
| UE — offre hors app par lien | 15 % / 10 % sur les ventes dans les 7 jours suivant le clic | [FAIT] [A4] |
| UE — distribution hors App Store | Core Technology Commission de 5 % ; l'ancienne Core Technology Fee par installation, l'Initial Acquisition Fee et la Store Services Fee sont supprimées | [FAIT] [A4][A5] |
| Autres pays | Chine 25 % / 12 % (15/03/2026) ; Taïwan 26 % (31/10/2026) ; États-Unis : liens d'achat externes autorisés sans autorisation préalable (3.1.1(a)) | [FAIT] [A27][A18] |

- **SBP** [FAIT] [A3] : au plus 1 M USD de « proceeds » (ventes nettes de commission et de taxes) sur l'année civile précédente et sur l'année en cours ; nouveaux développeurs éligibles ; **inscription à demander** ; comptes associés à déclarer ; taux réduit appliqué 15 jours après la fin du mois fiscal d'approbation ; dépassement → taux standard pour les ventes suivantes, requalification possible l'année d'après.
- **Assiette** [FAIT] [A24] : « proceeds are calculated based on the price… minus any applicable taxes » ; Apple calcule la taxe sur les transactions.
- **Paiement** [FAIT] [A22] : sous 45 jours après la fin du mois fiscal, un seul virement par mois fiscal ; seuil minimal 0,02 USD pour un compte France/EUR ; conditions : contrat « Paid Apps », coordonnées bancaires, formulaires fiscaux.

### 1.3 Statut « trader » (DSA)

- Déclaration obligatoire ; sans statut vérifié, mises à jour bloquées (depuis le 16/10/2024) et app retirée de l'App Store dans l'UE (depuis le 17/02/2025). [FAIT] [A7]
- Auto-évaluation par le développeur. Indices cités : revenus tirés de l'app (achats intégrés, app payante, **app financée par la publicité**), pratiques commerciales, immatriculation à la TVA, lien avec l'activité professionnelle. [FAIT] [A6] Un jeu monétisé édité sous micro-entreprise relève très probablement du statut trader. [HYPOTHÈSE]
- Trader individuel : **adresse ou boîte postale, téléphone, e-mail**, vérifiés (codes + justificatif) puis **publiés sur la fiche produit dans les 27 pays de l'UE**. Une adresse alternative exige un justificatif de rattachement. [FAIT] [A6]
- Non-trader : les consommateurs de l'UE sont informés que le droit de la consommation ne s'applique pas au contrat. [FAIT] [A6]

### 1.4 Exigences techniques

| Exigence | Détail | Statut |
|---|---|---|
| Xcode / SDK | Depuis le 28/04/2026 : build avec Xcode 26 ou plus et SDK iOS 26. **À partir d'avril 2027 : SDK iOS 27** (Xcode 27). Depuis le 09/09/2026 : cible iOS 13 au minimum | [FAIT] [A7][A8] |
| Mac | Xcode 26.0 à 26.3 : macOS Sequoia 15.6+ ; Xcode 26.4 à 26.6 : macOS Tahoe 26.2+ ; Xcode 27 : macOS Tahoe 26.6+. macOS 26 est le dernier système pour Mac Intel ; macOS 27 exige Apple silicon | [FAIT] [A9][A8] |
| Cloud | Xcode Cloud : 25 h/mois incluses, puis 49,99 USD/mois pour 100 h. Codemagic : 500 min/mois gratuites sur macOS M2 (compte personnel), puis 0,095 USD/min. GitHub Actions : runners standard gratuits pour les dépôts publics ; grands runners macOS 0,077 à 0,102 USD/min (tarif macOS standard non lu) | [FAIT] [A10][T16][T15] |
| Captures | iPhone Duo (sorti le 23/10/2026) : captures exigées à partir d'avril 2027 | [FAIT] [A27] |
| TestFlight | 100 testeurs internes (utilisateurs App Store Connect), 10 000 externes, build valable 90 jours, 30 appareils par testeur ; le premier build d'un groupe externe passe en revue ; lien public possible | [FAIT] [A11] |

Un Mac (ou une CI macOS) reste indispensable : même Godot exige « a computer running macOS with Xcode installed » pour l'export iOS. [FAIT] [T2]

### 1.5 Confidentialité côté Apple

| Sujet | Règle | Statut |
|---|---|---|
| App Privacy (« nutrition labels ») | Obligatoire pour toute soumission ; déclarer les données collectées par l'app **et par les SDK tiers** (régies, analytics) ; URL de politique de confidentialité obligatoire. Jeu avec pubs : identifiant d'appareil, données publicitaires, interactions, diagnostics, achats sont typiquement concernés | [FAIT] [A12] |
| Privacy manifests | Depuis le 01/05/2024 : raisons approuvées pour les « required reason API » (app et SDK). SDK listés devant fournir manifeste et signature : UnityFramework, Capacitor, Cordova, Flutter et de nombreux plugins, hermes, modules Firebase, GoogleUtilities. Google Mobile Ads, UMP, AppLovin et ironSource n'y figurent pas nommément | [FAIT] [A7][A13] |
| ATT | Autorisation requise avant tout « tracking » (croisement avec des données tierces à des fins publicitaires, partage à des courtiers) et avant l'accès à l'IDFA ; un SDK qui combine les données compte comme du tracking ; interdits : conditionner une fonction ou récompenser l'acceptation (5.1.2(i)), fingerprinting | [FAIT] [A14][A18] |
| ATT dans l'UE | À partir d'iOS 27.2 : invite alternative, **seule version disponible pour les apps distribuées en France**, Allemagne, Italie, Pologne et Roumanie ; nouvelle sollicitation possible un an après le choix. Sortie effective d'iOS 27.2 non confirmée | [FAIT] [A15][A14] |
| Publicité (2.5.18) | Pubs adaptées à la classification d'âge ; interstitiels identifiés comme tels, bouton de fermeture visible ; moyen de signaler une pub inappropriée ; pas de ciblage sur des données d'enfants | [FAIT] [A18] |
| Catégorie Enfants (1.3, 5.1.4) | Ni publicité ni analytics tiers (exceptions limitées) ; achats derrière un contrôle parental | [FAIT] [A18] |

### 1.6 Classification par âge et vérification d'âge

| Sujet | Règle | Statut |
|---|---|---|
| Système | 4+, 9+, 13+, 16+, 18+ (annoncé le 24/07/2025 ; réponses exigées depuis le 31/01/2026) | [FAIT] [A7][A27] |
| Effets pour un jeu | Publicité compatible 4+ ; loot boxes → 9+ (16+ en Australie, 18+ au Brésil) ; jeu d'argent simulé peu fréquent → 13+, fréquent → 18+ ; visuels de la fiche toujours conformes 4+ (2.3.8) | [FAIT] [A16][A20][A18] |
| Nouveautés 2026 | Questions « réseau social » exigées depuis septembre 2026 ; nouveaux descripteurs (localisation partagée, rencontres) à partir du 16/11/2026, à renseigner pour toutes les apps avant le 12/01/2027 ; iOS 27 : « Time Allowances » parentales par catégorie, dont Jeux | [FAIT] [A17][A8] |
| Vérification d'âge | API Declared Age Range (SDK iOS 26.2+). Apps 18+ bloquées sans confirmation d'âge en Australie, au Brésil et à Singapour depuis le 24/02/2026. Tranches d'âge transmises pour les nouveaux comptes : Utah 06/05/2026, Texas 04/06/2026 (loi SB 2420, après levée de l'injonction), Louisiane 01/07/2026. Le développeur reste responsable de ses propres obligations | [FAIT] [A20] |

### 1.7 App Review (Guidelines mises à jour le 16/10/2026)

| Règle | Contenu utile | Statut |
|---|---|---|
| 2.1 | App complète, testée, sans contenu provisoire ; plus de 40 % des problèmes non résolus | [FAIT] [A18][A19] |
| 3.1.1 | Tout déblocage de contenu numérique passe par l'achat intégré ; probabilités des loot boxes affichées avant l'achat ; monnaies achetées sans date d'expiration ; mécanisme de restauration | [FAIT] [A18] |
| 3.2.2(x) | Interdit de forcer une note, un avis ou un téléchargement ; inciter à regarder une publicité reste permis | [FAIT] [A18] |
| 4.1 | (a) ne pas copier une app populaire ni retoucher son nom ou son interface ; (b) usurpation : exclusion possible du programme ; (c) pas d'icône, de marque ni de nom d'un autre développeur sans son accord (ajout de novembre 2025) | [FAIT] [A18][A27] |
| 4.2 | L'app doit aller « beyond a repackaged website », être utile, unique, « app-like » et offrir une valeur de divertissement durable ; 4.2.6 : apps issues de gabarits commerciaux refusées | [FAIT] [A18] |
| 4.3 | (a) pas de multiples Bundle ID d'une même app ; (b) pas d'app indistinguable de l'existant ; récidive : exclusion possible | [FAIT] [A18] |
| 4.7 et 2.5.2 | 4.7 vise les mini-jeux HTML5 **non embarqués** dans le binaire. Un jeu web empaqueté (Capacitor) embarque son code : il relève de 4.2 et de 2.5.2 (pas de téléchargement de code modifiant les fonctionnalités) | [FAIT] + [HYPOTHÈSE] de lecture |
| 5.1 | Pas de connexion obligatoire sans fonction de compte ; suppression de compte dans l'app s'il y a création ; consentement explicite avant tout partage à des tiers ; minimisation | [FAIT] [A18] |
| Délais | « at least 50% of submissions in less than 24 hours and 90% in less than 48 hours » ; revue accélérée possible sur demande motivée | [FAIT] [A19] |

## 2. Google Play

### 2.1 Compte

| Sujet | Règle | Statut |
|---|---|---|
| Frais | 25 USD, paiement unique, sans renouvellement | [FAIT] [G9] (résumé de recherche) |
| Identité | Compte personnel : nom légal, adresse, pièce d'identité officielle, e-mail et téléphone vérifiés par code. Organisation : D-U-N-S | [FAIT] [G5] |
| Vérification des développeurs Android | Ouverte à tous depuis mars 2026 ; appliquée à partir du 30/09/2026 au Brésil, en Indonésie, à Singapour et en Thaïlande ; extension mondiale en 2027 ; environ 99 % des apps du Play enregistrées automatiquement | [FAIT] [G5] |
| Appareil | Les nouveaux comptes personnels doivent prouver l'accès à un appareil Android réel | [NV] |
| Coordonnées publiques | Nom légal, pays et e-mail de contact affichés ; adresse complète affichée pour un compte personnel qui monétise ; déclaration « trader » au titre du DSA | [NV] |

### 2.2 Tests et accès à la production

| Sujet | Règle | Statut |
|---|---|---|
| **Test fermé obligatoire** | Comptes personnels créés après le 13/11/2023 : **au moins 12 testeurs inscrits en continu pendant 14 jours** avant de demander l'accès production ; un testeur qui sort puis revient repart de zéro | [FAIT] [G2] |
| Historique | 20 testeurs à l'origine, ramenés à 12 (date du 11/12/2024 donnée par des sites tiers) | non confirmé |
| Demande d'accès | Examen en 7 jours ou moins selon des guides tiers ; Google peut imposer de prolonger le test si l'engagement est jugé faible ; la revue d'une nouvelle app peut ensuite prendre plusieurs jours | [NV] |
| Comptes organisation | Dispensés selon des sites tiers | non vérifié |
| Pistes | Interne : jusqu'à 100 testeurs, diffusion quasi immédiate. Fermé : listes d'e-mails ou groupes. Ouvert : public, accessible après l'accès production pour un compte personnel | [NV] |

Planning : compter 3 à 4 semaines au minimum entre le premier build en test fermé et la mise en production. [HYPOTHÈSE]

### 2.3 Exigences techniques

| Exigence | Détail | Statut |
|---|---|---|
| Niveau d'API cible | Depuis le 31/08/2026 : nouvelles apps et mises à jour sur **Android 16 (API 36)** ; apps existantes en API 35 au minimum pour rester visibles sur les appareils récents ; l'extension au 01/11/2026 est échue | [FAIT] [G1] |
| Format | AAB obligatoire pour les nouvelles apps depuis août 2021 (rappelé par la doc Godot [T2]) ; Play App Signing associé [NV] | [FAIT] / [NV] |
| Pages mémoire de 16 Ko | Apps ciblant Android 15+ avec code natif (Unity, Godot, SDK natifs) ; **à partir du 01/02/2027**, les mises à jour non conformes sont refusées ; AGP 8.5.1+ et NDK r28+ | [FAIT] [G3] |
| Play Billing Library | Version 7 échue (31/08/2026, extension 01/11/2026) : **version 8 au minimum**, valable jusqu'au 31/08/2027 ; version 9 disponible | [FAIT] [G4] |
| Mémoire (jeux) | Février 2027 : seuils de « bad behavior » mémoire exigés pour les jeux (Android vitals) | [FAIT] [G12] |
| Level Up | Programme volontaire (inscription dans la Play Console depuis le 01/09/2026) ouvrant des « rate card benefits » : Play Games Services v2, 10 succès au minimum, Sidekick, sauvegarde cloud, récompenses (30/09/2026 et 01/03/2027), multi-écrans | [FAIT] [G12] |

### 2.4 Commission

- **Régime historique** : 15 % sur le premier million USD annuel après inscription au programme, 30 % au-delà, 15 % sur les abonnements. [NV]
- **2026 — INCERTAIN** : des sources secondaires rapportent un accord Google–Epic (mars 2026) et une nouvelle grille au 30/06/2026 aux États-Unis, au Royaume-Uni et dans l'EEE, avec deux versions contradictoires : (a) 20 % sur les achats intégrés, 10 % sur les abonnements, plus 5 % si l'on utilise la facturation Google Play ; (b) 10 % jusqu'à 1 M USD par an, puis 20 % (nouvelles installations) ou 25 % (installations existantes), plus 5 % de frais de facturation. Aucune grille officielle n'a pu être lue. [G8]
- **Hypothèse de travail** : environ 15 % au total sous 1 M USD avec la facturation Google Play, soit le même ordre de grandeur qu'Apple en SBP. [HYPOTHÈSE] À vérifier dans la Play Console avant tout chiffrage.

### 2.5 Déclarations et politiques

| Sujet | Règle | Statut |
|---|---|---|
| Data safety | Formulaire obligatoire ; déclarer collecte et partage, **y compris par les SDK tiers** ; tout écart constaté expose à des mesures d'application | [FAIT] [G7] |
| Politique de confidentialité | URL obligatoire pour toutes les apps ; suppression de compte s'il y a des comptes | [NV] |
| Classification | Questionnaire IARC obligatoire (PEGI en Europe) ; une app non classée peut être retirée | [NV] |
| Public cible, Familles | Déclaration du public cible. Si des enfants sont visés : politique Familles (SDK publicitaires auto-certifiés uniquement, ni pub personnalisée ni identifiant publicitaire). Public mixte : écran d'âge neutre. Page officielle non lue [G13] | [NV] |
| Publicité | Déclaration « Contient des annonces » ; permission AD_ID pour lire l'identifiant publicitaire | [NV] (AD_ID cité en [G7]) |
| Signaux d'âge | API Play Age Signals (bêta) : tranches 0-12, 13-15, 16-17, 18+ ; usage limité aux expériences adaptées à l'âge, **interdit pour la publicité, le marketing, le profilage et l'analytics** ; niveaux de vérification renforcés seulement dans les régions réglementées (exemple : Texas) | [FAIT] [G6] |
| Usurpation, contenu répétitif, spam | Interdits : imiter une app ou une marque (nom, icône), publier plusieurs apps très similaires, apps sans valeur propre, simple WebView d'un site tiers | [NV] |

## 3. Confidentialité et conformité (UE / France)

| Thème | Règle | Implication | Statut |
|---|---|---|---|
| Traceurs (ePrivacy) | Consentement **préalable** à toute lecture ou écriture sur le terminal, y compris IDFA, IDFV, Android ID. Finalités soumises : publicité personnalisée, **mesure de la publicité non ciblée**, publicité géolocalisée, personnalisation, partage social | Ne pas initialiser le SDK publicitaire avant le choix ; les pubs « non personnalisées » n'échappent pas au consentement | [FAIT] [F1] fiche 16 |
| Forme du consentement | Refuser doit être aussi simple qu'accepter ; premier niveau (finalités, tiers), second niveau par finalité ; retrait possible à tout moment | CMP conforme et entrée « Confidentialité » permanente dans les réglages | [FAIT] [F1] |
| Mesure d'audience exemptée | Finalité unique pour l'éditeur, aucun suivi entre apps ou sites, statistiques anonymes, ni recoupement ni cession ; traceurs 13 mois, données 25 mois. Les offres qui réutilisent les données pour leur compte (Google Analytics cité) sont hors exemption | Firebase/Google Analytics : consentement requis ; outil auto-hébergé ou configuré pour l'exemption : possible sans consentement [HYPOTHÈSE] | [FAIT] [F1] fiche 17 |
| SDK tiers | L'éditeur doit auditer ses SDK, vérifier les engagements du fournisseur, le contrat de sous-traitance (art. 28 RGPD), les transferts hors UE, l'information et le consentement | Tenir un registre des SDK et de leurs contrats | [FAIT] [F1] fiche 9 |
| Recommandation CNIL « applications mobiles » | Publiée le 24/09/2024, contrôles annoncés à partir de 2025 ; une permission système (dont l'ATT) ne vaut pas consentement | Afficher la CMP, puis l'invite ATT | [NV] |
| CMP certifiée par Google | Depuis le 16/01/2024 (EEE, Royaume-Uni ; Suisse au 31/07/2024) : CMP certifiée et compatible TCF exigée pour diffuser les annonces AdMob, personnalisées ou non ; à défaut, seules les « annonces limitées » restent éligibles. UMP est la CMP gratuite de Google | Les plugins AdMob Godot, Capacitor et Unity intègrent UMP [FAIT] [T3][T4][T8] | [NV] |
| Transferts hors UE | Google, Unity, AppLovin traitent des données aux États-Unis ; cadre DPF (décision d'adéquation du 10/07/2023, recours rejeté par le Tribunal de l'UE le 03/09/2025, pourvoi en cours) | Vérifier la certification DPF et les clauses de chaque SDK ; l'indiquer dans la politique | [NV] |
| Mineurs — UE | RGPD art. 8 : consentement parental en dessous de 15 ans en France ; DSA art. 28 : pas de publicité fondée sur le profilage de mineurs sur les plateformes en ligne (lignes directrices du 14/07/2025) | Viser un public général 13+, ne pas cibler les enfants | [NV] |
| Mineurs — États-Unis | COPPA (moins de 13 ans) : règle modifiée en vigueur le 23/06/2025, conformité au 22/04/2026 [NV]. Lois sur les stores : Utah, Texas, Louisiane (dates en §1.6) [FAIT] [A20] ; Californie AB 1043 au 01/01/2027 [NV] | Choisir : intégrer les signaux d'âge ou restreindre la distribution. Portée exacte pour un éditeur français à faire valider | mixte |
| Information | Politique de confidentialité publique (exigée par Apple [FAIT] [A12], par Google [NV]) : responsable, finalités, bases légales, SDK destinataires, transferts, durées, droits, contact ; mentions légales de l'éditeur ; probabilités des loot boxes | Page web et accès depuis le jeu | [NV] |
| Consommation | Principes européens sur les monnaies virtuelles (03/2025) ; projet de Digital Fairness Act, statut fin 2026 inconnu | Afficher les prix réels | [NV] |

**Cadre interne de l'organisation** : prévoir dès la conception la minimisation, les droits des personnes, les durées de conservation et les mentions RGPD ; tout transfert de données hors UE (AdMob, Unity, AppLovin, Firebase, PostHog US) passe d'abord par la DPO ; si une base de données est nécessaire, PostgreSQL.

## 4. Monétisation

### 4.1 Régies et médiations

| | Google AdMob | Unity LevelPlay (ex-ironSource) | AppLovin MAX |
|---|---|---|---|
| Nature | Régie Google avec médiation et enchères | Médiation de Unity, réseaux Unity Ads et ironSource Ads | Médiation d'AppLovin et réseau AppLovin |
| Formats | Bannière, interstitiel, récompensée, interstitiel récompensé, ouverture d'app, natif [FAIT] [T3][T4] | Bannière, interstitiel, récompensée | Bannière, interstitiel, récompensée |
| Conditions | Titulaire majeur, vérification d'identité et d'adresse, **app-ads.txt** publié sur le site développeur déclaré dans la fiche store, règles de placement (pas d'interstitiel au lancement ni à la sortie, récompense annoncée et volontaire) | Compte Unity, app-ads.txt | Compte AppLovin, app-ads.txt |
| Paiement | Seuil de 70 € (100 USD), versement mensuel | Seuil de 100 USD, délai à vérifier | Non connu |
| Consentement UE | CMP certifiée obligatoire (UMP) | Reprend le consentement TCF | Reprend le consentement TCF |
| Statut | [NV] | [NV] | [NV] |

Pour un premier jeu, un seul SDK (AdMob) limite l'intégration et les contrats à gérer ; une médiation se justifie quand le volume d'impressions devient significatif. [HYPOTHÈSE]

### 4.2 Plugins par moteur (vérifiés sur GitHub et pub.dev le 12/11/2026)

| Moteur | AdMob | LevelPlay | AppLovin MAX | Achats intégrés |
|---|---|---|---|---|
| Unity | **Officiel Google** : v11.7.0 (12 nov.), Unity 2022.3 minimum, Android API 24+ [T8] | Officiel (`Unity-sdk`) [T10] | Officiel (`AppLovin-MAX-Unity-Plugin`) [T9] | RevenueCat `purchases-unity` [T11] ; Unity IAP [NV] |
| Godot 4 | **Communautaire** (Poing Studios, MIT, Godot 4.5+, Android et iOS, UMP inclus) [T3] | Aucun plugin officiel repéré [T10] | Officiel (`AppLovin-MAX-Godot`, Godot 4.x) [T9] | Pas de SDK RevenueCat repéré [T11] |
| Flutter | **Officiel** `google_mobile_ads` 9.2.0 (éditeur google.dev) [T6] | Officiel (`Flutter-SDK`) [T10] | Officiel [T9] | `purchases-flutter` [T11] |
| React Native | **Communautaire** (Invertase, Apache-2.0, New Architecture requise) [T7] | Officiel (`react-native-SDK`) [T10] | Officiel [T9] | `react-native-purchases` [T11] |
| Capacitor / Cordova | **Communautaire** `@capacitor-community/admob` v8.2.0 (Capacitor 8.5+, iOS 15+, API 24+, UMP et ATT inclus, MIT) [T4] | Aucun plugin officiel repéré | Aucun plugin officiel repéré (liste des dépôts lue en partie) | `purchases-capacitor`, `cordova-plugin-purchases` [T11] |

Ces éléments sont [FAIT] au sens « présence et état du dépôt » ; « non repéré » ne prouve pas l'absence, et la qualité réelle de chaque plugin reste à éprouver.

### 4.3 eCPM par format et par zone — [HYPOTHÈSE], fiabilité faible

Aucune source n'a pu être lue. Ordres de grandeur de mémoire (rapports publics 2023-2025 de type Appodeal, Tenjin, GameAnalytics), en USD pour 1 000 impressions, jeux casual et hypercasual :

| Format | États-Unis | Europe de l'Ouest / France | Reste du monde |
|---|---|---|---|
| Récompensée | 10 à 20 | 4 à 10 | 0,5 à 3 |
| Interstitielle | 6 à 14 | 2,5 à 7 | 0,4 à 2,5 |
| Bannière | 0,3 à 1,0 | 0,1 à 0,5 | 0,03 à 0,2 |

- iOS se situe en haut des fourchettes, Android en bas ; le quatrième trimestre est le plus élevé ; sans consentement (annonces limitées) et sans médiation, viser le bas des fourchettes. [HYPOTHÈSE]
- Formule : revenu pub par joueur actif quotidien = somme, par format, de (impressions par joueur × eCPM / 1 000).

### 4.4 Achats intégrés

| Sujet | Règle | Statut |
|---|---|---|
| Paliers Apple | 800 paliers de prix, plus 100 sur demande jusqu'à 10 000 USD ; un pays de référence, prix générés et maintenus pour les 174 autres storefronts | [FAIT] [A23] |
| Paliers Google | Prix libres par pays, plancher autour de 0,05 USD | [NV] |
| « Suppression des pubs » | Produit non consommable (Apple) ou produit ponctuel non consommé (Google), avec restauration des achats ; prix usuel de 2 à 5 € | [FAIT] pour la restauration [A18] ; [HYPOTHÈSE] pour le prix |
| Règles | Achat intégré obligatoire pour le contenu numérique, monnaies sans expiration, probabilités des loot boxes ; Play Billing Library 8+ | [FAIT] [A18][G4] |
| Promotion | Apple : les « offer codes » couvrent tous les types d'achats ; plus de création de codes promo pour achats intégrés depuis le 26/03/2026 | [FAIT] [A27] |
| Outils | RevenueCat : SDK officiels iOS, Android, Unity, Flutter, React Native, Capacitor, Cordova, Kotlin Multiplatform, web [FAIT] [T11] ; gratuit jusqu'à 2 500 USD de revenu mensuel suivi, puis 1 % [NV]. Alternative sans coût : StoreKit 2 et Play Billing en direct | mixte |

## 5. Repères de performance du marché — [HYPOTHÈSE], fiabilité faible

Aucun rapport n'a pu être consulté. Ordres de grandeur de mémoire, à remplacer par des [ESTIMATION] sourcées (GameAnalytics « Mobile Gaming Benchmarks », Adjust « Mobile App Trends », AppsFlyer « State of Gaming App Marketing », Liftoff « Casual Gaming Apps Report », Sensor Tower « State of Mobile Gaming », Tenjin « Ad Monetization Benchmark »).

| Indicateur | Médiane | Premier quartile (meilleurs 25 %) |
|---|---|---|
| Rétention J1, tous jeux mobiles | 20 à 23 % | 26 à 30 % |
| Rétention J7 | 3 à 5 % | 7 à 9 % |
| Rétention J28 / J30 | environ 1 % | 2,5 à 4 % |
| Durée de session | 4 à 5 min | 8 à 9 min |

| Segment | Seuils couramment cités par les éditeurs | Revenu par joueur actif quotidien (USD) | Part des pubs |
|---|---|---|---|
| Hypercasual | J1 ≥ 35 à 40 %, J7 ≥ 10 % | 0,03 à 0,10 | 90 à 100 % |
| Hybrid-casual | J1 ≥ 40 %, J7 ≥ 15 %, J30 ≥ 5 % | 0,10 à 0,30 | 40 à 70 % |
| Casual | J1 ≥ 35 %, J7 ≥ 12 %, J30 ≥ 5 % | 0,05 à 0,25 | très variable ; achats intégrés majoritaires chez les leaders |

| Coût par installation (USD) | Hypercasual iOS | Hypercasual Android | Casual iOS | Casual Android |
|---|---|---|---|---|
| États-Unis | 0,5 à 1,2 | 0,25 à 0,7 | 2 à 5 | 1 à 3 |
| Europe de l'Ouest | 0,3 à 0,8 | 0,15 à 0,4 | 1 à 3 | 0,5 à 1,5 |
| Reste du monde | 0,1 à 0,3 | 0,03 à 0,15 | 0,3 à 1 | 0,1 à 0,5 |

Un petit jeu indépendant sans médiation ni achat d'audience se situe plutôt en bas de ces fourchettes de revenu. [HYPOTHÈSE]

## 6. Outils d'analyse

| Outil | Faits vérifiés | Points RGPD | Statut |
|---|---|---|---|
| Statistiques des stores | App Store Connect Analytics enrichi le 25/03/2026 (plus de 100 indicateurs, cohortes, comparaison aux pairs) ; Play Console : statistiques et Android vitals | Aucun SDK ni transfert ajoutés par l'éditeur | [FAIT] [A27] / [NV] |
| Firebase / Google Analytics | Modules Firebase présents dans la liste Apple des SDK à manifeste | Hors exemption CNIL lorsque Google réutilise les données : consentement requis ; transfert vers les États-Unis | [FAIT] [A13][F1] |
| GameAnalytics | SDK officiels : Unity, Unreal, Android, iOS, JavaScript, Cordova, Defold, GameMaker, C++ ; ni Godot, ni Flutter, ni React Native visibles sur la page de l'organisation | Lieu d'hébergement et conditions à vérifier | [FAIT] [T12] |
| Unity Analytics | Réservé à Unity ; offre et conditions non relues | Consentement à gérer | [NV] |
| PostHog | Licence MIT (hors dossier `ee`) ; auto-hébergement Docker conseillé jusqu'à environ 100 000 événements par mois, sans support ; cloud aux États-Unis **ou dans l'UE** ; 1 million d'événements gratuits par mois ; SDK iOS, Android, React Native, Flutter | L'instance UE évite le transfert ; exemption de consentement seulement si la configuration respecte les critères CNIL | [FAIT] [T13] |
| Autres pistes | Matomo, Aptabase, Countly, TelemetryDeck (auto-hébergeables ou hébergées en Europe) | Non vérifiées | [NV] |

Données typiquement collectées : identifiant d'installation, modèle d'appareil, système, pays (déduit de l'adresse IP), événements de jeu, sessions, achats et revenus publicitaires. Elles sont à déclarer dans App Privacy et Data safety. [FAIT] pour l'obligation [A12][G7]

## 7. Technologies : faits utiles au choix du moteur

| Option | Faits | Statut |
|---|---|---|
| **Unity** | Conditions de l'éditeur mises à jour le 30/06/2026 : **Unity Personal jusqu'à 200 000 USD** de revenus et financements sur 12 mois ; Pro de 200 001 à 24 999 999 USD ; Enterprise au-delà. Le texte exclut tout « Runtime Fee » ou partage de revenus pour les versions antérieures. Mention « made with Unity » exigée seulement si le projet comporte des crédits | [FAIT] [T1] |
| | Runtime Fee abandonnée en septembre 2024 ; écran de démarrage facultatif en Personal depuis Unity 6 ; prix de Pro non relu | [NV] |
| | Écosystème publicitaire le plus complet : plugins officiels AdMob, LevelPlay, MAX ; UnityFramework soumis au manifeste Apple | [FAIT] [T8][T9][T10][A13] |
| **Godot** | Licence MIT, sans redevance ni seuil ; dernière version stable 4.7.2 (18/08/2026) | [FAIT] [T2] |
| | iOS : export depuis macOS avec Xcode uniquement. Android : OpenJDK 17, SDK plateforme 36, build Gradle pour produire un AAB | [FAIT] [T2] |
| | AdMob via un plugin communautaire ; MAX via un plugin officiel ; pas de plugin officiel LevelPlay ni RevenueCat repéré | [FAIT] [T3][T9][T10][T11] |
| **Web (TypeScript/HTML5) + Capacitor** | Capacitor 8.6.0 et 7.6.10 (28/10/2026), 9.0 en bêta ; Capacitor et Cordova soumis au manifeste Apple | [FAIT] [T5][A13] |
| | Apple : accepté si l'app dépasse le « repackaged website » (4.2) et n'exécute pas de code téléchargé modifiant ses fonctions (2.5.2). Google : pas d'interdiction connue d'un jeu embarqué dans une WebView | [FAIT] [A18] / [NV] |
| | AdMob et achats via des plugins communautaires ou RevenueCat ; pas de plugin officiel LevelPlay ni MAX repéré | [FAIT] [T4][T11] |
| **Portails web** | Poki et CrazyGames : partage des revenus publicitaires, SDK du portail obligatoire ; Poki : environ 50 %, et 100 % sur le trafic apporté par le développeur ; taux et exclusivités à vérifier | [NV] |
| **Playable ads** | Réseaux visés par l'outil open source `cocos-pnp` : AppLovin, Facebook, Google, ironSource, Liftoff, Mintegral, Moloco, Pangle, TikTok, Unity | [FAIT] [T14] |
| | Exigences usuelles : HTML autonome (fichier unique ou ZIP), environ 5 Mo au maximum, aucun appel réseau externe, API de sortie propre au réseau (`mraid.open`, `ExitApi.exit`…). Un jeu HTML5 se recycle plus directement en playable | [NV] / [HYPOTHÈSE] |

## 8. Coûts fixes et statut

### 8.1 Coûts minimaux

| Poste | Coût | Statut |
|---|---|---|
| Apple Developer Program | 99 USD par an | [FAIT] [A1] |
| Google Play | 25 USD une seule fois | [FAIT] [G9] |
| Build iOS | 0 € avec un Mac compatible (macOS 15.6+ jusqu'en avril 2027, puis macOS 26.6+) ; sinon CI macOS : 25 h/mois incluses dans Xcode Cloud, 500 min/mois gratuites chez Codemagic | [FAIT] [A9][A10][T16] |
| Appareils de test | Au moins un iPhone et un appareil Android réels | [HYPOTHÈSE] |
| Nom de domaine et page publique | 10 à 20 € par an ; héberge la politique de confidentialité, la page d'assistance et app-ads.txt | [HYPOTHÈSE] |
| Adresse publiable (DSA) | Boîte postale ou domiciliation pour ne pas publier son adresse personnelle : coût à chiffrer | [HYPOTHÈSE] |
| Régies, analytics, RevenueCat | 0 € au démarrage (offres gratuites) | [NV] |

**Total minimal** : environ 130 à 150 € la première année, puis 110 à 120 € par an, hors matériel et hors domiciliation. [HYPOTHÈSE]

### 8.2 Statut juridique et fiscalité (France) — à faire valider par l'Urssaf, le service des impôts ou un expert-comptable

| Sujet | Élément | Statut |
|---|---|---|
| Nécessité d'un statut | Encaisser des revenus récurrents (achats, publicité) constitue une activité professionnelle à immatriculer ; les stores n'exigent pas de société | [NV] |
| Micro-entreprise : plafonds | **Depuis le 01/01/2026 : 83 600 € par an** pour les prestations de services et activités libérales, **203 100 €** pour les ventes (77 700 € et 188 700 € auparavant) ; sortie du régime après deux années consécutives de dépassement | [FAIT] [F2] |
| Cotisations (sur le chiffre d'affaires) | Services BIC 21,2 % ; **BNC hors Cipav 25,6 % depuis janvier 2026** (24,6 % en 2025) ; ventes 12,3 % ; formation professionnelle 0,1 à 0,3 % | [FAIT] [F2] |
| Impôt sur le revenu | Abattement forfaitaire de 50 % (services BIC) ou 34 % (BNC) ; versement libératoire de 1,7 % (services BIC) ou 2,2 % (BNC) si le revenu fiscal de référence 2024 ne dépasse pas 29 315 € par part | [FAIT] [F2] |
| Qualification BIC ou BNC | Édition de jeux et vente d'espace publicitaire plutôt BIC ; programmation plutôt BNC : à trancher à l'immatriculation | [HYPOTHÈSE] |
| TVA collectée auprès des joueurs | Apple calcule la taxe et verse un montant net de taxes et de commission [FAIT] [A24] ; Google agit comme vendeur dans l'EEE et reverse la TVA [NV] | mixte |
| TVA du développeur | Franchise en base possible ; seuils 2026 **non vérifiés** ; les relations avec les entités irlandaises d'Apple et de Google relèvent des prestations intracommunautaires (numéro de TVA, autoliquidation, déclaration européenne de services) | [NV] |
| Formulaires fiscaux Apple | « All developers must complete a US tax form » ; hors États-Unis, un W-8BEN (particulier), W-8BEN-E ou W-8ECI selon le questionnaire | [FAIT] [A21] |
| Formulaires fiscaux Google | Informations fiscales américaines (W-8BEN) dans le profil de paiement ; retenue possible sur les revenus de source américaine, réduite par la convention France–États-Unis | [NV] |
| DAC7 | Apple déclare au fisc irlandais les développeurs résidents de l'UE vendant des « services personnels » ; le contenu numérique non personnalisé n'en fait « typiquement » pas partie : un jeu ne devrait pas être concerné | [FAIT] [A25] + [HYPOTHÈSE] |
| Autres | Cotisation foncière des entreprises, compte bancaire dédié au-delà de 10 000 € de chiffre d'affaires deux années de suite | [NV] |

## 9. Obligations bloquantes et points à re-vérifier

**Bloquant avant publication**

1. Google Play : test fermé de 12 testeurs pendant 14 jours consécutifs, puis demande d'accès production. [FAIT]
2. Apple : adhésion, contrat « Paid Apps », banque et formulaire fiscal ; build Xcode 26+ ; statut trader DSA vérifié ; App Privacy ; questionnaire d'âge complet (nouveaux descripteurs avant le 12/01/2027). [FAIT]
3. Android : API 36, AAB, Play Billing Library 8+, formulaire Data safety, classification IARC, public cible. [FAIT] / [NV]
4. Publicité dans l'UE : CMP certifiée (UMP) affichée avant l'initialisation du SDK, puis invite ATT sur iOS ; politique de confidentialité publique ; app-ads.txt. [FAIT] / [NV]
5. Statut juridique en France avant le premier encaissement. [NV]

**Points incertains à vérifier sur la source officielle avant toute décision**

| Point | Où vérifier |
|---|---|
| Grille 2026 des frais Google Play | Play Console, aide « Frais de service » |
| Affichage des coordonnées, pistes de test, Familles, usurpation et contenu répétitif sur Google Play | Aide Play Console, centre des règles |
| Conditions AdMob, LevelPlay et MAX : seuils de paiement, app-ads.txt, CMP certifiée | Aides des régies |
| eCPM, rétention, coût par installation, revenu par joueur | Rapports cités au §5 |
| Recommandation CNIL « applications mobiles », DPF, COPPA, lois des États américains, AB 1043 | CNIL, Commission européenne, FTC |
| Portée des lois américaines de vérification d'âge pour un éditeur français | Conseil juridique |
| Seuils 2026 de franchise de TVA, qualification BIC ou BNC, TVA intracommunautaire, retenues | Service des impôts, Urssaf, expert-comptable ; pages officielles en [F3] |
| Prix de Unity Pro, écran de démarrage, conditions de Poki et CrazyGames, exigences des playable ads | Sites des éditeurs |
| Sortie effective d'iOS 27.2 et de l'invite ATT alternative | Notes de version Apple |
| Annonces de juin à novembre 2026 hors Apple et Android | Nouvelle session de recherche |

## 10. Sources (toutes apparues dans les résultats de la session du 12/11/2026)

**Apple**
- [A1] https://developer.apple.com/programs/whats-included/
- [A2] https://developer.apple.com/support/enrollment/
- [A3] https://developer.apple.com/app-store/small-business-program/
- [A4] https://developer.apple.com/support/apps-in-the-eu/
- [A5] https://developer.apple.com/news/?id=gmws0jgp
- [A6] https://developer.apple.com/help/app-store-connect/manage-compliance-information/manage-european-union-digital-services-act-trader-requirements/
- [A7] https://developer.apple.com/news/upcoming-requirements/
- [A8] https://developer.apple.com/news/?id=k1mtkt1k
- [A9] https://developer.apple.com/support/xcode/
- [A10] https://developer.apple.com/xcode-cloud/
- [A11] https://developer.apple.com/testflight/ · https://developer.apple.com/help/app-store-connect/test-a-beta-version/testflight-overview
- [A12] https://developer.apple.com/app-store/app-privacy-details/
- [A13] https://developer.apple.com/support/third-party-SDK-requirements/
- [A14] https://developer.apple.com/app-store/user-privacy-and-data-use/
- [A15] https://developer.apple.com/news/?id=idsft9ai
- [A16] https://developer.apple.com/help/app-store-connect/reference/app-information/age-ratings-values-and-definitions/
- [A17] https://developer.apple.com/news/?id=yxkh08us · https://developer.apple.com/news/?id=tlur8uvi
- [A18] https://developer.apple.com/app-store/review/guidelines/
- [A19] https://developer.apple.com/distribute/app-review/
- [A20] https://developer.apple.com/news/?id=f5zj08ey · https://developer.apple.com/news/?id=sg176nne · https://developer.apple.com/support/age-assurance
- [A21] https://developer.apple.com/help/app-store-connect/manage-tax-information/provide-tax-information
- [A22] https://developer.apple.com/help/app-store-connect/getting-paid/overview-of-receiving-payments · https://developer.apple.com/help/app-store-connect/reference/reporting/minimum-payment-threshold
- [A23] https://developer.apple.com/help/app-store-connect/manage-app-pricing/set-a-price
- [A24] https://developer.apple.com/help/app-store-connect/manage-app-information/set-a-tax-category
- [A25] https://developer.apple.com/help/app-store-connect/manage-compliance-information/manage-information-for-directive-on-administrative-cooperation-7th-amendment
- [A26] https://developer.apple.com/support/compare-memberships/
- [A27] https://developer.apple.com/news/
- [A28] https://www.apple.com/newsroom/2026/08/apple-announces-changes-for-apps-in-the-european-union/ · https://www.ashurstperkinscoie.com/en/insights/apple-updates-app-store-developer-terms-in-the-eu/

**Google / Android**
- [G1] https://developer.android.com/google/play/requirements/target-sdk
- [G2] https://support.google.com/googleplay/android-developer/answer/14151465?hl=en (lu via un résumé de recherche)
- [G3] https://developer.android.com/guide/practices/page-sizes
- [G4] https://developer.android.com/google/play/billing/deprecation-faq
- [G5] https://developer.android.com/developer-verification · https://developer.android.com/developer-verification/guides/early-access
- [G6] https://developer.android.com/google/play/age-signals/overview · https://developer.android.com/google/play/age-signals/understand-age-signals-responses
- [G7] https://developer.android.com/privacy-and-security/declare-data-use
- [G8] sources secondaires : https://finance.yahoo.com/news/google-settles-epic-games-drops-200512326.html · https://tech-insider.org/epic-google-play-store-changes-2026/ · https://www.megamobilecontent.com/news/2026/06/26/google-slashes-play-store-fees-epic-settlement/ · https://launchshots.app/blog/google-play-15-percent-service-fee-2026
- [G9] https://support.google.com/googleplay/android-developer/answer/6112435?hl=fa (lu via un résumé de recherche)
- [G12] https://developer.android.com/games/guidelines
- [G13] https://play.google.com/console/about/families/ (adresse de redirection, page non lue)

**Technologies et outils**
- [T1] https://github.com/Unity-Technologies/TermsOfService
- [T2] https://github.com/godotengine/godot/releases · https://raw.githubusercontent.com/godotengine/godot/master/LICENSE.txt · https://raw.githubusercontent.com/godotengine/godot-docs/master/tutorials/export/exporting_for_ios.rst · https://raw.githubusercontent.com/godotengine/godot-docs/master/tutorials/export/exporting_for_android.rst
- [T3] https://github.com/poingstudios/godot-admob-plugin
- [T4] https://github.com/capacitor-community/admob
- [T5] https://github.com/ionic-team/capacitor/releases
- [T6] https://pub.dev/packages/google_mobile_ads
- [T7] https://github.com/invertase/react-native-google-mobile-ads
- [T8] https://github.com/googleads/googleads-mobile-unity/releases
- [T9] https://github.com/orgs/AppLovin/repositories?q=MAX · https://github.com/AppLovin/AppLovin-MAX-Godot · https://github.com/AppLovin/AppLovin-MAX-Flutter
- [T10] https://github.com/ironsource-mobile
- [T11] https://github.com/orgs/RevenueCat/repositories?q=purchases
- [T12] https://github.com/GameAnalytics
- [T13] https://github.com/PostHog/posthog
- [T14] https://github.com/ppgee/cocos-pnp
- [T15] https://raw.githubusercontent.com/github/docs/main/content/billing/reference/actions-runner-pricing.md · https://raw.githubusercontent.com/github/docs/main/content/billing/concepts/product-billing/github-actions.md
- [T16] https://raw.githubusercontent.com/codemagic-ci-cd/codemagic-docs/master/content/billing/pricing.md

**France**
- [F1] Guide RGPD du développeur de la CNIL, fiches 9, 16 et 17 : https://github.com/LINCnil/Guide-RGPD-du-developpeur
- [F2] Règles du simulateur officiel de l'Urssaf (dépôt betagouv/mon-entreprise) : https://github.com/betagouv/mon-entreprise/tree/master/modele-social/règles
- [F3] Références officielles citées par ce simulateur, non lues en session : https://www.service-public.fr/professionnels-entreprises/vosdroits/F32353 · https://entreprendre.service-public.gouv.fr/vosdroits/F36232 · https://www.autoentrepreneur.urssaf.fr/portail/accueil/une-question/questions-frequentes.html · https://bofip.impots.gouv.fr/bofip/1802-PGP.html
