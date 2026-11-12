using System;

namespace Game.Core.Engine
{
    /// <summary>Petites fonctions mathématiques partagées (aucune dépendance à Unity).</summary>
    public static class Mathx
    {
        public const float Tau = 6.2831853f;

        public static float Clamp(float v, float a, float b) { return v < a ? a : (v > b ? b : v); }
        public static int Clamp(int v, int a, int b) { return v < a ? a : (v > b ? b : v); }
        public static float Clamp01(float v) { return v < 0f ? 0f : (v > 1f ? 1f : v); }
        public static float Lerp(float a, float b, float t) { return a + (b - a) * t; }
        public static float InvLerp(float a, float b, float v) { return a == b ? 0f : Clamp01((v - a) / (b - a)); }
        public static float Smooth(float t) { t = Clamp01(t); return t * t * (3f - 2f * t); }

        /// <summary>Rapproche cur de target d'au plus maxDelta.</summary>
        public static float Approach(float cur, float target, float maxDelta)
        {
            if (cur < target) return MathF.Min(cur + maxDelta, target);
            return MathF.Max(cur - maxDelta, target);
        }

        /// <summary>Lissage exponentiel indépendant du nombre d'images par seconde.</summary>
        public static float Damp(float cur, float target, float lambda, float dt)
        {
            return Lerp(cur, target, 1f - MathF.Exp(-lambda * dt));
        }

        public static float EaseOutCubic(float t) { t = 1f - Clamp01(t); return 1f - t * t * t; }
        public static float EaseInCubic(float t) { t = Clamp01(t); return t * t * t; }

        public static float EaseOutBack(float t)
        {
            t = Clamp01(t) - 1f;
            const float s = 1.70158f;
            return t * t * ((s + 1f) * t + s) + 1f;
        }

        public static float EaseOutElastic(float t)
        {
            t = Clamp01(t);
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            return MathF.Pow(2f, -10f * t) * MathF.Sin((t * 10f - 0.75f) * (Tau / 3f)) + 1f;
        }
    }

    /// <summary>Générateur pseudo-aléatoire déterministe (même graine, même terrain sur tous les appareils).</summary>
    public struct Rng
    {
        private ulong _s;

        public Rng(ulong seed)
        {
            _s = seed * 0x9E3779B97F4A7C15UL + 0xD1B54A32D192ED03UL;
            if (_s == 0) _s = 0x1234567890ABCDEFUL;
            NextU(); NextU();
        }

        public uint NextU()
        {
            // xorshift64*
            _s ^= _s >> 12;
            _s ^= _s << 25;
            _s ^= _s >> 27;
            return (uint)((_s * 0x2545F4914F6CDD1DUL) >> 32);
        }

        /// <summary>Nombre dans [0, 1[.</summary>
        public float Next01() { return (NextU() >> 8) * (1f / 16777216f); }
        public float Range(float a, float b) { return a + (b - a) * Next01(); }
        /// <summary>Entier dans [a, b[.</summary>
        public int Range(int a, int b) { return b <= a ? a : a + (int)(NextU() % (uint)(b - a)); }
        public bool Chance(float p) { return Next01() < p; }
    }

    /// <summary>Bruit de valeur 2D déterministe, utilisé pour répartir matériaux et filons.</summary>
    public static class Noise
    {
        public static uint Hash(int x, int y, uint seed)
        {
            uint h = seed ^ 0x9E3779B9u;
            h ^= (uint)x * 0x85EBCA6Bu; h = (h << 13) | (h >> 19); h *= 0xC2B2AE35u;
            h ^= (uint)y * 0x27D4EB2Fu; h = (h << 15) | (h >> 17); h *= 0x165667B1u;
            h ^= h >> 15; h *= 0x2C1B3C6Du; h ^= h >> 12; h *= 0x297A2D39u; h ^= h >> 15;
            return h;
        }

        public static float Hash01(int x, int y, uint seed) { return (Hash(x, y, seed) >> 8) * (1f / 16777216f); }

