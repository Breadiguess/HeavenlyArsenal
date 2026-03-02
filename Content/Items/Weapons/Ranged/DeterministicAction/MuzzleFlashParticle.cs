using HeavenlyArsenal.Core;
using NoxusBoss.Core.Graphics.Automators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    internal class MuzzleFlashParticle : BaseParticle, IDrawSubtractive
    {
        public static ParticlePool<MuzzleFlashParticle> pool = new(500, GetNewParticle<MuzzleFlashParticle>);
        public Vector2 position;
        public Vector2 Velocity;
        public float Rotation;
        public int TimeLeft;
        public int MaxTime;
        public int FramePos;
        public void Prepare(Vector2 Position, float Rotation, int MaxTime)
        {

            position = Position;
            this.Rotation = Rotation;
            this.TimeLeft = MaxTime;
            this.MaxTime = TimeLeft;

        }

        public override void FetchFromPool()
        {
            base.FetchFromPool();
            Velocity = Vector2.Zero;

        }

        public override void Update(ref ParticleRendererSettings settings)
        {
            FramePos = (int)(6 * LumUtils.InverseLerp(MaxTime, MaxTime/4, TimeLeft))+2;

            TimeLeft--;
            if (TimeLeft <= 0)
            {
                ShouldBeRemovedFromRenderer = true;
            }
        }

        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            spritebatch.End();
            spritebatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, default, DefaultRasterizerScreenCull, null, Main.GameViewMatrix.TransformationMatrix);
            Texture2D tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Ranged/DeterministicAction/MuzzleFlash").Value;

            Vector2 DrawPos = position - Main.screenPosition;
            Rectangle Frame = tex.Frame(1, 8, 0, FramePos);
            Vector2 Origin = new Vector2(0, Frame.Height / 2);
            Main.EntitySpriteDraw(tex, DrawPos, Frame, Color.Crimson, Rotation, Origin, 1, 0);

            spritebatch.ResetToDefault();
        }

        void IDrawSubtractive.DrawSubtractive(SpriteBatch spriteBatch)
        {
            Texture2D tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Ranged/DeterministicAction/MuzzleFlash").Value;

            Vector2 DrawPos = position - Main.screenPosition;
            Rectangle Frame = tex.Frame(1, 8, 0, FramePos);
            Vector2 Origin = new Vector2(0, Frame.Height / 2);
            Main.EntitySpriteDraw(tex, DrawPos, Frame, Color.Crimson, Rotation, Origin, 1, 0);

        }
    }
}
