#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Agrège les « journaux de test » du jeu Dig For Real (format DFR-JOURNAL 1).

Utilisation :
    python3 agreger_journaux.py DOSSIER [--csv FICHIER.csv] [--exclure-session-releve]

    DOSSIER                    dossier contenant un journal par fichier .txt
    --csv                      fichier CSV à écrire (par défaut : DOSSIER/resultats.csv)
    --exclure-session-releve   ne compte pas la dernière session de chaque journal,
                               celle que le testeur a ouverte pour copier son journal
                               (à utiliser pour la campagne à distance)

Nom de fichier conseillé : CODE_AAAA-MM-JJ.txt, par exemple T07_2026-12-13.txt
(code du testeur, puis date de réception du journal). Le code n'est jamais un
nom de personne.

Python 3.8 ou plus, bibliothèque standard uniquement, aucun accès réseau.
Un fichier mal formé ne fait pas planter le script : il est signalé, et les
autres fichiers sont traités normalement.

Les définitions des mesures sont décrites dans testing/03-mesures-et-seuils.md.
"""

import argparse
import csv
import math
import re
import statistics
import sys
from datetime import date, timedelta
from pathlib import Path

# --- Réglages -----------------------------------------------------------------

# Ce que le format DFR-JOURNAL 1 implique, d'après le code du jeu au 12 novembre 2026
# (game/Assets/Game/Core/Game/GameApp.cs et Progress.cs) :
#   - ouvrir le jeu, même pour copier le journal, ajoute le jour courant à jours_joues :
#     le dernier jour de la liste est le jour de la copie, jamais compté comme un retour ;
#   - cette ouverture ajoute aussi une session (voir --exclure-session-releve) ;
#   - meilleure_couche est le nombre de couches franchies, de 0 à 12 ;
#   - un délai de -1 signale un événement qui n'a pas eu lieu ; ips_moyennes=0.0, aucune mesure.
# Si le jeu change l'un de ces points, il doit aussi changer le numéro de format.
VERSION_FORMAT = "1"
TAILLE_MAX_OCTETS = 200_000   # un journal pèse moins de 1 Ko
NB_COUCHES = 12
NB_SESSIONS_GARDEES = 20      # durees_sessions_s garde les 20 dernières sessions
SEUIL_DECROCHAGE_S = 180      # [HYPOTHÈSE] décrochage précoce : moins de 3 minutes de jeu en tout
SEUIL_IPS_BAS = 30            # [HYPOTHÈSE] en dessous, le testeur juge la lenteur, pas le jeu
PART_MALUS_AU_HASARD = 0.15   # cahier des charges §5.3 : un malus dans 3 paires de portails sur 10
Z_95 = 1.96                   # intervalle de confiance à 95 %

# Clés du format DFR-JOURNAL 1 et type attendu de leur valeur.
CLES = {
    "version_jeu": "texte",
    "premier_lancement": "date",
    "jours_joues": "dates",
    "sessions": "entier",
    "duree_totale_s": "decimal",
    "durees_sessions_s": "decimaux",
    "descentes_commencees": "entier",
    "descentes_terminees": "entier",
    "victoires": "entier",
    "pannes": "entier",
    "abandons": "entier",
    "meilleure_couche": "entier",
    "tentatives_par_couche": "entiers",
    "delai_premier_contact_s": "delai",
    "delai_premiere_amelioration_s": "delai",
    "ameliorations_achetees": "entier",
    "portails_bonus": "entier",
    "portails_malus": "entier",
    "pubs_simulees_proposees": "entier",
    "pubs_simulees_acceptees": "entier",
    "ips_moyennes": "decimal",
}

RE_ENTETE = re.compile(r"^DFR-JOURNAL\s+(\S+)$", re.IGNORECASE)
RE_ENTIER = re.compile(r"^\d+$")
RE_DECIMAL = re.compile(r"^-?\d+(?:[.,]\d+)?$")
RE_DATE = re.compile(r"^\d{4}-\d{2}-\d{2}$")
RE_NOM_FICHIER = re.compile(r"^(?P<code>.+?)[_-](?P<date>\d{4}-\d{2}-\d{2})$")
RE_FICTIF = re.compile(r"fictif|fictive", re.IGNORECASE)


class JournalInvalide(Exception):
    """Le fichier n'est pas un journal exploitable."""


# --- Lecture d'un journal -----------------------------------------------------

def vers_entier(valeur):
    if not RE_ENTIER.match(valeur):
        raise ValueError(valeur)
    return int(valeur)


