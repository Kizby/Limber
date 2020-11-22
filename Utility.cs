namespace XRL.World.Limber {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Wintellect.PowerCollections;
    using XRL.Language;
    using XRL.World.Effects;
    using XRL.World.Parts;
    using XRL.World.Parts.Mutation;
    using XRL.Rules;
    using XRL.UI;

    public static class Utility {
        public static bool debug;

        public static void MaybeLog(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
            if (debug) {
                MetricsManager.LogInfo(filePath + ":" + lineNumber + ": " + message);
            }
        }

        private static readonly Dictionary<string, Random> RandomDict = new Dictionary<string, Random>();
        public static Random Random(object part, GameObject target = null) {
            string key = "Kizby_Limber_" + part.GetType().Name;
            if (target != null) {
                key += "_" + target.id;
            }
            if (!RandomDict.ContainsKey(key)) {
                MaybeLog("Creating Random " + key);
                RandomDict[key] = Stat.GetSeededRandomGenerator(key);
            }
            return RandomDict[key];
        }

        public static BodyPart ChooseBodyPart(GameObject who, string verb, Predicate<BodyPart> which) {
            var parts = who.Body.GetParts().Where(which.Invoke).ToList();
            var strings = new List<string>(parts.Count);
            var keys = new List<char>(parts.Count);
            foreach (var part in parts) {
                var equipped = "";
                if (part.Equipped != null) {
                    equipped = " (" + part.Equipped.DisplayNameOnly + ")";
                }
                strings.Add(part.Name + equipped);
                keys.Add((char)('a' + keys.Count));
            }
            var possessive = who.IsPlayer() ? "your"
                                            : Grammar.MakePossessive(who.the + who.ShortDisplayName);
            var index = Popup.ShowOptionList(Options: strings.ToArray(),
                                             Hotkeys: keys.ToArray(),
                                             Intro: verb + " which of " + possessive + " parts?",
                                             AllowEscape: true);
            if (-1 == index) {
                return null;
            }

            return parts[index];
        }

        public static Dictionary<string, int> ColorIndices = new Dictionary<string, int> { { "gold", 0 }, { "rose", 1 }, { "azure", 2 }, { "jade", 3 } };
        public static Dictionary<string, string> ColorRenders = new Dictionary<string, string> { { "gold", "&W" }, { "rose", "&R" }, { "azure", "&B" }, { "jade", "&G" } };
        public static string GetFungalGasFromColor(string Color) {
            Stat.ReseedFrom("PufferType");
            return Algorithms.RandomShuffle(SporePuffer.InfectionList)[ColorIndices[Color.ToLower()]];
        }
        public static string GetFungalInfectionFromColor(string Color) {
            Stat.ReseedFrom("PufferType");
            return Algorithms.RandomShuffle(SporePuffer.InfectionObjectList)[ColorIndices[Color.ToLower()]];
        }

        public static void Puff(string color, Cell cell, GameObject actor, GameObject source = null, bool includeCenter = false) {
            if (source == null) {
                source = actor;
            }
            var localAdjacentCells = cell.GetLocalAdjacentCells();
            if (includeCenter) {
                localAdjacentCells.Add(cell);
            }
            var PuffObject = GetFungalGasFromColor(color);
            cell.ParticleBlip("&W*");
            for (int index = 0; index < localAdjacentCells.Count; ++index) {
                var gameObject = localAdjacentCells[index].AddObject(PuffObject);
                Gas gas = gameObject.GetPart<Gas>();
                gas.ColorString = ColorRenders[color];
                gas.Creator = actor;
                if (source.GetPart<GasGrenade>() is GasGrenade gasGrenade) {
                    gas.Density = gasGrenade.Density;
                }
                if (source.HasEffect("Phased")) {
                    _ = gameObject.ForceApplyEffect(new Phased(Stat.Random(23, 32)));
                }
                if (source.HasEffect("Omniphase")) {
                    _ = gameObject.ForceApplyEffect(new Omniphase(Stat.Random(46, 64)));
                }
            }
        }
    }
}
