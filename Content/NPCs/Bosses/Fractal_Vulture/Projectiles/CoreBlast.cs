using Luminance.Assets;
using NoxusBoss.Assets;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles;

internal class CoreBlast : ModProjectile
{
    public int index;

    public int OwnerIndex;

    private readonly int beamLength = 1800;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void AI()
    {
        if (Main.npc[OwnerIndex] != null && Main.npc[OwnerIndex].active && Main.npc[OwnerIndex].type == ModContent.NPCType<OtherworldlyCore>())
        {
            if (Projectile.timeLeft < 18)
            {
                Projectile.velocity *= 0;
            }
            else
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
        }

        Projectile.Center = Main.npc[OwnerIndex].Center + OtherworldlyCore.findShootVels(index, 3, Main.npc[OwnerIndex]);
    }

    public override void SetDefaults()
    {
        Projectile.timeLeft = 30;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.Size = new Vector2(30, 30);
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (Projectile.timeLeft < 18)
        {
            return false;
        }

        if (projHitbox.Intersects(targetHitbox))
        {
            return true;
        }

        var _ = float.NaN;
        var beamEndPos = Projectile.Center + Projectile.velocity * 1000;

        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, beamEndPos, 22 * Projectile.scale, ref _);
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
            var color = Color.Lerp(Color.Blue, Color.White, chargeFactor * 0.6f);

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
            var color = Color.Lerp(Color.White, Color.Blue, fadeFactor * 0.4f);

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

        //Utils.DrawBorderString(Main.spriteBatch, Projectile.timeLeft.ToString(), Projectile.Center - Main.screenPosition, Color.White);
        return false;
    }
}