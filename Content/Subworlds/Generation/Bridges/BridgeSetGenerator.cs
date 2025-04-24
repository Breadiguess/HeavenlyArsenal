using HeavenlyArsenal.Content.Tiles.ForgottenShrine;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation.Bridges;

public class BridgeSetGenerator(int left, int right, BridgeGenerationSettings settings)
{
    /// <summary>
    /// The leftmost point of the bridge, in tile coordinates.
    /// </summary>
    public readonly int Left = left;

    /// <summary>
    /// The rightmost part of the bridge, in tile coordinates.
    /// </summary>
    public readonly int Right = right;

    /// <summary>
    /// The settings that define how this bridge set should generate.
    /// </summary>
    public readonly BridgeGenerationSettings Settings = settings;

    public void Generate()
    {
        int groundLevelY = Main.maxTilesY - ForgottenShrineGenerationHelpers.GroundDepth;
        int waterLevelY = groundLevelY - ForgottenShrineGenerationHelpers.WaterDepth;
        int bridgeLowYPoint = waterLevelY - Settings.BridgeBeamHeight;
        int bridgeThickness = Settings.BridgeThickness;

        // Determine placement profile information, including precomputations of arch heights, fence data, etc.
        BridgeSetPlacementProfile profile = new BridgeSetPlacementProfile(this);

        for (int x = Left; x <= Right; x++)
        {
            int spanIndex = x - Left;
            int archHeight = profile.ArchHeights[spanIndex];
            float archHeightInterpolant = profile.ArchHeightInterpolants[spanIndex];

            // Place base bridge tiles.
            int extraThickness = (int)Utils.Remap(archHeightInterpolant, 0.6f, 0f, 0f, bridgeThickness * 1.25f);
            int archStartingY = bridgeLowYPoint - archHeight;
            PlaceBaseTiles(x, archStartingY, extraThickness);

            // Create walls underneath the bridge.
            PlaceWalls(x, archHeightInterpolant, archStartingY, extraThickness);

            // Place fences atop the bridge.
            PlaceFence(x, archStartingY, profile);
        }

        // Place decorations.
        int decorationStartY = bridgeLowYPoint - bridgeThickness;
        PlaceBeams(groundLevelY, bridgeLowYPoint);
        PlaceRopesUnderneathBridge(decorationStartY, profile);
        PlaceDecorationsUnderneathBridge(decorationStartY, 3, profile);
        PlaceOfudaUnderneathBridge(decorationStartY, 5, profile);
        GenerateRoof(bridgeLowYPoint, profile);
    }

    /// <summary>
    /// Places the base tiles for the bridge that the player can walk on.
    /// </summary>
    private void PlaceBaseTiles(int x, int archStartingY, int extraThickness)
    {
        int bridgeThickness = Settings.BridgeThickness;
        for (int dy = -extraThickness; dy < bridgeThickness; dy++)
        {
            int archY = archStartingY - dy;
            int tileID = DetermineBaseTileIDByHeight(dy, bridgeThickness);
            WorldGen.PlaceTile(x, archY, tileID);
        }
    }

    internal static int DetermineBaseTileIDByHeight(int y, int height)
    {
        int tileID = TileID.GrayBrick;
        if (y >= height - 2)
            tileID = TileID.RedDynastyShingles;
        else if (y >= height - 4)
            tileID = TileID.DynastyWood;

        return tileID;
    }

    /// <summary>
    /// Places guardrail fences above the bridge.
    /// </summary>
    private void PlaceFence(int x, int archStartingY, BridgeSetPlacementProfile profile)
    {
        int bridgeWidth = Settings.BridgeArchWidth;
        int bridgeThickness = Settings.BridgeThickness;
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
            fenceHeight += 7;
        }

