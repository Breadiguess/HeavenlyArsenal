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
    public override void SetStaticDefaults()
    {
       
    }

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 2;
    }
    public Vector2 SpiderLilyPosition => Main.MouseWorld;//Player.Center - Vector2.UnitY * 1f * LilyScale * 140f;


    public static readonly SoundStyle HeatReleaseLoop = GennedAssets.Sounds.Avatar.SuctionLoop;
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

        if (Time % 3 == 0)
        {
            
        }
        WeaponBar.DisplayBar(Color.SlateBlue, Color.Lerp(Color.DeepSkyBlue, Color.Crimson, Utils.GetLerpValue(0.3f, 0.8f, Main.LocalPlayer.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeat, true)), Main.LocalPlayer.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeat, 120, 1);

        Owner.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeat = Heat;

        Player player = Main.player[Projectile.owner];
        //Main.NewText($"Heat:{Heat}",Color.AliceBlue);
        //Main.NewText($"Actual Heat: {Main.LocalPlayer.GetModPlayer<HeavenlyArsenalPlayer>().CessationHeat}",Color.Pink);
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


        Projectile.Center = Owner.MountedCenter + Projectile.velocity.SafeNormalize(Vector2.Zero) * 25 + new Vector2(0, Owner.gfxOffY) + Main.rand.NextVector2Circular(2, 2) * Projectile.ai[2];

        Owner.heldProj = Projectile.whoAmI;

        Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.Zero), Owner.DirectionTo(Main.MouseWorld).SafeNormalize(Vector2.Zero), 0.08f) * Owner.HeldItem.shootSpeed;
        
        
        
        Projectile.rotation = Projectile.velocity.ToRotation();


        Projectile.spriteDirection = Projectile.direction;
        Owner.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);
        Owner.SetDummyItemTime(4);

        Vector2 toMouse = Main.MouseWorld - player.Center;
        Projectile.rotation = toMouse.ToRotation();


        
        Projectile.ai[2] = MathF.Sqrt(Utils.GetLerpValue(0, 50, Time, true) * Utils.GetLerpValue(10, 30, Projectile.timeLeft, true));


        
        if (Heat > 0|| player.channel)
        {
            Projectile.timeLeft = 2;
        }

        if (Owner.controlUseItem)
        {
            AbsorbHeat();
        }
        else if (!Owner.controlUseItem)
        {
           
            ReleaseHeat();
        }
        

        Projectile.ai[0]++;

    }
    

    public void AbsorbHeat()
    {
        if (Heat < maxHeat)
        {
            Heat += heatIncrement;
        }
        else if (Heat > maxHeat)
        {
            Heat = maxHeat;
        }

        if (Heat %2 == 0 )
        {
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), // The source for the projectile
                Owner.Center,
                Projectile.velocity, // speed
                ModContent.ProjectileType<LifeCessationEnergy>(),
                Player.HeldItem.damage,
                Player.HeldItem.knockBack,
                Owner.whoAmI
            );
        }
    
        
    }
    private float previousHeat = 0;
    private const float significantIncreaseThreshold = 0.1f; // Define the heat increase threshold for resetting HasScreamed.
    private const float minimumHeatThreshold = 0.5f; // Define the minimum heat to enable screaming.
    private const float lilyStarActivationInterval = 0.15f; // Interval for activating ReleaseLilyStars.

    public void ReleaseHeat()
    {
        if (Heat > 0) 
        {
            IsDisipateHeat = true;
            if (Heat > minimumHeatThreshold)
            {
                // Check if heat has risen significantly since the last call
                if (Heat - previousHeat >= significantIncreaseThreshold)
                {
                    HasScreamed = false; // Reset if heat rose significantly
                    Scream();
                }
            }
           

            Heat -= heatIncrement;
            

            // Adjust activation logic for ReleaseLilyStars to every 0.15 heat
            if (Heat % lilyStarActivationInterval < heatIncrement && Heat > 0.4)
            {
                ReleaseLilyStars(Main.player[Projectile.owner]);
            }
        }
        else if (Heat < 0)
        {
            Heat = minHeat;
            HasScreamed = false;
            IsDisipateHeat = false;
        }

        // Update previousHeat at the end
        previousHeat = Heat;
    }



    public void Scream()
    {
        Vector2 energySource = Owner.Center + Vector2.UnitY * Projectile.scale * 76f;
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

        for (int i = 0; i < starCount; i++)
        {
            SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.DisgustingStarSummon with { Volume = 1.3f, MaxInstances = 32 } ) ;
            
            // Fire the projectile
            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                SpiderLilyPosition,
                Vector2.Zero, // speed
                ModContent.ProjectileType<LillyStarProjectile>(),
                Player.HeldItem.damage,
                Player.HeldItem.knockBack,
                player.whoAmI
            );
        }



    }





    public override bool PreDraw(ref Color lightColor)
    {

        Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;

        Rectangle frame = texture.Frame(1, 1, 0, Projectile.frame);




        float rotation = Projectile.rotation+MathHelper.PiOver2;
        SpriteEffects spriteEffects = Projectile.direction * Player.gravDir < 0 ? SpriteEffects.FlipVertically : 0;
        Vector2 origin = new Vector2(frame.Width / 2, frame.Height * Projectile.direction * Player.gravDir);

        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, rotation, origin, Projectile.scale, spriteEffects, 0);

        Texture2D LillyTexture = GennedAssets.Textures.SecondPhaseForm.SpiderLily;
        
        Rectangle Lillyframe = LillyTexture.Frame(1, 3, 0, Projectile.frame);
        Vector2 Lorigin = new Vector2(Lillyframe.Width/2, Lillyframe.Height*1.5f * Projectile.direction * Player.gravDir);


        Main.EntitySpriteDraw(LillyTexture, Projectile.Center - Main.screenPosition, Lillyframe, lightColor, rotation, Lorigin, Projectile.scale*0.1f, spriteEffects, 0);
        return false;
    }
}
