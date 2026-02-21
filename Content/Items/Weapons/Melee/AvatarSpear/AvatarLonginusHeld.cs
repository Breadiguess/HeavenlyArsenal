using System.IO;
using CalamityMod;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Common.Ui;
using HeavenlyArsenal.Content.Particles;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles.Metaballs;
using Terraria.Audio;
using Terraria.Graphics;

namespace HeavenlyArsenal.Content.Items.Weapons.Melee.AvatarSpear;

public class AvatarLonginusHeld : ModProjectile
{
    public enum AvatarSpearAttacks
    {
        Idle,

        ThrowRupture,

        RapidStabs,

        HeavyStab,

        WhipSlash,

        // Empowered attacks
        // RapidStabs
        SecondSlash,

        RipOut,

        Rupture,

        Castigate
    }

    public const float RuptureCost = 0.2f;

    public const float CastigationCost = 0.55f;

    private static VertexStrip _slashStrip;

    public int CurrentFrame;

    public bool canHit;

    public bool throwMode;

    public int attackedNPC;

    private Vector2 handPosition;

    private float _slashScale;

    private bool _holdSlashUpdate;

    private bool useSlash;

    private bool holdTrailUpdate;

    private Vector2[] _slashPositions;

    private float[] _slashRotations;

    public ref float Time => ref Projectile.ai[0];

    public ref float AttackState => ref Projectile.ai[1];

    public bool InUse => Player.controlUseItem && Player.altFunctionUse == 0;

    public ref Player Player => ref Main.player[Projectile.owner];

    public bool IsEmpowered { get; private set; }

    public Vector2 JavelinOffset { get; set; }

