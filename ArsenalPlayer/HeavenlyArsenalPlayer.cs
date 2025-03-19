using System;
using System.Collections.Generic;
using CalamityMod;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using HeavenlyArsenal.Content.Items.Weapons.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using HeavenlyArsenal.Content.Items.Accessories;
using CalamityMod.Projectiles.Typeless;
using Terraria.Audio;
using HeavenlyArsenal.Projectiles.Misc;
using Terraria.Chat;







namespace HeavenlyArsenal.ArsenalPlayer
{
    public partial class HeavenlyArsenalPlayer : ModPlayer
    {
        internal bool ElectricVambrace;
        public int AvatarRifleCounter = 7;




        public bool HasReducedDashFirstFrame { get; private set; }


        public bool isVambraceDashing
        {
            get;
            set;
        }
        public bool hasAvatarRifle { 
            get; 
            private set; 
        }

        public override void PostUpdate()

        {
            if (ElectricVambrace)
            {
               
                if (Player.miscCounter % 3 == 2 && Player.dashDelay > 0) // Reduced dash cooldown by 33%
                    Player.dashDelay--;

                //Console.WriteLine(Player.dashDelay);
                
                if (Player.dashDelay == -1)// TODO: prevent working with special dashes, this was inconsitent with my old solution so I didn't keep it. not huge deal)
                {
                    Player.endurance += 0.1f;
                    if (!isVambraceDashing) // Dash isn't reduced, this is used to determine the first frame of dashing
                    {
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.4f, PitchVariance = 0.4f }, Player.Center);
                        
                        int damage = Player.ApplyArmorAccDamageBonusesTo(Player.GetBestClassDamage().ApplyTo(750));
                        isVambraceDashing = true;
                        //Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center + Player.velocity * 1.5f, Vector2.Zero, ModContent.ProjectileType<VambraceDash>(), damage, 20f, Player.whoAmI);

                        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center + Player.velocity * 1.5f, Vector2.Zero, ModContent.ProjectileType<VambraceDash>(), damage, 50f, Player.whoAmI);
                        //HasReducedDashFirstFrame = true;
                        Console.WriteLine("ElectricVambrace spawned a projectile (VambraceDash)!");

                    }
                   // float numberOfDusts = 10f;
                   // float rotFactor = 180f / numberOfDusts;
                    
                    //float sparkscale = MathF.Min(Player.velocity.X * Player.direction * 0.08f, 1.2f);
                    //Vector2 SparkVelocity1 = Player.velocity.RotatedBy(Player.direction * -3, default) * 0.1f - Player.velocity / 2f;
                    //SparkParticle spark = new SparkParticle(Player.Center + Player.velocity.RotatedBy(2f * Player.direction) * 1.5f, SparkVelocity1, false, Main.rand.Next(11, 13), sparkscale, Main.rand.NextBool() ? Color.DarkOrange : Color.OrangeRed);
                    //GeneralParticleHandler.SpawnParticle(spark);
                    //Vector2 SparkVelocity2 = Player.velocity.RotatedBy(Player.direction * 3, default) * 0.1f - Player.velocity / 2f;
                    //SparkParticle spark2 = new SparkParticle(Player.Center + Player.velocity.RotatedBy(-2f * Player.direction) * 1.5f, SparkVelocity2, false, Main.rand.Next(11, 13), sparkscale, Main.rand.NextBool() ? Color.DarkOrange : Color.OrangeRed);
                    //GeneralParticleHandler.SpawnParticle(spark2);

                    //if (Player.miscCounter % 6 == 0 && Player.velocity != Vector2.Zero)
                   // {
                     //   int damage = Player.ApplyArmorAccDamageBonusesTo(Player.GetBestClassDamage().ApplyTo(170));
                     //   Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center + Player.velocity * 1.5f, Vector2.Zero, ModContent.ProjectileType<PauldronDash>(), damage, 10f, Player.whoAmI);
                   // }






                    else
                        isVambraceDashing = false;
                }
            }   

            if (hasAvatarRifle)
            {

            }
        }


        public override void ResetEffects()
        {
            ElectricVambrace = false;
            hasAvatarRifle = false;
        }
    }
}