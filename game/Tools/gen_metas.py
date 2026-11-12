#!/usr/bin/env python3
"""Crée les fichiers .meta manquants du dossier Assets (identifiants stables, dérivés du chemin).

Unity les complète à l'import. À relancer après l'ajout d'un fichier si Unity n'est pas ouvert :
    python3 game/Tools/gen_metas.py
"""
import hashlib
import os
import sys

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Assets")
TAIL = "  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n"
IMPORTERS = {
    ".cs": "MonoImporter:\n  externalObjects: {}\n  serializedVersion: 2\n  defaultReferences: []\n  executionOrder: 0\n"
           "  icon: {instanceID: 0}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n",
    ".asmdef": "AssemblyDefinitionImporter:\n" + TAIL,
    ".shader": "ShaderImporter:\n  externalObjects: {}\n  defaultTextures: []\n  nonModifiableTextures: []\n"
               "  preprocessorOverride: 0\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n",
    ".bytes": "TextScriptImporter:\n" + TAIL,
    ".txt": "TextScriptImporter:\n" + TAIL,
    ".md": "TextScriptImporter:\n" + TAIL,
    ".json": "TextScriptImporter:\n" + TAIL,
}


def guid(rel):
    return hashlib.md5(("digforreal:" + rel.replace(os.sep, "/")).encode()).hexdigest()


def main():
    created = 0
    for base, dirs, files in os.walk(ROOT):
        dirs.sort()
        for name in sorted(dirs + [f for f in files if not f.endswith(".meta")]):
            path = os.path.join(base, name)
            meta = path + ".meta"
            if os.path.exists(meta):
                continue
            rel = os.path.relpath(path, os.path.dirname(ROOT))
            head = "fileFormatVersion: 2\nguid: %s\n" % guid(rel)
            if os.path.isdir(path):
                body = "folderAsset: yes\nDefaultImporter:\n" + TAIL
            else:
                body = IMPORTERS.get(os.path.splitext(name)[1].lower(), "DefaultImporter:\n" + TAIL)
            with open(meta, "w", newline="\n") as f:
                f.write(head + body)
            created += 1
    print("%d fichier(s) .meta créé(s)" % created)
    return 0


if __name__ == "__main__":
    sys.exit(main())
