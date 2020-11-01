using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace XRL.World.Limber
{
    public static class Utility {
        public static bool debug = false;

        public static void MaybeLog(string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0) {
            if (debug) {
                MetricsManager.LogInfo(filePath + ":" + lineNumber + ": " + message);
            }
        }

        private static Dictionary<string, Random> RandomDict = new Dictionary<string, Random>();
        public static Random Random(IPart part, GameObject target = null) {
            string key = "Kizby_Limber_" + part.GetType().Name;
            if (null != target || null != part.ParentObject) {
                key += "_" + (target?.id ?? part.ParentObject.id);
            }
            if (!RandomDict.ContainsKey(key)) {
                MaybeLog("Creating Random " + key);
                RandomDict[key] = XRL.Rules.Stat.GetSeededRandomGenerator(key);
            }
            return RandomDict[key];
        }
    }
}