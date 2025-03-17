using System;
using CalamityMod;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;
using Particle = CalamityMod.Particles.Particle;





namespace HeavenlyArsenal.Projectiles.Misc

{
    public class VambraceDischarge : ModProjectile, ILocalizedModType, IPixelatedPrimitiveRenderer
    {

        public int Time
        {
            get;
            set;
        }

        public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.BeforeProjectiles;
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];
        private static float ExplosionRadius = 150f;

        public override void SetDefaults()
        {
            //These shouldn't matter because its circular
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.friendly = true;
            Projectile.DamageType = DefaultDamageClass.Default;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 50;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

        }
        public override void AI()
        {
            Time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Electrified, 240);

            Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.AntiqueWhite, new Vector2(2f, 2f), Main.rand.NextFloat(12f, 25f), 0f, Main.rand.NextFloat(0.8f, 1.1f), 20);
            GeneralParticleHandler.SpawnParticle(pulse);
            Particle pulse2 = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Blue, new Vector2(2f, 2f), Main.rand.NextFloat(12f, 25f), 0f, Main.rand.NextFloat(0.6f, 0.9f), 20);
            GeneralParticleHandler.SpawnParticle(pulse2);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, ExplosionRadius, targetHitbox);
        public override bool? CanDamage() => base.CanDamage();
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = Math.Sign(Owner.direction);
        }

        public override bool? CanCutTiles() => false;





        public float BloodWidthFunction(float completionRatio)
        {
            float baseWidth = Projectile.width * 0.66f;
            float smoothTipCutoff = MathHelper.SmoothStep(0f, 1f, InverseLerp(0.09f, 0.3f, completionRatio));
            return smoothTipCutoff * baseWidth;
        }

        public Color BloodColorFunction(float completionRatio)
        {
            return Projectile.GetAlpha(new Color(82, 1, 23));
        }




        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D BloomCircleSmall = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/GreyscaleTextures/BloomCircleSmall").Value;


            float scaleFactor = Projectile.width / 50f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + Projectile.velocity;
            Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.DarkRed) with { A = 0 } * 0.2f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 1.2f, 0, 0f);
            Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.Red) with { A = 0 } * 0.4f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 0.64f, 0, 0f);
            Main.spriteBatch.Draw(BloomCircleSmall, drawPosition, null, Projectile.GetAlpha(Color.Orange) with { A = 0 } * 0.4f, 0f, BloomCircleSmall.Size() * 0.5f, scaleFactor * 0.3f, 0, 0f);
            return false;
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            Texture2D BubblyNoise = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/Noise/BubblyNoise").Value;
            Texture2D DendriticNoiseZoomedOut = ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/Noise/DendriticNoiseZoomedOut").Value;



            Rectangle viewBox = Projectile.Hitbox;
            Rectangle screenBox = new Rectangle((int)Main.screenPosition.X, (int)Main.screenPosition.Y, Main.screenWidth, Main.screenHeight);
            viewBox.Inflate(540, 540);
            if (!viewBox.Intersects(screenBox))
                return;

            float lifetimeRatio = Time / 240f;
            float dissolveThreshold = InverseLerp(0.67f, 1f, lifetimeRatio) * 0.5f;

            ManagedShader BloodShader = ShaderManager.GetShader("HeavenlyArsenal.BloodBlobShader");
            BloodShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly + Projectile.identity * 72.113f);
            BloodShader.TrySetParameter("dissolveThreshold", dissolveThreshold);
            BloodShader.TrySetParameter("accentColor", new Vector4(0.6f, 0.02f, -0.1f, 0f));
            BloodShader.SetTexture(BubblyNoise, 1, SamplerState.LinearWrap);
            BloodShader.SetTexture(DendriticNoiseZoomedOut, 2, SamplerState.LinearWrap);

            Console.WriteLine("VambraceDischarge!");

            PrimitiveSettings settings = new PrimitiveSettings(BloodWidthFunction, BloodColorFunction, _ => Projectile.Size * 0.5f + Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.56f, Pixelate: true, Shader: BloodShader);
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 9);
        }





    }
}