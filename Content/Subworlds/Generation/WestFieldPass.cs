using HeavenlyArsenal.Content.Subworlds.Generation.Bridges;
using HeavenlyArsenal.Content.Tiles.ForgottenShrine;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public class WestFieldPass : GenPass
{
    public WestFieldPass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Generating a field of lilies to the west.";

        BridgeGenerationSettings bridgeSettings = BaseBridgePass.BridgeGenerator.Settings;
        int groundLevelY = Main.maxTilesY - ForgottenShrineGenerationHelpers.GroundDepth;
        int waterLevelY = groundLevelY - ForgottenShrineGenerationHelpers.WaterDepth;
        int bridgeBeamBottomPoint = waterLevelY - bridgeSettings.BridgeBeamHeight;
        int bridgeLowYPoint = bridgeBeamBottomPoint - bridgeSettings.BridgeThickness;
        int left = 1;
        int right = BaseBridgePass.BridgeGenerator.Left;
        int curveDipWidth = ForgottenShrineGenerationHelpers.WaterCurveDipWidth;
        int protrusionWidth = curveDipWidth / 3;
        int maxTerrainBumpiness = 14;
        int maxWallHeight = 4;
        float terrainHorizontalVariance = 0.0056f;
        float wallHorizontalVariance = 0.013f;
        float heightMapSeed = WorldGen.genRand.NextFloat(10000f);
        ushort grassID = (ushort)ModContent.TileType<SacredGrass>();

        for (int x = left; x < right; x++)
        {
            int distanceFromEdge = Math.Abs(x - right);
            int bottom = Main.maxTilesY - 5;
            float edgeCurveInterpolant = LumUtils.InverseLerp(0f, curveDipWidth, distanceFromEdge);
            float easedCurveInterpolant = MathF.Sqrt(1.001f - edgeCurveInterpolant.Squared());

            int baseOffset = (int)MathF.Round(UnholySine(x * terrainHorizontalVariance + heightMapSeed) * (1f - easedCurveInterpolant) * -maxTerrainBumpiness);
            int hillyOffset = (int)MathF.Round(UnholySine(x * terrainHorizontalVariance * 0.3f + heightMapSeed * 1.1f) * (1f - easedCurveInterpolant) * -maxTerrainBumpiness * 2f);
            int bumpOffset = (int)MathF.Round(UnholySine(x * terrainHorizontalVariance * 5f + heightMapSeed * 1.1f) * (1f - easedCurveInterpolant) * -2f);
            int heightOffset = -baseOffset - hillyOffset - bumpOffset;
            int top = bridgeLowYPoint + heightOffset;

            for (int y = top; y < bottom; y++)
            {
                Tile t = Main.tile[x, y];
                t.HasTile = true;
                t.TileType = y == top ? grassID : TileID.Dirt;
            }

            int wallHeight = (int)MathF.Round(UnholySine(x * wallHorizontalVariance - heightMapSeed) * (1f - easedCurveInterpolant) * -maxWallHeight) + 1;
            for (int dy = -wallHeight; dy <= 2; dy++)
            {
                Tile t = Main.tile[x, top + dy];
                t.WallType = WallID.GrassUnsafe;
            }
        }

        for (int x = right; x < right + protrusionWidth; x++)
        {
            float protrusionCurveInterpolant = LumUtils.InverseLerp(0f, protrusionWidth, x - right);
            float easedProtrusionInterpolant = 1f - (1f - protrusionCurveInterpolant).Cubed();
            int bottom = groundLevelY;
            int top = (int)MathHelper.Lerp(waterLevelY, bottom, easedProtrusionInterpolant);
            for (int y = top; y < bottom; y++)
            {
                Tile t = Main.tile[x, y];
                t.HasTile = true;
                t.TileType = y == top ? grassID : TileID.Dirt;
            }
        }

        // Place a ton of lillies.
        int lilyCount = ForgottenShrineGenerationHelpers.WestIslandLilyCount;
        for (int i = 0; i < lilyCount; i++)
        {
            int lilyX = (int)(WorldGen.genRand.NextFloat(left, right) * 16f);
            int lilyY = (int)(LumUtils.FindGroundVertical(new Point((int)(lilyX / 16f), Main.maxTilesY - 10)).Y * 16f);
            Point tileAbove = new Point(lilyX / 16, lilyY / 16 - 1);
            if (Framing.GetTileSafely(tileAbove).LiquidAmount >= 20)
                continue;

            SpiderLilyRenderer.Register(new SpiderLilyData(new Point(lilyX, lilyY)));
        }
    }

    private static float UnholySine(float x)
    {
        float a = LumUtils.AperiodicSin(x);
        float b = LumUtils.AperiodicSin(x * 2f);
        float c = LumUtils.AperiodicSin(x * 3f);
        return MathF.Cbrt(a * b * c);
    }
}
