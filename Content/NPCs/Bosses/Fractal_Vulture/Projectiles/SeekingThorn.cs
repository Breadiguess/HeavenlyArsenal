using Luminance.Assets;
using Luminance.Common.Utilities;
using NoxusBoss.Assets;

namespace HeavenlyArsenal.Content.NPCs.Bosses.Fractal_Vulture.Projectiles;

public class SeekingThorn : ModProjectile
{
    public int Time
    {
        get => (int)Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    public override string Texture => MiscTexturesRegistry.InvisiblePixelPath;

    public override void SetStaticDefaults() 
    {
        
    }

    public override void SetDefaults()
    {
        Projectile.width = 12;
        Projectile.height = 12;

        Projectile.hostile = true;
        Projectile.friendly = false;

        Projectile.penetrate = 1;
        Projectile.timeLeft = 220;

        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.aiStyle = -1;
        Projectile.extraUpdates = 0;
    }

    public override void AI()
    {
        if (Main.rand.NextBool(3))
        {
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Grass);
        }

        Time++;

        if (Projectile.velocity.LengthSquared() < 0.001f)
        {
            Projectile.velocity = Vector2.UnitY * 0.1f;
        }

        // Visual orientation
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        const float MaxSpeed = 30.5f;
        const float Accel = 0.75f;
        const float TurnRate = 15f;
        const int HomingDelay = 30;
        const int HomingRamp = 20;
        const int HomingEnd = 160;

        if (Time < HomingDelay)
        {
            Projectile.velocity = Vector2.Clamp(Projectile.velocity * 1.02f, -Vector2.One * MaxSpeed, Vector2.One * MaxSpeed);

            return;
        }

        var target = voidVulture.Myself.As<voidVulture>().currentTarget as Player;

        if (target == null)
        {
            var best = FindClosestPlayer(Projectile.Center, 2600f);

            if (best != -1)
            {
                target = Main.player[best];
            }
        }

        if (target == null)
        {
            return;
        }

        var desiredDir = (target.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitY));

        var rampT = MathHelper.Clamp((Time - HomingDelay) / HomingRamp, 0f, 1f);
        var turnThisTick = TurnRate * MathHelper.Lerp(0.25f, 1f, rampT);

        var newDir = RotateTowards(Projectile.velocity.SafeNormalize(desiredDir), desiredDir, turnThisTick);

        var v = Projectile.velocity + newDir * Accel;

        if (v.Length() > MaxSpeed)
        {
            v = v.SafeNormalize(newDir) * MaxSpeed;
        }

        if (Time < HomingEnd)
        {
            Projectile.velocity = v;
        }
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

    private static Vector2 RotateTowards(Vector2 currentDir, Vector2 targetDir, float maxRadians)
    {
        var current = currentDir.ToRotation();
        var target = targetDir.ToRotation();
        var next = current.AngleTowards(target, maxRadians);

        return next.ToRotationVector2();
    }

    public override void OnKill(int timeLeft) { }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D a = GennedAssets.Textures.GreyscaleTextures.WhitePixel;

        Main.EntitySpriteDraw(a, Projectile.Center - Main.screenPosition, null, Color.AntiqueWhite, Projectile.rotation + MathHelper.PiOver2, a.Size() / 2, new Vector2(30, 10), 0);

        return base.PreDraw(ref lightColor);
    }
}