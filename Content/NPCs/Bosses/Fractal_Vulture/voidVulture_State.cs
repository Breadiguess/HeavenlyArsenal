using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles;
using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Solyn;
using Luminance.Core.Graphics;
using Luminance.Core.Sounds;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.AdvancedProjectileOwnership;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using NoxusBoss.Core.SoundSystems.Music;
using NoxusBoss.Core.Utilities;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.Localization;
using static Luminance.Common.Utilities.Utilities;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture;

partial class voidVulture
{
    public enum Behavior
    {
        debug,

        reveal,

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

    private static readonly Behavior[] AttackCycleOrder = new[]
    {
        Behavior.FlyAwayAndAmbush,
        Behavior.VomitCone,
        Behavior.RiseSpin,
        Behavior.CollidingCommet,
        Behavior.EjectCoreAndStalk
    };

    private static readonly Behavior[] AttackCycleOrderPhase2 = new[]
    {
        Behavior.VomitCone,
        Behavior.EjectCoreAndStalk,
        Behavior.FlyEjectBomb,
        Behavior.FlyAwayAndAmbush,
        Behavior.placeholder2,
        Behavior.placeholder3
    };

    public Behavior previousState;

    public float ReelSolynInterpolant;

    public Vector2 StoredSolynPos;

    public int letGOcount;

    private int attackIndex;

    private float BlastDir;

    private int DashesUsed;

    private bool ShouldDash;

    private int DashTimer;

    private Vector2 DashDirection;

    private bool Returning;

    private Vector2 FlyAwayOffset;

    private bool Staggered;

    private float StaggerTimer;

    /// <summary>
    ///     A looping sound for the VomitCone attack.
    /// </summary>
    public LoopedSoundInstance VomitLoop { get; set; }

    public LoopedSoundInstance RiserForSpin { get; set; }

    public Behavior currentState
    {
        get => (Behavior)NPC.ai[2];
        set => NPC.ai[2] = (float)value;
    }

    private int MaxDashes => Main.masterMode ? 5 : Main.expertMode ? 4 : 3;

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

    private void ManageTransitionCutscene()
    {
        if (StoredVomit != null)
        {
            VomitLoop.Stop();
        }

        if (RiserForSpin != null)
        {
            RiserForSpin.Stop();
        }

        if (Time == 1)
        {
            BattleSolynBird.SummonSolynForBattle(NPC.GetSource_FromThis(), currentTarget.Center, BattleSolynBird.SolynAIType.FightBird);
        }

        if (Time == 0)
        {
            attackIndex = 0;
            NPC.canDisplayBuffs = false;
            NPC.immortal = true;
            NPC.Opacity = 1;
            NPC.dontTakeDamage = true;

            if (CoreDeployed)
            {
                CoreDeployed = false;
            }

            hideBar = true;
        }

        const int StartTime = 180;
        const int EndTime = 680;
        NPC.velocity *= 0.7f;
        var cameraZoomInterpolant = InverseLerp(0f, 11f, Time);
        CameraPanSystem.PanTowards(NPC.Center, cameraZoomInterpolant);
        MusicVolumeManipulationSystem.MuffleFactor = 0.1f;

        if (Time == StartTime)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 1.5f);

            SoundEngine.PlaySound
                (
                    GennedAssets.Sounds.Avatar.Shriek with
                    {
                        PitchVariance = 0.2f,
                        Pitch = -1.2f
                    },
                    NPC.Center
                )
                .WithVolumeBoost(3);

            hideBar = false;
        }

        if (Time % 7 == 0 && Time > StartTime && Time < EndTime)
        {
            ScreenShakeSystem.StartShakeAtPoint(HeadPos, 0.2f * 6f);

            var burst = new ExpandingChromaticBurstParticle(HeadPos, Vector2.Zero, Main.rand.NextBool() ? Color.White : Color.White, 30, 0.2f, 1.7f);
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
        var next = !HasSecondPhaseTriggered ? AttackCycleOrder[attackIndex] : AttackCycleOrderPhase2[attackIndex];

        attackIndex++;

        if (attackIndex >= AttackCycleOrder.Length)
        {
            attackIndex = 0;
        }

        return next;
    }

    private void Debug()
    {
        TargetPosition = NPC.Center;
        currentState = Behavior.reveal;
    }

