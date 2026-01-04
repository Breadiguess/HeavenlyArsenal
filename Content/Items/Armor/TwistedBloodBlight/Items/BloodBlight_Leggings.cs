using CalamityMod.Items;
using CalamityMod.Items.Materials;
using HeavenlyArsenal.Content.Rarities;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Items
{
    [AutoloadEquip(EquipType.Legs)]
    public class BloodBlight_Leggings : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 18;
            Item.rare = ModContent.RarityType<BloodMoonRarity>();
            Item.defense = 15;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<GenericDamageClass>() += 0.12f;
            player.GetCritChance<GenericDamageClass>() += 4;
            player.moveSpeed -= 0.07f;
        }

       

    }
}
