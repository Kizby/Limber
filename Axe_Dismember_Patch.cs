using HarmonyLib;

namespace XRL.World.Parts.Skill.Limber {
    using XRL.World.Limber;

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

            LostPart = Utility.ChooseBodyPart(Defender, "Dismember", 
                p => Axe_Dismember.BodyPartIsDismemberable(p, Attacker, assumeDecapitate));
            if (null == LostPart) {
                // will still have hit them with an axe, but not dismembered
                return false;
            }

            // let Axe_Dismember.Dismember take it from here
            return true;
        }
    }
}