using CalamityMod;
using CalamityMod.NPCs.Providence;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Content.Particles.Metaballs;
using NoxusBoss.Core.World.WorldSaving;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech
{
    public enum UmbralLeechAI
    {
        Idle,
        SeekTarget,
        FeedTelegraph,
        FeedOnTarget,
        DisipateIntoBlood,
        FlyAway,
        DeathAnim
    }

    public class NewLeech : ModNPC
    {
       
        #region StolenCode
        /*
                                                 .:-:                             
                                               .--.--                             
                                              :=-:.==                             
                                     .:::.. .:-:-=:=-                             
                                     .:.::::....-=:::                             
                                     .:-::::::. =-.:-.                            
                                       ::.      ..:.:=-.                          
                                        .-:.      ..:..=:                         
                                          .. .:-+=::=+=.                          
                                           .-::-=-:..                             
                                          .:-:.                                   
                                     .:::.:+=:::.                                 
                              .:==++++-::-=+-::-+++==-:                           
                            .:=+=----::.....      ..:===:.                        
                        .::=++==+**+==:....           .:=+-::.                    
                        :-====**=:.:::-:. .:.   .:::..   .:=-.                    
                      .-+=-=++-... .:   :+##*-.   .. ...    :=:.                  
                     .-+---..:....::   -%%=.=#+:..    ...    .=:.                 
                     :+--........:. .-#%##+. :*%+::--:...     .-:                 
                    .--:...:..:....-*#%%%%*:  .-*#+:..:--:     :..                
                    .==. .:.::..:=##*#%%%#+:    .-+*+-:.:=:    :-.                
                   .-=-  .::--+#%%%@@%#%%*=:       .-+**+*=.   :=-.               
                  .::=-..:+%@####%%%##%##**+:         .-+%%-   =-::.              
                    .-*:.:#@@##%%%%**+++++*-.            *%:..-+-.                
                     :%%@#*%@%%%#%#*+==++**+-    .....  .*%=#@%#-                 
                      -@%#%@@%%%@@%@@@@@%%+:--=@@@@@@@+..#@=-+%-                  
                      .*#%#@%%%**@@@@@@@%*=:...*@@@@@+. :#%=*%+...                
                    ....+%%#%@%**#%%##*-=*=.::..:---:.  -#+*#=..                  
                        :-+@@@%#**###*-:+#=.    .  .... =%%+..                    
                        .-=#@@######**+#%#=.       .:. .*@*-.                     
                        .:=*%@%%##%%###%%#=..:.   ...  :#*==:.                    
                        .--##@@%#**%%%%%%@@%+:.   .:. :#@+==-.                    
                         .-*#@@@###@%%%%####=:....:: -%@%+*=.                     
                        .-:****%@%#%%%%#+:..:-:  .. =#*=+++-::.                   
                        ...-**+-+@@@##%###*+=:.  .=%@+.=+=:...                    
                           .-*++*%#@@@%#%%#-   :*%#=*=.--:.                       
                            -#+:+@##%@@@%#*++*#%#=..**:--..                       
                         .:=*@@%%@%%%%%#*+=+==:.   .*%#%%#=:.                     
        */
        #endregion
        #region setup
        /// <summary>
        /// A general‐purpose timer. Used by segments to wobble and by death animation.
        /// </summary>
        public ref float Time => ref NPC.ai[0];

        public ref float WiggleTime => ref NPC.localAI[0];
        /// <summary>
        /// Reference into NPC.ai[0]: stores the NPC ID of the head segment.
        /// All non‐head segments use this to know which NPC is the head.
        /// </summary>
        public ref float HeadID => ref NPC.ai[1];

        /// <summary>
        /// For child segments: the index (1, 2, 3, …) of this segment in the chain.
        /// For the head, SegmentNum is 0.
        /// </summary>
        public ref float SegmentNum => ref NPC.ai[2];

        /// <summary>
        /// Total number of segments (not counting head). Stored in NPC.localAI[0] for head,
        /// and copied to each spawned segment’s localAI[0] so that everyone knows the count.
        /// </summary>
        public int SegmentCount;

        /// <summary>
        /// Current AI state (Idle, Seek, Feed, etc.)
        /// </summary>
        private UmbralLeechAI CurrentState;

        /// <summary>
        /// The player or other entity we’re currently “feeding on” or chasing.
        /// </summary>
        private Entity currentTarget;

        /// <summary>
        /// Keeps track of each segment’s “ideal” position in world‐coordinates,
        /// so that body/tail segments smoothly interpolate/follow the segment in front.
        /// We fill this list in AI() each tick.
        /// </summary>
        private List<Vector2> SegmentPositions;

        private List<NPC> Segments;
        /// <summary>
        /// Only the head will fill this every tick.
        /// headPositions[0] = head.Center,
        /// headPositions[i] = “ideal” world position for the i-th segment (1..SegmentCount).
        /// </summary>
        public Vector2[] headPositions;

        private List<Vector2> WhiskerAnchors;

        private int DashCount;

        // This boolean toggles “feeding mode” so bodies know to lash harder.
        private bool isFeeding = false;

        public static readonly SoundStyle Bash = new SoundStyle(
            "HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/UmbralLeech_Bash_", 3
        );

        public static readonly SoundStyle Explode = new SoundStyle(
            "HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/GORE - Head_Crush_3"
        );

        public static readonly SoundStyle GibletDrop = new SoundStyle(
            "HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/GORE - Giblet_Drop_3"
        );

        public static readonly SoundStyle DeathNoise = new SoundStyle("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/Death", 3);
        public static readonly SoundStyle DyingNoise = new SoundStyle("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/Dying1");

        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech";

        public override void SetStaticDefaults()
        {
            // Prevent this NPC from dropping coins or souls, and from despawning to inactivity.
            NPCID.Sets.CantTakeLunchMoney[Type] = true;
            NPCID.Sets.CannotDropSouls[Type] = true;
            NPCID.Sets.DoesntDespawnToInactivityAndCountsNPCSlots[Type] = true;
            
        }

        public override void SetDefaults()
        {
            NPC.width = 30;
            NPC.height = 30;
            NPC.lifeMax = 50000;
            NPC.damage = 200;
            NPC.defense = 230;
            NPC.npcSlots = (NPC.whoAmI == Main.npc[(int)HeadID].whoAmI) ? 1 : 0;
            NPC.noGravity = true;
            NPC.aiStyle = -1;   
            CurrentState = UmbralLeechAI.Idle;
            NPC.knockBackResist = 0;
            
            SegmentPositions = new List<Vector2>();
        }

        public override void Load()
        {
            TailTexture = AssetDirectory.Textures.UmbralLeechTendril;
            //WhiskerTexture = AssetDirectory.Textures.UmbralLeechWhisker;
        }
       
        public float feedtime = 0;
      
        public override void OnSpawn(IEntitySource source)
        {
           
            int count;
            if (SegmentNum == 0)
            {
                //set up whisker locations
                
                //WhiskerAnchors.Add(new Vector2(NPC.Center.X, NPC.Center.Y));
                // Head segment: choose random segment count and store in localAI[0]
                count = Main.rand.Next(6, 16);
                SegmentCount = count;
                NPC.localAI[0] = count;

                HeadID = NPC.whoAmI;


                
                headPositions = new Vector2[count + 1]; 

               
                SegmentPositions = new List<Vector2>(new Vector2[count + 1]);
                Segments = new List<NPC>(new NPC[count + 1]);
                // For each index i = 1..count, spawn a body segment.
                for (int i = 1; i <= count; i++)
                {
                    int newNPC = NPC.NewNPC(
                        NPC.GetSource_FromAI(),
                        (int)SegmentPositions[i].X,
                        (int)SegmentPositions[i].Y,
                        Type,
                        ai0: 0,        
                        ai1: HeadID,   // HeadID
                        ai2: i         // SegmentNum
                    );

                    if (newNPC >= 0 && newNPC < Main.maxNPCs)
                    {
                        Main.npc[newNPC].localAI[0] = count;
                        Segments.Add(Main.npc[newNPC]);
                    }
                }
            }
            else
            {
                
                SegmentCount = (int)NPC.localAI[0];
                if (SegmentCount == 0 && HeadID >= 0 && HeadID < Main.maxNPCs)
                {
                    NPC head = Main.npc[(int)HeadID];
                    NPC Body = Main.npc[NPC.whoAmI];
                    if (head.active && head.type == Type)
                        SegmentCount = (int)head.localAI[0];
                    Body.realLife = head.whoAmI;
                }
            }
        }

       
        private Vector2[] TailPoints;
        private Vector2[] TailVels;
        #endregion
        public void RibbonPhysics()
        {
           //todo: make the tendrils more floaty and 
            int length = SegmentCount;
            if (TailVels != null)
            {
                for (int i = 0; i < TailVels.Length; i++)
                {
                    TailVels[i] = (NPC.rotation  * NPC.spriteDirection).ToRotationVector2() * MathHelper.Pi;
                }
            }
            else
            {
                TailVels = new Vector2[length];
            }

            if (TailPoints != null)
            {// Improved tail simulation using Verlet integration and spring-damper physics
                float drawScale = NPC.scale;
                float maxCycle = 800f;
                float t = WiggleTime % maxCycle;
                float flare = (float)Math.Sin(Math.PI * t / maxCycle);
                float flareAmount = MathHelper.Lerp(0.8f, 1.3f, Math.Abs(flare));

                // Anchor the first point to the NPC's base
                TailPoints[0] = NPC.Center + new Vector2(40 * flareAmount, -50 * NPC.spriteDirection).RotatedBy(NPC.rotation);

                // Tail simulation parameters
                float segmentLength = 10f * flareAmount;
                float springiness = 0.18f; // how strongly each segment follows the previous
                float damping = 1f;     // how much velocity is preserved (lower = more floppy)

                for (int i = 1; i < TailPoints.Length; i++)
                {
                    // Verlet integration: update position based on velocity
                    TailVels[i] *= damping;
                    TailPoints[i] += TailVels[i];

                    // Calculate the vector from this point to the previous
                    Vector2 toPrev = TailPoints[i - 1] - TailPoints[i];
                    float dist = toPrev.Length();
                    if (dist > 0.0001f)
                    {
                        Vector2 dir = toPrev / dist;
                        float diff = dist - segmentLength;
                        // Spring force: move this point toward/away from the previous to maintain segmentLength
                        TailPoints[i] += dir * diff * springiness;
                        // Add a bit of the spring force to velocity for more natural motion
                        TailVels[i] += dir * diff * springiness * 0.5f;
                    }

                    // Optional: add a small sine-based wiggle for organic movement
                    float wigglePhase = WiggleTime * 0.12f + i * 0.5f;
                    float wiggleMag = MathHelper.Lerp(0.5f, 2.5f, (float)i / TailPoints.Length);
                    Vector2 wiggle = new Vector2(0, (float)Math.Sin(wigglePhase) * wiggleMag);
                    TailPoints[i] += wiggle.RotatedBy(NPC.rotation);
                }
                TailPoints[0] = NPC.Center + new Vector2(4, -5 * NPC.spriteDirection).RotatedBy(NPC.rotation) * drawScale * flareAmount;

                for (int i = 1; i < TailPoints.Length; i++)
                {
                    TailPoints[i] += TailVels[i];
                    if (TailPoints[i].Distance(TailPoints[i - 1]) > 10)
                    {
                        TailPoints[i] = Vector2.Lerp(
                            TailPoints[i],
                            TailPoints[i - 1] + new Vector2(10 * flareAmount, 0).RotatedBy(TailPoints[i - 1].AngleTo(TailPoints[i])),
                            0.8f
                        );
                    }
                    
                }
            }
            else
            {
                TailPoints = new Vector2[length];
                for (int i = 0; i < TailPoints.Length; i++)
                {
                    TailPoints[i] = NPC.Center;
                }
            }
        }
        #region AI
        private const float PlayerDetectionRange = 4000f;

        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override void AI()
        {
            //Actual AI

            if (NPC.life > 1)
            {
                switch (CurrentState)
                {
                    case UmbralLeechAI.Idle:
                        // Look for a player
                        Player p = FindClosestPlayer(PlayerDetectionRange);
                        if (p != null)
                        {
                            currentTarget = p;
                            NPC.target = p.whoAmI;
                            CurrentState = UmbralLeechAI.SeekTarget;
                        }
                        break;

                    case UmbralLeechAI.SeekTarget:
                        if (currentTarget != null && currentTarget.active && SegmentNum == 0)
                        {
                            Vector2 toTarget = currentTarget.Center - NPC.Center;
                            NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Normalize(toTarget) * (8f + (float)SegmentCount / 12), 0.05f);

                            if (Vector2.Distance(NPC.Center, currentTarget.Center) < 170f && DashCount > 0)
                            {
                                CurrentState = UmbralLeechAI.FeedTelegraph;
                                Time = 0f;
                                feedtime = 0;
                            }
                            else if (Vector2.Distance(NPC.Center, currentTarget.Center) < 20 && DashCount <= 0)
                            {
                                CurrentState = UmbralLeechAI.FeedOnTarget;
                                Time = 0f;
                                feedtime = 0;
                            }
                        }
                        else
                        {
                            CurrentState = UmbralLeechAI.Idle;
                        }
                        break;

                    case UmbralLeechAI.FeedTelegraph:

                        ManageTelegraphDash();

                        break;

                    case UmbralLeechAI.FeedOnTarget:

                        if (currentTarget != null && currentTarget.active && SegmentNum == 0)
                        {
                            isFeeding = true;
                            NPC.dontTakeDamage = true;


                            Vector2 toPlayer = currentTarget.Center - NPC.Center;
                            NPC.Center = Vector2.Lerp(NPC.Center, currentTarget.Center, 0.5f);
                            Vector2 dir = Vector2.Normalize(toPlayer);
                            Vector2 perp = dir.RotatedBy(MathHelper.PiOver2);
                            float shake = (float)Math.Sin(Math.PI * Time / 4f) * 1f;
                            NPC.Center += perp * shake;
                            if (feedtime > 60f)
                            {
                                NPC.dontTakeDamage = false;
                                Time = 0f;
                                CurrentState = UmbralLeechAI.FlyAway;
                            }
                            feedtime++;
                        }
                        else
                        {
                            CurrentState = UmbralLeechAI.Idle;
                        }
                        break;

                    case UmbralLeechAI.FlyAway:
                        {
                            // Fly away from the player: pick a direction opposite to the current target
                            Vector2 awayDir = Vector2.Zero;
                            if (currentTarget != null && currentTarget.active)
                            {
                                awayDir = Vector2.Normalize(NPC.Center - currentTarget.Center);
                                if (awayDir.LengthSquared() < 0.01f)
                                    awayDir = Main.rand.NextVector2Unit();
                            }
                            else
                            {
                                awayDir = Main.rand.NextVector2Unit();
                            }
                            // Set a high velocity to quickly escape
                            float flySpeed = 10f + SegmentCount * 0.5f;
                            NPC.velocity = awayDir * flySpeed;
                            NPC.netUpdate = true;
                        }
                        if (Time > 60f)
                        {
                            Time = 0f;
                            CurrentState = UmbralLeechAI.Idle;
                            feedtime = 0;
                        }

                        break;

                    case UmbralLeechAI.DisipateIntoBlood:
                        DisipateIntoBlood();
                        break;

                    case UmbralLeechAI.DeathAnim:
                        NPC.damage = -1;
                        DoDeathAnimation();
                        break;
                }
            }


            if (SegmentNum == 0)
            {
                    // Define local offsets for whiskers relative to the head's center
                    Vector2[] localOffsets = new Vector2[]
                    {
                        new Vector2(4, NPC.directionY*-4),  
                        new Vector2(8, NPC.directionY*-15), 
                        new Vector2(18, NPC.directionY*-4), 
                        new Vector2(17, NPC.directionY * -15)
                    };

                    WhiskerAnchors = new List<Vector2>();
                    for (int i = 0; i < localOffsets.Length; i++)
                    {
                        Vector2 offset = localOffsets[i];
                        offset.Y *= NPC.direction;
                        Vector2 rotatedOffset = offset.RotatedBy(NPC.rotation);
                        Vector2 anchor = Main.npc[(int)HeadID].Center + rotatedOffset;
                        WhiskerAnchors.Add(anchor);
                    }
                
            }
          
            if (currentTarget != null && !isFeeding)
            {
                NPC.direction = Math.Sign(NPC.Center.X - currentTarget.Center.X);
            }
            //wiggle wiggle

            if (SegmentNum == 0f && NPC.life > 1)
            {
                int total = (int)SegmentCount;
                if (headPositions == null || headPositions.Length != total + 1)
                    headPositions = new Vector2[total + 1];

                
                headPositions[0] = NPC.Center;

                
                Vector2 headVelDir = Vector2.Zero;
                if (NPC.velocity.LengthSquared() > 0.001f)
                    headVelDir = Vector2.Normalize(NPC.velocity);
                NPC.rotation = NPC.velocity.ToRotation();

                
                if (CurrentState == UmbralLeechAI.FeedOnTarget && !isFeeding)
                {
                    isFeeding = true;
                }
                else if (CurrentState != UmbralLeechAI.FeedOnTarget && isFeeding)
                {
                    isFeeding = false;
                }

                

                
                float maxCycle = isFeeding ? 60f : 80f;
                if (WiggleTime > maxCycle)
                    WiggleTime -= maxCycle;

                //todo: while feeding, make the body segments fall into line behind the head, so that it more or less faces the same way as the head.
                for (int i = 1; i <= total; i++)
                {
                    Vector2 basePoint = headPositions[i - 1] - headVelDir * 40f;
                    Vector2 perp = headVelDir.RotatedBy(MathHelper.PiOver2);
                    
                    float phaseShift = i * (isFeeding ? 2.5f : 2f);
                    float sineValue;

                    if (isFeeding){sineValue = (float)Math.Sin(Math.PI * (WiggleTime + phaseShift) /1f); headPositions[i] = basePoint + perp * (sineValue * 18f); }
                    else {sineValue = (float)Math.Sin(Math.PI * (WiggleTime + phaseShift) / 10f); headPositions[i] = basePoint + perp * (sineValue * 18f);}
                }
            }
            if(TailPoints != null) 
                RibbonPhysics();
            WiggleTime++;
            //body
            if (SegmentNum >= 1f)
            {
                
                int headIndex = (int)HeadID;
                NPC.direction = Main.npc[headIndex].direction;
                if (headIndex < 0 || headIndex >= Main.maxNPCs || !Main.npc[headIndex].active)
                {
                    NPC.active = false;
                    return;
                }
                NewLeech head = (NewLeech)Main.npc[headIndex].ModNPC;
                WiggleTime = head.WiggleTime;
                int idx = (int)SegmentNum;
                int total = (int)head.SegmentCount;

                NPC precedingNPC = null;
                // The preceding segment is the one with SegmentNum == (this.SegmentNum - 1) and same HeadID
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC candidate = Main.npc[i];
                    if (candidate.active && candidate.type == Type && candidate.whoAmI != NPC.whoAmI)
                    {
                        if (candidate.ai[1] == HeadID && candidate.ai[2] == SegmentNum - 1)
                        {
                            precedingNPC = candidate;
                            break;
                        }
                    }
                }

                if (precedingNPC == null)
                {
                    NPC.active = false;
                    return;
                }

                if (head.headPositions == null || head.headPositions.Length != total + 1)
                {
                    // If something’s missing, just fall back to a simple snap
                    Vector2 fallbackDir = Main.npc[headIndex].Center - NPC.Center;
                    if (fallbackDir.Length() > 6f)
                        NPC.velocity = Vector2.Normalize(fallbackDir) * 109f;
                    return;
                }

                
                Vector2 toMe = NPC.Center - precedingNPC.Center;
                Vector2 dirAway;
                if (toMe.LengthSquared() > 0.001f)
                    dirAway = Vector2.Normalize(toMe);
                else
                    dirAway = Vector2.UnitY;

                Vector2 basePoint = precedingNPC.Center + dirAway * 25f;

                //shiggy with it time
                Vector2 prevVel = precedingNPC.velocity;
                Vector2 prevDir = prevVel.LengthSquared() > 0.005f ? Vector2.Normalize(prevVel) : Vector2.UnitX;
                Vector2 perp = prevDir.RotatedBy(MathHelper.PiOver2);
                float phaseShift = SegmentNum * (isFeeding ? 2.5f : 2.5f);
                float sineVal = (float)Math.Sin(Math.PI * (WiggleTime + phaseShift) / (isFeeding ? 5f : 10f));

              
                float baseAmplitude = isFeeding ? 5f : (SegmentCount)/5;
                float t = (float)idx / (float)total;
                // Use a smoothstep curve for tapering: 0 at ends, 1 in the middle
                // this is done becuase i think it looks good
                float taper = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((float)Math.Sin(Math.PI * t), 0f, 1f));
                float amplitude = baseAmplitude * taper;

                Vector2 wiggleOffset = perp * (sineVal * amplitude);

                Vector2 desired = basePoint + wiggleOffset;

                float lerpFactor = isFeeding ? 0.35f : 1f;
                NPC.Center = Vector2.Lerp(NPC.Center, desired, lerpFactor);
                NPC.rotation = (precedingNPC.Center - NPC.Center).ToRotation();
                NPC.life = Main.npc[headIndex].life;

               
                return;
            }
          
           


           
            if (NPC.life <= 2)
            {
                CurrentState = UmbralLeechAI.DeathAnim;
                DoDeathAnimation();

                int thisType = ModContent.NPCType<NewLeech>();
                float thisHeadID = HeadID;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.type == thisType)
                    {

                        float npcHeadID = npc.ai[1];
                        if (npcHeadID == thisHeadID)
                        {
                            if (npc.ModNPC is NewLeech leech)
                            {
                                leech.CurrentState = UmbralLeechAI.DeathAnim;
                                npc.life = 1;
                                npc.dontTakeDamage = true;
                                npc.netUpdate = true;
                            }
                        }
                    }
                }
            }

            if (WhiskerAnchors != null)
                ManageWhisker();

            if(DashCount < 3 && Time %55 == 0)
            {
                DashCount++;
            }
            Time++;
        }

        private bool Disipated = false;
        private void DisipateIntoBlood()
        {
            if (Time == 0)
            {
                NPC.hide = true;
                NPC.dontTakeDamage = true;
                NPC.noTileCollide = true;
            }


            if (Time >= 100)
            {
                NPC.hide = false;
                NPC.dontTakeDamage = false;
                for (int i = 0; i < 40; i++)
                {
                    for (int j = 0; j < 40; j++)
                    {
                        if (WorldGen.TileEmpty((int)NPC.Center.X + i, j))
                            NPC.noTileCollide = false;
                    }
                }
            }
        }


        private float TelegraphRot;
        /// <summary>
        /// Controls the telegraph dash for the leech.
        /// 
        /// </summary>
        private void ManageTelegraphDash()
        {
            if (Time == 0)
            {
                SoundEngine.PlaySound(Bash);
                DashCount--;
            }

            if (Time < 45f)
            {
                TelegraphRot = MathHelper.Lerp(TelegraphRot, NPC.rotation,0.1f);
                // Teleport to the target
                if (currentTarget != null && currentTarget.active)
                {

                    NPC.velocity *= 0.1f;
                }
                else
                {
                    CurrentState = UmbralLeechAI.Idle;
                }
            }
            if (Time > 45 && Time < 120)
            {
                if (Time % 5 == 0)
                {
                    SoundEngine.PlaySound(Bash);
                }
            }
            if (Time < 120f)
            {
                
                if (currentTarget != null && currentTarget.active)
                {
                    Vector2 toTarget = currentTarget.Center - NPC.Center;
                    float distance = toTarget.Length();
                    Vector2 dir = distance > 0.01f ? toTarget / distance : Vector2.Zero;

                    // Calculate a point beyond the player for overshoot
                    float overshootDistance = 60f; // How far past the player to aim
                    Vector2 overshootTarget = currentTarget.Center + dir * overshootDistance;

                    Vector2 toOvershoot = overshootTarget - NPC.Center;
                    Vector2 desiredVelocity = Vector2.Normalize(toOvershoot) * 20f;

                    // Add a small random angle to the dash for less predictability
                    float randomAngle = Main.rand.NextFloat(-0.15f, 0.15f);
                    desiredVelocity = desiredVelocity.RotatedBy(randomAngle);

                    NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, 0.1f);
                }
                
            }
            else
            {

                if (Vector2.Distance(NPC.Center, currentTarget.Center) < 70f)
                    CurrentState = UmbralLeechAI.FeedOnTarget;
                else
                    CurrentState = UmbralLeechAI.Idle;
               
            }
        }
       

        private void HandleIdle()
        {

        }
        private void HandleSeekTarget()
        {

        }
        private void HandleFeedOnTarget() 
        {
        
        }
        /// <summary>
        /// Return the closest active, alive player within maxDist, or null if none.
        /// </summary>
        private Player FindClosestPlayer(float maxDist)
        {
            Player best = null;
            float bestDist = maxDist;
            foreach (var p in Main.player)
            {
                if (!p.active || p.dead)
                    continue;
                float d = Vector2.Distance(p.Center, NPC.Center);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = p;
                }
            }
            return best;
        }



        public override bool CheckDead()
        {
            // Head should go into DeathAnim.
            NPC.life = 1;
            CurrentState = UmbralLeechAI.DeathAnim;


            

            NPC.dontTakeDamage = true;
            NPC.netUpdate = true;

            if (NPC.netSpam >= 10)
                NPC.netSpam = 9;

            return false;
        }

        private float DeathAnimationTimer;
        /// <summary>
        /// Plays a multi‐segment death sequence. Slows itself down, shakes, then finally explodes into gore.
        /// </summary>
        private void DoDeathAnimation()
        {
           
            WiggleTime = 0;
          
            for (int i = 0; i < SegmentCount; i++)
            {
                NPC.velocity *= 0.1f;
                SegmentPositions[i] += new Vector2();
            }
            //for(int i = 0; i< SegmentPositions.Count; i++)
            //{
                //SegmentPositions[i] += new Vector2(Main.rand.NextFloat(-10,10),Main.rand.NextFloat(-10,10));
            //}

            if (DeathAnimationTimer % 6 == 0)
            {
                BloodMetaball metaball = ModContent.GetInstance<BloodMetaball>();
                for(int u = 0; u < SegmentPositions.Count; u++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 bloodSpawnPosition = SegmentPositions[u];//Main.npc[Segments[u].whoAmI].Center;
                        Vector2 bloodVelocity = (Main.rand.NextVector2Circular(30f, 30f) - NPC.velocity) * Main.rand.NextFloat(0.2f, 1.2f);
                        metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(10f, 40f), Main.rand.NextFloat(2f));
                    }
                }
               
            }
            if (DeathAnimationTimer == 1 && Main.netMode != NetmodeID.Server && NPC.whoAmI == Main.npc[(int)HeadID].whoAmI)
            {
                    SoundEngine.PlaySound(DyingNoise with { MaxInstances = 1 });
                    
               
            }

            if (DeathAnimationTimer == 92f)
            {
            }

            if (Main.netMode == NetmodeID.Server && DeathAnimationTimer % 45f == 44f)
            {
                NPC.netUpdate = true;
                if (NPC.netSpam >= 10)
                    NPC.netSpam = 9;
            }

            if (DeathAnimationTimer >= 220f)
            {
                
                BloodMetaball metaball = ModContent.GetInstance<BloodMetaball>();
                for (int i = 0; i < 120; i++)
                {
                    Vector2 bloodSpawnPosition = NPC.Center + new Vector2(NPC.scale).RotatedBy(NPC.rotation);
                    Vector2 bloodVelocity = (Main.rand.NextVector2Circular(8f, 8f) - NPC.velocity) * Main.rand.NextFloat(0.2f, 1.2f);
                    metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(10f, 40f), Main.rand.NextFloat(2f));
                }
                SoundEngine.PlaySound(DeathNoise with { MaxInstances = 0}, NPC.Center);

                //createGore();
                
                NPC.HitEffect();
                NPC.NPCLoot();
                NPC.active = false;
               
                NPC.netUpdate = true;

                if (NPC.netSpam >= 10)
                    NPC.netSpam = 9;
            }

            DeathAnimationTimer++;
            //Main.NewText($"DeathTimer: {DeathAnimationTimer}");
        }

        
        public static Asset<Texture2D>[] UmbralLeechGores
        {
            get;
            private set;
        }
        private void GetGoreInfo(out Texture2D texture, out int goreID)
        {
            texture = null;
            goreID = 0;
            if (Main.netMode != NetmodeID.Server)
            {
                int variant = 0;
               
                if (SegmentNum == 0)
                {
                    // Head segment
                    variant = 0;
                }
                else if (SegmentNum == SegmentCount)
                {
                    // Tail segment
                    variant = UmbralLeechGores.Length - 1;
                }
                else
                {
                    // Body segment: pick a random variant (excluding head and tail)
                    variant = Main.rand.Next(1, UmbralLeechGores.Length - 1);
                }

                texture = UmbralLeechGores[variant].Value;
                goreID = ModContent.Find<ModGore>(Mod.Name, $"UmbralLeechGores{variant + 1}").Type;
            }
        }
        private void createGore()
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            //thanks lucille
            GetGoreInfo(out _, out int goreID);

            Gore.NewGore(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, goreID, NPC.scale);
        }
        public override bool PreKill()
        {
            return false;
        }
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (SegmentNum == 0f && !isFeeding)
            {
                // Near head: apply small knockback.
                NPC.velocity -= NPC.DirectionTo(item.Center) * item.knockBack * 0.1f;
            }

        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (SegmentNum == 0f && !isFeeding)
            {
                NPC.velocity -= NPC.SafeDirectionTo(projectile.Center, Vector2.Zero) * projectile.knockBack * 0.2f;//NPC.DirectionTo(projectile.Center) * projectile.knockBack * 0.2f;
            }

        }
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (CurrentState == UmbralLeechAI.FeedTelegraph && Time > 45)
            {
                CurrentState = UmbralLeechAI.FeedOnTarget;
            }
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.bloodMoon && BossDownedSaveSystem.HasDefeated<Providence>())
                return SpawnCondition.OverworldNightMonster.Chance * 0.1f;
            return 0f;

        }

        #endregion

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
           
            if(NPC.whoAmI == Main.npc[(int)HeadID].whoAmI)
            {
                
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<UmbralLeechDrop>(), 10,default,2));
            }
            
        }

        /// <summary>
        /// stores my silly whisker's rotations
        /// </summary>
        private List<float> WhiskerRot;
        private List<int> WhiskerFrame;
        
        /// <summary>
        /// Controls the whiskers.
        /// if the whisker rot list is empty (such as when the leech first initializes), then it populates it.
        /// </summary>
        private void ManageWhisker()
        {
            if (WhiskerFrame == null)
            {
                WhiskerFrame = new List<int>();
                for (int i = 0; i < WhiskerAnchors.Count; i++)
                {
                    WhiskerFrame.Add(Main.rand.Next(0, WhiskerFrameCount) + 1);
                }
            }
            if(WhiskerFrame != null && Time % 60*4 == 0) { 
                WhiskerFrame.Clear();
                for (int i = 0; i < WhiskerAnchors.Count; i++)
                {
                    WhiskerFrame.Add(Main.rand.Next(0, WhiskerFrameCount)+1 );
                }
            }
            if (WhiskerRot == null)
            {
                WhiskerRot = new List<float>();
                for (int i = 0; i< WhiskerAnchors.Count; i++)
                {
                    WhiskerRot.Add(0);
                }
            }
            else
            {
                
                for (int i = 0; i < WhiskerRot.Count; i++)
                {
                    //todo: ensure that rotations do not get weird as the head rotates.


                    // Calculate the center point of all whisker anchors
                    Vector2 center = Vector2.Zero;
                    for (int j = 0; j < WhiskerAnchors.Count; j++)
                        center += WhiskerAnchors[j];
                    center /= WhiskerAnchors.Count;

                    // Find the closest player and their distance
                    float playerDist = float.MaxValue;
                    Player closest = null;
                    foreach (var p in Main.player)
                    {
                        if (!p.active || p.dead) continue;
                        float d = Vector2.Distance(p.Center, NPC.Center);
                        if (d < playerDist)
                        {
                            playerDist = d;
                            closest = p;
                        }
                    }

                    Dust.NewDustPerfect(center, DustID.Cloud, Vector2.Zero, 0, default, 0.25f);

                    float phaseShift = (WiggleTime * 0.08f) + ((i % 2 == 0 ? 1 : 0) * MathHelper.PiOver2);
                    float amplitude = 0.5f; 
                    float baseAngle = Main.npc[(int)HeadID].velocity.ToRotation() * NPC.directionY;
                    float wiggle = (float)Math.Sin(phaseShift) * amplitude + Main.rand.NextFloat(0,0.1f);

                    // Calculate the angle away from the center
                    Vector2 away = Vector2.Normalize(WhiskerAnchors[i] - center);
                    float awayAngle = away.ToRotation();

                    // t = 0 (player close) => fully open, t = 1 (player far) => normal ish behavior
                    float t = Utils.GetLerpValue(0f, 400f, playerDist, true);

                    // allow for smooth transitions between open and close
                    float targetAngle;
                    if (closest != null && playerDist < 400f)
                    {
                        // Open whiskers away from center, but still wiggle a bit
                        targetAngle = MathHelper.Lerp(baseAngle + (i % 2 == 0 ? -0.5f : 0.5f), awayAngle + wiggle, 1f - t);
                    }
                    else
                    {
                        
                        targetAngle = baseAngle + (i % 2 == 0 ? -0.5f : 0.5f) + wiggle;
                    }

                   
                    WhiskerRot[i] = MathHelper.Lerp(WhiskerRot[i], targetAngle, 0.15f);

                    //Main.NewText($"{playerDist}, t:{t}");
                    //Dust.NewDustPerfect(WhiskerAnchors[i], DustID.Cloud, Vector2.Zero, 0, Color.AntiqueWhite, 0.25f);
                }
            }
        }
        #region drawcode
        public static Asset<Texture2D> TailTexture;
        private void DrawTail(Color lightColor, Vector2 Offset)
        {
            if (TailPoints != null && TailPoints.Length > 1)
            {
                // Anchor the first point to a point on the NPC determined by the offset.
                // This ensures the ribbon starts at a specific spot relative to the NPC.
                TailPoints[0] = NPC.Center + Offset.RotatedBy(NPC.rotation);

                for (int i = 0; i < TailPoints.Length - 1; i++)
                {
                    int style = 0;
                    if (i == TailPoints.Length - 3)
                    {
                        style = 1;
                    }
                    if (i > TailPoints.Length - 3)
                    {
                        style = 2;
                    }

                    Rectangle frame = TailTexture.Value.Frame(1, 3, 0, style);
                    float rotation = TailPoints[i].AngleTo(TailPoints[i + 1]);
                    Vector2 stretch = new Vector2(0.25f + Utils.GetLerpValue(0, TailPoints.Length, i, true),
                        TailPoints[i].Distance(TailPoints[i + 1]) / (frame.Height - 5)
                    );
                    Main.EntitySpriteDraw(
                        TailTexture.Value,
                        TailPoints[i] - Main.screenPosition + Offset,
                        frame,
                        lightColor.MultiplyRGBA(Color.Lerp(Color.DimGray, Color.White, (float)i / TailPoints.Length)),
                        rotation - MathHelper.PiOver2,
                        frame.Size() * new Vector2(0.5f, 0f),
                        stretch,
                        0,
                        0
                    );
                }
            }
        }

        private int WhiskerFrameCount = 4;
        private void DrawWhisker(Color lightColor)
        {
            Texture2D whisker = AssetDirectory.Textures.UmbralLeechWhisker.Value;
            Vector2 origin = new Vector2(0,(whisker.Height/4)/2);
           
            for (int i = 0; i < WhiskerAnchors.Count; i++)
            {
                
                Rectangle Frame;
                if(WhiskerFrame != null)
                Frame = new Rectangle(0, WhiskerFrame[i], whisker.Width, whisker.Height / WhiskerFrameCount );
                else Frame = new Rectangle(0,4, whisker.Width, whisker.Height / 4);
                Vector2 drawpos = WhiskerAnchors[i].ToPoint().ToVector2() - Main.screenPosition;

                Main.EntitySpriteDraw(whisker, drawpos, Frame, lightColor, WhiskerRot[i], origin, 1, SpriteEffects.None);
                //Utils.DrawBorderString(Main.spriteBatch, "|" + i.ToString()+"", drawpos + Vector2.UnitY * ( (i % 4 == 0 ? 1: 2)* 40), lightColor, 1);
                //Main.EntitySpriteDraw(whisker, WhiskerAnchors[i].ToPoint() - Main.screenPosition, null, lightColor,0,origin, 100, SpriteEffects.None);
                //Main.NewText($"Drawn whisker at {WhiskerAnchors[i] - Main.screenPosition}");
            }

        }

        private void DrawTelegraph(Color lightColor)
        {
            Texture2D Telegraph = AssetDirectory.Textures.UmbralLeechTelegraph.Value;

            Vector2 origin = new Vector2(0, Telegraph.Height/2);
            Color TelegraphColor = Color.Crimson with { A = 30};
            Main.EntitySpriteDraw(Telegraph, NPC.Center - Main.screenPosition, null, TelegraphColor, TelegraphRot, origin, 1, SpriteEffects.None );

        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
        {
            
            if(CurrentState == UmbralLeechAI.FeedTelegraph && Time < 45 && SegmentNum ==0)
            {
                DrawTelegraph(lightColor);
            }

            Texture2D texture = 
                ModContent.Request<Texture2D>(
                "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/NewLeech"
            ).Value;

            //Texture2D outline = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech2").Value;
            SpriteEffects Leech = NPC.direction == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            int row = (SegmentNum < 1f) ? 2 : 3;
           

            if (!NPC.IsABestiaryIconDummy & SegmentNum == 0)
            {
                //Utils.DrawBorderString(Main.spriteBatch, "|FeedTime: " + feedtime.ToString()+"|", NPC.Center - Vector2.UnitY * 120 - Main.screenPosition, Color.White);
                //Utils.DrawBorderString(Main.spriteBatch, "| " + CurrentState +"| Time: "+ Time.ToString() + " | wiggletime: " + WiggleTime.ToString(), NPC.Center - Vector2.UnitY * 140 - Main.screenPosition, Color.White);
            }
            if (SegmentNum % 2 == 0)
            {
                //Utils.DrawBorderString(Main.spriteBatch, "| Direction : " + NPC.direction, NPC.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.White);
                //Utils.DrawBorderString(Main.spriteBatch, "| DeathTimer: " + DeathAnimationTimer.ToString(), NPC.Center - Vector2.UnitY * 140 - Main.screenPosition, Color.White);


            }
           
            if (SegmentNum == SegmentCount)
            {
                row = 0;
                // Draw tendrils at the tail segment
                if (TailPoints == null || TailPoints.Length == 0)
                {
                    // Initialize ribbon points if not already done
                    int length = SegmentCount;
                    TailPoints = new Vector2[length];
                    for (int i = 0; i < length; i++)
                        TailPoints[i] = NPC.Center;
                }
                //Utils.DrawBorderString(Main.spriteBatch, "|Test " , NPC.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.White);
                DrawTail(lightColor, Vector2.Zero);
                DrawTail(lightColor, Vector2.Zero);
            }
            else if (SegmentNum == SegmentCount -1)
            {
                row = 1;
            }
            else if(SegmentNum == 0)
            {
                row = 4;
            }
                int animation = (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3;
            Rectangle sourceRect = texture.Frame(5, 1, row, 0);

            
            Vector2 origin = sourceRect.Size() / 2f;
          
           
            Main.EntitySpriteDraw(
                texture,
                NPC.Center - Main.screenPosition,
                sourceRect,
                lightColor,
                NPC.rotation,
                origin,
                1f,
                Leech
            );

            if (!NPC.IsABestiaryIconDummy && WhiskerAnchors != null)
            {
                DrawWhisker(lightColor);
            }
                
            return false;
        }

        #endregion
    }
}
