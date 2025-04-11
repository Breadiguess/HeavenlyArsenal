using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Avatar.Projectiles;
using NoxusBoss.Core.Graphics.GeneralScreenEffects;
using Terraria.Audio;
using static Luminance.Common.Utilities.Utilities;
using HeavenlyArsenal.ArsenalPlayer;
using HeavenlyArsenal.Common.utils;
using System;
using Terraria.DataStructures;
using HeavenlyArsenal.Common.UI;
using System.Threading;
using Luminance.Core.Sounds;


using System.Runtime.InteropServices;


namespace HeavenlyArsenal.Content.Projectiles.Weapons.Rogue;

class HeldLifeCessationProjectile : ModProjectile
{

    public float LilyScale
    {
        get;
        set;
    }
    public const float minHeat = 0;
   
    public const float maxHeat = 1;

    public float heatIncrement = 0.005f;

   

    public ref Player Player => ref Main.player[Projectile.owner];
    public ref Player Owner => ref Main.player[Projectile.owner];

    public LoopedSoundInstance AmbientLoop
    {
        get;
        set;
    }

    public ref float Heat => ref Owner.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeat;

    public bool IsDisipateHeat
    {
        get;
        set;
    }

    public ref float Time => ref Projectile.ai[2];
    public ref float Size => ref Projectile.ai[1];
    public bool IsAbsorbingHeat
    {
        get;
        set;
    }

