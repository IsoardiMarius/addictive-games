# Registre des exceptions au cadre de l'organisation

Ce fichier consigne tout écart aux règles du socle de l'organisation (interdictions, obligations et recommandations).
Une ligne par exception : la règle concernée, la demande, le risque expliqué, l'alternative proposée et la décision du builder.

| N° | Date | Règle concernée | Nature | Statut |
|----|------|-----------------|--------|--------|
| EX-001 | 2026-11-12 | Ne pas coder avant validation de la fiche d'intention | Recommandation | **Confirmée par le builder le 2026-11-12** — à régulariser (fiche d'intention à faire valider) |

---

## EX-001 — Prototype développé avant validation de la fiche d'intention

- **Date de la demande** : 12 novembre 2026.
- **Règle concernée** (recommandation) : « Pour une nouvelle application, évite de coder avant que sa fiche d'intention soit validée par le PO de l'équipe, ou par le K-lab pour un projet pédagogique. »
- **Demande du builder** : enchaîner dans la même session la recherche de marché, la conception et le développement d'un MVP jouable (dossier `game/`), sans attendre une validation préalable.
- **Risque expliqué au builder** : coder avant validation peut conduire à un prototype hors cadrage (périmètre, données traitées, hébergement, publication), qu'il faudrait ensuite reprendre ou abandonner.
- **Alternative conforme proposée** : terminer la recherche et la conception, soumettre la fiche d'intention (`docs/fiche-intention.md`) au PO ou au K-lab, puis coder après validation.
- **Décision du builder** : **confirmée le 12 novembre 2026** — après explication de la règle, du risque et de l'alternative, le builder a répondu « Oui, coder maintenant ». Le prototype est donc développé dans cette session.
- **Régularisation prévue** : la fiche d'intention est rédigée dans `docs/fiche-intention.md` pour être soumise au PO ou au K-lab ; toute suite du développement après le prototype devrait attendre sa validation.
- **Mesures de réduction du risque appliquées dans tous les cas** :
  - le prototype ne traite aucune donnée personnelle (sauvegarde locale de la progression uniquement) ;
  - aucun compte utilisateur, aucun serveur, aucune base de données ;
  - aucun SDK tiers (publicité, statistiques, achats) intégré à ce stade ;
  - aucun déploiement, aucune publication sur un store, aucun dépôt ou artifact rendu public ;
  - la fiche d'intention est rédigée en parallèle pour permettre une régularisation rapide.
- **À compléter par le builder** : nom du PO ou du référent K-lab compétent ; date de validation de la fiche d'intention.

---

## Points de vigilance à venir (aucune exception demandée à ce jour)

Ces étapes du projet s'écarteraient d'une règle si elles étaient engagées. Elles ne le sont pas : le builder a indiqué qu'aucune dépense, création de compte payant ou publication publique ne doit être faite sans son accord.

| Étape future | Règle concernée | Nature | Ce qu'il faudra faire avant |
|--------------|-----------------|--------|-----------------------------|
| Publier sur l'App Store / Google Play, ou héberger une version web de test | « Évite de déployer une application chez un hébergeur externe, et de rendre public un dépôt, un gist, un artifact ou un paquet » | Recommandation | Confirmation explicite du builder, puis nouvelle entrée dans ce registre |
| Intégrer un SDK de publicité ou de statistiques (AdMob, LevelPlay, Firebase…) | « Évite d'envoyer des données de l'organisation vers un service externe non approuvé » | Recommandation | Confirmation du builder + analyse RGPD (voir ci-dessous) |
| Traiter des données personnelles de joueurs ou de testeurs (identifiant publicitaire, adresse IP, e-mail de testeur…) | RGPD : collecte limitée au nécessaire, droits des personnes, durée de conservation, mentions d'information | **Obligation** | Prévoir ces éléments dès la conception (voir `testing/` et `launch/`) — aucune exception possible |
| Transférer des données hors de l'Union européenne (la plupart des régies publicitaires et outils de statistiques sont américains) | « Tout transfert de données hors de l'Union européenne passe d'abord par la DPO » | **Obligation** | Saisir la DPO avant toute intégration — aucune exception possible |
