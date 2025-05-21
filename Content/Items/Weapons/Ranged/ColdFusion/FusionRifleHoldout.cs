using CalamityMod;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Tiles;
using HeavenlyArsenal.Content.Items.Weapons.Melee;
using HeavenlyArsenal.Content.Projectiles.Misc;
using HeavenlyArsenal.Core.Physics.ClothManagement;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using NoxusBoss.Core.Graphics.RenderTargets;
using System;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using Particle = Luminance.Core.Graphics.Particle;



namespace HeavenlyArsenal.Content.Items.Weapons.Ranged.ColdFusion
{
    public class FusionRifleHoldout : ModProjectile
    {
        /// <summary>
        /// The cloth simulation attached to the front of this rifle.
        /// </summary>
        public ClothSimulation Cloth
        {
            get;
            private set;
        }

        /// <summary>
        /// The render target responsible for the rendering of this cloth.
        /// </summary>
        public static InstancedRequestableTarget ClothTarget
        {
            get;
            private set;
        }
        public ref Player Owner => ref Main.player[Projectile.owner];

        public new string LocalizationCategory => "Projectiles.Ranged";
        public bool OwnerCanShoot => Owner.HasAmmo(Owner.ActiveItem()) && !Owner.noItems && !Owner.CCed;
        public float ChargeupInterpolant => Utils.GetLerpValue(FusionRifle.ShootDelay, FusionRifle.MaxChargeTime, ChargeTimer, true);
        public ref float CurrentChargingFrames => ref Projectile.ai[0];
        public ref float ChargeTimer => ref Projectile.ai[1];
        public ref float ShootDelay => ref Projectile.localAI[0];
        //public override int AssociatedItemID => ModContent.ItemType<FusionRifle>();
        //public override int IntendedProjectileType => ModContent.ProjectileType<    ColdBurst>();
        public float Time { get; private set; }

        public static float CurrentChargeTime = FusionRifle.MaxChargeTime; // Default to MaxChargeTime

        public override void SetStaticDefaults()
        {
            ClothTarget = new InstancedRequestableTarget();
            Main.ContentThatNeedsRenderTargets.Add(ClothTarget);
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;

            base.SetStaticDefaults();
        }

        public static readonly SoundStyle Charging = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Ranged/FusionRifle/ZealotsCharge");
        public static readonly SoundStyle Fire = new SoundStyle("HeavenlyArsenal/Assets/Sounds/Items/Ranged/FusionRifle/ZealotsFire");
        public override void SetDefaults()
        {
            
            Projectile.width = Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.MaxUpdates = 4;
        }

        //todo: make this not static
        public static int BurstCount = 0; 

        private enum FusionRifleState
        {
            Idle,
            Charging,  
            Firing,    
            Delay      
        }

        private FusionRifleState CurrentState = FusionRifleState.Idle;
        private int StateTimer = 0; 



        public override void AI()
        {
            Cloth ??= new ClothSimulation(new Vector3(Projectile.Center, 0f), 11, 15, 3f, 60f, 0.02f);

            Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);


