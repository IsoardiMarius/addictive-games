using System;

namespace Game.Core.Engine
{
    /// <summary>
    /// Peint des formes lissées dans un tampon RGBA prémultiplié. Sert à fabriquer, au démarrage,
    /// toutes les images du jeu (aucune image importée). Les coordonnées sont en pixels.
    /// </summary>
    public sealed class Canvas
    {
        public readonly int W, H;
        public readonly float[] Px;
        /// <summary>Facteur appliqué à toutes les coordonnées : permet de dessiner en points d'écran.</summary>
        public float K = 1f;

        /// <summary>Crée une image de wPt × hPt points, à l'échelle donnée (pixels par point).</summary>
        public static Canvas Points(float wPt, float hPt, float scale)
        {
            var c = new Canvas((int)MathF.Ceiling(wPt * scale), (int)MathF.Ceiling(hPt * scale));
            c.K = scale;
            return c;
        }

        private float _r = 1f, _g = 1f, _b = 1f, _a = 1f;
        private float _r2, _g2, _b2, _a2;
        private int _grad; // 0 : uni, 1 : dégradé vertical, 2 : dégradé radial
        private float _g0, _g1, _gcx, _gcy;
        private bool _erase;
        private bool _clip;
        private float _cx, _cy, _chw, _chh, _cr;

        public Canvas(int w, int h)
        {
            W = Math.Max(1, w); H = Math.Max(1, h);
            Px = new float[W * H * 4];
        }

        // ---------------------------------------------------------------- peinture

        public Canvas Color(uint rgb, float a = 1f)
        {
            _r = ((rgb >> 16) & 255) / 255f; _g = ((rgb >> 8) & 255) / 255f; _b = (rgb & 255) / 255f; _a = a;
            _grad = 0; _erase = false;
            return this;
        }

        public Canvas GradV(uint rgbTop, uint rgbBottom, float y0, float y1, float aTop = 1f, float aBottom = 1f)
        {
            Color(rgbTop, aTop);
            _r2 = ((rgbBottom >> 16) & 255) / 255f; _g2 = ((rgbBottom >> 8) & 255) / 255f; _b2 = (rgbBottom & 255) / 255f; _a2 = aBottom;
            _grad = 1; _g0 = y0 * K; _g1 = y1 * K;
            return this;
        }

        public Canvas GradR(uint rgbIn, uint rgbOut, float cx, float cy, float radius, float aIn = 1f, float aOut = 1f)
        {
            Color(rgbIn, aIn);
            _r2 = ((rgbOut >> 16) & 255) / 255f; _g2 = ((rgbOut >> 8) & 255) / 255f; _b2 = (rgbOut & 255) / 255f; _a2 = aOut;
            _grad = 2; _gcx = cx * K; _gcy = cy * K; _g1 = Math.Max(0.001f, radius * K);
            return this;
        }

        /// <summary>Mode gomme : les formes suivantes effacent au lieu de peindre.</summary>
        public Canvas Eraser() { _erase = true; _grad = 0; _a = 1f; return this; }

        public Canvas Clip(float x, float y, float w, float h, float r)
        {
            _clip = true; _chw = w * K * 0.5f; _chh = h * K * 0.5f; _cx = x * K + _chw; _cy = y * K + _chh; _cr = r * K;
            return this;
        }

        public Canvas NoClip() { _clip = false; return this; }

        private static float SdRoundRect(float px, float py, float cx, float cy, float hw, float hh, float r)
        {
            float qx = MathF.Abs(px - cx) - (hw - r), qy = MathF.Abs(py - cy) - (hh - r);
            float ox = MathF.Max(qx, 0f), oy = MathF.Max(qy, 0f);
            return MathF.Sqrt(ox * ox + oy * oy) + MathF.Min(MathF.Max(qx, qy), 0f) - r;
        }

        private void Plot(int x, int y, float cov)
        {
            if (cov <= 0.002f) return;
            if (cov > 1f) cov = 1f;
            float fx = x + 0.5f, fy = y + 0.5f;
            if (_clip)
            {
                float cd = SdRoundRect(fx, fy, _cx, _cy, _chw, _chh, _cr);
                float cc = 0.5f - cd;
                if (cc <= 0f) return;
                if (cc < 1f) cov *= cc;
            }
            int i = (y * W + x) * 4;
            if (_erase)
            {
                float k = 1f - cov;
                Px[i] *= k; Px[i + 1] *= k; Px[i + 2] *= k; Px[i + 3] *= k;
                return;
            }
            float r = _r, g = _g, b = _b, a = _a;
            if (_grad != 0)
            {
                float t;
                if (_grad == 1) t = Mathx.Clamp01((fy - _g0) / (_g1 - _g0));
                else { float dx = fx - _gcx, dy = fy - _gcy; t = Mathx.Clamp01(MathF.Sqrt(dx * dx + dy * dy) / _g1); }
                r += (_r2 - r) * t; g += (_g2 - g) * t; b += (_b2 - b) * t; a += (_a2 - a) * t;
            }
            float sa = a * cov;
            float inv = 1f - sa;
            Px[i] = r * sa + Px[i] * inv;
            Px[i + 1] = g * sa + Px[i + 1] * inv;
            Px[i + 2] = b * sa + Px[i + 2] * inv;
            Px[i + 3] = sa + Px[i + 3] * inv;
        }

