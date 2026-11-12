using System;
using System.Collections.Generic;

namespace Game.Core.Engine
{
    public enum Sfx
    {
        BreakSoft, BreakMed, BreakHard, Pickup, Energy, GateGood, GateBad, Tnt, Boulder, Lava,
        SealHit, SealBreak, LowBattery, PowerDown, Win, Tap, Buy, CoinTick, Count
    }

    /// <summary>
    /// File d'attente des sons à jouer. Le jeu y dépose ses demandes ; la couche Unity les exécute.
    /// </summary>
    public sealed class AudioBus
    {
        public struct Shot { public Sfx Id; public float Vol, Pitch; }

        public readonly List<Shot> Shots = new List<Shot>(16);
        /// <summary>Boucle du foret : volume et hauteur suivent l'effort de l'engin.</summary>
        public float DrillVol, DrillPitch = 1f;
        public float MusicVol;
        public bool SfxOn = true, MusicOn = true;

        private readonly float[] _last = new float[(int)Sfx.Count];
        private float _time = 10f;

        public void Tick(float dt) { _time += dt; }

        public void Play(Sfx id, float vol = 1f, float pitch = 1f, float minInterval = 0f)
        {
            if (!SfxOn || Shots.Count >= 16) return;
            int i = (int)id;
            if (_time - _last[i] < minInterval) return;
            _last[i] = _time;
            Shot s; s.Id = id; s.Vol = vol; s.Pitch = pitch;
            Shots.Add(s);
        }
    }

    /// <summary>
    /// Tous les sons du jeu, fabriqués par synthèse au démarrage (aucun fichier audio, donc aucun droit tiers).
    /// </summary>
    public static class SoundBank
    {
        public const int Rate = 32000;
        private const float Tau = 6.2831853f;

        private sealed class Buf
        {
            public readonly float[] D;
            public readonly bool Wrap;
            private uint _seed = 12345u;
            public Buf(float seconds, bool wrap = false) { D = new float[(int)(seconds * Rate)]; Wrap = wrap; }

            public float Noise()
            {
                _seed = _seed * 1664525u + 1013904223u;
                return ((_seed >> 8) & 0xFFFF) / 32768f - 1f;
            }

            /// <summary>Ajoute une voix : f(t) reçoit le temps écoulé depuis le début de la note.</summary>
            public void Add(float start, float dur, Func<float, float> f)
            {
                int s0 = (int)(start * Rate), n = (int)(dur * Rate);
                for (int i = 0; i < n; i++)
                {
                    int k = s0 + i;
                    if (Wrap) k %= D.Length; else if (k >= D.Length) break;
                    float t = i / (float)Rate;
                    // Attaque et fin adoucies pour éviter les claquements.
                    float edge = MathF.Min(1f, MathF.Min(i / 64f, (n - i) / 160f));
                    D[k] += f(t) * edge;
                }
            }

            public float[] Finish(float peak)
            {
                float m = 0.0001f;
                for (int i = 0; i < D.Length; i++) { float a = MathF.Abs(D[i]); if (a > m) m = a; }
                float g = peak / m;
                for (int i = 0; i < D.Length; i++) D[i] *= g;
                return D;
            }
        }

        private static float Sin(float f, float t) { return MathF.Sin(Tau * f * t); }
        private static float Tri(float f, float t) { float p = f * t; p -= MathF.Floor(p); return 4f * MathF.Abs(p - 0.5f) - 1f; }
        private static float Saw(float f, float t) { float p = f * t; return 2f * (p - MathF.Floor(p)) - 1f; }
        private static float Sqr(float f, float t) { float p = f * t; return p - MathF.Floor(p) < 0.5f ? 1f : -1f; }
        private static float Dec(float t, float tau) { return MathF.Exp(-t / tau); }
        /// <summary>Phase d'un glissement exponentiel de fréquence de f0 vers f1.</summary>
        private static float Glide(float f0, float f1, float tau, float t)
        {
            return Tau * (f1 * t + (f0 - f1) * tau * (1f - MathF.Exp(-t / tau)));
        }

        /// <summary>Filtre passe-bas simple, appliqué à un tampon entier.</summary>
        private static void LowPass(float[] d, float cutoff, float cutoffEnd = -1f)
        {
            float y = 0f;
            for (int i = 0; i < d.Length; i++)
            {
                float fc = cutoffEnd < 0f ? cutoff : cutoff * MathF.Pow(cutoffEnd / cutoff, i / (float)d.Length);
                float a = 1f - MathF.Exp(-Tau * fc / Rate);
                y += a * (d[i] - y);
                d[i] = y;
            }
        }

