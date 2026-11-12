using System;

namespace Game.Core
{
    public enum CellType : byte
    {
        Empty = 0, Soft, Medium, Hard, Socle, Ore, Crystal, Tnt, Boulder, Lava, Basalt, Chest, Gate, Seal
    }

    public enum GateFx
    {
        PlusPod, Power, Speed, Energy, Magnet, Loot,   // bonus
        MinusPod, PowerDown, EnergyDown                // malus
    }

    public enum Upgrade { Drill = 0, Engine, Battery, Chassis, Magnet, Refinery, Count }

    /// <summary>Paramètres d'une couche : c'est tout ce qu'il faut pour en créer une nouvelle.</summary>
    public sealed class LayerDef
    {
        public string Key;            // clé de traduction du nom
        public string OreKey;
        public int Length;            // lignes de terrain avant le sceau
        public uint Bg0, Bg1;         // dégradé du fond
        public uint Soft, Med, Hard;  // teintes des trois duretés
        public uint Ore;              // couleur du minerai
        public float MedPct, HardPct, OrePct;
        public float Socle;           // amas de socle pour 10 lignes
        public float Crystal, Tnt, Boulder, Lava; // un élément toutes les N lignes (0 = absent)
        public float Cave;            // part de vide (cavernes)
        public int Chests = 1;
        public float HpMul;           // multiplicateur de résistance des blocs
        public float OreValue;        // valeur d'un filon, en pièces
    }

    /// <summary>
    /// Tous les réglages chiffrés du jeu. Modifier l'équilibrage se fait ici, et seulement ici.
    /// Le robot de test (Tools/Headless, commande « balance ») vérifie la courbe de progression.
    /// </summary>
    public static class Balance
    {
        public const int Cols = 11;
        public const int SealApproachRows = 3;
        public const int SealRows = 2;

        // Résistance de base des blocs (multipliée par LayerDef.HpMul).
        public const float HpSoft = 0.25f, HpMedium = 1.6f, HpHard = 6.5f, HpSocle = 60f;
        public const float HpOre = 1.85f, HpCrystal = 0.8f, HpTnt = 0.3f, HpBoulder = 2.2f, HpBasalt = 1.6f, HpChest = 3f;

        // Engin.
        public const float PodRadius = 0.46f;
        public const float BiteRadius = 0.62f;
        public const float BiteOffsetY = 0.10f;
        public const int MaxPods = 7;
        public const float FreeFallBoost = 1.5f;
        public const float LateralFactor = 1.6f;

        // Dangers et bonus.
        public const float LavaDrainPerSec = 3.5f;
        public const float BoulderHitCost = 3f;
        public const int LavaLifeTicks = 24;
        public const float CaTickRate = 12f;
        public const float ExplosionRadius = 2.4f;
        public const float ExplosionDamage = 40f;
        public const float OrbEnergy = 0.6f;
        public const float SealEnergyBonus = 3f;
        public const float SealHpBase = 24f;
        public const float SealRewardBase = 25f;
        public const float ChestRewardBase = 20f;
        public const float OreCellCoins = 1.5f;
        /// <summary>Le minerai vaut 2 fois plus à chaque couche.</summary>
        public const double OreGrowth = 1.7;
        /// <summary>Les blocs résistent 1,7 fois plus à chaque couche.</summary>
        public const double HpGrowth = 1.7;
        public const float LowEnergy = 5f;

        // Portails.
        public const float GatePowerMul = 1.5f, GateSpeedMul = 1.25f, GateEnergy = 5f, GateMagnetMul = 2f, GateLootAdd = 1f;
        public const float GatePowerDownMul = 0.7f, GateEnergyDown = 5f;
        public const int GateSpacingMin = 26, GateSpacingMax = 32;

