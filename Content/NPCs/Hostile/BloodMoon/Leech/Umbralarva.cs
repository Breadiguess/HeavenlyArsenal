using CalamityMod.Particles;
using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech
{
    public class Umbralarva : BloodmoonBaseNPC
    {
        public override int bloodBankMax => 2000;

        public override bool canBeSacrificed => segmentNum == 0;
        #region setup
        public enum LarvaAi
        {
            TrackPlayer,
            Lunge,

            Spit,

            StayNearMother

        }

        public LarvaAi CurrentState;

        private int LungeCooldown;
        private int LungeDurationMax = 120;

        private int SpitCooldown;


        private int MAX_SEGMENT_COUNT = 8;
        private int DEFAULT_SEGMENT_COUNT = 12;
        public float SEGMENT_DISTANCE = 18f; 
        private float HEAD_SPEED = 6.6f;
        private float HEAD_ACCEL = 0.18f;
        private int HISTORY_PER_SEGMENT = 16;

        /// <summary>
        /// Maintains a mapping of head identifiers to their respective lists of recorded head positions.
        /// </summary>
        /// <remarks>Each entry in the dictionary associates a unique head identifier (<see cref="int"/>)
        /// with a list of recorded positions (<see cref="Vector2"/>), where the newest position is stored at index 0.
        /// This static storage is used to track runtime history of head positions without relying on instance-specific
        /// storage.</remarks>
        private static Dictionary<int, List<Vector2>> _headPositionHistory = new Dictionary<int, List<Vector2>>();

        /// <summary>
        /// A static flag to prevent the recursive spawning of child segments..
        /// </summary>
        private static bool _suppressOnSpawnSpawning = false;

        public ref float Time => ref NPC.ai[0];
        public ref float segmentCount => ref NPC.ai[1];
        public ref float segmentNum => ref NPC.ai[2];
        
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
        }

        public override void SetDefaults()
        {
            NPC.width = 30;
            NPC.height = 30;
            NPC.lifeMax = 30000;
            NPC.damage = 160;
            NPC.defense = 201;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.aiStyle = -1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = AssetDirectory.Sounds.NPCs.Hostile.BloodMoon.UmbralLeech.DyingNoise;
           
        }

        public override void OnSpawn(IEntitySource source)
        {
            //todo: if this npc was spawned by another npc that isn't an umbral larva, set that npc as the mother.
            if (source is EntitySource_Parent parentSource && parentSource.Entity is NPC parentNpc && parentNpc.type != NPC.type)
            {
                Mother = parentNpc;
            }
            if (_suppressOnSpawnSpawning)
                return;

            // Only the head should spawn the chain stop it STOP IT
            if ((int)segmentNum != 0)
                return;

            if (segmentCount <= 0f)
                segmentCount = Main.rand.Next(5, MAX_SEGMENT_COUNT+1);

            int segCount = (int)segmentCount;

            if (!_headPositionHistory.ContainsKey(NPC.whoAmI))
                _headPositionHistory[NPC.whoAmI] = new List<Vector2>();


            _suppressOnSpawnSpawning = true;

            int lastIndex = NPC.whoAmI;
            for (int i = 1; i <= segCount; i++)
            {
                int newIndex = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X + i * 4, (int)NPC.Center.Y + i * 4, NPC.type);

                if (newIndex < 0 || newIndex >= Main.maxNPCs)
                    continue;

                NPC child = Main.npc[newIndex];

                
               
                child.ai[2] = i;  // Segment num
                child.ai[1] = segCount;  // segmentCount
                child.ai[0] = lastIndex; // predecessor whoAmI
                child.realLife = NPC.whoAmI; // reference to head
                child.netUpdate = true;
                
                lastIndex = newIndex;
            }

            _suppressOnSpawnSpawning = false;

            // sync to make sure supress doesn't break
            NPC.netUpdate = true;
        }
        #endregion

        public NPC Mother = null;

        public Player Target = null;

        private void StateMachine()
        {
            switch (CurrentState)
            {
                case LarvaAi.TrackPlayer:
                    HandleTrackPlayer();
                    break;
                case LarvaAi.Lunge:
                    HandleLungeAtPlayer();
                    break;
                case LarvaAi.Spit:
                    HandleSpit();
                    break;
            }
        }
        private void HandleTrackPlayer()
        {
            if(Target != null && Target.dead)
            {
                Target = null;
                return;
            }

            //todo: if target is null and not dead, find closest player and set it to target.
            // then skip over that step.
            if (Target == null || !Target.active)
            {
                float closestDist = float.MaxValue;
                Player closestPlayer = null;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player p = Main.player[i];  
                    if (p != null && p.active && !p.dead)
                    {
                        float d = Vector2.Distance(NPC.Center, p.Center);
                        if (d < closestDist)
                        {
                            closestDist = d;
                            closestPlayer = p;
                        }
                    }
                }
                if (closestPlayer != null)
                {
                    Target = closestPlayer;
                }
                else
                {
                    return;
                }
            }


            Vector2 toPlayer = Target.Center - NPC.Center;
            float dist = toPlayer.Length();
            if (dist > 8f)
            {
                toPlayer.Normalize();

                Vector2 desiredVel = toPlayer * HEAD_SPEED;

                Vector2 WiggleOffset = new Vector2(0, (float)Math.Sin(Time / 4 + Main.rand.NextFloat(0, 0.4f)) * 6).RotatedBy(NPC.rotation);
                NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVel + WiggleOffset, HEAD_ACCEL);
            }

            if(SpitCooldown <= 0 && dist < 480f && Main.rand.NextBool(120))
            {
                CurrentState = LarvaAi.Spit;
                Time = 0;
                return;
            }

            if (Main.rand.NextBool(76) && dist < 320f && LungeCooldown <=0)
            {
                CurrentState = LarvaAi.Lunge;
                Time = 0;
                SoundEngine.PlaySound(AssetDirectory.Sounds.NPCs.Hostile.BloodMoon.UmbralLeech.Bash with { PitchVariance = 0.2f, MaxInstances = 0, }, NPC.Center, null);
            }
        }
        private void HandleLungeAtPlayer()
        {
            if(Target == null || !Target.active || Target.dead)
            {
                CurrentState = LarvaAi.TrackPlayer;
                return;
            }

            float LungeDirection = NPC.Center.AngleTo(Target.Center);

            Vector2 desiredVel = new Vector2((float)Math.Cos(LungeDirection), (float)Math.Sin(LungeDirection)) * HEAD_SPEED * 2.5f;
            Vector2 WiggleOffset = new Vector2(0, (float)Math.Sin(Time / 4 + Main.rand.NextFloat(0, 0.4f)) * 6).RotatedBy(NPC.rotation) * 2;
            NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVel + WiggleOffset, 0.1f);

            if(Time > LungeDurationMax) {
                CurrentState = LarvaAi.TrackPlayer;
                Time = 0;
                LungeCooldown = 360;
            }

        }
        private void HandleSpit()
        {
            Vector2 ShootDirection = NPC.Center.AngleTo(Target.Center).ToRotationVector2();

            var a = Projectile.NewProjectileDirect(Projectile.GetSource_NaturalSpawn(), 
                NPC.Center, ShootDirection, ModContent.ProjectileType<BloodSpat>(), NPC.damage/4, 0);
                
                //NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, NPCID.VileSpitEaterOfWorlds, ai0: NPC.target, ai1: NPC.whoAmI);
            a.velocity = ShootDirection * 8f;

            SpitCooldown = 70;
            CurrentState = LarvaAi.TrackPlayer;
        }


        public override void AI()
        {
            bool isHead = ((int)segmentNum == 0);

            if (isHead)
            {
                HeadAI();
                if(LungeCooldown > 0)
                {
                    LungeCooldown--;
                }
                if(SpitCooldown > 0)
                {
                    SpitCooldown--;
                }
            }
           

            NPC.rotation = NPC.velocity.ToRotation();

           
        }
        public override void PostAI()
        {
            bool isHead = ((int)segmentNum == 0);
            // Keep the position history length reasonable: cap based on segmentCount
            if (isHead)
            {
                int headId = NPC.whoAmI;
                int needed = (int)(segmentCount * HISTORY_PER_SEGMENT) + 10;
                List<Vector2> hist = _headPositionHistory[headId];

                // push current position at front
                hist.Insert(0, NPC.Center);

                if (hist.Count > needed)
                    hist.RemoveRange(needed, hist.Count - needed);
            }
            else
            {
                BodyAI();

            }
        }
        private void HeadAI()
        {
            StateMachine();
            Time++;
        }
        
        private void BodyAI()
        {
            int headId = NPC.realLife;

            // If realLife is invalid, fallback to following predecessor chain stored in ai[0]
            if (headId < 0 || headId >= Main.maxNPCs || !Main.npc[headId].active)
            {
                // fallback - if predecessor invalid, die gracefully
                int pred = (int)NPC.ai[0];
                if (pred < 0 || pred >= Main.maxNPCs || !Main.npc[pred].active)
                {
                    NPC.active = false;
                    NPC.netUpdate = true;
                    return;
                }
                FollowPredecessor(pred);
                return;
            }

            // If there's no recorded history for the head yet, fallback to predecessor-follow
            if (!_headPositionHistory.ContainsKey(headId) || _headPositionHistory[headId].Count < 2)
            {
                int pred = (int)NPC.ai[0];
                FollowPredecessor(pred);
                return;
            }

            List<Vector2> hist = _headPositionHistory[headId];

            // distance behind the head this segment should be
            float distanceBehind = (int)segmentNum * SEGMENT_DISTANCE;

            // sample a point along the history at cumulative distance = distanceBehind
            Vector2 samplePoint;
            bool found = SamplePointAlongHistory(hist, distanceBehind, out samplePoint);

            if (!found)
            {
                // not enough history length to find that far back -> use predecessor snap/follow
                int pred = (int)NPC.ai[0];
                FollowPredecessor(pred);
                return;
            }

            // set position & smooth velocity
            Vector2 old = NPC.Center;
            NPC.Center = samplePoint;
            NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.Center - old, 0.8f);
        }

        #region Helpers

        // follow predecessor by pulling this NPC towards desired distance behind predecessor
        private void FollowPredecessor(int predIndex)
        {
            if (predIndex < 0 || predIndex >= Main.maxNPCs || !Main.npc[predIndex].active)
            {
                NPC.active = false;
                NPC.netUpdate = true;
                return;
            }

            NPC pred = Main.npc[predIndex];
            float desiredDist = SEGMENT_DISTANCE;
            Vector2 dir = NPC.Center - pred.Center;
            float curDist = dir.Length();

            if (curDist == 0f)
            {
                NPC.Center += new Vector2(0.01f, 0.01f);
                dir = NPC.Center - pred.Center;
                curDist = dir.Length();
            }

            float diff = curDist - desiredDist;
            if (Math.Abs(diff) > 0.01f)
            {
                dir /= curDist;
                NPC.Center -= dir * diff;
            }

            NPC.rotation = (NPC.Center - pred.Center).ToRotation() + MathHelper.PiOver2;
            NPC.velocity = Vector2.Lerp(NPC.velocity, pred.velocity, 0.9f);
        }

        // Walk the head position history (newest->oldest) looking for a point at exactly `distance` pixels
        // behind the head along the recorded polyline. Returns true if found and writes to `outPoint`.
        /// <summary>
        /// attempts to sample a point along the provided history of positions at a specified cumulative distance.
        /// </summary>
        /// <param name="historyNewestFirst"></param>
        /// <param name="distance"></param>
        /// <param name="outPoint"></param>
        /// <returns></returns>
        private bool SamplePointAlongHistory(List<Vector2> historyNewestFirst, float distance, out Vector2 outPoint)
        {
            outPoint = Vector2.Zero;
            if (historyNewestFirst == null || historyNewestFirst.Count < 2)
                return false;

            float accum = 0f;
            for (int i = 0; i < historyNewestFirst.Count - 1; i++)
            {
                Vector2 p0 = historyNewestFirst[i];
                Vector2 p1 = historyNewestFirst[i + 1];
                float segLen = Vector2.Distance(p0, p1);

                if (accum + segLen >= distance)
                {
                    float need = distance - accum;
                    if (segLen <= 0.0001f)
                    {
                        outPoint = p0;
                    }
                    else
                    {
                        float t = need / segLen; // 0..1
                        outPoint = Vector2.Lerp(p0, p1, t);
                    }
                    return true;
                }
                accum += segLen;
            }

            // If we exit loop, we don't have enough recorded distance. Use the oldest point as fallback.
            // Return false so caller can fallback to predecessor-follow (or place at end of history).
            return false;
        }
        #endregion
        public override bool CheckActive()
        {
            return false; 
        }

        public override void OnKill()
        {
            // If head died, optionally purge its history and optionally kill segments.
            if ((int)segmentNum == 0)
            {
                int headId = NPC.whoAmI;
                if (_headPositionHistory.ContainsKey(headId))
                    _headPositionHistory.Remove(headId);

                // Optionally kill child segments (uncomment if desired)
                //for (int i = 0; i < Main.maxNPCs; i++)
                //{
                //    NPC n = Main.npc[i];
                //    if (n.active && n.realLife == headId)
                //        n.active = false;
                //}
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D larva = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/Umbralarva").Value;
            Vector2 DrawPos = NPC.Center - Main.screenPosition;
            Vector2 Orig = new Vector2(larva.Width / 3 / 2, larva.Height / 2);
            int value = segmentNum == 0 ? 0 : segmentNum == segmentCount ? 2 : 1;
            Rectangle larvaFrame = larva.Frame(3, 1, value, 0);

            Main.EntitySpriteDraw(larva, DrawPos, larvaFrame, drawColor, NPC.rotation, Orig, 1, SpriteEffects.FlipHorizontally);


            if(segmentNum == 0)
            {
                Utils.DrawBorderString(spriteBatch, CurrentState.ToString(), DrawPos + Vector2.UnitY * 100, Color.AntiqueWhite);
            }

            //Utils.DrawBorderString(Main.spriteBatch, segmentNum.ToString(), DrawPos, Color.AntiqueWhite);
            return false;
        }

        // Build debug info by scanning NPC list for segments that reference this head via realLife
        public (int headID, List<int> segmentNpcIds, List<Vector2> positions) GetWormDebugInfo()
        {
            int headID = NPC.whoAmI;
            List<int> ids = new List<int>();
            List<Vector2> poss = new List<Vector2>();

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (n != null && n.active && n.realLife == headID)
                {
                    ids.Add(i);
                    poss.Add(n.Center);
                }
            }

            return (headID, ids, poss);
        }
    }
    
    public class BloodSpat : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.friendly = false;

            Projectile.width = Projectile.height = 14;
            Projectile.timeLeft = 300;

            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;


            

        }

        public override void AI()
        {
            Dust b = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Rain_BloodMoon, 0, 0, 0, Color.Crimson);

            Dust a = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Blood, 0, 0, 0, Color.Purple);
           

            a.velocity = Projectile.velocity;
            b.velocity = Projectile.velocity;

        }
        public override bool PreDraw(ref Color lightColor)
        {



            return false;
        }

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
    }
    
}
