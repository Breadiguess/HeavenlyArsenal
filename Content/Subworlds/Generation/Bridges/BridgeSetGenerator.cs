using HeavenlyArsenal.Content.Tiles.ForgottenShrine;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Subworlds.Generation.Bridges;

public class BridgeSetGenerator(int left, int right)
{
    /// <summary>
    /// The leftmost point of the bridge, in tile coordinates.
    /// </summary>
    public readonly int Left = left;

    /// <summary>
    /// The rightmost part of the bridge, in tile coordinates.
    /// </summary>
    public readonly int Right = right;

    private void GenerateBase()
    {
        int groundLevelY = Main.maxTilesY - ForgottenShrineGenerationHelpers.GroundDepth;
        int waterLevelY = groundLevelY - ForgottenShrineGenerationHelpers.WaterDepth;
        int bridgeLowYPoint = waterLevelY - ForgottenShrineGenerationHelpers.BridgeBeamHeight;
        int bridgeThickness = ForgottenShrineGenerationHelpers.BridgeThickness;

        int[] placeFenceSpokeMap = new int[Right - Left + 1];
        bool[] useDescendingFramesMap = new bool[Right - Left + 1];
        for (int x = Left; x <= Right; x++)
        {
            int index = x - Left;
            int previousHeight = CalculateArchHeight(x - 1);
            int archHeight = CalculateArchHeight(x);
            int nextArchHeight = CalculateArchHeight(x + 1);
            bool ascending = archHeight > previousHeight;
            bool descending = archHeight > nextArchHeight;
            if (ascending)
                placeFenceSpokeMap[index] = archHeight - previousHeight;
            if (descending)
            {
                placeFenceSpokeMap[index] = archHeight - nextArchHeight;
                useDescendingFramesMap[index] = true;
            }
        }

        for (int x = Left; x <= Right; x++)
        {
            int archHeight = CalculateArchHeight(x, out float archHeightInterpolant);

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

    public void Generate()
    {
        GenerateBase();
    }

    /// <summary>
    /// Places the base tiles for the bridge that the player can walk on.
    /// </summary>
    private void PlaceBaseTiles(int x, int archStartingY, int extraThickness)
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
    private void PlaceFence(int x, int archStartingY, int[] placeFenceSpokeMap, bool[] useDescendingFramesMap)
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
        if (x == Left || x == Right)
        {
            fenceFrameX = 0;
            fenceHeight += 2;
        }

        if (placeFenceSpokeMap[x - Left] >= 1)
        {
            fenceHeight += placeFenceSpokeMap[x - Left];
            fenceFrameX = useDescendingFramesMap[x - Left] ? 0 : 1;
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
    private void PlaceWalls(int x, float archHeightInterpolant, int archStartingY, int extraThickness)
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

    /// <summary>
    /// Determines the vertical offset of the bridge's arch at a given X position in tile coordinates, providing the arch height interpolant in the process.
    /// </summary>
    public int CalculateArchHeight(int x, out float archHeightInterpolant)
    {
        x -= Left;

        archHeightInterpolant = MathF.Abs(MathF.Sin(MathHelper.Pi * x / ForgottenShrineGenerationHelpers.BridgeArchWidth));
        float maxHeight = ForgottenShrineGenerationHelpers.BridgeArchHeight;
        if (ForgottenShrineGenerationHelpers.InRooftopBridgeRange(x))
            maxHeight *= ForgottenShrineGenerationHelpers.BridgeArchHeightBigBridgeFactor;

        return (int)MathF.Round(archHeightInterpolant * maxHeight);
    }

    /// <summary>
    /// Determines the vertical offset of the bridge's arch at a given X position in tile coordinates.
    /// </summary>
    public int CalculateArchHeight(int x) => CalculateArchHeight(x, out _);
}
