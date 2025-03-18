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
using HeavenlyArsenal.Common.Utilities;
using HeavenlyArsenal.Core.Physics.ClothManagement;


using static Luminance.Common.Utilities.Utilities;
using NoxusBoss.Core.Physics.VerletIntergration;
using HeavenlyArsenal.Common.utils;
using Luminance.Core.Graphics;
using CalamityMod.Items.Weapons.Ranged;
using Terraria.Localization;
using Terraria.DataStructures;


namespace HeavenlyArsenal.Content.Projectiles.Weapons.Ranged
{
    public class AvatarRifle_Holdout : BaseIdleHoldoutProjectile
    {

        public override LocalizedText DisplayName => CalamityUtils.GetItemName<AvatarRifle>();

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



        public ref Player Player => ref Main.player[Projectile.owner];


        private AvatarRifleState CurrentState = AvatarRifleState.Firing;
        private int StateTimer = 0;

        private int AmmoCount = 7; // Total shots before reload
        public int ReloadDuration = AvatarRifle.ReloadTime; // Duration for reload (in frames)
        public Vector2 origin = new Vector2(50, 50);

        private enum AvatarRifleState
        {
            Firing,  // Firing a shot
            PostFire, //handle recoil for a better visual experience
            Cycle,   // Cycle before cycling the bolt
            Reload   // Reloading after all shots are fired
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 10;
        }

        public override void SetDefaults()
        {
            firingSoundEffect = ModContent.Request<SoundEffect>("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/AvatarRifle_FireWIP2").Value;
            reloadSoundEffect = ModContent.Request<SoundEffect>("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/AvatarRifle_Reload").Value; // Add your reload sound here
            CycleSoundEffect = ModContent.Request<SoundEffect>("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/AvatarRifle_Cycle").Value;
            MagEmptySoundEffect = ModContent.Request<SoundEffect>("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/AvatarRifle_ClipEject").Value;


            Projectile.width = Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.MaxUpdates = 2;
            Player player = Main.player[Projectile.owner];
        }


        private Rope rope;



        public override void OnSpawn(IEntitySource source)
        {
            //Vector2 RopeStart = new Vector2((origin.X)+Main.screenPosition.X, (origin.Y - Main.screenPosition.Y)- 40 * Projectile.direction * Player.gravDir);
            Vector2 RopeStart = Projectile.Center + new Vector2(-20, 10); // Adjust offsets as needed
            Vector2 endPoint = Projectile.Center + new Vector2(90 * Projectile.direction, 10);

            rope = new Rope(RopeStart, endPoint, segmentCount: 30, segmentLength: 5f, gravity: new Vector2(0, 10f));
            rope.tileCollide = false; // Disable tile collision if unnecessary
            rope.damping = 0.5f;
        }

        private void CreateDustAtOrigin()
        {
            // Create dust at the center of the sprite

            Vector2 dustPosition = Projectile.Center;
            Dust dust = Dust.NewDustDirect(dustPosition, 1, 1, DustID.Smoke, 0f, 0f, 150, Color.White, 1f);
            dust.velocity *= 0.3f;
            dust.noGravity = true;
        }



        private void ApplyWindToRope()
        {
            float windSpeed = Math.Clamp(Main.WindForVisuals * Projectile.spriteDirection * 8f, -1.3f, 0f);
            Vector2 robePosition = Projectile.Center + new Vector2(14f, -50f);//.RotatedBy(Projecitle.Rotation);
            Vector3 wind = Vector3.UnitX * (LumUtils.AperiodicSin(ExistenceTimer * 0.029f) * 0.67f + windSpeed) * 1.74f;
        }
        public override void SafeAI()
        {





            CreateDustAtOrigin();
            Vector2 armPosition = Owner.RotatedRelativePoint(
                Owner.MountedCenter);

            //(Vector2)Owner.HandPosition, true);
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


            Vector2 RopeStart = Projectile.Center + new Vector2(-40, Projectile.direction * 10).RotatedBy(Projectile.rotation);
            Vector2 endPoint = Projectile.Center + new Vector2(90, Projectile.direction * 2).RotatedBy(Projectile.rotation);

            rope.segments[0].position = RopeStart;
            rope.segments[^1].position = endPoint;
            rope.gravity = new Vector2(0f, 0.5f);
            rope.Update();
            ApplyWindToRope();
        }

