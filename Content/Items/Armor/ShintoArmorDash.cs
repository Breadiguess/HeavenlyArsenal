using CalamityMod;
using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Enums;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Rogue;
using HeavenlyArsenal.ArsenalPlayer;
using HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;
using HeavenlyArsenal.Content.Projectiles.Misc;
using Microsoft.Xna.Framework;
using NoxusBoss.Assets;
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

    public override float CalculateDashSpeed(Player player) => 25.4f;

    public override void OnDashEffects(Player player)
    {
        Time = 0;
        SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.HarshGlitch, player.Center, null);
        player.GetModPlayer<ShintoArmorPlayer>().IsDashing = true;
        
        Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<ShintoArmorDash_Hand>(), 40, 0, -1, 0, 0, 0);
    }

    public override void MidDashEffects(Player player, ref float dashSpeed, ref float dashSpeedDecelerationFactor, ref float runSpeedDecelerationFactor)
    {
        player.GetModPlayer<ShintoArmorPlayer>().IsDashing = true;
        Time++;
        if (Time % 2 == 0)
        {
            Vector2 trailPos = player.Center - (player.velocity * 2) + Main.rand.NextVector2Circular(10, 20);
            float trailScale = player.velocity.X * player.direction * 0.08f;
            Color trailColor = Main.rand.NextBool(3) ? Color.DarkRed : Color.Black;
            Particle Trail = new SparkParticle(trailPos, player.velocity * 0.2f, false, 35, trailScale, trailColor);
            GeneralParticleHandler.SpawnParticle(Trail);

            {
                int fireBrightness = Main.rand.Next(20);
                Color fireColor = new Color(fireBrightness, fireBrightness, fireBrightness);
                /*
                if (i % 6 == 0)
                    fireColor = new Color(174, 0, Main.rand.Next(16), 0);
                */
                Vector2 position = player.Center + Main.rand.NextVector2Circular(60f, 60f);
                AntishadowFireParticleSystemManager.CreateNew(player.whoAmI, false, position, Main.rand.NextVector2Circular(17f, 17f), Vector2.One * Main.rand.NextFloat(30f, 125f), fireColor);
            }
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
        dashSpeed = 14f;
    }
}
