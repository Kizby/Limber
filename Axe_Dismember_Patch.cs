namespace XRL.World.Parts.Skill.Limber {
    using HarmonyLib;
    using XRL.World.Limber;

    [HarmonyPatch(typeof(Axe_Dismember), "Dismember")]
    public static class Axe_Dismember_Patch {
        public static bool Prefix(GameObject Attacker,
                           GameObject Defender,
                           ref BodyPart LostPart,
                           bool assumeDecapitate) {
            if (LostPart != null || !Attacker.IsPlayer() || !Defender.IsPlayerControlled()) {
                // no choice of part
                return true;
            }

            LostPart = Utility.ChooseBodyPart(Defender, "Dismember",
                p => Axe_Dismember.BodyPartIsDismemberable(p, Attacker, assumeDecapitate));
            if (LostPart == null) {
                // will still have hit them with an axe, but not dismembered
                return false;
            }

            // let Axe_Dismember.Dismember take it from here
            return true;
        }
    }
}
