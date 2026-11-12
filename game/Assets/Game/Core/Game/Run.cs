using System;
using System.Collections.Generic;
using Game.Core.Engine;

namespace Game.Core
{
    public enum Ev { Break, Pickup, Gate, Explosion, BoulderHit, BoulderLand, SealBreak, LowEnergy, Over, Chest }

    /// <summary>Ce qui vient de se passer dans la simulation ; l'affichage et le son s'en servent.</summary>
    public struct RunEvent
    {
        public Ev Type;
        public float X, Y, V;
        public int A, B;
    }

    public struct Pickup
    {
        public float X, Y, Vx, Vy, Age, Value;
        public byte Kind;   // 0 : minerai, 1 : énergie, 2 : pièce
        public byte Layer;
    }

    /// <summary>
    /// Une descente. Simulation pure et déterministe : aucune dépendance à l'affichage ni à Unity,
    /// ce qui permet de la tester et de la faire jouer par un robot.
    /// Unités : 1 = un bloc ; x de 0 à Cols ; y vers le bas.
    /// </summary>
    public sealed class Run
    {
        private struct Boom { public int C, R; public float T; }

        public readonly World W;
        public readonly int StartLayer;

        // Caractéristiques issues du garage.
        public float BasePower, BaseSpeed, EnergyMax, BaseMagnet, RefineMul, LavaResist;
        // Modificateurs gagnés aux portails pendant la descente.
        public float PowerMul = 1f, SpeedMul = 1f, MagnetMul = 1f, LootMul = 1f;
        public int Pods;

        public float X, Y, Vx, Tilt, Energy, Time, Load, SteerX;
        public bool Blocked, FreeFall, InLava, OnSeal, Over, Won, Finished;
        public int LayersCleared, Blocks, OreCells, GatesGood, GatesBad, BoulderHits;
        public double Coins;
        public float LavaTime;
        public bool Revived;
        /// <summary>Niveaux du garage, pour l'apparence de l'engin.</summary>
        public int DrillLevel, EngineLevel;

        public readonly List<Pickup> Pickups = new List<Pickup>(256);
        public readonly List<RunEvent> Events = new List<RunEvent>(64);

        private readonly List<Boom> _booms = new List<Boom>();
        private float _caAcc, _finishTimer;
        private bool _lowWarned;
        private Rng _rng;
        private bool[] _moved = new bool[0];
        private int _tick;

        public float Power { get { return BasePower * PowerMul; } }
        public float Speed { get { return BaseSpeed * SpeedMul; } }
        public float Magnet { get { return BaseMagnet * MagnetMul; } }
        public float HalfWidth { get { return (Pods - 1) * 0.5f + Balance.PodRadius; } }
        public int CurrentLayer { get { return W.LayerAt((int)MathF.Max(0f, Y)); } }
        public int DepthMeters { get { return Balance.LayerStartDepth(StartLayer) + (int)MathF.Max(0f, Y); } }
        /// <summary>Pièces obtenues à la vente (raffinerie comprise).</summary>
        public long SaleValue { get { return (long)Math.Round(Coins * RefineMul); } }

        public float PodX(int i) { return X + (i - (Pods - 1) * 0.5f); }

        public Run(int startLayer, uint seed, int[] up, bool tutorial)
        {
            StartLayer = startLayer;
            W = new World(startLayer, seed, tutorial);
            _rng = new Rng(seed * 31u + 7u);
            BasePower = Balance.Power(up[(int)Upgrade.Drill]);
            BaseSpeed = Balance.Speed(up[(int)Upgrade.Engine]);
            EnergyMax = Balance.BatterySeconds(up[(int)Upgrade.Battery]);
            Pods = Balance.StartPods(up[(int)Upgrade.Chassis]);
            LavaResist = Balance.LavaResist(up[(int)Upgrade.Chassis]);
            BaseMagnet = Balance.MagnetRadius(up[(int)Upgrade.Magnet]);
            RefineMul = Balance.RefineryMul(up[(int)Upgrade.Refinery]);
            DrillLevel = up[(int)Upgrade.Drill]; EngineLevel = up[(int)Upgrade.Engine];
            Energy = EnergyMax;
            X = SteerX = World.Cols * 0.5f;
            Y = -1.2f;
        }

