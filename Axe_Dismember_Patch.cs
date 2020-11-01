using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace XRL.World.Parts.Skill.Limber {
    using Language;
    using UI;

    [HarmonyPatch(typeof(Axe_Dismember), "Dismember")]
    public class Axe_Dismember_Patch {
        static bool Prefix(GameObject Attacker,
                           GameObject Defender,
                           Cell Where,
                           ref BodyPart LostPart,
                           GameObject Weapon,
                           GameObject Projectile,
                           bool assumeDecapitate) {
            if (LostPart != null || !Attacker.IsPlayer() || !Defender.IsPlayerControlled()) {
                // no choice of part
                return true;
            }
            var parts = Defender.Body.GetParts()
                                     .Where(p => Axe_Dismember.BodyPartIsDismemberable(p, Attacker, assumeDecapitate))
                                     .ToList();
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
            var possessive = Defender.IsPlayer() ? "your"
                                                 : Grammar.MakePossessive(Defender.the + Defender.ShortDisplayName);
            var index = Popup.ShowOptionList(Options: strings.ToArray(),
                                             Hotkeys: keys.ToArray(),
                                             Intro: ("Dismember which of " + possessive + " parts?"),
                                             AllowEscape: true);
            if (-1 == index) {
                // will still have hit them with an axe, but not dismembered
                return false;
            }

            // let Axe_Dismember.Dismember take it from here
            LostPart = parts[index];
            return true;
        }
    }
}