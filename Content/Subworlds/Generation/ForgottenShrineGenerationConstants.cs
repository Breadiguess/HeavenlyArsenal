using Terraria;

namespace HeavenlyArsenal.Content.Subworlds.Generation;

public static class ForgottenShrineGenerationConstants
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
    /// The height of beams for the bridge before the shrine.
    /// </summary>
    public static int BridgeBeamHeight => 11;

    /// <summary>
    /// The width of arches on the bridge.
    /// </summary>
    public static int BridgeArchWidth => 76;

    /// <summary>
    /// The maximum height of arches on the bridge.
    /// </summary>
    public static int BridgeArchHeight => 3;

    /// <summary>
    /// The vertical thickness of the bridge.
    /// </summary>
    public static int BridgeThickness => 6;

    /// <summary>
    /// The amount of tree structures to generate underwater.
    /// </summary>
    public static int UnderwaterTreeCount => Main.maxTilesX / 67;

    /// <summary>
    /// The amount of cattails to generate underwater.
    /// </summary>
    public static int CattailCount => Main.maxTilesX / 26;

    /// <summary>
    /// The maximum height of cattails above the water line.
    /// </summary>
    public static int MaxCattailHeight => 6;
}
