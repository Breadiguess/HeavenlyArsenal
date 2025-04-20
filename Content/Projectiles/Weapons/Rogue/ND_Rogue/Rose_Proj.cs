using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Rogue.ND_Rogue
{
    class Rose_Proj : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0]; 
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 500;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 1;
            Projectile.aiStyle = -1;
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetStaticDefaults()
        {
            //ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void AI()
        {
            // TODO:
            //The rose fires upwards and slows down before rapidly firing petals at the target
            //so: as long as time is greater than a certain point, rise upwards. after that time has passed,
            //spawn petals for a time before deleting the projectile
            if(Time>= Projectile.timeLeft)
            {
                for(int i= 0; i <6; i++)
                {
                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<Rose_Petal_Proj>(), (int)(Projectile.damage * 0.5f), 0f, Projectile.owner);
                    Main.projectile[proj].timeLeft = 60;
                    Main.projectile[proj].velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(360));
                }
            }
            else
            {
                Projectile.velocity.Y -= 0.1f;
                Projectile.velocity.X *= 0.99f;
                Projectile.velocity.Y *= 0.99f;
            }
        }

        public override bool? CanDamage()
        {
            return base.CanDamage();
        }

        public override void OnKill(int timeLeft)
        {
            //do vfx on death
            base.OnKill(timeLeft);
        }


        public override void CutTiles()
        {
            //dont cut tiles for the first bit of life
            base.CutTiles();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Projectiles/Weapons/Rogue/ND_Rogue/FlowerShuriken_ProjE").Value;
            Rectangle frame = texture.Frame(1, 6, 1, 6);


            SpriteEffects spriteEffects = SpriteEffects.None;
            Vector2 origin = new Vector2(frame.Width / 2, frame.Height/6 * Owner.gravDir);
            //float LillySquish = MathF.Cos(Main.GlobalTimeWrappedHourly * 10.5f + Projectile.Center.X + Projectile.Center.Y) * 1f;
            float LillyScale = 0.1f;
            Vector2 LillyPos = new Vector2(Projectile.Center.X, Projectile.Center.Y);
            Color glowmaskColor = new Color(2, 0, 156);
            Main.EntitySpriteDraw(texture, LillyPos - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, LillyScale, spriteEffects, 0f);
            return false;
        }




    }
}
