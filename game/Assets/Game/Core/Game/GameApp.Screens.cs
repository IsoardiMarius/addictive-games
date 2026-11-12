using System;
using System.Globalization;
using Game.Core.Engine;

namespace Game.Core
{
    /// <summary>Écrans hors partie : accueil, garage, fin de descente, réglages, et les surimpressions.</summary>
    public sealed partial class GameApp
    {
        private const uint Navy0 = 0x1A2050, Navy1 = 0x0B0D22, PanelRgb = 0x1F2450, Muted = 0x9AA4D8;
        private const uint Yellow = 0xFFC61A, YellowInk = 0x33240A, Green = 0x35C97F, Red = 0xF0505A, Indigo = 0x4254D6, Gray = 0x4A5078, Violet = 0x8A5BFF;

        private readonly float[] _buyPulse = new float[(int)Upgrade.Count];

        /// <summary>Zone cliquable affichée à l'image courante (sert aux tests automatiques de l'interface).</summary>
        public struct UiHit { public string Id; public float X, Y; public bool Enabled; }
        public readonly System.Collections.Generic.List<UiHit> Hits = new System.Collections.Generic.List<UiHit>();

        private void Hit(string id, float x, float y, bool enabled = true)
        {
            UiHit h; h.Id = id; h.X = x; h.Y = y; h.Enabled = enabled;
            Hits.Add(h);
        }

        // ------------------------------------------------------------------ briques d'interface

        private static bool In(float px, float py, float x, float y, float w, float h)
        {
            return px >= x && px <= x + w && py >= y && py <= y + h;
        }

        private float Appear(float delay) { return Mathx.EaseOutBack(Mathx.Clamp01((_screenT - delay) * 4.5f)); }

        private string Dec(float v)
        {
            string s = (MathF.Round(v * 10f) / 10f).ToString("0.#", CultureInfo.InvariantCulture);
            return Loc.French ? s.Replace('.', ',') : s;
        }

        /// <summary>Bouton à relief. Renvoie vrai au relâchement du doigt sur le bouton.</summary>
        private bool Btn(float x, float y, float w, float h, uint rgb, string label, float size, uint ink,
                         bool withIcon = false, Icon icon = Icon.Play, bool enabled = true)
        {
            bool originIn = In(_pressX, _pressY, x, y, w, h), curIn = In(_px, _py, x, y, w, h);
            bool armed = enabled && _down && originIn && curIn;
            Hit(label, x + w * 0.5f, y + h * 0.5f, enabled);
            float off = armed ? 3.5f : 0f;
            uint face = enabled ? rgb : Gray;
            Sprite panel = h < 44f ? Art.PanelSm : Art.Panel;
            G.Panel(panel, x, y + 4.5f, w, h, Col.Hex(Col.Shade(face, -0.42f)));
            G.Panel(panel, x, y + off, w, h, Col.Hex(face));
            G.Panel(Art.PanelSm, x + 10f, y + off + 4f, w - 20f, 5f, Col.Hex(0xFFFFFF, 0.20f));
            uint inkCol = Col.Hex(enabled ? ink : 0xB9BFDD);
            float iconW = withIcon ? size * 1.15f : 0f;
            float maxText = w - 22f - iconW;
            float tw = G.Measure(label, size);
            if (tw > maxText) { size *= maxText / tw; tw = maxText; }
            float cx = x + w * 0.5f, cy = y + h * 0.5f + off - 0.5f;
            float startX = cx - (tw + iconW) * 0.5f;
            if (withIcon)
            {
                Sprite s = Art.I(icon);
                G.Sprite(s, startX + iconW * 0.42f, cy, size / 24f * 1.05f, 0f, icon == Icon.Coin ? Col.White : inkCol);
            }
            G.TextMid(label, startX + iconW, cy, size, inkCol, Align.Left);
            bool clicked = enabled && _released && originIn && curIn;
            if (clicked) { Audio.Play(Sfx.Tap, 0.8f); Haptic(0); }
            return clicked;
        }

