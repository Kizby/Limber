namespace XRL.World.Parts {
    using Limber;

    public class LimberFungalGasGrenade : GasGrenade {
        public string Color {
            get => ParentObject.GetStringProperty("color"); set {
                ParentObject.SetStringProperty("color", value);
                GasObject = Utility.GetFungalGasFromColor(value);
                var render = ParentObject.GetPart<Render>();
                render.ColorString = Utility.ColorRenders[value];
                render.DisplayName = render.ColorString + value + "&y spore sac";
            }
        }

        protected override bool DoDetonate(Cell C, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false) {
            PlayWorldSound(GetPropertyOrTag("DetonatedSound"), 1f, combat: true);
            Utility.Puff(Color, C, Actor, ParentObject, true);
            DidX("burst", terminalPunctuation: "!");
            _ = ParentObject.Destroy(Silent: true);
            return true;
        }
    }
}
