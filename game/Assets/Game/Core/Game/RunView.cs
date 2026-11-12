using System;
using Game.Core.Engine;

namespace Game.Core
{
    /// <summary>Dessin de l'engin, réutilisé en partie, à l'accueil et au garage.</summary>
    public static class MachineArt
    {
        public static void Draw(Gfx g, Art art, float cx, float cy, int pods, float u, float time, float tilt, float load,
                                bool lights, int drillLevel, int engineLevel)
        {
            float width = pods * u * 0.96f, height = u * 0.92f;
            if (load > 0.7f) cx += MathF.Sin(time * 90f) * 0.7f;
            int frame = (int)(time * (9f + load * 16f)) % 3;
            int tf = (int)(time * (6f + load * 6f)) & 1;
            float k = u / 36f;

            if (lights)
            {
                g.SpriteXY(art.LightCone, cx, cy + height * 0.5f + 60f * k, (0.7f + pods * 0.24f) * k, k, 0f, Col.Glow(0xFFE9A8, 0.11f));
                g.Sprite(art.PGlow, cx, cy, (1.7f + pods * 0.55f) * k, 0f, Col.Glow(0xFFD98A, 0.10f));
            }

            // Forets, sous la carrosserie.
            float ds = k * 1.08f * MathF.Min(1.36f, 1f + drillLevel * 0.012f);
            for (int i = 0; i < pods; i++)
            {
                float px = cx + (i - (pods - 1) * 0.5f) * u;
                g.SpriteXY(art.Drill[(frame + i) % 3], px, cy + height * 0.5f - 4f * k + 13f * ds, ds, ds, tilt * -0.18f, Col.White);
            }

            // Chenilles.
            g.SpriteXY(art.Track[tf], cx - width * 0.5f - 1.5f * k, cy + 1f, k * 1.05f, k * 1.05f, 0f, Col.White);
            g.SpriteXY(art.Track[1 - tf], cx + width * 0.5f + 1.5f * k, cy + 1f, k * 1.05f, k * 1.05f, 0f, Col.White);

            // Échappements : un peu plus nombreux à chaque palier de moteur.
            int flames = Math.Min(4, 1 + engineLevel / 4);
            for (int i = 0; i < flames; i++)
            {
                float fx = cx + (flames == 1 ? 0f : (i / (float)(flames - 1) - 0.5f) * (width - 14f * k));
                float fy = 0.55f + 0.5f * load + 0.12f * MathF.Sin(time * 37f + i * 2f);
                g.SpriteXY(art.Flame, fx, cy - height * 0.5f - 7f * k * fy, k * 0.8f, k * fy, 0f, Col.White);
                if (lights) g.Sprite(art.PGlow, fx, cy - height * 0.5f - 6f * k, 0.5f * k, 0f, Col.Glow(0xFF9A3A, 0.25f));
            }

            g.Panel(art.Body, cx - width * 0.5f, cy - height * 0.5f, width, height);
            for (int i = 0; i < pods; i++)
            {
                float px = cx + (i - (pods - 1) * 0.5f) * u;
                g.SpriteXY(art.Stripes, px, cy + height * 0.27f, k, k, 0f, Col.White);
            }

            // Cabine et « yeux » qui regardent où va l'engin.
            float cabX = cx + tilt * 1.5f, cabY = cy - height * 0.12f;
            g.SpriteXY(art.Cab, cabX, cabY, k * 1.05f, k * 1.05f, 0f, Col.White);
            float ex = tilt * 1.9f * k, ey = 1.0f * k;
            uint pupil = Col.Hex(0x101B36);
            g.Sprite(art.PDot, cabX - 5.2f * k + ex, cabY + 0.5f * k + ey, 0.52f * k, 0f, pupil);
            g.Sprite(art.PDot, cabX + 5.2f * k + ex, cabY + 0.5f * k + ey, 0.52f * k, 0f, pupil);
        }
    }

    /// <summary>Affichage d'une descente : décor, terrain, portails, sceaux, objets, engin, jauges.</summary>
    public sealed class RunView
    {
        public const float ViewW = 390f;
        public static readonly float Cell = ViewW / World.Cols;

        public float CamY;
        private bool _snap = true;

        public void Reset() { _snap = true; }

        public void Update(Run run, float dt, float viewH)
        {
            float rows = viewH / Cell;
            float target = run.Y - rows * 0.31f;
            if (_snap) { CamY = target; _snap = false; }
            else CamY = Mathx.Damp(CamY, target, 9f, dt);
        }

        private static int Variant(int c, int r) { return (int)((Noise.Hash(c, r, 7u) >> 5) % 3u); }