        private bool IconBtn(float cx, float cy, Icon icon, uint rgb = PanelRgb, float radius = 22f)
        {
            float x = cx - radius, y = cy - radius, d = radius * 2f;
            bool originIn = In(_pressX, _pressY, x - 6f, y - 6f, d + 12f, d + 12f), curIn = In(_px, _py, x - 6f, y - 6f, d + 12f, d + 12f);
            bool armed = _down && originIn && curIn;
            Hit("icon:" + icon, cx, cy);
            float k = armed ? 0.9f : 1f;
            G.Sprite(Art.Circle, cx, cy + 2.5f, d / 48f * k, 0f, Col.Hex(Col.Shade(rgb, -0.45f)));
            G.Sprite(Art.Circle, cx, cy, d / 48f * k, 0f, Col.Hex(rgb));
            G.Sprite(Art.I(icon), cx, cy, radius / 22f * 0.95f * k, 0f, Col.White);
            bool clicked = _released && originIn && curIn;
            if (clicked) { Audio.Play(Sfx.Tap, 0.8f); Haptic(0); }
            return clicked;
        }

        private void CoinPill(float x, float y, bool rightAligned = false)
        {
            string s = Fmt.Short(P.Coins, Loc.French);
            float w = 46f + G.Measure(s, 18f);
            if (rightAligned) x -= w;
            G.Panel(Art.PanelSm, x, y, w, 34f, Col.Hex(0x0B0D22, 0.85f));
            G.Sprite(Art.Coin, x + 18f, y + 17f, 1.35f, 0f, Col.White);
            G.TextMid(s, x + 34f, y + 17f, 18f, Col.Hex(0xFFD23F), Align.Left);
        }

        private void Backdrop()
        {
            G.RectGrad(0f, 0f, ViewW, ViewH, Col.Hex(Navy0), Col.Hex(Navy1));
            G.Sprite(Art.PGlow, ViewW * 0.5f, ViewH * 0.28f, 7f, 0f, Col.Glow(0x3B4BD0, 0.10f));
        }

        private void Header(string title)
        {
            if (IconBtn(34f, SafeTop + 34f, Icon.ChevL)) Go(GameScreen.Home);
            G.TextMid(title, ViewW * 0.5f, SafeTop + 34f, 24f, Col.White);
            CoinPill(ViewW - 14f, SafeTop + 17f, true);
        }

        private void DrawToast()
        {
            if (_toastT > 2f || _toast.Length == 0) return;
            float a = _toastT > 1.6f ? 1f - (_toastT - 1.6f) / 0.4f : Mathx.Clamp01(_toastT * 8f);
            float w = G.Measure(_toast, 15f) + 36f;
            float y = ViewH - SafeBottom - 150f;
            G.Panel(Art.PanelSm, (ViewW - w) * 0.5f, y, w, 36f, Col.Hex(0x0B0D22, 0.94f * a));
            G.TextMid(_toast, ViewW * 0.5f, y + 18f, 15f, Col.Hex(0xFFFFFF, a));
        }

        // ------------------------------------------------------------------ accueil

        private void DrawHome()
        {
            float W = ViewW, H = ViewH, cell = RunView.Cell;
            Backdrop();
            int layer = Mathx.Clamp(P.SelectedLayer, 0, P.PlayableLayer);
            P.SelectedLayer = layer;
            LayerDef d = Balance.Layers[layer];

            // Le sol de la couche choisie, et l'engin posé dessus, prêt à forer.
            float groundTop = H - SafeBottom - cell * 3f;
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < World.Cols; c++)
                {
                    uint h = Noise.Hash(c, r + layer * 31, 99u);
                    bool med = r > 0 && (h % 5u) == 0u;
                    float x = c * cell, y = groundTop + r * cell;
                    G.Image(med ? Art.Med[h % 3u] : Art.Soft[h % 3u], x, y, cell, cell, Col.Hex(med ? d.Med : d.Soft));
                    if (r > 0 && (h % 11u) == 3u)
                    {
                        G.Image(Art.Gems, x, y, cell, cell, Col.Hex(d.Ore));
                        G.Sprite(Art.PGlow, x + cell * 0.5f, y + cell * 0.5f, 0.65f, 0f, Col.Glow(d.Ore, 0.3f));
                    }
                }
            G.RectGrad(0f, groundTop + cell * 1.5f, W, H - groundTop - cell * 1.5f, Col.Hex(Navy1, 0f), Col.Hex(Navy1, 0.85f));
            float my = groundTop - cell * 0.6f + MathF.Sin(_time * 2.4f) * 1.2f;
            MachineArt.Draw(G, Art, W * 0.5f, my, Balance.StartPods(P.Up[(int)Upgrade.Chassis]), cell, _time,
                            MathF.Sin(_time * 0.8f) * 0.6f, 0.1f, true, P.Up[(int)Upgrade.Drill], P.Up[(int)Upgrade.Engine]);

