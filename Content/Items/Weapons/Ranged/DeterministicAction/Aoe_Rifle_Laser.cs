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
using Terraria.Localization;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    internal class Aoe_Rifle_Laser : ModProjectile
    {
        public PiecewiseCurve ShrinkCurve;
        public bool PowerShot = false;
        public override string Texture =>  MiscTexturesRegistry.InvisiblePixelPath;
        public const int LASER_RANGE = 6_000;
        #region pixelation
        public override void Load()
        {
            On_Main.CheckMonoliths += PixelateLaser;
        }
        public static RenderTarget2D LaserTarget;
        private void PixelateLaser(On_Main.orig_CheckMonoliths orig)
        {
            if (LaserTarget == null || LaserTarget.IsDisposed)
                LaserTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
            else if (LaserTarget.Size() != new Vector2(Main.screenWidth / 2, Main.screenHeight / 2))
            {
                Main.QueueMainThreadAction(() =>
                {
                    LaserTarget.Dispose();
                    LaserTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth / 2, Main.screenHeight / 2);
                });
                return;
            }
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null);

            Main.graphics.GraphicsDevice.SetRenderTarget(LaserTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            foreach (Projectile projectile in Main.projectile.Where(n => n.active  && n.type == ModContent.ProjectileType<Aoe_Rifle_Laser>()))
            {
               DrawLaser(projectile.ModProjectile as Aoe_Rifle_Laser);


            }

            Main.graphics.GraphicsDevice.SetRenderTarget(null);

            Main.spriteBatch.End();

            orig();
        }

        void DrawLaser(Aoe_Rifle_Laser laser)
        {
            Texture2D tex = GennedAssets.Textures.GreyscaleTextures.BloomLine2;
            Vector2 Origin = new Vector2(tex.Width / 2, 0);
            Color color = Color.Lerp(Color.Red, Color.Crimson, 1 - LumUtils.InverseLerp(0, 20, laser.Projectile.timeLeft));
            float scalar = laser.ShrinkCurve.Evaluate(LumUtils.InverseLerp(0, 20, laser.Projectile.timeLeft));

            Vector2 Scale = new Vector2(1 * scalar, 30);
            if (laser.PowerShot)
            {
                Scale = new Vector2(5f * scalar, 30);
                Main.EntitySpriteDraw(tex, laser.Projectile.Center - Main.screenPosition, null, Color.White, -MathHelper.PiOver2, Origin, Scale * 0.4f, 0);

                Main.EntitySpriteDraw(tex, laser.Projectile.Center - Main.screenPosition, null, Color.Purple, -MathHelper.PiOver2, Origin, Scale * 0.6f, 0);

            }
            Main.EntitySpriteDraw(tex, laser.Projectile.Center - Main.screenPosition, null, color, -MathHelper.PiOver2, Origin, Scale, 0);
          

        }
        #endregion



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

        public override void CutTiles()
        {
            base.CutTiles();
        }
        public override bool? CanCutTiles()
        {
            return true;
            return base.CanCutTiles();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Aoe_Rifle_HitParticle particle = new Aoe_Rifle_HitParticle();
            particle.Prepare(target.Center, target.AngleTo(Projectile.Center), 60);
            float damageMulti = 1.2f;
            if (PowerShot)
            {
                damageMulti = 1.6f;
            }
            Projectile.damage = (int)(Projectile.damage * damageMulti);
            
            ParticleEngine.ShaderParticles.Add(particle);
        }
       
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.CritDamage.Base *= 2f;
            modifiers.ArmorPenetration += 30;
            if (PowerShot)
            {
                modifiers.DefenseEffectiveness *= 0;
                modifiers.ScalingBonusDamage += 3f;

                if (target.SuperArmor)
                {
                    modifiers = modifiers with
                    {
                        SuperArmor = false
                    };
                }
            }
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            var projName = Main.player[Projectile.owner].name.ToString();//Lang.GetProjectileName(Projectile.type).Value;
            var val = Main.rand.Next(0, 3);
            var text = NetworkText.FromKey($"Mods.{Mod.Name}.PlayerDeathMessages.Aoe_Rifle{val}", target.name, projName);

            modifiers = new Player.HurtModifiers
            {
                DamageSource = PlayerDeathReason.ByCustomReason(text),
                Dodgeable = PowerShot? false: true

            };
            modifiers.ArmorPenetration += 30f;
           
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Aoe_Rifle_HitParticle particle = new Aoe_Rifle_HitParticle();
            particle.Prepare(target.Center, target.AngleTo(Projectile.Center), 60);
            float damageMulti = 1.2f;
            if (PowerShot)
            {
                damageMulti = 1.6f;
            }
            Projectile.damage = (int)(Projectile.damage * damageMulti);

            ParticleEngine.ShaderParticles.Add(particle);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            //todo: laser collision
            Vector2 offset = new Vector2(LASER_RANGE, 0).RotatedBy(Projectile.rotation);
            float _ = 0;
            float sizeIncrease = 1;
            if (PowerShot)
                sizeIncrease = 2;
            return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size() * sizeIncrease, Projectile.Center, Projectile.Center + offset, 60f, ref _);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, default, default, null, Main.GameViewMatrix.ZoomMatrix);
            Main.EntitySpriteDraw(LaserTarget, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, LaserTarget.Size() / 2, 2, 0);
            Main.spriteBatch.ResetToDefault();


            return false;
        }
    }
}
