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
        int maxBumpiness = 6;
        float heightMapSeed = WorldGen.genRand.NextFloat(10000f);

        for (int x = left; x < right; x++)
        {
            int distanceFromEdge = Math.Abs(x - right);
            int bottom = Main.maxTilesY - 5;
            float edgeCurveInterpolant = LumUtils.InverseLerp(0f, 40f, distanceFromEdge);
            float easedCurveInterpolant = MathF.Sqrt(1.001f - edgeCurveInterpolant.Squared());
            bottom = (int)MathHelper.Lerp(bottom, waterLevelY, easedCurveInterpolant);

            int heightOffset = (int)MathF.Round(UnholySine(x / 51.4f + heightMapSeed) * (1f - easedCurveInterpolant) * -maxBumpiness);
            int top = bridgeLowYPoint + heightOffset;

            for (int y = top; y < bottom; y++)
            {
                Tile t = Main.tile[x, y];
                t.HasTile = true;
                t.TileType = y == top ? TileID.Grass : TileID.Dirt;
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
