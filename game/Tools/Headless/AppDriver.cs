using System;
using System.Diagnostics;
using System.IO;
using Game.Core;
using Game.Core.Engine;

namespace Headless;

/// <summary>Plateforme factice : sauvegarde en mémoire, presse-papiers simulé.</summary>
public sealed class MemoryPlatform : IPlatform
{
    public string Data = "";
    public string Clipboard = "";
    public int Haptics, Saves;
    public string Lang = "fr";
    public string LoadSave() { return Data; }
    public void StoreSave(string data) { Data = data; Saves++; }
    public void Haptic(int strength) { Haptics++; }
    public void CopyText(string text) { Clipboard = text; }
    public string Language { get { return Lang; } }
}

/// <summary>
/// Pilote le jeu complet comme le ferait un joueur : touchers, glissements, attente, captures d'écran.
/// C'est le test de bout en bout du prototype, sans Unity.
/// </summary>
public sealed class AppDriver
{
    public readonly GameApp App;
    public readonly MemoryPlatform Plat = new MemoryPlatform();
    public DateTime Now = new DateTime(2026, 11, 12, 10, 0, 0);
    /// <summary>Appelé après chaque image (capture vidéo, mixage du son).</summary>
    public Action<float> OnFrame;
    public SoftRaster Fb { get { return _fb; } }
    public double CoreMs;
    public long Frames, Quads;
    public int MaxQuads;

    private readonly SoftRaster _fb;
    private readonly float _px;      // pixels par point du jeu
    private readonly string _outDir;
    private float _fingerX = 195f, _fingerY = 600f;
    private bool _fingerDown;
    private int _shot;
    private readonly Stopwatch _sw = new Stopwatch();

    public AppDriver(string outDir, string save = "", int wPt = 390, int hPt = 844, float safeTop = 47f, float safeBottom = 34f, float scale = 2f, string lang = "fr")
    {
        _outDir = outDir;
        Plat.Data = save; Plat.Lang = lang;
        App = new GameApp(Plat, File.ReadAllBytes(Path.Combine(Program.RepoGame(), "Assets", "Game", "Resources", "Fonts", "GameFont.bytes")), scale, () => Now);
        App.Resize(wPt * scale, hPt * scale, safeTop * scale, safeBottom * scale);
        // Sur un écran large (tablette), la zone de jeu est centrée : un point du jeu ne vaut plus « scale » pixels.
        _px = wPt * scale / App.ScreenW;
        if (outDir != null) _fb = new SoftRaster((int)(wPt * scale), (int)(hPt * scale), _px);
    }

    public void Frame(float dt = 1f / 60f)
    {
        _sw.Restart();
        App.Frame(dt);
        _sw.Stop();
        CoreMs += _sw.Elapsed.TotalMilliseconds; Frames++;
        Quads += App.List.QuadCount; MaxQuads = Math.Max(MaxQuads, App.List.QuadCount);
        if (OnFrame != null) OnFrame(dt);
        App.Audio.Shots.Clear();
    }

    public void Wait(float seconds) { for (int i = 0; i < (int)(seconds * 60f); i++) Frame(); }

    public string Shot(string name)
    {
        if (_outDir == null) return null;
        _fb.Clear(0f, 0f, 0f);
        _fb.Draw(App.List, App.Atlas);
        string path = Path.Combine(_outDir, $"{_shot:00}-{name}.png");
        _fb.SavePng(path);
        _shot++;
        return path;
    }

    public void Down(float xPt, float yPt) { _fingerX = xPt; _fingerY = yPt; _fingerDown = true; App.PointerDown(xPt * _px, yPt * _px); }
    public void Move(float xPt, float yPt) { _fingerX = xPt; _fingerY = yPt; App.PointerMove(xPt * _px, yPt * _px); }
    public void Up() { _fingerDown = false; App.PointerUp(_fingerX * _px, _fingerY * _px); }

    /// <summary>Touche un élément d'interface repéré par son libellé.</summary>
    public bool Tap(string id)
    {
        foreach (var h in App.Hits)
        {
            if (h.Id != id) continue;
            if (!h.Enabled) return false;
            Down(h.X + App.OffsetX, h.Y); Frame(); Frame();
            Up(); Frame(); Frame();
            return true;
        }
        return false;
    }

    public bool Has(string id)
    {
        foreach (var h in App.Hits) if (h.Id == id) return true;
        return false;
    }

    /// <summary>
    /// Joue une descente en glissant le doigt comme un joueur (le robot décide de la cible).
    /// onFrame reçoit le temps écoulé ; reviveChoice : libellé à toucher si la relance est proposée.
    /// </summary>
    public void PlayRun(bool casual, Action<Run, float> onFrame = null, bool acceptRevive = false, float maxSeconds = 400f)
    {
        Run run = App.Run;
        var bot = new Bot(casual, (uint)(run.StartLayer * 131 + App.P.RunCounter));
        float t = 0f;
        const float dt = 1f / 60f;
        Down(195f + App.OffsetX, 600f); Frame();
        while (App.Screen == GameScreen.Run && t < maxSeconds)
        {
            run = App.Run;
            string yes = Loc.Get("revive.yes"), no = Loc.Get("revive.no");
            if (Has(yes))
            {
                if (_fingerDown) { Up(); Frame(); }
                if (onFrame != null) onFrame(run, -1f);
                Tap(acceptRevive ? yes : no);
                if (acceptRevive) { Wait(0.3f); if (onFrame != null) onFrame(run, -2f); Wait(2.4f); Down(195f + App.OffsetX, 600f); }
                Frame();
                continue;
            }
            if (!run.Over)
            {
                if (!_fingerDown) Down(195f + App.OffsetX, 600f);
                float target = bot.Decide(run, dt);
                float x = _fingerX + (target - run.SteerX) * RunView.Cell / 1.25f;
                if (x < 16f + App.OffsetX || x > 374f + App.OffsetX)
                {
                    // Bord d'écran atteint : le joueur relève le doigt et le repose au centre.
                    Up(); Frame(); Down(195f + App.OffsetX, 600f);
                }
                else Move(x, 600f);
            }
            Frame(); t += dt;
            if (onFrame != null) onFrame(run, t);
        }
        if (_fingerDown) { Up(); Frame(); }
    }
}