        public static readonly LayerDef[] Layers =
        {
            new LayerDef { Key = "layer.1", OreKey = "ore.1", Length = 55, Bg0 = 0x1A1430, Bg1 = 0x120E24, Soft = 0xC98B5A, Med = 0xA06A45, Hard = 0x7A5A4A, Ore = 0xFFB627,
                MedPct = 0.18f, HardPct = 0.02f, OrePct = 0.075f, Socle = 0f, Crystal = 40f },
            new LayerDef { Key = "layer.2", OreKey = "ore.2", Length = 65, Bg0 = 0x1C1330, Bg1 = 0x130D22, Soft = 0xD9785C, Med = 0xB5573F, Hard = 0x8A4A45, Ore = 0xFF7F50,
                MedPct = 0.28f, HardPct = 0.05f, OrePct = 0.075f, Socle = 0.3f, Crystal = 26f },
            new LayerDef { Key = "layer.3", OreKey = "ore.3", Length = 75, Bg0 = 0x141A2E, Bg1 = 0x0E1322, Soft = 0x8FA3B8, Med = 0x6E8199, Hard = 0x4F5F78, Ore = 0x3DDC97,
                MedPct = 0.34f, HardPct = 0.12f, OrePct = 0.08f, Socle = 0.5f, Crystal = 28f, Tnt = 14f },
            new LayerDef { Key = "layer.4", OreKey = "ore.4", Length = 85, Bg0 = 0x101C2A, Bg1 = 0x0A131E, Soft = 0x6FB3A8, Med = 0x4E8F87, Hard = 0x38696A, Ore = 0xCFE3FF,
                MedPct = 0.30f, HardPct = 0.10f, OrePct = 0.08f, Socle = 0.4f, Crystal = 28f, Tnt = 22f, Boulder = 9f, Cave = 0.16f },
            new LayerDef { Key = "layer.5", OreKey = "ore.5", Length = 95, Bg0 = 0x1E1A12, Bg1 = 0x14110B, Soft = 0xC4A66A, Med = 0x9C8050, Hard = 0x6E5A3C, Ore = 0xFFD700,
                MedPct = 0.34f, HardPct = 0.20f, OrePct = 0.11f, Socle = 0.6f, Crystal = 28f, Tnt = 11f, Boulder = 20f },
            new LayerDef { Key = "layer.6", OreKey = "ore.6", Length = 100, Bg0 = 0x15103A, Bg1 = 0x0D0A28, Soft = 0x8E7CF0, Med = 0x6A59C9, Hard = 0x4A3E96, Ore = 0x2EE6A8,
                MedPct = 0.26f, HardPct = 0.10f, OrePct = 0.09f, Socle = 0.4f, Crystal = 12f, Tnt = 24f },
            new LayerDef { Key = "layer.7", OreKey = "ore.7", Length = 110, Bg0 = 0x2A0F12, Bg1 = 0x1A090B, Soft = 0xB5654F, Med = 0x8A4238, Hard = 0x5E2B2B, Ore = 0xFF3B6B,
                MedPct = 0.34f, HardPct = 0.12f, OrePct = 0.09f, Socle = 0.5f, Crystal = 26f, Tnt = 18f, Boulder = 22f, Lava = 18f },
            new LayerDef { Key = "layer.8", OreKey = "ore.8", Length = 115, Bg0 = 0x0F0F1A, Bg1 = 0x08080F, Soft = 0x6C6C8C, Med = 0x4A4A66, Hard = 0x2E2E45, Ore = 0x4D9DFF,
                MedPct = 0.30f, HardPct = 0.34f, OrePct = 0.10f, Socle = 0.8f, Crystal = 26f, Tnt = 8f, Boulder = 20f, Lava = 20f },
            new LayerDef { Key = "layer.9", OreKey = "ore.9", Length = 120, Bg0 = 0x1A1A14, Bg1 = 0x10100B, Soft = 0xC9BE8F, Med = 0xA0966B, Hard = 0x716A4C, Ore = 0xB56BFF,
                MedPct = 0.36f, HardPct = 0.18f, OrePct = 0.10f, Socle = 0.7f, Crystal = 26f, Tnt = 16f, Boulder = 16f, Lava = 26f, Chests = 3 },
            new LayerDef { Key = "layer.10", OreKey = "ore.10", Length = 125, Bg0 = 0x081622, Bg1 = 0x040C14, Soft = 0x4F8FB8, Med = 0x396E94, Hard = 0x274E6E, Ore = 0x9FF4FF,
                MedPct = 0.30f, HardPct = 0.16f, OrePct = 0.10f, Socle = 0.5f, Crystal = 24f, Tnt = 16f, Boulder = 8f, Lava = 10f, Cave = 0.20f },
            new LayerDef { Key = "layer.11", OreKey = "ore.11", Length = 130, Bg0 = 0x24100A, Bg1 = 0x160905, Soft = 0xD08A3E, Med = 0xA6652B, Hard = 0x74441F, Ore = 0xFF6BD6,
                MedPct = 0.34f, HardPct = 0.26f, OrePct = 0.11f, Socle = 1.0f, Crystal = 24f, Tnt = 9f, Boulder = 12f, Lava = 11f },
            new LayerDef { Key = "layer.12", OreKey = "ore.12", Length = 140, Bg0 = 0x2A0A1E, Bg1 = 0x180511, Soft = 0xE0709A, Med = 0xB54C78, Hard = 0x7F3055, Ore = 0xFFF27A,
                MedPct = 0.36f, HardPct = 0.30f, OrePct = 0.12f, Socle = 1.0f, Crystal = 22f, Tnt = 9f, Boulder = 12f, Lava = 10f, Cave = 0.08f },
        };

