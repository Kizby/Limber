namespace XRL.World.Effects {
    using System;
    using System.Collections.Generic;
    using Qud.API;
    using XRL.Core;
    using XRL.World.Limber;
    using XRL.World.Parts;
    using XRL.Language;
    using XRL.UI;

    [Serializable]
    public class LimberCookingDomainLimbs_UnitLimbs_25Percent : ProceduralCookingEffectUnit {
        public double PartsPerPart = 0.5;
        public int PartsGained;

        public Random Random;

        public override string GetDescription() => PartsGained == 0 ? "Nothing happened." : "You gain " + Grammar.Cardinal(PartsGained) + " new body parts!";

        public override string GetTemplatedDescription() => "25% chance to gain multiple body parts.";

        public override void Init(GameObject target) {
            Random = Utility.Random(this, target);
            if (!25.in100(Random)) {
                PartsGained = 0;
            } else {
                // could be more efficient with a binomial distribution sampler, but n should be small-ish
                for (var i = 0; i < target.GetConcreteBodyPartCount(); ++i) {
                    if (Random.NextDouble() < PartsPerPart) {
                        ++PartsGained;
                    }
                }
            }
        }

        public override void Apply(GameObject go, Effect parent) {
            go.PermuteRandomMutationBuys();
            if (PartsGained == 0) {
                return;
            }
            var IsChimera = go.IsChimera();
            if (!IsChimera) {
                Popup.Show("Your meager body rebels at the ontological strain!");
            }
            var PartsRemaining = PartsGained;
            var Mutations = go.GetPart<Mutations>();
            var OriginalParts = IsChimera ? null : new HashSet<BodyPart>(go.Body.GetConcreteParts());
            var DismemberedParts = IsChimera ? null : new List<Body.DismemberedPart>(go.Body.DismemberedParts ?? new List<Body.DismemberedPart>());
            var Accomplishments = IsChimera ? null : new List<JournalAccomplishment>(JournalAPI.Accomplishments);
            while (0 < PartsRemaining) {
                _ = Mutations.AddChimericBodyPart();
                --PartsRemaining;
                if (!IsChimera) {
                    foreach (var part in go.Body.GetConcreteParts()) {
                        if (!OriginalParts.Contains(part)) {
                            // found the new one; drop it on the ground
                            _ = part.Dismember();
                            break;
                        }
                    }
                }
            }

            if (!IsChimera) {
                // reset the dismembered parts so anything gained here can't be regenerated
                go.Body.DismemberedParts = DismemberedParts;
                // reset the accomplishments so we don't get a bunch of dismemberments
                XRLCore.Core.Game.SetObjectGameState("Journal.Accomplishments", Accomplishments);
            }
        }

        public override void Remove(GameObject go, Effect parent) {
        }
    }
}
