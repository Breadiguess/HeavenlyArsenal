using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.Yharon;
using CalamityMod.Projectiles.Melee;
using HeavenlyArsenal.Content.Items.Armor.NewFolder;
using HeavenlyArsenal.Content.Items.Weapons.Ranged;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab;
using HeavenlyArsenal.Content.Tiles.Banners;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.RuntimeDetour;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles.Metaballs;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech
{
    public enum UmbralLeechAI
    {
        Idle = 0,
        SeekTarget = 1,
        FeedOnTarget = 2,
        FlyAway = 3,
        DeathAnim = 4
    }

    //public static int  = 40;
   //todo: make sure that all segments share the same defense
    class UmbralLeech : WormHead
    {

        public override void Init()
        {
            // Set the segment variance
            // If you want the segment length to be constant, set these two properties to the same value
            MinSegmentLength = 6;
            MaxSegmentLength = 12;

            CommonWormInit(this);
        }

        // This method is invoked from ExampleWormHead, ExampleWormBody and ExampleWormTail
        internal static void CommonWormInit(Worm worm)
        {
            // These two properties handle the movement of the worm
            worm.MoveSpeed = 5.5f;
            worm.Acceleration = 0.045f;
        }
        public override int BodyType => ModContent.NPCType<UmbralLeech_Body>();

        public override int TailType => ModContent.NPCType<UmbralLeech_Tail>();


        public static readonly SoundStyle Bash = new SoundStyle("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/UmbralLeech_Bash_", 3);

        private const int SegmentCount = 5;
        private int headWhoAmI;
        
        public ref float Time => ref NPC.ai[0];
        public ref float Latched => ref NPC.ai[1];
        public UmbralLeechAI CurrentState;

        // Detection ranges
        private const float PlayerDetectionRange = 600f;
        private const float NPCDetectionRange = 1000f;

        // Latch offset
        public ref float offsetX => ref NPC.localAI[0];
        public ref float offsetY => ref NPC.localAI[1];
        public Vector2 LatchOffset => new Vector2(offsetX, offsetY);

        // Blood tracking
        public int blood;
        public int bloodMax = 7;
        private const int FeedDuration = 180; // 3 seconds at 60 FPS

        // Target storage
        private Entity currentTarget;
        public static int[] ValidTargets = { ModContent.NPCType<AnAffrontToGod>(), ModContent.NPCType<Yharon>(),ModContent.NPCType<SuperDummyNPC>(), ModContent.NPCType<ArtilleryCrab>() };

        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech";
        public new string LocalizationCategory => "NPC.Hostile.Bloodmoon";

        public override void OnKill()
        {
            //DebugUtils.LogLine("UmbralLeech was killed");
            Main.NewText($"Umbral Leech was killed :/");
        }
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6;
            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech_Bestiary",
                Position = new Vector2(10f, 24f)
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
        }

        public override void SetDefaults()
        {
            NPC.height = NPC.width = 64;
            NPC.damage = 140;
            NPC.defense = 241;
            NPC.lifeMax = 10463;
            NPC.HitSound = GennedAssets.Sounds.Avatar.Chirp with { Volume = 0.1f, Pitch = 1f, PitchVariance = 0.5f };
            NPC.DeathSound = GennedAssets.Sounds.Common.LargeBloodSpill;
            NPC.value = 60f;
            NPC.knockBackResist = 1f;
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            Banner = Type;

            BannerItem = ModContent.ItemType<UmbralLeechBanner>();

            
            if (NPC.type == ModContent.NPCType<UmbralLeech>())
            {
                for (int i = 0; i < BuffLoader.BuffCount; i++)
                    NPC.buffImmune[i] = true;
            }
        }

        public override void OnSpawn(IEntitySource source)
        {
            Time = 0;
            CurrentState = UmbralLeechAI.Idle;
            headWhoAmI = NPC.whoAmI;
            int prev = headWhoAmI;
            // spawn body segments + tail
            for (int i = 0; i < SegmentCount; i++)
            {
                int type = i < SegmentCount - 1 ? ModContent.NPCType<UmbralLeech_Body>() : ModContent.NPCType<UmbralLeech_Tail>();
                int idx = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.position.X, (int)NPC.position.Y, type);
                NPC npcSegment = Main.npc[idx];
                // link prev->current, current.prev=prev
                npcSegment.localAI[2] = prev;
                Main.npc[prev].localAI[3] = idx;
                prev = idx;
            }
        }
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            // only draw for head
            return true;
        }
        public override void AI()
        {
            Main.NewText($"UmbralLeech State: {CurrentState.ToString()}" );
            Time++;
            if (NPC.life > 0)
            {
                switch (CurrentState)
                {
                    case UmbralLeechAI.Idle:
                        NPC.velocity *= 0.98f;
                        if (Time % 10 == 0)
                        {
                            // Try player first
                            var pl = FindClosestPlayer(PlayerDetectionRange);
                            if (pl != null)
                            {
                                currentTarget = pl;
                                NPC.target = pl.whoAmI;
                                CurrentState = UmbralLeechAI.SeekTarget;
                            }
                            else
                            {
                                var npc = FindClosestCustomNPC(NPCDetectionRange);
                                if (npc != null)
                                {
                                    currentTarget = npc;
                                    CurrentState = UmbralLeechAI.SeekTarget;
                                }
                            }
                        }
                        break;

                    case UmbralLeechAI.SeekTarget:
                        if (IsTargetInvalid())
                        {
                            CurrentState = UmbralLeechAI.Idle;
                            break;
                        }
                        MoveTowardsTarget(7f);
                        break;

                    case UmbralLeechAI.FeedOnTarget:
                        Main.NewText($"Feeding on target");
                        if (IsTargetInvalid())
                        {
                            CurrentState = UmbralLeechAI.FlyAway;
                            //NPC.dontTakeDamage = false;
                            Time = 0;
                            break;
                        }
                        // Stay latched
                        //todo: figure out why the fuck entering this state immediatley nullifies itself
                        NPC.Center = currentTarget.Center; //+ LatchOffset;
                        NPC.velocity = Vector2.Zero;
                        //NPC.frame.Y = feedingFrame * frameHeight; // TODO: uncomment when animation exists
                        if (Time % 30 == 0)
                        {
                            //blood++;
                            //NPC.HealEffect(-100);
                            //NPC.life = Math.Min(NPC.life + 100, NPC.lifeMax);
                            //bloodVFX(40);
                        }
                        if (blood >= bloodMax || Time >= FeedDuration)
                        {
                            DetachFromTarget();
                            CurrentState = UmbralLeechAI.FlyAway;
                            
                            Time = 0;
                        }
                        break;

                    case UmbralLeechAI.FlyAway:
                        if (Time <= 1)
                        {
                            Vector2 fleeDir = (NPC.Center - (currentTarget?.Center ?? NPC.Center)).SafeNormalize(Vector2.UnitY);
                            NPC.velocity = fleeDir * 6f;
                            // Flip sprite to flee direction
                            NPC.spriteDirection = fleeDir.X > 0 ? 1 : -1;
                        }
                        else
                        {
                            NPC.velocity *= 0.98f;
                        }
                        if (Time > 120)
                        {
                            CurrentState = UmbralLeechAI.Idle;
                            currentTarget = null;
                            Time = 0;
                        }
                        break;
                }
            }
            else
            {
                CurrentState = UmbralLeechAI.DeathAnim;
                // TODO: Death animation handling
            }

        }

        public override bool CheckDead()
        {
            
            if(CurrentState == UmbralLeechAI.DeathAnim)
            {
                return false;
            }
            else 
                return base.CheckDead();
            
            //return base.CheckDead();
        }
        private bool IsTargetInvalid()
        {
            if (currentTarget == null) return true;
            if (currentTarget is Player p && (p.dead || !p.active)) return true;
            if (currentTarget is NPC n && !n.active) return true;
            return false;
        }

        
        private void MoveTowardsTarget(float speed)
        {
            Vector2 toTarget = currentTarget.Center - NPC.Center;
            float dist = toTarget.Length();
            Vector2 dir = toTarget.SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, dir * speed, dist/1000f);
            NPC.spriteDirection = dir.X > 0 ? -1 : 1;
            if (dist < 30f)
            {
                //todo: make latch actually create and use latch offset
                SetupLatch(currentTarget);
                NPC.dontTakeDamage = true;
                CurrentState = UmbralLeechAI.FeedOnTarget;
                //Time = 0;
            }
        }

        private Player FindClosestPlayer(float maxDist)
        {
            Player best = null;
            float bestDist = maxDist;
            foreach (var p in Main.player)
            {
                if (!p.active || p.dead) continue;
                float d = Vector2.Distance(p.Center, NPC.Center);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = p;
                }
            }
            return best;
        }

        private NPC FindClosestCustomNPC(float maxDist)
        {
            NPC best = null;
            float bestDist = maxDist;
            foreach (var n in Main.npc)
            {
                if (!n.active) continue;
                if (!ValidTargets.Contains(n.type)) continue;
                float d = Vector2.Distance(n.Center, NPC.Center);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = n;
                }
            }
            return best;
        }

        public void bloodVFX(int n)
        {
            BloodMetaball metaball = ModContent.GetInstance<BloodMetaball>();
            for (int i = 0; i < n; i++)
            {
                Vector2 bloodSpawn = NPC.Center + new Vector2(10 * NPC.scale).RotatedBy(NPC.rotation);
                Vector2 bloodVel = (Main.rand.NextVector2Circular(20f, 20f) - NPC.velocity) * Main.rand.NextFloat(0.2f, 1.2f);
                metaball.CreateParticle(bloodSpawn, bloodVel, Main.rand.NextFloat(10f, 40f), Main.rand.NextFloat(2f));
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //debug text
            
            if (currentTarget != null)
                Utils.DrawBorderString(Main.spriteBatch, "Target: " + currentTarget.ToString(), NPC.Center - Vector2.UnitY * 130 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "State: " + CurrentState.ToString(), NPC.Center - Vector2.UnitY * 90 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "Blood: " + blood.ToString(), NPC.Center - Vector2.UnitY * 110 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "Rotation: " + NPC.rotation.ToString(), NPC.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.AntiqueWhite);
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            
            Rectangle frame = texture.Frame(1, 6, 0, (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3);
            Vector2 origin = new Vector2(texture.Frame().Width/2, texture.Height/6/2);
            SpriteEffects effect = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            spriteBatch.Draw(texture, NPC.Center - screenPos, frame, drawColor, NPC.rotation - MathHelper.PiOver2, origin, 1f, effect, 0f);
            return false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.bloodMoon && DownedBossSystem.downedYharon)
                return SpawnCondition.OverworldNightMonster.Chance * 0.1f;
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            bloodVFX(10);
            //blood++;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BloodOrb>(), 1, 1, 3));
            base.ModifyNPCLoot(npcLoot);
        }


        private void DetachFromTarget()
        {
            //NPC.dontTakeDamage = false;
            if (currentTarget is Player p)
                p.Hurt(PlayerDeathReason.ByNPC(NPC.whoAmI), blood * 5 + NPC.damage, 0);
            else if (currentTarget is NPC n)
            {
                n.SimpleStrikeNPC((int)Math.Pow(blood,7), 0, false, 1, DamageClass.Generic, true, 0, true);
                n.GetGlobalNPC<UmbralInfection>().IsInfected = true;
                n.GetGlobalNPC<UmbralInfection>().Time=0;
            }

               
            SoundEngine.PlaySound(Bash with { MaxInstances = 1, PitchVariance = 0.5f}, NPC.Center);
        }

        private void SetupLatch(Entity target)
        {
            //offsetX = NPC.Center.X - target.Center.X;
            //offsetY = NPC.Center.Y - target.Center.Y;
            NPC.velocity = Vector2.Zero;
            blood = 0;
            Time = 0;
        }

        public override void SendExtraAI(BinaryWriter writer) => writer.Write(blood);
        public override void ReceiveExtraAI(BinaryReader reader) => blood = reader.ReadInt32();


    }
    // Body segment class for worm
    public class UmbralLeech_Body : WormBody
    {
        public int HeadType => ModContent.NPCType<UmbralLeech>();
        public int TailType => ModContent.NPCType<UmbralLeech_Tail>();
        public int BodyType => ModContent.NPCType<UmbralLeech_Body>();
        // References to the previous and next segments
        private int NextSegment => (int)NPC.localAI[3];
    
        private int PrevSegment => (int)NPC.localAI[2];
       
        public override void Init()
        {
           UmbralLeech.CommonWormInit(this);
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
            };
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
            NPCID.Sets.RespawnEnemyID[NPC.type] = ModContent.NPCType<UmbralLeech>();
        }
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech_Body";

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.DiggerBody);
            NPC.aiStyle = -1;

            // Extra body parts should use the same Banner value as the main ModNPC.
            Banner = ModContent.NPCType<UmbralLeech>();
        }

     
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle frame = texture.Frame(1, 6, 0, (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3);
            Vector2 origin = new Vector2(texture.Frame().Width / 2, texture.Height / 6 / 2);
            SpriteEffects effect = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            spriteBatch.Draw(texture, NPC.Center - screenPos, frame, drawColor, NPC.rotation + MathHelper.PiOver2, origin, 1f, effect, 0f);
            return false;
        }




    }

    // Tail segment class for worm
    public class UmbralLeech_Tail :WormTail
    {
       

        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech_Tail";

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
            NPCID.Sets.RespawnEnemyID[NPC.type] = ModContent.NPCType<UmbralLeech>();
        }

        public override void SetDefaults()
        {
            NPC.CloneDefaults(NPCID.DiggerTail);
            NPC.aiStyle = -1;

            NPC.defDefense = 300;// ModContent.NPCType<UmbralLeech>();

            // Extra body parts should use the same Banner value as the main ModNPC.
            Banner = ModContent.NPCType<UmbralLeech>();
        }

        public override void Init()
        {
            UmbralLeech.CommonWormInit(this);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle frame = texture.Frame(1, 6, 0, (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3);
            Vector2 origin = new Vector2(0, texture.Height / 6/1.5f);
            //todo: refer to the head for sprite direction
            SpriteEffects effect = NPC.spriteDirection == 1 ? SpriteEffects.FlipVertically : SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(texture, NPC.Center - screenPos, frame, drawColor, NPC.rotation + MathHelper.PiOver2, origin, 1f, effect, 0f);
            return false;
        }
    }
    class UmbralInfection: GlobalNPC
    {
        public bool IsInfected { get; set; } = false;
        public override bool InstancePerEntity => true;

       
        public override void SetDefaults(NPC npc)
        {
            if (npc.type == ModContent.NPCType<UmbralLeech>())
            {
                IsInfected = false;
            }
        }
        public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers)
        {
            
            base.ModifyHitNPC(npc, target, ref modifiers);
        }

        public float Time;
        public override void ResetEffects(NPC npc)
        {
            if (npc.type == ModContent.NPCType<UmbralLeech>())
            {
                npc.GetGlobalNPC<UmbralInfection>().IsInfected = false;
            }
        }
        public override bool PreAI(NPC npc)
        {
            if (IsInfected)
            {
                // Apply 20% buff to stats (excluding max life)
                npc.defDamage = (int)(npc.damage * 1.2f);
                npc.defDefense = (int)(npc.defense * 1.4f);
                npc.ForcePartyHatOn = true;
                npc.knockBackResist *= 1.2f;
            }
            return base.PreAI(npc);
        } 
        public override void PostAI(NPC npc)
        {
            if (IsInfected)
            {
               
                if (Time > 400)
                {
                    npc.SimpleStrikeNPC((int)Math.Pow(400, 1.3), 0, false, 1, DamageClass.Generic, true, 0, true);
                    IsInfected = false;
                    Time = 0;
                }
                Time++;
            }
            base.PostAI(npc);
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.type != ModContent.NPCType<UmbralLeech>() && npc.type != ModContent.NPCType<UmbralLeech_Body>() && npc.type != ModContent.NPCType<UmbralLeech_Tail>() || IsInfected)
            {
                //Utils.DrawBorderString(Main.spriteBatch, "is Infected: " + IsInfected.ToString(), npc.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.White);
                //Utils.DrawBorderString(Main.spriteBatch, "time: " + Time.ToString(), npc.Center - Vector2.UnitY * 180 - Main.screenPosition, Color.White);

            }
            base.PostDraw(npc, spriteBatch, screenPos, drawColor);
        }
    }


}