        private static void HighPass(float[] d, float cutoff)
        {
            float y = 0f, a = 1f - MathF.Exp(-Tau * cutoff / Rate);
            for (int i = 0; i < d.Length; i++) { y += a * (d[i] - y); d[i] -= y; }
        }

        private static void Mix(Buf dst, float[] src, float gain)
        {
            for (int i = 0; i < src.Length && i < dst.D.Length; i++) dst.D[i] += src[i] * gain;
        }

        private static float[] NoiseBurst(float dur, float tau, float lp, float lpEnd = -1f, float hp = 0f)
        {
            var b = new Buf(dur);
            b.Add(0f, dur, t => b.Noise() * Dec(t, tau));
            if (hp > 0f) HighPass(b.D, hp);
            LowPass(b.D, lp, lpEnd);
            return b.D;
        }

        public static float[] Build(Sfx id)
        {
            Buf b;
            switch (id)
            {
                case Sfx.BreakSoft:
                    b = new Buf(0.10f);
                    Mix(b, NoiseBurst(0.10f, 0.022f, 1100f), 1f);
                    b.Add(0f, 0.10f, t => MathF.Sin(Glide(170f, 70f, 0.03f, t)) * Dec(t, 0.03f) * 0.7f);
                    return b.Finish(0.55f);
                case Sfx.BreakMed:
                    b = new Buf(0.12f);
                    Mix(b, NoiseBurst(0.12f, 0.028f, 2800f, -1f, 350f), 1f);
                    b.Add(0f, 0.12f, t => MathF.Sin(Glide(260f, 110f, 0.025f, t)) * Dec(t, 0.025f) * 0.5f);
                    return b.Finish(0.6f);
                case Sfx.BreakHard:
                    b = new Buf(0.18f);
                    Mix(b, NoiseBurst(0.18f, 0.012f, 5000f, -1f, 800f), 0.8f);
                    b.Add(0f, 0.18f, t => (Sin(620f, t) + Sin(933f, t) * 0.8f + Sin(1410f, t) * 0.6f + Sin(2210f, t) * 0.4f) * Dec(t, 0.045f) * 0.3f);
                    return b.Finish(0.6f);
                case Sfx.Pickup:
                    b = new Buf(0.11f);
                    b.Add(0f, 0.11f, t => (MathF.Sin(Glide(880f, 1500f, 0.03f, t)) + 0.3f * MathF.Sin(2f * Glide(880f, 1500f, 0.03f, t))) * Dec(t, 0.035f));
                    return b.Finish(0.45f);
                case Sfx.Energy:
                    b = new Buf(0.26f);
                    b.Add(0f, 0.16f, t => (Tri(660f, t) + 0.3f * Sin(1320f, t)) * Dec(t, 0.06f));
                    b.Add(0.07f, 0.19f, t => (Tri(990f, t) + 0.3f * Sin(1980f, t)) * Dec(t, 0.07f));
                    return b.Finish(0.5f);
                case Sfx.GateGood:
                    b = new Buf(0.55f);
                    float[] up = { 523.25f, 659.25f, 783.99f, 1046.5f };
                    for (int i = 0; i < 4; i++) { float f = up[i]; b.Add(i * 0.06f, 0.36f, t => (Tri(f, t) * 0.7f + Sin(f * 2f, t) * 0.3f) * Dec(t, 0.13f)); }
                    Mix(b, NoiseBurst(0.3f, 0.09f, 1500f, 7000f, 900f), 0.35f);
                    return b.Finish(0.6f);
                case Sfx.GateBad:
                    b = new Buf(0.42f);
                    b.Add(0f, 0.42f, t => (Sqr(1f, Glide(233f, 150f, 0.12f, t) / Tau) * 0.6f + Saw(1f, Glide(116f, 75f, 0.12f, t) / Tau) * 0.5f)
                                          * (0.75f + 0.25f * Sin(11f, t)) * Dec(t, 0.16f));
                    LowPass(b.D, 1300f);
                    return b.Finish(0.55f);
                case Sfx.Tnt:
                    b = new Buf(0.75f);
                    Mix(b, NoiseBurst(0.75f, 0.2f, 3800f, 160f), 1f);
                    b.Add(0f, 0.75f, t => MathF.Sin(Glide(100f, 32f, 0.12f, t)) * Dec(t, 0.24f) * 1.1f);
                    for (int i = 0; i < b.D.Length; i++) b.D[i] = MathF.Tanh(b.D[i] * 2.2f);
                    return b.Finish(0.85f);
                case Sfx.Boulder:
                    b = new Buf(0.3f);
                    Mix(b, NoiseBurst(0.3f, 0.05f, 700f), 0.8f);
                    b.Add(0f, 0.3f, t => MathF.Sin(Glide(95f, 42f, 0.06f, t)) * Dec(t, 0.09f));
                    return b.Finish(0.8f);
                case Sfx.Lava:
                    b = new Buf(0.32f);
                    b.Add(0f, 0.32f, t => b.Noise() * (0.6f + 0.4f * Sin(31f, t)) * MathF.Min(1f, t / 0.02f) * Dec(t, 0.12f));
                    HighPass(b.D, 1800f);
                    return b.Finish(0.4f);
                case Sfx.SealHit:
                    b = new Buf(0.16f);
                    Mix(b, NoiseBurst(0.16f, 0.008f, 6000f, -1f, 1000f), 0.6f);
                    b.Add(0f, 0.16f, t => (Sin(310f, t) + Sin(472f, t) * 0.8f + Sin(705f, t) * 0.6f + Sin(1130f, t) * 0.5f) * Dec(t, 0.04f) * 0.3f);
                    return b.Finish(0.5f);
                case Sfx.SealBreak:
                    b = new Buf(1.5f);
                    Mix(b, NoiseBurst(1.2f, 0.32f, 4200f, 140f), 1f);
                    b.Add(0f, 1.2f, t => MathF.Sin(Glide(90f, 28f, 0.2f, t)) * Dec(t, 0.38f) * 1.2f);
                    for (int i = 0; i < b.D.Length; i++) b.D[i] = MathF.Tanh(b.D[i] * 2f) * 0.8f;
                    float[] sh = { 783.99f, 987.77f, 1174.66f, 1567.98f };
                    for (int i = 0; i < 4; i++) { float f = sh[i]; b.Add(0.22f + i * 0.085f, 0.9f, t => (Tri(f, t) * 0.6f + Sin(f * 2f, t) * 0.25f) * Dec(t, 0.32f) * 0.45f); }
                    return b.Finish(0.9f);
                case Sfx.LowBattery:
                    b = new Buf(0.16f);
                    b.Add(0f, 0.16f, t => Sqr(880f, t) * Dec(t, 0.07f));
                    LowPass(b.D, 2600f);
                    return b.Finish(0.35f);
                case Sfx.PowerDown:
                    b = new Buf(0.85f);
                    b.Add(0f, 0.85f, t => Saw(1f, Glide(300f, 36f, 0.28f, t) / Tau) * (1f - t / 0.85f));
                    LowPass(b.D, 1900f, 220f);
                    return b.Finish(0.6f);
                case Sfx.Win:
                    b = new Buf(1.15f);
                    float[] w = { 523.25f, 659.25f, 783.99f, 1046.5f };
                    for (int i = 0; i < 4; i++) { float f = w[i]; b.Add(i * 0.09f, 0.3f, t => (Tri(f, t) * 0.7f + Sin(f * 2f, t) * 0.3f) * Dec(t, 0.11f)); }
                    for (int i = 0; i < 4; i++) { float f = w[i]; b.Add(0.4f, 0.75f, t => (Tri(f, t) * 0.6f + Sin(f * 2f, t) * 0.2f) * Dec(t, 0.3f) * 0.6f); }
                    return b.Finish(0.6f);
                case Sfx.Tap:
                    b = new Buf(0.06f);
                    b.Add(0f, 0.06f, t => Sin(1250f, t) * Dec(t, 0.012f) + b.Noise() * Dec(t, 0.004f) * 0.3f);
                    return b.Finish(0.4f);
                case Sfx.Buy:
                    b = new Buf(0.42f);
                    Mix(b, NoiseBurst(0.06f, 0.012f, 9000f, -1f, 3000f), 0.5f);
                    b.Add(0.03f, 0.3f, t => (Sin(1567.98f, t) + 0.4f * Sin(1567.98f * 2.76f, t)) * Dec(t, 0.09f));
                    b.Add(0.10f, 0.32f, t => (Sin(2093f, t) + 0.4f * Sin(2093f * 2.76f, t)) * Dec(t, 0.12f));
                    return b.Finish(0.5f);
                case Sfx.CoinTick:
                    b = new Buf(0.04f);
                    b.Add(0f, 0.04f, t => Sin(2100f, t) * Dec(t, 0.008f));
                    return b.Finish(0.3f);
            }
            return new float[64];
        }

