using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish
{
    class Jellyfish : ModNPC
    {
        public enum JellyfishAI
        {
            AppearFromRift,
            Idle,
            Die
        }
        public JellyfishAI JellyfishState
        {
            get => (JellyfishAI)NPC.ai[1];
            set => NPC.ai[1] = (int)value;
        }
        public ref float Time => ref NPC.ai[0];
        private float AlphaInterp = 0;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {
            NPC.height = 24;
            NPC.width = 24;
            NPC.aiStyle = -1;
            NPC.damage = int.MaxValue;
            NPC.defense = 0;
            NPC.lifeMax = 1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;

        }
        public override void OnSpawn(IEntitySource source)
        {
            JellyfishState = JellyfishAI.AppearFromRift;
            AlphaInterp = 0;
            
        }
        public override void AI()
        {
            StateMachine();
            Time++;
        }
        private void StateMachine()
        {
            switch (JellyfishState)
            {
                case JellyfishAI.AppearFromRift:
                    HandleAppearFromRift();
                    break;
                case JellyfishAI.Idle:
                    HandleIdle();
                    break;
                case JellyfishAI.Die:
                    break;
            }
        }
        private void HandleAppearFromRift()
        {
            if (Time == 0)
            {
                Rift darkParticle = Rift.pool.RequestParticle();
                darkParticle.Prepare(NPC.Center, Vector2.Zero, Color.AntiqueWhite, new Vector2(1, 1), NPC.velocity.ToRotation(), 1, 1, 60);


                ParticleEngine.Particles.Add(darkParticle);

                NPC.velocity.X += 1;
            }
            AlphaInterp = float.Lerp(AlphaInterp, 1, 0.05f);
            if(Time > 10 && AlphaInterp > 0.9f)
            {
                AlphaInterp = 1;
                Time = 0;
                JellyfishState = JellyfishAI.Idle;
            }
        }
        private void HandleIdle()
        {
            if (Time >= 10)
            {
                NPC.velocity *= 0.9f;
            }
            
            //todo: make slowly float up and down
           
            // Slowly float up and down using a sine wave with small amplitude and period
            float floatAmplitude = 0.8f; // How far up/down to move
            float floatPeriod = 120*4f;     // How many ticks for a full cycle
            float targetY = (float)Math.Sin(Time / floatPeriod * MathHelper.TwoPi) * floatAmplitude;
            NPC.velocity.Y = float.Lerp(NPC.velocity.Y, targetY, 0.5f);
            
        }
        private void HandleDeath()
        {

        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //todo: everything lmao
            Vector2 DrawPos = NPC.Center - screenPos;
            Utils.DrawBorderString(spriteBatch, "State: " + JellyfishState.ToString() + " | Alpha Interp: " + AlphaInterp.ToString(), DrawPos - Vector2.UnitY*-100, Color.White);
            Utils.DrawBorderString(spriteBatch, "Time: " + Time.ToString(), DrawPos - Vector2.UnitY * -80, Color.White);

            Texture2D placeholder = TextureAssets.Npc[Type].Value;


            Main.EntitySpriteDraw(placeholder, DrawPos, null, drawColor * AlphaInterp, NPC.rotation, placeholder.Size() * 0.5f, 1, SpriteEffects.None);
            return false;//base.PreDraw(spriteBatch, screenPos, drawColor);
        }
    }
}
