using Luminance.Common.Utilities;
using Luminance.Core.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Player = Terraria.Player;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Ranged
{
    class PossibilitySeed: ModProjectile
    {
        public LoopedSoundInstance FireLaserLoop
        {
            get;
            set;
        }
        public static readonly SoundStyle LaserLoop = GennedAssets.Sounds.NamelessDeity.CosmicLaserLoop;
        public ref float Time => ref Projectile.ai[0];
        public ref float GrowthStage => ref Projectile.ai[1];
        public ref float Firing => ref Projectile.ai[2];

        public ref Player Player => ref Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.damage = -1;
        }
        public override void SetStaticDefaults()
        {
          
        }
        public override void OnSpawn(IEntitySource source)
        {
            Firing = 0;
            
        }
        public override void AI()
        {
            if (Time > 60 && GrowthStage < 4)
            {
                GrowthStage++;
                Time = 0;
                //now, why am i re-setting this? its because im lazy and dont want to make another thing to check.
                //bad programmer moment.
                if (Main.netMode != NetmodeID.Server)
                {
                    CombatText.NewText(Projectile.Hitbox, Color.Gray, $"Growthstage: {GrowthStage}");
                }
            }
           

            if (GrowthStage != 4)
            {
                Projectile.timeLeft = 400;
            }
            else if (GrowthStage == 4)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    
                    
                    
                }
                //yes, this is the code for 
                NPC? target = Projectile.FindTargetWithinRange(1200f);
                if (target is not null)
                    AttackTarget(target);
                
                

            }
            //hold it, space cowboy
            Projectile.velocity *= 0.95f;
            Time++;
        }

        public void AttackTarget(NPC target)
        {
            // FIRE THE LASER
            if (Firing == 0)
            {
                Vector2 directionToTarget = Projectile.SafeDirectionTo(target.Center);
                Projectile.rotation = directionToTarget.ToRotation();
                //CombatText.NewText(Projectile.Hitbox, Color.Gray, $"I'MMA FIRIN MAH LASER", true);
                SoundEngine.PlaySound(GennedAssets.Sounds.NamelessDeity.PortalLaserShoot);
                int laser = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center,
                    Projectile.rotation.ToRotationVector2(),
                    ModContent.ProjectileType<PossibilitySeed_Laser>(), 9039, 0f, Projectile.owner);

                // Make the laser rotate with the original projectile
                if (Main.projectile[laser] is Projectile modLaser)
                {
                    modLaser.rotation = Projectile.rotation;
                }

                Firing++;
            }
            else
            {
                
                Vector2 directionToTarget = Projectile.SafeDirectionTo(target.Center);
                Projectile.rotation = directionToTarget.ToRotation();
            }

        }


        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            //kill the projectile, because its fragile !!
            Projectile.Kill();
            SoundEngine.PlaySound(GennedAssets.Sounds.NPCHit.NamelessDeityHurt,null);
            damageDone = 0; //99% sure that this is incorrect but its 12:19 am so i dont care
            
        }
        
        
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D seed = TextureAssets.Projectile[Projectile.type].Value;
            Rectangle seedFrame = Utils.Frame(seed, 1, 4, 1, (int)GrowthStage); 

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            //Main.NewText($"DrawPos: {drawPos}, Projectile center: {Projectile.Center}");
            Main.spriteBatch.Draw(seed, drawPos, seedFrame, lightColor, Projectile.rotation, seedFrame.Size() / 2f, 1f, SpriteEffects.None, 0);

            return false;
        }

    }
}
