using System;
using Wintellect.PowerCollections;

namespace XRL.World.Parts.Limber
{
    using Effects;
    using Mutation;
    using Rules;
    using World.Limber;
    using XRL.UI;

    [Serializable]
    public class SporeSac : IPart {
        public override bool WantEvent(int ID, int cascade) => base.WantEvent(ID, cascade) || ID == InventoryActionEvent.ID;

        public override bool HandleEvent(InventoryActionEvent E) {
            if (E.Command == "Apply" && E.Actor.CheckFrozen()) {
                var Infection = GetPropertyOrTag("PuffObject");
                if (Infection.Length == 1)
                {
                    // RNG magic until/unless SporePuffer adopts a sane randomization strategy
                    Stat.ReseedFrom("PufferType");
                    Infection = Algorithms.RandomShuffle<string>(SporePuffer.InfectionObjectList)[Convert.ToInt32(Infection)];
                }

                GameObject target = E.Actor;
                if (E.Actor.IsPlayer()) {
                    Cell cell = PickDirection(POV: E.Actor);
                    target = cell.GetCombatTarget(E.Actor);
                    if (null == target) {
                        // cancelled out
                        return true;
                    }
                }

                if (!target.IsPlayerControlled()) {
                    if (E.Actor.IsPlayer()) {
                        Popup.Show(target.The + target.ShortDisplayName + " refuses your ministrations!");
                    }
                    return true;
                }

                BodyPart part = Utility.ChooseBodyPart(target, "Apply " + ParentObject.DisplayNameOnly,
                    FungalSporeInfection.BodyPartSuitableForFungalInfection);
                if (null == part) {
                    // cancelled out
                    return true;
                }
                FungalSporeInfection.ApplyFungalInfection(target, Infection, part);
                this.ParentObject.Destroy();
            }
            return true;
        }
    }
}