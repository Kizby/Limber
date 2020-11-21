namespace XRL.World.Limber {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Wintellect.PowerCollections;
    using XRL.Language;
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

        public static Dictionary<string, int> ColorIndices = new Dictionary<string, int> { { "Gold", 0 }, { "Rose", 1 }, { "Azure", 2 }, { "Jade", 3 } };
        public static string GetFungalGasFromColor(string Color) {
            Stat.ReseedFrom("PufferType");
            return Algorithms.RandomShuffle(SporePuffer.InfectionList)[ColorIndices[Color]];
        }
        public static string GetFungalInfectionFromColor(string Color) {
            Stat.ReseedFrom("PufferType");
            return Algorithms.RandomShuffle(SporePuffer.InfectionObjectList)[ColorIndices[Color]];
        }
    }
}
