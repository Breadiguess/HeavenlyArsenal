using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using CalamityMod;
using HeavenlyArsenal.Content.Items.Weapons.Ranged;
using ReLogic.Content;
using Terraria.GameContent.UI.BigProgressBar;
using HeavenlyArsenal.Content.Gores;

using HeavenlyArsenal.Core.Physics.ClothManagement;


using static Luminance.Common.Utilities.Utilities;
using NoxusBoss.Core.Physics.VerletIntergration;
using HeavenlyArsenal.Common.utils;
using Luminance.Core.Graphics;


namespace HeavenlyArsenal.Content.Projectiles.Ranged
{
    public class AvatarRifle_Holdout : BaseIdleHoldoutProjectile
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override int AssociatedItemID => ModContent.ItemType<AvatarRifle>();
        public override int IntendedProjectileType => ModContent.ProjectileType<ParasiteParadiseProjectile>();

        private SoundEffectInstance firingSoundInstance;
        private SoundEffect firingSoundEffect;

        private SoundEffectInstance reloadSoundInstance;
        private SoundEffect reloadSoundEffect;

        private SoundEffectInstance MagEmptySoundInstance;
        private SoundEffect MagEmptySoundEffect;

        private SoundEffectInstance CycleSoundInstance;
        private SoundEffect CycleSoundEffect;

        public ClothSimulation Shroud
        {
            get;
            set;
        } = new ClothSimulation(Vector3.Zero, 
            
            //Width
            22, 
            //Height
            21,
            //spacing?
            4.4f,
            // stiffness
            60f,
            // dampening coeficcient
            0.019f);

        public int ExistenceTimer
        {
            get;
            set;
        }
        



      

        private AvatarRifleState CurrentState = AvatarRifleState.Firing;
        private int StateTimer = 0;

        private int AmmoCount = 7; // Total shots before reload
        public int ReloadDuration = AvatarRifle.ReloadTime; // Duration for reload (in frames)

        private enum AvatarRifleState
        {
            Firing,  // Firing a shot
            PostFire,
            Cycle,   // Cycle before cycling the bolt
            Reload   // Reloading after all shots are fired
        }

        public override void SetDefaults()
        {
            firingSoundEffect = ModContent.Request<SoundEffect>("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle_FireWIP2").Value;
            reloadSoundEffect = ModContent.Request<SoundEffect>("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle_Cycle").Value; // Add your reload sound here
            CycleSoundEffect = ModContent.Request<SoundEffect>("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle_Cycle").Value;
            MagEmptySoundEffect = ModContent.Request<SoundEffect>("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle_ClipEject").Value;


            Projectile.width = Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.MaxUpdates = 2;
            Player player = Main.player[Projectile.owner];
        }
        private void CreateDustAtOrigin()
        {
            // Create dust at the center of the sprite

            Vector2 dustPosition = Projectile.Center;
            Dust dust = Dust.NewDustDirect(dustPosition, 1, 1, DustID.Smoke, 0f, 0f, 150, Color.White, 1f);
            dust.velocity *= 0.3f; 
            dust.noGravity = true; 
        }
        public override void SafeAI()
        {
            //CreateDustAtOrigin();
            Vector2 armPosition = Owner.RotatedRelativePoint((Vector2)Owner.HandPosition, true);
            UpdateProjectileHeldVariables(armPosition);
            ManipulatePlayerVariables();

            if (Owner.HeldItem.type == ModContent.ItemType<AvatarRifle>())
            {
                switch (CurrentState)
                {
                    case AvatarRifleState.Firing:
                        HandleFiring();
                        break;
                    case AvatarRifleState.PostFire:
                        HandlePostFire();
                        break;
                    case AvatarRifleState.Cycle:
                        HandleCycle();
                        break;
                    case AvatarRifleState.Reload:
                        HandleReload();
                        break;
                }
            }
            else
            {
                CurrentState = AvatarRifleState.Firing;
                StateTimer = 0;
            }

            //UpdateCloth();
            ExistenceTimer++;
        }

