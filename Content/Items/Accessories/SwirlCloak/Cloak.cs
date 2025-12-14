using NoxusBoss.Content.Rarities;

namespace HeavenlyArsenal.Content.Items.Accessories.SwirlCloak;

internal class Cloak : ModItem, ILocalizedModType
{
    public override string LocalizationCategory => "Items.Accessories";

    public override void SetStaticDefaults() { }

    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.value = Item.buyPrice(0, 57, 40, 2);
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        //if(player.velocity.Length()< 0.01f)
        //    player.Calamity().accStealthGenBoost += 2;
        player.GetModPlayer<CloakPlayer>().Active = true;
    }
}