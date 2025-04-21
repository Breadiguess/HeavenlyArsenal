using Terraria.IO;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public class SetPlayerSpawnPointPass : GenPass
{
    public SetPlayerSpawnPointPass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Setting the player's spawn position.";

        /* TODO -- Update.
        float spacingPerBridge = ForgottenShrineGenerationHelpers.BridgeArchWidth * 16f;
        int groundLevelY = Main.maxTilesY - ForgottenShrineGenerationHelpers.GroundDepth;
        int waterLevelY = groundLevelY - ForgottenShrineGenerationHelpers.WaterDepth;
        int bridgeLowYPoint = waterLevelY - ForgottenShrineGenerationHelpers.BridgeBeamHeight - ForgottenShrineGenerationHelpers.BridgeThickness;
        float x = spacingPerBridge * (ForgottenShrineGenerationHelpers.BridgeRooftopsPerBridge + 0.5f) + ForgottenShrineGenerationHelpers.BridgeStartX * 16f;
        float y = bridgeLowYPoint * 16f + ForgottenShrineGenerationHelpers.CalculateArchHeight((int)(x / 16)) * -16f;

        Main.spawnTileX = (int)(x / 16);
        Main.spawnTileY = (int)(y / 16);
        */
    }
}
