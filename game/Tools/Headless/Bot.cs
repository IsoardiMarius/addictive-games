using System;
using System.Collections.Generic;
using System.Globalization;
using Game.Core;
using Game.Core.Engine;

namespace Headless;

/// <summary>
/// Robot joueur : dirige l'engin avec une heuristique simple (regarder quelques lignes plus bas,
/// éviter le dur, viser le minerai et les bons portails). Deux niveaux : « habile » et « occasionnel »
/// (réactions plus lentes, erreurs de visée). Sert à vérifier l'équilibrage, pas à remplacer des joueurs.
/// </summary>
public sealed class Bot
{
    private readonly bool _casual;
    private Rng _rng;
    private float _cooldown;
    private float _target;

    public Bot(bool casual, uint seed) { _casual = casual; _rng = new Rng(seed + 99u); _target = World.Cols * 0.5f; }

    public float Decide(Run run, float dt)
    {
        _cooldown -= dt;
        if (_cooldown > 0f) return _target;
        _cooldown = _casual ? _rng.Range(0.28f, 0.5f) : 0.12f;
        if (_casual && _rng.Chance(0.2f)) return _target; // inattention

        World w = run.W;
        float half = run.HalfWidth;
        float best = float.MaxValue, bestX = run.X;
        int y0 = (int)MathF.Floor(run.Y + 0.6f);
        for (float x = half; x <= World.Cols - half + 0.01f; x += 0.5f)
        {
            float cost = MathF.Abs(x - run.X) * 0.35f;
            int c0 = (int)MathF.Floor(x - half), c1 = (int)MathF.Floor(x + half - 0.01f);
            for (int r = y0; r < y0 + 8; r++)
            {
                float weight = 1f - (r - y0) * 0.07f;
                for (int c = c0; c <= c1; c++)
                {
                    CellType t = w.Get(c, r);
                    switch (t)
                    {
                        case CellType.Empty: break;
                        case CellType.Lava: cost += 6f * weight; break;
                        case CellType.Ore: cost += MathF.Min(w.MaxHp(c, r) / run.Power, 3f) * weight - 1.6f * weight; break;
                        case CellType.Crystal: cost -= (run.Energy < run.EnergyMax * 0.6f ? 3f : 1.2f) * weight; break;
                        case CellType.Tnt: cost -= 1.0f * weight; break;
                        case CellType.Chest: cost -= 2.5f * weight; break;
                        case CellType.Boulder: cost += 0.8f * weight; break;
                        case CellType.Gate:
                            {
                                int aux = w.Aux[r][c];
                                GateBand g = w.Gates[aux / 2];
                                GateFx fx = (aux & 1) == 0 ? g.A : g.B;
                                cost += (Balance.IsBonus(fx) ? -4f : 9f) * weight / Math.Max(1, c1 - c0 + 1);
                                break;
                            }
                        case CellType.Seal: break;
                        default:
                            cost += MathF.Min(w.Hp[Math.Min(r, w.Rows - 1)][c] / run.Power, 4f) * weight;
                            break;
                    }
                }
            }
            if (cost < best) { best = cost; bestX = x; }
        }
        _target = bestX + (_casual ? _rng.Range(-0.7f, 0.7f) : 0f);
        return _target;
    }
}

public static class BalanceSim
{
    private sealed class LayerStat { public int Runs, Wins; public double Time, Coins; public int Players; public double UpSum; }

    // Résumé de la dernière simulation (lu par les tests).
    public static int LastFinished, LastPlayers, LastFirstRunWins;
    public static double LastAvgRuns, LastAvgMinutes, LastAvgRunSeconds, LastMaxRunsOnALayer;