            if (Owner.HeldItem.type != ModContent.ItemType<FusionRifle>() || Owner.CCed || Owner.dead)
            {
                Projectile.Kill();
                return;
            }
            switch (CurrentState)
            {
                case FusionRifleState.Idle:
                    ChargeTimer = 0;
                    if (Owner.controlUseItem)
                    {
                        CurrentState = FusionRifleState.Charging;
                    }
                    break;
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
            UpdateCloth();
            Time++;
        }


        
        private void HandleCharging()
        {
            if (Owner.controlUseItem && Owner.channel)
            {
                if(ChargeTimer == 0)
                {
                    Owner.GetModPlayer<FusionRiflePlayer>().BurstCounter = 0;
                    ActiveSound activeSound = SoundEngine.FindActiveSound(Charging);
                    if (activeSound == null)
                    {
                        //activeSound.Stop();
                    }
                    SoundEngine.PlaySound(Charging with { MaxInstances = 0 , Volume = 1f, Type = SoundType.Sound}, Projectile.Center);
                }
                ChargeVFX();

                ChargeTimer +=  1 + 3 * Owner.GetModPlayer<FusionRiflePlayer>().BurstTier/7; 
                //Math.Clamp(ChargeTimer, 0, 120f);
                //ChargeTimer
            }

            if (ChargeTimer >= FusionRifle.MaxChargeTime)
            {
                
                CurrentState = FusionRifleState.Firing;
                StateTimer = 0;
                BurstCount = FusionRifle.BoltsPerBurst;
            }

            if (!Owner.controlUseItem || !Owner.channel)
            {

                if (ChargeTimer > 0)
                {
                    float intensity = ChargeupInterpolant * 0.5f;
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.RuneWizard, -Projectile.velocity* 10, 100, Color.Cyan, intensity * 10);
                    dust.noGravity = false;
                    ChargeTimer -= MathHelper.Lerp(ChargeTimer, 0, 0.1f);
                    if (ChargeTimer < 0)
                    {
                        ScreenShakeSystem.StartShake(28f, shakeStrengthDissipationIncrement: 0.4f);
                        GeneralScreenEffectSystem.ChromaticAberration.Start(Owner.Center, 3f, 90);
                        GeneralScreenEffectSystem.HighContrast.Start(Owner.Center, 3, 33);
                        ChargeTimer = 0;
                        CurrentState = FusionRifleState.Idle;
                    }

                }
            }

            
        }

