using System;
using System.IO;
using System.IO.Compression;
using Game.Core.Engine;

namespace Headless;

/// <summary>
/// Rendu logiciel du banc d'essai : dessine la même liste de rectangles que celle envoyée à Unity,
/// avec le même mélange de couleurs (alpha prémultiplié), et l'enregistre en PNG.
/// </summary>
public sealed class SoftRaster
{
    public readonly int W, H;
    public readonly float Scale;
    private readonly float[] _fb;

    public SoftRaster(int w, int h, float scale)
    {
        W = w; H = h; Scale = scale;
        _fb = new float[w * h * 3];
    }

    public void Clear(float r, float g, float b)
    {
        for (int i = 0; i < W * H; i++) { _fb[i * 3] = r; _fb[i * 3 + 1] = g; _fb[i * 3 + 2] = b; }
    }

    public void Draw(DrawList list, Atlas atlas)
    {
        var v = list.Verts;
        for (int q = 0; q < list.Count; q += 4)
        {
            // Le second triangle exclut la diagonale commune : aucun pixel n'est dessiné deux fois.
            Tri(atlas, v[q], v[q + 1], v[q + 2], false);
            Tri(atlas, v[q], v[q + 2], v[q + 3], true);
        }
    }

    private static float Edge(float ax, float ay, float bx, float by, float px, float py)
    {
        return (bx - ax) * (py - ay) - (by - ay) * (px - ax);
    }

    private void Tri(Atlas atlas, Vertex a, Vertex b, Vertex c, bool strictC)
    {
        float ax = a.X * Scale, ay = a.Y * Scale, bx = b.X * Scale, by = b.Y * Scale, cx = c.X * Scale, cy = c.Y * Scale;
        float area = Edge(ax, ay, bx, by, cx, cy);
        if (MathF.Abs(area) < 1e-6f) return;
        int x0 = Math.Max(0, (int)MathF.Floor(MathF.Min(ax, MathF.Min(bx, cx))));
        int x1 = Math.Min(W - 1, (int)MathF.Ceiling(MathF.Max(ax, MathF.Max(bx, cx))));
        int y0 = Math.Max(0, (int)MathF.Floor(MathF.Min(ay, MathF.Min(by, cy))));
        int y1 = Math.Min(H - 1, (int)MathF.Ceiling(MathF.Max(ay, MathF.Max(by, cy))));
        float inv = 1f / area;
        bool flat = a.Color == b.Color && b.Color == c.Color;
        bool solid = a.U == b.U && b.U == c.U && a.V == b.V && b.V == c.V;
        float[] ca = Unpack(a.Color), cb = Unpack(b.Color), cc = Unpack(c.Color);
        byte[] tex = atlas.Pixels;
        int tw = atlas.Width, th = atlas.Height;
        float sr = 1f, sg = 1f, sb = 1f, sa = 1f;
        if (solid) Sample(tex, tw, th, a.U, a.V, out sr, out sg, out sb, out sa);

        for (int y = y0; y <= y1; y++)
        {
            float py = y + 0.5f;
            for (int x = x0; x <= x1; x++)
            {
                float px = x + 0.5f;
                float w0 = Edge(bx, by, cx, cy, px, py) * inv;
                float w1 = Edge(cx, cy, ax, ay, px, py) * inv;
                float w2 = 1f - w0 - w1;
                if (w0 < 0f || w1 < 0f || w2 < 0f || (strictC && w2 <= 0f)) continue;
                float tr, tg, tb, ta;
                if (solid) { tr = sr; tg = sg; tb = sb; ta = sa; }
                else Sample(tex, tw, th, a.U * w0 + b.U * w1 + c.U * w2, a.V * w0 + b.V * w1 + c.V * w2, out tr, out tg, out tb, out ta);
                float r, g, bl, al;
                if (flat) { r = ca[0]; g = ca[1]; bl = ca[2]; al = ca[3]; }
                else
                {
                    r = ca[0] * w0 + cb[0] * w1 + cc[0] * w2; g = ca[1] * w0 + cb[1] * w1 + cc[1] * w2;
                    bl = ca[2] * w0 + cb[2] * w1 + cc[2] * w2; al = ca[3] * w0 + cb[3] * w1 + cc[3] * w2;
                }
                r *= tr; g *= tg; bl *= tb; al *= ta;
                if (r <= 0f && g <= 0f && bl <= 0f && al <= 0f) continue;
                int i = (y * W + x) * 3;
                float k = 1f - al;
                _fb[i] = r + _fb[i] * k; _fb[i + 1] = g + _fb[i + 1] * k; _fb[i + 2] = bl + _fb[i + 2] * k;
            }
        }
    }

    private static float[] Unpack(uint c)
    {
        return new[] { (c & 255) / 255f, ((c >> 8) & 255) / 255f, ((c >> 16) & 255) / 255f, ((c >> 24) & 255) / 255f };
    }

