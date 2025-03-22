using Luminance.Assets;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.Graphics.Automators;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.RocheLimit;

public class RocheLimitBlackHole : ModProjectile, IDrawsWithShader
{
    public enum BlackHoleState
    {
        MaterializeFromEnergy,
        StabilizeNearMouse,
        Vanish
    }

    /* Weapon concept:
     * 
     * A weapon which conjures a localized black hole in front of the player, with incredible mana reserves being casted to keep it stabilized.
     * Nearby enemies are sucked into the black hole, becoming spaghettified and disintegrated. Once inside the black hole they take unimaginable quantities of damage every single frame.
     * Upon death, the enemy's loot, along with a powerful burst of radiation from the black hole, are ejected outward at incredible speeds.
     * This ejection can do damage on its own, causing afflicted enemies to be dissolved by extreme temperatures
     */

    // TODO list:
    // 1. Set up usage animations, make the player cast energy that gets sucked into the black hole.
    // 2. Make item use mana and die if the player doesn't have any more to use.
    // 3. Add spaghettification/disintegration effects (Dear god I hope RTs work for this).
    // 4. Add burst ejections.
    // 5. Make a natural black hole dissipation effect instead of having the projectile get instantly deleted if casting conditions are not met.

    /// <summary>
    /// The owner of this black hole.
    /// </summary>
    public ref Player Owner => ref Main.player[Projectile.owner];

    /// <summary>
    /// The current state of this black hole.
    /// </summary>
    public BlackHoleState State
    {
        get;
        set;
    }

    /// <summary>
    /// The diameter of the sun that becomes this black hole.
    /// </summary>
    public ref float SunDiameter => ref Projectile.localAI[0];

    /// <summary>
    /// The spin timer for this sun. Is used before this projectile becomes a black hole.
    /// </summary>
    public ref float SunSpinTime => ref Projectile.localAI[1];

    /// <summary>
    /// The temperature of the sun in kelvin.
    /// </summary>
    public ref float SunTemperature => ref Projectile.localAI[2];

    /// <summary>
    /// A general-purpose timer for this black hole that is reset when a state transition occurs.
    /// </summary>
    public ref float Time => ref Projectile.ai[0];

    /// <summary>
    /// How long this black hole has existed for.
    /// </summary>
    public ref float ExistenceTimer => ref Projectile.ai[1];

    /// <summary>
    /// The current diameter of this black hole.
    /// </summary>
    public ref float BlackHoleDiameter => ref Projectile.ai[2];

    /// <summary>
    /// How long this black hole spends disappearing once it's no longer in use.
    /// </summary>
    public static int DisappearanceTime => LumUtils.SecondsToFrames(0.65f);

    /// <summary>
    /// How long it takes for this black hole to reach its maximum radius after spawning in.
    /// </summary>
    public static int GrowthTime => LumUtils.SecondsToFrames(0.67f);

    /// <summary>
    /// The maximum diameter of the star before it transforms into a black hole.
    /// </summary>
    public static float MaxSunDiameter => 540f;

    /// <summary>
    /// The maximum diameter of this black hole.
    /// </summary>
    public static float MaxBlackHoleDiameter => 325f;

