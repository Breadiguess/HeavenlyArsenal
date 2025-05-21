using NoxusBoss.Content.Rarities;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity;

[AutoloadEquip(EquipType.Legs)]
public class LightCultist_Pants : ModItem
{
    public override string LocalizationCategory => "Items.Armor.Vanity.LightCultist";
    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 18;
        Item.value = Item.sellPrice(gold: 10);
        Item.rare = ModContent.RarityType<NamelessDeityRarity>();
        Item.value = 0;
        Item.vanity = true;
    }


    public override void UpdateEquip(Player player)
    {
        player.GetDamage(DamageClass.Generic) += 0.20f;
        player.moveSpeed += 0.5f;
        player.runAcceleration *= 1.2f;
        player.maxRunSpeed *= 1.2f;
        player.accRunSpeed *= 0.5f;
        player.runSlowdown *= 2f;
    }
}