        private void UpdateCloth()
        {

            //I'm assuming thsi is referring to the LEVEL OF DETAIL
            int steps = 5;

            float windSpeed = Math.Clamp(Main.WindForVisuals * Projectile.spriteDirection * 8f, -1.3f, 0f);

            Vector2 ShroudPosition = Projectile.Center - Main.screenPosition;
            

            //actually implement wind into the cloth
            Vector3 wind = Vector3.UnitX * (LumUtils.AperiodicSin(ExistenceTimer * 0.029f) * 0.67f + windSpeed) * 1.74f;
            for (int i = 0; i < steps; i++)
            {
                for (int x = 0; x < Shroud.Width; x += 2)
                {
                    for (int y = 0; y < 2; y++)
                        ConstrainParticle(ShroudPosition, Shroud.particleGrid[x, y], 0f);
                }
                Main.NewText($"Simulating Shroud at {i}", Color.Cyan);
                Shroud.Simulate(
                    //deltaTime
                    0.051f,
                    //Has collision
                    false, 
                    //Gravity!!
                    Vector3.UnitY * 3.2f + wind);
            }
        }

        
        private void ConstrainParticle(Vector2 anchor, ClothPoint? point, float angleOffset)
        {
            if (point is null)
                return;

            float xInterpolant = point.X / (float)Shroud.Width;
            float angle = MathHelper.Lerp(MathHelper.PiOver2, MathHelper.TwoPi - MathHelper.PiOver2, xInterpolant);

            Vector3 ring = new Vector3(MathF.Cos(angle + angleOffset) * 50f, 0f, MathF.Sin(angle - MathHelper.PiOver2) * 10f);
            ring.Y += point.Y * 6f;

            point.Position = new Vector3(anchor, 0f) + ring;
            point.IsFixed = true;
        }

        private void HandleFiring()
        {
            if (Owner.channel&& Owner.HasAmmo(Owner.HeldItem))
            {
                if (AmmoCount > 0)
                {
                    // Play firing sound
                    firingSoundInstance = firingSoundEffect.CreateInstance();
                    firingSoundInstance.Play();

                    // Decrement ammo count
                    AmmoCount--;
                    FireProjectile();

                    // Transition to Cycle state for cycling the bolt
                    Cycled = false;
                    CurrentState = AvatarRifleState.PostFire;
                    StateTimer = AvatarRifle.CycleTime; // Adjust delay duration for bolt cycle
                }
                else
                {
                    FireProjectile();
                    // Transition to Reload state when ammo is depleted

                    MagEmptySoundInstance = firingSoundEffect.CreateInstance();
                    MagEmptySoundInstance.Play();

                    CurrentState = AvatarRifleState.Reload;
                    StateTimer = ReloadDuration;
                }

            }
        }




      

        private void HandlePostFire()
        {



            if (RecoilRotation > 1 && recoilIntensity > 0)
            {

                RecoilRotation *= 0.9f;
            }

            else
            {
                RecoilRotation = 0f;
                CurrentState = AvatarRifleState.Cycle;
                StateTimer = AvatarRifle.CycleTime;
            }
            
            
        }



        public bool Cycled = false;
        private float CycleOffset =0f;
        private void HandleCycle()
        {
            int holdCyclePosition = 50; // How long to stay in cycle position (ticks)

            if (!Cycled)
            {
                // Initiate cycling
                CycleOffset = MathHelper.ToRadians(15f); // Initial "dip" angle
                CycleSoundInstance = CycleSoundEffect.CreateInstance();
                CycleSoundEffect.Play();
                Cycled = true;

                // Visual effect (optional)
                Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Left, new Vector2(Projectile.direction * -5f, -10f), ModContent.GoreType<BulletGore>(), 1);
            }

            if (StateTimer > holdCyclePosition)
            {
                // Hold the cycling position
                CycleOffset = MathHelper.ToRadians(15f); // Keep the offset fixed during hold phase
            }
            else if (StateTimer > 0)
            {
                // Smoothly return CycleOffset to 0 in the second phase
                CycleOffset = MathHelper.Lerp(CycleOffset, 0, 0.1f); // Adjust 0.1f for the speed of the transition
            }
            else
            {
                // End of cycling state
                CurrentState = AvatarRifleState.Firing;
                Cycled = false;
            }

