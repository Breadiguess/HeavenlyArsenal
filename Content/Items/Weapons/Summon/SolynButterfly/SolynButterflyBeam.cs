using System.Collections.Generic;
using HeavenlyArsenal.Content.Items.Weapons.Summon.SolynButterfly;
using Luminance.Assets;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.Audio;
using static Luminance.Common.Utilities.Utilities;

namespace NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles.SolynProjectiles;

public class SolynButterflyBeam : ModProjectile, INotResistedByMars
{
    public Projectile Creator;

    /// <summary>
    ///     The owner of this laserbeam.
    /// </summary>
    public Player Owner => Main.player[Projectile.owner];

    /// <summary>
    ///     How long this laserbeam has existed, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[1];

    /// <summary>
    ///     How long this laserbeam currently is.
    /// </summary>
    public ref float LaserbeamLength => ref Projectile.ai[2];

    /// <summary>
    ///     How long this laserbeam should exist for, in frames.
    /// </summary>
    public static int Lifetime => SecondsToFrames(4.75f);

    /// <summary>
    ///     The maximum length of this laserbeam.
    /// </summary>
    public static float MaxLaserbeamLength => 5600f;

    /// <summary>
    ///     The color of the lens flare on this laserbeam.
    /// </summary>
    public static Color LensFlareColor => new(255, 174, 147);

