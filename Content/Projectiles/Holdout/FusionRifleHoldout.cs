using System;
using CalamityMod;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Sounds;
using HeavenlyArsenal.Content.Items.Weapons.Ranged;
using HeavenlyArsenal.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using NoxusBoss.Content.Particles;
using HeavenlyArsenal.Common.utils;

using Luminance.Core.Graphics;
using Particle = Luminance.Core.Graphics.Particle;
using HeavenlyArsenal.Content.Projectiles.Misc;
using HeavenlyArsenal.Content.Projectiles.Ranged;
using System.Linq;



namespace HeavenlyArsenal.Content.Projectiles.Holdout
{
    public class FusionRifleHoldout : BaseIdleHoldoutProjectile
    {
        
       
        public new string LocalizationCategory => "Projectiles.Ranged";
        public bool OwnerCanShoot => Owner.HasAmmo(Owner.ActiveItem()) && !Owner.noItems && !Owner.CCed;

        
       
        public float ChargeupInterpolant => Utils.GetLerpValue(FusionRifle.ShootDelay, FusionRifle.MaxChargeTime, ChargeTimer, true);
        public ref float CurrentChargingFrames => ref Projectile.ai[0];
        public ref float ChargeTimer => ref Projectile.ai[1];

        public ref float ShootDelay => ref Projectile.localAI[0];

        public override int AssociatedItemID => ModContent.ItemType<FusionRifle>();
        public override int IntendedProjectileType => ModContent.ProjectileType<ParasiteParadiseProjectile>();


        public static float CurrentChargeTime = FusionRifle.MaxChargeTime; // Default to MaxChargeTime

        public override void SetDefaults()
        {
            chargingSoundEffect = ModContent.Request<SoundEffect>("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_charge3").Value;
            firingSoundEffect = ModContent.Request<SoundEffect>("HeavenlyArsenal/Assets/Sounds/Items/fusionrifle_fire2").Value;

            Projectile.width = Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.MaxUpdates = 2;
        }

        private int BurstCount = 0; // Tracks how many projectiles are left in the burst
       
        private enum FusionRifleState
        {
            Charging,  // Charging up
            Firing,    // Firing the burst
            Delay      // Brief pause after firing
        }

        private FusionRifleState CurrentState = FusionRifleState.Charging;
        private int StateTimer = 0; // Timer to control transitions between states



        public override void SafeAI()
        {
            Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);

            switch (CurrentState)
            {
                case FusionRifleState.Charging:
                    HandleCharging();
                    break;

                case FusionRifleState.Firing:
                    HandleFiring();
                    break;

                case FusionRifleState.Delay:
                    HandleDelay();
                    break;
            }

            UpdateProjectileHeldVariables(armPosition);
            ManipulatePlayerVariables();
        }


        private SoundEffectInstance chargingSoundInstance; // Instance for playing sound
        private SoundEffect chargingSoundEffect;           // Preloaded sound effect

        private SoundEffectInstance firingSoundInstance;
        private SoundEffect firingSoundEffect;
        private void HandleCharging()
        {

            bool rancorCircleExists = Main.projectile.Any(p =>
            p.active &&
            p.type == ModContent.ProjectileType<RancorMagicCircle>() &&
            p.owner == Projectile.owner);

            if (Main.mouseLeft && Owner.channel)
            {
                if (ChargeTimer < CurrentChargeTime)
                    ChargeTimer++;
                if (!rancorCircleExists) // Only spawn if it doesn't already exist
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<RancorMagicCircle>(),
                        -1,
                        Projectile.knockBack,
                        Projectile.owner
                    );
                }




                // Handle the charging sound
                if (chargingSoundInstance == null) // If no sound is currently playing
                {
                    chargingSoundInstance = chargingSoundEffect.CreateInstance();
                    chargingSoundInstance.IsLooped = false;
                    chargingSoundInstance.Volume = 1f; // Adjust volume if necessary
                    chargingSoundInstance.Play(); // Start playing the sound
                }
                else if (chargingSoundInstance.State != SoundState.Playing)
                {
                    chargingSoundInstance.Play(); // Restart if it’s stopped
                }
               
                // Add gun shake effect as the rifle charges
                float shakeIntensity = 2f * ChargeupInterpolant;
                Projectile.position += new Vector2(
                    Main.rand.NextFloat(-shakeIntensity, shakeIntensity),
                    Main.rand.NextFloat(-shakeIntensity, shakeIntensity)
                );