        private void Emit(Ev type, float x, float y, int a = 0, int b = 0, float v = 0f)
        {
            RunEvent e; e.Type = type; e.X = x; e.Y = y; e.A = a; e.B = b; e.V = v;
            Events.Add(e);
        }

        private static bool Solid(CellType t) { return t != CellType.Empty && t != CellType.Lava && t != CellType.Gate; }

        private static bool CircleHitsCell(float cx, float cy, float rad, int c, int r)
        {
            float nx = Mathx.Clamp(cx, c, c + 1f), ny = Mathx.Clamp(cy, r, r + 1f);
            float dx = cx - nx, dy = cy - ny;
            return dx * dx + dy * dy < rad * rad;
        }

        /// <summary>Le joueur interrompt la descente : le minerai déjà ramassé est quand même vendu.</summary>
        public void Abort() { Over = true; }

        /// <summary>Relance la descente après une panne (pub récompensée).</summary>
        public void Revive()
        {
            Over = false; Revived = true; _lowWarned = false;
            Energy = EnergyMax * 0.5f;
        }

        // ------------------------------------------------------------------ boucle principale

        public void Step(float dt)
        {
            if (Over) return;
            Time += dt;
            W.EnsureRows((int)Y + 44);

            // Direction : l'engin rejoint la position demandée par le doigt.
            float minX = HalfWidth, maxX = World.Cols - HalfWidth;
            if (minX > maxX) minX = maxX = World.Cols * 0.5f;
            SteerX = Mathx.Clamp(SteerX, minX, maxX);
            float lat = Speed * Balance.LateralFactor;
            float want = Mathx.Clamp((SteerX - X) * 12f, -lat, lat);
            Vx = Mathx.Damp(Vx, want, 18f, dt);
            Tilt = Mathx.Damp(Tilt, Mathx.Clamp(Vx / lat, -1f, 1f), 10f, dt);

            // Forage : chaque foret entame tous les blocs qu'il touche.
            OnSeal = false;
            float dmg = Power * dt;
            for (int i = 0; i < Pods; i++) Bite(PodX(i), Y + Balance.BiteOffsetY, dmg);

            // Déplacement, par petits pas pour ne jamais traverser un bloc.
            FreeFall = !SolidBelow();
            float dy = Speed * (FreeFall ? Balance.FreeFallBoost : 1f) * dt, dx = Vx * dt;
            int steps = Math.Max(1, (int)MathF.Ceiling(MathF.Max(MathF.Abs(dx), MathF.Abs(dy)) / 0.07f));
            Blocked = false;
            for (int s = 0; s < steps; s++)
            {
                float ny = Y + dy / steps;
                if (!Collides(X, ny)) Y = ny; else Blocked = true;
                float nx = Mathx.Clamp(X + dx / steps, minX, maxX);
                if (!Collides(nx, Y)) X = nx; else Vx *= 0.6f;
            }
            Load = Mathx.Damp(Load, Blocked ? 1f : (FreeFall ? 0f : 0.4f), 12f, dt);

            CheckGates();

            // Lave.
            InLava = false;
            for (int i = 0; i < Pods && !InLava; i++)
            {
                float px = PodX(i);
                int c0 = (int)MathF.Floor(px - 0.4f), c1 = (int)MathF.Floor(px + 0.4f);
                int r0 = (int)MathF.Floor(Y - 0.4f), r1 = (int)MathF.Floor(Y + 0.4f);
                for (int r = r0; r <= r1 && !InLava; r++)
                    for (int c = c0; c <= c1; c++)
                        if (W.Get(c, r) == CellType.Lava && CircleHitsCell(px, Y, 0.38f, c, r)) { InLava = true; break; }
            }
            if (InLava)
            {
                Energy -= Balance.LavaDrainPerSec * (1f - LavaResist) * dt;
                LavaTime += dt;
            }

            // Rochers et lave : automate cellulaire à cadence fixe.
            _caAcc += dt;
            float tick = 1f / Balance.CaTickRate;
            while (_caAcc >= tick) { _caAcc -= tick; CaTick(); }

            // Explosions en attente.
            for (int i = _booms.Count - 1; i >= 0; i--)
            {
                Boom b = _booms[i];
                b.T -= dt;
                if (b.T <= 0f) { _booms.RemoveAt(i); Explode(b.C, b.R); }
                else _booms[i] = b;
            }

            UpdatePickups(dt);

            if (Finished)
            {
                _finishTimer -= dt;
                if (_finishTimer <= 0f) { Over = true; Emit(Ev.Over, X, Y); }
                return;
            }

            Energy -= dt;
            if (Energy <= Balance.LowEnergy && !_lowWarned) { _lowWarned = true; Emit(Ev.LowEnergy, X, Y); }
            if (Energy > Balance.LowEnergy + 1f) _lowWarned = false;
            if (Energy <= 0f) { Energy = 0f; Over = true; Emit(Ev.Over, X, Y); }
        }

