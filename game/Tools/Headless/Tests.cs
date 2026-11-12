using System;
using System.Collections.Generic;
using System.IO;
using Game.Core;
using Game.Core.Engine;

namespace Headless;

/// <summary>
/// Tests automatiques du cœur du jeu, sans Unity : dotnet run -c Release -- test
/// Code de sortie 0 si tout passe.
/// </summary>
public static class Tests
{
    private static int _failed, _passed;

    private static void Check(bool ok, string what)
    {
        if (ok) { _passed++; return; }
        _failed++;
        Console.WriteLine("  ÉCHEC : " + what);
    }

    private static void Section(string name, Action body)
    {
        int before = _failed;
        try { body(); }
        catch (Exception e) { _failed++; Console.WriteLine("  EXCEPTION : " + e); }
        Console.WriteLine((_failed == before ? "ok     " : "ÉCHEC  ") + name);
    }

    public static int RunAll()
    {
        Section("Simulation déterministe (même graine, même résultat)", Determinism);
        Section("Terrain : passage garanti, sceaux, portails, zones d'entrée", WorldInvariants);
        Section("Sauvegarde : aller-retour et données corrompues", SaveRoundTrip);
        Section("Barème des améliorations : coûts croissants, bornes", Costs);
        Section("Journal de test : format d'export", JournalFormat);
        Section("Police : tous les caractères des textes sont disponibles", FontCoverage);
        Section("Sons : synthèse sans saturation ni valeur invalide", Sounds);
        Section("Parcours complet de l'interface (premier lancement → achat → relance)", UiFlow);
        Section("Formats d'écran extrêmes et images irrégulières", Robustness);
        Section("Équilibrage : courbe de progression dans les bornes", BalanceCurve);
        Console.WriteLine($"{_passed} vérifications réussies, {_failed} en échec");
        return _failed == 0 ? 0 : 1;
    }

    private static Run Play(int layer, uint seed, int[] up, bool casual)
    {
        var run = new Run(layer, seed, up, layer == 0);
        var bot = new Bot(casual, seed);
        while (!run.Over && run.Time < 600f)
        {
            run.SteerX = bot.Decide(run, 1f / 60f);
            run.Step(1f / 60f);
            run.Events.Clear();
        }
        return run;
    }

    private static void Determinism()
    {
        int[] up = { 3, 1, 2, 0, 1, 1 };
        Run a = Play(2, 777, up, true), b = Play(2, 777, up, true), c = Play(2, 778, up, true);
        Check(a.Y == b.Y && a.Coins == b.Coins && a.Blocks == b.Blocks && a.Time == b.Time, "deux descentes identiques divergent");
        Check(a.Y != c.Y || a.Blocks != c.Blocks, "deux graines différentes donnent la même descente");
    }

    private static void WorldInvariants()
    {
        for (int layer = 0; layer < Balance.LayerCount; layer++)
            for (uint seed = 1; seed <= 6; seed++)
            {
                var w = new World(layer, seed, layer == 0 && seed == 1);
                w.EnsureRows(Balance.LayerRows(layer) + 10);
                int rows = Balance.LayerRows(layer);
                LayerDef d = Balance.Layers[layer];
                for (int r = 0; r < rows; r++)
                {
                    int best = 0, cur = 0;
                    for (int c = 0; c < World.Cols; c++)
                    {
                        if (w.Get(c, r) == CellType.Socle) cur = 0; else { cur++; best = Math.Max(best, cur); }
                        CellType t = w.Get(c, r);
                        if (r < 4) Check(t == CellType.Soft, $"couche {layer + 1} : la zone d'entrée contient {t}");
                        if (t != CellType.Empty && t != CellType.Gate && t != CellType.Seal && t != CellType.Lava)
                            Check(w.Hp[r][c] > 0f, $"couche {layer + 1} : bloc {t} sans résistance en ({c},{r})");
                    }
                    Check(best >= 7, $"couche {layer + 1}, graine {seed}, ligne {r} : moins de 7 colonnes libres de socle");
                }
                Check(w.Seals.Count >= 1 && w.Seals[0].Row == d.Length + Balance.SealApproachRows, $"couche {layer + 1} : sceau mal placé");
                for (int c = 0; c < World.Cols; c++)
                    Check(w.Get(c, w.Seals[0].Row) == CellType.Seal && w.Get(c, w.Seals[0].Row + 1) == CellType.Seal, $"couche {layer + 1} : sceau incomplet");
                int bands = 0;
                foreach (GateBand g in w.Gates) if (g.Row < rows) { bands++; Check(g.A != g.B, "portails identiques des deux côtés"); }
                Check(bands >= 1, $"couche {layer + 1} : aucun portail");
            }
        var tuto = new World(0, 5, true);
        Check(tuto.Gates[0].A == GateFx.PlusPod && tuto.Gates[0].B == GateFx.EnergyDown, "le premier portail du didacticiel n'est pas « +1 foret » contre un malus");
    }

