# Analyse de la concurrence — jeux jouables issus des « fausses publicités » mobiles

**Date :** 12 novembre 2026 · **Périmètre :** applications réellement jouables sur iOS et Android · **Lecteur :** développeur solo assisté par IA qui veut publier la version honnête d'un mini-jeu de publicité (méthode « Real War: Not Fake ») en corrigeant les défauts des concurrents.

## 0. Lecture rapide

- **Frustration n° 1 dans les cinq familles :** une interstitielle après chaque niveau (parfois en pleine partie) et un achat « No Ads » qui ne retire pas tout. C'est un défaut de modèle économique, pas de mécanique : un solo peut le corriger.
- **Écart demande / qualité le plus grand :** familles **D** (shooter à portails) et **C** (machine évolutive et destruction) ; preuves en section 8.
- **A et B sont saturées.** En A, les deux leaders sont mal notés sur Android (3,5 et 3,8) mais les clones se comptent par dizaines ; en B, les leaders sont bien notés (4,6 à 4,7).
- **E est hétérogène :** E2 (muraille type Kingshot) est prometteuse mais peu documentée ici, E3 a déjà un leader de qualité, E1 et E4 sont des marchés de clones.
- **Propriété intellectuelle :** la mécanique est libre, le nom ne l'est pas. Aucune marque tierce (« Last War », « Kingshot », « Flow ») dans le nom, l'icône ou les métadonnées.

## 1. Méthode, légende, fiabilité

