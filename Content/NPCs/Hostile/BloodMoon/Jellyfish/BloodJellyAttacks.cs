using HeavenlyArsenal.Core.Systems;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish
{
    partial class BloodJelly
    {
        public enum Behavior
        {
            Drift,
            FindTarget,

            CommandThreat,
            Railgun,
            Reposition,
            DiveBomb,

            StickAndExplode,

            Recycle
        }
        public Behavior CurrentState
        {
            get => (Behavior)NPC.ai[1];
            set => NPC.ai[1] = (int)value;
        }

        public void StateMachine()
        {
            switch (CurrentState)
            {
                case Behavior.Drift:
                    Drift();
                    break;

                case Behavior.FindTarget:
                    FindTarget();
                    break;
                case Behavior.CommandThreat:
                    CommandThreat();
                    break;

                case Behavior.Railgun:
                    RailGun();
                    break;
                case Behavior.Reposition:
                    Reposition();
                    break;
                case Behavior.DiveBomb:
                    DiveBomb();
                    break;

                case Behavior.StickAndExplode:
                    StickAndExplode();
                    break;

                case Behavior.Recycle:
                    Recycle();
                    break;
            }
        }

        void Drift()
        {

            if (currentTarget != null)
                NPC.velocity = NPC.AngleTo(currentTarget.Center).ToRotationVector2() * 3;
            NPC.rotation = NPC.rotation.AngleLerp(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.2f);
            //NPC.Center = Vector2.Lerp(NPC.Center, NPC.Center + new Vector2(0,MathF.Sin(Time/10.1f)*10), 0.2f);
            if (Time % 30 == 0)
            {
                CurrentState = Behavior.FindTarget;
            }
        }

        private int DetectionRange = 700;

        void FindTarget()
        {
            HashSet<Entity> temp = new HashSet<Entity>(200);
            foreach (Player player in Main.ActivePlayers)
            {
                if (player.Distance(NPC.Center) <= DetectionRange)
                {
                    temp.Add(player);
                }
            }
            if (temp.Count <= 0)
            {
                CurrentState = Behavior.Drift;
                return;
            }
            List<Entity> temp2 = temp.ToList<Entity>();

            temp2.Sort((a, b) => a.Distance(NPC.Center).CompareTo(b.Distance(NPC.Center)));

            string debugOutput = "";
            foreach (Entity a in temp2)
            {
                debugOutput += $"{a.ToString()}, {a.Distance(NPC.Center)}" + $"\n";
            }
            //Main.NewText(debugOutput);

            Time = 0;
            currentTarget = temp2[0];
            CurrentState = Behavior.CommandThreat;

        }

        void CommandThreat()
        {
            if (ThreatCount <= 0)
            {
                Vector2 down = Vector2.UnitY;

                Vector2 toPlayer = currentTarget.Center - NPC.Center;
                toPlayer.Normalize();


                float angleToDown = MathF.Acos(Vector2.Dot(toPlayer, down));


                if (toPlayer.Y < 0 || angleToDown > MathHelper.PiOver4)
                {
                    Time = 0;
                    CurrentState = Behavior.Railgun;
                    return;
                }



                Time = 0;
                CurrentState = Behavior.DiveBomb;
                return;
            }
            NPC.rotation = NPC.rotation.AngleLerp(NPC.AngleTo(currentTarget.Center) + MathHelper.PiOver2, 0.6f);
            NPC.velocity *= 0.8f;
            if (Time % Main.rand.Next(1, 4) == 0)
                for (int i = 0; i < ThreatCount; i++)
                {
                    int uuid = ThreatIndicies[i];
                    //Main.NewText(uuid);
                    Projectile threat = Main.projectile[uuid];//Main.projectile[ThreatIndicies[i]];
                    //Main.NewText("success!");
                    if (threat == null)
                    {
                        ThreatIndicies.RemoveAt(i);
                        i--;
                        continue;
                    }
                    if (Main.rand.NextBool(ThreatCount * 4))
                    {
                        TheThreat theThreat = threat.ModProjectile as TheThreat;

                        theThreat.Target = this.currentTarget;
                        theThreat.Time = 0;
                        theThreat.CurrentState = TheThreat.Behavior.Concussive;
                        SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.FeatherAppear with { MaxInstances = 2, PitchVariance = 0.2f, Volume = 0.25f });
                        NPC.velocity += (NPC.rotation + MathHelper.PiOver2).ToRotationVector2() * ThreatCount / 4;
                        ThreatIndicies.RemoveAt(i);
                        i--;
                    }
                }
        }
        public float recoilInterp;
        void RailGun()
        {
            const int fireInterval = 60;
            const int lockOnDuration = 55;
            const int chargeDuration = 5;
            const int attackEnd = 60 * 3 + 10;

            OpenInterpolant = float.Lerp(OpenInterpolant, 1, 0.09f);
            NPC.velocity *= 0.1f;

            int attackCycleTime = Time % fireInterval;

            if (attackCycleTime < lockOnDuration)
            {
                // Track target while locking on
                float desiredRot = NPC.Center.AngleTo(currentTarget.Center + currentTarget.velocity * 2) + MathHelper.PiOver2;
                NPC.rotation = NPC.rotation.AngleLerp(desiredRot, 0.2f);
            }
            else if (attackCycleTime > lockOnDuration)
            {
                //sound effect i think
            }

            if (attackCycleTime == lockOnDuration + chargeDuration - 1)
            {
                Vector2 velocity = (NPC.rotation - MathHelper.PiOver2).ToRotationVector2() * 10;
                Projectile shot = Projectile.NewProjectileDirect(
                    NPC.GetSource_FromThis(),
                    NPC.Center,
                    velocity,
                    ModContent.ProjectileType<JellyRailProjectile>(),
                    100,
                    0
                );
                if (shot.ModProjectile is JellyRailProjectile rail)
                    rail.OwnerIndex = NPC.whoAmI;
                recoilInterp = 1;
            }

            if (Time > attackEnd + 20)
            {
                Time = 0;
                CurrentState = Behavior.Reposition;
            }
        }

        void DiveBomb()
        {
            const int WindupTime = 90;
            Vector2 down = Vector2.UnitY;

            Vector2 toPlayer = currentTarget.Center - NPC.Center;
            toPlayer.Normalize();

            float angleToDown = MathF.Acos(Vector2.Dot(toPlayer, down));


            if (Time < 60)
                if (toPlayer.Y < 0 || angleToDown > MathHelper.PiOver4)
                {
                    currentTarget = default;
                    Time = 0;
                    CurrentState = Behavior.FindTarget;
                    return;
                }

            if (Time < WindupTime)
            {
                if (Time == WindupTime - 10)
                {
                    SoundEngine.PlaySound(GennedAssets.Sounds.Common.TeleportOut, NPC.Center);
                }
                NPC.knockBackResist = 0;

                // Only rotate toward target if we’re still aligned with bounds
                if (toPlayer.Y > 0 && angleToDown <= MathHelper.PiOver4)
                {
                    NPC.rotation = NPC.rotation.AngleLerp(
                        NPC.AngleTo(currentTarget.Center) + MathHelper.PiOver2,
                        0.5f
                    );
                }
                NPC.velocity *= 0.9f;
                NPC.Center = NPC.Center + Main.rand.NextVector2Unit(NPC.rotation);
            }
            else if (Time >= WindupTime)
            {
                NPC.noTileCollide = false;

                // Clamp the velocity direction so it stays within the 90° downward cone
                Vector2 diveDir = (NPC.rotation - MathHelper.PiOver2).ToRotationVector2();

                // Ensure downward and within cone
                if (Vector2.Dot(diveDir, down) < MathF.Cos(MathHelper.PiOver4))
                    diveDir = Vector2.Normalize(Vector2.Lerp(down, diveDir, 0.5f));
                if (Time == WindupTime + 1)
                    NPC.velocity = diveDir * 30f;
                else
                    NPC.velocity *= 1.12f;
                // Predict new position for this frame
                Vector2 nextPos = NPC.Center + NPC.velocity;

                // Perform a raytrace from old position to new position
                Point? hit = LineAlgorithm.RaycastTo(
                    (int)(NPC.oldPosition.X / 16f),
                    (int)(NPC.oldPosition.Y / 16f),
                    (int)(nextPos.X / 16f),
                    (int)(nextPos.Y / 16f)
                );

                if (hit.HasValue)
                {
                    // Convert back to world coordinates
                    Vector2 hitWorld = hit.Value.ToVector2() * 16f;

                    // Handle impact immediately
                    OnRayImpact(hitWorld);
                    return;
                }
            }

            if (Collision.SolidCollision(NPC.Center, NPC.width, NPC.height))
            {
                currentTarget = default;
                Time = 0;
                CurrentState = Behavior.StickAndExplode;
            }
        }
        private void OnRayImpact(Vector2 hitWorld)
        {

            NPC.noTileCollide = false;
            int SpawnCount = 30;
            NPC.Center += NPC.rotation.ToRotationVector2();
            for (int i = 0; i < SpawnCount; i++)
            {
                Collision.HitTiles(hitWorld, NPC.velocity, NPC.width, NPC.height);

                Dust d = Dust.NewDustDirect(
                    hitWorld - new Vector2(8, 8),
                    16, 16,
                    DustID.Dirt,
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-3f, -1f)
                );
                d.scale = Main.rand.NextFloat(1f, 1.8f);
                d.noGravity = false;

            }
            foreach (Player player in Main.ActivePlayers)
            {
                if (!player.active)
                    continue;

                float distance = Vector2.Distance(player.Center, NPC.Center);

                // Define min and max range for screenshake effect
                float maxRange = 700f;  // beyond this distance, no shake
                float minRange = 150f;  // within this distance, maximum shake

                if (distance < maxRange)
                {
                    // Normalize strength between 0 (at maxRange) and 1 (at minRange)
                    float strength = 1f - MathHelper.Clamp((distance - minRange) / (maxRange - minRange), 0f, 1f);

                    strength = MathF.Pow(strength, 2f); // smoother falloff

                    // Convert to shake magnitude — tweak to taste
                    float shakeMagnitude = MathHelper.Lerp(1f, 10f, strength);

                    if (player.whoAmI == Main.myPlayer)
                    {
                        ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 7f * strength,
                        shakeDirection: NPC.velocity.SafeNormalize(Vector2.Zero) * 2,
                        shakeStrengthDissipationIncrement: 0.7f - strength * 0.1f);
                    }
                }
            }
            SoundEngine.PlaySound(GennedAssets.Sounds.Mars.MissileExplode with { PitchVariance = 1, Pitch = -0.5f }, hitWorld);
            NPC.velocity = Vector2.Zero;
            Time = 0;
            this.CurrentState = Behavior.StickAndExplode;

        }

        void Reposition()
        {
            Entity target = currentTarget;
            if (target == null || !target.active)
                return;

            Vector2 toPlayer = target.Center - NPC.Center;
            float distance = NPC.Distance(currentTarget.Center);

            if (distance < 760f)
            {
                // Normalize direction away from the player
                Vector2 away = -toPlayer.SafeNormalize(Vector2.Zero);

                // Add a strong upward bias to make it flee upward, not just backward
                Vector2 upwardBias = new Vector2(0, -1.5f);

                // Combine the two
                Vector2 fleeDir = (away + upwardBias).SafeNormalize(Vector2.Zero);

                // Desired speed
                float speed = 8f;

                // Smoothly adjust velocity toward desired flee direction
                NPC.velocity = Vector2.Lerp(NPC.velocity, fleeDir * speed, 0.1f);

                NPC.rotation = NPC.rotation.AngleLerp(NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.1f);
            }
            else
            {
                if (Time < 200)
                    return;
                Time = 0;
                CurrentState = Behavior.Drift;
                // When player is far enough, maybe hover or patrol?
                NPC.velocity *= 0.95f;
                return;
            }
        }


        int nextBeepTime = 0;
        private bool HasExploded;
        void StickAndExplode()
        {
            NPC.knockBackResist = 0;
            NPC.velocity.X *= 0.3f;
            NPC.noGravity = true;


            int maxTime = 330;

            float progress = MathHelper.Clamp((float)Time / (maxTime - 30), 0f, 1f);

            float currentBeepDelay = MathHelper.Lerp(60f, 5f, progress * progress);

            // play beep when it's time
            if (Time >= nextBeepTime)
            {
                //placeholder
                SoundEngine.PlaySound(GennedAssets.Sounds.Common.Twinkle, NPC.Center);
                nextBeepTime = Time + (int)currentBeepDelay;
                warningPulseSpeed = 1;
            }
            warningPulseSpeed = float.Lerp(warningPulseSpeed, 0, 0.2f);
            if (Time >= maxTime && !HasExploded)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Enemies.DismalLanternExplode);
                int Type = ModContent.NPCType<JellyBloom>();

                Vector2 Pos = NPC.Center + new Vector2(0, 60).RotatedBy(NPC.rotation);//Tendrils[tendrilCount].Item1[Tendrils[tendrilCount].Item1.Length - 1];
                Projectile explosion = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), Pos, Vector2.Zero, ModContent.ProjectileType<JellyExplosion>(), 1000, 10);


                HasExploded = true;
            }
            if (Time >= maxTime + 100)
            {
                NPC.StrikeInstantKill();
            }
        }


        void Recycle()
        {

        }
    }
}
