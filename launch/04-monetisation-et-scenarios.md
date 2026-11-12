# Monétisation et scénarios — *Dig For Real* (nom provisoire)

> 12 novembre 2026.
>
> **Avertissement. Ce sont des scénarios, pas des prévisions. Aucun chiffre n'est validé tant que les tests n'ont pas eu lieu.** Le fichier 00 n'a pu sourcer aucun repère de marché : installations, rétention, eCPM et taux d'achat sont des hypothèses, à remplacer une à une par des mesures : `testing/` pour l'intérêt des pubs simulées et du Pass, le lancement limité pour la rétention.
>
> Étiquettes du fichier 00 : [FAIT], [NV] (non relu, à traiter comme une hypothèse), [HYPOTHÈSE]. Aucun SDK n'est intégré à ce jour.

## 1. Google AdMob ou Unity LevelPlay

| | Google AdMob | Unity LevelPlay |
|---|---|---|
| Nature | Régie, avec médiation et enchères [NV] | Médiation de Unity (réseaux Unity Ads et ironSource Ads) [NV] |
| Plugin Unity | Officiel, v11.7.0, Unity 2022.3 au minimum [FAIT] | Officiel [FAIT] |
| Consentement UE | Plateforme certifiée obligatoire [NV] ; UMP, gratuite, incluse dans le plugin [FAIT] | Reprend le consentement TCF : il faut une plateforme à côté [NV] |
| Conditions | Majorité, identité et adresse vérifiées, app-ads.txt [NV] | Compte Unity, app-ads.txt [NV] |
| Paiement | Seuil de 70 €, versement mensuel [NV] | Seuil de 100 USD, délai à vérifier [NV] |

- **Quand intégrer un SDK** : pas avant (1) l'avis de la DPO, (2) le feu vert du lancement limité (`01`, §2), (3) la confirmation du porteur de projet, consignée dans `docs/exception.md` (décision L11). D'ici là, les pubs restent simulées dans le prototype et absentes de la version 1.0.
- **Lequel d'abord : AdMob seul.** Deux emplacements suffisent, la fenêtre de consentement vient avec le plugin, et un seul SDK limite l'intégration, les contrats et les destinataires à présenter à la DPO [HYPOTHÈSE du fichier 00]. LevelPlay ajoute plusieurs réseaux, donc plusieurs destinataires et transferts.
- **LevelPlay ensuite, sous condition** : seulement si le volume approche celui du scénario favorable, environ 14 000 pubs vues par jour [HYPOTHÈSE]. Chaque SDK ajouté rouvre le dossier : DPO, exception, App Privacy, Data safety.

## 2. Emplacements

| Emplacement | Moment | Règle |
|---|---|---|
| Pub récompensée « Batterie de secours » | À la panne, une fois par descente | Relance avec une demi-batterie |
| Pub récompensée « Vente ×2 » | Écran de fin | Jamais pendant la descente |
| Interstitielle, bannière, pub à l'ouverture | — | Aucune |
| Pass Chantier | Boutique, et à côté de chaque offre de pub | Achat unique, restaurable [FAIT] ; donne les deux bonus sans pub, définitivement |
| Achats cosmétiques | Reportés | Non modélisés |

- Le bouton annonce la récompense et la pub ; rien ne se lance sans appui [NV]. Apple permet d'inciter à regarder une pub (3.2.2 x) [FAIT].
- Le Pass couvre tout : un pass partiel a valu à *Real War* des avis négatifs (`research/01`).

## 3. Hypothèses des trois scénarios

Les 12 mois commencent au lancement général : version 1.1 monétisée, distribution mondiale validée (`03`, §5).

