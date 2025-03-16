using CalamityMod.Particles;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod;
using HeavenlyArsenal.Content.Projectiles.Holdout;
using HeavenlyArsenal.Content.Items.Weapons.Ranged;

namespace HeavenlyArsenal.Content.Projectiles.Ranged;

public class RancorMagicCircle : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Magic";
    public Player Owner => Main.player[Projectile.owner];
    public ref float Time => ref Projectile.ai[0];
    private SlotId PulseLoopSoundSlot;
    public ActiveSound PulseLoopSound
    {
        get
        {
            ActiveSound sound;
            if (SoundEngine.TryGetActiveSound(PulseLoopSoundSlot, out sound))
                return sound;
            return null;
        }
    }
    public float ChargeupCompletion => MathHelper.Clamp(Time / ChargeupTime, 0f, 1f);
    public const int ChargeupTime = FusionRifle.MaxChargeTime;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 114;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 90000;
    }

    public override void AI()
    {
        // Always update before the laserbeam, so that it doesn't recieve strange offsets.
        Projectile.Calamity().UpdatePriority = 1f;

        // If the owner is no longer able to cast the circle, kill it.
        if (!Owner.channel || Owner.noItems || Owner.CCed)
        {
            Projectile.Kill();
            return;
        }

        if (Time >= 1f && Owner.ownedProjectileCounts[ModContent.ProjectileType<FusionRifleHoldout>()] <= 0)
        {
            Projectile.Kill();
            return;
        }

        // Adjust visual values such as scale and opacity when charging.
        AdjustVisualValues();

        // Update aim.
        UpdateAim();

        // Decide where to position the magic circle.
        Vector2 circlePointDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX * Owner.direction);
        Projectile.Center = Owner.Center + circlePointDirection * Projectile.scale * 56f;

        // Adjust the owner's direction.
        Owner.ChangeDir(Projectile.direction);

        // Do animation stuff.
        


        ActiveSound soundOut;
        // Handle charge stuff.
        if (Time < ChargeupTime)
            HandleChargeEffects();

        // Create an idle ominous sound once the laser has appeared.
        else if (!SoundEngine.TryGetActiveSound(PulseLoopSoundSlot, out soundOut) || !soundOut.IsPlaying)
            PulseLoopSoundSlot = SoundEngine.PlaySound(SoundID.DD2_EtherianPortalIdleLoop with { IsLooped = true }, Projectile.Center);

        // Make a cast sound effect soon after the circle appears.
        if (Time == 15f)
            SoundEngine.PlaySound(SoundID.Item117, Projectile.Center);

        Time++;
    }

    public void AdjustVisualValues()
    {
        Projectile.scale = Utils.GetLerpValue(0.1f, 35f, Time, true) * 1.4f;
        Projectile.Opacity = (float)Math.Pow(Projectile.scale / 1.4f, 2D);
        Projectile.rotation -= MathHelper.ToRadians(Projectile.scale * 4f);
    }

    public void UpdateAim()
    {
        // Only execute the aiming code for the owner since Main.MouseWorld is a client-side variable.
        if (Main.myPlayer != Projectile.owner)
            return;

        Vector2 idealDirection = Owner.SafeDirectionTo(Main.MouseWorld, Vector2.UnitX * Owner.direction);
        Vector2 newAimDirection = Projectile.velocity.MoveTowards(idealDirection, 1f);

        // Sync if the direction is different from the old one.
        // Spam caps are ignored due to the frequency of this happening.
        if (newAimDirection != Projectile.velocity)
        {
            Projectile.netUpdate = true;
            Projectile.netSpam = 0;
        }

        Projectile.velocity = newAimDirection;
        Projectile.direction = (Projectile.velocity.X > 0f).ToDirectionInt();
    }

       public void HandleChargeEffects()
    {
        // Play charge-up sound.
        if (Time == 30)
            SoundEngine.PlaySound(new("CalamityMod/Sounds/Custom/MoonLordLaserCharge"), Projectile.Center, _ => new ProjectileAudioTracker(Projectile).IsActiveAndInGame());

       
        // Create the laser once the charge animation is complete.
        if (FusionRifleHoldout.CurrentChargeTime == ChargeupTime - 1f)
        {
            // Play a laserbeam deathray sound. Should probably be replaced some day
            SoundEngine.PlaySound(SoundID.Zombie104, Projectile.Center);

            //if (Main.myPlayer == Projectile.owner)
            //   Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<RancorLaserbeam>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Projectile.identity);
        }
    }

    //I want to draw the ring so that it looks like part of it is infront of the fusion rifle while part of it is behind.
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D outerCircleTexture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        Texture2D outerCircleGlowmask = ModContent.Request<Texture2D>(Texture + "Glowmask").Value;
        //Texture2D innerCircleTexture = ModContent.Request<Texture2D>(Texture + "Inner").Value;
        //Texture2D innerCircleGlowmask = ModContent.Request<Texture2D>(Texture + "InnerGlowmask").Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;

        float directionRotation = Projectile.velocity.ToRotation();
        Color startingColor = Color.Red;
        Color endingColor = Color.Blue;

        void restartShader(Texture2D texture, float opacity, float circularRotation, BlendState blendMode)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, blendMode, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            CalamityUtils.CalculatePerspectiveMatricies(out Matrix viewMatrix, out Matrix projectionMatrix);

            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseColor(startingColor);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseSecondaryColor(endingColor);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseSaturation(directionRotation);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].UseOpacity(opacity);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uDirection"].SetValue((float)Projectile.direction);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uCircularRotation"].SetValue(circularRotation);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uImageSize0"].SetValue(texture.Size());
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["overallImageSize"].SetValue(outerCircleTexture.Size());
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Shader.Parameters["uWorldViewProjection"].SetValue(viewMatrix * projectionMatrix);
            GameShaders.Misc["CalamityMod:RancorMagicCircle"].Apply();
        }

        restartShader(outerCircleGlowmask, Projectile.Opacity, Projectile.rotation, BlendState.Additive);
        Main.EntitySpriteDraw(outerCircleGlowmask, drawPosition, null, Color.White, 0f, outerCircleGlowmask.Size() * 0.5f, Projectile.scale * 1.075f, SpriteEffects.None, 0);

        //restartShader(outerCircleTexture, Projectile.Opacity * 0.7f, Projectile.rotation, BlendState.AlphaBlend);
        //Main.EntitySpriteDraw(outerCircleTexture, drawPosition, null, Color.White, 0f, outerCircleTexture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

        //restartShader(innerCircleGlowmask, Projectile.Opacity * 0.5f, 0f, BlendState.Additive);
        //Main.EntitySpriteDraw(innerCircleGlowmask, drawPosition, null, Color.White, 0f, innerCircleGlowmask.Size() * 0.5f, Projectile.scale * 1.075f, SpriteEffects.None, 0);

        //restartShader(innerCircleTexture, Projectile.Opacity * 0.7f, 0f, BlendState.AlphaBlend);
        //Main.EntitySpriteDraw(innerCircleTexture, drawPosition, null, Color.White, 0f, innerCircleTexture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
        Main.spriteBatch.ExitShaderRegion();

        return false;
    }




    public override void PostDraw(Color lightColor)
    {
        base.PostDraw(lightColor);
    }

    public override void OnKill(int timeLeft) => PulseLoopSound?.Stop();

    public override bool ShouldUpdatePosition() => false;

    public override bool? CanDamage() => false;
}
