using System;
using Wintellect.PowerCollections;

namespace XRL.World.Parts
{
    using System.Collections.Generic;
    using Effects;
    using Mutation;
    using Rules;
    using World.Limber;
    using XRL.UI;

    [Serializable]
    public class LimberSporeSac : IPart {
        public string Infection = null;
        public static string[] Colors = {"Gold", "Azure", "Rose", "Jade"};
        public override bool WantEvent(int ID, int cascade) => base.WantEvent(ID, cascade) ||
                                                               ID == ObjectCreatedEvent.ID ||
                                                               ID == InventoryActionEvent.ID;

        public override bool HandleEvent(ObjectCreatedEvent E) {
            var PuffObject = ParentObject.GetPropertyOrTag("PuffObject");
            if (PuffObject.Length == 1)
            {
                // RNG magic until/unless SporePuffer adopts a sane randomization strategy
                int which = Convert.ToInt32(PuffObject);
                Stat.ReseedFrom("PufferType");
                int mapped = Algorithms.RandomShuffle(new List<int>{0, 1, 2, 3})[which];
                
                Infection = SporePuffer.InfectionObjectList[mapped];
                var Gas = SporePuffer.InfectionList[mapped];
                ParentObject.GetPart<GasGrenade>().GasObject = Gas;

                // a bit heavy-handed, but this way we don't have to hook anything in GasGrenade
                var GasBlueprint = GameObjectFactory.Factory.Blueprints[Gas];
                GasBlueprint.GetPart("Render").Parameters["ColorString"] = ParentObject.GetPart<Render>().ColorString;
                GasBlueprint.Props["Color"] = Colors[which];
                GasBlueprint.Tags["GasGenerationName"] = Colors[which] + " Spore Puffing";
            }
            return true;
        }

        public override bool HandleEvent(InventoryActionEvent E) {
            if (E.Command == "Apply" && E.Actor.CheckFrozen()) {
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

                BodyPart part = Utility.ChooseBodyPart(target, "Apply " + ParentObject.DisplayNameOnly + " to",
                    FungalSporeInfection.BodyPartSuitableForFungalInfection);
                if (null == part) {
                    // cancelled out
                    return true;
                }
                FungalSporeInfection.ApplyFungalInfection(target, Infection, part);

                if (E.Actor.IsPlayer() && !target.IsPlayer()) {
                    // ApplyFungalInfection didn't popup, so we need to
                    var blueprint = GameObjectFactory.Factory.Blueprints[Infection];
                    target.pPhysics.PlayWorldSound("FungalInfectionAcquired");
                    Popup.Show(target.The + target.ShortDisplayName + " has contracted " + blueprint.DisplayName() + "&y on " + target.its + " " + part.GetOrdinalName() + ".");
                }

                this.ParentObject.Destroy();
            }
            return true;
        }
    }
}