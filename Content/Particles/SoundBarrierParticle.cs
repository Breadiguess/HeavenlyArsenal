using HeavenlyArsenal.Core;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Particles;

public class SoundBarrierParticle : BaseParticle
{
    public static ParticlePool<SoundBarrierParticle> pool = new(500, GetNewParticle<SoundBarrierParticle>);

    public Vector2 Position;

    public Vector2 Velocity;

    public float Rotation;

    public Color ColorTint;

    public int MaxTime;

    public int TimeLeft;

    public float Scale;

    private int Style;

    private Vector2 Offset;

    public void Prepare(Vector2 position, Vector2 velocity, float rotation, Color color, float scale)
    {
        Position = position;
        Velocity = velocity * 3f;
        Rotation = rotation;
        ColorTint = color;
        MaxTime = 7 + (int)(5 * scale);
        Scale = scale;
        Offset = Main.rand.NextVector2Circular(10f, 10f);
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        TimeLeft = 0;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        Position += Velocity;
        Velocity *= 0.55f;

        if (++TimeLeft > MaxTime)
        {
            ShouldBeRemovedFromRenderer = true;
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        var texture = GennedAssets.Textures.Noise.FireNoiseA.Value;
        var progress = (float)TimeLeft / MaxTime;
        var shader = ShaderManager.GetShader("HeavenlyArsenal.RadialBlastEffect");
        shader.TrySetParameter("uProgress", progress);
        shader.TrySetParameter("uProgressInside", progress);
        shader.TrySetParameter("uNoiseOffset", Offset / 24f);
        shader.TrySetParameter("uOffset", new Vector2(-0.1f, 0f));
        shader.TrySetParameter("uNoiseStrength", 1.5f - progress);
        shader.TrySetParameter("useDissolve", false);
        shader.SetTexture(texture, 0, SamplerState.PointWrap);
        shader.SetTexture(GennedAssets.Textures.Noise.WatercolorNoiseA, 1, SamplerState.PointWrap);
        shader.Apply();

        var stretch = Scale * MathF.Cbrt(progress) * new Vector2(70f, 360f) / texture.Size();
        Main.spriteBatch.Draw(texture, Position + settings.AnchorPosition, texture.Frame(), ColorTint, Rotation, texture.Size() * 0.5f, stretch, 0, 0);
    }
}