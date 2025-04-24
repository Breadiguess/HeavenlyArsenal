using HeavenlyArsenal.Content.Subworlds.Generation.Bridges;
using HeavenlyArsenal.Content.Tiles.ForgottenShrine;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
        int baseElevationAboveSeaLevel = 24;
        int maxBaseElevation = ForgottenShrineGenerationHelpers.WaterDepth + baseElevationAboveSeaLevel;
        int basinDepth = 12;
        ushort grassID = (ushort)ModContent.TileType<SacredGrass>();

        // Create the island.
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

        int lilyCount = ForgottenShrineGenerationHelpers.ShrineIslandLilyCount;
        PlaceLillies(lilyCount, left, right);

        int pillarCount = ForgottenShrineGenerationHelpers.ShrineIslandPillarCount;
        PlacePillars(pillarCount, left, right);

        AttachPillars();
    }

    private static void PlaceLillies(int lilyCount, int left, int right)
    {
        SpiderLilyManager spiderLilies = ModContent.GetInstance<SpiderLilyManager>();
        for (int i = 0; i < lilyCount; i++)
        {
            int lilyX = (int)(WorldGen.genRand.NextFloat(left, right) * 16f);
            int lilyY = (int)(LumUtils.FindGroundVertical(new Point((int)(lilyX / 16f), Main.maxTilesY - 10)).Y * 16f);
            Point tileAbove = new Point(lilyX / 16, lilyY / 16 - 1);
            if (Framing.GetTileSafely(tileAbove).LiquidAmount >= 20)
                continue;

            spiderLilies.Register(new SpiderLilyData(new Point(lilyX, lilyY)));
        }
    }

    private static void PlacePillars(int pillarCount, int left, int right)
    {
        ShrinePillarManager pillarsManager = ModContent.GetInstance<ShrinePillarManager>();
        for (int i = 0; i < pillarCount; i++)
        {
            int pillarX = (int)(WorldGen.genRand.NextFloat(left, right) * 16f);
            int pillarY = (int)(LumUtils.FindGroundVertical(new Point((int)(pillarX / 16f), Main.maxTilesY - 10)).Y * 16f) + 24;

            Point pillarSpawnPosition = new Point(pillarX, pillarY);
            float pillarRotation = WorldGen.genRand.NextFloatDirection() * 0.23f;
            float pillarHeight = WorldGen.genRand.NextFloat(210f, 500f);
            pillarsManager.Register(new ShrinePillarData(pillarSpawnPosition, pillarRotation, pillarHeight));
        }
    }

    private static void AttachPillars()
    {
        ShrinePillarManager pillarsManager = ModContent.GetInstance<ShrinePillarManager>();
        ShrinePillarRopeManager ropesManager = ModContent.GetInstance<ShrinePillarRopeManager>();
        List<ShrinePillarData> pillarsByXPosition = pillarsManager.TileObjects.OrderBy(o => o.Position.X).ToList();

        for (int i = 1; i < pillarsByXPosition.Count; i++)
        {
            ShrinePillarData previousPillar = pillarsByXPosition[i - 1];
            ShrinePillarData currentPillar = pillarsByXPosition[i];
            float horizontalDistanceBetweenPillars = MathHelper.Distance(previousPillar.Position.X, currentPillar.Position.X);
            bool tooClose = horizontalDistanceBetweenPillars <= ForgottenShrineGenerationHelpers.MaxPillarAttachmentDistance * 0.33f;
            bool tooFar = horizontalDistanceBetweenPillars >= ForgottenShrineGenerationHelpers.MaxPillarAttachmentDistance;

            if (!tooClose && !tooFar)
            {
                if (!previousPillar.HasRopeAnchor)
                    previousPillar.RopeAnchorYInterpolant = WorldGen.genRand.NextFloat(0.55f, 0.8f);
                if (!currentPillar.HasRopeAnchor)
                    currentPillar.RopeAnchorYInterpolant = WorldGen.genRand.NextFloat(0.55f, 0.8f);

                Point start = previousPillar.RopeAnchorPosition.Value.ToPoint();
                Point end = currentPillar.RopeAnchorPosition.Value.ToPoint();
                float distanceBetweenPillars = previousPillar.RopeAnchorPosition.Value.Distance(currentPillar.RopeAnchorPosition.Value);
                float sagFactor = WorldGen.genRand.NextFloat(0.1f, 0.16f);
                ropesManager.Register(new ShrinePillarRopeData(start, end, distanceBetweenPillars * sagFactor));
            }
        }
    }
}