        private void UpdateCloth()
        {

            //I'm assuming thsi is referring to the LEVEL OF DETAIL
            int steps = 5;

            float windSpeed = Math.Clamp(Main.WindForVisuals * Projectile.spriteDirection * 8f, -1.3f, 0f);

            Vector2 ShroudPosition = Projectile.Center - Main.screenPosition;


            //actually implement wind into the cloth
            Vector3 wind = Vector3.UnitX * (AperiodicSin(ExistenceTimer * 0.029f) * 0.67f + windSpeed) * 1.74f;
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


        private void ConstrainParticle(Vector2 anchor, ClothPoint point, float angleOffset)
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
            if (Owner.channel && Owner.HasAmmo(Owner.HeldItem))
            {
                Main.NewText($"Firing! AmmoCount: {AmmoCount}", Color.Red);


                // Play firing sound
                firingSoundInstance = firingSoundEffect.CreateInstance();
                firingSoundInstance.Play();

                // Decrement ammo count
                AmmoCount--;
                FireProjectile();

                // Transition to Cycle state for cycling the bolt
                Cycled = false;
                CurrentState = AvatarRifleState.PostFire;
                StateTimer = AvatarRifle.CycleTime / 2; // Adjust delay duration for bolt cycle



            }
        }




        private void HandlePostFire()
        {
            Main.NewText($"Recoiling!", Color.Chocolate);


            if (StateTimer > 0)
            {
                StateTimer--;
                RecoilRotation *= 0.9f;
            }

            else
            {
                Main.NewText($"Recoiled!", Color.Chocolate);
                RecoilRotation = 0f;
                HasShellToEject = true;

                if (AmmoCount > 0)
                {
                    Main.NewText($"Mag not Empty, Cycle {AmmoCount}", Color.Chocolate);
                    CurrentState = AvatarRifleState.Cycle;
                    StateTimer = AvatarRifle.CycleTime;

                }
                else
                {
                    MagEmptySoundInstance = MagEmptySoundEffect.CreateInstance();
                    MagEmptySoundInstance.Play();
                    Main.NewText($"Mag Empty, Begin Reload", Color.Chocolate);
                    CurrentState = AvatarRifleState.Reload;
                    StateTimer = ReloadDuration;

                }

            }


        }



        public bool Cycled = false;
        private float CycleOffset = 0f;
        public bool HasShellToEject = true;
        private void HandleCycle()
        {
            int holdCyclePosition = 50; // How long to stay in cycle position (ticks)
            int frameDuration = 5; // How many ticks each frame lasts
            int totalFrames = 10; // Total frames for the projectile

            if (!Cycled)
            {
                // Initiate cycling
               // Initial "dip" angle
                CycleSoundInstance = CycleSoundEffect.CreateInstance();
                CycleSoundEffect.Play();
                Cycled = true;
                Main.NewText($"Cycling!", Color.Coral);

                Projectile.frame = 0; // Start animation from the first frame
                Projectile.frameCounter = 0; // Reset frame counter
            }

            if (StateTimer > holdCyclePosition)
            {
                // Hold the cycling position
               
                    CycleOffset = Projectile.spriteDirection*MathHelper.ToRadians(15f);
                //CycleOffset = Utils.AngleLerp(Projectile.rotation, Projectile.spriteDirection * MathHelper.ToRadians(15f), 0.5f);


                if (Projectile.frameCounter == 4)
                {
                    if (HasShellToEject)
                    {
                        HasShellToEject = false;
                        EjectShell(2039, origin, -40, 40);
                    }
                }
                // Update frame based on frame duration
                if (++Projectile.frameCounter >= frameDuration)
                {
                    Projectile.frameCounter = 0; // Reset the counter
                    Projectile.frame++; // Move to the next frame

                    if (Projectile.frame >= totalFrames)
                        Projectile.frame = 9; //stay here.
                }
            }
            else if (StateTimer > 0)
            {
                if (StateTimer > 0)
                {
                    StateTimer--;
                    CycleOffset *= 0.87f;
                }
                
            }
            else
            {
                // End of cycling state
                CycleOffset = 0f;
                CurrentState = AvatarRifleState.Firing;
                Cycled = false;
                Main.NewText($"Cycled!", Color.Coral);
                Projectile.frame = 0; // Reset frame to the starting frame
            }

            // Decrement StateTimer each tick
            StateTimer--;
        }



