using CalamityMod;
using CalamityMod.Items.Weapons.Melee;
using HeavenlyArsenal.Content.Buffs;
using Luminance.Assets;
using Luminance.Common.Easings;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Luminance.Core.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.DataStructures;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.Physics.VerletIntergration;
using NoxusBoss.Core.Utilities;
using ReLogic.Content;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;

public class AntishadowAssassin : ModProjectile
{
    public enum AssassinState
    {
        StayNearOwner,
        DissipateToHuntTarget,
        WaitBeforeSlashingTarget,
        PerformDirectionalSlices,
        SliceTargetRepeatedly,
        PostSliceDash,
        EmergeNearTarget,

        Leave
    }

    private readonly List<SlotId> attachedSounds = [];

    /// <summary>
    /// How long this assassin has existed for, in frames.
    /// </summary>
    public int ExistenceTimer
    {
        get;
        set;
    }

    /// <summary>
    /// The state of this assassin.
    /// </summary>
    public AssassinState State
    {
        get;
        set;
    }

    /// <summary>
    /// The type of mask used by this assassin.
    /// </summary>
    public int MaskVariant
    {
        get;
        set;
    }

    /// <summary>
    /// The index of the target of this assassin.
    /// </summary>
    public int TargetIndex
    {
        get;
        set;
    } = -1;

    /// <summary>
    /// The intensity of blur effects on this assassin.
    /// </summary>
    public float BlurIntensity
    {
        get;
        set;
    }

    /// <summary>
    /// The amount by which the assassin is rotating forward for a bow.
    /// </summary>
    public float BowRotation
    {
        get;
        set;
    }

    /// <summary>
    /// The opacity of the arm outline.
    /// </summary>
    public float ArmOutlineOpacity
    {
        get;
        set;
    }

    /// <summary>
    /// The amount by which this assassin has begun to disappear.
    /// </summary>
    public float DisappearanceInterpolant
    {
        get;
        set;
    }

    /// <summary>
    /// The amount by which the katana is unsheathed.
    /// </summary>
    public float KatanaUnsheathInterpolant
    {
        get;
        set;
    } = 1f;

    /// <summary>
    /// The amount by which this assassin's <see cref="AttackLoop"/> sound should be activated and noticeable.
    /// </summary>
    public float AttackLoopActivationInterpolant
    {
        get;
        set;
    }

    /// <summary>
    /// The starting position of this assassin during their dash.
    /// </summary>
    public Vector2 DashStart
    {
        get;
        set;
    }

    /// <summary>
    /// The ambient loop sound instance.
    /// </summary>
    public LoopedSoundInstance AmbientLoop
    {
        get;
        set;
    }

    /// <summary>
    /// The attacking loop sound instance.
    /// </summary>
    public LoopedSoundInstance AttackLoop
    {
        get;
        set;
    }

    /// <summary>
    /// The position of this assassin's mask.
    /// </summary>
    public Vector2 MaskPosition => Projectile.Center + new Vector2(Projectile.spriteDirection * -1f, -73f).RotatedBy(Projectile.rotation + BowRotation * Projectile.spriteDirection) * Projectile.scale;

    /// <summary>
    /// The owner of this assassin.
    /// </summary>
    public Player Owner => Main.player[Projectile.owner];

    /// <summary>
    /// The rope for the assassin's first rope.
    /// </summary>
    public VerletSimulatedRope BeadRopeA
    {
        get;
        set;
    }

    /// <summary>
    /// The rope for the assassin's first rope.
    /// </summary>
    public VerletSimulatedRope BeadRopeB
    {
        get;
        set;
    }

    /// <summary>
    /// The rotation of the assassin's left arm.
    /// </summary>
    public ref float LeftArmRotation => ref Projectile.localAI[0];

    /// <summary>
    /// The rotation of the assassin's right arm.
    /// </summary>
    public ref float RightArmRotation => ref Projectile.localAI[1];

    /// <summary>
    /// The rotation of the assassin's katanas.
    /// </summary>
    public ref float KatanaRotation => ref Projectile.localAI[2];

    /// <summary>
    /// The movement interpolant of this assassin during its stay near owner state.
    /// </summary>
    public ref float MovementInterpolant => ref Projectile.ai[0];

    /// <summary>
    /// How long this assassin has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[1];

    /// <summary>
    /// An optional number used for the purposes of per-state memorization. Usage varies based on time.
    /// </summary>
    public ref float AIData => ref Projectile.ai[2];

    /// <summary>
    /// The amount of mask variants that exist for this assassin.
    /// </summary>
    public static int TotalMasks => 5;

    /// <summary>
    /// The maximum search range this assassin can examine.
    /// </summary>
    public static float TargetingRange => 756f;

    /// <summary>
    /// The render target that holds the contents of this assassin's body and arms.
    /// </summary>
    public static InstancedRequestableTarget BodyTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// The render target that holds the contents of this assassin's arm outlines.
    /// </summary>
    public static InstancedRequestableTarget ArmOutlineTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// The render target that holds the contents of the entire assassin.
    /// </summary>
    public static InstancedRequestableTarget ResultsTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// The body texture asset wrapper.
    /// </summary>
    public static Asset<Texture2D> BodyTexture
    {
        get;
        private set;
    }

    /// <summary>
    /// The arm texture asset wrapper.
    /// </summary>
    public static Asset<Texture2D> ArmTexture
    {
        get;
        private set;
    }

    /// <summary>
    /// The arm outline texture asset wrapper.
    /// </summary>
    public static Asset<Texture2D> ArmOutlineTexture
    {
        get;
        private set;
    }

    /// <summary>
    /// The arm texture asset wrapper used when bowing.
    /// </summary>
    public static Asset<Texture2D> ArmBowTexture
    {
        get;
        private set;
    }

    /// <summary>
    /// The arm outline texture asset wrapper used when bowing.
    /// </summary>
    public static Asset<Texture2D> ArmBowOutlineTexture
    {
        get;
        private set;
    }

    /// <summary>
    /// The kasa texture asset wrapper.
    /// </summary>
    public static Asset<Texture2D> KasaTexture
    {
        get;
        private set;
    }

    /// <summary>
    /// The legs texture asset wrapper.
    /// </summary>
    public static Asset<Texture2D> LegsTexture
    {
        get;
        private set;
    }

    /// <summary>
    /// The mask texture asset wrappers.
    /// </summary>
    public static Asset<Texture2D>[] AntishadowAssassinMasks
    {
        get;
        private set;
    }

