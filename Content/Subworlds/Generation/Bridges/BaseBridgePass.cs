using Terraria.IO;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation.Bridges;

public class BaseBridgePass : GenPass
{
    private static BridgeSetGenerator bridgeGenerator;

    /// <summary>
    /// The width of each bridge in the set.
    /// </summary>
    public const int BridgeWidth = 84;

    /// <summary>
    /// The manager used by the bridge generation algorithm.
    /// </summary>
    public static BridgeSetGenerator BridgeGenerator
    {
        get => bridgeGenerator ??= CreateBridgeGenerator();
        private set => bridgeGenerator = value;
    }

    /// <summary>
    /// The settings for the bridge generator.
    /// </summary>
    public static readonly BridgeGenerationSettings GenerationSettings = new BridgeGenerationSettings()
    {
        BridgeBeamHeight = 9,
        BridgeArchWidth = BridgeWidth,
        BridgeUndersideRopeWidth = (int)(BridgeWidth * 0.6f),
        BridgeUndersideRopeSag = 6,
        BridgeArchHeight = 3,
        BridgeArchHeightBigBridgeFactor = 2f,
        BridgeThickness = 5,
        BridgeRooftopsPerBridge = 3,
        BridgeRooftopDynastyWoodLayerHeight = 2,
        BridgeRoofWallUndersideHeight = 4,
        BridgeBackWallHeight = 19,

        BridgeRooftopConfigurations =
        [
            // Standard.
            new ShrineRooftopSet().
                Add(new ShrineRooftopInfo(64, 36, 0)).
                Add(new ShrineRooftopInfo(40, 40, 10)).
                Add(new ShrineRooftopInfo(28, 27, 15)),
            
            // Pointy.
            new ShrineRooftopSet().
                Add(new ShrineRooftopInfo(64, 38, 0)).
                Add(new ShrineRooftopInfo(27, 93, 15)),

            // Flat.
            new ShrineRooftopSet().
                Add(new ShrineRooftopInfo(129, 28, 0)).
                Add(new ShrineRooftopInfo(60, 56, 11)),
        ]
    };

    public BaseBridgePass() : base("Terrain", 1f) { }

    private static BridgeSetGenerator CreateBridgeGenerator()
    {
        int left = 400;
        int right = left + BridgeWidth * 12;
        return new BridgeSetGenerator(left, right, GenerationSettings);
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating the bridge.";

        BridgeGenerator.Generate();
    }
}
