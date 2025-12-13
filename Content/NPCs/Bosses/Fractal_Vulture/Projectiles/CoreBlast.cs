using Luminance.Assets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles
{
    internal class CoreBlast : ModProjectile
    {
        private int beamLength = 1800;
        public int index;
        public int OwnerIndex;
        public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;
        public override void AI()
        {
          
            if (Main.npc[OwnerIndex]!=null && Main.npc[OwnerIndex].active && Main.npc[OwnerIndex].type == ModContent.NPCType<OtherworldlyCore>())
            {
                if (Projectile.timeLeft < 18)
                    Projectile.velocity *= 0;
                else
                    Projectile.rotation = Projectile.velocity.ToRotation();
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

            float _ = float.NaN;
            Vector2 beamEndPos = Projectile.Center + Projectile.velocity * 1000;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, beamEndPos, 22 * Projectile.scale, ref _);
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
                Color color = Color.Lerp(Color.Blue, Color.White, chargeFactor * 0.6f);
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
                    new Vector2(thickness * 1.2f, beamLength / tex.Height),
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
                Color color = Color.Lerp(Color.White, Color.Blue, fadeFactor * 0.4f);
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

            //Utils.DrawBorderString(Main.spriteBatch, Projectile.timeLeft.ToString(), Projectile.Center - Main.screenPosition, Color.White);
            return false;
        }
    }
}
