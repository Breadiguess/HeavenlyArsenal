using HeavenlyArsenal.Common.Graphics;
using HeavenlyArsenal.Content.Items.Accessories.VoidCrestOath;
using Luminance.Common.Easings;
using Luminance.Core.Graphics;
using NoxusBoss.Assets;
using Terraria.Audio;
using Terraria.DataStructures;
using static Luminance.Common.Utilities.Utilities;
using Player = Terraria.Player;

namespace HeavenlyArsenal.Content.Projectiles;

public class VoidCrest_Spear : ModProjectile
{
    public float PortalInterp;

    public float BaseSize;

    public float MaxScale = 7f;

    public int Maxtime = 100;

    private PiecewiseCurve StabCurve;

    public ref Player Owner => ref Main.player[Projectile.owner];

    public ref float Time => ref Projectile.ai[0];

    public ref float t => ref Projectile.ai[1];

    public int frameIndex
    {
        get => (int)Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    public ref float Progress => ref Projectile.localAI[0];

    public int TargetId { get; set; } = -1;

    public override string Texture => "HeavenlyArsenal/Content/Items/Accessories/VoidCrestOath/VoidCrest_Spear";

    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(10);
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Maxtime;
        Projectile.aiStyle = 0;
        Projectile.damage = 0;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Progress = 0;
        Projectile.localAI[2] = (float)Math.Round(Main.rand.NextFloatDirection());
        frameIndex = Main.rand.Next(0, 4);

        SoundEngine.PlaySound
        (
            GennedAssets.Sounds.Avatar.RiftOpen with
            {
                Volume = 0.5f,
                PitchVariance = 0.1f,
                MaxInstances = 0
            },
            Projectile.Center
        );

        Projectile.scale += BaseSize;

        if (StabCurve == null)
        {
            StabCurve = new PiecewiseCurve()
                .Add(EasingCurves.Cubic, EasingType.Out, 0.9f, 0.5f)
                .Add(EasingCurves.Linear, EasingType.In, 1, 0.6f)
                .Add(EasingCurves.Sine, EasingType.In, 0, 1.0f);
        }

        Projectile.rotation = Projectile.velocity.ToRotation();
        Projectile.velocity *= 0;

        t = 0;
    }

    public override void AI()
    {
        t = (float)Math.Clamp(t + 0.05, 0, 1);
        t = (float)Math.Round(t, 2);
        //Main.NewText(t);
        Progress = StabCurve.Evaluate(t);

        if (Time < Maxtime / 3)
        {
            PortalInterp = float.Lerp(PortalInterp, 1, 0.5f);
        }

        if (Time > Maxtime / 3)
        {
            PortalInterp = float.Lerp(PortalInterp, 0, 0.2f);
        }

        if (t == 0.35f)
        {
            SpawnParticle();

            var target = Main.projectile[TargetId];

            if (target != null)
            {
                SoundEngine.PlaySound
                (
                    GennedAssets.Sounds.NPCHit.AvatarHurt with
                    {
                        Volume = 0.5f,
                        PitchVariance = 0.1f
                    },
                    target.Center
                );

                target.active = false;
                target.Kill();
                target = null;
            }
        }

        Time++;

        if (PortalInterp <= 0.01f && Time > 10)
        {
            Projectile.Kill();
        }
    }

    private void SpawnParticle()
    {
        var particle = VoidCrest_DisintegrateParticle.pool.RequestParticle();

        var Velocity = Vector2.Zero;
        float Rot = 0;
        var AdjustedSpawn = Projectile.Center + (Projectile.rotation + MathHelper.ToRadians(10 - 20 * t) * Progress).ToRotationVector2() * 60;
        particle.Prepare(AdjustedSpawn, Velocity, Rot, 120);

        ParticleEngine.BehindProjectiles.Add(particle);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Projectile.Kill();
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        Projectile.Kill();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var a = Color.Red;
        var DrawPos = Projectile.Center - Main.screenPosition;
        RenderPortal();
        RenderSpearGlow(a);
        RenderSpear(a);

        return false;
    }

