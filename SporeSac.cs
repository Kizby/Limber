namespace XRL.World.Parts {
    using System;
    using XRL.World.Effects;
    using XRL.World.Limber;
    using XRL.UI;

    [Serializable]
    public class LimberSporeSac : IPart {
        public override void Register(GameObject Object) {
            Object.RegisterPartEvent(this, "ObjectExtracted");
            base.Register(Object);
        }

        public override bool FireEvent(Event E) {
            if (E.ID == "ObjectExtracted") {
                var source = E.GetGameObjectParameter("Source");
                var color = source.Property["color"];
                ParentObject.GetPart<LimberFungalGasGrenade>().Color = color;
                var preservable = ParentObject.RequirePart<PreservableItem>();
                preservable.Result = "LimberPreserved" + char.ToUpper(color[0]) + color.Substring(1) + "puff";
                preservable.Number = 8;
                return true;
            }
            return base.FireEvent(E);
        }

        public override bool WantEvent(int ID, int cascade) => base.WantEvent(ID, cascade) ||
                                                               ID == InventoryActionEvent.ID;

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

                var Infection = Utility.GetFungalInfectionFromColor(ParentObject.GetPart<LimberFungalGasGrenade>().Color);
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
