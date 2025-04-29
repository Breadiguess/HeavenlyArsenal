using Luminance.Common.DataStructures;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.NamelessDeity.SpecificEffectManagers;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.BaseEntities;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using static Luminance.Luminance;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Melee;
public class TelegraphedPortalLaserbeam : BaseTelegraphedPrimitiveLaserbeam, IPixelatedPrimitiveRenderer
{
    public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.AfterProjectiles;

    // This laser should be drawn with pixelation, and as such should not be drawn manually via the base projectile.
    public override bool UseStandardDrawing => false;

    public override int TelegraphPointCount => 33;

    public override int LaserPointCount => 45;

    public override float MaxLaserLength => 8000f;

    public override float LaserExtendSpeedInterpolant => 0.081f;

    public override ManagedShader TelegraphShader => ShaderManager.GetShader("NoxusBoss.NamelessDeityFlowerLaserTelegraphShader");

    public override ManagedShader LaserShader => ShaderManager.GetShader("NoxusBoss.NamelessDeityPortalLaserShader");

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = (int)MaxLaserLength + 400;
    }

    public override void SetDefaults()
    {
        Projectile.width = 112;
        Projectile.height = 112;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.timeLeft = 60000;
    }

    public override void PostAI()
    {
        // Fade out when the laser is about to die.
        Projectile.Opacity = InverseLerp(TelegraphTime + LaserShootTime - 1f, TelegraphTime + LaserShootTime - 11f, Time);

        // Periodically release post-firing particles when the laser is firing.
        // For performance reasons, these effects do not occur if the player is too far away to reasonably witness them.
        bool laserIsFiring = Time >= TelegraphTime && Time <= TelegraphTime + LaserShootTime - 4f;
        if (laserIsFiring && Projectile.WithinRange(Main.LocalPlayer.Center, 1000f))
        {
            // Periodically release outward pulses.
            if (Time % 4f == 3f)
            {
                PulseRing ring = new PulseRing(Projectile.Center, Vector2.Zero, new(229, 60, 90), 0.5f, 2.75f, 12);
                ring.Spawn();
            }
        }
    }

    public override void OnLaserFire()
    {
        GeneralScreenEffectSystem.ChromaticAberration.Start(Main.LocalPlayer.Center - Vector2.UnitY * 200f, 0.5f, 30);
        NamelessDeityKeyboardShader.BrightnessIntensity += 0.4f;

        if (Projectile.ai[2] == 1f)
        {
            SoundStyle sound = GennedAssets.Sounds.NamelessDeity.PortalLaserShoot with
            {
                Volume = 1.32f,
                PitchVariance = 0.1f,
                MaxInstances = 1,
                SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
            };
            SoundEngine.PlaySound(sound, Main.LocalPlayer.Center);
        }

        if (ScreenShakeSystem.OverallShakeIntensity <= 12f)
            ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, 6f, TwoPi, Vector2.UnitX, 0.09f);
    }

    public override float TelegraphWidthFunction(float completionRatio) => Projectile.Opacity * Projectile.width;

    public override Color TelegraphColorFunction(float completionRatio)
    {
        float timeFadeOpacity = InverseLerpBump(TelegraphTime - 1f, TelegraphTime - 7f, TelegraphTime - 20f, 0f, Time);
        float endFadeOpacity = InverseLerp(1f, 0.67f, completionRatio);
        Color baseColor = Color.Lerp(new(206, 46, 164), new(255, 0, 0), Projectile.identity / 9f % 0.7f);
        return baseColor * endFadeOpacity * timeFadeOpacity * Projectile.Opacity * 0.7f;
    }

    public override float LaserWidthFunction(float completionRatio) => Projectile.Opacity * Projectile.width;

    public override Color LaserColorFunction(float completionRatio)
    {
        float timeFade = InverseLerp(LaserShootTime - 1f, LaserShootTime - 8f, Time - TelegraphTime);
        float startFade = InverseLerp(0f, 0.065f, completionRatio);
        Color baseColor = Color.Lerp(new(206, 46, 164), Color.Orange, Projectile.identity / 9f % 0.7f);

        return baseColor * Projectile.Opacity * timeFade * startFade * 0.75f;
    }

    public override void PrepareTelegraphShader(ManagedShader telegraphShader)
    {
        telegraphShader.TrySetParameter("generalOpacity", Projectile.Opacity);
    }

    public override void PrepareLaserShader(ManagedShader laserShader)
    {
        laserShader.TrySetParameter("darknessNoiseScrollSpeed", 2.5f);
        laserShader.TrySetParameter("brightnessNoiseScrollSpeed", 1.7f);
        laserShader.TrySetParameter("darknessScrollOffset", Vector2.UnitY * (Projectile.identity * 0.3358f % 1f));
        laserShader.TrySetParameter("brightnessScrollOffset", Vector2.UnitY * (Projectile.identity * 0.3747f % 1f));
        laserShader.TrySetParameter("drawAdditively", false);
        laserShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.LinearWrap);
        laserShader.SetTexture(GennedAssets.Textures.Noise.DendriticNoiseZoomedOut, 2, SamplerState.LinearWrap);
    }

    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch) => DrawTelegraphOrLaser(true);

    // This is unrelated to the laser's drawing itself, and serves more as stuff that exists at the point at which the laser is being fired, to give the impression of a focal point.
    public override bool PreDraw(ref Color lightColor)
    {
        // Draw an energy source when firing.
        float sourceOpacity = InverseLerp(LaserShootTime - 1f, LaserShootTime - 6f, Time - TelegraphTime) * 0.92f;
        Vector2 sourceScale = Projectile.scale * new Vector2(1f, 3f);
        Vector2 sourceDrawPosition = Projectile.Center - Main.screenPosition + Projectile.velocity * 46f;
        Color sourceColor = Color.White * InverseLerp(-3f, 0f, Time - TelegraphTime) * sourceOpacity;
        sourceColor.A = 0;

        Main.spriteBatch.Draw(BloomCircleSmall, sourceDrawPosition, null, sourceColor, Projectile.velocity.ToRotation(), BloomCircleSmall.Size() * 0.5f, sourceScale, 0, 0f);

        return false;
    }
}