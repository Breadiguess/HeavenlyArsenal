using CalamityMod;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Gores;
using HeavenlyArsenal.Content.Items.Weapons.Ranged.LeverAction;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Core.Utilities;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;


namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.FuneralDirge
{
    public class AvatarRifle_Holdout : ModProjectile
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public static readonly SoundStyle FireSoundNormal = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/New/avatar rifle shot normal ", 3);

        public static readonly SoundStyle FireSoundStrong = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/New/avatar rifle shot strong ", 3);

        public static readonly SoundStyle FireSoundSuper = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/New/avatar rifle shot super ", 2);


        public static readonly SoundStyle ReloadSound = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/New/avatar rifle reload ", 2);

        public static readonly SoundStyle CycleSound = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/AvatarRifle_Cycle_Dronnor1");
        public static readonly SoundStyle CycleEmptySound = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/AvatarRifle_Cycle");
        public static readonly SoundStyle MagEmptySound = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Ranged/AvatarRifle/AvatarRifle_ClipEject");


        public int ExistenceTimer
        {
            get;
            set;
        }
        private Rope rope;

        private Vector2[] ribbonPoints;
        private Vector2[] ribbonVels;
        public static Asset<Texture2D> BeadRopeTexture;
        public static Asset<Texture2D> StringRopeTexture;


        public ref Player Owner => ref Main.player[Projectile.owner];

        private AvatarRifleState CurrentState = AvatarRifleState.Firing;
        private float StateTimer = 0;


        public ref float Time => ref Projectile.ai[0];
        public const float MaxAmmo = 7;
        public int AmmoCount = 7; // Total shots before reload
        public int ReloadDuration = AvatarRifle2.ReloadTime; // Duration for reload (in frames)
        public Vector2 origin = new Vector2(50, 50);

        private enum AvatarRifleState
        {
            Idle, // Idle state           
            Firing,  // Firing a shot
            PostFire, //handle recoil for a better visual experience
            Cycle,   // Cycle before cycling the bolt
            Reload   // Reloading after all shots are fired
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 19;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.MaxUpdates = 2;
            Player player = Main.player[Projectile.owner];
        }
        public override void OnSpawn(IEntitySource source)
        {
            AmmoCount = Math.Abs(Owner.GetModPlayer<AvatarRiflePlayer>().AvatarRifleCounter);
            Vector2 RopeStart = Projectile.Center + new Vector2(-19, 10); // Adjust offsets as needed
            Vector2 endPoint = Projectile.Center + new Vector2(78 * Projectile.direction, 10);
            rope = new Rope(RopeStart, endPoint, segmentCount: 30, segmentLength: 5f, gravity: new Vector2(0, 10f));
            rope.tileCollide = false;
            rope.damping = 0.5f;
        }
        private void CreateDustAtOrigin()
        {
            Vector2 dustPosition = Projectile.Center;
            Dust dust = Dust.NewDustDirect(dustPosition, 1, 1, DustID.Smoke, 0f, 0f, 150, Color.White, 1f);
            dust.velocity *= 0.3f;
            dust.noGravity = true;
        }


        public override void AI()
        {
            //WeaponBar.DisplayBar(Color.SlateBlue, Color.Lerp(Color.DeepSkyBlue, Color.Crimson, Utils.GetLerpValue(0.3f, 0.8f, (float)Main.LocalPlayer.GetModPlayer<HeavenlyArsenalPlayer>().AvatarRifleCounter/7, true)), (float)Main.LocalPlayer.GetModPlayer<HeavenlyArsenalPlayer>().AvatarRifleCounter/7, 10, 1,new Vector2(0,-40));
            RibbonPhysics();
            Vector2 RopeStart = Projectile.Center + new Vector2(-16, Projectile.direction * 7).RotatedBy(Projectile.rotation);
            Vector2 endPoint = Projectile.Center + new Vector2(95, Projectile.direction * -10).RotatedBy(Projectile.rotation);

            rope.segments[0].position = RopeStart;
            rope.segments[^1].position = endPoint;
            rope.gravity = new Vector2(0f, 0.5f);
            rope.Update();
            //CreateDustAtOrigin();
            Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter);
            UpdateProjectileHeldVariables(armPosition);
            ManipulatePlayerVariables();

            if (Owner.HeldItem.type == ModContent.ItemType<AvatarRifle2>())
            {
                switch (CurrentState)
                {
                    case AvatarRifleState.Idle:
                        if (Owner.controlUseItem)
                        {
                            CurrentState = AvatarRifleState.Firing;
                        }
                        break;
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
                CurrentState = AvatarRifleState.Idle;
                StateTimer = 0;
                Projectile.Kill();
            }

            Time++;
        }

        public bool Cycled = false;
        private float CycleOffset = 0f;
        public bool HasShellToEject = true;

        private void HandleFiring()
        {
            if (Owner.channel && Owner.HasAmmo(Owner.HeldItem))
            {
                SoundStyle baseSound;
                // pick the correct base sound
                if (AmmoCount >= 5)
                {
                    baseSound = AssetDirectory.Sounds.Items.Weapons.AvatarRifle.FireSoundNormal;

                }
                else if (AmmoCount < 5 && AmmoCount > 1)
                {
                    baseSound = AssetDirectory.Sounds.Items.Weapons.AvatarRifle.FireSoundStrong;

                }
                else
                {
                    baseSound = AssetDirectory.Sounds.Items.Weapons.AvatarRifle.FireSoundSuper;

                }
                //var baseSound = AmmoCount >= 3? FireSoundNormal: AmmoCount > 2 ? FireSoundStrong: FireSoundSuper;
                var sound = baseSound.WithVolumeScale(AmmoCount == 1 ? 2 : 1).WithPitchOffset(AmmoCount == 1 ? 0 : MaxAmmo - AmmoCount / 10);

                SoundEngine.PlaySound(sound, Projectile.position);
                var player = Owner.GetModPlayer<AvatarRiflePlayer>();
                player.AvatarRifleCounter--;


                AmmoCount--;// = (int)MaxAmmo;

                //Main.NewText(AmmoCount + ", " + player.AvatarRifleCounter);
                //AmmoCount = (int)Math.Floor(MaxAmmo);
                FireProjectile();

                Cycled = false;
                CurrentState = AvatarRifleState.PostFire;
                StateTimer = AvatarRifle2.CycleTime / 2;
            }

        }
        private void FireProjectile()
        {
            SoundEngine.PlaySound(FireSoundNormal);
            int bulletAMMO = ProjectileID.Bullet;
            Owner.PickAmmo(Owner.ActiveItem(), out bulletAMMO, out float SpeedNoUse, out int bulletDamage, out float kBackNoUse, out int _);
            Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);
            Vector2 tipPosition = armPosition + Projectile.velocity * Projectile.width * 1.55f + new Vector2(3, -3);
            Vector2 MuzzleFlash = armPosition + Projectile.velocity * Projectile.width * 2f + new Vector2(3, -6);
            CreateMuzzleFlash(MuzzleFlash, Projectile.velocity);

            float AmmoDifference = MaxAmmo - AmmoCount;
            Main.NewText(AmmoCount);
            RecoilRotation += Projectile.spriteDirection * MathHelper.ToRadians(34f);
            Projectile shot = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), tipPosition, Projectile.velocity * 12, bulletAMMO, Projectile.damage + (int)(MathF.Pow(AmmoDifference, MaxAmmo)), Projectile.knockBack, Projectile.owner);
            shot.GetGlobalProjectile<AvatarRifleSuperBullet>().hasEmpowerment = true;
            shot.GetGlobalProjectile<AvatarRifleSuperBullet>().empowerment = (int)MathHelper.Lerp((int)MathF.Pow(MaxAmmo - AmmoCount, 2), 2, (7 - AmmoCount) / MaxAmmo);

            AvatarRifle_MuzzleFlash darkParticle = AvatarRifle_MuzzleFlash.pool.RequestParticle();
            darkParticle.Prepare(tipPosition, Projectile.rotation, 70);


            ParticleEngine.Particles.Add(darkParticle);
            recoilIntensity = maxRecoil;
        }
        private void HandlePostFire()
        {
            if (StateTimer > 0)
            {
                StateTimer--;
                RecoilRotation *= 0.9f;
                return;
            }
            RecoilRotation = 0f;
            HasShellToEject = true;
            var player = Owner.GetModPlayer<AvatarRiflePlayer>();
           
           // Main.NewText(AmmoCount + ", " + player.AvatarRifleCounter);
            bool hasAmmoLeft = player.AvatarRifleCounter > 0;
            Owner.SetDummyItemTime(hasAmmoLeft ? 12 : 10);
            if (hasAmmoLeft)

            {

                float attackSpeed = Owner.GetAttackSpeed(DamageClass.Ranged);
                CurrentState = AvatarRifleState.Cycle;


            }
            else
            {
                SoundEngine.PlaySound(MagEmptySound, Projectile.Center).WithVolumeBoost(1.2f);

                CurrentState = AvatarRifleState.Reload;
                StateTimer = ReloadDuration;
            }
        }
        private void HandleCycle()
        {
            float attackSpeed = Owner.GetAttackSpeed(DamageClass.Generic);

            float SpeedMulti = 1f;
            if (Owner.GetModPlayer<AvatarRiflePlayer>().AvatarRifleEmpowered)
            {
                SpeedMulti = 0.67f;
            }
            //SpeedMulti += 1-attackSpeed;


            //soft cap at 50, hard cap at 30

            int baseHoldCyclePosition = 50; // base duration in ticks
            float holdCyclePosition = baseHoldCyclePosition * SpeedMulti;
            Math.Clamp(holdCyclePosition, 30, 1000);
            Owner.SetDummyItemTime(14);

            // Animation frame calculations
            int totalFrames = 11;
            int frameDuration = (int)holdCyclePosition / totalFrames;

            if (!Cycled)
            {
                StateTimer = MathHelper.Clamp(AvatarRifle2.CycleTime * SpeedMulti, 30, 1999);
                //Main.NewText($"StateTimer: {StateTimer}");
                SoundEngine.PlaySound(CycleSound.WithVolumeScale(1.5f).WithPitchOffset(Main.rand.NextFloat(-0.1f, 0.1f)), Projectile.position);
                Cycled = true;
                Projectile.frame = 0; // Start animation from the first frame
                Projectile.frameCounter = 0; // Reset frame counter
            }

            if (StateTimer > holdCyclePosition)
            {
                CycleOffset = Projectile.spriteDirection * MathHelper.ToRadians(15f);

                float Rot = Projectile.Center.ToRotation() + MathHelper.ToRadians(Projectile.frame) * 10 - MathHelper.PiOver2;
                Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Rot);


                if (Projectile.frameCounter == 4)
                {
                    if (HasShellToEject)
                    {
                        HasShellToEject = false;
                        EjectShell(203, origin, -20, 20);
                    }
                }
                if (++Projectile.frameCounter > frameDuration * SpeedMulti)
                {

                    Projectile.frameCounter = 0; // Reset the counter
                    Projectile.frame++; // Move to the next frame

                    if (Projectile.frame >= totalFrames)
                        Projectile.frame = 10; //stay here.
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
                Owner.SetDummyItemTime(0);
                // End of cycling state
                CycleOffset = 0f;
                if (Owner.controlUseItem)
                    CurrentState = AvatarRifleState.Firing;
                else
                    CurrentState = AvatarRifleState.Idle;
                Cycled = false;
                Projectile.frame = 0; // Reset frame to the starting frame

            }

            StateTimer--;
        }
        private void HandleReload()
        {

            int totalFrames = 8; // Total frames for the projectile
            int frameDuration = totalFrames;
            if (StateTimer == ReloadDuration)
            {
                Owner.SetDummyItemTime(ReloadDuration);
                Projectile.frame = 11;

                //Main.NewText($"Reloading", Color.AntiqueWhite);
                Gore.NewGore(Projectile.GetSource_FromThis(),
                               Projectile.Center - new Vector2(30 * Projectile.direction, 0),
                               new Vector2(Projectile.direction * -4f, 0f),
                               ModContent.GoreType<MagEjectGore>(), 1);
            }

            // Play reload sound halfway through the reload
            if (StateTimer == ReloadDuration / 3)
            {
                SoundEngine.PlaySound(ReloadSound, Projectile.Center).WithVolumeBoost(1.2f);
            }

            if (StateTimer > 0)
            {
                StateTimer--; // Count down the reload timer
                CycleOffset = Projectile.spriteDirection * MathHelper.ToRadians(15f);
                if (StateTimer <= ReloadDuration / 3)
                {
                    if (++Projectile.frameCounter > frameDuration)
                    {
                        Projectile.frameCounter = 0;

                        if (Projectile.frame < 17)
                            Projectile.frame++;

                        // Eject the magazine when reaching frame 14
                        if (Projectile.frame == 14)
                        {

                        }
                    }
                }
            }
            else
            {
                Owner.GetModPlayer<AvatarRiflePlayer>().AvatarRifleCounter = (int)MaxAmmo;
                Owner.SetDummyItemTime(1);
                Projectile.frame = 0;
                CycleOffset = 0f;
                SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot.WithVolumeScale(1.5f).WithPitchOffset(0.8f), Projectile.position);

                // Reset ammo and transition back to Firing state
                AmmoCount = 7;
                CurrentState = AvatarRifleState.Firing;
                // Main.NewText($"Reloaded!", Color.AntiqueWhite);
                //Main.NewText($"AvatarRifleCounter = {Owner.GetModPlayer<HeavenlyArsenalPlayer>().AvatarRifleCounter}", Color.AntiqueWhite);

            }
        }
        private void EjectShell(int SparkCount, Vector2 ShellPosition, float ShellVelocityMin, float ShellVelocityMax)
        {
            int BulletVariation = (int)Main.rand.NextFloat(ShellVelocityMin, ShellVelocityMax);
            Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Left, new Vector2(Projectile.direction * -5f, -7f), ModContent.GoreType<BulletGore>(), 1);
            for (int i = 0; i < SparkCount; i++)
            {
                float CasingVariation = Main.rand.NextFloat(ShellVelocityMin, ShellVelocityMax);
                int jelly = Dust.NewDust(ShellPosition, 1, 1, DustID.Torch, Projectile.velocity.X + CasingVariation, Projectile.velocity.Y + CasingVariation, 150, default, 1);
            }
        }


        private float recoilIntensity = 0f; // Tracks the current recoil intensity
        private float RecoilRotation = 0f;

        private const float maxRecoil = 20f; // Maximum recoil amount
        private float recoilRecoverySpeed = 0.5f; // Speed at which recoil eases out

        private void CreateMuzzleFlash(Vector2 muzzlePosition, Vector2 projectileVelocity)
        {
            int numParticles = 40; // Number of particles in the muzzle flash
            float angleVariance = MathHelper.ToRadians(30f); // Spread angle for the muzzle flash particles
            float particleSpeed = 6f + Main.rand.NextFloat(-2f, 4f); // Speed of particles moving outward

            for (int i = 0; i < numParticles; i++)
            {
                // Randomize particle direction within a cone based on the projectile's direction
                float randomAngle = Main.rand.NextFloat(-angleVariance, angleVariance);
                Vector2 direction = projectileVelocity.RotatedBy(randomAngle).SafeNormalize(Vector2.UnitX); // Normalize the direction
                Vector2 velocity = direction * particleSpeed * Main.rand.NextFloat(0.7f, 1.2f); // Randomize speed slightly

                // Create the dust at the muzzle position


                Dust dust = Dust.NewDustPerfect(muzzlePosition, DustID.AncientLight, velocity, 100, GetRandomFlashColor(), Main.rand.NextFloat(1.5f, 2f));
                dust.noGravity = true; // Floaty effect for the flash
                dust.fadeIn = 1f;
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
            Projectile.position = armPosition + Projectile.velocity.SafeNormalize(Vector2.UnitX) + recoilOffset;
            Projectile.Center = armPosition + recoilOffset;
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 oldVelocity = Projectile.velocity;
                float aimInterpolant = Utils.GetLerpValue(10f, 40f, Projectile.Distance(Main.MouseWorld), true);
                Vector2 tipPosition = armPosition + Projectile.velocity.SafeNormalize(Vector2.Zero) * Projectile.width * 0.85f + new Vector2(0, -3);
                Projectile.velocity = Vector2.Lerp(Projectile.Center.SafeDirectionTo(Main.MouseWorld), Projectile.SafeDirectionTo(Main.MouseWorld), aimInterpolant);


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



        private void DrawRibbon(Color lightColor)
        {
            Texture2D stringRopeTexture = GennedAssets.Textures.GreyscaleTextures.WhitePixel; // Rope texture
            Texture2D beadRopeTexture = GennedAssets.Textures.SecondPhaseForm.Beads3;        // Object at the end

            if (ribbonPoints != null)
            {
                for (int i = 0; i < ribbonPoints.Length - 1; i++)
                {
                    Vector2 direction = ribbonPoints[i + 1] - ribbonPoints[i];
                    float rotation = direction.ToRotation(); // Rotate to align texture with segment direction
                    float segmentLength = direction.Length(); // Distance between points determines stretch

                    // Use scaling/stretching for the rope appearance
                    Vector2 stretch = new Vector2(1f, segmentLength / stringRopeTexture.Height);

                    Main.EntitySpriteDraw(
                        stringRopeTexture,
                        ribbonPoints[i] - Main.screenPosition,
                        null, // Full texture
                        lightColor.MultiplyRGB(Color.Crimson),
                        rotation - MathHelper.PiOver2, // Adjust rotation for vertical texture alignment
                        new Vector2(stringRopeTexture.Width / 2, 0), // Origin at the texture's center-top
                        stretch,
                        SpriteEffects.None,
                        0
                    );
                }

                // Draw the object (e.g., bead/cloth) at the end of the rope
                Vector2 endPoint = ribbonPoints[ribbonPoints.Length - 1];

                // Calculate rotation for the last segment
                Vector2 lastDirection = endPoint - ribbonPoints[ribbonPoints.Length - 2]; // Direction from the second-to-last point to the last point
                float lastRotation = lastDirection.ToRotation();
                Vector2 ClothAnchorPoint = new Vector2(beadRopeTexture.Width / 2, 0);

                Main.EntitySpriteDraw(
                    beadRopeTexture,
                    endPoint - Main.screenPosition,
                    null, // Full texture
                    lightColor,
                    lastRotation - MathHelper.PiOver2, // Adjust rotation to align with texture
                    ClothAnchorPoint, // Origin at the object's center
                    0.13f, // Scale factor
                    SpriteEffects.None,
                    0
                );
            }
        }


        private Vector2 AnchorPosition()
        {

            return Projectile.Center + new Vector2(95, -5 * Projectile.spriteDirection).RotatedBy(Projectile.rotation) * Projectile.scale;
        }

        public void RibbonPhysics()
        {
            int length = 6; // Number of ribbon segments
            Vector2 gravity = new Vector2(0, 0.7f); // Gravity to pull the ribbon downward
            float maxDistance = 3f; // Spacing between segments
            float dampening = 0.5f; // Damping factor to stabilize motion

            // Initialize velocities if null
            if (ribbonVels == null)
            {
                ribbonVels = new Vector2[length];
            }

            // Initialize ribbon points if null
            if (ribbonPoints == null)
            {
                ribbonPoints = new Vector2[length];
                for (int i = 0; i < ribbonPoints.Length; i++)
                {
                    ribbonPoints[i] = Projectile.Center;
                }
            }


            //ribbonPoints[0] = AnchorPosition();
            float drawScale = Projectile.scale;


            // Update ribbon segments with physics
            for (int i = 1; i < ribbonPoints.Length; i++)
            {
                // Apply velocity and gravity
                ribbonVels[i] *= dampening; // Reduce velocity slightly to stabilize
                ribbonVels[i] += gravity;  // Apply gravity
                ribbonPoints[i] += ribbonVels[i]; // Update position based on velocity

                // Enforce distance constraint
                Vector2 direction = ribbonPoints[i] - ribbonPoints[i - 1];
                float currentDistance = direction.Length();

                if (currentDistance > maxDistance)
                {
                    // Correct positions to maintain spacing
                    Vector2 correction = direction.SafeNormalize(Vector2.Zero) * (currentDistance - maxDistance) * 0.5f;
                    ribbonPoints[i] -= correction;
                    ribbonPoints[i - 1] += correction;
                }

                // Apply rotation based on the segment's direction
                float segmentRotation = direction.ToRotation();
                if (i == ribbonPoints.Length - 1)
                {
                    // Store or apply the rotation for the last segment
                    ribbonVels[i] = segmentRotation.ToRotationVector2() * ribbonVels[i].Length();
                }
                ribbonPoints[0] = AnchorPosition();
            }
        }


        private Texture2D getBulletToDraw()
        {

            int bulletAMMO = AmmoID.Bullet;

            Texture2D BulletValue = TextureAssets.Item[bulletAMMO].Value;

            return BulletValue;
        }
        private void DrawBullet(Vector2 Drawpos, Color lightColor, Texture2D BulletTexture)
        {
            SpriteEffects spriteEffects = Projectile.direction * Owner.gravDir < 0 ? SpriteEffects.FlipVertically : 0;


            Rectangle frame = BulletTexture.Frame(1, 1, 0, Projectile.frame);
            Main.EntitySpriteDraw(BulletTexture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

        }

        private void drawRope(Color lightColor)
        {
            Vector2[] points = rope.GetPoints();


            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector2 start = points[i] - Main.screenPosition;
                Vector2 end = points[i + 1] - Main.screenPosition;


                Texture2D ropeTexture = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

                Vector2 RopeDirection = end - start;
                float Roperotation = (end - start).ToRotation();
                float length = RopeDirection.Length();



                Main.spriteBatch.Draw(ropeTexture, start, null, lightColor.MultiplyRGB(Color.Crimson), Roperotation, new Vector2(0, ropeTexture.Height / 2f), new Vector2(length / ropeTexture.Width, 3f), SpriteEffects.None, 0f);
            }


        }
        public override bool PreDraw(ref Color lightColor)
        {
            /*
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);

            Main.spriteBatch.PrepareForShaders();
            //new Texture Placeholder = GennedAssets.Textures.Extra.Code;
            ManagedShader postProcessingShader = ShaderManager.GetShader("HeavenlyArsenal.FusionRifleClothPostProcessingShader");
            postProcessingShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly);
            postProcessingShader.TrySetParameter("FlameColor", new Color(208, 37, 40).ToVector4());
            postProcessingShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 0, SamplerState.LinearWrap);
            postProcessingShader.Apply();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            */

            drawRope(lightColor);
            DrawRibbon(lightColor);


            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;//ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Projectiles/Weapons/Ranged/AvatarRifleProj/AvatarRifle_HoldoutN").Value;//
            //Texture2D Roots = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Weapons/Ranged/FuneralDirge/AvatarRifle_Holdout_Roots").Value;


            Vector2 drawPosition = Projectile.Center - Main.screenPosition;

            Rectangle frame = texture.Frame(1, 19, 0, Projectile.frame);


            float rotation = Projectile.rotation;
            SpriteEffects spriteEffects = Projectile.direction * Owner.gravDir < 0 ? SpriteEffects.FlipVertically : 0;
            Vector2 origin = new Vector2(frame.Width / 2 - 46, frame.Height / 2 - -2 * Projectile.direction * Owner.gravDir);


            float wind = Projectile.rotation + AperiodicSin(Main.GlobalTimeWrappedHourly * 0.56f + Projectile.Center.X + Projectile.Center.Y) *
            //clamps rotation kinda
            0.033f
            + Main.windSpeedCurrent * 0.17f;


            //Rectangle Lillyframe = Roots.Frame(1, 1, 0, 0);
            Vector2 LillyPos = new Vector2(Projectile.Center.X, Projectile.Center.Y);
            //Main.EntitySpriteDraw(Roots, LillyPos - Main.screenPosition, Lillyframe, lightColor, wind,new Vector2(origin.X, origin.Y - 15 * Projectile.spriteDirection), LillyScale, spriteEffects, 0f);
            Vector2 Bulletorigin = new Vector2(frame.Width / 2 - 24, frame.Height / 2 - 7 * Projectile.direction * Owner.gravDir);
            if (Owner.GetModPlayer<AvatarRiflePlayer>().AvatarRifleEmpowered)
            {
                for (int i = 0; i < 6; i++)
                {

                    float glowsize = 1.01f;

                    Vector2 drawOffset = (MathHelper.Pi * i / 6f).ToRotationVector2() + Vector2.One * (float)Math.Sin(Main.GlobalTimeWrappedHourly + i * 6) * 4f;
                    Main.spriteBatch.Draw(texture, drawPosition + drawOffset, frame, Color.Crimson with { A = 1 }, rotation, origin, glowsize, spriteEffects, 0f);
                }
            }





            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);


            /*
            Utils.DrawBorderString(Main.spriteBatch, "| Is empowered: " + Owner.GetModPlayer<AvatarRiflePlayer>().AvatarRifleEmpowered.ToString(), Projectile.Center - Vector2.UnitY * 60 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "| Rifle Charge : " + Owner.GetModPlayer<AvatarRiflePlayer>().RifleCharge.ToString() + " | RifleCharge Decay: " + Owner.GetModPlayer<AvatarRiflePlayer>().RifleChargeDecay, Projectile.Center - Vector2.UnitY * 80 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "| Empowerment Timer: " + Owner.GetModPlayer<AvatarRiflePlayer>().AvatarRifleEmpoweredTimer.ToString(), Projectile.Center - Vector2.UnitY * 100 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "| State: " + CurrentState.ToString() + " | StateTimer: " + StateTimer, Projectile.Center - Vector2.UnitY * 120 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "| AttackSpeed: " + Owner.GetTotalAttackSpeed<RangedDamageClass>().ToString(), Projectile.Center - Vector2.UnitY * 140 - Main.screenPosition, Color.White);
            //Utils.DrawBorderString(Main.spriteBatch, "| AmmoType: " + Owner.coinLuck.ToString(), Projectile.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.White);
            */


            return false;
        }


        public override bool? CanDamage() => false;
    }



}