using System;
using System.Collections.Generic;
using Game.Core.Engine;

namespace Game.Core
{
    /// <summary>
    /// Effets : débris, étincelles, textes volants, secousses d'écran, sons et vibrations.
    /// Ne modifie jamais la simulation : il ne fait que réagir à ses événements.
    /// Positions en blocs (comme la simulation).
    /// </summary>
    public sealed class Fx
    {
        private struct P
        {
            public float X, Y, Vx, Vy, Life, Max, Size, Rot, VRot, Grav, Drag, Grow;
            public uint Rgb;
            public byte Kind;   // 0 éclat, 1 point, 2 étincelle, 3 fumée, 4 lueur, 5 onde, 6 étoile
            public bool Add;
        }

        public struct Txt { public string S; public float X, Y, T, Life, Size; public uint Rgb; }

        private readonly P[] _p = new P[1100];
        private int _n;
        public readonly List<Txt> Texts = new List<Txt>();

        public float Trauma, Flash, HitStop, CoinPulse, EnergyPulse, BannerT = 99f;
        public uint FlashRgb = 0xFFFFFF;
        public string Banner = "", BannerSub = "";
        /// <summary>Message bref ancré à l'écran (effet d'un portail) : il ne chevauche jamais les jauges.</summary>
        public string Notice = "";
        public uint NoticeRgb = 0xFFFFFF;
        public float NoticeT = 99f;
        public bool ShakeOn = true;

        private Rng _r = new Rng(4242);
        private int _combo;
        private float _comboT, _hapticT, _lavaT, _sparkT, _lowT;

        public void Clear()
        {
            _n = 0; Texts.Clear(); Trauma = 0f; Flash = 0f; HitStop = 0f; BannerT = 99f; NoticeT = 99f; _combo = 0;
        }

        private void Add(byte kind, float x, float y, float vx, float vy, float life, float size, uint rgb,
                         float grav, float drag, bool add = false, float grow = 0f, float vrot = 0f)
        {
            if (_n >= _p.Length) return;
            P p;
            p.Kind = kind; p.X = x; p.Y = y; p.Vx = vx; p.Vy = vy; p.Life = life; p.Max = life; p.Size = size; p.Rgb = rgb;
            p.Grav = grav; p.Drag = drag; p.Add = add; p.Grow = grow; p.VRot = vrot; p.Rot = _r.Range(0f, Mathx.Tau);
            _p[_n++] = p;
        }

        public void Burst(float x, float y, int count, uint rgb, float speed, byte kind, float size, float life, float grav = 9f, bool add = false)
        {
            for (int i = 0; i < count; i++)
            {
                float a = _r.Range(0f, Mathx.Tau), sp = _r.Range(0.3f, 1f) * speed;
                Add(kind, x + _r.Range(-0.25f, 0.25f), y + _r.Range(-0.25f, 0.25f), MathF.Cos(a) * sp, MathF.Sin(a) * sp - speed * 0.25f,
                    life * _r.Range(0.6f, 1f), size * _r.Range(0.6f, 1.15f), rgb, grav, 2.2f, add, 0f, _r.Range(-9f, 9f));
            }
        }

        public void Text(string s, float x, float y, uint rgb, float size = 15f, float life = 0.9f)
        {
            Txt t; t.S = s; t.X = x; t.Y = y; t.T = 0f; t.Life = life; t.Size = size; t.Rgb = rgb;
            if (Texts.Count < 24) Texts.Add(t);
        }

        public void Shake(float amount) { Trauma = MathF.Min(1f, Trauma + amount); }

        public void ShowBanner(string title, string sub) { Banner = title; BannerSub = sub; BannerT = 0f; }

