using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Physics.InverseKinematics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Dust = Terraria.Dust;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab
{
  
    public class HemoCrab : BloodmoonBaseNPC
    {

        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/BigCrab/ArtilleryCrab";


        public override float buffPrio => 4;
        public override bool canBeSacrificed => false;
        public override int bloodBankMax => 10_000;

       
        public enum HemocrabAI
        {
            Idle,
            //me when i stride
            Traverse,


            //Ranged shenanigans
            CheckAmmo,
            Restock,
            LocateBombardPosition,

            FireAppocalypseCannon,

            //close range shit
            
            
        }

        public HemocrabAI CurrentState = HemocrabAI.Idle;
        public float BombardRange = 1000f;
        
        public ref float Time => ref NPC.ai[0];
        public int AmmoCount
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        
        //private Vector2 MoveTarget = Vector2.Zero;
        

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 100;
            NPC.damage = 200;
            NPC.defense = 130/2;
            NPC.lifeMax = 38470;
            NPC.value = 10000;
            NPC.aiStyle = -1;
            NPC.npcSlots = 3f;
            NPC.knockBackResist = 0f;
            
        }

        public override void AI()
        {
            Player player = Main.LocalPlayer;
            StateMachine();

            Vector2 difference = Main.MouseWorld -NPC.Center;
            //NPC.velocity = Vector2.Lerp(NPC.velocity, difference, 0.6f);
            HoverAboveGround(5 * 16);
            NPC.SimpleFlyMovement(difference, 0.1f);
            NPC.rotation =  NPC.rotation.AngleLerp(NPC.velocity.ToRotation(), 0.01f);


            KinematicChain a = new KinematicChain(30, 30, 30);
            ManageLegs();
        }
        
        private void StateMachine()
        {
            switch (CurrentState)
            {
                case HemocrabAI.Idle:
                    CurrentState = HemocrabAI.Traverse;
                    break;
            
                case HemocrabAI.Traverse:
                   
                    break;
            }
        }
        


        private void HoverAboveGround(int hoverPixels)
        {
            Point tilePos = NPC.Center.ToTileCoordinates();
            int groundY = -1;

            // Scan downward to find ground
            for (int y = tilePos.Y; y < Main.maxTilesY; y++)
            {
                Tile tile = Framing.GetTileSafely(tilePos.X, y);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    groundY = y * 16;
                    break;
                }
            }

            if (groundY == -1)
                return; // no ground below

            float targetY = groundY - hoverPixels - NPC.height; // desired top position
            float displacement = (NPC.Center.Y - targetY);

            // Physics constants
            float stiffness = 0.01f;   // springiness (higher = stiffer legs)
            float damping = 0.1f;     // resist oscillation
            float force = (-stiffness * displacement) - (damping * NPC.velocity.Y);

            // Apply as vertical velocity
            NPC.velocity.Y += force;

            
        }





        private void ManageLegs()
        {
            float height = 60f;
            float idealRotation = Math.Clamp((NPC.Center.X) * -0.023f, -0.2f, 0.2f);
            Vector2 baseDestination = NPC.Center + new Vector2(-NPC.direction * 250f, -30f);

            int tries = 0;
            while (!Collision.CanHitLine(baseDestination - Vector2.UnitX * 250f, 500, 1, NPC.Center, 1, 1))
            {
                baseDestination += Vector2.UnitX * NPC.direction * 16f;

                tries++;
                if (tries >= 50)
                    break;
            }
            float moveDistanceThreshold = 120f;
            Vector2 leftGround = FindGroundVertical((baseDestination - Vector2.UnitX * 140f).ToTileCoordinates()).ToWorldCoordinates();
            Vector2 rightGround = FindGroundVertical((baseDestination + Vector2.UnitX * 140f).ToTileCoordinates()).ToWorldCoordinates();


            Vector2 idealFingerA = NPC.Center + new Vector2(NPC.direction == 1 ? 205f : -190f, height);
            idealFingerA.X = float.Lerp(idealFingerA.X, NPC.Center.X, 0.85f);

            // Move finger A.

            Dust a = Dust.NewDustPerfect(FingerPositionA, DustID.Cloud, Vector2.Zero, 0, default, 1);
            a.position = FingerPositionA;
            idealFingerA = FindGroundVertical(idealFingerA.ToTileCoordinates()).ToWorldCoordinates(8f, -2f);
            MoveFinger(ref FingerAnimationCompletionA, ref FingerPositionA, ref FingerAnimationStartA, idealFingerA, moveDistanceThreshold);

        }


        /// <summary>
        /// The animation completion of finger A. Used when making the finger step forward via inverse kinematics calculations.
        /// </summary>
        public float FingerAnimationCompletionA;

        /// <summary>
        /// The animation starting point for finger A. Used when making the finger step forward via inverse kinematics calculations.
        /// </summary>
        public Vector2 FingerAnimationStartA;

        public void MoveFinger(ref float animationCompletion, ref Vector2 currentPosition, ref Vector2 start, Vector2 end, float moveDistanceThreshold = 140f)
        {
            bool fingerAStarted = FingerAnimationCompletionA > 0f && FingerAnimationCompletionA < 0.48f;
          
            bool animationJustStarted = fingerAStarted;
            if (animationCompletion <= 0f && !currentPosition.WithinRange(end, moveDistanceThreshold) && !animationJustStarted)
            {
                start = currentPosition;
                animationCompletion = 0.03f;
        }

            if (animationCompletion >= 0.03f)
        {
                animationCompletion = Saturate(animationCompletion + 0.067f);
                currentPosition = Vector2.SmoothStep(start, end, InverseLerp(0.1f, 0.9f, animationCompletion));
                currentPosition -= Vector2.UnitY * Convert01To010(animationCompletion) * 70f;

                if (animationCompletion >= 1f)
                    animationCompletion = 0f;
            }

            if (currentPosition.X >= 50f && currentPosition.X <= Main.maxTilesX * 16f - 50f)
                currentPosition.Y = float.Lerp(currentPosition.Y, FindGroundVertical(currentPosition.ToTileCoordinates()).ToWorldCoordinates(8f, 16f).Y, 0.075f);
        }

        /// <summary>
        /// The reach position for finger A. This is what inverse kinematics calculations will attempt to reach towards.
        /// </summary>
        public Vector2 FingerPositionA;

        private void RenderFingerA(Vector2 handPosition, float generalScale, float armRotation)
            {             
            float digitAScale = generalScale * 0.95f;
            Vector2 digitAStart = handPosition + new Vector2(NPC.spriteDirection * -20f, 0).RotatedBy(armRotation) * NPC.scale;
            Texture2D digitA1 = GennedAssets.Textures.Hands.FrontHandLeftFinger1Digit1;
            Texture2D digitA2 = GennedAssets.Textures.Hands.FrontHandLeftFinger1Digit2;
            Texture2D digitA3 = GennedAssets.Textures.Hands.FrontHandLeftFinger1Digit3;
            KinematicChain digitA = new KinematicChain(digitA1.Size().Length() * digitAScale * 0.707f, digitA2.Size().Length() * digitAScale * 0.707f, digitA3.Size().Length() * digitAScale * 0.707f)
            {
                StartingPoint = digitAStart
            };
            digitA[1].Constraints.Add(new UpwardOnlyConstraint(new Vector2(-NPC.spriteDirection, 0f)));

            digitA.Update(FingerPositionA - Main.screenPosition);
            ForwardKinematics(digitAScale, Color.White, digitAStart,
                [new(0.5f, 0f),
                new(0.55f, 0.1f),
                new(0f, 0.1f),
            ], [digitA1, digitA2, digitA3],
                [digitA[0].Offset.RotatedBy(0), digitA[1].Offset.RotatedBy(0 * 2f), digitA[2].Offset.RotatedBy(0 * 3f)],
                [0f, 0f, -0.3f]);
            }

        public void ForwardKinematics(float scale, Color color, Vector2 start, Vector2[] origins, Texture2D[] textures, Vector2[] offsets, float[] rotationOffsets, Vector2[]? manualOffsets = null)
        {
            SpriteEffects direction = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2[] drawPositions = new Vector2[textures.Length] ;
            Vector2 currentDrawPosition = start;
            for (int i = 0; i < textures.Length; i++)
            {
                drawPositions[i] = currentDrawPosition + (manualOffsets?[i] ?? Vector2.Zero);
                currentDrawPosition += offsets[i];
        }

            for (int i = 0; i < textures.Length; i++)
        {
                float rotation = offsets[i].ToRotation() - rotationOffsets[i] - MathHelper.PiOver2;
                Texture2D texture = textures[i];
                Vector2 origin = origins[i];
                if (direction == SpriteEffects.FlipHorizontally)
                    origin.X = 1f - origin.X;

                Main.spriteBatch.Draw(texture, drawPositions[i] - Main.screenPosition, null, color, rotation, origin * texture.Size(), scale, direction, 0f);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                return base.PreDraw(spriteBatch, screenPos, drawColor);
            }
            RenderFingerA(NPC.Center, 0.5f, NPC.rotation);
            Vector2 DrawPos = NPC.Center - Main.screenPosition;

            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height/13/2);

            SpriteEffects Direction = NPC.direction < 0 ? SpriteEffects.FlipHorizontally : 0;

            int currentFrame = (int)NPC.frameCounter;

            Rectangle frame = texture.Frame(1, 13, 0, currentFrame);

            float Rot = NPC.rotation +MathHelper.PiOver2;
            
            texture.Frame(5, 1, 0, 0);
            Main.EntitySpriteDraw(texture, DrawPos, frame, drawColor, Rot, origin, NPC.scale, Direction, 0);

            Rectangle CrabFrame = new Rectangle(0, BodyFrame * frameHeight, texture.Width, frameHeight);

            Utils.DrawBorderString(spriteBatch, FingerAnimationCompletionA.ToString(), DrawPos - Vector2.UnitY * 120, Color.AliceBlue);
            return false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.bloodMoon && DownedBossSystem.downedProvidence)
                return SpawnCondition.OverworldNightMonster.Chance * 0.01f;
            return 0f;
        }
    }

   }