        static Balance()
        {
            // Valeur du minerai et résistance des blocs : progressions géométriques, arrondies à deux chiffres significatifs.
            for (int i = 0; i < Layers.Length; i++)
            {
                double v = Math.Pow(OreGrowth, i);
                double mag = Math.Pow(10, Math.Floor(Math.Log10(v)) - 1);
                Layers[i].OreValue = (float)(Math.Round(v / mag) * mag);
                double h = Math.Pow(HpGrowth, i);
                double hm = Math.Pow(10, Math.Floor(Math.Log10(h)) - 1);
                Layers[i].HpMul = (float)(Math.Round(h / hm) * hm);
            }
        }

        public static int LayerCount { get { return Layers.Length; } }

        /// <summary>Profondeur (en mètres, 1 bloc = 1 m) du haut d'une couche depuis la surface.</summary>
        public static int LayerStartDepth(int layer)
        {
            int d = 0;
            for (int i = 0; i < layer && i < Layers.Length; i++) d += LayerRows(i);
            return d;
        }

        public static int LayerRows(int layer) { return Layers[layer].Length + SealApproachRows + SealRows; }

        public static float SealHp(int layer) { return SealHpBase * Layers[layer].HpMul * (1f + 0.10f * layer); }
        public static float SealReward(int layer) { return SealRewardBase * Layers[layer].OreValue; }

        public static float BaseHp(CellType t)
        {
            switch (t)
            {
                case CellType.Soft: return HpSoft;
                case CellType.Medium: return HpMedium;
                case CellType.Hard: return HpHard;
                case CellType.Socle: return HpSocle;
                case CellType.Ore: return HpOre;
                case CellType.Crystal: return HpCrystal;
                case CellType.Tnt: return HpTnt;
                case CellType.Boulder: return HpBoulder;
                case CellType.Basalt: return HpBasalt;
                case CellType.Chest: return HpChest;
                default: return 0f;
            }
        }

        // ------------------------------------------------------------ améliorations du garage

        private static readonly int[] MaxLevels = { 30, 12, 20, 3, 8, 15 };
        private static readonly float[] CostBase = { 30f, 40f, 35f, 400f, 50f, 60f };
        private static readonly float[] CostGrowth = { 1.42f, 1.5f, 1.45f, 6f, 1.6f, 1.5f };

        public static int MaxLevel(Upgrade u) { return MaxLevels[(int)u]; }

        public static long Cost(Upgrade u, int level)
        {
            double c = CostBase[(int)u] * Math.Pow(CostGrowth[(int)u], level);
            // Arrondi « propre » : deux chiffres significatifs.
            double mag = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(1.0, c))) - 1);
            return (long)(Math.Round(c / mag) * mag);
        }

        public static float Power(int level) { return 6f * MathF.Pow(1.25f, level); }
        public static float Speed(int level) { return 4.2f + 0.25f * level; }
        public static float BatterySeconds(int level) { return 26f + 2.2f * level; }
        public static int StartPods(int level) { return 1 + level; }
        public static float LavaResist(int level) { return 0.2f * level; }
        public static float MagnetRadius(int level) { return 1.2f + 0.3f * level; }
        public static float RefineryMul(int level) { return 1f + 0.15f * level; }

        public static bool IsBonus(GateFx fx) { return fx <= GateFx.Loot; }
    }
}