        // ------------------------------------------------------------------ forage et collisions

        private void Bite(float px, float py, float dmg)
        {
            float rad = Balance.BiteRadius;
            int c0 = (int)MathF.Floor(px - rad), c1 = (int)MathF.Floor(px + rad);
            int r0 = Math.Max(0, (int)MathF.Floor(py - rad)), r1 = (int)MathF.Floor(py + rad);
            bool sealHit = false;
            for (int r = r0; r <= r1; r++)
                for (int c = c0; c <= c1; c++)
                {
                    if (c < 0 || c >= World.Cols || r >= W.Rows) continue;
                    CellType t = (CellType)W.Types[r][c];
                    if (t == CellType.Empty || t == CellType.Lava || t == CellType.Gate) continue;
                    if (!CircleHitsCell(px, py, rad, c, r)) continue;
                    if (t == CellType.Seal)
                    {
                        if (!sealHit) { sealHit = true; OnSeal = true; DamageSeal(W.SealAt(r), dmg); }
                        continue;
                    }
                    Damage(c, r, dmg, 0.10f);
                }
        }

        private void Damage(int c, int r, float dmg, float tntDelay)
        {
            float hp = W.Hp[r][c] - dmg;
            W.Hp[r][c] = hp;
            if (hp <= 0f) Break(c, r, tntDelay);
        }

        private void DamageSeal(SealState seal, float dmg)
        {
            if (seal == null || seal.Broken) return;
            seal.Hp -= dmg;
            if (seal.Hp > 0f) return;
            seal.Hp = 0f; seal.Broken = true;
            for (int r = seal.Row; r < seal.Row + Balance.SealRows; r++)
                for (int c = 0; c < World.Cols; c++) { W.Types[r][c] = (byte)CellType.Empty; W.Hp[r][c] = 0f; }
            LayersCleared++;
            if (seal.Layer == StartLayer) Won = true;
            Energy = MathF.Min(Energy + Balance.SealEnergyBonus, EnergyMax * 1.5f);
            Coins += Balance.SealReward(seal.Layer);
            Emit(Ev.SealBreak, World.Cols * 0.5f, seal.Row + 1f, seal.Layer, 0, Balance.SealReward(seal.Layer));
            if (seal.Layer >= Balance.LayerCount - 1) { Finished = true; _finishTimer = 3.2f; }
        }

        private void Break(int c, int r, float tntDelay)
        {
            CellType t = (CellType)W.Types[r][c];
            int layer = W.LayerAt(r);
            byte aux = W.Aux[r][c];
            W.Types[r][c] = (byte)CellType.Empty; W.Hp[r][c] = 0f; W.Aux[r][c] = 0;
            Blocks++;
            float cx = c + 0.5f, cy = r + 0.5f;
            LayerDef d = Balance.Layers[layer];
            switch (t)
            {
                case CellType.Ore:
                    OreCells++;
                    Spawn(cx, cy, 0, 3, Balance.OreCellCoins * d.OreValue / 3f, layer, 6f);
                    break;
                case CellType.Crystal:
                    Spawn(cx, cy, 1, 2, Balance.OrbEnergy, layer, 4f);
                    break;
                case CellType.Chest:
                    Spawn(cx, cy, 2, 10, Balance.ChestRewardBase * d.OreValue / 10f, layer, 7f);
                    Emit(Ev.Chest, cx, cy, 0, layer);
                    break;
                case CellType.Tnt:
                    Boom b; b.C = c; b.R = r; b.T = tntDelay;
                    _booms.Add(b);
                    break;
            }
            Emit(Ev.Break, cx, cy, (int)t, layer, aux);
        }

