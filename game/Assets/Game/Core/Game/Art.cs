using System;
using Game.Core.Engine;

namespace Game.Core
{
    public enum Icon
    {
        Bolt, Drill, Gear, Battery, Width, Magnet, Gem, Pause, Close, Play, Home, Sound, Music, Vibrate,
        Globe, Copy, Trophy, Calendar, Lock, Check, ChevL, ChevR, Ad, Shake, Reset, Coin, Count
    }

    /// <summary>
    /// Toutes les images du jeu, dessinées par le code au démarrage (aucun fichier image).
    /// Les images « blanches » sont teintées à l'affichage : une seule image sert à toutes les couches.
    /// </summary>
    public sealed class Art
    {
        public const float TileSize = 36f;
        public const string Charset =
            " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_abcdefghijklmnopqrstuvwxyz{|}~" +
            "àâäçéèêëîïôöùûüÿœÀÂÄÇÉÈÊËÎÏÔÖÙÛÜŒ’«»–—…×÷·°€";
        public const string CharsetLarge = Charset;

        public Sprite[] Soft = new Sprite[3], Med = new Sprite[3], Hard = new Sprite[3], Crack = new Sprite[3];
        public Sprite Socle, Gems, Crystal, Tnt, Boulder, Lava, Chest, Seal, GatePanel, ArrowUp, ArrowDown;
        public Sprite Body, Cab, Stripes, Flame, LightCone;
        public Sprite[] Track = new Sprite[2], Drill = new Sprite[3];
        public Sprite Nugget, Orb, Coin;
        public Sprite PShard, PDot, PSpark, PSmoke, PGlow, PRing, PStar;
        public Sprite Panel, PanelSm, PanelLine, Circle, Vignette, Touch;
        public Sprite[] Icons = new Sprite[(int)Icon.Count];

        private float _s;
        private Atlas _atlas;

        public Sprite I(Icon id) { return Icons[(int)id]; }

        private Canvas New(float w, float h) { return Canvas.Points(w, h, _s); }

        public void Build(Atlas atlas, TrueTypeFont font)
        {
            _atlas = atlas; _s = atlas.Scale;
            var rng = new Rng(20261112);

            for (int v = 0; v < 3; v++) { Soft[v] = Tile(0, ref rng); Med[v] = Tile(1, ref rng); Hard[v] = Tile(2, ref rng); }
            Socle = BuildSocle();
            for (int v = 0; v < 3; v++) Crack[v] = BuildCrack(v, ref rng);
            Gems = BuildGems();
            Crystal = BuildCrystal();
            Tnt = BuildTnt(font);
            Boulder = BuildBoulder();
            Lava = BuildLava(ref rng);
            Chest = BuildChest();
            Seal = BuildSeal();
            BuildGate();
            BuildMachine();
            BuildPickups();
            BuildParticles();
            BuildUi();
            BuildIcons();
        }

        // ------------------------------------------------------------------ terrain

        private Sprite Tile(int kind, ref Rng rng)
        {
            const float T = TileSize, m = 0.8f, w = T - 2f * m;
            Canvas c = New(T, T);
            c.Color(0xD2D2D2).RoundRect(m, m, w, w, 6.5f);
            c.Clip(m, m, w, w, 6.5f);
            c.Color(0xFFFFFF).RoundRect(m, m, w, 6.5f, 0f);
            c.Color(0x9C9C9C).RoundRect(m, T - m - 8f, w, 8f, 0f);
            if (kind == 0)
            {
                for (int i = 0; i < 5; i++)
                    c.Color(0xB4B4B4, 0.9f).Circle(rng.Range(7f, T - 7f), rng.Range(10f, T - 12f), rng.Range(1.1f, 2.2f));
            }
            else if (kind == 1)
            {
                for (int i = 0; i < 3; i++)
                {
                    float x = rng.Range(8f, T - 10f), y = rng.Range(11f, T - 13f);
                    c.Color(0xB0B0B0).Ellipse(x, y, rng.Range(3.5f, 5.5f), rng.Range(2.4f, 3.6f));
                    c.Color(0xE9E9E9).Ellipse(x - 0.6f, y - 0.9f, 2.2f, 1.1f);
                }
                c.Color(0x8E8E8E, 0.9f).Line(rng.Range(6f, 14f), 9f, rng.Range(15f, 22f), rng.Range(15f, 20f), 1.3f);
            }
            else
            {
                float k = rng.Range(-4f, 4f);
                c.Color(0xECECEC).Poly(m, m + 6.5f, T * 0.55f + k, m + 6.5f, m, T * 0.62f);
                c.Color(0xA6A6A6).Poly(T - m, T - m - 8f, T * 0.45f + k, T - m - 8f, T - m, T * 0.38f);
                c.Color(0x7E7E7E).Line(T * 0.3f, T * 0.72f, T * 0.7f + k, T * 0.3f, 1.4f);
                c.Color(0xF4F4F4).Circle(8f, 12f, 1.5f).Circle(T - 8f, T - 13f, 1.5f);
            }
            c.NoClip();
            c.Color(0x000000, 0.30f).RoundRectStroke(m + 0.7f, m + 0.7f, w - 1.4f, w - 1.4f, 5.8f, 1.4f);
            return _atlas.Add(c);
        }

