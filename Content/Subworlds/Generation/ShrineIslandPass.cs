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

public class ShrineIslandPass : GenPass
{
    public ShrineIslandPass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating an island for a shrine.";

        BridgeGenerationSettings bridgeSettings = BaseBridgePass.BridgeGenerator.Settings;
        int left = BaseBridgePass.BridgeGenerator.Right + ForgottenShrineGenerationHelpers.LakeWidth + bridgeSettings.DockWidth;
        int right = left + ForgottenShrineGenerationHelpers.ShrineIslandWidth;
        int bottom = Main.maxTilesY - ForgottenShrineGenerationHelpers.GroundDepth;
        int baseElevationAboveSeaLevel = 13;
        int maxBaseElevation = ForgottenShrineGenerationHelpers.WaterDepth + baseElevationAboveSeaLevel;
        int basinDepth = 12;
        ushort grassID = (ushort)ModContent.TileType<SacredGrass>();

        for (int x = left; x <= right; x++)
        {
            float xInterpolant = LumUtils.InverseLerp(left, right, x);
            float heightInterpolant = LumUtils.InverseLerpBump(0f, 0.1f, 0.9f, 1f, xInterpolant);
            float easedHeightInterpolant = MathF.Pow(heightInterpolant, 0.66f);
            int top = (int)(bottom - maxBaseElevation * easedHeightInterpolant + LumUtils.Convert01To010(xInterpolant) * basinDepth * 0.999f);
            int stoneDepth = (int)MathHelper.Lerp(3f, 5f, LumUtils.AperiodicSin(xInterpolant * 23f).Squared());

            for (int y = top; y <= bottom; y++)
            {
                Tile t = Main.tile[x, y];
                t.HasTile = true;
                t.TileType = TileID.Dirt;
                if (y == top)
                    t.TileType = grassID;
                if (y >= top + stoneDepth)
                    t.TileType = TileID.Stone;
            }
        }
    }
}