    // Red -> orange -> yellow -> white -> bright blue.
    /// <summary>
    /// The color gradient used to color the sun based on its temperature.
    /// </summary>
    public static readonly Palette TemperatureGradient = new Palette(new Color(250, 80, 30), new Color(250, 143, 29), new Color(250, 192, 60), new Color(252, 240, 180), new Color(255, 255, 255), new Color(200, 236, 254));

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

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
        Projectile.netImportant = true;
    }

    public override void SendExtraAI(BinaryWriter writer) => writer.Write(SunDiameter);

    public override void ReceiveExtraAI(BinaryReader reader) => SunDiameter = reader.ReadSingle();

    public override void AI()
    {
        // Begin dying if no longer holding the click button or otherwise unable to use the item.
        if ((!Owner.channel || Owner.dead || !Owner.active || Owner.noItems || Owner.CCed) && State != BlackHoleState.Vanish)
            SwitchState(BlackHoleState.Vanish);

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

        float idealRotation = SmoothClamp(Projectile.velocity.X * 0.014f, 0.36f);
        Projectile.rotation = Projectile.rotation.AngleLerp(idealRotation, 0.1f);

        Time++;
        ExistenceTimer++;
    }

    private void DoBehavior_MaterializeFromEnergy()
    {
        SetPlayerItemAnimations();
        StandardMouseHoverMotion();

        int sunFormTime = 30;
        if (Time <= sunFormTime)
        {
            float sunGrowInterpolant = Time / sunFormTime;
            SunDiameter = EasingCurves.Sine.Evaluate(EasingType.Out, sunGrowInterpolant) * MaxSunDiameter * 0.5f;
        }

        float horizontalMovement = Projectile.position.X - Projectile.oldPosition.X;
        float sunSpinSpeedFactor = Math.Clamp(MaxSunDiameter / (SunDiameter + 1f), 0.01f, 4f) * LumUtils.InverseLerp(4f, 0f, horizontalMovement);
        float extraSpin = horizontalMovement * -0.001f;
        SunSpinTime += Owner.HorizontalDirectionTo(Projectile.Center) * sunSpinSpeedFactor * -0.009f + extraSpin;

        SunTemperature = MathHelper.Lerp(1200f, 12000f, 1f - LumUtils.Cos01(MathHelper.TwoPi * Time / 300f));
    }

    private void DoBehavior_StabilizeNearMouse()
    {
        SetPlayerItemAnimations();
        StandardMouseHoverMotion();

        float growthInterpolant = LumUtils.InverseLerp(0f, GrowthTime, ExistenceTimer);
        BlackHoleDiameter = EasingCurves.Cubic.Evaluate(EasingType.InOut, growthInterpolant) * MaxBlackHoleDiameter;
    }

    private void DoBehavior_Vanish()
    {
        Projectile.velocity = (Projectile.velocity * 0.92f).ClampLength(0f, 32f);
        BlackHoleDiameter *= 0.9f;
        SunDiameter *= 0.9f;
    }

    /// <summary>
    /// Performs standard mouse hover motion for this black hole, making it stay near the mouse up to a given distance.
    /// </summary>
    private void StandardMouseHoverMotion()
    {
        float hoverDistance = SmoothClamp(Owner.Distance(Main.MouseWorld), MaxBlackHoleDiameter * 0.5f + 56f);
        if (hoverDistance < 120f)
            hoverDistance = 120f;

        Vector2 hoverOffset = Owner.SafeDirectionTo(Main.MouseWorld) * hoverDistance;
        Vector2 stabilizedDestination = Owner.Center + hoverOffset;

        // Stay near the stabilized destination.
        // This only runs for the owner, since they're the client whose mouse should be listened.
        if (Main.myPlayer == Projectile.owner)
        {
            float flySpeedInterpolant = LumUtils.InverseLerp(0f, 54f, ExistenceTimer);
            Projectile.SmoothFlyNear(stabilizedDestination, flySpeedInterpolant * 0.11f, 1f - flySpeedInterpolant * 0.15f);

            Vector2 oldVelocity = Projectile.velocity;
            if (Projectile.velocity != oldVelocity)
            {
                Projectile.netUpdate = true;
                Projectile.netSpam = 0;
            }
        }
    }

    /// <summary>
    /// Switches the current state for this black hole.
    /// </summary>
    private void SwitchState(BlackHoleState state)
    {
        State = state;
        Time = 0f;
        Projectile.netUpdate = true;
    }

    /// <summary>
    /// Performs a smooth clamp that asymptotically approaches the maximum absolute value via a hyperbolic tangent function, rather than hard-clamping it past a threshold.
    /// </summary>
    /// <param name="x">The value to clamp.</param>
    /// <param name="max">The maximum absolute value.</param>
    private static float SmoothClamp(float x, float max)
    {
        return MathF.Tanh(x / max) * max;
    }

    /// <summary>
    /// Handles item animation effects for this black hole, ensuring that the player is considering to be casting an item, creating energy for the black hole to feed off of, etc.
    /// </summary>
    private void SetPlayerItemAnimations()
    {
        if (Main.myPlayer == Projectile.owner)
        {
            int idealDirection = (int)Owner.HorizontalDirectionTo(Main.MouseWorld);
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

    public void RenderBlackHole()
    {
        float blackRadius = 0.3f;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Texture2D invisiblePixel = MiscTexturesRegistry.InvisiblePixel.Value;
        ManagedShader blackHoleShader = ShaderManager.GetShader("HeavenlyArsenal.RealBlackHoleShader");
        blackHoleShader.TrySetParameter("blackHoleRadius", 0.3f);
        blackHoleShader.TrySetParameter("blackHoleCenter", Vector3.Zero);
        blackHoleShader.TrySetParameter("aspectRatioCorrectionFactor", 1f);
        blackHoleShader.TrySetParameter("accretionDiskColor", new Color(90, 126, 210).ToVector3()); // Blue: new Color(90, 126, 210).ToVector3()
        blackHoleShader.TrySetParameter("cameraAngle", 0.32f);
        blackHoleShader.TrySetParameter("cameraRotationAxis", new Vector3(1f, 0f, 0f));
        blackHoleShader.TrySetParameter("accretionDiskScale", new Vector3(1.1f, 0.2f, 1f));
        blackHoleShader.TrySetParameter("zoom", Vector2.One * blackRadius * 2.7f);
        blackHoleShader.TrySetParameter("accretionDiskRadius", 0.33f);
        blackHoleShader.SetTexture(GennedAssets.Textures.Noise.FireNoiseA, 1, SamplerState.LinearWrap);
        blackHoleShader.Apply();

        Main.spriteBatch.Draw(invisiblePixel, drawPosition, null, Color.Transparent, Projectile.rotation, invisiblePixel.Size() * 0.5f, BlackHoleDiameter, 0, 0f);
    }

    private void RenderSun()
    {
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Vector2 scale = Vector2.One * SunDiameter / GennedAssets.Textures.Noise.DendriticNoiseZoomedOut.Value.Size();

        // Calculate sun colors.
        float temperatureInterpolant = MathF.Pow(1f - MathF.Exp(-SunTemperature / 4000f), 2.3f);
        float coronaExponent = 1.23f;
        Vector3 mainColor = TemperatureGradient.SampleColor(temperatureInterpolant).ToVector3();
        Vector3 coronaColor = new Vector3(MathF.Pow(mainColor.X, coronaExponent), MathF.Pow(mainColor.Y, coronaExponent), MathF.Pow(mainColor.Z, coronaExponent));

        // The corona color calculations use rational functions, meaning that even the tiniest quantities can, on extremely small pixel scales, get inflated into high values.
        // To account for this, extremely low color values in the corona are simply zeroed out.
        float minCoronaThreshold = 0.09f;
        if (coronaColor.X < minCoronaThreshold)
            coronaColor.X = 0f;
        if (coronaColor.Y < minCoronaThreshold)
            coronaColor.Y = 0f;
        if (coronaColor.Z < minCoronaThreshold)
            coronaColor.Z = 0f;

        RenderShineGlow(drawPosition, new Color(coronaColor));

        // Supply information to the sun shader.
        ManagedShader sunShader = ShaderManager.GetShader("HeavenlyArsenal.RocheLimitSunShader");
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
    }

    private void RenderShineGlow(Vector2 drawPosition, Color shineColor)
    {
        Texture2D noise = GennedAssets.Textures.Noise.FireNoiseA;
        ManagedShader shineShader = ShaderManager.GetShader("NoxusBoss.RadialShineShader");
        shineShader.Apply();

        Vector2 shineScale = Vector2.One * SunDiameter * 1.6f / noise.Size();
        Main.spriteBatch.Draw(noise, drawPosition, null, shineColor * 0.45f, Projectile.rotation, noise.Size() * 0.5f, shineScale, 0, 0f);
    }

    public void DrawWithShader(SpriteBatch spriteBatch) => RenderSun();

    public override bool? CanDamage() => State == BlackHoleState.StabilizeNearMouse;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => LumUtils.CircularHitboxCollision(Projectile.Center, BlackHoleDiameter * 0.27f, targetHitbox);
}
