using Luminance.Assets;
using Luminance.Common.DataStructures;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.NPCs.Bosses.Avatar.SecondPhaseForm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;
using static NoxusBoss.Assets.GennedAssets;
//using Luminance.Assets.GennedAssets.Textures.Noise


namespace HeavenlyArsenal.Content.Projectiles.Magic;

public class Succ_Blood : ModProjectile, IPixelatedPrimitiveRenderer
{
    

    public PixelationPrimitiveLayer LayerToRenderTo => PixelationPrimitiveLayer.AfterProjectiles;

    /// <summary>
    /// How long this blob has existed for, in frames.
    /// </summary>
    public int Time
    {
        get;
        set;
    }

    /// <summary>
    /// Whether this blob is unaffected by gravity or not.
    /// </summary>
    public bool GravityUnaffected
    {
        get;
        set;
    }

    /// <summary>
    /// Whether this blob can do damage when moving upward or not.
    /// no lmao
    /// </summary>
    

   
    public ref float AccelerationBoost => ref Projectile.ai[2];

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
    }

    public override void SetDefaults()
    {
        Projectile.width = 64;//Main.rand?.Next(32, 93) ?? 48;


        Projectile.height = Projectile.width;
        Projectile.hostile = false;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.hide = true;
        Projectile.timeLeft = 240;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        CooldownSlot = ImmunityCooldownID.General;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(Time);
        writer.Write(GravityUnaffected);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        Time = reader.ReadInt32();
        GravityUnaffected = reader.ReadBoolean();
    }

    public override void OnSpawn(IEntitySource source)
    {
        base.OnSpawn(source);
    }


    public override void AI()
    {
        // Base amplitude and frequency for sine wave motion
        float baseAmplitude = 10f;
        float frequency = 0.2f;

        // Normalize the velocity vector to get the direction
        Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);

        // Calculate a perpendicular vector for sine wave movement
        Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

        // Determine if this projectile moves in the normal or inverted wave
        int waveDirection = (Projectile.ai[0] % 2 == 0) ? 1 : -1;

        // Calculate the sine wave offset using ai[1] as a time tracker
        float sineOffset = waveDirection * baseAmplitude * MathF.Sin(Projectile.ai[1] * frequency);

        // Apply the sine wave movement perpendicular to the velocity
        Projectile.position += perpendicular * sineOffset;

        // Maintain forward motion
        Projectile.position += Projectile.velocity;

        // Increment time for this projectile
        Projectile.ai[1] += 1f;

        // Ensure multiplayer sync
        Projectile.netUpdate = true;
    }







   

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

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        overPlayers.Add(index);
    }

    


    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
         Texture2D BubblyNoise =ModContent.Request<Texture2D>("NoxusBoss/Assets/Textures/Extra/Noise/BubblyNoise").Value;
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


       

        
        



        PrimitiveSettings settings = new PrimitiveSettings(BloodWidthFunction, BloodColorFunction, _ => Projectile.Size * 0.5f + Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.56f, Pixelate: true, Shader: BloodShader);
        PrimitiveRenderer.RenderTrail(Projectile.oldPos, settings, 9);
    }
}