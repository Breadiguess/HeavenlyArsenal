using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Misc
{
    class CosmicWatergun : ModItem
    {
        public override void AutoStaticDefaults()
        {
            base.AutoStaticDefaults();
        }

        

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.WaterGun);
        }
       
    }
}