def vers_decimal(valeur, negatif_permis=False):
    # Le jeu écrit un point décimal ; la virgule est tolérée (« 2,4 ») au cas où
    # un journal aurait été retapé à la main.
    if not RE_DECIMAL.match(valeur):
        raise ValueError(valeur)
    nombre = float(valeur.replace(",", "."))
    if nombre < 0 and not negatif_permis:
        raise ValueError(valeur)
    return nombre


def vers_delai(valeur):
    """Valeur négative (le jeu écrit -1) ou vide : l'événement n'a pas eu lieu."""
    if valeur == "":
        return None
    nombre = vers_decimal(valeur, negatif_permis=True)
    return None if nombre < 0 else nombre


def vers_date(valeur):
    if not RE_DATE.match(valeur):
        raise ValueError(valeur)
    return date.fromisoformat(valeur)  # ValueError si la date n'existe pas


def vers_liste(conversion):
    def convertir(valeur):
        return [conversion(e.strip()) for e in valeur.split(";") if e.strip()]
    return convertir


CONVERSIONS = {
    "texte": lambda valeur: valeur,
    "entier": vers_entier,
    "decimal": vers_decimal,
    "delai": vers_delai,
    "date": vers_date,
    "dates": vers_liste(vers_date),
    "entiers": vers_liste(vers_entier),
    "decimaux": vers_liste(vers_decimal),
}


def lire_texte(chemin):
    """Lit un fichier texte, quel que soit son encodage courant."""
    if not chemin.is_file():
        raise JournalInvalide("n'est pas un fichier")
    taille = chemin.stat().st_size
    if taille > TAILLE_MAX_OCTETS:
        raise JournalInvalide("trop volumineux pour être un journal (%d octets)" % taille)
    brut = chemin.read_bytes()
    if brut[:2] in (b"\xff\xfe", b"\xfe\xff"):
        return brut.decode("utf-16", errors="replace")
    return brut.decode("utf-8-sig", errors="replace")


def analyser_journal(texte):
    """Retourne (champs, avertissements). Lève JournalInvalide si ce n'est pas un journal.

    Le texte qui entoure le journal (formule de politesse, signature) est ignoré.
    Une clé absente ou une valeur illisible n'invalide pas le journal : les
    mesures qui en dépendent sont simplement « n/d ».
    """
    # Caractères invisibles (marque d'ordre d'octets, espace sans chasse) retirés.
    texte = texte.replace("﻿", "").replace("​", "")
    lignes = [ligne.strip() for ligne in texte.splitlines()]
    lignes = [ligne for ligne in lignes if ligne]
    if not lignes:
        raise JournalInvalide("fichier vide")

    debut, version = None, None
    for indice, ligne in enumerate(lignes):
        entete = RE_ENTETE.match(ligne)
        if entete:
            debut, version = indice, entete.group(1)
            break
    if debut is None:
        raise JournalInvalide("en-tête « DFR-JOURNAL 1 » introuvable")
    if version != VERSION_FORMAT:
        raise JournalInvalide(
            "version de format « %s » non prise en charge (attendue : %s)" % (version, VERSION_FORMAT))

    champs, avertissements = {}, []
    hors_journal = debut
    inconnues, doublons, illisibles = [], [], {}
    for ligne in lignes[debut + 1:]:
        if RE_ENTETE.match(ligne):
            avertissements.append("plusieurs journaux dans le fichier : seul le premier est lu")
            break
        cle, signe, valeur = (partie.strip() for partie in ligne.partition("="))
        if not signe or not cle:
            hors_journal += 1
        elif cle not in CLES:
            inconnues.append(cle[:40])
        elif cle in champs or cle in illisibles:
            doublons.append(cle)
        else:
            try:
                champs[cle] = CONVERSIONS[CLES[cle]](valeur)
            except ValueError:
                illisibles[cle] = valeur[:30]

    if not champs:
        raise JournalInvalide("aucune donnée lisible après l'en-tête")

    absentes = [cle for cle in CLES if cle not in champs and cle not in illisibles]
    if hors_journal:
        avertissements.append("%d ligne(s) hors journal ignorée(s)" % hors_journal)
    if inconnues:
        avertissements.append("clé(s) inconnue(s) ignorée(s) : " + ", ".join(inconnues))
    if doublons:
        avertissements.append("clé(s) en double, première valeur gardée : " + ", ".join(doublons))
    if illisibles:
        avertissements.append("valeur(s) illisible(s) : " + ", ".join(
            "%s=« %s »" % (cle, valeur) for cle, valeur in illisibles.items()))
    if absentes:
        avertissements.append("clé(s) absente(s) : " + ", ".join(absentes))
    avertissements.extend(verifier_coherence(champs))
    return champs, avertissements