        public void Update(float dt)
        {
            for (int i = _n - 1; i >= 0; i--)
            {
                _p[i].Life -= dt;
                if (_p[i].Life <= 0f) { _p[i] = _p[--_n]; continue; }
                float d = MathF.Exp(-_p[i].Drag * dt);
                _p[i].Vx *= d; _p[i].Vy = _p[i].Vy * d + _p[i].Grav * dt;
                _p[i].X += _p[i].Vx * dt; _p[i].Y += _p[i].Vy * dt;
                _p[i].Rot += _p[i].VRot * dt;
                _p[i].Size += _p[i].Grow * dt;
            }
            for (int i = Texts.Count - 1; i >= 0; i--)
            {
                Txt t = Texts[i];
                t.T += dt; t.Y -= dt * 1.3f;
                if (t.T >= t.Life) Texts.RemoveAt(i); else Texts[i] = t;
            }
            Trauma = MathF.Max(0f, Trauma - dt * 1.9f);
            Flash = MathF.Max(0f, Flash - dt * 3.2f);
            CoinPulse = MathF.Max(0f, CoinPulse - dt * 5f);
            EnergyPulse = MathF.Max(0f, EnergyPulse - dt * 4f);
            BannerT += dt; NoticeT += dt;
            _comboT -= dt; if (_comboT <= 0f) _combo = 0;
            _hapticT -= dt; _lavaT -= dt; _sparkT -= dt; _lowT -= dt;
        }

        public void ShakeOffset(float time, out float x, out float y)
        {
            if (!ShakeOn || Trauma <= 0f) { x = 0f; y = 0f; return; }
            float k = Trauma * Trauma * 7f;
            x = MathF.Sin(time * 71f) * k; y = MathF.Cos(time * 83f) * k;
        }

        private static uint ColorOf(CellType t, LayerDef d)
        {
            switch (t)
            {
                case CellType.Soft: return d.Soft;
                case CellType.Medium: return d.Med;
                case CellType.Hard: return d.Hard;
                case CellType.Socle: return 0x8A93B5;
                case CellType.Ore: return d.Ore;
                case CellType.Crystal: return 0x7EEBFF;
                case CellType.Tnt: return 0xE8433F;
                case CellType.Boulder: return 0x9A8F82;
                case CellType.Basalt: return 0x4A4458;
                case CellType.Chest: return 0xFFC83D;
                default: return 0xFFFFFF;
            }
        }