| Hypothèse | Prudent | Central | Favorable | Statut |
|---|---|---|---|---|
| Installations par mois, constantes, sans achat d'audience | 300 | 3 000 | 30 000 | [HYPOTHÈSE], sans base |
| Répartition Europe de l'Ouest / États-Unis / reste du monde | 60 / 10 / 30 % | idem | idem | [HYPOTHÈSE] |
| Rétention J1 / J7 / J30 | 20 / 3 / 1 % | 25 / 6 / 2 % | 35 / 10 / 4 % | [HYPOTHÈSE] : bas de la médiane ; entre médiane et premier quartile ; seuils des éditeurs hypercasual (fichier 00, §5) |
| Fin de vie d'un joueur | J90 | idem | idem | [HYPOTHÈSE] : zéro au-delà |
| Sessions par joueur actif et par jour | 2,0 | 2,5 | 3,5 | [HYPOTHÈSE] |
| Pubs récompensées vues par session | 0,4 | 0,6 | 0,8 | [HYPOTHÈSE], nettes des refus de consentement et des pubs non servies |
| Pubs vues par joueur actif et par jour | 0,8 | 1,5 | 2,8 | Produit des deux lignes précédentes |
| eCPM d'une pub récompensée, en USD : États-Unis / Europe de l'Ouest / reste du monde | 10 / 4 / 0,5 | 12,5 / 5,5 / 1,125 | 15 / 7 / 1,75 | [HYPOTHÈSE] : bas, premier quart et milieu des fourchettes du fichier 00, §4.3 |
| Taux d'achat du Pass, en part des installations | 0,1 % | 0,3 % | 1,0 % | [HYPOTHÈSE], sans base |
| Prix du Pass, taxes comprises | 3,99 € | idem | idem | [HYPOTHÈSE], dans l'usage de 2 à 5 € |
| Taxes incluses dans le prix | 20 % | idem | idem | [HYPOTHÈSE] |
| Commission des stores | 30 % | 15 % | 15 % | 30 % : Apple hors UE sans Small Business Program [FAIT]. 15 % : Apple avec ce programme [FAIT], Google [HYPOTHÈSE, grille 2026 incertaine] |
| Conversion | 1 USD = 1 € | idem | idem | [HYPOTHÈSE] de simplification |

## 4. Formules

1. **Rétention à l'âge *a*** (en jours) : r(0) = 1 ; r(a) = J1 × a^(−α) pour a de 1 à 7, avec α = ln(J1 / J7) / ln 7 ; r(a) = J7 × (a / 7)^(−β) pour a de 8 à 90, avec β = ln(J7 / J30) / ln(30 / 7). Jours actifs par installation = somme des r(a).
2. **Jours-joueurs sur 360 jours** = (installations par mois / 30) × somme, pour a de 0 à 90, de r(a) × (360 − a).
3. **Revenu pub** = jours-joueurs × pubs vues par joueur actif et par jour × somme par zone de (part de la zone × eCPM) / 1 000.
4. **Revenu net du Pass** = installations × taux d'achat × prix / (1 + taxes) × (1 − commission).
5. **Résultat** = (revenu pub + revenu net du Pass) × (1 − cotisations) − coûts fixes.

## 5. Coûts

| Poste | Montant | Statut |
|---|---|---|
| **Fixes, sur 12 mois** | **139 €** | |
| Apple Developer Program | 99 USD par an | [FAIT] |
| Google Play | 25 USD, une fois | [FAIT] |
| Nom de domaine et page publique | 15 € (fourchette de 10 à 20 €) | [HYPOTHÈSE] |
| Unity Personal, AdMob, UMP | 0 € ; Unity gratuit jusqu'à 200 000 USD de revenus | [FAIT] pour Unity, [NV] pour le reste |
| Boîte postale, matériel, dépôt de marque | Non comptés : à chiffrer | [HYPOTHÈSE] |
| **Variables** | | |
| Commission des stores | 15 à 30 % des achats, déjà déduite | voir §3 |
| Cotisations sociales, micro-entreprise | 21,5 % des encaissements : 21,2 % (services BIC) + 0,3 % (formation, haut de la fourchette) | Taux [FAIT] ; qualification BIC et assiette [HYPOTHÈSE] |
| Impôt sur le revenu, cotisation foncière | Non comptés ; versement libératoire de 1,7 % possible sous condition [FAIT] | à vérifier |

Aucun achat d'audience, aucun serveur. Le temps de travail du porteur de projet n'est pas valorisé.

## 6. Résultats sur 12 mois

Calculs vérifiés par un script Python (formule recoupée par une simulation jour par jour) ; montants arrondis à l'euro.

