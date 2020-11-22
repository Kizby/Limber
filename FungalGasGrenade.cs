namespace XRL.World.Parts {
    using Limber;

    public class LimberFungalGasGrenade : GasGrenade {
        private string color;

        public string Color {
            get => color; set {
                color = value;
                GasObject = Utility.GetFungalGasFromColor(color);
                var render = ParentObject.GetPart<Render>();
                render.ColorString = Utility.ColorRenders[color];
                render.DisplayName = render.ColorString + color + "&y spore sac";
            }
        }

        public override bool Detonate(Cell C, GameObject Actor = null, GameObject ApparentTarget = null) {
            PlayWorldSound(GetPropertyOrTag("DetonatedSound"), 1f, combat: true);
            Utility.Puff(Color, C, Actor, ParentObject, true);
            DidX("burst", terminalPunctuation: "!");
            _ = ParentObject.Destroy(Silent: true);
            return true;
        }
    }
}
