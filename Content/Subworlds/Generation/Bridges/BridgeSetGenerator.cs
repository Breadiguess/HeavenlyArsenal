using HeavenlyArsenal.Content.Tiles.ForgottenShrine;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
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

    public void Generate()
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

        int decorationStartY = bridgeLowYPoint - bridgeThickness;

        PlaceBeams(groundLevelY, bridgeLowYPoint);
        PlaceRopesUnderneathBridge(decorationStartY);
        PlaceLanternsUnderneathBridge(decorationStartY, 3);
        PlaceOfudaUnderneathBridge(decorationStartY, 5);
        GenerateRoof(bridgeLowYPoint);
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
        int fenceXPosition = CalculateXWrappedBySingleBridge(x);
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
    /// Places beams into the water at points where the arches are at their nadir.
    /// </summary>
    private void PlaceBeams(int groundLevelY, int bridgeLowYPoint)
    {
        for (int x = Left; x < Right; x++)
        {
            if (CalculateXWrappedBySingleBridge(x) == 0)
                PlaceBeam(groundLevelY, x, bridgeLowYPoint);
        }
    }

    /// <summary>
    /// Places a bridge beam that descends into the water below.
    /// </summary>
    private static void PlaceBeam(int groundLevelY, int startingX, int startingY)
    {
        int beamWidth = ForgottenShrineGenerationHelpers.BridgeBeamWidth;
        for (int dx = -beamWidth; dx <= beamWidth; dx++)
        {
            int x = startingX + dx;
            if (x < 0 || x >= Main.maxTilesX)
                continue;

            bool isBeamEdge = Math.Abs(dx) == beamWidth;
            ushort wallID = isBeamEdge ? WallID.LivingWood : WallID.GrayBrick;
            for (int y = startingY; y < groundLevelY; y++)
            {
                Main.tile[x, y].WallType = wallID;
                WorldGen.paintWall(x, y, PaintID.None);
            }
        }
    }

    /// <summary>
    /// Places ropes beneath bridges.
    /// </summary>
    private void PlaceRopesUnderneathBridge(int startY)
    {
        int bridgeWidth = ForgottenShrineGenerationHelpers.BridgeArchWidth;
        int innerRopeSpacing = (bridgeWidth - ForgottenShrineGenerationHelpers.BridgeUndersideRopeWidth) / 2;
        for (int x = Left; x < Right; x++)
        {
            // Only place ropes beneath bridges with a rooftop, to make them feel more sepcial
            if (!ForgottenShrineGenerationHelpers.InRooftopBridgeRange(x))
                continue;

            int ropeY = startY - ForgottenShrineGenerationHelpers.CalculateArchHeight(x);
            if (CalculateXWrappedBySingleBridge(x) == innerRopeSpacing)
            {
                Vector2 start = new Point(x, ropeY).ToWorldCoordinates();
                Vector2 end = new Point(x + bridgeWidth - innerRopeSpacing * 2, ropeY).ToWorldCoordinates();
                while (Framing.GetTileSafely(start).HasTile)
                    start.Y += 16f;
                while (Framing.GetTileSafely(end).HasTile)
                    end.Y += 16f;

                start.Y -= 11f;
                end.Y -= 11f;

                ShrineRopeSystem.Register(new ShrineRopeData(start.ToPoint(), end.ToPoint(), ForgottenShrineGenerationHelpers.BridgeUndersideRopeSag * 16f));
            }
        }
    }

    /// <summary>
    /// Places paper lanterns underneath bridges.
    /// </summary>
    private void PlaceLanternsUnderneathBridge(int startY, int spacing)
    {
        int bridgeWidth = ForgottenShrineGenerationHelpers.BridgeArchWidth;
        for (int x = bridgeWidth / 2; x < Right - bridgeWidth / 2; x += bridgeWidth)
        {
            if (x < Left || x >= Right)
                continue;

            int archOffset = ForgottenShrineGenerationHelpers.CalculateArchHeight(x);
            bool onlyPlaceInCenter = !ForgottenShrineGenerationHelpers.InRooftopBridgeRange(x);
            for (int dx = -1; dx <= 1; dx++)
            {
                int tileID = TileID.ChineseLanterns;
                int tileStyle = 0;

                // DON'T place the center lantern by default.
                if (dx == 0 && !onlyPlaceInCenter)
                    continue;

                // Only place the center lantern if necessary.
                if (dx != 0 && onlyPlaceInCenter)
                    continue;

                Point lanternPoint = ForgottenShrineGenerationHelpers.DescendToAir(new Point(x + dx * spacing, startY - archOffset));
                WorldGen.PlaceObject(lanternPoint.X, lanternPoint.Y, tileID, false, tileStyle);
            }
        }
    }

    /// <summary>
    /// Places ofuda underneath bridges.
    /// </summary>
    private void PlaceOfudaUnderneathBridge(int startY, int spacing)
    {
        int bridgeWidth = ForgottenShrineGenerationHelpers.BridgeArchWidth;
        int ofudaID = ModContent.TileType<PlacedOfuda>();
        for (int x = Left + bridgeWidth / 2; x < Right - bridgeWidth / 2; x += bridgeWidth)
        {
            int archOffset = ForgottenShrineGenerationHelpers.CalculateArchHeight(x);
            int ofudaOnEachSide = ForgottenShrineGenerationHelpers.InRooftopBridgeRange(x) ? 3 : 1;
            for (int dx = -ofudaOnEachSide; dx <= ofudaOnEachSide; dx++)
            {
                if (dx == 0)
                    continue;

                Point ofudaPoint = ForgottenShrineGenerationHelpers.DescendToAir(new Point(x + dx * spacing, startY - archOffset));
                Main.tile[ofudaPoint.X, ofudaPoint.Y].TileType = (ushort)ofudaID;
                Main.tile[ofudaPoint.X, ofudaPoint.Y].Get<TileWallWireStateData>().HasTile = true;
                TileEntity.PlaceEntityNet(ofudaPoint.X, ofudaPoint.Y, ModContent.TileEntityType<TEPlacedOfuda>());
            }
        }
    }

    /// <summary>
    /// Generates a roof out of patterned walls and periodic rooftop tiles at the center of bridges.
    /// </summary>
    private void GenerateRoof(int archTopY)
    {
        int bridgeWidth = ForgottenShrineGenerationHelpers.BridgeArchWidth;
        int wallHeight = ForgottenShrineGenerationHelpers.BridgeArchHeight + ForgottenShrineGenerationHelpers.BridgeBackWallHeight;
        int roofWallUndersideHeight = ForgottenShrineGenerationHelpers.BridgeRoofWallUndersideHeight;
        int roofBottomY = archTopY - wallHeight;
        int pillarSpacing = bridgeWidth / 3;
        int rooftopY = roofBottomY + 1;
        int rooftopsPerBridge = ForgottenShrineGenerationHelpers.BridgeRooftopsPerBridge;

        for (int x = Left; x < Right; x++)
        {
            int distanceFromPillar = TriangleWaveDistance(x, pillarSpacing);
            int patternHeight = (int)MathF.Round(MathHelper.Lerp(3f, 1f, LumUtils.Cos01(MathHelper.TwoPi * x / bridgeWidth * 3f)));
            for (int y = archTopY; y >= roofBottomY; y--)
            {
                int height = archTopY - y;
                if (y >= archTopY - ForgottenShrineGenerationHelpers.CalculateArchHeight(x))
                    continue;

                // Create pillars.
                if (distanceFromPillar == 1)
                    WorldGen.PlaceWall(x, y, WallID.WhiteDynasty);
                if (distanceFromPillar <= 3 && height == wallHeight - roofWallUndersideHeight - 1)
                    WorldGen.PlaceWall(x, y, WallID.WhiteDynasty);
                if (distanceFromPillar == 2 && height == wallHeight - roofWallUndersideHeight - 2)
                    WorldGen.PlaceWall(x, y, WallID.WhiteDynasty);
                if (distanceFromPillar == 0)
                    WorldGen.PlaceWall(x, y, WallID.Wood);

                // Create painted white dynasty tiles at the top.
                if (height >= wallHeight - roofWallUndersideHeight)
                {
                    WorldGen.KillWall(x, y);

                    ushort wallID = height >= wallHeight - patternHeight && height < wallHeight ? WallID.AshWood : WallID.WhiteDynasty;
                    WorldGen.PlaceWall(x, y, wallID);
                    WorldGen.paintWall(x, y, PaintID.SkyBluePaint);
                }
            }
        }

        // Place a roof over center points on the bridge.
        for (int x = Left; x < Right; x++)
        {
            int tiledBridgeSetX = CalculateXWrappedByBridgeSet(x);
            if (tiledBridgeSetX == bridgeWidth / 2)
            {
                var rooftopSet = WorldGen.genRand.Next(ForgottenShrineGenerationHelpers.BridgeRooftopConfigurations);
                foreach (var rooftop in rooftopSet.Rooftops)
                    GenerateRooftop(x, rooftopY - rooftop.VerticalOffset, rooftop.Width, rooftop.Height);
            }
        }

        // Adorn the bottom of the roof with cool things.
        // This has be done separately from the rooptop generation loop because otherwise the rooftops may be incomplete, making it impossible to place decorations at certain spots.
        for (int x = Left; x < Right; x++)
        {
            PlaceDecorationsUnderneathRooftop(x, roofBottomY);
            PlaceDecorationsAboveTopOfArch(x, roofBottomY);
        }
    }

    /// <summary>
    /// Places decorations underneath a generated rooftop.
    /// </summary>
    private void PlaceDecorationsUnderneathRooftop(int x, int roofBottomY)
    {
        int bridgeWidth = ForgottenShrineGenerationHelpers.BridgeArchWidth;
        int rooftopsPerBridge = ForgottenShrineGenerationHelpers.BridgeRooftopsPerBridge;
        int smallLanternSpacing = bridgeWidth / 19;
        int ofudaSpacing = bridgeWidth / 9;
        int tiledBridgeSetX = CalculateXWrappedByBridgeSet(x);
        int bridgeIndex = x / bridgeWidth / rooftopsPerBridge;
        int ofudaID = ModContent.TileType<PlacedOfuda>();
        int[] possibleLanternVariants = [3, 5, 26];

        // Place small lanterns.
        if (tiledBridgeSetX == bridgeWidth / 2 - smallLanternSpacing ||
            tiledBridgeSetX == bridgeWidth / 2 + smallLanternSpacing)
        {
            Point lanternPoint = ForgottenShrineGenerationHelpers.DescendToAir(new Point(x, roofBottomY));
            int lanternVariant = possibleLanternVariants[bridgeIndex * 2 % possibleLanternVariants.Length];
            WorldGen.PlaceObject(lanternPoint.X, lanternPoint.Y, TileID.HangingLanterns, false, lanternVariant);
        }

        // Place a large dynasty lantern.
        if (tiledBridgeSetX == bridgeWidth / 2)
        {
            Point lanternPoint = ForgottenShrineGenerationHelpers.DescendToAir(new Point(x, roofBottomY));
            WorldGen.PlaceObject(lanternPoint.X, lanternPoint.Y, TileID.Chandeliers, false, 22);
        }

        // Place ofuda.
        if (tiledBridgeSetX == bridgeWidth / 2 - ofudaSpacing ||
            tiledBridgeSetX == bridgeWidth / 2 + ofudaSpacing)
        {
            Main.tile[x, roofBottomY + 2].TileType = (ushort)ofudaID;
            Main.tile[x, roofBottomY + 2].Get<TileWallWireStateData>().HasTile = true;
            TileEntity.PlaceEntityNet(x, roofBottomY + 2, ModContent.TileEntityType<TEPlacedOfuda>());
        }
    }

    /// <summary>
    /// Places decorations atop the roof walls where there isn't a rooftop.
    /// </summary>
    private void PlaceDecorationsAboveTopOfArch(int x, int roofBottomY)
    {
        int bridgeWidth = ForgottenShrineGenerationHelpers.BridgeArchWidth;
        int tiledBridgeX = CalculateXWrappedBySingleBridge(x);
        bool atArchWithoutRooftop = x / bridgeWidth % ForgottenShrineGenerationHelpers.BridgeRooftopsPerBridge != 0;
        if (!atArchWithoutRooftop)
            return;

        if (tiledBridgeX == bridgeWidth / 2)
        {
            int tapestryID = ModContent.TileType<EnigmaticTapestry>();
            Main.tile[x, roofBottomY + 2].TileType = (ushort)tapestryID;
            Main.tile[x, roofBottomY + 2].Get<TileWallWireStateData>().HasTile = true;
            TileEntity.PlaceEntityNet(x, roofBottomY + 2, ModContent.TileEntityType<TEEnigmaticTapestry>());
        }
    }

    /// <summary>
    /// Generates a rooftop at a given position for a bridge.
    /// </summary>
    private static void GenerateRooftop(int x, int y, int roofWidth, int roofHeight)
    {
        if (roofHeight <= 1)
            return;

        int dynastyWoodLayerHeight = ForgottenShrineGenerationHelpers.BridgeRooftopDynastyWoodLayerHeight;
        for (int dy = 0; dy < roofHeight; dy++)
        {
            float heightInterpolant = dy / (float)(roofHeight - 1f);
            int width = (int)Math.Ceiling(MathF.Pow(1f - heightInterpolant, 2.3f) * roofWidth * 0.5f + 0.001f);
            for (int dx = -width; dx <= width; dx++)
            {
                // Shave off a bit of the bottom of the rooftop based on X position since otherwise it looks like a weird christmas tree.
                int verticalCull = (int)MathF.Round((1f - LumUtils.Convert01To010(LumUtils.InverseLerp(-width, width, dx))) * 4f);
                if (dy < verticalCull)
                    continue;

                Point p = new Point(x + dx, y - dy);
                bool isWoodLayer = dy < verticalCull + dynastyWoodLayerHeight && !Framing.GetTileSafely(p).HasTile;
                ushort tileID = isWoodLayer ? TileID.DynastyWood : TileID.BlueDynastyShingles;
                byte paintID = isWoodLayer ? PaintID.RedPaint : PaintID.BluePaint;
                WorldGen.PlaceTile(p.X, p.Y, tileID);
                WorldGen.paintTile(p.X, p.Y, paintID);
            }
        }
    }

    private static int TriangleWaveDistance(int x, int modulo)
    {
        return Math.Abs((x - modulo / 2) % modulo - modulo / 2);
    }

    public int CalculateXWrappedBySingleBridge(int x) => (x - Left) % ForgottenShrineGenerationHelpers.BridgeArchWidth;

    public int CalculateXWrappedByBridgeSet(int x) => (x - Left) % (ForgottenShrineGenerationHelpers.BridgeArchWidth * ForgottenShrineGenerationHelpers.BridgeRooftopsPerBridge);

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
