#!/usr/bin/env bash
# Vérifie tout ce qui peut l'être SANS Unity (SDK .NET 10 requis) :
#   bash game/Tools/verifier.sh
set -euo pipefail
cd "$(dirname "$0")"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1

echo "1/4  Cœur du jeu : compilation en C# 9 / .NET Standard 2.1 (comme le fera Unity)"
dotnet build Core/Game.Core.csproj -c Release -v q --nologo | tail -3

echo "2/4  Tests automatiques (simulation, terrain, sauvegarde, interface, équilibrage)"
dotnet run --project Headless/Headless.csproj -c Release -- test

echo "3/4  Couche Unity : compilation contre les bibliothèques de référence (Android, puis éditeur / iOS)"
dotnet build UnityCheck/UnityCheck.csproj -c Release -v q --nologo --no-incremental | tail -3
dotnet build UnityCheck/UnityCheck.csproj -c Release -v q --nologo --no-incremental -p:Variant=editor | tail -3

echo "4/4  Scripts d'éditeur (configuration et build)"
dotnet build EditorCheck/EditorCheck.csproj -c Release -v q --nologo --no-incremental | tail -3

echo "Tout est vert. Rappel : seule l'ouverture dans Unity valide le rendu, le son et les builds."