        private void Explode(int c, int r)
        {
            float rad = Balance.ExplosionRadius;
            Emit(Ev.Explosion, c + 0.5f, r + 0.5f);
            int ir = (int)MathF.Ceiling(rad);
            SealState sealDone = null;
            for (int rr = r - ir; rr <= r + ir; rr++)
                for (int cc = c - ir; cc <= c + ir; cc++)
                {
                    if (cc < 0 || cc >= World.Cols || rr < 0 || rr >= W.Rows) continue;
                    float dist = MathF.Sqrt((cc - c) * (cc - c) + (rr - r) * (rr - r));
                    if (dist > rad) continue;
                    CellType t = (CellType)W.Types[rr][cc];
                    if (t == CellType.Empty || t == CellType.Lava || t == CellType.Gate) continue;
                    float mul = W.DefAt(rr).HpMul;
                    if (t == CellType.Seal)
                    {
                        SealState s = W.SealAt(rr);
                        if (s != sealDone) { sealDone = s; DamageSeal(s, Balance.ExplosionDamage * 0.3f * mul); }
                        continue;
                    }
                    float d = Balance.ExplosionDamage * mul * (1f - 0.55f * dist / rad);
                    if (t == CellType.Socle) d *= 0.3f;
                    Damage(cc, rr, d, 0.06f + 0.05f * dist);
                }
            // Les explosions dispersent les objets voisins : spectaculaire, et l'aimant prend son sens.
            for (int i = 0; i < Pickups.Count; i++)
            {
                Pickup p = Pickups[i];
                float dx = p.X - (c + 0.5f), dy = p.Y - (r + 0.5f), d2 = dx * dx + dy * dy;
                if (d2 < rad * rad && d2 > 0.0001f)
                {
                    float d = MathF.Sqrt(d2);
                    p.Vx += dx / d * 5f; p.Vy += dy / d * 5f;
                    Pickups[i] = p;
                }
            }
        }

        private bool Collides(float x, float y)
        {
            float rad = Balance.PodRadius;
            for (int i = 0; i < Pods; i++)
            {
                float px = x + (i - (Pods - 1) * 0.5f);
                int c0 = (int)MathF.Floor(px - rad), c1 = (int)MathF.Floor(px + rad);
                int r0 = (int)MathF.Floor(y - rad), r1 = (int)MathF.Floor(y + rad);
                for (int r = r0; r <= r1; r++)
                    for (int c = c0; c <= c1; c++)
                        if (Solid(W.Get(c, r)) && CircleHitsCell(px, y, rad, c, r)) return true;
            }
            return false;
        }

        private bool SolidBelow()
        {
            for (int i = 0; i < Pods; i++)
            {
                float px = PodX(i), py = Y + 0.45f;
                int c0 = (int)MathF.Floor(px - 0.5f), c1 = (int)MathF.Floor(px + 0.5f);
                int r0 = (int)MathF.Floor(py - 0.5f), r1 = (int)MathF.Floor(py + 0.5f);
                for (int r = r0; r <= r1; r++)
                    for (int c = c0; c <= c1; c++)
                        if (Solid(W.Get(c, r)) && CircleHitsCell(px, py, 0.5f, c, r)) return true;
            }
            return false;
        }

        // ------------------------------------------------------------------ portails

        private void CheckGates()
        {
            float rad = Balance.PodRadius;
            for (int i = 0; i < Pods; i++)
            {
                float px = PodX(i);
                int c0 = (int)MathF.Floor(px - rad), c1 = (int)MathF.Floor(px + rad);
                int r0 = Math.Max(0, (int)MathF.Floor(Y - rad)), r1 = (int)MathF.Floor(Y + rad);
                for (int r = r0; r <= r1; r++)
                    for (int c = c0; c <= c1; c++)
                    {
                        if (c < 0 || c >= World.Cols || r >= W.Rows) continue;
                        if (W.Types[r][c] != (byte)CellType.Gate || !CircleHitsCell(px, Y, rad, c, r)) continue;
                        int aux = W.Aux[r][c];
                        TakeGate(aux / 2, aux & 1);
                    }
            }
        }

