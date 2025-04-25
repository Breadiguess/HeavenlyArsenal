using HeavenlyArsenal.Content.Subworlds;
using HeavenlyArsenal.Core;
using Luminance.Common.Utilities;
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
    public enum AIType
    {
        Bounce,
        DanceInAir
    }

    /// <summary>
    /// The particle pool responsible for candle particles.
    /// </summary>
    public static ParticlePool<SpiritCandleParticle> Pool
    {
        get;
        private set;
    } = new ParticlePool<SpiritCandleParticle>(512, GetNewParticle<SpiritCandleParticle>);

    /// <summary>
    /// A unique time offset value used for flickers to ensure variance.
    /// </summary>
    public float FlickerTimeOffset
    {
        get;
        set;
    }

    /// <summary>
    /// The type of behavior that this candle should use.
    /// </summary>
    public AIType Behavior
    {
        get;
        set;
    }

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
        FlickerTimeOffset = Main.rand.NextFloat();
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        Time = 0;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        Position += Velocity;

        if (Behavior == AIType.Bounce)
        {
            float squishRate = 54f;
            float squishWave = MathF.Sin(MathHelper.TwoPi * Time / squishRate);
            float horizontalSquish = MathF.Pow(squishWave * 0.5f + 0.5f, 2.3f) * 0.75f;
            horizontalSquish -= LumUtils.InverseLerp(0.4f, 0f, horizontalSquish) * 0.55f;
            horizontalSquish *= 0.05f;

            Scale = new Vector2(1f + horizontalSquish, 1f - horizontalSquish) * BaseScale;
            Velocity = Vector2.UnitY * squishWave * -0.7f;
        }
        else if (Behavior == AIType.DanceInAir)
        {
            Scale = BaseScale;
            float timeOffset = MathHelper.TwoPi * FlickerTimeOffset;
            float squishRate = 54f;
            float spinRate = 127f;
            float squishWave = MathF.Sin(MathHelper.TwoPi * Time / squishRate + timeOffset);
            float horizontalSquish = MathF.Pow(squishWave * 0.5f + 0.5f, 2.3f) * 0.75f;
            horizontalSquish -= LumUtils.InverseLerp(0.4f, 0f, horizontalSquish) * 0.55f;
            horizontalSquish *= 0.225f;

            float spin = MathF.Sin(MathHelper.TwoPi * Time / spinRate + timeOffset);

            Scale = new Vector2(1f + horizontalSquish, 1f - horizontalSquish) * BaseScale;
            Velocity = new Vector2(spin * 1.12f, squishWave * -0.9f);
            Rotation = spin * 0.18f + squishWave * 0.04f;
        }

        Time++;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        if (!Main.LocalPlayer.WithinRange(Position, 3000f))
            return;

        Texture2D texture = candleTexture.Value;
        float lightExposureFactor = Utils.Remap(Scale.Y / BaseScale.Y, 0f, 1.2f, 0.75f, 1.5f);
        Color light = Lighting.GetColor(Position.ToTileCoordinates()) * lightExposureFactor;
        Main.spriteBatch.Draw(texture, Position + settings.AnchorPosition, texture.Frame(), Color.MultiplyRGB(light), Rotation, texture.Size() * new Vector2(0.5f, 1f), Scale, 0, 0);

        Vector2 glowDrawPosition = Position + settings.AnchorPosition - Vector2.UnitY.RotatedBy(Rotation) * Scale.Y * 38f;
        float glowFlicker = MathHelper.Lerp(0.9f, 1f, LumUtils.Cos01(Main.GlobalTimeWrappedHourly * 20f + MathHelper.TwoPi * FlickerTimeOffset)) * LumUtils.Saturate(1f - (1f - Scale.Y / BaseScale.Y) * 3.3f) * 0.7f;
        Texture2D glow = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint.Value;
        Vector2 glowOrigin = glow.Size() * 0.5f;
        Main.spriteBatch.Draw(glow, glowDrawPosition, null, new Color(1f, 0.97f, 0.1f, 0f) * 0.9f, Rotation, glowOrigin, new Vector2(0.5f, Scale.Y * 1.2f) * glowFlicker * 0.2f, 0, 0f);
        Main.spriteBatch.Draw(glow, glowDrawPosition, null, new Color(1f, 0.75f, 0.1f, 0f) * 0.6f, Rotation, glowOrigin, glowFlicker * 0.4f, 0, 0f);
        Main.spriteBatch.Draw(glow, glowDrawPosition, null, new Color(1f, 0.41f, 0.1f, 0f) * 0.4f, Rotation, glowOrigin, glowFlicker * 0.7f, 0, 0f);

        ForgottenShrineDarknessSystem.QueueGlowAction(() =>
        {
            Main.spriteBatch.Draw(glow, glowDrawPosition, null, new Color(1f, 0.8f, 0.4f, 0f), Rotation, glowOrigin, glowFlicker * 1.1f, 0, 0f);
        });
    }
}