                // Add inward swirling dust effect with consistent 3D Y-axis tilt, no Z-axis distortion
                float initialDustRadius = 50f; // Starting radius (adjust as needed)
                float finalDustRadius = 10f;   // Ending radius near the target (adjust as needed)
                float dustRadius = MathHelper.Lerp(initialDustRadius, finalDustRadius, ChargeTimer / (float)CurrentChargeTime);

                // Adjustable offset for the barrel (change these values to position the swirl effect)
                Vector2 barrelOffset = new Vector2(45f, Projectile.direction*-7f); // Customize the base position of the dust effect
                barrelOffset = barrelOffset.RotatedBy(Projectile.rotation); // Rotate the offset based on the projectile's rotation

                // Define rotation angle for Y-axis tilt (in radians)
                float yAxisTilt = MathHelper.ToRadians(50f); // 20-degree tilt for the 3D plane

                // Add the swirling effect with the corrected Y-axis tilt
                float dustAngle = Main.GameUpdateCount * 0.1f; // Rotate based on time
                for (int i = 0; i < 3; i++) // Add multiple dust particles
                {
                    float angle = dustAngle + MathHelper.TwoPi / 3 * i; // Spread particles equally

                    // Calculate the swirl offset before applying the Y-axis tilt
                    float unrotatedX = (float)Math.Cos(angle) * dustRadius;
                    float unrotatedY = (float)Math.Sin(angle) * dustRadius;

                    // Apply only the desired Y-axis tilt
                    float tiltedX = unrotatedX * (float)Math.Cos(yAxisTilt); // Apply X-axis scaling for the tilt
                    float tiltedY = unrotatedY; // Retain Y without Z influence
                    float adjustedY = tiltedY + (unrotatedX * (float)Math.Sin(yAxisTilt) * 0.5f); // Minimal Z-axis impact on Y

                    // Finalize the swirl position by rotating it with the projectile's orientation
                    Vector2 finalSwirlOffset = new Vector2(tiltedX, adjustedY).RotatedBy(Projectile.rotation);

                    // Combine the barrel offset and the rotated swirl offset
                    Vector2 dustPosition = Projectile.Center + barrelOffset + finalSwirlOffset;

                    // Create the dust particle
                    Dust dust = Dust.NewDustPerfect(dustPosition, DustID.UnusedWhiteBluePurple, Vector2.Zero, 100, Color.Cyan, 1.5f);
                    dust.noGravity = true;
                }
                //Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
                //Vector2 spawnPosition = armPosition + Projectile.velocity.SafeNormalize(Vector2.Zero) * 50f;





