using Terraria.IO;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation.Bridges;

public class BaseBridgePass : GenPass
{
    public BaseBridgePass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating the bridge.";

        int left = ForgottenShrineGenerationHelpers.BridgeStartX;
        int right = left + ForgottenShrineGenerationHelpers.BridgeArchWidth * 12;
        new BridgeSetGenerator(left, right).Generate();
    }
}
