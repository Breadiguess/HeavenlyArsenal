using HeavenlyArsenal.Content.Tiles.ForgottenShrine;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public class CreateUnderwaterVegetationPass : GenPass
{
    public CreateUnderwaterVegetationPass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating underwater vegetation.";

        ushort treeID = (ushort)ModContent.TileType<UnderwaterTree>();
        for (int i = 0; i < ForgottenShrineGenerationConstants.UnderwaterTreeCount; i++)
        {
            float xInterpolant = i / (float)(ForgottenShrineGenerationConstants.UnderwaterTreeCount - 1f);
            int x = (int)MathHelper.Lerp(50f, Main.maxTilesX - 50f, xInterpolant) + WorldGen.genRand.Next(-24, 24);
            int y = Main.maxTilesY - ForgottenShrineGenerationConstants.GroundDepth - 1;
            Main.tile[x, y].TileType = treeID;
            Main.tile[x, y].Get<TileWallWireStateData>().HasTile = true;
            TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEUnderwaterTree>());
        }

        for (int i = 0; i < ForgottenShrineGenerationConstants.CattailCount; i++)
        {
            float xInterpolant = i / (float)(ForgottenShrineGenerationConstants.CattailCount - 1f);
            int x = (int)MathHelper.Lerp(20f, Main.maxTilesX - 20f, xInterpolant) + WorldGen.genRand.Next(-15, 15);
            int y = Main.maxTilesY - ForgottenShrineGenerationConstants.GroundDepth - 1;
            int height = ForgottenShrineGenerationConstants.WaterDepth + WorldGen.genRand.Next(1, ForgottenShrineGenerationConstants.MaxCattailHeight);
            GenerateCattail(x, y, height);
        }
    }

    private static void GenerateCattail(int x, int y, int height)
    {
        ushort cattailID = (ushort)ModContent.TileType<ShrineCattail>();
        for (int dy = 0; dy < height; dy++)
        {
            int frame = 5;
            if (dy == 0)
                frame = WorldGen.genRand.Next(5);
            else if (dy == height - 1)
                frame = WorldGen.genRand.Next(9, 13);
            else if (dy == height - 2)
                frame = WorldGen.genRand.Next(6, 9);

            Main.tile[x, y - dy].TileType = cattailID;
            Main.tile[x, y - dy].TileFrameX = (short)(frame * 18);
            Main.tile[x, y - dy].Get<TileWallWireStateData>().HasTile = true;
        }
    }
}
