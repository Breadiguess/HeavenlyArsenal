
using HeavenlyArsenal.Core;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Particles;

public class HeatLightning : BaseParticle
{
    public static ParticlePool<HeatLightning> pool = new ParticlePool<HeatLightning>(500, GetNewParticle<HeatLightning>);

    public Vector2 Position;
    public Vector2 Velocity;
    public float Rotation;
    public int MaxTime;
    public int TimeLeft;
    public float Scale;
    private int Style;
    private int SpriteEffect;
    private bool Flickering;
    private float FlickerAmount;

    public void Prepare(Vector2 position, Vector2 velocity, float rotation, int lifeTime, float scale)
    {
        Position = position;
        Velocity = velocity;
        Rotation = rotation;
        MaxTime = lifeTime;
        Scale = scale;
        Style = Main.rand.Next(10);
        SpriteEffect = Main.rand.Next(2);  
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        Velocity = Vector2.Zero;
        MaxTime = 40;
        TimeLeft = 0;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        Position += Velocity;
        Velocity *= 0.7f;

        if (Main.rand.NextBool(3))
        {
            SpriteEffect = Main.rand.Next(2);
            Flickering = Main.rand.NextBool();
            Style = Main.rand.Next(10);
        }

        FlickerAmount = Main.rand.NextFloat();

        TimeLeft++;
        if (TimeLeft > MaxTime)
            ShouldBeRemovedFromRenderer = true;
    }
  
    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        Texture2D texture = AssetDirectory.Textures.HeatLightning.Value;
        Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;

        Rectangle frame = texture.Frame(1, 10, 0, Style);
        SpriteEffects flip = SpriteEffect > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
        float progress = (float)TimeLeft / MaxTime;
        Color drawColor = Color.Lerp(Color.White with { A = 70 }, Color.DarkRed with { A = 50 }, Utils.GetLerpValue(MaxTime / 2f, MaxTime / 1.2f, TimeLeft, true));

        if (Flickering)
        {
            drawColor = Color.Lerp(Color.White with { A = 0 }, Color.RoyalBlue with { A = 50 }, FlickerAmount);
        }

        Main.spriteBatch.Draw(glow, Position - Main.screenPosition, glow.Frame(), Color.DarkRed with { A = 30 } * 0.2f, Rotation, glow.Size() * 0.5f, Scale * (1f + progress * 0.5f) * 0.15f, flip, 0);
        Main.spriteBatch.Draw(texture, Position - Main.screenPosition, frame, drawColor, Rotation, frame.Size() * 0.5f, Scale * new Vector2(1f, 1f + progress * FlickerAmount) * 0.5f, flip, 0);
    }
}