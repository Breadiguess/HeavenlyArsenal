using CalamityMod;
using CalamityMod.Graphics.Primitives;
using Luminance.Assets;
using Luminance.Common.Utilities;
using NoxusBoss.Assets;
using NoxusBoss.Core.Utilities;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles;

internal class IntersectingComet : ModProjectile
{
    public voidVulture Owner;

    public float Offset = 20;

    public Projectile SisterComet;

    /// <summary>
    ///     don't know why im adding this since projecitle.oldposition exists, but such is life.
    /// </summary>
    public Vector2 LastPos;

    public int Time
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 12;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.rotation = Projectile.velocity.ToRotation();
        // Projectile.extraUpdates = 1;
    }

    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(30, 30);
        Projectile.aiStyle = -1;
        Projectile.timeLeft = 1400;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.DamageType = DamageClass.Generic;
        Projectile.tileCollide = false;
    }

    public override void AI()
    {
        if (SisterComet == null || !SisterComet.active || Owner == null)
        {
            Projectile.active = false;

            return;
        }

        Projectile.velocity = new Vector2(10 + Math.Abs(Offset) / 10, MathF.Cos(Time / 14.1f) * Offset).RotatedBy(Projectile.rotation) * 1.2f;

        var prev = LastPos;
        var cur = Projectile.Center;
        LastPos = cur;

        var sister = SisterComet.ModProjectile<IntersectingComet>();

        if (sister == null)
        {
            Projectile.active = false;

            return;
        }

        var minDist = SegmentDistance
        (
            prev,
            cur,
            sister.LastPos,
            sister.Projectile.Center
        );

        if (minDist < 2f && Time > 10)
        {
            Explode();
        }

        Time++;
    }

    /// <summary>
    ///     what the fuck
    /// </summary>
    public static float SegmentDistance(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
    {
        // Based on solving the closest-point-on-segment pair analytically
        var d1 = p2 - p1;
        var d2 = q2 - q1;
        var r = p1 - q1;

        var a = Vector2.Dot(d1, d1);
        var e = Vector2.Dot(d2, d2);
        var f = Vector2.Dot(d2, r);

        float s, t;

        if (a <= float.Epsilon && e <= float.Epsilon)
        {
            return Vector2.Distance(p1, q1);
        }

        if (a <= float.Epsilon)
        {
            t = MathHelper.Clamp(f / e, 0f, 1f);

            return Vector2.Distance(p1, q1 + d2 * t);
        }

        var c = Vector2.Dot(d1, r);

        if (e <= float.Epsilon)
        {
            s = MathHelper.Clamp(-c / a, 0f, 1f);

            return Vector2.Distance(p1 + d1 * s, q1);
        }

        //I HATE MATH I HATE MATH I HATE MATH
        var b = Vector2.Dot(d1, d2);
        var denom = a * e - b * b;

        if (denom != 0f)
        {
            s = MathHelper.Clamp((b * f - c * e) / denom, 0f, 1f);
        }
        else
        {
            s = 0f;
        }

        t = (b * s + f) / e;

        if (t < 0f)
        {
            t = 0f;
            s = MathHelper.Clamp(-c / a, 0f, 1f);
        }
        else if (t > 1f)
        {
            t = 1f;
            s = MathHelper.Clamp((b - c) / a, 0f, 1f);
        }

        var c1 = p1 + d1 * s;
        var c2 = q1 + d2 * t;

        return Vector2.Distance(c1, c2);
    }

    private void Explode()
    {
        if (Projectile.ai[2] == 3)
        {
            var b = Projectile.NewProjectileDirect
                (Projectile.GetSource_FromThis(), Projectile.Center, Projectile.DirectionFrom(Owner.NPC.Center), ModContent.ProjectileType<CometBackBlast>(), (int)(Owner.NPC.defDamage / 1.5f), 30);

            b.As<CometBackBlast>().Owner = Owner.NPC;
        }

        for (var i = 0; i < 3; i++)
        {
            var a = Projectile.NewProjectileDirect
                (Projectile.GetSource_FromThis(), Projectile.Center, Projectile.AngleTo(Owner.NPC.Center).ToRotationVector2() * 10, ModContent.ProjectileType<EnergyAbsorption>(), 182, 0);

            a.As<EnergyAbsorption>().HomePos = Owner.NPC.Center;
            a.As<EnergyAbsorption>().Owner = Owner.NPC;

            Dust.NewDust(Projectile.Center, 40, 40, DustID.Cloud, Main.rand.NextFloat(-30, 30), Main.rand.NextFloat(-30, 30));
        }

        SoundEngine.PlaySound
            (
                GennedAssets.Sounds.Avatar.PlanetoidExplosion with
                {
                    PitchVariance = 0.2f,
                    MaxInstances = 0
                },
                Projectile.Center
            )
            .WithVolumeBoost(2);

        Projectile.Kill();
    }

    public Color TrailColor(float completionRatio)
    {
        var t = MathHelper.Clamp(completionRatio, 0f, 1f);
        var crimson = new Color(255, 255, 255);
        var brightness = MathHelper.SmoothStep(1f, 0.6f, t);

        // Interpolate between transparent and crimson
        var baseColor = Color.Lerp(Color.Transparent, crimson, 1f - t);

        var finalColor = baseColor * brightness * Projectile.Opacity;
        finalColor.A = (byte)MathHelper.Clamp(finalColor.A, 0, 255);

        return finalColor;
    }

    public float TrailWidth(float completionRatio)
    {
        var widthInterpolant = Utils.GetLerpValue(0f, 0.25f, completionRatio, true) * Utils.GetLerpValue(1.1f, 0.7f, completionRatio, true);

        return MathHelper.SmoothStep(2, 12f, widthInterpolant);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D thing = GennedAssets.Textures.GraphicalUniverseImager.EclipseSelectionBox_BloodMoon;

        Main.spriteBatch.EnterShaderRegion();
        //yes, i'm using the art attack shader. so sue me,
        GameShaders.Misc["CalamityMod:ArtAttack"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ZapTrail"));
        GameShaders.Misc["CalamityMod:ArtAttack"].Apply();

        PrimitiveRenderer.RenderTrail(Projectile.oldPos, new PrimitiveSettings(TrailWidth, TrailColor, _ => Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:ArtAttack"]), 180);
        Main.spriteBatch.ExitShaderRegion();

        Main.EntitySpriteDraw(thing, Projectile.Center - Main.screenPosition, null, Color.AntiqueWhite, Projectile.rotation, thing.Size() / 2, 1, 0);

        return base.PreDraw(ref lightColor);
    }
}