        /// <summary>Boucle d'une seconde, raccordée sans couture : grondement du foret.</summary>
        public static float[] BuildDrillLoop()
        {
            var b = new Buf(1f, true);
            float[] noise = new float[Rate];
            for (int i = 0; i < Rate; i++) noise[i] = b.Noise();
            HighPass(noise, 260f);
            LowPass(noise, 2300f);
            int fade = Rate / 16;
            for (int i = 0; i < fade; i++)
            {
                float k = i / (float)fade;
                noise[i] = noise[i] * k + noise[Rate - fade + i] * (1f - k);
            }
            b.Add(0f, 1f, t => 0f);
            for (int i = 0; i < Rate - fade; i++)
            {
                float t = i / (float)Rate;
                float tt = t * (Rate / (float)(Rate - fade));
                float am = 0.55f + 0.45f * Sin(24f, tt);
                b.D[i] = noise[i] * am * 0.9f;
            }
            // La partie raccordée est étirée sur toute la seconde pour que la boucle reste périodique.
            float[] loop = new float[Rate];
            for (int i = 0; i < Rate; i++)
            {
                float src = i * ((Rate - fade) / (float)Rate);
                int i0 = (int)src;
                int i1 = Math.Min(i0 + 1, Rate - fade - 1);
                float fr = src - i0;
                float t = i / (float)Rate;
                loop[i] = b.D[i0] * (1f - fr) + b.D[i1] * fr + Saw(55f, t) * 0.35f + Sin(110f, t) * 0.25f + Sin(220f, t) * 0.06f;
            }
            LowPass(loop, 3200f);
            float m = 0.0001f;
            for (int i = 0; i < Rate; i++) m = MathF.Max(m, MathF.Abs(loop[i]));
            for (int i = 0; i < Rate; i++) loop[i] *= 0.5f / m;
            return loop;
        }

