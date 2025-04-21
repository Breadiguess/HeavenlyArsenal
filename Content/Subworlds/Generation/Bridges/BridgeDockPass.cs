using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation.Bridges;

public class BridgeDockPass : GenPass
{
    public BridgeDockPass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating the bridge's dock.";

        BridgeGenerationSettings bridgeSettings = BaseBridgePass.BridgeGenerator.Settings;

        int dockWidth = 75;
        int left = BaseBridgePass.BridgeGenerator.Right + 1;
        int right = left + dockWidth;
        int baseDockDepth = bridgeSettings.BridgeBeamHeight + 1;
        int groundLevelY = Main.maxTilesY - ForgottenShrineGenerationHelpers.GroundDepth;
        int waterLevelY = groundLevelY - ForgottenShrineGenerationHelpers.WaterDepth;
        int bridgeLowYPoint = waterLevelY - bridgeSettings.BridgeBeamHeight;
        int supportBeamPlaceRate = 17;
        int lampPostPlaceRate = supportBeamPlaceRate;

        for (int x = left; x <= right; x++)
        {
            float xInterpolant = LumUtils.InverseLerp(left, right, x);
            float depthFactor = MathHelper.Lerp(1f, 0.25f, xInterpolant);
            int depth = (int)MathF.Ceiling(baseDockDepth * depthFactor);
            int bridgeTopY = bridgeLowYPoint - bridgeSettings.BridgeThickness + (int)MathF.Ceiling(LumUtils.Convert01To010(xInterpolant) * 2f) + 1;

            // Create the base of the dock.
            for (int dy = 0; dy < depth; dy++)
            {
                int y = bridgeTopY + dy;
                Tile t = Main.tile[x, y];
                t.HasTile = true;

                ushort tileID = (ushort)BridgeSetGenerator.DetermineBaseTileIDByHeight(depth - dy - 1, depth);
                if (tileID == TileID.RedDynastyShingles)
                    tileID = TileID.DynastyWood;

                t.TileType = tileID;
            }

            // Create support beams underneath the dock sometimes.
            if ((x - left) % supportBeamPlaceRate == supportBeamPlaceRate - 1)
            {
                int beamStartY = bridgeTopY + depth;
                for (int y = beamStartY; y < groundLevelY; y++)
                {
                    Tile t = Main.tile[x, y];
                    t.HasTile = true;
                    t.TileType = TileID.WoodenBeam;

                    if (y == waterLevelY - 3)
                    {
                        WorldGen.PlaceTile(x - 1, y, TileID.Torches);
                        WorldGen.PlaceTile(x + 1, y, TileID.Torches);
                    }
                }
            }

            // Create lamp posts on the dock.
            if ((x - left) % lampPostPlaceRate == lampPostPlaceRate / 2)
                WorldGen.PlaceTile(x, bridgeTopY - 1, TileID.Lampposts);

            // Create fences on the dock.
            if (x <= right - 2)
            {
                int fenceHeight = (x - left) % 4 == 0 ? 2 : 1;
                for (int dy = 1; dy <= fenceHeight; dy++)
                {
                    WorldGen.PlaceWall(x, bridgeTopY - dy, WallID.WoodenFence);
                    WorldGen.paintWall(x, bridgeTopY - dy, PaintID.WhitePaint);
                }
            }
        }
    }
}
