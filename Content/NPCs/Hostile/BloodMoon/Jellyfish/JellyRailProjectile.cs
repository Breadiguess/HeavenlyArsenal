using Luminance.Assets;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.Audio;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish;

internal class JellyRailProjectile : ModProjectile
{
    public int OwnerIndex;

    private readonly float beamLength = 10000f;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.hostile = true;
        Projectile.Size = new Vector2(10, 10);
        Projectile.penetrate = -1;
        Projectile.ArmorPenetration = 30;
        Projectile.timeLeft = 30;
        Projectile.tileCollide = false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (Projectile.timeLeft < 15)
        {
            return false;
        }

        // If the target is touching the beam's hitbox (which is a small rectangle vaguely overlapping the host Prism), that's good enough.
        if (projHitbox.Intersects(targetHitbox))
        {
            return true;
        }

        var _ = float.NaN;
        var beamEndPos = Projectile.Center + Projectile.velocity * 1000;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, beamEndPos, 22 * Projectile.scale, ref _);
    }

    public override void AI()
    {
        if (Projectile.timeLeft == 30)
        {
            SoundEngine.PlaySound
            (
                GennedAssets.Sounds.Mars.RailgunFire with
                {
                    PitchVariance = 1.4f
                }
            );

            foreach (var player in Main.ActivePlayers)
            {
                if (!player.active || player.dead)
                {
                    continue;
                }

                // Laser start and end positions
                var beamStart = Projectile.Center;
                var beamEnd = Projectile.Center + Projectile.velocity * 1000; // already computed in your logic

                // Get the player's center
                var playerPos = player.Center;

                var dist = DistanceFromPointToLine(playerPos, beamStart, beamEnd);

                var maxRange = 300f; // no shake beyond this
                var minRange = 100f; // full shake if closer than this

                if (dist < maxRange)
                {
                    var strength = 1f - MathHelper.Clamp((dist - minRange) / (maxRange - minRange), 0f, 1f);
                    strength = MathF.Pow(strength, 2f);
                    var shakeMagnitude = MathHelper.Lerp(0f, 30f, strength);

                    if (player.whoAmI == Main.myPlayer)
                    {
                        ScreenShakeSystem.StartShakeAtPoint
                        (
                            Projectile.Center,
                            shakeMagnitude,
                            shakeDirection: Projectile.velocity.SafeNormalize(Vector2.Zero) * 2,
                            shakeStrengthDissipationIncrement: 0.7f - strength * 0.01f
                        );
                    }
                }
            }
        }

        Projectile.Center = Main.npc[OwnerIndex].Center;
        Projectile.rotation = Projectile.velocity.ToRotation();
    }

    private float DistanceFromPointToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        var lineDir = lineEnd - lineStart;
        var lineLength = lineDir.Length();

        if (lineLength == 0)
        {
            return Vector2.Distance(point, lineStart);
        }

        lineDir /= lineLength; // normalize

        var projectedLength = Vector2.Dot(point - lineStart, lineDir);
        projectedLength = MathHelper.Clamp(projectedLength, 0, lineLength);

        var closest = lineStart + lineDir * projectedLength;

        return Vector2.Distance(point, closest);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = GennedAssets.Textures.GreyscaleTextures.BloomLine2;
        var origin = new Vector2(tex.Width / 2f, 0f);
        var start = Projectile.Center - Main.screenPosition;

        // Constants for effect timing
        const int chargeDuration = 15; // ticks before firing visual starts fading
        const int fadeDuration = 30; // total fadeout after main flash
        const int totalVisualDuration = chargeDuration + fadeDuration;

        // Time since spawn
        var t = totalVisualDuration - Projectile.timeLeft;

        var rot = Projectile.rotation - MathHelper.PiOver2;

        if (t < chargeDuration)
        {
            var chargeFactor = t / (float)chargeDuration;
            var thickness = MathHelper.Lerp(3f, 1f, chargeFactor);
            var color = Color.Lerp(Color.Red, Color.White, chargeFactor * 0.6f);

            color = color with
            {
                A = 0
            };

            var opacity = chargeFactor * 0.8f;

            Main.EntitySpriteDraw
            (
                tex,
                start,
                null,
                color * opacity,
                rot,
                origin,
                new Vector2(thickness, beamLength / tex.Height),
                SpriteEffects.None
            );
        }
        //fire in the hole or something
        else if (t == chargeDuration)
        {
            var thickness = 16f;

            var flashColor = Color.White with
            {
                A = 0
            };

            Main.EntitySpriteDraw
            (
                tex,
                start,
                null,
                Color.Red with
                {
                    A = 0
                },
                rot,
                origin,
                new Vector2(thickness * 1.2f, beamLength / tex.Height),
                SpriteEffects.None
            );

            Main.EntitySpriteDraw
            (
                tex,
                start,
                null,
                flashColor,
                rot,
                origin,
                new Vector2(thickness, beamLength / tex.Height),
                SpriteEffects.None
            );
        }
        //fade out
        else if (t > chargeDuration && t < totalVisualDuration)
        {
            float fadeTime = t - chargeDuration;
            var fadeFactor = 1f - fadeTime / fadeDuration;

            var thickness = MathHelper.Lerp(3f, 2f, 1 - fadeFactor);
            var length = beamLength * (1f + fadeTime / fadeDuration * 0.3f);
            var color = Color.Lerp(Color.White, Color.Crimson, fadeFactor * 0.4f);

            color = color with
            {
                A = 0
            };

            Main.EntitySpriteDraw
            (
                tex,
                start,
                null,
                color * fadeFactor,
                rot,
                origin,
                new Vector2(thickness, length / tex.Height),
                SpriteEffects.None
            );
        }

        return false;
    }
}