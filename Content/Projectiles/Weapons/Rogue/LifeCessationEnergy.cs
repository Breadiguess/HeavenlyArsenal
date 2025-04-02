using Luminance.Assets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Rogue
{
    class LifeCessationEnergy : ModProjectile
    {

        public ref float Size => ref Projectile.ai[1];
        public ref float Time => ref Projectile.ai[2];

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetStaticDefaults() 
        { 
        
        
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = Projectile.width;

            Projectile.aiStyle = 0;
            Projectile.damage = 300;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            base.AI();
        }
        public override bool? CanCutTiles()
        {
            return true;
        }



        public override bool? Colliding(Rectangle projHitbox, Microsoft.Xna.Framework.Rectangle targetHitbox)
        {

            
            return targetHitbox.IntersectsConeFastInaccurate(Projectile.Center, Size * Projectile.ai[2], Projectile.rotation, MathHelper.Pi / 7f);   
        }

    }
}
