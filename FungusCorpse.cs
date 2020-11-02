using System;

namespace XRL.World.Parts.Limber
{
    using XRL.Language;

    [Serializable]
    public class FungusCorpse : Corpse
    {
        public override bool SameAs(IPart p) => false;

        public override bool WantEvent(int ID, int cascade) => base.WantEvent(ID, cascade) || ID == BeforeDeathRemovalEvent.ID;

        public override bool HandleEvent(BeforeDeathRemovalEvent E) {
            string species = ParentObject.GetSpecies();
            if (null != species) {
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