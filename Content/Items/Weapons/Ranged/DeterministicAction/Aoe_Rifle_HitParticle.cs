using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Core;
using NoxusBoss.Assets;
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
            
            if(TimeLeft % 10 == 0)
            {
                //Aoe_Rifle_DeathParticle particle = new Aoe_Rifle_DeathParticle();
                //particle.Prepare(position + Main.rand.NextVector2CircularEdge(30, 30), 0, 120, null, Main.rand.Next(Aoe_Rifle_DeathParticle.SymbolList.Length));
                //ParticleEngine.BehindProjectiles.Add(particle);
            }
            TimeLeft--;
            if (TimeLeft <= 0)
            {
                ShouldBeRemovedFromRenderer = true;
            }
        }

        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            Texture2D tex = AssetDirectory.Textures.GlowCone.Value;
            Texture2D glow = GennedAssets.Textures.GreyscaleTextures.HollowCircleSoftEdge;
            Texture2D glow2 = GennedAssets.Textures.GreyscaleTextures.Corona;
            Texture2D ting = AssetDirectory.Textures.ShatteredBurst2.Value;
            float Rot = Rotation + MathHelper.Pi;
            Color thing = Color.Lerp(Color.Transparent, Color.White, LumUtils.InverseLerp(0, MaxTime, TimeLeft));
            Vector2 DrawPos = position - Main.screenPosition;
            for(int i = 0; i< 3; i++)
            {
                Main.EntitySpriteDraw(tex, DrawPos, null, thing with { A = 0 }, Rot + (i / 3f * MathHelper.PiOver2) - MathHelper.ToRadians(30), new Vector2(0, tex.Height / 2), new Vector2(1, 0.2f) * LumUtils.InverseLerp(0, MaxTime, TimeLeft) * (i == 1 ? 1.4f : 0.8f), 0);

            }
            Main.EntitySpriteDraw(tex, DrawPos, null, thing with { A = 0 }, Rotation, new Vector2(0, tex.Height / 2), new Vector2(1, 0.05f), 0);
            Main.EntitySpriteDraw(glow, DrawPos - new Vector2(-20,0).RotatedBy(Rotation), null, thing with { A = 0 }, Rotation, glow.Size() / 2, new Vector2(0.4f, 1f) * 0.1f, 0);
            Main.EntitySpriteDraw(ting, DrawPos - new Vector2(-20, 0).RotatedBy(Rotation), null, thing with { A = 0 }, Rotation, ting.Size() / 2, new Vector2(0.4f, 1f) * 0.1f , 0);

            //Main.EntitySpriteDraw(glow, DrawPos - new Vector2(-10, 0).RotatedBy(Rotation), null, thing with { A = 0 }*0.5f, Rotation, glow.Size() / 2, new Vector2(0.4f, 1f)*0.12f * LumUtils.InverseLerp(0, MaxTime, TimeLeft), 0);
            //Main.EntitySpriteDraw(glow2, DrawPos - new Vector2(-10, 0).RotatedBy(Rotation), null, thing with { A = 0 }, Rotation, glow2.Size() / 2, new Vector2(0.4f, 1f) * 0.28f * LumUtils.InverseLerp(0, MaxTime, TimeLeft), 0);

            //   Main.EntitySpriteDraw(ting, DrawPos, null, thing with { A = 0 }, Rotation, ting.Size() / 2, 0.2f+0.2f * (1 - LumUtils.InverseLerp(0, MaxTime, TimeLeft)), 0);
        }
    }
}