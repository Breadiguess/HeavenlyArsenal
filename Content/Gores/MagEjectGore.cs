using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Gores
{
    class MagEjectGore : ModGore
    {
        
        public override string Texture => base.Texture;


        public override void SetStaticDefaults()
        {
            Gore.goreTime = 40;

        }
    }
}