            // Barre du haut.
            CoinPill(14f, SafeTop + 14f);
            if (IconBtn(W - 34f, SafeTop + 31f, Icon.Gear)) Go(GameScreen.Settings);

            // Titre.
            float ly = SafeTop + 96f, s = Appear(0f);
            G.TextOutlined(Loc.Get("title.1"), W * 0.5f, ly, 40f * s, Col.White, Col.Hex(0x0B0D22), 3f);
            G.TextOutlined(Loc.Get("title.2"), W * 0.5f, ly + 52f, 64f * s, Col.Hex(Yellow), Col.Hex(0x0B0D22), 3.6f);
            G.TextMid(P.Finished ? Loc.Get("home.finished") : Loc.Get("tagline"), W * 0.5f, ly + 97f, 13.5f, Col.Hex(Muted));

            // Boutons.
            float garageY = groundTop - 129f, playY = garageY - 84f;
            float bw = 262f, bx = (W - bw) * 0.5f;

            // Carte de la couche.
            float regionTop = ly + 112f, regionBottom = playY - 12f;
            float ch = 104f, cw = 302f, cx = (W - cw) * 0.5f;
            float cy = regionTop + MathF.Max(0f, (regionBottom - regionTop - ch) * 0.5f);
            float a2 = Appear(0.08f);
            G.Panel(Art.Panel, cx, cy + 4f, cw, ch, Col.Hex(0x0B0D22, 0.6f));
            G.Panel(Art.Panel, cx, cy, cw, ch, Col.Hex(PanelRgb, 0.96f));
            G.Image(Art.Soft[0], cx + 16f, cy + 28f, 48f * a2, 48f * a2, Col.Hex(d.Soft));
            G.Image(Art.Gems, cx + 16f, cy + 28f, 48f * a2, 48f * a2, Col.Hex(d.Ore));
            G.Sprite(Art.PGlow, cx + 40f, cy + 52f, 0.9f, 0f, Col.Glow(d.Ore, 0.28f));
            G.Text(Loc.Get("layer") + " " + (layer + 1) + " / " + Balance.LayerCount, cx + 78f, cy + 28f, 13f, Col.Hex(Muted));
            G.Text(Loc.Get(d.Key).ToUpperInvariant(), cx + 78f, cy + 58f, 27f, Col.White);
            int d0 = Balance.LayerStartDepth(layer), d1 = d0 + Balance.LayerRows(layer);
            G.Text(Loc.Get("home.depth", d0, d1), cx + 78f, cy + 78f, 12.5f, Col.Hex(Muted));
            bool cleared = layer < P.MaxLayer;
            G.Sprite(Art.I(cleared ? Icon.Check : Icon.Lock), cx + 86f, cy + 91f, 0.5f, 0f, Col.Hex(cleared ? Green : Yellow));
            G.Text(Loc.Get(cleared ? "home.cleared" : "home.goal"), cx + 97f, cy + 95.5f, 12.5f, Col.Hex(cleared ? Green : Yellow));
            if (layer > 0 && IconBtn(22f, cy + ch * 0.5f, Icon.ChevL, PanelRgb, 18f)) P.SelectedLayer = layer - 1;
            if (layer < P.PlayableLayer && IconBtn(W - 22f, cy + ch * 0.5f, Icon.ChevR, PanelRgb, 18f)) P.SelectedLayer = layer + 1;

            if (Btn(bx, playY, bw, 70f, Yellow, Loc.Get("play"), 31f, YellowInk, true, Icon.Drill)) { StartRun(P.SelectedLayer); return; }
            if (Btn(bx, garageY, bw, 56f, Indigo, Loc.Get("garage"), 22f, 0xFFFFFF, true, Icon.Gear)) { Go(GameScreen.Garage); return; }
            if (P.CanAffordAny())
            {
                float bob = 1f + 0.08f * MathF.Sin(_time * 6f);
                G.Sprite(Art.Circle, bx + bw - 6f, garageY + 4f, 0.46f * bob, 0f, Col.Hex(Red));
                G.TextMid("!", bx + bw - 6f, garageY + 4f, 15f, Col.White);
            }
            G.Text("v" + Version + " · prototype", 8f, H - SafeBottom - 7f, 9f, Col.Hex(0xFFFFFF, 0.45f));
        }

