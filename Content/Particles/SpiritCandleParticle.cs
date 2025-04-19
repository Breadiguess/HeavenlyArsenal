using HeavenlyArsenal.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Particles;

public class SpiritCandleParticle : BaseParticle
{
    /// <summary>
    /// The particle pool responsible for candle particles.
    /// </summary>
    public static ParticlePool<SpiritCandleParticle> Pool
    {
        get;
        private set;
    } = new ParticlePool<SpiritCandleParticle>(128, GetNewParticle<SpiritCandleParticle>);

    /// <summary>
    /// The position of this candle.
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// The velocity of this candle.
    /// </summary>
    public Vector2 Velocity;

    /// <summary>
    /// The rotation of this candle.
    /// </summary>
    public float Rotation;

    /// <summary>
    /// The base color of this candle.
    /// </summary>
    public Color Color;

    /// <summary>
    /// How long this candle has existed for, in frames.
    /// </summary>
    public int Time;

    /// <summary>
    /// The scale of this candle.
    /// </summary>
    public Vector2 Scale;

    /// <summary>
    /// The base scale of this candle.
    /// </summary>
    public Vector2 BaseScale;

    private static readonly Asset<Texture2D> candleTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Particles/SpiritCandle");

    public void Prepare(Vector2 position, Vector2 velocity, float rotation, Color color, Vector2 scale)
    {
        Position = position;
        Velocity = velocity;
        Rotation = rotation;
        Color = color;
        BaseScale = scale;
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        Time = 0;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        Position += Velocity;

        float squishRate = 54f;
        float squishWave = MathF.Sin(MathHelper.TwoPi * Time / squishRate);
        float horizontalSquish = MathF.Pow(squishWave * 0.5f + 0.5f, 2.3f) * 0.75f;
        float baseHorizontalSquish = horizontalSquish;
        horizontalSquish -= LumUtils.InverseLerp(0.4f, 0f, horizontalSquish) * 0.55f;

        Scale = new Vector2(1f + horizontalSquish, 1f - horizontalSquish) * BaseScale;
        Rotation = MathF.Sin(MathHelper.Pi * Time / squishRate) * 0.2f;
        Velocity = new Vector2(Rotation * -12f, squishWave * -2f);

        if (baseHorizontalSquish <= 0.16f && Main.rand.NextBool())
        {
            Vector2 fireSpawnPosition = Position - Vector2.UnitY.RotatedBy(Rotation) * Scale.Y * 84f;
            Vector2 fireVelocity = Vector2.UnitY.RotatedBy(Rotation).RotatedByRandom(0.3f) * Main.rand.NextFloat(-4f, -2.4f);
            Dust ember = Dust.NewDustPerfect(fireSpawnPosition, 264, fireVelocity);
            ember.color = Color.Lerp(Color.Orange, Color.Red, Main.rand.NextFloat(0.65f));
            ember.noGravity = Main.rand.NextBool();
            ember.scale *= Main.rand.NextFloat(0.67f, 1.33f);
        }

        Time++;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        Texture2D texture = candleTexture.Value;
        float lightExposureFactor = Utils.Remap(Scale.Y / BaseScale.Y, 0f, 1.2f, 0.75f, 1.5f);
        Color light = Lighting.GetColor(Position.ToTileCoordinates()) * lightExposureFactor;
        Main.spriteBatch.Draw(texture, Position + settings.AnchorPosition, texture.Frame(), Color.MultiplyRGB(light), Rotation, texture.Size() * new Vector2(0.5f, 1f), Scale, 0, 0);

        Vector2 glowDrawPosition = Position + settings.AnchorPosition - Vector2.UnitY.RotatedBy(Rotation) * Scale.Y * 84f;
        float glowFlicker = MathHelper.Lerp(0.9f, 1.1f, LumUtils.Cos01(Main.GlobalTimeWrappedHourly * 30f + Position.X * 0.01f)) * LumUtils.Saturate(Scale.Y * 2.7f);
        Texture2D glow = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint.Value;
        Vector2 glowOrigin = glow.Size() * 0.5f;
        Main.spriteBatch.Draw(glow, glowDrawPosition, null, new Color(1f, 0.97f, 0.9f, 0f) * 0.9f, Rotation, glowOrigin, new Vector2(0.5f, Scale.Y * 2.5f) * glowFlicker * 0.2f, 0, 0f);
        Main.spriteBatch.Draw(glow, glowDrawPosition, null, new Color(1f, 0.95f, 0.4f, 0f) * 0.6f, Rotation, glowOrigin, glowFlicker * 0.4f, 0, 0f);
        Main.spriteBatch.Draw(glow, glowDrawPosition, null, new Color(1f, 0.61f, 0.2f, 0f) * 0.4f, Rotation, glowOrigin, glowFlicker * 0.7f, 0, 0f);
    }
}