        private void EjectShell(int SparkCount, Vector2 ShellPosition, float ShellVelocityMin, float ShellVelocityMax)
        {
            Main.NewText($"EjectShell!", Color.GhostWhite);
            int BulletVariation = (int)Main.rand.NextFloat(ShellVelocityMin, ShellVelocityMax);

            Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Left, new Vector2(Projectile.direction * -5f, -10f), ModContent.GoreType<BulletGore>(), 1);
            for (int i = 0; i <= SparkCount; i++)
            {
                float CasingVariation = Main.rand.NextFloat(ShellVelocityMin, ShellVelocityMax);

                Dust.NewDust(ShellPosition, 1, 1, DustID.GoldFlame, Projectile.velocity.X + CasingVariation, Projectile.velocity.Y + CasingVariation, 150, default, 1);
            }
        }

        private void HandleReload()
        {
            if (StateTimer == ReloadDuration)
            {
                Projectile.frame = 10;
                // Play reload sound once at the start of reload
                
                //new Vector2 GoreDirection = (-1f, 1f);
                //Gore.NewGore(Projectile.GetSource_FromThis,Projectile.Left, new Vector2(1*Projectile.direction,-1f), ModContent.GoreType<MagEjectGore>,1);
                Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Left, new Vector2(Projectile.direction * -4f, 0f), ModContent.GoreType<MagEjectGore>(), 4);
                Main.NewText($"Reloading", Color.AntiqueWhite);
            }

            if (StateTimer == ReloadDuration/2)
            {
                reloadSoundInstance = reloadSoundEffect.CreateInstance();
                reloadSoundInstance.Play();
            }
            if (StateTimer > 0)
            {
                StateTimer--; // Count down the reload
                CycleOffset = Projectile.spriteDirection * MathHelper.ToRadians(15f); // Keep the offset fixed during hold phase

            }
            else
            {
                Projectile.frame = 0;
                CycleOffset = 0f;
                SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot.WithVolumeScale(0.5f).WithPitchOffset(0.8f), Projectile.position);
                // Reset ammo and transition back to Firing state
                AmmoCount = 7;
                CurrentState = AvatarRifleState.Firing;
                Main.NewText($"Reloaded!", Color.AntiqueWhite);
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


            Vector2 tipPosition = armPosition + Projectile.velocity * Projectile.width * 1.55f + new Vector2(3, -3);
            CreateMuzzleFlash(tipPosition, Projectile.velocity);

