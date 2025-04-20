using HeavenlyArsenal.Content.Tiles.ForgottenShrine;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public class BaseBridgePass : GenPass
{
    public BaseBridgePass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating the bridge.";

        int groundLevelY = Main.maxTilesY - ForgottenShrineGenerationHelpers.GroundDepth;
        int waterLevelY = groundLevelY - ForgottenShrineGenerationHelpers.WaterDepth;
        int bridgeLowYPoint = waterLevelY - ForgottenShrineGenerationHelpers.BridgeBeamHeight;
        int bridgeThickness = ForgottenShrineGenerationHelpers.BridgeThickness;

        int[] placeFenceSpokeMap = new int[Main.maxTilesX];
        bool[] useDescendingFramesMap = new bool[Main.maxTilesX];
        for (int x = 1; x < Main.maxTilesX - 1; x++)
        {
            int previousHeight = ForgottenShrineGenerationHelpers.CalculateArchHeight(x - 1);
            int archHeight = ForgottenShrineGenerationHelpers.CalculateArchHeight(x);
            int nextArchHeight = ForgottenShrineGenerationHelpers.CalculateArchHeight(x + 1);
            bool ascending = archHeight > previousHeight;
            bool descending = archHeight > nextArchHeight;
            if (ascending)
                placeFenceSpokeMap[x] = archHeight - previousHeight;
            if (descending)
            {
                placeFenceSpokeMap[x] = archHeight - nextArchHeight;
                useDescendingFramesMap[x] = true;
            }
        }

        for (int x = 0; x < Main.maxTilesX; x++)
        {
            int archHeight = ForgottenShrineGenerationHelpers.CalculateArchHeight(x, out float archHeightInterpolant);

            // Place base bridge tiles.
            int extraThickness = (int)Utils.Remap(archHeightInterpolant, 0.6f, 0f, 0f, bridgeThickness * 1.25f);
            int archStartingY = bridgeLowYPoint - archHeight;
            PlaceBaseTiles(x, archStartingY, extraThickness);

            // Create walls underneath the bridge.
            PlaceWalls(x, archHeightInterpolant, archStartingY, extraThickness);

            // Place fences atop the bridge.
            PlaceFence(x, archStartingY, placeFenceSpokeMap, useDescendingFramesMap);
        }
    }

    /// <summary>
    /// Places the base tiles for the bridge that the player can walk on.
    /// </summary>
    private static void PlaceBaseTiles(int x, int archStartingY, int extraThickness)
    {
        int bridgeThickness = ForgottenShrineGenerationHelpers.BridgeThickness;
        for (int dy = -extraThickness; dy < bridgeThickness; dy++)
        {
            int archY = archStartingY - dy;
            int tileID = TileID.GrayBrick;
            if (dy >= bridgeThickness - 2)
                tileID = TileID.RedDynastyShingles;
            else if (dy >= bridgeThickness - 4)
                tileID = TileID.DynastyWood;

            WorldGen.PlaceTile(x, archY, tileID);
        }
    }

    /// <summary>
    /// Places guardrail fences above the bridge.
    /// </summary>
    private static void PlaceFence(int x, int archStartingY, int[] placeFenceSpokeMap, bool[] useDescendingFramesMap)
    {
        int bridgeWidth = ForgottenShrineGenerationHelpers.BridgeArchWidth;
        int bridgeThickness = ForgottenShrineGenerationHelpers.BridgeThickness;
        int fenceHeight = 4;
        int fenceFrameX = 2;
        int fenceXPosition = x % bridgeWidth;
        if (fenceXPosition == bridgeWidth / 3 || fenceXPosition == bridgeWidth * 2 / 3)
            fenceFrameX = 3;
        if (fenceXPosition == bridgeWidth / 2)
            fenceFrameX = 4;

        if (placeFenceSpokeMap[x] >= 1)
        {
            fenceHeight += placeFenceSpokeMap[x];
            fenceFrameX = useDescendingFramesMap[x] ? 0 : 1;
        }

        for (int dy = 0; dy < fenceHeight; dy++)
        {
            int fenceY = archStartingY - bridgeThickness - dy;
            Tile t = Main.tile[x, fenceY];
            t.TileType = (ushort)ModContent.TileType<CrimsonFence>();
            t.HasTile = true;
            t.TileFrameX = (short)(fenceFrameX * 18);

            int frameY = 2;
            if (dy == fenceHeight - 1)
                frameY = 0;
            if (dy == fenceHeight - 2)
                frameY = 1;
            if (dy == 0)
                frameY = 3;

            t.TileFrameY = (short)(frameY * 18);
        }
    }

    /// <summary>
    /// Places walls below the bridge.
    /// </summary>
    private static void PlaceWalls(int x, float archHeightInterpolant, int archStartingY, int extraThickness)
    {
        int bridgeThickness = ForgottenShrineGenerationHelpers.BridgeThickness;
        int wallHeight = (int)MathF.Round(MathHelper.Lerp(8f, 2f, MathF.Pow(archHeightInterpolant, 1.7f)));
        for (int dy = -extraThickness - wallHeight; dy < bridgeThickness - 2; dy++)
        {
            int wallY = archStartingY - dy;
            WorldGen.PlaceWall(x, wallY, WallID.LivingWood);
            WorldGen.paintWall(x, wallY, PaintID.GrayPaint);
        }
    }
}