def verifier_coherence(c):
    """Contrôles croisés. Ils avertissent, ils ne rejettent jamais un journal."""
    alertes = []
    premier, jours = c.get("premier_lancement"), c.get("jours_joues")
    if premier and jours:
        if min(jours) < premier:
            alertes.append("un jour joué précède premier_lancement")
        if premier not in jours:
            alertes.append("premier_lancement absent de jours_joues")
    durees, sessions = c.get("durees_sessions_s"), c.get("sessions")
    if durees is not None:
        limite = NB_SESSIONS_GARDEES if sessions is None else min(sessions, NB_SESSIONS_GARDEES)
        if len(durees) > limite:
            alertes.append("durees_sessions_s contient %d durées, %d attendues au plus"
                           % (len(durees), limite))
    # descentes_terminees compte les abandons dans le code du jeu, pas dans l'exemple
    # de format : les deux lectures sont admises.
    terminees, commencees = c.get("descentes_terminees"), c.get("descentes_commencees")
    victoires, pannes, abandons = c.get("victoires"), c.get("pannes"), c.get("abandons")
    if None not in (victoires, pannes, abandons, terminees):
        if terminees not in (victoires + pannes, victoires + pannes + abandons):
            alertes.append("descentes_terminees ne vaut ni victoires + pannes, "
                           "ni victoires + pannes + abandons")
    if None not in (terminees, commencees) and terminees > commencees:
        alertes.append("plus de descentes terminées que commencées")
    if None not in (victoires, pannes, abandons, commencees) and victoires + pannes + abandons > commencees:
        alertes.append("plus de victoires, pannes et abandons que de descentes commencées")
    proposees, acceptees = c.get("pubs_simulees_proposees"), c.get("pubs_simulees_acceptees")
    if None not in (proposees, acceptees) and acceptees > proposees:
        alertes.append("plus de pubs simulées acceptées que proposées")
    couche = c.get("meilleure_couche")
    if couche is not None and couche > NB_COUCHES:
        alertes.append("meilleure_couche inattendue : %d" % couche)
    tentatives = c.get("tentatives_par_couche")
    if tentatives is not None and len(tentatives) != NB_COUCHES:
        alertes.append("tentatives_par_couche contient %d valeurs au lieu de %d"
                       % (len(tentatives), NB_COUCHES))
    return alertes


def code_et_date(chemin):
    """T07_2026-12-13.txt -> ("T07", date(2026, 12, 13)) ; T07.txt -> ("T07", None)."""
    correspondance = RE_NOM_FICHIER.match(chemin.stem)
    if correspondance:
        try:
            return correspondance.group("code"), date.fromisoformat(correspondance.group("date"))
        except ValueError:
            pass
    return chemin.stem, None


# --- Mesures par testeur ------------------------------------------------------

def taux(numerateur, denominateur):
    """Rapport entre 0 et 1, ou None s'il n'a pas de sens."""
    if numerateur is None or denominateur is None or denominateur <= 0 or numerateur > denominateur:
        return None
    return numerateur / denominateur


