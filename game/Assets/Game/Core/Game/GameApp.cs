using System;
using Game.Core.Engine;

namespace Game.Core
{
    /// <summary>Ce que le jeu attend de la plateforme (Unity, ou le banc d'essai). Rien d'autre ne dépend du moteur.</summary>
    public interface IPlatform
    {
        string LoadSave();
        void StoreSave(string data);
        /// <summary>0 : léger, 1 : moyen, 2 : fort.</summary>
        void Haptic(int strength);
        void CopyText(string text);
        /// <summary>Code de langue du système : "fr", "en"...</summary>
        string Language { get; }
    }

    public enum GameScreen { Home, Run, Results, Garage, Settings }

    /// <summary>
    /// Le jeu entier : états, écrans, boucle de simulation et dessin de chaque image.
    /// La plateforme lui transmet les entrées (doigt) et le temps ; il remplit une liste de rectangles à dessiner
    /// et une file de sons à jouer.
    /// </summary>
    public sealed partial class GameApp
    {
        public const string Version = "0.1.0";
        public const float ViewW = 390f;
        private const float MinAspect = 16f / 9f;
        private const float SimStep = 1f / 60f;

        public readonly Atlas Atlas;
        public readonly DrawList List = new DrawList();
        public readonly Gfx G;
        public readonly Art Art = new Art();
        public readonly AudioBus Audio = new AudioBus();
        public Progress P;

        /// <summary>Taille de l'écran en points (la zone de jeu fait 390 points de large, centrée).</summary>
        public float ScreenW = ViewW, ViewH = 844f, OffsetX, SafeTop, SafeBottom;
        private float _ptPerPx = 1f;

        private readonly IPlatform _plat;
        private readonly Func<DateTime> _clock;

        public GameScreen Screen { get; private set; }
        private float _screenT, _time;

        // Doigt.
        private float _px, _py, _pressX = -999f, _pressY = -999f, _lastX;
        private bool _down, _pressed, _released;

        // Descente en cours.
        public Run Run { get; private set; }
        private readonly RunView _view = new RunView();
        private readonly Fx _fx = new Fx();
        private bool _started, _paused, _steering, _quit, _reviveOffered, _reviveOpen;
        private float _acc, _overT, _reviveT, _graceT;

        // Fin de descente.
        private long _sale;
        private bool _doubled, _record, _won, _finalWin, _adOffered;
        private int _resultLayer, _resultDepth, _sealPct, _metersLeft;

        // Pub simulée, message bref, réglages.
        private float _adT = -1f;
        private Action _adDone;
        private string _toast = "";
        private float _toastT = 99f, _resetArm = -1f;

        // Journal de test.
        private float _saveT, _fpsT;
        private int _fpsFrames;
        private DateTime _pausedAt;

        public GameApp(IPlatform platform, byte[] fontData, float pixelsPerPoint, Func<DateTime> clock = null)
        {
            _plat = platform;
            _clock = clock ?? (() => DateTime.Now);
            float scale = Mathx.Clamp(pixelsPerPoint, 1.5f, 3f);
            var font = new TrueTypeFont(fontData);
            Atlas = new Atlas(2048, scale <= 2.1f ? 1024 : 2048, scale);
            Art.Build(Atlas, font);
            G = new Gfx(Atlas, List);
            G.FontSmall = Atlas.AddFont(font, 18f, Art.Charset);
            G.FontLarge = Atlas.AddFont(font, 44f, Art.CharsetLarge);

            P = Progress.Parse(_plat.LoadSave());
            if (string.IsNullOrEmpty(P.Lang))
                P.Lang = (_plat.Language ?? "").StartsWith("fr", StringComparison.OrdinalIgnoreCase) ? "fr" : "en";
            ApplySettings();
            BeginSession();

            if (!P.TutorialDone) StartRun(0);
            else Go(GameScreen.Home);
        }

        private void ApplySettings()
        {
            Loc.French = P.Lang == "fr";
            Audio.SfxOn = P.Sound; Audio.MusicOn = P.Music;
            _fx.ShakeOn = P.Shake;
        }

        private string Today() { return _clock().ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture); }

        private void BeginSession()
        {
            P.J.Touch(Today());
            P.J.Sessions++;
            P.J.SessionSeconds.Add(0);
            while (P.J.SessionSeconds.Count > 20) P.J.SessionSeconds.RemoveAt(0);
        }

        public void Save() { _plat.StoreSave(P.Serialize()); }

        /// <summary>L'application passe en arrière-plan : on sauvegarde tout de suite.</summary>
        public void OnPause()
        {
            _pausedAt = _clock();
            if (Screen == GameScreen.Run && Run != null && !Run.Over && _started) _paused = true;
            Save();
        }

        public void OnResume()
        {
            if ((_clock() - _pausedAt).TotalSeconds > 300) BeginSession();
            else P.J.Touch(Today());
        }

        // ------------------------------------------------------------------ écran et entrées

