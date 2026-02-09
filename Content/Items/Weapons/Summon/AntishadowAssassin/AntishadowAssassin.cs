using CalamityMod;
using CalamityMod.Items.Weapons.Melee;
using HeavenlyArsenal.Content.Buffs;
using HeavenlyArsenal.Core.Physics.ClothManagement;
using Luminance.Assets;
using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using Luminance.Core.Sounds;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.Graphics.RenderTargets;
using NoxusBoss.Core.Physics.VerletIntergration;
using NoxusBoss.Core.Utilities;
using ReLogic.Content;
using ReLogic.Utilities;
using System.IO;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent.Shaders;
using Terraria.Graphics.Effects;
//using HeavenlyArsenal.Content.NPCs.Friendly.ForgottenShrine;

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

        BowAtSpirit,

        Leave
    }

    /// <summary>
    ///     The chime sound played by this assassin at random.
    /// </summary>
    public static readonly SoundStyle ChimeSound = new("HeavenlyArsenal/Assets/Sounds/Items/Summoner/AntishadowBreatheChime", 2);

    /// <summary>
    ///     The sound used when this assassin unsheathes its katana.
    /// </summary>
    public static readonly SoundStyle UnsheatheSound = new("HeavenlyArsenal/Assets/Sounds/Items/Summoner/AntishadowAssassinKatanaUnsheathe");

    /// <summary>
    ///     The sound used when this assassin begins to leave due to being desummoned.
    /// </summary>
    public static readonly SoundStyle LeaveSound = new("HeavenlyArsenal/Assets/Sounds/Items/Summoner/AntishadowAssassinLeave");

    /// <summary>
    ///     The looped sound used in an ambient context.
    /// </summary>
    public static readonly SoundStyle AntishadowAmbientLoopSound = new("HeavenlyArsenal/Assets/Sounds/Items/Summoner/AntishadowAmbientLoop");

    /// <summary>
    ///     The looped sound used when attacking.
    /// </summary>
    public static readonly SoundStyle AntishadowAttackLoopSound = new("HeavenlyArsenal/Assets/Sounds/Items/Summoner/AntishadowAttackLoop");

    private readonly List<SlotId> attachedSounds = [];

    /// <summary>
    ///     How long this assassin has existed for, in frames.
    /// </summary>
    public int ExistenceTimer { get; set; }

    /// <summary>
    ///     Whether this assassin can do damage.
    /// </summary>
    public bool DoesDamage { get; set; }

    /// <summary>
    ///     The state of this assassin.
    /// </summary>
    public AssassinState State { get; set; }

    /// <summary>
    ///     The type of mask used by this assassin.
    /// </summary>
    public int MaskVariant { get; set; }

    /// <summary>
    ///     A general-purpose cooldown that prevents this assassin from bowing repeatedly at targets.
    /// </summary>
    public int SpiritBowCooldown { get; set; }

    /// <summary>
    ///     The index of the target of this assassin.
    /// </summary>
    public int TargetIndex { get; set; } = -1;

    /// <summary>
    ///     The intensity of blur effects on this assassin.
    /// </summary>
    public float BlurIntensity { get; set; }

    /// <summary>
    ///     The amount by which the assassin is rotating forward for a bow.
    /// </summary>
    public float BowRotation { get; set; }

    /// <summary>
    ///     The opacity of the arm outline.
    /// </summary>
    public float ArmOutlineOpacity { get; set; }

    /// <summary>
    ///     The amount by which this assassin has begun to disappear.
    /// </summary>
    public float DisappearanceInterpolant { get; set; }

    /// <summary>
    ///     The amount by which the katana is unsheathed.
    /// </summary>
    public float KatanaUnsheathInterpolant { get; set; }

    /// <summary>
    ///     The amount by which this assassin's <see cref="AttackLoop" /> sound should be activated and
    ///     noticeable.
    /// </summary>
    public float AttackLoopActivationInterpolant { get; set; }

    /// <summary>
    ///     The starting position of this assassin during their dash.
    /// </summary>
    public Vector2 DashStart { get; set; }

    /// <summary>
    ///     The cloth simulation that composes this assassin's robe.
    /// </summary>
    public ClothSimulation Robe { get; set; } = new(Vector3.Zero, 12, 11, 8.7f, 60f, 0.019f);

    /// <summary>
    ///     The ambient loop sound instance.
    /// </summary>
    public LoopedSoundInstance AmbientLoop { get; set; }

    /// <summary>
    ///     The attacking loop sound instance.
    /// </summary>
    public LoopedSoundInstance AttackLoop { get; set; }

    /// <summary>
    ///     The position of this assassin's mask.
    /// </summary>
    public Vector2 MaskPosition => Projectile.Center + new Vector2(Projectile.spriteDirection * -1f, -73f).RotatedBy(BowRotation * Projectile.spriteDirection) * Projectile.scale;

    /// <summary>
    ///     The owner of this assassin.
    /// </summary>
    public Player Owner => Main.player[Projectile.owner];

    /// <summary>
    ///     The rope for the assassin's first bead.
    /// </summary>
    public VerletSimulatedRope BeadRopeA { get; set; }

    /// <summary>
    ///     The rope for the assassin's second bead.
    /// </summary>
    public VerletSimulatedRope BeadRopeB { get; set; }

    /// <summary>
    ///     The rope for the assassin's third bead.
    /// </summary>
    public VerletSimulatedRope BeadRopeC { get; set; }

    /// <summary>
    ///     The rope for the assassin's fourth bead.
    /// </summary>
    public VerletSimulatedRope BeadRopeD { get; set; }

    /// <summary>
    ///     The rotation of the assassin's left arm.
    /// </summary>
    public ref float LeftArmRotation => ref Projectile.localAI[0];

    /// <summary>
    ///     The rotation of the assassin's right arm.
    /// </summary>
    public ref float RightArmRotation => ref Projectile.localAI[1];

    /// <summary>
    ///     The rotation of the assassin's katanas.
    /// </summary>
    public ref float KatanaRotation => ref Projectile.localAI[2];

    /// <summary>
    ///     The movement interpolant of this assassin during its stay near owner state.
    /// </summary>
    public ref float MovementInterpolant => ref Projectile.ai[0];

    /// <summary>
    ///     How long this assassin has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.ai[1];

    /// <summary>
    ///     An optional number used for the purposes of per-state memorization. Usage varies based on time.
    /// </summary>
    public ref float AIData => ref Projectile.ai[2];

    /// <summary>
    ///     The amount of mask variants that exist for this assassin.
    /// </summary>
    public static int TotalMasks => 6;

    /// <summary>
    ///     The maximum search range this assassin can examine for targets.
    /// </summary>
    public static float TargetingRange => 756f;

    /// <summary>
    ///     The maximum search range this assassin can examine for spirits to bow towards.
    /// </summary>
    public static float SpiritBowingRange => 267f;

    /// <summary>
    ///     The scale of this assassin's arms.
    /// </summary>
    public static float ArmScale => 0.76f;

    /// <summary>
    ///     The render target that holds the contents of this assassin's body and arms.
    /// </summary>
    public static InstancedRequestableTarget BodyTarget { get; private set; }

    /// <summary>
    ///     The render target that holds the contents of this assassin's arm outlines.
    /// </summary>
    public static InstancedRequestableTarget ArmOutlineTarget { get; private set; }

    /// <summary>
    ///     The render target that holds the contents of the entire assassin.
    /// </summary>
    public static InstancedRequestableTarget ResultsTarget { get; private set; }

    /// <summary>
    ///     The body texture asset wrapper.
    /// </summary>
    public static Asset<Texture2D> BodyTexture { get; private set; }

    /// <summary>
    ///     The arm texture asset wrapper.
    /// </summary>
    public static Asset<Texture2D> ArmTexture { get; private set; }

    /// <summary>
    ///     The arm outline texture asset wrapper.
    /// </summary>
    public static Asset<Texture2D> ArmOutlineTexture { get; private set; }

    /// <summary>
    ///     The arm texture asset wrapper used when bowing.
    /// </summary>
    public static Asset<Texture2D> ArmBowTexture { get; private set; }

    /// <summary>
    ///     The arm outline texture asset wrapper used when bowing.
    /// </summary>
    public static Asset<Texture2D> ArmBowOutlineTexture { get; private set; }

    /// <summary>
    ///     The kasa texture asset wrapper.
    /// </summary>
    public static Asset<Texture2D> KasaTexture { get; private set; }

    public static Asset<Texture2D> AhogeTexture { get; private set; }

    /// <summary>
    ///     The legs texture asset wrapper.
    /// </summary>
    public static Asset<Texture2D> LegsTexture { get; private set; }

    /// <summary>
    ///     The mask texture asset wrappers.
    /// </summary>
    public static Asset<Texture2D>[] AntishadowAssassinMasks { get; private set; }

    /// <summary>
    ///     The fifth assassin mask texture asset wrapper.
    /// </summary>
    public static Asset<Texture2D> AntishadowAssassinMask5 { get; private set; }

    private void PerformStupidAssetBoilerplateLoading()
    {
        AntishadowAssassinMasks = new Asset<Texture2D>[TotalMasks];

        for (var i = 1; i <= TotalMasks; i++)
        {
            AntishadowAssassinMasks[i - 1] = ModContent.Request<Texture2D>($"HeavenlyArsenal/Content/Gores/AntishadowAssassinMask{i}");
        }

        var texturePrefix = $"{Mod.Name}/Content/Items/Weapons/Summon/AntishadowAssassin";
        BodyTexture = ModContent.Request<Texture2D>($"{texturePrefix}/AntishadowAssassin");
        ArmTexture = ModContent.Request<Texture2D>($"{texturePrefix}/AntishadowAssassinArm");
        ArmOutlineTexture = ModContent.Request<Texture2D>($"{texturePrefix}/AntishadowAssassinArm_Outline");
        ArmBowTexture = ModContent.Request<Texture2D>($"{texturePrefix}/AntishadowAssassinArmBow");
        ArmBowOutlineTexture = ModContent.Request<Texture2D>($"{texturePrefix}/AntishadowAssassinArmBow_Outline");
        DateTime date = DateTime.Now;

        KasaTexture = ModContent.Request<Texture2D>($"{texturePrefix}/AntishadowAssassinKasa");
        AhogeTexture = ModContent.Request<Texture2D>($"{texturePrefix}/AntishadowAssassin_Ahoge");
        LegsTexture = ModContent.Request<Texture2D>($"{texturePrefix}/AntishadowAssassinLegs");
    }

    public override void SetStaticDefaults()
    {
        if (!Main.dedServ)
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
        }

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
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;
        Projectile.timeLeft = 90000;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.minion = true;
        Projectile.hide = true; // Refer to AntishadowAssassinDyeRenderer.cs.
        Projectile.DamageType = DamageClass.Summon;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write((int)State);
        writer.Write(TargetIndex);
        writer.Write(SpiritBowCooldown);
        writer.WriteVector2(DashStart);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        State = (AssassinState)reader.ReadInt32();
        TargetIndex = reader.ReadInt32();
        SpiritBowCooldown = reader.ReadInt32();
        DashStart = reader.ReadVector2();
    }

    public override void AI()
    {

        //Main.NewText($"{Projectile.velocity.ToString()}, \n{(AvatarOfEmptiness.Myself != null? AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().CurrentState.ToString() + " ," + AvatarOfEmptiness.Myself.As<AvatarOfEmptiness>().AITimer.ToString(): null)}");
        // Initialize beads if necessary.
        if (BeadRopeA is null || BeadRopeB is null || BeadRopeC is null || BeadRopeD is null)
        {
            InitializeBeads();
        }

        AmbientLoop ??= LoopedSoundManager.CreateNew(AntishadowAmbientLoopSound, () => !Projectile.active);
        AttackLoop ??= LoopedSoundManager.CreateNew(AntishadowAttackLoopSound, () => !Projectile.active);

        PreUpdateResets();

        if (State != AssassinState.Leave)
        {
            HandleMinionBuffs();
        }

        //Main.NewText($"State: {State}");
        //Main.NewText($"Particle count: {AntishadowFireParticleSystemManager.BackParticleSystem.Keys.Count.ToString()}");
        ExecuteState();

        if (Owner.MinionAttackTargetNPC != -1 && Main.npc[Owner.MinionAttackTargetNPC].active)
        {
            TargetIndex = Owner.MinionAttackTargetNPC;
        }

        ResetVisuals();
        MoveBeadRopes();
        CreateFootSmoke();
        CreateIdleSounds();
        UpdateLoopedSounds();
        KeepSoundsAttached();
        DisturbLiquids();

        if (!Main.dedServ)
        {
            UpdateCloth();
        }

        Time++;
        ExistenceTimer++;

        if (SpiritBowCooldown >= 1)
        {
            SpiritBowCooldown--;
        }

        // Sets the projectiles velocity to 0 if it's not valid
        if (float.IsNaN(Projectile.velocity.X) ||
        float.IsNaN(Projectile.velocity.Y) ||
        float.IsInfinity(Projectile.velocity.X) ||
        float.IsInfinity(Projectile.velocity.Y))
        {
            Projectile.velocity = Vector2.Zero;
        }
    }

    /// <summary>
    ///     Executes the state of this assassin.
    /// </summary>
    private void ExecuteState()
    {
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
            case AssassinState.BowAtSpirit:
                DoBehavior_BowAtSpirit();

                break;
            case AssassinState.Leave:
                DoBehavior_Leave();

                break;
        }
    }

    /// <summary>
    ///     Updates the rope of this assassin, making it sway in the wind and stay attached to its back.
    /// </summary>
    private void UpdateCloth()
    {
        var steps = 25;
        var windSpeed = Math.Clamp(Main.WindForVisuals * Projectile.spriteDirection * 8f, -1.3f, 0f);
        var robePosition = Projectile.Center + new Vector2(14f, -50f).RotatedBy(BowRotation);
        var wind = Vector3.UnitX * (LumUtils.AperiodicSin(ExistenceTimer * 0.029f) * 0.67f + windSpeed) * 1.74f;

        for (var i = 0; i < steps; i++)
        {
            for (var x = 0; x < Robe.Width; x += 2)
            {
                for (var y = 0; y < 2; y++)
                {
                    ConstrainParticle(robePosition, Robe.particleGrid[x, y], 0f);
                }
            }

            Robe.Simulate(0.051f, false, Vector3.UnitY * 3.2f + wind);
        }
    }

    /// <summary>
    ///     Constrains a given cloth point, keeping it locked in place.
    /// </summary>
    private void ConstrainParticle(Vector2 anchor, ClothPoint? point, float angleOffset)
    {
        if (point is null)
        {
            return;
        }

        var xInterpolant = point.X / (float)Robe.Width;
        var angle = MathHelper.Lerp(MathHelper.PiOver2, MathHelper.TwoPi - MathHelper.PiOver2, xInterpolant);

        var ring = new Vector3(MathF.Cos(angle + angleOffset) * 50f, 0f, -MathF.Cos(angle) * 10f);
        ring.Y += point.Y * 6f;

        point.Position = new Vector3(anchor, 0f) + ring;
        point.IsFixed = true;
    }

    /// <summary>
    ///     Initializes beads atop the kasa of this assassin.
    /// </summary>
    private void InitializeBeads()
    {
        var ropePoints = 6;
        BeadRopeA = new VerletSimulatedRope(Projectile.Center, Vector2.Zero, ropePoints, 12.5f);
        BeadRopeB = new VerletSimulatedRope(Projectile.Center, Vector2.Zero, ropePoints, 18.5f);
        BeadRopeC = new VerletSimulatedRope(Projectile.Center, Vector2.Zero, ropePoints, 11.1f);
        BeadRopeD = new VerletSimulatedRope(Projectile.Center, Vector2.Zero, ropePoints, 11.3f);
    }

    private void DoBehavior_StayNearOwner()
    {
        HandleRepositionMotion(Owner.Center + new Vector2(-Owner.direction * 60f, -40f));

        // Fly up and down by default.
        // This gets functionally nullified if a reposition is ongoing.
        var sinusoidalSpeed = MathF.Cos(MathHelper.TwoPi * Time / 150f) * 0.33f;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.UnitY * sinusoidalSpeed, 0.1f);
        Projectile.Opacity = Projectile.Opacity.StepTowards(1f, 0.2f);
        Projectile.spriteDirection = (int)Projectile.HorizontalDirectionTo(Owner.Center);

        // Search for targets.
        var targetIndex = FindPotentialTarget();

        if (targetIndex >= 0)
        {
            TargetIndex = targetIndex;
            SwitchState(AssassinState.DissipateToHuntTarget);
        }

        // Search for a spirit to bow towards.
        else if (SpiritBowCooldown <= 0 && Main.rand.NextBool(75))
        {
            var spiritIndex = FindPotentialSpirit();

            if (spiritIndex >= 0)
            {
                SpiritBowCooldown = LumUtils.SecondsToFrames(12f);
                Projectile.spriteDirection = (int)Projectile.HorizontalDirectionTo(Main.npc[spiritIndex].Center);
                SwitchState(AssassinState.BowAtSpirit);
            }
        }
    }

    private void DoBehavior_DissipateToHuntTarget()
    {
        if (!EnsureTargetIsAlive())
        {
            return;
        }

        var target = Main.npc[TargetIndex];

        if (Time == 1f)
        {
            if (Projectile.WithinRange(target.Center, 300f))
            {
                SwitchState(AssassinState.WaitBeforeSlashingTarget);
            }

            DashStart = Projectile.Center;
            MovementInterpolant = 1f;
            Projectile.netUpdate = true;
        }

        HandleRepositionMotion(target.Center + target.SafeDirectionTo(Projectile.Center) * 75f);

        var armRotation = 0.99f;
        LeftArmRotation = LeftArmRotation.AngleLerp(armRotation, 0.3f);
        RightArmRotation = RightArmRotation.AngleLerp(-armRotation, 0.3f);
        KatanaRotation = KatanaRotation.AngleLerp(1.3f, 0.56f);

        // Look at the target.
        Projectile.spriteDirection = (int)Projectile.HorizontalDirectionTo(target.Center);

        if (Time >= 1f && MovementInterpolant <= 0.01f)
        {
            SwitchState(AssassinState.WaitBeforeSlashingTarget);
        }
    }

    private void DoBehavior_WaitBeforeSlashingTarget()
    {
        if (!EnsureTargetIsAlive())
        {
            return;
        }

        if (Time == 1f)
        {
            attachedSounds.Add(SoundEngine.PlaySound(UnsheatheSound, Projectile.Center).WithVolumeBoost(1.2f));
        }

        var armRotation = 0.99f;
        LeftArmRotation = LeftArmRotation.AngleLerp(armRotation, 0.3f);
        RightArmRotation = RightArmRotation.AngleLerp(-armRotation, 0.3f);
        KatanaRotation = KatanaRotation.AngleLerp(0.6f, 0.56f);

        // Vanish.
        //Projectile.Opacity = MathHelper.SmoothStep(0f, 1f, LumUtils.InverseLerp(18f, 7f, Time).Squared());
        KatanaUnsheathInterpolant = EasingCurves.Quadratic.Evaluate(EasingType.InOut, LumUtils.InverseLerp(0f, 11f, Time));

        if (Time >= 18f)
        {
            SwitchState(AssassinState.PerformDirectionalSlices);
        }

        var target = Main.npc[TargetIndex];

        var reelBackSpeed = MathF.Pow(1f - KatanaUnsheathInterpolant, 5f) * 35f;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.SafeDirectionTo(target.Center) * -reelBackSpeed, 0.46f);

        var idealRotation = MathHelper.PiOver4 * Projectile.spriteDirection * (target.Center.Y - Projectile.Center.Y).NonZeroSign() * Time / 18f;
        Projectile.rotation = Projectile.rotation.AngleLerp(idealRotation, 0.6f);
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
        Projectile.MaxUpdates = 4;

        KatanaUnsheathInterpolant = 1f;
        DoesDamage = true;

        var dashCount = 32;
        var target = Main.npc[TargetIndex];
        ref var dashCounter = ref AIData;
        var dashDuration = Utils.Clamp(15 - (int)dashCounter * 3, 5, 1000);

        var dashCycleHit = dashCounter == 0f
            ? 1
            : (int)(dashDuration * 0.5f); // Done to ensure that the slash sound always plays even if the target dies before the dash duration midway point is reached.

        if (Time == dashCycleHit)
        {
            //todo: come back to me and give it new sfx
            SoundEngine.PlaySound
            (
                (target.Organic() ? Murasama.OrganicHit : Murasama.InorganicHit) with
                {
                    MaxInstances = 9,
                    Volume = 0.4f,
                    PitchVariance = 0.4f,
                    Pitch = 0.4f
                }
            );

            ScreenShakeSystem.StartShakeAtPoint(target.Center, 3.6f);
        }

        var dashRadius = MathHelper.Lerp(401f, 200f, dashCounter / (dashCount - 1f));

        if (Time == 2f)
        {
            var teleportOffset = Main.rand.NextFloat(300f, 500f);
            var teleportDirection = Main.rand.NextVector2Unit();

            if (dashCounter == 0f)
                teleportDirection = Projectile.velocity.SafeNormalize(Vector2.Zero);

            if (target != null && target.CanBeChasedBy(this))
                Projectile.Center = target.Center + teleportDirection * teleportOffset;

            DashStart = Projectile.Center;
            Projectile.velocity = Projectile.SafeDirectionTo(target.Center);

            Projectile.netUpdate = true;
        }

        // Dash through the target.
        var dashCompletion = LumUtils.InverseLerp(0.1f, 0.9f, Time / dashDuration);
        var easedDashInterpolant = EasingCurves.Quadratic.Evaluate(EasingType.InOut, dashCompletion);
        var dashDirection = Projectile.velocity.SafeNormalize(Vector2.UnitY);
        var start = target.Center - dashDirection * dashRadius;
        var end = target.Center + dashDirection * dashRadius;
        Projectile.Center = Vector2.Lerp(start, end, easedDashInterpolant);

        var slashID = ModContent.ProjectileType<AntishadowUnidirectionalAssassinSlash>();

        if (Main.myPlayer == Projectile.owner)
        {
            var slashMaxOffset = 100f;
            var slashOffset = Main.rand.NextVector2Circular(slashMaxOffset, slashMaxOffset);
            var slashSpawnPosition = target.Center + slashOffset;

            var slash = Projectile.NewProjectile
            (
                Projectile.GetSource_FromThis(),
                slashSpawnPosition,
                Vector2.Zero,
                slashID,
                Projectile.damage,
                Projectile.knockBack,
                Projectile.owner,
                dashCounter,
                slashOffset.Length(),
                slashOffset.ToRotation()
            );

            Main.projectile[slash].originalDamage = Projectile.originalDamage;
            Main.projectile[slash].netUpdate = true;

            slashMaxOffset = 50f;
            slashID = ModContent.ProjectileType<AntishadowAssassinSlash>();
            var angleX = Main.rand.NextFloatDirection() * 1.1f;
            var angleY = Main.rand.NextFloatDirection() * 1.1f;
            slashSpawnPosition = Vector2.Lerp(Projectile.Center, target.Center, 0.8f) + Main.rand.NextVector2Circular(slashMaxOffset, slashMaxOffset);

            slash = Projectile.NewProjectile
                (Projectile.GetSource_FromThis(), slashSpawnPosition, Vector2.Zero, slashID, Projectile.damage, Projectile.knockBack, Projectile.owner, angleX, angleY, 0.67f);

            Main.projectile[slash].originalDamage = Projectile.originalDamage;
            Main.projectile[slash].netUpdate = true;
        }

        // Keep all relevant slashes attached to the assassin.
        foreach (var slash in Main.ActiveProjectiles)
        {
            var bend = Projectile.velocity.RotatedBy(MathHelper.PiOver2) * LumUtils.Convert01To010(dashCompletion) * 100f;

            if (slash.owner == Projectile.owner && slash.ai[0] == dashCounter && slash.type == slashID && dashCompletion < 0.9f)
            {
                float bendDirection = (slash.identity % 2 == 0).ToDirectionInt();
                var slashOffset = slash.ai[1] * slash.ai[2].ToRotationVector2() + bend * bendDirection;
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
            {
                SwitchState(AssassinState.SliceTargetRepeatedly);
                RerollMask(false);
            }
        }
    }

    private void DoBehavior_SliceTargetRepeatedly()
    {
        if (!EnsureTargetIsAlive())
        {
            TransitionToPostDashSlash();

            return;
        }

        KatanaUnsheathInterpolant = 1f;
        DoesDamage = true;

        var slashTime = 120;
        var target = Main.npc[TargetIndex];

        if (Time <= slashTime)
        {
            if (Time % 4f == 0f)
            {
                SoundEngine.PlaySound
                (
                    (target.Organic() ? Murasama.OrganicHit : Murasama.InorganicHit) with
                    {
                        MaxInstances = 16,
                        Volume = 0.5f,
                        PitchVariance = 0.2f
                    }
                );
            }

            Projectile.Center = target.Center;
            Projectile.velocity = target.velocity.SafeNormalize((target.position - target.oldPosition).SafeNormalize(Vector2.UnitX * Projectile.spriteDirection));
            Projectile.spriteDirection = Projectile.velocity.X.NonZeroSign();
            DashStart = Projectile.Center;
            ScreenShakeSystem.StartShakeAtPoint(target.Center, 1.75f);

            if (Main.myPlayer == Projectile.owner)
            {
                var slashMaxOffset = 50f;

                for (var i = 0; i < 5; i++)
                {
                    var angleX = Main.rand.NextFloatDirection() * 1.2f;
                    var angleY = Main.rand.NextFloatDirection() * 1.2f;
                    var slashSpawnPosition = target.Center + Main.rand.NextVector2Circular(slashMaxOffset, slashMaxOffset);

                    var slashID = ModContent.ProjectileType<AntishadowAssassinSlash>();

                    var slash = Projectile.NewProjectile
                        (Projectile.GetSource_FromThis(), slashSpawnPosition, Vector2.Zero, slashID, Projectile.damage, Projectile.knockBack, Projectile.owner, angleX, angleY, 1.15f);

                    Main.projectile[slash].originalDamage = Projectile.originalDamage;
                    Main.projectile[slash].netUpdate = true;
                }
            }
        }

        AttackLoopActivationInterpolant = MathHelper.Lerp(AttackLoopActivationInterpolant, 1f, 0.3f);
        Projectile.Opacity = LumUtils.InverseLerp(slashTime, slashTime + 15f, Time);

        if (Time >= slashTime + 15f)
        {
            SwitchState(AssassinState.PerformDirectionalSlices);
        }
    }

    private void DoBehavior_PostSliceDash()
    {
        var dashDuration = 32;

        Projectile.scale = Projectile.scale.StepTowards(1f, 0.3f);
        Projectile.velocity *= 0.685f;
        Projectile.Opacity = Projectile.Opacity.StepTowards(1f, 0.3f);

        var armRepositionInterpolant = LumUtils.Saturate(Time / dashDuration);
        var armRotation = MathHelper.SmoothStep(0.99f, 0.15f, MathF.Pow(armRepositionInterpolant, 0.4f));

        if (Projectile.spriteDirection == 1)
        {
            armRotation *= -1f;
        }

        LeftArmRotation = LeftArmRotation.AngleLerp(-armRotation, 0.3f);
        RightArmRotation = RightArmRotation.AngleLerp(armRotation, 0.3f);
        KatanaRotation = 0f;

        if (Time >= dashDuration)
        {
            SwitchState(AssassinState.StayNearOwner);
        }
    }

    private void DoBehavior_BowAtSpirit()
    {
        var bowDelay = 14;
        var bowTime = 56;
        var bowHoldTime = 40;

        var bowInterpolant = LumUtils.InverseLerpBump(0f, bowTime, bowTime + bowHoldTime, bowTime * 2 + bowHoldTime, Time - bowDelay);
        var angleReorientInterpolant = LumUtils.InverseLerp(0f, 0.2f, bowInterpolant) * 0.8f;
        BowRotation = MathHelper.SmoothStep(0f, MathHelper.PiOver4, EasingCurves.Quadratic.Evaluate(EasingType.InOut, bowInterpolant));

        LeftArmRotation = 0.15f.AngleLerp(BowRotation * 0.6f, angleReorientInterpolant);
        RightArmRotation = (-0.15f).AngleLerp(BowRotation * 1.1f, angleReorientInterpolant);
        KatanaRotation = bowInterpolant;
        ArmOutlineOpacity = LumUtils.InverseLerp(0f, 0.25f, bowInterpolant);
        Projectile.velocity.Y *= 0.9f;

        if (Time >= bowDelay + bowTime * 2 + bowHoldTime)
        {
            SwitchState(AssassinState.StayNearOwner);
        }
    }

    private void DoBehavior_Leave()
    {
        var bowDelay = 14;
        var bowTime = 56;
        var disappearDelay = 60;
        var disappearTime = 15;

        if (Time == 1f)
        {
            SoundEngine.PlaySound(LeaveSound, Projectile.Center).WithVolumeBoost(0.45f);
        }

        var bowInterpolant = LumUtils.InverseLerp(0f, bowTime, Time - bowDelay);
        var angleReorientInterpolant = LumUtils.InverseLerp(0f, 0.2f, bowInterpolant) * 0.8f;
        BowRotation = MathHelper.SmoothStep(0f, MathHelper.PiOver4, EasingCurves.Quadratic.Evaluate(EasingType.InOut, bowInterpolant));

        LeftArmRotation = 0.15f.AngleLerp(BowRotation * 0.6f, angleReorientInterpolant);
        RightArmRotation = (-0.15f).AngleLerp(BowRotation * 1.1f, angleReorientInterpolant);
        KatanaRotation = bowInterpolant;
        DisappearanceInterpolant = LumUtils.InverseLerp(0f, disappearTime, Time - bowDelay - bowTime - disappearDelay);
        ArmOutlineOpacity = LumUtils.InverseLerp(0f, 0.25f, bowInterpolant);

        Projectile.spriteDirection = (int)Projectile.HorizontalDirectionTo(Owner.Center);
        Projectile.velocity *= 0.8f;
        Projectile.Opacity = Projectile.Opacity.StepTowards(1f, 0.2f);
        Projectile.scale = 1f;

        // Create fire particles when disappearing.
        for (var i = 0; i < 5; i++)
        {
            if (Main.rand.NextBool(DisappearanceInterpolant))
            {
                var fireBrightness = Main.rand.Next(20);
                var fireColor = new Color(fireBrightness, fireBrightness, fireBrightness);

                if (i % 6 == 0)
                {
                    fireColor = new Color(174, 0, Main.rand.Next(16), 0);
                }

                var position = Projectile.Center + Main.rand.NextVector2Circular(60f, 60f);
                AntishadowFireParticleSystemManager.CreateNew(Projectile.owner, false, position, Main.rand.NextVector2Circular(17f, 17f), Vector2.One * Main.rand.NextFloat(30f, 125f), fireColor);
            }
        }

        // Exist in stasis, and free up the minion slots occupied by this assassin.
        Projectile.minionSlots = 0f;
        Projectile.timeLeft = 2;

        if (DisappearanceInterpolant >= 1f)
        {
            Projectile.Kill();
        }
    }

    private int FindPotentialTarget()
    {
        var potentialTargets = new List<NPC>(Main.maxNPCs);

        foreach (var target in Main.ActiveNPCs)
        {
            var inRange = Projectile.WithinRange(target.Center, TargetingRange) || Owner.WithinRange(target.Center, TargetingRange);

            if (!inRange || !target.CanBeChasedBy())
            {
                continue;
            }

            potentialTargets.Add(target);
        }

        // Pick the closest valid target to the owner, so that the assassin doesn't go flying off hunting enemies offscreen since they're within its
        // detection zone.
        if (potentialTargets.Count >= 1)
        {
            return potentialTargets.OrderBy(p => p.DistanceSQ(Owner.Center)).First().whoAmI;
        }

        return -1;
    }

    private int FindPotentialSpirit()
    {
        /*
        int spiritID = ModContent.NPCType<FadingSpirit>();
        List<NPC> potentialSpirits = new List<NPC>(Main.maxNPCs);
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.type != spiritID || npc.As<FadingSpirit>().State != FadingSpirit.SpiritAIState.Linger)
                continue;

            bool tooClose = Projectile.WithinRange(npc.Center, SpiritBowingRange * 0.37f);
            bool inRange = Projectile.WithinRange(npc.Center, SpiritBowingRange);
            if (!inRange || tooClose)
                continue;

            potentialSpirits.Add(npc);
        }

        // Pick the closest valid target to the owner, since it's expected that the assassin will remain stationary.
        if (potentialSpirits.Count >= 1)
            return potentialSpirits.OrderBy(p => p.DistanceSQ(Owner.Center)).First().whoAmI;
        */
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
            var targetIndex = FindPotentialTarget();

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
    ///     Switches this assassin's state to a different state of choice.
    /// </summary>
    private void SwitchState(AssassinState state)
    {
        AIData = 0f;
        Time = 0f;
        State = state;
        Projectile.netUpdate = true;
    }

    /// <summary>
    ///     Handles pre-update reset effects, such as resetting MaxUpdates.
    /// </summary>
    private void PreUpdateResets()
    {
        DoesDamage = false;
        Projectile.MaxUpdates = 1;
        ArmOutlineOpacity = 0f;
        AttackLoopActivationInterpolant = AttackLoopActivationInterpolant.StepTowards(0f, 0.02f);

        if (BlurIntensity <= 0.2f)
        {
            KatanaUnsheathInterpolant = KatanaUnsheathInterpolant.StepTowards(0f, 0.02f);
        }
    }

    /// <summary>
    ///     Resets visuals for this assassin, such rotations.
    /// </summary>
    private void ResetVisuals()
    {
        var idealArmRotation = 0.15f;

        LeftArmRotation = LeftArmRotation.AngleLerp(idealArmRotation, 0.09f);
        RightArmRotation = RightArmRotation.AngleLerp(-idealArmRotation, 0.09f);
        KatanaRotation = KatanaRotation.AngleLerp(0f, 0.08f);

        // Rotate based on current horizontal speed.
        var idealRotation = Math.Clamp((Projectile.position - Projectile.oldPosition).X * 0.004f, -0.5f, 0.5f);
        Projectile.rotation = Projectile.rotation.AngleLerp(idealRotation, 0.3f);

        Projectile.scale = Projectile.scale.StepTowards(1f, 0.01f);

        var idealBlur = LumUtils.InverseLerp(18f, 75f, (Projectile.position - Projectile.oldPosition).Length());
        BlurIntensity = MathHelper.Lerp(BlurIntensity, idealBlur, 0.16f);
    }

    /// <summary>
    ///     Handles repositioning motion for this assassin.
    /// </summary>
    private void HandleRepositionMotion(Vector2 destination)
    {
        var minMovementInterpolant = 0.02f;
        var doneMoving = MovementInterpolant <= minMovementInterpolant;

        // Dash if sufficiently far from the desired position and not already dashing.
        if (doneMoving && !Projectile.WithinRange(destination, 540f))
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
        {
            MovementInterpolant *= 0.8f;
        }

        // Scale down when moving.
        Projectile.scale = LumUtils.InverseLerp(0.05f, minMovementInterpolant, MovementInterpolant) + LumUtils.InverseLerp(0.65f, 1f, MovementInterpolant);

        if (MovementInterpolant >= 0.2f)
        {
            var armRotation = 0.99f;
            LeftArmRotation = LeftArmRotation.AngleLerp(armRotation, 0.56f);
            RightArmRotation = RightArmRotation.AngleLerp(-armRotation, 0.56f);

            // Create fire visuals.
            // Due to how oldPosition works (Seemingly having a zeroed position at initialization time), this waits a few frames
            // after being summoned before beginning.
            if (ExistenceTimer >= 10)
            {
                CreateMotionVisuals();
            }
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
    ///     Rerolls the mask variant for this assassin.
    /// </summary>
    private void RerollMask(bool CreateGore = true)
    {
        if (CreateGore)
        {
            CreateMaskGore();
        }

        var oldMaskVariant = MaskVariant;

        do
        {
            MaskVariant = Main.rand.Next(TotalMasks);
        }
        while (MaskVariant == oldMaskVariant);
    }

    /// <summary>
    ///     Drops this assassin's mask as a gore.
    /// </summary>
    private void CreateMaskGore()
    {
        if (Main.netMode == NetmodeID.Server)
        {
            return;
        }

        GetMaskInfo(out _, out var goreID);

        Gore.NewGore(Projectile.GetSource_FromThis(), MaskPosition, Vector2.Zero, goreID, Projectile.scale);
    }

    /// <summary>
    ///     Ensures that all motion visuals for when this assassin is dashing to its destination.
    /// </summary>
    private void CreateMotionVisuals()
    {
        if (Main.dedServ)
        {
            return;
        }

        var previous = DashStart;
        var current = Projectile.Center;
        var directionalForce = (current - previous).RotatedByRandom(0.4f) * 0.1f;
        var scaleFactor = Utils.Remap(current.Distance(previous), 30f, 125f, 1f, 0.5f) * 0.6f;
        var fireBrightness = Main.rand.Next(20);
        var fireColor = new Color(fireBrightness, fireBrightness, fireBrightness);
        var bigColorColor = fireColor;

        if (MovementInterpolant <= 0.35f)
        {
            scaleFactor *= 2f;
            bigColorColor = new Color(0, 0, 0);
        }

        // Create fire.
        AntishadowFireParticleSystemManager.CreateNew
            (Projectile.owner, false, current, Main.rand.NextVector2Circular(7f, 7f) + directionalForce, Vector2.One * Main.rand.NextFloat(100f, 175f) * scaleFactor, bigColorColor);

        if (!current.WithinRange(previous, 40f))
        {
            var steps = (int)(current.Distance(previous) / 21f);

            for (float i = 0; i < steps; i++)
            {
                var loopFireColor = fireColor;

                if (i % 6 == 0)
                {
                    loopFireColor = new Color(174, 0, Main.rand.Next(16), 0);
                }

                var position = Vector2.Lerp(previous, current, i / steps);

                if (Main.rand.NextBool(3))
                {
                    AntishadowFireParticleSystemManager.CreateNew
                    (
                        Projectile.owner,
                        false,
                        position,
                        Main.rand.NextVector2Circular(27f, 27f) + directionalForce * 0.45f,
                        Vector2.One * Main.rand.NextFloat(30f, 175f) * scaleFactor,
                        loopFireColor
                    );
                }

                if (Main.rand.NextBool(8) && Main.GetProjectileDesiredShader(Projectile) == 0)
                {
                    var fire = Dust.NewDustPerfect(position, 261);
                    fire.velocity = Main.rand.NextVector2Circular(30f, 30f) - directionalForce * 0.4f;
                    fire.color = Color.Red;
                    fire.noGravity = true;
                    fire.scale *= 1.2f;
                }
            }
        }
    }

    /// <summary>
    ///     Moves the ropes responsible for the motion of this assassin's kasa hat beads.
    /// </summary>
    private void MoveBeadRopes()
    {
        var ropeGravity = 0.5f;
        var windSpeed = Math.Clamp(Main.WindForVisuals, -0.6f, 0.6f);
        var angularOffset = BowRotation * Projectile.spriteDirection;
        var force = Vector2.UnitX * (LumUtils.AperiodicSin(Time * 0.024f) * 0.4f + windSpeed);
        BeadRopeA.Update(Projectile.Center + new Vector2(Projectile.spriteDirection * -36f, -114f).RotatedBy(angularOffset) * Projectile.scale, ropeGravity, force);

        force = Vector2.UnitX * (LumUtils.AperiodicSin(Time * 0.024f + 5.1f) * 0.4f + windSpeed);
        BeadRopeB.Update(Projectile.Center + new Vector2(Projectile.spriteDirection * 26f, -114f).RotatedBy(angularOffset) * Projectile.scale, ropeGravity, force);

        force = Vector2.UnitX * (LumUtils.AperiodicSin(Time * 0.024f + 3.7f) * 0.4f + windSpeed);
        BeadRopeC.Update(Projectile.Center + new Vector2(Projectile.spriteDirection * -20f, -114f).RotatedBy(angularOffset) * Projectile.scale, ropeGravity, force);

        force = Vector2.UnitX * (LumUtils.AperiodicSin(Time * 0.024f + 1.5f) * 0.4f + windSpeed);
        BeadRopeD.Update(Projectile.Center + new Vector2(Projectile.spriteDirection * 15f, -114f).RotatedBy(angularOffset) * Projectile.scale, ropeGravity, force);
    }

    /// <summary>
    ///     Creates smoke around the feet of this assassin.
    /// </summary>
    private void CreateFootSmoke()
    {
        if (Main.dedServ)
        {
            return;
        }

        // Create idle smoke.
        var fireBrightness = Main.rand.Next(26);
        var fireSpeed = -2.67f;
        var fireColor = new Color(fireBrightness, fireBrightness, fireBrightness) * 0.3f;

        if (Main.rand.NextBool(12))
        {
            fireColor = new Color(175, 0, Main.rand.Next(16), 0);
        }

        var fireSpawnPosition = Projectile.Bottom + new Vector2(Main.rand.NextFloatDirection() * 36f, Main.rand.NextFloatDirection() * 2f - 28f).RotatedBy(Projectile.rotation) * Projectile.scale;
        var fireVelocity = fireSpawnPosition.SafeDirectionTo(Projectile.Bottom) * fireSpeed - Vector2.UnitY * 3f;

        if (Main.rand.NextBool(2))
        {
            for (var i = 0; i < 2; i++)
            {
                AntishadowFireParticleSystemManager.CreateNew
                    (Projectile.owner, true, fireSpawnPosition, fireVelocity, new Vector2(0.81f, 1f) * Main.rand.NextFloat(20f, 65f), fireColor * Projectile.Opacity);
            }
        }

        if (Main.rand.NextBool(48))
        {
            var ember = new EmberParticle(fireSpawnPosition, Main.rand.NextVector2Circular(4f, 1f) - Vector2.UnitY * Main.rand.NextFloat(10f), Color.Crimson * Projectile.Opacity, 2f, 120);
            ember.Spawn();
        }
    }

    /// <summary>
    ///     Randomly creates idle sounds, such as wind chimes.
    /// </summary>
    private void CreateIdleSounds()
    {
        // Don't play idle sounds if moving fast.
        if (!Projectile.position.WithinRange(Projectile.oldPosition, 10f))
        {
            return;
        }

        var windSpeed = Math.Clamp(Main.WindForVisuals, -1f, 1f);
        var windChimeCreationProbability = MathHelper.Lerp(0.0016f, 0.005f, MathF.Abs(windSpeed));

        if (Projectile.soundDelay <= 0 && Main.rand.NextBool(windChimeCreationProbability) && State != AssassinState.Leave)
        {
            attachedSounds.Add
            (
                SoundEngine.PlaySound
                    (
                        ChimeSound with
                        {
                            MaxInstances = 3
                        },
                        Projectile.Center
                    )
                    .WithVolumeBoost(0.1f)
            );

            Projectile.soundDelay = 300;
        }
    }

    /// <summary>
    ///     Updates looped sounds for this assassin.
    /// </summary>
    private void UpdateLoopedSounds()
    {
        AmbientLoop.Update
        (
            Projectile.Center,
            sound =>
            {
                var idealPitch = LumUtils.InverseLerp(6f, 30f, Projectile.position.Distance(Projectile.oldPosition)) * 0.8f;
                sound.Volume = 3f;
                sound.Pitch = MathHelper.Lerp(sound.Pitch, idealPitch, 0.6f);
            }
        );

        AttackLoop.Update
        (
            Projectile.Center,
            sound =>
            {
                sound.Volume = AttackLoopActivationInterpolant * 2f;
                sound.Pitch = MathHelper.SmoothStep(0f, 0.8f, AttackLoopActivationInterpolant);
            }
        );
    }

    /// <summary>
    ///     Ensures that all idle sounds generated by this assassin stay attached to it as it moves.
    /// </summary>
    private void KeepSoundsAttached()
    {
        for (var i = 0; i < attachedSounds.Count; i++)
        {
            if (!SoundEngine.TryGetActiveSound(attachedSounds[i], out var sound) || sound is null)
            {
                attachedSounds.RemoveAt(i);
                i--;

                continue;
            }

            sound.Position = Projectile.Center;
        }
    }

    /// <summary>
    ///     Makes this assassin disturb liquids as it moves.
    /// </summary>
    private void DisturbLiquids()
    {
        var wsd = (WaterShaderData)Filters.Scene["WaterDistortion"].GetShader();

        var disruptionAreaInterpolant = LumUtils.InverseLerp(6f, 54f, Projectile.position.Distance(Projectile.oldPosition));

        if (State == AssassinState.SliceTargetRepeatedly)
        {
            disruptionAreaInterpolant = 1f;
        }

        wsd.QueueRipple(Projectile.Center, Color.White, Projectile.Size * disruptionAreaInterpolant * 2f, RippleShape.Circle, Projectile.rotation);
    }

    /// <summary>
    ///     Handles the application of minion buffs for this assassin, making it vanish if the buffs are
    ///     not present or the owner player has died.
    /// </summary>
    private void HandleMinionBuffs()
    {
        Owner.AddBuff(ModContent.BuffType<AntishadowAssassinBuff>(), 3);
        var hasMinion = Owner.GetValueRef<bool>("HasAntishadowAssassin");

        if (Owner.dead)
        {
            hasMinion.Value = false;
        }

        if (hasMinion.Value)
        {
            Projectile.timeLeft = 2;
        }
        else if (ExistenceTimer >= 5)
        {
            SwitchState(AssassinState.Leave);
        }

        if (State == AssassinState.Leave)
        {
            hasMinion.Value = false;
        }
    }

    /// <summary>
    ///     Draws a katana for this assassin.
    /// </summary>
    private void DrawKatana(Vector2 bladeDrawStartingPosition, bool flip, float angle)
    {
        if (Main.dedServ)
        {
            return;
        }

        float katanaWidthFunction(float completionRatio)
        {
            var baseWidth = 4f;

            if (completionRatio <= (Projectile.spriteDirection == 1 ? 0.13f : 0.19f))
            {
                baseWidth = 6.7f;
            }

            return LumUtils.InverseLerp(0f, 0.6f, Projectile.Opacity) * Projectile.scale * baseWidth;
        }

        Color katanaColorFunction(float completionRatio)
        {
            return Color.Red * Projectile.Opacity.Squared();
        }

        var appearanceInterpolant = KatanaUnsheathInterpolant;
        var katanaShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowKatanaShader");
        katanaShader.TrySetParameter("flip", flip);
        katanaShader.TrySetParameter("appearanceInterpolant", appearanceInterpolant);
        katanaShader.SetTexture(MiscTexturesRegistry.TurbulentNoise.Value, 1, SamplerState.PointWrap);

        PrimitiveSettings katanaPrimitiveSettings = new
        (
            katanaWidthFunction,
            katanaColorFunction,
            Shader: katanaShader,
            ProjectionAreaWidth: (int)WotGUtils.ViewportSize.X,
            ProjectionAreaHeight: (int)WotGUtils.ViewportSize.Y,
            UseUnscaledMatrix: true
        );

        var katanaReach = angle.ToRotationVector2() * appearanceInterpolant * Projectile.spriteDirection * MathF.Sqrt(Projectile.scale) * -138f;
        var orthogonalOffset = (angle + flip.ToDirectionInt() * -MathHelper.PiOver2).ToRotationVector2() * appearanceInterpolant * 30f;

        var katanaPositions = new Vector2[8];

        for (var i = 0; i < katanaPositions.Length; i++)
        {
            var completionRatio = i / (katanaPositions.Length - 1f);
            katanaPositions[i] = bladeDrawStartingPosition + katanaReach * completionRatio + Main.screenPosition;
            katanaPositions[i] += orthogonalOffset * completionRatio.Squared();
        }

        PrimitiveRenderer.RenderTrail(katanaPositions, katanaPrimitiveSettings, 40);
    }

    /// <summary>
    ///     Draws the main body for the purpose of rendering into the RT responsible for this assassin's
    ///     body.
    /// </summary>
    private void DrawIntoBodyTarget()
    {
        if (!Main.dedServ)
        {
            DrawRobe();
        }

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, LumUtils.CullOnlyScreen);

        var body = BodyTexture.Value;
        var legs = LegsTexture.Value;
        var arm = ArmTexture.Value;
        var armXOrigin = 32;
        var drawPosition = WotGUtils.ViewportSize * 0.5f;

        if (MathF.Abs(BowRotation) >= 0.01f)
        {
            arm = ArmBowTexture.Value;
            armXOrigin += 4;
        }

        // Draw the right arm behind the body.
        var rightArmDrawPosition = drawPosition + new Vector2(6f, -80f).RotatedBy(BowRotation);
        Main.spriteBatch.Draw(arm, rightArmDrawPosition, null, Color.White, RightArmRotation, new Vector2(arm.Width - armXOrigin, 10f), ArmScale, SpriteEffects.FlipHorizontally, 0f);

        // Draw the body.
        Main.spriteBatch.Draw(body, drawPosition, null, Color.White, BowRotation, body.Size() * new Vector2(0.5f, 1f), 1f, 0, 0f);

        // Draw legs.
        Main.spriteBatch.Draw(legs, drawPosition, null, Color.White, 0f, new Vector2(32f, 16f), 1f, 0, 0f);

        // Draw the left arm.
        var leftArmDrawPosition = drawPosition + new Vector2(-14f, -80f).RotatedBy(BowRotation);
        Main.spriteBatch.Draw(arm, leftArmDrawPosition, null, Color.White, LeftArmRotation, new Vector2(armXOrigin, 10f), ArmScale, 0, 0f);

        Main.spriteBatch.End();
    }

    /// <summary>
    ///     Draws the arm outlines into their designated RT.
    /// </summary>
    private void DrawIntoArmOutlineTarget()
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, LumUtils.CullOnlyScreen);

        var arm = ArmOutlineTexture.Value;
        var armXOrigin = 32;
        var drawPosition = WotGUtils.ViewportSize * 0.5f;

        if (MathF.Abs(BowRotation) >= 0.01f)
        {
            arm = ArmBowOutlineTexture.Value;
            armXOrigin += 4;
        }

        // Draw the right arm behind the body.
        var rightArmDrawPosition = drawPosition + new Vector2(6f, -80f).RotatedBy(BowRotation);
        Main.spriteBatch.Draw(arm, rightArmDrawPosition, null, Color.White, RightArmRotation, new Vector2(arm.Width - armXOrigin, 10f), ArmScale, SpriteEffects.FlipHorizontally, 0f);

        // Draw the left arm.
        var leftArmDrawPosition = drawPosition + new Vector2(-14f, -80f).RotatedBy(BowRotation);
        Main.spriteBatch.Draw(arm, leftArmDrawPosition, null, Color.White, LeftArmRotation, new Vector2(armXOrigin, 10f), ArmScale, 0, 0f);

        Main.spriteBatch.End();
    }

    /// <summary>
    ///     Draws beads and rope attached to the kasa hat for this assassin.
    /// </summary>
    private void DrawBeads(Vector2 center)
    {
        Texture2D whitePixel = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
        Texture2D beadTextureA = GennedAssets.Textures.SecondPhaseForm.Beads3;
        Texture2D beadTextureB = GennedAssets.Textures.SecondPhaseForm.Beads2;
        Texture2D beadTextureC = GennedAssets.Textures.SecondPhaseForm.Bell;
        Texture2D beadTextureD = GennedAssets.Textures.SecondPhaseForm.Beads1;
        var ropeColor = new Color(210, 13, 16) * Projectile.Opacity;
        var drawOffset = center - Projectile.Center;

        BeadRopeA?.DrawProjection
        (
            whitePixel,
            drawOffset,
            false,
            _ => ropeColor,
            widthFactor: Projectile.scale,
            projectionWidth: (int)WotGUtils.ViewportSize.X,
            projectionHeight: (int)WotGUtils.ViewportSize.Y,
            unscaledMatrix: true
        );

        BeadRopeB?.DrawProjection
        (
            whitePixel,
            drawOffset,
            false,
            _ => ropeColor,
            widthFactor: Projectile.scale,
            projectionWidth: (int)WotGUtils.ViewportSize.X,
            projectionHeight: (int)WotGUtils.ViewportSize.Y,
            unscaledMatrix: true
        );

        BeadRopeC?.DrawProjection
        (
            whitePixel,
            drawOffset,
            false,
            _ => ropeColor,
            widthFactor: Projectile.scale,
            projectionWidth: (int)WotGUtils.ViewportSize.X,
            projectionHeight: (int)WotGUtils.ViewportSize.Y,
            unscaledMatrix: true
        );

        BeadRopeD?.DrawProjection
        (
            whitePixel,
            drawOffset,
            false,
            _ => ropeColor,
            widthFactor: Projectile.scale,
            projectionWidth: (int)WotGUtils.ViewportSize.X,
            projectionHeight: (int)WotGUtils.ViewportSize.Y,
            unscaledMatrix: true
        );

        if (BeadRopeA is not null)
        {
            var beadRotation = (BeadRopeA.Rope[^2].Position - BeadRopeA.Rope[^1].Position).ToRotation() + MathHelper.PiOver2;
            Main.spriteBatch.Draw(beadTextureA, BeadRopeA.Rope[^5].Position + drawOffset, null, Color.White * Projectile.Opacity, beadRotation, new Vector2(24f, 6f), Projectile.scale * 0.08f, 0, 0f);
        }

        if (BeadRopeB is not null)
        {
            var beadRotation = (BeadRopeB.Rope[^2].Position - BeadRopeB.Rope[^1].Position).ToRotation() + MathHelper.PiOver2;
            Main.spriteBatch.Draw(beadTextureB, BeadRopeB.Rope[^4].Position + drawOffset, null, Color.White * Projectile.Opacity, beadRotation, new Vector2(30f, 243f), Projectile.scale * 0.1f, 0, 0f);
        }

        if (BeadRopeC is not null)
        {
            var beadRotation = (BeadRopeC.Rope[^2].Position - BeadRopeC.Rope[^1].Position).ToRotation() + MathHelper.PiOver2;
            Main.spriteBatch.Draw(beadTextureC, BeadRopeC.Rope[^4].Position + drawOffset, null, Color.White * Projectile.Opacity, beadRotation, new Vector2(45f, 41f), Projectile.scale * 0.1f, 0, 0f);
        }

        if (BeadRopeD is not null)
        {
            var beadRotation = (BeadRopeD.Rope[^2].Position - BeadRopeD.Rope[^1].Position).ToRotation() + MathHelper.PiOver2;
            Main.spriteBatch.Draw(beadTextureD, BeadRopeD.Rope[^4].Position + drawOffset, null, Color.White * Projectile.Opacity, beadRotation, new Vector2(17f, 46f), Projectile.scale * 0.1f, 0, 0f);
        }
    }

    /// <summary>
    ///     Renders this assassin's robe.
    /// </summary>
    private void DrawRobe()
    {
        var world = Matrix.CreateTranslation(-Projectile.Center.X + WotGUtils.ViewportSize.X * 0.5f, -Projectile.Center.Y + WotGUtils.ViewportSize.Y * 0.5f, 0f);
        var projection = Matrix.CreateOrthographicOffCenter(0f, WotGUtils.ViewportSize.X, WotGUtils.ViewportSize.Y, 0f, -1000f, 1000f);
        var matrix = world * projection;

        var clothShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssasinRobeShader");
        clothShader.TrySetParameter("opacity", LumUtils.InverseLerp(60f, 120f, ExistenceTimer));
        clothShader.TrySetParameter("transform", matrix);
        clothShader.Apply();

        Robe.Render();
    }

    /// <summary>
    ///     Draws the mask for this assassin.
    /// </summary>
    private void DrawMask(Vector2 center)
    {
        GetMaskInfo(out var mask, out _);

        if (mask is null)
        {
            return;
        }

        var faceDrawPosition = MaskPosition - Projectile.Center + center - Vector2.UnitY.RotatedBy(BowRotation * Projectile.spriteDirection) * 30f;

        Main.spriteBatch.Draw
        (
            mask,
            faceDrawPosition,
            null,
            Color.White * Projectile.Opacity,
            BowRotation * Projectile.spriteDirection,
            mask.Size() * 0.5f,
            Projectile.scale,
            Projectile.spriteDirection.ToSpriteDirection(),
            0f
        );
    }

    /// <summary>
    ///     Draws the kasa hat for this assassin.
    /// </summary>
    private void DrawHat(Vector2 center)
    {
        if (Main.specialSeedWorld) { }
        Texture2D hat = KasaTexture.Value;
        //its really bad, especially in comparison to the normal system
        // i don't really care 
        DateTime date = DateTime.Now;
        if (date.Month == 12 || (date.Day == 24 || date.Day == 25))
        {
            //hat = GennedAssets.Textures.NamelessDeity.SantaHat;
        }
        var hatDrawPosition = center + new Vector2(Projectile.spriteDirection * -4f, -128f).RotatedBy(BowRotation * Projectile.spriteDirection) * Projectile.scale;

        Vector2 scale;
        if (date.Month == 12 || (date.Day == 24 || date.Day == 25))
        {
            scale = new Vector2(0.2f, 0.1f) * Projectile.scale;
        }
        //else
        scale = new Vector2(Projectile.scale);

        Main.spriteBatch.Draw
        (
            hat,
            hatDrawPosition,
            null,
            Color.White * Projectile.Opacity,
            BowRotation * Projectile.spriteDirection,
            hat.Size() * 0.5f,
            scale,
            Projectile.spriteDirection.ToSpriteDirection() | SpriteEffects.FlipHorizontally,
            0f
        );
    }

    /// <summary>
    ///     Renders this assassin's contents into the final target.
    /// </summary>
    private void RenderIntoResultsTarget()
    {
        if (Main.dedServ)
        {
            return;
        }

        ArmOutlineTarget.Request(400, 400, Projectile.identity, DrawIntoArmOutlineTarget);
        BodyTarget.Request(400, 400, Projectile.identity, DrawIntoBodyTarget);

        if (BodyTarget.TryGetTarget(Projectile.identity, out var bodyTarget) &&
            bodyTarget is not null &&
            ArmOutlineTarget.TryGetTarget(Projectile.identity, out var outlineTarget) &&
            outlineTarget is not null)
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            var center = WotGUtils.ViewportSize * 0.5f;

            var baseRotation = 0f;
            var leftKatanaAngle = 0.15f;
            var rightKatanaAngle = Projectile.spriteDirection * 0.3f + 0.15f;

            var rightShoulderPosition = center + new Vector2(6f, -80f).RotatedBy(baseRotation + BowRotation * Projectile.spriteDirection) * Projectile.scale;
            var rightHandEnd = rightShoulderPosition + new Vector2(16f, 110f).RotatedBy(baseRotation + RightArmRotation) * Projectile.scale * ArmScale;
            var leftShoulderPosition = center + new Vector2(-14f, -80f).RotatedBy(baseRotation + BowRotation * Projectile.spriteDirection) * Projectile.scale;
            var leftHandEnd = leftShoulderPosition + new Vector2(-5f, 110f).RotatedBy(baseRotation + LeftArmRotation) * Projectile.scale * ArmScale;

            var katanaRotationOffset = Projectile.spriteDirection * KatanaRotation;
            DrawKatana(rightHandEnd, true, rightKatanaAngle + katanaRotationOffset);
            DrawKatana(leftHandEnd, true, leftKatanaAngle + katanaRotationOffset);

            Vector3[] palette =
            [
                new(1.5f),
                new(0f, 1f, 1.2f),
                new(1f, 0f, 0f)
            ];

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            var edgeColor = new Color(255, 0, 0);

            var overlayShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssassinColorProcessingShader");
            overlayShader.TrySetParameter("eyeScale", LumUtils.Cos01(Main.GlobalTimeWrappedHourly * 7.5f + Projectile.identity) * 0.2f + 1f);
            overlayShader.TrySetParameter("gradient", palette);
            overlayShader.TrySetParameter("gradientCount", palette.Length);
            overlayShader.TrySetParameter("textureSize", bodyTarget.Size());
            overlayShader.TrySetParameter("edgeColor", edgeColor.ToVector4() * Projectile.Opacity);
            overlayShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
            overlayShader.Apply();

            Main.spriteBatch.Draw(bodyTarget, center, null, Color.Black * Projectile.Opacity, 0f, bodyTarget.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection.ToSpriteDirection(), 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

            Main.spriteBatch.Draw
                (outlineTarget, center, null, edgeColor * Projectile.Opacity * ArmOutlineOpacity, 0f, outlineTarget.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection.ToSpriteDirection(), 0);

            DrawMask(center);
            DrawBeads(center);
            DrawHat(center);

            Main.spriteBatch.End();
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (Main.netMode == NetmodeID.Server)
        {
            return false;
        }

        ResultsTarget.Request(2000, 2000, Projectile.identity, RenderIntoResultsTarget);

        if (ResultsTarget.TryGetTarget(Projectile.identity, out var target) && target is not null)
        {
            var postProcessingShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssassinPostProcessingShader");
            postProcessingShader.TrySetParameter("blurOffset", BlurIntensity * 0.2f);
            postProcessingShader.TrySetParameter("blurDirection", Projectile.velocity.SafeNormalize(Vector2.UnitX));
            postProcessingShader.TrySetParameter("disappearanceInterpolant", DisappearanceInterpolant);
            postProcessingShader.TrySetParameter("direction", (float)Projectile.spriteDirection);
            postProcessingShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
            postProcessingShader.Apply();

            Main.spriteBatch.Draw(target, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, target.Size() * 0.5f, 1f, 0, 0);
        }

        return false;
    }

    public override void OnKill(int timeLeft)
    {
        for (var i = 0; i < attachedSounds.Count; i++)
        {
            if (!SoundEngine.TryGetActiveSound(attachedSounds[i], out var sound) || sound is null)
            {
                continue;
            }

            sound.Stop();
        }
    }

    public override bool? CanDamage()
    {
        return Projectile.scale >= 0.56f && DoesDamage;
    }
}