# Architecture technique

*12 novembre 2026.*

## 1. Choix de la technologie : Unity et C#

Décision du porteur de projet : un jeu mobile natif dès le départ, pas de prototype web.

| Critère | Unity + C# | Godot 4 | Flutter + Flame |
|---|---|---|---|
| Export iOS et Android | Oui | Oui | Oui |
| Plugins officiels de pubs | AdMob, LevelPlay, AppLovin MAX | AdMob communautaire, MAX officiel | AdMob officiel |
| Coût | Gratuit jusqu'à 200 000 USD de revenus (Unity Personal) | Gratuit (licence MIT) | Gratuit |
| Attendu par les éditeurs de jeux casual | Oui | Rarement | Non |
| Utilisé par Dan Lespect pour *Real War* | Oui | — | — |

Sources : `launch/00-reference-stores-monetisation.md` (tarifs et plugins), `research/01-etude-dan-lespect.md` (moteur de *Real War*).

**Contrainte rencontrée.** Unity ne pouvait être ni téléchargé ni lancé dans l'environnement de développement (réseau filtré, licence liée à un compte), et le SDK Android non plus. L'architecture ci-dessous en découle : tout ce qui peut être vérifié sans Unity l'a été.

## 2. Principe : le jeu ne dépend pas du moteur

```
┌────────────────────────── Game.Core (C# pur, aucune référence à Unity) ──────────────────────────┐
│  Balance · World · Run          règles, terrain, simulation déterministe                          │
│  Fx · RunView · écrans          effets, affichage, interface                                      │
│  Engine                         dessin des images, police, liste de rectangles, synthèse des sons │
│  Progress · Journal · Loc       sauvegarde, journal de test, textes français et anglais           │
└───────────────▲──────────────────────────────────────────────────────────▲────────────────────────┘
                │ IPlatform, DrawList, AudioBus                            │ mêmes interfaces
┌───────────────┴───────────────┐                          ┌───────────────┴────────────────────────┐
│ Game.Unity (4 fichiers)       │                          │ Tools/Headless (hors Unity)            │
│ écran, doigt, maillage, sons, │                          │ rendu logiciel, captures PNG, vidéo,   │
│ sauvegarde, vibrations        │                          │ robot joueur, tests automatiques       │
└───────────────────────────────┘                          └────────────────────────────────────────┘
```

À chaque image, la plateforme transmet au jeu le temps écoulé et la position du doigt. Le jeu répond par :

- une **liste de rectangles texturés** (`DrawList`), dans l'ordre d'affichage ;
- une **file de sons** à jouer (`AudioBus`).

Unity se contente de les exécuter : un maillage dynamique, une texture, un shader, donc **un seul appel de dessin par image**.

### Ce que ce choix apporte

| Avantage | Détail |
|---|---|
| Testable sans Unity | Les mêmes fichiers sont compilés en .NET Standard 2.1 et C# 9, puis joués par un robot |
| Visible sans Unity | Le banc d'essai dessine la même liste de rectangles et produit captures et vidéo |
| Aucun réglage manuel | Pas de scène à composer, pas de préfabriqué : rien ne peut être « mal relié » dans l'éditeur |
| Aucun asset tiers | Images et sons sont produits par le code ; seule la police est un fichier (licence OFL) |
| Net à toute résolution | L'atlas est généré au démarrage à la densité de l'écran (plafonnée à 3 pixels par point) |
| Portable | Changer de moteur reviendrait à réécrire les quatre fichiers de `Game.Unity` |

### Ce qu'il coûte

- On n'utilise pas les outils visuels d'Unity (scènes, interface, particules). Un graphiste ne peut pas retoucher l'écran dans l'éditeur : tout passe par le code.
- L'interface est dessinée par le jeu lui-même : pas de lecteur d'écran, pas de saisie de texte.
- Si une équipe artistique rejoint le projet, il faudra décider d'ouvrir l'affichage à des images importées (prévu : `Art.cs` est le seul endroit à modifier).

## 3. Fichiers clés

