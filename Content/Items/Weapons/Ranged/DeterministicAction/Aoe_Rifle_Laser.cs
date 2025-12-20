using HeavenlyArsenal.Common.Graphics;
using Luminance.Assets;
using Luminance.Common.Easings;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    internal class Aoe_Rifle_Laser : ModProjectile
    {
        public PiecewiseCurve ShrinkCurve;
        public override string Texture =>  MiscTexturesRegistry.InvisiblePixelPath;
        public const int LASER_RANGE = 6_000;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = LASER_RANGE;
        }


        public override void SetDefaults()
        {
            ShrinkCurve = new PiecewiseCurve()
                .Add(EasingCurves.Sine, EasingType.In, 0.24f, 0.4f,0.1f)
                .Add(EasingCurves.Exp, EasingType.Out, 1,1);
            Projectile.timeLeft = 10;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.Size = new Vector2(30, 30);
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }


        public override void OnSpawn(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity *= 0;

        }
        public override void PostAI()
        {
            Projectile.Center = Main.player[Projectile.owner].Center;
        }
        public override bool? CanCutTiles()
        {
            
            return base.CanCutTiles();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Aoe_Rifle_HitParticle particle = new Aoe_Rifle_HitParticle();
            particle.Prepare(target.Center, target.AngleTo(Projectile.Center), 60);

            ParticleEngine.ShaderParticles.Add(particle);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            //todo: laser collision
            Vector2 offset = new Vector2(LASER_RANGE, 0).RotatedBy(Projectile.rotation);
            float _ = 0;
            return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center, Projectile.Center + offset, 120f, ref _);
        }

        public override bool PreDraw(ref Color lightColor)
        {

            Texture2D tex = GennedAssets.Textures.GreyscaleTextures.BloomLine2;
            Vector2 Origin = new Vector2(tex.Width / 2, 0);
            Color color = Color.Lerp(Color.Red, Color.Crimson, 1-  LumUtils.InverseLerp(0,20, Projectile.timeLeft));
            float scalar = ShrinkCurve.Evaluate(LumUtils.InverseLerp(0, 20, Projectile.timeLeft));
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, color with { A = 0 }, Projectile.rotation - MathHelper.PiOver2, Origin, new Vector2(1 * scalar, 30), 0);


            return false;
        }
    }
}
