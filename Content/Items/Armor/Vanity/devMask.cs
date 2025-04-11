using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor.Vanity
{


    [AutoloadEquip(EquipType.Head)]
    internal class devMask : ModItem
    {
        
        
        public override void SetStaticDefaults()
        {
            //ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
            ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;


            Item.vanity = true;
        }






    }
}
