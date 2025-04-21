using Terraria.IO;
using Terraria.WorldBuilding;

namespace HeavenlyArsenal.Content.Subworlds.Generation.Bridges;

public class BaseBridgePass : GenPass
{
    /// <summary>
    /// The settings used by the bridge generation algorithm.
    /// </summary>
    public static readonly BridgeSetGenerator BridgeGenerator = new(400,
                                                                    1408, // 12x BridgeArchWidth on top of the original offset of 400.
                                                                    new BridgeGenerationSettings()
                                                                    {
                                                                        BridgeBeamHeight = 9,
                                                                        BridgeArchWidth = 84,
                                                                        BridgeUndersideRopeWidth = 50,
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
                                                                    });

    public BaseBridgePass() : base("Terrain", 1f) { }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating the bridge.";

        BridgeGenerator.Generate();
    }
}
