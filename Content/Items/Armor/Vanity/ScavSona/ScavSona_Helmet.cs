using Luminance.Assets;
using NoxusBoss.Content.Rarities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity.ScavSona
{
    [AutoloadEquip(EquipType.Head)]
    public class ScavSona_Helmet : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 14;
            Item.rare = ModContent.RarityType<AvatarRarity>();
            Item.vanity = true;
            Item.value = Item.sellPrice(0, 3, 0, 0);
            
        }

        public override void UpdateEquip(Player player)
        {
            
        }
    }


    
}