        public void Resize(float widthPx, float heightPx, float safeTopPx, float safeBottomPx)
        {
            if (widthPx < 1f || heightPx < 1f) return;
            if (heightPx / widthPx >= MinAspect)
            {
                _ptPerPx = ViewW / widthPx;
                ViewH = heightPx * _ptPerPx; ScreenW = ViewW; OffsetX = 0f;
            }
            else
            {
                // Écran trop large (tablette) : zone de jeu centrée, bandes sombres sur les côtés.
                ViewH = ViewW * MinAspect;
                _ptPerPx = ViewH / heightPx;
                ScreenW = widthPx * _ptPerPx; OffsetX = (ScreenW - ViewW) * 0.5f;
            }
            SafeTop = safeTopPx * _ptPerPx; SafeBottom = safeBottomPx * _ptPerPx;
        }

        public void PointerDown(float xPx, float yPx)
        {
            _px = xPx * _ptPerPx - OffsetX; _py = yPx * _ptPerPx;
            _pressX = _px; _pressY = _py; _lastX = _px;
            _down = true; _pressed = true;
            if (P.J.FirstTouchDelay < 0f) P.J.FirstTouchDelay = _time;
        }

        public void PointerMove(float xPx, float yPx)
        {
            _px = xPx * _ptPerPx - OffsetX; _py = yPx * _ptPerPx;
        }

        public void PointerUp(float xPx, float yPx)
        {
            PointerMove(xPx, yPx);
            _down = false; _released = true;
        }

        /// <summary>Touche « retour » d'Android. Renvoie faux si le jeu n'en fait rien.</summary>
        public bool Back()
        {
            if (_adT >= 0f) return true;
            switch (Screen)
            {
                case GameScreen.Run: if (Run != null && !Run.Over) _paused = !_paused; return true;
                case GameScreen.Garage: case GameScreen.Settings: case GameScreen.Results: Go(GameScreen.Home); return true;
                default: return false;
            }
        }

        private void Go(GameScreen s)
        {
            Screen = s; _screenT = 0f; _resetArm = -1f;
            _pressX = -999f; _pressY = -999f;
        }

        private void Toast(string text) { _toast = text; _toastT = 0f; }

        // ------------------------------------------------------------------ boucle principale

        public void Frame(float dt)
        {
            if (dt > 0.1f) dt = 0.1f;
            if (dt < 0f) dt = 0f;
            _time += dt; _screenT += dt; _toastT += dt;
            Audio.Tick(dt);
            TickJournal(dt);

            List.Clear();
            Hits.Clear();
            G.ResetTransform();
            G.Push(OffsetX, 0f);

            bool adOpen = _adT >= 0f;
            if (adOpen) { _pressed = false; _released = false; _pressX = -999f; }

            switch (Screen)
            {
                case GameScreen.Home: DrawHome(); break;
                case GameScreen.Run: UpdateRun(dt); DrawRun(); break;
                case GameScreen.Results: DrawResults(dt); break;
                case GameScreen.Garage: DrawGarage(); break;
                case GameScreen.Settings: DrawSettings(dt); break;
            }
            if (Screen != GameScreen.Run) { Audio.DrillVol = Mathx.Damp(Audio.DrillVol, 0f, 12f, dt); }
            Audio.MusicVol = Mathx.Damp(Audio.MusicVol, Audio.MusicOn ? (Screen == GameScreen.Run ? 0.26f : 0.36f) : 0f, 4f, dt);

            if (adOpen) DrawAd(dt);
            DrawToast();
            G.Pop();

            if (OffsetX > 0.5f)
            {
                G.Rect(0f, 0f, OffsetX, ViewH, Col.Hex(0x05060F));
                G.Rect(OffsetX + ViewW, 0f, ScreenW - OffsetX - ViewW + 1f, ViewH, Col.Hex(0x05060F));
            }
            _pressed = false; _released = false;
        }

        private void TickJournal(float dt)
        {
            Journal j = P.J;
            j.TotalSeconds += dt;
            if (j.SessionSeconds.Count > 0)
            {
                _saveT += dt;
                if (_saveT >= 1f)
                {
                    j.SessionSeconds[j.SessionSeconds.Count - 1] += (int)_saveT;
                    _saveT -= (int)_saveT;
                }
            }
            if (Screen == GameScreen.Run && !_paused && dt > 0f)
            {
                _fpsT += dt; _fpsFrames++;
                if (_fpsT >= 2f) { j.FpsSum += _fpsFrames / _fpsT; j.FpsCount++; _fpsT = 0f; _fpsFrames = 0; }
            }
        }

        // ------------------------------------------------------------------ descente

        public void StartRun(int layer)
        {
            layer = Mathx.Clamp(layer, 0, Balance.LayerCount - 1);
            uint seed = (uint)(P.RunCounter * 2654435761u + (uint)layer * 97u + 12345u);
            Run = new Run(layer, seed, P.Up, !P.TutorialDone);
            P.RunCounter++;
            P.J.RunsStarted++;
            P.J.Attempts[layer]++;
            _fx.Clear(); _view.Reset();
            _started = P.TutorialDone; _paused = false; _steering = false; _quit = false;
            _reviveOffered = false; _reviveOpen = false; _acc = 0f; _overT = 0f; _graceT = 0.45f;
            Go(GameScreen.Run);
        }