    private static void Sample(byte[] tex, int tw, int th, float u, float v, out float r, out float g, out float b, out float a)
    {
        float fx = u * tw - 0.5f, fy = v * th - 0.5f;
        int x0 = (int)MathF.Floor(fx), y0 = (int)MathF.Floor(fy);
        float tx = fx - x0, ty = fy - y0;
        int x1 = Math.Clamp(x0 + 1, 0, tw - 1), y1 = Math.Clamp(y0 + 1, 0, th - 1);
        x0 = Math.Clamp(x0, 0, tw - 1); y0 = Math.Clamp(y0, 0, th - 1);
        int i00 = (y0 * tw + x0) * 4, i10 = (y0 * tw + x1) * 4, i01 = (y1 * tw + x0) * 4, i11 = (y1 * tw + x1) * 4;
        float w00 = (1 - tx) * (1 - ty), w10 = tx * (1 - ty), w01 = (1 - tx) * ty, w11 = tx * ty;
        const float k = 1f / 255f;
        r = (tex[i00] * w00 + tex[i10] * w10 + tex[i01] * w01 + tex[i11] * w11) * k;
        g = (tex[i00 + 1] * w00 + tex[i10 + 1] * w10 + tex[i01 + 1] * w01 + tex[i11 + 1] * w11) * k;
        b = (tex[i00 + 2] * w00 + tex[i10 + 2] * w10 + tex[i01 + 2] * w01 + tex[i11 + 2] * w11) * k;
        a = (tex[i00 + 3] * w00 + tex[i10 + 3] * w10 + tex[i01 + 3] * w01 + tex[i11 + 3] * w11) * k;
    }

    public void SavePng(string path)
    {
        byte[] rgba = new byte[W * H * 4];
        for (int i = 0; i < W * H; i++)
        {
            rgba[i * 4] = (byte)Math.Clamp((int)(_fb[i * 3] * 255f + 0.5f), 0, 255);
            rgba[i * 4 + 1] = (byte)Math.Clamp((int)(_fb[i * 3 + 1] * 255f + 0.5f), 0, 255);
            rgba[i * 4 + 2] = (byte)Math.Clamp((int)(_fb[i * 3 + 2] * 255f + 0.5f), 0, 255);
            rgba[i * 4 + 3] = 255;
        }
        Png.Save(path, rgba, W, H);
    }
}

public static class Png
{
    private static readonly uint[] Table = MakeTable();

    private static uint[] MakeTable()
    {
        var t = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            uint c = n;
            for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            t[n] = c;
        }
        return t;
    }

    private static void Chunk(Stream s, string type, byte[] data)
    {
        byte[] len = { (byte)(data.Length >> 24), (byte)(data.Length >> 16), (byte)(data.Length >> 8), (byte)data.Length };
        s.Write(len);
        byte[] t = System.Text.Encoding.ASCII.GetBytes(type);
        s.Write(t);
        s.Write(data);
        uint crc = 0xFFFFFFFFu;
        foreach (byte b in t) crc = Table[(crc ^ b) & 255] ^ (crc >> 8);
        foreach (byte b in data) crc = Table[(crc ^ b) & 255] ^ (crc >> 8);
        crc ^= 0xFFFFFFFFu;
        s.Write(new[] { (byte)(crc >> 24), (byte)(crc >> 16), (byte)(crc >> 8), (byte)crc });
    }

    /// <summary>Enregistre une image RGBA 8 bits (alpha non prémultiplié) en PNG.</summary>
    public static void Save(string path, byte[] rgba, int w, int h)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var fs = File.Create(path);
        fs.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        byte[] hdr = { (byte)(w >> 24), (byte)(w >> 16), (byte)(w >> 8), (byte)w, (byte)(h >> 24), (byte)(h >> 16), (byte)(h >> 8), (byte)h, 8, 6, 0, 0, 0 };
        Chunk(fs, "IHDR", hdr);
        using var ms = new MemoryStream();
        using (var z = new ZLibStream(ms, CompressionLevel.Fastest, true))
        {
            for (int y = 0; y < h; y++)
            {
                z.WriteByte(0);
                z.Write(rgba, y * w * 4, w * 4);
            }
        }
        Chunk(fs, "IDAT", ms.ToArray());
        Chunk(fs, "IEND", Array.Empty<byte>());
    }

    /// <summary>Enregistre l'atlas (alpha prémultiplié) sur fond gris pour inspection visuelle.</summary>
    public static void SaveAtlas(string path, Atlas atlas)
    {
        int w = atlas.Width, h = atlas.Height;
        byte[] outp = new byte[w * h * 4];
        for (int i = 0; i < w * h; i++)
        {
            float a = atlas.Pixels[i * 4 + 3] / 255f;
            bool dark = ((i % w) / 32 + (i / w) / 32) % 2 == 0;
            float bg = dark ? 60f : 80f;
            outp[i * 4] = (byte)Math.Min(255f, atlas.Pixels[i * 4] + bg * (1 - a));
            outp[i * 4 + 1] = (byte)Math.Min(255f, atlas.Pixels[i * 4 + 1] + bg * (1 - a));
            outp[i * 4 + 2] = (byte)Math.Min(255f, atlas.Pixels[i * 4 + 2] + bg * (1 - a));
            outp[i * 4 + 3] = 255;
        }
        Save(path, outp, w, h);
    }
}