| | Prudent | Central | Favorable |
|---|---|---|---|
| Installations | 3 600 | 36 000 | 360 000 |
| Jours actifs par installation | 2,26 | 3,27 | 5,16 |
| Jours-joueurs | 7 854 | 112 096 | 1 739 176 |
| Pubs vues | 6 283 | 168 144 | 4 869 694 |
| eCPM moyen pondéré (USD) | 3,55 | 4,89 | 6,23 |
| Revenu pub | 22 € | 822 € | 30 314 € |
| Achats du Pass | 4 | 108 | 3 600 |
| Revenu net du Pass | 8 € | 305 € | 10 175 € |
| **Revenu total** | **31 €** | **1 127 €** | **40 488 €** |
| Cotisations (21,5 %) | 7 € | 242 € | 8 705 € |
| Coûts fixes | 139 € | 139 € | 139 € |
| **Résultat avant impôt sur le revenu** | **−115 €** | **+746 €** | **+31 644 €** |
| Revenu mensuel en régime établi (dès le 4e mois) | 3 € | 97 € | 3 547 € |
| Revenu par installation | 0,009 € | 0,031 € | 0,112 € |

**Lecture**

- **Prudent** : le jeu coûte ses frais fixes. Le revenu pub (22 €) reste sous le seuil de paiement d'AdMob (70 € [NV]) : il n'est même pas versé.
- **Central** : une centaine d'euros par mois. Les frais sont couverts, pas le temps passé.
- **Favorable** : sous le plafond de la micro-entreprise (83 600 € [FAIT]) et sous le seuil Unity Personal ; franchise de TVA à vérifier [NV].
- **Seuil de rentabilité**, aux hypothèses centrales : environ 5 700 installations sur l'année, soit 470 par mois.
- **Acheter des installations serait à perte** dans les trois scénarios : 0,009 à 0,112 € de revenu par installation, contre un coût de 0,15 à 0,8 USD pour un jeu hypercasual en Europe de l'Ouest [HYPOTHÈSE].
- **Cohérence** : le revenu par joueur actif et par jour (0,004 à 0,023 €) reste sous la fourchette hypercasual du fichier 00 (0,03 à 0,10 USD [HYPOTHÈSE]), ce qui va avec l'absence d'interstitielles. La pub pèse 73 à 75 % du revenu.
- **Distribution limitée à l'Union européenne** (60 % des installations) : revenu de 20 €, 738 € et 26 557 € ; résultat de −123 €, +440 € et +20 709 €.

## 7. Sensibilité

Une hypothèse varie seule, de sa valeur prudente à sa valeur favorable ; les autres restent au niveau central (résultat de référence : +746 €).

| Hypothèse modifiée | Résultat, valeur prudente | Résultat, valeur favorable | Écart |
|---|---|---|---|
| **Installations par mois** (300 à 30 000) | −51 € | +8 708 € | 8 759 € |
| **Pubs vues par joueur actif et par jour** (0,8 à 2,8) | +445 € | +1 305 € | 860 € |
| Taux d'achat du Pass (0,1 à 1,0 %) | +586 € | +1 305 € | 719 € |
| Rétention | +553 € | +1 102 € | 549 € |
| eCPM | +569 € | +922 € | 353 € |
| Commission (30 à 15 %) | +703 € | +746 € | 42 € |

Les deux hypothèses qui pèsent le plus, croisées (résultat sur 12 mois) :

| Installations par mois | 0,8 pub par jour | 1,5 pub par jour | 2,8 pubs par jour |
|---|---|---|---|
| 300 | −81 € | −51 € | +5 € |
| 3 000 | +445 € | +746 € | +1 305 € |
| 30 000 | +5 698 € | +8 708 € | +14 299 € |

- **L'audience décide de tout** : son écart est dix fois celui de n'importe quel autre levier. C'est aussi l'hypothèse la moins étayée : elle dépend de la portée des vidéos du porteur de projet.
- **Les pubs vues par joueur actif** se mesurent dès les tests : le journal compte les pubs simulées proposées et acceptées — un plafond, selon `testing/03`. Le taux d'achat du Pass suit de près ; le questionnaire de `testing/02` sonde le prix accepté.

## 8. Limites du modèle

- Installations constantes : ni pic de lancement, ni déclin.
- Rétention et achat sont indépendants dans le modèle ; en réalité, ce sont les joueurs fidèles qui achètent.
- Les acheteurs du Pass ne voient plus de pubs : le revenu pub est un peu surestimé.
- L'adhésion Apple se renouvelle pendant la période si le lancement suit de plusieurs mois l'ouverture du compte : retrancher alors 99 USD.
- Cotisations calculées sur les montants encaissés ; assiette à faire confirmer (Urssaf, expert-comptable).