    private void Idle()
    {
        var hoverHeight = -260f;
        var horizontalOffsetMax = 180f;

        var drift = (float)Math.Sin(Time / 35f) * horizontalOffsetMax;

        hoverHeight = -130f * Math.Abs(MathF.Sin(Time / 70f)) + -230;
        var idealPos = currentTarget.Center + new Vector2(drift, hoverHeight);
        TargetPosition = idealPos;

        if (Time > (HasSecondPhaseTriggered ? 140 : 240))
        {
            Time = 0;

            currentState = GetNextAttack();
        }
    }

    private void Reveal()
    {
        var cameraZoomInterpolant = InverseLerp(0f, 11f, Time);

        CameraPanSystem.PanTowards(NPC.Center, cameraZoomInterpolant);
        MusicVolumeManipulationSystem.MuffleFactor = 0.1f;
        hideBar = true;
        NPC.velocity *= 0;

        if (Time > 12)
        {
            NPC.Opacity = Utils.SmoothStep(0, 1, Time); //float.Lerp(NPC.Opacity, 1, 0.2f);
        }

        if (Time == 30)
        {
            SoundEngine.PlaySound
            (
                GennedAssets.Sounds.Common.Glitch with
                {
                    Pitch = 0.2f,
                    MaxInstances = 0
                }
            );
        }

        if (Time == 100)
        {
            SoundEngine.PlaySound
            (
                GennedAssets.Sounds.Avatar.Phase2IntroFaceManifest with
                {
                    Volume = 3f
                }
            );
        }

        if (Time > 100 && Time < 380)
        {
            ScreenShakeSystem.StartShakeAtPoint(HeadPos, 0.2f * 6f);

            if (Time > 100)
            {
                NPC.Center = Vector2.Lerp(NPC.Center, NPC.Center - new Vector2(0, 80), 0.2f);
            }
        }

        if (Time > 380)
        {
            var typeName = NPC.FullName;

            if (Main.netMode == 0)
            {
                Main.NewText(Language.GetTextValue("Announcement.HasAwoken", typeName), 175, 75);
            }
            else if (Main.netMode == 2)
            {
                ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Announcement.HasAwoken"), new Color(175, 75, 255));
            }

            //TileDisablingSystem.TilesAreUninteractable = true;
            //Main.NewText($"{NPC.GivenName} has awoken! ", Color.Purple);
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Phase2IntroNeckSnap);
            currentState = Behavior.Idle;
            NPC.dontTakeDamage = false;
            hideBar = false;
            Time = 0;
        }
    }

    private void RiseSpin()
    {
        const int StartTime = 40;
        const int AudioTime = 165;
        const int RiseTime = 230;
        const int SprayProjectileTime = 255;
        const int BehaviorEnd = 360;

        var projectileCount = !HasSecondPhaseTriggered ? 12 : 30;

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
            {
                RiserForSpin.Stop();
            }

            RiserForSpin = LoopedSoundManager.CreateNew(GennedAssets.Sounds.Avatar.LilyFiringLoop, () => !NPC.active);
        }

        if (RiserForSpin != null)

        {
            RiserForSpin.Update
            (
                NPC.Center,
                sound =>
                {
                    var thing = InverseLerpBump(AudioTime, AudioTime + 70, SprayProjectileTime, BehaviorEnd, Time);
                    //Main.NewText(thing);
                    //float thing = InverseLerpBump(ShootStart, ShootStart + 20, ShootStop - 20, ShootEnd, Time);
                    sound.Volume = 2.5f * thing; //InverseLerp(165, RiseTime, Time);
                    sound.Pitch = 1.2f * InverseLerp(165, RiseTime, Time);
                }
            );
        }

        if (Time > RiseTime - 30)

        {
            NPC.Opacity = InverseLerp(RiseTime - 30, RiseTime, Time);
        }

        if (Time > RiseTime - 20)
        {
            NPC.damage = NPC.defDamage;

            if (Time < RiseTime)
            {
                ScreenShakeSystem.StartShakeAtPoint(HeadPos, 0.2f * 6f);
            }

            if (Time == RiseTime)
            {
                ResetTail();
                targetInterpolant = 0.08f;

                //SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Chirp with { Volume = 2f, Pitch = 1.2f }).WithVolumeBoost(2);
                SoundEngine.PlaySound
                    (
                        GennedAssets.Sounds.Avatar.BloodFountainErupt with
                        {
                            Pitch = 1.4f
                        }
                    )
                    .WithVolumeBoost(2);

                TargetPosition = new Vector2(NPC.Center.X, currentTarget.Center.Y) - new Vector2(0, 600);
            }

            HeadPos = NPC.Center + TargetPosition.AngleFrom(NPC.Center).ToRotationVector2() * 60;

            if (Time == SprayProjectileTime)
            {
                float thing = !HasSecondPhaseTriggered ? 7 : 12;

                SoundEngine.PlaySound
                    (
                        GennedAssets.Sounds.Genesis.AntiseedPlant with
                        {
                            Volume = 4,
                            MaxInstances = 1,
                            Pitch = -1.3f
                        }
                    )
                    .WithVolumeBoost(5);

                for (var x = 0; x < 2; x++)
                {
                    for (var i = 1; i < projectileCount; i++)
                    {
                        var adjustedV = new Vector2(40, 0).RotatedBy
                                            (i / thing * MathHelper.PiOver2 * (x % 2 == 0 ? -1 : 1) + MathHelper.PiOver2) *
                                        (i % 2 == 0 ? 0.5f : 0.7f) *
                                        (!HasSecondPhaseTriggered ? 1 : 1.4f);

                        NPC.NewProjectileBetter(NPC.GetSource_FromThis(), NPC.Center, adjustedV, ModContent.ProjectileType<NowhereGoop>(), (int)(NPC.defDamage * 1.5f), 0);
                    }
                }
            }

            if (Time == BehaviorEnd)
            {
                if (RiserForSpin != null)
                {
                    if (RiserForSpin.LoopIsBeingPlayed)
                    {
                        RiserForSpin.Stop();
                    }
                }
            }
        }

        if (Time >= BehaviorEnd)
        {
            targetInterpolant = 0.2f;
            NPC.dontTakeDamage = false;
            Time = 0;

            previousState = currentState;
            currentState = Behavior.Idle;
        }
    }

    private void SpawnCommets()
    {
        const int PreCommetTime = 70;
        const int thing = 96;
        const int endTime = 140;

        if (Time == 1)
        {
            FlyAwayOffset = NPC.Center - currentTarget.Center;

            SoundEngine.PlaySound
                (
                    GennedAssets.Sounds.Avatar.RocksRedirect with
                    {
                        PitchVariance = 0.2f,
                        Pitch = -1.2f
                    },
                    NPC.Center
                )
                .WithVolumeBoost(4);
        }

        if (Time < PreCommetTime)
        {
            TargetPosition = currentTarget.Center + FlyAwayOffset;

            HeadPos = NPC.Center + new Vector2(100, 0).RotatedBy(NPC.AngleTo(currentTarget.Center)) * InverseLerp(0, 40, Time);
            DashDirection = NPC.DirectionTo(HeadPos);

            if (currentTarget.Distance(NPC.Center) < 500)
            {
                NPC.velocity -= NPC.Center.DirectionTo(currentTarget.Center);
            }
            else
            {
                NPC.velocity += NPC.Center.DirectionTo(currentTarget.Center) * 40 * InverseLerp(40, 0, currentTarget.Distance(NPC.Center));
            }
        }

        if (Time == PreCommetTime)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ErasureRiftClose);

            for (var i = 0; i < 4; i++)
            {
                var SpawnPos = HeadPos;

                if (i > 2)
                {
                    SpawnPos = HeadPos + new Vector2((i % 2 == 0 ? 1 : -1) * 40, 0).RotatedBy(NPC.Center.AngleTo(HeadPos));
                }

                var cometA = Projectile.NewProjectileDirect
                (
                    NPC.GetSource_FromThis(),
                    SpawnPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<IntersectingComet>(),
                    NPC.defDamage / 3,
                    0
                ); //AdvancedProjectileOwnershipSystem.NewOwnedProjectile<IntersectingComet>(NPC.GetSource_FromThis(), spawnPos, toTarget, ModContent.ProjectileType<IntersectingComet>(), 40, 0, NPC.whoAmI).ModProjectile as IntersectingComet;

                var cometB = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), SpawnPos, Vector2.Zero, ModContent.ProjectileType<IntersectingComet>(), NPC.defDamage / 3, 0);
                cometA.ai[0] = i * 2;
                cometB.ai[0] = i * 2;
                cometA.ai[2] = i;
                cometB.ai[2] = i;
                cometA.rotation = NPC.Center.DirectionTo(HeadPos).ToRotation();
                cometB.rotation = NPC.Center.DirectionTo(HeadPos).ToRotation();
                cometA.As<IntersectingComet>().Offset *= i + 1 / 3f;
                //Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), HeadPos, Vector2.Zero, ModContent.ProjectileType<IntersectingComet>(), 10, 0);

                cometB.As<IntersectingComet>().Offset = -cometA.As<IntersectingComet>().Offset;
                cometA.As<IntersectingComet>().SisterComet = cometB.As<IntersectingComet>().Projectile;
                cometB.As<IntersectingComet>().SisterComet = cometA.As<IntersectingComet>().Projectile;
                cometA.As<IntersectingComet>().Owner = this;
                cometB.As<IntersectingComet>().Owner = this;
            }
        }

        if (Time == 96) { }

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

    /// <summary>
    ///     TODO: OVERHAUL ME!!
    /// </summary>
    private void EjectCoreAndStalk()
    {
        const int DashWindup = 40;
        const int DashAccelTime = 60;
        const int DashStopTime = 80;

        var target = currentTarget as Player;

        if (Time == 1)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.Angry, NPC.Center);
        }

        if (Time == 40)
        {
            var core = NPC.NewNPCDirect(NPC.GetSource_FromThis(), HeadPos, ModContent.NPCType<OtherworldlyCore>());
            core.As<OtherworldlyCore>().Body = this;
            CoreDeployed = true;
        }

        if (Time > 40)
        {
            NPC.Opacity = float.Lerp(NPC.Opacity, 0f, 0.2f);

            if (NPC.Opacity < 0.2f)
            {
                NPC.Opacity = 0;
            }

            // Fade out -> low damage; fade in -> restore damage
            NPC.damage = NPC.Opacity < 0.4f ? 0 : NPC.defDamage;

            //reposition and dash
            var Dashtime = !HasSecondPhaseTriggered ? 60 : 120;

            if (!ShouldDash && Time % Dashtime == 0 && DashesUsed < MaxDashes)
            {
                if (DashesUsed >= MaxDashes)
                {
                    Time = 540;

                    return;
                }

                var side = Math.Sign((target.Center - NPC.Center).X);

                if (side == 0)
                {
                    side = Main.rand.NextBool() ? 1 : -1;
                }

                //TODO: calculate a better flank pos for the npc so that it doesn't tp too close to a player, so that the player doesn't run into them while they're still mostly invisible.
                var flankPos = target.Center + target.velocity + new Vector2(side * 500f, 0f).RotatedByRandom(MathHelper.Pi);
                NPC.Center = flankPos;
                ResetTail();
                // Prepare dash direction & engage dash mode
                DashDirection = NPC.DirectionTo(target.Center + target.velocity);
                DashesUsed++;
                SoundEngine.PlaySound(GennedAssets.Sounds.Common.Twinkle);
                ShouldDash = true;
                DashTimer = 0;
            }

            if (ShouldDash)
            {
                HeadPos = NPC.Center + NPC.velocity.ToRotation().ToRotationVector2() * 90;
                NPC.Opacity = float.Lerp(NPC.Opacity, 1, 0.4f);
                DashTimer++;

                if (DashTimer < DashWindup - 4)
                {
                    DashDirection = NPC.DirectionTo(target.Center);
                }

                if (DashTimer == DashWindup)
                {
                    SoundEngine.PlaySound
                        (
                            GennedAssets.Sounds.Avatar.DisgustingStarSever with
                            {
                                Pitch = 0.2f,
                                PitchVariance = 0.2f,
                                PitchRange = (-1.4f, 0f)
                            }
                        )
                        .WithVolumeBoost(1.3f);
                }

                // Acceleration phase
                if (DashTimer > DashWindup && DashTimer < DashAccelTime)
                {
                    NPC.velocity = Vector2.Lerp(NPC.velocity, DashDirection * 50f, 0.35f);
                }
                // Deceleration phase
                else if (DashTimer >= DashAccelTime && DashTimer < DashStopTime)
                {
                    NPC.velocity *= 0.9f;
                }
                else if (DashTimer > DashStopTime)
                {
                    // End dash
                    NPC.velocity *= 0.7f;
                    ShouldDash = false;
                }
            }

            if (Time > 600 && !CoreDeployed)
            {
                NPC.velocity *= 0;
                NPC.dontTakeDamage = false;
                previousState = currentState;
                TargetPosition = NPC.Center;
                currentState = Behavior.Idle;
                Time = 0;
                NPC.Opacity = 1;
                DashesUsed = 0;
            }

            if (DashesUsed >= MaxDashes)
            {
                NPC.velocity *= 0.4f;
                NPC.Opacity = float.Lerp(NPC.Opacity, 1, 0.2f);
            }
        }
    }

    private void FlyAwayAmbush()
    {
        const int screamTime = 10;
        const int FlyAwayTime = 30;
        const int MinimumReturnTime = 70 + FlyAwayTime;
        const int MaximumReturnTime = 40 + MinimumReturnTime;

        const int EndTime = 200;
        const float MaximumDistance = 1300;

        var GoopCount = !HasSecondPhaseTriggered ? 26 : 36;

        if (Time == 1)
        {
            Returning = false;
            targetInterpolant = 0;

            SoundEngine.PlaySound
                (
                    GennedAssets.Sounds.Avatar.Chirp with
                    {
                        Pitch = -1,
                        PitchRange = (-3, 0),
                        PitchVariance = 0.1f
                    }
                )
                .WithVolumeBoost(2);
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

            SoundEngine.PlaySound
            (
                GennedAssets.Sounds.Avatar.Clap with
                {
                    PitchRange = (-2, 0),
                    PitchVariance = 0.32f
                }
            );
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

                SoundEngine.PlaySound
                    (
                        GennedAssets.Sounds.Avatar.Inhale with
                        {
                            Pitch = -1
                        }
                    )
                    .WithVolumeBoost(3);

                DashDirection = NPC.Center.AngleTo(currentTarget.Center).ToRotationVector2();

                SoundEngine.PlaySound
                (
                    GennedAssets.Sounds.Avatar.ArmJutOut with
                    {
                        Pitch = -1.45f,
                        PitchVariance = 0.3f
                    }
                );

                for (var i = 0; i < GoopCount; i++)
                {
                    var adjustedV = NPC.AngleTo(currentTarget.Center).ToRotationVector2().RotatedBy(MathHelper.Pi * i / GoopCount - MathHelper.PiOver2) * 40 * (i % 2 == 0 ? 0.7f : 1);
                    var adjustedDamage = NPC.defDamage * 1.19f;
                    var actualDamage = (int)adjustedDamage;
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
            }
        }
    }

    private void Medusa()
    {
        var target = currentTarget as Player;
        MedusaPlayer mP;
        target.TryGetModPlayer(out mP);
        const float ViewAngle = 50;

        const int HeadBendBackStart = 40;
        const int HeadBendBackFinish = 100;
        const int NeckRipOpenStart = 110;
        const int NeckRipOpenEnd = 140;

        const int CircleEnd = 400;
        const int AttackEnd = 500;

        if (Time == 1)
        {
            SoundEngine.PlaySound
            (
                GennedAssets.Sounds.Avatar.FogRelease with
                {
                    PitchVariance = 0.5f
                }
            );
        }

        //bend head behind body
        HeadPos = NPC.Center + new Vector2(90 * NPC.direction, 0).RotatedBy(MathHelper.ToRadians(110 * -NPC.direction) * InverseLerp(HeadBendBackStart, HeadBendBackFinish, Time));

        //at that time, we will split the Neck in two, and begin drawing that weird thing in the center of the neck and head.
        if (Time == HeadBendBackFinish)
        {
            SoundEngine.PlaySound
            (
                GennedAssets.Sounds.Avatar.ArmJutOut with
                {
                    Volume = 0.3f,
                    Pitch = -2f,
                    PitchRange = (-2, 0)
                }
            );
        }

        //start attacking the player.
        if (Time > NeckRipOpenEnd && Time < CircleEnd)
        {
            var thing = 1 - InverseLerp(NeckRipOpenEnd, CircleEnd, Time);
            TargetPosition = currentTarget.Center + new Vector2(400 + 300 * thing, 0).RotatedBy(MathHelper.TwoPi + MathHelper.ToRadians(Time));

            if (NPC.Hitbox.IntersectsConeSlowMoreAccurate(target.Center, 1000, Main.MouseWorld.AngleFrom(target.Center), MathHelper.ToRadians(ViewAngle)))
            {
                if (mP.MedusaTimer % 2 == 0 && mP.MedusaTimer > mP.SafeThreshold)
                {
                    SoundEngine.PlaySound
                    (
                        GennedAssets.Sounds.Common.Twinkle with
                        {
                            Pitch = InverseLerp(0, 300, mP.MedusaTimer),
                            MaxInstances = 0
                        }
                    );
                }

                var a = Dust.NewDustDirect(target.Center, 30, 30, DustID.Cloud);
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
                SoundEngine.PlaySound
                    (
                        GennedAssets.Sounds.Avatar.AngryDistant with
                        {
                            PitchVariance = 0.2f
                        }
                    )
                    .WithVolumeBoost(3);
            }

            // stop tracking around the player.
            TargetPosition = NPC.Center;

            if (Time == CircleEnd + 10)
            {
                NPC.damage = NPC.defDamage;
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.JumpscareWeak);
                DashDirection = target.AngleFrom(NPC.Center).ToRotationVector2();
            }

            NPC.velocity = DashDirection * 80 * InverseLerp(CircleEnd, CircleEnd + 40, Time); //
            //Main.NewText(DashDirection);
        }
        //boon: 
        else if (Time >= CircleEnd && mP.MedusaStacks < 2 && !Staggered)
        {
            if (Time == CircleEnd)
            {
                SoundEngine.PlaySound
                    (
                        GennedAssets.Sounds.NamelessDeity.FeatherAppear with
                        {
                            Pitch = -0.4f
                        }
                    )
                    .WithVolumeBoost(4);
            }

            if (NPC.justHit)
            {
                StaggerTimer = 120;
            }
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

        var target = currentTarget as Player;

        if (Time == 1)
        {
            targetInterpolant = 0;

            SoundEngine.PlaySound
                (
                    GennedAssets.Sounds.Avatar.Angry with
                    {
                        Pitch = -1f,
                        PitchVariance = 0.23f
                    }
                )
                .WithVolumeBoost(3);
        }

        if (Time < startTime)
        {
            HeadPos = NPC.Center + new Vector2(100 * Math.Sign((NPC.Center - currentTarget.Center).X), 0);
        }

        if (Time == startTime - 10)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Common.Twinkle);
            var escapeDir = (NPC.Center - HeadPos).SafeNormalize(Vector2.UnitX);
            FlyAwayOffset = escapeDir * 1600f;
        }

        if (Time >= startTime && Time < beGoneBy)
        {
            var desiredVelocity = NPC.DirectionTo(FlyAwayOffset + NPC.Center) * 125 * InverseLerp(startTime, beGoneBy, Time);
            desiredVelocity.Y *= 0.3f;
            NPC.velocity = desiredVelocity.RotatedBy(MathHelper.ToRadians(MathF.Sin(Time / 10.1f) * 15 * InverseLerp(startTime, beGoneBy - 50, Time)));

            if (Time % 12 == 0)
            {
                var dropVelocity = NPC.DirectionTo(currentTarget.Center).RotatedByRandom(MathHelper.PiOver4) * 16;

                Projectile.NewProjectile
                (
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

    private void placeholder2()
    {
        HeadPos = NPC.Center + NPC.AngleTo(BattleSolynBird.GetOriginalSolyn().NPC.Center).ToRotationVector2() * 90;
        Projectile a;

        if (Time <= 4)
        {
            ReelSolynInterpolant = 0;
            letGOcount = 0;
            StoredSolynPos = Vector2.Zero;
        }

        if (Time == 40)
        {
            SoundEngine.PlaySound
            (
                GennedAssets.Sounds.Avatar.AbsoluteZeroWave with
                {
                    PitchVariance = 0.5f,
                    PitchRange = (-2, 0)
                }
            );

            a = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), HeadPos, Vector2.Zero, ModContent.ProjectileType<SeekingEnergy>(), 0, 10);
            a.As<SeekingEnergy>().Owner = NPC;
            a.As<SeekingEnergy>().Impaled = BattleSolynBird.GetOriginalSolyn().NPC;
        }

        var thing = (int)((1 - ReelSolynInterpolant) * 20) + 1;

        if (Time % thing == 0)
        {
            if (Main.rand.NextBool(4))
            {
                var b = Projectile.NewProjectileDirect
                    (NPC.GetSource_FromThis(), HeadPos, new Vector2(0, 10).RotateRandom(MathHelper.ToRadians(10)), ModContent.ProjectileType<NowhereGoop>(), NPC.defDamage, 0);

                b.ai[0] = 60;
            }
        }

        if (ReelSolynInterpolant == 1)
        {
            if (Time == 401)
            {
                SoundEngine.PlaySound
                    (
                        GennedAssets.Sounds.Genesis.AntiseedPlant with
                        {
                            Volume = 4,
                            MaxInstances = 1,
                            Pitch = -1.3f
                        }
                    )
                    .WithVolumeBoost(5);
            }

            HeadPos += Main.rand.NextVector2Unit();
            NPC.Center += Main.rand.NextVector2Unit() * 10;

            if (Time % 3 == 0)
            {
                for (var i = 0; i < 5; i++)
                {
                    var c = Projectile.NewProjectileDirect(NPC.GetSource_FromThis(), NPC.Center, Main.rand.NextVector2Unit() * 10, ModContent.ProjectileType<NowhereGoop>(), NPC.defDamage, 0);
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

    #region cone vomit

    private float neckSpinInterpolant;

    private float baseRotation;

    private Vector2 VomitDirection;

    private Projectile StoredVomit;

    public Vector2 SolynChosenShield;

    public static int VomitCone_ShootStart = 40;

    public static int VomitCone_ShootStop => !Myself.As<voidVulture>().HasSecondPhaseTriggered ? 240 : 260;

    public static int VomitCone_ShootEnd => !Myself.As<voidVulture>().HasSecondPhaseTriggered ? 270 : 300;

    private void VomitCone()
    {
        const int ShootStart = 40;
        var ShootStop = VomitCone_ShootStop;
        var ShootEnd = VomitCone_ShootEnd;

        var tNorm = InverseLerp(ShootStart, ShootStop, Time);
        var bell = Convert01To010(tNorm);

        var toTarget = NPC.DirectionTo(currentTarget.Center);
        var distToTarget = NPC.Distance(currentTarget.Center);
        var angleToTarget = NPC.Center.AngleTo(currentTarget.Center) + (HasSecondPhaseTriggered ? MathHelper.Pi : 0);

        if (Time == 1)
        {
            NPC.damage = 0;

            // Choose offset position without lerping through player
            var side = Math.Sign(NPC.Center.X - currentTarget.Center.X);

            if (side != 0)
            {
                var desiredOffset = new Vector2(250 * side, 0);
                var safe = currentTarget.Center + desiredOffset;

                // Prevent passing directly through player *crosses fingers*    
                if (Collision.CheckAABBvLineCollision
                    (
                        currentTarget.Hitbox.TopLeft(),
                        currentTarget.Hitbox.Size(),
                        NPC.Center,
                        safe
                    ))
                {
                    // Pick a vertical offset instead
                    safe = currentTarget.Center +
                           new Vector2(0, 240 * side)
                               .RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f));
                }

                //TargetPosition = safe.RotatedByRandom(MathHelper.TwoPi);
            }

            if (!HasSecondPhaseTriggered)
            {
                NPC.velocity += toTarget * 10;
            }

            SolynChosenShield = currentTarget.Center - NPC.Center;
        }

        if (Time <= ShootStart)
        {
            if (Time == 1)
            {
                VomitLoop?.Stop();

                VomitLoop = LoopedSoundManager.CreateNew
                (
                    GennedAssets.Sounds.Avatar.HeavyBloodStreamLoop,
                    () => !NPC.active
                );

                SoundEngine.PlaySound
                (
                    GennedAssets.Sounds.Avatar.Clap with
                    {
                        PitchVariance = 0.2f,
                        Pitch = -1
                    }
                );
            }

            baseRotation = angleToTarget;
            HeadPos = NPC.Center + angleToTarget.ToRotationVector2() * 70f;

            if (Time % 5 == 0)
            {
                HeadPos += Main.rand.NextVector2Square(1, 3);
            }

            var baseAimDir = NPC.DirectionTo(currentTarget.Center);
            VomitDirection = baseAimDir;
        }

        //start shooting
        if (Time >= ShootStart)
        {
            var predictedDist = (currentTarget.Center + currentTarget.velocity)
                .Distance(NPC.Center);

            var targetMovingAway = predictedDist > distToTarget;

            // Movement during the vomit
            if (targetMovingAway && !HasSecondPhaseTriggered)
            {
                var followStrength = InverseLerp(0f, 1000f, distToTarget);

                NPC.velocity = Vector2.Lerp
                (
                    NPC.velocity,
                    currentTarget.velocity * 0.8f,
                    followStrength
                );
            }
            else
            {
                var attackWeight = InverseLerpBump
                (
                    ShootStart,
                    ShootStart + 40,
                    ShootStop - 40,
                    ShootEnd,
                    Time
                );

                var suckStrength = InverseLerp(440, 680, distToTarget);
                SuckNearbyPlayersGently(4000, suckStrength * attackWeight);

                NPC.velocity *= 0.8f;
            }

            // Vomit sound control
            if (VomitLoop != null)
            {
                VomitLoop.Update
                (
                    HeadPos,
                    sound =>
                    {
                        sound.Pitch = -1.3f * bell + 0.3f;
                        sound.Volume = 2.4f * bell;
                    }
                );
            }

            if (!HasSecondPhaseTriggered)
            {
                neckSpinInterpolant = neckSpinInterpolant.AngleLerp(MathHelper.Pi, 0.012f);
            }
            else
            {
                neckSpinInterpolant = tNorm;
            }

            var spinBase = baseRotation;
            Vector2 headOffset;

            if (!HasSecondPhaseTriggered)
            {
                var sweepRoot = NPC.Center;

                var sweepAngleOffset =
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

            HeadPos = Time < ShootStart + 10
                ? Vector2.Lerp(HeadPos, NPC.Center + headOffset, 0.25f)
                : NPC.Center + headOffset;
        }

        // spawn Vomit COne
        if (Time == ShootStart)
        {
            targetInterpolant = 0;

            StoredVomit = Main.projectile[
                NPC.NewProjectileBetter
                (
                    NPC.GetSource_FromThis(),
                    HeadPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<ConeVomit>(),
                    (int)(NPC.defDamage / 1.6f),
                    0
                )];

            (StoredVomit.ModProjectile as ConeVomit).Owner = NPC;

            NPC.velocity -= NPC.DirectionTo(HeadPos) * 10;
        }

        // Scale vomit & fire goop
        if (Time > ShootStart && Time < ShootStop)
        {
            var fireScale = InverseLerpBump
            (
                ShootStart,
                ShootStart + 20,
                ShootStop - 20,
                ShootEnd,
                Time
            );

            StoredVomit.scale = fireScale;

            if (Time < ShootStop)
            {
                var baseV = NPC.Center.AngleTo(HeadPos).ToRotationVector2() * 26f;

                var divergence = Convert01To010
                (
                    InverseLerp(ShootStart, ShootEnd, Time)
                );

                for (var i = 0; i < 3; i++)
                {
                    var shot = baseV.RotatedByRandom
                               (
                                   MathHelper.PiOver4 * 0.5f * divergence
                               ) *
                               Main.rand.NextFloat(0.9f, 1.2f) *
                               1.4f;

                    if (Main.rand.NextBool(2))
                    {
                        SoundEngine.PlaySound
                            (
                                GennedAssets.Sounds.Avatar.DisgustingStarSever with
                                {
                                    MaxInstances = 0,
                                    PitchVariance = 0.365f,
                                    PitchRange = (-2, -1)
                                }
                            )
                            .WithVolumeBoost(0.2f);

                        NPC.NewProjectileBetter
                        (
                            NPC.GetSource_FromThis(),
                            HeadPos + shot,
                            shot,
                            ModContent.ProjectileType<NowhereGoop>(),
                            (int)(NPC.defDamage * 1.12f),
                            0
                        );
                    }
                }
            }
        }

        if (Time > ShootStop)
        {
            if (VomitLoop?.LoopIsBeingPlayed ?? false)
            {
                VomitLoop.Stop();
            }

            if (StoredVomit != null)
            {
                StoredVomit.scale = MathHelper.Lerp(StoredVomit.scale, 0, 0.2f);
            }
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

    private void SuckNearbyPlayersGently(float radius = 900f, float pullStrength = 0.35f)
    {
        var center = NPC.Center;

        for (var i = 0; i < Main.maxPlayers; i++)
        {
            var p = Main.player[i];

            if (!p.active || p.dead)
            {
                continue;
            }

            var dist = Vector2.Distance(p.Center, center);

            if (dist > radius)
            {
                continue;
            }

            if (p.grappling[0] != -1)
            {
                continue;
            }

            var dir = (center - p.Center).SafeNormalize(Vector2.Zero);

            var closeness = Utils.GetLerpValue(radius, 0f, dist, true);

            p.velocity += dir * pullStrength * closeness;

            if (pullStrength > 0)
            {
                p.mount?.Dismount(p);
            }
        }
    }

    #endregion
}