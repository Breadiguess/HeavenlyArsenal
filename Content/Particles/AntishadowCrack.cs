using HeavenlyArsenal.Core;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Particles;

public class AntishadowCrack : BaseParticle
{
    public static ParticlePool<AntishadowCrack> pool = new(500, GetNewParticle<AntishadowCrack>);

    public Vector2 position;

    public Vector2 Velocity;

    public float Rotation;

    public int MaxTime;

    public int TimeLeft;

    public Color ColorTint;

    public Color ColorGlow;

    public float Scale;

    private int Style;

    private int SpriteEffect;

    public float direction { get; private set; }

    public void Prepare(Vector2 position, Vector2 velocity, float rotation, int lifeTime, Color color, Color glowColor, float scale)
    {
        position = position;
        Velocity = velocity;
        Rotation = rotation;
        MaxTime = lifeTime;
        ColorTint = color;
        ColorGlow = glowColor;
        Scale = scale;
        Style = Main.rand.Next(3);
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
        Velocity += new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-0.1f, 0.1f));
        Velocity *= 1.1f;

        TimeLeft++;

        if (TimeLeft > MaxTime)
        {
            ShouldBeRemovedFromRenderer = true;
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
    {
        var texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Particles/LightningParticle").Value;
        var texture2 = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Extra/VoidLake").Value;

        var frame = texture.Frame(1, 10, 0, Style);
        var flip = direction > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
        var flickerSpeed = 1;
        var drawColor = ColorTint * (0.8f + MathF.Sin(TimeLeft * flickerSpeed) * 0.2f);
        Vector2 position = default;

        var dissolveEffect = AssetDirectory.Effects.FlameDissolve.Value;
        dissolveEffect.Parameters["uTexture0"].SetValue(texture);
        dissolveEffect.Parameters["uTextureScale"].SetValue(new Vector2(0.7f + 1 * 0.05f));
        dissolveEffect.Parameters["uFrameCount"].SetValue(10);
        dissolveEffect.Parameters["uProgress"].SetValue(Utils.GetLerpValue(MaxTime / 3f, MaxTime, TimeLeft, true));
        dissolveEffect.Parameters["uPower"].SetValue(4f + Utils.GetLerpValue(MaxTime / 4f, MaxTime / 3f, TimeLeft, true) * 40f);
        dissolveEffect.Parameters["uNoiseStrength"].SetValue(1f);
        dissolveEffect.CurrentTechnique.Passes[0].Apply();

        var rotation = 0;

        Main.spriteBatch.Draw
            (texture, position - Main.screenPosition, frame, drawColor, rotation + MathHelper.Pi / 3f * direction, frame.Size() * 0.5f, Scale * new Vector2(1f, 1f + TimeLeft * 0.05f) * 0.5f, flip, 0);

        Main.pixelShader.CurrentTechnique.Passes[0].Apply();

        //float alpha = 1f - progress;

        // Apply the alpha value to the draw color
        //Color drawColor = Color.Lerp(ColorTint, ColorGlow, Utils.GetLerpValue(0.3f, 0.7f, progress, true)) * Utils.GetLerpValue(1f, 0.9f, progress, true) * alpha;

        // Adjust the scale based on the progress
        //float widthScale = Scale * (1f - progress); // Decrease the width over time
        //float heightScale = Scale; // Keep the height constant

        //Vector2 anchorPosition = new Vector2(frame.Width / 2, frame.Height);

        // Draw the particle with the adjusted scale
        //spritebatch.Draw(texture, position + settings.AnchorPosition, texture.Frame(), drawColor, Rotation, texture.Size() * 0.5f, new Vector2(widthScale, heightScale), (SpriteEffects)SpriteEffect, 0);
        // spritebatch.Draw(texture, position + settings.AnchorPosition, glowFrame, glowColor, Rotation + MathHelper.PiOver2, glowFrame.Size() * 0.5f, Scale, (SpriteEffects)SpriteEffect, 0);
    }
}