            // Decrement StateTimer each tick
            StateTimer--;
        }




        private void HandleReload()
        {
            if (StateTimer == ReloadDuration)
            {
                // Play reload sound once at the start of reload
                reloadSoundInstance = reloadSoundEffect.CreateInstance();
                reloadSoundInstance.Play();
                //new Vector2 GoreDirection = (-1f, 1f);
                //Gore.NewGore(Projectile.GetSource_FromThis,Projectile.Left, new Vector2(1*Projectile.direction,-1f), ModContent.GoreType<MagEjectGore>,1);
                Gore.NewGore(Projectile.GetSource_FromThis(),Projectile.Left, new Vector2(Projectile.direction * -5f, -10f), ModContent.GoreType<MagEjectGore>(), 1);
            
            }

            if (StateTimer > 0)
            {
                StateTimer--; // Count down the reload
                
            }
            else
            {
                SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot.WithVolumeScale(0.5f).WithPitchOffset(0.8f), Projectile.position);
                // Reset ammo and transition back to Firing state
                AmmoCount = 7;
                CurrentState = AvatarRifleState.Firing;

            }
        }



        private float recoilIntensity = 0f; // Tracks the current recoil intensity
        private float RecoilRotation = 0f;
        private const float maxRecoil = 10f; // Maximum recoil amount
        private float recoilRecoverySpeed = 0.5f; // Speed at which recoil eases out
        private void FireProjectile()
        {
            int bulletAMMO = ProjectileID.Bullet;
            Owner.PickAmmo(Owner.ActiveItem(), out bulletAMMO, out float SpeedNoUse, out int bulletDamage, out float kBackNoUse, out int _);
            
            Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            // Logic for spawning the projectile
            Owner.PickAmmo(Owner.HeldItem, out int projToShoot, out _, out _, out _, out _);

            RecoilRotation +=  MathHelper.ToRadians(15f); // Spread angle for the muzzle flash particles
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center, Projectile.velocity * 14, bulletAMMO, Projectile.damage, Projectile.knockBack, Projectile.owner);
            SoundEngine.PlaySound(SoundID.Item41 with { Volume = 0.75f }, Projectile.Center);
            
            recoilIntensity = maxRecoil;
            if (AmmoCount > 0)
            {

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
            
            if (recoilIntensity > 0f)
            {
                recoilIntensity -= recoilRecoverySpeed;
                if (recoilIntensity < 0f)
                    recoilIntensity = 0f; // Clamp to prevent negative values
            }
            Vector2 origin = new Vector2(50, 5);
            Vector2 recoilOffset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * recoilIntensity;
            


            //Dust.NewDust(armPosition, 3, 3, DustID.Adamantite, 0, 0, 150, default, 1);
            


            Projectile.position = armPosition + 
                      Projectile.velocity.SafeNormalize(Vector2.UnitX) * -3f +
                      recoilOffset;
            Projectile.Center = armPosition;



            Projectile.rotation = Projectile.velocity.ToRotation() -RecoilRotation+CycleOffset;//-RecoilRotation;
            Projectile.spriteDirection = Projectile.direction;
            Projectile.timeLeft = 2;
        }

        public void ManipulatePlayerVariables()
        {
            Owner.ChangeDir(Projectile.direction);
            Owner.heldProj = Projectile.whoAmI;

            
           // float frontArmRotation = Projectile.rotation;
           // if (Owner.direction == -1)
           //     frontArmRotation += MathHelper.PiOver2;
           // else
           //     frontArmRotation -= MathHelper.PiOver2 - frontArmRotation;
            //frontArmRotation = 0;
            //frontArmRotation += Projectile.rotation + MathHelper.Pi + Owner.direction * MathHelper.PiOver2 + 0.12f;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation-MathHelper.PiOver2);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.PiOver2);
            
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin =  new Vector2 (50,5);
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
           



            float rotation = Projectile.rotation;
            SpriteEffects direction = SpriteEffects.None;
            if (Math.Cos(rotation) < 0f)
            {
                direction = SpriteEffects.FlipHorizontally;
                rotation += MathHelper.Pi;
            }
            if (Projectile.spriteDirection < 0f)
            {
               
                
                origin = new Vector2(90, 5); // Origin for flipped direction
            }
            else
            {
                origin = new Vector2(50, 5); // Origin for normal direction
               
            }

            Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);


            //DrawShroud();

            return false;
        }


        private void DrawShroud()
        {
            Matrix world = Matrix.CreateTranslation(-Projectile.Center.X + WotGUtils.ViewportSize.X * 0.5f, -Projectile.Center.Y + WotGUtils.ViewportSize.Y * 0.5f, 0f);
            Matrix projection = Matrix.CreateOrthographicOffCenter(0f, WotGUtils.ViewportSize.X, WotGUtils.ViewportSize.Y, 0f, -1000f, 1000f);
            Matrix matrix = world * projection;

            ManagedShader clothShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssasinRobeShader");
            clothShader.TrySetParameter("opacity", 150);
            clothShader.TrySetParameter("transform", matrix);
            clothShader.Apply();
            Main.NewText($"DrawingShroud", Color.Cyan);
            Shroud.Render();
        }





        public override bool? CanDamage() => false;
    }
}