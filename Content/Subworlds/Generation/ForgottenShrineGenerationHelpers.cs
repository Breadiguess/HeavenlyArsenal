using Microsoft.Xna.Framework;
using Terraria;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public static class ForgottenShrineGenerationHelpers
{
    /// <summary>
    /// The depth of ground beneath the shrine's shallow water.
    /// </summary>
    public static int GroundDepth => (int)(Main.maxTilesY * 0.35f);

    /// <summary>
    /// The depth of water beneath the shrine.
    /// </summary>
    public static int WaterDepth => 33;

    /// <summary>
    /// The amount of tiles dedicated to make water caves curve downward.
    /// </summary>
    public static int WaterCurveDipWidth => 51;

    /// <summary>
    /// The amount of tree structures to generate underwater.
    /// </summary>
    public static int UnderwaterTreeCount => Main.maxTilesX / 43;

    /// <summary>
    /// The amount of cattails to generate underwater.
    /// </summary>
    public static int CattailCount => Main.maxTilesX / 9;

    /// <summary>
    /// The maximum height of cattails above the water line.
    /// </summary>
    public static int MaxCattailHeight => 6;

    /// <summary>
    /// The amount of lilypads to generate atop the water.
    /// </summary>
    public static int LilypadCount => Main.maxTilesX / 17;

    /// <summary>
    /// The width of the lake before the shrine.
    /// </summary>
    public static int LakeWidth => (int)(Main.maxTilesX * 0.1f);

    /// <summary>
    /// The width of the shrine island.
    /// </summary>
    public static int ShrineIslandWidth => (int)(Main.maxTilesX * 0.25f);

    /// <summary>
    /// The amount of lilies on the west island.
    /// </summary>
    public static int WestIslandLilyCount => 600;

    /// <summary>
    /// The amount of lilies on the shrine island.
    /// </summary>
    public static int ShrineIslandLilyCount => 1600;

    /// <summary>
    /// The amount of pillars on the shrine island.
    /// </summary>
    public static int ShrineIslandPillarCount => ShrineIslandWidth / 44;

    /// <summary>
    /// The distance pillars of the gate surrounding the island shrine have from the horizontal center point.
    /// </summary>
    public static int ShrineIslandGatePillarDistance => 18;

    /// <summary>
    /// The amount of candles to generate on the shrine island, near the shrine itself.
    /// </summary>
    public static int ShrineIslandCandleCount => 71;

    /// <summary>
    /// The minimum horizontal distance pillars can be from each other before it becomes impossible for them to have attached ropes.
    /// </summary>
    public static float MinPillarAttachmentDistance => 111f;

    /// <summary>
    /// The maximum horizontal distance pillars can be from each other before it becomes impossible for them to have attached ropes.
    /// </summary>
    public static float MaxPillarAttachmentDistance => 332f;

    /// <summary>
    /// The width of the subworld.
    /// </summary>
    public static int SubworldWidth => 3372;

    /// <summary>
    /// The height of the subworld.
    /// </summary>
    public static int SubworldHeight => 554;

    /// <summary>
    /// Calculates the point at which a starting point reaches open air upon moving downward.
    /// </summary>
    internal static Point DescendToAir(Point p)
    {
        while (p.Y < Main.maxTilesY && Main.tile[p.X, p.Y].HasTile)
            p.Y++;

        return p;
    }
}
