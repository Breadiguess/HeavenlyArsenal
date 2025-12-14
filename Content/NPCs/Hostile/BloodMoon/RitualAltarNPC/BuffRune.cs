using HeavenlyArsenal.Core;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;

internal class BuffRune : BaseParticle
{
    public static ParticlePool<BuffRune> pool = new(500, GetNewParticle<BuffRune>);

    public int TimeLeftMax;

    public int TimeLeft;

    public Vector2 position;

    public Vector2 StartPos;

    public int Variation;

    public float opacity;

    public void Prepare(Vector2 Pos, int Variation, int maxTime)
    {
        position = Pos;
        StartPos = Pos;
        TimeLeftMax = maxTime;
        this.Variation = Variation;
        TimeLeft = 0;
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        TimeLeft = 0;
        opacity = 0;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        TimeLeft++;

        // Fraction of total lifetime
        var progress = TimeLeft / (float)TimeLeftMax;

        // Fade in at start, fade out near the end
        if (progress < 0.3f)
        {
            opacity = MathHelper.Lerp(opacity, 1f, 0.15f); // smooth fade-in
        }
        else if (progress > 0.8f)
        {
            opacity = MathHelper.Lerp(opacity, 0f, 0.2f); // rapid fade-out
        }
        else
        {
            opacity = 1f;
        }

        // Float upward smoothly
        position.Y -= 0.5f * (1f - progress); // slows as it rises

        if (TimeLeft >= TimeLeftMax)
        {
            ShouldBeRemovedFromRenderer = true;
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        var Tex = AssetDirectory.Textures.Particles.RuneParticle.Value;
        var DrawPos = position - Main.screenPosition;
        var Scale = Vector2.One;

        var FrameOuter = Tex.Frame(2, 6, 1, Variation);
        var FrameInner = Tex.Frame(2, 6, 0, Variation);

        var color = Color.Lerp(Color.AntiqueWhite, Color.Crimson, 0.86f) * opacity;

        for (var i = 0; i < 6; i++)
        {
            var rotation = (float)i / 6 * MathHelper.TwoPi;

            Main.EntitySpriteDraw
            (
                Tex,
                DrawPos + new Vector2(4, 0).RotatedBy(rotation + Main.GlobalTimeWrappedHourly * 10.1f),
                FrameOuter,
                color with
                {
                    A = 0
                } *
                0.75f,
                0f,
                FrameOuter.Size() * 0.5f,
                Scale * 1f,
                SpriteEffects.None
            );
        }

        Main.EntitySpriteDraw
        (
            Tex,
            DrawPos,
            FrameInner,
            Color.AntiqueWhite * opacity,
            0f,
            FrameInner.Size() * 0.5f,
            Scale,
            SpriteEffects.None
        );
    }
}