        /// <summary>Boucle musicale de huit mesures (la mineur pentatonique), raccordée sans couture.</summary>
        public static float[] BuildMusicLoop()
        {
            const float beat = 0.6f, eighth = beat * 0.5f, bar = beat * 4f;
            var b = new Buf(bar * 8f, true);
            // Fondamentales de basse par mesure : la, la, fa, sol, la, la, do, sol.
            float[] roots = { 55f, 55f, 43.65f, 49f, 55f, 55f, 65.41f, 49f };
            float[][] chords =
            {
                new[] { 440f, 523.25f, 659.25f, 523.25f }, new[] { 440f, 523.25f, 659.25f, 783.99f },
                new[] { 349.23f, 440f, 523.25f, 440f }, new[] { 392f, 493.88f, 587.33f, 493.88f },
                new[] { 440f, 523.25f, 659.25f, 523.25f }, new[] { 659.25f, 523.25f, 440f, 523.25f },
                new[] { 523.25f, 659.25f, 783.99f, 659.25f }, new[] { 392f, 493.88f, 587.33f, 783.99f },
            };
            bool[] bassOn = { true, false, true, true, false, true, true, false };
            for (int m = 0; m < 8; m++)
            {
                float t0 = m * bar;
                for (int e = 0; e < 8; e++)
                {
                    float ts = t0 + e * eighth;
                    if (bassOn[e])
                    {
                        float f = roots[m] * (e == 3 || e == 6 ? 2f : 1f);
                        b.Add(ts, 0.3f, t => (Saw(f, t) * 0.5f + Sin(f, t) * 0.9f) * Dec(t, 0.14f) * 0.55f);
                    }
                    float af = chords[m][e % 4] * (e >= 4 && m % 2 == 1 ? 2f : 1f);
                    b.Add(ts, 0.34f, t => (Tri(af, t) * 0.7f + Sin(af * 2f, t) * 0.15f) * Dec(t, 0.12f) * 0.16f);
                    if (e % 2 == 1) b.Add(ts, 0.05f, t => b.Noise() * Dec(t, 0.012f) * 0.07f);
                }
                for (int k = 0; k < 4; k++)
                    b.Add(t0 + k * beat, 0.16f, t => MathF.Sin(Glide(120f, 44f, 0.035f, t)) * Dec(t, 0.06f) * (k % 2 == 0 ? 0.6f : 0.35f));
            }
            LowPass(b.D, 5200f);
            return b.Finish(0.5f);
        }
    }
}
