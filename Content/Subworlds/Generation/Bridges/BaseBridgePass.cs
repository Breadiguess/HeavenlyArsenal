using Terraria.IO;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation.Bridges;

public class BaseBridgePass : GenPass
{
    public static readonly BridgeSetGenerator BridgeGenerator = new(ForgottenShrineGenerationHelpers.BridgeStartX, ForgottenShrineGenerationHelpers.BridgeStartX + ForgottenShrineGenerationHelpers.BridgeArchWidth * 12);

    public BaseBridgePass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating the bridge.";

        BridgeGenerator.Generate();
    }
}