def mesurer(code, c, exclure_session_releve=False):
    """Calcule les mesures d'un testeur à partir des champs lus dans son journal."""
    alertes = []
    premier = c.get("premier_lancement")
    jours = set(c.get("jours_joues") or [])
    jours_connus = premier is not None and bool(jours)
    # Ouvrir le jeu pour copier le journal ajoute ce jour à jours_joues : le dernier
    # jour de la liste est donc celui de la copie. On ne sait pas si le testeur a
    # joué ce jour-là : il ne compte jamais comme un retour.
    jour_copie = max(jours) if jours else None

    def retour(n):
        """« oui », « non », ou « ? » si le journal a été copié à J+n ou avant."""
        if not jours_connus:
            return "?"
        cible = premier + timedelta(days=n)
        if cible < jour_copie:
            return "oui" if cible in jours else "non"
        if cible == jour_copie:
            alertes.append("journal copié à J+%d : le retour à J+%d est indéterminé" % (n, n))
        return "?"

    retour_j1, retour_j7 = retour(1), retour(7)
    semaine_observee = jours_connus and jour_copie > premier + timedelta(days=7)
    jours_semaine = None
    if semaine_observee:
        jours_semaine = sum(1 for d in jours if premier <= d <= premier + timedelta(days=7))

    # Décrochage précoce : moins de SEUIL_DECROCHAGE_S de jeu en tout et aucun jour
    # joué de J+1 à J+7 (jour de la copie exclu). Ne dépend pas du nombre de sessions.
    duree_totale = c.get("duree_totale_s")
    decrochage = None
    if duree_totale is not None and jours_connus:
        revenu = any(premier < d <= premier + timedelta(days=7) and d != jour_copie for d in jours)
        if duree_totale >= SEUIL_DECROCHAGE_S or revenu:
            decrochage = "non"
        else:
            decrochage = "oui" if semaine_observee else "?"

    durees = c.get("durees_sessions_s")
    sessions = c.get("sessions")
    if exclure_session_releve:  # la dernière session est celle de la copie du journal
        if durees:
            durees = durees[:-1]
        if sessions:
            sessions -= 1

    commencees = c.get("descentes_commencees")
    victoires, pannes = c.get("victoires"), c.get("pannes")
    # Descentes menées à leur terme, par une victoire ou par une panne (abandons exclus).
    jugees = None if None in (victoires, pannes) else victoires + pannes
    achats = c.get("ameliorations_achetees")
    ips = c.get("ips_moyennes") or None  # 0 = aucune mesure de fluidité
    bonus, malus = c.get("portails_bonus"), c.get("portails_malus")
    total_portails = None if None in (bonus, malus) else bonus + malus
    proposees, acceptees = c.get("pubs_simulees_proposees"), c.get("pubs_simulees_acceptees")
    # « A accepté au moins une pub simulée » n'a de sens que si une pub a été proposée.
    a_accepte_pub = None
    if acceptees:
        a_accepte_pub = "oui"
    elif acceptees == 0 and proposees:
        a_accepte_pub = "non"

    return {
        "testeur": code,
        "jour_copie": jour_copie,
        "version_jeu": c.get("version_jeu"),
        "premier_lancement": premier,
        "retour_j1": retour_j1,
        "retour_j7": retour_j7,
        "jours_joues_j0_j7": jours_semaine,
        "sessions": sessions,
        "duree_totale_s": duree_totale,
        "duree_mediane_session_s": statistics.median(durees) if durees else None,
        "descentes_commencees": commencees,
        "descentes_jugees": jugees,
        "victoires": victoires,
        "abandons": c.get("abandons"),
        "descentes_par_session": (commencees / sessions
                                  if commencees is not None and sessions else None),
        "taux_victoire": taux(victoires, jugees),
        "taux_abandon_descente": taux(c.get("abandons"), commencees),
        "meilleure_couche": c.get("meilleure_couche"),
        "tentatives_par_couche": c.get("tentatives_par_couche"),
        "ameliorations_achetees": achats,
        "a_achete_amelioration": None if achats is None else ("oui" if achats >= 1 else "non"),
        "delai_premier_contact_s": c.get("delai_premier_contact_s"),
        "delai_premiere_amelioration_s": c.get("delai_premiere_amelioration_s"),
        "portails_malus": malus,
        "portails_total": total_portails,
        "part_portails_malus": taux(malus, total_portails),
        "pubs_simulees_proposees": proposees,
        "pubs_simulees_acceptees": acceptees,
        "taux_acceptation_pubs": taux(acceptees, proposees),
        "a_accepte_pub": a_accepte_pub,
        "ips_moyennes": ips,
        "decrochage_precoce": decrochage,
        "alertes": alertes,
    }


# --- Agrégation ---------------------------------------------------------------

def intervalle_wilson(k, n):
    """Intervalle de confiance à 95 % d'une proportion k/n (méthode de Wilson)."""
    p = k / n
    diviseur = 1 + Z_95 ** 2 / n
    centre = (p + Z_95 ** 2 / (2 * n)) / diviseur
    demi = Z_95 * math.sqrt(p * (1 - p) / n + Z_95 ** 2 / (4 * n * n)) / diviseur
    return max(0.0, centre - demi), min(1.0, centre + demi)


def compter(valeurs):
    """Liste de « oui » / « non » / autre -> (nombre de oui, nombre de réponses, indéterminés)."""
    oui = sum(1 for v in valeurs if v == "oui")
    reponses = oui + sum(1 for v in valeurs if v == "non")
    return oui, reponses, len(valeurs) - reponses


def mediane(valeurs):
    connues = [v for v in valeurs if v is not None]
    return statistics.median(connues) if connues else None


def regrouper(testeurs, cle_numerateur, cle_denominateur, borne=True):
    """Somme deux compteurs sur les testeurs où ils sont connus et cohérents.

    borne=True : le rapport est un taux, le numérateur ne peut pas dépasser le dénominateur.
    """
    numerateur = denominateur = 0
    for t in testeurs:
        a, b = t[cle_numerateur], t[cle_denominateur]
        if a is not None and b and (a <= b or not borne):
            numerateur += a
            denominateur += b
    return numerateur, denominateur


# --- Mise en forme ------------------------------------------------------------

def nb(x, decimales=1):
    """Nombre à la française : 5.857 -> « 5,9 » ; 300.0 -> « 300 » ; None -> « n/d »."""
    if x is None:
        return "n/d"
    texte = "%.*f" % (decimales, x)
    if "." in texte:
        texte = texte.rstrip("0").rstrip(".")
    return texte.replace(".", ",")


