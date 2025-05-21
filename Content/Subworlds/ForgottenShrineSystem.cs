using HeavenlyArsenal.Content.NPCs;
using HeavenlyArsenal.Content.Subworlds.Generation;
using HeavenlyArsenal.Content.Subworlds.Generation.Bridges;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity;
using NoxusBoss.Core.GlobalInstances;
using NoxusBoss.Core.Graphics.SpecificEffectManagers;
using NoxusBoss.Core.Graphics.UI;
using NoxusBoss.Core.Utilities;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
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

    /// <summary>
    /// The horizontal distance in parsecs displayed if the player has the appropriate info accessory.
    /// </summary>
    public static ulong HorizontalDistanceParsecs => 7411267481601;

    /// <summary>
    /// The vertical distance in parsecs displayed if the player has the appropriate info accessory.
    /// </summary>
    public static ulong VerticalDistanceParsecs => 7233858412997;

    public override void OnModLoad()
    {
        OnEnter += BaseBridgePass.CreateCandles;
        OnEnter += ShrinePass.CreateCandles;

        CellPhoneInfoModificationSystem.WeatherReplacementTextEvent += UseWeatherText;
        CellPhoneInfoModificationSystem.MoonPhaseReplacementTextEvent += UseMoonNotFoundText;
        CellPhoneInfoModificationSystem.PlayerXPositionReplacementTextEvent += UseParsecsPositionTextX;
        CellPhoneInfoModificationSystem.PlayerYPositionReplacementTextEvent += UseParsecsPositionTextY;
        GlobalNPCEventHandlers.EditSpawnPoolEvent += OnlyAllowFriendlySpawnsInShrine;
        GlobalNPCEventHandlers.EditSpawnRateEvent += IncreaseFriendlySpawnsInShrine;
        GlobalTileEventHandlers.IsTileUnbreakableEvent += MakeShrineUnbreakable;
        GlobalWallEventHandlers.IsWallUnbreakableEvent += MakeShrineUnbreakable;
        On_Main.DrawBlack += ForceDrawBlack;
    }

    private static void OnlyAllowFriendlySpawnsInShrine(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        if (!WasInSubworldLastFrame)
            return;

        // Get a collection of all NPC IDs in the spawn pool that are not critters.
        IEnumerable<int> npcsToRemove = pool.Keys.Where(npcID => !NPCID.Sets.CountsAsCritter[npcID]);

        // Use the above collection as a blacklist, removing all NPCs that are included in it, effectively ensuring only critters may spawn in the shrine.
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

    private static void ForceDrawBlack(On_Main.orig_DrawBlack orig, Main self, bool force)
    {
        if (WasInSubworldLastFrame)
            force = true;

        orig(self, force);
    }

    private bool MakeShrineUnbreakable(int x, int y, int type) => SubworldSystem.IsActive<ForgottenShrineSubworld>();

    private string UseWeatherText(string originalText)
    {
        if (WasInSubworldLastFrame)
        {
            if (MathF.Abs(Main.windSpeedCurrent) >= 0.03f)
                return Language.GetTextValue("Mods.HeavenlyArsenal.CellPhoneInfoOverrides.ForgottenShrineWindyText");

            return Language.GetTextValue("Mods.HeavenlyArsenal.CellPhoneInfoOverrides.ForgottenShrineText");
        }

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
            return Language.GetText($"Mods.NoxusBoss.CellPhoneInfoOverrides.ParsecText").Format($"{HorizontalDistanceParsecs:n0}");

        return null;
    }

    private string UseParsecsPositionTextY(string originalText)
    {
        if (WasInSubworldLastFrame)
            return Language.GetText($"Mods.NoxusBoss.CellPhoneInfoOverrides.ParsecText").Format($"{VerticalDistanceParsecs:n0}");

        return null;
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

        // TODO -- Remove later.

       // bool summonTheHorde = Main.LocalPlayer.name != "ModTester2" && Main.LocalPlayer.name != "Lucille";
        //if (summonTheHorde)
        //{
        //    EmptinessSprayPlayerDeletionSystem.PlayerWasDeletedByNamelessDeity = true;
         //   EmptinessSprayPlayerDeletionSystem.PlayerWasDeleted = true;
        //}
        /*
        if (summonTheHorde && Main.rand.NextBool(60))
            if (Main.rand.NextBool(2))
            {
                NPC.NewNPC(new EntitySource_WorldEvent(), (int)Main.LocalPlayer.Center.X, (int)Main.LocalPlayer.Center.Y - 400, ModContent.NPCType<AvatarOfEmptiness>());
            }
            else
                NPC.NewNPC(new EntitySource_WorldEvent(), (int)Main.LocalPlayer.Center.X, (int)Main.LocalPlayer.Center.Y - 400, ModContent.NPCType<NamelessDeityBoss>());
        */
        EnableBackground();
        Main.time = Main.nightLength * 0.71;
        Main.dayTime = false;
        Main.windSpeedCurrent = Main.windSpeedCurrent.StepTowards(0f, 0.01f);
        Sandstorm.Happening = false;
    }

    public override void PostDrawTiles() => EnableBackground();

    private static void EnableBackground()
    {
        if (SubworldSystem.IsActive<ForgottenShrineSubworld>())
        {
            ModContent.GetInstance<ForgottenShrineBackground>().ShouldBeActive = true;
            ModContent.GetInstance<ForgottenShrineBackground>().Opacity = 1f;
        }
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        tileColor = Color.Lerp(tileColor, new Color(0.6f, 0.4f, 0.4f), ModContent.GetInstance<ForgottenShrineBackground>().Opacity);
    }
}