    private static void SaveRoundTrip()
    {
        var p = new Progress { Coins = 123456789, MaxLayer = 7, SelectedLayer = 5, BestDepth = 812, RunCounter = 41, TutorialDone = true, Sound = false, Lang = "en" };
        p.Up[0] = 12; p.Up[3] = 2; p.J.Sessions = 9; p.J.Days.Add("2026-11-12"); p.J.Days.Add("2026-11-13"); p.J.Attempts[6] = 4; p.J.FirstTouchDelay = 2.5f;
        Progress q = Progress.Parse(p.Serialize());
        Check(q.Coins == p.Coins && q.MaxLayer == 7 && q.SelectedLayer == 5 && q.BestDepth == 812 && q.RunCounter == 41, "progression mal relue");
        Check(q.Up[0] == 12 && q.Up[3] == 2 && q.TutorialDone && !q.Sound && q.Lang == "en", "améliorations ou réglages mal relus");
        Check(q.J.Sessions == 9 && q.J.Days.Count == 2 && q.J.Attempts[6] == 4 && Math.Abs(q.J.FirstTouchDelay - 2.5f) < 0.01f, "journal mal relu");
        foreach (string bad in new[] { null, "", "n'importe quoi", "coins=abc\nup=x;y;z\nmaxLayer=999\n", "====\n\n=\ncoins=-50\nup=999;999" })
        {
            Progress r = Progress.Parse(bad);
            Check(r.Coins >= 0 && r.MaxLayer <= Balance.LayerCount && r.Up[0] <= Balance.MaxLevel(Upgrade.Drill), "une sauvegarde corrompue produit un état invalide");
        }
    }

    private static void Costs()
    {
        for (int u = 0; u < (int)Upgrade.Count; u++)
        {
            long prev = 0;
            for (int l = 0; l < Balance.MaxLevel((Upgrade)u); l++)
            {
                long c = Balance.Cost((Upgrade)u, l);
                Check(c > prev, $"coût non croissant : {(Upgrade)u} niveau {l}");
                prev = c;
            }
        }
        Check(Balance.Cost(Upgrade.Drill, 0) <= 40, "la première amélioration doit rester très abordable");
        Check(Balance.StartPods(Balance.MaxLevel(Upgrade.Chassis)) <= Balance.MaxPods, "trop de forets au départ");
        for (int i = 1; i < Balance.LayerCount; i++)
            Check(Balance.Layers[i].HpMul > Balance.Layers[i - 1].HpMul && Balance.Layers[i].OreValue > Balance.Layers[i - 1].OreValue, "progression des couches non croissante");
    }

