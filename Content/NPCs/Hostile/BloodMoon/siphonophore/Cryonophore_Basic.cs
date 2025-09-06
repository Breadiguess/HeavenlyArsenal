using CalamityMod;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Banners;
using CalamityMod.NPCs.CalamityAIs.CalamityRegularEnemyAIs;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.siphonophore
{

    public class CryonoPhoreLegChain
    {
        private readonly NPC Owner;
        private readonly Vector2 BaseOffset;
        private const int SegmentCount = 6;
        private const float SegmentLength = 18f;
        private const float MaxSearchDistance = 200f; // Max pixel distance to search for ground
        private const float StepThreshold = 12f;       // Minimum distance to initiate a step
        private const float StepDuration = 15f;        // Ticks per step
        private const float StepLiftHeight = 8f;       // How high the foot lifts

        private readonly Vector2[] Positions;
        private bool Grounded;

        private Vector2 footTarget;
        private Vector2 previousFoot;

        // Step state
        private Vector2 stepStart;
        private Vector2 stepEnd;
        private float stepProgress; // 0..1
        private bool isStepping;

        public bool IsFootGrounded => Grounded;

        public CryonoPhoreLegChain(NPC owner, Vector2 baseOffset)
        {
            Owner = owner;
            BaseOffset = baseOffset;
            Positions = new Vector2[SegmentCount];

            Vector2 basePos = owner.Bottom + baseOffset;
            for (int i = 0; i < SegmentCount; i++)
                Positions[i] = basePos + new Vector2(0, i * SegmentLength);

            previousFoot = Positions[^1];
            footTarget = previousFoot;
            isStepping = false;
            stepProgress = 1f;
        }

        public void Update()
        {
            // 1. Determine raw desired ground point
            Vector2 raw = Owner.Bottom + BaseOffset + new Vector2(Owner.velocity.X * 10f, 32f);
            Vector2 groundPoint = FindFloorPosition(raw);

            // 2. Initiate step if far enough and not already stepping
            if (!isStepping && Vector2.Distance(previousFoot, groundPoint) > StepThreshold &&
                Vector2.Distance(raw, previousFoot) <= MaxSearchDistance)
            {
                isStepping = true;
                stepProgress = 0f;
                stepStart = previousFoot;
                stepEnd = groundPoint;
            }

            // 3. Update stepping
            if (isStepping)
            {
                stepProgress += 1f / StepDuration;
                if (stepProgress >= 1f)
                {
                    stepProgress = 1f;
                    isStepping = false;
                }
                // Lerp horizontally, add vertical lift
                float lift = MathF.Sin(stepProgress * MathF.PI) * StepLiftHeight;
                footTarget = Vector2.Lerp(stepStart, stepEnd, stepProgress) - new Vector2(0, lift);
                Grounded = false;
            }
            else
            {
                // Keep foot planted
                footTarget = previousFoot;
                Grounded = true;
            }

            previousFoot = footTarget;

            // 4. Solve IK to position segments
            SolveIK();
        }

        private void SolveIK()
        {
            // Backward pass: anchor foot
            Positions[^1] = footTarget;
            for (int i = SegmentCount - 2; i >= 0; i--)
            {
                Vector2 dir = Positions[i] - Positions[i + 1];
                dir.Normalize();
                Positions[i] = Positions[i + 1] + dir * SegmentLength;
            }

            // Forward pass: fix base to NPC
            Positions[0] = Owner.Bottom + BaseOffset;
            for (int i = 1; i < SegmentCount; i++)
            {
                Vector2 dir = Positions[i] - Positions[i - 1];
                dir.Normalize();
                Positions[i] = Positions[i - 1] + dir * SegmentLength;
            }
        }

        private Vector2 FindFloorPosition(Vector2 from)
        {
            Point tilePos = from.ToTileCoordinates();
            int maxTiles = (int)(MaxSearchDistance / 16f);
            for (int i = 0; i < maxTiles; i++)
            {
                int y = tilePos.Y + i;
                if (WorldGen.SolidTile(tilePos.X, y))
                    return new Vector2(tilePos.X * 16 + 8, y * 16);
            }
            // No valid ground within range
            return previousFoot;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 screenPos, Color color)
        {
            for (int i = 0; i < Positions.Length - 1; i++)
            {
                Vector2 start = Positions[i];
                Vector2 end = Positions[i + 1];
                Utils.DrawLine(spriteBatch, start, end, color, color, 2f);
            }
        }
    }



    public class Cryonophore : ModNPC
    {
        #region setup

        private List<CryonoPhoreLegChain> LegChains;
        

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.damage = 42;
            NPC.width = 50;
            NPC.height = 64;
            NPC.defense = 100;
          
            NPC.lifeMax = 40400;
            NPC.knockBackResist = 0.2f;
            NPC.value = Item.buyPrice(0, 0, 5, 0);
            NPC.HitSound = SoundID.NPCHit5 with { PitchVariance = 0.4f };
            NPC.DeathSound = SoundID.NPCDeath7;
            Banner = NPC.type;
            BannerItem = ModContent.ItemType<CryonBanner>();
            NPC.noGravity = true;
            NPC.coldDamage = true;
            NPC.Calamity().VulnerableToHeat = true;
            NPC.Calamity().VulnerableToCold = false;
            NPC.Calamity().VulnerableToSickness = false;

          
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,
                
                new FlavorTextBestiaryInfoElement("Mods.HeavenylArsenal.Bestiary.Cryonophore_Basic")
            });
        }

        public enum CryonophorAI
        {
            Idle,
            SummonBlizzard,
            Attack,
            Retreat
        }
        public CryonophorAI CurrentState;

        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/siphonophore/Cryonophore";
        #endregion
        public override void OnSpawn(IEntitySource source)
        {
            CurrentState = CryonophorAI.Idle;
            LegChains = new List<CryonoPhoreLegChain>
            {
                new CryonoPhoreLegChain(NPC, new Vector2(-30f, 0f)),
                new CryonoPhoreLegChain(NPC, new Vector2(30f, 0f)),
                new CryonoPhoreLegChain(NPC, new Vector2(15f, 10f)),
                new CryonoPhoreLegChain(NPC, new Vector2(-15f, 10f))
            };
        }
        public override void AI()
        {

            //StateMachine();

            NPC.Center = Main.MouseWorld;
            foreach (var leg in LegChains)
                leg.Update();
        }

        private void StateMachine()
        {
            switch (CurrentState)
            {
                case CryonophorAI.Idle:
                   HandleIdle();
                    break;
                case CryonophorAI.SummonBlizzard:
                    HandleSummonBlizzard();
                    break;
                case CryonophorAI.Attack:
                    HandleAttack();
                    break;
                case CryonophorAI.Retreat:
                    HandleRetreat();
                    break;
            }
        }
        private void HandleIdle()
        {
            NPC.TargetClosest();
            if (NPC.HasValidTarget)
            {
                NPC.velocity.X = 0f;
                NPC.velocity.Y = 0f;
                CurrentState = CryonophorAI.Attack;
            }
        }
        private void HandleSummonBlizzard()
        {

        }
        private void HandleAttack()
        {
            if (NPC.HasValidTarget)
            {
                Player target = Main.player[NPC.target];
                Vector2 direction = target.Center - NPC.Center;
                direction.Normalize();
                NPC.velocity.X= direction.X * 3f; // Adjust speed as necessary
                if (NPC.collideX && NPC.velocity.Y == 0f)
                {
                    NPC.velocity.Y = -6f;
                }
            }
            else
            {
                CurrentState = CryonophorAI.Idle;
            }
        }
        private void HandleRetreat()
        {
            
            if (NPC.HasValidTarget)
            {
                Player target = Main.player[NPC.target];
                Vector2 direction = NPC.Center - target.Center;
                direction.Normalize();
                NPC.velocity = direction * 2f; // Adjust speed as necessary
            }
            else
            {
                CurrentState = CryonophorAI.Idle;
            }
        }
        public override bool? CanFallThroughPlatforms()
        {
            return base.CanFallThroughPlatforms();
        }
        public override void OnKill()
        {
            base.OnKill(); 
        }
        public override bool SpecialOnKill() => true;

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            
            Vector2 DrawPos = NPC.Center - Main.screenPosition;

            Main.EntitySpriteDraw(texture, DrawPos, null, drawColor, NPC.rotation, texture.Size() * 0.5f, NPC.scale, SpriteEffects.None, 0);
            if(!NPC.IsABestiaryIconDummy)
            foreach (var leg in LegChains)
                leg.Draw(spriteBatch, screenPos,  drawColor);

            return false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            /*
            return spawnInfo.Player.ZoneSnow &&
                !spawnInfo.Player.PillarZone() &&
                !spawnInfo.Player.ZoneDungeon &&
                !spawnInfo.Player.InSunkenSea() &&
                Main.hardMode && !spawnInfo.PlayerInTown && !spawnInfo.Player.ZoneOldOneArmy && !Main.snowMoon && !Main.pumpkinMoon ? 0.045f : 0f;
       */
            if (Main.bloodMoon && DownedBossSystem.downedYharon)
                return 0f;
            else
                return 0f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            if (hurtInfo.Damage > 0)
            {
                target.AddBuff(BuffID.Frostburn, 120, true);
                target.AddBuff(BuffID.Chilled, 90, true);
            }
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 3; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Frost, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 15; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Frost, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.Add(ModContent.ItemType<EssenceofEleum>(),3,0,5);
    }

    public class BlizardSource : ModProjectile
    {
        #region Setup
        /// <summary>
        /// The entity that owns this projectile. Should always be a cryonophore but i just made it an entity.  
        /// </summary>
        public Entity Owner
        {
            get;
            private set;
        }
        public Entity Target
        {
            get;
            private set;
        }
        public enum BlizzardState
        {
           Creation,
           Active,
           Disipate
        }
        public BlizzardState CurrentState;
        public override void OnSpawn(IEntitySource source)
        {
            CurrentState = BlizzardState.Creation;
            if (source is EntitySource_Parent parentSource && parentSource.Entity is NPC npc)
            {
                Owner = npc;
            }
        }
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1800;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1; 
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 4000;
        }
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/siphonophore/Cryonophore_Basic";
        public ref float Time => ref Projectile.ai[0];

        #endregion
        #region AI
        public override void AI()
        {
            StateMachine();
            Time++;
        }
        private void StateMachine()
        {
            switch (CurrentState)
            {
                case BlizzardState.Creation:
                    HandleCreation();
                    break;
                case BlizzardState.Active:
                    HandleActive();
                    break;
                case BlizzardState.Disipate:
                    HandleDisipate();
                    break;
            }
        }

        private void HandleCreation()
        {
            
        }

        private void HandleActive()
        {
            
        }

        private void HandleDisipate()
        {
           
        }
        #endregion
        #region DrawCode
        public override bool PreDraw(ref Color lightColor)
        {


            return base.PreDraw(ref lightColor);
        }
        #endregion
    }
}