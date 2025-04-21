using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Content.Subworlds.Generation;
using HeavenlyArsenal.Content.Waters;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Graphics.LightingMask;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using NoxusBoss.Core.Graphics.UI;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.GameContent.Shaders;
using Terraria.ID;
using Terraria.Localization;
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

        CellPhoneInfoModificationSystem.WeatherReplacementTextEvent += UseWeatherText;
        CellPhoneInfoModificationSystem.MoonPhaseReplacementTextEvent += UseMoonNotFoundText;
        CellPhoneInfoModificationSystem.PlayerXPositionReplacementTextEvent += UseParsecsPositionTextX;
        CellPhoneInfoModificationSystem.PlayerYPositionReplacementTextEvent += UseParsecsPositionTextY;
        GlobalNPCEventHandlers.EditSpawnPoolEvent += OnlyAllowFriendlySpawnsInShrine;
        GlobalNPCEventHandlers.EditSpawnRateEvent += IncreaseFriendlySpawnsInShrine;
    }

    private static void OnlyAllowFriendlySpawnsInShrine(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        if (!WasInSubworldLastFrame)
            return;

        // Get a collection of all NPC IDs in the spawn pool that are not critters.
        IEnumerable<int> npcsToRemove = pool.Keys.Where(npcID => !NPCID.Sets.CountsAsCritter[npcID]);

        // Use the above collection as a blacklist, removing all NPCs that are included in it, effectively ensuring only critters may spawn in the garden.
        foreach (int npcIDToRemove in npcsToRemove)
            pool.Remove(npcIDToRemove);
    }

    private static void IncreaseFriendlySpawnsInShrine(Player player, ref int spawnRate, ref int maxSpawns)
    {
        if (WasInSubworldLastFrame)
        {
            spawnRate = 180;
            maxSpawns = LumUtils.AnyBosses() ? 0 : 20;
        }
    }

    private string UseWeatherText(string originalText)
    {
        if (WasInSubworldLastFrame)
            return Language.GetTextValue("Mods.HeavenlyArsenal.CellPhoneInfoOverrides.ForgottenShrineText");

        return null;
    }

    private string UseMoonNotFoundText(string originalText)
    {
        if (WasInSubworldLastFrame)
            return Language.GetTextValue("GameUI.WaningCrescent");

        return null;
    }

    private string UseParsecsPositionTextX(string originalText)
    {
        if (WasInSubworldLastFrame)
            return Language.GetText($"Mods.NoxusBoss.CellPhoneInfoOverrides.ParsecText").Format($"{7411267996481:n0}");

        return null;
    }

    private string UseParsecsPositionTextY(string originalText)
    {
        if (WasInSubworldLastFrame)
            return Language.GetText($"Mods.NoxusBoss.CellPhoneInfoOverrides.ParsecText").Format($"{7233858412997:n0}");

        return null;
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
        float spacingPerBridge = ForgottenShrineGenerationHelpers.BridgeArchWidth * 16f;
        int horizontalCoverage = Main.maxTilesX - ForgottenShrineGenerationHelpers.BridgeStartX;
        int candleCount = horizontalCoverage / ForgottenShrineGenerationHelpers.BridgeArchWidth;
        int groundLevelY = Main.maxTilesY - ForgottenShrineGenerationHelpers.GroundDepth;
        int waterLevelY = groundLevelY - ForgottenShrineGenerationHelpers.WaterDepth;
        int bridgeLowYPoint = waterLevelY - ForgottenShrineGenerationHelpers.BridgeBeamHeight - ForgottenShrineGenerationHelpers.BridgeThickness;
        int bridgeIndex = 0;
        int snappedBridgeStartX = ForgottenShrineGenerationHelpers.BridgeStartX;
        snappedBridgeStartX -= snappedBridgeStartX % ForgottenShrineGenerationHelpers.BridgeArchWidth;

        float x = spacingPerBridge * (ForgottenShrineGenerationHelpers.BridgeRooftopsPerBridge - 0.5f) + snappedBridgeStartX * 16f;
        for (int i = 0; i < candleCount; i++)
        {
            // Ensure that candles only appear on bridges without a roof.
            if (bridgeIndex % ForgottenShrineGenerationHelpers.BridgeRooftopsPerBridge != 1)
            {
                float verticalOffset = ForgottenShrineGenerationHelpers.CalculateArchHeight((int)(x / 16)) * -16f - 30f;
                Vector2 candleSpawnPosition = new Vector2(x + 8f, bridgeLowYPoint * 16f + verticalOffset);

                SpiritCandleParticle candle = SpiritCandleParticle.Pool.RequestParticle();
                candle.Prepare(candleSpawnPosition, Vector2.Zero, 0f, Color.White, Vector2.One * 0.35f);

                ParticleEngine.Particles.Add(candle);
            }

            // Increment the bridge index and X position.
            bridgeIndex++;
            x += spacingPerBridge;
        }
    }

    private void DisableIdleLiquidDistortion(On_WaterShaderData.orig_Apply orig, WaterShaderData self)
    {
        // Ensure that orig is still called, so as to not mess up any detours to this method made by other mods.
        orig(self);

        // However, at the same time, if the subworld is active, apply a separate water distortion shader, so that the water can be rendered completely still by default.
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

        if (!WasInSubworldLastFrame)
            return;

        ModContent.GetInstance<ForgottenShrineBackground>().ShouldBeActive = true;
        Main.time = Main.nightLength * 0.71;
        Main.dayTime = false;
        Main.windSpeedCurrent = 0f;
        Sandstorm.Happening = false;
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
        reflectionShader.TrySetParameter("reflectionWaviness", 0.0023f);
        reflectionShader.SetTexture(TileTargetManagers.LiquidTarget, 2, SamplerState.LinearClamp);
        reflectionShader.Activate();
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (!SubworldSystem.IsActive<ForgottenShrineSubworld>())
            return;

        tileColor = new Color(0.6f, 0.4f, 0.4f);
    }
}
