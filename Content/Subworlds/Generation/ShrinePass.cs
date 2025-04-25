using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Content.Subworlds.Generation.Bridges;
using HeavenlyArsenal.Content.Tiles.ForgottenShrine;
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

public class ShrinePass : GenPass
{
    public ShrinePass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating a sacred shrine.";

        BridgeGenerationSettings bridgeSettings = BaseBridgePass.BridgeGenerator.Settings;
        int pillarDistance = ForgottenShrineGenerationHelpers.ShrineIslandGatePillarDistance;
        int brickGroundDistance = (int)(pillarDistance * 1.5f);
        int pillarHeight = (int)(pillarDistance * 2.3f);
        int left = BaseBridgePass.BridgeGenerator.Right + ForgottenShrineGenerationHelpers.LakeWidth + bridgeSettings.DockWidth;
        int right = left + ForgottenShrineGenerationHelpers.ShrineIslandWidth;
        int gateCenterX = (left + right) / 2;
        TransformGroundIntoBricks(gateCenterX - brickGroundDistance, gateCenterX + brickGroundDistance);
        ConstructToriiGate(gateCenterX - pillarDistance, gateCenterX + pillarDistance, pillarHeight);
    }

    private static void TransformGroundIntoBricks(int left, int right)
    {
        for (int x = left; x <= right; x++)
        {
            float xInterpolant = LumUtils.InverseLerp(left, right, x);
            int y = LumUtils.FindGroundVertical(new Point(x, 10)).Y + 1;
            int depth = (int)MathHelper.Lerp(2f, 4.5f, LumUtils.Convert01To010(xInterpolant));
            for (int dy = 0; dy < depth; dy++)
            {
                Tile t = Framing.GetTileSafely(x, y + dy);
                t.HasTile = true;
                t.TileType = TileID.RedStucco;
                t.TileColor = PaintID.BlackPaint;
            }
        }
    }

    private static void ConstructToriiGate(int leftX, int rightX, int gateHeight)
    {
        int leftY = LumUtils.FindGroundVertical(new Point(leftX, 10)).Y + 1;
        int rightY = LumUtils.FindGroundVertical(new Point(rightX, 10)).Y + 1;
        Point center = new Point((leftX + rightX) / 2, (leftY + rightY) / 2);

        PlaceToriiGateColumn(new Point(leftX, leftY), gateHeight, true);
        PlaceToriiGateColumn(new Point(rightX, rightY), gateHeight, true);

        int upperArchY = center.Y - gateHeight;
        int middleArchY = upperArchY + 12;
        int upperArchSpread = 15;
        int middleArchSpread = upperArchSpread / 3;
        PlaceToriiGateColumn(new Point(center.X - 11, middleArchY - 1), middleArchY - upperArchY - 2, false);
        PlaceToriiGateColumn(new Point(center.X + 11, middleArchY - 1), middleArchY - upperArchY - 2, false);

        Point shimenawaLeftPosition = new Point((center.X - 13) * 16, middleArchY * 16 + 8);
        Point shimenawaRightPosition = new Point((center.X + 13) * 16, middleArchY * 16 + 8);
        ModContent.GetInstance<ShimenawaRopeManager>().Register(new ShimenawaRopeData(shimenawaLeftPosition, shimenawaRightPosition, 32f));

        Point leftLanternPosition = new Point(shimenawaLeftPosition.X - 8, shimenawaLeftPosition.Y + 8);
        Point rightLanternPosition = new Point(shimenawaRightPosition.X + 8, shimenawaRightPosition.Y + 8);
        ModContent.GetInstance<HangingLanternRopeManager>().Register(new HangingLanternRopeData(leftLanternPosition, gateHeight * 6f)
        {
            Direction = 1
        });
        ModContent.GetInstance<HangingLanternRopeManager>().Register(new HangingLanternRopeData(rightLanternPosition, gateHeight * 6f)
        {
            Direction = -1
        });

        PlaceToriiGateMiddleArch(leftX - middleArchSpread, rightX + middleArchSpread, middleArchY);
        PlaceToriiGateUpperArch(leftX - upperArchSpread, rightX + upperArchSpread, upperArchY);
    }

    private static void PlaceToriiGateColumn(Point point, int gateHeight, bool includePedestal)
    {
        for (int dy = includePedestal ? 3 : 0; dy < gateHeight; dy++)
        {
            Tile gate = Main.tile[point.X, point.Y - dy];

            gate.HasTile = true;
            gate.TileType = TileID.BorealBeam;
            gate.WallType = WallID.PalladiumColumn;
            gate.TileColor = PaintID.DeepRedPaint;
            gate.WallColor = PaintID.RedPaint;
        }
        if (includePedestal)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = 1; dy < 3; dy++)
                {
                    Tile pedestal = Main.tile[point.X + dx, point.Y - dy];
                    pedestal.HasTile = true;
                    pedestal.TileType = TileID.StoneSlab;
                    pedestal.TileColor = PaintID.BlackPaint;
                }
            }
        }
    }

    private static void PlaceToriiGateMiddleArch(int left, int right, int y)
    {
        for (int x = left; x <= right; x++)
        {
            for (int dy = -1; dy < 1; dy++)
            {
                Tile arch = Main.tile[x, y + dy];
                arch.HasTile = true;
                arch.TileType = TileID.DynastyWood;
                arch.TileColor = PaintID.RedPaint;
            }
        }
    }

    private static void PlaceToriiGateUpperArch(int left, int right, int y)
    {
        for (int x = left; x <= right; x++)
        {
            float xInterpolant = LumUtils.InverseLerp(left, right - 1f, x);
            int archVerticalOffset = (int)MathF.Round(LumUtils.Convert01To010(xInterpolant) * 4f);

            for (int dy = -2; dy < 2; dy++)
            {
                Tile arch = Main.tile[x, y + archVerticalOffset + dy];
                arch.HasTile = true;
                arch.TileType = TileID.RedDynastyShingles;
                arch.WallType = WallID.None;
                arch.TileColor = PaintID.BlackPaint;

                if (dy <= 0)
                    SmoothenPass.PointsToNotSmoothen.Add(new Point(x, y + archVerticalOffset + dy));
            }
        }
    }

    /// <summary>
    /// Places candles near the shrine.
    /// </summary>
    internal static void CreateCandles()
    {
        BridgeGenerationSettings bridgeSettings = BaseBridgePass.BridgeGenerator.Settings;
        int left = BaseBridgePass.BridgeGenerator.Right + ForgottenShrineGenerationHelpers.LakeWidth + bridgeSettings.DockWidth;
        int right = left + ForgottenShrineGenerationHelpers.ShrineIslandWidth;
        int gateCenterX = (left + right) / 2;
        int gateCenterY = Main.maxTilesY - ForgottenShrineGenerationHelpers.GroundDepth - ForgottenShrineGenerationHelpers.WaterDepth - 1;
        Vector2 gateCenterWorldCoords = new Vector2(gateCenterX, gateCenterY).ToWorldCoordinates();

        int tries = 0;
        int candleCount = ForgottenShrineGenerationHelpers.ShrineIslandCandleCount;
        List<Vector2> placedCandlePositions = new List<Vector2>(candleCount);
        for (int i = 0; i < candleCount; i++)
        {
            tries++;

            float horizontalCoverage = tries * 5f + 750f;
            Vector2 candleSpawnPosition = gateCenterWorldCoords - Vector2.UnitY * 140f + WorldGen.genRand.NextVector2Circular(horizontalCoverage, 125f);

            // Make sure candles do not spawn inside of tiles or water by shoving the spawn position out of such things if necessary.
            while (Collision.SolidCollision(candleSpawnPosition, 16, 12) || Collision.WetCollision(candleSpawnPosition, 16, 32))
                candleSpawnPosition.Y -= 16f;

            float candleSocialDistancing = 36f;

            // Keep the shrine free of candles.
            if (MathHelper.Distance(candleSpawnPosition.X, gateCenterWorldCoords.X) <= ForgottenShrineGenerationHelpers.ShrineIslandGatePillarDistance * 16f + 50f)
            {
                i--;
                continue;
            }

            // Partially ground the candles.
            float candleGroundY = LumUtils.FindGroundVertical(candleSpawnPosition.ToTileCoordinates()).Y * 16f;
            candleSpawnPosition.Y = MathHelper.Lerp(candleSpawnPosition.Y, candleGroundY, 0.33f);

            // Try again if the candle position is too close to a candle that was already placed.
            if (placedCandlePositions.Any(p => p.WithinRange(candleSpawnPosition, candleSocialDistancing)))
            {
                i--;
                continue;
            }

            float backgroundInterpolant = WorldGen.genRand.NextFloat();
            Color color = Color.Lerp(Color.White, Color.Gray, backgroundInterpolant);
            SpiritCandleParticle candle = SpiritCandleParticle.Pool.RequestParticle();
            candle.Behavior = SpiritCandleParticle.AIType.DanceInAir;
            candle.Prepare(candleSpawnPosition, Vector2.Zero, 0f, color, Vector2.One * MathHelper.Lerp(1f, 0.65f, backgroundInterpolant));
            ParticleEngine.Particles.Add(candle);

            placedCandlePositions.Add(candleSpawnPosition);
        }
    }
}