        public void Draw(Gfx g, Art art, Run run, Fx fx, float viewH, float time)
        {
            World w = run.W;
            float cell = Cell;
            int midRow = (int)(CamY + viewH / cell * 0.5f);
            LayerDef bg = w.DefAt(Math.Max(0, midRow));
            g.RectGrad(0f, 0f, ViewW, viewH, Col.Hex(bg.Bg0), Col.Hex(bg.Bg1));

            // Lueurs lointaines : donnent de la profondeur sans gêner la lecture.
            for (int i = 0; i < 4; i++)
            {
                float py = (i * 310f - CamY * cell * 0.35f) % 1240f;
                if (py < 0f) py += 1240f;
                g.Sprite(art.PGlow, 60f + (i * 137) % 280, py - 200f, 3.4f, 0f, Col.Glow(bg.Soft, 0.045f));
            }

            fx.ShakeOffset(time, out float sx, out float sy);
            g.Push(sx, sy - CamY * cell);

            int r0 = Math.Max(0, (int)MathF.Floor(CamY) - 1), r1 = Math.Min(w.Rows - 1, (int)MathF.Ceiling(CamY + viewH / cell) + 1);
            uint socleTint = Col.Hex(0x8A93B5);

            // Premier passage : les blocs.
            for (int r = r0; r <= r1; r++)
            {
                LayerDef d = w.DefAt(r);
                byte[] types = w.Types[r];
                for (int c = 0; c < World.Cols; c++)
                {
                    CellType t = (CellType)types[c];
                    if (t == CellType.Empty || t == CellType.Gate) continue;
                    float x = c * cell, y = r * cell;
                    int v = Variant(c, r);
                    bool cracks = false;
                    switch (t)
                    {
                        case CellType.Soft: g.Image(art.Soft[v], x, y, cell, cell, Col.Hex(d.Soft)); break;
                        case CellType.Medium: g.Image(art.Med[v], x, y, cell, cell, Col.Hex(d.Med)); cracks = true; break;
                        case CellType.Hard: g.Image(art.Hard[v], x, y, cell, cell, Col.Hex(d.Hard)); cracks = true; break;
                        case CellType.Socle: g.Image(art.Socle, x, y, cell, cell, socleTint); cracks = true; break;
                        case CellType.Basalt: g.Image(art.Med[v], x, y, cell, cell, Col.Hex(0x4A4458)); cracks = true; break;
                        case CellType.Ore:
                            {
                                int host = w.Aux[r][c];
                                Sprite hs = host == 2 ? art.Hard[v] : (host == 0 ? art.Soft[v] : art.Med[v]);
                                uint hc = host == 2 ? d.Hard : (host == 0 ? d.Soft : d.Med);
                                g.Image(hs, x, y, cell, cell, Col.Hex(Col.Shade(hc, -0.22f)));
                                g.Image(art.Gems, x, y, cell, cell, Col.Hex(d.Ore));
                                cracks = true;
                                break;
                            }
                        case CellType.Crystal:
                            g.Image(art.Soft[v], x, y, cell, cell, Col.Hex(Col.Shade(d.Soft, -0.5f)));
                            g.Image(art.Crystal, x, y, cell, cell, Col.White);
                            break;
                        case CellType.Tnt: g.Image(art.Tnt, x, y, cell, cell, Col.White); break;
                        case CellType.Boulder: g.Image(art.Boulder, x, y, cell, cell, Col.White); cracks = true; break;
                        case CellType.Chest:
                            g.Image(art.Hard[v], x, y, cell, cell, Col.Hex(Col.Shade(d.Hard, -0.2f)));
                            g.Image(art.Chest, x, y, cell, cell, Col.White);
                            break;
                        case CellType.Lava:
                            {
                                float pulse = 1f + 0.04f * MathF.Sin(time * 5f + c * 1.7f + r);
                                g.Sprite(art.Lava, x + cell * 0.5f, y + cell * 0.5f, cell / Art.TileSize * pulse, 0f, Col.White);
                                break;
                            }
                        case CellType.Seal: g.Image(art.Seal, x, y, cell + 0.5f, cell + 0.5f, Col.White); break;
                    }
                    if (cracks)
                    {
                        float max = w.MaxHp(c, r);
                        float f = max > 0f ? w.Hp[r][c] / max : 1f;
                        if (f < 0.78f) g.Image(art.Crack[f < 0.28f ? 2 : (f < 0.55f ? 1 : 0)], x, y, cell, cell, Col.White);
                    }
                }
            }

            // Second passage : les lueurs (par-dessus tous les blocs).
            for (int r = r0; r <= r1; r++)
            {
                LayerDef d = w.DefAt(r);
                byte[] types = w.Types[r];
                for (int c = 0; c < World.Cols; c++)
                {
                    CellType t = (CellType)types[c];
                    float x = (c + 0.5f) * cell, y = (r + 0.5f) * cell;
                    float tw = 0.5f + 0.5f * MathF.Sin(time * 3.1f + c * 2.3f + r * 1.1f);
                    if (t == CellType.Ore) g.Sprite(art.PGlow, x, y, 0.62f + 0.1f * tw, 0f, Col.Glow(d.Ore, 0.26f + 0.12f * tw));
                    else if (t == CellType.Crystal) g.Sprite(art.PGlow, x, y, 0.8f, 0f, Col.Glow(0x4BE3FF, 0.3f + 0.15f * tw));
                    else if (t == CellType.Lava) g.Sprite(art.PGlow, x, y, 1.0f, 0f, Col.Glow(0xFF7A1A, 0.34f));
                    else if (t == CellType.Chest) g.Sprite(art.PStar, x + 8f, y - 9f, 0.5f + 0.3f * tw, time * 1.5f, Col.Glow(0xFFF1A8, 0.9f));
                    else if (t == CellType.Tnt) g.Sprite(art.PGlow, x, y, 0.55f, 0f, Col.Glow(0xFF4A3A, 0.10f + 0.10f * tw));
                }
            }

            DrawGates(g, art, w, r0, r1, time);
            DrawSeals(g, art, w, r0, r1, run, time);

            // Objets à ramasser.
            for (int i = 0; i < run.Pickups.Count; i++)
            {
                Pickup p = run.Pickups[i];
                float x = p.X * cell, y = p.Y * cell;
                if (p.Kind == 0) g.Sprite(art.Nugget, x, y, 1f, p.Age * 6f + i, Col.Hex(Balance.Layers[p.Layer].Ore));
                else if (p.Kind == 1) { g.Sprite(art.PGlow, x, y, 0.42f, 0f, Col.Glow(0x4BE3FF, 0.45f)); g.Sprite(art.Orb, x, y, 0.9f, 0f, Col.White); }
                else g.Sprite(art.Coin, x, y, 0.85f, 0f, Col.White);
            }

            MachineArt.Draw(g, art, run.X * cell, run.Y * cell, run.Pods, cell, time, run.Tilt, run.Over ? 0f : run.Load, true, run.DrillLevel, run.EngineLevel);
            fx.Draw(g, art, cell);
            fx.DrawTexts(g, cell);
            g.Pop();

            // Obscurité des profondeurs, puis éclairs colorés des événements.
            int layer = run.CurrentLayer;
            g.Image(art.Vignette, 0f, 0f, ViewW, viewH, Col.Hex(0x000000, 0.35f + 0.03f * layer));
            if (run.Energy < Balance.LowEnergy && !run.Over && !run.Finished)
            {
                float a = 0.25f + 0.2f * MathF.Sin(time * 9f);
                g.Image(art.Vignette, 0f, 0f, ViewW, viewH, Col.Hex(0xFF2B3D, a));
            }
            if (fx.Flash > 0f) g.Rect(0f, 0f, ViewW, viewH, Col.Glow(fx.FlashRgb, fx.Flash * 0.22f));
        }