        private void TakeGate(int index, int side)
        {
            if (index < 0 || index >= W.Gates.Count) return;
            GateBand g = W.Gates[index];
            if (g.Taken) return;
            g.Taken = true; g.TakenSide = side;
            for (int r = g.Row; r < g.Row + 2; r++)
                for (int c = 0; c < World.Cols; c++)
                    if (W.Types[r][c] == (byte)CellType.Gate) { W.Types[r][c] = (byte)CellType.Empty; W.Aux[r][c] = 0; }
            GateFx fx = side == 0 ? g.A : g.B;
            Apply(fx);
            if (Balance.IsBonus(fx)) GatesGood++; else GatesBad++;
            float gx = side == 0 ? World.Cols * 0.25f : World.Cols * 0.75f;
            Emit(Ev.Gate, gx, g.Row + 1f, (int)fx, side);
        }

        public void Apply(GateFx fx)
        {
            switch (fx)
            {
                case GateFx.PlusPod: if (Pods < Balance.MaxPods) Pods++; else PowerMul *= 1.2f; break;
                case GateFx.Power: PowerMul *= Balance.GatePowerMul; break;
                case GateFx.Speed: SpeedMul *= Balance.GateSpeedMul; break;
                case GateFx.Energy: Energy = MathF.Min(Energy + Balance.GateEnergy, EnergyMax * 1.5f); break;
                case GateFx.Magnet: MagnetMul *= Balance.GateMagnetMul; break;
                case GateFx.Loot: LootMul += Balance.GateLootAdd; break;
                case GateFx.MinusPod: if (Pods > 1) Pods--; else Energy -= 3f; break;
                case GateFx.PowerDown: PowerMul *= Balance.GatePowerDownMul; break;
                case GateFx.EnergyDown: Energy -= Balance.GateEnergyDown; break;
            }
            float minX = HalfWidth, maxX = World.Cols - HalfWidth;
            if (minX <= maxX) X = Mathx.Clamp(X, minX, maxX);
        }

        // ------------------------------------------------------------------ rochers et lave

        private bool PodTouchesCell(int c, int r)
        {
            for (int i = 0; i < Pods; i++)
                if (CircleHitsCell(PodX(i), Y, Balance.PodRadius + 0.05f, c, r)) return true;
            return false;
        }

        private void CaTick()
        {
            _tick++;
            int r0 = Math.Max(0, (int)Y - 8), r1 = Math.Min(W.Rows - 2, (int)Y + 30);
            if (r1 < r0) return;
            int rows = r1 - r0 + 1;
            if (_moved.Length < rows * World.Cols) _moved = new bool[rows * World.Cols];
            Array.Clear(_moved, 0, rows * World.Cols);
            bool leftToRight = (_tick & 1) == 0;

            for (int r = r1; r >= r0; r--)
                for (int k = 0; k < World.Cols; k++)
                {
                    int c = leftToRight ? k : World.Cols - 1 - k;
                    if (_moved[(r - r0) * World.Cols + c]) continue;
                    CellType t = (CellType)W.Types[r][c];
                    if (t == CellType.Boulder)
                    {
                        if ((CellType)W.Types[r + 1][c] == CellType.Empty)
                        {
                            MoveCell(c, r, c, r + 1, 1);
                            if (r + 1 <= r1) _moved[(r + 1 - r0) * World.Cols + c] = true;
                            if (PodTouchesCell(c, r + 1))
                            {
                                W.Types[r + 1][c] = (byte)CellType.Empty; W.Hp[r + 1][c] = 0f; W.Aux[r + 1][c] = 0;
                                Energy -= Balance.BoulderHitCost;
                                BoulderHits++;
                                Emit(Ev.BoulderHit, c + 0.5f, r + 1.5f, 0, W.LayerAt(r));
                            }
                        }
                        else if (W.Aux[r][c] == 1)
                        {
                            W.Aux[r][c] = 0;
                            Emit(Ev.BoulderLand, c + 0.5f, r + 0.5f);
                        }
                    }
                    else if (t == CellType.Lava)
                    {
                        int life = W.Aux[r][c];
                        int tc = c, tr = r;
                        if ((CellType)W.Types[r + 1][c] == CellType.Empty) tr = r + 1;
                        else
                        {
                            int d = _rng.Chance(0.5f) ? 1 : -1;
                            if (W.Get(c + d, r) == CellType.Empty) tc = c + d;
                            else if (W.Get(c - d, r) == CellType.Empty) tc = c - d;
                        }
                        if (tc != c || tr != r)
                        {
                            if (life == 0) life = Balance.LavaLifeTicks;
                            MoveCell(c, r, tc, tr, (byte)life);
                            if (tr <= r1) _moved[(tr - r0) * World.Cols + tc] = true;
                        }
                        if (life > 0)
                        {
                            life--;
                            if (life <= 0) W.Set(tc, tr, CellType.Basalt);
                            else W.Aux[tr][tc] = (byte)life;
                        }
                    }
                }
        }

