namespace HeavenlyArsenal.Content.Items.Accessories.Cosmetic;

public class LightHaloItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.vanity = true;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);
        
        player.GetModPlayer<LightHaloPlayer>().Enabled = true;
    }
}