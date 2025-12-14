using Luminance.Assets;
using NoxusBoss.Assets;

namespace HeavenlyArsenal.Content.Items.Accessories.SwirlCloak;

internal class SwirlCloak_Star : ModProjectile
{
    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.width = 22;
        Projectile.height = 22;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 300;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.alpha = 0;
        Projectile.extraUpdates = 2;
        Projectile.scale = 1f;
    }

    public override void AI()
    {
        Projectile.velocity *= 1.02f;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D a = GennedAssets.Textures.GreyscaleTextures.BloomCircle;
        var DrawPos = Projectile.Center - Main.screenPosition;
        Main.EntitySpriteDraw(a, DrawPos, null, lightColor, 0, a.Size() * 0.5f, 0.1f, 0);

        return base.PreDraw(ref lightColor);
    }
}