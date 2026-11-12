using System;
using System.Collections.Generic;

namespace Game.Core.Engine
{
    /// <summary>Image d'un caractère : masque de couverture et métriques, en pixels.</summary>
    public struct GlyphBitmap
    {
        public byte[] Coverage;
        public int Width, Height;
        /// <summary>Décalage horizontal du bord gauche de l'image par rapport au point d'écriture.</summary>
        public float BearingX;
        /// <summary>Hauteur du bord supérieur de l'image au-dessus de la ligne de base.</summary>
        public float BearingY;
        public float Advance;
    }

    /// <summary>
    /// Lecteur de police TrueType (.ttf à contours quadratiques) et dessinateur de caractères, en C# pur.
    /// Le jeu dessine ainsi son texte lui-même : même rendu dans Unity et dans le banc d'essai.
    /// </summary>
    public sealed class TrueTypeFont
    {
        private readonly byte[] _d;
        private int _glyf, _loca, _hmtx, _cmapSub;
        private int _cmapFormat;
        private int _numGlyphs, _unitsPerEm, _locaFormat, _numHMetrics;

        /// <summary>Métriques verticales, en fraction du corps (em).</summary>
        public float Ascent, Descent, LineGap;

        private readonly Dictionary<int, int> _glyphCache = new Dictionary<int, int>();

        public TrueTypeFont(byte[] data)
        {
            _d = data ?? throw new ArgumentNullException(nameof(data));
            int numTables = U16(4);
            int head = 0, hhea = 0, maxp = 0, cmap = 0;
            for (int i = 0; i < numTables; i++)
            {
                int rec = 12 + i * 16;
                string tag = "" + (char)_d[rec] + (char)_d[rec + 1] + (char)_d[rec + 2] + (char)_d[rec + 3];
                int off = (int)U32(rec + 8);
                switch (tag)
                {
                    case "glyf": _glyf = off; break;
                    case "loca": _loca = off; break;
                    case "head": head = off; break;
                    case "hhea": hhea = off; break;
                    case "hmtx": _hmtx = off; break;
                    case "maxp": maxp = off; break;
                    case "cmap": cmap = off; break;
                }
            }
            if (_glyf == 0 || _loca == 0 || head == 0 || hhea == 0 || _hmtx == 0 || maxp == 0 || cmap == 0)
                throw new InvalidOperationException("Police non prise en charge : tables TrueType manquantes.");

            _unitsPerEm = U16(head + 18);
            _locaFormat = S16(head + 50);
            _numGlyphs = U16(maxp + 4);
            Ascent = S16(hhea + 4) / (float)_unitsPerEm;
            Descent = S16(hhea + 6) / (float)_unitsPerEm;
            LineGap = S16(hhea + 8) / (float)_unitsPerEm;
            _numHMetrics = U16(hhea + 34);

            // Table de correspondance caractère → glyphe : format 12 si possible, sinon format 4.
            int n = U16(cmap + 2), best = -1;
            for (int i = 0; i < n; i++)
            {
                int rec = cmap + 4 + i * 8;
                int platform = U16(rec), encoding = U16(rec + 2);
                int sub = cmap + (int)U32(rec + 4);
                int format = U16(sub);
                int score = -1;
                if (format == 12 && (platform == 3 || platform == 0)) score = 3;
                else if (format == 4 && platform == 3 && encoding == 1) score = 2;
                else if (format == 4 && platform == 0) score = 1;
                if (score > best) { best = score; _cmapSub = sub; _cmapFormat = format; }
            }
            if (best < 0) throw new InvalidOperationException("Police non prise en charge : table cmap inconnue.");
        }

        private int U16(int o) { return (_d[o] << 8) | _d[o + 1]; }
        private int S16(int o) { return (short)((_d[o] << 8) | _d[o + 1]); }
        private uint U32(int o) { return ((uint)_d[o] << 24) | ((uint)_d[o + 1] << 16) | ((uint)_d[o + 2] << 8) | _d[o + 3]; }

