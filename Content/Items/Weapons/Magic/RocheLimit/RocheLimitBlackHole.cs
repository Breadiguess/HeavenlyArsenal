using System.Collections.Generic;
using System.IO;
using Luminance.Assets;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.SoundSystems;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.RocheLimit;

public class RocheLimitBlackHole : ModProjectile, IDrawsOverRocheLimitDistortion
{
    public enum BlackHoleState
    {
        MaterializeFromEnergy,

        StabilizeNearMouse,

        Vanish
    }

    // Red -> orange -> yellow -> white -> bright blue.
    /// <summary>
    ///     The color gradient used to color the sun based on its temperature.
    /// </summary>
    public static readonly Palette TemperatureGradient = new
        (new Color(250, 80, 30), new Color(250, 143, 29), new Color(250, 192, 60), new Color(252, 240, 180), new Color(255, 255, 255), new Color(200, 236, 254));

    public float Layer => 1f;

    /// <summary>
    ///     A remapped version of the temperature to fit a 0-1 interpolant range for color sampling.
    /// </summary>
    public float TemperatureInterpolant => MathF.Pow(1f - MathF.Exp(-SunTemperature / 4000f), 2.3f);

    /// <summary>
    ///     The owner of this black hole.
    /// </summary>
    public ref Player Owner => ref Main.player[Projectile.owner];

    /// <summary>
    ///     The looped sound instance for this black hole.
    /// </summary>
    public LoopedSoundInstance Brrrrrrr { get; private set; }

    /// <summary>
    ///     The current state of this black hole.
    /// </summary>
    public BlackHoleState State { get; set; }

    /// <summary>
    ///     The diameter of distortion effects produced by this black hole.
    /// </summary>
    public float DistortionDiameter { get; set; }

    /// <summary>
    ///     The intensity of glow effects over the sun.
    /// </summary>
    public float SunGlowIntensity { get; set; }

    /// <summary>
    ///     The diameter of the sun that becomes this black hole.
    /// </summary>
    public ref float SunDiameter => ref Projectile.localAI[0];

    /// <summary>
    ///     The spin timer for this sun. Is used before this projectile becomes a black hole.
    /// </summary>
    public ref float SunSpinTime => ref Projectile.localAI[1];

    /// <summary>
    ///     The temperature of the sun in kelvin.
    /// </summary>
    public ref float SunTemperature => ref Projectile.localAI[2];

    /// <summary>
    ///     A general-purpose timer for this black hole that is reset when a state transition occurs.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    ///     How long this black hole has existed for.
    /// </summary>
    public ref float ExistenceTimer => ref Projectile.ai[1];

    /// <summary>
    ///     The current diameter of this black hole.
    /// </summary>
    public ref float BlackHoleDiameter => ref Projectile.ai[2];

    /// <summary>
    ///     How long this black hole spends disappearing once it's no longer in use.
    /// </summary>
    public static int DisappearanceTime => LumUtils.SecondsToFrames(0.65f);

    /// <summary>
    ///     How long it takes for this black hole to reach its maximum radius after spawning in.
    /// </summary>
    public static int GrowthTime => LumUtils.SecondsToFrames(0.6f);

    /// <summary>
    ///     The maximum diameter of the star before it collapses.
    /// </summary>
    public static float MaxSunDiameter => 700f;

    /// <summary>
    ///     The diameter factor of the star before it transforms into a black hole.
    /// </summary>
    public static float CollapsedSunDiameterFactor => 0.7f;