        private Sprite BuildSocle()
        {
            const float T = TileSize, m = 0.5f, w = T - 2f * m;
            Canvas c = New(T, T);
            c.Color(0xB9B9B9).RoundRect(m, m, w, w, 4f);
            c.Clip(m, m, w, w, 4f);
            c.Color(0xE2E2E2).RoundRect(m, m, w, 5f, 0f);
            c.Color(0x868686).RoundRect(m, T - 7f, w, 7f, 0f);
            c.Color(0x7A7A7A).Line(7f, 7f, T - 7f, T - 7f, 3f).Line(T - 7f, 7f, 7f, T - 7f, 3f);
            c.Color(0xD8D8D8).Line(7f, 6.2f, T - 7f, T - 7.8f, 0.9f);
            c.NoClip();
            foreach (float x in new[] { 6.5f, T - 6.5f })
                foreach (float y in new[] { 6.5f, T - 6.5f })
                    c.Color(0x6E6E6E).Circle(x, y, 2.6f).Color(0xF0F0F0).Circle(x - 0.5f, y - 0.6f, 1.2f);
            c.Color(0x000000, 0.40f).RoundRectStroke(m + 0.7f, m + 0.7f, w - 1.4f, w - 1.4f, 3.4f, 1.4f);
            return _atlas.Add(c);
        }

        private Sprite BuildCrack(int stage, ref Rng rng)
        {
            const float T = TileSize;
            Canvas c = New(T, T);
            c.Color(0x000000, 0.62f);
            int lines = stage + 1;
            for (int l = 0; l < lines; l++)
            {
                float x = rng.Range(9f, T - 9f), y = 5f;
                for (int i = 0; i < 4 + stage; i++)
                {
                    float nx = Mathx.Clamp(x + rng.Range(-7f, 7f), 5f, T - 5f), ny = y + rng.Range(4f, 8f);
                    if (ny > T - 5f) break;
                    c.Line(x, y, nx, ny, 1.5f + stage * 0.25f);
                    x = nx; y = ny;
                }
            }
            return _atlas.Add(c);
        }

        private void Diamond(Canvas c, float cx, float cy, float r)
        {
            c.Color(0x000000, 0.35f).PolyRound(1.2f, cx, cy - r, cx + r * 0.8f, cy, cx, cy + r, cx - r * 0.8f, cy);
            c.Color(0xDADADA).Poly(cx, cy - r, cx + r * 0.8f, cy, cx, cy + r, cx - r * 0.8f, cy);
            c.Color(0xFFFFFF).Poly(cx, cy - r, cx, cy, cx - r * 0.8f, cy);
            c.Color(0xA4A4A4).Poly(cx, cy + r, cx + r * 0.8f, cy, cx, cy);
            c.Color(0xFFFFFF).Circle(cx - r * 0.25f, cy - r * 0.35f, r * 0.14f);
        }

        private Sprite BuildGems()
        {
            Canvas c = New(TileSize, TileSize);
            Diamond(c, 12f, 13.5f, 6.8f);
            Diamond(c, 24f, 18.5f, 8.2f);
            Diamond(c, 14.5f, 26.5f, 5.6f);
            return _atlas.Add(c);
        }

