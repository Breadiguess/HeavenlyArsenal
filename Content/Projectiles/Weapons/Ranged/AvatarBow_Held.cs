using CalamityMod.Items.Weapons.Ranged;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria;
using Terraria.ModLoader;
using HeavenlyArsenal.Content.Items.Weapons.Ranged;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod;
using Terraria.ID;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Ranged
{
    class AvatarBow_Held : ModProjectile
    {
        public bool InUse => Owner.controlUseItem && Owner.altFunctionUse == 0;
        public Player Owner => Main.player[Projectile.owner];
        public bool OwnerCanShoot => Owner.HasAmmo(Owner.ActiveItem()) && !Owner.noItems && !Owner.CCed;
        public Vector2 topStringOffset => new Vector2(-23f, -36f);
        public Vector2 topString2Offset => new Vector2(10f, -36f);
        public Vector2 bottomStringOffset => new Vector2(-10f, 40f);
        public float StringHalfHeight => (Math.Abs(topStringOffset.Y+topString2Offset.Y)/2 + Math.Abs(bottomStringOffset.Y)) * 0.5f;

        public float ChargeupInterpolant => Utils.GetLerpValue(HeavenlyGale.ShootDelay, HeavenlyGale.MaxChargeTime, ChargeTimer, true);
        public ref float CurrentChargingFrames => ref Projectile.ai[0];
        public ref float ChargeTimer => ref Projectile.ai[1];

        public float StringReelbackInterpolant
        {
            get
            {
                int duration = Owner.ActiveItem().useAnimation;
                float time = duration - AvatarBow.ShootDelay;
                float firstHalf = Utils.GetLerpValue(8f, 0f, time, true);
                float secondHalf = Utils.GetLerpValue(8f, duration * 0.6f, time, true);
                return (MathHelper.Clamp(ChargeTimer,0,1) * 6)-0.5f;//firstHalf + secondHalf;
            }
        }
        public float StringReelbackDistance => Projectile.width * StringReelbackInterpolant * 0.3f;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 176;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.MaxUpdates = 2;
            Projectile.timeLeft = 400;
            Projectile.aiStyle = 0;
        }

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }
        public override void AI()
        {

            if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
            {
                Projectile.Kill();
                
            }
            //ChargeTimer++;
            Main.NewText($"Charge interp: {ChargeupInterpolant}", Color.AntiqueWhite);
            Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            Vector2 tipPosition = armPosition + Projectile.velocity * Projectile.width * 0.45f;

            UpdateProjectileHeldVariables(armPosition);
            ManipulatePlayerVariables();
            /*
            bool activatingShoot = AvatarBow.ShootDelay <= 0 && Main.mouseLeft && !Main.mapFullscreen && !Owner.mouseInterface;
            if (Main.myPlayer == Projectile.owner && OwnerCanShoot && activatingShoot)
            {
                SoundEngine.PlaySound(AvatarBow.FireSound, Projectile.Center);
                int ShootDelay = Owner.ActiveItem().useAnimation;
                Projectile.netUpdate = true;
            }
            */
           
            Projectile.damage = Owner.ActiveItem() is null ? 0 : Owner.GetWeaponDamage(Owner.ActiveItem());
            /*
            if (AvatarBow.ShootDelay > 0f && Projectile.FinalExtraUpdate())
            {
                float shootCompletionRatio = 1f - AvatarBow.ShootDelay / (Owner.ActiveItem().useAnimation - 1f);
                float bowAngularOffset = (float)Math.Sin(MathHelper.TwoPi * shootCompletionRatio) * 0.4f;
                float damageFactor = Utils.Remap(ChargeTimer, 0f, HeavenlyGale.MaxChargeTime, 1f, HeavenlyGale.MaxChargeDamageBoost);

                // Fire arrows.
                if (AvatarBow.ShootDelay % HeavenlyGale.ArrowShootRate == 0)
                {
                    Vector2 arrowDirection = Projectile.velocity.RotatedBy(bowAngularOffset);

                    // Release a streak of energy.
                    Color energyBoltColor = CalamityUtils.MulticolorLerp(shootCompletionRatio, CalamityUtils.ExoPalette);
                    energyBoltColor = Color.Lerp(energyBoltColor, Color.White, 0.35f);
                    SquishyLightParticle exoEnergyBolt = new(tipPosition + arrowDirection * 16f, arrowDirection * 4.5f, 0.85f, energyBoltColor, 40, 1f, 5.4f, 4f, 0.08f);
                    GeneralParticleHandler.SpawnParticle(exoEnergyBolt);

                    // Update the tip position for one frame.
                    tipPosition = armPosition + arrowDirection * Projectile.width * 0.45f;

                    if (Main.myPlayer == Projectile.owner && Owner.HasAmmo(Owner.ActiveItem()))
                    {
                        Item heldItem = Owner.ActiveItem();
                        Owner.PickAmmo(heldItem, out int projectileType, out float shootSpeed, out int damage, out float knockback, out _);
                        damage = (int)(damage * damageFactor);
                        projectileType = ModContent.ProjectileType<ExoCrystalArrow>();

                        bool createLightning = ChargeTimer / HeavenlyGale.MaxChargeTime >= HeavenlyGale.ChargeLightningCreationThreshold;
                        Vector2 arrowVelocity = arrowDirection * shootSpeed;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPosition, arrowVelocity, projectileType, damage, knockback, Projectile.owner, createLightning.ToInt());
                    }
                }
            */




            if (InUse)
            {
                ChargeTimer++;
                if(ChargeTimer == 1)
                {
                    int bulletAMMO = ProjectileID.WoodenArrowFriendly;
                    Owner.PickAmmo(Owner.ActiveItem(), out bulletAMMO, out float SpeedNoUse, out int bulletDamage, out float kBackNoUse, out int _);

                    Projectile shot = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), tipPosition, Projectile.velocity * 12, bulletAMMO, Projectile.damage, Projectile.knockBack, Projectile.owner);

                }
            }
            else
            {
               ChargeTimer--;
            }


        }


        public void UpdateProjectileHeldVariables(Vector2 armPosition)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                float aimInterpolant = Utils.GetLerpValue(10f, 40f, Projectile.Distance(Main.MouseWorld), true);
                Vector2 oldVelocity = Projectile.velocity;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.SafeDirectionTo(Main.MouseWorld), aimInterpolant);
                if (Projectile.velocity != oldVelocity)
                {
                    Projectile.netSpam = 0;
                    Projectile.netUpdate = true;
                }
            }

            Projectile.position = armPosition - Projectile.Size * 0.5f + Projectile.velocity.SafeNormalize(Vector2.UnitY) * 44f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = Projectile.direction;
            Projectile.timeLeft = 2;
        }

        public void ManipulatePlayerVariables()
        {
            Owner.ChangeDir(Projectile.direction);
            Owner.heldProj = Projectile.whoAmI;

            // Make the player lower their front arm a bit to indicate the pulling of the string.
            // This is precisely calculated by representing the top half of the string as a right triangle and using SOH-CAH-TOA to
            // calculate the respective angle from the appropriate widths and heights.
            float frontArmRotation = (float)Math.Atan(StringHalfHeight / MathHelper.Max(StringReelbackDistance, 0.001f) * 0.5f);
            if (Owner.direction == -1)
                frontArmRotation += MathHelper.PiOver4;
            else
                frontArmRotation = MathHelper.PiOver2 - frontArmRotation;
            frontArmRotation += Projectile.rotation + MathHelper.Pi + Owner.direction * MathHelper.PiOver2 + 0.12f;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, frontArmRotation);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() - MathHelper.PiOver2);
        }

        public override bool PreDraw(ref Color lightColor)
        {

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            Vector2 topOfBow = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation + topStringOffset.ToRotation()) * topStringOffset.Length();
            Vector2 topOfBow2 = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation + topString2Offset.ToRotation()) * topString2Offset.Length();

            Vector2 bottomOfBow = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation + bottomStringOffset.ToRotation()) * bottomStringOffset.Length();
            Vector2 endOfString = Projectile.Center - Projectile.rotation.ToRotationVector2() * (StringReelbackDistance + (1f - StringReelbackInterpolant) * 25f);

            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;


            Color stringColor = new(105, 239, 145);

            float rotation = Projectile.rotation;

            int direction = Projectile.spriteDirection;
            SpriteEffects flipEffect = direction > 0 ? 0 : SpriteEffects.FlipVertically;

            Main.spriteBatch.DrawLineBetter(topOfBow, endOfString, stringColor, 2f);
            Main.spriteBatch.DrawLineBetter(topOfBow2, endOfString, stringColor, 2f);
            Main.spriteBatch.DrawLineBetter(bottomOfBow, endOfString, stringColor, 2f);
            
            
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, flipEffect, 0f);
            
            return false;
        }

        public override bool? CanDamage() => false;
    }
}