            RecoilRotation += Projectile.spriteDirection * MathHelper.ToRadians(34f); // Spread angle for the muzzle flash particles
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), tipPosition, Projectile.velocity * 14, bulletAMMO, Projectile.damage, Projectile.knockBack, Projectile.owner);
            //SoundEngine.PlaySound(SoundID.Item41 with { Volume = 0.75f }, Projectile.Center);
            //Dust.NewDust(tipPosition, 1, 1, DustID.Firefly, Projectile.spriteDirection*5, 0, 100, default, 1);


            recoilIntensity = maxRecoil;
            if (AmmoCount > 0)
            {

            }
        }




        private void CreateMuzzleFlash(Vector2 muzzlePosition, Vector2 projectileVelocity)
        {
            int numParticles = 40; // Number of particles in the muzzle flash
            float angleVariance = MathHelper.ToRadians(30f); // Spread angle for the muzzle flash particles
            float particleSpeed = 6f+ Main.rand.NextFloat(-2f,4f); // Speed of particles moving outward

            for (int i = 0; i < numParticles; i++)
            {
                // Randomize particle direction within a cone based on the projectile's direction
                float randomAngle = Main.rand.NextFloat(-angleVariance, angleVariance);
                Vector2 direction = projectileVelocity.RotatedBy(randomAngle).SafeNormalize(Vector2.UnitX); // Normalize the direction
                Vector2 velocity = direction * particleSpeed * Main.rand.NextFloat(0.7f, 1.2f); // Randomize speed slightly

                // Create the dust at the muzzle position


                Dust dust = Dust.NewDustPerfect(muzzlePosition, DustID.AncientLight, velocity, 100, GetRandomFlashColor(), Main.rand.NextFloat(1.5f, 2f));
                dust.noGravity = true; // Floaty effect for the flash
                dust.fadeIn = 1f; // Makes the flash brighter initially
            }
        }
        private Color GetRandomFlashColor()
        {
            // Randomize between white, yellow, and orange
            switch (Main.rand.Next(3))
            {
                case 0: return Color.DarkCyan;//Colors(178, 255, 254);
                case 1: return Color.Blue;
                case 2: return Color.LightSkyBlue;
                default: return Color.White; // Default fallback
            }
        }





        public void UpdateProjectileHeldVariables(Vector2 armPosition)
        {
            Vector2 recoilOffset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * recoilIntensity;
            //COmment so that i can push again
            Projectile.position = armPosition +
                      Projectile.velocity.SafeNormalize(Vector2.UnitX) * -3f +
                      recoilOffset;
            Projectile.Center = armPosition + recoilOffset;



            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 tipPosition = armPosition + Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.85f + new Vector2(0, -3);
                float aimInterpolant = Utils.GetLerpValue(10f, 40f, Projectile.Distance(Main.MouseWorld), true);
                Vector2 oldVelocity = Projectile.velocity;
                Projectile.velocity = Vector2.Lerp(tipPosition, Projectile.SafeDirectionTo(Main.MouseWorld), aimInterpolant);
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





            Projectile.rotation = Projectile.velocity.ToRotation() - RecoilRotation + CycleOffset;
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
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.PiOver2);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation + MathHelper.PiOver2);

        }


        public override bool PreDraw(ref Color lightColor)
        {

            Vector2[] points = rope.GetPoints();


            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector2 start = points[i] - Main.screenPosition;
                Vector2 end = points[i + 1] - Main.screenPosition;

                // Optional: Draw the rope segment with a texture
                Texture2D ropeTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Projectiles/Weapons/Ranged/avatarRifle_Cloth").Value;

                Vector2 RopeDirection = end - start;
                float Roperotation = (end - start).ToRotation();
                float length = RopeDirection.Length();


                //Main.EntitySpriteDraw(ropeTexture, start, frame, lightColor, Projectile.rotation, new Vector2(0, ropeTexture.Height / 2f), Projectile.scale, spriteEffects, 0);
                Main.spriteBatch.Draw(ropeTexture, start, null, lightColor, Roperotation + MathHelper.PiOver2, new Vector2(0, ropeTexture.Height / 2f), new Vector2(length / ropeTexture.Width, 1f), SpriteEffects.None, 0f);
            }



            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            Rectangle frame = texture.Frame(1, 11, 0, Projectile.frame);


            float rotation = Projectile.rotation;
            //SpriteEffects direction = SpriteEffects.None;

            SpriteEffects spriteEffects = Projectile.direction * Player.gravDir < 0 ? SpriteEffects.FlipVertically : 0;
            Vector2 origin = new Vector2(frame.Width / 2 - 24, frame.Height / 2 - 7 * Projectile.direction * Player.gravDir);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);
            //Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);
            //Main.EntitySpriteDraw(texture, drawPosition, frame, lightColor, Projectile.rotation, frame.Size() * 0.5f, 1f, 0, 0);

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