    /// <summary>
    /// The fifth assassin mask texture asset wrapper.
    /// </summary>
    public static Asset<Texture2D> AntishadowAssassinMask5
    {
        get;
        private set;
    }

    /// <summary>
    /// The chime sound played by this assassin at random.
    /// </summary>
    public static readonly SoundStyle ChimeSound = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Summoner/AntishadowBreatheChime", 2);

    /// <summary>
    /// The sound used when this assassin begins to leave due to being desummoned.
    /// </summary>
    public static readonly SoundStyle LeaveSound = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Summoner/AntishadowAssassinLeave");

    /// <summary>
    /// The looped sound used in an ambient context.
    /// </summary>
    public static readonly SoundStyle AntishadowAmbientLoopSound = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Summoner/AntishadowAmbientLoop");

    /// <summary>
    /// The looped sound used when attacking.
    /// </summary>
    public static readonly SoundStyle AntishadowAttackLoopSound = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Summoner/AntishadowAttackLoop");

    private void PerformStupidAssetBoilerplateLoading()
    {
        AntishadowAssassinMasks = new Asset<Texture2D>[TotalMasks];
        for (int i = 1; i <= TotalMasks; i++)
            AntishadowAssassinMasks[i - 1] = ModContent.Request<Texture2D>($"HeavenlyArsenal/Content/Gores/AntishadowAssassinMask{i}");

        string texturePrefix = $"{Mod.Name}/Content/Items/Weapons/Summon/AntishadowAssassin";
        BodyTexture = ModContent.Request<Texture2D>($"{texturePrefix}/AntishadowAssassin");
        ArmTexture = ModContent.Request<Texture2D>($"{texturePrefix}/AntishadowAssassinArm");
        ArmOutlineTexture = ModContent.Request<Texture2D>($"{texturePrefix}/AntishadowAssassinArm_Outline");
        ArmBowTexture = ModContent.Request<Texture2D>($"{texturePrefix}/AntishadowAssassinArmBow");
        ArmBowOutlineTexture = ModContent.Request<Texture2D>($"{texturePrefix}/AntishadowAssassinArmBow_Outline");
        KasaTexture = ModContent.Request<Texture2D>($"{texturePrefix}/AntishadowAssassinKasa");
        LegsTexture = ModContent.Request<Texture2D>($"{texturePrefix}/AntishadowAssassinLegs");
    }

    public override void SetStaticDefaults()
    {
        BodyTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(BodyTarget);
        ArmOutlineTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(ArmOutlineTarget);
        ResultsTarget = new InstancedRequestableTarget();
        Main.ContentThatNeedsRenderTargets.Add(ResultsTarget);

        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = 12;
        ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
        ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;

        PerformStupidAssetBoilerplateLoading();
    }

    public override void SetDefaults()
    {
        Projectile.width = 74;
        Projectile.height = 172;
        Projectile.netImportant = true;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.minionSlots = AntishadowBead.MinionSlotRequirement;
        Projectile.timeLeft = 90000;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Summon;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write((int)State);
        writer.Write(TargetIndex);
        writer.WriteVector2(DashStart);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        State = (AssassinState)reader.ReadInt32();
        TargetIndex = reader.ReadInt32();
        DashStart = reader.ReadVector2();
    }

    public override void AI()
    {
        // Initialize beads if necessary.
        if (BeadRopeA is null || BeadRopeB is null)
        {
            BeadRopeA = new VerletSimulatedRope(Projectile.Center, Vector2.Zero, 6, 12.5f);
            BeadRopeB = new VerletSimulatedRope(Projectile.Center, Vector2.Zero, 6, 18.5f);
        }
        AmbientLoop ??= LoopedSoundManager.CreateNew(AntishadowAmbientLoopSound, () => !Projectile.active);
        AttackLoop ??= LoopedSoundManager.CreateNew(AntishadowAttackLoopSound, () => !Projectile.active);

        PreUpdateResets();
        if (State != AssassinState.Leave)
            HandleMinionBuffs();

        switch (State)
        {
            case AssassinState.StayNearOwner:
                DoBehavior_StayNearOwner();
                break;
            case AssassinState.DissipateToHuntTarget:
                DoBehavior_DissipateToHuntTarget();
                break;
            case AssassinState.WaitBeforeSlashingTarget:
                DoBehavior_WaitBeforeSlashingTarget();
                break;
            case AssassinState.PerformDirectionalSlices:
                DoBehavior_PerformDirectionalSlices();
                break;
            case AssassinState.SliceTargetRepeatedly:
                DoBehavior_SliceTargetRepeatedly();
                break;
            case AssassinState.PostSliceDash:
                DoBehavior_PostSliceDash();
                break;
            case AssassinState.Leave:
                DoBehavior_Leave();
                break;
        }

        ResetVisuals();
        MoveBeadRops();
        CreateFootSmoke();
        CreateIdleSounds();
        UpdateLoopedSounds();
        KeepSoundsAttached();

        Time++;
        ExistenceTimer++;
    }

    private void DoBehavior_StayNearOwner()
    {
        HandleRepositionMotion(Owner.Center + new Vector2(-Owner.direction * 60f, -40f));

        // Fly up and down by default.
        // This gets functionally nullified if a reposition is ongoing.
        float sinusoidalSpeed = MathF.Cos(MathHelper.TwoPi * Time / 150f) * 0.33f;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.UnitY * sinusoidalSpeed, 0.1f);
        Projectile.Opacity = Projectile.Opacity.StepTowards(1f, 0.2f);
        Projectile.spriteDirection = (int)Projectile.HorizontalDirectionTo(Owner.Center);

