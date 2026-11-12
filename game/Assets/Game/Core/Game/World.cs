using System;
using System.Collections.Generic;
using Game.Core.Engine;

namespace Game.Core
{
    public sealed class GateBand
    {
        public int Row;
        public GateFx A, B;      // A : portail de gauche, B : portail de droite
        public bool Taken;
        public int TakenSide;
    }

    public sealed class SealState
    {
        public int Layer, Row;
        public float Hp, MaxHp;
        public bool Broken;
    }

    /// <summary>
    /// Le sous-sol : une grille de blocs produite ligne par ligne à partir d'une graine.
    /// La ligne 0 est le haut de la couche de départ ; y augmente vers le bas.
    /// </summary>
    public sealed class World
    {
        public const int Cols = Balance.Cols;
        public const int FinaleRows = 26;
        /// <summary>Largeur maximale de socle par ligne : il reste au moins sept colonnes pour un engin large.</summary>
        public const int MaxSoclePerRow = Cols - Balance.MaxPods;

        public readonly List<byte[]> Types = new List<byte[]>();
        public readonly List<float[]> Hp = new List<float[]>();
        public readonly List<byte[]> Aux = new List<byte[]>();
        public readonly List<int> RowLayer = new List<int>();
        public readonly List<GateBand> Gates = new List<GateBand>();
        public readonly List<SealState> Seals = new List<SealState>();

        public readonly uint Seed;
        public readonly int StartLayer;
        public bool Complete;
        private int _nextLayer;
        private readonly bool _tutorial;

        public int Rows { get { return Types.Count; } }

        public World(int startLayer, uint seed, bool tutorial)
        {
            StartLayer = startLayer; Seed = seed; _nextLayer = startLayer; _tutorial = tutorial;
            EnsureRows(60);
        }

        public void EnsureRows(int upToRow)
        {
            while (Rows <= upToRow && !Complete) AppendLayer(_nextLayer++);
        }

        public CellType Get(int c, int r)
        {
            if (c < 0 || c >= Cols) return CellType.Socle;
            if (r < 0) return CellType.Empty;
            if (r >= Rows) return CellType.Socle;
            return (CellType)Types[r][c];
        }

        public int LayerAt(int r)
        {
            if (RowLayer.Count == 0) return StartLayer;
            return RowLayer[Mathx.Clamp(r, 0, RowLayer.Count - 1)];
        }

        public LayerDef DefAt(int r) { return Balance.Layers[LayerAt(r)]; }

        public float MaxHp(int c, int r)
        {
            CellType t = Get(c, r);
            if (r < 0 || r >= Rows) return 1f;
            float hp = Balance.BaseHp(t) * DefAt(r).HpMul;
            if (t == CellType.Ore)
            {
                int host = Aux[r][c];
                hp *= host == 2 ? 2.2f : (host == 0 ? 0.6f : 1f);
            }
            return hp;
        }

        public void Set(int c, int r, CellType t, byte aux = 0)
        {
            if (c < 0 || c >= Cols || r < 0 || r >= Rows) return;
            Types[r][c] = (byte)t;
            Aux[r][c] = aux;
            Hp[r][c] = 0f;
            Hp[r][c] = MaxHp(c, r);
        }

        public SealState SealAt(int r)
        {
            for (int i = 0; i < Seals.Count; i++)
                if (r >= Seals[i].Row && r < Seals[i].Row + Balance.SealRows) return Seals[i];
            return null;
        }

        /// <summary>Premier sceau intact situé sous la ligne donnée.</summary>
        public SealState NextSeal(float y)
        {
            for (int i = 0; i < Seals.Count; i++)
                if (!Seals[i].Broken && Seals[i].Row + Balance.SealRows > y) return Seals[i];
            return null;
        }

        private void AddRows(int count, int layer)
        {
            for (int i = 0; i < count; i++)
            {
                Types.Add(new byte[Cols]); Hp.Add(new float[Cols]); Aux.Add(new byte[Cols]); RowLayer.Add(layer);
            }
        }

