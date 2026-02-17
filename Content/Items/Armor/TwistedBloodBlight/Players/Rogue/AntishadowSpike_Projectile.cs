using NoxusBoss.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyArsenal.Content.Items.Armor.TwistedBloodBlight.Players.Rogue
{
    public class AntishadowSpike_Projectile : ModProjectile
    {
        public override string Texture => "HeavenlyArsenal/Content/Items/Armor/TwistedBloodBlight/Players/Rogue/BloodEcho";
        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            
        }
        public override bool PreDraw(ref Color lightColor)
        {
            float rotation = Projectile.rotation - MathHelper.PiOver2;
            Texture2D tex = ModContent.Request<Texture2D>("HeavenlyArsenal/Content/Items/Armor/TwistedBloodBlight/Players/Rogue/BloodEcho").Value;
            Texture2D glow = GennedAssets.Textures.GreyscaleTextures.BloomCircleSmall;
            float LoopCount = 6;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, default, default, null, Main.GameViewMatrix.ZoomMatrix);
            for (int i = 0; i < LoopCount; i++)
            {
                Color color = Color.Lerp(Color.Crimson, Color.PaleVioletRed, i / LoopCount);
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition + new Vector2(2, 0).RotatedBy(i / LoopCount * MathHelper.TwoPi + (Main.GlobalTimeWrappedHourly)), null, color, rotation, tex.Size() / 2, 1, 0);

            }

            Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, Color.Crimson, rotation, glow.Size() / 2, new Vector2(0.18f, 0.5f), 0);
            Main.spriteBatch.ResetToDefault();
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.Black, rotation, tex.Size() / 2, 1, 0);

            return base.PreDraw(ref lightColor);
        }
    }
}
