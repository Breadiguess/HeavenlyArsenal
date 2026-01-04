using CalamityMod;
using HeavenlyArsenal.Content.Items.Armor.ShintoArmor;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players;
using HeavenlyArsenal.Content.Rarities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Items
{
    [AutoloadEquip(EquipType.Head)]
    internal class BloodBlight_Helmet : ModItem, ILocalizedModType
    {
        public override string LocalizationCategory => "Items.Armor.BloodBlight";

        public override void SetDefaults()
        {
            Item.rare = ModContent.RarityType<BloodMoonRarity>();
        }
        public override void UpdateEquip(Player player)
        {
        }
        public override void UpdateArmorSet(Player player)
        {
            player.GetModPlayer<BloodBlightParasite_Player>().Active = true;
            if (player.HeldItem.DamageType.CountsAsClass(ModContent.GetInstance<RogueDamageClass>()))
            {
                player.Calamity().wearingRogueArmor = true;
                    
                player.Calamity().rogueStealthMax += 1.15f;
                player.Calamity().rogueStealthMax = MathF.Round(player.Calamity().rogueStealthMax, 2);

            }
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<BloodBlight_Chestplate>() && legs.type == ModContent.ItemType<BloodBlight_Leggings>();
        }

    }
}
