using Luminance.Assets;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Hostile.BloodMoon.Jellyfish
{
    internal class JellyRailProjectile : ModProjectile
    {
        public int OwnerIndex;
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.Size = new Vector2(10,10);
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

            float _ = float.NaN;
            Vector2 beamEndPos = Projectile.Center + Projectile.velocity * 1000;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, beamEndPos, 22 * Projectile.scale, ref _);
        }
        public override void AI()
        {
            if(Projectile.timeLeft == 30)
            {
                SoundEngine.PlaySound(GennedAssets.Sounds.Mars.RailgunFire with { PitchVariance = 1.4f });
                foreach (Player player in Main.ActivePlayers)
                {
                    if (!player.active || player.dead)
                        continue;

                    // Laser start and end positions
                    Vector2 beamStart = Projectile.Center;
                    Vector2 beamEnd = Projectile.Center + Projectile.velocity * 1000;// already computed in your logic

                    // Get the player's center
                    Vector2 playerPos = player.Center;

                    float dist = DistanceFromPointToLine(playerPos, beamStart, beamEnd);

                    float maxRange = 300f; // no shake beyond this
                    float minRange = 100f; // full shake if closer than this

                    if (dist < maxRange)
                    {
                        float strength = 1f - MathHelper.Clamp((dist - minRange) / (maxRange - minRange), 0f, 1f);
                        strength = MathF.Pow(strength, 2f); 
                        float shakeMagnitude = MathHelper.Lerp(0f, 30f, strength);
                        if (player.whoAmI == Main.myPlayer)
                        {
                            ScreenShakeSystem.StartShakeAtPoint(Projectile.Center, shakeMagnitude,
                            shakeDirection: Projectile.velocity.SafeNormalize(Vector2.Zero) * 2,
                            shakeStrengthDissipationIncrement: 0.7f - strength * 0.01f);
                        }
                    }
                }
            }
                
            Projectile.Center = Main.npc[OwnerIndex].Center;
            Projectile.rotation = Projectile.velocity.ToRotation();
            
        }
        float beamLength = 10000f;
        float DistanceFromPointToLine(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 lineDir = lineEnd - lineStart;
            float lineLength = lineDir.Length();
            if (lineLength == 0)
                return Vector2.Distance(point, lineStart);

            lineDir /= lineLength; // normalize

            float projectedLength = Vector2.Dot(point - lineStart, lineDir);
            projectedLength = MathHelper.Clamp(projectedLength, 0, lineLength);

            Vector2 closest = lineStart + lineDir * projectedLength;
            return Vector2.Distance(point, closest);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = GennedAssets.Textures.GreyscaleTextures.BloomLine2;
            Vector2 origin = new Vector2(tex.Width / 2f, 0f);
            Vector2 start = Projectile.Center - Main.screenPosition;

            // Constants for effect timing
            const int chargeDuration = 15; // ticks before firing visual starts fading
            const int fadeDuration = 30;   // total fadeout after main flash
            const int totalVisualDuration = chargeDuration + fadeDuration;

            // Time since spawn
            int t = totalVisualDuration - Projectile.timeLeft;
           

            float rot = Projectile.rotation - MathHelper.PiOver2;
            if (t < chargeDuration)
            {
                float chargeFactor = t / (float)chargeDuration;
                float thickness = MathHelper.Lerp(3f, 1f, chargeFactor);
                Color color = Color.Lerp(Color.Red, Color.White, chargeFactor * 0.6f);
                color = color with { A = 0 };
                float opacity = chargeFactor * 0.8f;

                Main.EntitySpriteDraw(
                    tex,
                    start,
                    null,
                    color * opacity,
                    rot,
                    origin,
                    new Vector2(thickness, beamLength / tex.Height),
                    SpriteEffects.None,
                    0
                );
            }
            //fire in the hole or something
            else if (t == chargeDuration)
            {
                float thickness = 16f;
                Color flashColor = Color.White with { A = 0 };
                Main.EntitySpriteDraw(
                    tex,
                    start,
                    null,
                    Color.Red with { A = 0 },
                    rot,
                    origin,
                    new Vector2(thickness*1.2f, beamLength / tex.Height),
                    SpriteEffects.None,
                    0
                );
                Main.EntitySpriteDraw(
                    tex,
                    start,
                    null,
                    flashColor,
                    rot,
                    origin,
                    new Vector2(thickness, beamLength / tex.Height),
                    SpriteEffects.None,
                    0
                );
            }
            //fade out
            else if (t > chargeDuration && t < totalVisualDuration)
            {
                float fadeTime = t - chargeDuration;
                float fadeFactor = 1f - (fadeTime / fadeDuration);

                float thickness = MathHelper.Lerp(3f, 2f, 1 - fadeFactor);
                float length = beamLength * (1f + fadeTime / fadeDuration * 0.3f);
                Color color = Color.Lerp(Color.White, Color.Crimson, fadeFactor * 0.4f);
                color = color with { A = 0 };
                Main.EntitySpriteDraw(
                    tex,
                    start,
                    null,
                    color * fadeFactor,
                    rot,
                    origin,
                    new Vector2(thickness, length / tex.Height),
                    SpriteEffects.None,
                    0
                );
            }

            return false; 
        }

    }
}
