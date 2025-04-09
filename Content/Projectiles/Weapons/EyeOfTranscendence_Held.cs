using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons
{
    class EyeOfTranscendence_Held : ModProjectile
    {
        public override string Texture => "HeavenlyArsenal/Content/Projectiles/Misc/ChaliceOfFun_Juice";
        public override void SetDefaults()
        {
            Projectile.damage = 38;
        }
    }
}