    private static void JournalFormat()
    {
        var j = new Journal();
        j.Touch("2026-11-14"); j.Touch("2026-11-15"); j.Sessions = 2; j.SessionSeconds.Add(120); j.SessionSeconds.Add(300);
        string[] lines = j.Export("0.1.0").TrimEnd('\n').Split('\n');
        string[] keys =
        {
            "version_jeu", "premier_lancement", "jours_joues", "sessions", "duree_totale_s", "durees_sessions_s", "descentes_commencees",
            "descentes_terminees", "victoires", "pannes", "abandons", "meilleure_couche", "tentatives_par_couche", "delai_premier_contact_s",
            "delai_premiere_amelioration_s", "ameliorations_achetees", "portails_bonus", "portails_malus", "pubs_simulees_proposees",
            "pubs_simulees_acceptees", "ips_moyennes"
        };
        Check(lines[0] == "DFR-JOURNAL 1", "en-tête du journal");
        Check(lines.Length == keys.Length + 1, "nombre de lignes du journal");
        for (int i = 0; i < keys.Length && i + 1 < lines.Length; i++) Check(lines[i + 1].StartsWith(keys[i] + "="), "clé attendue : " + keys[i]);
        Check(lines[3] == "jours_joues=2026-11-14;2026-11-15", "liste des jours joués");
        Check(Array.IndexOf(lines, "tentatives_par_couche=0;0;0;0;0;0;0;0;0;0;0;0") >= 0, "tentatives par couche");
    }

    private static void FontCoverage()
    {
        var font = Program.LoadFont();
        var missing = new SortedSet<char>();
        string all = Loc.AllText() + "0123456789+-×%.,!?:·/ kMGT";
        foreach (char ch in all)
            if (ch != ' ' && ch != '{' && ch != '}' && (!font.Has(ch) || Art.Charset.IndexOf(ch) < 0)) missing.Add(ch);
        Check(missing.Count == 0, "caractères absents de la police ou du jeu de caractères : " + string.Join(" ", missing));
    }

    private static void Sounds()
    {
        var all = new List<float[]>();
        for (int i = 0; i < (int)Sfx.Count; i++) all.Add(SoundBank.Build((Sfx)i));
        all.Add(SoundBank.BuildDrillLoop()); all.Add(SoundBank.BuildMusicLoop());
        foreach (float[] d in all)
        {
            float peak = 0f; bool bad = false; double sum = 0;
            foreach (float v in d) { if (float.IsNaN(v) || float.IsInfinity(v)) bad = true; peak = Math.Max(peak, Math.Abs(v)); sum += v; }
            Check(!bad, "échantillon invalide dans un son");
            Check(peak > 0.05f && peak <= 1.0001f, "niveau de crête hors bornes : " + peak);
            Check(Math.Abs(sum / d.Length) < 0.05, "composante continue excessive");
        }
        // Raccord de la boucle : le saut entre la fin et le début ne doit pas dépasser les sauts internes du signal.
        float[] loop = SoundBank.BuildDrillLoop();
        float maxStep = 0f;
        for (int i = 1; i < loop.Length; i++) maxStep = Math.Max(maxStep, Math.Abs(loop[i] - loop[i - 1]));
        Check(Math.Abs(loop[0] - loop[loop.Length - 1]) <= maxStep * 1.2f, "la boucle du foret ne se raccorde pas");
    }

