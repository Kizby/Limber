using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace XRL.World.Limber
{
    using Language;
    using UI;

    public static class Utility {
        public static bool debug = false;

        public static void MaybeLog(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
            if (debug) {
                MetricsManager.LogInfo(filePath + ":" + lineNumber + ": " + message);
            }
        }

        private static Dictionary<string, Random> RandomDict = new Dictionary<string, Random>();
        public static Random Random(Object part, GameObject target = null) {
            string key = "Kizby_Limber_" + part.GetType().Name;
            if (null != target) {
                key += "_" + target.id;
            }
            if (!RandomDict.ContainsKey(key)) {
                MaybeLog("Creating Random " + key);
                RandomDict[key] = XRL.Rules.Stat.GetSeededRandomGenerator(key);
            }
            return RandomDict[key];
        }

        public static BodyPart ChooseBodyPart(GameObject who, string verb, Predicate<BodyPart> which) {
            var parts = who.Body.GetParts().Where(which.Invoke).ToList();
            var strings = new List<string>(parts.Count());
            var keys = new List<char>(parts.Count());
            foreach (var part in parts) {
                var equipped = "";
                if (null != part.Equipped) {
                    equipped = " (" + part.Equipped.DisplayNameOnly + ")";
                }
                strings.Add(part.Name + equipped);
                keys.Add((char)('a' + keys.Count()));
            }
            var possessive = who.IsPlayer() ? "your"
                                            : Grammar.MakePossessive(who.the + who.ShortDisplayName);
            var index = Popup.ShowOptionList(Options: strings.ToArray(),
                                             Hotkeys: keys.ToArray(),
                                             Intro: (verb + " which of " + possessive + " parts?"),
                                             AllowEscape: true);
            if (-1 == index) {
                return null;
            }

            return parts[index];
        }
    }
}