        public int GlyphIndex(int codepoint)
        {
            if (_glyphCache.TryGetValue(codepoint, out int cached)) return cached;
            int g = 0;
            if (_cmapFormat == 4)
            {
                if (codepoint <= 0xFFFF)
                {
                    int segCount = U16(_cmapSub + 6) / 2;
                    int endCodes = _cmapSub + 14;
                    int startCodes = endCodes + segCount * 2 + 2;
                    int idDeltas = startCodes + segCount * 2;
                    int idRangeOffsets = idDeltas + segCount * 2;
                    for (int i = 0; i < segCount; i++)
                    {
                        if (codepoint > U16(endCodes + i * 2)) continue;
                        int start = U16(startCodes + i * 2);
                        if (codepoint < start) break;
                        int rangeOffset = U16(idRangeOffsets + i * 2);
                        int delta = U16(idDeltas + i * 2);
                        if (rangeOffset == 0) g = (codepoint + delta) & 0xFFFF;
                        else
                        {
                            int addr = idRangeOffsets + i * 2 + rangeOffset + (codepoint - start) * 2;
                            int v = U16(addr);
                            g = v == 0 ? 0 : (v + delta) & 0xFFFF;
                        }
                        break;
                    }
                }
            }
            else
            {
                uint groups = U32(_cmapSub + 12);
                for (uint i = 0; i < groups; i++)
                {
                    int rec = _cmapSub + 16 + (int)i * 12;
                    uint start = U32(rec), end = U32(rec + 4);
                    if ((uint)codepoint < start) break;
                    if ((uint)codepoint <= end) { g = (int)(U32(rec + 8) + ((uint)codepoint - start)); break; }
                }
            }
            if (g >= _numGlyphs) g = 0;
            _glyphCache[codepoint] = g;
            return g;
        }

        public bool Has(int codepoint) { return GlyphIndex(codepoint) != 0; }

        /// <summary>Avance horizontale d'un caractère, en fraction du corps.</summary>
        public float Advance(int codepoint)
        {
            int g = GlyphIndex(codepoint);
            int idx = g < _numHMetrics ? g : _numHMetrics - 1;
            return U16(_hmtx + idx * 4) / (float)_unitsPerEm;
        }

        private int GlyphOffset(int g, out int length)
        {
            int a, b;
            if (_locaFormat == 0) { a = U16(_loca + g * 2) * 2; b = U16(_loca + g * 2 + 2) * 2; }
            else { a = (int)U32(_loca + g * 4); b = (int)U32(_loca + g * 4 + 4); }
            length = b - a;
            return _glyf + a;
        }

        // Un contour = liste de points (x, y, sur la courbe ?) en unités de police.
        private struct Pt { public float X, Y; public bool On; }

        private void ReadGlyph(int g, float m00, float m01, float m10, float m11, float tx, float ty, List<List<Pt>> contours, int depth)
        {
            if (depth > 6 || g < 0 || g >= _numGlyphs) return;
            int off = GlyphOffset(g, out int len);
            if (len <= 0) return;
            int nContours = S16(off);
            int p = off + 10;
            if (nContours >= 0)
            {
                int[] ends = new int[nContours];
                for (int i = 0; i < nContours; i++) { ends[i] = U16(p); p += 2; }
                int nPts = nContours > 0 ? ends[nContours - 1] + 1 : 0;
                int insLen = U16(p); p += 2 + insLen;
                byte[] flags = new byte[nPts];
                for (int i = 0; i < nPts; i++)
                {
                    byte f = _d[p++];
                    flags[i] = f;
                    if ((f & 8) != 0)
                    {
                        int rep = _d[p++];
                        for (int r = 0; r < rep && i + 1 < nPts; r++) flags[++i] = f;
                    }
                }
                float[] xs = new float[nPts], ys = new float[nPts];
                int v = 0;
                for (int i = 0; i < nPts; i++)
                {
                    byte f = flags[i];
                    if ((f & 2) != 0) { int dx = _d[p++]; v += (f & 16) != 0 ? dx : -dx; }
                    else if ((f & 16) == 0) { v += S16(p); p += 2; }
                    xs[i] = v;
                }
                v = 0;
                for (int i = 0; i < nPts; i++)
                {
                    byte f = flags[i];
                    if ((f & 4) != 0) { int dy = _d[p++]; v += (f & 32) != 0 ? dy : -dy; }
                    else if ((f & 32) == 0) { v += S16(p); p += 2; }
                    ys[i] = v;
                }
                int startPt = 0;
                for (int c = 0; c < nContours; c++)
                {
                    var list = new List<Pt>(ends[c] - startPt + 1);
                    for (int i = startPt; i <= ends[c]; i++)
                    {
                        Pt pt;
                        pt.X = xs[i] * m00 + ys[i] * m10 + tx;
                        pt.Y = xs[i] * m01 + ys[i] * m11 + ty;
                        pt.On = (flags[i] & 1) != 0;
                        list.Add(pt);
                    }
                    if (list.Count >= 2) contours.Add(list);
                    startPt = ends[c] + 1;
                }
            }
            else
            {
                // Glyphe composé (lettres accentuées) : assemblage de glyphes transformés.
                while (true)
                {
                    int flags = U16(p), child = U16(p + 2); p += 4;
                    float ax, ay;
                    if ((flags & 1) != 0) { ax = S16(p); ay = S16(p + 2); p += 4; }
                    else { ax = (sbyte)_d[p]; ay = (sbyte)_d[p + 1]; p += 2; }
                    if ((flags & 2) == 0) { ax = 0f; ay = 0f; }
                    float a = 1f, b = 0f, c = 0f, d = 1f;
                    if ((flags & 8) != 0) { a = d = S16(p) / 16384f; p += 2; }
                    else if ((flags & 0x40) != 0) { a = S16(p) / 16384f; d = S16(p + 2) / 16384f; p += 4; }
                    else if ((flags & 0x80) != 0)
                    {
                        a = S16(p) / 16384f; b = S16(p + 2) / 16384f; c = S16(p + 4) / 16384f; d = S16(p + 6) / 16384f; p += 8;
                    }
                    // Composition : point' = M_parent * (M_enfant * point + décalage) + t_parent
                    float n00 = a * m00 + b * m10, n01 = a * m01 + b * m11;
                    float n10 = c * m00 + d * m10, n11 = c * m01 + d * m11;
                    float ntx = ax * m00 + ay * m10 + tx, nty = ax * m01 + ay * m11 + ty;
                    ReadGlyph(child, n00, n01, n10, n11, ntx, nty, contours, depth + 1);
                    if ((flags & 0x20) == 0) break;
                }
            }
        }