    private static void UiFlow()
    {
        var d = new AppDriver(null);
        d.Wait(0.5f);
        Check(d.App.Screen == GameScreen.Run, "le premier lancement doit ouvrir directement la partie");
        float y0 = d.App.Run.Y;
        d.Wait(1.5f);
        Check(d.App.Run.Y == y0 && d.App.Run.Energy == d.App.Run.EnergyMax, "rien ne doit bouger avant le premier toucher");
        d.PlayRun(true);
        Check(d.App.Screen == GameScreen.Results, "la descente doit se terminer sur l'écran de fin");
        Check(d.App.Run.Won, "la première descente doit être gagnée par un joueur occasionnel");
        Check(d.App.P.Coins > 0 && d.App.P.MaxLayer >= 1 && d.App.P.TutorialDone, "la victoire doit être enregistrée");
        d.Wait(2f);
        long before = d.App.P.Coins;
        Check(d.Tap(Loc.Get("res.double")), "bouton de vente doublée absent");
        d.Wait(3f);
        Check(d.App.P.Coins == before * 2, "la pub simulée doit doubler la vente");
        Check(d.Tap(Loc.Get("garage")), "bouton garage absent");
        d.Wait(0.5f);
        long cost = Balance.Cost(Upgrade.Drill, 0);
        long coins = d.App.P.Coins;
        Check(d.Tap(Fmt.Short(cost, true)), "achat du foret impossible");
        Check(d.App.P.Up[0] == 1 && d.App.P.Coins == coins - cost, "l'achat doit débiter et monter le niveau");
        Check(d.App.P.J.Upgrades == 1 && d.App.P.J.FirstUpgradeDelay > 0f, "le journal doit noter le premier achat");

        // Relance de l'application avec la même sauvegarde.
        var e = new AppDriver(null, d.Plat.Data);
        e.Wait(0.5f);
        Check(e.App.Screen == GameScreen.Home, "au second lancement, l'accueil doit s'afficher");
        Check(e.App.P.Up[0] == 1 && e.App.P.Coins == d.App.P.Coins && e.App.P.MaxLayer == d.App.P.MaxLayer, "la progression doit survivre à une relance");
        Check(e.App.P.J.Sessions == 2, "une relance compte une nouvelle session");
        Check(e.Tap("icon:Gear"), "bouton réglages absent");
        e.Wait(0.3f);
        Check(e.Tap("row:Copy") && e.Plat.Clipboard.StartsWith("DFR-JOURNAL 1"), "copie du journal de test");
        Check(e.Tap("row:Globe") && !Loc.French, "bascule de langue");
        e.Tap("row:Globe");
        Check(Loc.French, "retour au français");
        e.Tap("row:Reset"); e.Wait(0.1f); e.Tap("row:Reset"); e.Wait(0.2f);
        Check(e.App.P.Coins == 0 && e.App.P.Up[0] == 0 && e.App.P.MaxLayer == 0, "la remise à zéro doit effacer la progression");
        Check(e.App.P.J.Sessions == 2, "la remise à zéro conserve le journal de test");
    }

    private static void Robustness()
    {
        foreach (var (w, h) in new[] { (390, 844), (360, 640), (320, 480), (820, 1180), (1024, 768), (200, 900) })
        {
            var d = new AppDriver(null, "", w, h, 0f, 0f, 1.5f);
            d.Frame(0f); d.Frame(5f); d.Frame(1f / 240f);
            d.Wait(0.3f);
            d.PlayRun(true, null, false, 20f);
            Check(d.App.List.Count > 0, $"rien n'est dessiné en {w}×{h}");
        }
    }

    private static void BalanceCurve()
    {
        BalanceSim.Go(40, true, false, true);
        Check(BalanceSim.LastFinished == BalanceSim.LastPlayers, "tous les parcours simulés doivent atteindre la fin du jeu");
        Check(BalanceSim.LastFirstRunWins >= BalanceSim.LastPlayers * 0.95, "la première descente doit être gagnée presque à coup sûr");
        Check(BalanceSim.LastAvgRuns >= 20 && BalanceSim.LastAvgRuns <= 60, $"nombre de descentes pour finir hors bornes : {BalanceSim.LastAvgRuns:0.0}");
        Check(BalanceSim.LastAvgRunSeconds >= 25 && BalanceSim.LastAvgRunSeconds <= 90, $"durée moyenne d'une descente hors bornes : {BalanceSim.LastAvgRunSeconds:0.0} s");
        Check(BalanceSim.LastMaxRunsOnALayer <= 9, $"une couche demande trop de tentatives : {BalanceSim.LastMaxRunsOnALayer:0.0}");
    }
}
