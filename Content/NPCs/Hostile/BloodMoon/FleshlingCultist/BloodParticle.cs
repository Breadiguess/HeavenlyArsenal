using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.Map;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.FleshlingCultist
{
    internal class BloodParticle : BaseParticle
    {
        public static ParticlePool<BloodParticle> pool = new ParticlePool<BloodParticle>(500, GetNewParticle<BloodParticle>);

        public int MaxTime;
        public int TimeLeft;

        public Vector2[] trailPos;
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 EndPosition;
        public NPC endNPC;
        public void Prepare(Vector2 position, int Maxtime, NPC endNPC)
        {
            Position = position;
            this.MaxTime = Maxtime*3;

            this.endNPC = endNPC;
            trailPos = new Vector2[6];
            for (int i = 0; i < trailPos.Length; i++)
            {
                trailPos[i] = Position;
            }
        }

        public override void FetchFromPool()
        {
            base.FetchFromPool();
            TimeLeft = 0;
            if(endNPC != null && endNPC.active)
                EndPosition = endNPC.Top + new Vector2(0,-40);
           
        }

        public override void Update(ref ParticleRendererSettings settings)
        {
            if(endNPC != null)
                EndPosition = endNPC.Top + new Vector2(0, -40);
            Position = Vector2.Lerp(Position + new Vector2(MathF.Sin(TimeLeft/4) * 4, 0).RotatedBy(Position.AngleTo(EndPosition) + MathHelper.PiOver2), EndPosition, 0.01f);
            
            for (int i = 1; i < trailPos.Length; i++)
            {
                trailPos[i] = Vector2.Lerp(trailPos[i], trailPos[i-1],0.4f);
            }
            trailPos[0] = Position;
            TimeLeft++;
            if (TimeLeft > MaxTime || Position.Distance(EndPosition)<3)
                ShouldBeRemovedFromRenderer = true;
        }


        public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch)
        {
            Texture2D tex = GennedAssets.Textures.GreyscaleTextures.BloomCirclePinpoint;
            Vector2 DrawPos;
            for(int i = 0; i < trailPos.Length; i++)
            {
                DrawPos = trailPos[i] - Main.screenPosition;
                float Rot = trailPos[i].AngleTo(Position);
                Vector2 Scale = new Vector2(1, 1) * 0.2f * (1 - i / (float)trailPos.Length) * 0.7f;
                Main.EntitySpriteDraw(tex, DrawPos, null, Color.Crimson with { A = 0 }, Rot + MathHelper.PiOver2, tex.Size() * 0.5f, Scale, 0);
                //Utils.DrawBorderString(Main.spriteBatch, i.ToString(), DrawPos, Color.AntiqueWhite);
            }


            DrawPos = Position - Main.screenPosition;
            float rot = Position.AngleTo(EndPosition);
            Main.EntitySpriteDraw(tex, DrawPos, null, Color.Crimson with { A = 0 }, rot + MathHelper.PiOver2, tex.Size() * 0.5f, 0.2f, 0);
        }

    }
}