        private static void DrawGates(Gfx g, Art art, World w, int r0, int r1, float time)
        {
            float cell = Cell;
            for (int i = 0; i < w.Gates.Count; i++)
            {
                GateBand band = w.Gates[i];
                if (band.Taken || band.Row + 2 < r0 || band.Row > r1) continue;
                for (int side = 0; side < 2; side++)
                {
                    GateFx fx = side == 0 ? band.A : band.B;
                    bool good = Balance.IsBonus(fx);
                    uint rgb = good ? 0x39B6FFu : 0xFF4D5Eu;
                    float x = side == 0 ? 2f : (World.Cols / 2 + 1) * cell + 1f;
                    float wdt = (World.Cols / 2) * cell - 3f, y = band.Row * cell + 1f, h = 2f * cell - 2f;
                    float pulse = 0.5f + 0.5f * MathF.Sin(time * 4f + side * 1.5f);
                    g.Panel(art.GatePanel, x, y, wdt, h, Col.Hex(rgb, 0.92f));
                    g.Sprite(art.PGlow, x + wdt * 0.5f, y + h * 0.5f, 2.2f, 0f, Col.Glow(rgb, 0.10f + 0.08f * pulse));
                    float cx = x + wdt * 0.5f;
                    Sprite arrow = good ? art.ArrowUp : art.ArrowDown;
                    g.Sprite(arrow, cx, y + h * 0.30f + (good ? -1.5f : 1.5f) * pulse, 0.95f, 0f, Col.White);
                    string label = Loc.Get("gate." + fx);
                    float size = 16.5f;
                    float tw = g.Measure(label, size);
                    if (tw > wdt - 14f) size *= (wdt - 14f) / tw;
                    g.TextOutlined(label, cx, y + h * 0.70f, size, Col.White, Col.Hex(Col.Shade(rgb, -0.72f)), 1.6f);
                }
            }
        }

