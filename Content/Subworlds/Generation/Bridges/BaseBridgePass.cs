using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using Microsoft.Xna.Framework;
using Terraria;
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
        DockWidth = 75,

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
                Add(new ShrineRooftopInfo(27, 79, 15)),

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
        int right = left + BridgeWidth * 16;
        return new BridgeSetGenerator(left, right, GenerationSettings);
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Creating the bridge.";

        BridgeGenerator.Generate();
    }

    /// <summary>
    /// Places candles along the bridge.
    /// </summary>
    internal static void CreateCandles()
    {
        BridgeGenerationSettings settings = BridgeGenerator.Settings;
        int groundLevelY = Main.maxTilesY - ForgottenShrineGenerationHelpers.GroundDepth;
        int waterLevelY = groundLevelY - ForgottenShrineGenerationHelpers.WaterDepth;
        int bridgeLowYPoint = waterLevelY - settings.BridgeBeamHeight - settings.BridgeThickness;
        for (int tileX = BridgeGenerator.Left; tileX < BridgeGenerator.Right; tileX++)
        {
            if (BridgeGenerator.InNonRooftopBridgeRange(tileX) &&
                BridgeGenerator.CalculateXWrappedBySingleBridge(tileX) == settings.BridgeArchWidth / 2)
            {
                float worldX = tileX * 16f + 8f;
                float verticalOffset = BridgeGenerator.CalculateArchHeight(tileX) * -16f - 30f;
                Vector2 candleSpawnPosition = new Vector2(worldX, bridgeLowYPoint * 16f + verticalOffset);

                SpiritCandleParticle candle = SpiritCandleParticle.Pool.RequestParticle();
                candle.Behavior = SpiritCandleParticle.AIType.Bounce;
                candle.Prepare(candleSpawnPosition, Vector2.Zero, 0f, Color.White, Vector2.One);

                ParticleEngine.Particles.Add(candle);
            }
        }
    }
}
