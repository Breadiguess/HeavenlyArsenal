
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

    public void Prepare(Vector2 position, Vector2 velocity, float rotation, int lifeTime, Color color, Color glowColor, float scale)
    {
        Position = position;
        Velocity = velocity;
        Rotation = rotation;
        MaxTime = lifeTime;
        ColorTint = color;
        ColorGlow = glowColor;
        Scale = scale;
        Style = Main.rand.Next(3);
        SpriteEffect = Main.rand.Next(2);
        Main.NewText($"AvatarRifle_Muzzleflash Drawn!", Color.AntiqueWhite);
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
        Position += Velocity;
        Velocity *= 0.85f;

        TimeLeft++;
        if (TimeLeft > MaxTime)
            ShouldBeRemovedFromRenderer = true;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Particles/FirstParticle").Value;


        float progress = (float)TimeLeft / MaxTime;
        int frameCount = (int)MathF.Floor(MathF.Sqrt(progress) * 7);
        Rectangle frame = texture.Frame(7, 6, frameCount, Style);
        Rectangle glowFrame = texture.Frame(7, 6, frameCount, Style + 3);
        Color drawColor = Color.Lerp(ColorTint, ColorGlow, Utils.GetLerpValue(0.3f, 0.7f, progress, true)) * Utils.GetLerpValue(1f, 0.9f, progress, true);
        Color glowColor = ColorGlow * Utils.GetLerpValue(1f, 0.5f, progress, true);
        
        spritebatch.Draw(texture, Position + settings.AnchorPosition, frame, drawColor, Rotation + MathHelper.PiOver2, frame.Size() * 0.5f, Scale, (SpriteEffects)SpriteEffect, 0);
        spritebatch.Draw(texture, Position + settings.AnchorPosition, glowFrame, glowColor, Rotation + MathHelper.PiOver2, glowFrame.Size() * 0.5f, Scale, (SpriteEffects)SpriteEffect, 0);
    }
}