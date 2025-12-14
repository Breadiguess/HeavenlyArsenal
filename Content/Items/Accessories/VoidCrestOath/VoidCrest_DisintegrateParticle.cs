using HeavenlyArsenal.Core;
using NoxusBoss.Assets;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath;

internal class VoidCrest_DisintegrateParticle : BaseParticle
{
    public static ParticlePool<VoidCrest_DisintegrateParticle> pool = new(500, GetNewParticle<VoidCrest_DisintegrateParticle>);

    public Vector2 Position;

    public Vector2 Velocity;

    public float Rotation;

    public int MaxTime;

    public int TimeLeft;

    public float Scale;

    public float Opacity;

    public int Frame;

    public void Prepare(Vector2 position, Vector2 velocity, float rotation, int lifeTime)
    {
        Position = position;
        Velocity = velocity;
        Rotation = rotation;
        MaxTime = lifeTime;
        Scale = 0;
        Opacity = 0;
        Frame = Main.rand.Next(0, 30);
    }

    public override void FetchFromPool()
    {
        base.FetchFromPool();
        Velocity = Vector2.Zero;
        MaxTime = 1;
        TimeLeft = 0;
    }

    public override void Update(ref ParticleRendererSettings settings)
    {
        Scale = float.Lerp(Scale, 1, 0.15f);

        if (TimeLeft < MaxTime / 2)
        {
            Opacity = float.Lerp(Opacity, 1, 0.2f);
        }
        else
        {
            Opacity = float.Lerp(Opacity, 0, 0.2f);
        }

        TimeLeft++;

        if (TimeLeft > MaxTime)
        {
            ShouldBeRemovedFromRenderer = true;
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        Texture2D Glow = GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall;
        var texture = AssetDirectory.Textures.Items.Accessories.VoidCrestOath.VoidSigil.Value;
        var texture2 = AssetDirectory.Textures.Items.Accessories.VoidCrestOath.VoidSigil2.Value;
        var DrawPos = Position - Main.screenPosition;

        var frame = texture.Frame(1, 30, 0, Frame);
        var frame2 = texture2.Frame(1, 30, 0, Frame);

        var Origin = new Vector2(frame.Width / 2, frame.Height / 2);
        var Origin2 = new Vector2(frame2.Width / 2, frame2.Height / 2);

        var scale = new Vector2(5) * (1 - Scale);

        var BaseColor = Color.BlueViolet with
        {
            A = 0
        };

        //new Color(0, 0,Color.Aqua.B) with { A = 0 };
        var A = BaseColor * Opacity;

        var B = BaseColor * Opacity;
        var Rot = MathHelper.ToRadians(90) * 10 * (1 - Scale);

        Main.EntitySpriteDraw(Glow, DrawPos, null, BaseColor, Rot, Glow.Size() * 0.5f, scale * 0.37f, SpriteEffects.None);

        //Main.EntitySpriteDraw(texture, DrawPos, frame, A, Rot, Origin, scale, SpriteEffects.None);

        Main.EntitySpriteDraw(texture2, DrawPos, frame, B, Rot, Origin, scale, SpriteEffects.None);
    }
}