    /// <summary>
    ///     The speed at which this laserbeam aims towards the mouse.
    /// </summary>
    public static float MouseAimSpeedInterpolant => 0.02f;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 8000;
    }

    public override void SetDefaults()
    {
        Projectile.width = 96;
        Projectile.height = 96;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.friendly = true;
        Projectile.timeLeft = Lifetime;
        Projectile.localNPCHitCooldown = 1;
        Projectile.MaxUpdates = 2;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.hide = true;
        Projectile.DamageType = DamageClass.Summon;
    }

    public override void AI()
    {
        if (Projectile.owner < 0 || Projectile.owner >= Main.maxPlayers || !Owner.active || Owner.dead)
        {
            Projectile.Kill();

            return;
        }

        if (Time == 2f && Main.LocalPlayer.WithinRange(Projectile.Center, 3000f))
        {
            SoundEngine.PlaySound
            (
                GennedAssets.Sounds.Mars.SolynStarBeamFire with
                {
                    Volume = 1.05f
                }
            );
        }

        var solyn = Main.projectile[Owner.GetModPlayer<ButterflyMinionPlayer>().Butterfly.whoAmI];
        Creator = solyn;
        AimTowardsMouse(solyn);

        Projectile.Center = solyn.Center + Projectile.velocity * 100;

        LaserbeamLength = Utils.Clamp(LaserbeamLength + 175f, 0f, MaxLaserbeamLength);

        ScreenShakeSystem.StartShake(InverseLerp(0f, 20f, Time) * 2f);

        CreateOuterParticles();

        Time++;
    }

    public override void OnKill(int timeLeft)
    {
        Creator.ai[1] = 0;
        var a = Creator.ModProjectile as ButterflyMinion;
        a.AttackCooldown = ButterflyMinion.AttackCooldownMax;
        //a.AttackCooldownMax;
    }

    /// <summary>
    ///     Makes this beam slowly aim towards the user's mouse.
    /// </summary>
    public void AimTowardsMouse(Projectile butterfly)
    {
        if (Main.myPlayer != Projectile.owner)
        {
            return;
        }

        var oldVelocity = Projectile.velocity;

        var butterflyInstance = butterfly.ModProjectile as ButterflyMinion;

        if (butterflyInstance == null)
        {
            return;
        }

        var target = butterflyInstance.targetNPC;

        if (target == null || !target.active)
        {
            return;
        }

        var idealRotation = Projectile.AngleTo(target.Center);
        Projectile.velocity = Projectile.velocity.ToRotation().AngleLerp(idealRotation, MouseAimSpeedInterpolant).ToRotationVector2();

        if (Projectile.velocity != oldVelocity)
        {
            Projectile.netUpdate = true;
            Projectile.netSpam = 0;
        }
    }

    /// <summary>
    ///     Creates particles along the deathray's outer boundaries.
    /// </summary>
    public void CreateOuterParticles()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            return;
        }

        var perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);

        for (var i = 0; i < 6; i++)
        {
            var arcLifetime = Main.rand.Next(6, 14);
            var energyLengthInterpolant = Main.rand.NextFloat();
            var perpendicularDirection = Main.rand.NextFromList(-1f, 1f);
            var arcReachInterpolant = Main.rand.NextFloat();
            var energySpawnPosition = Projectile.Center + Projectile.velocity * energyLengthInterpolant * LaserbeamLength + perpendicular * LaserWidthFunction(0.5f) * perpendicularDirection * 0.9f;
            var arcOffset = perpendicular.RotatedBy(1.04f) * float.Lerp(40f, 320f, (float)Math.Pow(arcReachInterpolant, 4f)) * perpendicularDirection;
            NewProjectileBetter(Projectile.GetSource_FromAI(), energySpawnPosition, arcOffset, ModContent.ProjectileType<SmallTeslaArc>(), 0, 0f, -1, arcLifetime, 1f);
        }
    }

    public float LaserWidthFunction(float completionRatio)
    {
        var initialBulge = Convert01To010(InverseLerp(0.15f, 0.85f, LaserbeamLength / MaxLaserbeamLength)) * InverseLerp(0f, 0.05f, completionRatio) * 32f;
        var idealWidth = initialBulge + (float)Math.Cos(Main.GlobalTimeWrappedHourly * 90f) * 6f + Projectile.width;
        var closureInterpolant = InverseLerp(0f, 8f, Lifetime - Time);

        var circularStartInterpolant = InverseLerp(0.05f, 0.012f, completionRatio);
        var circularStart = (float)Math.Sqrt(1.001f - circularStartInterpolant.Squared());

        return Utils.Remap(LaserbeamLength, 0f, MaxLaserbeamLength, 4f, idealWidth) * closureInterpolant * circularStart;
    }

    public float BloomWidthFunction(float completionRatio)
    {
        return LaserWidthFunction(completionRatio) * 1.9f;
    }

    public Color LaserColorFunction(float completionRatio)
    {
        var lengthOpacity = InverseLerp(0f, 0.45f, LaserbeamLength / MaxLaserbeamLength);
        var startOpacity = InverseLerp(0f, 0.032f, completionRatio);
        var endOpacity = InverseLerp(0.95f, 0.81f, completionRatio);
        var opacity = lengthOpacity * startOpacity * endOpacity;
        var startingColor = Projectile.GetAlpha(new Color(255, 45, 123));

        return startingColor * opacity;
    }

    public static Color BloomColorFunction(float completionRatio)
    {
        return new Color(255, 10, 150) * InverseLerpBump(0.02f, 0.05f, 0.81f, 0.95f, completionRatio) * 0.34f;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var theMagicFactorThatMakesEveryElectricShineEffectSoMuchBetter = Cos01(Main.GlobalTimeWrappedHourly * 85f);

        var laserPositions = Projectile.GetLaserControlPoints(12, LaserbeamLength);
        laserPositions[0] -= Projectile.velocity * 10;

        // Draw bloom.
        var shader = ShaderManager.GetShader("NoxusBoss.PrimitiveBloomShader");
        shader.TrySetParameter("innerGlowIntensity", 0.45f);
        var bloomSettings = new PrimitiveSettings(BloomWidthFunction, BloomColorFunction, Shader: shader, UseUnscaledMatrix: false);
        PrimitiveRenderer.RenderTrail(laserPositions, bloomSettings, 46);

        // Draw the beam.
        var deathrayShader = ShaderManager.GetShader("NoxusBoss.SolynTagTeamBeamShader");
        deathrayShader.TrySetParameter("secondaryColor", new Color(255, 196, 36).ToVector4());
        deathrayShader.TrySetParameter("lensFlareColor", LensFlareColor.ToVector4());
        deathrayShader.SetTexture(GennedAssets.Textures.Noise.DendriticNoiseZoomedOut, 1, SamplerState.PointWrap);

        var laserSettings = new PrimitiveSettings(LaserWidthFunction, LaserColorFunction, Shader: deathrayShader, UseUnscaledMatrix: false);
        PrimitiveRenderer.RenderTrail(laserPositions, laserSettings, 75);

        // Draw a superheated lens flare and bloom instance at the center of the beam.
        var shineIntensity = InverseLerp
                                 (0f, 12f, Time) *
                             InverseLerp(0f, 7f, Projectile.timeLeft) *
                             float.Lerp(1f, 1.2f, theMagicFactorThatMakesEveryElectricShineEffectSoMuchBetter) *
                             SolynTagTeamChargeUp.MaxGleamScaleFactor;

        var drawPosition = Projectile.Center - Main.screenPosition;
        var glow = GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall.Value;
        var flare = MiscTexturesRegistry.ShineFlareTexture.Value;

        for (var i = 0; i < 3; i++)
        {
            Main.spriteBatch.Draw
            (
                flare,
                drawPosition,
                null,
                Projectile.GetAlpha
                (
                    LensFlareColor with
                    {
                        A = 0
                    }
                ),
                0f,
                flare.Size() * 0.5f,
                shineIntensity * 2f,
                0,
                0f
            );
        }

        for (var i = 0; i < 2; i++)
        {
            Main.spriteBatch.Draw
            (
                glow,
                drawPosition,
                null,
                Projectile.GetAlpha
                (
                    LensFlareColor with
                    {
                        A = 0
                    }
                ),
                0f,
                glow.Size() * 0.5f,
                shineIntensity * 2f,
                0,
                0f
            );
        }

        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        var _ = 0f;
        var laserWidth = LaserWidthFunction(0.25f) * 1.8f;
        var start = Projectile.Center;
        var end = start + Projectile.velocity.SafeNormalize(Vector2.Zero) * LaserbeamLength * 0.95f;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, laserWidth, ref _);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }

    public override bool ShouldUpdatePosition()
    {
        return false;
    }
}