        private static float Quantile(float[] values, float q)
        {
            float[] s = (float[])values.Clone();
            Array.Sort(s);
            return s[Mathx.Clamp((int)(q * (s.Length - 1)), 0, s.Length - 1)];
        }

        private void AppendLayer(int li)
        {
            if (li >= Balance.LayerCount)
            {
                // Après le dernier sceau : la chambre du Cœur, vide.
                AddRows(FinaleRows, Balance.LayerCount - 1);
                Complete = true;
                return;
            }

            LayerDef d = Balance.Layers[li];
            int L = d.Length, start = Rows;
            uint s = Seed * 2654435761u + (uint)li * 40503u + 17u;
            var rng = new Rng(((ulong)Seed << 20) ^ (ulong)(li * 7919 + 13));
            AddRows(L + Balance.SealApproachRows + Balance.SealRows, li);

            // 1. Matériaux de base, répartis par un bruit lissé : les duretés forment des veines.
            float[] n = new float[L * Cols];
            for (int r = 0; r < L; r++)
                for (int c = 0; c < Cols; c++)
                    n[r * Cols + c] = Noise.Fbm(c * 0.30f, r * 0.30f, s, 3);
            float tHard = Quantile(n, 1f - d.HardPct), tMed = Quantile(n, 1f - d.HardPct - d.MedPct);
            for (int r = 0; r < L; r++)
                for (int c = 0; c < Cols; c++)
                {
                    float v = n[r * Cols + c];
                    CellType t = v > tHard ? CellType.Hard : (v > tMed ? CellType.Medium : CellType.Soft);
                    if (r < 4) t = CellType.Soft;
                    Types[start + r][c] = (byte)t;
                }

            // 2. Cavernes.
            if (d.Cave > 0f)
            {
                float[] cave = new float[L * Cols];
                for (int r = 0; r < L; r++)
                    for (int c = 0; c < Cols; c++)
                        cave[r * Cols + c] = Noise.Fbm(c * 0.21f, r * 0.21f, s + 101u, 2);
                float tCave = Quantile(cave, 1f - d.Cave);
                for (int r = 6; r < L - 6; r++)
                    for (int c = 0; c < Cols; c++)
                        if (cave[r * Cols + c] > tCave) Types[start + r][c] = (byte)CellType.Empty;
            }

            // 3. Filons : plus riches au cœur de la roche dure (risque contre récompense).
            float[] ore = new float[L * Cols];
            for (int r = 0; r < L; r++)
                for (int c = 0; c < Cols; c++)
                    ore[r * Cols + c] = Noise.Fbm(c * 0.55f, r * 0.55f, s + 202u, 2)
                                        + (Types[start + r][c] == (byte)CellType.Hard ? 0.05f : 0f);
            float tOre = Quantile(ore, 1f - d.OrePct);
            for (int r = 4; r < L; r++)
                for (int c = 0; c < Cols; c++)
                {
                    byte cur = Types[start + r][c];
                    if (cur == (byte)CellType.Empty || ore[r * Cols + c] <= tOre) continue;
                    Aux[start + r][c] = (byte)(cur == (byte)CellType.Hard ? 2 : (cur == (byte)CellType.Soft ? 0 : 1));
                    Types[start + r][c] = (byte)CellType.Ore;
                }

            // 4. Socle : amas quasi indestructibles accrochés à un bord. Il reste toujours
            //    au moins sept colonnes libres, pour qu'un engin large puisse passer.
            int socles = (int)MathF.Round(L / 10f * d.Socle);
            int[] socleLeft = new int[L + 8], socleRight = new int[L + 8];
            for (int i = 0; i < socles; i++)
            {
                int row = rng.Range(8, Math.Max(9, L - 10));
                bool left = rng.Chance(0.5f);
                int w = rng.Range(2, 5), h = rng.Range(2, 5);
                for (int rr = 0; rr < h; rr++)
                {
                    int ww = Math.Max(1, w - Math.Abs(rr - h / 2));
                    int line = row + rr;
                    // Jamais plus de quatre colonnes de socle sur une même ligne, les deux bords confondus.
                    int other = left ? socleRight[line] : socleLeft[line];
                    ww = Math.Min(ww, MaxSoclePerRow - other);
                    if (ww <= 0) continue;
                    if (left) socleLeft[line] = Math.Max(socleLeft[line], ww); else socleRight[line] = Math.Max(socleRight[line], ww);
                    for (int cc = 0; cc < ww; cc++)
                        Put(start, L, left ? cc : Cols - 1 - cc, line, CellType.Socle, 0);
                }
            }

            // 5. Cristaux de batterie, par paires.
            if (d.Crystal > 0f)
                for (int i = 0, k = Math.Max(1, (int)MathF.Round(L / d.Crystal)); i < k; i++)
                {
                    int c = rng.Range(0, Cols - 1), r = rng.Range(6, Math.Max(7, L - 6));
                    Put(start, L, c, r, CellType.Crystal, 0);
                    if (rng.Chance(0.5f)) Put(start, L, c + 1, r, CellType.Crystal, 0); else Put(start, L, c, r + 1, CellType.Crystal, 0);
                }

            // 6. Veines de TNT : une explosion en déclenche une autre.
            if (d.Tnt > 0f)
                for (int i = 0, k = (int)MathF.Round(L / d.Tnt); i < k; i++)
                {
                    int c = rng.Range(0, Cols), r = rng.Range(8, Math.Max(9, L - 8));
                    int len = rng.Range(3, 7);
                    for (int j = 0; j < len; j++)
                    {
                        Put(start, L, c, r, CellType.Tnt, 0);
                        c = Mathx.Clamp(c + rng.Range(-1, 2), 0, Cols - 1);
                        if (rng.Chance(0.45f)) r++;
                    }
                }

            // 7. Rochers : ils tombent quand on creuse dessous.
            if (d.Boulder > 0f)
                for (int i = 0, k = (int)MathF.Round(L / d.Boulder); i < k; i++)
                {
                    int c = rng.Range(0, Cols), r = rng.Range(6, Math.Max(7, L - 8));
                    Put(start, L, c, r, CellType.Boulder, 0);
                    if (Types[start + r + 1][c] == (byte)CellType.Empty) Put(start, L, c, r + 1, CellType.Soft, 0);
                }

            // 8. Poches de lave, enfermées dans la roche jusqu'à ce qu'on les perce.
            if (d.Lava > 0f)
                for (int i = 0, k = (int)MathF.Round(L / d.Lava); i < k; i++)
                {
                    int c = rng.Range(1, Cols - 2), r = rng.Range(10, Math.Max(11, L - 10));
                    for (int dc = -1; dc <= 1; dc++) Put(start, L, c + dc, r, CellType.Lava, 0);
                    Put(start, L, c, r + 1, CellType.Lava, 0);
                    if (rng.Chance(0.6f)) Put(start, L, c + (rng.Chance(0.5f) ? 1 : -1), r + 1, CellType.Lava, 0);
                    for (int rr = r - 1; rr <= r + 2; rr++)
                        for (int cc = c - 2; cc <= c + 2; cc++)
                        {
                            if (cc < 0 || cc >= Cols || rr < 4 || rr >= L) continue;
                            if (Types[start + rr][cc] == (byte)CellType.Empty) Types[start + rr][cc] = (byte)CellType.Medium;
                        }
                }

            // 9. Coffres, sertis dans la roche dure près d'un bord.
            for (int i = 0; i < d.Chests; i++)
            {
                int r = rng.Range(L / 4, Math.Max(L / 4 + 1, 3 * L / 4));
                int c = rng.Chance(0.5f) ? rng.Range(1, 3) : rng.Range(Cols - 3, Cols - 1);
                for (int rr = r - 1; rr <= r + 1; rr++)
                    for (int cc = c - 1; cc <= c + 1; cc++)
                        Put(start, L, cc, rr, CellType.Hard, 0);
                Put(start, L, c, r, CellType.Chest, 0);
            }

            // 10. Portails : une paire tous les 22 à 28 mètres.
            bool tutorialLayer = _tutorial && li == StartLayer;
            int g = tutorialLayer ? 7 : rng.Range(10, 15);
            int plusPods = 0;
            bool first = true;
            while (g < L - 9)
            {
                var band = new GateBand { Row = start + g };
                PickEffects(ref rng, band, tutorialLayer && first, ref plusPods);
                first = false;
                int index = Gates.Count;
                Gates.Add(band);
                for (int rr = g - 2; rr <= g + 2; rr++)
                    for (int c = 0; c < Cols; c++)
                    {
                        bool inBand = rr == g || rr == g + 1;
                        if (!inBand) { Types[start + rr][c] = (byte)CellType.Soft; Aux[start + rr][c] = 0; }
                        else if (c == Cols / 2) { Types[start + rr][c] = (byte)CellType.Medium; Aux[start + rr][c] = 0; }
                        else { Types[start + rr][c] = (byte)CellType.Gate; Aux[start + rr][c] = (byte)(index * 2 + (c < Cols / 2 ? 0 : 1)); }
                    }
                g += rng.Range(Balance.GateSpacingMin, Balance.GateSpacingMax + 1);
            }

            // 11. Zone d'approche puis sceau.
            for (int r = L; r < L + Balance.SealApproachRows; r++)
                for (int c = 0; c < Cols; c++) Types[start + r][c] = (byte)CellType.Soft;
            for (int r = L + Balance.SealApproachRows; r < L + Balance.SealApproachRows + Balance.SealRows; r++)
                for (int c = 0; c < Cols; c++) Types[start + r][c] = (byte)CellType.Seal;
            float sealHp = Balance.SealHp(li);
            Seals.Add(new SealState { Layer = li, Row = start + L + Balance.SealApproachRows, Hp = sealHp, MaxHp = sealHp });

            // Points de résistance initiaux.
            for (int r = start; r < Rows; r++)
                for (int c = 0; c < Cols; c++) Hp[r][c] = MaxHp(c, r);
        }

