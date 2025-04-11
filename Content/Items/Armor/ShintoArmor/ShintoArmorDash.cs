using CalamityMod;
using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Enums;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Rogue;
using HeavenlyArsenal.ArsenalPlayer;
using HeavenlyArsenal.Common.utils;
using HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;
using HeavenlyArsenal.Content.Particles;
using HeavenlyArsenal.Content.Projectiles.Misc;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
using System.Security.Principal;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Armor;

public class ShintoArmorDash : PlayerDashEffect
{
    public static new string ID => "ShintoArmorDash";

    public override DashCollisionType CollisionType => DashCollisionType.NoCollision;

    public override bool IsOmnidirectional => false;
    public int Time = 0;
    public bool AngleSwap = true;

    public override float CalculateDashSpeed(Player player) => 30.4f;

    public override void OnDashEffects(Player player)
    {
        Time = 0;
        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.HarshGlitch with { PitchVariance = 0.45f, MaxInstances = 0, }, player.Center, null);
        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.ArmSwing with { PitchVariance = 0.25f, MaxInstances = 0, }, player.Center, null);
        player.GetModPlayer<ShintoArmorPlayer>().IsDashing = true;
        player.SetImmuneTimeForAllTypes(20); 
        for (int i = 0; i < Main.rand.Next(1, 5); i++)
        {
            Vector2 lightningPos = player.Center + Main.rand.NextVector2Circular(24, 24);

            HeatLightning particle = HeatLightning.pool.RequestParticle();
            particle.Prepare(lightningPos, player.velocity + Main.rand.NextVector2Circular(10, 10), Main.rand.NextFloat(-2f, 2f), 10 + i * 3, Main.rand.NextFloat(0.5f, 1f));
            ParticleEngine.Particles.Add(particle);
        }

        //Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<ShintoArmorDash_Hand>(), 40, 0, -1, 0, 0, 0);  
    }

    public override void MidDashEffects(Player player, ref float dashSpeed, ref float dashSpeedDecelerationFactor, ref float runSpeedDecelerationFactor)
    {
        player.GetModPlayer<ShintoArmorPlayer>().IsDashing = true;
        Time++;
        //if (Time % 1 == 0)
        //AntishadowCrack darkParticle = AntishadowCrack.pool.RequestParticle();
        //darkParticle.Prepare(player.Center, player.velocity * 0.2f, Main.rand.NextFloat()*10f, 35, Color.Red, Color.DarkRed,1f);


       // ParticleEngine.Particles.Add(darkParticle);
        for (int i = 0; i < 7; i++)
        {

            Vector2 trailPos = player.Center - (player.velocity * 2);
            float trailScale = player.velocity.X * player.direction * 0.04f;
            Color trailColor = Color.DarkRed;
            Particle Trail = new SparkParticle(trailPos, player.velocity * 0.2f, false, 35, trailScale, trailColor);
            GeneralParticleHandler.SpawnParticle(Trail);
        }

            for (int i = 0; i < 16; i++)
            {


            //Particle Trail2 = new SparkParticle(trailPos, player.velocity * 0.2f, false, 35, trailScale*0.98f, Color.DarkRed);
            //GeneralParticleHandler.SpawnParticle(Trail2);
            
                int fireBrightness = Main.rand.Next(40);
                Color fireColor = new Color(fireBrightness, fireBrightness, fireBrightness);
                
               if(Main.rand.NextBool(3)&& player.velocity.X > 20*player.direction)
                    fireColor = new Color(220, 20, Main.rand.Next(16), 255);

                
                Vector2 position = player.Center + Main.rand.NextVector2Circular(30f, 30f);
                AntishadowFireParticleSystemManager.CreateNew(player.whoAmI, false, position, Main.rand.NextVector2Circular(30f, player.velocity.X * 0.76f), Vector2.One * Main.rand.NextFloat(30f, 50f), fireColor);
            
            }
        /*
        // Periodically release scythes.
        player.Calamity().statisTimer++;
        if (Main.myPlayer == player.whoAmI && player.Calamity().statisTimer % 5 == 0)
        {
            int scytheDamage = (int)player.GetBestClassDamage().ApplyTo(250);
            scytheDamage = player.ApplyArmorAccDamageBonusesTo(scytheDamage);

            int scythe = Projectile.NewProjectile(player.GetSource_FromAI(), player.Center, player.velocity.RotatedBy(player.direction * (AngleSwap ? 30 : -30), default) * 0.1f - player.velocity / 2f, ModContent.ProjectileType<CosmicScythe>(), scytheDamage, 5f, player.whoAmI); ;
            if (scythe.WithinBounds(Main.maxProjectiles))
            {
                Main.projectile[scythe].DamageType = DamageClass.Generic;
                Main.projectile[scythe].usesIDStaticNPCImmunity = true;
                Main.projectile[scythe].idStaticNPCHitCooldown = 10;
            }

            AngleSwap = !AngleSwap;

        }
        */
        // Dash at a faster speed than the default value.
        dashSpeed = 19f;
    }
}
