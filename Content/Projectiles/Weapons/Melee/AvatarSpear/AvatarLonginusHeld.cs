
using HeavenlyArsenal.Content.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
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
        RapidStabs,
        LungeStab,
        // Another
        WhipSlash,

        // Empowered attacks
        Empowered_RapidStabs,
        Empowered_HeavyThrust,
        Empowered_RipOut,
        Empowered_Castigation
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

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 8;

        Projectile.extraUpdates = 2;
    }

    public ref float Time => ref Projectile.ai[0];
    public ref float AttackState => ref Projectile.ai[1];

    public bool canCollide;

    public override void AI()
    {
        Projectile.timeLeft = 2;
        if (Player.HeldItem.type != ModContent.ItemType<AvatarLonginus>() || Player.CCed || Player.dead)
        {
            Projectile.Kill();
            return;
        }

        Player.heldProj = Projectile.whoAmI;

        bool enchantments = false;
        Vector2 offset = Vector2.Zero;
        canCollide = false;

        switch (AttackState)
        {
            default:
            case (int)AttackStates.Idle:

                Time = 0;

                Projectile.scale = 0.7f;
                Projectile.velocity = Vector2.Zero;
                Projectile.rotation = Utils.AngleLerp(Projectile.rotation, -MathHelper.PiOver2 + 1f * Player.direction + Player.velocity.X * 0.015f + Player.velocity.Y * 0.01f * Player.direction, 0.33f);
                Projectile.spriteDirection = Player.direction;

                Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -MathHelper.PiOver4 * Player.direction);
                Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, -MathHelper.PiOver2 * Player.direction);
                Projectile.Center = Player.RotatedRelativePoint(Player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, -MathHelper.PiOver4 * Player.direction));

                if (Player.controlUseItem)
                {
                    AttackState = (int)AttackStates.RapidStabs;
                }

                break;

            case (int)AttackStates.RapidStabs:

                Player.SetDummyItemTime(5);
                enchantments = true;

                const int WindUp = 50;
                const int StabCount = 3;
                const int WindDown = 50;

                int StabTime = 10 + (int)(50 * Player.GetAttackSpeed(DamageClass.Melee));

                if (Time < 2)
                {
                    if (Main.myPlayer == Projectile.owner)
                    {
                        Projectile.velocity = Projectile.DirectionTo(Main.MouseWorld) * 20f;
                        Projectile.netUpdate = true;
                    }
                }

                if (Time < WindUp)
                {
                    float windProgress = Time / (WindUp - 1f);

                    float wiggle = MathF.Sin(MathF.Pow(windProgress, 2f) * MathHelper.Pi) * -0.4f * Projectile.direction;
                    Projectile.rotation = Utils.AngleLerp(Projectile.rotation, Projectile.velocity.ToRotation() + wiggle, 0.9f);
                    offset = new Vector2(MathHelper.SmoothStep(0, -50, windProgress), 0).RotatedBy(Projectile.rotation);

                    Projectile.scale = 1f - windProgress * 0.33f;
                }
                else if (Time < WindUp + StabTime)
                {
                    float windDownProgress = Utils.GetLerpValue(WindDown, 0f, Time - WindUp - StabTime, true);
                    if (Time < WindUp + StabTime)
                        canCollide = true;

                    float stabProgress = Utils.GetLerpValue(0, StabTime / StabCount, (Time - WindUp) % (StabTime / StabCount));
                    float stabCurve = MathF.Cbrt(stabProgress);

                    if (Time % (StabTime / StabCount) == 0)
                    {
                        if (Main.myPlayer == Projectile.owner)
                        {
                            Projectile.velocity = Projectile.DirectionTo(Main.MouseWorld).RotatedByRandom(0.2f) * 20f;
                            Projectile.netUpdate = true;
                        }
                    }

                    Projectile.rotation = Projectile.velocity.ToRotation();

                    offset = new Vector2(stabCurve * 200 - 50, 0).RotatedBy(Projectile.rotation);

                    Projectile.scale = 1f + stabCurve * 0.5f;
                }
                else
                {
                    float windDownProgress = Utils.GetLerpValue(WindDown / 4f, WindDown, Time - WindUp - StabTime, true);
                    offset = new Vector2(150, 0).RotatedBy(Projectile.rotation) * MathF.Sqrt(1f - windDownProgress);
                    Projectile.scale = 1.5f - windDownProgress;
                }

                Player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

                Time++;

                if (Time > WindUp + StabTime + WindDown)
                {
                    Time = 0;

                    if (Player.controlUseItem)
                        AttackState = (int)AttackStates.RapidStabs;
                    else
                        AttackState = (int)AttackStates.Idle;
                }

                break;
        }

        Projectile.Center = Player.RotatedRelativePoint(Player.MountedCenter) + offset - Projectile.velocity;
        SetAnimation(Player);

        if (enchantments)
            Projectile.EmitEnchantmentVisualsAt(Projectile.Center + new Vector2(150 * Projectile.scale, 0).RotatedBy(Projectile.rotation) - new Vector2(60), 120, 120);
    }

    public void SetAnimation(Player player)
    {
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
        hitbox.Width = (int)(200 * Projectile.scale);
        hitbox.Height = (int)(200 * Projectile.scale);
        hitbox.Location = (Projectile.Center + new Vector2(130 * Projectile.scale, 0).RotatedBy(Projectile.rotation) - hitbox.Size() / 2).ToPoint();
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (canCollide)
            return base.Colliding(projHitbox, targetHitbox);

        return false;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        //Add to percentage. probably add a check for if the next hit should restore the spear to normal/empowered state
        base.OnHitNPC(target, hit, damageDone);
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
        const int gripDistance = 30;
        Vector2 origin = new Vector2(texture.Width / 2 - gripDistance, texture.Height / 2 + (gripDistance + 2) * Player.gravDir * direction);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, texture.Frame(), Color.White, Projectile.rotation + MathHelper.PiOver4 * direction, origin, Projectile.scale, flipEffect, 0);

        return false;
    }

    private void DrawHitbox()
    {
        Rectangle hitBox = Projectile.Hitbox;
        ModifyDamageHitbox(ref hitBox);
        hitBox.Location -= Main.screenPosition.ToPoint();
        Main.spriteBatch.Draw(TextureAssets.BlackTile.Value, hitBox, Color.Red * 0.5f);
    }
}