        private static void DrawSeals(Gfx g, Art art, World w, int r0, int r1, Run run, float time)
        {
            float cell = Cell;
            for (int i = 0; i < w.Seals.Count; i++)
            {
                SealState s = w.Seals[i];
                if (s.Broken || s.Row + Balance.SealRows < r0 || s.Row > r1) continue;
                float y = s.Row * cell;
                // Bande de sécurité au-dessus de la dalle.
                for (float x = 0f; x < ViewW; x += 26f * 1.2f)
                    g.SpriteXY(art.Stripes, x + 15.6f, y - 3.6f, 1.2f, 1.2f, 0f, Col.White);
                float frac = s.Hp / s.MaxHp;
                bool hit = run.OnSeal;
                if (hit) g.Rect(0f, y, ViewW, Balance.SealRows * cell, Col.Glow(0xFFFFFF, 0.10f + 0.08f * MathF.Sin(time * 60f)));
                float bw = ViewW - 90f, bx = 45f, by = y + cell - 9f;
                g.Panel(art.PanelSm, bx - 3f, by - 3f, bw + 6f, 24f, Col.Hex(0x0B0D22, 0.9f));
                g.Panel(art.PanelSm, bx, by, MathF.Max(16f, bw * frac), 18f, Col.Hex(frac > 0.5f ? 0xFFC61Au : (frac > 0.2f ? 0xFF9A3Au : 0xFF4D5Eu)));
                string label = Loc.Get("hud.seal") + "  " + Fmt.Short(MathF.Ceiling(s.Hp), Loc.French);
                g.TextOutlined(label, ViewW * 0.5f, by + 9f, 14f, Col.White, Col.Hex(0x0B0D22), 1.4f);
            }
        }

