using System;
using System.Collections.Generic;
using System.Linq;
    

using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;

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
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.damage = 300;
        }
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

        public override void AI()
        {
            Projectile.velocity *= 0.9999f;
            Projectile.velocity.Y += 100 * (float)Math.Sin(Time);

            Projectile.rotation = MathHelper.PiOver2+Projectile.velocity.ToRotation();
            if(Projectile.velocity.Length() <= Vector2.One.Length())
            {
                Projectile.velocity = Vector2.Zero;

            }
            
        }
        public override bool PreDraw(ref Color lightColor)
        {

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Vector2 origin = texture.Size() / 2;


            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, 1, SpriteEffects.None); 
            return false;
        }
    }
}
