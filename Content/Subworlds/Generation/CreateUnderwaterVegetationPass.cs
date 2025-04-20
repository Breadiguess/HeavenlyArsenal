using HeavenlyArsenal.Content.Tiles.ForgottenShrine;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
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

        // Create underwater trees.
        ushort treeID = (ushort)ModContent.TileType<UnderwaterTree>();
        for (int i = 0; i < ForgottenShrineGenerationHelpers.UnderwaterTreeCount; i++)
        {
            float xInterpolant = i / (float)(ForgottenShrineGenerationHelpers.UnderwaterTreeCount - 1f);
            int x = (int)MathHelper.Lerp(50f, Main.maxTilesX - 50f, xInterpolant) + WorldGen.genRand.Next(-24, 24);
            int y = Main.maxTilesY - ForgottenShrineGenerationHelpers.GroundDepth - 1;
            Main.tile[x, y].TileType = treeID;
            Main.tile[x, y].Get<TileWallWireStateData>().HasTile = true;
            TileEntity.PlaceEntityNet(x, y, ModContent.TileEntityType<TEUnderwaterTree>());
        }

        // Create cattails.
        int cattailCount = ForgottenShrineGenerationHelpers.CattailCount;
        for (int i = 0; i < cattailCount; i++)
        {
            float xInterpolant = i / (float)(cattailCount - 1f);
            int x = (int)MathHelper.Lerp(20f, Main.maxTilesX - 20f, xInterpolant) + WorldGen.genRand.Next(-20, 20);
            int y = Main.maxTilesY - ForgottenShrineGenerationHelpers.GroundDepth - 1;
            int height = ForgottenShrineGenerationHelpers.WaterDepth + WorldGen.genRand.Next(1, ForgottenShrineGenerationHelpers.MaxCattailHeight);
            GenerateCattail(x, y, height);
        }

        // Create lilypads.
        for (int i = 0; i < ForgottenShrineGenerationHelpers.LilypadCount; i++)
        {
            int x = WorldGen.genRand.Next(10, Main.maxTilesX - 10);
            int y = Main.maxTilesY - ForgottenShrineGenerationHelpers.GroundDepth - ForgottenShrineGenerationHelpers.WaterDepth;

            if (Framing.GetTileSafely(x, y).HasTile)
                continue;

            Main.tile[x, y].Get<TileWallWireStateData>().HasTile = true;
            Main.tile[x, y].TileType = TileID.LilyPad;
            Main.tile[x, y].TileFrameX = (short)(18 * WorldGen.genRand.Next(3, 15));
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
