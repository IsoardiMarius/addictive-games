# Dig For Real — prototype (Unity, iOS et Android)

Jeu mobile en portrait, jouable à un doigt : on dirige un engin de chantier qui s'enfonce dans le sol, on ramasse le minerai, on le vend, on améliore l'engin. Règles complètes : [`game-design/02-cahier-des-charges.md`](../game-design/02-cahier-des-charges.md).

## État réel du prototype

| Élément | État |
|---|---|
| Règles du jeu, terrain, progression, écrans, sauvegarde (C# pur, dossier `Assets/Game/Core`) | **Compilé et testé ici** : 10 familles de tests, environ 87 000 vérifications, parcours complet joué par un robot |
| Rendu de chaque écran | **Vu ici** sur captures produites par le banc d'essai (dossier `docs/captures`) |
| Couche Unity (`Assets/Game/Unity`) et scripts d'éditeur (`Assets/Editor`) | **Compilés ici** contre des bibliothèques de référence d'Unity 2021 ; **jamais lancés dans Unity** |
| Shader, lecture du doigt, sons et vibrations sur un vrai téléphone | **Non vérifiés** |
| APK Android, projet Xcode | **Non construits** (Unity et le SDK Android étaient inaccessibles dans l'environnement de développement) |

Autrement dit : le jeu fonctionne, mais sa première exécution dans Unity reste à faire. La liste « Premier lancement » ci-dessous décrit quoi regarder.

## Ouvrir et jouer dans Unity

1. Installer **Unity Hub**, puis **Unity 6** (6000.0 LTS ou plus récent) avec le module *Android Build Support* (et *iOS Build Support* sur Mac).
2. Dans Unity Hub : *Add* → choisir le dossier `game/`. Si la version exacte demandée n'est pas installée, choisir votre version d'Unity 6 et accepter la mise à niveau.
3. À la première ouverture, le projet se configure seul (portrait, scène `Assets/Scenes/Main.unity`, Android 64 bits). Sinon : menu **Dig For Real > 1. Configurer le projet**.
4. Dans la fenêtre *Game*, choisir une résolution en portrait (par exemple 1080 × 2400), puis appuyer sur **Lecture**. La souris remplace le doigt ; `Échap` remplace la touche retour.

Il n'y a rien à régler à la main : aucune scène à composer, aucun objet à relier. Le jeu se crée au lancement (`GameBootstrap`).

## Construire une version de test

| Cible | Depuis l'éditeur | Résultat |
|---|---|---|
| Android | **Dig For Real > 2. Construire l'APK Android de test** | `game/Builds/DigForReal-test.apk`, à copier sur le téléphone |
| iOS | **Dig For Real > 3. Générer le projet Xcode (iOS)** (Mac requis) | `game/Builds/iOS`, à ouvrir dans Xcode |

En ligne de commande (chemin de l'exécutable Unity à adapter) :

```bash
Unity -batchmode -quit -projectPath game -executeMethod Game.EditorTools.ProjectSetup.BuildAndroid -logFile -
```

L'identifiant d'application est provisoire (`com.example.digforreal`, dans `Assets/Editor/ProjectSetup.cs`) : à remplacer avant toute publication. Installer un APK sur son propre téléphone ne publie rien.

## Premier lancement : quoi vérifier

| Symptôme | Cause probable | Correction |
|---|---|---|
| Erreurs rouges dans la *Console* | Écart d'API entre versions d'Unity | Copier le message complet et le transmettre pour correction |
| Écran sombre, rien ne s'affiche | Shader ou police non trouvés (message dans la *Console*) | Vérifier `Assets/Game/Resources/Shaders/SpriteBatch.shader` et `Assets/Game/Resources/Fonts/GameFont.bytes` |
| Image à l'envers ou en miroir | Convention d'axes | `MeshBatcher.cs`, ligne `localScale` |
| Couleurs délavées ou trop sombres | Projet en espace colorimétrique linéaire | Relancer **1. Configurer le projet** (force *Gamma*) |
| Le doigt ou la souris ne fait rien | *Active Input Handling* réglé sur le nouveau système seul | *Project Settings > Player > Active Input Handling* : *Input Manager (Old)* ou *Both* |
| Pas de son | Lecture audio coupée dans l'éditeur, ou sons désactivés dans les réglages du jeu | Bouton *Mute Audio* de la fenêtre *Game* ; écran Réglages du jeu |
| Jeu lent sur un téléphone ancien | Nombre de rectangles dessinés | Voir « Performances » dans [`game-design/03-architecture-technique.md`](../game-design/03-architecture-technique.md) |

## Travailler sans Unity : le banc d'essai

Le dossier `Tools/` fait tourner le même code de jeu hors Unity. Il faut le SDK .NET 10 (et `ffmpeg` pour la vidéo).

```bash
bash game/Tools/verifier.sh
```

```bash
dotnet run --project game/Tools/Headless -c Release -- tour game/Tools/out/tour
```

```bash
dotnet run --project game/Tools/Headless -c Release -- balance 60 occasionnel
```

```bash
dotnet run --project game/Tools/Headless -c Release -- video game/Tools/out/video/demo.mp4
```

| Commande | Rôle |
|---|---|
| `verifier.sh` | Compile le cœur, lance les tests, compile la couche Unity et les scripts d'éditeur |
| `tour` | Joue le jeu de bout en bout et enregistre une capture de chaque écran |
| `balance` | Fait jouer des parcours complets à un robot et affiche la courbe de progression |
| `video` | Enregistre une séquence de jeu avec le son (base pour les vidéos de test) |
| `shots`, `sheet` | Captures d'une descente ; planche de toutes les images générées |

## Organisation du code

```
game/
├── Assets/
│   ├── Game/Core/         Le jeu, en C# pur (aucune référence à Unity)
│   │   ├── Engine/        Dessin de formes, police TrueType, atlas, liste de rectangles, synthèse des sons
│   │   └── Game/          Balance.cs (tous les réglages), World, Run, Fx, RunView, écrans, sauvegarde, textes
│   ├── Game/Unity/        Pont vers Unity : écran, doigt, affichage, sons, sauvegarde, vibrations
│   ├── Game/Resources/    Shader et police (licence OFL jointe)
│   └── Editor/            Configuration du projet et builds
├── Packages/, ProjectSettings/
├── Tools/                 Banc d'essai hors Unity, tests, vérifications (ignoré par Unity)
└── docs/captures/         Captures produites par le banc d'essai
```

Pour modifier l'équilibrage ou ajouter une couche : `Assets/Game/Core/Game/Balance.cs`, puis `balance` et `verifier.sh`.

## Données et licences

- Le jeu n'a aucun accès réseau et ne collecte rien. La progression est enregistrée sur l'appareil (`PlayerPrefs`).
- Police : Rubik ExtraBold, licence SIL Open Font License 1.1 (`Assets/Game/Resources/Fonts/GameFont-LICENCE-OFL.txt`).
- Images et sons : produits par le code du jeu, sans fichier tiers.
- Les paquets NuGet utilisés par `Tools/UnityCheck` et `Tools/EditorCheck` (bibliothèques de référence d'Unity) servent uniquement à vérifier la compilation ; ils ne sont ni copiés dans le dépôt ni livrés avec le jeu.
