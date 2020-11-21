namespace XRL.World.Parts {
    using System;
    using XRL.World.Effects;
    using XRL.World.Limber;
    using XRL.UI;

    [Serializable]
    public class LimberSporeSac : IPart {
        public override bool WantEvent(int ID, int cascade) => base.WantEvent(ID, cascade) ||
                                                               ID == ObjectCreatedEvent.ID ||
                                                               ID == InventoryActionEvent.ID;

        public override bool HandleEvent(ObjectCreatedEvent e) {
            var Color = ParentObject.Property["Color"];
            var Gas = Utility.GetFungalGasFromColor(Color);
            ParentObject.GetPart<GasGrenade>().GasObject = Gas;

            // a bit heavy-handed, but this way we don't have to hook anything in GasGrenade
            var GasBlueprint = GameObjectFactory.Factory.Blueprints[Gas];
            GasBlueprint.GetPart("Render").Parameters["ColorString"] = ParentObject.GetPart<Render>().ColorString;
            GasBlueprint.Tags["GasGenerationName"] = Color + " Spore Puffing";
            return true;
        }

        public override bool HandleEvent(InventoryActionEvent e) {
            if (e.Command == "Apply" && e.Actor.CheckFrozen()) {
                GameObject target = e.Actor;
                if (e.Actor.IsPlayer()) {
                    Cell cell = PickDirection(POV: e.Actor);
                    target = cell.GetCombatTarget(e.Actor);
                    if (target == null) {
                        // cancelled out
                        return true;
                    }
                }

                if (!target.IsPlayerControlled()) {
                    if (e.Actor.IsPlayer()) {
                        Popup.Show(target.The + target.ShortDisplayName + " refuses your ministrations!");
                    }
                    return true;
                }

                BodyPart part = Utility.ChooseBodyPart(target, "Apply " + ParentObject.DisplayNameOnly + " to",
                    FungalSporeInfection.BodyPartSuitableForFungalInfection);
                if (part == null) {
                    // cancelled out
                    return true;
                }

                var Infection = Utility.GetFungalInfectionFromColor(ParentObject.Property["Color"]);
                _ = FungalSporeInfection.ApplyFungalInfection(target, Infection, part);

                if (e.Actor.IsPlayer() && !target.IsPlayer()) {
                    // ApplyFungalInfection didn't popup, so we need to
                    var blueprint = GameObjectFactory.Factory.Blueprints[Infection];
                    target.pPhysics.PlayWorldSound("FungalInfectionAcquired");
                    Popup.Show(target.The + target.ShortDisplayName + " has contracted " + blueprint.DisplayName() + "&y on " + target.its + " " + part.GetOrdinalName() + ".");
                }

                _ = ParentObject.Destroy();
            }
            return true;
        }
    }
}
