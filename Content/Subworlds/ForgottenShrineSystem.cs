using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Content.Subworlds.Generation;
using HeavenlyArsenal.Content.Waters;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Graphics.LightingMask;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using SubworldLibrary;
using System;
using System.Reflection;
using Terraria;
using Terraria.GameContent.Shaders;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Subworlds;

public class ForgottenShrineSystem : ModSystem
{
    /// <summary>
    /// Whether the current client was in the shrine subworld last frame.
    /// </summary>
    public static bool WasInSubworldLastFrame
    {
        get;
        private set;
    }

    /// <summary>
    /// An event that's invoked when the shrine subworld is entered.
    /// </summary>
    public static event Action OnEnter;

    public override void OnModLoad()
    {
        On_Main.CalculateWaterStyle += ForceShrineWater;
        On_WaterShaderData.Apply += DisableIdleLiquidDistortion;
        OnEnter += CreateCandles;
    }

    // Not doing this results in beach water somehow having priority over shrine water in the outer parts of the subworld.
    private static int ForceShrineWater(On_Main.orig_CalculateWaterStyle orig, bool ignoreFountains)
    {
        if (SubworldSystem.IsActive<ForgottenShrineSubworld>())
            return ModContent.GetInstance<ForgottenShrineWater>().Slot;

        return orig(ignoreFountains);
    }

    private static void CreateCandles()
    {
        float spacingPerBridge = ForgottenShrineGenerationConstants.BridgeArchWidth * 16f;
        int candleCount = Main.maxTilesX / ForgottenShrineGenerationConstants.BridgeArchWidth;
        int groundLevelY = Main.maxTilesY - ForgottenShrineGenerationConstants.GroundDepth;
        int waterLevelY = groundLevelY - ForgottenShrineGenerationConstants.WaterDepth;
        int bridgeLowYPoint = waterLevelY - ForgottenShrineGenerationConstants.BridgeBeamHeight;
        float x = spacingPerBridge * 0.5f;
        for (int i = 0; i < candleCount; i++)
        {
            Vector2 candleSpawnPosition = new Vector2(x, bridgeLowYPoint * 16f - 425f);

            SpiritCandleParticle candle = SpiritCandleParticle.Pool.RequestParticle();
            candle.Prepare(candleSpawnPosition, Vector2.Zero, 0f, Color.White, Vector2.One * 0.5f);

            ParticleEngine.Particles.Add(candle);

            x += spacingPerBridge;
        }
    }

    private void DisableIdleLiquidDistortion(On_WaterShaderData.orig_Apply orig, WaterShaderData self)
    {
        orig(self);

        if (SubworldSystem.IsActive<ForgottenShrineSubworld>())
        {
            Vector2 screenSize = Main.ScreenSize.ToVector2();
            RenderTarget2D distortionTarget = (RenderTarget2D)typeof(WaterShaderData).GetField("_distortionTarget", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self);
            ManagedShader waterShader = ShaderManager.GetShader("HeavenlyArsenal.ShrineWaterShader");
            waterShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
            waterShader.TrySetParameter("screenOffset", (Main.screenPosition - Main.screenLastPosition) / screenSize);
            waterShader.TrySetParameter("targetSize", screenSize);
            waterShader.SetTexture(distortionTarget, 1);
            waterShader.SetTexture(TileTargetManagers.LiquidTarget, 2, SamplerState.LinearClamp);
            waterShader.Apply();

            Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        }
    }

    public override void PreUpdateEntities()
    {
        bool inSubworld = SubworldSystem.IsActive<ForgottenShrineSubworld>();
        if (WasInSubworldLastFrame != inSubworld)
        {
            WasInSubworldLastFrame = inSubworld;
            if (inSubworld)
                OnEnter?.Invoke();
        }
    }

    public override void PostDrawTiles()
    {
        ManagedScreenFilter mistShader = ShaderManager.GetFilter("HeavenlyArsenal.ForgottenShrineMistShader");
        ManagedScreenFilter reflectionShader = ShaderManager.GetFilter("HeavenlyArsenal.ForgottenShrineWaterReflectionShader");
        if (!WasInSubworldLastFrame)
        {
            for (int i = 0; i < 30; i++)
            {
                mistShader.Update();
                reflectionShader.Update();
            }
            ModContent.GetInstance<ForgottenShrineBackground>().Opacity = 0f;

            return;
        }

        mistShader.TrySetParameter("targetSize", Main.ScreenSize.ToVector2());
        mistShader.TrySetParameter("oldScreenPosition", Main.screenLastPosition);
        mistShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        mistShader.TrySetParameter("mistColor", new Color(84, 74, 154).ToVector4());
        mistShader.TrySetParameter("noiseAppearanceThreshold", 0.3f);
        mistShader.TrySetParameter("mistCoordinatesZoom", new Vector2(1f, 0.4f));
        mistShader.TrySetParameter("mistHeight", 160f);
        mistShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
        mistShader.SetTexture(TileTargetManagers.LiquidTarget, 2, SamplerState.LinearClamp);
        mistShader.SetTexture(LightingMaskTargetManager.LightTarget, 3, SamplerState.LinearClamp);
        mistShader.Activate();

        reflectionShader.TrySetParameter("targetSize", Main.ScreenSize.ToVector2());
        reflectionShader.TrySetParameter("oldScreenPosition", Main.screenLastPosition);
        reflectionShader.TrySetParameter("zoom", Main.GameViewMatrix.Zoom);
        reflectionShader.TrySetParameter("reflectionStrength", 0.47f);
        reflectionShader.TrySetParameter("reflectionMaxDepth", 146f);
        reflectionShader.SetTexture(TileTargetManagers.LiquidTarget, 2, SamplerState.LinearClamp);
        reflectionShader.Activate();

        ModContent.GetInstance<ForgottenShrineBackground>().ShouldBeActive = true;
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (!SubworldSystem.IsActive<ForgottenShrineSubworld>())
            return;

        tileColor = new Color(0.6f, 0.4f, 0.4f);
    }
}
