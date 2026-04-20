using HeavenlyArsenal.Content.Items.Weapons.Ranged.DeterministicAction;
using HeavenlyArsenal.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Renderers;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.EdgyDualSwords
{
    internal class TrioSwordHitEffect :BaseParticle
    {
        public static ParticlePool<TrioSwordHitEffect> pool = new(500, GetNewParticle<TrioSwordHitEffect>);

        public Color color;
        public Vector2 Position;
        public float Rotation;
        public int TimeLeft;
        public int MaxTime;
        public void Prepare(Vector2 Pos, float rotation, int MaxTime, Color color)
        {
            Position = Pos;
            Rotation = rotation;
            this.MaxTime = MaxTime;
            TimeLeft = this.MaxTime;
            this.color = color;
        }

        public override void Update(ref ParticleRendererSettings settings)
        {
            if (TimeLeft-- <= 0)
                ShouldBeRemovedFromRenderer = true;
        }

        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            Texture2D Tex = AssetDirectory.Textures.SlashEffect.Value;
            Vector2 DrawPos = Position - Main.screenPosition;

            Vector2 Origin = new Vector2(120, Tex.Height / 2);


            Color color = this.color * (TimeLeft / (float)MaxTime);
            const float Iterations = 12;
            for(int i = 0; i< Iterations; i++)
            {
                Main.EntitySpriteDraw(Tex, DrawPos + new Vector2(1,0).RotatedBy(i/Iterations), null, color with { A = 0 }, Rotation + MathHelper.Pi + MathHelper.ToRadians(i/12f), Origin, 0.2f, 0);

            }



        }
    }
}
