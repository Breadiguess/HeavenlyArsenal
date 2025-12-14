using HeavenlyArsenal.Core;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Particles;

internal class FusionReaction : BaseParticle
{
    public static ParticlePool<FusionReaction> pool = new(500, GetNewParticle<FusionReaction>);

    public void Prepare() { }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        ShouldBeRemovedFromRenderer = true;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) { }
}