
using HeavenlyArsenal.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Particles;

public class AvatarRifle_MuzzleFlash : BaseParticle
{
    public static ParticlePool<AvatarRifle_MuzzleFlash> pool = new ParticlePool<AvatarRifle_MuzzleFlash>(500, GetNewParticle<AvatarRifle_MuzzleFlash>);

    public Vector2 Anchor;
    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;
    public int MaxTime;
    public int TimeLeft;
    public Color ColorTint;
    public Color ColorGlow;
    public float Scale;
    private int Style;
    private int SpriteEffect;

    public void Prepare(Vector2 position, Vector2 velocity, float rotation, int lifeTime, Color color, Color glowColor, float scale, Vector2 anchor)
    {
        Anchor = anchor;
        Position = position;
        Velocity = velocity;
        Rotation = rotation;
        MaxTime = lifeTime;
        ColorTint = color;
        ColorGlow = glowColor;
        Scale = scale;
        Style = Main.rand.Next(3);
        SpriteEffect = Main.rand.Next(2);
        //Main.NewText($"AvatarRifle_Muzzleflash Drawn!", Color.AntiqueWhite);
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        Velocity = Vector2.Zero;
        MaxTime = 1;
        TimeLeft = 0;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
       
        //Velocity += new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-0.1f, 0.1f));
        //Velocity *= 1.1f;
        //Position = Anchor;
       // Rotation = Velocity.ToRotation();
        TimeLeft++;
        if (TimeLeft > MaxTime)
            ShouldBeRemovedFromRenderer = true;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Particles/MuzzleFlashParticle").Value;
        float progress = (float)TimeLeft / MaxTime;
        int frameCount = (int)MathF.Floor(MathF.Sqrt(progress) * 6);
        Rectangle frame = texture.Frame(1, 6, 0, frameCount);

        float alpha = 1f - progress;
        Color drawColor = Color.Lerp(ColorTint, ColorGlow, Utils.GetLerpValue(0.3f, 0.7f, progress, true)) * Utils.GetLerpValue(1f, 0.9f, progress, true) * alpha;
        float widthScale = Scale;// * (1f - progress); // Decrease the width over time
        float heightScale = Scale; // Keep the height constant

        Vector2 anchorPosition = new Vector2(frame.Width /2, frame.Height/6);


        spritebatch.Draw(texture, Position + settings.AnchorPosition, frame, drawColor, Rotation, texture.Size() * 0.5f, new Vector2(widthScale, heightScale), (SpriteEffects)SpriteEffect, 0);


        // Draw the particle with the adjusted scale
       // spritebatch.Draw(texture, Position + settings.AnchorPosition, glowFrame, glowColor, Rotation + MathHelper.PiOver2, glowFrame.Size() * 0.5f, Scale, (SpriteEffects)SpriteEffect, 0);
    }

}