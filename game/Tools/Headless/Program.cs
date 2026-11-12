using System;
using System.IO;
using Game.Core.Engine;

namespace Headless;

/// <summary>
/// Banc d'essai hors Unity. Commandes :
///   fonts &lt;dossier-ttf&gt; &lt;sortie.png&gt;   compare des polices
/// </summary>
public static class Program
{
    public static int Main(string[] args)
    {
        string cmd = args.Length > 0 ? args[0] : "help";
        switch (cmd)
        {
            case "fonts": return Fonts(args[1], args[2]);
            case "balance": return BalanceSim.Go(args.Length > 1 ? int.Parse(args[1]) : 20, !(args.Length > 2 && args[2] == "habile"), args.Length > 3);
            case "shots": return Shots(args[1], args.Length > 2 ? int.Parse(args[2]) : 0, args.Length > 3 ? uint.Parse(args[3]) : 1u, args.Length > 4 ? args[4] : "");
            case "tour": return Tour(args[1]);
            case "test": return Tests.RunAll();
            case "sheet": return Sheet(args[1], args.Length > 2 ? float.Parse(args[2], System.Globalization.CultureInfo.InvariantCulture) : 3f);
            default:
                Console.WriteLine("Commandes : test | balance [parcours] [occasionnel|habile] [v] | tour <dossier> | shots <dossier> [couche] [graine] [niveaux] | sheet <png> [échelle] | fonts <dossier-ttf> <png>");
                return 1;
        }
    }

    public static string RepoGame()
    {
        string d = AppContext.BaseDirectory;
        while (d != null && !Directory.Exists(Path.Combine(d, "Assets", "Game"))) d = Path.GetDirectoryName(d);
        return d ?? throw new Exception("dossier game/ introuvable");
    }

    public static TrueTypeFont LoadFont()
    {
        return new TrueTypeFont(File.ReadAllBytes(Path.Combine(RepoGame(), "Assets", "Game", "Resources", "Fonts", "GameFont.bytes")));
    }

    /// <summary>Planche de contrôle : toutes les images générées, agrandies, avec leur nom.</summary>
    private static int Sheet(string outPath, float scale)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var font = LoadFont();
        var atlas = new Atlas(2048, 2048, scale);
        var art = new Game.Core.Art();
        art.Build(atlas, font);
        var list = new DrawList();
        var g = new Gfx(atlas, list);
        g.FontSmall = atlas.AddFont(font, 18, Game.Core.Art.Charset);
        g.FontLarge = atlas.AddFont(font, 44, Game.Core.Art.CharsetLarge);
        sw.Stop();
        Console.WriteLine($"atlas : {sw.ElapsedMilliseconds} ms, rempli à {atlas.FillRatio * 100:0.0} %, échelle {scale}");
        Png.SaveAtlas(Path.ChangeExtension(outPath, null) + "-atlas.png", atlas);

