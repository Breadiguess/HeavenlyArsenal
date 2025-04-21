using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Content.Subworlds.Generation;
using HeavenlyArsenal.Content.Subworlds.Generation.Bridges;
using Microsoft.Xna.Framework;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Graphics.UI;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.Events;
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
            maxSpawns = LumUtils.AnyBosses() ? 0 : 5;
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

    private static void CreateCandles()
    {
        BridgeGenerationSettings settings = BaseBridgePass.BridgeGenerator.Settings;
        int groundLevelY = Main.maxTilesY - ForgottenShrineGenerationHelpers.GroundDepth;
        int waterLevelY = groundLevelY - ForgottenShrineGenerationHelpers.WaterDepth;
        int bridgeLowYPoint = waterLevelY - settings.BridgeBeamHeight - settings.BridgeThickness;
        for (int tileX = BaseBridgePass.BridgeGenerator.Left; tileX < BaseBridgePass.BridgeGenerator.Right; tileX++)
        {
            if (BaseBridgePass.BridgeGenerator.InNonRooftopBridgeRange(tileX) &&
                BaseBridgePass.BridgeGenerator.CalculateXWrappedBySingleBridge(tileX) == settings.BridgeArchWidth / 2)
            {
                float worldX = tileX * 16f + 8f;
                float verticalOffset = BaseBridgePass.BridgeGenerator.CalculateArchHeight(tileX) * -16f - 30f;
                Vector2 candleSpawnPosition = new Vector2(worldX, bridgeLowYPoint * 16f + verticalOffset);
                SpiritCandleParticle candle = SpiritCandleParticle.Pool.RequestParticle();
                candle.Prepare(candleSpawnPosition, Vector2.Zero, 0f, Color.White, Vector2.One * 0.35f);

                ParticleEngine.Particles.Add(candle);
            }
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

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (!SubworldSystem.IsActive<ForgottenShrineSubworld>())
            return;

        tileColor = new Color(0.6f, 0.4f, 0.4f);
    }
}
