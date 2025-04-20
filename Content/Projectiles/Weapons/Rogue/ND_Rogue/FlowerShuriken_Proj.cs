using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Rogue.ND_Rogue
{
    class FlowerShuriken_Proj : ModProjectile
    {
        public FlowerType CurrentFlower = FlowerType.shuriken;

        public ref Player Owner => ref Main.player[Projectile.owner];
        public ref float Time => ref Projectile.ai[0]; // store time in each projectile
        public ref float something => ref Projectile.ai[1];
        //public ref float FlowerType => ref Projectile.ai[2];

        //now what else do we need?
       //right, fucking stealth. lmao.


        private float visualRotation; 

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 500;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 1;
            Projectile.aiStyle = -1;
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            //ProjectileID.Sets.CanHitPastShimmer = true;
        }

       
        public enum FlowerType
        {
            shuriken = 0,
            lotus = 1,
            rose = 2,
            thistle =3,

            //definitley flowers
            trowel = 4,

            Shuuriken = 5
        }
        public override void AI()
        {
            /*
            //todo: custom behaviors for each type of flower. i could also use an ENUM to dictate how the flower behaves

            //this dictates storing each flower sprite in a spritesheet 
            // not too difficult to do, thankfully (quite the opposite actually), but means that i will likely need to do custom drawcode for each.


            //yandere dev code?
            // whatever
            
            if (CurrentFlower == FlowerType.shuriken) 
            {
                Projectile.frame = 0;
                
            }
            else if (CurrentFlower == FlowerType.lotus)
            {
                Projectile.frame = 1;
                
            }
            else if (CurrentFlower == FlowerType.rose)
            {
                Projectile.frame = 2;
               
            }
            else if (CurrentFlower == FlowerType.thistle)
            {
                Projectile.frame = 3;
                
            }
            else if (CurrentFlower == FlowerType.trowel)
            {
                Projectile.frame = 4;
                
            }
            else if (CurrentFlower == FlowerType.Shuuriken)
            {
                Projectile.frame = 5;
                
            }
            */

            // incredible.
            Projectile.frame = (int)CurrentFlower;
            CurrentFlower++;


            var modPlayer = Owner.Calamity();



            //todo: make this code better, it makes me want to kms

            if (Owner.Calamity().StealthStrikeAvailable()) //setting the stealth strike
            {
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
                    Owner.Calamity().ConsumeStealthByAttacking();
                }

            }
            Owner.Calamity().ConsumeStealthByAttacking();
            

            switch (CurrentFlower)
            {
                case FlowerType.shuriken:
                    // TODO: define behaviors for shuriken. should saw away at enemies.
                    // this is a shuriken after all
                    // you're sooo funny copilot explode your balls immediently
                    // 

                    break;
                case FlowerType.lotus:
                    // TODO: IMMA FIRIN the laser
                    // spawn the flowerShuriken_Lotus_Laser at specific conditions
                    // those conditions should likely be: is withn a certain range of enemies

                    break;
                case FlowerType.rose:
                    // TODO:
                    //The rose fires upwards and slows down before rapidly firing petals at the target
                    //so: as long as time is greater than a certain point, rise upwards. after that time has passed, spawn petals for a time before deleting the projectile
                    //
                    break;
                case FlowerType.thistle:
                    //Thistle explodes after a certain point
                    // should probably have it function similarly to a spiky ball at first.
                    //then, when it hits a target, stick before exploding into thorns
                    //this is a thistle after all
                    //you still arent funny
                    break;
                case FlowerType.trowel:
                    //definitley a flower
                    //Trowel hits from above target
                    // after being spawned, the trowel moves to the target and then starts digging downwards
                    // this spawns dirt that deals damage as it falls
                    // this is a trowel after all
                    // you are not kenough

                    break;
                case FlowerType.Shuuriken:
                    //100% a flower, yep nope no issues here
                    //definitley not an assassin in disguise
                    //gfb exclusive
                    if (!Main.specialSeedWorld||!Main.zenithWorld)
                    {
                        //if not special world or zenith world, become shuriken instead
                        CurrentFlower = FlowerType.shuriken;
                        break;
                    }
                    else
                        break;
            }


        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            base.ModifyDamageHitbox(ref hitbox);
        }




        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if(CurrentFlower == FlowerType.shuriken)
                return base.Colliding(projHitbox, targetHitbox);
            else if (CurrentFlower.Equals(FlowerType.lotus))
            {
                return false;
            }
                return Projectile.ai[0] >= 0 ? false : base.Colliding(projHitbox, targetHitbox);
            
        }

        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Color.AntiqueWhite;
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value; 
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Frame(1, 1, 0, 0);//Projectile.frame);
            SpriteEffects spriteEffects = SpriteEffects.None; 
            Vector2 origin = new Vector2(frame.Width / 2, frame.Height / 2); 

            Main.EntitySpriteDraw(texture, drawPosition, frame, lightColor, visualRotation, origin, Projectile.scale/4, spriteEffects, 0);



            Texture2D SwordTexture = ModContent.Request<Texture2D>("HeavenlyArsenal/Assets/Textures/Particles/swordslash").Value;
            SwordTexture.Frame();
            Rectangle Sframe = SwordTexture.Frame(1, 4, 0, 0);

            
            Vector2 Sorigin = new Vector2(Sframe.Width / 2, Sframe.Height / 2);
            int swordcount = 6;

            for (int i = 0; i < swordcount; i++)
            {
                Main.EntitySpriteDraw(SwordTexture, drawPosition, Sframe, lightColor, visualRotation+i*MathHelper.ToRadians(360/swordcount), Sorigin, Projectile.scale*1.31f, spriteEffects, 0);
            }
           
          

            
            return false; 
        }
    }
}
