using System;
using CalamityMod;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Sounds;
using HeavenlyArsenal.Content.Items.Misc;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Misc
{
    public class incomplete_gunHoldout : BaseIdleHoldoutProjectile
    {


        public override void SetDefaults()
        {
           

            Projectile.width = Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.MaxUpdates = 2;
        }
        public override int AssociatedItemID => ModContent.ItemType<Incomplete_gun>();
        public override int IntendedProjectileType => ModContent.ProjectileType<RicoshotCoin>();


        private int clickCooldown = 0;
        public override void SafeAI()
        {
            Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            UpdateProjectileHeldVariables(armPosition);
            ManipulatePlayerVariables();

            // Handle "attempting to fire" logic
            if (clickCooldown > 0)
                clickCooldown--;

            if (Main.mouseLeft && Main.myPlayer == Projectile.owner && clickCooldown <= 0)
            {
                AttemptFire();
                clickCooldown = 20; // Cooldown duration (in frames)
            }
        }
        private void AttemptFire()
        {
            // Play a clicking sound
            //SoundEngine.PlaySound(new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/incompleteGun_click"), Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item38, Projectile.Center);
            // Emit harmless sparks
            for (int i = 0; i < 5; i++)
            {
                Vector2 velocity = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30)) * Main.rand.NextFloat(10f, 20f);
                Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, velocity, 150, Color.Orange, 1f);
                spark.noGravity = true;
            }

            // Optional: Add slight visual feedback for the player (e.g., recoil or shake)
            Vector2 recoil = -Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f;
            Projectile.position += recoil;
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
            float frontArmRotation = Projectile.rotation * 0.5f;
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
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;



            float rotation = Projectile.rotation;
            SpriteEffects direction = SpriteEffects.None;
            if (Math.Cos(rotation) < 0f)
            {
                direction = SpriteEffects.FlipHorizontally;
                rotation += MathHelper.Pi;
            }

            Color stringColor = new(105, 239, 145);


           
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);
            return false;
        }

        // No damage for u stupid
        public override bool? CanDamage() => false;
    }
}