        /// <summary>Jauges et messages en surimpression.</summary>
        public void DrawHud(Gfx g, Art art, Run run, Fx fx, float viewH, float safeTop, float time, bool started, bool pausePressed)
        {
            float top = safeTop + 10f;
            // Batterie.
            float bx = 14f, bw = ViewW - 14f - 58f, bh = 24f;
            float frac = Mathx.Clamp01(run.Energy / run.EnergyMax);
            bool low = run.Energy < Balance.LowEnergy && !run.Finished;
            uint barRgb = low ? 0xFF4D5Eu : (frac < 0.4f ? 0xFFC61Au : 0x35D6FFu);
            if (run.Energy > run.EnergyMax) barRgb = 0x7CFFB2u;
            float grow = fx.EnergyPulse * 2.5f;
            g.Panel(art.PanelSm, bx - 3f, top - 3f - grow, bw + 6f, bh + 6f + grow * 2f, Col.Hex(0x0B0D22, 0.88f));
            float fw = MathF.Max(18f, bw * frac);
            float blink = low ? 0.7f + 0.3f * MathF.Sin(time * 12f) : 1f;
            g.Panel(art.PanelSm, bx, top - grow, fw, bh + grow * 2f, Col.Hex(barRgb, blink));
            g.Rect(bx + 6f, top + 3f - grow, MathF.Max(0f, fw - 12f), 3.5f, Col.Hex(0xFFFFFF, 0.35f));
            g.Sprite(art.I(Icon.Bolt), bx + 14f, top + bh * 0.5f, 0.72f, 0f, Col.Hex(0x0B0D22));
            string secs = ((int)MathF.Ceiling(MathF.Max(0f, run.Energy))).ToString() + " " + Loc.Get("unit.s");
            g.TextOutlined(secs, bx + bw - 8f, top + bh * 0.5f, 15f, Col.White, Col.Hex(0x0B0D22), 1.4f, Align.Right);

            // Bouton pause.
            float pr = pausePressed ? 0.9f : 1f;
            g.Sprite(art.Circle, ViewW - 28f, top + bh * 0.5f, 0.78f * pr, 0f, Col.Hex(0x0B0D22, 0.88f));
            g.Sprite(art.I(Icon.Pause), ViewW - 28f, top + bh * 0.5f, 0.8f * pr, 0f, Col.White);

            // Progression vers le sceau.
            SealState seal = run.W.NextSeal(run.Y);
            float py = top + bh + 9f;
            if (seal != null)
            {
                int layerStart = seal.Row - Balance.SealApproachRows - Balance.Layers[seal.Layer].Length;
                float prog = Mathx.Clamp01((run.Y - layerStart) / (seal.Row - layerStart));
                g.Panel(art.PanelSm, bx, py, bw, 6f, Col.Hex(0x0B0D22, 0.8f));
                g.Panel(art.PanelSm, bx, py, MathF.Max(6f, bw * prog), 6f, Col.Hex(0xFFC61A));
                g.Sprite(art.I(Icon.Lock), bx + bw + 9f, py + 3f, 0.55f, 0f, Col.Hex(0xFFC61A));
            }

            // Pièces et profondeur.
            float row = py + 26f;
            float cp = 1f + fx.CoinPulse * 0.18f;
            g.Sprite(art.Coin, bx + 10f, row, 1.25f * cp, 0f, Col.White);
            g.TextOutlined(Fmt.Short(run.Coins, Loc.French), bx + 24f, row, 19f * cp, Col.Hex(0xFFD23F), Col.Hex(0x0B0D22), 1.8f, Align.Left);
            g.TextOutlined(run.DepthMeters + " " + Loc.Get("unit.m"), ViewW - 14f, row, 21f, Col.White, Col.Hex(0x0B0D22), 1.8f, Align.Right);

            // Bonus actifs.
            float chipX = bx, chipY = row + 26f;
            chipX = Chip(g, art, Icon.Drill, run.PowerMul, chipX, chipY);
            chipX = Chip(g, art, Icon.Gear, run.SpeedMul, chipX, chipY);
            chipX = Chip(g, art, Icon.Magnet, run.MagnetMul, chipX, chipY);
            Chip(g, art, Icon.Gem, run.LootMul, chipX, chipY);

            // Bandeau de couche franchie.
            if (fx.BannerT < 2.4f && fx.Banner.Length > 0)
            {
                float t = fx.BannerT;
                float s = Mathx.EaseOutBack(MathF.Min(1f, t * 3.5f));
                float a = t > 1.9f ? 1f - (t - 1.9f) / 0.5f : 1f;
                float cy = viewH * 0.27f;
                float size = 30f;
                float tw = g.Measure(fx.Banner, size);
                if (tw > ViewW - 30f) size *= (ViewW - 30f) / tw;
                g.TextOutlined(fx.Banner, ViewW * 0.5f, cy, size * s, Col.Hex(0xFFD23F, a), Col.Hex(0x0B0D22, a), 2.6f);
                if (fx.BannerSub.Length > 0)
                    g.TextOutlined(fx.BannerSub, ViewW * 0.5f, cy + 34f, 20f * s, Col.Hex(0x7CFFB2, a), Col.Hex(0x0B0D22, a), 2f);
            }

            if (low && !run.Over)
                g.TextOutlined(Loc.Get("hud.low"), ViewW * 0.5f, top + bh + 86f, 15f, Col.Hex(0xFF8A94, 0.6f + 0.4f * MathF.Sin(time * 12f)), Col.Hex(0x0B0D22), 1.6f);

            // Avant le premier toucher : la main qui montre le geste.
            if (!started)
            {
                float hy = viewH * 0.74f;
                float hx = ViewW * 0.5f + MathF.Sin(time * 2.6f) * 78f;
                g.Panel(art.PanelSm, ViewW * 0.5f - 96f, hy - 9f, 192f, 18f, Col.Hex(0xFFFFFF, 0.10f));
                g.Sprite(art.Touch, hx, hy, 1f, 0f, Col.Hex(0xFFFFFF, 0.95f));
                g.TextOutlined(Loc.Get("hint.drag"), ViewW * 0.5f, hy + 46f, 20f, Col.White, Col.Hex(0x0B0D22), 2f);
            }
        }

        private static float Chip(Gfx g, Art art, Icon icon, float mul, float x, float y)
        {
            if (MathF.Abs(mul - 1f) < 0.01f) return x;
            bool good = mul > 1f;
            string s = "×" + (MathF.Round(mul * 10f) / 10f).ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (Loc.French) s = s.Replace('.', ',');
            float w = 30f + g.Measure(s, 13f);
            g.Panel(art.PanelSm, x, y - 11f, w, 22f, Col.Hex(good ? 0x12406Au : 0x6A1220u, 0.9f));
            g.Sprite(art.I(icon), x + 12f, y, 0.62f, 0f, Col.White);
            g.TextMid(s, x + 23f, y, 13f, Col.White, Align.Left);
            return x + w + 5f;
        }
    }
}
