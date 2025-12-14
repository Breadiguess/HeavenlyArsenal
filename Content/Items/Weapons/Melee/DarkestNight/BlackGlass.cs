using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.DarkestNight;

public class BlackGlass : ModProjectile
{
    public Color GlowColor;

    public ref Player Owner => ref Main.player[Projectile.owner];

    public ref float Time => ref Projectile.ai[0];

    public override string GlowTexture => "HeavenlyArsenal/Content/Items/Weapons/Melee/DarkestNight/BlackGlass_Glow";

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 4;
    }

    public override void SetDefaults()
    {
        Projectile.hostile = false;
        Projectile.friendly = true;

        Projectile.Size = new Vector2(10, 10);
        Projectile.DamageType = DamageClass.Melee;
        Projectile.tileCollide = false;
        Projectile.extraUpdates = 2;
        Projectile.scale = 0.0f;
    }

    public override void AI()
    {
        if (Projectile.scale < 1)
        {
            Projectile.scale = MathHelper.Clamp(float.Lerp(Projectile.scale, 1.15f, 0.15f), 0, 1);
        }

        var thing = Utils.Remap(Time / (30f * Projectile.MaxUpdates), 0, 1, 0.0f, 1f);
        //Main.NewText($"{thing}");
        var TrackStrength = float.Lerp(0.09f, 0, thing);
        var target = Projectile.FindTargetWithinRange(500);

        if (target != null && Time > 7)
        {
            var loc = Projectile.Center.AngleTo(target.Center);
            Projectile.rotation = Projectile.rotation.AngleLerp(loc, TrackStrength);
        }

        var Offset = new Vector2(0, (float)Math.Cos(Time) / 10).RotatedBy(Projectile.rotation) * 10;
        var desiredVel = Projectile.rotation.ToRotationVector2() * 17;

        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel + Offset, 0.5f);

        if (Time > 30 * Projectile.MaxUpdates)
        {
            ExplodeIntoLight();
        }

        Time++;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        for (var i = 1; i < 8; i++)
        {
            var particle = BlackGlassFragment.pool.RequestParticle();

            var AdjustedPos = Projectile.Center + new Vector2(-20, 0).RotatedBy(Projectile.rotation);

            var AdjustedVelocity = Projectile.velocity + new Vector2(Main.rand.NextFloat(-1, 30), Main.rand.NextFloat(-20, 20)).RotatedBy(Projectile.rotation);
            var rotation = Projectile.rotation + MathHelper.ToRadians(Main.rand.NextFloat(-20, 20));
            float Scale = 1;
            particle.Prepare(Projectile.Center, AdjustedVelocity, rotation, 120, GlowColor, 1, 0, i);

            if (!Main.rand.NextBool(2))
            {
                ParticleEngine.ShaderParticles.Add(particle);
            }
            else
            {
                Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), AdjustedPos, AdjustedVelocity, ModContent.ProjectileType<BlackGlassFragment_Projectile>(), 0, 0);
            }
        }
    }

    public void ExplodeIntoLight()
    {
        var fragment = BlackGlassFragment.pool.RequestParticle();

        var AdjustedPos = Projectile.Center + new Vector2(-20, 0).RotatedBy(Projectile.rotation);

        var AdjustedVelocity = new Vector2(Main.rand.NextFloat(-1, 30), Main.rand.NextFloat(-20, 20)).RotatedBy(Projectile.rotation);
        var rotation = Projectile.rotation + MathHelper.ToRadians(Main.rand.NextFloat(-20, 20));
        float Scale = 1;

        for (var i = 0; i < 7; i++)
        {
            fragment.Prepare(Projectile.Center, AdjustedVelocity, rotation, 120, GlowColor, 1, 0, i);
            ParticleEngine.ShaderParticles.Add(fragment);
        }

        var particle = LightFlash.pool.RequestParticle();

        var coin = Main.rand.NextBool(5);

        particle.Prepare
        (
            Projectile.Center,
            Vector2.Zero,
            Projectile.rotation + MathHelper.ToRadians(Main.rand.Next(60)),
            60,
            Main.rand.NextFloat(0.1f, 0.4f),
            GlowColor,
            coin
        );

        ParticleEngine.ShaderParticles.Add(particle);
        Projectile.velocity = Vector2.Zero;
        ScreenShakeSystem.StartShake(3);

        SoundEngine.PlaySound
        (
            GennedAssets.Sounds.Common.Twinkle with
            {
                MaxInstances = 16,
                PitchVariance = 0.5f
            }
        );

        //var d = Projectile.NewProjectile();
        var d = Projectile.NewProjectileDirect(Owner.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BlindingLight>(), 1, 0);
        d.rotation = Projectile.rotation;
        Projectile.Kill();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var Base = ModContent.Request<Texture2D>(Texture).Value;
        var Glow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Melee/DarkestNight/BlackGlass_Glow").Value;

        var DrawPos = Projectile.Center - Main.screenPosition;
        var Origin = Base.Size() * 0.5f;
        var Grigin = Glow.Size() * 0.5f;
        var Rot = Projectile.rotation + MathHelper.PiOver2;

        var Scale = new Vector2(0.6f) * Projectile.scale;
        var GlowScale = new Vector2(1, 1) * 0.75f * Projectile.scale;

        var flip = Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        var GlowMulti = float.Lerp(0, 1f, Math.Clamp(Time / 20, 0, 1));

        GlowColor = RainbowColorGenerator.TrailColorFunction((float)Math.Abs(Math.Sin(Main.GlobalTimeWrappedHourly + Projectile.whoAmI * 10)));

        Main.EntitySpriteDraw
        (
            Glow,
            DrawPos,
            null,
            GlowColor with
            {
                A = 0
            } *
            GlowMulti,
            Rot,
            Glow.Size() * 0.5f,
            GlowScale,
            flip
        );

        Main.EntitySpriteDraw(Base, DrawPos, null, Color.AntiqueWhite, Rot, Origin, Scale, flip);
        DrawAfterImages(flip, GlowColor);

        //Utils.DrawBorderString(Main.spriteBatch, Time.ToString(), DrawPos, Color.AntiqueWhite);
        return false;
    }

    public void DrawAfterImages(SpriteEffects spriteEffect, Color glowColor)
    {
        var Base = ModContent.Request<Texture2D>(Texture).Value;
        var Glow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Melee/DarkestNight/BlackGlass_Glow").Value;

        var Origin = Base.Size() * 0.5f;

        for (var i = Projectile.oldPos.Length - 1; i >= 0; i--)

        {
            var DrawPos = Projectile.oldPos[i] - Main.screenPosition;
            DrawPos += Projectile.Center - Projectile.position;
            var Rot = Projectile.oldRot[i] + MathHelper.PiOver2;

            var Scale = Projectile.scale * new Vector2(Math.Clamp((Projectile.oldPos.Length - i) / 10f * 2, 0.1f, 2));

            Main.EntitySpriteDraw
            (
                Glow,
                DrawPos,
                null,
                glowColor with
                {
                    A = 0
                },
                Rot,
                Glow.Size() * 0.5f,
                Scale,
                spriteEffect
            );

            Main.EntitySpriteDraw(Base, DrawPos, null, Color.AntiqueWhite, Rot, Origin, Scale, spriteEffect);
        }
    }
}