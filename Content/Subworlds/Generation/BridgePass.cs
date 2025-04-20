using HeavenlyArsenal.Content.Tiles.ForgottenShrine;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public class BridgePass : GenPass
{
    public BridgePass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating the bridge.";

        int bridgeWidth = ForgottenShrineGenerationConstants.BridgeArchWidth;
        int groundLevelY = Main.maxTilesY - ForgottenShrineGenerationConstants.GroundDepth;
        int waterLevelY = groundLevelY - ForgottenShrineGenerationConstants.WaterDepth;
        int bridgeLowYPoint = waterLevelY - ForgottenShrineGenerationConstants.BridgeBeamHeight;
        int bridgeThickness = ForgottenShrineGenerationConstants.BridgeThickness;

        int[] placeFenceSpokeMap = new int[Main.maxTilesX];
        bool[] useDescendingFramesMap = new bool[Main.maxTilesX];
        for (int x = 1; x < Main.maxTilesX - 1; x++)
        {
            int previousHeight = CalculateArchHeight(x - 1, out _);
            int archHeight = CalculateArchHeight(x, out _);
            int nextArchHeight = CalculateArchHeight(x + 1, out _);
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

        // Adorn the bridge with ropes and create beams.
        int innerRopeSpacing = (bridgeWidth - ForgottenShrineGenerationConstants.BridgeUndersideRopeWidth) / 2;
        for (int x = 0; x < Main.maxTilesX; x++)
        {
            if (x >= 5 && x < Main.maxTilesX - 5 && x % bridgeWidth == 0)
                PlaceBeam(groundLevelY, x, bridgeLowYPoint);

            int ropeY = bridgeLowYPoint - bridgeThickness - CalculateArchHeight(x, out _);
            if (x >= 5 && x < Main.maxTilesX - 5 && x % bridgeWidth == innerRopeSpacing)
            {
                Point start = new Point(x, ropeY).ToWorldCoordinates().ToPoint();
                Point end = new Point(x + bridgeWidth - innerRopeSpacing * 2, ropeY).ToWorldCoordinates().ToPoint();
                while (Framing.GetTileSafely(start.ToVector2()).HasTile)
                    start.Y += 16;
                while (Framing.GetTileSafely(end.ToVector2()).HasTile)
                    end.Y += 16;
                start.Y -= 11;
                end.Y -= 11;

                ShrineRopeSystem.Register(new ShrineRopeData(start, end, ForgottenShrineGenerationConstants.BridgeUndersideRopeSag * 16f));
            }
        }

        // Create lanterns beneath the bridge.
        int decorationStartY = bridgeLowYPoint - bridgeThickness;
        PlaceLanterns(decorationStartY, 3);
        PlaceOfuda(decorationStartY, 5);
        GenerateRoof(bridgeLowYPoint);
    }

    /// <summary>
    /// Determines the vertical offset of the bridge's arch at a given X position in tile coordinates.
    /// </summary>
    internal static int CalculateArchHeight(int x, out float archHeightInterpolant)
    {
        archHeightInterpolant = MathF.Abs(MathF.Sin(MathHelper.Pi * x / ForgottenShrineGenerationConstants.BridgeArchWidth));
        return (int)MathF.Round(archHeightInterpolant * ForgottenShrineGenerationConstants.BridgeArchHeight);
    }

    /// <summary>
    /// Places the base tiles for the bridge that the player can walk on.
    /// </summary>
    private static void PlaceBaseTiles(int x, int archStartingY, int extraThickness)
    {
        int bridgeThickness = ForgottenShrineGenerationConstants.BridgeThickness;
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
        int bridgeWidth = ForgottenShrineGenerationConstants.BridgeArchWidth;
        int bridgeThickness = ForgottenShrineGenerationConstants.BridgeThickness;
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
        int bridgeThickness = ForgottenShrineGenerationConstants.BridgeThickness;
        int wallHeight = (int)MathF.Round(MathHelper.Lerp(8f, 2f, MathF.Pow(archHeightInterpolant, 1.7f)));
        for (int dy = -extraThickness - wallHeight; dy < bridgeThickness - 2; dy++)
        {
            int wallY = archStartingY - dy;
            WorldGen.PlaceWall(x, wallY, WallID.LivingWood);
            WorldGen.paintWall(x, wallY, PaintID.GrayPaint);
        }
    }

    /// <summary>
    /// Places a bridge beam that descends into the water below.
    /// </summary>
    private static void PlaceBeam(int groundLevelY, int startingX, int startingY)
    {
        // round(height * abs(sin(pi * x / width))) < 1
        // height * abs(sin(pi * x / width)) < 1
        // abs(sin(pi * x / width)) < 1 / height
        // sin(pi * x / width) < 1 / height
        // sin(pi * x / width) = 1 / height
        // pi * x / width = arcsin(1 / height)
        // x = arcsin(1 / height) * width / pi

        // For a bit of artistic preference, 0.5 will be used instead of 1 like in the original equation, making the beams a bit thinner.
        float intermediateArcsine = MathF.Asin(0.5f / ForgottenShrineGenerationConstants.BridgeArchHeight);
        int beamWidth = (int)MathF.Round(intermediateArcsine * ForgottenShrineGenerationConstants.BridgeArchWidth / MathHelper.Pi);
        if (ForgottenShrineGenerationConstants.BridgeArchHeight == 0)
            beamWidth = ForgottenShrineGenerationConstants.BridgeArchWidth / 33;

        for (int dx = -beamWidth; dx <= beamWidth; dx++)
        {
            int x = startingX + dx;
            bool atEdge = Math.Abs(dx) == beamWidth;
            for (int y = startingY; y < groundLevelY; y++)
            {
                bool useWoodenBeams = atEdge;
                if (useWoodenBeams)
                    Main.tile[x, y].WallType = WallID.LivingWood;
                else
                    Main.tile[x, y].WallType = WallID.GrayBrick;
                WorldGen.paintWall(x, y, PaintID.None);
            }
        }
    }

    /// <summary>
    /// Places paper lanterns underneath the bridge.
    /// </summary>
    private static void PlaceLanterns(int startY, int spacing)
    {
        int bridgeWidth = ForgottenShrineGenerationConstants.BridgeArchWidth;
        for (int x = bridgeWidth / 2; x < Main.maxTilesX - bridgeWidth / 2; x += bridgeWidth)
        {
            int archOffset = CalculateArchHeight(x, out _);
            for (int dx = -1; dx <= 1; dx++)
            {
                Point lanternPoint = new Point(x + dx * spacing, startY - archOffset);
                while (Framing.GetTileSafely(lanternPoint).HasTile)
                    lanternPoint.Y++;

                WorldGen.PlaceObject(lanternPoint.X, lanternPoint.Y, TileID.ChineseLanterns);
            }
        }
    }

    /// <summary>
    /// Places ofuda underneath the bridge.
    /// </summary>
    private static void PlaceOfuda(int startY, int spacing)
    {
        int bridgeWidth = ForgottenShrineGenerationConstants.BridgeArchWidth;
        int ofudaID = ModContent.TileType<PlacedOfuda>();
        for (int x = bridgeWidth / 2; x < Main.maxTilesX - bridgeWidth / 2; x += bridgeWidth)
        {
            int archOffset = CalculateArchHeight(x, out _);
            for (int dx = -2; dx <= 2; dx++)
            {
                if (dx == 0)
                    continue;

                Point ofudaPoint = new Point(x + dx * spacing, startY - archOffset);
                while (Framing.GetTileSafely(ofudaPoint).HasTile)
                    ofudaPoint.Y++;

                Main.tile[ofudaPoint.X, ofudaPoint.Y].TileType = (ushort)ofudaID;
                Main.tile[ofudaPoint.X, ofudaPoint.Y].Get<TileWallWireStateData>().HasTile = true;
                TileEntity.PlaceEntityNet(ofudaPoint.X, ofudaPoint.Y, ModContent.TileEntityType<TEPlacedOfuda>());
            }
        }
    }

    private static void GenerateRoof(int archTopY)
    {
        int bridgeWidth = ForgottenShrineGenerationConstants.BridgeArchWidth;
        int wallHeight = ForgottenShrineGenerationConstants.BridgeArchHeight + 21;
        int ceilingWallHeight = 4;
        int roofBottomY = archTopY - wallHeight;
        int ofudaID = ModContent.TileType<PlacedOfuda>();
        int pillarSpacing = bridgeWidth / 3;
        int rooftopY = roofBottomY + 1;
        int ofudaSpacing = 9;

        // Create pillars.
        for (int y = archTopY; y >= roofBottomY; y--)
        {
            int height = archTopY - y;
            for (int x = 5; x < Main.maxTilesX - 5; x++)
            {
                if (y >= archTopY - CalculateArchHeight(x, out _))
                    continue;

                int distanceFromPillar = TriangleWaveDistance(x, pillarSpacing);
                if (distanceFromPillar == 1)
                    WorldGen.PlaceWall(x, y, WallID.WhiteDynasty);
                if (distanceFromPillar <= 3 && height == wallHeight - ceilingWallHeight - 1)
                    WorldGen.PlaceWall(x, y, WallID.WhiteDynasty);
                if (distanceFromPillar == 2 && height == wallHeight - ceilingWallHeight - 2)
                    WorldGen.PlaceWall(x, y, WallID.WhiteDynasty);
                if (distanceFromPillar == 0)
                    WorldGen.PlaceWall(x, y, WallID.Wood);
            }
        }

        // Create painted white dynasty tiles at the top.
        for (int y = archTopY; y >= roofBottomY; y--)
        {
            int height = archTopY - y;
            for (int x = 5; x < Main.maxTilesX - 5; x++)
            {
                if (y >= archTopY - CalculateArchHeight(x, out _))
                    continue;

                if (height >= wallHeight - ceilingWallHeight)
                {
                    WorldGen.KillWall(x, y);

                    int patternHeight = (int)MathF.Round(MathHelper.Lerp(3f, 1f, LumUtils.Cos01(MathHelper.TwoPi * x / bridgeWidth * 3f)));
                    ushort wallID = height >= wallHeight - patternHeight && height < wallHeight ? WallID.AshWood : WallID.WhiteDynasty;
                    WorldGen.PlaceWall(x, y, wallID);
                    WorldGen.paintWall(x, y, PaintID.SkyBluePaint);
                }
            }
        }

        // Place a roof over center points on the bridge.
        for (int x = 5; x < Main.maxTilesX - 5; x++)
        {
            int tiledBridgeX = x % (bridgeWidth * 2);
            if (tiledBridgeX == bridgeWidth / 2)
            {
                int roofWidth = (int)(ForgottenShrineGenerationConstants.BridgeArchWidth / 1.3f);
                int roofHeight = (int)(ForgottenShrineGenerationConstants.BridgeArchWidth / 2.3f);
                GenerateRooftop(x, rooftopY, roofWidth, roofHeight);
                GenerateRooftop(x, rooftopY - (int)(roofHeight / 3.5f), (int)(roofWidth * 0.62f), (int)(roofHeight * 1.12f));
                GenerateRooftop(x, rooftopY - (int)(roofHeight / 2.3f), (int)(roofWidth * 0.45f), (int)(roofHeight * 0.75f));
            }

            if (tiledBridgeX == bridgeWidth / 2)
            {
                int y = roofBottomY - 4;
                while (Framing.GetTileSafely(x, y).HasTile)
                    y++;

                WorldGen.PlaceObject(x, y, TileID.Chandeliers, false, 22);
            }

            if (tiledBridgeX == bridgeWidth / 2 - ofudaSpacing ||
                tiledBridgeX == bridgeWidth / 2 + ofudaSpacing)
            {
                Main.tile[x, rooftopY + 1].TileType = (ushort)ofudaID;
                Main.tile[x, rooftopY + 1].Get<TileWallWireStateData>().HasTile = true;
                TileEntity.PlaceEntityNet(x, rooftopY + 1, ModContent.TileEntityType<TEPlacedOfuda>());
            }
        }
    }

    private static void GenerateRooftop(int x, int y, int roofWidth, int roofHeight)
    {
        for (int dy = 0; dy < roofHeight; dy++)
        {
            float heightInterpolant = dy / (float)(roofHeight - 1f);
            int width = (int)Math.Ceiling(MathF.Pow(1f - heightInterpolant, 2.3f) * roofWidth * 0.5f + 0.001f);
            for (int dx = -width; dx <= width; dx++)
            {
                // Shave off a bit of the bottom of the rooftop based on X position since otherwise it looks like a werid christmas tree.
                int verticalCull = (int)MathF.Round((1f - LumUtils.Convert01To010(LumUtils.InverseLerp(-width, width, dx))) * 4f);
                if (dy < verticalCull)
                    continue;

                WorldGen.PlaceTile(x + dx, y - dy, TileID.BlueDynastyShingles);
                WorldGen.paintTile(x + dx, y - dy, PaintID.BluePaint);
            }
        }
    }

    private static int TriangleWaveDistance(int x, int modulo)
    {
        return Math.Abs((x - modulo / 2) % modulo - modulo / 2);
    }
}