def pct(x):
    return "n/d" if x is None else "%d %%" % int(x * 100 + 0.5)


def avec_unite(x, unite):
    return "n/d" if x is None else "%s %s" % (nb(x), unite)


def tableau(lignes, marge="  "):
    """Aligne des lignes de cellules en colonnes de largeur fixe."""
    largeurs = [max(len(ligne[i]) for ligne in lignes) for i in range(len(lignes[0]))]
    return "\n".join(
        (marge + "  ".join(cellule.ljust(largeur) for cellule, largeur in zip(ligne, largeurs))).rstrip()
        for ligne in lignes)


def ligne_proportion(libelle, valeurs):
    oui, reponses, indetermines = compter(valeurs)
    if reponses == 0:
        valeur, detail = "n/d", ""
    else:
        bas, haut = intervalle_wilson(oui, reponses)
        valeur = "%d/%d = %s" % (oui, reponses, pct(oui / reponses))
        detail = "IC 95 %% : %s à %s" % (pct(bas), pct(haut))
    if indetermines:
        detail = (detail + " ; " if detail else "") + "%d indéterminé(s)" % indetermines
    return [libelle, valeur, detail]


def texte_regroupe(testeurs, cle_numerateur, cle_denominateur, est_un_taux=True):
    numerateur, denominateur = regrouper(testeurs, cle_numerateur, cle_denominateur, borne=est_un_taux)
    if denominateur == 0:
        return "regroupé : n/d"
    rapport = numerateur / denominateur
    return "regroupé : %d/%d = %s" % (
        numerateur, denominateur, pct(rapport) if est_un_taux else nb(rapport))


def afficher_par_testeur(testeurs):
    lignes = [["Testeur", "J+1", "J+7", "Jours", "Sess.", "Méd. sess.", "Desc./sess.",
               "Victoires", "Abandons", "Couches", "Amél.", "Pubs simulées"]]
    for t in testeurs:
        pubs = pct(t["taux_acceptation_pubs"])
        if t["taux_acceptation_pubs"] is not None:
            pubs += " (%d/%d)" % (t["pubs_simulees_acceptees"], t["pubs_simulees_proposees"])
        lignes.append([
            t["testeur"], t["retour_j1"], t["retour_j7"], nb(t["jours_joues_j0_j7"]),
            nb(t["sessions"]), avec_unite(t["duree_mediane_session_s"], "s"),
            nb(t["descentes_par_session"]), pct(t["taux_victoire"]),
            pct(t["taux_abandon_descente"]), nb(t["meilleure_couche"]),
            t["a_achete_amelioration"] or "n/d", pubs])
    print("PAR TESTEUR")
    print(tableau(lignes))
    print("  Légende : J+1, J+7 = a rejoué ce jour-là (« ? » = journal copié ce jour-là ou avant) ;")
    print("  Jours = jours joués de J0 à J+7 ; Sess. = sessions ; Méd. sess. = durée médiane d'une")
    print("  session ; Desc./sess. = descentes par session ; Victoires = victoires / (victoires +")
    print("  pannes) ; Abandons = descentes quittées en cours / descentes commencées ;")
    print("  Couches = couches franchies (meilleure_couche) ; Amél. = a acheté une amélioration ;")
    print("  n/d = non disponible.")