        const float zoom = 2.2f;
        float x = 8, y = 8, rowH = 0;
        int W = 390, H = 1500;
        var fb = new SoftRaster((int)(W * 2f), (int)(H * 2f), 2f);
        fb.Clear(0.10f, 0.11f, 0.22f);
        void Put(string name, Sprite s)
        {
            float w = Math.Max(s.W * zoom, 34), h = s.H * zoom;
            if (x + w > W - 8) { x = 8; y += rowH + 16; rowH = 0; }
            g.Image(s, x + (w - s.W * zoom) / 2, y, s.W * zoom, h, name.StartsWith("Soft") || name.StartsWith("Med") || name.StartsWith("Hard") ? Col.Hex(0xC98B5A) : Col.White);
            g.Text(name, x + w / 2, y + h + 9, 7.5f, Col.Hex(0x9aa4d8), Align.Center);
            x += w + 8; rowH = Math.Max(rowH, h);
        }
        foreach (var f in typeof(Game.Core.Art).GetFields())
        {
            if (f.FieldType == typeof(Sprite)) Put(f.Name, (Sprite)f.GetValue(art));
            else if (f.FieldType == typeof(Sprite[]))
            {
                var arr = (Sprite[])f.GetValue(art);
                for (int i = 0; i < arr.Length; i++)
                    Put(f.Name == "Icons" ? ((Game.Core.Icon)i).ToString() : f.Name + i, arr[i]);
            }
        }
        fb.Draw(list, atlas);
        fb.SavePng(outPath);
        Console.WriteLine("écrit : " + outPath);
        return 0;
    }

    /// <summary>Fait jouer une descente au robot et enregistre des captures à des instants choisis.</summary>
    private static int Shots(string outDir, int layer, uint seed, string upSpec)
    {
        const float scale = 2f, viewH = 844f;
        var font = LoadFont();
        var atlas = new Atlas(2048, 2048, scale);
        var art = new Game.Core.Art();
        art.Build(atlas, font);
        var list = new DrawList();
        var g = new Gfx(atlas, list);
        g.FontSmall = atlas.AddFont(font, 18, Game.Core.Art.Charset);
        g.FontLarge = atlas.AddFont(font, 44, Game.Core.Art.CharsetLarge);
        int[] up = new int[6];
        if (upSpec.Length > 0) { var parts = upSpec.Split(','); for (int i = 0; i < parts.Length && i < 6; i++) up[i] = int.Parse(parts[i]); }
        var run = new Game.Core.Run(layer, seed, up, layer == 0);
        var bot = new Bot(true, seed);
        var fx = new Game.Core.Fx();
        var view = new Game.Core.RunView();
        var audio = new AudioBus();
        var fb = new SoftRaster((int)(390 * scale), (int)(viewH * scale), scale);
        float time = 0f; int shot = 0; float nextShot = 0.6f;
        const float dt = 1f / 60f;
        string pending = null;
        while (!run.Over && time < 120f)
        {
            run.SteerX = bot.Decide(run, dt);
            if (fx.HitStop > 0f) fx.HitStop -= dt; else run.Step(dt);
            foreach (var e in run.Events)
            {
                fx.Handle(e, run, audio, _ => { });
                if (e.Type == Game.Core.Ev.Gate) pending = "portail";
                if (e.Type == Game.Core.Ev.Explosion) pending = "explosion";
                if (e.Type == Game.Core.Ev.SealBreak) pending = "sceau";
                if (e.Type == Game.Core.Ev.BoulderHit) pending = "rocher";
            }
            run.Events.Clear();
            fx.Continuous(run, audio, true, dt);
            audio.Shots.Clear();
            fx.Update(dt);
            view.Update(run, dt, viewH);
            time += dt;
            bool take = time >= nextShot;
            string tag = "t" + ((int)time).ToString("00");
            if (pending != null && shot < 40) { take = true; tag = pending; pending = null; nextShot = time + 0.12f; }
            if (run.OnSeal && time >= nextShot - 3.5f && shot < 40) { }
            if (take)
            {
                list.Clear(); g.ResetTransform();
                view.Draw(g, art, run, fx, viewH, time);
                view.DrawHud(g, art, run, fx, viewH, 44f, time, true, false);
                fb.Clear(0, 0, 0);
                fb.Draw(list, atlas);
                string path = Path.Combine(outDir, $"run-{shot:00}-{tag}.png");
                fb.SavePng(path);
                Console.WriteLine($"{path}  quads={list.QuadCount} y={run.Y:0.0} énergie={run.Energy:0.0} pods={run.Pods} pièces={run.Coins:0}");
                shot++;
                if (time >= nextShot) nextShot = time + (time < 8f ? 2.4f : 6f);
            }
        }
        Console.WriteLine($"fin : {time:0.0}s profondeur {run.Y:0.0} gagné={run.Won}");
        return 0;
    }

    private static string SaveText(int maxLayer, string up, long coins)
    {
        return $"v=1\ncoins={coins}\nup={up.Replace(',', ';')}\nmaxLayer={maxLayer}\nselLayer={maxLayer}\ntuto=1\nsound=1\nmusic=1\nhaptics=1\nshake=1\nlang=fr\nruns=7\n";
    }

    /// <summary>Visite guidée automatique : joue le jeu de bout en bout et enregistre chaque écran.</summary>
    private static int Tour(string outDir)
    {
        Directory.CreateDirectory(outDir);
        foreach (string f in Directory.GetFiles(outDir, "*.png")) File.Delete(f);
        var d = new AppDriver(outDir);
        d.Wait(0.7f);
        d.Shot("premier-lancement");

        // Première descente (couche 1, didacticiel).
        bool s1 = false, s2 = false, s3 = false, s4 = false; float gateT = -1f, clearT = -1f;
        d.PlayRun(true, (run, t) =>
        {
            if (t < 0f) return;
            if (!s1 && t > 1.2f) { s1 = true; d.Shot("descente"); }
            if (gateT < 0f && run.GatesGood + run.GatesBad > 0) gateT = t;
            if (!s2 && gateT >= 0f && t > gateT + 0.12f) { s2 = true; d.Shot("portail-franchi"); }
            if (!s3 && run.OnSeal) { s3 = true; d.Shot("sceau"); }
            if (clearT < 0f && run.LayersCleared > 0) clearT = t;
            if (!s4 && clearT >= 0f && t > clearT + 0.35f) { s4 = true; d.Shot("couche-franchie"); }
        });
        Console.WriteLine($"descente 1 : écran={d.App.Screen} couches franchies={d.App.Run.LayersCleared} pièces={d.App.P.Coins}");
        d.Wait(2.2f);
        d.Shot("fin-victoire");

        // Vente doublée par pub simulée.
        if (d.Tap(Game.Core.Loc.Get("res.double"))) { d.Wait(0.8f); d.Shot("pub-simulee"); d.Wait(2.0f); d.Shot("vente-doublee"); }

        // Garage : achats.
        d.Tap(Game.Core.Loc.Get("garage")); d.Wait(0.6f);
        d.Shot("garage");
        int bought = 0;
        for (int i = 0; i < 12; i++)
        {
            bool any = false;
            for (int u = 0; u < 6 && !any; u++)
            {
                var up = (Game.Core.Upgrade)u;
                int lvl = d.App.P.Up[u];
                if (lvl >= Game.Core.Balance.MaxLevel(up)) continue;
                long cost = Game.Core.Balance.Cost(up, lvl);
                if (cost > d.App.P.Coins) continue;
                if (d.Tap(Game.Core.Engine.Fmt.Short(cost, true))) { any = true; bought++; d.Wait(0.15f); }
            }
            if (!any) break;
        }
        d.Wait(0.3f);
        d.Shot("garage-apres-achats");
        Console.WriteLine($"garage : {bought} achats, niveaux = {string.Join(",", d.App.P.Up)}, pièces restantes = {d.App.P.Coins}");

        // Accueil et réglages.
        d.Tap("icon:ChevL"); d.Wait(0.6f); d.Shot("accueil");
        d.Tap("icon:Gear"); d.Wait(0.4f); d.Shot("reglages");
        d.Tap("row:Copy"); d.Wait(0.3f); d.Shot("journal-copie");
        Console.WriteLine("--- journal de test copié ---\n" + d.Plat.Clipboard + "---");
        d.Tap("icon:ChevL"); d.Wait(0.4f);

        // Descentes suivantes : jusqu'à voir une panne, la relance proposée, puis la pause.
        bool sawFail = false, pauseShot = false;
        for (int n = 0; n < 12 && !sawFail; n++)
        {
            d.Tap(Game.Core.Loc.Get("play")); d.Wait(0.3f);
            d.PlayRun(true, (run, t) =>
            {
                if (t == -1f) d.Shot("relance-proposee");
                if (!pauseShot && t > 3f) { pauseShot = true; d.Up(); d.Frame(); d.Down(390f - 28f, 47f + 22f); d.Frame(); d.Up(); d.Frame(); d.Wait(0.2f); d.Shot("pause"); d.Tap(Game.Core.Loc.Get("pause.resume")); }
            });
            d.Wait(2.0f);
            if (!d.App.Run.Won) { sawFail = true; d.Shot("fin-panne"); }
            Console.WriteLine($"descente {n + 2} : couche {d.App.Run.StartLayer + 1} gagnée={d.App.Run.Won} prof={d.App.Run.DepthMeters} m vente={d.App.Run.SaleValue}");
            d.Tap(Game.Core.Loc.Get("garage")); d.Wait(0.3f);
            for (int u = 0; u < 6; u++)
            {
                var up = (Game.Core.Upgrade)u;
                int lvl = d.App.P.Up[u];
                if (lvl < Game.Core.Balance.MaxLevel(up) && Game.Core.Balance.Cost(up, lvl) <= d.App.P.Coins) d.Tap(Game.Core.Engine.Fmt.Short(Game.Core.Balance.Cost(up, lvl), true));
            }
            d.Tap("icon:ChevL"); d.Wait(0.4f);
        }
        Console.WriteLine($"cœur du jeu : {d.CoreMs / d.Frames:0.000} ms par image en moyenne, {d.Quads / d.Frames} rectangles en moyenne, {d.MaxQuads} au maximum, {d.Frames} images, {d.Plat.Saves} sauvegardes");

        // Couches avancées : TNT, cavernes, lave.
        foreach (var (layer, up, name) in new[] { (2, "5,2,4,0,1,1", "couche-3-tnt"), (3, "7,3,5,1,2,3", "couche-4-cavernes"), (6, "15,9,12,2,6,9", "couche-7-magma"), (11, "28,12,20,3,8,15", "couche-12-coeur") })
        {
            var e = new AppDriver(outDir, SaveText(layer, up, 1234567));
            e.Wait(0.5f);
            e.Tap(Game.Core.Loc.Get("play")); e.Wait(0.2f);
            int k = 0; float next = 3.5f; bool boom = false;
            e.PlayRun(true, (run, t) =>
            {
                if (t >= next && k < 3) { e.Shot(name + "-" + (char)('a' + k)); k++; next = t + 5f; }
            });
            Console.WriteLine($"{name} : gagné={e.App.Run.Won} prof={e.App.Run.DepthMeters} m durée={e.App.Run.Time:0.0}s rochers={e.App.Run.BoulderHits} lave={e.App.Run.LavaTime:0.0}s");
            if (layer == 11) { e.Wait(2.5f); e.Shot("fin-du-jeu"); }
        }

        // Autres formats d'écran et anglais.
        var small = new AppDriver(outDir, SaveText(4, "9,5,7,1,3,4", 5200), 390, 693, 0f, 0f);
        small.Wait(0.6f); small.Shot("accueil-16-9");
        small.Tap(Game.Core.Loc.Get("garage")); small.Wait(0.5f); small.Shot("garage-16-9");
        var tablet = new AppDriver(outDir, SaveText(4, "9,5,7,1,3,4", 5200), 600, 800, 24f, 20f);
        tablet.Wait(0.6f); tablet.Shot("accueil-tablette");
        var english = new AppDriver(outDir, SaveText(4, "9,5,7,1,3,4", 5200).Replace("lang=fr", "lang=en"));
        english.Wait(0.6f); english.Shot("accueil-anglais");
        Game.Core.Loc.French = true;
        return 0;
    }

    private static int Fonts(string dir, string outPath)
    {
        const float scale = 2f;
        string[] files = Directory.GetFiles(dir, "*.ttf");
        Array.Sort(files);
        int w = 390, h = 150 * files.Length;
        var fb = new SoftRaster((int)(w * scale), (int)(h * scale), scale);
        fb.Clear(0.06f, 0.07f, 0.16f);
        const string chars = " ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,:;!?+-×%/'()éèêàçùôîÉÈÀÇ";
        float y = 0;
        foreach (string f in files)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var font = new TrueTypeFont(File.ReadAllBytes(f));
            var atlas = new Atlas(2048, 2048, scale);
            var list = new DrawList();
            var g = new Gfx(atlas, list);
            g.FontSmall = atlas.AddFont(font, 20, chars);
            g.FontLarge = atlas.AddFont(font, 48, chars);
            sw.Stop();
            g.Text(Path.GetFileNameWithoutExtension(f) + "  (" + sw.ElapsedMilliseconds + " ms)", 12, y + 22, 13, Col.Hex(0x9aa4d8));
            g.TextOutlined("COUCHE 3 FRANCHIE !", 195, y + 52, 30, Col.Hex(0xffd23f), Col.Hex(0x141833), 2.2f);
            g.Text("Énergie +8 s · Puissance ×2 · 1 245 m", 12, y + 96, 18, Col.White);
            g.Text("Glisse pour diriger l'engin. Améliorer : 3,4k", 12, y + 120, 15, Col.Hex(0xc7cdf2));
            g.TextMid("+1 FORET", 320, y + 136, 22, Col.Hex(0x4be3ff));
            fb.Draw(list, atlas);
            y += 150;
        }
        fb.SavePng(outPath);
        Console.WriteLine("écrit : " + outPath);
        return 0;
    }
}
