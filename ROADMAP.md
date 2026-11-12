# Feuille de route

*Mise à jour : 12 novembre 2026.*

## Avancement par phase

| Phase | État | Ce qui est fait | Ce qui manque |
|---|---|---|---|
| 1. Recherche de marché | Faite, avec réserves | Étude Dan Lespect, 28 fiches de publicités | Sources directes non consultées (réseau bloqué) ; vidéo de référence non visionnée |
| 2. Étude de concurrence | Faite, avec réserves | Cinq familles, risques de propriété intellectuelle | Notes et installations à relever directement sur les stores |
| 3. Sélection du concept | Faite | Classement de six concepts, recommandation argumentée | Mesure réelle de l'intensité publicitaire du concept retenu |
| 4. Conception | Faite | Cahier des charges, identité visuelle, architecture | — |
| 5. Prototypage | **Partielle** | Jeu complet, testé hors Unity ; couche Unity compilée | **Premier lancement dans Unity, build Android, essai sur téléphone** |
| 6. Validation | Préparée | Protocole, questionnaires, seuils, script d'agrégation, simulation par robot | **Aucun test avec de vrais joueurs** |
| 7. Préparation au lancement | Préparée | Plan, fiches stores, conformité, scénarios économiques | Comptes développeur, décisions de publication, vérification des repères chiffrés |

## Prochaines tâches, dans l'ordre

### A. Débloquer (quelques minutes chacune)

1. **Donner à Claude l'accès en écriture au dépôt GitHub** : l'envoi (`git push`) a été refusé pendant la session. Connecter GitHub à Claude et installer l'application GitHub Claude sur le dépôt. D'ici là, le travail est livré sous forme d'archive.
2. **Faire valider la fiche d'intention** (`docs/fiche-intention.md`) par le PO ou le K-lab, et compléter les champs « à compléter » de `docs/exception.md`.

### B. Faire tourner le jeu pour de vrai (une demi-journée)

3. Installer Unity 6 et ouvrir `game/` (voir `game/README.md`).
4. Appuyer sur Lecture ; parcourir la liste « Premier lancement » ; transmettre toute erreur de la *Console*.
5. Construire l'APK de test et l'installer sur un téléphone Android.
6. Sur le téléphone : vérifier la fluidité, le son, les vibrations, la zone d'encoche, la reprise après mise en veille.
7. Écouter les sons et la musique : ils ont été conçus sans pouvoir être entendus. Signaler ce qui gêne.

### C. Valider le concept (deux à trois semaines)

8. Cinq séances observées, puis 15 à 20 testeurs pendant sept jours (`testing/01-protocole-tests-joueurs.md`).
9. Agréger les journaux de test (`testing/outils/agreger_journaux.py`) et remplir le tableau de `testing/03-mesures-et-seuils.md`.
10. Tester quatre à cinq vidéos de jeu réel (`testing/04-tests-video-et-plan-de-diffusion.md`). La commande `video` du banc d'essai fournit une première séquence.
11. Décider : continuer, corriger ou arrêter, selon les seuils fixés à l'avance.

### D. Améliorer le jeu (selon les retours)

| Priorité | Évolution | Raison |
|---|---|---|
| Haute | Rééquilibrage à partir des données réelles | Le robot ne remplace pas des joueurs |
| Haute | Vibrations sur iOS | Absentes du prototype |
| Moyenne | Mode Abysse (descente sans fin) et défi du jour | Rejouabilité après la fin du jeu |
| Moyenne | Collection de reliques, missions | Objectifs à long terme |
| Moyenne | Habillages de l'engin | Achats cosmétiques |
| Basse | Images dessinées par un graphiste, si une identité plus riche est souhaitée | Le rendu actuel est entièrement produit par le code |

### E. Préparer la publication (après validation seulement)

12. Vérifier la disponibilité du nom.
13. Saisir la DPO avant toute intégration de régie publicitaire ou d'outil de statistiques.
14. Suivre `launch/01-plan-de-lancement.md` : chaque dépense et chaque étape publique y figure comme une décision à prendre, à consigner dans `docs/exception.md`.

## Obstacles rencontrés pendant la session

| Obstacle | Effet | Contournement ou suite à donner |
|---|---|---|
| Réseau filtré : YouTube, stores, TikTok, Reddit, Meta Ad Library, fakeadgames.com, outils de veille | Recherche menée sur des extraits de résultats | Élargir l'accès réseau de l'environnement, ou vérifier à la main les points listés dans chaque document |
| Quota de 200 recherches web atteint | Plusieurs recherches non faites (listées dans chaque document) | Relancer une session de recherche ciblée |
| Unity impossible à installer ou à lancer | Aucune exécution dans le moteur | Jeu écrit en C# pur, testé hors Unity ; couche Unity vérifiée à la compilation |
| SDK Android inaccessible (`dl.google.com` bloqué) | Aucun APK construit | Build depuis Unity sur votre machine ; modèle de flux GitHub fourni, non testé |
| Envoi vers GitHub refusé (accès en écriture absent) | Dépôt distant non mis à jour | Archive fournie ; voir la tâche 1 |

## Points à relire avec un œil critique

- Les documents de `launch/` et de `testing/` ont été rédigés à partir du document de référence et du cahier des charges, sans nouvelle recherche : leurs chiffres de marché sont des repères, pas des faits.
- Le flux `.github/workflows/apk-android-unity.yml` n'a jamais été exécuté.
- L'identifiant d'application `com.example.digforreal` est provisoire.