def afficher_global(testeurs):
    def colonne(cle):
        return [t[cle] for t in testeurs]

    medianes_session = [v for v in colonne("duree_mediane_session_s") if v is not None]
    etendue = ("de %s à %s s" % (nb(min(medianes_session)), nb(max(medianes_session)))
               if medianes_session else "")
    acheteurs = sum(1 for v in colonne("delai_premiere_amelioration_s") if v is not None)
    ips = [v for v in colonne("ips_moyennes") if v is not None]
    lents = sum(1 for v in ips if v < SEUIL_IPS_BAS)
    # Sans mode sans fin, un testeur qui a franchi les 12 couches n'a plus rien à jouer.
    jeu_fini = [None if v is None else ("oui" if v >= NB_COUCHES else "non")
                for v in colonne("meilleure_couche")]

    lignes = [
        ligne_proportion("Retour à J+1", colonne("retour_j1")),
        ligne_proportion("Retour à J+7", colonne("retour_j7")),
        ["Jours joués de J0 à J+7, médiane", nb(mediane(colonne("jours_joues_j0_j7"))),
         "sur 8 possibles"],
        ["Durée médiane de session, médiane des testeurs",
         avec_unite(mediane(colonne("duree_mediane_session_s")), "s"), etendue],
        ["Descentes par session, médiane des testeurs", nb(mediane(colonne("descentes_par_session"))),
         texte_regroupe(testeurs, "descentes_commencees", "sessions", est_un_taux=False)],
        ["Taux de victoire, médiane des testeurs", pct(mediane(colonne("taux_victoire"))),
         texte_regroupe(testeurs, "victoires", "descentes_jugees")],
        ["Descentes abandonnées, médiane des testeurs", pct(mediane(colonne("taux_abandon_descente"))),
         texte_regroupe(testeurs, "abandons", "descentes_commencees")],
        ligne_proportion("Décrochage précoce (moins de %d s de jeu, aucun retour)" % SEUIL_DECROCHAGE_S,
                         colonne("decrochage_precoce")),
        ligne_proportion("Testeurs ayant acheté une amélioration", colonne("a_achete_amelioration")),
        ["Délai avant la première amélioration, médiane",
         avec_unite(mediane(colonne("delai_premiere_amelioration_s")), "s"),
         "%d testeur(s) concerné(s)" % acheteurs],
        ["Délai avant le premier toucher, médiane",
         avec_unite(mediane(colonne("delai_premier_contact_s")), "s"), ""],
        ["Couches franchies (meilleure_couche), médiane", nb(mediane(colonne("meilleure_couche"))), ""],
        ligne_proportion("Testeurs ayant franchi les %d couches (jeu fini)" % NB_COUCHES, jeu_fini),
        ["Pubs simulées acceptées, médiane des testeurs", pct(mediane(colonne("taux_acceptation_pubs"))),
         texte_regroupe(testeurs, "pubs_simulees_acceptees", "pubs_simulees_proposees")],
        ligne_proportion("Testeurs ayant accepté au moins une pub simulée", colonne("a_accepte_pub")),
        ["Portails malus choisis, médiane des testeurs", pct(mediane(colonne("part_portails_malus"))),
         "%s ; au hasard : environ %s" % (
             texte_regroupe(testeurs, "portails_malus", "portails_total"), pct(PART_MALUS_AU_HASARD))],
        ["Images par seconde, minimum", nb(min(ips)) if ips else "n/d",
         "%d testeur(s) sous %d" % (lents, SEUIL_IPS_BAS) if ips else ""],
    ]
    print("GLOBAL (%d testeur(s) ; chaque testeur compte une fois)" % len(testeurs))
    print(tableau(lignes))
    print("  « regroupé » = tous les événements additionnés : chiffre dominé par les gros joueurs.")


def afficher_couches(testeurs):
    """Répartition de meilleure_couche et essais par couche (testeurs ayant tenté la couche)."""
    franchies = [t["meilleure_couche"] for t in testeurs if t["meilleure_couche"] is not None]
    essais = [t["tentatives_par_couche"] for t in testeurs if t["tentatives_par_couche"]]
    print("RÉPARTITION DE meilleure_couche (couches franchies) ET TENTATIVES PAR COUCHE")
    if not franchies and not essais:
        print("  n/d")
        return
    lignes = [["N", "Testeurs ayant franchi N couches", "Testeurs ayant tenté la couche N",
               "Tentatives sur la couche N : médiane (max.)"]]
    for n in sorted(set(range(0, NB_COUCHES + 1)) | set(franchies)):
        effectif = franchies.count(n)
        tentees = [e[n - 1] for e in essais if 0 < n <= len(e) and e[n - 1] > 0]
        lignes.append([
            str(n), ("%d  %s" % (effectif, "#" * effectif)).rstrip(),
            str(len(tentees)) if 1 <= n <= NB_COUCHES else "-",
            "%s (%s)" % (nb(statistics.median(tentees)), nb(max(tentees))) if tentees else "-"])
    print(tableau(lignes))


# --- CSV ----------------------------------------------------------------------