        private void Bounds(float x0, float y0, float x1, float y1, out int ix0, out int iy0, out int ix1, out int iy1)
        {
            ix0 = Math.Max(0, (int)MathF.Floor(x0 - 1f)); iy0 = Math.Max(0, (int)MathF.Floor(y0 - 1f));
            ix1 = Math.Min(W - 1, (int)MathF.Ceiling(x1 + 1f)); iy1 = Math.Min(H - 1, (int)MathF.Ceiling(y1 + 1f));
        }

        // ---------------------------------------------------------------- formes

        public Canvas RoundRect(float x, float y, float w, float h, float r)
        {
            x *= K; y *= K; w *= K; h *= K; r *= K;
            float hw = w * 0.5f, hh = h * 0.5f, cx = x + hw, cy = y + hh;
            r = MathF.Min(r, MathF.Min(hw, hh));
            Bounds(x, y, x + w, y + h, out int x0, out int y0, out int x1, out int y1);
            for (int py = y0; py <= y1; py++)
                for (int px = x0; px <= x1; px++)
                    Plot(px, py, 0.5f - SdRoundRect(px + 0.5f, py + 0.5f, cx, cy, hw, hh, r));
            return this;
        }

        public Canvas RoundRectStroke(float x, float y, float w, float h, float r, float thickness)
        {
            x *= K; y *= K; w *= K; h *= K; r *= K; thickness *= K;
            float hw = w * 0.5f, hh = h * 0.5f, cx = x + hw, cy = y + hh, ht = thickness * 0.5f;
            r = MathF.Min(r, MathF.Min(hw, hh));
            Bounds(x - ht, y - ht, x + w + ht, y + h + ht, out int x0, out int y0, out int x1, out int y1);
            for (int py = y0; py <= y1; py++)
                for (int px = x0; px <= x1; px++)
                    Plot(px, py, 0.5f - (MathF.Abs(SdRoundRect(px + 0.5f, py + 0.5f, cx, cy, hw, hh, r)) - ht));
            return this;
        }

        public Canvas Circle(float cx, float cy, float r)
        {
            cx *= K; cy *= K; r *= K;
            Bounds(cx - r, cy - r, cx + r, cy + r, out int x0, out int y0, out int x1, out int y1);
            for (int py = y0; py <= y1; py++)
                for (int px = x0; px <= x1; px++)
                {
                    float dx = px + 0.5f - cx, dy = py + 0.5f - cy;
                    Plot(px, py, 0.5f - (MathF.Sqrt(dx * dx + dy * dy) - r));
                }
            return this;
        }

        public Canvas CircleStroke(float cx, float cy, float r, float thickness)
        {
            cx *= K; cy *= K; r *= K; thickness *= K;
            float ht = thickness * 0.5f;
            Bounds(cx - r - ht, cy - r - ht, cx + r + ht, cy + r + ht, out int x0, out int y0, out int x1, out int y1);
            for (int py = y0; py <= y1; py++)
                for (int px = x0; px <= x1; px++)
                {
                    float dx = px + 0.5f - cx, dy = py + 0.5f - cy;
                    Plot(px, py, 0.5f - (MathF.Abs(MathF.Sqrt(dx * dx + dy * dy) - r) - ht));
                }
            return this;
        }

        public Canvas Ellipse(float cx, float cy, float rx, float ry)
        {
            cx *= K; cy *= K; rx *= K; ry *= K;
            Bounds(cx - rx, cy - ry, cx + rx, cy + ry, out int x0, out int y0, out int x1, out int y1);
            for (int py = y0; py <= y1; py++)
                for (int px = x0; px <= x1; px++)
                {
                    float dx = px + 0.5f - cx, dy = py + 0.5f - cy;
                    float k0 = MathF.Sqrt(dx * dx / (rx * rx) + dy * dy / (ry * ry));
                    float k1 = MathF.Sqrt(dx * dx / (rx * rx * rx * rx) + dy * dy / (ry * ry * ry * ry));
                    float d = k1 > 1e-6f ? k0 * (k0 - 1f) / k1 : -MathF.Min(rx, ry);
                    Plot(px, py, 0.5f - d);
                }
            return this;
        }

        /// <summary>Segment épais à bouts ronds.</summary>
        public Canvas Line(float ax, float ay, float bx, float by, float thickness)
        {
            ax *= K; ay *= K; bx *= K; by *= K; thickness *= K;
            float ht = thickness * 0.5f;
            Bounds(MathF.Min(ax, bx) - ht, MathF.Min(ay, by) - ht, MathF.Max(ax, bx) + ht, MathF.Max(ay, by) + ht, out int x0, out int y0, out int x1, out int y1);
            float ex = bx - ax, ey = by - ay, len2 = ex * ex + ey * ey;
            for (int py = y0; py <= y1; py++)
                for (int px = x0; px <= x1; px++)
                {
                    float qx = px + 0.5f - ax, qy = py + 0.5f - ay;
                    float t = len2 > 1e-6f ? Mathx.Clamp01((qx * ex + qy * ey) / len2) : 0f;
                    float dx = qx - ex * t, dy = qy - ey * t;
                    Plot(px, py, 0.5f - (MathF.Sqrt(dx * dx + dy * dy) - ht));
                }
            return this;
        }

