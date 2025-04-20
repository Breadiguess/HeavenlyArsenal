using System.Collections.Generic;
using Terraria;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public static class ForgottenShrineGenerationConstants
{
    /// <summary>
    /// Represents data for a rooftop generation set for a bridge.
    /// </summary>
    /// <param name="Width">The width of the rooftop.</param>
    /// <param name="Height">The height of the rooftop.</param>
    /// <param name="VerticalOffset">The vertical placement offset of this rooftop relative to the base roof level.</param>
    public record struct ShrineRooftopInfo(int Width, int Height, int VerticalOffset);

    /// <summary>
    /// Represents a set of rooftops atop each other.
    /// </summary>
    /// <param name="Rooftops"></param>
    public record ShrineRooftopSet(List<ShrineRooftopInfo> Rooftops)
    {
        public ShrineRooftopSet() : this([]) { }

        /// <summary>
        /// Adds a new rooftop to this set.
        /// </summary>
        public ShrineRooftopSet Add(ShrineRooftopInfo rooftop)
        {
            Rooftops.Add(rooftop);
            return this;
        }
    }

    /// <summary>
    /// The depth of ground beneath the shrine's shallow water.
    /// </summary>
    public static int GroundDepth => (int)(Main.maxTilesY * 0.35f);

    /// <summary>
    /// The depth of water beneath the shrine.
    /// </summary>
    public static int WaterDepth => 33;

    /// <summary>
    /// The height of beams for the bridge before the shrine.
    /// </summary>
    public static int BridgeBeamHeight => 11;

    /// <summary>
    /// The width of arches on the bridge.
    /// </summary>
    public static int BridgeArchWidth => 84;

    /// <summary>
    /// The amount of horizontal coverage that ropes underneath the bridge have, in tile coordinates.
    /// </summary>
    public static int BridgeUndersideRopeWidth => (int)(BridgeArchWidth * 0.6f);

    /// <summary>
    /// The amount of vertical coverage that ropes beneath the bridge should have due to sag, in tile coordinates.
    /// </summary>
    public static int BridgeUndersideRopeSag => (int)(BridgeBeamHeight * 0.7f);

    /// <summary>
    /// The maximum height of arches on the bridge.
    /// </summary>
    public static int BridgeArchHeight => 4;

    /// <summary>
    /// The vertical thickness of the bridge.
    /// </summary>
    public static int BridgeThickness => 5;

    /// <summary>
    /// The amount of tree structures to generate underwater.
    /// </summary>
    public static int UnderwaterTreeCount => Main.maxTilesX / 60;

    /// <summary>
    /// The amount of cattails to generate underwater.
    /// </summary>
    public static int CattailCount => Main.maxTilesX / 26;

    /// <summary>
    /// The maximum height of cattails above the water line.
    /// </summary>
    public static int MaxCattailHeight => 7;

    /// <summary>
    /// The amount of lilypads to generate atop the water.
    /// </summary>
    public static int LilypadCount => 0;

    /// <summary>
    /// The set of possible rooftops that can be selected for a given roof on the bridge.
    /// </summary>
    public static List<ShrineRooftopSet> BridgeRooftopConfigurations =>
    [
        // Standard.
        new ShrineRooftopSet().
            Add(new ShrineRooftopInfo((int)(BridgeArchWidth / 1.3f), (int)(BridgeArchWidth / 2.3f), 0)).
            Add(new ShrineRooftopInfo((int)(BridgeArchWidth / 2.1f), (int)(BridgeArchWidth / 2.05f), (int)(BridgeArchWidth / 8.1f))).
            Add(new ShrineRooftopInfo((int)(BridgeArchWidth / 2.9f), (int)(BridgeArchWidth / 3.07f), (int)(BridgeArchWidth / 5.3f))),

        // Pointy.
        new ShrineRooftopSet().
            Add(new ShrineRooftopInfo((int)(BridgeArchWidth / 1.3f), (int)(BridgeArchWidth / 2.2f), 0)).
            Add(new ShrineRooftopInfo((int)(BridgeArchWidth / 3.1f), (int)(BridgeArchWidth / 0.9f), (int)(BridgeArchWidth / 5.6f))),

        // Flat.
        new ShrineRooftopSet().
            Add(new ShrineRooftopInfo((int)(BridgeArchWidth / 0.65f), (int)(BridgeArchWidth / 2.9f), 0)).
            Add(new ShrineRooftopInfo((int)(BridgeArchWidth / 1.4f), (int)(BridgeArchWidth / 1.5f), (int)(BridgeArchWidth / 7.2f))),
    ];
}