        private void Put(int start, int length, int c, int r, CellType t, byte aux)
        {
            if (c < 0 || c >= Cols || r < 4 || r >= length - 1) return;
            Types[start + r][c] = (byte)t;
            Aux[start + r][c] = aux;
        }

        private static void PickEffects(ref Rng rng, GateBand band, bool tutorial, ref int plusPods)
        {
            if (tutorial) { band.A = GateFx.PlusPod; band.B = GateFx.EnergyDown; plusPods++; return; }
            GateFx Bonus(ref Rng r, ref int pods)
            {
                while (true)
                {
                    float x = r.Next01() * 8.8f;
                    GateFx fx = x < 1.2f ? GateFx.PlusPod : (x < 3.2f ? GateFx.Power : (x < 4.5f ? GateFx.Speed : (x < 6.5f ? GateFx.Energy : (x < 7.3f ? GateFx.Magnet : GateFx.Loot))));
                    if (fx == GateFx.PlusPod) { if (pods >= 2) continue; pods++; }
                    return fx;
                }
            }
            GateFx a = Bonus(ref rng, ref plusPods), b;
            if (rng.Chance(0.3f))
            {
                float x = rng.Next01();
                b = x < 0.5f ? GateFx.EnergyDown : (x < 0.85f ? GateFx.PowerDown : GateFx.MinusPod);
            }
            else
            {
                do { b = Bonus(ref rng, ref plusPods); } while (b == a);
            }
            if (rng.Chance(0.5f)) { band.A = a; band.B = b; } else { band.A = b; band.B = a; }
        }
    }
}
