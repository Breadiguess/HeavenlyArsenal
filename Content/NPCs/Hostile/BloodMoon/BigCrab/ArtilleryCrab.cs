using CalamityMod;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using static NoxusBoss.Assets.GennedAssets.Textures;
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

    class ArtilleryCrab : ModNPC
    {
        public HemocrabAI CurrentState = HemocrabAI.Idle;
        public float BombardRange = 1000f;
        public float BombardRPM = 6f;
        private const float Gravity = 0.2f;
        private const float LaunchSpeed = 15f;
        private const float WalkSpeed = 4f;
        private const float ChargeSpeed = 8f;

        public ref float BombardTimer => ref NPC.ai[0];
        public ref float AmmoCount => ref NPC.ai[1];
        public ref float EnrageFlag => ref NPC.ai[2];

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 60;
            NPC.damage = 200;
            NPC.defense = 130;
            NPC.lifeMax = 48370;
            NPC.value = 10000;
            NPC.aiStyle = -1;
            NPC.npcSlots = 3f;
            NPC.knockBackResist = 0.2f;
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];
            if (!target.active || target.dead)
                NPC.TargetClosest();
            target = Main.player[NPC.target];

            float distance = Vector2.Distance(NPC.Center, target.Center);

            switch (CurrentState)
            {
                case HemocrabAI.Idle:
                    NPC.TargetClosest();
                    CurrentState = HemocrabAI.MoveToRange;
                    break;

                case HemocrabAI.MoveToRange:
                    if (distance > BombardRange)
                        MoveTowards(target.Center, WalkSpeed);
                    else if (distance < BombardRange * 0.7f)
                        MoveAway(target.Center, WalkSpeed);
                    else
                    {
                        if (NPC.collideY)
                            CurrentState = HemocrabAI.BombardTarget;
                        BombardTimer = 0;
                        AmmoCount = 5;
                    }
                    break;

                case HemocrabAI.BombardTarget:
                    // Only bombard when on ground
                    NPC.velocity = Vector2.Zero;
                    if (AmmoCount < 5 && ++NPC.ai[3] % 120 == 0)
                        AmmoCount++;

                    BombardTimer++;
                    float interval = (60 * 5) / BombardRPM;
                    // Ensure crab is grounded before firing
                    if (BombardTimer >= interval && AmmoCount > 0 && NPC.collideY)
                    {
                        BombardTimer = 0;
                        AmmoCount--;
                        FireMortarAt(target.Center);
                    }

                    if (AmmoCount <= 0 || distance < 300f)
                    {
                        CurrentState = HemocrabAI.EnragedMelee;
                        BombardTimer = 0;
                    }
                    break;

                case HemocrabAI.EnragedMelee:
                    EnrageFlag = 1;
                    if (BombardTimer < 90)
                        CurrentState = HemocrabAI.Evicerate;
                    else if (BombardTimer < 180)
                        CurrentState = HemocrabAI.Disembowl;
                    else
                    {
                        BombardTimer = 0;
                        if (distance > BombardRange * 1.1f)
                        {
                            EnrageFlag = 0;
                            CurrentState = HemocrabAI.MoveToRange;
                        }
                    }
                    BombardTimer++;
                    break;

                case HemocrabAI.Evicerate:
                    if(BombardTimer ==0)
                        NPC.velocity = Vector2.Zero;

                    if (BombardTimer <= 60)
                    {
                        ChargeAt(target.Center, ChargeSpeed * 2f);
                        
                    
                    }
                    else
                    {
                        CurrentState = HemocrabAI.Disembowl;
                        BombardTimer = 0;
                    }
                        BombardTimer++;

                    break;

                case HemocrabAI.Disembowl:
                    // Fire 3 chaos balls, then charge back


                    NPC.velocity = Vector2.Zero;
                    
                    // Shoot a chaos ball every 30 ticks, up to 3
                    if (BombardTimer == 60  && BombardTimer % 30 == 0)
                    {
                        for(int i = 0; i < 2; i++)
                        {
                            //when i = 0, the angle should be offset by -15 degrees
                            //when i = 1, the angle should be offset by 0
                            //when i = 2, the angle should be offset by 15 degrees
                            Vector2 dir = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 10f;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dir,
                                ProjectileID.AncientDoomProjectile, NPC.damage / 10, 0f, Main.myPlayer);
                        }
                        
                    }
                    // After 3 shots, charge and transition
                    if (BombardTimer > 90)
                    {
                        ChargeAt(target.Center, ChargeSpeed * 2f);

                    }

                    if(BombardTimer >=120)
                    { 
                        CurrentState = HemocrabAI.Disengage;
                        BombardTimer = 0;
                    }
                    BombardTimer++;
                    break;

                case HemocrabAI.Disengage:
                    MoveAway(target.Center, 5 *WalkSpeed);
                    BombardTimer++;
                    if (BombardTimer > 120 || distance > 100)
                    {
                        CurrentState = HemocrabAI.MoveToRange;
                        BombardTimer = 0;
                    }
                    break;

                case HemocrabAI.DeathAnim:
                    NPC.velocity = Vector2.Zero;
                    NPC.life = 0;
                    NPC.checkDead();
                    break;
            }
        }



        private void FireMortarAt(Vector2 targetPos)
        {
            // Simplified fixed-angle mortar logic for consistent arcs
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
            HandleJump(dir);
        }

        private void MoveAway(Vector2 pos, float speed)
        {
            int dir = Math.Sign(NPC.Center.X - pos.X);
            NPC.velocity.X = dir * speed;
            HandleJump(dir);
        }

        private void HandleJump(int xDirection)
        {
            Vector2 origin = NPC.Bottom + new Vector2(xDirection * (NPC.width / 2 + 2), 0);
            Vector2 stepTarget = origin + new Vector2(xDirection * 16, -16);
            Point pFeet = origin.ToTileCoordinates();
            if (WorldGen.SolidTile(pFeet.X + xDirection, pFeet.Y))
            {
                Point pStep = stepTarget.ToTileCoordinates();
                if (!WorldGen.SolidTile(pStep.X, pStep.Y))
                {
                    NPC.velocity.Y = -10f;
                }
            }
        }

        private void ChargeAt(Vector2 pos, float speed)
        {
            // Only affect horizontal motion, keep existing Y velocity
            int dirX = Math.Sign(pos.X - NPC.Center.X);
            NPC.velocity.X = dirX * speed;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Utils.DrawBorderString(spriteBatch, " | State: " + CurrentState, NPC.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(spriteBatch, " | Ammo: " + AmmoCount, NPC.Center - Vector2.UnitY * 140 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(spriteBatch, " | Timer: " + BombardTimer, NPC.Center - Vector2.UnitY * 120 - Main.screenPosition, Color.White);
            return base.PreDraw(spriteBatch, screenPos, drawColor);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.bloodMoon && DownedBossSystem.downedYharon)
                return SpawnCondition.OverworldNightMonster.Chance * 0.01f;
            return 0f;
        }
    }

    class BloodMortar : ModNPC
    {
        public override string Texture => "HeavenlyArsenal/Content/NPCs/Hostile/BloodMoon/Leech/UmbralLeech";
        public ref float Xcoord => ref NPC.localAI[0];
        public ref float Ycoord => ref NPC.localAI[1];
        private const float Gravity = 0.2f;
        private bool exploded = false;

        public override void SetStaticDefaults()
        {
            NPCID.Sets.ProjectileNPC[NPC.type] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 50;
            NPC.height = 50;
            NPC.damage = 488;
            NPC.lifeMax = 1;
            NPC.defDefense = 4000;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
        }

        public override void AI()
        {
            NPC.velocity.Y += Gravity;
            NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
            if (!exploded && Collision.SolidCollision(NPC.position + NPC.velocity, NPC.width, NPC.height))
            {
                exploded = true;
                Explode();
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D Glowball = GreyscaleTextures.BloomFlare;
            float GlowScale = 0.1f;
            Vector2 glowScale = new Vector2(0.4f, 0.2f);
            Vector2 Gorigin = new Vector2(Glowball.Size().X / 2, Glowball.Size().Y / 2);


            
            Main.spriteBatch.Draw(Glowball, NPC.Center + NPC.velocity / 2 - Main.screenPosition, null,
                     (Color.Violet with { A = 0 }) * 0.2f, NPC.velocity.ToRotation(), Gorigin, glowScale, SpriteEffects.None, 0f);


            return false;// base.PreDraw(spriteBatch, screenPos, drawColor);
        }
        private void Explode()
        {
            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center, Vector2.Zero,
                ProjectileID.DD2ExplosiveTrapT3Explosion, NPC.damage, 0f, Main.myPlayer);
            for (int i = 0; i < 20; i++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood,
                    Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3));
            NPC.active = false;
        }
    }
}
