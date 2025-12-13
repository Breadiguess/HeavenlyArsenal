using CalamityMod;
using CalamityMod.Items.Materials;
using HeavenlyArsenal.Content.Items.Materials.BloodMoon;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Dust = Terraria.Dust;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab
{
    public enum HemocrabAI
    {
        Idle,
        MoveToRange,
        BombardTarget,
        EnragedMelee,
        Evicerate,
        Disembowl,
        Disengage,
        DeathAnim
    }

    class ArtilleryCrab : BloodMoonBaseNPC
    {
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/BigCrab/ArtilleryCrab";
        public HemocrabAI CurrentState = HemocrabAI.Idle;
        public float BombardRange = 1000f;
        public float BombardRPM = 6f;
        private const float Gravity = 0.2f;
        private const float LaunchSpeed = 15f;
        private const float WalkSpeed = 4f;
        private const float ChargeSpeed = 8f;

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange([
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon,  
				new FlavorTextBestiaryInfoElement("Mods.HeavenlyArsenal.Bestiary.ArtilleryCrab1"),

				//new FlavorTextBestiaryInfoElement("Mods.HeavenlyArsenal.Bestiary.ArtilleryCrab2")
            ]);
        }
        
        public override int bloodBankMax => 3_000;
        public ref float Time => ref NPC.ai[0];
        public ref float AmmoCount => ref NPC.ai[1];
        public ref float EnrageFlag => ref NPC.ai[2];

        public override void Load()
        {
        }

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height =55;
            NPC.damage = 200;
            NPC.defense = 130/2;
            NPC.lifeMax = 38470;
            NPC.value = 10000;
            NPC.aiStyle = -1;
            NPC.npcSlots = 3f;
            NPC.knockBackResist = 0f;
            SpawnModBiomes =
            [
                ModContent.GetInstance<RiftEclipseBloodMoon>().Type
            ];
        }
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 13;
            
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];
            if (!target.active || target.dead)
                NPC.TargetClosest();

            target = Main.player[NPC.target];

            StateMachine(target);
            if(Time%4 == 0 && CurrentState != HemocrabAI.BombardTarget)
              
                if (AmmoCount < 5 && ++NPC.ai[3] % 120 == 0)
                    AmmoCount++;

            Time++;
        }


        private void StateMachine(Player target)
        {
            float distance = Vector2.Distance(NPC.Center, target.Center);

            switch (CurrentState)
            {
                case HemocrabAI.Idle:
                    DoIdle(target);
                    break;

                case HemocrabAI.MoveToRange:
                    DoMoveToRange(target, distance);
                    break;

                case HemocrabAI.BombardTarget:
                    DoBombardTarget(target, distance);
                    break;

                case HemocrabAI.EnragedMelee:
                    DoEnragedMelee(target, distance);
                    break;

                case HemocrabAI.Evicerate:
                    DoEvicerate(target);
                    break;

                case HemocrabAI.Disembowl:
                    DoDisembowl(target);
                    break;

                case HemocrabAI.Disengage:
                    DoDisengage(target, distance);
                    break;

                case HemocrabAI.DeathAnim:
                    DoDeathAnim();
                    break;
            }
        }

        private void DoIdle(Player target)
        {
            NPC.TargetClosest();
            CurrentState = HemocrabAI.MoveToRange;
        }

        private void DoMoveToRange(Player target, float distance)
        {
            if (distance > BombardRange)
                MoveTowards(target.Center, WalkSpeed * 1);
            else if (distance < BombardRange * 0.7f)
                MoveAway(target.Center, WalkSpeed * 3);
            else
            {
                if (NPC.collideY)
                    CurrentState = HemocrabAI.BombardTarget;

                Time = 0;
                AmmoCount = 5;
            }
        }

        private void DoBombardTarget(Player target, float distance)
        {
            float pushRadius = 40f;
            float pushStrength = 13f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];
                if (other.active && other.whoAmI != NPC.whoAmI && other.type == NPC.type)
                {
                    float dist = Vector2.Distance(NPC.Center, other.Center);
                    if (dist < pushRadius && dist > 0f)
                    {
                        Vector2 pushDir = (NPC.Center - other.Center).SafeNormalize(Vector2.Zero);
                        float pushAmount = (pushRadius - dist) / pushRadius;
                        NPC.velocity += pushDir * pushStrength * pushAmount;
                    }
                }
            }
            // Only bombard when on ground
            NPC.takenDamageMultiplier = 0.25f;
            NPC.velocity.X *= 0.4f;
            
            float interval = (60 * 5) / BombardRPM;
            // Ensure crab is grounded before firing
            if (Time >= interval && AmmoCount > 0 && NPC.collideY)
            {
                Time = 0;
                AmmoCount--;
                FireMortarAt(target.Center);
            }

            if (AmmoCount <= 0 || distance < 300f)
            {
                CurrentState = HemocrabAI.EnragedMelee;
                NPC.takenDamageMultiplier = 1;
                Time = 0;
            }
        }

        private void DoEnragedMelee(Player target, float distance)
        {
            EnrageFlag = 1;

            if (Time < 90)
                CurrentState = HemocrabAI.Evicerate;
            else if (Time < 180)
                CurrentState = HemocrabAI.Evicerate;
            else
            {
                Time = 0;
                if (distance > BombardRange * 1.1f)
                {
                    EnrageFlag = 0;
                    CurrentState = HemocrabAI.MoveToRange;
                }
            }

            
        }

        private void DoEvicerate(Player target)
        {
            if (Time == 0)
                NPC.velocity *= 0.5f;

            Vector2 dist = target.Center - NPC.Center;

            if (Time <= 120 && dist.Length() > 60)
            {
                ChargeAt(target.Center, ChargeSpeed * 1.2f);
            }
            else
            {
                CurrentState = HemocrabAI.Disembowl;
                Time = 0;
            }

            
        }

        private void DoDisembowl(Player target)
        {
            float ChargeDistance = NPC.Center.X - target.Center.X;
            float pushRadius = 40f;
            float pushStrength = 13f; 

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];
                if (other.active && other.whoAmI != NPC.whoAmI && other.type == NPC.type)
                {
                    float dist = Vector2.Distance(NPC.Center, other.Center);
                    if (dist < pushRadius && dist > 0f)
                    {
                        Vector2 pushDir = (NPC.Center - other.Center).SafeNormalize(Vector2.Zero);
                        float pushAmount = (pushRadius - dist) / pushRadius; 
                        NPC.velocity += pushDir * pushStrength * pushAmount;
                    }
                }
            }
            if (!(ChargeDistance > 300))
                NPC.velocity.X = 0;

            if (Time <= 20 && Time % 6 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dir = (target.Center - NPC.Center).SafeNormalize(new Vector2(0, i * 15 - 15)) * 10f;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dir, ModContent.ProjectileType<Bloodproj>(), NPC.damage / 10, 0.1f, Main.myPlayer);
                }
            }

            if (Time > 60 && Math.Abs(ChargeDistance) < 400)
            {
               ChargeAt(NPC.Center + Vector2.UnitX * NPC.direction, ChargeSpeed * 2f);
            }

            if (Time >= 120)
            {
                CurrentState = HemocrabAI.Disengage;
                Time = 0;
            }

            
        }
       
        private void DoDisengage(Player target, float distance)
        {
            MoveAway(target.Center, 5 * WalkSpeed);
            

            if (Time > 120 || distance > 200)
            {
                CurrentState = HemocrabAI.MoveToRange;
                Time = 0;
            }
        }

        private void DoDeathAnim()
        {
            NPC.velocity = Vector2.Zero;
            NPC.life = 0;
            NPC.checkDead();
        }

        public override void OnKill()
        {
            for(int i = 0; i < 5; i++)
            {
                createGore(i);
            }
            
        }
        public const int totalFrameCount = 13;
        public const int WalkFrameCount = 6;
        public const int BombardFrameCount = 7;
        public int BodyFrame;
        public override void PostAI()
        {
            if(NPC.velocity.X != 0)
            {
                NPC.direction = Math.Sign(NPC.velocity.X);
            }
            if (CurrentState == HemocrabAI.MoveToRange || CurrentState == HemocrabAI.Disembowl)
            {
                // If the crab is moving, play the walking animation (frames 0-5)
                if (NPC.velocity.X != 0)
                {
                    BodyFrame = (int)((Main.GameUpdateCount / 10) % WalkFrameCount);
                }
                else
                    BodyFrame = 0;
            }
            if (CurrentState == HemocrabAI.BombardTarget)
            {
                // If the crab is bombarding, play the bombard animation (frames 6-12)
                BodyFrame = WalkFrameCount + (int)((Time / 10) % BombardFrameCount);
            }
        }

        private void FireMortarAt(Vector2 targetPos)    
        {
            
            float angle = MathHelper.ToRadians(75f); // steep 75° arc
            const float v0 = LaunchSpeed;

            Vector2 direction = targetPos - NPC.Center;
            float distance = direction.Length();
            float vx = v0 * (float)Math.Cos(angle);
            float vy = v0 * (float)Math.Sin(angle);

            // Scale vx to cover horizontal distance
            vx = vx * (distance / (vx * (2 * vy / Gravity)));

            Vector2 launchV = new Vector2(
                vx * Math.Sign(direction.X),
                -vy // inverted for Terraria Y-axis
            );

            int idx = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y,
            ModContent.NPCType<BloodMortar>());
            NPC mortar = Main.npc[idx];
            mortar.velocity = launchV;
            mortar.localAI[0] = targetPos.X;
            mortar.localAI[1] = targetPos.Y;

            SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
        }
        
        private void MoveTowards(Vector2 pos, float speed)
        {
            int dir = Math.Sign(pos.X - NPC.Center.X);
            NPC.velocity.X = dir * speed;
            if(NPC.velocity.Y == 0)
                HandleJump(dir);
        }

        private void MoveAway(Vector2 pos, float speed)
        {
            int dir = Math.Sign(NPC.Center.X - pos.X);
            NPC.velocity.X = dir * speed;
            if (NPC.velocity.Y == 0)
                HandleJump(dir);
        }


        private void HandleJump(int xDirection)
        {
            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            //todo: if 
            if(CurrentState == HemocrabAI.BombardTarget && (damageDone-NPC.life)/NPC.lifeMax > NPC.life/NPC.lifeMax * 1.1)
            {
                CurrentState = HemocrabAI.EnragedMelee;
                Time = 0;
            }
            base.OnHitByProjectile(projectile, hit, damageDone);
        }
        private void ChargeAt(Vector2 pos, float speed)
        {
            
            int dirX = Math.Sign(pos.X - NPC.Center.X);
            NPC.velocity.X = dirX * speed;
            HandleJump(dirX);
            float pushRadius = 40f;
            float pushStrength = 13f; 

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC other = Main.npc[i];
                if (other.active && other.whoAmI != NPC.whoAmI && other.type == NPC.type)
                {
                    float dist = Vector2.Distance(NPC.Center, other.Center);
                    if (dist < pushRadius && dist > 0f)
                    {
                        Vector2 pushDir = (NPC.Center - other.Center).SafeNormalize(Vector2.Zero);
                        float pushAmount = (pushRadius - dist) / pushRadius; 
                        NPC.velocity += pushDir * pushStrength * pushAmount;
                    }
                }
            }
        }
        public override void OnSpawn(IEntitySource source)
        {
            BigCrabGores = new Asset<Texture2D>[5];
            for (int i = 1; i <= 5; i++)
                BigCrabGores[i - 1] = ModContent.Request<Texture2D>($"HeavenlyArsenal/Content/Gores/Enemy/BloodMoon/BigCrab/CrabGore{i}");
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.NormalvsExpert(ModContent.ItemType<ShellFragment>(), 30, 25));
            npcLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<ArtilleryCrab>()));

            npcLoot.Add(ModContent.ItemType<BloodOrb>(), 1, 40, 48);
        }

        public static Asset<Texture2D>[] BigCrabGores
        {
            get;
            private set;
        }
        private void GetGoreInfo(out Texture2D texture, out int goreID, int Variant)
        {
            texture = null;
            goreID = 0;
            if (Main.netMode != NetmodeID.Server)
            {
                int variant = Variant;

                
                texture = BigCrabGores[variant].Value;
                goreID = ModContent.Find<ModGore>(Mod.Name, $"CrabGore{variant + 1}").Type;
            }
        }
        private void createGore(int i)
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            //thanks lucille
            GetGoreInfo(out _, out int goreID, i);

            Gore.NewGore(NPC.GetSource_FromThis(), NPC.Center, Vector2.Zero, goreID, NPC.scale);
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (NPC.IsABestiaryIconDummy)
            {
                return base.PreDraw(spriteBatch, screenPos, drawColor);
            }
            if (!NPC.IsABestiaryIconDummy)
            {
                //Utils.DrawBorderString(spriteBatch, " | State: " + CurrentState, NPC.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.White);
                //Utils.DrawBorderString(spriteBatch, " | Ammo: " + AmmoCount, NPC.Center - Vector2.UnitY * 140 - Main.screenPosition, Color.White);
                //Utils.DrawBorderString(spriteBatch, " | Timer: " + Time, NPC.Center - Vector2.UnitY * 120 - Main.screenPosition, Color.White);

            }

            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/BigCrab/ArtilleryCrab").Value;

            int frameHeight = texture.Height / totalFrameCount;
            Vector2 DrawPos = NPC.Center - Main.screenPosition;
           
            SpriteEffects Direction = NPC.direction < 0 ? SpriteEffects.FlipHorizontally : 0;

            Rectangle CrabFrame = texture.Frame(1, 13, 0, BodyFrame);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight - 30);

            //new Rectangle(0, BodyFrame * frameHeight, texture.Width, frameHeight);

            Main.EntitySpriteDraw(texture, DrawPos, CrabFrame, drawColor, 0, origin, 1, Direction, 0);
            return false;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.bloodMoon && DownedBossSystem.downedProvidence)
                return SpawnCondition.OverworldNightMonster.Chance * 0.12f;
            return 0f;
        }
    }

    public class BloodMortar : ModNPC
    {
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/BigCrab/Bloodproj";
        public ref float Xcoord => ref NPC.ai[1];
        public ref float Ycoord => ref NPC.ai[2];
        private const float Gravity = 0.2f;
        private bool exploded = false;

        public ref float Owner => ref NPC.ai[0];
        public override void SetStaticDefaults()
        {
            NPCID.Sets.ProjectileNPC[NPC.type] = true;
            NPCID.Sets.CannotDropSouls[NPC.type] = true;
            // NPCID.Sets

            this.ExcludeFromBestiary();
        }

       
      
        public override bool? CanFallThroughPlatforms()
        {
            return true;
        }
        public override void SetDefaults()
        {
            NPC.width = 50;
            NPC.height = 50;
            NPC.damage = 488;
            NPC.lifeMax = 100;
            NPC.defDefense = 4000;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
           
        }
        
        public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
        {
            base.OnHitPlayer(target, hurtInfo);
        }
        public override void AI()
        {
            NPC.velocity.Y += Gravity;
            NPC.rotation = NPC.velocity.ToRotation();
            Dust a;
            for(int i = 0; i< 10; i++)
            {
                a = Dust.NewDustDirect(NPC.Center, 30,30, DustID.Blood);
            }
            if (!exploded && Collision.SolidCollision(NPC.position + NPC.velocity, NPC.width, NPC.height))
            {
                exploded = true;
                Explode();
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/BigCrab/BloodMortar").Value;
            Texture2D Glowball = AssetDirectory.Textures.BigGlowball.Value;
            Vector2 glowScale = new Vector2(1f, 1f);
            Vector2 Gorigin = new Vector2(texture.Size().X / 2, texture.Size().Y / 2);
            Vector2 DrawPos = NPC.Center - Main.screenPosition;



            Main.spriteBatch.Draw(texture, DrawPos, null,
                     lightColor, NPC.velocity.ToRotation(), Gorigin, glowScale, SpriteEffects.None, 0f);

            Main.EntitySpriteDraw(Glowball, DrawPos, null, Color.Crimson, NPC.velocity.ToRotation(), Glowball.Size() * 0.5f, glowScale * 0.2f, SpriteEffects.None);
            return false;
        }
        private void Explode()
        {
            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
           // Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center, Vector2.Zero,
            //    ProjectileID.DD2ExplosiveTrapT3Explosion, NPC.damage, 0f, NPC.whoAmI);
            for (int i = 0; i < 20; i++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood,
                    Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-30,0));
            NPC.active = false;
        }
    }

    
}
