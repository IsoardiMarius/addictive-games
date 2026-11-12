using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Game.Core
{
    /// <summary>
    /// Journal de test : compteurs anonymes, conservés sur l'appareil. Rien n'est envoyé :
    /// le testeur copie lui-même ce texte depuis les réglages s'il veut le transmettre.
    /// Aucun identifiant, aucune donnée personnelle.
    /// </summary>
    public sealed class Journal
    {
        public string FirstLaunch = "";
        public List<string> Days = new List<string>();
        public int Sessions;
        public float TotalSeconds;
        public List<int> SessionSeconds = new List<int>();
        public int RunsStarted, RunsEnded, Wins, Fails, Quits;
        public int BestLayer;
        public int[] Attempts = new int[Balance.LayerCount];
        public float FirstTouchDelay = -1f, FirstUpgradeDelay = -1f;
        public int Upgrades, GatesGood, GatesBad, AdsOffered, AdsAccepted;
        public float FpsSum; public int FpsCount;

        public void Touch(string today)
        {
            if (string.IsNullOrEmpty(FirstLaunch)) FirstLaunch = today;
            if (!Days.Contains(today)) Days.Add(today);
        }

        /// <summary>Texte que le testeur copie (format décrit dans testing/).</summary>
        public string Export(string version)
        {
            var c = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.Append("DFR-JOURNAL 1\n");
            sb.Append("version_jeu=").Append(version).Append('\n');
            sb.Append("premier_lancement=").Append(FirstLaunch).Append('\n');
            sb.Append("jours_joues=").Append(string.Join(";", Days)).Append('\n');
            sb.Append("sessions=").Append(Sessions).Append('\n');
            sb.Append("duree_totale_s=").Append((int)TotalSeconds).Append('\n');
            sb.Append("durees_sessions_s=").Append(string.Join(";", SessionSeconds)).Append('\n');
            sb.Append("descentes_commencees=").Append(RunsStarted).Append('\n');
            sb.Append("descentes_terminees=").Append(RunsEnded).Append('\n');
            sb.Append("victoires=").Append(Wins).Append('\n');
            sb.Append("pannes=").Append(Fails).Append('\n');
            sb.Append("abandons=").Append(Quits).Append('\n');
            sb.Append("meilleure_couche=").Append(BestLayer).Append('\n');
            sb.Append("tentatives_par_couche=").Append(string.Join(";", Attempts)).Append('\n');
            sb.Append("delai_premier_contact_s=").Append(FirstTouchDelay.ToString("0.0", c)).Append('\n');
            sb.Append("delai_premiere_amelioration_s=").Append(FirstUpgradeDelay.ToString("0", c)).Append('\n');
            sb.Append("ameliorations_achetees=").Append(Upgrades).Append('\n');
            sb.Append("portails_bonus=").Append(GatesGood).Append('\n');
            sb.Append("portails_malus=").Append(GatesBad).Append('\n');
            sb.Append("pubs_simulees_proposees=").Append(AdsOffered).Append('\n');
            sb.Append("pubs_simulees_acceptees=").Append(AdsAccepted).Append('\n');
            sb.Append("ips_moyennes=").Append((FpsCount > 0 ? FpsSum / FpsCount : 0f).ToString("0.0", c)).Append('\n');
            return sb.ToString();
        }
    }

    /// <summary>Progression du joueur et réglages. Sauvegardés localement sous forme de texte « clé=valeur ».</summary>
    public sealed class Progress
    {
        public const int SaveVersion = 1;

        public long Coins;
        public int[] Up = new int[(int)Upgrade.Count];
        /// <summary>Nombre de couches franchies (0 à 12) : la couche d'indice MaxLayer est la prochaine à percer.</summary>
        public int MaxLayer;
        public int SelectedLayer;
        public int BestDepth;
        public int RunCounter;
        public bool TutorialDone, Finished;
        public bool Sound = true, Music = true, Haptics = true, Shake = true;
        public string Lang = "";
        public Journal J = new Journal();

        public int PlayableLayer { get { return Math.Min(MaxLayer, Balance.LayerCount - 1); } }

        public bool CanAffordAny()
        {
            for (int u = 0; u < Up.Length; u++)
                if (Up[u] < Balance.MaxLevel((Upgrade)u) && Balance.Cost((Upgrade)u, Up[u]) <= Coins) return true;
            return false;
        }

        // ------------------------------------------------------------------ sérialisation

        public string Serialize()
        {
            var c = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            void Kv(string k, string v) { sb.Append(k).Append('=').Append(v).Append('\n'); }
            Kv("v", SaveVersion.ToString(c));
            Kv("coins", Coins.ToString(c));
            Kv("up", string.Join(";", Up));
            Kv("maxLayer", MaxLayer.ToString(c));
            Kv("selLayer", SelectedLayer.ToString(c));
            Kv("bestDepth", BestDepth.ToString(c));
            Kv("runs", RunCounter.ToString(c));
            Kv("tuto", TutorialDone ? "1" : "0");
            Kv("finished", Finished ? "1" : "0");
            Kv("sound", Sound ? "1" : "0");
            Kv("music", Music ? "1" : "0");
            Kv("haptics", Haptics ? "1" : "0");
            Kv("shake", Shake ? "1" : "0");
            Kv("lang", Lang);
            Kv("j.first", J.FirstLaunch);
            Kv("j.days", string.Join(";", J.Days));
            Kv("j.sessions", J.Sessions.ToString(c));
            Kv("j.total", J.TotalSeconds.ToString("0.0", c));
            Kv("j.sess", string.Join(";", J.SessionSeconds));
            Kv("j.started", J.RunsStarted.ToString(c));
            Kv("j.ended", J.RunsEnded.ToString(c));
            Kv("j.wins", J.Wins.ToString(c));
            Kv("j.fails", J.Fails.ToString(c));
            Kv("j.quits", J.Quits.ToString(c));
            Kv("j.best", J.BestLayer.ToString(c));
            Kv("j.attempts", string.Join(";", J.Attempts));
            Kv("j.touch", J.FirstTouchDelay.ToString("0.00", c));
            Kv("j.firstUp", J.FirstUpgradeDelay.ToString("0.0", c));
            Kv("j.ups", J.Upgrades.ToString(c));
            Kv("j.gGood", J.GatesGood.ToString(c));
            Kv("j.gBad", J.GatesBad.ToString(c));
            Kv("j.adsOff", J.AdsOffered.ToString(c));
            Kv("j.adsAcc", J.AdsAccepted.ToString(c));
            Kv("j.fpsSum", J.FpsSum.ToString("0.0", c));
            Kv("j.fpsN", J.FpsCount.ToString(c));
            return sb.ToString();
        }

        /// <summary>Relit une sauvegarde. Toute valeur absente ou illisible garde sa valeur par défaut.</summary>
        public static Progress Parse(string text)
        {
            var p = new Progress();
            if (string.IsNullOrEmpty(text)) return p;
            var c = CultureInfo.InvariantCulture;
            var map = new Dictionary<string, string>();
            foreach (string line in text.Split('\n'))
            {
                int i = line.IndexOf('=');
                if (i > 0) map[line.Substring(0, i)] = line.Substring(i + 1).TrimEnd('\r');
            }
            long L(string k, long def) { return map.TryGetValue(k, out string s) && long.TryParse(s, NumberStyles.Integer, c, out long v) ? v : def; }
            float F(string k, float def) { return map.TryGetValue(k, out string s) && float.TryParse(s, NumberStyles.Float, c, out float v) ? v : def; }
            bool B(string k, bool def) { return map.TryGetValue(k, out string s) ? s == "1" : def; }
            string S(string k) { return map.TryGetValue(k, out string s) ? s : ""; }
            void Ints(string k, int[] dst)
            {
                string[] parts = S(k).Split(';');
                for (int i = 0; i < dst.Length && i < parts.Length; i++)
                    if (int.TryParse(parts[i], NumberStyles.Integer, c, out int v)) dst[i] = v;
            }

            p.Coins = Math.Max(0, L("coins", 0));
            Ints("up", p.Up);
            for (int u = 0; u < p.Up.Length; u++) p.Up[u] = Math.Max(0, Math.Min(p.Up[u], Balance.MaxLevel((Upgrade)u)));
            p.MaxLayer = (int)Math.Max(0, Math.Min(L("maxLayer", 0), Balance.LayerCount));
            p.SelectedLayer = (int)Math.Max(0, Math.Min(L("selLayer", 0), p.PlayableLayer));
            p.BestDepth = (int)L("bestDepth", 0);
            p.RunCounter = (int)L("runs", 0);
            p.TutorialDone = B("tuto", false);
            p.Finished = B("finished", false);
            p.Sound = B("sound", true); p.Music = B("music", true); p.Haptics = B("haptics", true); p.Shake = B("shake", true);
            p.Lang = S("lang");
            p.J.FirstLaunch = S("j.first");
            foreach (string d in S("j.days").Split(';')) if (d.Length > 0) p.J.Days.Add(d);
            p.J.Sessions = (int)L("j.sessions", 0);
            p.J.TotalSeconds = F("j.total", 0f);
            foreach (string d in S("j.sess").Split(';')) if (int.TryParse(d, NumberStyles.Integer, c, out int v)) p.J.SessionSeconds.Add(v);
            p.J.RunsStarted = (int)L("j.started", 0); p.J.RunsEnded = (int)L("j.ended", 0);
            p.J.Wins = (int)L("j.wins", 0); p.J.Fails = (int)L("j.fails", 0); p.J.Quits = (int)L("j.quits", 0);
            p.J.BestLayer = (int)L("j.best", 0);
            Ints("j.attempts", p.J.Attempts);
            p.J.FirstTouchDelay = F("j.touch", -1f); p.J.FirstUpgradeDelay = F("j.firstUp", -1f);
            p.J.Upgrades = (int)L("j.ups", 0);
            p.J.GatesGood = (int)L("j.gGood", 0); p.J.GatesBad = (int)L("j.gBad", 0);
            p.J.AdsOffered = (int)L("j.adsOff", 0); p.J.AdsAccepted = (int)L("j.adsAcc", 0);
            p.J.FpsSum = F("j.fpsSum", 0f); p.J.FpsCount = (int)L("j.fpsN", 0);
            return p;
        }
    }
}
