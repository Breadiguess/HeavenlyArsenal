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
        public override float RollChance(Item item)
        {
            return base.RollChance(item);
        }
        public override bool CanRoll(Item item)
        {

            return base.CanRoll(item) && item.ModItem is Aoe_Rifle_Item;
        }
        

        public override void SetStats(ref float damageMult, ref float knockbackMult, ref float useTimeMult, ref float scaleMult, ref float shootSpeedMult, ref float manaMult, ref int critBonus)
        {
            damageMult = 2;
            knockbackMult = 2;
            useTimeMult = 2;
            shootSpeedMult = 2;
            critBonus *= 10;
            
        }
    }
}
