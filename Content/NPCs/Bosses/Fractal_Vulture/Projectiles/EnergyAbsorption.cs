using CalamityMod;
using CalamityMod.Graphics.Primitives;
using Luminance.Assets;
using Terraria.Graphics.Shaders;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles;

internal class EnergyAbsorption : ModProjectile
{
    public NPC Owner;

    public Vector2 HomePos;

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetDefaults()
    {
        Projectile.timeLeft = 300;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.Size = new Vector2(30, 30);
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 10;
        ProjectileID.Sets.TrailingMode[Type] = 3;
    }

    public override void AI()
    {
        var Offset = new Vector2(0, 30);

        if (Owner != null)
        {
            HomePos = Owner.Center;
        }

        if (Owner != null && Owner.active)
        {
            Projectile.velocity = Projectile.AngleTo
                                          (HomePos + Offset)
                                      .ToRotationVector2()
                                      .RotatedBy(MathF.Sin(Projectile.ai[0]++ / 10.1f + Projectile.whoAmI) * LumUtils.InverseLerp(0, 60, Projectile.Distance(HomePos + Offset))) *
                                  20f;
        }

        if (Projectile.Center.Distance(HomePos + Offset) < 4)
        {
            Projectile.active = false;
        }
    }

    private float TrailWidth(float trailLengthInterpolant, Vector2 vertexPosition)
    {
        var widthInterpolant = Utils.GetLerpValue(0f, 0.25f, trailLengthInterpolant, true) * Utils.GetLerpValue(1.1f, 0.7f, trailLengthInterpolant, true);

        return MathHelper.SmoothStep(8f, 20f, widthInterpolant);
    }

    private Color TrailColor(float trailLengthInterpolant, Vector2 vertexPosition)
    {
        var t = MathHelper.Clamp(trailLengthInterpolant, 0f, 1f);
        var crimson = new Color(255, 255, 255);
        var brightness = MathHelper.SmoothStep(1f, 0.6f, t);

        // Interpolate between transparent and crimson
        var baseColor = Color.Lerp(Color.Transparent, crimson, 1f - t);

        var finalColor = baseColor * brightness * Projectile.Opacity;
        finalColor.A = (byte)MathHelper.Clamp(finalColor.A, 0, 255);

        return finalColor;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Main.spriteBatch.EnterShaderRegion();
        //yes, i'm using the art attack shader. so sue me,
        GameShaders.Misc["CalamityMod:ArtAttack"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
        GameShaders.Misc["CalamityMod:ArtAttack"].Apply();

        PrimitiveRenderer.RenderTrail(Projectile.oldPos, new PrimitiveSettings(TrailWidth, TrailColor, shader: GameShaders.Misc["CalamityMod:ArtAttack"]), 180);
        Main.spriteBatch.ExitShaderRegion();

        return base.PreDraw(ref lightColor);
    }

  
}