using CalamityMod.Items;
using HeavenlyArsenal.Content.Rarities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Items
{
    [AutoloadEquip(EquipType.Body)]
    public class BloodBlight_Chestplate :ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 22;
            Item.rare = ModContent.RarityType<BloodMoonRarity>();
            Item.defense = 22;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<GenericDamageClass>() += 0.1f;
            player.GetCritChance<GenericDamageClass>() += 6;
            player.moveSpeed -= 0.15f;
        }

    }
}