# (nom de colonne, clé de la mesure, nature) ; nature : texte, nombre, pourcentage, ouinon
COLONNES_CSV = [
    ("testeur", "testeur", "texte"),
    ("jour_copie", "jour_copie", "texte"),
    ("version_jeu", "version_jeu", "texte"),
    ("premier_lancement", "premier_lancement", "texte"),
    ("retour_j1", "retour_j1", "ouinon"),
    ("retour_j7", "retour_j7", "ouinon"),
    ("jours_joues_j0_j7", "jours_joues_j0_j7", "nombre"),
    ("sessions", "sessions", "nombre"),
    ("duree_totale_s", "duree_totale_s", "nombre"),
    ("duree_mediane_session_s", "duree_mediane_session_s", "nombre"),
    ("descentes_commencees", "descentes_commencees", "nombre"),
    ("descentes_par_session", "descentes_par_session", "nombre"),
    ("taux_victoire_pct", "taux_victoire", "pourcentage"),
    ("taux_abandon_descente_pct", "taux_abandon_descente", "pourcentage"),
    ("meilleure_couche", "meilleure_couche", "nombre"),
    ("a_achete_amelioration", "a_achete_amelioration", "ouinon"),
    ("ameliorations_achetees", "ameliorations_achetees", "nombre"),
    ("delai_premier_contact_s", "delai_premier_contact_s", "nombre"),
    ("delai_premiere_amelioration_s", "delai_premiere_amelioration_s", "nombre"),
    ("part_portails_malus_pct", "part_portails_malus", "pourcentage"),
    ("pubs_simulees_proposees", "pubs_simulees_proposees", "nombre"),
    ("pubs_simulees_acceptees", "pubs_simulees_acceptees", "nombre"),
    ("taux_acceptation_pubs_pct", "taux_acceptation_pubs", "pourcentage"),
    ("a_accepte_pub_simulee", "a_accepte_pub", "ouinon"),
    ("ips_moyennes", "ips_moyennes", "nombre"),
    ("decrochage_precoce", "decrochage_precoce", "ouinon"),
    ("nb_avertissements", "nb_avertissements", "nombre"),
]


def cellule_csv(valeur, nature):
    if valeur is None:
        return ""
    if nature == "nombre":
        return nb(valeur, 2)
    if nature == "pourcentage":
        return nb(valeur * 100, 1)
    return str(valeur)


def ecrire_csv(chemin, testeurs):
    """Une ligne par testeur, puis une ligne GLOBAL (médiane des testeurs, ou « oui/réponses »).

    Format français : séparateur point-virgule, virgule décimale, UTF-8 avec
    marque d'ordre d'octets, pour s'ouvrir directement dans un tableur.
    """
    ligne_globale = []
    for nom, cle, nature in COLONNES_CSV:
        valeurs = [t[cle] for t in testeurs]
        if nom == "testeur":
            ligne_globale.append("GLOBAL")
        elif nature == "ouinon":
            oui, reponses, _ = compter(valeurs)
            ligne_globale.append("%d/%d" % (oui, reponses))
        elif nature in ("nombre", "pourcentage") and nom != "nb_avertissements":
            ligne_globale.append(cellule_csv(mediane(valeurs), nature))
        else:
            ligne_globale.append("")
    with open(chemin, "w", encoding="utf-8-sig", newline="") as sortie:
        redacteur = csv.writer(sortie, delimiter=";", lineterminator="\n")
        redacteur.writerow([nom for nom, _, _ in COLONNES_CSV])
        for t in testeurs:
            redacteur.writerow([cellule_csv(t[cle], nature) for _, cle, nature in COLONNES_CSV])
        redacteur.writerow(ligne_globale)


# --- Programme principal ------------------------------------------------------

def charger(dossier):
    """Lit tous les .txt du dossier.

    Retourne (fichiers trouvés, journaux retenus, fichiers écartés avec leur raison, alertes de lot).
    """
    fichiers = sorted((p for p in dossier.iterdir() if p.suffix.lower() == ".txt"),
                      key=lambda p: p.name.lower())
    lus, ecartes, alertes_lot = [], [], []
    for chemin in fichiers:
        try:
            champs, avertissements = analyser_journal(lire_texte(chemin))
        except JournalInvalide as erreur:
            ecartes.append((chemin.name, str(erreur)))
            continue
        except OSError as erreur:
            ecartes.append((chemin.name, "lecture impossible (%s)" % (erreur.strerror or erreur)))
            continue
        except Exception as erreur:  # garde-fou : un fichier imprévu n'arrête jamais le lot
            ecartes.append((chemin.name, "erreur inattendue (%s)" % type(erreur).__name__))
            continue
        code, date_fichier = code_et_date(chemin)
        jours = champs.get("jours_joues")
        if date_fichier and jours and date_fichier < max(jours):
            avertissements.append("la date du nom de fichier précède le dernier jour du journal")
        lus.append({"fichier": chemin.name, "code": code, "date_fichier": date_fichier,
                    "champs": champs, "avertissements": avertissements})

    # Un testeur, un journal : le journal est cumulatif, on garde le plus récent.
    par_code = {}
    for journal in lus:
        par_code.setdefault(journal["code"], []).append(journal)
    retenus = []
    for code in sorted(par_code, key=str.lower):
        candidats = sorted(par_code[code], key=lambda j: (j["champs"].get("duree_totale_s") or 0,
                                                          j["date_fichier"] or date.min))
        retenus.append(candidats[-1])
        for ancien in candidats[:-1]:
            ecartes.append((ancien["fichier"], "journal plus ancien du testeur %s, remplacé par %s"
                            % (code, candidats[-1]["fichier"])))

    # Deux testeurs avec exactement le même journal : doublon probable.
    signatures = {}
    for journal in retenus:
        signature = repr(sorted(journal["champs"].items(), key=lambda paire: paire[0]))
        if signature in signatures:
            alertes_lot.append("%s et %s ont un contenu identique : doublon probable"
                               % (signatures[signature], journal["fichier"]))
        else:
            signatures[signature] = journal["fichier"]

    versions = sorted({j["champs"]["version_jeu"] for j in retenus if j["champs"].get("version_jeu")})
    if len(versions) > 1:
        alertes_lot.append("plusieurs versions du jeu dans le lot (%s) : ne pas les mélanger sans le dire"
                           % ", ".join(versions))
    fictifs = [bool(RE_FICTIF.search(j["fichier"])) for j in retenus]
    if any(fictifs) and not all(fictifs):
        alertes_lot.append("journaux fictifs et journaux réels mélangés : retirer les fichiers fictifs")
    return fichiers, retenus, ecartes, alertes_lot


