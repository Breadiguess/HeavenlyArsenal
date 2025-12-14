using HeavenlyArsenal.Core;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Particles;

public class HeatLightning : BaseParticle
{
    public static ParticlePool<HeatLightning> pool = new(500, GetNewParticle<HeatLightning>);

    public Vector2 position;

    public Vector2 Velocity;

    public float Rotation;

    public int MaxTime;

    public int TimeLeft;

    public float Scale;

    private int Style;

    private int SpriteEffect;

    private bool Flickering;

    private float FlickerAmount;

    public void Prepare(Vector2 position, Vector2 velocity, float rotation, int lifeTime, float scale)
    {
        this.position = position;
        Velocity = velocity;
        Rotation = velocity.ToRotation() + rotation;
        MaxTime = lifeTime;
        Scale = scale;
        Style = Main.rand.Next(10);
        SpriteEffect = Main.rand.Next(2);
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
        position += Velocity;
        Velocity *= 0.8f;

        if (Main.rand.NextBool(3))
        {
            SpriteEffect = Main.rand.Next(2);
            Flickering = Main.rand.NextBool();
            Style = Main.rand.Next(10);
        }

        FlickerAmount = Main.rand.NextFloat();

        TimeLeft++;

        if (TimeLeft > MaxTime)
        {
            ShouldBeRemovedFromRenderer = true;
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        var texture = AssetDirectory.Textures.HeatLightning.Value;
        var glow = AssetDirectory.Textures.BigGlowball.Value;

        var frame = texture.Frame(1, 10, 0, Style);
        var flip = SpriteEffect > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
        var progress = (float)TimeLeft / MaxTime;

        var drawColor = Color.Lerp
        (
            Color.White with
            {
                A = 70
            },
            Color.DarkRed with
            {
                A = 30
            },
            Utils.GetLerpValue(MaxTime / 2f, MaxTime / 1.2f, TimeLeft, true)
        );

        if (Flickering)
        {
            drawColor = Color.Lerp
            (
                Color.LightGoldenrodYellow with
                {
                    A = 0
                },
                Color.RoyalBlue with
                {
                    A = 200
                },
                FlickerAmount
            );
        }

        Main.spriteBatch.Draw
        (
            glow,
            position - Main.screenPosition,
            glow.Frame(),
            Color.DarkRed with
            {
                A = 30
            } *
            0.2f,
            Rotation,
            glow.Size() * 0.5f,
            Scale * (1f + progress * 0.5f) * 0.15f,
            flip,
            0
        );

        Main.spriteBatch.Draw(texture, position - Main.screenPosition, frame, drawColor, Rotation, frame.Size() * 0.5f, Scale * new Vector2(1f, 1f + progress * FlickerAmount) * 0.5f, flip, 0);
    }
}