        private Sprite BuildCrystal()
        {
            const float T = TileSize;
            Canvas c = New(T, T);
            Action<float, float, float, float> shard = (x, top, w, tilt) =>
            {
                float b = T - 4f;
                c.Color(0x06586F).PolyRound(1.3f, x - w, b, x - w + tilt, top + w, x + tilt, top, x + w + tilt, top + w, x + w, b);
                c.GradV(0xC8FBFF, 0x18BFEA, top, b).Poly(x - w, b, x - w + tilt, top + w, x + tilt, top, x + w + tilt, top + w, x + w, b);
                c.Color(0xFFFFFF, 0.85f).Poly(x - w + tilt, top + w, x + tilt, top, x + tilt * 0.6f, b - 2f, x - w, b);
                c.Color(0x18BFEA, 0.35f).Poly(x + tilt, top, x + w + tilt, top + w, x + w, b, x + tilt * 0.6f, b - 2f);
            };
            shard(10f, 12f, 4.2f, -2f);
            shard(26f, 9f, 4.6f, 2.2f);
            shard(18f, 4f, 5.4f, 0f);
            return _atlas.Add(c);
        }

        private Sprite BuildTnt(TrueTypeFont font)
        {
            const float T = TileSize, m = 1.2f, w = T - 2f * m;
            Canvas c = New(T, T);
            c.Color(0xE8433F).RoundRect(m, m, w, w, 6f);
            c.Clip(m, m, w, w, 6f);
            c.Color(0xFF7A70).RoundRect(m, m, w, 5f, 0f);
            c.Color(0xA81F2C).RoundRect(m, T - 8f, w, 8f, 0f);
            c.Color(0xFFF1C9).RoundRect(m, 11f, w, 14f, 0f);
            c.Color(0xA81F2C).RoundRect(m, 10f, w, 1.6f, 0f).RoundRect(m, 24.4f, w, 1.6f, 0f);
            c.NoClip();
            c.Color(0x7A0E1B).Text(font, "TNT", T * 0.5f, 22.4f, 12.5f);
            c.Color(0x000000, 0.35f).RoundRectStroke(m + 0.7f, m + 0.7f, w - 1.4f, w - 1.4f, 5.3f, 1.4f);
            return _atlas.Add(c);
        }

        private Sprite BuildBoulder()
        {
            const float T = TileSize;
            Canvas c = New(T, T);
            float[] p = { 9f, 5f, 24f, 3.5f, 32f, 11f, 33f, 23f, 26f, 32f, 12f, 33f, 3.5f, 25f, 3f, 12f };
            c.Color(0x3A342E).PolyRound(3.2f, p);
            c.Color(0x9A8F82).PolyRound(1.6f, p);
            c.Clip(2f, 2f, T - 4f, T - 4f, 12f);
            c.Color(0xC2B8AB).Ellipse(13f, 11f, 9f, 5.5f);
            c.Color(0x74695E).Ellipse(22f, 29f, 12f, 5.5f);
            c.NoClip();
            c.Color(0x4A423A).Line(18f, 12f, 22f, 19f, 1.5f).Line(22f, 19f, 19f, 25f, 1.5f).Line(10f, 20f, 14f, 23f, 1.3f);
            return _atlas.Add(c);
        }

        private Sprite BuildLava(ref Rng rng)
        {
            const float T = TileSize;
            Canvas c = New(T, T);
            c.GradV(0xFFE45C, 0xFF6A14, 2f, T - 2f).RoundRect(0.5f, 0.5f, T - 1f, T - 1f, 9f);
            c.Clip(0.5f, 0.5f, T - 1f, T - 1f, 9f);
            for (int i = 0; i < 5; i++)
                c.Color(0xFFF7B0, 0.9f).Ellipse(rng.Range(6f, T - 6f), rng.Range(6f, T - 8f), rng.Range(2.5f, 5f), rng.Range(1.5f, 2.6f));
            for (int i = 0; i < 4; i++)
                c.Color(0xD9480F, 0.85f).Ellipse(rng.Range(6f, T - 6f), rng.Range(10f, T - 5f), rng.Range(2f, 4f), rng.Range(1.3f, 2.2f));
            c.NoClip();
            return _atlas.Add(c);
        }

        private Sprite BuildChest()
        {
            const float T = TileSize;
            Canvas c = New(T, T);
            c.Color(0x3B2410).RoundRect(3f, 6f, T - 6f, T - 10f, 5f);
            c.Color(0x9A6330).RoundRect(4.5f, 7.5f, T - 9f, T - 13f, 4f);
            c.Clip(4.5f, 7.5f, T - 9f, T - 13f, 4f);
            c.Color(0xB87A3E).RoundRect(4.5f, 7.5f, T - 9f, 8f, 0f);
            c.Color(0x3B2410).RoundRect(4.5f, 16f, T - 9f, 1.8f, 0f);
            c.Color(0xFFC83D).RoundRect(9f, 7.5f, 3.6f, T, 0f).RoundRect(T - 12.6f, 7.5f, 3.6f, T, 0f);
            c.NoClip();
            c.Color(0x3B2410).RoundRect(14.5f, 13.5f, 7f, 8.5f, 2f);
            c.Color(0xFFE08A).RoundRect(15.6f, 14.6f, 4.8f, 6.3f, 1.4f);
            c.Color(0x3B2410).Circle(18f, 17.2f, 1.2f);
            return _atlas.Add(c);
        }

