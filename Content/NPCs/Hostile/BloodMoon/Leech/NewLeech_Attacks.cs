using CalamityMod;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.NPCs.Other;
using HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.RitualAltarNPC;
using Microsoft.Xna.Framework;
using NoxusBoss.Content.Particles.Metaballs;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Leech
{
    public partial class newLeech
    {
        public enum Behavior
        {
            Debug,
            Idle,
            LookForTarget,
            seekTarget,
            Lunge,
            latch,
            flee,
            DisipateIntoBlood,
            Latch,

            DeathAnim
        }



        void StateMachine()
        {
            switch (CurrentState)
            {
                case Behavior.Debug:
                    //if(Time > 120)
                    //CurrentState = Behavior.DisipateIntoBlood;
                    //Player a = Main.player[NPC.FindClosestPlayer()];
                    //if (a != null)
                    //{
                    //    NPC.Center = Vector2.Lerp(NPC.Center, a.Center, 0.1f);
                    //    NPC.velocity =  new Vector2(0, MathF.Cos(Time / 7.1f) * 10).RotatedBy(NPC.rotation);

                    //}

                    CurrentState = Behavior.Idle;
                    break;
                case Behavior.Idle:
                    CurrentState = Behavior.LookForTarget;
                    break;
                case Behavior.LookForTarget:
                    LookForTarget();

                    break;
                case Behavior.seekTarget:
                    seekTarget();
                    break;
                case Behavior.latch:
                    latch();
                    break;
                case Behavior.flee:
                    ManageFlee();
                    break;
                case Behavior.Lunge:
                    ManageLunge();
                    break;

                case Behavior.DisipateIntoBlood:
                    DisipateIntoBlood(hasUsedEmergency);
                    break;


                case Behavior.DeathAnim:
                    DoDeathAnimation();
                        break;
            }
        }


        void LookForTarget()
        {
            Player temp2 = Main.player[NPC.FindClosestPlayer()];

            List<NPC> temp = new List<NPC>(Main.npc.Length);

            float pickPlayerChance = 0;

            float pickNPCChance = 0;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.type == NPC.type || npc.type == ModContent.NPCType<RitualAltar>())
                    continue;

                if (npc.type == ModContent.NPCType<ExhumedHeart>())
                    continue;
                if (npc.type == ModContent.NPCType<Umbralarva>())

                if (npc.Distance(NPC.Center) < 1000 && !temp.Contains(npc) && !BlackListProjectileNPCs.BlackListedNPCs.Contains(npc.whoAmI))
                {
                    temp.Add(npc);
                }
            }
            if (temp.Count > 1)
            {

                temp.Sort((a, b) => a.Distance(NPC.Center).CompareTo(b.Distance(NPC.Center)));
            }
            else if (temp.Count != 1)
            {
                currentTarget = temp2;
                playerTarget = null;
                NPCTarget = null;
                CurrentState = Behavior.seekTarget;
                return;
            }

            if (blood < bloodBankMax / 1.5f)
            {
                pickNPCChance = (float)bloodBankMax / (blood + 1);
            }
            else
                pickPlayerChance = 0.75f;

            currentTarget = null;
            float thing = Main.rand.NextFloat(0, 1);
            if (pickNPCChance - thing > pickPlayerChance - thing)
            {
                playerTarget = null;
                NPCTarget = null;
                currentTarget = temp[0];
                temp.Clear();
                CurrentState = Behavior.seekTarget;
            }
            else
            {
                playerTarget = null;
                NPCTarget = null;
                currentTarget = temp2;
                temp.Clear();
                CurrentState = Behavior.seekTarget;
            }
            /*

            if (playerTarget.Distance(NPC.Center) > temp[0].Distance(NPC.Center))
            {
                playerTarget = default;
                NPCTarget = default;
                currentTarget = temp[0];
                temp.Clear();

                CurrentState = Behavior.seekTarget;

            }
            else
            {
                currentTarget = playerTarget;
                playerTarget = null;
                NPCTarget = null;
                CurrentState = Behavior.seekTarget;
            }

            */

        }

        void seekTarget()
        {
            if (currentTarget != null)
            {
                NPC.velocity = NPC.AngleTo(currentTarget.Center).ToRotationVector2() * 10;

                if (Main.rand.NextBool(3) && currentTarget is Player && Time % 160 == 0)
                {
                    Time = 0;
                    CurrentState = Behavior.Lunge;
                    

                }
                if (NPC.Center.Distance(currentTarget.Center) < 10f)
                {
                    Time = 0;
                    CurrentState = Behavior.latch;
                }
            }
            else
            {
                CurrentState = Behavior.LookForTarget;
            }
        }
        void ManageLunge()
        {

            if (Time == 1)
            {
                //Main.NewText("i am dashing!");
                SoundEngine.PlaySound(AssetDirectory.Sounds.NPCs.Hostile.BloodMoon.UmbralLeech.Bash with { Pitch = 1.4f});
                //DashCount--;
            }

            if (Time < 45f)
            {
                //  TelegraphRot = MathHelper.Lerp(TelegraphRot, NPC.rotation, 0.1f);
                // Teleport to the target
                if (currentTarget != null && currentTarget.active)
                {
                    
                    NPC.velocity *= 0.1f;
                }
                else
                {
                    CurrentState = Behavior.Idle;
                }
            }
            if (Time > 45 && Time < 120)
            {
                if (Time % 5 == 0)
                {
                    SoundEngine.PlaySound(AssetDirectory.Sounds.NPCs.Hostile.BloodMoon.UmbralLeech.Bash with { MaxInstances = 3 });
                }
            }
            if (Time < 120f)
            {
                accelerationInterp = float.Lerp(accelerationInterp, 1, 0.2f);
                if (currentTarget != null && currentTarget.active)
                {
                    Vector2 toTarget = currentTarget.Center - NPC.Center;
                    float distance = toTarget.Length();
                    Vector2 dir = distance > 0.01f ? toTarget / distance : Vector2.Zero;

                    // Calculate a point beyond the player for overshoot
                    float overshootDistance = 60f; // How far past the player to aim
                    Vector2 overshootTarget = currentTarget.Center + dir * overshootDistance;

                    Vector2 toOvershoot = overshootTarget - NPC.Center;
                    Vector2 desiredVelocity = Vector2.Normalize(toOvershoot) * 20f * accelerationInterp;

                    // Add a small random angle to the dash for less predictability
                    float randomAngle = Main.rand.NextFloat(-0.15f, 0.15f);
                    desiredVelocity = desiredVelocity.RotatedBy(randomAngle);

                    NPC.velocity = Vector2.Lerp(NPC.velocity, desiredVelocity, 0.1f + 0.1f * accelerationInterp);
                }

            }
            else
            {
                
                if (Vector2.Distance(NPC.Center, currentTarget.Center) < 70f)
                    CurrentState = Behavior.seekTarget;
                else
                    if (Main.rand.NextBool(4))
                    CurrentState = Behavior.DisipateIntoBlood;
                else
                    CurrentState = Behavior.seekTarget;

            }

        }
        void latch()
        {
            if (currentTarget == null || !currentTarget.active)
            {

                CurrentState = Behavior.Idle;
            }
            else
            {
                NPC.damage = 0;
                

                NPC.Center = currentTarget.Center;
                if (currentTarget is NPC)
                {
                    if (Time > 180)
                    {
                        NPC temp = currentTarget as NPC;
                        temp.AddBuff(ModContent.BuffType<UmbralSickness>(), 600);
                        Time = 0;
                        NPC.damage = NPC.defDamage;
                        CurrentState = Behavior.flee;
                        blood += bloodBankMax / 5;
                    }
                }
                else
                {
                    if (Time > 60)
                        if (currentTarget is Player)
                        {
                            Player temp = currentTarget as Player;
                            if (!temp.HasIFrames())
                            {
                                temp.AddBuff(ModContent.BuffType<UmbralSickness>(), 600);
                                temp.Heal(-1);

                            }
                            blood += bloodBankMax / 5;
                            Time = 0;
                            NPC.damage = NPC.defDamage;
                            CurrentState = Behavior.flee;
                        }
                        else
                            throw new System.Exception("what the fuck did you do man.");
                }
            }

        }
        void ManageFlee()
        {
            if (currentTarget == null)
            {
                CurrentState = Behavior.Idle;
                return;
            }
            Vector2 difference = NPC.Center.AngleFrom(currentTarget.Center).ToRotationVector2() * 10;
            NPC.velocity = Vector2.Lerp(NPC.velocity, difference, 0.2f);
            Collision.StepUp(ref NPC.position, ref NPC.velocity, 30, 30, ref NPC.stepSpeed, ref NPC.gfxOffY);
            if (NPC.Distance(currentTarget.Center) > 1400 || Time > 240)
            {
                currentTarget = default;
                CurrentState = Behavior.Idle;
            }
        }
        Vector2 DisiapteEnd;
        void DisipateIntoBlood(bool hasused)
        {
            if(currentTarget != null)
            NPC.rotation = currentTarget.AngleFrom(NPC.Center);

            if(Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.Server)
                HeavenlyArsenal.ClearAllBuffs(NPC);
            ForceResetHitboxes();
            if (Time < 200)
            {   
                
                NPC.takenDamageMultiplier = float.Lerp(NPC.takenDamageMultiplier, 0, 0.2f);
                NPC.velocity *= 0.9f;
                if (NPC.velocity.Length() < 0.1f)
                {
                    NPC.Opacity = float.Lerp(NPC.Opacity, -1f, 0.1f);
                    if (NPC.Opacity > 0.1f)
                        foreach (var box in AdjHitboxes)
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                                Dust.NewDustPerfect(box.Center(), DustID.Blood, velocity, Scale: 1.5f);
                            }
                        }

                }
                if (NPC.Opacity < 0.1f)
                {
                    NPC.Opacity = 0;
                    NPC.dontTakeDamage = true;
                    if (DisiapteEnd == default)
                        DisiapteEnd = Main.rand.NextVector2CircularEdge(400, 400);
                    NPC.Center = currentTarget.Center + DisiapteEnd;
                    NPC.velocity = NPC.Center.AngleTo(currentTarget.Center).ToRotationVector2() * 10;
                    if (!hasused)
                    {
                        NPC.life *= 3;
                        hasUsedEmergency = true;
                    }

                }

            }
            else
            {
                if (Time == 201)
                    NPC.netUpdate = true;
                NPC.velocity *= NPC.Opacity;
                //Main.NewText(NPC.Opacity);
                if (DisiapteEnd != default)
                    DisiapteEnd = default;

                if (NPC.Opacity < 1)
                    NPC.Opacity = float.Lerp(NPC.Opacity, 1, 0.1f);

                if (NPC.Opacity > 0.9f)
                {
                    NPC.takenDamageMultiplier = float.Lerp(NPC.takenDamageMultiplier, default, 0.2f);
                    NPC.dontTakeDamage = false;
                }

                if (NPC.Opacity >= 0.99f)
                {
                    NPC.takenDamageMultiplier = 1;
                    NPC.Opacity = 1;
                    CurrentState = Behavior.seekTarget;
                    Time = 0;
                }
            }


        }

        void DoDeathAnimation()
        {
            if(NPC.GetGlobalNPC<RitualBuffNPC>().hasRitualBuff && NPC.GetGlobalNPC<RitualBuffNPC>().IsRessurecting)
            {
                CurrentState = Behavior.Idle;
                return;
            }
            BloodMetaball metaball = ModContent.GetInstance<BloodMetaball>();
            const int DeathTime = 260;
            CosmeticTime = -1;
            if (Time == 1 && Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(AssetDirectory.Sounds.NPCs.Hostile.BloodMoon.UmbralLeech.DyingNoise with { MaxInstances = 1 });


            }

            if(Time % 60 == 0)
            {
                NPC.netUpdate = true;
            }
            if (Time > DeathTime)
            {
                for (int u = 0; u < AdjHitboxes.Length; u++)
                {
                    createGore(AdjHitboxes[u].Center(), u);
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 bloodSpawnPosition = AdjHitboxes[u].Center();//Main.npc[Segments[u].whoAmI].Center;
                        Vector2 bloodVelocity = (Main.rand.NextVector2Circular(30f, 30f) - NPC.velocity) * Main.rand.NextFloat(0.2f, 1.2f);
                        metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(10f, 40f), Main.rand.NextFloat(2f));
                    }
                }
                SoundEngine.PlaySound(new SoundStyle("HeavenlyArsenal/Assets/Sounds/NPCs/Hostile/BloodMoon/UmbralLeech/Death", 3) with { MaxInstances = 0 }, NPC.Center);

                if (blood == bloodBankMax)
                {
                    NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Umbralarva>());
                }

                NPC.NPCLoot();
                NPC.life = 0;
                NPC.active = false;
            }
            else if(Time<DeathTime - 10 && Time > DeathTime - 20)
            {
                SoundEngine.PlaySound(AssetDirectory.Sounds.NPCs.Hostile.BloodMoon.UmbralLeech.Explode, NPC.Center);
            }
            else if(Time< DeathTime- 20)
            {
                
                if (Time % 6 == 0)
                {
                 
                    for (int u = 0; u < AdjHitboxes.Length; u++)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 bloodSpawnPosition = AdjHitboxes[u].Center()    ;//Main.npc[Segments[u].whoAmI].Center;
                            Vector2 bloodVelocity = (Main.rand.NextVector2Circular(30f, 30f) - NPC.velocity) * Main.rand.NextFloat(0.2f, 1.2f);
                            metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(10f, 40f), Main.rand.NextFloat(2f));
                        }
                    }

                }

                NPC.velocity *= 0.8f;
                for(int i = 0; i< AdjHitboxes.Length; i++)
                {
                    AdjHitboxes[i].Location += Main.rand.NextVector2Circular(0, 10).RotatedBy(NPC.rotation).ToPoint();
                }
            }    
        }
    }
}