- 44 recherches web exécutées, en anglais et en français ; 4 autres refusées, le quota partagé de la session étant épuisé. Je n'ai vu que des **extraits et résumés de résultats**, pas les pages, sauf deux pages de developer.apple.com lues directement. App Store, Google Play, Sensor Tower, AppMagic, Appfigures et support.google.com n'ont pas pu être ouverts.
- **[FAIT]** : donnée rapportée par une fiche store, un texte officiel ou la presse, vue en extrait et non recontrôlée. **[ESTIMATION]** : chiffre d'un outil d'estimation ou d'un agrégateur (Sensor Tower, AppMagic, AppGoblin, AppBrain, Marlvel, GameBrain). **[HYPOTHÈSE]** : jugement d'analyste. **(m)** : hypothèse tirée de ma mémoire, absente des résultats. **n.t.** : non trouvé. **n.c.** : date de la donnée non communiquée. **GP** : Google Play. **AS** : App Store.
- Les agrégateurs divergent et datent rarement leurs captures (rapports Marlvel générés automatiquement, « score d'audience » recalculé par GameBrain, installations estimées par AppGoblin) : ce sont des ordres de grandeur.
- Je n'ai lancé aucun jeu. Les critiques citées sont des [FAIT] rapportés par les extraits, traduits ; le rendu visuel est marqué (m), comme la présence sur Android de Sand Balls, Stone Miner, Water Connect Puzzle et Gold and Goblins.

## 2. Famille A — Puzzle de sauvetage (goupilles, « draw to save »)

### 2.1 Chiffres

| Application | Éditeur · année · plateformes | Popularité | Note et avis (date de la donnée) | Src |
|---|---|---|---|---|
| Pull the Pin | Popcore · 2020 (m) · iOS, Android | GP ≈ 198 M installations [ESTIMATION AppGoblin, n.c.] ; tranche 100 M à 1 Md [FAIT via AppBrain] | Android 3,5 sur 1,35 M avis ; iOS 4,4 sur 250 k [ESTIMATION Marlvel, 17/04/2026] | A1–A4 |
| Pull Him Out | Lion Studios · 2020 (m) · iOS, Android | 20,4 M installations, 521 k joueurs mensuels (fiche iOS) [ESTIMATION AppGoblin, n.c.] | 4,25 sur 521 k avis ; iOS 4,3, Android 4,2 [ESTIMATION Marlvel, n.c.] | A5–A7 |
| Save the Girl | Lion Studios · jeu le plus téléchargé au monde en avril 2020 [ESTIMATION Sensor Tower via PocketGamer.biz] · iOS, Android | GP ≈ 182 à 186 M installations [ESTIMATION AppGoblin, n.c.] | Android 3,8 sur 969 k avis ; iOS 4,4 sur 313 k [ESTIMATION Marlvel, n.c.] | A8–A11 |
| « Hero Rescue » (homonymes) | Rescue Hero: Pull The Pin (GameeStudio) ; Rescue Hero: Pull Pin Games (MobStudios) ; Hero Rescue – Pin Puzzle (Wiggle Woggle) · années n.t. | 5,2 M et 1,4 M installations [ESTIMATION AppGoblin, n.c.] ; le « Hero Rescue » d'origine : n.t. | 91,4 k et 4 k avis ; GameBrain 78 % sur 1 403 avis (Wiggle Woggle) [ESTIMATION, n.c.] | A12–A15 |
| Home Pin / How to Loot | Gamee ; Homa ; A42 Games · années n.t. · iOS, Android | n.t. ; A42 revendique « plus de 75 000 joueurs » sur l'AS [FAIT déclaré] | GameBrain 85 % sur 247 avis (Home Pull Pin), 84 % sur 711 (How to Loot) [ESTIMATION, n.c.] | A16–A19 |
| Save the Doge | WONDER GROUP · 2022 (m) · Android ; iOS à vérifier | 118,9 M téléchargements [ESTIMATION agrégateur, juillet 2025] ; « plus de 80 M » [FAIT presse, attribution incertaine] | GP 4,5 sur 359 k avis [ESTIMATION agrégateur, juillet 2025] ; GameBrain 41 % sur 10 000 avis [ESTIMATION] | A20–A22 |

Référence : selon Playrix, ces mini-jeux figurent dans Homescapes et Gardenscapes depuis avril 2020 ; environ 0,03 % des joueurs de Homescapes les atteignaient, et l'ASA britannique a jugé les pubs trompeuses à l'automne 2020 [FAIT, A23–A25].

### 2.2 Expérience et monétisation

| Application | Critiques récurrentes | Visuel / fluidité | Répétitivité · progression | Monétisation |
|---|---|---|---|---|
| Pull the Pin | Pub après presque chaque niveau, vidéos non passables ; « No Ads » payé mais pubs persistantes ; « trop facile », « fait pour regarder des pubs » | 3D épurée, physique de billes (m) | Niveaux courts, difficulté plate ; progression linéaire | Interstitielles, récompensées, achat « No Ads » |
| Pull Him Out | Pub « après chaque niveau », « toutes les 5 secondes » ; niveaux faciles et répétitifs ; des joueurs coupent le Wi-Fi pour y échapper | n.t. | Répétitif selon les avis | « No Ads » retire bannières et interstitielles, pas les récompensées ; revenus à 77 % publicitaires [ESTIMATION AppGoblin] |
| Save the Girl | Pub pour gagner, perdre ou passer un niveau ; pubs qui ne se chargent pas et bloquent la reprise ; demande de nouveaux niveaux | n.t. | Contenu fini, vite consommé | Interstitielles, récompensées, achats intégrés |
| Hero Rescue, Home Pin, How to Loot | « Les pubs gâchent l'expérience » ; Hero Rescue 2 « miné par les pubs » ; monétisation = premier thème négatif | Graphismes et humour plutôt loués | n.t. | Gratuit, achats intégrés, option « no ads » |
| Save the Doge | Bugs ; « nombre excessif de pubs non passables » | 2D, tracé au doigt et physique (m) | n.t. | n.t. |

### 2.3 Synthèse

- **Saturation : forte.** Acteurs : Popcore, Lion Studios, Gamee, Homa, Wonder Group. Une seule page de résultats AppGoblin affichait une dizaine d'applications « pull pin » distinctes [FAIT].
- **Frustrations exploitables :** pub après chaque niveau, « No Ads » incomplet, puzzles trop faciles, contenu fini.
- **Angles réalistes :** vrais puzzles à difficulté croissante et à solutions multiples ; aucune interstitielle, pub récompensée réservée aux indices ; niveau du jour ; univers graphique propre.
- **Technique :** 2D avec moteur physique, accessible. Le coût est dans le contenu : 100 à 200 niveaux faits main, car la génération procédurale garantit mal l'intérêt d'un puzzle à goupilles [HYPOTHÈSE]. Prévoir un éditeur et un solveur par simulation.
- **PI et stores :** mécanique libre, noms génériques. Le risque est le rejet pour application « indiscernable » (Apple 4.3 b) et le contenu répétitif côté Google. Ne reprendre aucun personnage d'annonceur.

**Sources A :** A1 https://marlvel.ai/apps/pull-the-pin · A2 https://appgoblin.info/apps/com.maroieqrwlk.unpin/keywords · A3 https://www.appbrain.com/dev/Popcore+Games/ · A4 https://apps.apple.com/us/app/pull-the-pin/id1496150467?l=es · A5 https://appgoblin.info/apps/1515233303 · A6 https://marlvel.ai/api/llm/apps/games/pull-him-out · A7 https://apps.apple.com/gb/app/pull-him-out/id1515233303 · A8 https://appgoblin.info/apps/com.xmgame.savethegirl · A9 https://marlvel.ai/api/llm/apps/games/save-the-girl · A10 https://www.pocketgamer.biz/top-global-downloads-april-2020-sensor-tower · A11 https://apps.apple.com/au/app/save-the-girl/id1490384223 · A12 https://appgoblin.info/apps/com.ohzegame.rescuehero · A13 https://appgoblin.info/apps/com.rescuehero.pull.pin · A14 https://gamebrain.co/game/hero-rescue-pin-puzzle-pul · A15 https://gamebrain.co/game/hero-rescue-2 · A16 https://apps.apple.com/us/app/how-to-loot-pull-the-pin/id1505997676 · A17 https://apps.apple.com/us/app/id1510387438 · A18 https://gamebrain.co/game/home-pull-pin-how-to-loot · A19 https://gamebrain.co/game/how-to-loot-pull-the-pin-1 · A20 https://gamebrain.co/game/save-the-doge · A21 https://grand-screen.com/games/save-the-doge/ · A22 https://www.pocketgamer.biz/deconstructing-save-the-doge-and-draw-to-mesh-puzzle-games · A23 https://www.gamedeveloper.com/business/uk-regulator-bans-misleading-i-homescapes-i-i-gardenscapes-i-pin-puzzle-ads · A24 https://gameworldobserver.com/2020/10/13/playrixs-misleading-ads-banned-uk · A25 https://gamerant.com/advertising-standards-authority-misleading-ad-ban/

## 3. Famille B — Tuyaux, chemins, liquides

### 3.1 Chiffres

| Application | Éditeur · année · plateformes | Popularité | Note et avis (date de la donnée) | Src |
|---|---|---|---|---|
| Flow Free | Big Duck Games · 2012 (m) · iOS, Android | n.t. (le seuil de 100 M n'a pas pu être confirmé) | 4,72 sur ≈ 1,9 M avis ; iOS 4,8 sur 390 k, Android 4,7 sur 1,6 M [ESTIMATION Marlvel, n.c.] | B1–B3 |
| Water Sort Puzzle | IEC Global · 2020 (m) · iOS, Android | GP : tranche 100 M+ [FAIT, extrait de fiche] | GP 4,6 sur 1,34 M avis [FAIT, extrait, n.c.] ; AS ≈ 4,7 sur ≈ 324 k [ESTIMATION agrégateur, n.c.] | B4–B7 |
| Water Connect Puzzle | IEC Global · année n.t. · iOS, Android | n.t. | AS États-Unis 4,69 sur 43,2 k notes, la même page affichant aussi 3,87 [ESTIMATION agrégateur, n.c.] ; GameBrain 48 % sur 1 100 avis [ESTIMATION] | B8–B11 |
| Dig This! | Raketspel · 2019 (m) · iOS, Android | GP ≈ 72,9 M installations [ESTIMATION AppGoblin, mars 2026] | GP 4,43 sur 548 k avis [ESTIMATION AppGoblin, mars 2026] ; iOS 4,7 sur ≈ 405 k [ESTIMATION AppFollow, fin 2019] | B12–B14 |
| Sand Balls | SayGames · 2019 (m) · iOS, Android | n.t. | AS 4,6 sur 281 k avis [ESTIMATION App Pricing Lab, n.c.] | B15–B16 |
| Happy Glass | Lion Studios · 2018 [FAIT] · iOS, Android | n.t. | 3,7 sur 1,1 M avis [ESTIMATION Marlvel, 05/05/2026] ; GameBrain 47 % sur 1 300 avis [ESTIMATION] | B17–B20 |

Non couverts, faute de recherches restantes : Plumber et les autres jeux de rotation de tuyaux.

### 3.2 Expérience et monétisation

| Application | Critiques récurrentes | Visuel / fluidité | Répétitivité · progression | Monétisation |
|---|---|---|---|---|
| Flow Free | Pubs « très longues » qui « abîment l'appli » ; signalements de redirections malveillantes [ESTIMATION Marlvel, à confirmer] | 2D minimaliste et lisible (m) | Déclinaisons en applications séparées (Bridges, Hexes, Warps, Shapes) [FAIT] ; packs de grilles | Pubs, achats de packs (m) |
| Water Sort Puzzle | Pubs « excessives et inappropriées » ; éblouissement après une mise à jour | 2D simple (m) | Répétitif par nature, niveaux numérotés | Pubs, achats intégrés, « No Ads » 2,99 $ [FAIT] |
| Water Connect Puzzle | Pub après chaque niveau ; le son des pubs coupe la musique ; minuteur imposé | n.t. | n.t. | Pubs, achats, option « No Ads » [FAIT] |
| Dig This! | « Pub de 30 secondes après CHAQUE partie » ; gels ; l'éditeur répond que la pub finance le développement | Physique 2D de sable et de billes (m) | « Plus de 9 000 niveaux » selon un avis ; ajouts réguliers | « No ads » 4,99 $ (2,99 $ selon la région) ; packs de solutions et packs saisonniers payants [FAIT] |
| Sand Balls | « Pub toutes les minutes » ; « No Ads » payé mais pubs encore exigées pour débloquer véhicules, clés et niveaux ; plantages après pub | 3D colorée (m) | n.t. | Interstitielles et récompensées ; « No Ads » partiel |
| Happy Glass | Common Sense Media : « noyé sous un flot de pubs » ; pub pour jouer comme pour rejouer ; données partagées à des fins de ciblage (Pixalate) | 2D au trait, physique de liquide (m) | Des centaines de niveaux (plus de 500 selon une source) | Interstitielles entre niveaux, achats intégrés |

### 3.3 Synthèse

- **Saturation : forte**, avec des leaders bien notés (Big Duck Games, IEC Global). L'offre satisfait déjà la demande sur les grilles et le tri de couleurs.
- **Frustrations exploitables :** pubs longues ou après chaque niveau, minuteurs, « No Ads » partiel ; surtout sur les puzzles à physique de liquide (Happy Glass 3,7 ; plaintes vives sur Sand Balls).
- **Angles réalistes :** jeu payant à petit prix ou pubs uniquement volontaires ; mode sans minuteur ; puzzles de liquide avec vraie physique ; grille quotidienne générée.
- **Technique :** la famille la plus accessible. 2D pur, génération procédurale fiable avec solveur (chemins, tuyaux, tri) [HYPOTHÈSE]. La physique de particules coûte plus cher.
- **PI et stores :** la fiche de Flow Free indique que « Flow » et « Flow Free » sont des marques déposées [FAIT] : éviter « Flow » dans le nom. Catégorie établie, donc exposition à Apple 4.3 b.

**Sources B :** B1 https://marlvel.ai/api/llm/apps/games/flow-free · B2 https://apps.apple.com/gb/app/flow-free/id526641427 · B3 https://appgoblin.info/apps/com.bigduckgames.flowbridges/trends · B4 https://apps.apple.com/us/app/1514542157 · B5 https://play.google.com/store/apps/eventdetails/4831948684745034394 · B6 https://kimola.com/reports/unlock-insights-water-sort-puzzle-user-feedback-report-app-store-us-153352 · B7 https://splitmetrics.com/apps/water-sort-puzzle/id1514542157 · B8 https://apps.apple.com/app/id1544706956 · B9 https://worldsapps.com/reviews-water-connect-puzzle · B10 https://gamebrain.co/game/water-connect-puzzle · B11 https://marlvel.ai/intel-report/games/water-connect-puzzle · B12 https://appgoblin.info/apps/se.raketspel.digaround · B13 https://apps.apple.com/gb/app/dig-this/id1453411110 · B14 https://appfollow.io/ios/dig-it/1453411110?country=us · B15 https://apps.apple.com/gb/app/sand-balls/id1474046667 · B16 https://apppricinglab.com/app/apple/1474046667 · B17 https://www.commonsensemedia.org/app-reviews/happy-glass · B18 https://marlvel.ai/intel-report/games/happy-glass · B19 https://gamebrain.co/game/happy-glass · B20 https://pixalate.com/blog/app-review-series-happy-glass

## 4. Famille C — Machine évolutive et destruction

### 4.1 Chiffres

| Application | Éditeur · année · plateformes | Popularité | Note et avis (date de la donnée) | Src |
|---|---|---|---|---|
| Hole.io | Voodoo · 2018 (m) · iOS, Android | GP ≈ 391,8 M installations [ESTIMATION AppGoblin, n.c.] ; 10,6 M+ téléchargements sur 30 jours [ESTIMATION Insightrackr, n.c.] | GP 3,0 à 3,1 sur 1,3 M avis ; AS 4,6 sur 2 M [ESTIMATION agrégateurs, n.c.] ; GameBrain 51 % sur 10 000 avis | C1–C5 |
| Attack Hole – Black Hole Games | Homa (studio Redline Games) · 2022–2023 (m) · iOS, Android | Plus de 90 M téléchargements en avril 2024 [FAIT déclaré par Homa] ; ≈ 100,9 M [ESTIMATION AppGoblin] | GP 4,2 sur ≈ 445 k avis [FAIT, extrait, n.c.] | C6–C9 |
| Stone Miner | ZPLAY (m) · 2021 (m) · iOS, Android | n.t. | AS ≈ 4,67 sur ≈ 115 k avis [ESTIMATION agrégateur, n.c.] ; 4,51 sur ≈ 246 k [ESTIMATION Marlvel] ; GameBrain 37 % sur 1 060 avis | C10–C12 |
| Stone Grass: Mowing Simulator | Freeplay · 2022 (m) · iOS, Android | GP : tranche 50 M+ [FAIT] ; 72,9 puis 77,5 M [ESTIMATION AppGoblin] | GP 4,2 sur 558 k avis [FAIT, extrait, n.c.] | C13–C15 |
| Drill & Collect: idle mine dig | APPS Teknoloji · 2022 (m) · iOS, Android | ≈ 15,1 M installations [ESTIMATION AppGoblin] | AS 4,7 sur 82,7 k avis ; 4,20 sur 172 k selon AppGoblin [ESTIMATION, n.c.] | C16–C18 |
| Dig Deep | CrazyLabs · 2022 (m) · iOS, Android | Android ≈ 43 M, iOS ≈ 6,4 M [ESTIMATION AppGoblin] | AS ≈ 4,7 sur 107 à 128 k notes [FAIT, extraits, n.c.] | C19–C21 |
| Gold and Goblins: Idle Games | AppQuantum / Redcell Games · 2020 (m) · iOS, Android | 6 M de joueurs, 760 k joueurs quotidiens, 7 M$ en trois mois [FAIT déclaré par l'éditeur, promotionnel, non daté] | AS 4,4 sur 40 k notes [FAIT, extrait, n.c.] | C24–C25 |
| A Game About Digging A Hole (portage officiel) | DoubleBee ; rokaplay et Headup · 31/07/2025 [FAIT] · iOS, Android | Plus d'un million d'exemplaires vendus, annonce éditeur toutes plateformes [FAIT] ; installations mobiles n.t. | n.t. | C26–C28 |

Sans donnée de store : Ground Digger: Lava Hole Drill (Homa, Android et iOS ; descente par couches de sable, de glace et de lave jusqu'au centre de la Terre ; C22–C23). Démolition : Building Destruction (180°) ≈ 11,4 M installations et 50,4 k avis [ESTIMATION AppGoblin] ; City Demolish: Rocket Smash! (plus de 300 niveaux) ; Fake Island: Demolish! ; aucun titre à 100 M n'est apparu (C29–C31).

### 4.2 Expérience et monétisation

| Application | Critiques récurrentes | Visuel / fluidité | Répétitivité · progression | Monétisation |
|---|---|---|---|---|
| Hole.io | Pubs fréquentes, dont des pubs « à la mi-temps » en pleine partie ; achat « No Ads » non restauré ; adversaires robots, multijoueur jugé trompeur, mode en ligne retiré | 3D simple (m) ; batterie, plantages et absence de son signalés | Parties courtes identiques | Interstitielles, achat « No Ads » contesté |
| Attack Hole | Fréquence des pubs ; valeur du VIP contestée [ESTIMATION Marlvel] | n.t. | Niveaux et améliorations | Abonnement VIP sans pub à 6,99 $ par semaine après 3 jours d'essai [FAIT] ; ≈ 69 % pub, 31 % achats [ESTIMATION AppGoblin] |
| Stone Miner | « No Ads » payé mais fenêtres de pub « toutes les quelques secondes » ; restauration d'achat défaillante ; pubs nécessaires aux améliorations ; sauvegarde perdue | Boucle jugée « fun et addictive » ; surchauffe et plantages signalés | « Seulement 40 niveaux », pas de musique | Interstitielles, récompensées, « No Ads » non respecté selon les avis |
| Stone Grass | Sentiment récent négatif [ESTIMATION Marlvel] ; détail n.t. | n.t. | n.t. | Part de la pub : 36 à 60 % des revenus selon la capture [ESTIMATION AppGoblin] |
| Drill & Collect | « Pub toutes les 15 à 20 secondes » ; d'autres avis jugent la pub supportable ; un demi-point d'écart entre iOS et Android | n.t. | Améliorations de foreuse, jeu incrémental | Interstitielles, achat sans pub |
| Dig Deep | « Trop de pubs », pubs qui font planter le jeu | Jugé relaxant | Lassant à la longue | Interstitielles, récompensées |
| Gold and Goblins | n.t. | n.t. | Fusion et jeu incrémental, événements compétitifs limités dans le temps | Gemmes, offres d'événement dès ≈ 3,99 $, « Remove All Ads Forever » à 14,99 $ [FAIT] |
| A Game About Digging A Hole | L'éditeur dénonce « une tripotée d'applis » qui réutilisent ses images ou clonent le jeu [FAIT] | Portage 3D du jeu PC | Jeu court avec une fin (m) | Payant, 4,99 $ [FAIT] |

### 4.3 Synthèse

- **Saturation : moyenne à forte.** Forte sur le « trou noir » (Voodoo, Homa, clones comme Hole Master: Army Attack) et sur le minier incrémental (Idle Miner Tycoon de Kolibri : tranche 100 M+, 4,5 sur 5,8 M avis [FAIT, extrait]). Moyenne sur la machine qui creuse et se transforme. Faible sur la démolition.
- **Frustrations exploitables :** pub toutes les 15 à 20 secondes, « No Ads » non respecté, contenu court, faux multijoueur, surchauffe.
- **Angles réalistes :** boucle finie « creuser, vendre, améliorer, nouvelle zone » avec une vraie fin, à la manière d'A Game About Digging A Hole ; améliorations visibles sur la machine ; aucune interstitielle ; prix unique ou déblocage unique.
- **Technique :** les concurrents sont en 3D, mais une vue en coupe 2D avec terrain destructible en grille est faisable [HYPOTHÈSE]. Peu de niveaux faits main ; l'effort porte sur les sensations (particules, sons, vibrations) et sur l'équilibrage de l'économie.
- **PI et stores :** « A Game About Digging A Hole™ » est exploité comme une marque et son éditeur surveille les clones. Voodoo a déjà obtenu en France une condamnation pour concurrence déloyale et parasitisme (section 7). Ne pas reprendre l'apparence d'un concurrent.

**Sources C :** C1 https://appgoblin.info/apps/io.voodoo.holeio · C2 https://blog.insightrackr.com/en/docs/HoleioIntelligence · C3 https://marlvel.ai/api/llm/apps/games/hole-io · C4 https://gamebrain.co/game/hole-io · C5 https://apps.apple.com/app/hole-io/id1389111413 · C6 https://www.homagames.com/games/attack-hole-redline-games · C7 https://appgoblin.info/apps/com.redlinegames.attackhole/trends · C8 https://apps.apple.com/us/app/id1661115841 · C9 https://appireview.pages.dev/com.redlinegames.attackhole · C10 https://apps.apple.com/us/app/id1579285349?l=en-us · C11 https://marlvel.ai/apps/stone-miner/reviews · C12 https://gamebrain.co/game/stone-miner · C13 https://appgoblin.info/apps/com.frph.stonegrass/trends · C14 https://apps.apple.com/app/id1606739002 · C15 https://marlvel.ai/api/llm/apps/games/stone-grass-lawn-mower-game · C16 https://apps.apple.com/us/app/id1642304873 · C17 https://apppricinglab.com/app/apple/1642304873 · C18 https://appgoblin.info/apps/tr.com.apps.drill.and.collect/trends · C19 https://apps.apple.com/us/app/dig-deep/id1620044758 · C20 https://appgoblin.info/apps/com.dig.deep · C21 https://marlvel.ai/apps/dig-deep/reviews · C22 https://skich.app/games/ground-digger-lava-hole-drill · C23 https://apkpure.com/ground-digger-lava-hole-drill/com.higames.grounddigger · C24 https://apps.apple.com/us/app/id1529762599?l=en-us · C25 https://appquantum.com/publishing-use-cases/G_Goblins_AppQ.pdf · C26 https://store.steampowered.com/news/app/3244220/view/537735054151909742 · C27 https://www.gamespress.com/A-Game-About-Digging-A-Hole-Is-Out-Now-Mobile · C28 https://steamcommunity.com/app/3244220/allnews/ · C29 https://appgoblin.info/apps/com.hunderdeightydegree.VoxelDestructionPrototype · C30 https://appgoblin.info/apps/com.fourties.citydemolish · C31 https://mwm.ai/apps/fake-island-demolish/1603568507 · Hole Master : https://appgoblin.info/apps/com.armyevolution.attackhole/trends · Idle Miner Tycoon : https://play.google.com/store/apps/eventdetails/4832077538198796431

## 5. Famille D — Shooter à multiplicateurs et portails

### 5.1 Chiffres

| Application | Éditeur · année · plateformes | Popularité | Note et avis (date de la donnée) | Src |
|---|---|---|---|---|
| Real War: Not Fake | DevDop (Dan Lespect, indépendant français) ; certaines fiches GP affichent « Heseri Games » · 27/11/2025 [FAIT] · Android, iOS, web | GP : tranche 500 k+ (AppBrain) ou 1 M+ (fiche GP) selon la capture [FAIT] | GP 2,9 sur 6,64 k avis ; 3,4 sur 9,46 k ; 3,61 sur 7 631 (AppBrain) ; AS 3,6 sur 53 notes, ou 4,4 sur 5 dans un autre pays [FAIT, extraits, n.c.] | D1–D7 |
| Last War: Survival (mini-jeu intégré) | FirstFun / FUNFLY · 2023 (m) · iOS, Android | 180 M$ d'achats intégrés en septembre 2025, premier mondial [ESTIMATION Sensor Tower via TechNode] ; plus de 1,1 Md$ sur un an [ESTIMATION Sensor Tower, source secondaire] | n.t. | D8–D14 |
| Mob Control | Voodoo · 2021 (m) · iOS, Android | GP ≈ 204,5 M installations [ESTIMATION AppGoblin] ; ≈ 85,2 M$ d'achats intégrés en 2025, hors pub [ESTIMATION AppMagic via maf.ad] | GP 4,19 sur 793 k avis [ESTIMATION AppGoblin, n.c.] ; Android 4,3 sur ≈ 886 k [ESTIMATION Marlvel] | D15–D19 |
| Count Masters: Crowd Runner 3D | Freeplay · 2021 (m) · iOS, Android | GP : tranche 100 M+ [FAIT] ; 251,5 M [ESTIMATION agrégateur] | GP 4,6 sur 704 k avis [ESTIMATION agrégateur, n.c.] | D20–D22 |

Sans donnée : Real Survival: Not Fake (DevDop, fiche AppBrain seulement, D23) ; jeux web « comme la pub », dont celui d'Arcade Whale (« notre propre jeu, pas le Last War officiel ») et l'archive fakeadgames.com (D24–D25).

### 5.2 Expérience et monétisation

| Application | Critiques récurrentes | Visuel / fluidité | Répétitivité · progression | Monétisation |
|---|---|---|---|---|
| Real War: Not Fake | « Publicité lourde » ; « payer devient le seul moyen de progresser » ; presse favorable (« plutôt bon », Notebookcheck) | Unity 6 selon un site tiers ; rendu n.t. | Mode sans fin (score) et mode aventure (niveaux, pièces) ; classement annoncé | Pubs ; retirer les pubs coûte 10 000 pièces, pour 10 pièces par niveau, 100 par pub regardée, 200 pour rejoindre le Discord [FAIT] ; achats intégrés sur Android |
| Last War (mini-jeu) | Le shooter n'est qu'un mode annexe d'un jeu de stratégie 4X (ouverture, « Special Ops », « Frontline Breakthrough ») ; fidélité à la pub jugée « partielle » | n.t. | Mini-jeu servant de tutoriel puis d'activité secondaire | Achats intégrés ; plus de 50 % des impressions publicitaires du jeu montrent ce concept [ESTIMATION Sensor Tower, source secondaire] |
| Mob Control | Pubs forcées même après une victoire ; « No Ads » incomplet ; écran noir après une pub ; « pay-to-win », pic de difficulté | 3D stylisée (m) | Sentiment de progression payante | Pubs et achats ; ≈ 60 % pub, 40 % achats sur iOS [ESTIMATION AppGoblin] |
| Count Masters | Pubs fréquentes, bugs ; « lassant après une heure ou deux » ; saccades « quel que soit le téléphone » | Saccades signalées | Très répétitif | Interstitielles, « No ADS » 2,99 $ [FAIT] |

**Cas Real War: Not Fake.** Lancé par une vidéo YouTube de son auteur (« The Greatest Developer ») puis relayé par la presse, le jeu a atteint la tranche 500 k à 1 M+ sur Google Play en moins d'un an [FAIT]. Sa note Android reste médiocre, à cause des pubs et du coût de leur retrait. Son nom évoque « Last War » sans le citer. Un site tiers non affilié (realwarnotfake.com) capte le trafic de la marque : réserver son nom de domaine dès le départ.

### 5.3 Synthèse

- **Saturation : moyenne** [HYPOTHÈSE]. Deux géants voisins (Mob Control, Count Masters) mais une offre autonome fidèle à la pub très mince : un titre notable, un dérivé et des jeux web. La recherche de clones sur les stores est incomplète (quota épuisé). FirstFun a lui-même fait campagne en 2024 sur l'idée d'un « vrai jeu tiré du faux jeu des pubs » [FAIT, D12].
- **Frustrations exploitables :** le jeu promis par la pub n'existe que comme mode annexe ; pubs lourdes et retrait hors de prix chez le seul concurrent direct ; progression payante chez les géants.
- **Angles réalistes :** le jeu de la pub, mais profond : portes à vrai choix risque contre gain, boss, améliorations de type roguelite, mode sans fin avec classement ; aucune interstitielle, pubs récompensées volontaires, achat unique bon marché ; version web gratuite pour l'acquisition ; journal de développement vidéo.
- **Technique :** 2D ou 3D simple. Contenu paramétrique (vagues, portes), donc génération procédurale facile. Le point dur est l'affichage de centaines d'unités à 60 images par seconde sur Android d'entrée de gamme [HYPOTHÈSE].
- **PI et stores :** risque élevé si le nom, l'icône, les mots-clés ou les visuels renvoient à « Last War » ; faible si l'habillage est original.

**Sources D :** D1 https://play.google.com/store/apps/details?id=com.devdop.realwar&hl=en_US · D2 https://www.appbrain.com/app/real-war-not-fake/com.devdop.realwar · D3 https://apps.apple.com/fr/app/real-war-not-fake/id6753818799?l=en-GB · D4 https://www.notebookcheck.net/Someone-actually-made-one-of-those-fake-ad-games-and-it-s-pretty-good.1188256.0.html · D5 https://imp.news/gaming/real-war-the-mobile-game-thats-exactly-what-the-ad-promised-82522/ · D6 https://appgoblin.info/apps/com.devdop.realwar · D7 https://realwarnotfake.com/reviews/real-war-not-fake-review · D8 https://technode.com/2025/10/30/funflys-last-war-tops-global-mobile-game-revenue-chart-in-september-with-180-million-in-earnings/ · D9 https://wnhub.io/news/analytics/item-45553 · D10 https://www.baijing.cn/article/51753 · D11 https://mmoculture.com/reviews/last-war-survival-game-review/ · D12 https://www.forbes.com/sites/callumbooth/2024/10/24/anthony-starrs-fake-mobile-game-adverts-explained/ · D13 https://en.wikipedia.org/wiki/Last_War:_Survival_Game · D14 https://www.codecrush.asia/blog/last-war-survival-anatomy-of-an-addictive-game · D15 https://appgoblin.info/apps/com.vincentb.MobControl/keywords · D16 https://maf.ad/en/blog/top-mobile-games-2025/ · D17 https://www.blog.udonis.co/mobile-marketing/mobile-games/mob-control · D18 https://apps.apple.com/gb/app/mob-control/id1562817072 · D19 https://marlvel.ai/api/llm/apps/games/mob-control · D20 https://apps.apple.com/us/app/count-masters-crowd-runner-3d/id1568245971?uo=4 · D21 https://grand-screen.com/games/count-masters-stickman-games/ · D22 https://justuseapp.com/en/app/1568245971/count-masters-crowd-runner-3d/reviews · D23 https://www.appbrain.com/app/real-survival-not-fake/com.devdop.realsurvivalnotfake · D24 https://arcadewhale.com/blog/games-that-are-actually-like-their-ads/ · D25 https://fakeadgames.com/archive/last-war-survival

## 6. Famille E — Autres mécaniques de pub

### 6.1 Chiffres

| Sous-famille · application | Éditeur · année · plateformes | Popularité | Note et avis (date de la donnée) | Src |
|---|---|---|---|---|
| E1 · Hero Tower War – Merge Puzzle | Gamee · 16/06/2021 [FAIT] · iOS, Android | GP n.t. | AS 3,9 [FAIT, n.c.] ; GameBrain 60 % sur 780 avis [ESTIMATION] | E1–E3 |
| E1 · Stick Hero: Mighty Tower Wars ; Stick War: Hero Tower Defense (Rocket Studio) | année n.t. · iOS, Android | n.t. | Stick War: Hero Tower Defense : AS 4,5 sur 9,9 k notes [FAIT, n.c.] | E4–E5 |
| E2 · Kingshot | Century Games · 2025 (m) · iOS, Android | 42 M téléchargements et 220 M$ [ESTIMATION AppMagic via Udonis, 2025] ; ≈ 811 M$ la première année [ESTIMATION compilée par Gameszoom, 2026] ; ≈ 1,7 M joueurs quotidiens en octobre 2025 [ESTIMATION Sensor Tower via Naavik] | n.t. | E6–E11 |
| E3 · Earn to Die Rogue | Not Doppler · 2024 (m) · iOS, Android | 10 M téléchargements en quatre mois [FAIT, Pocket Gamer] ; Android ≈ 15,8 à 17,6 M [ESTIMATION AppGoblin] | iOS 4,61 sur 11,9 k notes [ESTIMATION AppGoblin, n.c.] ; Android ≈ 100 k avis, note n.t. | E12–E16 |
| E4 · Weapon Evolution (Rollic ou Babil Studios selon la fiche) ; Evolution Gun (Cool Corp) ; Weapon Road (Fiber Games) ; Weapon Evolve (Mooncake Game) | années n.t. · iOS ; Android n.t. | n.t. | n.t. | E17–E20 |

### 6.2 Expérience et monétisation

| Application | Critiques récurrentes | Visuel / fluidité | Répétitivité · progression | Monétisation |
|---|---|---|---|---|
| Hero Tower War ; Stick Hero: Mighty Tower Wars | « Deux pubs par partie même après l'achat No Ads » ; « encore une pub après chaque manche » ; pour Stick Hero, « mécaniques pauvres et pubs agaçantes » (GameBrain) | n.t. | Puzzle d'ordre d'attaque et de fusion | Pubs, « Remove Ads » 2,99 $ [FAIT] |
| Kingshot | Avis : jeu de construction et d'alliances, pas la défense de tour en temps réel montrée ; l'éditeur répond que les pubs montrent « une petite partie du jeu » ; certaines pubs auraient emprunté des visuels à Thronefall (Udonis) | n.t. | Stratégie 4X | Achats intégrés ; dépense d'acquisition estimée entre 250 et 300 M$ [ESTIMATION Gamigion, par inférence] |
| Earn to Die Rogue | Conduite « minimale », éclipsée par les phases de tir à pied ; pic de difficulté | 2D soignée (m) | Structure roguelite | Aucune pub forcée, pubs récompensées facultatives, pas de jauge d'énergie [FAIT, critique] ; ≈ 83 % achats, 17 % pub sur Android [ESTIMATION AppGoblin] |
| Runners d'évolution | n.t. | n.t. | Portes qui font évoluer l'arme, de la lance au laser | Gratuit, achats ; retrait des pubs à 3,99 $ sur une fiche Weapon Evolution [FAIT] |

### 6.3 Synthèse par sous-famille

| Sous-famille | Saturation et acteurs | Frustrations exploitables | Angle solo | Technique | PI et stores |
|---|---|---|---|---|---|
| E1 Tour à étages | Forte, clones homonymes (Gamee, Rocket Studio, Icebear) ; qualité faible | Pubs après chaque manche, « No Ads » non respecté, mécanique pauvre | Vrai puzzle de calcul, niveaux générés et vérifiés par solveur | 2D, très accessible, procédural | Faible ; titres génériques, Apple 4.3 b |
| E2 Muraille et archers | Offre autonome non vérifiée (recherches refusées) ; demande publicitaire forte | Le jeu montré n'existe pas dans le produit | Défense de mur en parties de 5 à 10 minutes, choix d'améliorations à chaque vague | 2D, vagues procédurales, équilibrage exigeant | Moyen à élevé : « Kingshot », et visuels proches de Thronefall à proscrire |
| E3 Véhicule contre zombies | Moyenne ; leader de qualité | Peu : Earn to Die Rogue est sans pub forcée ; seule la conduite déçoit | Recentrer sur la conduite et l'amélioration visible du véhicule | 2D à physique de véhicule, tronçons générés | Moyen : marque « Earn to Die » |
| E4 Runner d'évolution | Forte, titres interchangeables | n.t. ; faible profondeur [HYPOTHÈSE] | Peu d'angle durable pour un solo | 3D simple, paramétrique | Faible à moyen ; Apple 4.3 b |

**Sources E :** E1 https://apps.apple.com/app/id1570840391 · E2 https://gamebrain.co/game/hero-tower-wars-merge-puzzle · E3 https://game-solver.com/?p=780201 · E4 https://gamebrain.co/game/stick-hero-mighty-tower-wars · E5 https://apps.apple.com/app/id1644023573 · E6 https://play.google.com/store/apps/details?id=com.run.tower.defense&hl=en_US · E7 https://apps.apple.com/us/app/kingshot/id6739554056 · E8 https://www.blog.udonis.co/mobile-marketing/mobile-games/kingshot · E9 https://gameszoom.com/kingshot/ · E10 https://naavik.co/digest/century-games-4x-portfolio-strategy/ · E11 https://www.gamigion.com/inside-kingshot-the-ua-funnel-every-game-needs/ · E12 https://appgoblin.info/apps/com.notdoppler.earntodierogue · E13 https://appgoblin.info/apps/1564024870 · E14 https://www.pocketgamer.com/earn-to-die-rogue/10-million-downloads · E15 https://mobigamescenter.substack.com/p/earn-to-die-rogue · E16 https://gamebrain.co/game/earn-to-die-rogue · E17 https://apps.apple.com/gh/app/id1598710684?l=en-gb · E18 https://apps.apple.com/nl/app/evolution-gun/id6473553882 · E19 https://apps.apple.com/app/id1673770685 · E20 https://apps.apple.com/de/app/weapon-evolve/id1632395600

## 7. Risques stores et PI pour un jeu inspiré d'une publicité

Ce qui suit n'est pas un avis juridique.

### 7.1 Règles des stores

| Règle | Texte clé | Conséquence pratique | Src |
|---|---|---|---|
| Apple 4.1 (a) Copycats | « Don't simply copy the latest popular app on the App Store, or make some minor changes to another app's name or UI and pass it off as your own. » | Reprendre une mécanique passe si le nom, l'interface et l'habillage sont originaux | S1 |
| Apple 4.1 (c) | « You cannot use another developer's icon, brand, or product name in your app's icon or name, without approval from the developer. » | « Last War », « Kingshot », « Hole.io » interdits dans le nom et l'icône | S1 |
| Apple, aide 4.1 | « Don't include references to popular app names or trademarked terms in any metadata field. » ; « Don't copy the look and feel of other apps. » ; à l'inverse, Apple encourage « a unique spin on an existing concept » | Interdit aussi dans le sous-titre, les mots-clés et la description | S2 |
| Apple 2.3.7 | « don't try to pack any of your metadata with trademarked terms, popular app names […] just to game the system » | « Real version of the Last War ad » est à exclure ; Apple peut modifier les mots-clés | S1 |
| Apple 4.3 (b) Spam | « Don't submit apps that are indistinguishable from what's already widely available. » | Risque principal pour A, B, E1 et E4 | S1 |
| Apple 5.2.1 | « Don't use protected third-party material such as trademarks, copyrighted works, or patented ideas in your app without permission » | Aucun visuel, personnage, musique ou extrait de la pub d'origine | S1 |
| Apple 2.3.1 (a) | Promouvoir une application de façon trompeuse, « whether within or outside of the App Store », est un motif de retrait | Un jeu « conforme à la pub » doit le rester dans ses propres pubs et captures | S1 |
| Google Play, usurpation d'identité | « Don't imply that your app is related to or authorised by someone when it isn't. » [FAIT, extrait] ; depuis 2022, éviter icônes, titres, descriptions ou éléments du jeu pouvant tromper sur le lien avec une autre application [FAIT, presse] | Même interdiction que chez Apple pour le titre et l'icône | S3, S4 |
| Google Play, propriété intellectuelle | Texte officiel non obtenu. [HYPOTHÈSE (m)] : interdiction des applications portant atteinte aux marques et droits d'auteur de tiers | À lire sur la page officielle | — |
| Google Play, spam et contenu répétitif | Texte officiel non obtenu. [HYPOTHÈSE (m)] : refus des applications offrant la même expérience que d'autres déjà présentes. Vu en extrait : les blocs de mots-clés répétitifs ou sans rapport enfreignent la règle sur les fiches [FAIT] | Pas de déclinaisons en série, pas de bourrage de mots-clés | S5 |

Les forums développeurs d'Apple montrent des refus effectifs au titre du 4.1 (c), avec le message : « The app's icon and name contain an icon, brand, or product name that belongs to the following developer » [FAIT, S6].

### 7.2 Ce qui est protégeable et ce qui ne l'est pas

| Élément | Statut | Src |
|---|---|---|
| Mécanique, règles, idée de jeu | Non protégé par le droit d'auteur. Le Copyright Office américain indique que le droit d'auteur ne protège ni l'idée d'un jeu, ni son nom ou titre, ni sa méthode de jeu [FAIT, cité par un cabinet] | S7 |
| Expression : graphismes, personnages, sons, textes, interface, agencement visuel | Protégé. Dans Tetris Holding c. Xio (New Jersey, 2012), le clone a été condamné pour avoir repris l'apparence, alors que règles et mécaniques restaient libres [FAIT] | S8, S9 |
| Nom, logo, icône | Protégeables par le droit des marques ; ce sont eux que visent Apple 4.1 (c) et la règle d'usurpation de Google | S1, S3 |
| Combinaison de fonctionnalités d'un jeu à succès | En France, la copie peut être sanctionnée sans droit d'auteur : Voodoo a obtenu 125 000 € et le retrait de Wood Shop (Rollic, Hero Games) pour concurrence déloyale et parasitisme. À l'inverse, Voodoo a perdu en appel contre Homa en décembre 2023, les jeux étant jugés assez différents [FAIT, presse] | S10–S13 |

### 7.3 Usage d'une marque tierce, en pratique

- **À exclure :** la marque dans le nom, le sous-titre, les mots-clés, l'icône ou les captures ; la reprise de l'acteur, des personnages ou du montage de la pub ; un nom de développeur ambigu.
- **Acceptable** [HYPOTHÈSE] : décrire le genre sans marque (« shooter à portails multiplicateurs ») ; un nom allusif mais distinct, comme « Real War: Not Fake ».
- **Hors stores** (vidéo, site) : citer la pub d'origine pour expliquer la démarche reste un risque de marque, à apprécier au cas par cas [HYPOTHÈSE].
- **Publicité trompeuse :** l'argument « honnête » engage aussi les propres pubs du projet (précédent Playrix devant l'ASA, section 2).
- **Données personnelles :** une régie publicitaire traite des identifiants et peut transférer des données hors de l'Union européenne ; consentement, information RGPD et validation du délégué à la protection des données sont à prévoir dès la conception [HYPOTHÈSE, hors résultats].

**Sources S :** S1 https://developer.apple.com/app-store/review/guidelines/ (lu directement) · S2 https://developer.apple.com/help/app-review/guideline-reference/4-1-prevent-copycat-rejection (lu directement) · S3 https://support.google.com/googleplay/android-developer/answer/9888374 (extrait seulement) · S4 https://androidauthority.com/google-play-store-copycat-apps-3191920 · S5 https://android-developers.googleblog.com/2020/10/developer-tips-and-guides-common-policy.html · S6 https://developer.apple.com/forums/thread/821107 · S7 https://fkks.com/news/how-courts-view-copyright-protection-for-video-games · S8 https://www.loeb.com/zh-hans/insights/publications/2012/06/tetris-holding-llc-v-xio-interactive-inc · S9 https://gamedeveloper.com/business/judge-rules-in-favor-of-tetris-company-in-cloning-dispute · S10 https://www.pocketgamer.biz/voodoo-wins-court-case-rollic/ · S11 https://gameworldobserver.com/2020/09/22/voodo-wins-court-case-studio-copying-mechanics-woodturning · S12 https://www.pocketgamer.biz/news/83238/homa-wins-craft-island-defence-in-court-case-with-voodoo-lasting-almost-three-years/ · S13 https://www.worldipreview.com/copyright/litigation-in-the-hyper-casual-game-market-21238

## 8. Où l'écart entre demande et qualité de l'offre est le plus grand

1. **Famille D.** Demande : le concept du shooter à portails représente plus de la moitié des impressions publicitaires d'un jeu estimé à 180 M$ d'achats intégrés sur le seul mois de septembre 2025 [ESTIMATION]. Offre : ce jeu ne le propose qu'en mode annexe, et le seul concurrent autonome identifié, œuvre d'un indépendant, a dépassé 500 000 installations malgré une note Google Play de 2,9 à 3,6 et un retrait des pubs à 10 000 pièces [FAIT]. La demande existe ; la qualité reste à prendre.
2. **Famille C.** Demande : Hole.io est estimé à environ 392 M d'installations et à plus de 10 M de téléchargements sur 30 jours, à une date non précisée [ESTIMATION]. Offre : note Android de 3,0, pubs en pleine partie, faux multijoueur ; chez Stone Miner, un « No Ads » payé puis non respecté et 40 niveaux. Le succès d'un jeu payant, court et sans pub (A Game About Digging A Hole : plus d'un million d'exemplaires, porté sur mobile à 4,99 $, puis cloné) montre qu'une offre de qualité trouve preneur [FAIT].

La famille A présente aussi un écart chiffré (Pull the Pin : ≈ 198 M d'installations pour 3,5 sur Android), mais la densité de clones y renchérit la différenciation [HYPOTHÈSE]. E2 reste à vérifier.

## 9. Tableau comparatif des familles

| Famille | Saturation | Frustration exploitable | Faisabilité solo | Risque PI |
|---|---|---|---|---|
| A Puzzle de sauvetage | Forte | Élevée : pub après chaque niveau, puzzles trop faciles | Bonne en 2D ; contenu fait main coûteux | Faible à moyen (Apple 4.3 b) |
| B Tuyaux, chemins, liquides | Forte, leaders bien notés | Moyenne : pubs longues, minuteurs | Très bonne : 2D et procédural | Moyen (« Flow » déposé) |
| C Machine évolutive, destruction | Moyenne à forte | Élevée : pubs incessantes, « No Ads » non respecté, contenu court | Moyenne : 2D possible, équilibrage lourd | Moyen (clones surveillés, parasitisme) |
| D Shooter à portails | Moyenne, offre fidèle mince | Très élevée : promesse non tenue, pubs lourdes | Bonne : procédural, attention aux performances | Élevé si allusion à la marque, faible sinon |
| E1 Tour à étages | Forte, qualité faible | Moyenne | Très bonne | Faible |
| E2 Muraille et archers | Non vérifiée | Élevée [HYPOTHÈSE] | Bonne | Moyen à élevé |
| E3 Véhicule contre zombies | Moyenne, leader solide | Faible | Moyenne | Moyen |
| E4 Runner d'évolution | Forte | Moyenne, faible profondeur | Très bonne | Faible à moyen |

## 10. Limites, sources inaccessibles, vérifications manuelles

**Sources inaccessibles :** fiches App Store et Google Play en direct ; Sensor Tower, AppMagic, Appfigures ; Reddit et YouTube ; règles Google Play sur support.google.com. Toutes les données de store viennent d'extraits ou d'agrégateurs tiers, souvent non datés.

**Lacunes connues :**
- Quatre recherches refusées : notes de Last War et de Kingshot, clones autonomes du shooter à portails et de la défense de muraille, version mobile de Thronefall, détail de Real Survival: Not Fake.
- Non couverts : Plumber et jeux de tuyaux, autres runners à foule, autres successeurs d'Earn to Die ; le « Hero Rescue » d'origine n'a pas pu être distingué de ses homonymes.
- À confirmer : années marquées (m), éditeur de Stone Miner, valeurs n.t., et la coïncidence des 72,9 M d'installations affichées à la fois pour Dig This! et Stone Grass.

**Vérifications manuelles à faire :**
1. Relever sur les deux stores, à date, la note, le nombre d'avis, la tranche d'installations et la dernière mise à jour de chaque titre, en priorité Real War: Not Fake, Last War, Kingshot, Hole.io, Pull the Pin et Stone Miner.
2. Lire les 50 derniers avis à une et deux étoiles de Real War: Not Fake, Hole.io et Stone Miner.
3. Clarifier l'éditeur de Real War: Not Fake (DevDop ou Heseri Games) et le statut de Real Survival: Not Fake.
4. Chercher dans les stores : « gate shooter », « not fake », « real ad game », « wall defense archer roguelike », « tower number puzzle ».
5. Lire les règles Google Play sur l'usurpation d'identité, la propriété intellectuelle et le spam.
6. Vérifier les dépôts de marque (INPI, EUIPO, USPTO) : Last War, Kingshot, Flow Free, Hole.io, Earn to Die, A Game About Digging A Hole.
7. Mesurer la demande actuelle, et non cumulée, par les téléchargements mensuels récents d'un outil d'estimation.
