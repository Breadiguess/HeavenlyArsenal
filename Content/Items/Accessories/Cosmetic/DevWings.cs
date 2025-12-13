using Luminance.Core.Hooking;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using NoxusBoss.Content.Rarities;
using NoxusBoss.Core.CrossCompatibility.Inbound.BaseCalamity;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Accessories.Cosmetic;

[AutoloadEquip(EquipType.Wings)]
public class DevWing : ModItem
{
    public static int WingSlotID
    {
        get;
        private set;
    }

    public override string Texture => "HeavenlyArsenal/Content/Items/Accessories/Cosmetic/ExampleWings";
    public new string LocalizationCategory => "Items.Accessories";

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;

        WingSlotID = Item.wingSlot;

        
        ArmorIDs.Wing.Sets.Stats[WingSlotID] = new WingStats(100000000, 16.67f, 3.7f, true, 23.5f, 4f);
        new ManagedILEdit("Let Totally not divine wings Hover", Mod, edit =>
        {
            IL_Player.Update += edit.SubscriptionWrapper;
        }, edit =>
        {
            IL_Player.Update -= edit.SubscriptionWrapper;
        }, LetWingsHover).Apply();

        On_Player.WingMovement += UseHoverMovement;
    }

    private static void LetWingsHover(ILContext context, ManagedILEdit edit)
    {
        ILCursor cursor = new ILCursor(context);


         if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcI4(37)))
        {
            edit.LogFailure("The 'if ((player.wingsLogic == 37' check could not be found.");
            return;
        }

        // Find the local index of the usingWings bool by going backwards to the first usingWings = true line.
        int usingWingsIndex = 0;
        if (!cursor.TryGotoPrev(MoveType.After, i => i.MatchStloc(out usingWingsIndex)))
        {
            edit.LogFailure("The usingWings local variable's index could not be found.");
            return;
        }

        // Go back to the start of the method and find the place where the usingWings bool is initialized with the usingWings = false line.
        cursor.Goto(0);
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchStloc(usingWingsIndex)))
        {
            edit.LogFailure("The first initialization of the usingWings local variable could not be found.");
            return;
        }

        // Transform the usingWings = true line like so:
        // bool usingWings = true;
        // bool usingWings = true | (player.wingsLogic == WingSlotID && player.controlJump && player.TryingToHoverDown && player.wingTime > 0f);
        // Notice that this includes the same condition used for the "is the player using wings to hover right now?" check.

        // It would be more efficient to remove the true, but for defensive programming purposes this merely adds onto existing local variable definitions, rather than
        // completely replacing them.
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate((Player player) => player.wingsLogic == WingSlotID && player.controlJump && player.TryingToHoverDown && player.wingTime > 0f);
        cursor.Emit(OpCodes.Or);
    }

    private void UseHoverMovement(On_Player.orig_WingMovement orig, Player player)
    {
        orig(player);
        if (player.wingsLogic == WingSlotID && player.TryingToHoverDown)
            player.velocity.Y = -0.0001f;
    }

    public override void SetDefaults()
    {
        Item.width = 26;
        Item.height = 30;
        Item.rare = ModContent.RarityType<AvatarRarity>();
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.noFallDmg = true;
        CalamityCompatibility.GrantInfiniteCalFlight(player);

        if (!hideVisual)
            Lighting.AddLight(player.Center, Vector3.One);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        int lastTooltipIndex = tooltips.FindLastIndex(t => t.Name.Contains("Tooltip"));

        tooltips.Add(new TooltipLine(Mod, "PressDownNotif", Language.GetTextValue("CommonItemTooltip.PressDownToHover")));
    }

    public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    {
        ascentWhenFalling = 2f;
        ascentWhenRising = 0.184f;
        maxCanAscendMultiplier = 1.2f;
        maxAscentMultiplier = 3.25f;
        constantAscend = 0.29f;
    }
}