using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.IO;
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
        for (int x = 0; x < Main.maxTilesX; x++)
        {
            float archHeightInterpolant = MathF.Abs(MathF.Sin(MathHelper.Pi * x / bridgeWidth));
            int archHeight = (int)MathF.Round(archHeightInterpolant * ForgottenShrineGenerationConstants.BridgeArchHeight);

            for (int dy = 0; dy < ForgottenShrineGenerationConstants.BridgeThickness; dy++)
            {
                int archY = bridgeLowYPoint - archHeight - dy;
                WorldGen.PlaceTile(x, archY, TileID.DynastyWood);
            }

            if (x >= 5 && x < Main.maxTilesX - 5 && x % bridgeWidth == 0)
                PlaceBeam(groundLevelY, x, bridgeLowYPoint - archHeight);
        }
    }

    private static void PlaceBeam(int groundLevelY, int startingX, int startingY)
    {
        // round(height * abs(sin(pi * x / width))) < 1
        // height * abs(sin(pi * x / width)) < 1
        // abs(sin(pi * x / width)) < 1 / height
        // sin(pi * x / width) < 1 / height
        // sin(pi * x / width) = 1 / height
        // pi * x / width = arcsin(1 / height)
        // x = arcsin(1 / height) * width / pi
        float intermediateArcsine = MathF.Asin(1f / ForgottenShrineGenerationConstants.BridgeArchHeight);
        int beamWidth = (int)MathF.Round(intermediateArcsine * ForgottenShrineGenerationConstants.BridgeArchWidth / MathHelper.Pi);
        for (int dx = -beamWidth; dx <= beamWidth; dx++)
        {
            int x = startingX + dx;
            for (int y = startingY; y < groundLevelY; y++)
                WorldGen.PlaceTile(x, y, TileID.WoodenBeam);
        }
    }
}
