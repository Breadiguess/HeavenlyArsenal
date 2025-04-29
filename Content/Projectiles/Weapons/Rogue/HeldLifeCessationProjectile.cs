using CalamityMod;
using HeavenlyArsenal.ArsenalPlayer;
using HeavenlyArsenal.Common.Ui;
using HeavenlyArsenal.Content.Buffs.LifeAndCessation;
using HeavenlyArsenal.Content.Items.Weapons.Rogue;
using Luminance.Core.Graphics;
using Luminance.Core.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Luminance.Common.Utilities.Utilities;



namespace HeavenlyArsenal.Content.Projectiles.Weapons.Rogue;

class HeldLifeCessationProjectile : ModProjectile
{
    public ref float Time => ref Projectile.ai[0];
    public ref Player Player => ref Main.player[Projectile.owner];
    public ref Player Owner => ref Main.player[Projectile.owner];
    public ref float Heat => ref Owner.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeat;

    public const float minHeat = 0;

    public const float maxHeat = 1;

    public float heatIncrement = 0.015f;

    public bool IsAbsorbingHeat
    {
        get;
        set;
    }
    public bool IsDisipateHeat
    {
        get;
        set;
    }
    public float LilyScale
    {
        get;
        set;
    }
    public LoopedSoundInstance AmbientLoop
    {
        get;
        set;
    }
    //draws the dust vfx on attack cone
    private Dust[] heatDusts;
    private  int HeatDustCount = 500; // How many dust particles do we want?


    
    public override void SetStaticDefaults()
    {
       
    }

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true  ;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 2;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 5;
        
    }
    public Vector2 SpiderLilyPosition => (Main.MouseWorld + new Vector2(0,40f))+ Main.rand.NextVector2CircularEdge(2600/2f, 2600/2f);//Player.Center - Vector2.UnitY * 1f * LilyScale * 140f;

    public static readonly SoundStyle HeatReleaseLoopStart = GennedAssets.Sounds.Avatar.UniversalAnnihilationCharge;
    public static readonly SoundStyle HeatReleaseLoop = GennedAssets.Sounds.Environment.DivineStairwayStep;
        //SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.SuctionLoop with { Volume = 0.3f, MaxInstances = 32 });
    
    public override void OnSpawn(IEntitySource source)
    {
        Projectile.ai[0] = 0;
        
      
    }
    public bool HasScreamed = false;

    private void UpdateLoopedSounds()
    {
        AmbientLoop.Update(Projectile.Center, sound =>
        {
            float idealPitch = LumUtils.InverseLerp(6f, 30f, Projectile.position.Distance(Projectile.oldPosition)) * 0.8f;
            sound.Volume = 0f;
            sound.Pitch = MathHelper.Lerp(sound.Pitch, idealPitch, 0.6f);
        });

    }

    public override void AI()
    {
        /// Creates the visual that indicates heat
        /// 
        WeaponBar.DisplayBar(Color.SlateBlue, Color.Lerp(Color.DeepSkyBlue, Color.Crimson, Utils.GetLerpValue(0.3f, 0.8f, Main.LocalPlayer.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeat, true)), Main.LocalPlayer.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeat, 120, 1, new Vector2(0, -40f));

        ManipulatePlayerVariables();
        
        Player player = Main.player[Projectile.owner];
        /*
        
        Vector2 toMouse = Main.MouseWorld - player.Center;
        
        //this code is straight ass.
        Owner.heldProj = Projectile.whoAmI;
        Projectile.rotation = toMouse.ToRotation();

        Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), Owner.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.Zero), 0.1f);// * Owner.HeldItem.shootSpeed;
        Projectile.Center = Owner.MountedCenter + Projectile.velocity.SafeNormalize(Projectile.velocity).RotatedBy(toMouse.ToRotation());// + new Vector2(0, Owner.gfxOffY); //+ Main.rand.NextVector2Circular(2, 2); //* Projecti
        Projectile.spriteDirection = Projectile.direction;
        ManipulatePlayerVariables();

        */
        Owner.heldProj = Projectile.whoAmI;
        Projectile.Center = Owner.MountedCenter + new Vector2(Owner.direction * 20f,13f);// + new Vector2(0, Owner.gfxOffY); //+ Main.rand.NextVector2Circular(2, 2); //* Projecti
        Projectile.rotation = MathHelper.ToRadians(-90);
        //TODO: make this shit less ass

        if (Heat > 0|| player.channel)
        {
            Projectile.timeLeft = 2;
        }

        if (Owner.controlUseItem)
        {
            IsDisipateHeat = false;
            AbsorbHeat();
            if (Heat != maxHeat)
            {
                IsAbsorbingHeat = true;
                
            }
            else
                IsAbsorbingHeat = false;
           
        }
        else if (!Owner.controlUseItem)
        {
            IsAbsorbingHeat = false;
            ReleaseHeat();
            if (Heat != minHeat)
            {
                IsDisipateHeat = true;
               
            }
            else
                IsDisipateHeat = false;
        }
        if (Player.HeldItem.type != ModContent.ItemType<LifeAndCessation>() || Player.CCed || Player.dead)
        {
            Projectile.Kill();
            return;
        }

        if (IsDisipateHeat)
        {
            AmbientLoop ??= LoopedSoundManager.CreateNew(HeatReleaseLoop, () => !Projectile.active);
            UpdateLoopedSounds();
        }

        //Owner.SetDummyItemTime(4);
        Projectile.timeLeft = 4;
        Time++;
        if (Heat > 0)
            if (!GeneralScreenEffectSystem.RadialBlur.Active)
                GeneralScreenEffectSystem.RadialBlur.Start(Owner.Center, Heat * 2f, 1);

        if (Heat == maxHeat && Owner.controlUseItem)
        {
            if(Time % Main.rand.NextFloat(0,5) == 0)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Genesis.CreakyAmbient with { Volume = 2f, PitchVariance = 1f });
            }
        }
    }

    public void ManipulatePlayerVariables()
    {
       //Owner.ChangeDir(Projectile.direction);
        Owner.heldProj = Projectile.whoAmI;

       // Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.Center.ToRotation() - MathHelper.PiOver2);
       // Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, Projectile.Center.ToRotation() - MathHelper.PiOver2);

    }

    //todo: make it update properly with the Player's velocity
    public void AbsorbHeat()
    {

        // Increase and clamp heat.
        Heat = MathHelper.Clamp(Heat + heatIncrement / 10, minHeat, maxHeat);

        // Ensure our dust array is initialized.
        if (heatDusts == null || heatDusts.Length == 0)
        {
            heatDusts = new Dust[HeatDustCount];
            CreateHeatDusts();
        }

        // Define a threshold distance: when dust is closer than this to the projectile center,
        // we consider it “absorbed” and create a new dust particle.
        float absorptionThreshold = 29f;
        Vector2 offset = new Vector2(Projectile.Center.X, Projectile.Center.Y - 10);
        // Loop through each dust particle.
        for (int i = 0; i < HeatDustCount; i++)
        {
            // If the dust is missing or inactive, create one in the cone.
            if (heatDusts[i] == null || !heatDusts[i].active)
            {
                heatDusts[i] = CreateConeHeatDust();
            }
            else
            {
                Dust dust = heatDusts[i];
                // Calculate the distance to the projectile center.
                float distance = Vector2.Distance(dust.position, offset);

                // If the dust is close enough, mark it inactive and create a new one.
                if (distance < absorptionThreshold)
                {
                    dust.active = false;
                    heatDusts[i] = CreateConeHeatDust();
                    continue; // Skip further processing for this dust.
                }

                // Otherwise, pull the dust toward the projectile.
                // Otherwise, pull the dust toward the projectile.
                Vector2 pullDirection = offset - dust.position;

                // Apply player movement compensation — this helps the dust "keep up"
                Vector2 movementCompensation = Owner.velocity * 0.9f; // Tune this multiplier

                float pullSpeed = 19f;
                dust.velocity = pullDirection.SafeNormalize(Vector2.Zero) * pullSpeed + movementCompensation;

            }

            // Optional: tweak visual properties for a smooth effect.
            heatDusts[i].scale = 1.2f;
            heatDusts[i].noGravity = true;
        }
    }

    /// <summary>
    /// Spawns all heat dust particles within the defined cone in front of the projectile.
    /// </summary>
    private void CreateHeatDusts()
    {
        for (int i = 0; i < HeatDustCount; i++)
        {
            heatDusts[i] = CreateConeHeatDust();
        }
    }

    /// <summary>
    /// Creates a single dust particle spawned within a cone in front of the projectile,
    /// respecting its current rotation.
    ///  TODO: make it move with the player
    /// </summary>
    private Dust CreateConeHeatDust()
    {
        // Define the half-angle of the cone. Adjust this to widen or narrow the spread.
        float halfConeAngle = MathHelper.Pi / 8f;
        Vector2 toMouse = Main.MouseWorld - Owner.Center;
        // Choose a random angle within the cone, centered around the projectile rotation.
        float randomAngle = toMouse.ToRotation() + Main.rand.NextFloat(-halfConeAngle, halfConeAngle);

        // Set minimum and maximum distance for the dust's spawn offset relative to the projectile.
        float minDistance = 200f;
        float maxDistance = 400f;


        float spawnDistance = Main.rand.NextFloat(minDistance, maxDistance);

        // Calculate the offset using the chosen angle.
        Vector2 offset = new Vector2(spawnDistance, 0).RotatedBy(randomAngle);


        // Spawn the dust at the calculated offset.
        Dust dust = Dust.NewDustPerfect(
            Projectile.Center + offset,
            DustID.Torch,     // Change to your desired dust type.
            Vector2.Zero,     // Initial velocity will be set in the update loop.
            100,              // Alpha value (transparency).
            Color.White,      // Color override.
            1.5f              // Scale.
        );
        dust.noGravity = true;
        return dust;
    }


    //remind m e to increase these, as its too powerful otherwise

    private float previousHeat = 0;
    private float newRot; //delete later
    private float significantIncreaseThreshold = 0.25f; // Define the heat increase threshold for resetting HasScreamed.
    private float minimumHeatThreshold = 0.65f; // Define the minimum heat to enable screaming.
    private float lilyStarActivationInterval = 0.15f; // Interval for activating ReleaseLilyStars.

 
    public void ReleaseHeat()
    {
        Vector2 toMouse = Main.MouseWorld - Owner.Center;
        // Choose a random angle within the cone, centered around the projectile rotation.
        
   
        if (Heat > 0) 
        {
            
            var modPlayer = Player.Calamity();
            


            //todo: make this code better, it makes me want to kms

            if (Player.Calamity().StealthStrikeAvailable()) //setting the stealth strike
            {
                if (Heat > minimumHeatThreshold)
                {

                }
                int stealth = Projectile.NewProjectile(Projectile.GetSource_FromThis(), 
                    Projectile.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<HeldLifeCessation_StealthStrike>(),
                    Projectile.damage,
                    0f, 
                    Owner.whoAmI);
                Main.NewText($"Stealth strike created: {stealth}");
                if (stealth.WithinBounds(Main.maxProjectiles))
                {
                    Main.projectile[stealth].Calamity().stealthStrike = true;
                    Main.projectile[stealth].usesLocalNPCImmunity = true;
                    Player.Calamity().ConsumeStealthByAttacking();
                }

            }
            Player.Calamity().ConsumeStealthByAttacking();
            if (Heat > minimumHeatThreshold)
            {
               

                // Check if heat has risen significantly since the last call
                if (Heat - previousHeat >= significantIncreaseThreshold)
                {

                    HasScreamed = false; // Reset if heat rose significantly
                    Scream();
                }
                else
                {
                    //SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.RocksRedirect with { PitchVariance = 0.4f, Volume = 1f});
                }
            }
            else
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.RocksRedirect with { PitchVariance = 0.4f, Volume = 1f });

            }


            // Heat = MathHelper.Clamp(Heat-(float)Math.Pow(heatIncrement,Heat-0.5f),minHeat,maxHeat);
            //Heat = MathHelper.Lerp(MathHelper.Clamp(Heat, minHeat, maxHeat), Heat - heatIncrement,0.4f);

            /*
             In thermodynamics, entropy (S) is defined as the amount of heat (q) absorbed or emitted isothermally 
            and reversibly divided by the absolute temperature (T). The formula for entropy change (ΔS) is given by:
            ΔS = q_rev / T
            Where:
            ΔS is the change in entropy
            q_rev is the reversible heat exchange
            T is the absolute temperature in Kelvin

            https://en.wikipedia.org/wiki/First_law_of_thermodynamics
             */

            // Highest possible temperature : 2 trillion kelvins
            // Lowest possible temperature : 0 kelvins
            //TODO: find a better damned calculation for heat loss

            /*
            heat = Q
            'work' = w
            u = internal energy
            that heat supplied to the system is positive, 
            but work done by the system is subtracted, a change in the internal energy, 
            ΔU, is written
            change in heat => P


            P = Q - w
            this results in a linear equation, unless work is somehow different- that it is:
            https://en.wikipedia.org/wiki/Work_(thermodynamics)
            now, we have to figure out how much work the rock is doing inorder to succ all the heat up
            https://en.wikipedia.org/wiki/Work_(thermodynamics)#Gravitational_work


            okay, lucille just reccomended i use laplacian mathematics
            https://en.wikipedia.org/wiki/Laplace_operator#Diffusion

            */

            float diffusionRate = 0.005f;
            float ambientHeat = 0.0f;

            Heat = MathHelper.Clamp( Heat - (diffusionRate * (Heat - heatIncrement)),minHeat,maxHeat);
            //Heat = 0;
            if(Heat < 0.1)
            {
                Heat = 0;
            }
          //  Heat = (float)Math.Pow(Heat / 10,heatIncrement);
            
            //Main.NewText($"Heat: {Heat}");
            // Adjust activation logic for ReleaseLilyStars to every 0.15 heat
            if (Heat > minimumHeatThreshold && Time % 5 == 0)//(Heat % lilyStarActivationInterval < heatIncrement && Heat > 0.4)
            {
                ReleaseLilyStars(Main.player[Projectile.owner]);
            }
        }
        else if (Heat <= 0)
        {
            Heat = minHeat;
            HasScreamed = false;
            
        }

        // Update previousHeat at the end
        previousHeat = Heat;

    }
    public float FadeOutInterpolant => InverseLerp(0f, 11f, Projectile.timeLeft);

    public void HeatFullSparkle()
    {
        Texture2D sparkle = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Extra/Sparkle").Value;
        Texture2D ChromaticSpires = GennedAssets.Textures.GreyscaleTextures.ChromaticSpires;

        float spireScale = MathHelper.Lerp(0.85f, 1.1f, Sin01(Main.GlobalTimeWrappedHourly * 17.5f + Projectile.identity)) * Projectile.scale * 0.46f;
        float spireOpacity = MathF.Pow(FadeOutInterpolant, 1.9f) * Projectile.Opacity;
        Vector2 offset = new Vector2(Projectile.Center.X, Projectile.Center.Y - 10);

        float rot = 3 + Main.GlobalTimeWrappedHourly;
        Vector2 drawPosition = offset - Main.screenPosition;
        Main.spriteBatch.Draw(ChromaticSpires, drawPosition, null, (Color.Crimson with { A = (byte)(10 * Heat) }) * spireOpacity, rot, ChromaticSpires.Size() * 0.5f, spireScale, 0, 0f);
        Main.spriteBatch.Draw(ChromaticSpires, drawPosition, null, (Color.Crimson with { A = (byte)(10 * Heat) }) * spireOpacity, rot + MathHelper.PiOver2, ChromaticSpires.Size() * 0.5f, spireScale, 0, 0f);
        Main.spriteBatch.Draw(ChromaticSpires, drawPosition, null, (Color.Crimson with { A = (byte)(10 * Heat) }) * spireOpacity, rot + MathHelper.PiOver4, ChromaticSpires.Size() * 0.5f, spireScale, 0, 0f);



        Vector2 sparklePos = Projectile.Center + new Vector2(6, 0).RotatedBy(Projectile.rotation);
        
        Color sparkleColor = new Color(255, 20, 0,100);//new GradientColor(SlimeUtils.GoozColors, 0.2f, 0.2f).ValueAt(Time + 10);
        

        Vector2 sparkleScaleX = new Vector2(4.5f, 4.33f);
        Vector2 sparkleScaleY = new Vector2(1.5f, 1.33f);
        //Main.EntitySpriteDraw(sparkle, sparklePos - Main.screenPosition, sparkle.Frame(), Color.Crimson * 0.3f, 0f, sparkle.Size() * 0.5f, sparkleScaleX, 0, 0);
        //Main.EntitySpriteDraw(sparkle, sparklePos - Main.screenPosition, sparkle.Frame(), Color.Black * 0.3f, MathHelper.PiOver2, sparkle.Size() * 0.5f, sparkleScaleY, 0, 0);
        
    }
    
    
    public void Scream()
    {
        Vector2 energySource =Projectile.Center;
        if (!HasScreamed)
        {
            float screenShakeIntensity = InverseLerp(0f, 100 * 0.58f, 0).Squared() * 14.5f;
            ScreenShakeSystem.SetUniversalRumble(screenShakeIntensity, MathHelper.TwoPi, null, 0.2f);

            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ExplosionTeleport);
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.LilyFireStart.WithVolumeScale(1).WithPitchOffset(1 - Heat));
            //Main.rand.NextFloat(-1,0)));
            ScreenShakeSystem.StartShake(28f, shakeStrengthDissipationIncrement: 0.4f);
            GeneralScreenEffectSystem.ChromaticAberration.Start(Player.Center, 3f, 90);
            GeneralScreenEffectSystem.HighContrast.Start(Player.Center, 3, 33);

            if (Main.netMode != NetmodeID.MultiplayerClient)
               NewProjectileBetter(Projectile.GetSource_FromThis(), energySource, Vector2.Zero, ModContent.ProjectileType<DarkWave>(), 0, 0f);
            HasScreamed = true;
        }
    }

    private void ReleaseLilyStars(Player player)
    {
        
        int starCount = 1;
        //TODO: make it better, dipshit
        for (int i = 0; i < starCount; i++)
        {
            Heat -= 0.005f;
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.DisgustingStarSummon with { Volume = 0.3f, MaxInstances = 32, PitchVariance = 0.5f} ) ;
            
            // Fire the projectile
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                SpiderLilyPosition,
                Vector2.Zero, // speed
                ModContent.ProjectileType<LillyStarProjectile>(),
                Projectile.damage,
                Player.HeldItem.knockBack,
                player.whoAmI
            );
        }



    }


    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (IsAbsorbingHeat)
        {
            target.AddBuff(ModContent.BuffType<HeatBurnBuff>(), 600, true);
            
            CombatText.NewText(target.targetRect, Color.AntiqueWhite,1,true,true);
        }
        else
            target.AddBuff(ModContent.BuffType<ColdBurnBuff>(), 600, false);
        if (IsAbsorbingHeat)
        {
            Heat += 0.005f;
        }

        base.OnHitNPC(target, hit, damageDone);
    }
    
    // todo: add thermal shock
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        //target.HitSound = GennedAssets.Sounds.Avatar.Phase2IntroNeckSnap;
        //target.stat
        //target.AddBuff(ModContent.BuffType<ThermalShock>, 600, true);
    }
   
    public override bool PreDraw(ref Color lightColor)
    {

        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;

        Rectangle frame = texture.Frame(1, 1, 0, 0);
        /*
        Vector2 sparklePos = Projectile.Center + new Vector2(6, 0).RotatedBy(Projectile.rotation);
        Texture2D sparkle = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Extra/Sparkle").Value;
        Color sparkleColor = new Color(255, 20, 0);//new GradientColor(SlimeUtils.GoozColors, 0.2f, 0.2f).ValueAt(Time + 10);
        sparkleColor.A = 0;

        Vector2 sparkleScaleX = new Vector2(1.5f, 1.33f) * Projectile.ai[2];
        Vector2 sparkleScaleY = new Vector2(1.5f, 1.33f) * Projectile.ai[2];
        Main.EntitySpriteDraw(sparkle, sparklePos - Main.screenPosition, sparkle.Frame(), Color.Black * 0.3f, 0f, sparkle.Size() * 0.5f, sparkleScaleX, 0, 0);
        Main.EntitySpriteDraw(sparkle, sparklePos - Main.screenPosition, sparkle.Frame(), Color.Black * 0.3f, MathHelper.PiOver2, sparkle.Size() * 0.5f, sparkleScaleY, 0, 0);

        Vector2[] positions = new Vector2[500];
        float[] rotations = new float[500];
        for (int i = 0; i < 500; i++)
        {
            rotations[i] = newRot.AngleLerp(Projectile.rotation, MathF.Sqrt(i / 500f)) + MathF.Sin(Time * 0.2f - i / 50f) * 0.1f * (1f - i / 500f) * Projectile.ai[2];
            positions[i] = sparklePos + new Vector2(Size * (i / 500f) * Projectile.ai[2], 0).RotatedBy(rotations[i]);


        }
        */



        float rotation = Projectile.rotation+MathHelper.PiOver2;
        SpriteEffects spriteEffects = Projectile.spriteDirection * Owner.gravDir < 0 ? SpriteEffects.FlipVertically : 0;
        Vector2 origin = new Vector2(frame.Width / 2, frame.Height * Owner.gravDir);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, rotation, origin, Projectile.scale, spriteEffects, 0);
        // this is making me annoyed just to look at it tbh

        float wind = AperiodicSin(Main.GlobalTimeWrappedHourly * 0.56f + Projectile.Center.X + Projectile.Center.Y) *
            //clamps rotation kinda
            0.033f
            + Main.windSpeedCurrent * 0.17f ;

        Texture2D LillyTexture = GennedAssets.Textures.SecondPhaseForm.SpiderLily;
        Rectangle Lillyframe = LillyTexture.Frame(1, 3, 0, (int)(Main.GlobalTimeWrappedHourly * 10.1f) % 3);

        Vector2 Lorigin = new Vector2(Lillyframe.Width / 2, Lillyframe.Height+54  * Owner.gravDir);
        //me sucking the joy from my life:
        float LillySquish = MathF.Cos(Main.GlobalTimeWrappedHourly * 10.5f + Projectile.Center.X + Projectile.Center.Y) * 1f;
        //im sure there will be no repercussions there
        //Vector2 LillyScale = new Vector2(1f - LillySquish, 1f + LillySquish);
        float LillyScale = 0.1f;
        // Main.EntitySpriteDraw(LillyTexture, Projectile.Center - Main.screenPosition, Lillyframe, lightColor, rotation, Lorigin, Projectile.scale*0.1f, spriteEffects, 0);


        Vector2 LillyPos = new Vector2(Projectile.Center.X, Projectile.Center.Y);

        
        if (Heat <= maxHeat && Heat > 0.4)
        {
            HeatFullSparkle();

        }
        
        Color glowmaskColor = new Color(2, 0, 156);
        Main.EntitySpriteDraw(LillyTexture, LillyPos - Main.screenPosition, Lillyframe, lightColor, wind, Lorigin, LillyScale, spriteEffects, 0f);
        /*
        ManagedShader psychedelicShader = ShaderManager.GetShader("HeavenlyArsenal.ColdShader");

        // Time and intensity settings
        psychedelicShader.TrySetParameter("globalTime", Main.GlobalTimeWrappedHourly); 
        psychedelicShader.TrySetParameter("opacity", 100f);
        psychedelicShader.TrySetParameter("intensityFactor", 1f); 
        psychedelicShader.TrySetParameter("psychedelicExponent", 3f); 

        // Color tweaking
        psychedelicShader.TrySetParameter("colorAccentuationFactor", 1.5f);
        psychedelicShader.TrySetParameter("colorToAccentuate", new Vector3(1f, 0.5f, 0f)); // Example: Orange-ish
        psychedelicShader.TrySetParameter("goldColor", new Color(1f, 0.85f, 0.3f, 1f).ToVector4());
        psychedelicShader.TrySetParameter("psychedelicColorTint", new Color(0.2f, 0.3f, 1f, 0.3f).ToVector4());

        // Set texture slots
        psychedelicShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoiseDetailed, 0, SamplerState.AnisotropicWrap); // baseTexture
        psychedelicShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap); // psychedelicTexture
        psychedelicShader.SetTexture(GennedAssets.Textures.Noise.BurnNoise, 2, SamplerState.AnisotropicWrap); // noiseTexture

        psychedelicShader.Apply();
        */
        return false;
        
    }


   
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        //Main.NewText($"Mouse rot: {Main.MouseWorld.ToRotation()},  Projecitle rotation: {Projectile.rotation}, Projectile velocity to rotation: {Projectile.velocity.ToRotation()}");
        Vector2 toMouse = Main.MouseWorld - Owner.Center;
        return targetHitbox.IntersectsConeFastInaccurate(Projectile.Center,
            //distance
            445,
            //angle to be pointed to
            toMouse.ToRotation(),
            //cone size
            MathHelper.Pi / 7f);
      
    }


    
    public override bool? CanDamage() => Owner.controlUseItem&&!IsDisipateHeat;
  
}
