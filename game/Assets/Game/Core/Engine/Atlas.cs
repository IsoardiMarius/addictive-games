using System;
using System.Collections.Generic;

namespace Game.Core.Engine
{
    /// <summary>Zone de la texture commune. W et H sont en points d'écran.</summary>
    public struct Sprite
    {
        public float U0, V0, U1, V1;
        public float W, H;
    }

    public struct Glyph
    {
        public Sprite S;
        public float BearX, BearY, Adv;
        public bool Visible;
    }

    /// <summary>Jeu de caractères dessinés à une taille donnée.</summary>
    public sealed class FontSet
    {
        public float Size;
        public float Ascent, Descent, CapHeight;
        public readonly Dictionary<int, Glyph> Glyphs = new Dictionary<int, Glyph>();
    }

    /// <summary>
    /// Texture unique qui contient toutes les images et tous les caractères du jeu.
    /// Elle est produite au démarrage, à la résolution de l'écran : le rendu reste net partout.
    /// </summary>
    public sealed class Atlas
    {
        public readonly int Width, Height;
        public readonly byte[] Pixels;
        /// <summary>Pixels de texture par point d'écran.</summary>
        public readonly float Scale;
        public Sprite White;

        private int _x = 1, _y = 1, _shelf;
        private long _used;

        public Atlas(int width, int height, float scale)
        {
            Width = width; Height = height; Scale = scale;
            Pixels = new byte[width * height * 4];
            var c = new Canvas(8, 8);
            c.Color(0xFFFFFF).RoundRect(-4, -4, 16, 16, 0);
            Sprite s = Add(c);
            // Le « pixel blanc » pointe au centre du bloc : aucune bavure possible.
            float u = (s.U0 + s.U1) * 0.5f, v = (s.V0 + s.V1) * 0.5f;
            s.U0 = s.U1 = u; s.V0 = s.V1 = v;
            White = s;
        }

        public float FillRatio { get { return _used / (float)((long)Width * Height); } }

        public Sprite Add(Canvas c)
        {
            if (_x + c.W + 1 > Width) { _x = 1; _y += _shelf + 2; _shelf = 0; }
            if (_y + c.H + 1 > Height)
                throw new InvalidOperationException("Atlas plein : augmenter sa taille ou réduire l'échelle.");
            c.CopyTo(Pixels, Width, _x, _y);
            Sprite s;
            s.U0 = _x / (float)Width; s.V0 = _y / (float)Height;
            s.U1 = (_x + c.W) / (float)Width; s.V1 = (_y + c.H) / (float)Height;
            s.W = c.W / Scale; s.H = c.H / Scale;
            _x += c.W + 2;
            if (c.H > _shelf) _shelf = c.H;
            _used += (long)(c.W + 2) * (c.H + 2);
            return s;
        }

        public FontSet AddFont(TrueTypeFont font, float sizePt, string chars)
        {
            var set = new FontSet { Size = sizePt, Ascent = font.Ascent * sizePt, Descent = font.Descent * sizePt };
            float pxPerEm = sizePt * Scale;
            for (int i = 0; i < chars.Length; i++)
            {
                int cp = chars[i];
                if (set.Glyphs.ContainsKey(cp)) continue;
                if (cp != ' ' && !font.Has(cp)) continue;
                GlyphBitmap gb = font.Render(cp, pxPerEm);
                Glyph g = default(Glyph);
                g.Adv = gb.Advance / Scale;
                if (gb.Coverage != null)
                {
                    var c = new Canvas(gb.Width, gb.Height);
                    c.Color(0xFFFFFF).Mask(gb.Coverage, gb.Width, gb.Height, 0, 0);
                    g.S = Add(c);
                    g.BearX = gb.BearingX / Scale; g.BearY = gb.BearingY / Scale;
                    g.Visible = true;
                    if (cp == 'H') set.CapHeight = (gb.Height - 2) / Scale;
                }
                set.Glyphs[cp] = g;
            }
            if (set.CapHeight <= 0f) set.CapHeight = sizePt * 0.7f;
            return set;
        }
    }
}
