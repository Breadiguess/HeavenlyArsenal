using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    internal class Aoe_Rifle_Reforge : ModPrefix
    {
        public override bool CanRoll(Item item)
        {
            if(item.type != ModContent.ItemType<Aoe_Rifle_Item>()) 
                return true; 
            return false;
            
        }
        public override LocalizedText DisplayName => base.DisplayName;

        public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus)
        {
            damageMult = 2;
            knockbackMult = 2;
            useTimeMult = 2;
            scaleMult = 2; 
            shootSpeedMult = 2;

            critBonus *= 4;
        }
    }
}