        public static float Value(float x, float y, uint seed)
        {
            int xi = (int)MathF.Floor(x), yi = (int)MathF.Floor(y);
            float fx = x - xi, fy = y - yi;
            float u = fx * fx * (3f - 2f * fx), v = fy * fy * (3f - 2f * fy);
            float a = Hash01(xi, yi, seed), b = Hash01(xi + 1, yi, seed);
            float c = Hash01(xi, yi + 1, seed), d = Hash01(xi + 1, yi + 1, seed);
            return Mathx.Lerp(Mathx.Lerp(a, b, u), Mathx.Lerp(c, d, u), v);
        }

        public static float Fbm(float x, float y, uint seed, int octaves)
        {
            float sum = 0f, amp = 0.5f, norm = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum += Value(x, y, seed + (uint)i * 7919u) * amp;
                norm += amp;
                x *= 2.03f; y *= 2.03f; amp *= 0.5f;
            }
            return sum / norm;
        }
    }

    /// <summary>
    /// Couleurs compactées sur 32 bits, en alpha prémultiplié (octets R, V, B, A en mémoire).
    /// Une couleur d'alpha nul mais de teinte non nulle s'ajoute au fond (effet lumineux).
    /// </summary>
    public static class Col
    {
        public const uint White = 0xFFFFFFFFu;
        public const uint Black = 0xFF000000u;
        public const uint Clear = 0u;

        private static uint B(float v) { int i = (int)(v * 255f + 0.5f); return (uint)(i < 0 ? 0 : (i > 255 ? 255 : i)); }

        /// <summary>Couleur à partir de composantes non prémultipliées dans [0, 1].</summary>
        public static uint Rgba(float r, float g, float b, float a = 1f)
        {
            return B(r * a) | (B(g * a) << 8) | (B(b * a) << 16) | (B(a) << 24);
        }

        /// <summary>Couleur à partir d'un code 0xRRVVBB et d'une opacité.</summary>
        public static uint Hex(uint rgb, float a = 1f)
        {
            return Rgba(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, a);
        }

        /// <summary>Couleur additive (lumière) d'intensité k.</summary>
        public static uint Glow(uint rgb, float k = 1f)
        {
            return B(((rgb >> 16) & 255) / 255f * k) | (B(((rgb >> 8) & 255) / 255f * k) << 8) | (B((rgb & 255) / 255f * k) << 16);
        }

        /// <summary>Multiplie les quatre canaux (fondu d'une couleur prémultipliée).</summary>
        public static uint Fade(uint c, float k)
        {
            if (k >= 1f) return c;
            if (k <= 0f) return 0u;
            uint r = (uint)((c & 255) * k), g = (uint)(((c >> 8) & 255) * k);
            uint b = (uint)(((c >> 16) & 255) * k), a = (uint)(((c >> 24) & 255) * k);
            return r | (g << 8) | (b << 16) | (a << 24);
        }

        public static uint Mix(uint c0, uint c1, float t)
        {
            t = Mathx.Clamp01(t);
            uint r = (uint)((c0 & 255) + (((int)(c1 & 255)) - (int)(c0 & 255)) * t);
            uint g = (uint)(((c0 >> 8) & 255) + ((int)((c1 >> 8) & 255) - (int)((c0 >> 8) & 255)) * t);
            uint b = (uint)(((c0 >> 16) & 255) + ((int)((c1 >> 16) & 255) - (int)((c0 >> 16) & 255)) * t);
            uint a = (uint)(((c0 >> 24) & 255) + ((int)((c1 >> 24) & 255) - (int)((c0 >> 24) & 255)) * t);
            return r | (g << 8) | (b << 16) | (a << 24);
        }

        /// <summary>Éclaircit (k &gt; 0) ou assombrit (k &lt; 0) un code 0xRRVVBB.</summary>
        public static uint Shade(uint rgb, float k)
        {
            float r = ((rgb >> 16) & 255) / 255f, g = ((rgb >> 8) & 255) / 255f, b = (rgb & 255) / 255f;
            if (k >= 0f) { r += (1f - r) * k; g += (1f - g) * k; b += (1f - b) * k; }
            else { r *= 1f + k; g *= 1f + k; b *= 1f + k; }
            return (B(r) << 16) | (B(g) << 8) | B(b);
        }
    }
}
