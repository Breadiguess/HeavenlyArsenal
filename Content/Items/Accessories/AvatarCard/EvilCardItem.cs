using NoxusBoss.Content.Rarities;

namespace HeavenlyArsenal.Content.Items.Accessories.AvatarCard;

public class EvilCardItem : ModItem
{
    public override string LocalizationCategory => "Items.Accessories";

    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.DefaultToAccessory();
        
        Item.rare = ModContent.RarityType<AvatarRarity>();
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);
        
        player.GetModPlayer<EvilCardPlayer>().Enabled = true;
    }
}