    /// <summary>Simule des parcours complets (descente, vente, achats) et affiche la courbe de progression.</summary>
    public static int Go(int players, bool casual, bool verbose, bool quiet = false)
    {
        var stats = new LayerStat[Balance.LayerCount];
        for (int i = 0; i < stats.Length; i++) stats[i] = new LayerStat();
        int finished = 0, firstRunWins = 0; double totalRuns = 0, totalTime = 0;
        double[] prio = { 1.0, 0.55, 0.8, 0.9, 0.35, 0.6 };

        for (int p = 0; p < players; p++)
        {
            int[] up = new int[(int)Upgrade.Count];
            long coins = 0; int layer = 0; int runs = 0; double time = 0;
            int runsOnLayer = 0; double timeOnLayer = 0, coinsOnLayer = 0;
            while (layer < Balance.LayerCount && runs < 400)
            {
                uint seed = (uint)(p * 7919 + runs * 104729 + 1);
                var run = new Run(layer, seed, up, runs == 0);
                var bot = new Bot(casual, seed);
                const float dt = 1f / 60f;
                while (!run.Over && run.Time < 600f)
                {
                    run.SteerX = bot.Decide(run, dt);
                    run.Step(dt);
                    run.Events.Clear();
                }
                runs++; runsOnLayer++; time += run.Time; timeOnLayer += run.Time;
                if (runs == 1 && run.Won) firstRunWins++;
                long sale = run.SaleValue; coins += sale; coinsOnLayer += sale;
                if (verbose && p == 0)
                    Console.WriteLine($"  run {runs,3} couche {layer + 1,2} : {run.Time,5:0.0}s prof {run.Y,6:0.0} gagné={run.Won} +{run.LayersCleared} blocs {run.Blocks,4} minerai {run.OreCells,3} vente {sale,8} | F{up[0]} M{up[1]} B{up[2]} C{up[3]} A{up[4]} R{up[5]} pods {run.Pods}");
                if (run.LayersCleared > 0)
                {
                    var st = stats[layer];
                    st.Runs += runsOnLayer; st.Time += timeOnLayer; st.Coins += coinsOnLayer; st.Players++; st.UpSum += up[0];
                    layer += run.LayersCleared; runsOnLayer = 0; timeOnLayer = 0; coinsOnLayer = 0;
                    // Les couches sautées dans la même descente comptent pour zéro tentative.
                }
                // Achats : meilleure priorité par pièce dépensée, tant qu'il reste de quoi payer.
                while (true)
                {
                    int bestU = -1; double bestScore = 0;
                    for (int u = 0; u < up.Length; u++)
                    {
                        if (up[u] >= Balance.MaxLevel((Upgrade)u)) continue;
                        long cost = Balance.Cost((Upgrade)u, up[u]);
                        if (cost > coins) continue;
                        double score = prio[u] / cost;
                        if (score > bestScore) { bestScore = score; bestU = u; }
                    }
                    if (bestU < 0) break;
                    coins -= Balance.Cost((Upgrade)bestU, up[bestU]);
                    up[bestU]++;
                }
            }
            if (layer >= Balance.LayerCount) finished++;
            totalRuns += runs; totalTime += time;
        }

        LastFinished = finished; LastPlayers = players; LastFirstRunWins = firstRunWins;
        LastAvgRuns = totalRuns / players; LastAvgMinutes = totalTime / players / 60.0; LastAvgRunSeconds = totalTime / Math.Max(1.0, totalRuns);
        LastMaxRunsOnALayer = 0;
        foreach (var st in stats) if (st.Players > 0) LastMaxRunsOnALayer = Math.Max(LastMaxRunsOnALayer, st.Runs / (double)st.Players);
        if (quiet) return 0;
        Console.WriteLine($"Robot {(casual ? "occasionnel" : "habile")}, {players} parcours simulés");
        Console.WriteLine("couche | descentes pour passer | durée moy. d'une descente | pièces par descente | niveau de foret au passage");
        for (int i = 0; i < stats.Length; i++)
        {
            var s = stats[i];
            if (s.Players == 0) { Console.WriteLine($"{i + 1,6} | (franchie dans la foulée ou jamais atteinte)"); continue; }
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0,6} | {1,21:0.0} | {2,24:0.0}s | {3,19:0} | {4,10:0.0}",
                i + 1, s.Runs / (double)s.Players, s.Time / Math.Max(1, s.Runs), s.Coins / Math.Max(1, s.Runs), s.UpSum / s.Players));
        }
        Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
            "jeu terminé : {0}/{1} parcours ; en moyenne {2:0} descentes et {3:0} minutes de jeu", finished, players, totalRuns / players, totalTime / players / 60.0));
        return 0;
    }
}