    private Dust[] heatDusts;
    private const int HeatDustCount = 20; // How many dust particles do we want?

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
    }
    public Vector2 SpiderLilyPosition => (Main.MouseWorld + new Vector2(0,40f))+ Main.rand.NextVector2CircularEdge(50, 50);//Player.Center - Vector2.UnitY * 1f * LilyScale * 140f;

    public static readonly SoundStyle HeatReleaseLoopStart = GennedAssets.Sounds.Avatar.UniversalAnnihilationCharge;
    public static readonly SoundStyle HeatReleaseLoop = GennedAssets.Sounds.Avatar.UniversalAnnihilationLoop;
        //SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.SuctionLoop with { Volume = 0.3f, MaxInstances = 32 });
    
    public override void OnSpawn(IEntitySource source)
    {
        Projectile.ai[0] = 0;
        
        Projectile.ai[1] = 0; // Whether the projectile is in the process of firing
        //Projectile.ai[3] = 0;//time
    }
    public bool HasScreamed = false;

    private void UpdateLoopedSounds()
    {
        AmbientLoop.Update(Projectile.Center, sound =>
        {
            float idealPitch = LumUtils.InverseLerp(6f, 30f, Projectile.position.Distance(Projectile.oldPosition)) * 0.8f;
            sound.Volume = 3f;
            sound.Pitch = MathHelper.Lerp(sound.Pitch, idealPitch, 0.6f);
        });

    }

    public override void AI()
    {
        if (Projectile.ai[2] % 100 == 0)
        {
            Projectile.frame++;
            if (Projectile.frame > 2)
            {
                Projectile.frame = 0;
            }
        }
        if (Time % 3 == 0)
        {
            
        }
        WeaponBar.DisplayBar(Color.SlateBlue, Color.Lerp(Color.DeepSkyBlue, Color.Crimson, Utils.GetLerpValue(0.3f, 0.8f, Main.LocalPlayer.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeat, true)), Main.LocalPlayer.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeat, 120, 1);

        Owner.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeat = Heat;

        Player player = Main.player[Projectile.owner];
        Vector2 toMouse = Main.MouseWorld - player.Center;

        if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
        {
            Projectile.Kill();
            return;
        }

        if (IsDisipateHeat)
        {
            AmbientLoop ??= LoopedSoundManager.CreateNew(HeatReleaseLoop, () => !Projectile.active);
            UpdateLoopedSounds();
        }

        Projectile.ai[2] = MathF.Sqrt(Utils.GetLerpValue(0, 50, Time, true) * Utils.GetLerpValue(10, 30, Projectile.timeLeft, true));
        Owner.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
        Owner.SetDummyItemTime(4);

        Owner.heldProj = Projectile.whoAmI;
        //Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), Owner.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.Zero), 0.08f) * Owner.HeldItem.shootSpeed;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), Owner.DirectionTo(Main.MouseWorld), 0.4f);
        Projectile.Center = Owner.MountedCenter + Projectile.velocity.SafeNormalize(Vector2.Zero); // * 25 + new Vector2(0, Owner.gfxOffY) + Main.rand.NextVector2Circular(2, 2); //* Projectile.ai[2];
        Projectile.rotation = toMouse.ToRotation();

        Main.NewText($"Velocity = {Projectile.velocity}", Color.AntiqueWhite);
        
       


       
        Owner.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
        Projectile.spriteDirection = Projectile.direction;
        Owner.SetDummyItemTime(4);

       
        


       



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
        

        Projectile.ai[0]++;
        Main.NewText($"IsAbsorbingHeat: {IsAbsorbingHeat}, IsDisipateHeat: {IsDisipateHeat}", Color.CadetBlue);


    }


    public void AbsorbHeat()
    {
        // Increase and clamp heat.
        Heat = MathHelper.Clamp(Heat + heatIncrement, minHeat, maxHeat);

        // Ensure our dust array is initialized.
        if (heatDusts == null || heatDusts.Length == 0)
        {
            heatDusts = new Dust[HeatDustCount];
            CreateHeatDusts();
        }

        // Define a threshold distance: when dust is closer than this to the projectile center,
        // we consider it “absorbed” and create a new dust particle.
        const float absorptionThreshold = 10f;

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
                float distance = Vector2.Distance(dust.position, Projectile.Center);

                // If the dust is close enough, mark it inactive and create a new one.
                if (distance < absorptionThreshold)
                {
                    dust.active = false;
                    heatDusts[i] = CreateConeHeatDust();
                    continue; // Skip further processing for this dust.
                }

                // Otherwise, pull the dust toward the projectile.
                Vector2 pullDirection = Projectile.Center - dust.position;
                float pullSpeed = 2f;
                dust.velocity = pullDirection.SafeNormalize(Vector2.Zero) * pullSpeed;
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
    /// </summary>
    private Dust CreateConeHeatDust()
    {
        // Define the half-angle of the cone. Adjust this to widen or narrow the spread.
        float halfConeAngle = MathHelper.Pi / 8f;

        // Choose a random angle within the cone, centered around the projectile rotation.
        float randomAngle = Projectile.rotation + Main.rand.NextFloat(-halfConeAngle, halfConeAngle);

        // Set minimum and maximum distance for the dust's spawn offset relative to the projectile.
        float minDistance = 60f;
        float maxDistance = 80f;
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


    private float previousHeat = 0;
    private float newRot;
    private const float significantIncreaseThreshold = 0.1f; // Define the heat increase threshold for resetting HasScreamed.
    private const float minimumHeatThreshold = 0.5f; // Define the minimum heat to enable screaming.
    private const float lilyStarActivationInterval = 0.15f; // Interval for activating ReleaseLilyStars.

 
    public void ReleaseHeat()
    {
       
        if (Heat > 0) 
        {
            
            
            if (Heat > minimumHeatThreshold)
            {
                // Check if heat has risen significantly since the last call
                if (Heat - previousHeat >= significantIncreaseThreshold)
                {
                    HasScreamed = false; // Reset if heat rose significantly
                    Scream();
                }
            }
           

            Heat = MathHelper.Clamp(Heat-heatIncrement,minHeat,maxHeat);
            

            // Adjust activation logic for ReleaseLilyStars to every 0.15 heat
            if (Heat % lilyStarActivationInterval < heatIncrement && Heat > 0.4)
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
        /*
        Texture2D ChromaticSpires = GennedAssets.Textures.GreyscaleTextures.ChromaticSpires;

        float spireScale = MathHelper.Lerp(0.85f, 1.1f, Sin01(Main.GlobalTimeWrappedHourly * 17.5f + Projectile.identity)) * Projectile.scale * 0.46f;
        float spireOpacity = MathF.Pow(FadeOutInterpolant, 1.9f) * Projectile.Opacity;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Main.spriteBatch.Draw(ChromaticSpires, drawPosition, null, (Color.Violet with { A = 0 }) * spireOpacity, Projectile.rotation + MathHelper.PiOver4, ChromaticSpires.Size() * 0.5f, spireScale, 0, 0f);
        */
        
        Vector2 sparklePos = Projectile.Center + new Vector2(6, 0).RotatedBy(Projectile.rotation);
        Texture2D sparkle = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Extra/Sparkle").Value;
        Color sparkleColor = new Color(255, 20, 0);//new GradientColor(SlimeUtils.GoozColors, 0.2f, 0.2f).ValueAt(Time + 10);
        sparkleColor.A = 0;

        Vector2 sparkleScaleX = new Vector2(1.5f, 1.33f) * Projectile.ai[2];
        Vector2 sparkleScaleY = new Vector2(1.5f, 1.33f) * Projectile.ai[2];
        Main.EntitySpriteDraw(sparkle, sparklePos - Main.screenPosition, sparkle.Frame(), Color.Black * 0.3f, 0f, sparkle.Size() * 0.5f, sparkleScaleX, 0, 0);
        Main.EntitySpriteDraw(sparkle, sparklePos - Main.screenPosition, sparkle.Frame(), Color.Black * 0.3f, MathHelper.PiOver2, sparkle.Size() * 0.5f, sparkleScaleY, 0, 0);
        
    }
    
    
    public void Scream()
    {
        Vector2 energySource =Projectile.Center;
        if (!HasScreamed)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ExplosionTeleport);
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.LilyFireStart.WithVolumeScale(1).WithPitchOffset(1 - Heat));
            //Main.rand.NextFloat(-1,0)));
            ScreenShakeSystem.StartShake(28f, shakeStrengthDissipationIncrement: 0.4f);
            GeneralScreenEffectSystem.ChromaticAberration.Start(Player.Center, 3f, 90);

            if (Main.netMode != NetmodeID.MultiplayerClient)
               NewProjectileBetter(Projectile.GetSource_FromThis(), energySource, Vector2.Zero, ModContent.ProjectileType<DarkWave>(), 0, 0f);
            HasScreamed = true;
        }
    }

    private void ReleaseLilyStars(Player player)
    {
        
        int starCount = 3;
        //TODO: make it better, dipshit
        for (int i = 0; i < starCount; i++)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.DisgustingStarSummon with { Volume = 1.3f, MaxInstances = 32 } ) ;
            
            // Fire the projectile
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                SpiderLilyPosition,
                Vector2.Zero, // speed
                ModContent.ProjectileType<LillyStarProjectile>(),
                Projectile.damage * 3,
                Player.HeldItem.knockBack,
                player.whoAmI
            );
        }



    }


    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (IsAbsorbingHeat)
        {
            Heat += 0.1f;

        }

        base.OnHitNPC(target, hit, damageDone);
    }


    public override bool PreDraw(ref Color lightColor)
    {

        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;

        Rectangle frame = texture.Frame(1, 1, 0, 0);

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


        float rotation = Projectile.rotation+MathHelper.PiOver2;
        SpriteEffects spriteEffects = Projectile.direction * Player.gravDir < 0 ? SpriteEffects.FlipVertically : 0;
        Vector2 origin = new Vector2(frame.Width / 2, frame.Height * Projectile.direction * Player.gravDir);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, rotation, origin, Projectile.scale, spriteEffects, 0);

        Texture2D LillyTexture = GennedAssets.Textures.SecondPhaseForm.SpiderLily;
        
        Rectangle Lillyframe = LillyTexture.Frame(1, 3, 0, Projectile.frame);

        
        Vector2 Lorigin = new Vector2(Lillyframe.Width/2, Lillyframe.Height*1.5f * Projectile.direction * Player.gravDir);


        Main.EntitySpriteDraw(LillyTexture, Projectile.Center - Main.screenPosition, Lillyframe, lightColor, rotation, Lorigin, Projectile.scale*0.1f, spriteEffects, 0);

        if (Heat == maxHeat)
        {
            HeatFullSparkle();

        }


        return false;
    }



    public override bool? Colliding(Rectangle projHitbox, Microsoft.Xna.Framework.Rectangle targetHitbox)
    {
        return targetHitbox.IntersectsConeFastInaccurate(Projectile.Center, 400, Projectile.rotation+MathHelper.PiOver2, MathHelper.Pi / 8f);
    }



    public override bool? CanDamage()
    {
        /*
        if (IsAbsorbingHeat && !IsDisipateHeat)
        {
            return true;
        }
        else if (IsDisipateHeat)
        {
            return false;
        }
        else
        {
            return false;
        }
    } 

        */
        return false;
    }
}