        private Sprite BuildSeal()
        {
            const float T = TileSize;
            Canvas c = New(T, T);
            c.GradV(0x6C7CA0, 0x39445F, 0f, T).RoundRect(-2f, -2f, T + 4f, T + 4f, 0f);
            c.Color(0x2A3248).RoundRect(-2f, T - 3f, T + 4f, 5f, 0f).RoundRect(-2f, -2f, 3f, T + 4f, 0f);
            c.Color(0x8FA0C8).RoundRect(1f, 0f, T, 2.2f, 0f);
            foreach (float x in new[] { 7f, T - 6f })
                foreach (float y in new[] { 8f, T - 9f })
                    c.Color(0x242B3D).Circle(x, y, 3f).Color(0xC3CEE8).Circle(x - 0.5f, y - 0.6f, 1.5f);
            c.Color(0x2A3248, 0.8f).RoundRectStroke(11f, 11f, T - 21f, T - 22f, 3f, 1.6f);
            return _atlas.Add(c);
        }

        private void BuildGate()
        {
            Canvas c = New(28f, 28f);
            c.Color(0xFFFFFF, 0.36f).RoundRect(1f, 1f, 26f, 26f, 8f);
            c.Color(0xFFFFFF, 0.95f).RoundRectStroke(2f, 2f, 24f, 24f, 7f, 2.2f);
            GatePanel = _atlas.Add(c);
            for (int k = 0; k < 2; k++)
            {
                c = New(20f, 20f);
                float d = k == 0 ? 1f : -1f, cy = 10f;
                c.Color(0xFFFFFF).PolyRound(1.2f, 10f, cy - 8f * d, 18f, cy + 1f * d, 13f, cy + 1f * d, 13f, cy + 8f * d, 7f, cy + 8f * d, 7f, cy + 1f * d, 2f, cy + 1f * d);
                if (k == 0) ArrowUp = _atlas.Add(c); else ArrowDown = _atlas.Add(c);
            }
        }

        // ------------------------------------------------------------------ engin