| Fichier | Rôle |
|---|---|
| `Core/Game/Balance.cs` | **Tous** les réglages : couches, résistances, coûts, portails, dangers |
| `Core/Game/World.cs` | Génération du terrain à partir d'une graine |
| `Core/Game/Run.cs` | Simulation d'une descente (pas fixe de 1/60 s, déterministe) |
| `Core/Game/GameApp.cs`, `GameApp.Screens.cs` | États, écrans, déroulé complet |
| `Core/Game/Art.cs` | Dessin de toutes les images |
| `Core/Engine/Sound.cs` | Synthèse de tous les sons et de la musique |
| `Unity/GameHost.cs` | Démarrage automatique, écran, doigt, boucle |
| `Unity/MeshBatcher.cs` | Envoi de la liste de rectangles à la carte graphique |
| `Editor/ProjectSetup.cs` | Configuration du projet et builds |

## 4. Ce qui est vérifié, et comment

| Vérification | Outil | Résultat au 12 novembre 2026 |
|---|---|---|
| Compilation du cœur comme dans Unity (C# 9, .NET Standard 2.1) | `Tools/Core` | Réussie |
| Tests automatiques | `Tools/Headless test` | 86 974 vérifications, 0 échec |
| Parcours complet de l'interface au doigt | `Tools/Headless tour` | Réussi, 33 captures |
| Compilation de la couche Unity (variantes Android et éditeur / iOS) | `Tools/UnityCheck` | Réussie contre Unity 2021.3 |
| Compilation des scripts d'éditeur | `Tools/EditorCheck` | Réussie contre Unity 2021.1, sauf trois lignes propres à Unity 2021.2 et plus récent |
| Exécution dans l'éditeur Unity | — | **Non faite** |
| Build Android et iOS | — | **Non faits** |
| Test sur téléphone | — | **Non fait** |

La compilation contre les bibliothèques de référence a déjà évité deux erreurs qui auraient bloqué l'ouverture du projet (un conflit de nom avec `UnityEngine.Screen`, une API absente hors mobile).

**Limite à garder en tête** : les bibliothèques de référence sont celles d'Unity 2021, pas d'Unity 6. Les API utilisées sont anciennes et stables, mais seule une ouverture dans Unity 6 tranchera.

## 5. Performances

| Mesure (banc d'essai, ordinateur de développement) | Valeur |
|---|---|
| Rectangles dessinés par image | 645 en moyenne, 1 064 au maximum |
| Temps de calcul du jeu par image (simulation et préparation du dessin) | 0,03 ms |
| Génération de l'atlas au démarrage (échelle 3) | 0,1 s |

Ces chiffres ne valent pas mesure sur téléphone : un appareil d'entrée de gamme est dix à trente fois plus lent, ce qui laisse encore de la marge. Le journal de test relève les images par seconde réelles.

Leviers si un téléphone peine : baisser le plafond de densité de l'atlas (`GameApp`), réduire le nombre de débris (`Fx.cs`), supprimer la seconde passe de lueurs (`RunView.cs`).

## 6. Points d'extension prévus

| Besoin futur | Où l'ajouter | Précaution |
|---|---|---|
| Vraies pubs récompensées | Nouvelle interface dans `IPlatform` ; remplacer `StartAd` dans `GameApp.Screens.cs` | Consentement, RGPD, saisine de la DPO avant toute intégration |
| Achats intégrés | Nouvelle interface dans `IPlatform` | Idem ; règles des stores |
| Statistiques d'usage | Remplacer ou compléter `Journal` | Idem ; préférer un outil hébergé dans l'Union européenne |
| Nouvelle couche | Une ligne dans `Balance.Layers`, ses textes dans `Loc.cs` | Relancer `balance` |
| Nouvelle langue | Une colonne dans `Loc.cs` | Vérifier les caractères (`Art.Charset`) |
| Vibrations sur iOS | Module natif appelé depuis `UnityPlatform.Haptic` | Mac requis |

Le jeu ne nécessite ni compte, ni serveur, ni base de données. Si une base devenait un jour nécessaire, le cadre de l'organisation impose PostgreSQL.
