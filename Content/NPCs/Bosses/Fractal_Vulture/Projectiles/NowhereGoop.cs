using HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Solyn;
using HeavenlyArsenal.Content.Particles.Metaballs;
using Luminance.Common.Utilities;
using NoxusBoss.Assets;
using NoxusBoss.Content.NPCs.Bosses.Draedon.Projectiles.SolynProjectiles;
using NoxusBoss.Core.Utilities;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles;

public class NowhereGoop : ModProjectile
{
    public Vector2 biggestVel;

    public int Time
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override void SetDefaults()
    {
        Projectile.aiStyle = -1;
        Projectile.timeLeft = 280;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Generic;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.Size = new Vector2(60, 60);
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation();

        if (Time < 60)
        {
            biggestVel = Projectile.oldVelocity;
        }

        if (Time > 60)
        {
            if (Projectile.tileCollide)
            {
                Projectile.tileCollide = true;
            }

            Projectile.velocity.X *= 0.94f;

            if (Projectile.velocity.Y < biggestVel.Length())
            {
                Projectile.velocity.Y += 1;
            }

            Projectile.Size = new Vector2(60, 60) * MathHelper.Clamp(LumUtils.InverseLerp(200, 60, Time), 0.2f, 1);
            Projectile.damage = (int)(Projectile.originalDamage * LumUtils.InverseLerp(200, 60, Time));
        }

        HandleCollidingWithShield();

        Time++;
    }

    private void HandleCollidingWithShield()
    {
        var forcefieldID = ModContent.ProjectileType<DirectionalSolynForcefield3>();

        if (DirectionalSolynForcefield3.Myself != null)
        {
            if (DirectionalSolynForcefield3.Myself.type == forcefieldID && DirectionalSolynForcefield3.Myself.Opacity >= 0.6f && Projectile.Distance(DirectionalSolynForcefield3.Myself.Center) < 100)
            {
                var hitboxCollision = DirectionalSolynForcefield3.Myself.Colliding(DirectionalSolynForcefield3.Myself.Hitbox, Projectile.Hitbox);
                //bool reasonableIncomingAngle = DirectionalSolynForcefield3.Myself.velocity.AngleBetween(-Projectile.velocity) <= 0.23f;

                var spinMovingAverage = DirectionalSolynForcefield3.Myself.As<DirectionalSolynForcefield>().SpinSpeedMovingAverage;
                var tryingToCheese = spinMovingAverage >= MathHelper.ToRadians(13f);

                if (hitboxCollision && !tryingToCheese)
                {
                    SoundEngine.PlaySound
                        (
                            GennedAssets.Sounds.Solyn.ForcefieldHit with
                            {
                                MaxInstances = 0
                            },
                            Projectile.Center
                        )
                        .WithVolumeBoost(1.45f);

                    Projectile.velocity = Projectile.velocity * -1.2f + Main.player[DirectionalSolynForcefield3.Myself.owner].velocity;
                    Projectile.hostile = false;
                    Projectile.friendly = true;
                    Projectile.penetrate = 1;
                    Projectile.damage *= 7;
                    Projectile.damage = Math.Clamp(Projectile.damage, 0, 3_000);
                }
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        for (var i = 0; i < 12; i++)
        {
            var Vel = oldVelocity.RotatedByRandom(MathHelper.PiOver4) * 0.3f;
            Dust.NewDustPerfect(Projectile.Center, DustID.Cloud, Vel);
        }

        return base.OnTileCollide(oldVelocity);
    }

    public override void PostAI()
    {
        //SludgeBall.CreateParticle(Projectile.Center, Projectile.velocity, 14f);
        for (var i = 0; i < 5; i++)
        {
            var size = Main.rand.NextFloat(13f, 56);

            if (Time > 60)
            {
                size = Main.rand.NextFloat(13f, 20 + 30 * LumUtils.InverseLerpBump(0, 60, 60, 120, Time));
            }

            SludgeBall.CreateParticle
            (
                Projectile.Center + Main.rand.NextVector2Circular(13f, 13f) + Projectile.velocity,
                Projectile.velocity.RotatedBy(MathHelper.Pi).RotatedByRandom(MathHelper.ToRadians(9)) * Main.rand.NextFloat(0.3f),
                size
            );
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        //Utils.DrawBorderString(Main.spriteBatch, Projectile.damage.ToString(), Projectile.Center - Main.screenPosition, Color.AntiqueWhite, scale: 4);

        //Texture2D circle = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Particles/Metaballs/BasicCircle").Value;

        //Main.EntitySpriteDraw(circle, Projectile.Center - Main.screenPosition, null, Color.Black, Projectile.rotation, circle.Size() / 2, 0.4f, 0);
        return false;
    }
}