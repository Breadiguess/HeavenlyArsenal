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
    public static int LilypadCount => 0;

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