    private void RenderSpear(Color c)
    {
        var texture = ModContent.Request<Texture2D>(Texture).Value;
        var DrawPos = Projectile.Center - Main.screenPosition;
        var Scale = new Vector2(1, 1 * Progress) * Progress;
        var frame = texture.Frame(4, 1, frameIndex);
        var origin = new Vector2(frame.Width / 2, frame.Height);

        var Adjust = Projectile.localAI[2] * MathHelper.ToRadians(10 - 20 * t) * Progress;
        var Rot = Projectile.rotation + MathHelper.PiOver2 + Adjust;

        Main.EntitySpriteDraw(texture, DrawPos, frame, c, Rot, origin, Scale, SpriteEffects.None);
    }

    private void RenderSpearGlow(Color c)
    {
        var texture = ModContent.Request<Texture2D>(Texture + "Glow").Value;
        var DrawPos = Projectile.Center - Main.screenPosition;
        var Scale = new Vector2(1, 1 * Progress) * Progress;
        var frame = texture.Frame(4, 1, frameIndex);
        var origin = new Vector2(frame.Width / 2, frame.Height / 1.1f);

        var Adjust = Projectile.localAI[2] * MathHelper.ToRadians(10 - 20 * t) * Progress;
        var Rot = Projectile.rotation + MathHelper.PiOver2 + Adjust;

        Main.EntitySpriteDraw(texture, DrawPos, frame, c, Rot, origin, Scale * 1.2f, SpriteEffects.None);
    }

    private void RenderPortal()
    {
        Main.spriteBatch.PrepareForShaders();

        var squish = 0.5f;
        var scaleFactor = PortalInterp;
        var color = new Color(77, 0, 2);
        var edgeColor = new Color(1f, 0.08f, 0.08f);
        var glow = AssetDirectory.Textures.BigGlowball.Value;

        var innerRiftTexture = GennedAssets.Textures.FirstPhaseForm.RiftInnerTexture.Value;
        var textureArea = Projectile.Size * new Vector2(1f - squish, 1f) / innerRiftTexture.Size() * MaxScale * scaleFactor * 1.6f;

        Main.EntitySpriteDraw
        (
            glow,
            Projectile.Center - Main.screenPosition,
            glow.Frame(),
            Color.Red with
            {
                A = 200
            },
            Projectile.rotation,
            glow.Size() * 0.5f,
            new Vector2(0.12f, 0.25f) * 0.325f * PortalInterp,
            0
        );

        var riftShader = ShaderManager.GetShader("NoxusBoss.DarkPortalShader");
        riftShader.TrySetParameter("time", Main.GlobalTimeWrappedHourly * 0.1f);
        riftShader.TrySetParameter("baseCutoffRadius", 0.1f);
        riftShader.TrySetParameter("swirlOutwardnessExponent", 0.151f);
        riftShader.TrySetParameter("swirlOutwardnessFactor", 3.67f);
        riftShader.TrySetParameter("vanishInterpolant", InverseLerp(1f, 0f, Projectile.scale - Projectile.identity / 13f % 0.2f));
        riftShader.TrySetParameter("edgeColor", edgeColor.ToVector4());
        riftShader.TrySetParameter("edgeColorBias", 0f);
        riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 1, SamplerState.AnisotropicWrap);
        riftShader.SetTexture(GennedAssets.Textures.Noise.WavyBlotchNoise, 2, SamplerState.AnisotropicWrap);
        riftShader.Apply();

        Main.spriteBatch.Draw(innerRiftTexture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(color), Projectile.rotation, innerRiftTexture.Size() * 0.5f, textureArea, 0, 0f);
        Main.spriteBatch.ResetToDefault();
    }
}

public class VoidCrestInterceptorGlobalProjectile : GlobalProjectile
{
    public override bool PreAI(Projectile projectile)
    {
        return true;
    }
}