        private void BuildMachine()
        {
            // Carrosserie extensible (les coins gardent leur forme quand l'engin s'élargit).
            Canvas c = New(30f, 30f);
            c.Color(0x33240A).RoundRect(0f, 0f, 30f, 30f, 10.5f);
            c.Color(0xFFC61A).RoundRect(1.7f, 1.7f, 26.6f, 26.6f, 9f);
            c.Clip(1.7f, 1.7f, 26.6f, 26.6f, 9f);
            c.Color(0xFFE680).RoundRect(1.7f, 1.7f, 26.6f, 6f, 0f);
            c.Color(0xE39A00).RoundRect(1.7f, 21f, 26.6f, 8f, 0f);
            c.NoClip();
            Body = _atlas.Add(c);

            // Cabine : vitre sombre et deux phares qui servent d'yeux.
            c = New(26f, 16f);
            c.Color(0x33240A).RoundRect(0f, 0f, 26f, 16f, 6f);
            c.GradV(0x243B6B, 0x101B36, 1f, 15f).RoundRect(1.6f, 1.6f, 22.8f, 12.8f, 4.6f);
            c.Color(0x5F86D6, 0.55f).Poly(4f, 2.5f, 11f, 2.5f, 6.5f, 8f, 3f, 8f);
            c.Color(0xFFFFFF).Circle(8f, 8.4f, 4.3f).Circle(18f, 8.4f, 4.3f);
            Cab = _atlas.Add(c);

            // Bandes de sécurité jaunes et noires.
            c = New(26f, 6f);
            c.Color(0x1B1B1B).RoundRect(0f, 0f, 26f, 6f, 2f);
            c.Clip(0.6f, 0.6f, 24.8f, 4.8f, 1.6f);
            for (int i = -1; i < 6; i++)
                c.Color(0xFFD23F).Poly(i * 6f, 6f, i * 6f + 3f, 6f, i * 6f + 7f, 0f, i * 6f + 4f, 0f);
            c.NoClip();
            Stripes = _atlas.Add(c);

            // Chenilles (deux images pour l'animation).
            for (int f = 0; f < 2; f++)
            {
                c = New(10f, 32f);
                c.Color(0x14161C).RoundRect(0f, 0f, 10f, 32f, 4.5f);
                c.Color(0x3C4150).RoundRect(1.4f, 1.4f, 7.2f, 29.2f, 3.4f);
                c.Clip(1.4f, 1.4f, 7.2f, 29.2f, 3.4f);
                for (int i = -1; i < 8; i++)
                    c.Color(0x7B8499).RoundRect(1.4f, i * 5f + f * 2.5f, 7.2f, 2f, 0.8f);
                c.NoClip();
                Track[f] = _atlas.Add(c);
            }

            // Foret : cône d'acier à spirale (trois images pour la rotation).
            for (int f = 0; f < 3; f++)
            {
                c = New(26f, 26f);
                c.Color(0x1C2130).PolyRound(1.6f, 3f, 4f, 23f, 4f, 13f, 24f);
                c.Color(0xD5DCEA).Poly(3.6f, 4.6f, 22.4f, 4.6f, 13f, 22.6f);
                c.Clip(0f, 4.6f, 26f, 22f, 0f);
                for (int i = -2; i < 6; i++)
                {
                    float y = i * 6f + f * 2f;
                    c.Color(0x8C97B2).Poly(0f, y + 3f, 26f, y - 3f, 26f, y, 0f, y + 6f);
                }
                // Efface ce qui dépasse du cône.
                c.NoClip();
                c.Eraser().Poly(0f, 4.6f, 3.2f, 4.6f, 13f, 23.4f, 13f, 26f, 0f, 26f);
                c.Eraser().Poly(26f, 4.6f, 22.8f, 4.6f, 13f, 23.4f, 13f, 26f, 26f, 26f);
                c.Color(0xFFFFFF, 0.7f).Line(8.5f, 7f, 12f, 14.5f, 1.3f);
                c.Color(0x1C2130).RoundRect(1.5f, 0.5f, 23f, 5.6f, 2.2f);
                c.Color(0x5D6883).RoundRect(2.7f, 1.6f, 20.6f, 3.2f, 1.4f);
                Drill[f] = _atlas.Add(c);
            }

            c = New(14f, 22f);
            c.GradV(0xFFF3A6, 0xFF7A1A, 2f, 20f, 1f, 0.9f).PolyRound(2.5f, 7f, 20f, 3f, 9f, 5f, 4f, 7f, 2.5f, 9f, 4f, 11f, 9f);
            Flame = _atlas.Add(c);

            c = New(120f, 130f);
            c.GradV(0xFFFFFF, 0xFFFFFF, 0f, 126f, 0.55f, 0f).Poly(48f, 0f, 72f, 0f, 118f, 128f, 2f, 128f);
            LightCone = _atlas.Add(c);
        }

        // ------------------------------------------------------------------ objets ramassés

        private void BuildPickups()
        {
            Canvas c = New(12f, 12f);
            float[] p = { 6f, 0.8f, 11f, 4.6f, 9.2f, 10.8f, 2.8f, 10.8f, 1f, 4.6f };
            c.Color(0x000000, 0.4f).PolyRound(0.8f, p);
            c.Color(0xDCDCDC).Poly(p);
            c.Color(0xFFFFFF).Poly(6f, 0.8f, 6f, 6f, 1f, 4.6f);
            c.Color(0xA2A2A2).Poly(9.2f, 10.8f, 2.8f, 10.8f, 6f, 6f);
            Nugget = _atlas.Add(c);

            c = New(16f, 16f);
            c.Color(0x06586F).Circle(8f, 8f, 7.6f);
            c.GradV(0xB9FAFF, 0x12B5E3, 1f, 15f).Circle(8f, 8f, 6.2f);
            c.Color(0xFFFFFF).Poly(9.4f, 2.4f, 4.6f, 8.8f, 7.6f, 8.8f, 6.4f, 13.6f, 11.4f, 6.8f, 8.4f, 6.8f);
            Orb = _atlas.Add(c);

            c = New(16f, 16f);
            c.Color(0x7A4A00).Circle(8f, 8f, 7.7f);
            c.Color(0xFFC61A).Circle(8f, 8f, 6.4f);
            c.Color(0xFFE680).Circle(8f, 8f, 4.4f);
            c.Color(0xE39A00).CircleStroke(8f, 8f, 4.4f, 1f);
            c.Color(0xFFFFFF, 0.9f).Circle(6f, 5.6f, 1.3f);
            Coin = _atlas.Add(c);
        }