        /// <summary>Polygone plein quelconque. pts = x0, y0, x1, y1, ...</summary>
        public Canvas Poly(params float[] pts) { return PolyImpl(pts, 0f, 0f); }

        /// <summary>Polygone plein aux coins adoucis d'un rayon donné.</summary>
        public Canvas PolyRound(float radius, params float[] pts) { return PolyImpl(pts, radius, 0f); }

        /// <summary>Contour de polygone fermé.</summary>
        public Canvas PolyStroke(float thickness, params float[] pts) { return PolyImpl(pts, 0f, thickness); }

        private Canvas PolyImpl(float[] pts, float round, float stroke)
        {
            if (K != 1f) { var q = new float[pts.Length]; for (int z = 0; z < q.Length; z++) q[z] = pts[z] * K; pts = q; round *= K; stroke *= K; }
            int n = pts.Length / 2;
            if (n < 3) return this;
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < n; i++)
            {
                minX = MathF.Min(minX, pts[i * 2]); maxX = MathF.Max(maxX, pts[i * 2]);
                minY = MathF.Min(minY, pts[i * 2 + 1]); maxY = MathF.Max(maxY, pts[i * 2 + 1]);
            }
            float pad = round + stroke * 0.5f;
            Bounds(minX - pad, minY - pad, maxX + pad, maxY + pad, out int x0, out int y0, out int x1, out int y1);
            for (int py = y0; py <= y1; py++)
                for (int px = x0; px <= x1; px++)
                {
                    float fx = px + 0.5f, fy = py + 0.5f;
                    float best = float.MaxValue;
                    bool inside = false;
                    for (int i = 0, j = n - 1; i < n; j = i++)
                    {
                        float ax = pts[j * 2], ay = pts[j * 2 + 1], bx = pts[i * 2], by = pts[i * 2 + 1];
                        float ex = bx - ax, ey = by - ay, qx = fx - ax, qy = fy - ay;
                        float len2 = ex * ex + ey * ey;
                        float t = len2 > 1e-6f ? Mathx.Clamp01((qx * ex + qy * ey) / len2) : 0f;
                        float dx = qx - ex * t, dy = qy - ey * t;
                        float d2 = dx * dx + dy * dy;
                        if (d2 < best) best = d2;
                        if ((ay > fy) != (by > fy) && fx < (bx - ax) * (fy - ay) / (by - ay) + ax) inside = !inside;
                    }
                    float d = MathF.Sqrt(best);
                    if (inside) d = -d;
                    if (stroke > 0f) d = MathF.Abs(d) - stroke * 0.5f; else d -= round;
                    Plot(px, py, 0.5f - d);
                }
            return this;
        }

        /// <summary>Peint un masque de couverture (glyphe de police) avec la peinture courante.</summary>
        public Canvas Mask(byte[] cov, int mw, int mh, int dx, int dy)
        {
            for (int y = 0; y < mh; y++)
            {
                int py = dy + y;
                if (py < 0 || py >= H) continue;
                for (int x = 0; x < mw; x++)
                {
                    int px = dx + x;
                    if (px < 0 || px >= W) continue;
                    byte c = cov[y * mw + x];
                    if (c != 0) Plot(px, py, c / 255f);
                }
            }
            return this;
        }

        /// <summary>Écrit un texte dans l'image (étiquettes peintes sur les blocs).</summary>
        public Canvas Text(TrueTypeFont font, string text, float cx, float baselineY, float pxPerEm)
        {
            cx *= K; baselineY *= K; pxPerEm *= K;
            float width = 0f;
            for (int i = 0; i < text.Length; i++) width += font.Advance(text[i]) * pxPerEm;
            float pen = cx - width * 0.5f;
            for (int i = 0; i < text.Length; i++)
            {
                GlyphBitmap g = font.Render(text[i], pxPerEm);
                if (g.Coverage != null)
                    Mask(g.Coverage, g.Width, g.Height, (int)MathF.Round(pen + g.BearingX), (int)MathF.Round(baselineY - g.BearingY));
                pen += g.Advance;
            }
            return this;
        }

        /// <summary>Copie le tampon, converti en octets, dans une texture d'atlas.</summary>
        public void CopyTo(byte[] dst, int dstW, int ox, int oy)
        {
            for (int y = 0; y < H; y++)
            {
                int di = ((oy + y) * dstW + ox) * 4;
                int si = y * W * 4;
                for (int x = 0; x < W * 4; x++)
                {
                    float v = Px[si + x] * 255f + 0.5f;
                    dst[di + x] = (byte)(v < 0f ? 0 : (v > 255f ? 255 : (int)v));
                }
            }
        }
    }
}
