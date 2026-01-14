using HeavenlyArsenal.Core;
using NoxusBoss.Assets;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Summoner
{
    internal class BloodOvermind_Particle : BaseParticle
    {

        public static ParticlePool<BloodOvermind_Particle> pool = new(500, GetNewParticle<BloodOvermind_Particle>);

        public Vector2 position;

        public Vector2 Velocity;

        public float Rotation;

        public int MaxTime;

        public int TimeLeft;

        public Color ColorTint;

        public Color ColorGlow;

        public float Scale;

        public float direction { get; private set; }

        public void Prepare(Vector2 position, Vector2 velocity, float rotation, int lifeTime, Color color, Color glowColor, float scale)
        {
            this.position = position;
            Velocity = velocity;
            Rotation = rotation;
            MaxTime = lifeTime;
            ColorTint = color;
            ColorGlow = glowColor;
            Scale = scale;
        }

        public override void FetchFromPool()
        {
            base.FetchFromPool();
            Velocity = Vector2.Zero;
            MaxTime = 30;
            TimeLeft = 0;
        }

        public override void Update(ref ParticleRendererSettings settings)
        {
            MaxTime = 30;
            position += Velocity;
            Velocity += new Vector2(Main.rand.NextFloat(-0.1f, 0.1f) * 10, Main.rand.NextFloat(-0.1f, 0.1f));
            Velocity *= 0.95f;

            TimeLeft++;
            ColorTint = Color.Lerp(ColorTint, Color.WhiteSmoke, 0.1f);
            if (TimeLeft > MaxTime)
            {
                ShouldBeRemovedFromRenderer = true;
            }
        }

        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            var texture = AssetDirectory.Textures.BigGlowball.Value;
            Texture2D noise = GennedAssets.Textures.Noise.BurnNoise;
            var frame = texture.Frame(1, 1, 0, 0);
            var flickerSpeed = 1;
            var drawColor = ColorTint * (0.8f + MathF.Sin(TimeLeft * flickerSpeed) * 0.2f);

            var dissolveEffect = AssetDirectory.Effects.FlameDissolve.Value;
            dissolveEffect.Parameters["uTexture0"].SetValue(noise);
            dissolveEffect.Parameters["uTextureScale"].SetValue(new Vector2(1));
            dissolveEffect.Parameters["uFrameCount"].SetValue(1);
            dissolveEffect.Parameters["uProgress"].SetValue(Utils.GetLerpValue(MaxTime / 3f, MaxTime, TimeLeft, true));
            dissolveEffect.Parameters["uPower"].SetValue(4f + Utils.GetLerpValue(MaxTime / 4f, MaxTime / 3f, TimeLeft, true) * 40f);
            dissolveEffect.Parameters["uNoiseStrength"].SetValue(1f);
            dissolveEffect.CurrentTechnique.Passes[0].Apply();


            Main.spriteBatch.Draw
                (texture, position - Main.screenPosition, frame, drawColor * (1-LumUtils.InverseLerp(0,MaxTime, TimeLeft)), Rotation + MathHelper.Pi / 3f * direction, frame.Size() * 0.5f, Scale * new Vector2(0.7f, 0.7f) * 0.25f * LumUtils.InverseLerp(MaxTime, 0, TimeLeft), 0, 0);

            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

    }


}
