
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Common.Players;
using HeavenlyArsenal.Common.UI;
using HeavenlyArsenal.Content.Items.Weapons.Melee;
using HeavenlyArsenal.Content.Particles;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles;
using NoxusBoss.Content.Particles.Metaballs;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;


namespace HeavenlyArsenal.Content.Projectiles.Weapons.Melee.AvatarSpear;

public class AvatarLonginusHeld : ModProjectile
{
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
        Projectile.DamageType = DamageClass.Melee;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.hide = true;
        Projectile.noEnchantmentVisuals = true;
        Projectile.manualDirectionChange = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 17;
    }

    public ref float Time => ref Projectile.ai[0];
    public ref float AttackState => ref Projectile.ai[1];

    public bool canHit;
    public bool throwMode;
    private Vector2 handPosition;

    public bool InUse => Player.controlUseItem && Player.altFunctionUse == 0;

    public ref Player Player => ref Main.player[Projectile.owner];

    public bool IsEmpowered { get; private set; }

    public Vector2 JavelinOffset { get; set; }

    public int HitTimer { get; set; }

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
        Castigation
    }

    public const float RuptureCost = 0.2f;
    public const float CastigationCost = 0.55f;

    public override void AI()
    {
        Projectile.extraUpdates = 3;
        Projectile.timeLeft = 2;

        if (Player.HeldItem.type != ModContent.ItemType<AvatarLonginus>() || Player.CCed || Player.dead)
        {
            Projectile.Kill();
            return;
        }

        Projectile.damage = (int)Player.GetTotalDamage(DamageClass.Melee).ApplyTo(Player.HeldItem.damage);
        Player.heldProj = Projectile.whoAmI;

        throwMode = false;
        canHit = false;

        useTrail = false;
        holdTrailUpdate = false;

        Vector2 offset = Vector2.Zero;
        handPosition = Player.MountedCenter;
        float attackSpeed = Player.GetAttackSpeed(DamageClass.Melee) * (1f + Projectile.extraUpdates * 0.15f);

        if (AttackState != (int)AvatarSpearAttacks.Idle)
        {
            if (Time < 2 && Main.myPlayer == Projectile.owner)
            {
                Projectile.velocity = Player.DirectionTo(Main.MouseWorld) * 20f;
                Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
                Projectile.netUpdate = true;
            }
        }

        switch (AttackState)
        {
            default:
            case (int)AvatarSpearAttacks.Idle:

                Time = 0;

                Projectile.scale = 1f;
                Projectile.velocity = Vector2.Zero;
                float motionBob = Player.velocity.X * 0.02f - Player.velocity.Y * 0.015f * Player.direction;
                Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Player.fullRotation - MathHelper.PiOver2 + 1f * Player.direction + motionBob, 0.1f);
                Projectile.spriteDirection = Player.direction;

                Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, -MathHelper.PiOver2 * Player.direction + motionBob * 1.2f);
                Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, -0.3f * Player.direction + motionBob * 0.3f);
                handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.ThreeQuarters, -0.3f * Player.direction);

                if (Player.altFunctionUse == 1 && Player.ItemTimeIsZero)
                {
                    if (IsEmpowered && Player.GetModPlayer<AvatarSpearHeatPlayer>().ConsumeHeat(0.5f, false))
                        AttackState = (int)AvatarSpearAttacks.ThrowRupture; //replace with castigate when it actually does something
                    else
                        AttackState = (int)AvatarSpearAttacks.ThrowRupture;
                }
                else if (InUse && Player.ItemTimeIsZero)
                    AttackState = (int)AvatarSpearAttacks.RapidStabs;

                IsEmpowered = Player.GetModPlayer<AvatarSpearHeatPlayer>().Active;

                break;

            case (int)AvatarSpearAttacks.RapidStabs:

                Player.SetDummyItemTime(10);

                const int RapidWindUp = 50;
                int RapidStabCount = IsEmpowered ? 10 : 5;
                const int RapidWindDown = 50;

                int RapidStabTime = (RapidStabCount) * 13 + (int)(50 / attackSpeed);

                if (Time < RapidWindUp)
                {
                    float windProgress = Time / (RapidWindUp - 1f);

                    float wiggle = MathF.Sin(MathF.Pow(windProgress, 2f) * MathHelper.Pi) * -0.4f * Projectile.direction;
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() + wiggle, 0.9f);
                    offset = new Vector2(MathHelper.SmoothStep(0, -50, windProgress), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f - windProgress * 0.33f;

                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, Projectile.rotation - MathHelper.PiOver2);
                    handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.None, Projectile.rotation - MathHelper.PiOver2);
                }

                else if (Time < RapidWindUp + RapidStabTime)
                {
                    float windDownProgress = Utils.GetLerpValue(RapidWindDown, 0f, Time - RapidWindUp - RapidStabTime, true);
                    if (Time < RapidWindUp + RapidStabTime)
                        canHit = true;

                    float timePerStab = MathF.Ceiling(RapidStabTime / RapidStabCount);
                    float stabProgress = Utils.GetLerpValue(0, timePerStab, (Time - RapidWindUp) % timePerStab);
                    if (Time - RapidWindUp >= timePerStab * RapidStabCount)
                        stabProgress = 1f;

                    float stabCurve = Utils.GetLerpValue(0, 0.5f, stabProgress, true);

                    if (Time % timePerStab == 0)
                    {
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.StakeImpale with { Pitch = 1f, PitchVariance = 0.2f, Volume = 0.4f, MaxInstances = 0 }, Projectile.Center);

                        if (Main.myPlayer == Projectile.owner)
                        {
                            float accuracy = Utils.GetLerpValue(RapidStabTime, RapidStabTime * 0.5f, Time - RapidWindUp, true);
                            Projectile.velocity = Player.DirectionTo(Main.MouseWorld).RotatedByRandom(accuracy * 0.5f) * 15f;
                            Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
                            Projectile.netUpdate = true;
                        }
                    }
                    
                    Projectile.rotation = Projectile.velocity.ToRotation();
                    offset = new Vector2(stabCurve * 200 - 50, 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f + stabCurve * 0.5f;

                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                    int handSwingDir = (int)(Utils.GetLerpValue(0, RapidStabTime, Time - RapidWindUp, true) * RapidStabCount) % 2 > 0 ? 1 : -1;
                    float handRot = Projectile.rotation - MathHelper.PiOver2 + (1f - stabCurve) * handSwingDir * Player.direction;
                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, handRot);
                    handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, handRot);
                }
                else
                {
                    float windDownProgress = Utils.GetLerpValue(RapidWindDown / 4f, RapidWindDown, Time - RapidWindUp - RapidStabTime, true);
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
                Projectile.extraUpdates = 0;

                bool isSecondSlash = AttackState == (int)AvatarSpearAttacks.SecondSlash;

                int SlashWindUp = isSecondSlash ? 20 : 30;
                int SlashTime = (isSecondSlash ? 20 : 35) + (int)(30 / attackSpeed);
                int SlashWindDown = (isSecondSlash ? 30 : 20) + (int)(25 / attackSpeed);
                float SlashRotation = MathHelper.ToRadians(190);

                if (isSecondSlash && Time < 3) // Flip the second slash
                    Projectile.direction = Projectile.velocity.X > 0 ? -1 : 1;

                if (Time < SlashWindUp)
                {
                    float windProgress = Time / (SlashWindUp - 1f);

                    float wiggle = -MathF.Sqrt(windProgress) * SlashRotation * -Projectile.direction;

                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() + wiggle, 0.9f);
                    offset = new Vector2(MathHelper.SmoothStep(0, -30, windProgress), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f - windProgress * (1 - windProgress);

                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
                }
                else
                {
                    if (Time > SlashWindDown + 4 && Time < SlashWindDown + SlashTime + SlashWindDown / 2)
                        useTrail = true;

                    if (Time == SlashWindUp + 1)
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.StakeGraze with { Pitch = 0.2f, PitchVariance = 0.1f, Volume = 0.66f, MaxInstances = 0 }, Projectile.Center);

                    if (Time == SlashWindUp + SlashTime - 1)
                    {
                        DoShake();
                        SoundEngine.PlaySound(GennedAssets.Sounds.Common.MediumBloodSpill with { Pitch = 1f, PitchVariance = 0.1f, MaxInstances = 0 }, Projectile.Center);
                    }

                    float slashProgress = Utils.GetLerpValue(0, SlashTime, Time - SlashWindUp, true);
                    float windDown = Utils.GetLerpValue(0, SlashWindDown, Time - SlashWindUp - SlashTime, true);

                    float slashCurve = MathF.Pow(slashProgress, 1.5f);

                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                    float currentSlashRot = MathHelper.Lerp(-SlashRotation, SlashRotation * 0.1f + MathF.Pow(windDown, 4f) * 0.2f, slashCurve) * Projectile.direction;
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
                            AttackState = (int)AvatarSpearAttacks.SecondSlash;
                        else
                            AttackState = (int)AvatarSpearAttacks.RapidStabs;

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

                int HeavyWindUp = IsEmpowered ? 60 : 80;
                int HeavyThrustTime = 10 + (int)(30 / Player.GetAttackSpeed(DamageClass.Melee));
                int HeavyWindDown = 10 + (int)(20 / Player.GetAttackSpeed(DamageClass.Melee));

                if (Time < HeavyWindUp)
                {
                    float windProgress = Time / (HeavyWindUp - 1f);

                    float wiggle = (windProgress * -0.5f - 0.5f) * Projectile.direction;
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() + wiggle, windProgress);
                    offset = new Vector2(MathHelper.SmoothStep(0, -50, windProgress), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f;
                    Projectile.velocity = Player.DirectionTo(Main.MouseWorld) * 15f;

                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.PiOver2);
                    handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.PiOver2);

                    if (Time == (IsEmpowered ? 0 : HeavyWindUp / 4))
                        SoundEngine.PlaySound(GennedAssets.Sounds.Enemies.DismalLanternSway with { Pitch = 0.4f, MaxInstances = 0 }, Projectile.Center);
                }
                else
                {
                    if (Time == HeavyWindUp + 7)
                    {
                        BreakSoundBarrierParticle(1f);
                        DoShake(0.7f);
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.StakeGraze with { Pitch = 0.6f, PitchVariance = 0.2f, Volume = 0.3f, MaxInstances = 0 }, Projectile.Center);
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.StakeImpale with { Pitch = 0.2f, PitchVariance = 0.2f, Volume = 0.6f, MaxInstances = 0 }, Projectile.Center);
                    }
                    
                    canHit = true;
                    float thrustProgress = Utils.GetLerpValue(0, HeavyThrustTime, Time - HeavyWindUp, true);
                    float windDown = Utils.GetLerpValue(0, HeavyWindDown, Time - HeavyWindUp - HeavyThrustTime, true);

                    float thrustCurve = Utils.GetLerpValue(0, 0.15f, thrustProgress, true);
                    float windDownCurve = MathF.Cbrt(windDown);

                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, Player.DirectionTo(Main.MouseWorld) * 15f, 0.01f);
                        Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
                        Projectile.netUpdate = true;
                    }

                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() - (thrustCurve - 1f) * 0.3f * Projectile.direction, 0.5f - thrustCurve * 0.2f);
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

                    float pullProgress = Time / PullTime;

                    if (attackedNPC > -1 && attackedNPC < Main.npc.Length)
                    {
                        if (Main.npc[attackedNPC].active && Main.npc[attackedNPC].life > 2)
                            Projectile.Center = Main.npc[attackedNPC].Center + JavelinOffset;
                        else
                            Time = PullTime;
                    }

                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation(), 0.5f);
                    offset = new Vector2(150 + MathF.Sin(pullProgress * MathHelper.Pi) * 50, 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1.75f;
                }
                else
                {
                    canHit = true;

                    if (Time == PullTime + 1)
                    {
                        DoShake(1.5f);
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ArmJutOut with { Pitch = -0.1f, MaxInstances = 0 }, Projectile.Center);

                        // MANKIND IS DEAD
                        // BLOOD IS FUEL
                        // HELL IS FULL
                        BloodMetaball metaball = ModContent.GetInstance<BloodMetaball>();
                        for (int i = 0; i < 50; i++)
                        { 
                            Vector2 bloodSpawnPosition = Projectile.Center + new Vector2(150 * Projectile.scale + offset.X, offset.Y).RotatedBy(Projectile.rotation);
                            Vector2 bloodVelocity = (Main.rand.NextVector2Circular(8f, 8f) - Projectile.velocity) * Main.rand.NextFloat(0.2f, 1.2f);
                            metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(10f, 40f), Main.rand.NextFloat(2f));
                        }
                    }

                    float ripProgress = Utils.GetLerpValue(0, RipTime, Time - PullTime, true);
                    float twirl = MathF.Cbrt(ripProgress) * 2f * (1f - ripProgress * 0.3f) 
                        + MathHelper.TwoPi * MathHelper.SmoothStep(0f, 1f, Utils.GetLerpValue(0.3f, 1f, ripProgress, true));
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() + twirl * Projectile.direction, 0.5f - ripProgress * 0.3f);

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
                        SoundEngine.PlaySound(GennedAssets.Sounds.Common.Twinkle with { Pitch = 0f, PitchVariance = 0.2f, Volume = 0.5f, MaxInstances = 0 }, Projectile.Center);

                    HitTimer = 0;

                    Player.velocity -= Projectile.velocity.SafeNormalize(Vector2.Zero) * 0.5f;
                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                    float windProgress = Time / ThrowWindUp;

                    float wiggle = (windProgress * -0.5f - 0.5f) * Projectile.direction;
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() + wiggle, windProgress);
                    offset = new Vector2(MathHelper.SmoothStep(0, -80, windProgress), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f;
                }
                else if (Time < ThrowWindUp + ThrowTime)
                {
                    if (Time == ThrowWindUp)
                    {
                        Projectile.Center = Player.MountedCenter;
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.StakeGraze with { Pitch = -0.1f, PitchVariance = 0.2f, MaxInstances = 0 }, Projectile.Center);
                        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.RiftOpen with { Pitch = 1f, PitchVariance = 0.2f, Volume = 0.3f, MaxInstances = 0 }, Projectile.Center);
                    }

                    if (Time == ThrowWindUp + 2)
                        BreakSoundBarrierParticle(-0.5f);

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
                        Main.SetCameraLerp(0.1f, 10);

                    canHit = true;
                    throwMode = true;

                    if (HitTimer <= 0 && !Collision.SolidCollision(Projectile.Center - new Vector2(20) + Projectile.velocity.SafeNormalize(Vector2.Zero) * 20, 40, 40))
                        Player.Center = Vector2.Lerp(Player.Center, Projectile.Center, 0.3f);
                    else
                        Projectile.Center = Vector2.Lerp(Projectile.Center, Player.MountedCenter, MathF.Pow(Utils.GetLerpValue(0, TPTime, Time - ThrowWindUp - ThrowTime, true), 5f) * 0.2f);
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

                bool validNPC = attackedNPC > -1 && attackedNPC < Main.npc.Length;
                if (validNPC)
                {
                    if (Main.npc[attackedNPC].active && Main.npc[attackedNPC].life > 2)
                        Projectile.Center = Main.npc[attackedNPC].Center + JavelinOffset;
                    else
                        Time = AvatarSpearRupture.FlickerTime + AvatarSpearRupture.ExplosionTime;
                }

                if (!validNPC || Time > AvatarSpearRupture.FlickerTime + AvatarSpearRupture.ExplosionTime)
                {
                    AttackState = (int)AvatarSpearAttacks.Idle;
                    Time = 0;
                }

                Time++;

                break;

        }

        handPosition = Player.RotatedRelativePoint(handPosition);

        if (!throwMode)
            Projectile.Center = handPosition + offset - Projectile.velocity;

        UpdateTrail();

        if (canHit)
        {
            for (int i = 0; i < 3; i++)
            {
                int size = (int)(70 * MathF.Exp(-i));
                Projectile.EmitEnchantmentVisualsAt(Projectile.Center + new Vector2((120 + 40 * i) * Projectile.scale, 0).RotatedBy(Projectile.rotation) - new Vector2(size / 2), size, size);
            }
        }

        Projectile.localAI[0]++;
        if (Projectile.localAI[0] > 240)
            Projectile.localAI[0] = 0;

        Lighting.AddLight(Player.Center, Color.DarkRed.ToVector3());

        if (HitTimer > 0)
            HitTimer--; 

        if (Main.myPlayer == Projectile.owner)
        {
            float heat = Player.GetModPlayer<AvatarSpearHeatPlayer>().Heat;
            if (heat > 0f)
                WeaponBar.DisplayBar(Color.DarkRed, Color.Red, heat, style: 1, BarOffset: IsEmpowered ? Main.rand.NextVector2Circular(2, 2) : Vector2.Zero);
        }
    }

    public void SetTrailToCurrent()
    {
        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            Projectile.oldPos[i] = Projectile.Center;
            Projectile.oldRot[i] = Projectile.rotation;
        }
    }

    public void UpdateTrail()
    {
        if (Projectile.numUpdates <= 0)
        {
            Vector2 playerPosOffset = Player.position - Player.oldPosition;
            for (int i = Projectile.oldPos.Length - 1; i > 0; i--)
            {
                Projectile.oldPos[i] = Projectile.oldPos[i - 1];
                Projectile.oldRot[i] = Projectile.oldRot[i - 1];

                if (!throwMode)
                    Projectile.oldPos[i] += playerPosOffset;
            }

            if (!holdTrailUpdate)
            {
                Projectile.oldPos[0] = Projectile.Center;
                Projectile.oldRot[0] = Projectile.rotation;
            }

            if (!throwMode)
                Projectile.oldPos[0] += playerPosOffset - Player.velocity;
        }
    }

    public void HandleEmpowerment() => IsEmpowered = Player.GetModPlayer<AvatarSpearHeatPlayer>().Active;

    private void DoShake(float strength = 1f)
    {
        if (Main.myPlayer == Projectile.owner)
            ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, 7f * strength, 
                shakeDirection: Projectile.velocity.SafeNormalize(Vector2.Zero) * 2, 
                shakeStrengthDissipationIncrement: 0.7f - strength * 0.1f);
    }

    private void BreakSoundBarrierParticle(float speed)
    {
        Vector2 soundBarrierVelocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * speed;
        Vector2 soundBarrierPosition = Projectile.Center + new Vector2(100, 0).RotatedBy(Projectile.rotation);

        SoundBarrierParticle particle = SoundBarrierParticle.pool.RequestParticle();
        particle.Prepare(soundBarrierPosition, -soundBarrierVelocity * 30f + Player.velocity, Projectile.rotation, Color.DarkRed with { A = 200 }, 0.5f);
        SoundBarrierParticle largerParticle = SoundBarrierParticle.pool.RequestParticle();
        largerParticle.Prepare(soundBarrierPosition, -soundBarrierVelocity * 50f + Player.velocity, Projectile.rotation, Color.DarkRed with { A = 200 }, 1f);

        ParticleEngine.ShaderParticles.Add(particle);
        ParticleEngine.ShaderParticles.Add(largerParticle);

    }

    public int attackedNPC;

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (IsEmpowered)
            modifiers.FinalDamage *= 1.33f;

        if (AttackState == (int)AvatarSpearAttacks.ThrowRupture)
            modifiers.FinalDamage /= 3;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        OnHitEffects(target.Center, target.velocity, target.CanBeChasedBy(this));

        int ruptureType = ModContent.ProjectileType<AvatarSpearRupture>();
        if (AttackState == (int)AvatarSpearAttacks.ThrowRupture && Player.ownedProjectileCounts[ruptureType] < 1)
        {
            if (Player.GetModPlayer<AvatarSpearHeatPlayer>().ConsumeHeat(RuptureCost))
            {
                AttackState = (int)AvatarSpearAttacks.Rupture;
                Time = 0;

                Vector2 bombVelocity = Player.DirectionTo(target.Center).SafeNormalize(Vector2.Zero) * 20;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Player.MountedCenter, bombVelocity, ruptureType, Projectile.damage * 2, 0.5f, Player.whoAmI, ai1: target.whoAmI + 1);
            }
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
        float addHeat = 0.01f;
        bool forceAddHeat = false;

        switch (AttackState)
        {
            case (int)AvatarSpearAttacks.WhipSlash:
            case (int)AvatarSpearAttacks.SecondSlash: addHeat = 0.1f; break;
            case (int)AvatarSpearAttacks.HeavyStab: addHeat = IsEmpowered ? 0.33f : 0.02f; break;
            case (int)AvatarSpearAttacks.RipOut: addHeat = IsEmpowered ? 0.33f : 0f; break;
            case (int)AvatarSpearAttacks.ThrowRupture: addHeat = 0.002f; break;
        }

        if (IsEmpowered)
            addHeat *= 0.15f;

        if ((HitTimer <= 0 || forceAddHeat) && canAddHeat)
            Player.GetModPlayer<AvatarSpearHeatPlayer>().AddHeat(addHeat);

        DoShake(0.2f);

        BloodMetaball metaball = ModContent.GetInstance<BloodMetaball>();
        for (int i = 0; i < (IsEmpowered ? 6 : 2); i++)
        {
            Vector2 bloodSpawnPosition = targetPosition + Main.rand.NextVector2Circular(24, 24);
            Vector2 bloodVelocity = (Main.rand.NextVector2Circular(5f, 5f) + Projectile.velocity) * Main.rand.NextFloat(0.2f, 1f);
            metaball.CreateParticle(bloodSpawnPosition, bloodVelocity, Main.rand.NextFloat(10f, 40f), Main.rand.NextFloat(2f));
        }

        for (int i = 0; i < Main.rand.Next(1, 5); i++)
        {
            Vector2 lightningPos = targetPosition + Main.rand.NextVector2Circular(24, 24);

            HeatLightning particle = HeatLightning.pool.RequestParticle();
            particle.Prepare(lightningPos, targetVelocity + Main.rand.NextVector2Circular(10, 10), Main.rand.NextFloat(-1f, 1f), 10 + i * 4, Main.rand.NextFloat(0.5f, 1.5f));
            ParticleEngine.Particles.Add(particle);
        }
    }

    public override bool? CanCutTiles() => canHit;

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
        hitbox.Inflate(200, 200);
        hitbox.Location += (new Vector2(180 * Projectile.scale, 0).RotatedBy(Projectile.rotation)).ToPoint();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (canHit)
        {
            float dist = 200f;
            if (AttackState == (int)AvatarSpearAttacks.ThrowRupture)
                dist = 100f;

            Vector2 offset = new Vector2(dist * Projectile.scale * (IsEmpowered ? 1.5f : 1f), 0).RotatedBy(Projectile.rotation);
            float _ = 0;
            return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center - offset / 2, Projectile.Center + offset, 120f, ref _);
        }

        return false;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(IsEmpowered);
        writer.Write(HitTimer);
        writer.WriteVector2(JavelinOffset);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        IsEmpowered = reader.ReadBoolean();
        HitTimer = reader.Read();
        JavelinOffset = reader.ReadVector2();
    }

    private bool useTrail;
    private bool holdTrailUpdate;

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Rectangle frame = texture.Frame(1, 2, 0, IsEmpowered ? 1 : 0);
        Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;

        float scale = Projectile.scale;
        int direction = Projectile.spriteDirection;
        SpriteEffects flipEffect = direction > 0 ? 0 : SpriteEffects.FlipVertically;
        int gripDistance = IsEmpowered ? 30 : 60;
        Vector2 origin = new Vector2(frame.Width / 2 - gripDistance, frame.Height / 2 + (gripDistance - 4) * Player.gravDir * direction);
        Vector2 spearHeadPosition = Projectile.Center + new Vector2(90 * Projectile.scale, 0).RotatedBy(Projectile.rotation);

        float glowAmt = (IsEmpowered ? 0.6f : 0.4f) + MathF.Cbrt(Math.Clamp(HitTimer / 5f, 0f, 1f)) * 0.4f;

        if (useTrail)
            DrawTrail();

        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            Color color = Color.Lerp(Color.Red with { A = 100 } * 0.5f, Color.Blue * 0.15f, i / 9f);
            Main.EntitySpriteDraw(texture, Projectile.oldPos[i] - Main.screenPosition, frame, color * 0.5f, Projectile.oldRot[i] + MathHelper.PiOver4 * direction, origin, scale, flipEffect, 0);
        }

        Vector2 highlightOffset = new Vector2(3, 0).RotatedBy(Main.GlobalTimeWrappedHourly * 2 * MathHelper.TwoPi);
        Main.EntitySpriteDraw(texture, Projectile.Center + highlightOffset - Main.screenPosition, frame, Color.Red with { A = 100 } * 0.5f, Projectile.rotation + MathHelper.PiOver4 * direction, origin, scale, flipEffect, 0);
        Main.EntitySpriteDraw(texture, Projectile.Center - highlightOffset - Main.screenPosition, frame, Color.RoyalBlue with { A = 100 } * 0.5f, Projectile.rotation + MathHelper.PiOver4 * direction, origin, scale, flipEffect, 0);

        Main.EntitySpriteDraw(glow, spearHeadPosition - Main.screenPosition, glow.Frame(), Color.Red with { A = 120 } * (glowAmt + 0.2f), Projectile.rotation, glow.Size() * 0.5f, scale * 0.15f, flipEffect, 0);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation + MathHelper.PiOver4 * direction, origin, scale, flipEffect, 0);
        Main.EntitySpriteDraw(glow, spearHeadPosition - Main.screenPosition, glow.Frame(), Color.DarkRed with { A = 0 } * glowAmt, Projectile.rotation, glow.Size() * 0.5f, scale * 0.4f, flipEffect, 0);

        return false;
    }

    private void DrawTrail()
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

        ManagedShader trailShader = ShaderManager.GetShader("HeavenlyArsenal.LonginusSlash");
        trailShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 0, SamplerState.LinearWrap);

        Vector2[] positions = new Vector2[Projectile.oldPos.Length];
        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            positions[i] = Projectile.oldPos[i] + new Vector2(120 * Projectile.scale, 0).RotatedBy(Projectile.oldRot[i]);
        }

        PrimitiveRenderer.RenderTrail(Projectile.oldPos, new PrimitiveSettings(TrailWidthFunction, TrailColorFunction, Shader: trailShader), Projectile.oldPos.Length * 2);
        
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
    }

    private float TrailWidthFunction(float p) => 100 * Projectile.direction;
    private Color TrailColorFunction(float p) => Color.Lerp(Color.Red, Color.MidnightBlue * 0.5f, p);
}