        /// <summary>Réagit à un événement de la simulation.</summary>
        public void Handle(RunEvent e, Run run, AudioBus audio, Action<int> haptic)
        {
            switch (e.Type)
            {
                case Ev.Break:
                    {
                        CellType t = (CellType)e.A;
                        LayerDef d = Balance.Layers[e.B];
                        uint rgb = ColorOf(t, d);
                        int count = t == CellType.Soft ? 3 : 5;
                        Burst(e.X, e.Y, count, rgb, 5.5f, 0, 5.5f, 0.55f, 14f);
                        Burst(e.X, e.Y, 1, Col.Shade(rgb, 0.25f), 2f, 3, 16f, 0.4f, -1f);
                        if (t == CellType.Ore || t == CellType.Crystal || t == CellType.Chest)
                        {
                            Burst(e.X, e.Y, 5, rgb, 7f, 6, 6f, 0.5f, 4f, true);
                            Add(4, e.X, e.Y, 0f, 0f, 0.25f, 42f, rgb, 0f, 0f, true, 40f);
                        }
                        if (t == CellType.Soft) audio.Play(Sfx.BreakSoft, 0.32f, _r.Range(0.9f, 1.15f), 0.05f);
                        else if (t == CellType.Hard || t == CellType.Socle || t == CellType.Boulder) { audio.Play(Sfx.BreakHard, 0.55f, _r.Range(0.9f, 1.1f), 0.06f); Shake(0.12f); }
                        else audio.Play(Sfx.BreakMed, 0.42f, _r.Range(0.9f, 1.12f), 0.05f);
                        break;
                    }
                case Ev.Pickup:
                    if (e.A == 1)
                    {
                        EnergyPulse = 1f;
                        audio.Play(Sfx.Energy, 0.5f, 1f, 0.07f);
                    }
                    else
                    {
                        CoinPulse = 1f;
                        _combo = Math.Min(_combo + 1, 14); _comboT = 0.55f;
                        audio.Play(Sfx.Pickup, 0.45f, 1f + _combo * 0.055f, 0.035f);
                    }
                    if (_hapticT <= 0f) { _hapticT = 0.09f; haptic(0); }
                    break;
                case Ev.Gate:
                    {
                        GateFx fx = (GateFx)e.A;
                        bool good = Balance.IsBonus(fx);
                        uint rgb = good ? 0x4BD6FFu : 0xFF5A6Eu;
                        Notice = Loc.Get("gate." + fx); NoticeRgb = rgb; NoticeT = 0f;
                        Add(5, e.X, e.Y, 0f, 0f, 0.45f, 30f, rgb, 0f, 0f, true, 260f);
                        Burst(e.X, e.Y, 14, rgb, 9f, 6, 7f, 0.7f, 3f, true);
                        Flash = 0.35f; FlashRgb = rgb;
                        Shake(good ? 0.25f : 0.45f);
                        audio.Play(good ? Sfx.GateGood : Sfx.GateBad, 0.8f);
                        haptic(1);
                        break;
                    }
                case Ev.Explosion:
                    Burst(e.X, e.Y, 16, 0xFFB03A, 13f, 2, 9f, 0.5f, 6f, true);
                    Burst(e.X, e.Y, 8, 0xFFF1A8, 8f, 1, 6f, 0.4f, 2f, true);
                    Burst(e.X, e.Y, 6, 0x55504C, 4f, 3, 30f, 0.8f, -1.5f);
                    Add(4, e.X, e.Y, 0f, 0f, 0.3f, 120f, 0xFFC56B, 0f, 0f, true, 200f);
                    Add(5, e.X, e.Y, 0f, 0f, 0.4f, 24f, 0xFFE2A8, 0f, 0f, true, 330f);
                    Shake(0.55f); HitStop = MathF.Max(HitStop, 0.035f);
                    audio.Play(Sfx.Tnt, 0.9f, _r.Range(0.85f, 1.15f), 0.04f);
                    haptic(2);
                    break;
                case Ev.BoulderHit:
                    Burst(e.X, e.Y, 12, 0x9A8F82, 8f, 0, 7f, 0.7f, 14f);
                    Text("-" + ((int)Balance.BoulderHitCost) + " " + Loc.Get("unit.s"), e.X, e.Y - 1f, 0xFF5A6E, 20f, 1.1f);
                    Flash = 0.5f; FlashRgb = 0xFF3B4E;
                    Shake(0.8f); HitStop = MathF.Max(HitStop, 0.05f);
                    audio.Play(Sfx.Boulder, 1f);
                    haptic(2);
                    break;
                case Ev.BoulderLand:
                    Burst(e.X, e.Y + 0.4f, 4, 0x9A8F82, 3f, 3, 14f, 0.4f, -1f);
                    audio.Play(Sfx.Boulder, 0.35f, 1.3f, 0.12f);
                    break;
                case Ev.Chest:
                    Burst(e.X, e.Y, 18, 0xFFD23F, 10f, 6, 8f, 0.8f, 5f, true);
                    audio.Play(Sfx.Buy, 0.8f);
                    haptic(1);
                    break;
                case Ev.SealBreak:
                    for (int i = 0; i < World.Cols; i++)
                    {
                        Burst(i + 0.5f, e.Y, 6, 0x6C7CA0, 12f, 0, 8f, 1f, 16f);
                        Burst(i + 0.5f, e.Y, 3, 0xFFD23F, 10f, 6, 8f, 0.9f, 4f, true);
                    }
                    Add(5, e.X, e.Y, 0f, 0f, 0.6f, 40f, 0xFFFFFF, 0f, 0f, true, 700f);
                    Add(4, e.X, e.Y, 0f, 0f, 0.5f, 240f, 0xFFE9A8, 0f, 0f, true, 500f);
                    Flash = 0.9f; FlashRgb = 0xFFFFFF;
                    Shake(1f); HitStop = 0.11f;
                    audio.Play(Sfx.SealBreak, 1f);
                    haptic(2);
                    if (e.A >= Balance.LayerCount - 1) ShowBanner(Loc.Get("hud.final"), "");
                    else ShowBanner(Loc.Get("hud.cleared", e.A + 1), "+" + ((int)Balance.SealEnergyBonus) + " " + Loc.Get("unit.s"));
                    break;
                case Ev.LowEnergy:
                    audio.Play(Sfx.LowBattery, 0.7f);
                    break;
                case Ev.Over:
                    if (!run.Finished) audio.Play(Sfx.PowerDown, 0.8f);
                    break;
            }
        }