        private bool OnPauseButton(float x, float y)
        {
            float cx = ViewW - 28f, cy = SafeTop + 22f;
            return (x - cx) * (x - cx) + (y - cy) * (y - cy) < 26f * 26f;
        }

        private void UpdateRun(float dt)
        {
            Run run = Run;
            if (_pressed && !_paused && !_reviveOpen && !run.Over)
            {
                if (OnPauseButton(_px, _py)) { _paused = true; Audio.Play(Sfx.Tap); }
                else { _steering = true; _lastX = _px; _started = true; }
            }
            if (!_down) _steering = false;
            if (_steering && !_paused)
            {
                run.SteerX += (_px - _lastX) / RunView.Cell * 1.25f;
                _lastX = _px;
            }

            bool simulate = _started && !_paused && !_reviveOpen && _adT < 0f;
            if (simulate)
            {
                if (_graceT > 0f) { _graceT -= dt; run.Energy = MathF.Max(run.Energy, run.EnergyMax); }
                if (_fx.HitStop > 0f) _fx.HitStop -= dt;
                else
                {
                    _acc += dt;
                    int steps = 0;
                    while (_acc >= SimStep && steps < 4)
                    {
                        _acc -= SimStep; steps++;
                        bool wasOver = run.Over;
                        run.Step(SimStep);
                        for (int i = 0; i < run.Events.Count; i++) _fx.Handle(run.Events[i], run, Audio, Haptic);
                        run.Events.Clear();
                        if (run.Over && !wasOver) { _overT = 0f; break; }
                    }
                    if (steps == 4) _acc = 0f;
                }
            }
            _fx.Continuous(run, Audio, simulate, dt);
            if (!_paused) _fx.Update(dt);
            _view.Update(run, dt, ViewH);

            if (run.Over && !_reviveOpen && _adT < 0f)
            {
                _overT += dt;
                // Une relance par descente est proposée quand le sceau était à portée.
                if (!_reviveOffered && !run.Won && !_quit && !run.Finished && _overT > 0.5f && LayerProgress(run) > 0.6f)
                {
                    _reviveOffered = true; _reviveOpen = true; _reviveT = 0f;
                    P.J.AdsOffered++;
                }
                else if (_overT > (run.Finished ? 0.4f : 1.1f)) EndRun();
            }
        }

        private void Haptic(int strength) { if (P.Haptics) _plat.Haptic(strength); }

        private static float LayerProgress(Run run)
        {
            int len = Balance.Layers[run.StartLayer].Length + Balance.SealApproachRows;
            return Mathx.Clamp01(run.Y / len);
        }

        private void EndRun()
        {
            Run run = Run;
            _sale = run.SaleValue;
            _won = run.Won; _finalWin = run.Finished;
            _resultLayer = run.StartLayer + Math.Max(0, run.LayersCleared - 1);
            _resultDepth = run.DepthMeters;
            _record = _resultDepth > P.BestDepth && P.TutorialDone;
            _doubled = false; _adOffered = false;

            SealState seal = run.W.NextSeal(run.Y);
            _sealPct = 0; _metersLeft = 0;
            if (seal != null && !run.Won)
            {
                if (seal.Hp < seal.MaxHp) _sealPct = Math.Max(1, (int)MathF.Ceiling(seal.Hp / seal.MaxHp * 100f));
                else _metersLeft = Math.Max(1, (int)MathF.Ceiling(seal.Row - run.Y));
            }

            P.Coins += _sale;
            if (_resultDepth > P.BestDepth) P.BestDepth = _resultDepth;
            if (run.LayersCleared > 0)
            {
                P.MaxLayer = Math.Min(Balance.LayerCount, Math.Max(P.MaxLayer, run.StartLayer + run.LayersCleared));
                P.SelectedLayer = P.PlayableLayer;
            }
            if (run.Finished) P.Finished = true;
            P.TutorialDone = true;
            Journal j = P.J;
            j.RunsEnded++;
            if (_quit) j.Quits++; else if (_won) j.Wins++; else j.Fails++;
            j.GatesGood += run.GatesGood; j.GatesBad += run.GatesBad;
            if (P.MaxLayer > j.BestLayer) j.BestLayer = P.MaxLayer;
            Save();

            if (_won) Audio.Play(Sfx.Win, 0.9f);
            Go(GameScreen.Results);
        }

        private void DrawRun()
        {
            Run run = Run;
            _view.Draw(G, Art, run, _fx, ViewH, _time);
            bool pausePressed = _down && OnPauseButton(_pressX, _pressY);
            _view.DrawHud(G, Art, run, _fx, ViewH, SafeTop, _time, _started, pausePressed);
            if (_reviveOpen) DrawRevive();
            else if (_paused) DrawPause();
        }
    }
}
