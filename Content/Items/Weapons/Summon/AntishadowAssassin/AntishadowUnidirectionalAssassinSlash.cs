using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NoxusBoss.Assets;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace HeavenlyArsenal.Content.Items.Weapons.Summon.AntishadowAssassin;

public class AntishadowUnidirectionalAssassinSlash : ModProjectile
{
    /// <summary>
    /// How long this slash should exist for, in frames.
    /// </summary>
    public static int Lifetime => LumUtils.SecondsToFrames(0.4f);

    /// <summary>
    /// How long this slash has existed for, in frames.
    /// </summary>
    public ref float Time => ref Projectile.localAI[0];

    /// <summary>
    /// The offset of this slash relative to its target.
    /// </summary>
    public ref float OffsetRadius => ref Projectile.ai[1];

    /// <summary>
    /// The offset angle of this slash relative to its target.
    /// </summary>
    public ref float OffsetAngle => ref Projectile.ai[2];

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailingMode[Type] = 2;
        ProjectileID.Sets.TrailCacheLength[Type] = Lifetime / 2;
    }

    public override void SetDefaults()
    {
        Projectile.width = 50;
        Projectile.height = 50;
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

        ProjectileID.Sets.TrailCacheLength[Type] = Lifetime / 2;
    }

    public override void AI()
    {
        Time++;
    }

    private float TrailWidthFunction(float completionRatio) => Projectile.scale * 58f;

    private Color TrailColorFunction(float completionRatio)
    {
        float lifetimeRatio = Time / Lifetime;
        return Color.Black * Projectile.Opacity * LumUtils.InverseLerp(1f, 0.75f, lifetimeRatio);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (Main.dedServ)
            return false;
        float lifetimeRatio = Time / Lifetime;
        ManagedShader trailShader = ShaderManager.GetShader("HeavenlyArsenal.AntishadowAssassinSlashShader");
        trailShader.TrySetParameter("sheenEdgeColorWeak", new Vector4(2f, 0f, lifetimeRatio * 0.3f, 1f));
        trailShader.TrySetParameter("sheenEdgeColorStrong", new Vector4(2f, 2f, 1.3f, 1f));
        trailShader.TrySetParameter("noiseSlant", 1.95f);
        trailShader.TrySetParameter("noiseInfluenceFactor", 0.5f);
        trailShader.TrySetParameter("opacityFadeExponent", 1f);
        trailShader.SetTexture(GennedAssets.Textures.Noise.PerlinNoise, 1, SamplerState.LinearWrap);
        trailShader.SetTexture(TextureAssets.Projectile[Type], 2, SamplerState.LinearWrap);

        PrimitiveRenderer.RenderTrail(Projectile.oldPos, new PrimitiveSettings(default, default, _ => Projectile.Size * 0.5f, Shader: trailShader, UseUnscaledMatrix: true)
        {
            WidthFunction = TrailWidthFunction,
            ColorFunction = TrailColorFunction
        }, 12);
        return false;
    }
}