        /// <summary>Effets continus : étincelles du foret, braises de lave, grondement du moteur.</summary>
        public void Continuous(Run run, AudioBus audio, bool active, float dt)
        {
            float targetVol = active && !run.Over ? 0.16f + 0.5f * run.Load : 0f;
            audio.DrillVol = Mathx.Damp(audio.DrillVol, targetVol, 10f, dt);
            audio.DrillPitch = 0.78f + 0.3f * run.Load + 0.12f * (run.SpeedMul - 1f) + (run.FreeFall ? 0.25f : 0f);
            if (!active || run.Over) return;

            if (run.Blocked && _sparkT <= 0f)
            {
                _sparkT = 0.03f;
                int pod = _r.Range(0, run.Pods);
                float x = run.PodX(pod) + _r.Range(-0.3f, 0.3f), y = run.Y + 0.75f;
                Add(2, x, y, _r.Range(-6f, 6f), _r.Range(-9f, -3f), 0.25f, 7f, run.OnSeal ? 0xBFE0FFu : 0xFFD98Au, 22f, 1f, true, 0f, 0f);
                if (run.OnSeal) { audio.Play(Sfx.SealHit, 0.4f, _r.Range(0.9f, 1.1f), 0.11f); Shake(0.05f); }
            }
            if (run.InLava && _lavaT <= 0f)
            {
                _lavaT = 0.07f;
                Burst(run.X + _r.Range(-run.HalfWidth, run.HalfWidth), run.Y, 3, 0xFF8A2A, 5f, 1, 6f, 0.5f, -6f, true);
                Flash = MathF.Max(Flash, 0.22f); FlashRgb = 0xFF5A1E;
                Shake(0.1f);
                audio.Play(Sfx.Lava, 0.6f, _r.Range(0.9f, 1.1f), 0.22f);
            }
            if (run.Energy < Balance.LowEnergy && !run.Finished && _lowT <= 0f)
            {
                _lowT = 0.8f;
                audio.Play(Sfx.LowBattery, 0.5f, 1f + (Balance.LowEnergy - run.Energy) * 0.04f);
            }
        }

        public void Draw(Gfx g, Art art, float cell)
        {
            for (int i = 0; i < _n; i++)
            {
                P p = _p[i];
                float k = p.Life / p.Max;
                float x = p.X * cell, y = p.Y * cell;
                uint c = p.Add ? Col.Glow(p.Rgb, k) : Col.Hex(p.Rgb, MathF.Min(1f, k * 1.6f));
                switch (p.Kind)
                {
                    case 0: g.Sprite(art.PShard, x, y, p.Size / 8f * (0.5f + 0.5f * k), p.Rot, c); break;
                    case 1: g.Sprite(art.PDot, x, y, p.Size / 8f * k, 0f, c); break;
                    case 2: g.SpriteXY(art.PSpark, x, y, p.Size / 16f, p.Size / 16f * (0.6f + k), MathF.Atan2(p.Vy, p.Vx) + 1.5708f, c); break;
                    case 3: g.Sprite(art.PSmoke, x, y, p.Size / 28f * (1.6f - 0.6f * k), 0f, Col.Hex(p.Rgb, k * 0.5f)); break;
                    case 4: g.Sprite(art.PGlow, x, y, p.Size / 72f, 0f, Col.Glow(p.Rgb, k * 0.8f)); break;
                    case 5: g.Sprite(art.PRing, x, y, p.Size / 64f, 0f, Col.Glow(p.Rgb, k)); break;
                    case 6: g.Sprite(art.PStar, x, y, p.Size / 18f * (0.4f + 0.6f * k), p.Rot, c); break;
                }
            }
        }

        public void DrawTexts(Gfx g, float cell)
        {
            for (int i = 0; i < Texts.Count; i++)
            {
                Txt t = Texts[i];
                float k = t.T / t.Life;
                float pop = Mathx.EaseOutBack(MathF.Min(1f, t.T * 6f));
                float a = k > 0.7f ? 1f - (k - 0.7f) / 0.3f : 1f;
                g.TextOutlined(t.S, t.X * cell, t.Y * cell, t.Size * pop, Col.Hex(t.Rgb, a), Col.Hex(0x0B0D22, a), 2f);
            }
        }
    }
}
