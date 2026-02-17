using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Core;
using NoxusBoss.Assets;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodCult.FleshkinAcolyte_Assassin
{
    internal class LarvaeBombSmoke : BaseParticle
    {
        public static ParticlePool<LarvaeBombSmoke> pool = new(500, GetNewParticle<LarvaeBombSmoke>);

        public int timeLeft;

        public int timeLeftMax;

        public float Progress;

        public float Scale;

        public Vector2 StartPos;

        public Vector2 Position;

        public Vector2 Velocity;

        public float Rotation;

        public void Prepare(Vector2 Position, Vector2 initialVelocity, int LifeTime, float Scale, Vector2 StartPos)
        {
            this.Position = Position;
            Velocity = initialVelocity;
            timeLeftMax = LifeTime;
            this.Scale = Scale;
            this.StartPos = StartPos;
            Rotation = initialVelocity.ToRotation();
        }

        public override void FetchFromPool()
        {
            base.FetchFromPool();
            timeLeft = 0;
            Progress = 0;
        }

        public override void Update(ref ParticleRendererSettings settings)
        {
          

            Position += Velocity;
            Progress = float.Lerp(Progress, 1, 0.02f);

            if (timeLeft > timeLeftMax)
            {
                ShouldBeRemovedFromRenderer = true;
            }

            timeLeft++;
        }

        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            Texture2D tex = GennedAssets.Textures.Particles.FireParticleA;
            Texture2D Debug = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

            var DrawPos = Position - Main.screenPosition;

            var columns = 6;
            var rows = 6;
            var totalFrames = columns * rows; // 36 total frames

            // progress = 0 → 1
            var progress = Progress;

            var frameOffset = 1;

            var frameIndex = (int)(progress * (totalFrames - 1)) + frameOffset;
            frameIndex = Math.Min(frameIndex, totalFrames - 1);

            var frameX = frameIndex % columns;
            var frameY = frameIndex / columns;

            var Frm = tex.Frame(columns, rows, frameX, frameY);

            var a = Color.Crimson with
            {
                A = 0
            };

            var Origin = new Vector2(Frm.Width / 2, Frm.Height / 2 + 45);
           
            Main.EntitySpriteDraw(tex, DrawPos, Frm, a, Rotation, Origin, Scale, 0);
        }
    }
}