        private void ChargeVFX()
        {
            
            float shakeIntensity = 2f * ChargeupInterpolant;
            Projectile.position += new Vector2(
                Main.rand.NextFloat(-shakeIntensity, shakeIntensity),
                Main.rand.NextFloat(-shakeIntensity, shakeIntensity)
            );

            // Add inward swirling dust effect with consistent 3D Y-axis tilt, no Z-axis distortion
            float initialDustRadius = 50f; // Starting radius (adjust as needed)
            float finalDustRadius = 10f;   // Ending radius near the target (adjust as needed)
            float dustRadius = MathHelper.Lerp(initialDustRadius, finalDustRadius, ChargeTimer / CurrentChargeTime);

            // Adjustable offset for the barrel (change these values to position the swirl effect)
            Vector2 barrelOffset = new Vector2(45f, Projectile.direction * -7f); // Customize the base position of the dust effect
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
                float adjustedY = tiltedY + unrotatedX * (float)Math.Sin(yAxisTilt) * 0.5f; // Minimal Z-axis impact on Y

                // Finalize the swirl position by rotating it with the projectile's orientation
                Vector2 finalSwirlOffset = new Vector2(tiltedX, adjustedY).RotatedBy(Projectile.rotation);
                Vector2 dustPosition = Projectile.Center + barrelOffset + finalSwirlOffset;
                Dust dust = Dust.NewDustPerfect(dustPosition, DustID.UnusedWhiteBluePurple, Vector2.Zero, 100, Color.Cyan, 1.5f);
                dust.noGravity = true;
            }
        }
        private void ConstrainParticle(Vector2 anchor, ClothPoint point, float angleOffset)
        {
            if (point is null)
                return;

            float xInterpolant = point.X / (float)Cloth.Width;
            float angle = MathHelper.Lerp(MathHelper.PiOver2, MathHelper.TwoPi - MathHelper.PiOver2, xInterpolant);

            Vector2 offset = new Vector2(MathF.Cos(angle + angleOffset) * 10f, 0f).RotatedBy(Projectile.rotation);
            Vector3 ring = new Vector3(offset.X, offset.Y, MathF.Sin(angle - MathHelper.PiOver2) * 10f);
            ring.Y += point.Y * 6f;

            point.Position = new Vector3(anchor, 0f) + ring;
            point.IsFixed = true;
        }

        private void UpdateCloth()
        {
            int steps = 24;
            float windSpeed = Math.Clamp(Main.WindForVisuals * Projectile.spriteDirection * 8f, -1.3f, 1.3f);
            Vector2 clothPosition = Projectile.Center + new Vector2(26f, Projectile.velocity.X.NonZeroSign() * -3f).RotatedBy(Projectile.rotation) * Projectile.scale;

            Vector2 previousBarrelEnd = Projectile.Center + Projectile.oldRot[1].ToRotationVector2() * Projectile.scale * 30f;
            Vector2 barrelEnd = Projectile.Center + Projectile.oldRot[0].ToRotationVector2() * Projectile.scale * 30f;
            Vector3 rotationalForce = new Vector3(barrelEnd - previousBarrelEnd, 0f) * -4f;

            //float recoilIntensity = Projectile.velocity.Length() * someScaleFactor; // Replace 'someScaleFactor' with appropriate scaling.
            //Vector3 recoilForce = new Vector3(barrelEnd - previousBarrelEnd, 0f) * recoilIntensity * -4f;

            Vector3 recoilForce = new Vector3(-Projectile.velocity.X * recoilIntensity, 0f, 0f); // Adjust for backward recoil.
            Vector3 combinedForce = rotationalForce + recoilForce;

            for (int i = 0; i < steps; i++)
            {
                for (int x = 0; x < Cloth.Width; x += 2)
                {
                    for (int y = 0; y < 2; y++)
                        ConstrainParticle(clothPosition, Cloth.particleGrid[x, y], 0f);
                    for (int y = 0; y < Cloth.Height; y++)
                    {
                        Vector3 localWind = Vector3.UnitX * (LumUtils.AperiodicSin(Time * 0.01f + y * 0.05f) * windSpeed) * 1.2f;
                        Cloth.particleGrid[x, y].AddForce(localWind +combinedForce);
                    }
                }

                Cloth.Simulate(0.051f, false, Vector3.UnitY * 4f);
            }
        }

        private void HandleFiring()
        {
            //todo: if playing charging sound, stop
            ActiveSound activeSound = SoundEngine.FindActiveSound(Charging);
            if (activeSound != null)
            {
                activeSound.Stop();
            }
            //Owner.itemAnimation = Owner.itemAnimationMax;
            if (BurstCount == FusionRifle.BoltsPerBurst)
            {
                SoundEngine.PlaySound(Fire with { MaxInstances = 0, PitchVariance = 0.1f, Pitch = Owner.GetModPlayer<FusionRiflePlayer>().BurstTier/20 }, Projectile.Center);
                DisipateHeat(true);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Owner.Center * 2f, Projectile.velocity, 10, 3, 10, -0.5f, null));
            }
            if (BurstCount > 0)
            {
                if (StateTimer <= 0)
                {
                    Owner.PickAmmo(Owner.HeldItem, out _, out _, out _, out _, out _);
                    FireBurstProjectile();
                    BurstCount--;
                    //Main.NewText($"BurstCount: {BurstCount}, state: {CurrentState}", Color.AntiqueWhite);
                    StateTimer = 5; 
                }
                else
                {
                    StateTimer--; // Count down the delay timer
                }

                if(BurstCount %3 == 0)
                {
                   
                }
            }
            else
            {

                //Main.NewText($"Bolts fired: {countburst}", Color.AliceBlue);
                
                CurrentState = FusionRifleState.Delay; // Transition to delay state after the burst
                StateTimer = 60 - (int)Owner.GetModPlayer<FusionRiflePlayer>().BurstTier / 7  * 2; 
                //ChargeTimer = 0; // Reset charge
            }
        }

        private void HandleDelay()
        {
            if (StateTimer == 60)
            {
                
            }
            if (StateTimer > 0)
            {
                StateTimer--; // Count down the delay
                DisipateHeat(false);
                //todo: make it so that by the time state is zero, Charge has been fully disipated

                ChargeTimer = MathHelper.Lerp(0,ChargeTimer, StateTimer / 60f); 
               
            }
            else
            {
                ChargeTimer = 0;
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
            Vector2 baseExhaustDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX) * -Projectile.direction; // Base direction opposite of projectile's movement
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
       
        public void FireBurstProjectile()
        {
            Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);

            float chargePower = ChargeupInterpolant;
            
            
            int damage = Projectile.damage * 1+ (int)Owner.GetModPlayer<FusionRiflePlayer>().BurstTier / 14;
            
            
            float knockback = Projectile.knockBack;
            Vector2 spawnPosition = armPosition + Projectile.velocity.SafeNormalize(Vector2.Zero) * 50f;


            float angleVariance = MathHelper.ToRadians(2f);
            float randomAngle = Main.rand.NextFloat(-angleVariance, angleVariance); // Randomize the angle within the variance
            Vector2 adjustedVelocity = Projectile.velocity.RotatedBy(randomAngle); // Apply the random angle to the velocity

            // Spawn the projectile with the adjusted velocity
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, adjustedVelocity * (70f + chargePower),
                ModContent.ProjectileType<ColdBurst>(),
                damage + 500 * (int)Owner.GetModPlayer<FusionRiflePlayer>().BurstTier,
                knockback,
                Projectile.owner
            );


            Vector2 MuzzleFlashPosition = spawnPosition + Projectile.velocity.SafeNormalize(Vector2.Zero) * 70f;
            recoilIntensity = maxRecoil;
        }
        private static float recoilIntensity = 0f; // Tracks the current recoil intensity
        private const float maxRecoil = 10f; // Maximum recoil amount
        private float recoilRecoverySpeed = 0.99f; // Speed at which recoil eases out
        public Vector2 Recoil => Projectile.velocity.SafeNormalize(Vector2.Zero) * recoilIntensity;

        public static Vector2 RecoilOffset = new Vector2(-recoilIntensity, 0);  
        public void UpdateProjectileHeldVariables(Vector2 armPosition)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                float aimInterpolant = Utils.GetLerpValue(0.1f, 1f, Projectile.Distance(Main.MouseWorld), true);
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
                recoilIntensity *= recoilRecoverySpeed;
                if (recoilIntensity < 0f)
                    recoilIntensity = 0f; // Clamp to prevent negative values
            }

            Vector2 recoilOffset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * recoilIntensity;
            
            //Projectile.position = armPosition - Projectile.Size * 0.5f + Projectile.velocity.SafeNormalize(Vector2.UnitY) * 34f + recoilOffset;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.Center = Owner.MountedCenter;
            Projectile.spriteDirection = Projectile.direction;
            Projectile.timeLeft = 2;
        }

        public void ManipulatePlayerVariables()
        {
            Owner.ChangeDir(Projectile.direction);
            Owner.heldProj = Projectile.whoAmI;


            float frontArmRotation = Projectile.rotation;//* 0.5f;
            if (Owner.direction == -1)
            {
                frontArmRotation -= MathHelper.PiOver2;
            }
            
            else
            {
                frontArmRotation -= MathHelper.PiOver2;
            }
                //frontArmRotation = MathHelper.PiOver2;// - frontArmRotation;
            //frontArmRotation += Projectile.rotation + MathHelper.Pi + Owner.direction * MathHelper.PiOver2 + 0.12f;
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, frontArmRotation);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, frontArmRotation);// Projectile.velocity.ToRotation() - MathHelper.PiOver2);
        }

      

        public void AdjustVisualValues(ref float scale, ref float opacity, ref float rotation, float time)
        {
            scale = Utils.GetLerpValue(0.1f, 35f, time, true) * 1.4f;
            opacity = (float)Math.Pow(scale / 1.4f, 2D);
            rotation -= MathHelper.ToRadians(scale * 4f);
        }

        private void DrawCloth()
        {
            Matrix world = Matrix.CreateTranslation(-Projectile.Center.X + 25.5f * Projectile.direction + WotGUtils.ViewportSize.X * 0.5f, -Projectile.Center.Y + Projectile.velocity.Y * 40 +WotGUtils.ViewportSize.Y * 0.5f, 0f);
            Matrix projection = Matrix.CreateOrthographicOffCenter(0f, WotGUtils.ViewportSize.X, WotGUtils.ViewportSize.Y, 0f, -1000f, 1000f);
            Matrix matrix = world * projection;

            ManagedShader clothShader = ShaderManager.GetShader("HeavenlyArsenal.FusionRifleClothShader");
            clothShader.TrySetParameter("opacity", Projectile.Opacity);
            clothShader.TrySetParameter("transform", matrix);
            clothShader.Apply();

            Cloth.Render();
        }

        public override bool PreDraw(ref Color lightColor)
        {

            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle frame = texture.Frame(1, 1, 0, 0);
            float rotation = Projectile.rotation;
            SpriteEffects spriteEffects = Projectile.direction * Owner.gravDir < 0 ? SpriteEffects.FlipVertically : 0;
            Vector2 origin = new Vector2(frame.Width / 2 - 24 * Projectile.direction, frame.Height / 2 -6 * Owner.gravDir);

            Vector2 drawPosition = Projectile.Center - Main.screenPosition;


            float chargeOffset = ChargeupInterpolant * Projectile.scale * 5f;
            Color chargeColor = Color.Lerp(Color.Crimson, Color.Gold, (float)Math.Cos(Main.GlobalTimeWrappedHourly * 7.1f) * 0.5f + 0.5f) * ChargeupInterpolant * 0.6f;
            chargeColor.A = 0;
            ClothTarget.Request(350, 350, Projectile.whoAmI, DrawCloth);
            if (ClothTarget.TryGetTarget(Projectile.whoAmI, out RenderTarget2D clothTarget) && clothTarget is not null)
            {
                Main.spriteBatch.PrepareForShaders();
                //new Texture Placeholder = GennedAssets.Textures.Extra.Code;
                ManagedShader postProcessingShader = ShaderManager.GetShader("HeavenlyArsenal.FusionRifleClothPostProcessingShader");
                postProcessingShader.TrySetParameter("textureSize", clothTarget.Size());
                postProcessingShader.TrySetParameter("edgeColor", new Color(208, 37, 40).ToVector4());
                postProcessingShader.SetTexture(GennedAssets.Textures.SecondPhaseForm.Beads3, 0, SamplerState.LinearWrap);
                postProcessingShader.Apply();

                Main.spriteBatch.Draw(clothTarget, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(Color.White), 0f, clothTarget.Size() * 0.5f, 1f, 0, 0f);
                Main.spriteBatch.ResetToDefault();

            }


            SpriteEffects direction = SpriteEffects.None;
            if (Math.Cos(rotation) < 0f)
            {
                direction = SpriteEffects.FlipHorizontally;
                rotation += MathHelper.Pi;
            }

            if (CurrentState == FusionRifleState.Firing)
            {
                Texture2D Corona = GennedAssets.Textures.GreyscaleTextures.Corona;

                Vector2 CoronaScale = new Vector2(0.1f, 0.2f);
                Vector2 Corigin = new Vector2(Corona.Size().X / 2, Corona.Size().Y / 2);


                Vector2 CoronaPosition = new Vector2(Projectile.Center.X, Projectile.Center.Y) - Main.screenPosition;//Projectile.Center - Main.screenPosition;

                {
                    // Main.spriteBatch.Draw(Glowball, Projectile.Center + Projectile.velocity / 2 - Main.screenPosition, null, lightColor.MultiplyRGB(Color.AntiqueWhite), Projectile.rotation, Gorigin, glowScale, SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(Corona, CoronaPosition, null, (Color.Violet with { A = 0 }) * 0.4f, Projectile.rotation, Corona.Size() * 0.5f, CoronaScale, 0, 0f);

                }
                //Main.GlobalTimeWrappedHourly
            }
            Color stringColor = new(129, 18, 42);


            for (int i = 0; i < 5; i++)
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 6f).ToRotationVector2() * chargeOffset;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, null, chargeColor, rotation, origin, Projectile.scale, direction, 0f);
            }


            Main.spriteBatch.Draw(texture, drawPosition - Recoil, null, Projectile.GetAlpha(lightColor), rotation, origin, Projectile.scale, direction, 0f);
            Main.spriteBatch.ExitShaderRegion();

            Texture2D Glowball = GennedAssets.Textures.GreyscaleTextures.Corona;
            float GlowScale = ChargeupInterpolant * 0.1f;
            Vector2 Gorigin = new Vector2(Glowball.Size().X / 2, Glowball.Size().Y / 2);
            if (ChargeTimer > 1)
            {


                Vector2 armPosition = Owner.RotatedRelativePoint(Owner.MountedCenter, true);



                Vector2 tipPosition = armPosition + Projectile.velocity * Projectile.width * 1.55f + new Vector2(3, -3);
                //todo: atone for my sins
                Main.spriteBatch.Draw(Glowball, tipPosition - Main.screenPosition, null,
                    (Color.Violet with { A = 0 }) * 0.4f,
                    rotation, Gorigin, GlowScale, direction, 0f);
            }

            /*
            Utils.DrawBorderString(Main.spriteBatch, "|State: " + CurrentState.ToString() + " | State Timer: " + StateTimer.ToString(), Projectile.Center - Vector2.UnitY * 90 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "|Charge: " + ChargeTimer.ToString() + " | Charge iter: " + (1 + 3 * Owner.GetModPlayer<FusionRiflePlayer>().BurstTier / 7).ToString() + " | 120/chargeiter " + (120 / (1 + 3 * Owner.GetModPlayer<FusionRiflePlayer>().BurstTier / 7)).ToString(), Projectile.Center - Vector2.UnitY * 110 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "|MaxCharge: " + FusionRifle.MaxChargeTime.ToString(), Projectile.Center - Vector2.UnitY * 130 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "|BurstCounter: " + Owner.GetModPlayer<FusionRiflePlayer>().BurstCounter.ToString() + " | BurstTier: " + Owner.GetModPlayer<FusionRiflePlayer>().BurstTier, Projectile.Center - Vector2.UnitY * 150 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "|Controlled burst active: " + Owner.GetModPlayer<FusionRiflePlayer>().ControlledBurstActive + " | ControlledBurst Timer: " + Owner.GetModPlayer<FusionRiflePlayer>().ControlledBurstTimer.ToString(), Projectile.Center - Vector2.UnitY * 170 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "|Recoil Offset: " + RecoilOffset.ToString(), Projectile.Center - Vector2.UnitY * 190 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "|Estimated damage: " + (Projectile.damage * (1 + Owner.GetModPlayer<FusionRiflePlayer>().BurstTier / 14)).ToString(), Projectile.Center - Vector2.UnitY * 210 - Main.screenPosition, Color.White);

            Utils.DrawBorderString(Main.spriteBatch, "|VolatileRound Chance " + Owner.GetModPlayer<FusionRiflePlayer>().VolCount.ToString() + " | Volatile Rounds: " + Owner.GetModPlayer<FusionRiflePlayer>().VolatileRounds.ToString(), Projectile.Center - Vector2.UnitY * 230 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "|VolatileTimer " + Owner.GetModPlayer<FusionRiflePlayer>().VolatileRoundTimer.ToString(), Projectile.Center - Vector2.UnitY * 250 - Main.screenPosition, Color.White);
            */
            return false;
        }
        public override bool? CanDamage() => false;
    }


    public class VolatileRounds : GlobalNPC
    {
        
        public void ChainExplosion(Player owner, NPC target, int Damage, float Radius, bool Chaining)
        {

            // chose the target
            // radius of the explosion
            // deal damage of the explosion
            // decide whether it should also trigger more volatile explosions on nearby targets.
            Projectile.NewProjectile(owner.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<VolatileExplosion>(), Damage,0, owner.whoAmI);
            
            popVolatile(target);
        }
        public override bool InstancePerEntity => true;
        public int VolatileCooldown; // when volatile is triggered, enter a cooldown which will prevent this npc from recieving volatile again until the cooldown expires.
        public bool VolatileActive = false; // when true, npc will explode after taking some damage 
        public int VolatileTimer = 0; // this tracks how long the npc has had volatile for. after it reaches zero, remove volatile.
        public float VolatileSafe; //if this is above zero, then volatile cannot be triggered.
        public void popVolatile(NPC target)
        {
            target.GetGlobalNPC<VolatileRounds>().VolatileActive = false;
            target.GetGlobalNPC<VolatileRounds>().VolatileCooldown = 120;
            target.GetGlobalNPC<VolatileRounds>().VolatileTimer = 0;
            target.GetGlobalNPC<VolatileRounds>().VolatileSafe = 0;


        }
        public override void ResetEffects(NPC npc)
        {
            if(VolatileActive == false)
            {
                VolatileTimer = 0;
                VolatileSafe = 0;
            }

        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            base.UpdateLifeRegen(npc, ref damage);
        }

        public override void AI(NPC npc)
        {
         
            base.AI(npc);
        }
        public override void PostAI(NPC npc)
        {
            if (VolatileActive)
            {
                if (VolatileSafe > 0)
                {
                    VolatileSafe--;
                }
                if (VolatileTimer > 0 && VolatileSafe == 0)
                {
                    VolatileTimer--;
                }
                if (VolatileTimer == 0)
                {
                    VolatileActive = false;
                }
            }
            else if (VolatileCooldown > 0)
            {
                if (VolatileActive)
                    VolatileActive = false;
                VolatileCooldown--;
            }
            base.PostAI(npc);
        }
        public override void HitEffect(NPC npc, NPC.HitInfo hit)
        {
            if (VolatileActive)
            {
                if (VolatileSafe == 0 && hit.Damage >= Math.Max(500, npc.lifeMax / 100))
                {

                    ChainExplosion(Main.player[npc.target], npc, npc.lifeMax*(int)0.3f, 100, true);
                    
                }
            }
            base.HitEffect(npc, hit);
        }


        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            /*
            Utils.DrawBorderString(Main.spriteBatch, "Is Volatile: " + VolatileActive.ToString(), npc.Center - Vector2.UnitY * 160 - Main.screenPosition, Color.White);

            Utils.DrawBorderString(Main.spriteBatch, "Volatile Timer: " + VolatileTimer.ToString(), npc.Center - Vector2.UnitY * 180 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "VolatileSafe: " + VolatileSafe.ToString(), npc.Center - Vector2.UnitY * 200 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "Volatile Cooldown: " + VolatileCooldown.ToString(), npc.Center - Vector2.UnitY * 220 - Main.screenPosition, Color.White);
            */


            if (VolatileActive)
            {
                Texture2D texture = AssetDirectory.Textures.BigGlowball.Value;
                float GlowScale = 0.3f;
                Vector2 Gorigin = new Vector2(texture.Size().X / 2, texture.Size().Y / 2);

                Main.spriteBatch.Draw(texture, npc.Center - Main.screenPosition, null,
                    drawColor.MultiplyRGB(Color.Purple),
                    npc.rotation, Gorigin, GlowScale, SpriteEffects.None, 0f);
            }
           
            base.PostDraw(npc, spriteBatch, screenPos, drawColor);
        }
    }
}