        // ------------------------------------------------------------------ garage

        private static readonly Icon[] UpIcons = { Icon.Drill, Icon.Gear, Icon.Battery, Icon.Width, Icon.Magnet, Icon.Gem };
        private static readonly uint[] UpColors = { 0xFF7A45, 0x4BA3FF, 0x35C97F, 0xFFC61A, 0xF0505A, 0xB56BFF };

        private string UpValue(Upgrade u, int level)
        {
            switch (u)
            {
                case Upgrade.Drill: { float p = Balance.Power(level); return p < 100f ? Dec(p) : Fmt.Short(p, Loc.French); }
                case Upgrade.Engine: return Dec(Balance.Speed(level)) + " m/s";
                case Upgrade.Battery: return ((int)MathF.Round(Balance.BatterySeconds(level))) + " s";
                case Upgrade.Chassis: return Balance.StartPods(level).ToString();
                case Upgrade.Magnet: return Dec(Balance.MagnetRadius(level)) + " m";
                default: return "×" + Dec(Balance.RefineryMul(level));
            }
        }

        private void DrawGarage()
        {
            float W = ViewW, H = ViewH;
            Backdrop();
            Header(Loc.Get("garage"));

            float previewY = SafeTop + 112f;
            G.Sprite(Art.PGlow, W * 0.5f, previewY, 3.2f, 0f, Col.Glow(0xFFC61A, 0.10f));
            MachineArt.Draw(G, Art, W * 0.5f, previewY - 4f, Balance.StartPods(P.Up[(int)Upgrade.Chassis]), 40f, _time,
                            MathF.Sin(_time * 0.9f) * 0.5f, 0.2f, false, P.Up[(int)Upgrade.Drill], P.Up[(int)Upgrade.Engine]);

            float top = SafeTop + 172f, bottom = H - SafeBottom - 90f;
            float gap = 10f, cardW = (W - 32f - gap) * 0.5f;
            float cardH = MathF.Min(152f, (bottom - top - gap * 2f) / 3f);
            for (int i = 0; i < (int)Upgrade.Count; i++)
            {
                float x = 16f + (i % 2) * (cardW + gap), y = top + (i / 2) * (cardH + gap);
                UpgradeCard((Upgrade)i, x, y, cardW, cardH);
                _buyPulse[i] = MathF.Max(0f, _buyPulse[i] - 0.06f);
            }
            if (Btn(16f, H - SafeBottom - 78f, W - 32f, 62f, Yellow, Loc.Get("play"), 28f, YellowInk, true, Icon.Drill)) StartRun(P.SelectedLayer);
        }

