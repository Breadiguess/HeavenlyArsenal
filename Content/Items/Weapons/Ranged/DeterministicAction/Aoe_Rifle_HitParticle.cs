using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction
{
    public class Aoe_Rifle_HitParticle : BaseParticle
    {
        public static ParticlePool<Aoe_Rifle_HitParticle> pool = new(500, GetNewParticle<Aoe_Rifle_HitParticle>);
        public Vector2 position;
        public Vector2 Velocity;
        public float Rotation;
        public int TimeLeft;
        public int MaxTime;
        public void Prepare(Vector2 Position,float Rotation,int MaxTime)
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
            
            TimeLeft--;
            if (TimeLeft <= 0)
            {
                ShouldBeRemovedFromRenderer = true;
            }
        }

        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            Texture2D tex = AssetDirectory.Textures.GlowCone.Value;


            Color thing = Color.Lerp(Color.Transparent, Color.Crimson, LumUtils.InverseLerp(0, MaxTime, TimeLeft));
            Vector2 DrawPos = position - Main.screenPosition;
            for(int i = 0; i< 3; i++)
            {
                Main.EntitySpriteDraw(tex, DrawPos, null, thing with { A = 0 }, Rotation + (i / 3f * MathHelper.Pi) - MathHelper.ToRadians(60), new Vector2(0, tex.Height / 2), new Vector2(1, 0.2f) * LumUtils.InverseLerp(0, MaxTime, TimeLeft), 0);

            }

        }
    }
}