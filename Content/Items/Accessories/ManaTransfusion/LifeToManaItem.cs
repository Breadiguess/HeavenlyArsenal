using CalamityMod.Rarities;
using Luminance.Assets;

namespace HeavenlyArsenal.Content.Items.Accessories.ManaTransfusion;

public class LifeToManaItem : ModItem
{
    public override string Texture => MiscTexturesRegistry.PixelPath;
    
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.accessory = true;

        // TODO: Use Item.sellPrice and Item.buyPrice to set coin values more explicitly.
        Item.value = 43840;

        Item.rare = ModContent.RarityType<Violet>();
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);

        player.statManaMax2 += 60;

        player.GetModPlayer<ManaTransfusionPlayer>().Enabled = true;
    }
}