using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Common.Players;
using HeavenlyArsenal.Content.Particles;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using NoxusBoss.Content.Particles.Metaballs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Projectiles.Weapons.Melee.AvatarSpear;

public class AvatarSpearRupture : ModProjectile
{
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 30;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }

    public override void SetDefaults()
    {
        Projectile.width = 64;
        Projectile.height = 64;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2000;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.hide = true;
        Projectile.extraUpdates = 2;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
        Projectile.noEnchantmentVisuals = true;
    }

    public Player Player => Main.player[Projectile.owner];

    public ref float Time => ref Projectile.ai[0];

    public int Target => (int)(Projectile.ai[1] - 1);

    public const int FlickerTime = 100;
    public const int ExplosionTime = 100;

    public override void AI()
    {
        Projectile.velocity = Vector2.Zero;
        if (Time < FlickerTime)
        {
            if (Target > -1 && !Player.dead)
            {
                float targetProgress = Utils.GetLerpValue(0, FlickerTime, Time, true);
                NPC target = Main.npc[Target];
                if (!target.active && target.life < 3)
                {
                    Time = FlickerTime;
                    return;
                }

                Projectile.Center = Vector2.Lerp(Player.Center, target.Center, targetProgress * 0.9f);

                if (Time % 18 == 0)
                    SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.StakeImpale with { Pitch = targetProgress, Volume = 0.5f, MaxInstances = 0 }, Projectile.Center);
            }
            else
                Time = FlickerTime;
        }
        else
        {
            float explodeProgress = Utils.GetLerpValue(0, ExplosionTime, Time - FlickerTime, true);
            Projectile.Resize(500, 500);

            if (Target > -1 && !Player.dead)
            {
                if (Main.myPlayer == Projectile.owner)
                    Main.SetCameraLerp(0.4f, 20);

                Player.GetModPlayer<HidePlayer>().ShouldHide = true;
                Player.SetImmuneTimeForAllTypes(60);
            }

            if (Time == FlickerTime + 1)
            {
                if (!Collision.SolidCollision(Projectile.Center - new Vector2(20) + Projectile.velocity.SafeNormalize(Vector2.Zero) * 20, 40, 40))
                    Player.Center = Main.npc[Target].Center;

                for (int i = 0; i < 100; i++)
                {
                    BloodMetaball metaball = ModContent.GetInstance<BloodMetaball>();
                    metaball.CreateParticle(Projectile.Center, Main.rand.NextVector2Circular(20, 20), Main.rand.NextFloat(20f, 70f), Main.rand.NextFloat(1f, 15f));
                }

                SoundEngine.PlaySound(GennedAssets.Sounds.Avatar.BloodFountainErupt with { Volume = 0.4f, MaxInstances = 0 }, Projectile.Center);
                SoundEngine.PlaySound(GennedAssets.Sounds.Common.LargeBloodSpill with { Pitch = -0.5f, MaxInstances = 0 }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { MaxInstances = 0 }, Projectile.Center);
            }

            BloodMetaball smallBlood = ModContent.GetInstance<BloodMetaball>();
            smallBlood.CreateParticle(Projectile.Center, Main.rand.NextVector2Circular(30, 30), Main.rand.NextFloat(10f, 40f), Main.rand.NextFloat(1f, 12f));

            if (Time % 5 == 0)
            {
                Vector2 lightningPos = Projectile.Center + Main.rand.NextVector2Circular(20, 20);
                HeatLightning particle = HeatLightning.pool.RequestParticle();
                particle.Prepare(lightningPos, Main.rand.NextVector2Circular(40, 40), Main.rand.NextFloat(-1f, 1f), Main.rand.Next(5, 10), Main.rand.NextFloat(1f, 2f) + explodeProgress * 0.5f);
                ParticleEngine.Particles.Add(particle);
            }

            if (Time % 3 == 0 && Time < FlickerTime + ExplosionTime / 4)
            {
                BleedingBurstParticle bombParticle = BleedingBurstParticle.pool.RequestParticle();
                Color randomColor = Color.Lerp(Color.Black, Color.DarkRed, Main.rand.NextFloat());
                bombParticle.Prepare(Projectile.Center + Main.rand.NextVector2Circular(35, 35), Main.rand.NextVector2Circular(35, 35), Main.rand.NextFloat(-1f, 1f), randomColor, Main.rand.NextFloat(0.5f, 1.5f));
                ParticleEngine.ShaderParticles.Add(bombParticle);
            }

            ScreenShakeSystem.StartShake(10f * (1f - explodeProgress), shakeStrengthDissipationIncrement: 0.7f);

            if (Time > FlickerTime + ExplosionTime)
                Projectile.Kill();
        }

        Player.SetDummyItemTime(10);

        Time++;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (Time > FlickerTime)
        {
            Vector2 targetCenter = targetHitbox.Center();
            Vector2 circle = Projectile.Center + Projectile.DirectionTo(targetCenter).SafeNormalize(Vector2.Zero) * Math.Min(Projectile.Distance(targetCenter), Projectile.width);
            Dust.QuickDust(circle, Color.Cyan);
            return targetHitbox.Contains(circle.ToPoint());
        }

        return false;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overPlayers.Add(index);

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[Type].Value;
        Texture2D glow = AssetDirectory.Textures.BigGlowball.Value;

        if (Time < FlickerTime)
        {
            float flickerScale = Projectile.scale * (0.2f + MathF.Sin(Time / 4f) * 0.1f) * MathF.Sin(Time / FlickerTime * MathHelper.Pi);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, glow.Frame(), Color.DarkRed with { A = 0 }, Projectile.rotation, glow.Size() * 0.5f, flickerScale * 3f, 0, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, glow.Frame(), Color.Red with { A = 50 } * 0.5f, Projectile.rotation, glow.Size() * 0.5f, flickerScale * 2f, 0, 0);
            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, glow.Frame(), Color.Coral with { A = 10 }, Projectile.rotation, glow.Size() * 0.5f, flickerScale, 0, 0);
        }

        return false;
    }
}
