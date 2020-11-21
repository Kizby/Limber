namespace XRL.World.Parts {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using XRL.Language;
    using Sheeter;
    using XRL.UI;
    using XRL.World.Capabilities;
    using XRL.World.Limber;
    using XRL.World.Parts.Mutation;

    [Serializable]
    [MedNamesExtension]
    public class Limber_Tonic_Applicator : IPart, IMedNamesExtension {
        private static readonly Regex _extraLines = new Regex("\n\n+");

        public override void Register(GameObject Object) {
            Object.RegisterPartEvent(this, "ApplyTonic");
            base.Register(Object);
        }

        public override bool FireEvent(Event E) {
            if (E.ID == "ApplyTonic") {
                GameObject target = E.GetGameObjectParameter("Target");
                target.PermuteRandomMutationBuys();
                var Messages = new List<string>();
                var gainLimb = false;
                Mutations mutations = null;
                if (target.IsPlayer()) {
                    Popup.Show("As the tonic floods your veins, you feel the latent genetic pressure of eons.");
                }
                if (ParentObject.HasPart("Temporary") && !target.HasPart("Temporary")) {
                    Messages.Add((target.IsPlayer() ? "Your" :
                                                      Grammar.MakePossessive(target.The + target.ShortDisplayName)) +
                                 " genes ache longingly.");
                } else {
                    var Random = Utility.Random(this, target);
                    var goodColor = ColorCoding.ConsequentialColor(ColorAsGoodFor: target);
                    var badColor = ColorCoding.ConsequentialColor(ColorAsBadFor: target);
                    if (target.IsTrueKin()) {
                        var which = new[] { "Strength", "Agility", "Toughness" }.GetRandomElement(Random);
                        if (target.HasStat(which)) {
                            ++target.Statistics[which].BaseValue;
                            Messages.Add(goodColor +
                                         (target.IsPlayer() ? "You" : target.The + target.ShortDisplayName) +
                                         target.GetVerb("gain") + " 1 point of " + which + "!");
                        }
                    } else /*if mutant*/ {
                        mutations = target.GetPart<Mutations>();
                        if (target.IsEsper()) {
                            // can't chimerify; reduce glimmer instead
                            int glimmerReduction = Random.Next(1, 5);
                            int currentGlimmer = target.GetPsychicGlimmer();
                            if (glimmerReduction > currentGlimmer) {
                                glimmerReduction = currentGlimmer;
                            }
                            _ = target.ModIntProperty("GlimmerModifier", -glimmerReduction);
                            target.SyncMutationLevelAndGlimmer();
                            Messages.Add("Nerves weave and ossify, unable to escape " +
                                         (target.IsPlayer() ? "your" : Grammar.MakePossessive(target.the + target.ShortDisplayName)) +
                                         " psychic cage.");
                        } else if (!target.IsChimera()) {
                            var mentals = target.GetMentalMutations();
                            var mentalDefects = target.GetMutationsOfCategory("MentalDefects");
                            var mpToTransfer = 0;
                            if (mentals.Count > 0 || mentalDefects.Count > 0) {
                                Messages.Add((target.IsPlayer() ? "Your" :
                                                                  Grammar.MakePossessive(target.The + target.ShortDisplayName)) +
                                             " genome sloughs off superfluous layers of alien thoughtstuff.");
                                Messages.Add("");
                            }
                            if (mentals.Count > 0) {
                                // choose 1d4 MP of investment in mental mutations to remove...
                                mpToTransfer = Random.Next(1, 5);
                                var totalLevels = mentals.Map(m => m.BaseLevel).Sum();
                                var toReduce = new List<Tuple<BaseMutation, int>>(mpToTransfer);
                                if (totalLevels <= mpToTransfer) {
                                    // remove everything, will be eligible for Chimera
                                    mpToTransfer = totalLevels;
                                    foreach (var mental in mentals) {
                                        toReduce.Add(Tuple.Create(mental, mental.BaseLevel));
                                    }
                                } else {
                                    // magically pick mpToTransfer mutations weighted by level
                                    var remainingLevels = totalLevels;
                                    var remainingReduction = mpToTransfer;
                                    foreach (var mental in mentals) {
                                        var thisMentalReduction = 0;
                                        while (0 < remainingReduction && Random.Next(0, remainingLevels) < mental.BaseLevel - thisMentalReduction + remainingReduction - 1) {
                                            ++thisMentalReduction;
                                            --remainingLevels;
                                            --remainingReduction;
                                        }
                                        if (0 < thisMentalReduction) {
                                            toReduce.Add(Tuple.Create(mental, thisMentalReduction));
                                        }
                                        remainingLevels -= mental.BaseLevel - thisMentalReduction;
                                        if (0 >= remainingReduction) {
                                            break;
                                        }
                                    }
                                }
                                // ... remove them...
                                var lostMentals = new List<MutationEntry>();
                                foreach (var mental in toReduce) {
                                    var mutation = mental.Item1;
                                    var reduction = mental.Item2;
                                    if (mutation.BaseLevel <= reduction) {
                                        // remove the mutation altogether
                                        lostMentals.Add(mutation.GetMutationEntry());
                                        mutations.RemoveMutation(mutation);
                                    } else {
                                        // reduce the mutation level
                                        mutations.LevelMutation(mutation, mutation.BaseLevel - reduction);
                                    }
                                }
                                // ... and replace any lost mental mutations with physical mutations
                                foreach (var mental in lostMentals) {
                                    // expensive to regenerate this each time, but won't be very many times and
                                    // want to make sure exclusions from e.g. Stinger are handled correctly
                                    Messages.Add(badColor +
                                                 (target.IsPlayer() ? "You" : target.The + target.ShortDisplayName) +
                                                 target.GetVerb("lose") + " " + mental.DisplayName + "!");
                                    var eligiblePhysicals = mutations.GetMutatePool(m => m.Category.Name.EndsWith("Physical")).Shuffle(Random);
                                    var similarPhysicals = eligiblePhysicals.Where(p => p.Cost == mental.Cost);
                                    var otherPhysicals = eligiblePhysicals.Where(p => p.Cost != mental.Cost);
                                    foreach (var physical in similarPhysicals.Concat(otherPhysicals)) {
                                        _ = mutations.AddMutation(physical, 1);
                                        Messages.Add(goodColor +
                                                     (target.IsPlayer() ? "You" : target.The + target.ShortDisplayName) +
                                                     target.GetVerb("gain") + " " + physical.DisplayName + "!");
                                        --mpToTransfer;
                                        break;
                                    } // else if there are no valid physical mutations, don't add anything new
                                }
                            }
                            while (0 < mpToTransfer) {
                                var physicals = target.GetPhysicalMutations();
                                BaseMutation which = null;
                                var canLevelNormally = physicals.Where(m => m.CanIncreaseLevel());
                                if (canLevelNormally.Any()) {
                                    which = canLevelNormally.GetRandomElement(Random);
                                } else {
                                    var underMaxLevel = physicals.Where(m => m.BaseLevel < m.MaxLevel);
                                    if (underMaxLevel.Any()) {
                                        which = underMaxLevel.GetRandomElement(Random);
                                    }
                                }
                                if (which != null) {
                                    mutations.LevelMutation(which, which.BaseLevel + 1);
                                    --mpToTransfer;
                                } else {
                                    // no physical mutations to put the levels in, spend the rest on a new one
                                    foreach (var physical in mutations.GetMutatePool(m => m.Category.Name.EndsWith("Physical")).Shuffle(Random)) {
                                        _ = mutations.AddMutation(physical, 1);
                                        Messages.Add(goodColor +
                                                     (target.IsPlayer() ? "You" : target.The + target.ShortDisplayName) +
                                                     target.GetVerb("gain") + " " + physical.DisplayName + "!");
                                        break;
                                    }
                                    mpToTransfer = 0;
                                }
                            }
                            if (target.GetMentalMutations().Count == 0) {
                                foreach (var mutation in mentalDefects) {
                                    mutations.RemoveMutation(mutation);

                                    var mental = mutation.GetMutationEntry();
                                    Messages.Add(goodColor +
                                                 (target.IsPlayer() ? "You" : target.The + target.ShortDisplayName) +
                                                 target.GetVerb("lose") + " " + mental.DisplayName + "!");

                                    var eligibleDefects = MutationFactory.GetMutationsOfCategory("PhysicalDefects").Shuffle(Random);
                                    var similarDefects = eligibleDefects.Where(p => p.Cost == mental.Cost);
                                    var otherDefects = eligibleDefects.Where(p => p.Cost != mental.Cost);
                                    foreach (var physical in similarDefects.Concat(otherDefects)) {
                                        _ = mutations.AddMutation(physical, 1);
                                        Messages.Add(badColor +
                                                     (target.IsPlayer() ? "You" : target.The + target.ShortDisplayName) +
                                                     target.GetVerb("gain") + " " + physical.DisplayName + "!");
                                        break;
                                    } // else if there are no valid physical defects, don't add anything new
                                }

                                target.Property["MutationLevel"] =
                                    target.Property.GetValueOrDefault("MutationLevel", "") + "Chimera";
                                Messages.Add("");
                                Messages.Add(goodColor +
                                             (target.IsPlayer() ? "You" : target.The + target.ShortDisplayName) +
                                             target.GetVerb("become") + " a Chimera!");
                            }
                        } else /*target.IsChimera*/ {
                            // 50% chance of new limb, 50% chance of mutation level gain
                            if (Random.Next(2) == 0) {
                                // new limb! defer until the other messages are shown to actually gain it
                                gainLimb = true;
                            } else {
                                var physicals = target.GetPhysicalMutations().Where(m => m.CanLevel());
                                if (physicals.Any()) {
                                    // +1 to level of a physical mutation, uncapped
                                    var which = physicals.GetRandomElement(Random);
                                    const string source = "{{r-r-r-R-R-W distribution|limbic fluid}} injections";
                                    var found = false;
                                    foreach (var mod in mutations.MutationMods) {
                                        if (mod.sourceName == source && mod.mutationName == which.Name) {
                                            ++mod.bonus;
                                            found = true;
                                        }
                                    }
                                    if (!found) {
                                        _ = mutations.AddMutationMod(which.Name, 1, Mutations.MutationModifierTracker.SourceType.StatMod, source);
                                    }
                                    Messages.Add(goodColor +
                                                 (target.IsPlayer() ? "You" : target.The + target.ShortDisplayName) +
                                                 target.GetVerb("gain") + " one rank of " + which.DisplayName + "!");
                                } else {
                                    // +1 MP if we can't level anything
                                    target.GainMP(1);
                                    Messages.Add(goodColor +
                                                 (target.IsPlayer() ? "You" : target.The + target.ShortDisplayName) +
                                                 target.GetVerb("gain") + " one MP!");
                                }
                            }
                        }
                    }
                }
                if (target.IsPlayer() && Messages.Count > 0) {
                    var output = string.Join("\n", Messages);
                    output = _extraLines.Replace(output, "\n\n");
                    Popup.Show(output);
                } else {
                    foreach (var Message in Messages) {
                        AddPlayerMessage(Message);
                    }
                }
                if (gainLimb) {
                    _ = mutations.AddChimericBodyPart();
                }
            }
            return base.FireEvent(E);
        }

        /// <summary>
        /// need to add a new tonic description since we're adding a tonic
        /// </summary>
        public int Priority() {
            // unlikely to collide with another mod's priority
            return 284844701;
        }

        public void OnInitializeMedNames(List<string> medNames) {
            medNames.Add("pink,&M");
        }
    }
}
