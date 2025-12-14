using HeavenlyArsenal.Core;
using NoxusBoss.Assets;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Particles;

internal class BlindingLight_hitVFX : BaseParticle
{
    public static ParticlePool<BlindingLight_hitVFX> pool = new(500, GetNewParticle<BlindingLight_hitVFX>);

    public Vector2 position;

    public Vector2 Velocity;

    public float Rotation;

    public float progress;

    public int MaxTime;

    public int TimeLeft;

    public float Scale;

    public Color GlowColor;

    public void Prepare(Vector2 position, Vector2 velocity, float rotation, int lifeTime, float scale, Color glowColor)
    {
        this.position = position;
        Velocity = velocity;
        Rotation = velocity.ToRotation() + rotation;
        MaxTime = lifeTime;
        Scale = scale;
        GlowColor = glowColor;
        progress = 0;
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
        Velocity *= 0.5f;
        position += Velocity;
        progress = float.Lerp(progress, 1, 0.25f);
        Rotation = Velocity.ToRotation();

        if (progress >= 1)
        {
            ShouldBeRemovedFromRenderer = true;
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        Texture2D tex = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;
        Texture2D tex2 = GennedAssets.Textures.GreyscaleTextures.Star;

        var DrawPos = position - Main.screenPosition;
        var Origin = tex.Size() * 0.5f;
        var Origin2 = new Vector2(tex.Width / 2, 0);
        var Rot = Rotation;
        var scale = Scale * (1 - progress);

        var AdjustedColor = GlowColor with
        {
            A = 0
        };

        var AdjustedScale = new Vector2(scale, scale) * 0.25f;
        Main.EntitySpriteDraw(tex, DrawPos, null, AdjustedColor, Rot, Origin, scale, SpriteEffects.None);
        Main.EntitySpriteDraw(tex2, DrawPos, null, AdjustedColor, Rot + MathHelper.PiOver2, Origin2, AdjustedScale, SpriteEffects.None);
    }
}