using System;
using System.Runtime.InteropServices;

namespace Game.Core.Engine
{
    /// <summary>Sommet envoyé tel quel à la carte graphique (même disposition mémoire côté Unity).</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public float X, Y, Z;
        public uint Color;
        public float U, V;
    }

    /// <summary>Liste des rectangles à dessiner pour l'image en cours (4 sommets par rectangle, dans l'ordre d'affichage).</summary>
    public sealed class DrawList
    {
        public Vertex[] Verts = new Vertex[4096 * 4];
        public int Count;

        public int QuadCount { get { return Count / 4; } }
        public void Clear() { Count = 0; }

        public void Quad(float x0, float y0, float x1, float y1, float x2, float y2, float x3, float y3,
                         float u0, float v0, float u1, float v1, uint c0, uint c1, uint c2, uint c3)
        {
            if (Count + 4 > Verts.Length) Array.Resize(ref Verts, Verts.Length * 2);
            int i = Count;
            Verts[i].X = x0; Verts[i].Y = y0; Verts[i].Color = c0; Verts[i].U = u0; Verts[i].V = v0;
            Verts[i + 1].X = x1; Verts[i + 1].Y = y1; Verts[i + 1].Color = c1; Verts[i + 1].U = u1; Verts[i + 1].V = v0;
            Verts[i + 2].X = x2; Verts[i + 2].Y = y2; Verts[i + 2].Color = c2; Verts[i + 2].U = u1; Verts[i + 2].V = v1;
            Verts[i + 3].X = x3; Verts[i + 3].Y = y3; Verts[i + 3].Color = c3; Verts[i + 3].U = u0; Verts[i + 3].V = v1;
            Count += 4;
        }
    }

    public enum Align { Left = 0, Center = 1, Right = 2 }

    /// <summary>
    /// Dessin immédiat : chaque image, le jeu redessine tout (décor, engin, interface) dans une DrawList.
    /// Les coordonnées sont en points d'écran, origine en haut à gauche, y vers le bas.
    /// </summary>
    public sealed class Gfx
    {
        public readonly DrawList List;
        public readonly Atlas Atlas;
        public FontSet FontSmall, FontLarge;

        private float _tx, _ty, _sc = 1f;
        private readonly float[] _stack = new float[3 * 16];
        private int _sp;

        public Gfx(Atlas atlas, DrawList list) { Atlas = atlas; List = list; }

        // ------------------------------------------------------------ transformation

        public void Push(float tx, float ty, float scale = 1f)
        {
            if (_sp < 16) { _stack[_sp * 3] = _tx; _stack[_sp * 3 + 1] = _ty; _stack[_sp * 3 + 2] = _sc; }
            _sp++;
            _tx += tx * _sc; _ty += ty * _sc; _sc *= scale;
        }

        public void Pop()
        {
            if (_sp <= 0) return;
            _sp--;
            if (_sp < 16) { _tx = _stack[_sp * 3]; _ty = _stack[_sp * 3 + 1]; _sc = _stack[_sp * 3 + 2]; }
        }

        public void ResetTransform() { _tx = 0f; _ty = 0f; _sc = 1f; _sp = 0; }

        // ------------------------------------------------------------ rectangles et images

        public void Rect(float x, float y, float w, float h, uint color)
        {
            RectGrad(x, y, w, h, color, color);
        }

        public void RectGrad(float x, float y, float w, float h, uint top, uint bottom)
        {
            float x0 = x * _sc + _tx, y0 = y * _sc + _ty, x1 = (x + w) * _sc + _tx, y1 = (y + h) * _sc + _ty;
            Sprite s = Atlas.White;
            List.Quad(x0, y0, x1, y0, x1, y1, x0, y1, s.U0, s.V0, s.U1, s.V1, top, top, bottom, bottom);
        }

        /// <summary>Image étirée dans un rectangle.</summary>
        public void Image(in Sprite s, float x, float y, float w, float h, uint color = Col.White)
        {
            float x0 = x * _sc + _tx, y0 = y * _sc + _ty, x1 = (x + w) * _sc + _tx, y1 = (y + h) * _sc + _ty;
            List.Quad(x0, y0, x1, y0, x1, y1, x0, y1, s.U0, s.V0, s.U1, s.V1, color, color, color, color);
        }

        /// <summary>Image centrée en (cx, cy), avec échelle et rotation (radians).</summary>
        public void Sprite(in Sprite s, float cx, float cy, float scale = 1f, float rot = 0f, uint color = Col.White)
        {
            SpriteXY(s, cx, cy, scale, scale, rot, color);
        }

        public void SpriteXY(in Sprite s, float cx, float cy, float sx, float sy, float rot, uint color)
        {
            float hw = s.W * 0.5f * sx * _sc, hh = s.H * 0.5f * sy * _sc;
            float px = cx * _sc + _tx, py = cy * _sc + _ty;
            if (rot == 0f)
            {
                List.Quad(px - hw, py - hh, px + hw, py - hh, px + hw, py + hh, px - hw, py + hh,
                          s.U0, s.V0, s.U1, s.V1, color, color, color, color);
                return;
            }
            float c = MathF.Cos(rot), n = MathF.Sin(rot);
            float ax = hw * c, ay = hw * n, bx = -hh * n, by = hh * c;
            List.Quad(px - ax - bx, py - ay - by, px + ax - bx, py + ay - by, px + ax + bx, py + ay + by, px - ax + bx, py - ay + by,
                      s.U0, s.V0, s.U1, s.V1, color, color, color, color);
        }

        /// <summary>Panneau extensible découpé en neuf zones : les coins gardent leur forme.</summary>
        public void Panel(in Sprite s, float x, float y, float w, float h, uint color = Col.White)
        {
            float bw = s.W * 0.5f, bh = s.H * 0.5f;
            if (w < bw * 2f) bw = w * 0.5f;
            if (h < bh * 2f) bh = h * 0.5f;
            float um = (s.U0 + s.U1) * 0.5f, vm = (s.V0 + s.V1) * 0.5f;
            // Tableaux sur la pile : aucune allocation mémoire par image.
            Span<float> xs = stackalloc float[4];
            Span<float> ys = stackalloc float[4];
            Span<float> us = stackalloc float[4];
            Span<float> vs = stackalloc float[4];
            xs[0] = x; xs[1] = x + bw; xs[2] = x + w - bw; xs[3] = x + w;
            ys[0] = y; ys[1] = y + bh; ys[2] = y + h - bh; ys[3] = y + h;
            us[0] = s.U0; us[1] = um; us[2] = um; us[3] = s.U1;
            vs[0] = s.V0; vs[1] = vm; vs[2] = vm; vs[3] = s.V1;
            for (int j = 0; j < 3; j++)
                for (int i = 0; i < 3; i++)
                {
                    if (xs[i + 1] - xs[i] <= 0.001f || ys[j + 1] - ys[j] <= 0.001f) continue;
                    float x0 = xs[i] * _sc + _tx, y0 = ys[j] * _sc + _ty, x1 = xs[i + 1] * _sc + _tx, y1 = ys[j + 1] * _sc + _ty;
                    List.Quad(x0, y0, x1, y0, x1, y1, x0, y1, us[i], vs[j], us[i + 1], vs[j + 1], color, color, color, color);
                }
        }

        // ------------------------------------------------------------ texte

        private FontSet Pick(float size) { return (FontLarge != null && size > FontSmall.Size * 1.25f) ? FontLarge : FontSmall; }

        public float Measure(string text, float size)
        {
            if (string.IsNullOrEmpty(text)) return 0f;
            FontSet f = Pick(size);
            float k = size / f.Size, w = 0f;
            for (int i = 0; i < text.Length; i++)
                if (f.Glyphs.TryGetValue(text[i], out Glyph g)) w += g.Adv * k;
            return w;
        }

        /// <summary>Écrit un texte. y est la ligne de base. Renvoie la largeur écrite.</summary>
        public float Text(string text, float x, float y, float size, uint color, Align align = Align.Left)
        {
            if (string.IsNullOrEmpty(text)) return 0f;
            FontSet f = Pick(size);
            float k = size / f.Size;
            float width = Measure(text, size);
            float pen = align == Align.Left ? x : (align == Align.Center ? x - width * 0.5f : x - width);
            for (int i = 0; i < text.Length; i++)
            {
                if (!f.Glyphs.TryGetValue(text[i], out Glyph g)) continue;
                if (g.Visible)
                {
                    float gx = pen + g.BearX * k, gy = y - g.BearY * k;
                    Image(g.S, gx, gy, g.S.W * k, g.S.H * k, color);
                }
                pen += g.Adv * k;
            }
            return width;
        }

        /// <summary>Texte centré verticalement sur cy (hauteur des capitales).</summary>
        public float TextMid(string text, float x, float cy, float size, uint color, Align align = Align.Center)
        {
            FontSet f = Pick(size);
            return Text(text, x, cy + f.CapHeight * (size / f.Size) * 0.5f, size, color, align);
        }

        /// <summary>Texte avec contour, lisible sur un fond chargé.</summary>
        public float TextOutlined(string text, float x, float cy, float size, uint color, uint outline, float ow, Align align = Align.Center)
        {
            for (int i = 0; i < 8; i++)
            {
                float a = i * (Mathx.Tau / 8f);
                TextMid(text, x + MathF.Cos(a) * ow, cy + MathF.Sin(a) * ow, size, outline, align);
            }
            TextMid(text, x, cy + ow * 0.9f, size, outline, align);
            return TextMid(text, x, cy, size, color, align);
        }
    }

    /// <summary>Mise en forme des nombres affichés.</summary>
    public static class Fmt
    {
        /// <summary>Nombre court : 950, 1,2k, 34k, 5,6M.</summary>
        public static string Short(double v, bool french)
        {
            string sep = french ? "," : ".";
            if (v < 0) return "-" + Short(-v, french);
            if (v < 1000) return ((long)Math.Round(v)).ToString();
            string[] units = { "k", "M", "G", "T" };
            int u = -1;
            while (v >= 999.5 && u < units.Length - 1) { v /= 1000.0; u++; }
            if (v >= 99.95) return ((long)Math.Round(v)).ToString() + units[u];
            long tenths = (long)Math.Round(v * 10.0);
            long whole = tenths / 10, frac = tenths % 10;
            return frac == 0 ? whole.ToString() + units[u] : whole.ToString() + sep + frac.ToString() + units[u];
        }
    }
}
