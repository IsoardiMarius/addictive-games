using System.Collections.Generic;

namespace Game.Core
{
    /// <summary>Textes du jeu, en français et en anglais. Ajouter une langue = ajouter une colonne.</summary>
    public static class Loc
    {
        public static bool French = true;

        private static readonly Dictionary<string, string[]> T = new Dictionary<string, string[]>
        {
            // clé, français, anglais
            { "title.1", new[] { "DIG FOR", "DIG FOR" } },
            { "title.2", new[] { "REAL", "REAL" } },
            { "tagline", new[] { "Tu conduis. Tu creuses. Pour de vrai.", "You drive. You dig. For real." } },
            { "play", new[] { "FORER", "DIG" } },
            { "garage", new[] { "GARAGE", "GARAGE" } },
            { "settings", new[] { "RÉGLAGES", "SETTINGS" } },
            { "layer", new[] { "COUCHE", "LAYER" } },
            { "layer.1", new[] { "Terre", "Topsoil" } }, { "layer.2", new[] { "Argile", "Clay" } },
            { "layer.3", new[] { "Roche", "Rock" } }, { "layer.4", new[] { "Cavernes", "Caverns" } },
            { "layer.5", new[] { "Filons", "Lodes" } }, { "layer.6", new[] { "Cristaux", "Crystals" } },
            { "layer.7", new[] { "Magma", "Magma" } }, { "layer.8", new[] { "Obsidienne", "Obsidian" } },
            { "layer.9", new[] { "Ruines", "Ruins" } }, { "layer.10", new[] { "Gouffre", "Chasm" } },
            { "layer.11", new[] { "Noyau", "Outer Core" } }, { "layer.12", new[] { "Le Cœur", "The Heart" } },
            { "ore.1", new[] { "Ambre", "Amber" } }, { "ore.2", new[] { "Cuivre", "Copper" } },
            { "ore.3", new[] { "Jade", "Jade" } }, { "ore.4", new[] { "Argent", "Silver" } },
            { "ore.5", new[] { "Or", "Gold" } }, { "ore.6", new[] { "Émeraude", "Emerald" } },
            { "ore.7", new[] { "Rubis", "Ruby" } }, { "ore.8", new[] { "Saphir", "Sapphire" } },
            { "ore.9", new[] { "Améthyste", "Amethyst" } }, { "ore.10", new[] { "Diamant", "Diamond" } },
            { "ore.11", new[] { "Iridium", "Iridium" } }, { "ore.12", new[] { "Étoile", "Starstone" } },
            { "home.goal", new[] { "Perce le sceau", "Break the seal" } },
            { "home.cleared", new[] { "Franchie", "Cleared" } },
            { "home.depth", new[] { "de {0} à {1} m", "{0} to {1} m" } },
            { "home.finished", new[] { "Tu as atteint le Cœur de la Terre !", "You reached the Heart of the Earth!" } },
            { "hint.drag", new[] { "GLISSE POUR DIRIGER", "DRAG TO STEER" } },
            { "hud.low", new[] { "BATTERIE FAIBLE", "LOW BATTERY" } },
            { "hud.cleared", new[] { "COUCHE {0} FRANCHIE !", "LAYER {0} CLEARED!" } },
            { "hud.final", new[] { "LE CŒUR EST À TOI !", "THE HEART IS YOURS!" } },
            { "hud.seal", new[] { "SCEAU", "SEAL" } },
            { "gate.PlusPod", new[] { "+1 FORET", "+1 DRILL" } },
            { "gate.Power", new[] { "PUISSANCE +50%", "POWER +50%" } },
            { "gate.Speed", new[] { "VITESSE +25%", "SPEED +25%" } },
            { "gate.Energy", new[] { "ÉNERGIE +5 s", "ENERGY +5 s" } },
            { "gate.Magnet", new[] { "AIMANT ×2", "MAGNET ×2" } },
            { "gate.Loot", new[] { "BUTIN ×2", "LOOT ×2" } },
            { "gate.MinusPod", new[] { "-1 FORET", "-1 DRILL" } },
            { "gate.PowerDown", new[] { "PUISSANCE -30%", "POWER -30%" } },
            { "gate.EnergyDown", new[] { "ÉNERGIE -5 s", "ENERGY -5 s" } },
            { "res.win", new[] { "COUCHE {0} FRANCHIE !", "LAYER {0} CLEARED!" } },
            { "res.final", new[] { "TU AS ATTEINT LE CŒUR !", "YOU REACHED THE HEART!" } },
            { "res.fail", new[] { "PANNE SÈCHE", "OUT OF POWER" } },
            { "res.quit", new[] { "DESCENTE INTERROMPUE", "RUN ABANDONED" } },
            { "res.sealLeft", new[] { "Il restait {0} % du sceau", "{0}% of the seal was left" } },
            { "res.metersLeft", new[] { "Encore {0} m jusqu'au sceau", "{0} m short of the seal" } },
            { "res.depth", new[] { "Profondeur", "Depth" } },
            { "res.blocks", new[] { "Blocs détruits", "Blocks destroyed" } },
            { "res.ore", new[] { "Filons récoltés", "Lodes mined" } },
            { "res.gates", new[] { "Portails bonus", "Bonus gates" } },
            { "res.sale", new[] { "VENTE DU MINERAI", "ORE SALE" } },
            { "res.refinery", new[] { "Raffinerie ×{0}", "Refinery ×{0}" } },
            { "res.double", new[] { "VENTE ×2", "SALE ×2" } },
            { "res.doubleNote", new[] { "Pub simulée - version de test", "Simulated ad - test build" } },
            { "res.doubled", new[] { "VENTE DOUBLÉE", "SALE DOUBLED" } },
            { "res.again", new[] { "REJOUER", "RETRY" } },
            { "res.next", new[] { "CONTINUER", "CONTINUE" } },
            { "res.record", new[] { "NOUVEAU RECORD", "NEW RECORD" } },
            { "revive.title", new[] { "BATTERIE DE SECOURS ?", "BACKUP BATTERY?" } },
            { "revive.yes", new[] { "RELANCER", "REVIVE" } },
            { "revive.no", new[] { "Non merci", "No thanks" } },
            { "ad.simulated", new[] { "PUB SIMULÉE", "SIMULATED AD" } },
            { "ad.note", new[] { "Aucune vraie pub dans cette version de test", "No real ad in this test build" } },
            { "up.Drill", new[] { "Foret", "Drill" } }, { "up.Engine", new[] { "Moteur", "Engine" } },
            { "up.Battery", new[] { "Batterie", "Battery" } }, { "up.Chassis", new[] { "Châssis", "Chassis" } },
            { "up.Magnet", new[] { "Aimant", "Magnet" } }, { "up.Refinery", new[] { "Raffinerie", "Refinery" } },
            { "up.Drill.d", new[] { "Puissance", "Power" } }, { "up.Engine.d", new[] { "Vitesse", "Speed" } },
            { "up.Battery.d", new[] { "Autonomie", "Run time" } }, { "up.Chassis.d", new[] { "Forets", "Drills" } },
            { "up.Magnet.d", new[] { "Portée", "Range" } }, { "up.Refinery.d", new[] { "Prix de vente", "Sale price" } },
            { "up.level", new[] { "Niv. {0}", "Lv. {0}" } },
            { "up.max", new[] { "MAX", "MAX" } },
            { "set.sound", new[] { "Sons", "Sounds" } }, { "set.music", new[] { "Musique", "Music" } },
            { "set.haptics", new[] { "Vibrations", "Vibration" } }, { "set.shake", new[] { "Secousses d'écran", "Screen shake" } },
            { "set.lang", new[] { "Langue : Français", "Language: English" } },
            { "set.journal", new[] { "Copier mon journal de test", "Copy my test log" } },
            { "set.journalNote", new[] { "Compteurs anonymes. Rien n'est envoyé.", "Anonymous counters. Nothing is sent." } },
            { "set.copied", new[] { "Journal copié !", "Log copied!" } },
            { "set.reset", new[] { "Réinitialiser la progression", "Reset progress" } },
            { "set.resetConfirm", new[] { "Toucher encore pour confirmer", "Tap again to confirm" } },
            { "set.resetDone", new[] { "Progression effacée", "Progress erased" } },
            { "set.credits", new[] { "Police Rubik (licence OFL). Images et sons générés par le code.", "Rubik font (OFL licence). Art and sound generated by code." } },
            { "set.privacy", new[] { "Aucune donnée ne quitte cet appareil.", "No data leaves this device." } },
            { "pause", new[] { "PAUSE", "PAUSED" } },
            { "pause.resume", new[] { "REPRENDRE", "RESUME" } },
            { "pause.quit", new[] { "REMONTER", "SURFACE" } },
            { "on", new[] { "OUI", "ON" } }, { "off", new[] { "NON", "OFF" } },
            { "unit.m", new[] { "m", "m" } }, { "unit.s", new[] { "s", "s" } },
        };

        public static string Get(string key)
        {
            return T.TryGetValue(key, out string[] v) ? v[French ? 0 : 1] : key;
        }

        public static string Get(string key, object a) { return Get(key).Replace("{0}", a.ToString()); }
        public static string Get(string key, object a, object b) { return Get(key).Replace("{0}", a.ToString()).Replace("{1}", b.ToString()); }

        /// <summary>Tous les caractères utilisés par les textes (sert à vérifier que la police les contient).</summary>
        public static string AllText()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var kv in T) { sb.Append(kv.Value[0]); sb.Append(kv.Value[1]); }
            return sb.ToString();
        }
    }
}
