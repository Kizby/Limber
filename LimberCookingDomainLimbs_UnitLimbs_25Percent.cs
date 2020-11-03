using System;
using System.Collections.Generic;
using Qud.API;

namespace XRL.World.Effects
{
    using Core;
    using World.Limber;
    using Parts;
    using XRL.Language;
    using XRL.UI;

    [Serializable]
    public class LimberCookingDomainLimbs_UnitLimbs_25Percent : ProceduralCookingEffectUnit
    {
        public double PartsPerPart = 0.5;
        public int PartsGained;

        public Random Random;

        public override string GetDescription() => 0 == PartsGained ? "Nothing happened." : "You gain " + Grammar.Cardinal(PartsGained) + " new body parts!";

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

        public override void Apply(GameObject target, Effect parent)
        {
            target.PermuteRandomMutationBuys();
            if (0 == PartsGained) {
                return;
            }
            var IsChimera = target.IsChimera();
            if (!IsChimera) {
                Popup.Show("Your meager body rebels at the ontological strain!");
            }
            var PartsRemaining = PartsGained;
            var Mutations = target.GetPart<Mutations>();
            var OriginalParts = IsChimera ? null : new HashSet<BodyPart>(target.Body.GetConcreteParts());
            var DismemberedParts = IsChimera ? null : new List<Body.DismemberedPart>(target.Body.DismemberedParts ?? new List<Body.DismemberedPart>());
            var Accomplishments = IsChimera ? null : new List<JournalAccomplishment>(JournalAPI.Accomplishments);
            while (0 < PartsRemaining) {
                Mutations.AddChimericBodyPart();
                --PartsRemaining;
                if (!IsChimera) {
                    foreach (var part in target.Body.GetConcreteParts()) {
                        if (!OriginalParts.Contains(part)) {
                            // found the new one; drop it on the ground
                            part.Dismember();
                            break;
                        }
                    }
                }
            }

            if (!IsChimera) {
                // reset the dismembered parts so anything gained here can't be regenerated
                target.Body.DismemberedParts = DismemberedParts;
                // reset the accomplishments so we don't get a bunch of dismemberments
                XRLCore.Core.Game.SetObjectGameState("Journal.Accomplishments", Accomplishments);
            }
        }

        public override void Remove(GameObject target, Effect parent)
        {
        }
    }
}