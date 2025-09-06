using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Core;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish
{
    class Jellyfish : ModNPC
    {
        public Player Target
        {
            get;
            set;
        }
        public enum JellyfishAI
        {
            AppearFromRift,
            Idle,
            Placeholder1,
            Placeholder2,
            Placeholder3,
            Die
        }
        public ref float Time => ref NPC.ai[0];
        public JellyfishAI JellyfishState
        {
            get => (JellyfishAI)NPC.ai[1];
            set => NPC.ai[1] = (int)value;
        }
        public ref float AttackTimer => ref NPC.ai[2];
       
        private int currentCurveIndex = 0;

        //todo: write extra ai that tells it what Bezier Curve to use. this is so that the curve isn't inherently linked to the current state.
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(currentCurveIndex);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            currentCurveIndex = reader.ReadInt32();
        }
        private float AlphaInterp = 0;
        private float PortalInterp = 0;

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }
        private List<BezierCurve> attackCurves;
        public override void SetDefaults()
        {
            NPC.height = 24;
            NPC.width = 24;
            NPC.aiStyle = -1;
            NPC.damage = int.MaxValue;
            NPC.defense = 0;
            NPC.lifeMax = 1000000000;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;


            attackCurves = new List<BezierCurve>
            {
               //0  
                new BezierCurve(
                    new Vector2(0, 0),
                    new Vector2(250, 200),
                    new Vector2(400, 0),
                    new Vector2(200, -200)
                ),

                //1
                new BezierCurve(
                    new Vector2(-30, 0),
                    new Vector2(190, 100),
                    new Vector2(-100, 0)
            ),
                //2
                new BezierCurve(
                    new Vector2(0, 0),
                    new Vector2(250, 200),
                    new Vector2(400, 0),
                    new Vector2(200, -200)
                ),

            };

        }
        private Vector2 RiftLocation;
        private Vector2 ReferenceLocation;
        public override void OnSpawn(IEntitySource source)
        {
            JellyfishState = JellyfishAI.AppearFromRift;
            RiftLocation = NPC.Center;
            AlphaInterp = 0;
            PortalInterp = 0;
            
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
                case JellyfishAI.Placeholder1:
                    HandlePlaceholder1();
                    break;

                case JellyfishAI.Placeholder2:
                    HandlePlaceholder2();
                    break;

                case JellyfishAI.Placeholder3:
                    HandlePlaceholder3();
                    break;
                case JellyfishAI.Die:
                    break;
            }
        }
        private void HandleAppearFromRift()
        {
            if (Time == 0)
            {
                /*Rift darkParticle = Rift.pool.RequestParticle();
                darkParticle.Prepare(NPC.Center, Vector2.Zero, Color.AntiqueWhite, new Vector2(1, 1), NPC.velocity.ToRotation(), 1, 1, 60);


                ParticleEngine.Particles.Add(darkParticle);
                */
                NPC.velocity.X += 1;
            }
            AlphaInterp = float.Lerp(AlphaInterp, 1, 0.05f);
            PortalInterp = float.Lerp(PortalInterp, 1, 0.25f);
            if(Time > 10 && AlphaInterp > 0.9f)
            {
                AlphaInterp = 1;
                Time = 0;
                PortalInterp = 1;
                JellyfishState = JellyfishAI.Idle;
            }
        }
        private void HandleIdle()
        {
            if (Time >= 10)
            {
                NPC.velocity *= 0.9f;
                PortalInterp = Utils.GetLerpValue(PortalInterp, 0, 0.4f, true);//float.Lerp(PortalInterp, 0, 0.2f);
                if (PortalInterp < 0.01f)
                    PortalInterp = 0;
                
            }
            if(Time> 50)
            {
                JellyfishState = JellyfishAI.Placeholder1;
                ReferenceLocation = NPC.Center;
                AttackTimer = 0;
                //todo: set Target to the closest player
                Player closestPlayer = null;
                float closestDist = float.MaxValue;
                foreach (Player player in Main.player)
                {
                    if (player != null && player.active && !player.dead)
                    {
                        float dist = Vector2.Distance(NPC.Center, player.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestPlayer = player;
                        }
                    }
                }
                Target = closestPlayer;
                Time = 0;
            }
            /*    
                //todo: make slowly float up and down

                // Slowly float up and down using a sine wave with small amplitude and period
            float floatAmplitude = 0.8f; // How far up/down to move
            float floatPeriod = 120*4f;     // How many ticks for a full cycle
            float targetY = (float)Math.Sin(Time / floatPeriod * MathHelper.TwoPi) * floatAmplitude;
            NPC.velocity.Y = float.Lerp(NPC.velocity.Y, targetY, 0.5f);
            */
        }
        // Handles moving the NPC along the given Bezier curve based on the provided progress (0 to 1).
        private void MoveAlongCurve(BezierCurve curve, float Progress)
        {
            // Clamp progress to [0, 1]
            Progress = Math.Clamp(Progress, 0f, 1f);

            // Use ReferenceLocation as the base position
            Vector2 basePosition = ReferenceLocation;

            // Evaluate the current position on the curve
            Vector2 targetOffset = curve.Evaluate(Progress);
            NPC.Center = basePosition + targetOffset;

            // Calculate rotation based on the direction to the next point on the curve
            float nextT = Math.Clamp(Progress + 0.01f, 0f, 1f);
            Vector2 currentPoint = curve.Evaluate(Progress);
            Vector2 nextPoint = curve.Evaluate(nextT);
            Vector2 direction = nextPoint - currentPoint;
            if (direction.LengthSquared() > 0.0001f)
                NPC.rotation = direction.ToRotation();

            //debug

            int debugPoints = 36;
            for (int i = 0; i <= debugPoints; i++)
            {
                float t = i / (float)debugPoints;
                Vector2 pointOnCurve = curve.Evaluate(t);
                Vector2 dustPosition = basePosition + pointOnCurve;
                Dust.NewDustPerfect(dustPosition, DustID.Cloud, Vector2.Zero);
            }
        }
        private void HandlePlaceholder1()
        {

            BezierCurve curve = new BezierCurve(
                    new Vector2(0, 0),
                    new Vector2(250, 200),
                    new Vector2(400, 200),
                    new Vector2(600, 0)
                );//attackCurves[currentCurveIndex];


            AttackTimer = (float)Utils.Lerp(AttackTimer, 1, 0.1f);//Utils.Clamp(Time / 120f, 0f, 1f);
            MoveAlongCurve(curve, AttackTimer);
            if (AttackTimer >= 0.99f)
            {
                AttackTimer = 0;
                if(currentCurveIndex == 0)
                {
                    currentCurveIndex = 1;
                }
                else
                {
                    ResetValues();
                    JellyfishState = JellyfishAI.Placeholder2;
                }
               

            }
        }
        private void HandlePlaceholder2()
        {
            BezierCurve curve = new BezierCurve(
                    new Vector2(-30, 0),
                    new Vector2(190, 100),
                    new Vector2(-100, 0)
            );//attackCurves[1];

            // Normalize t from 0 to 1 over, say, a 120‑tick attack
            //AttackTimer = (float)Utils.Lerp(AttackTimer, 1, 0.1f);//Utils.Clamp(Time / 120f, 0f, 1f);


            AttackTimer = 0;//(float)Utils.Lerp(AttackTimer, 1, 0.1f);//Utils.Clamp(Time / 120f, 0f, 1f);
            MoveAlongCurve(curve, AttackTimer);
            //todo: run along the curve generated by the bezier and draw dust for debug
           
            if (AttackTimer == 1)
            {
                JellyfishState = JellyfishAI.Idle;
                ResetValues();
            }
        }
        private void HandlePlaceholder3()
        {
            // Placeholder for future logic
        }
        private void HandleDeath()
        {

        }
        private void ResetValues()
        {
            Time = 0;
            AttackTimer = 0;
            ReferenceLocation = NPC.Center;
            currentCurveIndex = 0; 

        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D DebugArrow = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Jellyfish/Jellyfish_DebugArrow").Value;
            //todo: everything lmao
            if(!NPC.IsABestiaryIconDummy)
            DrawRift(RiftLocation, NPC.rotation);


            Vector2 DrawPos = NPC.Center - screenPos;
            if (!NPC.IsABestiaryIconDummy){
                Utils.DrawBorderString(spriteBatch, "State: " + JellyfishState.ToString() + " | Alpha Interp: " + AlphaInterp.ToString(), DrawPos - Vector2.UnitY*-100, Color.White);
                Utils.DrawBorderString(spriteBatch, "Time: " + Time.ToString() + ", Portal Interp" + PortalInterp.ToString(), DrawPos - Vector2.UnitY * -80, Color.White);
            }

            float Rot = NPC.rotation + MathHelper.PiOver2;
            Texture2D placeholder = TextureAssets.Npc[Type].Value;

            Main.EntitySpriteDraw(DebugArrow, DrawPos, null, drawColor * AlphaInterp, Rot, DebugArrow.Size() * 0.5f, 1, SpriteEffects.None);

            Main.EntitySpriteDraw(placeholder, DrawPos, null, drawColor * AlphaInterp, Rot, placeholder.Size() * 0.5f, 1, SpriteEffects.None);
           return false;//base.PreDraw(spriteBatch, screenPos, drawColor);
        }
        private void DrawRift(Vector2 Position, float Rotation)
        {
            Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;

            // float vanishTime = Utils.GetLerpValue(0, 20, 40, true) * Utils.GetLerpValue(0, 20, 4, true);


            Vector2 PortalSize = new Vector2(0.12f, 0.25f) * PortalInterp;
            Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            Main.EntitySpriteDraw(glow, Position- Main.screenPosition, glow.Frame(), Color.Red with { A = 200 }, Rotation, glow.Size() * 0.5f, PortalSize, 0, 0);

            Texture2D innerRiftTexture = AssetDirectory.Textures.VoidLake.Value;
            Color edgeColor = new Color(1f, 0.06f, 0.06f);
            float timeOffset = Main.myPlayer * 2.5552343f;

            ManagedShader riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
            riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.2f + timeOffset);
            riftShader.TrySetParameter("baseCutoffRadius", 0.2f);
            riftShader.TrySetParameter("swirlOutwardnessExponent", 0.5f);
            riftShader.TrySetParameter("swirlOutwardnessFactor", 3f);
            riftShader.TrySetParameter("vanishInterpolant", 1-PortalInterp);
            riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
            riftShader.TrySetParameter("edgeColorBias", 0.1f);
            riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
            riftShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap);
            riftShader.Apply();

            Main.spriteBatch.Draw(innerRiftTexture, Position - Main.screenPosition, null, Color.White, Rotation + MathHelper.Pi, innerRiftTexture.Size() * 0.5f, PortalSize, 0, 0);

            Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);


        }
    }
}
