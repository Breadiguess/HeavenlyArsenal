using HeavenlyArsenal.Core;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Particles;

public class BleedingBurstParticle : BaseParticle
{
    public static ParticlePool<BleedingBurstParticle> pool = new(500, GetNewParticle<BleedingBurstParticle>);

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
        Velocity = velocity;
        Rotation = velocity.ToRotation() + rotation;
        ColorTint = color;
        MaxTime = 7 + (int)(15 / Math.Clamp(scale, 0.1f, 10f));
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
        Position += Velocity * (1f - MathF.Cbrt((float)TimeLeft / MaxTime)) * 0.5f;

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
        shader.TrySetParameter("uProgressInside", Utils.GetLerpValue(0.4f, 1f, progress, true));
        shader.TrySetParameter("uNoiseOffset", Offset / 24f);
        shader.TrySetParameter("uOffset", Velocity);
        shader.TrySetParameter("uNoiseStrength", 2f - progress * 1.5f);
        shader.TrySetParameter("useDissolve", true);
        shader.SetTexture(texture, 0, SamplerState.PointWrap);
        shader.SetTexture(GennedAssets.Textures.Noise.FireNoiseA, 1, SamplerState.PointWrap);
        shader.Apply();

        var stretch = Scale * MathF.Cbrt(progress) * new Vector2(300f) / texture.Size();
        Main.spriteBatch.Draw(texture, Position + settings.AnchorPosition, texture.Frame(), ColorTint, Rotation, texture.Size() * 0.5f, stretch, 0, 0);
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        var glow = AssetDirectory.Textures.BigGlowball.Value;

        Main.spriteBatch.Draw
        (
            glow,
            Position + settings.AnchorPosition,
            glow.Frame(),
            ColorTint with
            {
                A = 0
            } *
            0.33f,
            Rotation,
            glow.Size() * 0.5f,
            Scale * MathF.Cbrt(progress) * 0.5f,
            0,
            0
        );
    }
}