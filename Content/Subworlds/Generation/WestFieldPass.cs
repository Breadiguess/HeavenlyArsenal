using HeavenlyArsenal.Content.Subworlds.Generation.Bridges;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.IO;
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
        int coverage = right - left;
        int curveDipWidth = ForgottenShrineGenerationHelpers.WaterCurveDipWidth;
        int maxTerrainBumpiness = 25;
        int maxWallHeight = 3;
        float terrainHorizontalVariance = 0.009f;
        float wallHorizontalVariance = 0.013f;
        float heightMapSeed = WorldGen.genRand.NextFloat(10000f);

        for (int x = left; x < right; x++)
        {
            int distanceFromEdge = Math.Abs(x - right);
            int bottom = Main.maxTilesY - 5;
            float edgeCurveInterpolant = LumUtils.InverseLerp(0f, curveDipWidth, distanceFromEdge);
            float easedCurveInterpolant = MathF.Sqrt(1.001f - edgeCurveInterpolant.Squared());
            bottom = (int)MathHelper.Lerp(bottom, waterLevelY, easedCurveInterpolant);

            int hillyOffset = (int)MathF.Round(UnholySine(x * terrainHorizontalVariance * 0.3f + heightMapSeed * 1.1f) * (1f - easedCurveInterpolant) * -maxTerrainBumpiness * 2);
            int heightOffset = (int)MathF.Round(UnholySine(x * terrainHorizontalVariance + heightMapSeed) * (1f - easedCurveInterpolant) * -maxTerrainBumpiness - hillyOffset);
            int top = bridgeLowYPoint + heightOffset;

            for (int y = top; y < bottom; y++)
            {
                Tile t = Main.tile[x, y];
                t.HasTile = true;
                t.TileType = y == top ? TileID.Grass : TileID.Dirt;
            }

            int wallHeight = (int)MathF.Round(UnholySine(x * wallHorizontalVariance - heightMapSeed) * (1f - easedCurveInterpolant) * -maxWallHeight);
            for (int dy = -wallHeight; dy <= 2; dy++)
            {
                Tile t = Main.tile[x, top + dy];
                t.WallType = WallID.GrassUnsafe;
            }
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
