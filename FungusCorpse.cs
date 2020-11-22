namespace XRL.World.Parts {
    using System;
    using AiUnity.Common.Extensions;

    [Serializable]
    public class LimberFungusCorpse : Corpse {
        public override bool SameAs(IPart p) => false;

        public override bool WantEvent(int ID, int cascade) => base.WantEvent(ID, cascade) || ID == BeforeDeathRemovalEvent.ID;

        public override bool HandleEvent(BeforeDeathRemovalEvent E) {
            // make a note of the original puffer color on the corpse
            var color = ParentObject.GetSpecies()?.Before("puff");
            if (color != null) {
                CorpseObject = GameObject.create(CorpseBlueprint);
                CorpseObject.SetStringProperty("color", color);
            }
            return base.HandleEvent(E);
        }
    }
}
