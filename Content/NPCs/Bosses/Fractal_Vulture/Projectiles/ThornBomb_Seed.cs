using Luminance.Assets;
using NoxusBoss.Assets;
using NoxusBoss.Core.Utilities;
using Terraria.Audio;
using Terraria.DataStructures;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles;

public class ThornBomb_Seed : ModProjectile
{
    private const int FuseTime = 120;

    private const int ThornCount = 5;

    private const float DriftDamp = 0.985f;

    private const float DetonateBurstSpeed = 6.5f;

    int NeedleStormStart = FuseTime + 10;
    int NeedleStormEnd = FuseTime + 60;
    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public int Time
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.DrawScreenCheckFluff[Type] = 4000;
    }

    public override void SetDefaults()
    {
        Projectile.width = 60;
        Projectile.height = 60;

        Projectile.hostile = true;
        Projectile.friendly = false;

        Projectile.penetrate = -1;
        Projectile.timeLeft = 1800;

        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.aiStyle = -1;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.damage = 0;
        Projectile.scale = 0;
    }

    public override void AI()
    {
        Projectile.scale = LumUtils.InverseLerp(0, 30, Time);
        Projectile.damage = (int)(Projectile.originalDamage * LumUtils.InverseLerp(40, FuseTime - 40, Time));
        Projectile.velocity *= DriftDamp;

        Projectile.rotation += 0.08f * (Projectile.direction == 0 ? 1f : Projectile.direction) * LumUtils.InverseLerp(FuseTime, 0, Time);

        if (Time > FuseTime - 15)
        {
            Projectile.scale = 1 + 2*LumUtils.InverseLerp(FuseTime - 15, FuseTime, Time);
        }

        if (Time == FuseTime)
        {
            Detonate();
            //Projectile.Kill();
        }
        if(Time < NeedleStormEnd && Time > NeedleStormStart)
        {
            if (Time % 10 == 0)
            LaunchNeedleStorm();
        }
        if (Time > NeedleStormEnd)
            Projectile.Opacity = 1-LumUtils.InverseLerp(NeedleStormEnd, NeedleStormEnd + 60, Time);

        if (Projectile.Opacity == 0)
            Projectile.Kill();
        Time++;
    }

    private void LaunchNeedleStorm()
    {
        const float InitialSpeed = 27.5f;

        var angleStep = MathHelper.TwoPi / ThornCount;

        SoundEngine.PlaySound(SoundID.Item141 with { PitchVariance = 0.2f, MaxInstances = 0}, Projectile.Center).WithVolumeBoost(2);
        for (var i = 0; i < ThornCount; i++)
        {
            var angle = angleStep * i + Projectile.rotation;
            var velocity = angle.ToRotationVector2() * InitialSpeed * 2;

            Projectile a = Projectile.NewProjectileDirect
            (
                Projectile.GetSource_FromThis(),
                Projectile.Center+ new Vector2(Main.rand.NextFloat(-10,10),0),
                velocity,
                ModContent.ProjectileType<SeekingThorn>(),
                Projectile.damage / 4,
                10f
            );
            a.As<SeekingThorn>().Time = 160;
        }
    }
    private void Detonate()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            return;
        }

        const float InitialSpeed = 7.5f;

        var angleStep = MathHelper.TwoPi / ThornCount;

        for (var i = 0; i < ThornCount; i++)
        {
            var angle = angleStep * i + Projectile.rotation;
            var velocity = angle.ToRotationVector2() * InitialSpeed * 2;

            Projectile.NewProjectile
            (
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                velocity,
                ModContent.ProjectileType<SeekingThorn>(),
                Projectile.damage / 4,
                10f
            );
        }

        SoundEngine.PlaySound
            (
                SoundID.Item14 with
                {
                    PitchVariance = 0.4f
                },
                Projectile.Center
            )
            .WithVolumeBoost(4);
    }

    private static int FindClosestPlayer(Vector2 from, float maxDist)
    {
        var best = -1;
        var bestD = maxDist;

        for (var i = 0; i < Main.maxPlayers; i++)
        {
            var p = Main.player[i];

            if (!p.active || p.dead)
            {
                continue;
            }

            var d = Vector2.Distance(from, p.Center);

            if (d < bestD)
            {
                bestD = d;
                best = i;
            }
        }

        return best;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D thornTex = GennedAssets.Textures.GreyscaleTextures.WhitePixel;
        Texture2D thornGlow = GennedAssets.Textures.GreyscaleTextures.BloomFlare;
        Texture2D coreTex = GennedAssets.Textures.GreyscaleTextures.HollowCircleSoftEdge;

        Texture2D thing = AssetDirectory.Textures.BigGlowball.Value;

        Vector2 DrawPos = Projectile.Center - Main.screenPosition;


        Color a = Color.Lerp(Color.White, Color.Red, LumUtils.InverseLerp(0, FuseTime, Time)) * Projectile.Opacity;
        Main.EntitySpriteDraw(thing, DrawPos, null, a, Projectile.rotation, thing.Size() / 2, 0.15f * Projectile.scale, 0);

        Main.EntitySpriteDraw(thornGlow, DrawPos, null, a with { A = 0 }, Projectile.rotation, thornGlow.Size() / 2, 0.15f * Projectile.scale, 0);
        Main.EntitySpriteDraw
        (
            coreTex,
            DrawPos,
            null,
            Color.White with
            {
                A = 0
            },
            0f,
            coreTex.Size() * 0.5f,
            0.1f * Projectile.scale,
            0
        );

        const float Radius = 70;

        var angleStep = MathHelper.TwoPi / ThornCount;

        var pulse = 1f + 0.1f * MathF.Sin(Time * 0.25f);

        for (var i = 0; i < ThornCount; i++)
        {
            var angle = angleStep * i + Projectile.rotation;

            var offset = angle.ToRotationVector2() * Radius * pulse;
            var pos = DrawPos + offset * LumUtils.InverseLerp(0, FuseTime, Time);

            var rotation = angle + MathHelper.PiOver2;

            Main.EntitySpriteDraw(thornTex, pos, null, a, rotation, new Vector2(0, 0.5f), new Vector2(2f, 60f) * Projectile.scale, 0);

            for(int x = 0; x< 12; x++)
            {
                float thing2 = LumUtils.InverseLerp(0, FuseTime, Time);
                Vector2 newpos = DrawPos + (offset + new Vector2(0, 32* thing2).RotatedBy(x / 12f * MathHelper.TwoPi) * thing2 )* thing2;

                Main.EntitySpriteDraw(thornTex, newpos, null, a, rotation, new Vector2(0, 0.5f), new Vector2(2f, 60f) * Projectile.scale, 0);
            }

            Utils.DrawLine(Main.spriteBatch, Projectile.Center, Projectile.Center + new Vector2(4000,0).RotatedBy(angle), a * LumUtils.InverseLerp(FuseTime - 60, FuseTime - 10, Time), a * LumUtils.InverseLerp(FuseTime - 60, FuseTime - 10, Time), 4f);
        }

       // Utils.DrawBorderString(Main.spriteBatch, Projectile.damage.ToString(), DrawPos, Color.AntiqueWhite);

        return false;
    }
}