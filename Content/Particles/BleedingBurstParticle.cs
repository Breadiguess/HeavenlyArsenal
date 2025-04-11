using HeavenlyArsenal.Core;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Renderers;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;

namespace HeavenlyArsenal.Content.Particles;

public class BleedingBurstParticle : BaseParticle
{
    public static ParticlePool<BleedingBurstParticle> pool = new ParticlePool<BleedingBurstParticle>(500, GetNewParticle<BleedingBurstParticle>);

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
        MaxTime = 5 + (int)(20 / Math.Clamp(scale, 0.1f, 10f));
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
            ShouldBeRemovedFromRenderer = true;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        Texture2D texture = GennedAssets.Textures.Noise.FireNoiseA.Value;
        float progress = (float)TimeLeft / MaxTime;
        ManagedShader shader = ShaderManager.GetShader("HeavenlyArsenal.RadialBlastEffect");
        shader.TrySetParameter("uProgress", progress);
        shader.TrySetParameter("uProgressInside", Utils.GetLerpValue(0.4f, 1f, progress, true));
        shader.TrySetParameter("uNoiseOffset", Offset / 24f);
        shader.TrySetParameter("uOffset", Velocity);
        shader.TrySetParameter("uNoiseStrength", 2f - progress * 1.5f);
        shader.TrySetParameter("useDissolve", true);
        shader.SetTexture(texture, 0, SamplerState.PointWrap);
        shader.SetTexture(GennedAssets.Textures.Noise.FireNoiseA, 1, SamplerState.PointWrap);
        shader.Apply();

        Vector2 stretch = Scale * MathF.Cbrt(progress) * new Vector2(300f) / texture.Size();
        Main.spriteBatch.Draw(texture, Position + settings.AnchorPosition, texture.Frame(), ColorTint, Rotation, texture.Size() * 0.5f, stretch, 0, 0);
    }
}