        /// <summary>Dessine un caractère à la taille demandée (pixels par em).</summary>
        public GlyphBitmap Render(int codepoint, float pxPerEm)
        {
            GlyphBitmap res = default(GlyphBitmap);
            res.Advance = Advance(codepoint) * pxPerEm;
            int g = GlyphIndex(codepoint);
            var contours = new List<List<Pt>>();
            ReadGlyph(g, 1f, 0f, 0f, 1f, 0f, 0f, contours, 0);
            if (contours.Count == 0) return res;

            float scale = pxPerEm / _unitsPerEm;
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (var c in contours)
                foreach (var pt in c)
                {
                    if (pt.X < minX) minX = pt.X; if (pt.X > maxX) maxX = pt.X;
                    if (pt.Y < minY) minY = pt.Y; if (pt.Y > maxY) maxY = pt.Y;
                }
            // Origine de l'image calée sur des pixels entiers pour garder des bords nets.
            float left = MathF.Floor(minX * scale) - 1f, top = MathF.Ceiling(maxY * scale) + 1f;
            int w = (int)(MathF.Ceiling(maxX * scale) - left) + 2;
            int h = (int)(top - MathF.Floor(minY * scale)) + 2;
            if (w <= 0 || h <= 0 || w > 4096 || h > 4096) return res;

            float[] acc = new float[w * h + 4];
            foreach (var c in contours)
            {
                int n = c.Count;
                // Point de départ sur la courbe (ou milieu implicite entre deux points de contrôle).
                Pt start;
                int first;
                if (c[0].On) { start = c[0]; first = 1; }
                else if (c[n - 1].On) { start = c[n - 1]; first = 0; }
                else { start.X = (c[0].X + c[n - 1].X) * 0.5f; start.Y = (c[0].Y + c[n - 1].Y) * 0.5f; start.On = true; first = 0; }

                float curX = start.X * scale - left, curY = top - start.Y * scale;
                float startX = curX, startY = curY;
                bool hasCtrl = false; float ctrlX = 0f, ctrlY = 0f;
                int count = c[0].On ? n - 1 : (c[n - 1].On ? n - 1 : n);
                for (int k = 0; k < count; k++)
                {
                    Pt pt = c[(first + k) % n];
                    float px = pt.X * scale - left, py = top - pt.Y * scale;
                    if (pt.On)
                    {
                        if (hasCtrl) { Quad(acc, w, h, curX, curY, ctrlX, ctrlY, px, py); hasCtrl = false; }
                        else Segment(acc, w, h, curX, curY, px, py);
                        curX = px; curY = py;
                    }
                    else
                    {
                        if (hasCtrl)
                        {
                            float mx = (ctrlX + px) * 0.5f, my = (ctrlY + py) * 0.5f;
                            Quad(acc, w, h, curX, curY, ctrlX, ctrlY, mx, my);
                            curX = mx; curY = my;
                        }
                        ctrlX = px; ctrlY = py; hasCtrl = true;
                    }
                }
                if (hasCtrl) Quad(acc, w, h, curX, curY, ctrlX, ctrlY, startX, startY);
                else Segment(acc, w, h, curX, curY, startX, startY);
            }

            byte[] cov = new byte[w * h];
            for (int y = 0; y < h; y++)
            {
                float sum = 0f;
                int row = y * w;
                for (int x = 0; x < w; x++)
                {
                    sum += acc[row + x];
                    float a = MathF.Abs(sum);
                    cov[row + x] = (byte)(a >= 1f ? 255 : (int)(a * 255f + 0.5f));
                }
            }
            res.Coverage = cov; res.Width = w; res.Height = h;
            res.BearingX = left; res.BearingY = top;
            return res;
        }

