using HeavenlyArsenal.Content.Subworlds.Generation.Bridges;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public class ShrinePass : GenPass
{
    public ShrinePass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating a sacred shrine.";

        BridgeGenerationSettings bridgeSettings = BaseBridgePass.BridgeGenerator.Settings;
        int left = BaseBridgePass.BridgeGenerator.Right + ForgottenShrineGenerationHelpers.LakeWidth + bridgeSettings.DockWidth;
        int right = left + ForgottenShrineGenerationHelpers.ShrineIslandWidth;
    }
}
