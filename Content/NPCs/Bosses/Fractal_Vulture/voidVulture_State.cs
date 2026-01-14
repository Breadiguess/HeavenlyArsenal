using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles;
using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Solyn;
using Luminance.Core.Graphics;
using Luminance.Core.Sounds;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Friendly;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using NoxusBoss.Core.SoundSystems.Music;
using NoxusBoss.Core.Utilities;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture
{
    partial class voidVulture
    {

        /// <summary>
        /// A looping sound for the VomitCone attack.
        /// </summary>
        public LoopedSoundInstance? VomitLoop
        {
            get;
            set;
        }
        public LoopedSoundInstance? RiserForSpin
        {
            get;
            set;
        }
        public enum Behavior
        {
            debug,
            reveal,
            Roar,
            Idle,
            VomitCone,
            RiseSpin,
            CollidingCommet,
            EjectCoreAndStalk,

            FlyAwayAndAmbush,
            Medusa,

            PhaseTransition,


            FlyEjectBomb,
            placeholder2,
            placeholder3


        }
        public Behavior currentState
        {
            get => (Behavior)NPC.ai[2];
            set => NPC.ai[2] = (float)value;
        }
        public Behavior previousState;
        private int attackIndex = 0;
        private static Behavior[] AttackCycleOrder = new Behavior[]
        {
            Behavior.FlyAwayAndAmbush,
            Behavior.VomitCone,
            Behavior.RiseSpin,
            Behavior.CollidingCommet,
            Behavior.EjectCoreAndStalk
        };

        private static Behavior[] AttackCycleOrderPhase2 = new Behavior[]
        {
            Behavior.VomitCone,
            Behavior.EjectCoreAndStalk,
            Behavior.FlyEjectBomb,
            Behavior.FlyAwayAndAmbush,
            Behavior.placeholder2,
            Behavior.placeholder3
        };
        public void StateMachine()
        {
            switch (currentState)
            {
                case Behavior.debug:
                    Debug();
                    break;
                case Behavior.reveal:
                    Reveal();
                    break;
                case Behavior.Roar:
                    Roar();
                    break;
                case Behavior.Idle:
                    Idle();
                    break;
                case Behavior.VomitCone:
                    VomitCone();
                    break;
                case Behavior.RiseSpin:
                    RiseSpin();
                    break;
                case Behavior.CollidingCommet:
                    SpawnCommets();
                    break;
                case Behavior.EjectCoreAndStalk:
                    EjectCoreAndStalk();
                    break;
                case Behavior.FlyAwayAndAmbush:
                    FlyAwayAmbush();
                    break;
                case Behavior.Medusa:
                    Medusa();
                    break;
                case Behavior.PhaseTransition:
                    ManageTransitionCutscene();
                    break;
                case Behavior.FlyEjectBomb:
                    flyDropBombs();
                    break;
                case Behavior.placeholder2:
                    placeholder2();
                    break;
                case Behavior.placeholder3:
                    placeholder3();
                    break;
            }
        }


        void ManageTransitionCutscene()
        {
            if (StoredVomit != null)
                VomitLoop.Stop();
            if (RiserForSpin != null)
                RiserForSpin.Stop();

            if (Time == 1)
            {
                //BattleSolynBird.SummonSolynForBattle(NPC.GetSource_FromThis(), currentTarget.Center, BattleSolynBird.SolynAIType.FightBird);
                  BattleSolyn.SummonSolynForBattle(NPC.GetSource_FromThis(), currentTarget.Center, Funny);
            }
            if (Time == 0)
            {
                attackIndex = 0;
                NPC.canDisplayBuffs = false;
                NPC.immortal = true;
                NPC.Opacity = 1;
                NPC.dontTakeDamage = true;
                if (CoreDeployed)
                    CoreDeployed = false;

                hideBar = true;
            }
            const int StartTime = 180;
            const int EndTime = 680;
            NPC.velocity *= 0.7f;
            float cameraZoomInterpolant = InverseLerp(0f, 11f, Time);
            CameraPanSystem.PanTowards(NPC.Center, cameraZoomInterpolant);
            MusicVolumeManipulationSystem.MuffleFactor = 0.1f;

            if (Time == StartTime)
            {
                NPC.lifeMax = (int)(NPC.lifeMax * 1.5f);
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Shriek with { PitchVariance = 0.2f, Pitch = -1.2f }, NPC.Center).WithVolumeBoost(3);

                hideBar = false;
            }

            if (Time % 7 == 0 && Time > StartTime && Time < EndTime)
            {
                ScreenShakeSystem.StartShakeAtPoint(HeadPos, 0.2f * 6f);

                ExpandingChromaticBurstParticle burst = new ExpandingChromaticBurstParticle(HeadPos, Vector2.Zero, Main.rand.NextBool() ? Color.White : Color.White, 30, 0.2f, 1.7f);
                burst.Spawn();

                HeadPos += Main.rand.NextVector2CircularEdge(50f, 50f) * 0.2f;

                GeneralScreenEffectSystem.ChromaticAberration.Start(NPC.Center, 6f, 7);
            }

            if (Time > StartTime && NPC.life <= NPC.lifeMax)
            {
                NPC.life = (int)float.Lerp(NPC.life, NPC.lifeMax, 0.08f);
                NPC.life++;
            }
            if (Time > EndTime)
            {
                currentState = Behavior.Idle;
                NPC.canDisplayBuffs = true;
                NPC.immortal = false;
                NPC.life = NPC.lifeMax;
                HasDoneCutscene = true;
                NPC.dontTakeDamage = false;
                NPC.noGravity = false;
            }
        }

        private Behavior GetNextAttack()
        {
            Behavior next = !HasSecondPhaseTriggered ? AttackCycleOrder[attackIndex] : AttackCycleOrderPhase2[attackIndex];

            attackIndex++;
            if (attackIndex >= (!HasSecondPhaseTriggered? AttackCycleOrder.Length : AttackCycleOrderPhase2.Length))
                attackIndex = 0;

            return next;
        }

        void Debug()
        {
            TargetPosition = NPC.Center;
            currentState = Behavior.reveal;
            return;

        }
        void Idle()
        {


            float hoverHeight = -260f;
            float horizontalOffsetMax = 180f;

            float drift = (float)Math.Sin(Time / 35f) * horizontalOffsetMax;

            hoverHeight = -130f * Math.Abs(MathF.Sin(Time / 70f)) + -230;
            Vector2 idealPos = currentTarget.Center + new Vector2(drift, hoverHeight);
            TargetPosition = idealPos;

            if (Time > (HasSecondPhaseTriggered ? 140 : 240))
            {
                Time = 0;

                currentState = GetNextAttack();

                return;
            }

        }

        void Reveal()
        {
            const int headraiseTime = 270;

            const int endTime = 340;

            if(Time == 1)
            {
                targetInterpolant = 0;
                for (int i = 0; i < 2; i++)
                {
                    wings[i].WingRotation = MathHelper.ToRadians(50);
                }
            }
            float cameraZoomInterpolant = InverseLerp(0f, 11f, Time);

            CameraPanSystem.PanTowards(NPC.Center, cameraZoomInterpolant);
            MusicVolumeManipulationSystem.MuffleFactor = 0.1f;
            hideBar = true;
            NPC.velocity *= 0;
            if (Time > 12)
                NPC.Opacity = Utils.SmoothStep(0, 1, Time);//float.Lerp(NPC.Opacity, 1, 0.2f);
            if (Time == 30)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Common.Glitch with { Pitch = 0.2f, MaxInstances = 0 });
            }
            if (Time == 100)
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Phase2IntroFaceManifest with { Volume = 3f });
            if (Time > 100 && Time < 380)
            {
                ScreenShakeSystem.StartShakeAtPoint(HeadPos, 6*InverseLerp(100, headraiseTime, Time));
                if (Time > 100 && Time < headraiseTime + 40)
                {
                    if(Time < 120)
                    NPC.Center = Vector2.Lerp(NPC.Center, NPC.Center - new Vector2(0, 80), 0.2f);
                    HeadPos = NPC.Center + new Vector2(0, 100);
                }
                if (Time > headraiseTime)
                {
                    HeadPos = Vector2.Lerp(HeadPos, NPC.Center + new Vector2(0, -100), 0.2f);

                    for (int i = 0; i < 2; i++)
                    {
                        wings[i].WingRotation = wings[i].WingRotation.AngleLerp(MathHelper.ToRadians(-30), 0.02f);
                    }

                }
                    
            }

          
            if (Time > endTime)
            {
               
                //TileDisablingSystem.TilesAreUninteractable = true;
                //Main.NewText($"{NPC.GivenName} has awoken! ", Color.Purple);
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Phase2IntroNeckSnap);
                currentState = Behavior.Roar;
                Time = -1;
                
            }

        }

        private void Roar()
        {
            // Plan (pseudocode):
            // 1. Keep wing rotations lerping to final pose.
            // 2. During the scream window, compute a "bump" interpolant that
            //    rises then falls using InverseLerpBump so that intensity is strongest
            //    in the middle of the scream region.
            // 3. Use that bump to:
            //    - vary spawn frequency (probability per tick) of chromatic burst particles
            //    - vary burst size/count/scale/lifetime
            //    - vary screen-shake strength (we combine bump with a linear fade so the
            //      shake peaks earlier and decays nicely)
            // 4. Trigger bursts probabilistically each tick according to computed probability.
            // 5. Preserve the initial "one-shot" scream sound and existing fade-out behavior.
            //
            // Implementation notes:
            // - Use InverseLerpBump(ScreamTime, ScreamTime + rampIn, StopScreaming - rampOut, StopScreaming, Time)
            //   to produce a bump in the middle of the interval.
            // - Convert bump into spawn probability and particle parameters via MathHelper.Lerp.
            // - Use ScreenShakeSystem.StartShakeAtPoint with a shake strength derived from both
            //   the bump (for mid-peak shaping) and a simple linear fade (to respect original decay).
            // - Use Main.rand.NextFloat() to determine whether to spawn a burst on this tick.

            for (int i = 0; i < 2; i++)
            {
                wings[i].WingRotation = wings[i].WingRotation.AngleLerp(MathHelper.ToRadians(-30), 0.02f);
            }

            const int ScreamTime = 60;
            const int StopScreaming = 320;
            const int EndBehavior = 380;
            float cameraZoomInterpolant = 1f;
            CameraPanSystem.PanTowards(NPC.Center, cameraZoomInterpolant);
            MusicVolumeManipulationSystem.MuffleFactor = 0.1f;
            if (Time >= ScreamTime && Time < StopScreaming)
            {
                if (Time == ScreamTime)
                {
                    string typeName = NPC.FullName;
                    if (Main.netMode == 0)
                        Main.NewText(Language.GetTextValue("Announcement.HasAwoken", typeName), 175, 75);
                    else if (Main.netMode == 2)
                        ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasAwoken"), new Color(175, 75, 255));

                    SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Roar with {  Pitch = -0.3f}, NPC.Center).WithVolumeBoost(3);
                }

                // Create a bump-shaped interpolant that peaks roughly mid-way through the scream window.
                // rampIn and rampOut control how quickly the bump rises/falls.
                float rampIn = 60f;
                float rampOut = 80f;
                float bump = InverseLerpBump(ScreamTime, ScreamTime + rampIn, StopScreaming - rampOut, StopScreaming, Time);
                bump = MathHelper.Clamp(bump, 0f, 1f);

                // Also compute a simple linear fade across the window for screenshake base behavior.
                float linearFade = MathHelper.Clamp(InverseLerp(ScreamTime, StopScreaming, Time), 0f, 1f);

                // Frequency (probability per tick) increases with bump.
                float spawnProbability = MathHelper.Lerp(0.08f, 0.25f, bump);

                // Particle parameters scale with bump.
                int burstCount = (int)MathHelper.Lerp(12f, 18f, bump);
                float burstScale = MathHelper.Lerp(0.12f, 0.28f, bump);
                float burstLife = MathHelper.Lerp(0.6f, 1.7f, bump);

                // Screen shake: combine bump and linear fade to produce a natural feel.
                // We want strong shake early-to-mid and reduce to none by StopScreaming.
                float shakeStrength = 6f * (1f - linearFade) * (1f - 0.4f * bump); // bump slightly tempers peak
                shakeStrength = Math.Max(0f, shakeStrength);

                // Spawn bursts probabilistically (so frequency changes smoothly).
                if (Main.rand.NextFloat() < spawnProbability || Time % 12 == 0) // keep occasional rhythm at very low bump
                {
                    ScreenShakeSystem.StartShakeAtPoint(HeadPos, shakeStrength, shakeStrengthDissipationIncrement: 0.4f);

                    ExpandingChromaticBurstParticle burst = new ExpandingChromaticBurstParticle(
                        HeadPos,
                        Vector2.Zero,
                        Color.White * 0.6f,
                        burstCount,
                        burstScale,
                        burstLife);

                    burst.Spawn();
                }
            }
            if (Time >= EndBehavior)
            {
                currentState = Behavior.Idle;
                targetInterpolant = 0.2f;
                NPC.dontTakeDamage = false;
                hideBar = false;
                Time = -1;

            }
        }

        #region cone vomit
        float neckSpinInterpolant;
        float baseRotation;
        Vector2 VomitDirection;
        Projectile StoredVomit;
        public Vector2 SolynChosenShield;
        public static int VomitCone_ShootStart = 40;
        public static int VomitCone_ShootStop
        {
            get => !Myself.As<voidVulture>().HasSecondPhaseTriggered ? 165 : 260;
        }
        public static int VomitCone_ShootEnd
        {
            get => !Myself.As<voidVulture>().HasSecondPhaseTriggered ? 200 : 300;
        }
        void VomitCone()
        {
            const int ShootStart = 40;
            int ShootStop = VomitCone_ShootStop;
            int ShootEnd = VomitCone_ShootEnd;

            float tNorm = InverseLerp(ShootStart, ShootStop, Time);
            float bell = Convert01To010(tNorm);

            Vector2 toTarget = NPC.DirectionTo(currentTarget.Center);
            float distToTarget = NPC.Distance(currentTarget.Center);
            float angleToTarget = NPC.Center.AngleTo(currentTarget.Center)
                                + (HasSecondPhaseTriggered ? MathHelper.Pi : 0);

            if (Time == 1)
            {
                NPC.damage = 0;

                // Choose offset position without lerping through player
                int side = Math.Sign(NPC.Center.X - currentTarget.Center.X);
                if (side != 0)
                {
                    Vector2 desiredOffset = new Vector2(250 * side, 0);
                    Vector2 safe = currentTarget.Center + desiredOffset;

                    // Prevent passing directly through player *crosses fingers*    
                    if (Collision.CheckAABBvLineCollision(
                        currentTarget.Hitbox.TopLeft(), currentTarget.Hitbox.Size(),
                        NPC.Center, safe))
                    {
                        // Pick a vertical offset instead
                        safe = currentTarget.Center + new Vector2(0, 240 * side)
                            .RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f));
                    }

                    //TargetPosition = safe.RotatedByRandom(MathHelper.TwoPi);
                }
                if (!HasSecondPhaseTriggered)
                    NPC.velocity += toTarget * 10;
                else
                    SolynChosenShield = currentTarget.Center - NPC.Center;
            }

            if (Time <= ShootStart)
            {
                if (Time == 1)
                {
                    VomitLoop?.Stop();
                    VomitLoop = LoopedSoundManager.CreateNew(
                        GennedAssets.Sounds.Avatar.HeavyBloodStreamLoop,
                        () => !NPC.active);

                    SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Clap with
                    {
                        PitchVariance = 0.2f,
                        Pitch = -1
                    });
                }

                baseRotation = angleToTarget;
                HeadPos = NPC.Center + angleToTarget.ToRotationVector2() * 70f;

                if (Time % 5 == 0)
                    HeadPos += Main.rand.NextVector2Square(1, 3);

                Vector2 baseAimDir = NPC.DirectionTo(currentTarget.Center);
                VomitDirection = baseAimDir;
            }

            //start shooting
            if (Time >= ShootStart)
            {
                float predictedDist = (currentTarget.Center + currentTarget.velocity)
                    .Distance(NPC.Center);

                bool targetMovingAway = predictedDist > distToTarget;

                // Movement during the vomit
                if (targetMovingAway && !HasSecondPhaseTriggered)
                {
                    float followStrength = InverseLerp(0f, 1000f, distToTarget);
                    NPC.velocity = Vector2.Lerp(NPC.velocity,
                        currentTarget.velocity * 0.8f,
                        followStrength);
                }
                else
                {
                    float attackWeight = InverseLerpBump(
                        ShootStart, ShootStart + 40,
                        ShootStop - 40, ShootEnd,
                        Time);

                    float suckStrength = InverseLerp(440, 680, distToTarget);
                    SuckNearbyPlayersGently(4000, suckStrength * attackWeight);

                    NPC.velocity *= 0.8f;
                }

                // Vomit sound control
                if (VomitLoop != null)
                {
                    VomitLoop.Update(HeadPos, sound =>
                    {
                        sound.Pitch = -1.3f * bell + 0.3f;
                        sound.Volume = 2.4f * bell;
                    });
                }


                if (!HasSecondPhaseTriggered)
                    neckSpinInterpolant = MathHelper.ToRadians(250) * InverseLerp(ShootStart, ShootStop, Time);
                else
                    neckSpinInterpolant = tNorm;

                float spinBase = baseRotation;
                Vector2 headOffset;

                if (!HasSecondPhaseTriggered)
                {
                    Vector2 sweepRoot = NPC.Center;

                    float sweepAngleOffset =
                        (neckSpinInterpolant - MathHelper.PiOver2) *
                        -VomitDirection.Y.NonZeroSign();

                    headOffset = new Vector2(90f, 0f)
                    .RotatedBy(baseRotation + sweepAngleOffset);

                    HeadPos = sweepRoot + headOffset;

                }
                else
                {
                    headOffset = new Vector2(90, 0)
                        .RotatedBy(spinBase + MathHelper.TwoPi * neckSpinInterpolant * 2);
                }

                HeadPos = (Time < ShootStart + 10)
                    ? Vector2.Lerp(HeadPos, NPC.Center + headOffset, 0.25f)
                    : NPC.Center + headOffset;
            }

            // spawn Vomit COne
            if (Time == ShootStart)
            {
                targetInterpolant = 0;
                StoredVomit = Main.projectile[
                    NPC.NewProjectileBetter(NPC.GetSource_FromThis(),
                    HeadPos, Vector2.Zero,
                    ModContent.ProjectileType<ConeVomit>(),
                    (int)(NPC.defDamage / 1.6f), 0)];

                (StoredVomit.ModProjectile as ConeVomit).Owner = NPC;

                NPC.velocity -= NPC.DirectionTo(HeadPos) * 10;
            }

            // Scale vomit & fire goop
            if (Time > ShootStart && Time < ShootStop)
            {
                float fireScale = InverseLerpBump(
                    ShootStart, ShootStart + 20,
                    ShootStop - 20, ShootEnd,
                    Time);

                StoredVomit.scale = fireScale;

                if (Time < ShootStop)
                {
                    Vector2 baseV = (NPC.Center.AngleTo(HeadPos)).ToRotationVector2() * 26f;

                    float divergence = Convert01To010(
                        InverseLerp(ShootStart, ShootEnd, Time));

                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 shot = baseV.RotatedByRandom(
                                (MathHelper.PiOver4 * 0.5f) * divergence)
                            * Main.rand.NextFloat(0.9f, 1.2f) * 1.4f;

                        if (Main.rand.NextBool(2))
                        {
                            SoundEngine.PlaySound(
                                GennedAssets.Sounds.Avatar.DisgustingStarSever with
                                {
                                    MaxInstances = 0,
                                    PitchVariance = 0.365f,
                                    PitchRange = (-2, -1)
                                }).WithVolumeBoost(0.2f);

                            NPC.NewProjectileBetter(NPC.GetSource_FromThis(),
                                HeadPos + shot, shot,
                                ModContent.ProjectileType<NowhereGoop>(),
                                (int)(NPC.defDamage * 1.12f), 0);
                        }
                    }
                }
            }

            if (Time > ShootStop)
            {
                if (VomitLoop?.LoopIsBeingPlayed ?? false)
                    VomitLoop.Stop();

                if (StoredVomit != null)
                    StoredVomit.scale = MathHelper.Lerp(StoredVomit.scale, 0, 0.2f);
            }

            if (Time > ShootEnd)
            {
                targetInterpolant = 0.2f;
                NPC.velocity = Vector2.Zero;
                neckSpinInterpolant = 0;
                baseRotation = 0;

                Time = 0;
                previousState = currentState;
                currentState = Behavior.Idle;
            }
        }




        void SuckNearbyPlayersGently(float radius = 900f, float pullStrength = 0.35f)
        {
            Vector2 center = NPC.Center;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (!p.active || p.dead)
                    continue;

                float dist = Vector2.Distance(p.Center, center);
                if (dist > radius)
                    continue;


                if (p.grappling[0] != -1)
                    continue;


                Vector2 dir = (center - p.Center).SafeNormalize(Vector2.Zero);

                float closeness = Utils.GetLerpValue(radius, 0f, dist, true);

                p.velocity += dir * pullStrength * closeness;
                if (pullStrength > 0)
                    p.mount?.Dismount(p);
            }
        }
        #endregion


        void RiseSpin()
        {
            const int StartTime = 40;
            int AudioTime =!HasSecondPhaseTriggered? 165 : 130;
            int RiseTime = !HasSecondPhaseTriggered? 230 : 195;
            int SprayProjectileTime = !HasSecondPhaseTriggered ? 255 : 225;
            int BehaviorEnd = !HasSecondPhaseTriggered ? 360 : 360-25;

            int projectileCount = !HasSecondPhaseTriggered ? 12 : 30;
            if (NPC.Opacity < 0.2f)
            {
                NPC.canDisplayBuffs = true;
                NPC.dontTakeDamage = true;
            }
            else
            {
                NPC.canDisplayBuffs = false;
                NPC.dontTakeDamage = false;
            }

            if (Time < StartTime)
            {

                NPC.Opacity = float.Lerp(NPC.Opacity, 0, 0.2f);
            }
            if (NPC.Opacity < 0.1 && Time < RiseTime)
            {

                NPC.Opacity = 0;
                NPC.Center = currentTarget.Center + new Vector2(0, 500) + currentTarget.velocity * 16;
                ResetTail();
                TargetPosition = NPC.Center;
            }
            if (Time == AudioTime)
            {

                if (RiserForSpin != null)
                    RiserForSpin.Stop();
                RiserForSpin = LoopedSoundManager.CreateNew(GennedAssets.Sounds.Avatar.LilyFiringLoop, () => !NPC.active);
            }
            if (RiserForSpin != null)

                RiserForSpin.Update(NPC.Center, sound =>
                {
                    float thing = InverseLerpBump(AudioTime, AudioTime + 70, SprayProjectileTime, BehaviorEnd, Time);
                    //Main.NewText(thing);
                    //float thing = InverseLerpBump(ShootStart, ShootStart + 20, ShootStop - 20, ShootEnd, Time);
                    sound.Volume = 2.5f * thing; //InverseLerp(165, RiseTime, Time);
                    sound.Pitch = 1.2f * InverseLerp(165, RiseTime, Time);
                });

            if (Time > RiseTime - 30)

                NPC.Opacity = InverseLerp(RiseTime - 30, RiseTime, Time);
            if (Time > RiseTime - 20)
            {




                NPC.damage = NPC.defDamage;
                if (Time < RiseTime)
                    ScreenShakeSystem.StartShakeAtPoint(HeadPos, 0.2f * 6f);
                if (Time == RiseTime)
                {
                    ResetTail();
                    
                    targetInterpolant = !HasSecondPhaseTriggered? 0.08f : 0.09f;
                    //SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Chirp with { Volume = 2f, Pitch = 1.2f }).WithVolumeBoost(2);
                    SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.BloodFountainErupt with { Pitch = 1.4f }).WithVolumeBoost(2);
                    TargetPosition = new Vector2(NPC.Center.X, currentTarget.Center.Y) - new Vector2(0, 600);
                }
                HeadPos = NPC.Center + TargetPosition.AngleFrom(NPC.Center).ToRotationVector2() * 60;
                if (Time == SprayProjectileTime)
                {
                    float thing = !HasSecondPhaseTriggered ? 7 : 12;
                    SoundEngine.PlaySound(GennedAssets.Sounds.Genesis.AntiseedPlant with { Volume = 4, MaxInstances = 1, Pitch = -1.3f }).WithVolumeBoost(5);
                    for (int x = 0; x < 2; x++)
                    {

                        for (int i = 1; i < projectileCount; i++)
                        {
                            Vector2 adjustedV = new Vector2(40, 0).RotatedBy((i / thing * MathHelper.PiOver2) * (x % 2 == 0 ? -1 : 1) + MathHelper.PiOver2) * (i % 2 == 0 ? 0.5f : 0.7f) * (!HasSecondPhaseTriggered ? 1 : 1.4f);

                            NPC.NewProjectileBetter(NPC.GetSource_FromThis(), NPC.Center, adjustedV, ModContent.ProjectileType<NowhereGoop>(), (int)(NPC.defDamage * 1.5f), 0);
                        }
                    }

                }
                if (Time == BehaviorEnd)
                {

                    if (RiserForSpin != null)
                        if (RiserForSpin.LoopIsBeingPlayed)
                            RiserForSpin.Stop();
                }

            }
            if (Time >= BehaviorEnd)
            {
                targetInterpolant = 0.2f;
                NPC.dontTakeDamage = false;
                Time = 0;

                previousState = currentState;
                currentState = Behavior.Idle;
                return;
            }
        }

        float BlastDir;
        void SpawnCommets()
        {

            const int PreCommetTime = 70;
            const int thing = 96;
            const int endTime = 140;
            if (Time == 1)
            {
                FlyAwayOffset = NPC.Center - currentTarget.Center;

                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.RocksRedirect with { PitchVariance = 0.2f, Pitch = -1.2f }, NPC.Center).WithVolumeBoost(4);
            }
            if (Time < PreCommetTime)
            {
                TargetPosition = currentTarget.Center + FlyAwayOffset;

                HeadPos = NPC.Center + new Vector2(100, 0).RotatedBy(NPC.AngleTo(currentTarget.Center)) * InverseLerp(0, 40, Time);
                DashDirection = NPC.DirectionTo(HeadPos);
                if (currentTarget.Distance(NPC.Center) < 500)
                    NPC.velocity -= NPC.Center.DirectionTo(currentTarget.Center);
                else
                    NPC.velocity += NPC.Center.DirectionTo(currentTarget.Center) * 40 * InverseLerp(40, 0, currentTarget.Distance(NPC.Center));
            }
            if (Time == PreCommetTime)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ErasureRiftClose);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 SpawnPos = HeadPos;
                    if (i > 2)
                    {
                        SpawnPos = HeadPos + new Vector2((i % 2 == 0 ? 1 : -1) * 40, 0).RotatedBy(NPC.Center.AngleTo(HeadPos));
                    }
                    Projectile cometA = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), SpawnPos, Vector2.Zero, ModContent.ProjectileType<IntersectingComet>(), NPC.defDamage / 3, 0);//AdvancedProjectileOwnershipSystem.NewOwnedProjectile<IntersectingComet>(NPC.GetSource_FromThis(), spawnPos, toTarget, ModContent.ProjectileType<IntersectingComet>(), 40, 0, NPC.whoAmI).ModProjectile as IntersectingComet;
                    Projectile cometB = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), SpawnPos, Vector2.Zero, ModContent.ProjectileType<IntersectingComet>(), NPC.defDamage / 3, 0);
                    cometA.ai[0] = i * 2;
                    cometB.ai[0] = i * 2;
                    cometA.ai[2] = i;
                    cometB.ai[2] = i;
                    cometA.rotation = NPC.Center.DirectionTo(HeadPos).ToRotation();
                    cometB.rotation = NPC.Center.DirectionTo(HeadPos).ToRotation();
                    cometA.As<IntersectingComet>().Offset *= (i + 1 / 3f);
                    //Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), HeadPos, Vector2.Zero, ModContent.ProjectileType<IntersectingComet>(), 10, 0);

                    cometB.As<IntersectingComet>().Offset = -cometA.As<IntersectingComet>().Offset;
                    cometA.As<IntersectingComet>().SisterComet = cometB.As<IntersectingComet>().Projectile;
                    cometB.As<IntersectingComet>().SisterComet = cometA.As<IntersectingComet>().Projectile;
                    cometA.As<IntersectingComet>().Owner = this;
                    cometB.As<IntersectingComet>().Owner = this;
                }

            }
            if (Time == 96)
            {

            }
            if (Time > endTime)
            {
                BlastDir = 0;
                Time = 0;
                NPC.damage = 0;
                NPC.velocity = Vector2.Zero;
                previousState = currentState;
                currentState = Behavior.Idle;
            }
        }


        #region EjectCore And Stalk
        int MaxDashes
        {
            get => Main.masterMode ? 5 : Main.expertMode ? 4 : 3;
        }
        int DashesUsed;
        int DashTimer;

        bool InDash;
        Vector2 DashDirection;
        Vector2 DashStartPos;
        bool WaitingToDash;
        int PostDashFadeTimer;

        int TotalAttackTime;
        int TimePerDash;
        int PostDashFadeTime = 20;
        void EjectCoreAndStalk()
        {
            const int CoreDeployTime = 20;

            const int DashWindupTime = 50;
            const int DashAccelTime = 12;
            const int DashSustainTime = 12;
            const int DashRecoverTime = 20;

            const int PreDashDelay = 45;      // after core eject, before first dash


            const float DashSpeed = 52f;


            Player target = currentTarget as Player;
            if (target == null || !target.active)
                return;

            if (Time == 1)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Angry, NPC.Center);

                DashesUsed = 0;
                DashTimer = 0;
                InDash = false;
                WaitingToDash = true;
                PostDashFadeTimer = 0;

                TimePerDash =
                    DashWindupTime +
                    DashAccelTime +
                    DashSustainTime +
                    DashRecoverTime +
                    PostDashFadeTime;

                TotalAttackTime = CoreDeployTime + PreDashDelay + TimePerDash * MaxDashes + 60;
            }


            // Deploy Core
            if (Time == CoreDeployTime && !CoreDeployed)
            {
                NPC core = NPC.NewNPCDirect(
                    NPC.GetSource_FromThis(),
                    HeadPos,
                    ModContent.NPCType<OtherworldlyCore>());

                if (core.active)
                {
                    core.As<OtherworldlyCore>().Body = this;
                    CoreDeployed = true;
                }
            }


            // exit condition
            if (Time >= TotalAttackTime || DashesUsed >= MaxDashes && !InDash)
            {
                NPC.velocity *= 0.8f;
                NPC.Opacity = float.Lerp(NPC.Opacity, 1f, 0.15f);
                NPC.damage = NPC.defDamage;

                if (NPC.velocity.LengthSquared() < 1f)
                {
                    EndAttack();
                }
                return;
            }
            if (PostDashFadeTimer > 0)
            {
                PostDashFadeTimer--;

                NPC.damage = 0;
                NPC.velocity *= 0.7f;

                NPC.Opacity = InverseLerp(PostDashFadeTime, 0, PostDashFadeTimer);
                if (NPC.Opacity < 0.02f)
                    NPC.Opacity = 0f;


                if (PostDashFadeTimer == PostDashFadeTime - 1)
                {
                    NPC.velocity = Vector2.Zero;
                    ResetTail();
                }

                return;
            }

            if (WaitingToDash)
            {
                NPC.damage = 0;
                NPC.velocity *= 0.85f;

                NPC.Opacity = float.Lerp(NPC.Opacity, 0f, 0.15f);
                if (NPC.Opacity < 0.02f)
                    NPC.Opacity = 0f;

                if (Time >= CoreDeployTime + PreDashDelay)
                    WaitingToDash = false;

                return;
            }
            if (!InDash && DashesUsed < MaxDashes)
            {
                BeginDash(target);
                return;
            }


            DashTimer++;

            NPC.Opacity = InverseLerp(0, DashWindupTime - 5, DashTimer);

            // Wind-up: track target
            if (DashTimer <= DashWindupTime - 5)
            {
                HeadPos = NPC.Center + currentTarget.DirectionTo(NPC.Center) * -100;
                NPC.Center += currentTarget.velocity * 0.7f * (1 - InverseLerp(0, DashWindupTime - 5, DashTimer));
                if (DashTimer < DashWindupTime - 10)
                DashDirection = NPC.DirectionTo(target.Center + target.velocity * 5f);
                NPC.velocity *= 0.85f;
                NPC.damage = 0;
                return;
            }

            // Acceleration
            if (DashTimer <= DashWindupTime + DashAccelTime)
            {
                HeadPos = NPC.Center + DashDirection * 120;
                NPC.velocity = Vector2.Lerp(
                    NPC.velocity,
                    DashDirection * DashSpeed,
                    0.45f);

                NPC.damage = NPC.defDamage;
                return;
            }

            // Sustain
            if (DashTimer <= DashWindupTime + DashAccelTime + DashSustainTime)
            {
                HeadPos = NPC.Center + DashDirection * 160;
                NPC.velocity = DashDirection * DashSpeed;
                return;
            }

            // Recovery
            if (DashTimer <= TimePerDash)
            {
                HeadPos = NPC.Center + DashDirection * 120;
                NPC.velocity *= 0.88f;
                NPC.damage = 0;
                return;
            }

            // End dash cleanly
            EndDash();
        }

        void BeginDash(Player target)
        {
            InDash = true;
            DashTimer = 0;

            int side = Math.Sign(target.Center.X - NPC.Center.X);
            if (side == 0)
                side = Main.rand.NextBool() ? 1 : -1;

            Vector2 flankPos =
                target.Center +
                new Vector2(side * 480f, -80f).RotatedByRandom(MathHelper.TwoPi);

            NPC.Center = flankPos;
            NPC.velocity = Vector2.Zero;

            DashDirection = NPC.DirectionTo(target.Center);
            DashesUsed++;

            ResetTail();

            SoundEngine.PlaySound(GennedAssets.Sounds.Common.Twinkle, NPC.Center);
        }

        void EndDash()
        {

            InDash = false;
            DashTimer = 0;
            PostDashFadeTimer = PostDashFadeTime;
        }
        void EndAttack()
        {
            NPC.velocity = Vector2.Zero;
            NPC.Opacity = 1f;
            NPC.damage = NPC.defDamage;
            NPC.dontTakeDamage = false;

            previousState = currentState;
            currentState = Behavior.Idle;
            TargetPosition = NPC.Center;

            DashesUsed = 0;
            DashTimer = 0;
            Time = 0;
        }

        #endregion

        bool Returning;
        Vector2 FlyAwayOffset;
        void FlyAwayAmbush()
        {

            const int screamTime = 10;
            const int FlyAwayTime = 30;
            const int MinimumReturnTime = 70 + FlyAwayTime;
            const int MaximumReturnTime = 40 + MinimumReturnTime;

            const int EndTime = 200;
            const float MaximumDistance = 1300;

            int GoopCount = !HasSecondPhaseTriggered ? 26 : 36;
            if (Time == 1)
            {
                Returning = false;
                targetInterpolant = 0;

                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Chirp with { Pitch = -1, PitchRange = (-3, 0), PitchVariance = 0.1f }).WithVolumeBoost(2);
            }

            if (Time < screamTime && Time % 5 == 0)
            {
                //im lazy
                HeadPos += Main.rand.NextVector2Unit(4);

            }
            //fly away
            if (Time > screamTime && !Returning)
            {
                HeadPos = NPC.Center + NPC.AngleTo(NPC.Center + NPC.velocity).ToRotationVector2() * 40;
                NPC.velocity = NPC.AngleFrom(currentTarget.Center).ToRotationVector2() * 40 * InverseLerp(screamTime, 90, Time);
                NPC.Opacity = InverseLerp(MaximumDistance, 100, NPC.Distance(currentTarget.Center));
            }
            if (NPC.Distance(currentTarget.Center) > MaximumDistance && Time > FlyAwayTime && !Returning)
            {
                FlyAwayOffset = NPC.Center - currentTarget.Center;
                NPC.Opacity = 0;
                NPC.velocity *= 0;
                Returning = true;
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Clap with { PitchRange = (-2, 0), PitchVariance = 0.32f });
            }
            if (Time < MinimumReturnTime && Returning)
            {
                NPC.Opacity = float.Lerp(NPC.Opacity, 1, 0.2f);
                NPC.Center = currentTarget.Center + FlyAwayOffset.RotatedBy(MathHelper.ToRadians(180));
                ResetTail();
            }
            if (Returning && Time >= MinimumReturnTime)
            {

                if (Time == MinimumReturnTime)
                {
                    NPC.damage = NPC.defDamage;
                    SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Inhale with { Pitch = -1 }).WithVolumeBoost(3);
                    DashDirection = NPC.Center.AngleTo(currentTarget.Center).ToRotationVector2();

                    SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ArmJutOut with { Pitch = -1.45f, PitchVariance = 0.3f });
                    for (int i = 0; i < GoopCount; i++)
                    {
                        Vector2 adjustedV = NPC.AngleTo(currentTarget.Center).ToRotationVector2().RotatedBy(MathHelper.Pi * i / (float)GoopCount - MathHelper.PiOver2) * 40 * (i % 2 == 0 ? 0.7f : 1);
                        float adjustedDamage = NPC.defDamage * 1.19f;
                        int actualDamage = (int)adjustedDamage;
                        NPC.NewProjectileBetter(NPC.GetSource_FromThis(), NPC.Center, adjustedV, ModContent.ProjectileType<NowhereGoop>(), actualDamage, 0);

                    }
                }

                if (Time > MinimumReturnTime)
                {
                    NPC.velocity = DashDirection * 90 * Math.Abs(InverseLerp(MinimumReturnTime, MaximumReturnTime, Time));
                    HeadPos = NPC.Center + DashDirection * 3;
                }


                if (Time > EndTime)
                {

                    NPC.velocity *= 0;
                    targetInterpolant = 0.2f;
                    Time = 0;
                    NPC.Opacity = 1;
                    previousState = currentState;
                    currentState = Behavior.Idle;
                    return;
                }
            }
        }

        bool Staggered = false;
        float StaggerTimer = 0;
        void Medusa()
        {

            Player target = currentTarget as Player;
            MedusaPlayer mP;
            target.TryGetModPlayer<MedusaPlayer>(out mP);
            const float ViewAngle = 50;

            const int HeadBendBackStart = 40;
            const int HeadBendBackFinish = 100;
            const int NeckRipOpenStart = 110;
            const int NeckRipOpenEnd = 140;

            const int CircleEnd = 400;
            const int AttackEnd = 500;
            if (Time == 1)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.FogRelease with { PitchVariance = 0.5f });
            }
            //bend head behind body
            HeadPos = NPC.Center + new Vector2(90 * NPC.direction, 0).RotatedBy(MathHelper.ToRadians(110 * -NPC.direction) * InverseLerp(HeadBendBackStart, HeadBendBackFinish, Time));

            //at that time, we will split the Neck in two, and begin drawing that weird thing in the center of the neck and head.
            if (Time == HeadBendBackFinish)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ArmJutOut with { Volume = 0.3f, Pitch = -2f, PitchRange = (-2, 0) });
            }
            //start attacking the player.
            if (Time > NeckRipOpenEnd && Time < CircleEnd)
            {
                float thing = (1 - InverseLerp(NeckRipOpenEnd, CircleEnd, Time));
                TargetPosition = currentTarget.Center + new Vector2(400 + 300 * thing, 0).RotatedBy(MathHelper.TwoPi + MathHelper.ToRadians(Time));

                if (NPC.Hitbox.IntersectsConeSlowMoreAccurate(target.Center, 1000, Main.MouseWorld.AngleFrom(target.Center), MathHelper.ToRadians(ViewAngle)))
                {
                    if (mP.MedusaTimer % 2 == 0 && mP.MedusaTimer > mP.SafeThreshold)
                    {
                        SoundEngine.PlaySound(GennedAssets.Sounds.Common.Twinkle with { Pitch = InverseLerp(0, 300, mP.MedusaTimer), MaxInstances = 0 });
                    }
                    Dust a = Dust.NewDustDirect(target.Center, 30, 30, DustID.Cloud);
                    a.velocity = target.Center.DirectionTo((NPC.Center + HeadPos) / 2).RotatedByRandom(MathHelper.ToRadians(30)) * 10;

                    mP.MedusaTimer++;
                    mP.PurgeTimer = -1;
                }
            }
            //punishment
            if (Time >= CircleEnd && mP.MedusaStacks >= 5)
            {
                if (Time == CircleEnd)
                {
                    SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.AngryDistant with { PitchVariance = 0.2f }).WithVolumeBoost(3);
                }
                // stop tracking around the player.
                TargetPosition = NPC.Center;


                if (Time == CircleEnd + 10)
                {
                    NPC.damage = NPC.defDamage;
                    SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.JumpscareWeak);
                    DashDirection = target.AngleFrom(NPC.Center).ToRotationVector2();
                }
                NPC.velocity = DashDirection * 80 * InverseLerp(CircleEnd, CircleEnd + 40, Time);//
                //Main.NewText(DashDirection);
            }
            //boon: 
            else if (Time >= CircleEnd && mP.MedusaStacks < 2 && !Staggered)
            {
                if (Time == CircleEnd)
                {
                    SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.FeatherAppear with { Pitch = -0.4f }).WithVolumeBoost(4);
                }
                if (NPC.justHit)
                    StaggerTimer = 120;


            }

            if (Time >= AttackEnd && !Staggered)
            {
                NPC.velocity = Vector2.Zero;
                Time = 0;
                previousState = currentState;
                currentState = Behavior.Idle;
            }
        }

        private void flyDropBombs()
        {
            const int startTime = 50;
            const int beGoneBy = 120;
            const int attackDone = 360;

            Player target = currentTarget as Player;

            if (Time == 1)
            {
                targetInterpolant = 0;
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Angry with { Pitch = -1f, PitchVariance = 0.23f }).WithVolumeBoost(3);
            }
            if (Time < startTime)
            {
                HeadPos = NPC.Center + new Vector2(100 * Math.Sign((NPC.Center - currentTarget.Center).X), 0);
            }

            if (Time == startTime - 10)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Common.Twinkle);
                Vector2 escapeDir = (NPC.Center - HeadPos).SafeNormalize(Vector2.UnitX);
                FlyAwayOffset = escapeDir * 1600f;
            }


            if (Time >= startTime && Time < beGoneBy)
            {
                Vector2 desiredVelocity = NPC.DirectionTo(FlyAwayOffset + NPC.Center) * 125 * InverseLerp(startTime, beGoneBy, Time);
                desiredVelocity.Y *= 0.3f;
                NPC.velocity = desiredVelocity.RotatedBy(MathHelper.ToRadians(MathF.Sin(Time / 10.1f) * 15 * InverseLerp(startTime, beGoneBy - 50, Time)));

                if (Time % 12 == 0)
                {
                    Vector2 dropVelocity = NPC.DirectionTo(currentTarget.Center).RotatedByRandom(MathHelper.PiOver4) * 16;

                    Projectile.NewProjectile(
                        NPC.GetSource_FromThis(),
                        NPC.Center + (Main.rand.NextBool() ? wingPos[0] : wingPos[1]),
                        dropVelocity,
                        ModContent.ProjectileType<ThornBomb_Seed>(),
                        NPC.defDamage,
                        0f

                    );
                }
            }
            if (Time >= beGoneBy)
            {
                NPC.dontTakeDamage = true;
                NPC.Opacity = 0;
                ResetTail();
                NPC.Center = currentTarget.Center + FlyAwayOffset;
                NPC.velocity *= 0.98f;
                NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 0f, 0.08f);
            }

            if (Time > attackDone)
            {
                NPC.dontTakeDamage = false;
                targetInterpolant = 0.2f;
                NPC.Opacity = 1;
                Time = 0;
                currentState = Behavior.Idle;
            }
        }

        public float ReelSolynInterpolant;
        public Vector2 StoredSolynPos;
        public int letGOcount;
        private void placeholder2()
        {
            HeadPos = NPC.Center + NPC.AngleTo(BattleSolyn.GetOriginalSolyn().NPC.Center).ToRotationVector2() * 90;
            Projectile a;
            if (Time <= 4)
            {
                ReelSolynInterpolant = 0;
                letGOcount = 0;
                StoredSolynPos = Vector2.Zero;
            }
            if (Time == 40)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.AbsoluteZeroWave with { PitchVariance = 0.5f, PitchRange = (-2, 0) });
                a = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), HeadPos, Vector2.Zero, ModContent.ProjectileType<SeekingEnergy>(), 0, 10);
                a.As<SeekingEnergy>().Owner = this.NPC;
                a.As<SeekingEnergy>().Impaled = BattleSolyn.GetOriginalSolyn().NPC;

            }
            int thing = (int)((1 - ReelSolynInterpolant) * 20) + 1;
            if (Time % thing == 0)
            {
                if (Main.rand.NextBool(4))
                {

                    Projectile b = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), HeadPos, new Vector2(0, 10).RotateRandom(MathHelper.ToRadians(10)), ModContent.ProjectileType<NowhereGoop>(), NPC.defDamage, 0);
                    b.ai[0] = 60;
                }
            }

            if (ReelSolynInterpolant == 1)
            {
                if (Time == 401)
                {
                    SoundEngine.PlaySound(GennedAssets.Sounds.Genesis.AntiseedPlant with { Volume = 4, MaxInstances = 1, Pitch = -1.3f }).WithVolumeBoost(5);

                }
                HeadPos += Main.rand.NextVector2Unit();
                NPC.Center += Main.rand.NextVector2Unit() * 10;
                if (Time % 3 == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Projectile c = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Main.rand.NextVector2Unit() * 10, ModContent.ProjectileType<NowhereGoop>(), NPC.defDamage, 0);

                    }

                }
            }
            ReelSolynInterpolant = InverseLerp(40, 400, Time);

            if (Time > 500)
            {
                StoredSolynPos = Vector2.Zero;
                Time = -1;
                currentState = Behavior.Idle;
            }
        }

        private void placeholder3()
        {
            Time = -1;
             currentState = Behavior.RiseSpin;
        }



    }
}