def main(arguments=None):
    try:  # accents lisibles même si la console n'est pas en UTF-8
        sys.stdout.reconfigure(encoding="utf-8", errors="replace")
        sys.stderr.reconfigure(encoding="utf-8", errors="replace")
    except (AttributeError, ValueError):
        pass

    analyseur = argparse.ArgumentParser(
        description="Agrège les journaux de test de Dig For Real (format DFR-JOURNAL 1).")
    analyseur.add_argument("dossier", help="dossier contenant un journal par fichier .txt")
    analyseur.add_argument("--csv", help="fichier CSV à écrire (défaut : DOSSIER/resultats.csv)")
    analyseur.add_argument("--exclure-session-releve", action="store_true",
                           help="ne pas compter la dernière session de chaque journal, "
                                "ouverte pour copier le journal (campagne à distance)")
    options = analyseur.parse_args(arguments)

    dossier = Path(options.dossier)
    if not dossier.is_dir():
        print("Erreur : « %s » n'est pas un dossier." % dossier, file=sys.stderr)
        return 2
    try:
        fichiers, retenus, ecartes, alertes_lot = charger(dossier)
    except OSError as erreur:
        print("Erreur : dossier illisible (%s)." % (erreur.strerror or erreur), file=sys.stderr)
        return 2
    if not fichiers:
        print("Aucun fichier .txt dans « %s »." % dossier)
        return 1

    testeurs = []
    for journal in retenus:
        mesures = mesurer(journal["code"], journal["champs"], options.exclure_session_releve)
        mesures["alertes"] = journal["avertissements"] + mesures["alertes"]
        mesures["nb_avertissements"] = len(mesures["alertes"])
        testeurs.append(mesures)

    print("JOURNAUX DE TEST - %d fichier(s) .txt lu(s) dans %s : %d retenu(s), %d écarté(s)"
          % (len(fichiers), dossier, len(testeurs), len(ecartes)))
    if any(RE_FICTIF.search(journal["fichier"]) for journal in retenus):
        print("*** DONNÉES FICTIVES (d'après les noms de fichiers) : "
              "ces chiffres ne décrivent aucun joueur réel. ***")
    if options.exclure_session_releve:
        print("Session de relevé exclue : la dernière session de chaque journal n'est pas comptée.")
    print()

    if ecartes:
        print("FICHIERS ÉCARTÉS")
        for nom, raison in ecartes:
            print("  %s : %s" % (nom, raison))
        print()
    if not testeurs:
        print("Aucun journal exploitable : rien à calculer.")
        return 1

    afficher_par_testeur(testeurs)
    print()
    afficher_global(testeurs)
    print()
    afficher_couches(testeurs)
    print()

    avec_alertes = [t for t in testeurs if t["alertes"]]
    if avec_alertes or alertes_lot:
        print("AVERTISSEMENTS")
        for alerte in alertes_lot:
            print("  Lot : %s" % alerte)
        for t in avec_alertes:
            print("  %s :" % t["testeur"])
            for alerte in t["alertes"]:
                print("    - %s" % alerte)
        print()

    print("Rappel : avec %d testeur(s), ces chiffres sont des signaux directionnels, pas des taux"
          % len(testeurs))
    print("fiables. Lire les intervalles (IC 95 %) et testing/03-mesures-et-seuils.md, section 4.")

    chemin_csv = Path(options.csv) if options.csv else dossier / "resultats.csv"
    try:
        ecrire_csv(chemin_csv, testeurs)
    except OSError as erreur:
        print("Erreur : CSV non écrit dans « %s » (%s)." % (chemin_csv, erreur.strerror or erreur),
              file=sys.stderr)
        return 1
    print("CSV écrit : %s" % chemin_csv)
    return 0


if __name__ == "__main__":
    sys.exit(main())