        private static void Quad(float[] acc, int w, int h, float x0, float y0, float cx, float cy, float x1, float y1)
        {
            float dx = x0 - 2f * cx + x1, dy = y0 - 2f * cy + y1;
            float dev = MathF.Sqrt(dx * dx + dy * dy);
            int n = 1 + (int)MathF.Sqrt(dev * 3f);
            if (n > 24) n = 24;
            float px = x0, py = y0;
            for (int i = 1; i <= n; i++)
            {
                float t = i / (float)n, u = 1f - t;
                float qx = u * u * x0 + 2f * u * t * cx + t * t * x1;
                float qy = u * u * y0 + 2f * u * t * cy + t * t * y1;
                Segment(acc, w, h, px, py, qx, qy);
                px = qx; py = qy;
            }
        }

        /// <summary>Accumule l'aire signée couverte par un segment (remplissage exact par balayage).</summary>
        private static void Segment(float[] acc, int w, int h, float ax, float ay, float bx, float by)
        {
            if (ay == by) return;
            float dir = 1f;
            if (ay > by) { dir = -1f; float t = ax; ax = bx; bx = t; t = ay; ay = by; by = t; }
            float dxdy = (bx - ax) / (by - ay);
            float x = ax;
            if (ay < 0f) x -= ay * dxdy;
            int yStart = Math.Max(0, (int)ay), yEnd = Math.Min(h, (int)MathF.Ceiling(by));
            for (int y = yStart; y < yEnd; y++)
            {
                int line = y * w;
                float dy = MathF.Min(y + 1, by) - MathF.Max(y, ay);
                float xNext = x + dxdy * dy;
                float d = dy * dir;
                float x0 = x < xNext ? x : xNext, x1 = x < xNext ? xNext : x;
                float x0Floor = MathF.Floor(x0);
                int x0i = (int)x0Floor;
                float x1Ceil = MathF.Ceiling(x1);
                int x1i = (int)x1Ceil;
                if (x0i < 0) { x0i = 0; x0Floor = 0f; }
                if (x1i > w - 1) x1i = w - 1;
                if (x1i <= x0i + 1)
                {
                    float xmf = 0.5f * (x + xNext) - x0Floor;
                    acc[line + x0i] += d - d * xmf;
                    acc[line + x0i + 1] += d * xmf;
                }
                else
                {
                    float s = 1f / (x1 - x0);
                    float x0f = x0 - x0Floor;
                    float a0 = 0.5f * s * (1f - x0f) * (1f - x0f);
                    float x1f = x1 - x1Ceil + 1f;
                    float am = 0.5f * s * x1f * x1f;
                    acc[line + x0i] += d * a0;
                    if (x1i == x0i + 2)
                    {
                        acc[line + x0i + 1] += d * (1f - a0 - am);
                    }
                    else
                    {
                        float a1 = s * (1.5f - x0f);
                        acc[line + x0i + 1] += d * (a1 - a0);
                        for (int xi = x0i + 2; xi < x1i - 1; xi++) acc[line + xi] += d * s;
                        float a2 = a1 + (x1i - x0i - 3) * s;
                        acc[line + x1i - 1] += d * (1f - a2 - am);
                    }
                    acc[line + x1i] += d * am;
                }
                x = xNext;
            }
        }
    }
}
