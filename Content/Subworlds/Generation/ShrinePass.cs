using HeavenlyArsenal.Content.Subworlds.Generation.Bridges;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public class ShrinePass : GenPass
{
    public ShrinePass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating a sacred shrine.";

        BridgeGenerationSettings bridgeSettings = BaseBridgePass.BridgeGenerator.Settings;
        int left = BaseBridgePass.BridgeGenerator.Right + ForgottenShrineGenerationHelpers.LakeWidth + bridgeSettings.DockWidth;
        int right = left + ForgottenShrineGenerationHelpers.ShrineIslandWidth;

        int gateLeft = (left + right) / 2;
        ConstructToriiGate(gateLeft - 18, gateLeft + 18, 41);
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
        PlaceToriiGateColumn(new Point(center.X - 11, middleArchY), middleArchY - upperArchY, false);
        PlaceToriiGateColumn(new Point(center.X + 11, middleArchY), middleArchY - upperArchY, false);

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
}
