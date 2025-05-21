using System;
using System.Collections.Generic;
using System.Linq;
    

using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab
{
    class Bloodproj : ModProjectile
    {
        public ref float Time => ref Projectile.ai[0];
        
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.timeLeft = 400;
        }
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void AI()
        {
            Projectile.velocity *= 0.9999f;
            Projectile.velocity.Y += 100 * (float)Math.Sin(Time);

            if(Projectile.velocity.Length() <= Vector2.One.Length())
            {
                Projectile.velocity = Vector2.Zero;

            }
            
        }
    }
}
