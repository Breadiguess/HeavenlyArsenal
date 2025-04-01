
using HeavenlyArsenal.Content.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;


namespace HeavenlyArsenal.Content.Projectiles.Weapons.Melee.AvatarSpear;

public class AvatarLonginusHeld : ModProjectile
{
    public ref Player Player => ref Main.player[Projectile.owner];

    public bool IsEmpowered { get; set; }

    public enum AttackStates
    {
        Idle,
        ThrowTeleport,

        RapidStabs,
        HeavyStab,
        WhipSlash,

        // Empowered attacks
        // RapidStabs
        //SecondSlash,
        SuperHeavyThrust,
        RipOut,
        Castigation
    }

    public override void SetStaticDefaults()
    {
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
        Projectile.localNPCHitCooldown = 20;
    }

    public ref float Time => ref Projectile.ai[0];
    public ref float AttackState => ref Projectile.ai[1];

    public bool canHit;
    public bool throwMode;

    public override void AI()
    {
        Projectile.extraUpdates = 3;
        Projectile.timeLeft = 2;

        if (Player.HeldItem.type != ModContent.ItemType<AvatarLonginus>() || Player.CCed || Player.dead)
        {
            Projectile.Kill();
            return;
        }

        Player.heldProj = Projectile.whoAmI;

        throwMode = false;
        canHit = false;
        Vector2 offset = Vector2.Zero;
        Vector2 handPosition = Player.MountedCenter;
        float attackSpeed = Player.GetAttackSpeed(DamageClass.Melee) * (1f + Projectile.extraUpdates * 0.15f);

        if (AttackState != (int)AttackStates.Idle)
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
            case (int)AttackStates.Idle:

                Time = 0;

                Projectile.scale = 1f;
                Projectile.velocity = Vector2.Zero;
                float motionBob = Player.velocity.X * 0.02f - Player.velocity.Y * 0.015f * Player.direction;
                Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Player.fullRotation - MathHelper.PiOver2 + 1f * Player.direction + motionBob, 0.1f);
                Projectile.spriteDirection = Player.direction;

                Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, -MathHelper.PiOver2 * Player.direction + motionBob * 1.2f);
                Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, -0.3f * Player.direction + motionBob * 0.3f);
                handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.ThreeQuarters, -0.3f * Player.direction);

                if (Player.controlUseItem)
                    AttackState = (int)AttackStates.RapidStabs;

                break;

            case (int)AttackStates.RapidStabs:

                Player.SetDummyItemTime(5);

                const int RapidWindUp = 50;
                const int RapidStabCount = 4;
                const int RapidWindDown = 50;

                int RapidStabTime = 20 + (RapidStabCount + 3) * 5 + (int)(50 / attackSpeed);

                if (Time < RapidWindUp)
                {
                    float windProgress = Time / (RapidWindUp - 1f);

                    float wiggle = MathF.Sin(MathF.Pow(windProgress, 2f) * MathHelper.Pi) * -0.4f * Projectile.direction;
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() + wiggle, 0.9f);
                    offset = new Vector2(MathHelper.SmoothStep(0, -50, windProgress), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f - windProgress * 0.33f;

                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, Projectile.rotation - MathHelper.PiOver2);
                    handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.None, Projectile.rotation - MathHelper.PiOver2);
                }

                else if (Time < RapidWindUp + RapidStabTime - 1) // -1 here because the modulo is a bit quirky
                {
                    float windDownProgress = Utils.GetLerpValue(RapidWindDown, 0f, Time - RapidWindUp - RapidStabTime, true);
                    if (Time < RapidWindUp + RapidStabTime)
                        canHit = true;

                    float stabProgress = Utils.GetLerpValue(0, RapidStabTime / RapidStabCount, (Time - RapidWindUp) % (RapidStabTime / RapidStabCount));
                    float stabCurve = Utils.GetLerpValue(0, 0.7f, stabProgress, true);

                    if (Time % (RapidStabTime / RapidStabCount) == 0)
                    {
                        SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.IntroScreenSlice with { Pitch = 1f, MaxInstances = 0 }, Projectile.Center);
                        if (Main.myPlayer == Projectile.owner)
                        {
                            float accuracy = Utils.GetLerpValue(RapidStabTime * 1.5f, 0, Time - RapidWindUp, true);
                            Projectile.velocity = Player.DirectionTo(Main.MouseWorld).RotatedByRandom(accuracy) * 20f;
                            Projectile.direction = Projectile.velocity.X > 0 ? 1 : -1;
                            Projectile.netUpdate = true;
                        }
                    }

                    Projectile.rotation = Projectile.velocity.ToRotation();
                    offset = new Vector2(stabCurve * 200 - 50, 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f + stabCurve * 0.5f;

                    int handSwingDir = (int)(Utils.GetLerpValue(0, RapidStabTime, Time - RapidWindUp, true) * RapidStabCount) % 2 > 0 ? 1 : -1;
                    float handRot = Projectile.rotation - MathHelper.PiOver2 + (1.5f - stabProgress) * handSwingDir * Player.direction;
                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, handRot);
                    handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, handRot);
                }
                else
                {
                    float windDownProgress = Utils.GetLerpValue(RapidWindDown / 4f, RapidWindDown, Time - RapidWindUp - RapidStabTime, true);
                    offset = new Vector2(150, 0).RotatedBy(Projectile.rotation) * MathF.Cbrt(1f - windDownProgress);
                    Projectile.scale = 1.5f - MathF.Pow(windDownProgress, 3);

                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
                    handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
                }

                Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                Time++;

                if (Time > RapidWindUp + RapidStabTime + RapidWindDown / 2)
                {
                    if (Player.controlUseItem)
                    {
                        AttackState = (int)AttackStates.WhipSlash;
                        Time = 0;
                    }
                    else if (Time > RapidWindUp + RapidStabTime + RapidWindDown)
                    {
                        AttackState = (int)AttackStates.Idle;
                        Time = 0;
                    }
                }

                break;

            case (int)AttackStates.WhipSlash:

                const int SlashWindUp = 30;
                int SlashTime = 35 + (int)(30 / attackSpeed);
                int SlashWindDown = 20 + (int)(30 / attackSpeed);
                float SlashRotation = MathHelper.ToRadians(190);

                if (Time < SlashWindUp)
                {
                    float windProgress = Time / (SlashWindUp - 1f);

                    float wiggle = -MathF.Sqrt(windProgress) * SlashRotation * Projectile.direction;
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() + wiggle, 0.9f);
                    offset = new Vector2(MathHelper.SmoothStep(0, -30, windProgress), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f - windProgress * (1 - windProgress);
                }
                else
                {
                    if (Time < SlashWindUp + SlashTime)
                        canHit = true;

                    if (Time == SlashWindUp + 1)
                        SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.DaggerBurst with { MaxInstances = 0 }, Projectile.Center);

                    if (Time == SlashWindUp + SlashTime - 1)
                        SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.SliceTelegraph with { MaxInstances = 0 }, Projectile.Center);

                    float slashProgress = Utils.GetLerpValue(0, SlashTime, Time - SlashWindUp, true);
                    float windDown = Utils.GetLerpValue(0, SlashWindDown, Time - SlashWindUp - SlashTime, true);

                    float slashCurve = MathF.Pow(slashProgress, 1.5f);

                    float currentSlashRot = MathHelper.Lerp(-SlashRotation, SlashRotation * 0.2f + MathF.Pow(windDown, 4f) * 0.2f, slashCurve) * Projectile.direction;
                    Projectile.rotation = Projectile.velocity.ToRotation() + currentSlashRot;

                    offset = new Vector2(-30 + 180 * MathF.Sin(slashProgress * MathHelper.PiOver2) * (1f - MathF.Pow(windDown, 2f)), 0).RotatedBy(Projectile.rotation);

                    Projectile.scale = 1f + slashProgress * (1f - MathF.Sqrt(windDown));
                }

                Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
                handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
                Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
                Time++;

                if (Time > SlashWindUp + SlashTime + SlashWindDown)
                {
                    if (Player.controlUseItem)
                    {
                        AttackState = (int)AttackStates.HeavyStab;
                        Time = 0;
                    }
                    else if (Time > SlashTime)
                    {
                        AttackState = (int)AttackStates.Idle;
                        Time = 0;
                    }
                }

                break;

            case (int)AttackStates.HeavyStab:

                const int HeavyWindUp = 80;
                int HeavyThrustTime = 10 + (int)(30 / Player.GetAttackSpeed(DamageClass.Melee));
                int HeavyWindDown = 10 + (int)(20 / Player.GetAttackSpeed(DamageClass.Melee));

                if (Time < HeavyWindUp)
                {
                    float windProgress = Time / (HeavyWindUp - 1f);

                    float wiggle = (windProgress * -0.5f - 0.5f) * Projectile.direction;
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() + wiggle, windProgress);
                    offset = new Vector2(MathHelper.SmoothStep(0, -80, windProgress), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f;

                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, Projectile.rotation + MathHelper.PiOver2);
                    handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.None, Projectile.rotation + MathHelper.PiOver2);
                }
                else
                {
                    if (Time == HeavyWindUp + 1)
                        SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.DaggerGrazeEcho with { Pitch = 0.5f, MaxInstances = 0 }, Projectile.Center);

                    canHit = true;
                    float thrustProgress = Utils.GetLerpValue(0, HeavyThrustTime, Time - HeavyWindUp, true);
                    float windDown = Utils.GetLerpValue(0, HeavyWindDown, Time - HeavyWindUp - HeavyThrustTime, true);

                    float thrustCurve = Utils.GetLerpValue(0, 0.2f, thrustProgress, true);
                    float windDownCurve = MathF.Cbrt(windDown);

                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() - (thrustCurve - 1f) * 0.3f * Projectile.direction, 0.5f - thrustCurve * 0.2f);
                    offset = new Vector2(MathHelper.SmoothStep(0, 150, thrustCurve) * (1f - windDownCurve), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1.5f - windDownCurve * 0.5f;

                    Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2 * thrustProgress);
                    handPosition = Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2 * thrustProgress);
                }

                Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                Time++;

                if (Time > HeavyWindUp + HeavyThrustTime + HeavyWindDown - 5)
                {
                    if (Player.controlUseItem)
                    {
                        AttackState = (int)AttackStates.Idle;
                        Time = 0;
                    }
                    else if (Time > HeavyWindUp + HeavyThrustTime - HeavyWindDown)
                    {
                        AttackState = (int)AttackStates.Idle;
                        Time = 0;
                    }
                }

                break;

            case (int)AttackStates.ThrowTeleport:

                const int ThrowWindUp = 30;
                const int ThrowTime = 50;
                const int TPTime = 30;

                Player.velocity *= 0.9f;
                Player.SetImmuneTimeForAllTypes(60);

                if (Time < ThrowWindUp)
                {
                    Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                    float windProgress = Time / (ThrowWindUp - 1f);

                    float wiggle = (windProgress * -0.5f - 0.5f) * Projectile.direction;
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() + wiggle, windProgress);
                    offset = new Vector2(MathHelper.SmoothStep(0, -80, windProgress), 0).RotatedBy(Projectile.rotation);
                    Projectile.scale = 1f;
                }
                else if (Time < ThrowWindUp + ThrowTime)
                {
                    if (Time == ThrowWindUp)
                    {
                        SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.DaggerBurst with { Pitch = 1f, MaxInstances = 0 }, Projectile.Center);
                        Projectile.Center = Player.MountedCenter;
                    }

                    Projectile.extraUpdates = 8;

                    canHit = true;
                    throwMode = true;
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 16;
                    Projectile.rotation = Projectile.velocity.ToRotation();

                    if (Collision.SolidCollision(Projectile.Center - new Vector2(10) + Projectile.velocity, 20, 20) && Time < ThrowWindUp + ThrowTime)
                    {
                        SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
                        Time = ThrowWindUp + ThrowTime;
                        Projectile.velocity *= 0.01f;
                    }
                }
                else
                {
                    Projectile.velocity *= 0.9f;

                    if (Main.myPlayer == Projectile.owner)
                        Main.SetCameraLerp(0.1f, 20);

                    canHit = true;
                    throwMode = true;
                    Player.Center = Vector2.Lerp(Player.Center, Projectile.Center, 0.15f);
                }

                Time++;

                if (Time > ThrowWindUp + ThrowTime + TPTime)
                {
                    if (Player.controlUseItem)
                    {
                        AttackState = (int)AttackStates.HeavyStab;
                        Time = 0;
                    }
                    else if (Time > ThrowWindUp + ThrowTime + TPTime)
                    {
                        AttackState = (int)AttackStates.Idle;
                        Time = 0;
                    }
                }

                break;
        }

        if (!throwMode)
            Projectile.Center = Player.RotatedRelativePoint(handPosition) + offset - Projectile.velocity;

        if (canHit)
        {
            for (int i = 0; i < 3; i++)
            {
                int size = (int)(110 * MathF.Exp(-i));
                Projectile.EmitEnchantmentVisualsAt(Projectile.Center + new Vector2((120 + 40 * i) * Projectile.scale, 0).RotatedBy(Projectile.rotation) - new Vector2(size / 2), size, size);
            }
        }
    }

    public void HandleEmpowerment()
    {

    }

    public override void OnKill(int timeLeft)
    {
    }

    public override bool? CanCutTiles() => false;

    public override void ModifyDamageHitbox(ref Rectangle hitbox)
    {
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (canHit)
        {
            Vector2 offset = new Vector2(150 * Projectile.scale, 0).RotatedBy(Projectile.rotation);
            float _ = 0;
            return Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), Projectile.Center - offset, Projectile.Center + offset, 100f, ref _);
        }    

        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        //Add to percentage. probably add a check for if the next hit should restore the spear to normal/empowered state


    }

    //public string StringTexture = "HeavenlyArsenal/Content/Projectiles/Weapons/Melee/AvatarSpear/AvatarSpear_Lantern_String";
    //public string LanternTexture = "HeavenlyArsenal/Content/Projectiles/Weapons/Melee/AvatarSpear/AvatarSpear_Lantern";
    //public string SpearTexture = "HeavenlyArsenal/Content/Projectiles/Weapons/Melee/AvatarSpear/AvatarSpear_Holdout";
    //public string EmpoweredTexture = "HeavenlyArsenal/Content/Projectiles/Weapons/Melee/AvatarSpear/AvatarSpear_Holdout_Empowered";

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        int direction = Projectile.spriteDirection;
        SpriteEffects flipEffect = direction > 0 ? 0 : SpriteEffects.FlipVertically;
        int gripDistance = 30;
        Vector2 origin = new Vector2(texture.Width / 2 - gripDistance, texture.Height / 2 + (gripDistance + 2) * Player.gravDir * direction);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, texture.Frame(), Color.White, Projectile.rotation + MathHelper.PiOver4 * direction, origin, Projectile.scale, flipEffect, 0);

        return false;
    }
}
