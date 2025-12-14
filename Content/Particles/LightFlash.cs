using HeavenlyArsenal.Core;
using NoxusBoss.Assets;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Particles;

internal class LightFlash : BaseParticle
{
    public static ParticlePool<LightFlash> pool = new(500, GetNewParticle<LightFlash>);

    public bool AltTexture;

    public int altTexFrame;

    public Vector2 position;

    public Vector2 Velocity;

    public float Rotation;

    public float progress;

    public int MaxTime;

    public int TimeLeft;

    public float Scale;

    public Color GlowColor;

    public void Prepare(Vector2 position, Vector2 velocity, float rotation, int lifeTime, float scale, Color glowColor, bool CollapseExplosion = false)
    {
        this.position = position;
        Velocity = velocity;
        Rotation = velocity.ToRotation() + rotation;
        MaxTime = lifeTime;
        Scale = scale;
        GlowColor = glowColor;
        progress = 0;
        AltTexture = CollapseExplosion;
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        Velocity = Vector2.Zero;
        MaxTime = 40;
        TimeLeft = 0;
        altTexFrame = -1;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        if (altTexFrame == -1)
        {
            altTexFrame = Main.rand.Next(0, 2);
        }

        position += Velocity;
        Velocity *= 0.8f;
        progress = float.Lerp(progress, 20, 0.2f);

        GlowColor *= 1.1f;
        TimeLeft++;

        if (TimeLeft > MaxTime)
        {
            ShouldBeRemovedFromRenderer = true;
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        Texture2D Spire = GennedAssets.Textures.GreyscaleTextures.ChromaticSpires;

        Texture2D Star = !AltTexture ? GennedAssets.Textures.GreyscaleTextures.FourPointedStar : GennedAssets.Textures.GreyscaleTextures.RadialFlare;

        Texture2D texture = GennedAssets.Textures.GreyscaleTextures.ChromaticBurst;

        Texture2D Glowtex = GennedAssets.Textures.GreyscaleTextures.BloomFlare;

        var DrawPos = position - Main.screenPosition;
        var flip = SpriteEffects.None;
        var GlowSize = new Vector2(0.05f, 0.05f) * Scale * (float)Math.Abs(1 + Math.Cos(TimeLeft / 60)); // * (1f + progress * 0.5f) * 0.05f;

        var Size = new Vector2(0.055f) * progress * Scale;

        var GlowOrigin = Spire.Size() * 0.5f;
        var TexOrigin = texture.Size() * 0.5f;

        var StarRect = !AltTexture ? Star.Frame() : Star.Frame(1, 2, 0, altTexFrame);

        var starOrigin = !AltTexture ? Star.Size() * 0.5f : new Vector2(Star.Width / 2, StarRect.Height / 2);

        var Adjusted = GlowColor with
                       {
                           A = 0
                       } *
                       (20 - progress);

        Main.spriteBatch.Draw(Glowtex, DrawPos, Glowtex.Frame(), Adjusted, Rotation, Glowtex.Size() * 0.5f, Size * 0.6f, flip, 0);
        var StarSize = !AltTexture ? Size : Size * 0.6f;

        Main.spriteBatch.Draw(Spire, DrawPos, Spire.Frame(), Adjusted, Rotation, GlowOrigin, Size, flip, 0);

        Main.spriteBatch.Draw(texture, DrawPos, texture.Frame(), Adjusted, Rotation, TexOrigin, Size * 0.5f, flip, 0);

        Main.spriteBatch.Draw(Star, DrawPos, StarRect, Adjusted, Rotation + MathHelper.ToRadians(90), starOrigin, StarSize, flip, 0);
    }
}