    /// <summary>
    ///     The maximum diameter of this black hole.
    /// </summary>
    public static float MaxBlackHoleDiameter => MaxSunDiameter * CollapsedSunDiameterFactor;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    /// <summary>
    ///     Renders this projectile.
    /// </summary>
    public void RenderOverDistortion()
    {
        if (SunDiameter >= 0.5f)
        {
            RenderSun();
        }

        if (BlackHoleDiameter >= 0.5f)
        {
            RenderBlackHole();
        }
    }

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 3600;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.manualDirectionChange = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 1;
        Projectile.hide = true;
        Projectile.netImportant = true;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write((int)State);
        writer.Write(SunDiameter);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        State = (BlackHoleState)reader.ReadInt32();
        SunDiameter = reader.ReadSingle();
    }

    public override void AI()
    {
        // Update damage based on the curent magic damage stat, to ensure that mana sickness affects it.
        Projectile.damage = Owner.HeldMouseItem() is null ? 0 : Owner.GetWeaponDamage(Owner.HeldMouseItem());

        // Begin dying if no longer holding the click button or otherwise unable to use the item.
        if ((!Owner.channel || Owner.dead || !Owner.active || Owner.noItems || Owner.CCed) && State != BlackHoleState.Vanish)
        {
            SwitchState(BlackHoleState.Vanish);
        }

        // Check if mana should be consumed.
        // If it should and the player has none, begin dying.
        if (State != BlackHoleState.Vanish && ExistenceTimer % RocheLimit.ManaConsumptionRate == RocheLimit.ManaConsumptionRate - 1)
        {
            if (!Owner.CheckMana(Owner.HeldMouseItem(), -1, true))
            {
                SwitchState(BlackHoleState.Vanish);
            }
        }

        switch (State)
        {
            case BlackHoleState.MaterializeFromEnergy:
                DoBehavior_MaterializeFromEnergy();

                break;
            case BlackHoleState.StabilizeNearMouse:
                DoBehavior_StabilizeNearMouse();

                break;
            case BlackHoleState.Vanish:
                DoBehavior_Vanish();

                break;
        }

        var relativeSpeed = MathF.Max(0f, Projectile.velocity.Length() - Owner.velocity.Length());

        Brrrrrrr?.Update
        (
            Projectile.Center,
            sound =>
            {
                sound.Volume = Projectile.Opacity;
                sound.Pitch = SmoothClamp(relativeSpeed * 0.0072f, 0.19f);
            }
        );

        var idealRotation = SmoothClamp(Projectile.velocity.X * 0.015f, 0.4f);
        Projectile.rotation = Projectile.rotation.AngleLerp(idealRotation, 0.19f);

        Time++;
        ExistenceTimer++;
    }

    private void DoBehavior_MaterializeFromEnergy()
    {
        SetPlayerItemAnimations();
        StandardMouseHoverMotion();

        if (ExistenceTimer >= 5f)
        {
            CastEnergy();
        }

        var sunFormTime = 30;
        var collapseWaitDelay = 15;
        var collapseTime = 60;
        var duration = sunFormTime + collapseWaitDelay + collapseTime + collapseTime;
        var sunGrowInterpolant = LumUtils.InverseLerp(0f, sunFormTime, Time);
        var sunExpandInterpolant = EasingCurves.Sine.Evaluate(EasingType.Out, sunGrowInterpolant);

        var collapseInterpolant = LumUtils.InverseLerp(0f, collapseTime, Time - sunFormTime - collapseWaitDelay);
        SunTemperature = MathHelper.SmoothStep(1200f, 12000f, collapseInterpolant);
        SunGlowIntensity = MathF.Pow(LumUtils.InverseLerp(0.4f, 1f, collapseInterpolant), 0.4f);
        sunExpandInterpolant = MathHelper.SmoothStep(sunExpandInterpolant, CollapsedSunDiameterFactor, MathF.Pow(collapseInterpolant, 0.7f));

        SunDiameter = sunExpandInterpolant * MaxSunDiameter;
        DistortionDiameter = collapseInterpolant * SunDiameter * 0.33f;

        var horizontalMovement = Projectile.position.X - Projectile.oldPosition.X;
        var sunSpinSpeedFactor = Math.Clamp(MaxSunDiameter / (SunDiameter + 1f), 0.01f, 4f) * LumUtils.InverseLerp(4f, 0f, horizontalMovement);
        var extraSpin = horizontalMovement * -0.001f;
        SunSpinTime += Owner.HorizontalDirectionTo(Projectile.Center) * sunSpinSpeedFactor * -0.009f + extraSpin;

        ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, collapseInterpolant.Squared() * 1.5f);

        if (Time == duration - 143)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.StarCrush, Projectile.Center);
        }

        if (Time >= duration)
        {
            SwitchState(BlackHoleState.StabilizeNearMouse);
        }
    }

    private void DoBehavior_StabilizeNearMouse()
    {
        SetPlayerItemAnimations();
        StandardMouseHoverMotion();
        CastMinorParticles();
        AbsorbGores();
        AbsorbDusts();

        Brrrrrrr ??= LoopedSoundManager.CreateNew(GennedAssets.Sounds.NamelessDeity.QuasarLoopStart, GennedAssets.Sounds.NamelessDeity.QuasarLoop, () => !Projectile.active);

        var growthInterpolant = LumUtils.InverseLerp(0f, GrowthTime, Time);
        var easedGrowthInterpolant = EasingCurves.Cubic.Evaluate(EasingType.InOut, growthInterpolant);
        BlackHoleDiameter = easedGrowthInterpolant * MaxBlackHoleDiameter + MathF.Cos(MathHelper.TwoPi * Time / 120f) * 10f;
        SunDiameter = LumUtils.InverseLerp(0.37f, 0f, easedGrowthInterpolant) * MaxSunDiameter * CollapsedSunDiameterFactor;
        DistortionDiameter = MathF.Max(DistortionDiameter, BlackHoleDiameter * 0.275f);

        ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, LumUtils.Convert01To010(growthInterpolant) * 12.5f, shakeStrengthDissipationIncrement: 1.1f);
        ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, LumUtils.InverseLerp(0.67f, 1f, growthInterpolant) * 1.5f, shakeStrengthDissipationIncrement: 0.56f);
    }

    private void DoBehavior_Vanish()
    {
        var decayFactor = 0.81f;
        Projectile.velocity = (Projectile.velocity * 0.92f).ClampLength(0f, 32f);
        BlackHoleDiameter *= decayFactor;
        SunDiameter *= decayFactor;
        DistortionDiameter *= decayFactor;
        Projectile.Opacity *= decayFactor;

        if (DistortionDiameter < 3f)
        {
            DistortionDiameter = 0f;
        }
    }

    /// <summary>
    ///     Performs a smooth clamp that asymptotically approaches the maximum absolute value via a
    ///     hyperbolic tangent function, rather than hard-clamping it past a threshold.
    /// </summary>
    /// <param name="x">The value to clamp.</param>
    /// <param name="max">The maximum absolute value.</param>
    private static float SmoothClamp(float x, float max)
    {
        return MathF.Tanh(x / max) * max;
    }

    /// <summary>
    ///     Performs standard mouse hover motion for this black hole, making it stay near the mouse up to a
    ///     given distance.
    /// </summary>
    private void StandardMouseHoverMotion()
    {
        var hoverDistance = SmoothClamp(Owner.Distance(Main.MouseWorld), MaxBlackHoleDiameter * 0.5f + 100f);

        if (hoverDistance < 120f)
        {
            hoverDistance = 120f;
        }

        var hoverOffset = Owner.SafeDirectionTo(Main.MouseWorld) * hoverDistance;
        var stabilizedDestination = Owner.Center + hoverOffset;

        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Owner.AngleTo(Projectile.Center) - MathHelper.PiOver2);

        // Stay near the stabilized destination.
        // This only runs for the owner, since they're the client whose mouse should be listened to.
        if (Main.myPlayer == Projectile.owner)
        {
            var flySpeedInterpolant = LumUtils.InverseLerp(0f, 54f, ExistenceTimer);
            Projectile.SmoothFlyNear(stabilizedDestination, flySpeedInterpolant * 0.11f, 1f - flySpeedInterpolant * 0.15f);

            var oldVelocity = Projectile.velocity;

            if (Projectile.velocity != oldVelocity)
            {
                Projectile.netUpdate = true;
                Projectile.netSpam = 0;
            }
        }
    }

    /// <summary>
    ///     Makes the owner player cast energy into this projectile.
    /// </summary>
    private void CastEnergy()
    {
        var speedFactor = MathF.Max(SunDiameter / MaxSunDiameter, BlackHoleDiameter / MaxBlackHoleDiameter);
        var handPosition = Owner.Center + Owner.SafeDirectionTo(Projectile.Center) * 20f;

        for (var i = 0; i < 3; i++)
        {
            var fireVelocity = handPosition.SafeDirectionTo(Projectile.Center).RotatedByRandom(0.09f) * Main.rand.NextFloat(10f, 120f) * speedFactor;
            var fireSize = Main.rand.NextFloat(15f, 120f);
            var fireColor = Color.Lerp(new Color(209, 37, 5), new Color(255, 219, 32), Main.rand.NextFloat(0.4f));
            fireColor.A /= 3;

            RocheLimitBlackHoleRenderer.ParticleSystem.CreateNew(handPosition, fireVelocity, Vector2.One * fireSize, fireColor);
        }

        CastMinorParticles();
    }

    /// <summary>
    ///     Makes the owner player cast minor particles into this projectile.
    /// </summary>
    private void CastMinorParticles()
    {
        var stardustColor = TemperatureGradient.SampleColor(TemperatureInterpolant);
        var handPosition = Owner.Center + Owner.SafeDirectionTo(Projectile.Center) * 18f;

        for (var i = 0; i < 3; i++)
        {
            var energyVelocity = handPosition.SafeDirectionTo(Projectile.Center) * Main.rand.NextFloat(3f, 15f);

            var stardust = new HighDefinitionSmokeParticle(handPosition + Vector2.UnitY * 40f, energyVelocity, stardustColor, 27, Main.rand.NextFloat(0.3f, 0.85f), 0f);
            stardust.Spawn();
        }
    }

    /// <summary>
    ///     Makes this black hole absorb and delete gores.
    /// </summary>
    private void AbsorbGores()
    {
        for (var i = 0; i < Main.maxGore; i++)
        {
            var gore = Main.gore[i];
            gore.position = Vector2.Lerp(gore.position, Projectile.Center, 0.02f);
            gore.velocity += gore.position.SafeDirectionTo(Projectile.Center) * 4f;

            if (gore.position.WithinRange(Projectile.Center, 150f))
            {
                gore.active = false;
            }
        }
    }

    private void AbsorbDusts()
    {
        for (var i = 0; i < Main.maxGore; i++)
        {
            var dust = Main.dust[i];
            dust.position = Vector2.Lerp(dust.position, Projectile.Center, 0.02f);
            dust.velocity += dust.position.SafeDirectionTo(Projectile.Center) * 4f;

            if (dust.position.WithinRange(Projectile.Center, 150f))
            {
                dust.active = false;
            }
        }
    }

    /// <summary>
    ///     Switches the current state for this black hole.
    /// </summary>
    private void SwitchState(BlackHoleState state)
    {
        State = state;
        Time = 0f;
        Projectile.netUpdate = true;
    }

    /// <summary>
    ///     Makes this black hole fire a relativistic jet.
    /// </summary>
    public void ReleaseJet(Vector2 jetDirection)
    {
        SoundEngine.PlaySound
        (
            GennedAssets.Sounds.NamelessDeity.BigSupernova with
            {
                MaxInstances = 7
            },
            Projectile.Center
        );

        if (ScreenShakeSystem.OverallShakeIntensity < 7f)
        {
            ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, 7f);
        }

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            var jetID = ModContent.ProjectileType<RelativisticJet>();
            var jetSpeed = 29f;
            var jetVelocity = jetDirection * jetSpeed;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center - jetVelocity, jetVelocity, jetID, Projectile.damage * 3, 0f, Projectile.owner);
        }

        var bloom = new StrongBloom(Projectile.Center, Vector2.Zero, Color.DeepSkyBlue, 5f, 20);
        bloom.Spawn();

        bloom = new StrongBloom(Projectile.Center, Vector2.Zero, Color.White * 0.6f, 3f, 18);
        bloom.Spawn();

        for (var i = 0; i < 75; i++)
        {
            var fireSize = Main.rand.NextFloat(15f, 120f);
            var fireVelocity = jetDirection.RotatedByRandom(0.48f) * Main.rand.NextFloat(150f);
            var fireColor = Color.Lerp(Color.White, Color.DeepSkyBlue, Main.rand.NextFloat(0.4f));
            fireColor.A /= 4;

            RocheLimitBlackHoleRenderer.ParticleSystem.CreateNew(Projectile.Center, fireVelocity, Vector2.One * fireSize, fireColor);
        }
    }

    /// <summary>
    ///     Handles item animation effects for this black hole, ensuring that the player is considering to
    ///     be casting an item, creating energy for the black hole to feed off of, etc.
    /// </summary>
    private void SetPlayerItemAnimations()
    {
        if (Main.myPlayer == Projectile.owner)
        {
            var idealDirection = (int)Owner.HorizontalDirectionTo(Main.MouseWorld);

            if (Projectile.direction != idealDirection)
            {
                Projectile.direction = idealDirection;
                Projectile.netUpdate = true;
            }
        }

        Owner.heldProj = Projectile.whoAmI;
        Owner.SetDummyItemTime(3);
        Owner.ChangeDir(Projectile.direction);
        Owner.itemLocation = Projectile.Center;
        Projectile.timeLeft = DisappearanceTime;
    }

    /// <summary>
    ///     Renders the sun form for this projectile.
    /// </summary>
    private void RenderBlackHole()
    {
        if (Main.netMode == NetmodeID.Server)
        {
            return;
        }

        var blackRadius = 0.3f;
        var accretionDiskScale = MathF.Pow(BlackHoleDiameter / MaxBlackHoleDiameter, 0.65f);
        var drawPosition = Projectile.Center - Main.screenPosition;
        var invisiblePixel = MiscTexturesRegistry.InvisiblePixel.Value;
        var blackHoleShader = ShaderManager.GetShader("HeavenlyArsenal.RealBlackHoleShader");
        blackHoleShader.TrySetParameter("blackHoleRadius", 0.3f);
        blackHoleShader.TrySetParameter("blackHoleCenter", Vector3.Zero);
        blackHoleShader.TrySetParameter("aspectRatioCorrectionFactor", 1f);
        blackHoleShader.TrySetParameter("accretionDiskColor", new Color(72, 112, 246).ToVector3() * accretionDiskScale.Squared() * 1.5f);
        blackHoleShader.TrySetParameter("cameraAngle", 0.32f);
        blackHoleShader.TrySetParameter("cameraRotationAxis", new Vector3(1f, 0f, 0f));
        blackHoleShader.TrySetParameter("accretionDiskScale", accretionDiskScale * new Vector3(1.12f, 0.2f, 1f));
        blackHoleShader.TrySetParameter("zoom", Vector2.One * blackRadius * 2.7f);
        blackHoleShader.TrySetParameter("accretionDiskRadius", 0.33f);
        blackHoleShader.TrySetParameter("accretionDiskSpinSpeed", 1.25f);
        blackHoleShader.SetTexture(GennedAssets.Textures.Noise.FireNoiseA, 1, SamplerState.LinearWrap);
        blackHoleShader.Apply();

        Main.spriteBatch.Draw(invisiblePixel, drawPosition, null, Color.Transparent, Projectile.rotation, invisiblePixel.Size() * 0.5f, BlackHoleDiameter, 0, 0f);
    }

    /// <summary>
    ///     Renders the sun form for this projectile.
    /// </summary>
    private void RenderSun()
    {
        if (Main.netMode == NetmodeID.Server)
        {
            return;
        }

        var drawPosition = Projectile.Center - Main.screenPosition;
        var scale = Vector2.One * SunDiameter / GennedAssets.Textures.Noise.DendriticNoiseZoomedOut.Value.Size();

        // Calculate sun colors.
        var coronaExponent = 1.23f;
        var mainColor = TemperatureGradient.SampleColor(TemperatureInterpolant).ToVector3();
        var coronaColor = new Vector3(MathF.Pow(mainColor.X, coronaExponent), MathF.Pow(mainColor.Y, coronaExponent), MathF.Pow(mainColor.Z, coronaExponent));

        // The corona color calculations use rational functions, meaning that even the tiniest quantities can, on extremely small pixel scales, get inflated into high values.
        // To account for this, extremely low color values in the corona are simply zeroed out.
        var minCoronaThreshold = 0.09f;

        if (coronaColor.X < minCoronaThreshold)
        {
            coronaColor.X = 0f;
        }

        if (coronaColor.Y < minCoronaThreshold)
        {
            coronaColor.Y = 0f;
        }

        if (coronaColor.Z < minCoronaThreshold)
        {
            coronaColor.Z = 0f;
        }

        coronaColor = Vector3.Lerp(coronaColor, Vector3.One * 2f, TemperatureInterpolant);

        RenderShineGlow(drawPosition, new Color(coronaColor));

        // Supply information to the sun shader.
        var sunShader = ShaderManager.GetShader("HeavenlyArsenal.RocheLimitSunShader");
        sunShader.TrySetParameter("coronaIntensityFactor", 0.23f);
        sunShader.TrySetParameter("mainColor", mainColor);
        sunShader.TrySetParameter("darkerColor", mainColor);
        sunShader.TrySetParameter("coronaColor", coronaColor);
        sunShader.TrySetParameter("subtractiveAccentFactor", Vector3.Zero);
        sunShader.TrySetParameter("sphereSpinTime", SunSpinTime);
        sunShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
        sunShader.SetTexture(GennedAssets.Textures.Extra.PsychedelicWingTextureOffsetMap, 2, SamplerState.LinearWrap);
        sunShader.Apply();

        // Draw the sun.
        Texture2D fireNoise = GennedAssets.Textures.Noise.FireNoiseA;
        Main.spriteBatch.Draw(fireNoise, drawPosition, null, new Color(mainColor), Projectile.rotation, fireNoise.Size() * 0.5f, scale, 0, 0f);

        // Draw glow effects.
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        var glowScale = (0.75f + LumUtils.Cos01(Main.GlobalTimeWrappedHourly * 75f) * SunGlowIntensity * 0.4f) * SunGlowIntensity;
        Texture2D glow = GennedAssets.Textures.GreyscaleTextures.BloomCircle;
        var glowColor = new Color(0.81f, 1f, 1f) * SunGlowIntensity * 0.7f;

        for (var i = 0; i < 2; i++)
        {
            Main.spriteBatch.Draw(glow, drawPosition, null, glowColor, 0f, glow.Size() * 0.5f, Vector2.One * SunDiameter * glowScale / glow.Size(), 0, 0f);
        }
    }

    /// <summary>
    ///     Renders the shine glow behind this projectile's sun form.
    /// </summary>
    private void RenderShineGlow(Vector2 drawPosition, Color shineColor)
    {
        if (Main.netMode == NetmodeID.Server)
        {
            return;
        }

        Texture2D noise = GennedAssets.Textures.Noise.FireNoiseA;
        var shineShader = ShaderManager.GetShader("NoxusBoss.RadialShineShader");
        shineShader.Apply();

        var shineScale = Vector2.One * SunDiameter * 1.6f / noise.Size();
        Main.spriteBatch.Draw(noise, drawPosition, null, shineColor * 0.45f, Projectile.rotation, noise.Size() * 0.5f, shineScale, 0, 0f);
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (Main.netMode == NetmodeID.Server)
        {
            return false;
        }

        Main.spriteBatch.End();

        Main.spriteBatch.Begin
            (SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        Texture2D noise = GennedAssets.Textures.Noise.FireNoiseA;
        var shineShader = ShaderManager.GetShader("NoxusBoss.RadialShineShader");
        shineShader.Apply();

        var shineScaleFactor = LumUtils.InverseLerp(0f, 15f, ExistenceTimer);
        var shineSize = MathHelper.Lerp(135f, 155f, LumUtils.Cos01(Main.GlobalTimeWrappedHourly * 76f));
        var drawPosition = Owner.Center - Main.screenPosition + Owner.SafeDirectionTo(Projectile.Center) * 20f;
        var shineScale = new Vector2(1.5f, 1f) * shineScaleFactor * shineSize / noise.Size();
        Main.spriteBatch.Draw(noise, drawPosition, null, Color.White * Projectile.Opacity * 0.2f, Owner.AngleTo(Projectile.Center) + MathHelper.PiOver2, noise.Size() * 0.5f, shineScale, 0, 0f);
        Main.spriteBatch.ResetToDefault();

        return false;
    }

    // See RocheLimitGlobalNPC.cs
    public override bool? CanDamage()
    {
        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        return LumUtils.CircularHitboxCollision(Projectile.Center, BlackHoleDiameter * 0.27f, targetHitbox);
    }
}