        private void BuildParticles()
        {
            Canvas c = New(8f, 8f);
            c.Color(0xFFFFFF).RoundRect(0.5f, 0.5f, 7f, 7f, 1.8f);
            PShard = _atlas.Add(c);

            c = New(8f, 8f);
            c.Color(0xFFFFFF).Circle(4f, 4f, 3.6f);
            PDot = _atlas.Add(c);

            c = New(5f, 16f);
            c.Color(0xFFFFFF).Poly(2.5f, 0f, 5f, 8f, 2.5f, 16f, 0f, 8f);
            PSpark = _atlas.Add(c);

            c = New(28f, 28f);
            c.GradR(0xFFFFFF, 0xFFFFFF, 14f, 14f, 13.5f, 0.55f, 0f).Circle(14f, 14f, 13.5f);
            PSmoke = _atlas.Add(c);

            c = New(72f, 72f);
            c.GradR(0xFFFFFF, 0xFFFFFF, 36f, 36f, 35f, 1f, 0f).Circle(36f, 36f, 35f);
            // Second passage : cœur plus intense, bords très doux.
            c.GradR(0xFFFFFF, 0xFFFFFF, 36f, 36f, 14f, 0.6f, 0f).Circle(36f, 36f, 14f);
            PGlow = _atlas.Add(c);

            c = New(64f, 64f);
            c.Color(0xFFFFFF).CircleStroke(32f, 32f, 28f, 4f);
            PRing = _atlas.Add(c);

            c = New(18f, 18f);
            c.Color(0xFFFFFF).Poly(9f, 0f, 11f, 7f, 18f, 9f, 11f, 11f, 9f, 18f, 7f, 11f, 0f, 9f, 7f, 7f);
            PStar = _atlas.Add(c);
        }

        // ------------------------------------------------------------------ interface

        private void BuildUi()
        {
            Canvas c = New(36f, 36f);
            c.Color(0xFFFFFF).RoundRect(0f, 0f, 36f, 36f, 16f);
            Panel = _atlas.Add(c);

            c = New(20f, 20f);
            c.Color(0xFFFFFF).RoundRect(0f, 0f, 20f, 20f, 8f);
            PanelSm = _atlas.Add(c);

            c = New(36f, 36f);
            c.Color(0xFFFFFF).RoundRectStroke(1.5f, 1.5f, 33f, 33f, 14.5f, 2.4f);
            PanelLine = _atlas.Add(c);

            c = New(48f, 48f);
            c.Color(0xFFFFFF).Circle(24f, 24f, 23.5f);
            Circle = _atlas.Add(c);

            c = New(96f, 96f);
            c.GradR(0x000000, 0x000000, 48f, 48f, 70f, 0f, 0.75f).RoundRect(0f, 0f, 96f, 96f, 0f);
            Vignette = _atlas.Add(c);

            c = New(44f, 44f);
            c.Color(0xFFFFFF, 0.35f).Circle(22f, 22f, 21f);
            c.Color(0xFFFFFF).CircleStroke(22f, 22f, 19f, 2.6f);
            c.Color(0xFFFFFF).Circle(22f, 22f, 9f);
            Touch = _atlas.Add(c);
        }