        private void UpgradeCard(Upgrade u, float x, float y, float w, float h)
        {
            int i = (int)u, level = P.Up[i], max = Balance.MaxLevel(u);
            bool maxed = level >= max;
            long cost = maxed ? 0 : Balance.Cost(u, level);
            bool afford = !maxed && P.Coins >= cost;
            float pulse = _buyPulse[i];
            G.Panel(Art.Panel, x, y + 3f, w, h, Col.Hex(0x0B0D22, 0.6f));
            G.Panel(Art.Panel, x - pulse * 2f, y - pulse * 2f, w + pulse * 4f, h + pulse * 4f, Col.Hex(Col.Shade(PanelRgb, pulse * 0.25f), 0.97f));
            G.Sprite(Art.Circle, x + 26f, y + 26f, 0.72f, 0f, Col.Hex(UpColors[i]));
            G.Sprite(Art.I(UpIcons[i]), x + 26f, y + 26f, 0.82f, 0f, Col.White);
            G.Text(Loc.Get("up." + u), x + 50f, y + 24f, 16.5f, Col.White);
            G.Text(maxed ? Loc.Get("up.max") : Loc.Get("up.level", level + 1), x + 50f, y + 40f, 11.5f, Col.Hex(Muted));

            float vy = y + 46f + (h - 46f - 48f) * 0.5f;
            G.TextMid(Loc.Get("up." + u + ".d"), x + w * 0.5f, vy - 9f, 11f, Col.Hex(Muted));
            string cur = UpValue(u, level);
            if (maxed) G.TextMid(cur, x + w * 0.5f, vy + 9f, 16f, Col.Hex(Green));
            else
            {
                string next = UpValue(u, level + 1);
                float wc = G.Measure(cur, 15f), wn = G.Measure(next, 15f), total = wc + 18f + wn;
                float sx = x + (w - total) * 0.5f;
                G.TextMid(cur, sx, vy + 9f, 15f, Col.White, Align.Left);
                G.Sprite(Art.I(Icon.ChevR), sx + wc + 9f, vy + 9f, 0.42f, 0f, Col.Hex(Muted));
                G.TextMid(next, sx + wc + 18f, vy + 9f, 15f, Col.Hex(Green), Align.Left);
            }

            string label = maxed ? Loc.Get("up.max") : Fmt.Short(cost, Loc.French);
            if (Btn(x + 10f, y + h - 48f, w - 20f, 36f, Green, label, 17f, 0xFFFFFF, !maxed, Icon.Coin, afford))
            {
                P.Coins -= cost;
                P.Up[i]++;
                P.J.Upgrades++;
                if (P.J.FirstUpgradeDelay < 0f) P.J.FirstUpgradeDelay = P.J.TotalSeconds;
                _buyPulse[i] = 1f;
                Audio.Play(Sfx.Buy, 0.9f);
                Haptic(1);
                Save();
            }
        }

        // ------------------------------------------------------------------ fin de descente

