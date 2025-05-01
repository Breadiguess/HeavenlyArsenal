using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.Yharon;
using CalamityMod.Projectiles.Boss;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.BigCrab;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech
{
    public enum LeechActivities
    {
        attack,
        leech,
        explode,
        wiggle,
        deathAnim
    }
    public class UmbralLeech2 : ModNPC
    {
        public static int[] ValidTargets = { ModContent.NPCType<AnAffrontToGod>(), ModContent.NPCType<Yharon>(), ModContent.NPCType<SuperDummyNPC>(), ModContent.NPCType<ArtilleryCrab>() };

        LeechActivities CurrentState;
        Vector2 stickPos;
        List<Vector2> smoothPos = [];


        public ref float HeadID => ref NPC.ai[0];
        public ref float Time => ref NPC.ai[2];
        public ref float SegmentNum => ref NPC.ai[1];

        public ref float SegmentCount => ref NPC.localAI[0];

        public static readonly SoundStyle Bash = new SoundStyle("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/UmbralLeech_Bash_", 3);
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Rectangle sourceRect = texture.Frame(3, 8, NPC.ai[1] < 1 ? 2 : 1, 0);
            Main.EntitySpriteDraw(texture, NPC.Center - Main.screenPosition, sourceRect, Lighting.GetColor((NPC.Center / 16).ToPoint()), NPC.rotation, sourceRect.Size() / 2f, 1, SpriteEffects.None);

            if (SegmentNum % 2 == 0) 
            {
                Utils.DrawBorderString(Main.spriteBatch, "| State: " + CurrentState.ToString(), NPC.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.White);
                Utils.DrawBorderString(Main.spriteBatch, "| DeathTimer: " + DeathAnimationTimer.ToString(), NPC.Center - Vector2.UnitY * 140 - Main.screenPosition, Color.White);

               
            }
            if (SegmentNum != 0)
            {
                Utils.DrawBorderString(Main.spriteBatch, "| HeadID: " + HeadID.ToString(), NPC.Center - Vector2.UnitY * 120 - Main.screenPosition, Color.White);
                Utils.DrawBorderString(Main.spriteBatch, "| Segment: " + SegmentNum.ToString(), NPC.Center - Vector2.UnitY * 100 - Main.screenPosition, Color.White);

            }



            return false;
        }
        public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
        {
            return HeadID == NPC.whoAmI && !NPC.dontTakeDamage;
        }
        public override void AI()
        {
            //Main.NewText($"Velocity: {NPC.velocity}, Position: {NPC.position}, Segment:{NPC.ai[1]}, state: {CurrentState}");
            float nearestPlayer = 9999;
            NPC.target = -1;
            foreach (var playerScan in Main.ActivePlayers)
            {
                if (!playerScan.dead && NPC.Distance(playerScan.Center) + playerScan.aggro < nearestPlayer)
                {
                    nearestPlayer = NPC.Distance(playerScan.Center);
                    NPC.target = playerScan.whoAmI;
                }
            }



            if (NPC.target < 0)
            {
                NPC.EncourageDespawn(10);
                NPC.velocity.X /= 1.1f;
                NPC.velocity.Y += 1.2f;
            }
            //Do shit
            else
            {
                Player target = Main.player[NPC.target];
                switch (CurrentState)
                {
                    case LeechActivities.attack:
                        {
                            NPC.rotation = NPC.DirectionTo(target.Center).ToRotation();
                            NPC.velocity += NPC.DirectionTo(target.Center) * 0.4f;
                            if (SegmentNum == 0)
                                NPC.velocity /= 1.05f;
                        }
                        break;
                    case LeechActivities.leech:
                        {
                            if (++Time > 60)
                            {
                                CurrentState = LeechActivities.explode;
                            }
                            NPC.Center = target.Center + stickPos;
                            NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero); //= 1.05f;
                        }
                        break;
                    case LeechActivities.explode:
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                CurrentState = LeechActivities.deathAnim;
                            }
                        }
                        break;
                    case LeechActivities.wiggle:
                        {
                            NPC head = Main.npc[(int)HeadID];
                            if (head.active)
                            {
                                foreach (var npcScan in Main.ActiveNPCs)
                                {
                                    if (npcScan.type == Type && npcScan.ai[0] == HeadID && npcScan.ai[1] == NPC.ai[1] - 1)
                                    {
                                        smoothPos[0] = head.Center;
                                        for (int i = 1; i < smoothPos.Count; i++)
                                        {
                                            smoothPos[i] = smoothPos[i - 1] + Vector2.Normalize(smoothPos[i - 1].DirectionTo(smoothPos[i]) * 9 - Vector2.Normalize(head.velocity)) * 26;
                                            //Dust dust = Dust.NewDustPerfect(smoothPos[i], 59 + i);
                                            //dust.noGravity = true;
                                        }
                                        if (SegmentNum < 2)
                                        {
                                            if (--Time < 1)
                                            {
                                                Time = 20;
                                            }
                                            //NPC.Center = smoothPos[(int)NPC.ai[1] - 1] - Vector2.Normalize(head.velocity).RotatedBy(Math.Sin(Math.PI * Time / 10f) / 2f) * NPC.width / 2f;
                                            NPC.Center = smoothPos[(int)NPC.ai[1] - 1] - Vector2.Normalize(head.velocity).RotatedBy(Math.Sin(Math.PI * Time / 10f) / 2f) * 26;
                                            NPC.Center = npcScan.Center + npcScan.DirectionTo(NPC.Center) * 26;
                                            NPC.rotation = NPC.DirectionTo(npcScan.Center).ToRotation();
                                        }
                                        else
                                        {
                                            if (--Time < NPC.ai[1] * 4 + 1)
                                            {
                                                Time = 20 + NPC.ai[1] * 4;
                                            }
                                            //NPC.Center = smoothPos[(int)NPC.ai[1] - 1] - Vector2.Normalize(head.velocity).RotatedBy(Math.Sin(Math.PI * NPC.ai[2] / 10f) / 2f) * NPC.width / 2f;
                                            NPC.Center = smoothPos[(int)NPC.ai[1] - 1] - Vector2.Normalize(head.velocity).RotatedBy(Math.Sin(Math.PI * Time / 10f) / 2f) * 26;
                                            NPC.Center = npcScan.Center + npcScan.DirectionTo(NPC.Center) * 26;
                                            NPC.rotation = NPC.DirectionTo(npcScan.Center).ToRotation();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //NPC.active = false;
                            }
                        }
                        break;
                    case LeechActivities.deathAnim:
                        DoDeathAnimation();
                        /*
                        if (++Time % 10 == 0)
                        {
                            if (HeadID == NPC.whoAmI)
                                SoundEngine.PlaySound(Bash with { PitchVariance = 0.3f }, NPC.Center);
                        }

                        if (++Time > 120)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                NPC.StrikeInstantKill();
                            }

                        }
                        */
                        Time++;
                        
                        break;
                }
            }
        }
        //Because knockback normally fucking 100% negates velocity. This grants 80% negation of the effects of knockback THE RIGHT WAY
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (NPC.ai[1] <= 1)
            {
                NPC.velocity -= NPC.DirectionTo(item.Center) * item.knockBack * 0.2f;
            }
            else
            {
                Main.npc[(int)NPC.ai[0]].life -= damageDone;
                NPC.life = Main.npc[(int)NPC.ai[0]].life;
            }
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (NPC.ai[1] < 1)
            {
                NPC.velocity -= NPC.DirectionTo(projectile.Center) * projectile.knockBack * 0.2f;
            }
            else
            {
                Main.npc[(int)NPC.ai[0]].life -= damageDone;
                NPC.life = Main.npc[(int)NPC.ai[0]].life;
            }
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            if (SegmentNum == 0)
                CurrentState = LeechActivities.leech;
            NPC.damage = 0;
            NPC.dontTakeDamage = true;
            NPC.velocity.SafeNormalize(Vector2.Zero);
            stickPos = NPC.Center - target.Center;
            target.AddBuff(ModContent.BuffType<LeechBuff>(), 60);
        }
        public override void OnSpawn(IEntitySource source)
        {
            if (SegmentNum < 1)
            {
                CurrentState = LeechActivities.attack;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 1; i < Main.rand.Next(6, 16); i++)
                    {
                        NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y + i, Type, default, NPC.whoAmI, i, i * 4);
                    }
                }
            }
            else
            {
                CurrentState = LeechActivities.wiggle;
                for (int i = 0; i < NPC.ai[1]; i++)
                {
                    smoothPos.Add(Main.npc[(int)HeadID].Center + new Vector2(0, i));
                }
            }
        }
        public override void SetDefaults()
        {
            NPC.damage = 100;
            NPC.defense = 150;
            NPC.friendly = false;
            NPC.height = 52;
            NPC.knockBackResist = 0;
            NPC.lifeMax = 32000;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.takenDamageMultiplier = 0.8f;
            NPC.width = 78;
        }
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 3;
        }
        public override bool CheckDead()
        {
            NPC.life = 1;
            CurrentState = LeechActivities.deathAnim;

            NPC.active = true;
            NPC.dontTakeDamage = true;

            NPC.netUpdate = true;

            // Prevent netUpdate from being blocked by the spam counter.
            if (NPC.netSpam >= 10)
                NPC.netSpam = 9;

            return false;
        }
        public float DeathAnimationTimer;
        public void DoDeathAnimation()
        {
            for(int i = 0; i < SegmentCount; i++)
            {
                NPC.velocity *= 0.1f;
            }
            
            


            if (DeathAnimationTimer == 1f)
            {
                if (Main.netMode != NetmodeID.Server && Main.LocalPlayer.WithinRange(NPC.Center, 4800f))
                    SoundEngine.PlaySound(Bash with { Volume = 1.65f });
            }
            // Begin fading out before the exploding sun animation happens.
            //if (DeathAnimationTimer >= 370f)
                //NPC.Opacity *= 0.97f;

            if (DeathAnimationTimer == 92f)
            {
               // SoundEngine.PlaySound(HolyBlast.ImpactSound, NPC.Center);
               // if (Main.netMode != NetmodeID.MultiplayerClient)
               //     Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<HolyExplosionBoom>(), 0, 0f);
            }

            if (Main.netMode == NetmodeID.Server && DeathAnimationTimer % 45f == 44f)
            {
                NPC.netUpdate = true;

                // Prevent netUpdate from being blocked by the spam counter.
                if (NPC.netSpam >= 10)
                    NPC.netSpam = 9;
            }

            // Die and create drops after the star is gone.
            if (DeathAnimationTimer >= 125f)
            {
                NPC.active = false;
                NPC.HitEffect();
                NPC.NPCLoot();

                NPC.netUpdate = true;

                // Prevent netUpdate from being blocked by the spam counter.
                if (NPC.netSpam >= 10)
                    NPC.netSpam = 9;
            }

            DeathAnimationTimer++;

        }
    }
    public class LeechBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<LeechPlayer>().leeched = true;
        }
    }
    public class LeechPlayer : ModPlayer
    {
        public bool leeched = false;
        public override void ResetEffects()
        {
            leeched = false;
        }
        public override void UpdateLifeRegen()
        {
            if (leeched)
            {
                if (Player.lifeRegen > 0)
                {
                    Player.lifeRegen = 0;
                }
                Player.lifeRegen -= 120;
            }
        }
    }
}