    public int HitTimer { get; set; }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = -1;
        ProjectileID.Sets.TrailCacheLength[Type] = 10;
        ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
    }

    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.DamageType = ModContent.GetInstance<TrueMeleeDamageClass>();
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.hide = true;
        Projectile.noEnchantmentVisuals = true;
        Projectile.manualDirectionChange = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 17;

        const int slashLength = 24;
        _slashPositions = new Vector2[slashLength];
        _slashRotations = new float[slashLength];
    }
    public Vector2 offset = Vector2.Zero;
    public float attackSpeed => Player.GetAttackSpeed(DamageClass.Melee) * (1f + Projectile.extraUpdates * 0.15f);
    public override void AI()
    {
        Projectile.extraUpdates = 3;
        Projectile.timeLeft = 2;
        Player.GetModPlayer<AvatarSpearHeatPlayer>().Empowered = IsEmpowered;

        if (Player.HeldItem.type != ModContent.ItemType<AvatarLonginus>() || Player.CCed || Player.dead || Player.HasBuff(BuffID.Cursed))
        {
            Projectile.Kill();

            return;
        }

        if (IsEmpowered)
        {
            CurrentFrame = (int)float.Lerp(CurrentFrame, 11, 0.25f);
        }
        else
        {
            CurrentFrame = (int)float.Lerp(CurrentFrame, 1, 0.25f);
        }

        Projectile.damage = (int)Player.GetTotalDamage(DamageClass.Melee).ApplyTo(Player.HeldItem.damage);
        Player.heldProj = Projectile.whoAmI;

        throwMode = false;
        canHit = false;

        useSlash = false;
        holdTrailUpdate = false;

        handPosition = Player.MountedCenter;
        

        if (AttackState != (int)AvatarSpearAttacks.Idle)
        {
            if (Time < 2 && Main.myPlayer == Projectile.owner)
            {
                ResetTrail();
                Projectile.velocity = Player.DirectionTo(Main.MouseWorld) * 20f;
                Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
                Projectile.netUpdate = true;
            }
        }

        StateMachine();


        handPosition = Player.RotatedRelativePoint(handPosition);

        if (!throwMode)
        {
            Projectile.Center = handPosition + offset - Projectile.velocity;
        }

        if (canHit)
        {
            for (var i = 0; i < 3; i++)
            {
                var size = (int)(70 * MathF.Exp(-i));
                Projectile.EmitEnchantmentVisualsAt(Projectile.Center + new Vector2((120 + 40 * i) * Projectile.scale, 0).RotatedBy(Projectile.rotation) - new Vector2(size / 2), size, size);
            }
        }

        Projectile.localAI[0]++;

        if (Projectile.localAI[0] > 240)
        {
            Projectile.localAI[0] = 0;
        }

        UpdateTrail();

        if (!_holdSlashUpdate)
        {
            UpdateSlash();
        }

        _holdSlashUpdate = false;

        Lighting.AddLight(Player.Center, Color.DarkRed.ToVector3());

        if (HitTimer > 0)
        {
            HitTimer--;
        }

        if (Main.myPlayer == Projectile.owner)
        {
            var heat = Player.GetModPlayer<AvatarSpearHeatPlayer>().Heat;

            if (heat > 0f)
            {
                WeaponBar.DisplayBar(Color.DarkRed, Color.Red, heat, style: 1, BarOffset: IsEmpowered ? Main.rand.NextVector2Circular(2, 2) : Vector2.Zero);
            }
        }
    }


    public void StateMachine()
    {

        switch (AttackState)
        {
            default:
            case (int)AvatarSpearAttacks.Idle:

                Time = 0;

                Projectile.scale = 1f;
                Projectile.velocity = Vector2.Zero;
                var motionBob = Player.velocity.X * 0.02f - Player.velocity.Y * 0.015f * Player.direction;
                Projectile.rotation = Projectile.rotation.AngleLerp(Player.fullRotation - MathHelper.PiOver2 + 1f * Player.direction + motionBob, 0.1f);
                Projectile.spriteDirection = Player.direction;

                Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, -MathHelper.PiOver2 * Player.direction + motionBob * 1.2f);
                Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, -0.3f * Player.direction + motionBob * 0.3f);
                handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.ThreeQuarters, -0.3f * Player.direction);

                if (Player.altFunctionUse == 1 && Player.ItemTimeIsZero)
                {
                    if (IsEmpowered && Player.GetModPlayer<AvatarSpearHeatPlayer>().ConsumeHeat(0.5f))
                    {
                        AttackState = (int)AvatarSpearAttacks.Castigate;
                    }
                    else
                    {
                        AttackState = (int)AvatarSpearAttacks.ThrowRupture;
                    }
                }
                else if (InUse && Player.ItemTimeIsZero)
                {
                    AttackState = (int)AvatarSpearAttacks.RapidStabs;
                }

                HandleEmpowerment();

                break;

            case (int)AvatarSpearAttacks.RapidStabs:

                Player.SetDummyItemTime(10);

                const int RapidWindUp = 50;
                var RapidStabCount = IsEmpowered ? 10 : 5;
                const int RapidWindDown = 50;

                var RapidStabTime = RapidStabCount * 13 + (int)(50 / attackSpeed);

                if (Time < RapidWindUp)
                {
                    var windProgress = Time / (RapidWindUp - 1f);

                    var wiggle = MathF.Sin(MathF.Pow(windProgress, 2f) * MathHelper.Pi) * -0.4f * Projectile.direction;
                    Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.ToRotation() + wiggle, 0.9f);
                    offset = new Vector2(MathHelper.SmoothStep(0, -50, windProgress), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f - windProgress * 0.33f;

                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, Projectile.rotation - MathHelper.PiOver2);
                    handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.None, Projectile.rotation - MathHelper.PiOver2);
                }

                else if (Time < RapidWindUp + RapidStabTime)
                {
                    var windDownProgress = Utils.GetLerpValue(RapidWindDown, 0f, Time - RapidWindUp - RapidStabTime, true);

                    if (Time < RapidWindUp + RapidStabTime)
                    {
                        canHit = true;
                    }

                    var timePerStab = MathF.Ceiling(RapidStabTime / RapidStabCount);
                    var stabProgress = Utils.GetLerpValue(0, timePerStab, (Time - RapidWindUp) % timePerStab);

                    if (Time - RapidWindUp >= timePerStab * RapidStabCount)
                    {
                        stabProgress = 1f;
                    }

                    var stabCurve = Utils.GetLerpValue(0, 0.5f, stabProgress, true);

                    if (Time % timePerStab == 0)
                    {
                        SoundEngine.PlaySound
                        (
                            GennedAssets.Sounds.Avatar.StakeImpale with
                            {
                                Pitch = 1f,
                                PitchVariance = 0.2f,
                                Volume = 0.4f,
                                MaxInstances = 0
                            },
                            Projectile.Center
                        );

                        if (Main.myPlayer == Projectile.owner)
                        {
                            var accuracy = Utils.GetLerpValue(RapidStabTime, RapidStabTime * 0.5f, Time - RapidWindUp, true);
                            Projectile.velocity = Player.DirectionTo(Main.MouseWorld).RotatedByRandom(accuracy * 0.5f) * 15f;
                            Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
                            Projectile.netUpdate = true;
                        }
                    }

                    Projectile.rotation = Projectile.velocity.ToRotation();
                    offset = new Vector2(stabCurve * 200 - 50, 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f + stabCurve * 0.5f;

                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                    var handSwingDir = (int)(Utils.GetLerpValue(0, RapidStabTime, Time - RapidWindUp, true) * RapidStabCount) % 2 > 0 ? 1 : -1;
                    var handRot = Projectile.rotation - MathHelper.PiOver2 + (1f - stabCurve) * handSwingDir * Player.direction;
                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, handRot);
                    handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, handRot);
                }
                else
                {
                    var windDownProgress = Utils.GetLerpValue(RapidWindDown / 4f, RapidWindDown, Time - RapidWindUp - RapidStabTime, true);
                    offset = new Vector2(150, 0).RotatedBy(Projectile.rotation) * MathF.Cbrt(1f - windDownProgress);
                    Projectile.scale = 1.5f - MathF.Pow(windDownProgress, 3);

                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
                    handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
                }

                Time++;

                if (Time > RapidWindUp + RapidStabTime + RapidWindDown / 2)
                {
                    if (InUse)
                    {
                        HandleEmpowerment();
                        AttackState = (int)AvatarSpearAttacks.HeavyStab;
                        Time = 0;
                    }
                    else if (Time > RapidWindUp + RapidStabTime + RapidWindDown)
                    {
                        AttackState = (int)AvatarSpearAttacks.Idle;
                        Time = 0;
                    }
                }

                break;

            case (int)AvatarSpearAttacks.WhipSlash:
            case (int)AvatarSpearAttacks.SecondSlash:

                Player.SetDummyItemTime(10);

                var isSecondSlash = AttackState == (int)AvatarSpearAttacks.SecondSlash;

                var SlashWindUp = isSecondSlash ? 20 : 30;
                var SlashTime = (isSecondSlash ? 20 : 35) + (int)(30 / attackSpeed);
                var SlashWindDown = (isSecondSlash ? 30 : 20) + (int)(25 / attackSpeed);
                var SlashRotation = MathHelper.ToRadians(190);

                if (isSecondSlash && Time < 3) // Flip the second slash
                {
                    Projectile.direction = Projectile.velocity.X > 0 ? -1 : 1;
                }

                ResetTrail();

                if (Time < SlashWindUp)
                {
                    var windProgress = Time / (SlashWindUp - 1f);

                    var wiggle = -MathF.Sqrt(windProgress) * SlashRotation * -Projectile.direction;

                    Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.ToRotation() + wiggle, 1f - windProgress);
                    offset = new Vector2(MathHelper.SmoothStep(0, -30, windProgress), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f - windProgress * (1 - windProgress);

                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
                }
                else
                {
                    if (Time > SlashWindUp + 2 && Time < SlashWindDown + SlashTime + SlashWindDown / 3)
                    {
                        useSlash = true;
                    }
                    else
                    {
                        ResetSlash();
                    }

                    if (Time > SlashWindDown + SlashTime)
                    {
                        _holdSlashUpdate = true;
                    }

                    canHit = true;

                    if (Time == SlashWindUp + 1)
                    {
                        SoundEngine.PlaySound
                        (
                            GennedAssets.Sounds.Avatar.StakeGraze with
                            {
                                Pitch = 0.2f,
                                PitchVariance = 0.1f,
                                Volume = 0.66f,
                                MaxInstances = 0
                            },
                            Projectile.Center
                        );
                    }

                    if (Time == SlashWindUp + SlashTime - 1)
                    {
                        DoShake();

                        SoundEngine.PlaySound
                        (
                            GennedAssets.Sounds.Common.MediumBloodSpill with
                            {
                                Pitch = 1f,
                                PitchVariance = 0.1f,
                                MaxInstances = 0
                            },
                            Projectile.Center
                        );
                    }

                    var slashProgress = Utils.GetLerpValue(0, SlashTime, Time - SlashWindUp, true);
                    var windDown = Utils.GetLerpValue(0, SlashWindDown, Time - SlashWindUp - SlashTime, true);

                    var slashCurve = MathF.Pow(slashProgress, 1.5f);

                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                    var currentSlashRot = MathHelper.Lerp(-SlashRotation, SlashRotation * 0.1f + MathF.Pow(windDown, 4f) * 0.2f, slashCurve) * Projectile.direction;
                    Projectile.rotation = Projectile.velocity.ToRotation() + currentSlashRot;

                    float slashDistance = isSecondSlash ? 120 : 200;
                    offset = new Vector2(-30 + slashDistance * MathF.Sin(slashProgress * MathHelper.PiOver2) * (1f - MathF.Pow(windDown, 2f)), 0).RotatedBy(Projectile.rotation);

                    Projectile.scale = 1f + slashProgress * (1f - MathF.Sqrt(windDown)) * (slashDistance / 180f);
                }

                Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
                handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);

                Time++;

                if (Time > SlashWindUp + SlashTime + SlashWindDown)
                {
                    HandleEmpowerment();

                    if (InUse)
                    {
                        if (IsEmpowered && !isSecondSlash)
                        {
                            AttackState = (int)AvatarSpearAttacks.SecondSlash;
                        }
                        else
                        {
                            AttackState = (int)AvatarSpearAttacks.RapidStabs;
                        }

                        Time = 0;
                    }
                    else if (Time > SlashTime)
                    {
                        AttackState = (int)AvatarSpearAttacks.Idle;
                        Time = 0;
                    }
                }

                break;

            case (int)AvatarSpearAttacks.HeavyStab:

                Player.SetDummyItemTime(10);

                var HeavyWindUp = IsEmpowered ? 60 : 80;
                var HeavyThrustTime = 10 + (int)(30 / Player.GetAttackSpeed(DamageClass.Melee));
                var HeavyWindDown = 10 + (int)(20 / Player.GetAttackSpeed(DamageClass.Melee));

                if (Time < HeavyWindUp)
                {
                    ResetTrail(true);

                    var windProgress = Time / (HeavyWindUp - 1f);

                    var wiggle = (windProgress * -0.5f - 0.5f) * Projectile.direction;
                    Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.ToRotation() + wiggle * 2f * (1f - windProgress), windProgress * 0.5f);
                    offset = new Vector2(MathHelper.SmoothStep(0, -50, windProgress), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f;
                    Projectile.velocity = Player.DirectionTo(Main.MouseWorld) * 15f;

                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.PiOver2);
                    handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.PiOver2);

                    if (Time == (IsEmpowered ? 0 : HeavyWindUp / 4))
                    {
                        SoundEngine.PlaySound
                        (
                            GennedAssets.Sounds.Enemies.DismalLanternSway with
                            {
                                Pitch = 0.4f,
                                MaxInstances = 0
                            },
                            Projectile.Center
                        );
                    }
                }
                else
                {
                    if (Time == HeavyWindUp + 7)
                    {
                        BreakSoundBarrierParticle(1f);
                        DoShake(0.7f);

                        SoundEngine.PlaySound
                        (
                            GennedAssets.Sounds.Avatar.StakeGraze with
                            {
                                Pitch = 0.6f,
                                PitchVariance = 0.2f,
                                Volume = 0.3f,
                                MaxInstances = 0
                            },
                            Projectile.Center
                        );

                        SoundEngine.PlaySound
                        (
                            GennedAssets.Sounds.Avatar.StakeImpale with
                            {
                                Pitch = 0.2f,
                                PitchVariance = 0.2f,
                                Volume = 0.6f,
                                MaxInstances = 0
                            },
                            Projectile.Center
                        );
                    }

                    canHit = true;
                    var thrustProgress = Utils.GetLerpValue(0, HeavyThrustTime, Time - HeavyWindUp, true);
                    var windDown = Utils.GetLerpValue(0, HeavyWindDown, Time - HeavyWindUp - HeavyThrustTime, true);

                    var thrustCurve = Utils.GetLerpValue(0, 0.15f, thrustProgress, true);
                    var windDownCurve = MathF.Cbrt(windDown);

                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, Player.DirectionTo(Main.MouseWorld) * 15f, 0.01f);
                        Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
                        Projectile.netUpdate = true;
                    }

                    Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.ToRotation() - (thrustCurve - 1f) * 0.3f * Projectile.direction, 0.5f - thrustCurve * 0.2f);
                    offset = new Vector2(MathHelper.SmoothStep(0, 150, thrustCurve) * (1f - windDownCurve), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1.75f - windDownCurve * 0.5f;

                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
                    handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
                }

                Time++;

                if (Time > HeavyWindDown + HeavyThrustTime + HeavyWindDown - 20 && InUse && IsEmpowered && HitTimer > 1)
                {
                    AttackState = (int)AvatarSpearAttacks.RipOut;
                    Time = 0;
                }

                if (Time > HeavyWindUp + HeavyThrustTime + HeavyWindDown - 5)
                {
                    HandleEmpowerment();

                    if (InUse)
                    {
                        AttackState = (int)AvatarSpearAttacks.WhipSlash;
                        Time = 0;
                    }
                    else if (Time > HeavyWindUp + HeavyThrustTime - HeavyWindDown)
                    {
                        AttackState = (int)AvatarSpearAttacks.Idle;
                        Time = 0;
                    }
                }

                break;

            case (int)AvatarSpearAttacks.RipOut:

                Player.SetDummyItemTime(10);

                const int PullTime = 80;
                const int RipTime = 90;

                if (Time < PullTime)
                {
                    throwMode = true;

                    var pullProgress = Time / PullTime;

                    if (attackedNPC > -1 && attackedNPC < Main.npc.Length)
                    {
                        if (Main.npc[attackedNPC].active && Main.npc[attackedNPC].life > 2)
                        {
                            Projectile.Center = Main.npc[attackedNPC].Center + JavelinOffset;
                        }
                        else
                        {
                            Time = PullTime;
                        }
                    }

                    Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.ToRotation(), 0.5f);
                    offset = new Vector2(150 + MathF.Sin(pullProgress * MathHelper.Pi) * 50, 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1.75f;
                }
                else
                {
                    canHit = true;

                    if (Time == PullTime + 1)
                    {
                        DoShake(1.5f);

                        SoundEngine.PlaySound
                        (
                            GennedAssets.Sounds.Avatar.ArmJutOut with
                            {
                                Pitch = -0.1f,
                                MaxInstances = 0
                            },
                            Projectile.Center
                        );

                        //i didnt write this lmao
                        //thanks blockaroz
                        // MANKIND IS DEAD
                        // BLOOD IS FUEL
                        // HELL IS FULL
                        var metaball = ModContent.GetInstance<BloodMetaball>();

                        for (var i = 0; i < 50; i++)
                        {
                            var bloodSpawnPosition = Projectile.Center + new Vector2(150 * Projectile.scale + offset.X, offset.Y).RotatedBy(Projectile.rotation);
                            var bloodVelocity = (Main.rand.NextVector2Circular(8f, 8f) - Projectile.velocity) * Main.rand.NextFloat(0.2f, 1.2f);
                            metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(10f, 40f), Main.rand.NextFloat(2f));
                        }
                    }

                    var ripProgress = Utils.GetLerpValue(0, RipTime, Time - PullTime, true);
                    var twirl = MathF.Cbrt(ripProgress) * 2f * (1f - ripProgress * 0.3f) + MathHelper.TwoPi * MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(0.3f, 1f, ripProgress, true));
                    Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.ToRotation() + twirl * Projectile.direction, 0.5f - ripProgress * 0.3f);

                    Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, 0.05f);
                }

                Time++;

                if (Time > PullTime + RipTime - 10)
                {
                    HandleEmpowerment();

                    if (InUse)
                    {
                        AttackState = (int)AvatarSpearAttacks.WhipSlash;
                        Time = 0;
                    }
                    else if (Time > PullTime + RipTime)
                    {
                        AttackState = (int)AvatarSpearAttacks.Idle;
                        Time = 0;
                    }
                }

                break;

            case (int)AvatarSpearAttacks.ThrowRupture:

                Player.SetDummyItemTime(10);

                const int ThrowWindUp = 30;
                const int ThrowTime = 50;
                const int TPTime = 50;

                if (Time < ThrowWindUp)
                {
                    if (Time == 1)
                    {
                        SoundEngine.PlaySound
                        (
                            GennedAssets.Sounds.Common.Twinkle with
                            {
                                Pitch = 0f,
                                PitchVariance = 0.2f,
                                Volume = 0.5f,
                                MaxInstances = 0
                            },
                            Projectile.Center
                        );
                    }

                    HitTimer = 0;

                    Player.velocity -= Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.5f;
                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                    var windProgress = Time / ThrowWindUp;

                    var wiggle = (windProgress * -0.5f - 0.5f) * Projectile.direction;
                    Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.velocity.ToRotation() + wiggle, windProgress);
                    offset = new Vector2(MathHelper.SmoothStep(0, -80, windProgress), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f;
                }
                else if (Time < ThrowWindUp + ThrowTime)
                {
                    if (Time == ThrowWindUp)
                    {
                        Projectile.Center = Player.MountedCenter;

                        SoundEngine.PlaySound
                        (
                            GennedAssets.Sounds.Avatar.StakeGraze with
                            {
                                Pitch = -0.1f,
                                PitchVariance = 0.2f,
                                MaxInstances = 0
                            },
                            Projectile.Center
                        );

                        SoundEngine.PlaySound
                        (
                            GennedAssets.Sounds.Avatar.RiftOpen with
                            {
                                Pitch = 1f,
                                PitchVariance = 0.2f,
                                Volume = 0.3f,
                                MaxInstances = 0
                            },
                            Projectile.Center
                        );
                    }

                    if (Time == ThrowWindUp + 2)
                    {
                        BreakSoundBarrierParticle(-0.5f);
                    }

                    Projectile.extraUpdates = 8;

                    canHit = true;
                    throwMode = true;
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 20f;
                    Projectile.rotation = Projectile.velocity.ToRotation();
                }
                else
                {
                    Projectile.extraUpdates = 2;
                    Projectile.velocity *= 0.6f;

                    if (Main.myPlayer == Projectile.owner)
                    {
                        Main.SetCameraLerp(0.1f, 10);
                    }

                    canHit = true;
                    throwMode = true;

                    if (HitTimer <= 0 && !Collision.SolidCollision(Projectile.Center - new Vector2(20) + Projectile.velocity.SafeNormalize(Vector2.Zero) * 20, 40, 40))
                    {
                        Player.Center = Vector2.Lerp(Player.Center, Projectile.Center, 0.3f);
                    }
                    else
                    {
                        Projectile.Center = Vector2.Lerp(Projectile.Center, Player.MountedCenter, MathF.Pow(Utils.GetLerpValue(0, TPTime, Time - ThrowWindUp - ThrowTime, true), 5f) * 0.2f);
                    }
                }

                Player.velocity *= 0.98f;

                if (HitTimer > 0)
                {
                    throwMode = true;
                    HitTimer = TPTime + 10;

                    if (Time < ThrowTime + ThrowWindUp)
                    {
                        Time = ThrowWindUp + ThrowTime;
                        Projectile.velocity *= -0.5f;
                        Player.SetImmuneTimeForAllTypes(30);
                    }
                }

                Time++;

                if (Time > ThrowWindUp + ThrowTime + TPTime)
                {
                    AttackState = (int)AvatarSpearAttacks.Idle;
                    Time = 0;
                }

                break;

            case (int)AvatarSpearAttacks.Rupture:

                Projectile.velocity *= 0.9f;
                throwMode = true;

                var validNPC = attackedNPC > -1 && attackedNPC < Main.npc.Length;

                if (validNPC)
                {
                    if (Main.npc[attackedNPC].active && Main.npc[attackedNPC].life > 2)
                    {
                        Projectile.Center = Main.npc[attackedNPC].Center + JavelinOffset;
                    }
                    else
                    {
                        Time = AvatarSpearRupture.FlickerTime + AvatarSpearRupture.ExplosionTime + 2;
                    }
                }

                if (!validNPC || Time > AvatarSpearRupture.FlickerTime + AvatarSpearRupture.ExplosionTime)
                {
                    AttackState = (int)AvatarSpearAttacks.Idle;
                    Time = 0;
                }

                Time++;

                break;

            case (int)AvatarSpearAttacks.Castigate:

                Projectile.extraUpdates = 6;

                const int CastigateWindTime = 100;
                const int PortalWaves = 7;
                const int PortalsPerWave = 3;
                const int PortalTime = 111;
                const float MaxTime = PortalWaves * PortalTime + CastigateWindTime;

                Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                var twirlProgress = Time / MaxTime;
                var castigateTwirl = MathHelper.TwoPi * 2 * PortalWaves * MathHelper.SmoothStep(0f, 1f, twirlProgress);

                var castigateWind = Utils.GetLerpValue(2, CastigateWindTime, Time, true);
                var castigateHandRot = (-MathHelper.Pi * MathF.Sqrt(castigateWind) + MathF.Sin(castigateTwirl) * 0.5f) * Player.direction;
                Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, castigateHandRot);
                handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, castigateHandRot);

                Projectile.rotation = Projectile.rotation.AngleLerp(-castigateTwirl * Projectile.direction, MathF.Pow(castigateWind, 3f));

                canHit = Time > CastigateWindTime;

                Projectile.scale = MathF.Sin(twirlProgress * 4f) * 0.4f + 1f;

                if (Time == CastigateWindTime)
                {
                    SoundEngine.PlaySound
                    (
                        GennedAssets.Sounds.Avatar.ArcticWindGust with
                        {
                            Volume = 0.5f,
                            Pitch = 0.5f,
                            MaxInstances = 0
                        },
                        Projectile.Center
                    );
                }

                if (Time >= CastigateWindTime)
                {
                    DoShake(0.1f);

                    if ((Time - CastigateWindTime) % PortalTime == PortalTime / 3)
                    {
                        SoundEngine.PlaySound
                        (
                            GennedAssets.Sounds.Avatar.ArmSwing with
                            {
                                Volume = 0.33f,
                                PitchVariance = 0.5f,
                                MaxInstances = 0
                            },
                            Projectile.Center
                        );

                        for (var l = 0; l < PortalsPerWave; l++)
                        {
                            var velocity = (Main.rand.NextVector2Circular(30, 30) + Main.rand.NextVector2CircularEdge(20, 20)).RotatedBy(l / 3 * MathHelper.TwoPi);
                            velocity += Player.velocity / 2;

                            var rift = Projectile.NewProjectileDirect
                                (Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<LonginusRift>(), Projectile.damage * 2, 1f, Player.whoAmI);

                            rift.scale *= Main.rand.NextFloat(1f, 1.4f);
                            rift.direction = Main.rand.NextBool().ToDirectionInt();
                            rift.timeLeft += Main.rand.Next(-20, 20);
                        }
                    }
                }

                if (Time > CastigateWindTime + 5)
                {
                    useSlash = true;
                }
                else
                {
                    ResetSlash();
                }

                _slashScale = 0.7f;

                if (Time > MaxTime)
                {
                    AttackState = (int)AvatarSpearAttacks.Idle;
                    Time = 0;
                }

                Time++;

                break;
        }
    }
    public void ResetTrail(bool rotation = false)
    {
        for (var i = 0; i < Projectile.oldPos.Length; i++)
        {
            Projectile.oldPos[i] = Projectile.Center;

            if (rotation)
            {
                Projectile.oldRot[i] = Projectile.rotation;
            }
        }
    }

    public void UpdateTrail()
    {
        var playerPosOffset = Player.position - Player.oldPosition;

        if (Projectile.numUpdates == 0)
        {
            playerPosOffset = Vector2.Zero;

            for (var i = Projectile.oldPos.Length - 1; i > 0; i--)
            {
                Projectile.oldPos[i] = Projectile.oldPos[i - 1];
                Projectile.oldRot[i] = Projectile.rotation.AngleLerp(Projectile.oldRot[i - 1], 0.1f);

                if (!throwMode)
                {
                    Projectile.oldPos[i] += playerPosOffset;
                }
            }

            if (!holdTrailUpdate)
            {
                Projectile.oldPos[0] = Projectile.Center + Projectile.velocity;
                Projectile.oldRot[0] = Projectile.rotation;
            }

            if (!throwMode)
            {
                Projectile.oldPos[0] += playerPosOffset - Player.velocity;
            }
        }
    }

    public void ResetSlash()
    {
        _slashScale = 1f;

        for (var i = _slashPositions.Length - 1; i > 0; i--)
        {
            _slashPositions[i] = new Vector2(200 * Projectile.scale, 0).RotatedBy(Projectile.rotation);
            _slashRotations[i] = Projectile.rotation + MathHelper.PiOver2 * Projectile.direction;
        }
    }

    public void UpdateSlash()
    {
        for (var i = _slashPositions.Length - 1; i > 0; i--)
        {
            _slashRotations[i] = _slashRotations[i - 1];
            _slashPositions[i] = _slashPositions[i - 1];
        }

        _slashRotations[0] = Projectile.rotation + MathHelper.PiOver2 * Projectile.direction;
        _slashPositions[0] = new Vector2(200 * Projectile.scale * _slashScale, 0).RotatedBy(Projectile.rotation);
    }

    public void HandleEmpowerment()
    {
        IsEmpowered = Player.GetModPlayer<AvatarSpearHeatPlayer>().Active;
    }

    private void DoShake(float strength = 1f)
    {
        if (Main.myPlayer == Projectile.owner)
        {
            ScreenShakeSystem.StartShakeAtPoint
            (
                Projectile.Center,
                7f * strength,
                shakeDirection: Projectile.velocity.SafeNormalize(Vector2.Zero) * 2,
                shakeStrengthDissipationIncrement: 0.7f - strength * 0.1f
            );
        }
    }

    private void BreakSoundBarrierParticle(float speed)
    {
        var soundBarrierVelocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * speed;
        var soundBarrierPosition = Projectile.Center + new Vector2(100, 0).RotatedBy(Projectile.rotation);

        var particle = SoundBarrierParticle.pool.RequestParticle();

        particle.Prepare
        (
            soundBarrierPosition,
            -soundBarrierVelocity * 30f + Player.velocity,
            Projectile.rotation,
            Color.DarkRed with
            {
                A = 200
            },
            0.5f
        );

        var largerParticle = SoundBarrierParticle.pool.RequestParticle();

        largerParticle.Prepare
        (
            soundBarrierPosition,
            -soundBarrierVelocity * 50f + Player.velocity,
            Projectile.rotation,
            Color.DarkRed with
            {
                A = 200
            },
            1f
        );

        ParticleEngine.ShaderParticles.Add(particle);
        ParticleEngine.ShaderParticles.Add(largerParticle);
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (IsEmpowered)
        {
            modifiers.FinalDamage *= 1.33f;
        }

        if (AttackState == (int)AvatarSpearAttacks.ThrowRupture)
        {
            modifiers.FinalDamage /= 3;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        OnHitEffects(target.Center, target.velocity, target.CanBeChasedBy(this));

        var ruptureType = ModContent.ProjectileType<AvatarSpearRupture>();

        if (AttackState == (int)AvatarSpearAttacks.ThrowRupture && Player.ownedProjectileCounts[ruptureType] < 1)
        {
            if (Player.GetModPlayer<AvatarSpearHeatPlayer>().ConsumeHeat(RuptureCost))
            {
                AttackState = (int)AvatarSpearAttacks.Rupture;
                Time = 0;

                var bombVelocity = Player.DirectionTo(target.Center).SafeNormalize(Vector2.Zero) * 20;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Player.MountedCenter, bombVelocity, ruptureType, Projectile.damage * 2, 0.5f, Player.whoAmI, ai1: target.whoAmI + 1);
            }
        }

        switch (AttackState)
        {
            case (int)AvatarSpearAttacks.RapidStabs:

                if (IsEmpowered && HitTimer <= 0)
                {
                    var spear = Projectile.NewProjectileDirect
                    (
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        Main.rand.NextVector2Circular(30, 30),
                        ModContent.ProjectileType<AntishadowLonginus>(),
                        Projectile.damage,
                        1f,
                        Projectile.owner
                    );

                    spear.ai[1] = target.whoAmI + 1;
                    spear.scale *= Main.rand.NextFloat(0.9f, 1.3f);
                }

                break;
        }

        if (Main.myPlayer == Projectile.owner)
        {
            HitTimer = 5;
            JavelinOffset = Projectile.Center - target.Center;
            attackedNPC = target.whoAmI;
            Projectile.netUpdate = true;
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        OnHitEffects(target.Center, target.velocity, true);

        if (Main.myPlayer == Projectile.owner)
        {
            HitTimer = 5;
            JavelinOffset = Projectile.Center - target.Center;
            attackedNPC = -1;
            Projectile.netUpdate = true;
        }
    }

    public void OnHitEffects(Vector2 targetPosition, Vector2 targetVelocity, bool canAddHeat)
    {
        var addHeat = 0.01f;
        var forceAddHeat = false;

        switch (AttackState)
        {
            case (int)AvatarSpearAttacks.WhipSlash:
            case (int)AvatarSpearAttacks.SecondSlash: addHeat = 0.1f; break;
            case (int)AvatarSpearAttacks.HeavyStab: addHeat = IsEmpowered ? 0.33f : 0.02f; break;
            case (int)AvatarSpearAttacks.RipOut: addHeat = IsEmpowered ? 0.33f : 0f; break;
            case (int)AvatarSpearAttacks.ThrowRupture: addHeat = 0.002f; break;
        }

        if (IsEmpowered)
        {
            addHeat *= 0.15f;
        }

        if ((HitTimer <= 0 || forceAddHeat) && canAddHeat)
        {
            Player.GetModPlayer<AvatarSpearHeatPlayer>().AddHeat(addHeat);
        }

        DoShake(0.2f);

        var metaball = ModContent.GetInstance<BloodMetaball>();

        for (var i = 0; i < (IsEmpowered ? 6 : 2); i++)
        {
            var bloodSpawnPosition = targetPosition + Main.rand.NextVector2Circular(24, 24);
            var bloodVelocity = (Main.rand.NextVector2Circular(5f, 5f) + Projectile.velocity) * Main.rand.NextFloat(0.2f, 1f);
            metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(10f, 40f), Main.rand.NextFloat(2f));
        }

        for (var i = 0; i < Main.rand.Next(1, 5); i++)
        {
            var lightningPos = targetPosition + Main.rand.NextVector2Circular(24, 24);

            var particle = HeatLightning.pool.RequestParticle();
            particle.Prepare(lightningPos, targetVelocity + Main.rand.NextVector2Circular(10, 10), Main.rand.NextFloat(-1f, 1f), 10 + i * 4, Main.rand.NextFloat(0.5f, 1.5f));
            ParticleEngine.Particles.Add(particle);
        }
    }

    public override bool? CanCutTiles()
    {
        return canHit;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        hitbox.Inflate(200, 200);
        hitbox.Location += new Vector2(180 * Projectile.scale, 0).RotatedBy(Projectile.rotation).ToPoint();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (canHit)
        {
            var dist = 200f;

            if (AttackState == (int)AvatarSpearAttacks.ThrowRupture)
            {
                dist = 100f;
            }

            var offset = new Vector2(dist * Projectile.scale * (IsEmpowered ? 1.5f : 1f), 0).RotatedBy(Projectile.rotation);
            float _ = 0;

            return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center - offset / 2, Projectile.Center + offset, 120f, ref _);
        }

        return false;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(canHit);
        writer.Write(IsEmpowered);
        writer.Write(HitTimer);
        writer.WriteVector2(JavelinOffset);
        writer.Write(CurrentFrame);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        canHit = reader.ReadBoolean();
        IsEmpowered = reader.ReadBoolean();
        HitTimer = reader.ReadInt32();
        JavelinOffset = reader.ReadVector2();
        CurrentFrame = reader.ReadInt32();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var texturePath = "HeavenlyArsenal/Content/Items/Weapons/Melee/AvatarSpear/AvatarLonginusHeld";
        //if(Main.LocalPlayer.name == "ModTester2")
        //    texturePath = "HeavenlyArsenal/Content/Projectiles/Weapons/Melee/AvatarSpear/AvatarLonginusHeld";

        var texture = ModContent.Request<Texture2D>(texturePath).Value;
        //default: TextureAssets.Projectile[Type].Value;

        Rectangle frame;
        //if (Main.LocalPlayer.name == "ModTester2")
        //    frame = texture.Frame(1, 2,0, IsEmpowered? 2 :1);

        {
            frame = texture.Frame(1, 2, 0, IsEmpowered? 1:0);
        }

        var glow = AssetDirectory.Textures.BigGlowball.Value;

        var scale = Projectile.scale;
        var direction = Projectile.spriteDirection;
        var flipEffect = direction > 0 ? 0 : SpriteEffects.FlipVertically;
        var gripDistance = IsEmpowered ? 40 : 60;
        var origin = new Vector2(frame.Width / 2 - gripDistance, frame.Height / 2 + (gripDistance - 4) * Player.gravDir * direction);
        var spearHeadPosition = Projectile.Center + new Vector2(90 * Projectile.scale, 0).RotatedBy(Projectile.rotation);

        var glowAmt = (IsEmpowered ? 0.7f : 0.4f) + MathF.Cbrt(Math.Clamp(HitTimer / 5f, 0f, 1f)) * 0.4f;

        for (var i = 0; i < Projectile.oldPos.Length; i++)
        {
            var color = Color.Lerp
                        (
                            Color.Red with
                            {
                                A = 200
                            } *
                            0.5f,
                            Color.DarkBlue with
                            {
                                A = 50
                            },
                            i / 9f
                        ) *
                        (1f - (float)i / Projectile.oldPos.Length);

            Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Main.screenPosition, frame, color * 0.5f, Projectile.oldRot[i] + MathHelper.PiOver4 * direction, origin, scale, flipEffect);
        }

        if (useSlash)
        {
            DrawSlash();
        }

        var highlightOffset = new Vector2(3, 0).RotatedBy(Main.GlobalTimeWrappedHourly * 2 * MathHelper.TwoPi);

        Main.EntitySpriteDraw
        (
            texture,
            Projectile.Center + highlightOffset - Main.screenPosition,
            frame,
            Color.Red with
            {
                A = 100
            } *
            0.5f,
            Projectile.rotation + MathHelper.PiOver4 * direction,
            origin,
            scale,
            flipEffect
        );

        Main.EntitySpriteDraw
        (
            texture,
            Projectile.Center - highlightOffset - Main.screenPosition,
            frame,
            Color.RoyalBlue with
            {
                A = 100
            } *
            0.5f,
            Projectile.rotation + MathHelper.PiOver4 * direction,
            origin,
            scale,
            flipEffect
        );

        Main.EntitySpriteDraw
        (
            glow,
            spearHeadPosition - Main.screenPosition,
            glow.Frame(),
            Color.Red with
            {
                A = 120
            } *
            (glowAmt + 0.2f),
            Projectile.rotation,
            glow.Size() * 0.5f,
            scale * 0.15f,
            flipEffect
        );

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation + MathHelper.PiOver4 * direction, origin, scale, flipEffect);

        Main.EntitySpriteDraw
        (
            glow,
            spearHeadPosition - Main.screenPosition,
            glow.Frame(),
            Color.DarkRed with
            {
                A = 0
            } *
            glowAmt,
            Projectile.rotation,
            glow.Size() * 0.5f,
            scale * 0.4f,
            flipEffect
        );

        return false;
    }

    private void DrawSlash()
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        // Have a shader prepared, only special thing is that it uses a normalized matrix
        var trailShader = ShaderManager.GetShader("HeavenlyArsenal.LonginusSlash");

        if (Main.LocalPlayer.name == "ModTester2")
        {
            trailShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 0, SamplerState.LinearWrap);
        }
        else
        {
            trailShader.SetTexture(GennedAssets.Textures.Noise.SwirlNoise, 0, SamplerState.PointClamp);
        }

        trailShader.TrySetParameter("uTime", Projectile.localAI[0] / 30f);
        trailShader.TrySetParameter("uWorldViewProjection", Main.GameViewMatrix.NormalizedTransformationmatrix);
        trailShader.TrySetParameter("uColor", Color.White.ToVector4() * 0.66f);
        trailShader.Apply();

        // Rendering primitives involves setting vertices of each triangle to form quads
        // This does it for us
        // Have a list of positions and rotations to create vertices, width function to determine how far vertices are from the center
        // Color function determines each vertex's color, which can be used in the shader
        _slashStrip ??= new VertexStrip();
        _slashStrip.PrepareStrip(_slashPositions, _slashRotations, TrailColorFunction, TrailWidthFunction, Player.Center - Main.screenPosition, _slashPositions.Length, true);
        _slashStrip.DrawTrail();

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
    }

    private float TrailWidthFunction(float p)
    {
        return (IsEmpowered ? 110 : 75) * Projectile.scale * _slashScale * Projectile.direction;
    }

    private Color TrailColorFunction(float p)
    {
        return Color.Lerp
        (
            Color.Red with
            {
                A = 120
            },
            Color.DarkCyan with
            {
                A = 50
            },
            p
        );
    }
}