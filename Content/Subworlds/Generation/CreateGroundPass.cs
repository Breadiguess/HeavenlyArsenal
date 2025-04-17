using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public class CreateGroundPass : GenPass
{
    public CreateGroundPass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Placing ground.";

        int groundDepth = ForgottenShrineGenerationConstants.GroundDepth;
        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = Main.maxTilesY - groundDepth; y < Main.maxTilesY; y++)
                WorldGen.PlaceTile(x, y, TileID.Stone);
        }

        int waterDepth = ForgottenShrineGenerationConstants.WaterDepth;
        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = Main.maxTilesY - groundDepth - waterDepth; y < Main.maxTilesY - groundDepth; y++)
                Main.tile[x, y].LiquidAmount = byte.MaxValue;
        }
    }
}
