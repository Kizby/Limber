namespace XRL.World.Parts {
    using System;
    using XRL.Language;

    [Serializable]
    public class LimberFungusCorpse : Corpse {
        public override bool SameAs(IPart p) => false;

        public override bool WantEvent(int ID, int cascade) => base.WantEvent(ID, cascade) || ID == BeforeDeathRemovalEvent.ID;

        public override bool HandleEvent(BeforeDeathRemovalEvent E) {
            // change what's butchered from this to match the original species
            var species = ParentObject.GetSpecies();
            if (species != null) {
                CorpseObject = GameObject.create(CorpseBlueprint);
                var butcherable = CorpseObject.GetPart<Butcherable>();
                var toButcher = butcherable.OnSuccess + Grammar.InitialCap(species);
                if (GameObjectFactory.Factory.Blueprints.ContainsKey(toButcher)) {
                    CorpseObject.GetPart<Butcherable>().OnSuccess = toButcher;
                }
            }
            return base.HandleEvent(E);
        }
    }
}