        // Search for targets.
        int targetIndex = FindPotentialTarget();
        if (targetIndex >= 0)
        {
            TargetIndex = targetIndex;
            SwitchState(AssassinState.DissipateToHuntTarget);
        }
    }

    private void DoBehavior_DissipateToHuntTarget()
    {
        if (!EnsureTargetIsAlive())
            return;

        NPC target = Main.npc[TargetIndex];
        if (Time == 1f)
        {
            if (Projectile.WithinRange(target.Center, 300f))
                SwitchState(AssassinState.PerformDirectionalSlices);

            DashStart = Projectile.Center;
            MovementInterpolant = 1f;
            Projectile.netUpdate = true;
        }

        HandleRepositionMotion(target.Center + target.SafeDirectionTo(Projectile.Center) * 75f);

        float armRotation = 0.99f;
        if (Projectile.spriteDirection == 1)
            armRotation *= -1f;

        LeftArmRotation = LeftArmRotation.AngleLerp(armRotation, 0.3f);
        RightArmRotation = RightArmRotation.AngleLerp(-armRotation, 0.3f);
        KatanaRotation = KatanaRotation.AngleLerp(1.3f, 0.56f);

        // Look at the target.
        Projectile.spriteDirection = (int)Projectile.HorizontalDirectionTo(target.Center);

        if (Time >= 1f && MovementInterpolant <= 0.01f)
            SwitchState(AssassinState.WaitBeforeSlashingTarget);
    }

    private void DoBehavior_WaitBeforeSlashingTarget()
    {
        // Vanish.
        Projectile.Opacity = MathHelper.SmoothStep(0f, 1f, LumUtils.InverseLerp(10f, 0f, Time));
        if (Time >= 10f)
            SwitchState(AssassinState.PerformDirectionalSlices);
    }

    private void DoBehavior_PerformDirectionalSlices()
    {
        if (!EnsureTargetIsAlive())
        {
            TransitionToPostDashSlash();
            Projectile.velocity = (Projectile.position - Projectile.oldPosition).SafeNormalize(Vector2.Zero) * 180f;
            return;
        }

        // Go FAST.
        Projectile.MaxUpdates = 2;

        int dashCount = 11;
        NPC target = Main.npc[TargetIndex];
        ref float dashCounter = ref AIData;
        int dashDuration = Utils.Clamp(20 - (int)dashCounter * 3, 5, 1000);
        int dashCycleHit = dashCounter == 0f ? 1 : (int)(dashDuration * 0.5f); // Done to ensure that the slash always plays even if the target before the dash duration midway point is reached.
        if (Time == dashCycleHit)
        {
            SoundEngine.PlaySound((target.Organic() ? Murasama.OrganicHit : Murasama.InorganicHit) with { MaxInstances = 9 });
            ScreenShakeSystem.StartShakeAtPoint(target.Center, 3.6f);
        }

        float dashRadius = MathHelper.Lerp(401f, 200f, dashCounter / (dashCount - 1f));

        if (Time == 1f)
        {
            float teleportOffset = Main.rand.NextFloat(300f, 500f);
            Projectile.Center = target.Center + Main.rand.NextVector2Unit() * teleportOffset;
            DashStart = Projectile.Center;
            Projectile.velocity = Projectile.SafeDirectionTo(target.Center);
            Projectile.netUpdate = true;
        }

        // Dash through the target.
        float dashCompletion = LumUtils.InverseLerp(0.1f, 0.9f, Time / dashDuration);
        float easedDashInterpolant = EasingCurves.Quadratic.Evaluate(EasingType.InOut, dashCompletion);
        Vector2 dashDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY);
        Vector2 start = target.Center - dashDirection * dashRadius;
        Vector2 end = target.Center + dashDirection * dashRadius;
        Projectile.Center = Vector2.Lerp(start, end, easedDashInterpolant);

        int slashID = ModContent.ProjectileType<AntishadowUnidirectionalAssassinSlash>();
        if (Main.myPlayer == Projectile.owner)
        {
            float slashMaxOffset = 100f;
            Vector2 slashOffset = Main.rand.NextVector2Circular(slashMaxOffset, slashMaxOffset);
            Vector2 slashSpawnPosition = target.Center + slashOffset;

            int slash = Projectile.NewProjectile(Projectile.GetSource_FromThis(), slashSpawnPosition, Vector2.Zero, slashID, Projectile.damage, Projectile.knockBack, Projectile.owner, dashCounter, slashOffset.Length(), slashOffset.ToRotation());
            Main.projectile[slash].originalDamage = Projectile.originalDamage;
            Main.projectile[slash].netUpdate = true;

            slashMaxOffset = 50f;
            slashID = ModContent.ProjectileType<AntishadowAssassinSlash>();
            float angleX = Main.rand.NextFloatDirection() * 1.1f;
            float angleY = Main.rand.NextFloatDirection() * 1.1f;
            slashSpawnPosition = Vector2.Lerp(Projectile.Center, target.Center, 0.8f) + Main.rand.NextVector2Circular(slashMaxOffset, slashMaxOffset);

            slash = Projectile.NewProjectile(Projectile.GetSource_FromThis(), slashSpawnPosition, Vector2.Zero, slashID, Projectile.damage, Projectile.knockBack, Projectile.owner, angleX, angleY, 0.56f);
            Main.projectile[slash].originalDamage = Projectile.originalDamage;
            Main.projectile[slash].netUpdate = true;
        }

        // Keep all relevant slashes attached to the assassin.
        foreach (Projectile slash in Main.ActiveProjectiles)
        {
            Vector2 bend = Projectile.velocity.RotatedBy(MathHelper.PiOver2) * LumUtils.Convert01To010(dashCompletion) * 100f;
            if (slash.owner == Projectile.owner && slash.ai[0] == dashCounter && slash.type == slashID && dashCompletion < 0.9f)
            {
                float bendDirection = (slash.identity % 2 == 0).ToDirectionInt();
                Vector2 slashOffset = slash.ai[1] * slash.ai[2].ToRotationVector2() + bend * bendDirection;
                slash.Center = Projectile.Center + slashOffset;
            }
        }
        CreateMotionVisuals();

        AttackLoopActivationInterpolant = MathHelper.Lerp(AttackLoopActivationInterpolant, 1f, 0.3f);

        if (Time >= dashDuration)
        {
            Time = 0f;
            Projectile.netUpdate = true;

            dashCounter++;
            if (dashCounter >= dashCount)
                SwitchState(AssassinState.SliceTargetRepeatedly);
        }
    }

    private void DoBehavior_SliceTargetRepeatedly()
    {
        if (!EnsureTargetIsAlive())
        {
            TransitionToPostDashSlash();
            return;
        }

        int slashTime = 120;
        NPC target = Main.npc[TargetIndex];
        if (Time <= slashTime)
        {
            if (Time % 4f == 0f)
                SoundEngine.PlaySound((target.Organic() ? Murasama.OrganicHit : Murasama.InorganicHit) with { MaxInstances = 16 });

            Projectile.Center = target.Center;
            Projectile.velocity = target.velocity.SafeNormalize((target.position - target.oldPosition).SafeNormalize(Vector2.UnitX * Projectile.spriteDirection));
            Projectile.spriteDirection = Projectile.velocity.X.NonZeroSign();
            DashStart = Projectile.Center;
            ScreenShakeSystem.StartShakeAtPoint(target.Center, 2f);

            if (Main.myPlayer == Projectile.owner)
            {
                float slashMaxOffset = 50f;
                for (int i = 0; i < 5; i++)
                {
                    float angleX = Main.rand.NextFloatDirection() * 1.2f;
                    float angleY = Main.rand.NextFloatDirection() * 1.2f;
                    Vector2 slashSpawnPosition = target.Center + Main.rand.NextVector2Circular(slashMaxOffset, slashMaxOffset);

                    int slashID = ModContent.ProjectileType<AntishadowAssassinSlash>();
                    int slash = Projectile.NewProjectile(Projectile.GetSource_FromThis(), slashSpawnPosition, Vector2.Zero, slashID, Projectile.damage, Projectile.knockBack, Projectile.owner, angleX, angleY, 1.15f);
                    Main.projectile[slash].originalDamage = Projectile.originalDamage;
                    Main.projectile[slash].netUpdate = true;
                }
            }
        }

        AttackLoopActivationInterpolant = MathHelper.Lerp(AttackLoopActivationInterpolant, 1f, 0.3f);
        Projectile.Opacity = LumUtils.InverseLerp(slashTime, slashTime + 15f, Time);
        if (Time >= slashTime + 15f)
            SwitchState(AssassinState.StayNearOwner);
    }

    private void DoBehavior_PostSliceDash()
    {
        int dashDuration = 32;

        Projectile.scale = Projectile.scale.StepTowards(1f, 0.3f);
        Projectile.velocity *= 0.685f;
        Projectile.Opacity = Projectile.Opacity.StepTowards(1f, 0.3f);

        float armRepositionInterpolant = LumUtils.Saturate(Time / dashDuration);
        float armRotation = MathHelper.SmoothStep(0.99f, 0.15f, MathF.Pow(armRepositionInterpolant, 0.4f));
        if (Projectile.spriteDirection == 1)
            armRotation *= -1f;

        LeftArmRotation = LeftArmRotation.AngleLerp(-armRotation, 0.3f);
        RightArmRotation = RightArmRotation.AngleLerp(armRotation, 0.3f);
        KatanaRotation = 0f;

        if (Time >= dashDuration)
            SwitchState(AssassinState.StayNearOwner);
    }

    private void DoBehavior_Leave()
    {
        int bowDelay = 14;
        int bowTime = 56;
        int disappearDelay = 60;
        int disappearTime = 15;

        if (Time == 1f)
            SoundEngine.PlaySound(LeaveSound, Projectile.Center).WithVolumeBoost(0.45f);

        float bowInterpolant = LumUtils.InverseLerp(0f, bowTime, Time - bowDelay);
        float angleReorientInterpolant = LumUtils.InverseLerp(0f, 0.2f, bowInterpolant) * 0.8f;
        BowRotation = MathHelper.SmoothStep(0f, MathHelper.PiOver4, EasingCurves.Quadratic.Evaluate(EasingType.InOut, bowInterpolant));

        LeftArmRotation = 0.15f.AngleLerp(BowRotation * 0.6f, angleReorientInterpolant);
        RightArmRotation = (-0.15f).AngleLerp(BowRotation * 1.1f, angleReorientInterpolant);
        KatanaRotation = bowInterpolant;
        KatanaUnsheathInterpolant = LumUtils.InverseLerp(bowDelay, 0f, Time);
        DisappearanceInterpolant = LumUtils.InverseLerp(0f, disappearTime, Time - bowDelay - bowTime - disappearDelay);
        ArmOutlineOpacity = LumUtils.InverseLerp(0f, 0.25f, bowInterpolant);

        Projectile.scale = 1f;

        // Create fre particles when disappearing.
        for (int i = 0; i < 5; i++)
        {
            if (Main.rand.NextBool(DisappearanceInterpolant))
            {
                int fireBrightness = Main.rand.Next(20);
                Color fireColor = new Color(fireBrightness, fireBrightness, fireBrightness);
                if (i % 6 == 0)
                    fireColor = new Color(174, 0, Main.rand.Next(16), 0);

                Vector2 position = Projectile.Center + Main.rand.NextVector2Circular(60f, 60f);
                AntishadowFireParticleSystemManager.ParticleSystem.CreateNew(position, Main.rand.NextVector2Circular(17f, 17f), Vector2.One * Main.rand.NextFloat(30f, 125f), fireColor);
            }
        }

        // Exist in stasis, and free up the minion slots occupied by this assassin.
        Projectile.minionSlots = 0f;
        Projectile.timeLeft = 2;

        if (DisappearanceInterpolant >= 1f)
            Projectile.Kill();
    }

    private int FindPotentialTarget()
    {
        List<NPC> potentialTargets = new List<NPC>(Main.maxNPCs);
        foreach (NPC target in Main.ActiveNPCs)
        {
            bool inRange = Projectile.WithinRange(target.Center, TargetingRange) || Owner.WithinRange(target.Center, TargetingRange);
            if (!inRange || !target.CanBeChasedBy())
                continue;

            potentialTargets.Add(target);
        }

        // MathHelper.Pick the closest valid target to the owner, so that the assassin doesn't go flying off hunting enemies offscreen since they're within its
        // detection zone.
        if (potentialTargets.Count >= 1)
            return potentialTargets.OrderBy(p => p.DistanceSQ(Owner.Center)).First().whoAmI;

        return -1;
    }

    private void TransitionToPostDashSlash()
    {
        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 175f;
        Projectile.spriteDirection = Projectile.velocity.X.NonZeroSign();
        SwitchState(AssassinState.PostSliceDash);
    }

    private bool EnsureTargetIsAlive()
    {
        if (TargetIndex <= -1 || TargetIndex >= Main.maxNPCs || !Main.npc[TargetIndex].active || !Main.npc[TargetIndex].CanBeChasedBy())
        {
            // There is currently no valid target.
            // Check for a new one. If no new one can be found, then this returns false and targeting ceases.
            int targetIndex = FindPotentialTarget();
            if (targetIndex <= -1 || !Main.npc[targetIndex].active)
            {
                SwitchState(AssassinState.StayNearOwner);
                TargetIndex = -1;
                return false;
            }

            TargetIndex = targetIndex;
            return true;
        }

        return true;
    }

    /// <summary>
    /// Switches this assassin's state to a different state of choice.
    /// </summary>
    private void SwitchState(AssassinState state)
    {
        AIData = 0f;
        Time = 0f;
        State = state;
        Projectile.netUpdate = true;
    }

    /// <summary>
    /// Handles pre-update reset effects, such as resetting MaxUpdates.
    /// </summary>
    private void PreUpdateResets()
    {
        Projectile.MaxUpdates = 1;
        KatanaUnsheathInterpolant = 1f;
        ArmOutlineOpacity = 0f;
        AttackLoopActivationInterpolant = AttackLoopActivationInterpolant.StepTowards(0f, 0.02f);
    }

    /// <summary>
    /// Resets visuals for this assassin, such rotations.
    /// </summary>
    private void ResetVisuals()
    {
        float idealArmRotation = 0.15f;

        LeftArmRotation = LeftArmRotation.AngleLerp(idealArmRotation, 0.09f);
        RightArmRotation = RightArmRotation.AngleLerp(-idealArmRotation, 0.09f);
        KatanaRotation = KatanaRotation.AngleLerp(0f, 0.08f);

        // Rotate based on current horizontal speed.
        float idealRotation = Math.Clamp((Projectile.position - Projectile.oldPosition).X * 0.004f, -0.5f, 0.5f);
        Projectile.rotation = Projectile.rotation.AngleLerp(idealRotation, 0.3f);

        Projectile.scale = Projectile.scale.StepTowards(1f, 0.01f);

        float idealBlur = LumUtils.InverseLerp(18f, 75f, (Projectile.position - Projectile.oldPosition).Length());
        BlurIntensity = MathHelper.Lerp(BlurIntensity, idealBlur, 0.16f);
    }

    /// <summary>
    /// Handles repositioning motion for this assassin.
    /// </summary>
    private void HandleRepositionMotion(Vector2 destination)
    {
        float minMovementInterpolant = 0.02f;
        bool doneMoving = MovementInterpolant <= minMovementInterpolant;

        // Dash if sufficiently far from the desired position and not already dashing.
        if (doneMoving && !Projectile.WithinRange(destination, 400f))
        {
            RerollMask();
            DashStart = Projectile.Center;
            MovementInterpolant = 1f;
            Projectile.netUpdate = true;
        }

        // Handle motion.
        Projectile.Center = Vector2.SmoothStep(Projectile.Center, destination, MathF.Pow(LumUtils.Convert01To010(MovementInterpolant), 4f) * 0.8f);
        MovementInterpolant *= 0.9f;
        if (MovementInterpolant < 0.6f)
            MovementInterpolant *= 0.8f;

        // Scale down when moving.
        Projectile.scale = LumUtils.InverseLerp(0.05f, minMovementInterpolant, MovementInterpolant) + LumUtils.InverseLerp(0.65f, 1f, MovementInterpolant);

        if (MovementInterpolant >= 0.2f)
        {
            float armRotation = 0.99f;
            LeftArmRotation = LeftArmRotation.AngleLerp(armRotation, 0.56f);
            RightArmRotation = RightArmRotation.AngleLerp(-armRotation, 0.56f);

            // Create fire visuals.
            // Due to how oldPosition works (Seemingly having a zeroed position at initialization time), this waits a few frames
            // after being summoned before beginning.
            if (ExistenceTimer >= 10)
                CreateMotionVisuals();
        }
    }

    private void GetMaskInfo(out Texture2D texture, out int goreID)
    {
        texture = null;
        goreID = 0;
        if (Main.netMode != NetmodeID.Server)
        {
            texture = AntishadowAssassinMasks[MaskVariant].Value;
            goreID = ModContent.Find<ModGore>(Mod.Name, $"AntishadowAssassinMask{MaskVariant + 1}").Type;
        }
    }

    /// <summary>
    /// Rerolls the mask variant for this assassin.
    /// </summary>
    private void RerollMask()
    {
        CreateMaskGore();

        int oldMaskVariant = MaskVariant;
        do
        {
            MaskVariant = Main.rand.Next(TotalMasks);
        }
        while (MaskVariant == oldMaskVariant);
    }

    /// <summary>
    /// Drops this assassin's mask as a gore.
    /// </summary>
    private void CreateMaskGore()
    {
        if (Main.netMode == NetmodeID.Server)
            return;

        GetMaskInfo(out _, out int goreID);

        Gore.NewGore(Projectile.GetSource_FromThis(), MaskPosition, Vector2.Zero, goreID, Projectile.scale);
    }

    /// <summary>
    /// Ensures that all motion visuals for when this assassin is dashing to its destination.
    /// </summary>
    private void CreateMotionVisuals()
    {
        Vector2 previous = DashStart;
        Vector2 current = Projectile.Center;
        Vector2 directionalForce = (current - previous).RotatedByRandom(0.4f) * 0.1f;
        float scaleFactor = Utils.Remap(current.Distance(previous), 30f, 125f, 1f, 0.5f) * 0.6f;
        int fireBrightness = Main.rand.Next(20);
        Color fireColor = new Color(fireBrightness, fireBrightness, fireBrightness);
        Color bigColorColor = fireColor;

        if (MovementInterpolant <= 0.35f)
        {
            scaleFactor *= 2f;
            bigColorColor = new Color(0, 0, 0);
        }

        // Create fire.
        AntishadowFireParticleSystemManager.ParticleSystem.CreateNew(current, Main.rand.NextVector2Circular(7f, 7f) + directionalForce, Vector2.One * Main.rand.NextFloat(100f, 175f) * scaleFactor, bigColorColor);
        if (!current.WithinRange(previous, 40f))
        {
            int steps = (int)(current.Distance(previous) / 21f);
            for (float i = 0; i < steps; i++)
            {
                Color loopFireColor = fireColor;
                if (i % 6 == 0)
                    loopFireColor = new Color(174, 0, Main.rand.Next(16), 0);

                Vector2 position = Vector2.Lerp(previous, current, i / steps);
                AntishadowFireParticleSystemManager.ParticleSystem.CreateNew(position, Main.rand.NextVector2Circular(27f, 27f) + directionalForce * 0.45f, Vector2.One * Main.rand.NextFloat(30f, 175f) * scaleFactor, loopFireColor);

                if (Main.rand.NextBool(4))
                {
                    Dust fire = Dust.NewDustPerfect(position, 261);
                    fire.velocity = Main.rand.NextVector2Circular(30f, 30f) - directionalForce * 0.4f;
                    fire.color = Color.Red;
                    fire.noGravity = true;
                    fire.scale *= 1.2f;
                }
            }
        }
    }

    /// <summary>
    /// Moves the ropes responsible for the motion of this assassin's kasa hat beads.
    /// </summary>
    private void MoveBeadRops()
    {
        float ropeGravity = 0.5f;
        float windSpeed = Math.Clamp(Main.WindForVisuals, -0.6f, 0.6f);
        Vector2 force = Vector2.UnitX * (LumUtils.AperiodicSin(Time * 0.024f) * 0.4f + windSpeed);
        BeadRopeA.Update(Projectile.Center + new Vector2(Projectile.spriteDirection * -36f, -114f).RotatedBy(Projectile.rotation + BowRotation * Projectile.spriteDirection) * Projectile.scale, ropeGravity, force);

        force = Vector2.UnitX * (LumUtils.AperiodicSin(Time * 0.024f + 5.1f) * 0.4f + windSpeed);
        BeadRopeB.Update(Projectile.Center + new Vector2(Projectile.spriteDirection * 26f, -114f).RotatedBy(Projectile.rotation + BowRotation * Projectile.spriteDirection) * Projectile.scale, ropeGravity, force);
    }

    /// <summary>
    /// Creates smoke around the feet of this assassin.
    /// </summary>
    private void CreateFootSmoke()
    {
        // Create idle smoke.
        int fireBrightness = Main.rand.Next(26);
        float fireSpeed = -2.67f;
        Color fireColor = new Color(fireBrightness, fireBrightness, fireBrightness) * 0.3f;
        if (Main.rand.NextBool(12))
            fireColor = new Color(175, 0, Main.rand.Next(16), 0);

        Vector2 fireSpawnPosition = Projectile.Bottom + new Vector2(Main.rand.NextFloatDirection() * 36f, Main.rand.NextFloatDirection() * 24f - 16f).RotatedBy(Projectile.rotation) * Projectile.scale;
        Vector2 fireVelocity = fireSpawnPosition.SafeDirectionTo(Projectile.Bottom) * fireSpeed - Vector2.UnitY * 3f;
        AntishadowFireParticleSystemManager.BackParticleSystem.CreateNew(fireSpawnPosition, fireVelocity, new Vector2(0.81f, 1f) * Main.rand.NextFloat(20f, 65f), fireColor * Projectile.Opacity);

        if (Main.rand.NextBool(24))
        {
            EmberParticle ember = new EmberParticle(fireSpawnPosition, Main.rand.NextVector2Circular(4f, 1f) - Vector2.UnitY * Main.rand.NextFloat(10f), Color.Crimson * Projectile.Opacity, 2f, 120);
            ember.Spawn();
        }
    }

    /// <summary>
    /// Randomly creates idle sounds, such as wind chimes.
    /// </summary>
    private void CreateIdleSounds()
    {
        // Don't play idle sounds if moving fast.
        if (!Projectile.position.WithinRange(Projectile.oldPosition, 10f))
            return;

        float windSpeed = Math.Clamp(Main.WindForVisuals, -1f, 1f);
        float windChimeCreationProbability = MathHelper.Lerp(0.0016f, 0.005f, MathF.Abs(windSpeed));
        if (Projectile.soundDelay <= 0 && Main.rand.NextBool(windChimeCreationProbability) && State != AssassinState.Leave)
        {
            attachedSounds.Add(SoundEngine.PlaySound(ChimeSound with { MaxInstances = 3 }, Projectile.Center).WithVolumeBoost(0.1f));
            Projectile.soundDelay = 300;
        }
    }

    /// <summary>
    /// Updates looped sounds for this assassin.
    /// </summary>
    private void UpdateLoopedSounds()
    {
        AmbientLoop.Update(Projectile.Center, sound =>
        {
            float idealPitch = LumUtils.InverseLerp(6f, 30f, Projectile.position.Distance(Projectile.oldPosition)) * 0.8f;
            sound.Volume = 1.45f;
            sound.Pitch = MathHelper.Lerp(sound.Pitch, idealPitch, 0.6f);
        });
        AttackLoop.Update(Projectile.Center, sound =>
        {
            sound.Volume = AttackLoopActivationInterpolant * 1.3f;
            sound.Pitch = MathHelper.SmoothStep(0f, 0.8f, AttackLoopActivationInterpolant);
        });
    }

    /// <summary>
    /// Ensures that all idle sounds generated by this assassin stay attached to it as it moves.
    /// </summary>
    private void KeepSoundsAttached()
    {
        for (int i = 0; i < attachedSounds.Count; i++)
        {
            if (!SoundEngine.TryGetActiveSound(attachedSounds[i], out ActiveSound sound) || sound is null)
            {
                attachedSounds.RemoveAt(i);
                i--;
                continue;
            }

            sound.Position = Projectile.Center;
        }
    }

    /// <summary>
    /// Handles the application of minion buffs for this assassin, making it vanish if the buffs are not present or the owner player has died.
    /// </summary>
    private void HandleMinionBuffs()
    {
        Owner.AddBuff(ModContent.BuffType<AntishadowAssassinBuff>(), 3);
        Referenced<bool> hasMinion = Owner.GetValueRef<bool>("HasAntishadowAssassin");
        if (Owner.dead)
            hasMinion.Value = false;
        if (hasMinion.Value)
            Projectile.timeLeft = 2;
        else if (ExistenceTimer >= 5)
            SwitchState(AssassinState.Leave);

        if (State == AssassinState.Leave)
            hasMinion.Value = false;
    }

    /// <summary>
    /// Draws a katana for this assassin.
    /// </summary>
    private void DrawKatana(Vector2 bladeDrawStartingPosition, bool flip, float angle)
    {
        float katanaWidthFunction(float completionRatio)
        {
            float baseWidth = 4f;
            if (completionRatio <= (Projectile.spriteDirection == 1 ? 0.13f : 0.19f))
                baseWidth = 6.7f;

            return Projectile.Opacity * Projectile.scale * baseWidth;
        }
        Color katanaColorFunction(float completionRatio) => Projectile.GetAlpha(Color.Red);

        float appearanceInterpolant = KatanaUnsheathInterpolant;
        ManagedShader katanaShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowKatanaShader");
        katanaShader.TrySetParameter("flip", flip);
        katanaShader.TrySetParameter("appearanceInterpolant", appearanceInterpolant);
        katanaShader.SetTexture(MiscTexturesRegistry.TurbulentNoise.Value, 1, SamplerState.PointWrap);

        PrimitiveSettings katanaPrimitiveSettings = new(katanaWidthFunction, katanaColorFunction, Shader: katanaShader, ProjectionAreaWidth: (int)WotGUtils.ViewportSize.X, ProjectionAreaHeight: (int)WotGUtils.ViewportSize.Y, UseUnscaledMatrix: true);

        Vector2 katanaReach = angle.ToRotationVector2() * appearanceInterpolant * Projectile.spriteDirection * MathF.Sqrt(Projectile.scale) * -138f;
        Vector2 orthogonalOffset = (angle + flip.ToDirectionInt() * -MathHelper.PiOver2).ToRotationVector2() * appearanceInterpolant * 30f;

        Vector2[] katanaPositions = new Vector2[8];
        for (int i = 0; i < katanaPositions.Length; i++)
        {
            float completionRatio = i / (float)(katanaPositions.Length - 1f);
            katanaPositions[i] = bladeDrawStartingPosition + katanaReach * completionRatio + Main.screenPosition;
            katanaPositions[i] += orthogonalOffset * completionRatio.Squared();
        }

        PrimitiveRenderer.RenderTrail(katanaPositions, katanaPrimitiveSettings, 40);
    }

    /// <summary>
    /// Draws the main body for the purpose of rendering into the RT responsible for this assassin's body.
    /// </summary>
    private void DrawIntoBodyTarget()
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, LumUtils.CullOnlyScreen);

        Texture2D body = BodyTexture.Value;
        Texture2D legs = LegsTexture.Value;
        Texture2D arm = ArmTexture.Value;
        int armXOrigin = 32;
        Vector2 drawPosition = WotGUtils.ViewportSize * 0.5f;

        if (MathF.Abs(BowRotation) >= 0.01f)
        {
            arm = ArmBowTexture.Value;
            armXOrigin += 4;
        }

        // Draw the right arm behind the body.
        float armScale = 0.76f;
        Vector2 rightArmDrawPosition = drawPosition + new Vector2(6f, -80f).RotatedBy(BowRotation);
        Main.spriteBatch.Draw(arm, rightArmDrawPosition, null, Color.White, RightArmRotation, new Vector2(arm.Width - armXOrigin, 10f), armScale, SpriteEffects.FlipHorizontally, 0f);

        // Draw the body.
        Main.spriteBatch.Draw(body, drawPosition, null, Color.White, BowRotation, body.Size() * new Vector2(0.5f, 1f), 1f, 0, 0f);

        // Draw legs.
        Main.spriteBatch.Draw(legs, drawPosition, null, Color.White, 0f, new Vector2(32f, 16f), 1f, 0, 0f);

        // Draw the left arm.
        Vector2 leftArmDrawPosition = drawPosition + new Vector2(-14f, -80f).RotatedBy(BowRotation);
        Main.spriteBatch.Draw(arm, leftArmDrawPosition, null, Color.White, LeftArmRotation, new Vector2(armXOrigin, 10f), armScale, 0, 0f);

        Main.spriteBatch.End();
    }

    /// <summary>
    /// Draws the arm outlines into their designated RT.
    /// </summary>
    private void DrawIntoArmOutlineTarget()
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, LumUtils.CullOnlyScreen);

        Texture2D arm = ArmOutlineTexture.Value;
        int armXOrigin = 32;
        Vector2 drawPosition = WotGUtils.ViewportSize * 0.5f;

        if (MathF.Abs(BowRotation) >= 0.01f)
        {
            arm = ArmBowOutlineTexture.Value;
            armXOrigin += 4;
        }

        // Draw the right arm behind the body.
        float armScale = 0.76f;
        Vector2 rightArmDrawPosition = drawPosition + new Vector2(6f, -80f).RotatedBy(BowRotation);
        Main.spriteBatch.Draw(arm, rightArmDrawPosition, null, Color.White, RightArmRotation, new Vector2(arm.Width - armXOrigin, 10f), armScale, SpriteEffects.FlipHorizontally, 0f);

        // Draw the left arm.
        Vector2 leftArmDrawPosition = drawPosition + new Vector2(-14f, -80f).RotatedBy(BowRotation);
        Main.spriteBatch.Draw(arm, leftArmDrawPosition, null, Color.White, LeftArmRotation, new Vector2(armXOrigin, 10f), armScale, 0, 0f);

        Main.spriteBatch.End();
    }

    /// <summary>
    /// Draws beads and rope attached to the kasa hat for this assassin.
    /// </summary>
    private void DrawBeads(Vector2 center)
    {
        Texture2D whitePixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
        Texture2D beadTextureA = GennedAssets.Textures.SecondPhaseForm.Beads3;
        Texture2D beadTextureB = GennedAssets.Textures.SecondPhaseForm.Beads2;
        Color ropeColor = new Color(210, 13, 16) * Projectile.Opacity;
        Vector2 drawOffset = center - Projectile.Center;
        BeadRopeA?.DrawProjection(whitePixel, drawOffset, false, _ => ropeColor, widthFactor: Projectile.scale, projectionWidth: (int)WotGUtils.ViewportSize.X, projectionHeight: (int)WotGUtils.ViewportSize.Y, unscaledMatrix: true);
        BeadRopeB?.DrawProjection(whitePixel, drawOffset, false, _ => ropeColor, widthFactor: Projectile.scale, projectionWidth: (int)WotGUtils.ViewportSize.X, projectionHeight: (int)WotGUtils.ViewportSize.Y, unscaledMatrix: true);

        if (BeadRopeA is not null)
        {
            float beadRotation = (BeadRopeA.Rope[^2].Position - BeadRopeA.Rope[^1].Position).ToRotation() + MathHelper.PiOver2;
            Main.spriteBatch.Draw(beadTextureA, BeadRopeA.Rope[^5].Position + drawOffset, null, Color.White * Projectile.Opacity, beadRotation, new Vector2(24f, 6f), Projectile.scale * 0.08f, 0, 0f);
        }
        if (BeadRopeB is not null)
        {
            float beadRotation = (BeadRopeB.Rope[^2].Position - BeadRopeB.Rope[^1].Position).ToRotation() + MathHelper.PiOver2;
            Main.spriteBatch.Draw(beadTextureB, BeadRopeB.Rope[^4].Position + drawOffset, null, Color.White * Projectile.Opacity, beadRotation, new Vector2(30f, 243f), Projectile.scale * 0.1f, 0, 0f);
        }
    }

    /// <summary>
    /// Draws the mask for this assassin.
    /// </summary>
    private void DrawMask(Vector2 center)
    {
        GetMaskInfo(out Texture2D mask, out _);
        if (mask is null)
            return;

        Vector2 faceDrawPosition = MaskPosition - Projectile.Center + center - Vector2.UnitY.RotatedBy(Projectile.rotation + BowRotation * Projectile.spriteDirection) * 30f;
        Main.spriteBatch.Draw(mask, faceDrawPosition, null, Color.White * Projectile.Opacity, Projectile.rotation + BowRotation * Projectile.spriteDirection, mask.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection.ToSpriteDirection(), 0f);
    }

    /// <summary>
    /// Draws the kasa hat for this assassin.
    /// </summary>
    private void DrawHat(Vector2 center)
    {
        Texture2D hat = KasaTexture.Value;
        Vector2 hatDrawPosition = center + new Vector2(Projectile.spriteDirection * -4f, -128f).RotatedBy(Projectile.rotation + BowRotation * Projectile.spriteDirection) * Projectile.scale;
        Main.spriteBatch.Draw(hat, hatDrawPosition, null, Color.White * Projectile.Opacity, Projectile.rotation + BowRotation * Projectile.spriteDirection, hat.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection.ToSpriteDirection(), 0f);
    }

    /// <summary>
    /// Renders this assassin's contents into the final target.
    /// </summary>
    private void RenderIntoResultsTarget()
    {
        ArmOutlineTarget.Request(400, 400, Projectile.identity, DrawIntoArmOutlineTarget);
        BodyTarget.Request(400, 400, Projectile.identity, DrawIntoBodyTarget);
        if (BodyTarget.TryGetTarget(Projectile.identity, out RenderTarget2D bodyTarget) && bodyTarget is not null &&
            ArmOutlineTarget.TryGetTarget(Projectile.identity, out RenderTarget2D outlineTarget) && outlineTarget is not null)
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            Vector2 center = WotGUtils.ViewportSize * 0.5f;

            float baseRotation = Projectile.rotation;
            float leftKatanaAngle = Projectile.rotation + 0.15f;
            float rightKatanaAngle = Projectile.rotation - 0.15f;
            if (Projectile.spriteDirection == 1)
                rightKatanaAngle += 0.3f;
            else
                leftKatanaAngle -= 0.3f;

            Vector2 rightShoulderPosition = center + new Vector2(6f, -80f).RotatedBy(baseRotation + BowRotation * Projectile.spriteDirection) * Projectile.scale;
            Vector2 rightHandEnd = rightShoulderPosition + new Vector2(12f, 84f).RotatedBy(baseRotation + RightArmRotation) * Projectile.scale;
            Vector2 leftShoulderPosition = center + new Vector2(-14f, -80f).RotatedBy(baseRotation + BowRotation * Projectile.spriteDirection) * Projectile.scale;
            Vector2 leftHandEnd = leftShoulderPosition + new Vector2(-12f, 84f).RotatedBy(baseRotation + LeftArmRotation) * Projectile.scale;

            float katanaRotationOffset = Projectile.spriteDirection * KatanaRotation;
            DrawKatana(rightHandEnd, true, rightKatanaAngle + katanaRotationOffset);
            DrawKatana(leftHandEnd, true, leftKatanaAngle + katanaRotationOffset);

            Vector3[] palette = new Vector3[]
            {
                new Vector3(1.5f),
                new Vector3(0f, 1f, 1.2f),
                new Vector3(1f, 0f, 0f),
            };

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            Color edgeColor = new Color(255, 0, 0);

            ManagedShader overlayShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssassinColorProcessingShader");
            overlayShader.TrySetParameter("eyeScale", LumUtils.Cos01(Main.GlobalTimeWrappedHourly * 7.5f + Projectile.identity) * 0.2f + 1f);
            overlayShader.TrySetParameter("gradient", palette);
            overlayShader.TrySetParameter("gradientCount", palette.Length);
            overlayShader.TrySetParameter("textureSize", bodyTarget.Size());
            overlayShader.TrySetParameter("edgeColor", edgeColor.ToVector4() * Projectile.Opacity);
            overlayShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
            overlayShader.Apply();

            Main.spriteBatch.Draw(bodyTarget, center, null, Color.Black * Projectile.Opacity, Projectile.rotation, bodyTarget.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection.ToSpriteDirection(), 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            Main.spriteBatch.Draw(outlineTarget, center, null, edgeColor * Projectile.Opacity * ArmOutlineOpacity, Projectile.rotation, outlineTarget.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection.ToSpriteDirection(), 0);

            DrawMask(center);
            DrawBeads(center);
            DrawHat(center);

            Main.spriteBatch.End();
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.PrepareForShaders();

        ResultsTarget.Request(600, 600, Projectile.identity, RenderIntoResultsTarget);
        if (ResultsTarget.TryGetTarget(Projectile.identity, out RenderTarget2D target) && target is not null)
        {
            ManagedShader postProcessingShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssassinPostProcessingShader");
            postProcessingShader.TrySetParameter("blurOffset", BlurIntensity * 0.2f);
            postProcessingShader.TrySetParameter("blurDirection", Projectile.velocity.SafeNormalize(Vector2.UnitX));
            postProcessingShader.TrySetParameter("disappearanceInterpolant", DisappearanceInterpolant);
            postProcessingShader.TrySetParameter("direction", (float)Projectile.spriteDirection);
            postProcessingShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
            postProcessingShader.Apply();

            Main.spriteBatch.Draw(target, Projectile.Center - Main.screenPosition, null, Color.White, 0f, target.Size() * 0.5f, 1f, 0, 0);
        }

        Main.spriteBatch.ResetToDefault();

        return false;
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < attachedSounds.Count; i++)
        {
            if (!SoundEngine.TryGetActiveSound(attachedSounds[i], out ActiveSound sound) || sound is null)
                continue;

            sound.Stop();
        }
    }

    public override bool? CanDamage() => Projectile.scale >= 0.56f;
}