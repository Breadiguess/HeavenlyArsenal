using CalamityMod;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Particles;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.AvatarRogue
{
    class HeatThiefLance : ModProjectile
    {
        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0];
        public ref float FadeIn => ref Projectile.ai[1];
        public bool Stuck;
        public ref float HitX => ref Projectile.localAI[0];
        public ref float HitY => ref Projectile.localAI[1];



        public int TimeAt;
        public float stuckTime
        {
            get;
            set;
        } = 200f;
        public int TargetIndex
        {
            get;
            set;
        } = -1;

        public bool disapear
        {
            get;
            set;
        }

        public Vector2 HitOffset
        {
            get;
            private set;
        } 

        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetStaticDefaults() 
        { 
        
        
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = Projectile.width;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.aiStyle = - 1;
            Projectile.damage = 300;
            Projectile.timeLeft = 400;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; 
            Projectile.DamageType = ModContent.GetInstance<RogueDamageClass>();
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 1;
           
        }
        public override void OnSpawn(IEntitySource source)
        {
            disapear = false;
            FadeIn = 0;
           // Rift darkParticle = Rift.pool.RequestParticle();
           // darkParticle.Prepare(Projectile.Center - Projectile.velocity,Vector2.Zero, Color.AntiqueWhite, new Vector2(1,1), Projectile.velocity.ToRotation(), 1,1 ,60);


          //  ParticleEngine.Particles.Add(darkParticle);
        }
        public override void AI()
        {
            float launchTime = 100 / (1 + Owner.GetModPlayer<CessationPlayer>().CessationHeat);
            if(Time<= launchTime||FadeIn <= 1)
            {
                FadeIn = MathHelper.Lerp(0, 1, Time / launchTime);
                Projectile.rotation= Projectile.rotation.AngleLerp((Main.MouseWorld - Projectile.Center).ToRotation()+MathHelper.PiOver2, FadeIn);

                Projectile.scale = MathHelper.Lerp(0, 1, FadeIn);
            }
           

            if (Time < launchTime)
            {
                Projectile.velocity = Owner.velocity;
            }
            if (Time > launchTime && !Stuck)
            {
                Vector2 toMouse = Main.MouseWorld - Projectile.Center;

                
                Vector2 heatlance = 200*Vector2.Zero.SafeDirectionTo(toMouse);
                if (Time == launchTime+1)
                { 
                    Projectile.velocity = heatlance;
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(Owner.Center, Projectile.rotation.ToRotationVector2(), 10, 3, 10, -0.5f, null));
                    SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.SliceTelegraph with { PitchVariance = 0.4f});
                }
               
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                
            }
            if (Stuck)
            {
                Projectile.Center = HitOffset;
                Projectile.timeLeft = 2;
                //todo: use the hit npc's velocity, so that no matter how it moves, the nail is still stuck in it
                Projectile.velocity = Vector2.Zero;
                stuckTime--;
                //TODO: if the npc dies while stuck time is above zero, kill the projectile. else, disapear the projectile
                if (TargetIndex <= -1 && TargetIndex >= Main.npc.Length || !Main.npc[TargetIndex].active)
                {
                    if(stuckTime <= 0)
                    {
                    TimeAt = (int)Time;
                    Stuck = false;
                    disapear = true;
                    }
                    else
                    {
                        //Projectile.Kill();
                    }
                }
               
            }
            Time++;
            if (disapear)
            {
                Projectile.timeLeft = 400;
                
              
                if (Time - TimeAt >= 30 && Time-TimeAt < 31)
                {
                    //Projectile.velocity -= Projectile.rotation.ToRotationVector2();
                }
                Projectile.damage = -1;
                FadeIn = MathHelper.Lerp(0, 1, ((TimeAt/100)/(Time/100))/2);
                //Main.NewText($"fadein: {FadeIn}");
                if (FadeIn <= 0)
                {
                    //Projectile.Kill();
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            
            if(!Stuck)
            {
                HitX = target.position.X + target.width / 2;
                HitY = target.position.Y + target.height / 2;
                Projectile.position = HitOffset;


                TargetIndex = target.whoAmI;
                Stuck = true;
                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.StakeGraze);
                Projectile.localNPCHitCooldown = 40;
            }
            else
            {
                Owner.GetModPlayer<CessationPlayer>().CessationHeat++;
            }
               // Main.NewText($"HitNPC:{target.type}, ");
            base.OnHitNPC(target, hit, damageDone);
        }
        public override bool? CanCutTiles()
        {
            return true;
        }


        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Cast a short line at the angle of the projectile to prevent clipping through enemies
            float collisionPoint = 0f;
            Vector2 start = Projectile.Center;
            Vector2 end = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 400f; 
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, Projectile.width, ref collisionPoint))
            {
                return true;
            }
            return false;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            /*
            Utils.DrawBorderString(Main.spriteBatch, "| Stuck: " + Stuck.ToString() + " | Disapearing: " + disapear.ToString(), Projectile.Center - Vector2.UnitY * 220 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "| FadeIn:" + FadeIn.ToString(), Projectile.Center - Vector2.UnitY * 240 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "| StuckTime: " + stuckTime.ToString(), Projectile.Center - Vector2.UnitY * 260 - Main.screenPosition, Color.White);
            Utils.DrawBorderString(Main.spriteBatch, "| Time overall: " + Time.ToString() + " | timeAfterDiseapearing: " + (Time- TimeAt).ToString(), Projectile.Center - Vector2.UnitY * 200 - Main.screenPosition, Color.White);
            */
            Texture2D texture = GennedAssets.Textures.Projectiles.FallingMeleeWeapon;

            Vector2 origin = texture.Size() / 2;

            Vector2 scale = new Vector2(1, 1) * 0.15f;

            
            byte alpha = (byte)Math.Round(255 * FadeIn,0);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, lightColor*FadeIn, Projectile.rotation - MathHelper.PiOver2, origin, scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