        private void DrawResults(float dt)
        {
            float W = ViewW, H = ViewH, t = _screenT;
            Backdrop();
            if (_won) G.Sprite(Art.PGlow, W * 0.5f, SafeTop + 90f, 8f, 0f, Col.Glow(0xFFC61A, 0.16f));
            CoinPill(W - 14f, SafeTop + 14f, true);

            string title = _won ? (_finalWin ? Loc.Get("res.final") : Loc.Get("res.win", _resultLayer + 1)) : Loc.Get(_quit ? "res.quit" : "res.fail");
            float size = 31f, tw = G.Measure(title, size);
            if (tw > W - 28f) size *= (W - 28f) / tw;
            uint titleRgb = _won ? Yellow : 0xFF8A94u;
            G.TextOutlined(title, W * 0.5f, SafeTop + 88f, size * Appear(0f), Col.Hex(titleRgb), Col.Hex(0x0B0D22), 3f);

            string sub = "";
            if (!_won && _sealPct > 0) sub = Loc.Get("res.sealLeft", _sealPct);
            else if (!_won && _metersLeft > 0) sub = Loc.Get("res.metersLeft", _metersLeft);
            else if (_won && !_finalWin && P.MaxLayer < Balance.LayerCount)
                sub = Loc.Get("layer") + " " + (P.MaxLayer + 1) + " : " + Loc.Get(Balance.Layers[P.MaxLayer].Key);
            G.TextMid(sub, W * 0.5f, SafeTop + 122f, 15f, Col.Hex(Muted));

            // Statistiques.
            float sy = SafeTop + 148f, sh = 148f;
            G.Panel(Art.Panel, 20f, sy, W - 40f, sh, Col.Hex(PanelRgb, 0.96f));
            Run run = Run;
            StatRow(sy + 30f, Loc.Get("res.depth"), _resultDepth + " " + Loc.Get("unit.m"), _record);
            StatRow(sy + 62f, Loc.Get("res.blocks"), run.Blocks.ToString(), false);
            StatRow(sy + 94f, Loc.Get("res.ore"), run.OreCells.ToString(), false);
            StatRow(sy + 126f, Loc.Get("res.gates"), run.GatesGood.ToString(), false);

            // Vente du minerai : le compteur grimpe.
            float vy = sy + sh + 14f, vh = 94f;
            G.Panel(Art.Panel, 20f, vy, W - 40f, vh, Col.Hex(0x2A2A12, 0.96f));
            G.Panel(Art.PanelLine, 20f, vy, W - 40f, vh, Col.Hex(Yellow, 0.9f));
            G.TextMid(_doubled ? Loc.Get("res.doubled") : Loc.Get("res.sale"), W * 0.5f, vy + 20f, 13f, Col.Hex(Yellow));
            float k = Mathx.EaseOutCubic(Mathx.Clamp01((t - 0.45f) / 1.1f));
            long shown = (long)Math.Round(_sale * (double)k) * (_doubled ? 2 : 1);
            if (k > 0f && k < 1f && _sale > 0) Audio.Play(Sfx.CoinTick, 0.5f, 0.9f + k * 0.5f, 0.045f);
            string amount = "+" + Fmt.Short(shown, Loc.French);
            float aw = G.Measure(amount, 36f);
            G.Sprite(Art.Coin, W * 0.5f - aw * 0.5f - 16f, vy + 52f, 2.1f, 0f, Col.White);
            G.TextMid(amount, W * 0.5f + 14f, vy + 52f, 36f, Col.Hex(0xFFD23F));
            if (run.RefineMul > 1.001f) G.TextMid(Loc.Get("res.refinery", Dec(run.RefineMul)), W * 0.5f, vy + 80f, 11.5f, Col.Hex(Muted));

            // Actions.
            float by = H - SafeBottom - 84f;
            float gapTop = vy + vh, gapBottom = by - 100f;
            if (gapBottom - gapTop > 110f)
                MachineArt.Draw(G, Art, W * 0.5f, (gapTop + gapBottom) * 0.5f - 4f, run.Pods, 38f, _time, MathF.Sin(_time * 0.9f) * 0.5f,
                                _won ? 0.5f : 0f, _won, run.DrillLevel, run.EngineLevel);
            if (!_doubled && _sale > 0 && t > 0.9f)
            {
                if (!_adOffered) { _adOffered = true; P.J.AdsOffered++; }
                if (Btn(20f, by - 86f, W - 40f, 56f, Violet, Loc.Get("res.double"), 22f, 0xFFFFFF, true, Icon.Ad))
                {
                    P.J.AdsAccepted++;
                    StartAd(() => { P.Coins += _sale; _doubled = true; Save(); Audio.Play(Sfx.Buy, 0.9f); });
                }
                G.TextMid(Loc.Get("res.doubleNote"), W * 0.5f, by - 16f, 10.5f, Col.Hex(Muted));
            }
            float half = (W - 40f - 12f) * 0.5f;
            if (Btn(20f, by, half, 62f, Indigo, Loc.Get("garage"), 20f, 0xFFFFFF, true, Icon.Gear)) { Go(GameScreen.Garage); return; }
            if (P.CanAffordAny())
            {
                G.Sprite(Art.Circle, 20f + half - 6f, by + 4f, 0.46f * (1f + 0.08f * MathF.Sin(_time * 6f)), 0f, Col.Hex(Red));
                G.TextMid("!", 20f + half - 6f, by + 4f, 15f, Col.White);
            }
            bool next = _won && !_finalWin;
            if (Btn(20f + half + 12f, by, half, 62f, Yellow, Loc.Get(next ? "res.next" : "res.again"), 20f, YellowInk, true, Icon.Drill))
                StartRun(P.SelectedLayer);
        }

        private void StatRow(float y, string label, string value, bool badge)
        {
            G.TextMid(label, 40f, y, 15f, Col.Hex(Muted), Align.Left);
            G.TextMid(value, ViewW - 40f, y, 17f, Col.White, Align.Right);
            if (badge)
            {
                string s = Loc.Get("res.record");
                float w = G.Measure(s, 10f) + 14f, vx = ViewW - 40f - G.Measure(value, 17f) - w - 8f;
                G.Panel(Art.PanelSm, vx, y - 9f, w, 18f, Col.Hex(Green));
                G.TextMid(s, vx + w * 0.5f, y, 10f, Col.Hex(0x06301C));
            }
        }

        // ------------------------------------------------------------------ réglages

