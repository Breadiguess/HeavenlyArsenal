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
        int bridgeThickness = ForgottenShrineGenerationConstants.BridgeThickness;
        for (int x = 0; x < Main.maxTilesX; x++)
        {
            float archHeightInterpolant = MathF.Abs(MathF.Sin(MathHelper.Pi * x / bridgeWidth));
            int archHeight = (int)MathF.Round(archHeightInterpolant * ForgottenShrineGenerationConstants.BridgeArchHeight);

            int extraThickness = (int)Utils.Remap(archHeightInterpolant, 0.6f, 0f, 0f, bridgeThickness * 1.25f);
            for (int dy = -extraThickness; dy < bridgeThickness; dy++)
            {
                int archY = bridgeLowYPoint - archHeight - dy;
                int tileID = TileID.GrayBrick;
                if (dy >= bridgeThickness - 2)
                    tileID = TileID.RedDynastyShingles;
                else if (dy >= bridgeThickness - 4)
                    tileID = TileID.DynastyWood;

                WorldGen.PlaceTile(x, archY, tileID);
            }

            int wallHeight = (int)MathF.Round(MathHelper.Lerp(8f, 2f, MathF.Pow(archHeightInterpolant, 1.7f)));
            for (int dy = -extraThickness - wallHeight; dy < bridgeThickness - 2; dy++)
            {
                int wallY = bridgeLowYPoint - archHeight - dy;
                WorldGen.PlaceWall(x, wallY, WallID.LivingWood);
                WorldGen.paintWall(x, wallY, PaintID.GrayPaint);
            }
        }

        for (int x = 0; x < Main.maxTilesX; x++)
        {
            if (x >= 5 && x < Main.maxTilesX - 5 && x % bridgeWidth == 0)
                PlaceBeam(groundLevelY, x, bridgeLowYPoint);
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

        // For a bit of artistic preference, 0.5 will be used instead of 1 like in the original equation, making the beams a bit thinner.
        float intermediateArcsine = MathF.Asin(0.5f / ForgottenShrineGenerationConstants.BridgeArchHeight);
        int beamWidth = (int)MathF.Round(intermediateArcsine * ForgottenShrineGenerationConstants.BridgeArchWidth / MathHelper.Pi);
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
}
