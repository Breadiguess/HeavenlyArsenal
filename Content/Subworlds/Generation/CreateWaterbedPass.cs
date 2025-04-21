using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public class CreateWaterbedPass : GenPass
{
    public CreateWaterbedPass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Placing the waterbed.";

        int groundDepth = ForgottenShrineGenerationHelpers.GroundDepth;
        for (int y = Main.maxTilesY - groundDepth; y < Main.maxTilesY; y++)
        {
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                Tile t = Main.tile[x, y];
                t.HasTile = true;
                t.TileType = TileID.Mud;
            }
        }

        int waterDepth = ForgottenShrineGenerationHelpers.WaterDepth;
        for (int y = Main.maxTilesY - groundDepth - waterDepth; y < Main.maxTilesY - groundDepth; y++)
        {
            for (int x = 0; x < Main.maxTilesX; x++)
                Main.tile[x, y].LiquidAmount = byte.MaxValue;
        }
    }
}
