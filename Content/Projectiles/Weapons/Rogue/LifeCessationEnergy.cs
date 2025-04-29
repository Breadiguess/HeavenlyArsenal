using CalamityMod;
using Luminance.Assets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Rogue
{
    class LifeCessationLance : ModProjectile
    {
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0];
        public bool Stuck;
        public ref float HitX => ref Projectile.localAI[0];
        public ref float HitY => ref Projectile.localAI[1];
        public Vector2 HitOffset => new Vector2(HitX, HitY);

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetStaticDefaults() 
        { 
        
        
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = Projectile.width;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.aiStyle = - 1;
            Projectile.damage = 300;
            Projectile.timeLeft = 400;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; 
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
           
        }

        public override void AI()
        {

            if (Time< 200 && !Stuck)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            
            
            if (Stuck)
            {
                Projectile.Center= HitOffset;
            }
            Time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if(target.life < damageDone && !Stuck)
            {
                HitX = target.position.X + target.width / 2;
                HitY = target.position.Y + target.height / 2;
                Projectile.position = HitOffset;
                Stuck = true;
            }

            base.OnHitNPC(target, hit, damageDone);
        }
        public override bool? CanCutTiles()
        {
            return true;
        }



        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return targetHitbox.IntersectsConeFastInaccurate(Projectile.Center, Projectile.scale, Projectile.rotation, MathHelper.Pi / 7f);   
        }

        public override bool PreDraw(ref Color lightColor)
        {

            Utils.DrawBorderString(Main.spriteBatch, "Stuck: " + Stuck.ToString(), Projectile.Center - Vector2.UnitY * 220 - Main.screenPosition, Color.White);
            return base.PreDraw(ref lightColor);
        }
    }
}
