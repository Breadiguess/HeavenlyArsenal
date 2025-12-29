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
            player.GetModPlayer<BloodBlightParasite_Player>().Active = true;
        }
        public override void UpdateArmorSet(Player player)
        {

        }

        
    }
}
