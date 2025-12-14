using Luminance.Assets;

namespace HeavenlyArsenal.Content.Items.Weapons.Rogue.Temp;

internal class Bola_Ricochet : ModProjectile
{
    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults() { }

    public override void SetDefaults()
    {
        Projectile.tileCollide = true;
        Projectile.stopsDealingDamageAfterPenetrateHits = false;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.Size = new Vector2(20, 20);
    }

    public override void AI() { }

    public override bool PreDraw(ref Color lightColor)
    {
        var ball = AssetDirectory.Textures.BigGlowball.Value;

        var DrawPos = Projectile.Center - Main.screenPosition;
        var origin = ball.Size() * 0.5f;

        Main.EntitySpriteDraw(ball, DrawPos, null, Color.AntiqueWhite, 0, origin, 0.01f, 0);

        return false;
    }
}