                //
            }

            // Stop the charging sound when charge is complete or interrupted
            if (ChargeTimer >= FusionRifle.MaxChargeTime)
            {
                SoundEngine.PlaySound(FusionRifle.FullyChargedSound, Projectile.Center);
                CurrentState = FusionRifleState.Firing; // Transition to firing state
                StateTimer = 0;
                BurstCount = FusionRifle.ArrowsPerBurst;

                if (chargingSoundInstance != null)
                {
                    chargingSoundInstance.Stop(); // Stop the sound when charge is complete
                    chargingSoundInstance = null; // Reset to allow new sound creation
                }
            }

            if (!Main.mouseLeft || !Owner.channel) // Handle charging interruption
            {
                if (chargingSoundInstance != null)
                {
                    chargingSoundInstance.Stop(); // Stop the charging sound
                    chargingSoundInstance = null; // Clear the instance to allow new creation
                }

                if (ChargeTimer > 0) // Rapidly drain the charge
                {
                    float intensity = ChargeupInterpolant * 0.5f; // Reduce glow intensity
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RuneWizard, Vector2.Zero, 100, Color.Cyan, intensity*10);
                    dust.noGravity = false;
                    ChargeTimer -= 10; // Decrease charge timer
                    if (ChargeTimer < 0)
                        ChargeTimer = 0; // Clamp to zero
                }
            }
        }


        private void HandleFiring()
        {
            Owner.itemAnimation = Owner.itemAnimationMax;
            if (BurstCount == FusionRifle.ArrowsPerBurst)
            {
                firingSoundInstance = firingSoundEffect.CreateInstance();
                firingSoundInstance.Play();
                DisipateHeat(true);
            }
            if (BurstCount > 0)
            {
                if (StateTimer <= 0)
                {
                    FireBurstProjectile();
                    BurstCount--;
                    
                    StateTimer = 5; // Cycle between burst projectiles (adjust as needed)
                }
                else
                {
                    StateTimer--; // Count down the delay timer
                }
            }
            else
            {
                Owner.PickAmmo(Owner.HeldItem, out _, out _, out _, out _, out _);
                CurrentState = FusionRifleState.Delay; // Transition to delay state after the burst
                StateTimer = 60; // Cycle duration after firing
                ChargeTimer = 0; // Reset charge
            }
        }

        private void HandleDelay()
        {
            if (StateTimer > 0)
            {
                StateTimer--; // Count down the delay
                DisipateHeat(false);
                
            }
            else
            {
                CurrentState = FusionRifleState.Charging; // Transition back to charging
            }
        }

        private void DisipateHeat(bool CreateSmoke)
        {

            // Create heat-like dissipating dust for each vent
            int numberOfVents = 4; // Adjust this to match the number of vents
            float ventSpacing = 10f; // Adjust the spacing between vents
            Vector2 initialOffset = new Vector2(10f, Projectile.direction * -5f); // Initial offset for the vent system (adjust as needed)

            // Adjust exhaust velocity based on the base projectile's velocity and rotation
            Vector2 baseExhaustDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX)*-Projectile.direction; // Base direction opposite of projectile's movement
            baseExhaustDirection = baseExhaustDirection.RotatedBy(Projectile.rotation); // Align with the projectile's rotation




            float angleVariance = MathHelper.ToRadians(2f); // Adjust the angle variance in degrees
            float randomAngle = Main.rand.NextFloat(-angleVariance, angleVariance); // Randomize the angle within the variance
            bool TopExhaust = false;

            for (int i = 0; i < numberOfVents; i++)
            {
                if (i % 2 == 0)
                {
                    TopExhaust = true;
                }
                else
                {
                    TopExhaust = false;
                }
                    // Calculate the vent position relative to the projectile
                    Vector2 ventOffset = new Vector2(i * ventSpacing, 0); // Iterate over X position
                ventOffset = ventOffset.RotatedBy(Projectile.rotation); // Rotate the offset by the projectile's rotation

                Vector2 ventPosition = Projectile.Center + initialOffset.RotatedBy(Projectile.rotation) + ventOffset;
                if (CreateSmoke == true)
                {
                    if (TopExhaust)
                    {
                        baseExhaustDirection += new Vector2(0, 0.5f); // Slight upward adjustment for top exhaust
                    }
                    else
                    {
                        baseExhaustDirection += new Vector2(0, -0.5f); // Slight downward adjustment for bottom exhaust
                    }

                    // Spawn the exhaust projectile
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), ventPosition,
                        baseExhaustDirection * 3f, // Multiply direction by exhaust speed
                        ModContent.ProjectileType<FusionRifle_Exhaust>(),
                        -1, // No damage
                        0, // No knockback
                        Projectile.owner);
                }
              

                // Only create dust at intervals determined by the loop
                if (StateTimer % numberOfVents == i) // Stagger the dust creation
                {
                    // Create dissipating heat dust


                    Dust dust = Dust.NewDustPerfect(ventPosition, DustID.Smoke, new Vector2(0, 40), 100, Color.Gray, 1f);
                    dust.noGravity = true;

                    // Add heat dissipation movement: rising and swaying
                    float swayIntensity = 5f; // Intensity of back-and-forth motion
                    float swaySpeed = 0.1f; // Speed of back-and-forth motion
                    float riseSpeed = -5f + Main.rand.NextFloat(-2.5f, 2.5f);

                    // Apply swaying and rising motion
                    dust.velocity.X = (float)Math.Sin(Main.GameUpdateCount * swaySpeed + i) * swayIntensity;
                    dust.velocity.Y = riseSpeed;
                }
            }


        }
        private void CreateMuzzleFlash(Vector2 muzzlePosition, Vector2 projectileVelocity)
        {
            int numParticles = 10; // Number of particles in the muzzle flash
            float angleVariance = MathHelper.ToRadians(15f); // Spread angle for the muzzle flash particles
            float particleSpeed = 6f; // Speed of particles moving outward

            for (int i = 0; i < numParticles; i++)
            {
                // Randomize particle direction within a cone based on the projectile's direction
                float randomAngle = Main.rand.NextFloat(-angleVariance, angleVariance);
                Vector2 direction = projectileVelocity.RotatedBy(randomAngle).SafeNormalize(Vector2.UnitX); // Normalize the direction
                Vector2 velocity = direction * particleSpeed * Main.rand.NextFloat(0.7f, 1.2f); // Randomize speed slightly

                // Create the dust at the muzzle position
                Particle deltaruneExplosionParticle = new DeltaruneExplosionParticle(muzzlePosition, Vector2.Zero, Color.AntiqueWhite, 48, 1);

                Dust dust = Dust.NewDustPerfect(muzzlePosition, DustID.Torch, velocity, 100, GetRandomFlashColor(), Main.rand.NextFloat(1.5f, 2f));
                dust.noGravity = true; // Floaty effect for the flash
                dust.fadeIn = 1f; // Makes the flash brighter initially
            }
        }

        // Helper function to randomize flash colors
        private Color GetRandomFlashColor()
        {
            // Randomize between white, yellow, and orange
            switch (Main.rand.Next(3))
            {
                case 0: return Color.White;
                case 1: return Color.Yellow;
                case 2: return Color.Orange;
                default: return Color.White; // Default fallback
            }
        }




        public void FireBurstProjectile()
        {
            Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);

            float chargePower = ChargeupInterpolant;
            int damage = Projectile.damage;
            float knockback = Projectile.knockBack;
            Vector2 spawnPosition = armPosition + Projectile.velocity.SafeNormalize(Vector2.Zero) * 50f;

            // Add variance to the projectile's velocity
            float angleVariance = MathHelper.ToRadians(2f); // Adjust the angle variance in degrees
            float randomAngle = Main.rand.NextFloat(-angleVariance, angleVariance); // Randomize the angle within the variance
            Vector2 adjustedVelocity = Projectile.velocity.RotatedBy(randomAngle); // Apply the random angle to the velocity

            // Spawn the projectile with the adjusted velocity
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, adjustedVelocity * (40f + chargePower),
                ModContent.ProjectileType<ParasiteParadiseProjectile>(),
                damage,
                knockback,
                Projectile.owner
            );


            Vector2 MuzzleFlashPosition = armPosition + Projectile.velocity.SafeNormalize(Vector2.Zero) * 70f;

            CreateMuzzleFlash(MuzzleFlashPosition, Projectile.velocity);




            DeltaruneExplosionParticle deltaruneExplosionParticle = new DeltaruneExplosionParticle(spawnPosition, Vector2.Zero, Color.AntiqueWhite, 48,1);
                



            // Trigger the recoil effect
            recoilIntensity = maxRecoil; // Set the recoil to its maximum value

            
            //PunchCameraModifier = new Vector2(Main.rand.NextFloat(-50f, 50f), Main.rand.NextFloat(-50f, 50f));
            //replace recoilVector with something else later

            //Main.instance.CameraModifiers.Add(new PunchCameraModifier(spawnPosition*2f, -recoilVector,10,1,2,-1,null));
        }


        private float recoilIntensity = 0f; // Tracks the current recoil intensity
        private const float maxRecoil = 10f; // Maximum recoil amount
        private const float recoilRecoverySpeed = 0.1f; // Speed at which recoil eases out




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
            // Update recoil intensity (ease it out over time)
            if (recoilIntensity > 0f)
            {
                recoilIntensity -= recoilRecoverySpeed;
                if (recoilIntensity < 0f)
                    recoilIntensity = 0f; // Clamp to prevent negative values
            }

            Vector2 recoilOffset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * recoilIntensity;
            Projectile.position = armPosition - Projectile.Size * 0.5f + Projectile.velocity.SafeNormalize(Vector2.UnitY) * 34f  + recoilOffset;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = Projectile.direction;
            Projectile.timeLeft = 2;
        }

        public void ManipulatePlayerVariables()
        {
            Owner.ChangeDir(Projectile.direction);
            Owner.heldProj = Projectile.whoAmI;

            
            float frontArmRotation =Projectile.rotation * 0.5f;
            if (Owner.direction == -1)
                frontArmRotation += MathHelper.PiOver2;
            else
                frontArmRotation = MathHelper.PiOver2 - frontArmRotation;
            frontArmRotation += Projectile.rotation + MathHelper.Pi + Owner.direction * MathHelper.PiOver2 + 0.12f;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, frontArmRotation);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() - MathHelper.PiOver2);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            //Texture2D textureGlow = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/HeavenlyGaleProjGlow").Value;
            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            

          
            float chargeOffset = ChargeupInterpolant * Projectile.scale * 5f;
            Color chargeColor = Color.Lerp(Color.Red, Color.DarkRed, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 7.1f) * 0.5f + 0.5f) * ChargeupInterpolant * 0.6f;
            chargeColor.A = 0;

            float rotation = Projectile.rotation;
            SpriteEffects direction = SpriteEffects.None;
            if (Math.Cos(rotation) < 0f)
            {
                direction = SpriteEffects.FlipHorizontally;
                rotation += MathHelper.Pi;
            }

            Color stringColor = new(129, 18, 42);
           

            // Draw a backglow effect as an indicator of charge.
            for (int i = 0; i < 5; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 6f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, chargeColor, rotation, origin, Projectile.scale, direction, 0f);
            }
            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);
            
            return false;
        }

        // The bow itself should not do contact damage.
        public override bool? CanDamage() => false;
    }
}