        private void MoveCell(int c, int r, int tc, int tr, byte aux)
        {
            W.Types[tr][tc] = W.Types[r][c]; W.Hp[tr][tc] = W.Hp[r][c]; W.Aux[tr][tc] = aux;
            W.Types[r][c] = (byte)CellType.Empty; W.Hp[r][c] = 0f; W.Aux[r][c] = 0;
        }

        // ------------------------------------------------------------------ objets à ramasser

        private void Spawn(float x, float y, byte kind, int count, float value, int layer, float burst)
        {
            for (int i = 0; i < count; i++)
            {
                if (Pickups.Count >= 300) { Collect(kind, value, x, y); continue; }
                Pickup p;
                float a = _rng.Range(0f, Mathx.Tau), sp = _rng.Range(0.35f, 1f) * burst;
                p.X = x + _rng.Range(-0.2f, 0.2f); p.Y = y + _rng.Range(-0.2f, 0.2f);
                p.Vx = MathF.Cos(a) * sp; p.Vy = MathF.Sin(a) * sp - 1.5f;
                p.Age = 0f; p.Value = value; p.Kind = kind; p.Layer = (byte)layer;
                Pickups.Add(p);
            }
        }

        private void Collect(byte kind, float value, float x, float y)
        {
            if (kind == 1) Energy = MathF.Min(Energy + value, EnergyMax * 1.5f);
            else Coins += value * LootMul;
            Emit(Ev.Pickup, x, y, kind, 0, kind == 1 ? value : value * LootMul);
        }

        private void UpdatePickups(float dt)
        {
            float drag = MathF.Exp(-4.5f * dt);
            float half = (Pods - 1) * 0.5f;
            float reach = Magnet + 0.2f;
            for (int i = Pickups.Count - 1; i >= 0; i--)
            {
                Pickup p = Pickups[i];
                p.Age += dt;
                float tx = Mathx.Clamp(p.X, X - half, X + half), ty = Y - 0.1f;
                float dx = tx - p.X, dy = ty - p.Y;
                float d = MathF.Sqrt(dx * dx + dy * dy);
                if (p.Age > 0.12f && d < reach && d > 0.001f)
                {
                    // Aspiration : l'objet rejoint l'engin plus vite que celui-ci ne descend.
                    float sp = 16f + Speed * 1.6f;
                    p.Vx = Mathx.Damp(p.Vx, dx / d * sp, 14f, dt);
                    p.Vy = Mathx.Damp(p.Vy, dy / d * sp, 14f, dt);
                }
                else { p.Vx *= drag; p.Vy *= drag; }
                p.X += p.Vx * dt; p.Y += p.Vy * dt;
                if (p.X < 0.2f) { p.X = 0.2f; p.Vx = MathF.Abs(p.Vx) * 0.5f; }
                if (p.X > World.Cols - 0.2f) { p.X = World.Cols - 0.2f; p.Vx = -MathF.Abs(p.Vx) * 0.5f; }
                if (p.Age > 0.12f && d < 0.62f)
                {
                    Collect(p.Kind, p.Value, p.X, p.Y);
                    Pickups.RemoveAt(i);
                    continue;
                }
                if (p.Y < Y - 14f || p.Age > 12f) { Pickups.RemoveAt(i); continue; }
                Pickups[i] = p;
            }
        }
    }
}
