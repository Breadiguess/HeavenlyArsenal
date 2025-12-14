using System.Collections.Generic;
using CalamityMod.Items;
using HeavenlyArsenal.Common.Utilities;
using Luminance.Core.Hooking;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using NoxusBoss.Content.Rarities;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.Localization;

namespace HeavenlyArsenal.Content.Items.Armor.ShintoArmor;

[AutoloadEquip(EquipType.Wings)]
internal class ShintoArmorWings : ModItem, ILocalizedModType
{
    public static int WingSlotID { get; private set; }

    public override string Texture => "HeavenlyArsenal/Content/Items/Armor/ShintoArmor/ShintoArmorWings_Item";

    public override string LocalizationCategory => "Items.Accessories";

    public override void SetStaticDefaults()
    {
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<ShintoArmorBreastplate>();
        Item.wingSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Wings);
        WingSlotID = Item.wingSlot;
        ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(100000000, 16.67f, 3.7f, true, 23.5f, 4f);

        new ManagedILEdit
            ("Let Totally not divine wings Hover", Mod, edit => { IL_Player.Update += edit.SubscriptionWrapper; }, edit => { IL_Player.Update -= edit.SubscriptionWrapper; }, LetWingsHover).Apply();

        On_Player.WingMovement += UseHoverMovement;
    }

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 20;
        Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
        Item.rare = ModContent.RarityType<AvatarRarity>();
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.noFallDmg = true;
    }

    public override bool WingUpdate(Player player, bool inUse)
    {
        if (player.controlJump && player.wingTime > 0 && player.velocity.Y != 0)
        {
            var frameRate = 5; // FPS
            var maxFrames = 7; // Total frames

            if (player.wingFrame == 0)
            {
                player.wingFrame = 1;
            }

            // Reset frames
            if (player.wingFrame >= maxFrames)
            {
                player.wingFrameCounter = 0;
                player.wingFrame = 0;
            }

            if (player.wingFrameCounter % frameRate == 0)
            {
                player.wingFrame++;
            }

            player.wingFrameCounter++;
        }
        else
        {
            player.wingFrameCounter = 0;
            player.wingFrame = 0; // On ground

            if (player.velocity.Y != 0)
            {
                player.wingFrame = 1; // Falling

                if (player.controlJump && player.velocity.Y > 0)
                {
                    player.wingFrame = 1; // Gliding
                }
            }
        }

        return true;
    }

    public override void VerticalWingSpeeds
        (Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
    {
        ascentWhenFalling = 1f;
        ascentWhenRising = 0.17f;
        maxCanAscendMultiplier = 1.2f;
        maxAscentMultiplier = 3.25f;
        constantAscend = 0.15f;
    }

    #region the same code copied 3 times

    private static void LetWingsHover(ILContext context, ManagedILEdit edit)
    {
        var cursor = new ILCursor(context);

        /* This is the general layout of the code, with local variables cleaned up and extraneous comments added:
         *
         * bool usingWings = false;
         * if (((player.velocity.Y == 0f || player.sliding) && player.releaseJump) || (player.autoJump && player.justJumped))
         * {
         *     player.mount.ResetFlightTime(player.velocity.X);
         *     player.wingTime = (float)player.wingTimeMax;
         * }
         *
         * // Performs the standard wings check.
         * if (player.wingsLogic > 0 && player.controlJump && player.wingTime > 0f && player.jump == 0 && player.velocity.Y != 0f)
         * {
         *     usingWings = true;
         * }
         *
         * // Determine whether the player the player is using wings for a special hover.
         * // Notably, this does not include modded wing IDs.
         * if ((player.wingsLogic == 22 || player.wingsLogic == 28 || player.wingsLogic == 30 || player.wingsLogic == 32 || player.wingsLogic == 29 || player.wingsLogic == 33 || player.wingsLogic == 35 || player.wingsLogic == 37 || player.wingsLogic == 45) && player.controlJump && player.TryingToHoverDown && player.wingTime > 0f)
         * {
         *     usingWings = true;
         * }
         */

        // Search for the start of the if ((player.wingsLogic == 22 || player.wingsLogic == 28... || player.wingsLogic == 37 statement
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcI4(37)))
        {
            edit.LogFailure("The 'if ((player.wingsLogic == 37' check could not be found.");

            return;
        }

        // Find the local index of the usingWings bool by going backwards to the first usingWings = true line.
        var usingWingsIndex = 0;

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
        {
            player.velocity.Y = -0.0001f;
        }
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (Main.LocalPlayer.GetModPlayer<ShintoArmorPlayer>().SetActive)
        {
            var lastTooltipIndex = tooltips.FindLastIndex(t => t.Name.Contains("Tooltip"));

            tooltips.Add(new TooltipLine(Mod, "PressDownNotif", Language.GetTextValue("CommonItemTooltip.PressDownToHover")));
        }
    }

    #endregion
}

public class ShintoArmorWingsDraw : PlayerDrawLayer
{
    public static Asset<Texture2D> yharwingTexture;

    public override void Load()
    {
        yharwingTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/ShintoArmor/ShintoArmorWings_Wings_Real");
    }

    public override Position GetDefaultPosition()
    {
        return new AfterParent(PlayerDrawLayers.Wings);
    }

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        var HasWings = drawInfo.drawPlayer.wings == EquipLoader.GetEquipSlot(Mod, "ShintoArmorWings", EquipType.Wings);
        var HasArmorSet = drawInfo.drawPlayer.GetModPlayer<ShintoArmorPlayer>().SetActive;
        var NoVanityWings = drawInfo.drawPlayer.wings != EquipLoader.GetEquipSlot(Mod, "ShintoArmorWings", EquipType.Wings);

        if ((HasWings || HasArmorSet) && !NoVanityWings)
        {
            return true;
        }

        return false;
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        var drawPlayer = drawInfo.drawPlayer;

        if (drawPlayer.dead)
        {
            return;
        }

        var texture = yharwingTexture.Value;

        var Position = drawInfo.BodyPosition() +
                       new Vector2(16 * -drawInfo.drawPlayer.direction, -6) +
                       new Vector2(0, drawPlayer.GetModPlayer<ShintoArmorPlayer>().offset * drawPlayer.GetModPlayer<ShintoArmorPlayer>().ShadeTeleportInterpolant);

        //Vector2 pos = new Vector2((int)(Position.X - Main.screenPosition.X + (drawPlayer.width / 2) - (2 * drawPlayer.direction)), (int)(Position.Y - Main.screenPosition.Y + (drawPlayer.height / 2) - 2f * drawPlayer.gravDir));
        var lightColor = Lighting.GetColor((int)drawPlayer.Center.X / 16, (int)drawPlayer.Center.Y / 16, Color.White);
        var color = lightColor * (1 - drawInfo.shadow);

        var Frame = texture.Frame(1, 8, 0, drawInfo.drawPlayer.wingFrame);

        var d = new DrawData(texture, Position, Frame, color, 0f, new Vector2(texture.Width / 2, texture.Height / 18), 1f, drawInfo.playerEffect);
        d.color = drawInfo.colorArmorBody;
        d.shader = drawInfo.drawPlayer.cWings;
        drawInfo.DrawDataCache.Add(d);
    }
}