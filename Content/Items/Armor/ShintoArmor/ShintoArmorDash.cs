using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Enums;
using CalamityMod.Particles;
using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;
using HeavenlyArsenal.Content.Particles;
using NoxusBoss.Assets;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.Items.Armor;

public class ShintoArmorDash : PlayerDashEffect
{
    public int Time;

    public bool AngleSwap = true;

    public override string DashID => "ShintoArmorDash";
    public new static string ID => "ShintoArmorDash";

    public override DashCollisionType CollisionType => DashCollisionType.NoCollision;

    public override bool IsOmnidirectional => false;

    public override float CalculateDashSpeed(Player player)
    {
        return 30.4f;
    }

    public override void OnDashEffects(Player player)
    {
        Time = 0;

        SoundEngine.PlaySound
        (
            GennedAssets.Sounds.Avatar.HarshGlitch with
            {
                PitchVariance = 0.45f,
                MaxInstances = 0
            },
            player.Center
        );

        SoundEngine.PlaySound
        (
            GennedAssets.Sounds.Avatar.ArmSwing with
            {
                PitchVariance = 0.25f,
                MaxInstances = 0
            },
            player.Center
        );

        player.SetImmuneTimeForAllTypes(20);

        for (var i = 0; i < Main.rand.Next(1, 5); i++)
        {
            var lightningPos = player.Center + Main.rand.NextVector2Circular(24, 24);

            var particle = HeatLightning.pool.RequestParticle();
            particle.Prepare(lightningPos, player.velocity + Main.rand.NextVector2Circular(10, 10), Main.rand.NextFloat(-2f, 2f), 10 + i * 3, Main.rand.NextFloat(0.5f, 1f));
            ParticleEngine.Particles.Add(particle);
        }
        /*
       DashBlob Blob = ModContent.GetInstance<DashBlob>();
        for (int i = 0; i < 12; i++)

        {
            float randomoffset = Main.rand.Next(-40, 40);
            Vector2 bloodSpawnPosition = player.Center + new Vector2(Main.rand.Next(-40, 40), Main.rand.Next(-70, 70));

            //var dust = Dust.NewDustPerfect(bloodSpawnPosition, DustID.AncientLight, Vector2.Zero, default, Color.Red);
            //dust.noGravity = true;
            Blob.player = player;

            Blob.CreateParticle(bloodSpawnPosition, Vector2.Zero, 0, 0);
        }*/
        //Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<ShintoArmorDash_Hand>(), 40, 0, -1, 0, 0, 0);  
    }

    public override void MidDashEffects(Player player, ref float dashSpeed, ref float dashSpeedDecelerationFactor, ref float runSpeedDecelerationFactor)
    {
        for (var i = 0; i < 7; i++)
        {
            var trailPos = player.Center - player.velocity * 2;
            var trailScale = player.velocity.X * player.direction * 0.04f;
            var trailColor = Color.DarkRed;
            Particle Trail = new SparkParticle(trailPos, player.velocity * 0.2f, false, 35, trailScale, trailColor);
            GeneralParticleHandler.SpawnParticle(Trail);
        }

        for (var i = 0; i < 16; i++)
        {
            var trailPos = player.Center - player.velocity * 2;
            var trailScale = player.velocity.X * player.direction * 0.04f;
            var fireBrightness = Main.rand.Next(40);
            var fireColor = new Color(fireBrightness, fireBrightness, fireBrightness);

            if (Main.rand.NextBool(3) && player.velocity.X > 20 * player.direction)
            {
                fireColor = new Color(220, 20, Main.rand.Next(16), 255);
            }

            var position = player.Center + Main.rand.NextVector2Circular(30f, 30f);

            AntishadowFireParticleSystemManager.CreateNew
                (player.whoAmI, false, position, Main.rand.NextVector2Circular(30f, player.velocity.X * 0.76f), Vector2.One * Main.rand.NextFloat(30f, 50f), fireColor);
        }

        Time++;
        dashSpeed = 19f;
    }
}