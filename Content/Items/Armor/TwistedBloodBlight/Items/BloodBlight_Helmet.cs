using CalamityMod;
using HeavenlyArsenal.Content.Items.Armor.BaseArmor;
using HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players;
using HeavenlyArsenal.Content.Rarities;
using System.Collections.Generic;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Items
{
    [AutoloadEquip(EquipType.Head)]
    public class BloodBlight_Helmet : BaseArmorItem
    {
        public override void SetDefaults()
        {
            Item.rare = ModContent.RarityType<BloodMoonRarity>();
            Item.width = 20;
            Item.height = 20;
        }

        protected override void ApplyEquipStats(Player player)
        {
            Stats.AddCrit(player, 8);
        }

        public override void UpdateArmorSet(Player player)
        {
            // This still runs normally; base class does not interfere
            player.setBonus = " ";

            var parasite = player.GetModPlayer<BloodBlightParasite_Player>();
            parasite.Active = true;

            if (player.HeldItem.DamageType.CountsAsClass(ModContent.GetInstance<RogueDamageClass>()))
            {
                player.Calamity().wearingRogueArmor = true;
                Stats.RecordCustom(
                   player,
                   apply: p => p.Calamity().rogueStealthMax += 0.15f,
                   textOrKey: "Mods.HeavenlyArsenal.Armor.BloodBlight.SetBonus.RogueStealth",
                   color: Color.Red,
                   15
               );
            }
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<BloodBlight_Chestplate>()
                && legs.type == ModContent.ItemType<BloodBlight_Leggings>();
        }
    }
}
