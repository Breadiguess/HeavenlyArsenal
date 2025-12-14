using Luminance.Assets;
using Luminance.Core.Graphics;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Items.Weapons.Magic.RocheLimit;

public class RelativisticJet : ModProjectile, IDrawsOverRocheLimitDistortion
{
    public float Layer => 2f;

    /// <summary>
    ///     How long this jet can exist for.
    /// </summary>
    public static int Lifetime => LumUtils.SecondsToFrames(0.25f);

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public void RenderOverDistortion()
    {
        if (Main.netMode == NetmodeID.Server)
        {
            return;
        }

        var jetShader = ShaderManager.GetShader("HeavenlyArsenal.RelativisticJetShader");
        jetShader.SetTexture(TextureAssets.Extra[ExtrasID.FlameLashTrailShape], 1, SamplerState.LinearWrap);

        PrimitiveRenderer.RenderTrail(Projectile.oldPos, new PrimitiveSettings(JetWidthFunction, JetColorFunction, Shader: jetShader), 46);
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = Lifetime * 10;
    }

    public override void SetDefaults()
    {
        var jetSize = Main.rand?.Next(80, 105) ?? 80;
        Projectile.width = jetSize;
        Projectile.height = jetSize;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.MaxUpdates = 5;
        Projectile.timeLeft = Projectile.MaxUpdates * Lifetime;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 1;
        Projectile.hide = true;
    }

    public override void AI()
    {
        var lifetimeRatio = 1f - Projectile.timeLeft / (float)Lifetime / Projectile.MaxUpdates;
        Projectile.scale = LumUtils.InverseLerpBump(0f, 0.2f, 0.45f, 1f, lifetimeRatio);
    }

    private float JetWidthFunction(float completionRatio)
    {
        var lifetimeRatio = 1f - Projectile.timeLeft / (float)Lifetime / Projectile.MaxUpdates;
        var widening = MathHelper.Lerp(0.9f, 1f, MathF.Sqrt(LumUtils.InverseLerp(1f, 0.05f, completionRatio)));
        var curve = MathHelper.Lerp(0.6f, 1f, LumUtils.Convert01To010(completionRatio));

        return (Projectile.width * widening * curve + Projectile.width * (1f - completionRatio) * lifetimeRatio * 4f) * Projectile.scale;
    }

    public Color JetColorFunction(float completionRatio)
    {
        return Projectile.GetAlpha(new Color(11, 75, 255)) * LumUtils.InverseLerp(1f, 0.87f, completionRatio) * LumUtils.InverseLerp(0.05f, 0.2f, completionRatio);
    }
}