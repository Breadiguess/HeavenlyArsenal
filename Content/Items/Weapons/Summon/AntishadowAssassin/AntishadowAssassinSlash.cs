using Luminance.Core.Graphics;
using Terraria.GameContent;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;

public class AntishadowAssassinSlash : ModProjectile
{
    /// <summary>
    ///     The visual coverage of this slash.
    /// </summary>
    public float Coverage { get; set; }

    /// <summary>
    ///     How long this slash should exist for, in frames.
    /// </summary>
    public static int Lifetime => LumUtils.SecondsToFrames(0.25f);

    /// <summary>
    ///     The X rotation angle of this slash.
    /// </summary>
    public ref float AngleX => ref Projectile.ai[0];

    /// <summary>
    ///     The Y rotation angle of this slash.
    /// </summary>
    public ref float AngleY => ref Projectile.ai[1];

    /// <summary>
    ///     The size multiplier of this slash.
    /// </summary>
    public ref float SizeMultiplier => ref Projectile.ai[2];

    /// <summary>
    ///     How long this slash has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.localAI[0];

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = Lifetime / 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 600;
        Projectile.height = 600;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = Lifetime;
        Projectile.penetrate = -1;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 1;
        Projectile.tileCollide = false;
        Projectile.hide = true;
        Projectile.minion = true;
        Projectile.DamageType = DamageClass.Summon;
        Projectile.MaxUpdates = 2;
    }

    public override void AI()
    {
        if (Time == 0f)
        {
            Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Coverage = Main.rand.NextFloat(50f, 600f) * SizeMultiplier;
        }

        Time++;
        Projectile.scale = LumUtils.Convert01To010(Time / Lifetime + 0.001f);

        var fireBrightness = Main.rand.Next(0, 15);
        var fireColor = new Color(fireBrightness, fireBrightness, fireBrightness);

        if (Main.rand.NextBool(6))
        {
            fireColor = new Color(174, 0, Main.rand.Next(23), 0);
        }

        if (Time % 12f == 0f)
        {
            AntishadowFireParticleSystemManager.CreateNew
                (Projectile.owner, false, Projectile.Center, Main.rand.NextVector2Circular(77f, 77f), Vector2.One * Main.rand.NextFloat(30f, 105f) * SizeMultiplier, fireColor);
        }
    }

    private float TrailWidthFunction(float completionRatio)
    {
        return Projectile.scale * 67f;
    }

    private Color TrailColorFunction(float completionRatio)
    {
        var lifetimeRatio = Time / Lifetime;

        return Color.Black * Projectile.Opacity * LumUtils.InverseLerp(1f, 0.75f, lifetimeRatio);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var lifetimeRatio = Time / Lifetime;
        var trailShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssassinSlashShader");
        trailShader.TrySetParameter("sheenEdgeColorWeak", new Vector4(2f, 0f, lifetimeRatio * 0.6f, 1f));
        trailShader.TrySetParameter("sheenEdgeColorStrong", new Vector4(2f, 2f, 1.25f, 1f));
        trailShader.TrySetParameter("noiseSlant", 1.95f);
        trailShader.TrySetParameter("noiseInfluenceFactor", 0.5f);
        trailShader.TrySetParameter("opacityFadeExponent", 2f);
        trailShader.SetTexture(GennedAssets.Textures.Noise.SwirlNoise, 1, SamplerState.LinearWrap);
        trailShader.SetTexture(TextureAssets.Projectile[Type], 2, SamplerState.LinearWrap);

        var swingArc = lifetimeRatio * -MathHelper.Pi + Projectile.rotation;
        var points = new Vector2[52];
        var transformation = Matrix.CreateRotationX(AngleX) * Matrix.CreateRotationY(AngleY);

        for (var i = 0; i < points.Length; i++)
        {
            var trailInterpolant = i / (float)points.Length;
            var offset = (MathHelper.Pi * trailInterpolant + swingArc).ToRotationVector2();
            points[i] = Projectile.Center + Vector2.Transform(offset, transformation) * Coverage * 0.5f;
        }

        PrimitiveRenderer.RenderTrail
        (
            points,
            new PrimitiveSettings(default, default, Shader: trailShader, UseUnscaledMatrix: true)
            {
                WidthFunction = TrailWidthFunction,
                ColorFunction = TrailColorFunction
            },
            17
        );

        return false;
    }
}