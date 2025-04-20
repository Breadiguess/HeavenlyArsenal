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

public class BridgeDetailsPass : GenPass
{
    public BridgeDetailsPass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating bridge details.";

        // Adorn the bridge with ropes and create beams.
        int bridgeWidth = ForgottenShrineGenerationHelpers.BridgeArchWidth;
        int groundLevelY = Main.maxTilesY - ForgottenShrineGenerationHelpers.GroundDepth;
        int waterLevelY = groundLevelY - ForgottenShrineGenerationHelpers.WaterDepth;
        int bridgeLowYPoint = waterLevelY - ForgottenShrineGenerationHelpers.BridgeBeamHeight;
        int innerRopeSpacing = (bridgeWidth - ForgottenShrineGenerationHelpers.BridgeUndersideRopeWidth) / 2;
        int bridgeThickness = ForgottenShrineGenerationHelpers.BridgeThickness;

        for (int x = 0; x < Main.maxTilesX; x++)
        {
            if (x >= 5 && x < Main.maxTilesX - 5 && x % bridgeWidth == 0)
                PlaceBeam(groundLevelY, x, bridgeLowYPoint);

            int ropeY = bridgeLowYPoint - bridgeThickness - ForgottenShrineGenerationHelpers.CalculateArchHeight(x, out _);
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

                ShrineRopeSystem.Register(new ShrineRopeData(start, end, ForgottenShrineGenerationHelpers.BridgeUndersideRopeSag * 16f));
            }
        }

        // Create lanterns beneath the bridge.
        int decorationStartY = bridgeLowYPoint - bridgeThickness;
        PlaceLanterns(decorationStartY, 3);
        PlaceOfuda(decorationStartY, 5);
        GenerateRoof(bridgeLowYPoint);
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
        float intermediateArcsine = MathF.Asin(0.5f / ForgottenShrineGenerationHelpers.BridgeArchHeight);
        int beamWidth = (int)MathF.Round(intermediateArcsine * ForgottenShrineGenerationHelpers.BridgeArchWidth / MathHelper.Pi);
        if (ForgottenShrineGenerationHelpers.BridgeArchHeight == 0)
            beamWidth = ForgottenShrineGenerationHelpers.BridgeArchWidth / 33;

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
        int bridgeWidth = ForgottenShrineGenerationHelpers.BridgeArchWidth;
        for (int x = bridgeWidth / 2; x < Main.maxTilesX - bridgeWidth / 2; x += bridgeWidth)
        {
            int archOffset = ForgottenShrineGenerationHelpers.CalculateArchHeight(x, out _);
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
        int bridgeWidth = ForgottenShrineGenerationHelpers.BridgeArchWidth;
        int ofudaID = ModContent.TileType<PlacedOfuda>();
        for (int x = bridgeWidth / 2; x < Main.maxTilesX - bridgeWidth / 2; x += bridgeWidth)
        {
            int archOffset = ForgottenShrineGenerationHelpers.CalculateArchHeight(x, out _);
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
        int bridgeWidth = ForgottenShrineGenerationHelpers.BridgeArchWidth;
        int wallHeight = ForgottenShrineGenerationHelpers.BridgeArchHeight + ForgottenShrineGenerationHelpers.BridgeBackWallHeight;
        int ceilingWallHeight = 4;
        int roofBottomY = archTopY - wallHeight;
        int ofudaID = ModContent.TileType<PlacedOfuda>();
        int pillarSpacing = bridgeWidth / 3;
        int rooftopY = roofBottomY + 1;
        int smallLanternSpacing = bridgeWidth / 19;
        int ofudaSpacing = bridgeWidth / 9;
        int rooftopsPerBridge = ForgottenShrineGenerationHelpers.BridgeRooftopsPerBridge;

        // Create pillars.
        for (int y = archTopY; y >= roofBottomY; y--)
        {
            int height = archTopY - y;
            for (int x = 5; x < Main.maxTilesX - 5; x++)
            {
                if (y >= archTopY - ForgottenShrineGenerationHelpers.CalculateArchHeight(x, out _))
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
                if (y >= archTopY - ForgottenShrineGenerationHelpers.CalculateArchHeight(x, out _))
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
            int tiledBridgeX = x % (bridgeWidth * rooftopsPerBridge);
            if (tiledBridgeX == bridgeWidth / 2)
            {
                ForgottenShrineGenerationHelpers.ShrineRooftopSet rooftopSet = WorldGen.genRand.Next(ForgottenShrineGenerationHelpers.BridgeRooftopConfigurations);
                foreach (var rooftop in rooftopSet.Rooftops)
                    GenerateRooftop(x, rooftopY - rooftop.VerticalOffset, rooftop.Width, rooftop.Height);
            }
        }

        // Adorn the bottom of the roof with cool things.
        for (int x = 5; x < Main.maxTilesX - 5; x++)
        {
            int tiledBridgeX = x % (bridgeWidth * rooftopsPerBridge);

            // Place small lanterns.
            if (tiledBridgeX == bridgeWidth / 2 - smallLanternSpacing ||
                tiledBridgeX == bridgeWidth / 2 + smallLanternSpacing)
            {
                int y = roofBottomY - 4;
                while (Framing.GetTileSafely(x, y).HasTile)
                    y++;

                int lanternVariant = WorldGen.genRand.NextFromList(3, 5, 26);
                WorldGen.PlaceObject(x, y, TileID.HangingLanterns, false, lanternVariant);
            }

            // Place a large dynasty lantern.
            if (tiledBridgeX == bridgeWidth / 2)
            {
                int y = roofBottomY - 4;
                while (Framing.GetTileSafely(x, y).HasTile)
                    y++;

                WorldGen.PlaceObject(x, y, TileID.Chandeliers, false, 22);
            }

            // Place ofuda.
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