        private void BuildIcons()
        {
            const uint W = 0xFFFFFF;
            Canvas c;

            c = New(24f, 24f);
            c.Color(W).PolyRound(0.8f, 14.5f, 1.5f, 5f, 13.5f, 10.6f, 13.5f, 8.6f, 22.5f, 19f, 9.6f, 13.2f, 9.6f);
            Icons[(int)Icon.Bolt] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).RoundRect(3f, 2f, 18f, 5f, 1.8f).PolyRound(0.8f, 5f, 8.5f, 19f, 8.5f, 12f, 22f);
            c.Eraser().Line(6f, 13.5f, 18f, 10.5f, 1.6f).Line(8.5f, 18f, 15.5f, 15.6f, 1.5f);
            Icons[(int)Icon.Drill] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathx.Tau / 8f;
                c.Line(12f + MathF.Cos(a) * 6f, 12f + MathF.Sin(a) * 6f, 12f + MathF.Cos(a) * 9.6f, 12f + MathF.Sin(a) * 9.6f, 4.2f);
            }
            c.Circle(12f, 12f, 7.4f);
            c.Eraser().Circle(12f, 12f, 3.3f);
            Icons[(int)Icon.Gear] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).RoundRectStroke(2.5f, 6.5f, 17f, 11f, 3f, 2.2f).RoundRect(20.5f, 9.5f, 2.5f, 5f, 1f);
            c.RoundRect(5.3f, 9.3f, 3.2f, 5.4f, 0.8f).RoundRect(9.4f, 9.3f, 3.2f, 5.4f, 0.8f).RoundRect(13.5f, 9.3f, 3.2f, 5.4f, 0.8f);
            Icons[(int)Icon.Battery] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).Line(4f, 12f, 20f, 12f, 2.6f).Line(4f, 12f, 8.5f, 7.5f, 2.6f).Line(4f, 12f, 8.5f, 16.5f, 2.6f)
                .Line(20f, 12f, 15.5f, 7.5f, 2.6f).Line(20f, 12f, 15.5f, 16.5f, 2.6f);
            Icons[(int)Icon.Width] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).CircleStroke(12f, 12.5f, 6.2f, 4.6f);
            c.Eraser().RoundRect(0f, 0f, 24f, 12.5f, 0f);
            c.Color(W).RoundRect(3.5f, 3f, 4.6f, 10f, 1f).RoundRect(15.9f, 3f, 4.6f, 10f, 1f);
            c.Eraser().RoundRect(3f, 6.4f, 5.6f, 1.6f, 0f).RoundRect(15.4f, 6.4f, 5.6f, 1.6f, 0f);
            Icons[(int)Icon.Magnet] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).PolyRound(0.8f, 6.5f, 3.5f, 17.5f, 3.5f, 22f, 9.5f, 12f, 21.5f, 2f, 9.5f);
            c.Eraser().Line(2.5f, 9.5f, 21.5f, 9.5f, 1.4f).Line(8.5f, 9.5f, 12f, 20.5f, 1.3f).Line(15.5f, 9.5f, 12f, 20.5f, 1.3f);
            Icons[(int)Icon.Gem] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).RoundRect(5f, 4f, 5f, 16f, 1.8f).RoundRect(14f, 4f, 5f, 16f, 1.8f);
            Icons[(int)Icon.Pause] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).Line(5.5f, 5.5f, 18.5f, 18.5f, 3.4f).Line(18.5f, 5.5f, 5.5f, 18.5f, 3.4f);
            Icons[(int)Icon.Close] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).PolyRound(1.8f, 7.5f, 5f, 19f, 12f, 7.5f, 19f);
            Icons[(int)Icon.Play] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).PolyRound(1.2f, 12f, 3f, 21.5f, 11.5f, 18.5f, 11.5f, 18.5f, 20.5f, 5.5f, 20.5f, 5.5f, 11.5f, 2.5f, 11.5f);
            c.Eraser().RoundRect(10f, 14f, 4f, 7f, 1f);
            Icons[(int)Icon.Home] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).CircleStroke(11f, 12f, 6.6f, 2f).CircleStroke(11f, 12f, 10.2f, 2f);
            c.Eraser().RoundRect(0f, 0f, 13.5f, 24f, 0f).Poly(13.5f, 12f, 24f, 0f, 13.5f, 0f).Poly(13.5f, 12f, 24f, 24f, 13.5f, 24f);
            c.Color(W).PolyRound(1f, 3f, 9f, 7f, 9f, 12f, 4.5f, 12f, 19.5f, 7f, 15f, 3f, 15f);
            Icons[(int)Icon.Sound] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).Ellipse(7.5f, 18f, 4.3f, 3.3f).Ellipse(17.5f, 15.5f, 4.3f, 3.3f)
                .Line(10.6f, 18f, 10.6f, 6f, 2.2f).Line(20.6f, 15.5f, 20.6f, 3.6f, 2.2f).Line(10.6f, 6.4f, 20.6f, 4f, 3.4f);
            Icons[(int)Icon.Music] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).RoundRectStroke(8f, 3.5f, 8f, 17f, 2.2f, 2.2f)
                .Line(4.6f, 8f, 4.6f, 16f, 2f).Line(19.4f, 8f, 19.4f, 16f, 2f).Line(1.6f, 10f, 1.6f, 14f, 1.8f).Line(22.4f, 10f, 22.4f, 14f, 1.8f);
            Icons[(int)Icon.Vibrate] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).CircleStroke(12f, 12f, 9f, 2f).Line(3f, 12f, 21f, 12f, 1.8f);
            c.Color(W).Line(12f, 3f, 12f, 21f, 1.8f);
            c.Color(W).CircleStroke(12f, 12f, 9f, 2f);
            c.Color(W).Line(7.2f, 5.2f, 6f, 12f, 1.7f).Line(6f, 12f, 7.2f, 18.8f, 1.7f).Line(16.8f, 5.2f, 18f, 12f, 1.7f).Line(18f, 12f, 16.8f, 18.8f, 1.7f);
            Icons[(int)Icon.Globe] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).RoundRectStroke(8f, 7.5f, 12f, 13.5f, 2.4f, 2.2f);
            c.Color(W).Line(5f, 16.5f, 5f, 5.4f, 2.2f).Line(5f, 4.4f, 14f, 4.4f, 2.2f);
            Icons[(int)Icon.Copy] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).PolyRound(1f, 6.5f, 3.5f, 17.5f, 3.5f, 16.5f, 11.5f, 13.5f, 14.5f, 10.5f, 14.5f, 7.5f, 11.5f)
                .RoundRect(10.8f, 14f, 2.4f, 4.5f, 0.5f).RoundRect(7f, 18f, 10f, 3f, 1.2f)
                .CircleStroke(5.5f, 7.5f, 2.8f, 1.6f).CircleStroke(18.5f, 7.5f, 2.8f, 1.6f);
            Icons[(int)Icon.Trophy] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).RoundRectStroke(3.5f, 5.5f, 17f, 15f, 3f, 2.2f).RoundRect(3.5f, 5.5f, 17f, 5f, 2f)
                .Line(8f, 3f, 8f, 7f, 2.2f).Line(16f, 3f, 16f, 7f, 2.2f)
                .Circle(8.5f, 14f, 1.3f).Circle(12f, 14f, 1.3f).Circle(15.5f, 14f, 1.3f).Circle(8.5f, 17.4f, 1.3f).Circle(12f, 17.4f, 1.3f);
            Icons[(int)Icon.Calendar] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).CircleStroke(12f, 9f, 4.6f, 2.4f).RoundRect(5f, 10.5f, 14f, 10.5f, 2.6f);
            c.Eraser().Circle(12f, 15f, 1.7f).RoundRect(11.2f, 15f, 1.6f, 3.4f, 0.6f);
            Icons[(int)Icon.Lock] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).Line(4.5f, 12.5f, 9.8f, 17.8f, 3.6f).Line(9.8f, 17.8f, 19.5f, 6.5f, 3.6f);
            Icons[(int)Icon.Check] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).Line(15f, 4.5f, 8f, 12f, 3.4f).Line(8f, 12f, 15f, 19.5f, 3.4f);
            Icons[(int)Icon.ChevL] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).Line(9f, 4.5f, 16f, 12f, 3.4f).Line(16f, 12f, 9f, 19.5f, 3.4f);
            Icons[(int)Icon.ChevR] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).RoundRectStroke(2.5f, 5f, 19f, 14f, 3.4f, 2.2f).PolyRound(0.8f, 9.6f, 8.4f, 16f, 12f, 9.6f, 15.6f);
            Icons[(int)Icon.Ad] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).Line(3f, 8f, 7.5f, 5f, 2.2f).Line(7.5f, 5f, 12f, 8f, 2.2f).Line(12f, 8f, 16.5f, 5f, 2.2f).Line(16.5f, 5f, 21f, 8f, 2.2f)
                .Line(3f, 14f, 7.5f, 11f, 2.2f).Line(7.5f, 11f, 12f, 14f, 2.2f).Line(12f, 14f, 16.5f, 11f, 2.2f).Line(16.5f, 11f, 21f, 14f, 2.2f)
                .Line(3f, 20f, 7.5f, 17f, 2.2f).Line(7.5f, 17f, 12f, 20f, 2.2f).Line(12f, 20f, 16.5f, 17f, 2.2f).Line(16.5f, 17f, 21f, 20f, 2.2f);
            Icons[(int)Icon.Shake] = _atlas.Add(c);

            c = New(24f, 24f);
            c.Color(W).CircleStroke(12f, 12.5f, 7.4f, 2.4f);
            c.Eraser().Poly(12f, 12.5f, 3f, 2f, 12f, 0f);
            c.Color(W).PolyRound(0.6f, 12.4f, 1.6f, 12.4f, 9f, 7f, 5.3f);
            Icons[(int)Icon.Reset] = _atlas.Add(c);

            Icons[(int)Icon.Coin] = Coin;
        }
    }
}
