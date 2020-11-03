using System;
using XRL.World.Limber;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class LimberSporesGeneration : GasGeneration {
        public readonly string Color;
        public LimberSporesGeneration(string Color) {
            this.Color = Color;
            this.GasObject = Utility.GetFungalGasFromColor(Color);
            SyncFromBlueprint();
        }
        public override int GetReleaseDuration(int Level) => 1;
        public override int GetReleaseCooldown(int Level) => 50;
        public override string GetReleaseAbilityName() => "Puff " + Color + " Spores";
    }

    [Serializable]
    public class LimberGoldSporesGeneration : LimberSporesGeneration {
        public LimberGoldSporesGeneration() : base("Gold") {}
    }

    [Serializable]
    public class LimberAzureSporesGeneration : LimberSporesGeneration {
        public LimberAzureSporesGeneration() : base("Azure") {}
    }

    [Serializable]
    public class LimberRoseSporesGeneration : LimberSporesGeneration {
        public LimberRoseSporesGeneration() : base("Rose") {}
    }

    [Serializable]
    public class LimberJadeSporesGeneration : LimberSporesGeneration {
        public LimberJadeSporesGeneration() : base("Jade") {}
    }
}