        private void DrawSettings(float dt)
        {
            float W = ViewW, H = ViewH;
            Backdrop();
            Header(Loc.Get("settings"));
            if (_resetArm >= 0f) { _resetArm -= dt; }

            float y = SafeTop + 82f;
            const float rh = 50f, gap = 8f;
            if (Toggle(y, Icon.Sound, Loc.Get("set.sound"), P.Sound)) { P.Sound = !P.Sound; Changed(); }
            y += rh + gap;
            if (Toggle(y, Icon.Music, Loc.Get("set.music"), P.Music)) { P.Music = !P.Music; Changed(); }
            y += rh + gap;
            if (Toggle(y, Icon.Vibrate, Loc.Get("set.haptics"), P.Haptics)) { P.Haptics = !P.Haptics; Changed(); Haptic(1); }
            y += rh + gap;
            if (Toggle(y, Icon.Shake, Loc.Get("set.shake"), P.Shake)) { P.Shake = !P.Shake; Changed(); }
            y += rh + gap;
            if (RowButton(y, Icon.Globe, Loc.Get("set.lang"), PanelRgb)) { P.Lang = P.Lang == "fr" ? "en" : "fr"; Changed(); }
            y += rh + gap;
            if (RowButton(y, Icon.Copy, Loc.Get("set.journal"), 0x23406A))
            {
                _plat.CopyText(P.J.Export(Version));
                Toast(Loc.Get("set.copied"));
            }
            y += rh + 2f;
            G.TextMid(Loc.Get("set.journalNote"), W * 0.5f, y + 8f, 11f, Col.Hex(Muted));
            y += 26f;
            bool armed = _resetArm > 0f;
            if (RowButton(y, Icon.Reset, Loc.Get(armed ? "set.resetConfirm" : "set.reset"), armed ? Red : 0x4A2330u))
            {
                if (armed)
                {
                    var fresh = new Progress { Sound = P.Sound, Music = P.Music, Haptics = P.Haptics, Shake = P.Shake, Lang = P.Lang, J = P.J, TutorialDone = true };
                    P = fresh;
                    Save();
                    Toast(Loc.Get("set.resetDone"));
                    _resetArm = -1f;
                }
                else _resetArm = 3f;
            }

            float fy = H - SafeBottom - 58f;
            G.TextMid(Loc.Get("set.privacy"), W * 0.5f, fy, 12f, Col.Hex(0xC7CDF2));
            G.TextMid(Loc.Get("set.credits"), W * 0.5f, fy + 18f, 9.5f, Col.Hex(Muted));
            G.TextMid("Dig For Real · v" + Version, W * 0.5f, fy + 36f, 10f, Col.Hex(Muted));
        }

        private void Changed() { ApplySettings(); Save(); }

        private bool Toggle(float y, Icon icon, string label, bool on)
        {
            bool clicked = Row(y, icon, label, PanelRgb);
            float sw = 54f, sx = ViewW - 20f - 14f - sw, sy = y + 11f;
            G.Panel(Art.PanelSm, sx, sy, sw, 28f, Col.Hex(on ? Green : Gray));
            G.Sprite(Art.Circle, on ? sx + sw - 14f : sx + 14f, sy + 14f, 0.46f, 0f, Col.White);
            return clicked;
        }

        private bool RowButton(float y, Icon icon, string label, uint rgb) { return Row(y, icon, label, rgb); }

        private bool Row(float y, Icon icon, string label, uint rgb)
        {
            float x = 20f, w = ViewW - 40f, h = 50f;
            bool originIn = In(_pressX, _pressY, x, y, w, h), curIn = In(_px, _py, x, y, w, h);
            bool armed = _down && originIn && curIn;
            Hit("row:" + icon, x + w * 0.5f, y + h * 0.5f);
            G.Panel(Art.Panel, x, y, w, h, Col.Hex(Col.Shade(rgb, armed ? 0.12f : 0f), 0.97f));
            G.Sprite(Art.I(icon), x + 28f, y + h * 0.5f, 0.85f, 0f, Col.White);
            float size = 16f, maxW = w - 130f, tw = G.Measure(label, size);
            if (tw > maxW) size *= maxW / tw;
            G.TextMid(label, x + 52f, y + h * 0.5f, size, Col.White, Align.Left);
            bool clicked = _released && originIn && curIn;
            if (clicked) { Audio.Play(Sfx.Tap, 0.8f); Haptic(0); }
            return clicked;
        }

        // ------------------------------------------------------------------ surimpressions

