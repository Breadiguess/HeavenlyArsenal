using Luminance.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    internal class Aoe_Rifle_EjectedBullet: ModProjectile
    {
        public override string Texture =>  MiscTexturesRegistry.InvisiblePixelPath;


        public bool TouchedGrass = false;
        public Item Inherited;
        public int Time
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.ignoreWater = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            base.OnSpawn(source);
        }
        public override void AI()
        {
            Projectile.scale = 1f;
            Projectile.extraUpdates = 1;
            Time++;
            Player Owner = Main.player[Projectile.owner];
            if (!TouchedGrass)
            {
                if (Projectile.timeLeft <= 240)
                    Projectile.rotation += 0.04f * Projectile.direction;
                else
                    Projectile.rotation = (new Vector2(0, -5 * Projectile.direction) + Projectile.velocity * -1.2f).ToRotation();

                Projectile.velocity.Y += 0.087f;
                Projectile.velocity.X *= 0.99f;

                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity, DustID.SteampunkSteam, -Projectile.velocity.RotatedByRandom(0.1f) * Main.rand.NextFloat(0.2f, 1f), 140, default, Main.rand.NextFloat(0.35f, 0.5f));
                    dust.noGravity = false;
                    dust.color = Color.White;
                }
            }
            else
            {
                Projectile.velocity = Vector2.Zero;
            }
            Projectile.alpha = (int)(255 * Utils.GetLerpValue(60, 0, Projectile.timeLeft, true));
            if (Collision.SolidCollision(Projectile.Center - Projectile.velocity * 1.5f, 2, 2))
                Projectile.tileCollide = true;
        }
        public override bool PreDraw(ref Color lightColor)
        {


            return base.PreDraw(ref lightColor);
        }
    }
}