        int spanIndex = x - Left;
        if (profile.FenceExtraHeightMap[spanIndex] >= 1)
        {
            fenceHeight += profile.FenceExtraHeightMap[spanIndex];
            fenceFrameX = profile.FenceDescendingFlags[spanIndex] ? 0 : 1;
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
        int bridgeThickness = Settings.BridgeThickness;
        int wallHeight = (int)MathF.Round(MathHelper.Lerp(8f, 2f, MathF.Pow(archHeightInterpolant, 1.7f)));
        for (int dy = -extraThickness - wallHeight; dy < bridgeThickness - 2; dy++)
        {
            int wallY = archStartingY - dy;
            bool isBottom = dy == -extraThickness - wallHeight;
            ushort wallID = isBottom ? WallID.RichMaogany : WallID.GreenDungeonSlab;

            WorldGen.PlaceWall(x, wallY, wallID);
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
    private void PlaceBeam(int groundLevelY, int startingX, int startingY)
    {
        int beamWidth = Settings.BridgeBeamWidth;
        for (int dx = -beamWidth; dx <= beamWidth; dx++)
        {
            int x = startingX + dx;
            if (x < Left || x >= Right)
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
    private void PlaceRopesUnderneathBridge(int startY, BridgeSetPlacementProfile profile)
    {
        int bridgeWidth = Settings.BridgeArchWidth;
        int innerRopeSpacing = (bridgeWidth - Settings.BridgeUndersideRopeWidth) / 2;
        for (int x = Left; x < Right; x++)
        {
            // Only place ropes beneath bridges with a rooftop, to make them feel more sepcial
            if (!InRooftopBridgeRange(x))
                continue;

            int spanIndex = x - Left;
            int ropeY = startY - profile.ArchHeights[spanIndex];
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

                ModContent.GetInstance<OrnamentalShrineRopeSystem>().Register(new OrnamentalShrineRopeData(start.ToPoint(), end.ToPoint(), Settings.BridgeUndersideRopeSag * 16f));
            }
        }
    }

    /// <summary>
    /// Places decorations underneath bridges.
    /// </summary>
    private void PlaceDecorationsUnderneathBridge(int startY, int spacing, BridgeSetPlacementProfile profile)
    {
        int bridgeWidth = Settings.BridgeArchWidth;
        for (int x = Left + bridgeWidth / 2; x < Right - bridgeWidth / 2; x += bridgeWidth)
        {
            if (x < Left || x >= Right)
                continue;

            int spanIndex = x - Left;
            int archOffset = profile.ArchHeights[spanIndex];
            bool onlyPlaceInCenter = !InRooftopBridgeRange(x);
            for (int dx = -1; dx <= 1; dx++)
            {
                int tileID = TileID.ChineseLanterns;
                int tileStyle = 0;
                if (onlyPlaceInCenter)
                {
                    tileID = TileID.Banners;
                    tileStyle = WorldGen.genRand.Next(4);
                }

                // DON'T place the center lantern by default.
                if (dx == 0 && !onlyPlaceInCenter)
                    continue;

                // Only place the center lantern if necessary.
                if (dx != 0 && onlyPlaceInCenter)
                    continue;

                int xOffset = dx >= 1 ? 1 : 0;
                Point lanternPoint = ForgottenShrineGenerationHelpers.DescendToAir(new Point(x + dx * spacing + xOffset, startY - archOffset));
                WorldGen.PlaceObject(lanternPoint.X, lanternPoint.Y, tileID, false, tileStyle);
            }
        }
    }

    /// <summary>
    /// Places ofuda underneath bridges.
    /// </summary>
    private void PlaceOfudaUnderneathBridge(int startY, int spacing, BridgeSetPlacementProfile profile)
    {
        int bridgeWidth = Settings.BridgeArchWidth;
        int ofudaID = ModContent.TileType<PlacedOfuda>();
        for (int x = Left + bridgeWidth / 2; x < Right - bridgeWidth / 2; x += bridgeWidth)
        {
            int spanIndex = x - Left;
            int archOffset = profile.ArchHeights[spanIndex];
            int ofudaOnEachSide = InRooftopBridgeRange(x) ? 3 : 1;
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
    private void GenerateRoof(int archTopY, BridgeSetPlacementProfile profile)
    {
        int bridgeWidth = Settings.BridgeArchWidth;
        int wallHeight = Settings.BridgeArchHeight + Settings.BridgeBackWallHeight;
        int roofWallUndersideHeight = Settings.BridgeRoofWallUndersideHeight;
        int roofBottomY = archTopY - wallHeight;
        int pillarSpacing = bridgeWidth / 3;
        int pillarHeightCutoffWidth = bridgeWidth / 4;

        for (int x = Left; x < Right; x++)
        {
            int distanceFromPillar = TriangleWaveDistance(x - Left, pillarSpacing);
            int spanIndex = x - Left;

            // Scuffed solution to make pillars partially generated the left side of the bridge set.
            if (x <= Left + 5)
                distanceFromPillar = x - Left + 1;

            float cutoffInterpolant = 1f - LumUtils.InverseLerpBump(Left, Left + pillarHeightCutoffWidth, Right - pillarHeightCutoffWidth, Right, x);
            float easedCutoffInterpolant = 1f - MathF.Sqrt(1.001f - cutoffInterpolant.Squared());
            int localWallHeight = (int)(wallHeight * (1f - easedCutoffInterpolant));

            float patternSinusoid = MathF.Pow(6.1f, MathF.Cos(MathHelper.TwoPi * (x - Left) / bridgeWidth * 3f) - 1f);
            int patternHeight = (int)MathF.Round(MathHelper.Lerp(3f, 1f, patternSinusoid));
            if (InRooftopBridgeRange(x))
                patternHeight++;

            for (int y = archTopY; y >= roofBottomY; y--)
            {
                int height = archTopY - y;
                if (y >= archTopY - profile.ArchHeights[spanIndex])
                    continue;
                if (height >= localWallHeight)
                    continue;

                // Create pillars.
                if (distanceFromPillar == 1)
                    WorldGen.PlaceWall(x, y, WallID.WhiteDynasty);
                if (distanceFromPillar <= 3 && height == localWallHeight - roofWallUndersideHeight - 1)
                    WorldGen.PlaceWall(x, y, WallID.WhiteDynasty);
                if (distanceFromPillar == 2 && height == localWallHeight - roofWallUndersideHeight - 2)
                    WorldGen.PlaceWall(x, y, WallID.WhiteDynasty);
                if (distanceFromPillar == 0)
                    WorldGen.PlaceWall(x, y, WallID.Wood);

                // Create painted white dynasty tiles at the top.
                if (height >= localWallHeight - roofWallUndersideHeight)
                {
                    WorldGen.KillWall(x, y);

                    ushort wallID = height >= localWallHeight - patternHeight && height < localWallHeight ? WallID.AshWood : WallID.WhiteDynasty;
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
                var rooftopSet = WorldGen.genRand.Next(Settings.BridgeRooftopConfigurations);
                foreach (var rooftop in rooftopSet.Rooftops)
                    GenerateRooftop(x, roofBottomY - rooftop.VerticalOffset + 1, rooftop.Width, rooftop.Height);
            }
        }

        // Place decorations at points of descent along the bridge.
        for (int x = Left; x < Right; x++)
        {
            int tiledBridgeSetX = CalculateXWrappedByBridgeSet(x);
            if (tiledBridgeSetX == bridgeWidth * Settings.BridgeRooftopsPerBridge / 2 + bridgeWidth / 2)
            {
                Point chandelierPosition = new Point(x, roofBottomY + Settings.BridgeRoofWallUndersideHeight + 3);
                WorldUtils.Gen(new Point(chandelierPosition.X, chandelierPosition.Y - 1), new Shapes.Mound(5, 3), new Actions.PlaceTile(TileID.RedDynastyShingles));

                WorldGen.PlaceObject(chandelierPosition.X, chandelierPosition.Y, TileID.Chandeliers, style: 45);
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
        int bridgeWidth = Settings.BridgeArchWidth;
        int rooftopsPerBridge = Settings.BridgeRooftopsPerBridge;
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
        int bridgeWidth = Settings.BridgeArchWidth;
        if (InRooftopBridgeRange(x))
            return;

        int tiledBridgeX = CalculateXWrappedBySingleBridge(x);
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
    private void GenerateRooftop(int x, int y, int roofWidth, int roofHeight)
    {
        if (roofHeight <= 1)
            return;

        int dynastyWoodLayerHeight = Settings.BridgeRooftopDynastyWoodLayerHeight;
        for (int dy = 0; dy < roofHeight; dy++)
        {
            float heightInterpolant = dy / (float)(roofHeight - 1f);
            int width = (int)Math.Ceiling(MathF.Pow(1f - heightInterpolant, 2.3f) * roofWidth * 0.5f + 0.001f);
            for (int dx = -width; dx <= width; dx++)
            {
                // Shave off a bit of the bottom of the rooftop based on X position since otherwise it looks like a weird christmas tree.
                float horizontalInterpolant = LumUtils.InverseLerp(-width, width, dx);
                float verticalBump = 1f - LumUtils.Convert01To010(horizontalInterpolant);
                int verticalCull = (int)MathF.Round(verticalBump * 4f);
                if (dy < verticalCull)
                    continue;

                Point p = new Point(x + dx, y - dy);
                bool isWoodLayer = dy < verticalCull + dynastyWoodLayerHeight && !Framing.GetTileSafely(p).HasTile;
                ushort tileID = isWoodLayer ? TileID.DynastyWood : TileID.BlueDynastyShingles;
                byte paintID = isWoodLayer ? PaintID.RedPaint : PaintID.BluePaint;

                Tile t = Main.tile[p];
                t.TileType = tileID;
                t.HasTile = true;

                WorldGen.paintTile(p.X, p.Y, paintID);
            }
        }
    }

    private static int TriangleWaveDistance(int x, int modulo)
    {
        return Math.Abs((x - modulo / 2) % modulo - modulo / 2);
    }

    /// <summary>
    /// Calculates what a given X position in tile coordinates is relative to a given bridge for this generator.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CalculateXWrappedBySingleBridge(int x) => (x - Left) % Settings.BridgeArchWidth;

    /// <summary>
    /// Calculates what a given X position in tile coordinates is relative to a given bridge set for this generator.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CalculateXWrappedByBridgeSet(int x) => (x - Left) % (Settings.BridgeArchWidth * Settings.BridgeRooftopsPerBridge);

    /// <summary>
    /// Determines whether a given X position in tile coordinates is in the range of a bridge with a rooftop.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InRooftopBridgeRange(int x) => (x - Left) / Settings.BridgeArchWidth % Settings.BridgeRooftopsPerBridge == 0;

    /// <summary>
    /// Determines whether a given X position in tile coordinates is in the range of a bridge without a rooftop.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InNonRooftopBridgeRange(int x) => (x - Left) / Settings.BridgeArchWidth % Settings.BridgeRooftopsPerBridge != 0;

    /// <summary>
    /// Determines the vertical offset of the bridge's arch at a given X position in tile coordinates, providing the arch height interpolant in the process.
    /// </summary>
    public int CalculateArchHeight(int x, out float archHeightInterpolant)
    {
        archHeightInterpolant = MathF.Abs(MathF.Sin(MathHelper.Pi * (x - Left) / Settings.BridgeArchWidth));
        float maxHeight = Settings.BridgeArchHeight;
        if (InRooftopBridgeRange(x))
            maxHeight *= Settings.BridgeArchHeightBigBridgeFactor;

        return (int)MathF.Round(archHeightInterpolant * maxHeight);
    }

    /// <summary>
    /// Determines the vertical offset of the bridge's arch at a given X position in tile coordinates.
    /// </summary>
    public int CalculateArchHeight(int x) => CalculateArchHeight(x, out _);
}
