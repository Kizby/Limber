using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects
{    
    [Serializable]
    public class LimberCookingDomainFungus_UnitGoldpuff : ProceduralCookingEffectUnitMutation<LimberGoldSporesGeneration> {
        public LimberCookingDomainFungus_UnitGoldpuff() {
            AddedTier = "1";
            BonusTier = "0";
        }
    }
    
    [Serializable]
    public class LimberCookingDomainFungus_UnitAzurepuff : ProceduralCookingEffectUnitMutation<LimberAzureSporesGeneration> {
        public LimberCookingDomainFungus_UnitAzurepuff() {
            AddedTier = "1";
            BonusTier = "0";
        }}
    
    [Serializable]
    public class LimberCookingDomainFungus_UnitRosepuff : ProceduralCookingEffectUnitMutation<LimberRoseSporesGeneration> {
        public LimberCookingDomainFungus_UnitRosepuff() {
            AddedTier = "1";
            BonusTier = "0";
        }}
    
    [Serializable]
    public class LimberCookingDomainFungus_UnitJadepuff : ProceduralCookingEffectUnitMutation<LimberJadeSporesGeneration> {
        public LimberCookingDomainFungus_UnitJadepuff() {
            AddedTier = "1";
            BonusTier = "0";
        }}
}