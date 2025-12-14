using System.Collections.Generic;
using CalamityMod;
using CalamityMod.Items;
using Terraria.GameContent;
using Terraria.Localization;

namespace HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath;

[AutoloadEquip(EquipType.Neck)]
public class VoidCrestOath : ModItem, ILocalizedModType
{
    public const string HaloEquippedVariableName = "WearingVoidCrest";

    public new string LocalizationCategory => "Items.Accessories";

    public override void SetStaticDefaults()
    {
        // Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 6));
        // ItemID.Sets.AnimatesAsSoul[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 78;
        Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
        Item.rare = ItemRarityID.Purple;
        Item.accessory = true;
    }

    public override void UpdateVanity(Player player)
    {
        var modPlayer = player.GetModPlayer<VoidCrestOathPlayer>();
        modPlayer.Vanity = true;

        if (player.isDisplayDollOrInanimate)
        {
            modPlayer.ResourceInterp = 1;
        }
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (!Main.specialSeedWorld)
        {
            return;
        }

        var text = Language.GetTextValue("Mods.HeavenlyArsenal.Items.Accessories.VoidCrestOath.GFBtooltip");

        var line = new TooltipLine(Mod, "VoidCrestTroll", text);
        var insertIndex = tooltips.FindIndex(t => t.Mod == "Terraria" && t.Name.StartsWith("Tooltip"));

        if (insertIndex == -1)
        {
            tooltips.Add(line);
        }
        else
        {
            tooltips.Insert(insertIndex + 2, line);
        }
    }

    public override void EquipFrameEffects(Player player, EquipType type)
    {
        player.GetModPlayer<VoidCrestOathPlayer>().Vanity = true;

        if (player.isDisplayDollOrInanimate || Main.gameMenu)
        {
            player.GetModPlayer<VoidCrestOathPlayer>().ResourceInterp = 1;
        }
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        //i don't car </3
        var modPlayer = player.GetModPlayer<VoidCrestOathPlayer>();
        modPlayer.voidCrestOathEquipped = true;
        modPlayer.Vanity = false;

        if (hideVisual)
        {
            modPlayer.Hide = true;
            modPlayer.ResourceInterp = 0;
        }
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        CalamityUtils.DrawInventoryCustomScale
        (
            spriteBatch,
            TextureAssets.Item[Type].Value,
            position,
            frame,
            drawColor,
            itemColor,
            origin,
            scale,
            0.6f,
            new Vector2(0f, -2f)
        );

        return false;
    }
}