        private void DrawPause()
        {
            float W = ViewW, H = ViewH;
            G.Rect(0f, 0f, W, H, Col.Hex(0x05060F, 0.72f));
            float pw = 300f, ph = 236f, px = (W - pw) * 0.5f, py = (H - ph) * 0.5f;
            G.Panel(Art.Panel, px, py, pw, ph, Col.Hex(PanelRgb));
            G.TextMid(Loc.Get("pause"), W * 0.5f, py + 40f, 28f, Col.White);
            if (Btn(px + 24f, py + 78f, pw - 48f, 60f, Yellow, Loc.Get("pause.resume"), 23f, YellowInk, true, Icon.Play)) _paused = false;
            if (Btn(px + 24f, py + 154f, pw - 48f, 50f, Gray, Loc.Get("pause.quit"), 18f, 0xFFFFFF, true, Icon.Home))
            {
                _paused = false; _quit = true;
                Run.Abort();
                _overT = 0.6f;
            }
        }

        private void DrawRevive()
        {
            float W = ViewW, H = ViewH;
            _reviveT += 1f / 60f;
            G.Rect(0f, 0f, W, H, Col.Hex(0x05060F, 0.72f));
            float pw = 320f, ph = 250f, px = (W - pw) * 0.5f, py = (H - ph) * 0.5f;
            float s = Mathx.EaseOutBack(Mathx.Clamp01(_reviveT * 5f));
            G.Panel(Art.Panel, px, py + (1f - s) * 30f, pw, ph, Col.Hex(PanelRgb));
            G.Sprite(Art.I(Icon.Battery), W * 0.5f, py + 44f, 1.7f, 0f, Col.Hex(0x35D6FF));
            G.TextMid(Loc.Get("revive.title"), W * 0.5f, py + 88f, 20f, Col.White);
            bool yes = Btn(px + 24f, py + 112f, pw - 48f, 58f, Green, Loc.Get("revive.yes"), 22f, 0xFFFFFF, true, Icon.Ad);
            G.TextMid(Loc.Get("res.doubleNote"), W * 0.5f, py + 186f, 10.5f, Col.Hex(Muted));
            float left = Mathx.Clamp01(1f - _reviveT / 6f);
            G.Panel(Art.PanelSm, px + 24f, py + 198f, (pw - 48f) * left, 5f, Col.Hex(Muted, 0.7f));
            bool no = Row2(px + 60f, py + 208f, pw - 120f, 34f, Loc.Get("revive.no"));
            if (yes)
            {
                P.J.AdsAccepted++;
                StartAd(() => { Run.Revive(); _reviveOpen = false; _overT = 0f; _graceT = 0.4f; _fx.Clear(); });
            }
            else if (no || _reviveT > 6f) { _reviveOpen = false; _overT = 1.2f; }
        }

        private bool Row2(float x, float y, float w, float h, string label)
        {
            bool originIn = In(_pressX, _pressY, x, y, w, h), curIn = In(_px, _py, x, y, w, h);
            Hit(label, x + w * 0.5f, y + h * 0.5f);
            G.TextMid(label, x + w * 0.5f, y + h * 0.5f, 15f, Col.Hex(_down && originIn && curIn ? 0xFFFFFFu : Muted));
            return _released && originIn && curIn;
        }

        private void StartAd(Action done) { _adT = 0f; _adDone = done; }

        /// <summary>Pub récompensée simulée : aucune régie n'est intégrée dans le prototype.</summary>
        private void DrawAd(float dt)
        {
            const float duration = 2.2f;
            _adT += dt;
            float W = ViewW, H = ViewH;
            G.Rect(0f, 0f, W, H, Col.Hex(0x05060F, 0.97f));
            G.Sprite(Art.I(Icon.Ad), W * 0.5f, H * 0.5f - 70f, 3f, 0f, Col.Hex(Violet));
            G.TextMid(Loc.Get("ad.simulated"), W * 0.5f, H * 0.5f - 6f, 28f, Col.White);
            G.TextMid(Loc.Get("ad.note"), W * 0.5f, H * 0.5f + 26f, 12.5f, Col.Hex(Muted));
            float k = Mathx.Clamp01(_adT / duration);
            G.Panel(Art.PanelSm, 60f, H * 0.5f + 54f, W - 120f, 10f, Col.Hex(PanelRgb));
            G.Panel(Art.PanelSm, 60f, H * 0.5f + 54f, MathF.Max(10f, (W - 120f) * k), 10f, Col.Hex(Violet));
            if (_adT >= duration)
            {
                _adT = -1f;
                Action done = _adDone; _adDone = null;
                if (done != null) done();
            }
        }
    }
}
