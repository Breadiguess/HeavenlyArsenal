using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Rogue.ND_Rogue
{
    class Rose_Petal_Proj : ModProjectile
    {

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.scale = 1f;
            Projectile.width = Projectile.height = 6;
            Projectile.timeLeft = 180;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
        }
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }
        public override void AI()
        {
            //slowly float to the ground while dealing damage
            base.AI();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return base.PreDraw(ref lightColor);    
        }
    }
}
